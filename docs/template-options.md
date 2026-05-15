# Template Options

這份文件整理 `dotnet new clean-code-api-worker` 的主要選項，以及不同組合會產生哪些專案、套件、設定與範例程式碼。

## 快速範例

預設完整服務：

```powershell
dotnet new clean-code-api-worker -n SampleService
```

等同：

```powershell
dotnet new clean-code-api-worker -n SampleService `
  --include-worker true `
  --auth jwt `
  --database sqlserver `
  --sample-domain orders
```

極簡骨架：

```powershell
dotnet new clean-code-api-worker -n MinimalService `
  --include-worker false `
  --auth none `
  --database none `
  --sample-domain minimal
```

SQLite 版本：

```powershell
dotnet new clean-code-api-worker -n SqliteService --database sqlite
```

## Option 總覽

| Option | 可選值 | 預設 | 用途 |
| --- | --- | --- | --- |
| `--include-worker` | `true`、`false` | `true` | 是否產生背景 Worker 與 outbox dispatcher。 |
| `--auth` | `jwt`、`none` | `jwt` | API 是否啟用 JWT Bearer authentication。 |
| `--database` | `sqlserver`、`sqlite`、`none` | `sqlserver` | Persistence provider 與 EF Core 是否啟用。 |
| `--sample-domain` | `orders`、`minimal` | `orders` | 是否包含 Orders 範例 domain、use cases、endpoints 與 tests。 |

## 產出差異矩陣

| 組合 | 產出內容 | 適合情境 |
| --- | --- | --- |
| 預設完整組合 | WebApi、Worker、JWT、SQL Server、Orders 範例、unit/integration tests。 | 大多數後端服務起手式，需要 API、背景處理與資料庫。 |
| `--include-worker false` | 不產生 `src/*Worker`，也不產生 Worker integration tests。 | 純 API 服務，暫時不需要背景任務。 |
| `--auth none` | 不產生 JWT options，不註冊 authentication middleware，Orders endpoint 不要求 authorization。 | 內部 prototype、由 gateway 統一處理 auth、或尚未接 identity provider。 |
| `--database sqlite` | EF Core 使用 SQLite provider，保留 persistence 與 DB health check。 | 小型服務、local-first service、或偏好 SQLite 的部署環境。 |
| `--database none` | 移除 EF Core persistence、database options、DB health check；Orders 範例改走 in-memory path。 | 先建立乾淨架構骨架，資料來源稍後再決定。 |
| `--sample-domain minimal` | 移除 Orders domain/application/endpoints/tests，只保留分層骨架與基礎 API 能力。 | 新團隊要從空白業務模型開始，不想先刪範例碼。 |

## `--include-worker`

預設值：`true`

`true` 時包含：

- `src/Template.Worker`
- Worker project reference 與 solution entry
- Outbox polling worker
- `IOutboxMessageDispatcher` 使用範例
- Worker integration tests

`false` 時移除：

- Worker project
- Worker integration tests
- solution 中對 Worker 的 reference

使用建議：

- 有 outbox、排程、event dispatch、queue consumer 時保留 Worker。
- 只有同步 HTTP API 時可以關閉。
- 即使先關閉，未來仍可依現有 `Template.Worker` 範例手動加回背景服務。

## `--auth`

預設值：`jwt`

| 值 | 產出差異 |
| --- | --- |
| `jwt` | 保留 `JwtOptions`、JWT package、`AddAuthentication()`、`UseAuthentication()`、`UseAuthorization()` 與 endpoint authorization。 |
| `none` | 移除 JWT options 與 authentication middleware，endpoint 不要求 bearer token。 |

使用建議：

- Production API 預設使用 `jwt`。
- 如果 auth 統一由 API Gateway、BFF 或 service mesh 處理，可選 `none`，但仍要在部署邊界補上保護。
- 若未來改接 OIDC provider，請以 `jwt` 產出後替換 WebApi authentication registration。

## `--database`

預設值：`sqlserver`

| 值 | Persistence | Health Check | 適合情境 |
| --- | --- | --- | --- |
| `sqlserver` | EF Core SQL Server | `/health/ready` 檢查 database | 一般正式後端服務。 |
| `sqlite` | EF Core SQLite | `/health/ready` 檢查 database | 輕量部署、local-first、測試與 demo。 |
| `none` | 不產生 EF Core persistence | ready check 不檢查 DB | 尚未決定資料庫、或此服務不需要資料庫。 |

注意事項：

- `database=none` 主要是可編譯骨架，不代表 production storage 策略。
- 若選 `sqlite`，請確認 deployment 環境的檔案系統持久化策略。
- 若選 `sqlserver`，請在上線前建立 migration 與 migration 套用流程。

## `--sample-domain`

預設值：`orders`

| 值 | 產出差異 |
| --- | --- |
| `orders` | 包含 Orders aggregate、use cases、validators、repository、endpoints、unit tests、integration tests。 |
| `minimal` | 移除 Orders 範例，只保留 Clean Architecture 骨架、基礎 middleware、health checks 與測試框架。 |

使用建議：

- 想學習本範本的功能落位方式，選 `orders`。
- 要直接建立正式專案並避免刪除範例功能，選 `minimal`。
- 新功能開發請參考 [新增功能食譜](feature-cookbook.md)。

## 常用組合

| 目標 | 指令 |
| --- | --- |
| 完整 API + Worker + SQL Server + JWT | `dotnet new clean-code-api-worker -n BillingService` |
| 純 API + SQL Server + JWT | `dotnet new clean-code-api-worker -n CatalogService --include-worker false` |
| 純 API 極簡骨架 | `dotnet new clean-code-api-worker -n InternalTool --include-worker false --auth none --database none --sample-domain minimal` |
| SQLite demo service | `dotnet new clean-code-api-worker -n DemoService --database sqlite` |
| 從空白 domain 開始但保留 API hardening | `dotnet new clean-code-api-worker -n NewService --sample-domain minimal` |

## 驗證產出

每次修改 template option 或 conditional source 後，至少驗證以下三組：

```powershell
dotnet new install . --force

$default = Join-Path $env:TEMP "CleanCodeTemplateDefault"
dotnet new clean-code-api-worker -n SampleService -o $default
dotnet build "$default/SampleService.slnx"
dotnet test "$default/SampleService.slnx" --no-build

$minimal = Join-Path $env:TEMP "CleanCodeTemplateMinimal"
dotnet new clean-code-api-worker -n MinimalService -o $minimal --include-worker false --auth none --database none --sample-domain minimal
dotnet build "$minimal/MinimalService.slnx"
dotnet test "$minimal/MinimalService.slnx" --no-build

$sqlite = Join-Path $env:TEMP "CleanCodeTemplateSqlite"
dotnet new clean-code-api-worker -n SqliteService -o $sqlite --database sqlite
dotnet build "$sqlite/SqliteService.slnx"
dotnet test "$sqlite/SqliteService.slnx" --no-build
```

產出請放在 repository 外的 temp path，避免把 generated files 混入 template source。
