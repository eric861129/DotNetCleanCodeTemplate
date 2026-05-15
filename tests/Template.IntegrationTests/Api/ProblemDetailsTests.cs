using System.Net;
//#if (useJwt)
using System.Net.Http.Headers;
//#endif
using System.Net.Http.Json;
//#if (useJwt)
using System.Security.Claims;
using System.Text;
//#endif
using System.Text.Json;
//#if (useJwt)
using Microsoft.AspNetCore.Authentication.JwtBearer;
//#endif
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
//#if (useJwt)
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
//#endif
using Template.Application.Common;
using Template.Application.Orders;
using Template.IntegrationTests.Support;

namespace Template.IntegrationTests.Api;

public sealed class ProblemDetailsTests : IDisposable
{
    private readonly TemplateWebApplicationFactory _factory = new();

    [Fact]
    public async Task ValidationFailureReturnsStableValidationProblemShape()
    {
        var client = CreateAuthenticatedClient(_factory);

        var response = await client.PostAsJsonAsync("/api/v1/orders", new CreateOrderRequest("", []));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await ReadProblemDetailsAsync(response);
        Assert.Equal(400, problem.GetProperty("status").GetInt32());
        Assert.Equal("Validation failed", problem.GetProperty("title").GetString());
        Assert.Equal("One or more validation errors occurred.", problem.GetProperty("detail").GetString());
        Assert.Equal("Validation.Failed", problem.GetProperty("code").GetString());
        Assert.True(problem.GetProperty("errors").TryGetProperty(nameof(CreateOrderRequest.CustomerId), out var customerIdErrors));
        Assert.Contains("Customer id is required.", customerIdErrors.EnumerateArray().Select(error => error.GetString()));
        Assert.True(problem.GetProperty("errors").TryGetProperty(nameof(CreateOrderRequest.Items), out var itemErrors));
        Assert.Contains("An order must contain at least one item.", itemErrors.EnumerateArray().Select(error => error.GetString()));
    }

    [Fact]
    public async Task NotFoundReturnsStableProblemDetailsShape()
    {
        var client = CreateAuthenticatedClient(_factory);

        var response = await client.GetAsync($"/api/v1/orders/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await ReadProblemDetailsAsync(response);
        Assert.Equal(404, problem.GetProperty("status").GetInt32());
        Assert.Equal("Resource not found", problem.GetProperty("title").GetString());
        Assert.Equal("Order was not found.", problem.GetProperty("detail").GetString());
        Assert.Equal("Orders.NotFound", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task ConflictReturnsStableProblemDetailsShape()
    {
        using var factory = new TemplateWebApplicationFactory(configureTestServices: services =>
        {
            services.RemoveAll<IUseCase<CreateOrderRequest, Result<OrderResponse>>>();
            services.AddScoped<IUseCase<CreateOrderRequest, Result<OrderResponse>>, ConflictCreateOrderUseCase>();
        });
        var client = CreateAuthenticatedClient(factory);

        var response = await client.PostAsJsonAsync(
            "/api/v1/orders",
            new CreateOrderRequest("customer-001", [new CreateOrderItemRequest("Clean Code", 1, 30m)]));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await ReadProblemDetailsAsync(response);
        Assert.Equal(409, problem.GetProperty("status").GetInt32());
        Assert.Equal("Conflict", problem.GetProperty("title").GetString());
        Assert.Equal("Order already exists.", problem.GetProperty("detail").GetString());
        Assert.Equal("Orders.Conflict", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task DomainErrorReturnsStableProblemDetailsShape()
    {
        using var factory = new TemplateWebApplicationFactory(configureTestServices: services =>
        {
            services.RemoveAll<IUseCase<CreateOrderRequest, Result<OrderResponse>>>();
            services.AddScoped<IUseCase<CreateOrderRequest, Result<OrderResponse>>, DomainErrorCreateOrderUseCase>();
        });
        var client = CreateAuthenticatedClient(factory);

        var response = await client.PostAsJsonAsync(
            "/api/v1/orders",
            new CreateOrderRequest("customer-001", [new CreateOrderItemRequest("Clean Code", 1, 30m)]));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await ReadProblemDetailsAsync(response);
        Assert.Equal(400, problem.GetProperty("status").GetInt32());
        Assert.Equal("Business rule violation", problem.GetProperty("title").GetString());
        Assert.Equal("Only pending orders can be marked as paid.", problem.GetProperty("detail").GetString());
        Assert.Equal("Orders.InvalidState", problem.GetProperty("code").GetString());
    }

    [Fact]
    public async Task UnhandledExceptionReturnsProblemDetails()
    {
        using var factory = new TemplateWebApplicationFactory(configureTestServices: services =>
        {
            services.RemoveAll<IUseCase<GetOrderByIdRequest, Result<OrderResponse>>>();
            services.AddScoped<IUseCase<GetOrderByIdRequest, Result<OrderResponse>>, ThrowingGetOrderByIdUseCase>();
        });
        var client = CreateAuthenticatedClient(factory);

        var response = await client.GetAsync($"/api/v1/orders/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await ReadProblemDetailsAsync(response);
        Assert.Equal(500, problem.GetProperty("status").GetInt32());
        Assert.False(string.IsNullOrWhiteSpace(problem.GetProperty("title").GetString()));
    }

    public void Dispose()
    {
        _factory.Dispose();
    }

    private static async Task<JsonElement> ReadProblemDetailsAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(content);

        return document.RootElement.Clone();
    }

    private static HttpClient CreateAuthenticatedClient(TemplateWebApplicationFactory factory)
    {
        var client = factory.CreateClient();
        //#if (useJwt)
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken());
        //#endif

        return client;
    }

    private sealed class ConflictCreateOrderUseCase : IUseCase<CreateOrderRequest, Result<OrderResponse>>
    {
        public Task<Result<OrderResponse>> HandleAsync(CreateOrderRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<OrderResponse>.Failure(
                Error.Conflict("Orders.Conflict", "Order already exists.")));
        }
    }

    private sealed class DomainErrorCreateOrderUseCase : IUseCase<CreateOrderRequest, Result<OrderResponse>>
    {
        public Task<Result<OrderResponse>> HandleAsync(CreateOrderRequest request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<OrderResponse>.Failure(
                Error.Domain("Orders.InvalidState", "Only pending orders can be marked as paid.")));
        }
    }

    private sealed class ThrowingGetOrderByIdUseCase : IUseCase<GetOrderByIdRequest, Result<OrderResponse>>
    {
        public Task<Result<OrderResponse>> HandleAsync(GetOrderByIdRequest request, CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("ProblemDetails test exception.");
        }
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
