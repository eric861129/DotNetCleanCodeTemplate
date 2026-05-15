# 新增功能食譜

這份食譜示範如何在本範本新增一個功能，並維持 Clean Code / Pragmatic Clean Architecture 的分層邊界。

以下用 `Customers` 功能當例子。實際專案也可以套用到 `Invoices`、`Products`、`Payments` 等功能。

## 目標流程

新增一個功能時，請照這個順序思考：

```text
Domain rule
  -> Application use case
  -> Validator
  -> Repository interface
  -> Infrastructure implementation
  -> WebApi endpoint
  -> Unit tests
  -> Integration tests
```

核心原則：

- 業務規則放 `Domain`。
- 流程協調放 `Application`。
- 資料庫與外部系統放 `Infrastructure`。
- HTTP route、status code 與 request binding 放 `WebApi`。
- 測試先覆蓋 Domain / Application，再補 HTTP integration。

## 1. Domain 放哪裡

建立 feature folder：

```text
src/Template.Domain/Customers/
├── Customer.cs
└── CustomerStatus.cs
```

範例：

```csharp
using Template.Domain.Common;

namespace Template.Domain.Customers;

public sealed class Customer
{
    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public string Email { get; private set; }

    private Customer(Guid id, string name, string email)
    {
        Id = id;
        Name = name;
        Email = email;
    }

    public static Customer Register(string name, string email)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Customer name is required.");
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new DomainException("Customer email is required.");
        }

        return new Customer(Guid.NewGuid(), name.Trim(), email.Trim());
    }
}
```

注意：

- 不要在 Domain 使用 EF Core attribute。
- 不要在 Domain 回傳 HTTP status code。
- 不要在 Domain 注入 repository、logger、configuration。

## 2. Application Use Case 怎麼命名

建立 feature folder：

```text
src/Template.Application/Customers/
├── RegisterCustomerRequest.cs
├── CustomerResponse.cs
├── RegisterCustomerUseCase.cs
├── GetCustomerByIdUseCase.cs
├── CustomerMapping.cs
├── RegisterCustomerRequestValidator.cs
└── ICustomerRepository.cs
```

命名建議：

| 類型 | 命名 |
| --- | --- |
| 建立 | `CreateOrderUseCase`、`RegisterCustomerUseCase` |
| 查詢單筆 | `GetOrderByIdUseCase`、`GetCustomerByIdUseCase` |
| 查詢列表 | `GetOrdersUseCase`、`GetCustomersUseCase` |
| 更新 | `UpdateCustomerProfileUseCase` |
| 刪除或停用 | `DeactivateCustomerUseCase` |

Use case 應該描述使用者意圖，不要命名成模糊的 `CustomerService` 或 `CustomerManager`。

範例 request / response：

```csharp
namespace Template.Application.Customers;

public sealed record RegisterCustomerRequest(
    string Name,
    string Email);

public sealed record CustomerResponse(
    Guid Id,
    string Name,
    string Email);
```

範例 use case：

```csharp
using Template.Application.Common;
using Template.Domain.Common;
using Template.Domain.Customers;

namespace Template.Application.Customers;

public sealed class RegisterCustomerUseCase(
    IValidator<RegisterCustomerRequest> validator,
    ICustomerRepository customerRepository,
    IUnitOfWork unitOfWork)
    : IUseCase<RegisterCustomerRequest, Result<CustomerResponse>>
{
    public async Task<Result<CustomerResponse>> HandleAsync(
        RegisterCustomerRequest request,
        CancellationToken cancellationToken)
    {
        var validationResult = validator.Validate(request);
        if (!validationResult.IsValid)
        {
            return Result<CustomerResponse>.Failure(Error.Validation(validationResult.Errors));
        }

        try
        {
            var customer = Customer.Register(request.Name, request.Email);

            await customerRepository.AddAsync(customer, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<CustomerResponse>.Success(customer.ToResponse());
        }
        catch (DomainException exception)
        {
            return Result<CustomerResponse>.Failure(Error.Domain("Customers.InvalidState", exception.Message));
        }
    }
}
```

