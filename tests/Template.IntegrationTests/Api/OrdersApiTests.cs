using System.Net;
//#if (useJwt)
using System.Net.Http.Headers;
//#endif
using System.Net.Http.Json;
//#if (useJwt)
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
//#endif
//#if (useJwt)
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
//#endif
using Template.Application.Common;
using Template.Application.Orders;
using Template.IntegrationTests.Support;

namespace Template.IntegrationTests.Api;

public sealed class OrdersApiTests : IDisposable
{
    private readonly TemplateWebApplicationFactory _factory = new();

    [Fact]
    public async Task PostOrderCreatesOrder()
    {
        var client = _factory.CreateClient();
        //#if (useJwt)
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken());
        //#endif
        var request = new CreateOrderRequest(
            "customer-001",
            [new CreateOrderItemRequest("Clean Code", 2, 30m)]);

        var response = await client.PostAsJsonAsync("/api/v1/orders", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<OrderResponse>();
        Assert.NotNull(created);
        Assert.Equal(60m, created.TotalAmount);
    }

    [Fact]
    public async Task GetOrderReturnsCreatedOrder()
    {
        var client = _factory.CreateClient();
        //#if (useJwt)
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken());
        //#endif
        var request = new CreateOrderRequest(
            "customer-001",
            [new CreateOrderItemRequest("Clean Code", 1, 30m)]);

        var createResponse = await client.PostAsJsonAsync("/api/v1/orders", request);
        var created = await createResponse.Content.ReadFromJsonAsync<OrderResponse>();

        var response = await client.GetAsync($"/api/v1/orders/{created!.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var order = await response.Content.ReadFromJsonAsync<OrderResponse>();
        Assert.Equal(created.Id, order!.Id);
    }

    [Fact]
    public async Task GetOrdersReturnsPagedOrders()
    {
        var client = _factory.CreateClient();
        //#if (useJwt)
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken());
        //#endif

        await client.PostAsJsonAsync(
            "/api/v1/orders",
            new CreateOrderRequest("customer-001", [new CreateOrderItemRequest("Clean Code", 1, 30m)]));
        await client.PostAsJsonAsync(
            "/api/v1/orders",
            new CreateOrderRequest("customer-002", [new CreateOrderItemRequest("Refactoring", 1, 45m)]));

        var response = await client.GetAsync("/api/v1/orders?page=1&pageSize=1");

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
        //#if (useJwt)
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken());
        //#endif

        var response = await client.PostAsJsonAsync("/api/v1/orders", new CreateOrderRequest("", []));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task GetMissingOrderReturnsProblemDetails()
    {
        var client = _factory.CreateClient();
        //#if (useJwt)
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken());
        //#endif

        var response = await client.GetAsync($"/api/v1/orders/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    //#if (useJwt)
    [Fact]
    public async Task PostOrderRequiresAuthentication()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/orders",
            new CreateOrderRequest("customer-001", [new CreateOrderItemRequest("Clean Code", 1, 30m)]));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
    //#endif

    public void Dispose()
    {
        _factory.Dispose();
    }

    //#if (useJwt)
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
    //#endif
}
