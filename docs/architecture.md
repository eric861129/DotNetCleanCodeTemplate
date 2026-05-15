# 架構說明

此範本採用 Pragmatic Clean Architecture。它保留清楚的架構邊界，但避免為了形式而加入過多框架儀式。

## Solution 結構

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

## 依賴方向

依賴往內層流動：

```text
WebApi ─────────┐
Worker ────────┼──> Application ───> Domain
Infrastructure ┘
```

`Infrastructure` 也會依賴 `Domain`，因為它需要用 EF Core mapping domain entity。`Domain` 不依賴任何其他專案。

## 各層責任

| 層級 | 負責 | 不負責 |
| --- | --- | --- |
| Domain | Entity、invariant、value-like record、domain exception | HTTP、EF Core、logging、configuration |
| Application | Use case、port/interface、DTO、result model | SQL、controller、worker loop |
| Infrastructure | EF Core、repository、unit of work、outbox persistence | 業務決策 |
| WebApi | Minimal API route、auth middleware、Swagger、health check | 業務規則 |
| Worker | 背景 polling 與 dispatch 排程 | Domain 計算 |

## Request Flow

`POST /api/v1/orders` 流程：

```text
HTTP request
  -> OrderEndpoints
  -> CreateOrderUseCase
  -> Order.Create(...)
  -> IOrderRepository / IOutboxMessageRepository
  -> AppDbContext
  -> SQL Server
```

API 負責轉換 HTTP。Use case 負責協調流程。Domain 保護業務規則。Infrastructure 負責持久化。

## Outbox Flow

```text
CreateOrderUseCase
  -> 儲存 Order
  -> 儲存 OutboxMessage
  -> 一次 commit 兩者

OutboxDispatcherWorker
  -> 建立 scope
  -> 讀取 pending messages
  -> dispatch 每一筆 message
  -> 標記 processed 或 failed
  -> commit 變更
```

預設 dispatcher 只寫 log。實際專案可用 queue、email、webhook 或 event bus 實作替換 `IOutboxMessageDispatcher`。

## 新增功能方式

以新增 invoice 功能為例：

1. 在 `Template.Domain` 加入 domain model 與規則。
2. 在 `Template.Application` 加入 request/response record 與 use case。
3. 在 Application 加入 persistence 或外部服務介面。
4. 在 `Template.Infrastructure` 實作這些介面。
5. 視需求在 WebApi 加 endpoint，或在 Worker 加背景工作。
6. 先補 unit test，再補邊界 integration test。
