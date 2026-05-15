# 測試指南

此範本包含 unit test 與 integration test。

## 測試專案

| 專案 | 用途 |
| --- | --- |
| `Template.UnitTests` | 不依賴 infrastructure，測試 Domain 與 Application 行為。 |
| `Template.IntegrationTests` | 測試 WebApi、EF Core 與 Worker integration 行為。 |

## 執行全部測試

```powershell
dotnet test
```

## Unit Test 策略

Unit test 應快速且聚焦。

目前範例：

- `OrderTests` 驗證訂單總額、空品項拒絕、狀態轉換。
- `ResultTests` 驗證 typed error 與成功/失敗狀態。
- `CreateOrderRequestValidatorTests` 驗證 request validation。
- `CreateOrderUseCaseTests` 驗證建立訂單時也會寫入 outbox message，且 validation failure 不會寫入 repository/outbox。
- `GetOrdersUseCase` 驗證分頁結果。

當測試重點是 Application orchestration，而不是 EF Core，請使用 in-memory fake 實作 Application interface。

## Integration Test 策略

Integration test 使用 `WebApplicationFactory<Program>` 與 SQLite，避免測試時必須啟動 SQL Server。

目前範例：

- `POST /api/orders` 可建立訂單。
- `GET /api/orders/{id}` 可取得已建立訂單。
- `GET /api/orders?page=1&pageSize=20` 可回傳分頁資料。
- validation failure 回傳 `400 ValidationProblemDetails`。
- not found 回傳 `404 ProblemDetails`。
- `/health/live` 與 `/health/ready` 可回傳 healthy。
- response 會帶出 correlation id 與基本 security headers。
- 超過 rate limit 時回傳 `429 Too Many Requests`。
- 未驗證時建立訂單會回傳 `401`。
- Outbox dispatcher 會將 pending message 標記為 processed。

## Template 驗證

修改下列內容時，必須驗證 template：

- `.template.config/template.json`
- project name
- namespace
- solution file
- README template 使用說明

請在 repository 外產生專案：

```powershell
$out = Join-Path $env:TEMP "CleanCodeTemplateSample"
dotnet new clean-code-api-worker -n SampleService -o $out
dotnet restore "$out/SampleService.slnx"
dotnet build "$out/SampleService.slnx" --no-restore
dotnet test "$out/SampleService.slnx" --no-build
```

核心組合也要至少驗證：

```powershell
$minimal = Join-Path $env:TEMP "CleanCodeTemplateMinimal"
dotnet new clean-code-api-worker -n MinimalService -o $minimal --include-worker false --auth none --database none --sample-domain minimal
dotnet build "$minimal/MinimalService.slnx"
dotnet test "$minimal/MinimalService.slnx" --no-build

$sqlite = Join-Path $env:TEMP "CleanCodeTemplateSqlite"
dotnet new clean-code-api-worker -n SqliteService -o $sqlite --database sqlite
dotnet build "$sqlite/SqliteService.slnx"
dotnet test "$sqlite/SqliteService.slnx" --no-build
```
