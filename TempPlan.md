# CleanCodeTemplate 萬用化加強計畫

## Summary
將目前範本從「固定 API + Worker + SQL Server + JWT + Orders 範例」提升為更通用的後端服務範本。實作分三批：先補標準 API 能力，再補運維與容器化，最後補 `dotnet new` 條件式 template options。

## Key Changes
- **標準錯誤處理 + ProblemDetails**
  - 將 `Result<T>` 擴充為包含 `ErrorCode`、`ErrorType`、`Message`、可選 `ValidationErrors`。
  - 新增 WebApi result mapper，統一轉換為 `ProblemDetails` / `ValidationProblemDetails`。
  - Domain validation、request validation、not found、conflict 等錯誤使用一致 HTTP status mapping。

- **Request Validation**
  - 採用自訂輕量 validator，不引入 FluentValidation。
  - 新增 `IValidator<TRequest>`、`ValidationResult`、`ValidationError`。
  - Application use case 在建立 domain 前先驗證 request。
  - `CreateOrderRequest` 驗證 customer id、items、product name、quantity、unit price。

- **Orders Pagination 範例**
  - 新增 `GET /api/orders?page=1&pageSize=20`。
  - 新增 `PagedResult<T>` 與 `GetOrdersRequest` / `GetOrdersUseCase`。
  - `IOrderRepository` 增加分頁查詢與 count 查詢。
  - WebApi endpoint 回傳分頁結果，保持 endpoint thin。

- **Options Validation**
  - 新增 `DatabaseOptions`、強化 `JwtOptions`、`OutboxOptions`。
  - Startup 使用 `ValidateOnStart()` 驗證必要設定。
  - JWT signing key、issuer、audience、database provider、outbox batch/polling interval 都要有明確驗證規則。

- **Health Check 分級**
  - 新增：
    - `GET /health/live`
    - `GET /health/ready`
  - `/health/live` 只確認 process alive。
  - `/health/ready` 檢查 database readiness。
  - 保留 `/health` 作為相容入口，指向 ready check 或文件標示為 legacy/simple check。

- **Dockerfile**
  - 新增 WebApi Dockerfile。
  - 新增 Worker Dockerfile。
  - 更新 `docker-compose.yml`，保留 SQL Server，並提供可選 API/Worker service 範例。
  - 文件補上 build/run image 指令。

- **Template Options**
  - `.template.config/template.json` 新增核心四選項：
    - `--include-worker true|false`
    - `--auth jwt|none`
    - `--database sqlserver|sqlite|none`
    - `--sample-domain orders|minimal`
  - 條件式包含/排除 Worker 專案、JWT 設定、EF Core persistence、Orders 範例檔案與測試。
  - Template 驗證至少覆蓋預設完整組合與 minimal 組合。

## Public APIs / Interfaces / Types
- 新增 Application contracts:
  - `Error`
  - `ErrorType`
  - `Result<T>` enhanced shape
  - `ValidationError`
  - `ValidationResult`
  - `IValidator<TRequest>`
  - `PagedResult<T>`
- 新增 Orders use case:
  - `GetOrdersRequest`
  - `GetOrdersUseCase`
- 擴充 repository:
  - `IOrderRepository.GetPagedAsync(...)`
  - `IOrderRepository.CountAsync(...)`
- 新增 WebApi endpoints:
  - `GET /api/orders`
  - `GET /health/live`
  - `GET /health/ready`
- 新增 options:
  - `DatabaseOptions`
  - 強化 `JwtOptions`
  - 強化 `OutboxOptions`

## Implementation Plan
- **Batch 1: API 基礎能力**
  - 擴充 `Result<T>` 與錯誤模型。
  - 新增 ProblemDetails mapping helper。
  - 新增輕量 validation contracts 與 `CreateOrderRequestValidator`。
  - 修改 `CreateOrderUseCase` / `GetOrderByIdUseCase` 使用 typed errors。
  - 新增 Orders 分頁 use case、repository 查詢與 endpoint。

- **Batch 2: 運維能力**
  - 導入 options validation。
  - 補 live/ready health checks 與 DB readiness。
  - 新增 WebApi/Worker Dockerfile。
  - 更新 docker compose 與繁中 docs。

- **Batch 3: Template Options**
  - 更新 `.template.config/template.json` symbols 與 conditional sources。
  - 讓 `include-worker=false` 產出時不包含 Worker project/reference。
  - 讓 `auth=none` 產出時移除 JWT package、options、authentication middleware 與 endpoint authorization。
  - 讓 `database=none` 產出時移除 EF Core persistence 與 DB health readiness，改用 minimal in-memory 或 no-op sample path。
  - 讓 `sample-domain=minimal` 產出乾淨骨架，不包含 Orders demo endpoints/use cases/tests。
  - 補 template options 文件與驗證指令。

## Test Plan
- Unit tests:
  - `Result<T>` 成功/失敗狀態與 typed error。
  - `CreateOrderRequestValidator` 驗證空 customer、空 items、空 product name、非正數 quantity/unit price。
  - `CreateOrderUseCase` validation failure 不寫 repository/outbox。
  - `GetOrdersUseCase` 回傳 `PagedResult<OrderResponse>`。

- Integration tests:
  - validation failure 回傳 `400 ValidationProblemDetails`。
  - not found 回傳 `404 ProblemDetails`。
  - `GET /api/orders?page=1&pageSize=20` 回傳分頁資料。
  - `/health/live` 回傳 healthy。
  - `/health/ready` 在 SQLite test database 可 healthy。
  - JWT 開啟時 order endpoints 仍需要 auth。

- Template verification:
  - 預設組合：`include-worker=true auth=jwt database=sqlserver sample-domain=orders` 可 restore/build/test。
  - Minimal 組合：`include-worker=false auth=none database=none sample-domain=minimal` 可 restore/build/test。
  - SQLite 組合：`database=sqlite` 可 restore/build/test。
  - 驗證輸出一律產生到 repository 外的 temp path。

## Assumptions
- Request validation 採用自訂輕量 validator，不引入 FluentValidation。
- Template options 採核心四選項，不加入 docker/ci/docs 開關，避免條件爆炸。
- 預設產出仍維持目前完整服務體驗：API + Worker + JWT + SQL Server + Orders。
- `database=none` 與 `sample-domain=minimal` 目標是產出可編譯骨架，不提供完整 business demo。
- 所有文件維持繁體中文為主，保留必要英文技術名詞。
- 遵守 repository 規則：禁止批量刪除檔案或目錄。