## 3. Validator 怎麼寫

Validator 放在 Application feature folder：

```text
src/Template.Application/Customers/RegisterCustomerRequestValidator.cs
```

範例：

```csharp
using Template.Application.Common;

namespace Template.Application.Customers;

public sealed class RegisterCustomerRequestValidator : IValidator<RegisterCustomerRequest>
{
    public ValidationResult Validate(RegisterCustomerRequest request)
    {
        var errors = new List<ValidationError>();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors.Add(new ValidationError(nameof(request.Name), "Customer name is required."));
        }

        if (string.IsNullOrWhiteSpace(request.Email))
        {
            errors.Add(new ValidationError(nameof(request.Email), "Customer email is required."));
        }

        return errors.Count == 0
            ? ValidationResult.Success()
            : ValidationResult.Failure(errors);
    }
}
```

原則：

- Request validation 檢查輸入格式與必要欄位。
- Domain method 檢查業務 invariant。
- 不要只靠 API endpoint 做驗證，因為 use case 也可能被 Worker 或其他入口呼叫。

## 4. Repository Interface 放哪裡

Repository interface 放在 Application feature folder，因為 Application 定義它需要什麼 persistence 能力。

```text
src/Template.Application/Customers/ICustomerRepository.cs
```

範例：

```csharp
using Template.Domain.Customers;

namespace Template.Application.Customers;

public interface ICustomerRepository
{
    Task AddAsync(Customer customer, CancellationToken cancellationToken);

    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
}
```

不要讓 Application 依賴 EF Core `DbContext`，也不要讓 WebApi 直接呼叫 Infrastructure repository。

## 5. Infrastructure 怎麼接

新增 EF repository 與 mapping：

```text
src/Template.Infrastructure/Persistence/
├── CustomerRepository.cs
└── Configurations/CustomerConfiguration.cs
```

範例 repository：

```csharp
using Microsoft.EntityFrameworkCore;
using Template.Application.Customers;
using Template.Domain.Customers;

namespace Template.Infrastructure.Persistence;

public sealed class CustomerRepository(AppDbContext dbContext) : ICustomerRepository
{
    public async Task AddAsync(Customer customer, CancellationToken cancellationToken)
    {
        await dbContext.Customers.AddAsync(customer, cancellationToken);
    }

    public Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return dbContext.Customers.SingleOrDefaultAsync(customer => customer.Id == id, cancellationToken);
    }
}
```

也要更新：

```text
src/Template.Infrastructure/Persistence/AppDbContext.cs
src/Template.Infrastructure/DependencyInjection.cs
```

註冊範例：

```csharp
services.AddScoped<ICustomerRepository, CustomerRepository>();
```

如果你有支援 `--database none` 的 template option，也要思考是否需要 in-memory repository。

## 6. Endpoint 怎麼接

新增 endpoint group：

```text
src/Template.WebApi/Endpoints/CustomerEndpoints.cs
```

範例：

```csharp
using Template.Application.Common;
using Template.Application.Customers;
using Template.WebApi.Http;

namespace Template.WebApi.Endpoints;

public static class CustomerEndpoints
{
    public static IEndpointRouteBuilder MapCustomerEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/customers")
            .WithTags("Customers")
            .RequireRateLimiting("global");

        group.RequireAuthorization();

        group.MapPost("/", async (
            RegisterCustomerRequest request,
            IUseCase<RegisterCustomerRequest, Result<CustomerResponse>> useCase,
            CancellationToken cancellationToken) =>
        {
            var result = await useCase.HandleAsync(request, cancellationToken);

            return result.ToHttpResult(customer => Results.Created($"/api/customers/{customer.Id}", customer));
        })
        .WithName("RegisterCustomer")
        .Produces<CustomerResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status401Unauthorized);

        return app;
    }
}
```

再到：

