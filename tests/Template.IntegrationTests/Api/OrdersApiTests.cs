using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Template.Application.Orders;
using Template.Application.Common;
using Template.Infrastructure.Persistence;

namespace Template.IntegrationTests.Api;

public sealed class OrdersApiTests : IAsyncLifetime
{
    private readonly SqliteConnection _connection = new("Data Source=CleanCodeTemplateTests;Mode=Memory;Cache=Shared");
    private WebApplicationFactory<Program> _factory = null!;

    [Fact]
    public async Task PostOrderCreatesOrder()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken());
        var request = new CreateOrderRequest(
            "customer-001",
            [new CreateOrderItemRequest("Clean Code", 2, 30m)]);

        var response = await client.PostAsJsonAsync("/api/orders", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<OrderResponse>();
        Assert.NotNull(created);
        Assert.Equal(60m, created.TotalAmount);
    }

    [Fact]
    public async Task GetOrderReturnsCreatedOrder()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken());
        var request = new CreateOrderRequest(
            "customer-001",
            [new CreateOrderItemRequest("Clean Code", 1, 30m)]);

        var createResponse = await client.PostAsJsonAsync("/api/orders", request);
        var created = await createResponse.Content.ReadFromJsonAsync<OrderResponse>();

        var response = await client.GetAsync($"/api/orders/{created!.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var order = await response.Content.ReadFromJsonAsync<OrderResponse>();
        Assert.Equal(created.Id, order!.Id);
    }

    [Fact]
    public async Task GetOrdersReturnsPagedOrders()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken());

        await client.PostAsJsonAsync(
            "/api/orders",
            new CreateOrderRequest("customer-001", [new CreateOrderItemRequest("Clean Code", 1, 30m)]));
        await client.PostAsJsonAsync(
            "/api/orders",
            new CreateOrderRequest("customer-002", [new CreateOrderItemRequest("Refactoring", 1, 45m)]));

        var response = await client.GetAsync("/api/orders?page=1&pageSize=1");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonAsync<PagedResult<OrderResponse>>();
        Assert.NotNull(page);
        Assert.Equal(2, page.TotalCount);
        Assert.Equal(1, page.Page);
        Assert.Single(page.Items);
    }

    [Fact]
    public async Task PostOrderValidationFailureReturnsValidationProblem()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken());

        var response = await client.PostAsJsonAsync("/api/orders", new CreateOrderRequest("", []));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task GetMissingOrderReturnsProblemDetails()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken());

        var response = await client.GetAsync($"/api/orders/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task PostOrderRequiresAuthentication()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/orders",
            new CreateOrderRequest("customer-001", [new CreateOrderItemRequest("Clean Code", 1, 30m)]));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    public async Task InitializeAsync()
    {
        await _connection.OpenAsync();
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");
                builder.ConfigureAppConfiguration((_, configurationBuilder) =>
                {
                    configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Database:Provider"] = "Sqlite",
                        ["ConnectionStrings:DefaultConnection"] = _connection.ConnectionString
                    });
                });

                builder.ConfigureTestServices(services =>
                {
                    var dbContextDescriptors = services
                        .Where(descriptor =>
                            descriptor.ServiceType == typeof(DbContextOptions)
                            || descriptor.ServiceType == typeof(DbContextOptions<AppDbContext>)
                            || descriptor.ServiceType.FullName?.Contains("IDbContextOptionsConfiguration", StringComparison.Ordinal) == true)
                        .ToList();

                    foreach (var descriptor in dbContextDescriptors)
                    {
                        services.Remove(descriptor);
                    }

                    services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));

                    using var provider = services.BuildServiceProvider();
                    using var scope = provider.CreateScope();
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    dbContext.Database.EnsureCreated();
                });
            });
    }

    public async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
        await _connection.DisposeAsync();
    }

    private static string CreateToken()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("local-development-signing-key-please-change"));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = "CleanCodeTemplate",
            Audience = "CleanCodeTemplate",
            Subject = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, "integration-test")]),
            Expires = DateTime.UtcNow.AddMinutes(10),
            SigningCredentials = credentials
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}
