# Clean Code 專案規範

這份文件定義使用此範本建立專案時應遵守的 Clean Code 標準。目標不是寫出炫技的程式，而是讓團隊成員，甚至幾個月後的自己，都能快速理解、測試與安全修改。

## 核心原則

### 命名清楚

名稱必須表達意圖。避免縮寫、臨時命名，以及模糊的技術分類。

不建議：

```csharp
var d = GetData();
```

建議：

```csharp
var activeUsers = GetActiveUsers();
```

方法名稱應清楚描述行為：

```csharp
CalculateOrderTotal()
ValidateUserPermission()
SendPasswordResetEmail()
```

好的名稱要能回答三個問題：這是什麼、它做什麼、為什麼放在這裡。

### 方法職責單一

一個方法應該只做一件明確的事，而且維持在同一層抽象。

不建議：

```csharp
public void ProcessOrder(Order order)
{
    ValidateOrder(order);
    CalculatePrice(order);
    SaveOrder(order);
    SendEmail(order);
}
```

建議：

```csharp
public void ProcessOrder(Order order)
{
    orderValidator.Validate(order);
    orderPriceCalculator.Calculate(order);
    orderRepository.Save(order);
    orderNotificationService.SendConfirmation(order);
}
```

方法可以協調流程，但細節責任應拆給有清楚名稱的協作者。

### 專案結構清楚

此範本使用 Clean Architecture 邊界：

```text
src/
├── Template.Domain
├── Template.Application
├── Template.Infrastructure
├── Template.WebApi
└── Template.Worker
tests/
├── Template.UnitTests
└── Template.IntegrationTests
```

| 層級 | 責任 |
| --- | --- |
| Domain | 核心業務規則與 invariant。 |
| Application | Use case、request/response contract、流程協調。 |
| Infrastructure | 資料庫、檔案系統、外部 API 與技術細節。 |
| WebApi | HTTP 輸入輸出、驗證 middleware、endpoint mapping。 |
| Worker | 背景執行與排程處理。 |
| Tests | 行為的 unit test 與 integration test。 |

### 避免重複程式碼

不要把業務規則複製到多個地方。重複出現的概念應給它一個清楚名稱。

不建議：

```csharp
if (user.Role == "Admin" || user.Role == "Manager")
{
    // allow access
}
```

建議：

```csharp
if (user.HasManagementPermission())
{
    // allow access
}
```

只有在兩段程式代表不同概念，只是目前剛好長得相似時，才接受保留重複。

### 依賴關係清楚

業務邏輯應依賴抽象，不直接依賴基礎設施細節。

不建議：

```csharp
public class OrderService
{
    private readonly SqlConnection _connection = new("...");
}
```

建議：

```csharp
public class OrderService
{
    private readonly IOrderRepository _orderRepository;

    public OrderService(IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }
}
```

Infrastructure 實作介面。Domain 與 Application 不依賴 EF Core、SQL Server、HTTP 或背景工作型別。

### 容易測試

業務行為必須能在不啟動 API、Worker 或 SQL Server 的情況下測試。業務規則放在 Domain，使用案例流程放在 Application。

API endpoint 應保持薄：

```csharp
group.MapPost("/", async (
    CreateOrderRequest request,
    IUseCase<CreateOrderRequest, Result<OrderResponse>> useCase,
    CancellationToken cancellationToken) =>
{
    var result = await useCase.HandleAsync(request, cancellationToken);
    return result.IsSuccess
        ? Results.Created($"/api/orders/{result.Value.Id}", result.Value)
        : Results.BadRequest(new { message = result.Error });
});
```

Endpoint 只處理 HTTP input/output 與 use case result 轉換，不計算總額、不直接改 domain 狀態、不寫 SQL。

### 註解少但有價值

優先讓程式碼本身清楚，不用註解重複描述程式碼。

不建議：

```csharp
// check if user is active
if (u.Status == 1)
{
}
```

建議：

```csharp
if (user.IsActive)
{
}
```

註解應用來說明設計決策、外部限制、非直覺行為與取捨。

## 命名規則

- 使用領域語言：`Order`、`OrderItem`、`OutboxMessage`、`CreateOrderUseCase`。
- 避免模糊後綴：`Helper`、`Utility`、`Manager`、`CommonService`、`Misc`。
- Interface 應描述角色：`IOrderRepository`、`IUnitOfWork`、`IUseCase<TRequest, TResponse>`。
- Request/response record 應描述 API 或 use case 意圖：`CreateOrderRequest`、`OrderResponse`。
- 測試名稱應描述行為：`CreateCalculatesTotalAmountFromItems`。

## 分層規則

- Domain 不可 reference Application、Infrastructure、WebApi 或 Worker。
- Application 可以 reference Domain。
- Infrastructure 可以 reference Application 與 Domain。
- WebApi 可以 reference Application 與 Infrastructure。
- Worker 可以 reference Application 與 Infrastructure。
- Tests 可以 reference 被驗證的層級。

## 方法與類別規則

- 方法應小到可以輕鬆閱讀；可行時避免需要捲動很久。
- 一個 class 應只有一個主要變更理由。
- Request/response 優先使用 immutable record。
- 必要依賴使用 constructor injection。
- I/O 一律優先使用 async API。
- 預期中的業務失敗使用明確 result type。
- Domain invariant 違反與非預期錯誤可以使用 exception。

## Code Review 檢查表

| 問題 | 好的狀況 |
| --- | --- |
| 新人能不能快速理解這段程式放在哪裡？ | 可以 |
| 名稱是否能表達意圖？ | 可以 |
| 方法是否只做一件清楚的事？ | 大多是 |
| 業務規則是否放在 Domain 或 Application？ | 是 |
| Infrastructure 細節是否被隔離？ | 是 |
| 行為是否容易寫 unit test？ | 是 |
| 是否避免 Helper、Utility、Temp、Misc 這類模糊命名？ | 是 |
| 新行為是否有測試保護？ | 是 |

## 總結

Clean Code 專案應該具備高可讀性、明確責任、低耦合、容易測試與容易維護。程式碼應像活的文件：清楚、可理解、可安全修改，並且能隨著系統成長。