```text
src/Template.WebApi/Extensions/WebApiApplicationBuilderExtensions.cs
```

掛上：

```csharp
app.MapCustomerEndpoints();
```

原則：

- Endpoint 只處理 HTTP。
- 不在 endpoint 裡建立 domain entity。
- 不在 endpoint 裡直接呼叫 DbContext。
- 用 `ResultExtensions.ToHttpResult(...)` 統一轉換錯誤。

## 7. DI 怎麼補

Application 註冊：

```text
src/Template.Application/DependencyInjection.cs
```

```csharp
services.AddScoped<IValidator<RegisterCustomerRequest>, RegisterCustomerRequestValidator>();
services.AddScoped<IUseCase<RegisterCustomerRequest, Result<CustomerResponse>>, RegisterCustomerUseCase>();
```

Infrastructure 註冊：

```text
src/Template.Infrastructure/DependencyInjection.cs
```

```csharp
services.AddScoped<ICustomerRepository, CustomerRepository>();
```

## 8. Unit Test 怎麼補

建議先補 Domain 與 Application test：

```text
tests/Template.UnitTests/Customers/
├── CustomerTests.cs
├── RegisterCustomerRequestValidatorTests.cs
└── RegisterCustomerUseCaseTests.cs
```

測試重點：

- Domain 建立成功。
- Domain 拒絕不合法狀態。
- Validator 擋掉空白 name/email。
- Use case validation failure 不寫 repository。
- Use case 成功時會寫 repository 並 commit unit of work。

## 9. Integration Test 怎麼補

新增：

```text
tests/Template.IntegrationTests/Api/CustomersApiTests.cs
```

使用既有 factory：

```csharp
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Template.Application.Customers;
using Template.IntegrationTests.Support;

namespace Template.IntegrationTests.Api;

public sealed class CustomersApiTests : IDisposable
{
    private readonly TemplateWebApplicationFactory _factory = new();

    [Fact]
    public async Task PostCustomerCreatesCustomer()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CreateToken());

        var response = await client.PostAsJsonAsync(
            "/api/customers",
            new RegisterCustomerRequest("Alice", "alice@example.com"));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    public void Dispose()
    {
        _factory.Dispose();
    }
}
```

若你的專案關閉 JWT，則不需要設定 `Authorization` header；若 token helper 名稱不同，請沿用 `OrdersApiTests` 目前的建立方式。

## 10. 完成前檢查

一般功能：

```powershell
dotnet restore Template.slnx
dotnet build Template.slnx --no-restore
dotnet test Template.slnx --no-build
```

修改 template 條件或專案結構時，也要驗證：

```powershell
dotnet new install . --force

$out = Join-Path $env:TEMP "CleanCodeTemplateSample"
dotnet new clean-code-api-worker -n SampleService -o $out
dotnet build "$out/SampleService.slnx"
dotnet test "$out/SampleService.slnx" --no-build
```

## 常見錯誤

| 錯誤 | 改法 |
| --- | --- |
| 把業務規則寫在 endpoint | 移到 Domain method。 |
| Application 直接依賴 `AppDbContext` | 改成 Application 定義 repository interface，由 Infrastructure 實作。 |
| Validator 只寫在 WebApi | 改成 Application validator，讓所有入口共用。 |
| Use case 名稱叫 `CustomerService` | 改成描述意圖的 use case，例如 `RegisterCustomerUseCase`。 |
| 測試只打 integration test | 先補 Domain/Application unit test，再補 HTTP integration test。 |
| 新增 feature 忘記 DI | 補 `Application.DependencyInjection` 與 `Infrastructure.DependencyInjection`。 |

## 簡短總結

新增功能時，讓每一層只做自己的事：

- Domain 決定「規則」。
- Application 決定「流程」。
- Infrastructure 決定「怎麼存、怎麼連外部」。
- WebApi 決定「HTTP 怎麼進出」。
- Tests 證明「規則與流程真的可被安全修改」。
