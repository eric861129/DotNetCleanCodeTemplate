# Production Checklist

這份清單用來協助團隊把由本範本建立的服務推向正式環境。請把它視為上線前 review 表，而不是一次性的文件；每個新服務都應依實際部署平台調整。

## 1. Secret 與設定

| 項目 | 檢查內容 |
| --- | --- |
| JWT signing key | 已替換 `appsettings.json` 內的 development signing key，正式環境不使用範例值。 |
| Signing key 強度 | `Jwt:SigningKey` 至少 32 字元，且由安全隨機來源產生。 |
| Issuer / Audience | `Jwt:Issuer` 與 `Jwt:Audience` 明確代表此服務或系統邊界。 |
| Connection string | `ConnectionStrings:DefaultConnection` 由 secret storage、環境變數或平台設定注入。 |
| App settings | 正式環境設定與本機設定分離，不把 production secret commit 到 repository。 |
| Options validation | API / Worker 啟動時會跑 `ValidateOnStart()`，部署流程要能及早發現設定錯誤。 |

建議來源：

- Kubernetes Secret / External Secrets。
- Azure Key Vault、AWS Secrets Manager、GCP Secret Manager。
- GitHub Actions / CI/CD platform 的 encrypted secrets。
- Container platform 的 environment variables。

## 2. JWT 與驗證策略

上線前請確認：

- Signing key 已從本機範例值更換。
- Token issuer 與 audience 與身分提供者一致。
- Token lifetime、refresh token、key rotation 由身分系統統一管理。
- API gateway 或 identity provider 的 clock skew 與服務端驗證設定相容。
- 若改用外部 OAuth / OIDC provider，請把 `JwtOptions` 與 authentication registration 調整成對應 provider。

此範本的 JWT 設定是可替換的預設方案，不應阻止團隊接入企業既有 identity platform。

## 3. Database 與 Migration

| 項目 | 檢查內容 |
| --- | --- |
| Provider | Production 使用的 `Database:Provider` 已明確設定，例如 `SqlServer`。 |
| Connection string | 使用正式資料庫帳號，權限符合最小權限原則。 |
| Migration 產生 | EF Core migration 已加入版本控制並通過 review。 |
| Migration 套用 | 已決定由 CI/CD、release job、DBA 流程或人工步驟套用。 |
| Rollback 策略 | schema 變更有 rollback 或 forward-fix 計畫。 |
| Seed data | 若需要初始資料，已區分 production seed 與 test/demo seed。 |

建議不要讓 WebApi 在正式環境啟動時自動套用 migration。比較安全的做法是：

```powershell
dotnet ef database update --project src/Template.Infrastructure --startup-project src/Template.WebApi
```

或在部署流程建立獨立 migration job，讓 migration 的失敗不會被應用程式啟動流程掩蓋。

## 4. Health Check 與 Probe

本範本提供：

| Endpoint | 建議用途 |
| --- | --- |
| `/health/live` | Liveness probe，只確認 process alive。 |
| `/health/ready` | Readiness probe，確認必要依賴可用，目前包含 database readiness。 |
| `/health` | 相容入口，目前等同 ready check。 |

部署平台建議：

- Liveness probe 指向 `/health/live`。
- Readiness probe 指向 `/health/ready`。
- Probe timeout 與 failure threshold 要符合資料庫冷啟動與 migration 流程。
- 若服務新增 message broker、cache、外部 API，請評估是否加入 ready check。
- 不要把慢速外部 API 放進 liveness check，避免平台誤殺健康 process。

## 5. Rate Limit 與邊界保護

正式環境請依實際流量調整：

```json
{
  "RateLimiting": {
    "PermitLimit": 100,
    "WindowSeconds": 60,
    "QueueLimit": 0
  }
}
```

檢查重點：

- API gateway、reverse proxy 與 app 內 rate limit 的責任有明確分工。
- 內部服務呼叫與外部 public traffic 需要不同限制時，應拆 policy。
- `QueueLimit` 若大於 `0`，要確認 request timeout 與使用者體驗。
- `429 Too Many Requests` 是否需要被 frontend、client SDK 或 retry policy 正確處理。
- 觀察 production metrics 後再調整，而不是只沿用本機預設值。

## 6. Docker Image Build

Dockerfile 位置：

```text
src/Template.WebApi/Dockerfile
src/Template.Worker/Dockerfile
```

建議 build：

```powershell
docker build -f src/Template.WebApi/Dockerfile -t sample-service-api:local .
docker build -f src/Template.Worker/Dockerfile -t sample-service-worker:local .
```

上線前確認：

- `.dockerignore` 已排除 `.git`、`bin`、`obj`、測試產物與不必要文件。
- Image tag 包含 commit sha 或 release version。
- CI/CD 會掃描 image vulnerability。
- Runtime image 不包含 SDK 與測試輸出。
- Container 內的 connection string、JWT key、rate limit 設定由環境注入。
- WebApi 與 Worker 可獨立 build、deploy、scale。

## 7. Outbox Dispatcher

範本預設的 dispatcher 是示範用，正式環境應替換為實際 transport。

檢查重點：

- 已把 `LoggingOutboxMessageDispatcher` 替換成 message broker、HTTP callback 或其他正式 dispatcher。
- Dispatch 成功後會標記 `ProcessedOnUtc`。
- Dispatch 失敗會紀錄錯誤與 retry count。
- Retry 上限、dead-letter 策略與告警條件已定義。
- Dispatcher 的行為具備 idempotency，重送不會造成重複副作用。
- Worker 可以獨立水平擴充時，資料庫查詢與鎖定策略不會重複處理同一筆 message。

若你的情境需要更強的一致性或高吞吐量，請把 outbox table、batch size、polling interval 與 message broker 的 delivery guarantee 一起設計。

## 8. Logging、Tracing 與 Metrics

本範本已提供基本 request logging 與 `X-Correlation-Id`。

上線前建議補齊：

| 類型 | 檢查內容 |
| --- | --- |
| Logging | Log 格式、等級、PII masking、retention policy 已確認。 |
| Correlation | Gateway / client request id 已對應到 `X-Correlation-Id`。 |
| Tracing | 若平台支援 OpenTelemetry，WebApi、Worker、EF Core 與外部呼叫應串起 trace。 |
| Metrics | 至少追蹤 request rate、latency、error rate、DB health、outbox pending count、dispatch failure count。 |
| Alerting | 5xx、ready check failure、outbox retry 過高、DB connection failure 有告警。 |

Production log 不應包含：

- JWT token 原文。
- Password、API key、connection string。
- 完整信用卡或個資欄位。
- 大量 request / response body，除非已遮罩且有明確保留策略。

## 9. API 與錯誤處理

確認：

- 所有 validation failure 都回傳 `400 ValidationProblemDetails`。
- Not found、conflict、domain error 由 `Result` / `ProblemDetails` 統一轉換。
- Endpoint 保持 thin，不直接存取 `DbContext`。
- Swagger 在 production 是否開啟已由團隊決定。
- CORS、security headers、HTTPS redirection 與 reverse proxy 設定符合部署平台。

## 10. 測試與 Release Gate

上線前至少執行：

```powershell
dotnet restore Template.slnx
dotnet build Template.slnx --no-restore
dotnet test Template.slnx --no-build
```

如果修改了 template option，也要驗證：

```powershell
dotnet new install . --force
dotnet new clean-code-api-worker -n SampleService
dotnet new clean-code-api-worker -n MinimalService --include-worker false --auth none --database none --sample-domain minimal
dotnet new clean-code-api-worker -n SqliteService --database sqlite
```

Release gate 建議包含：

- Unit tests。
- Integration tests。
- Template generation tests。
- Docker image build。
- Migration dry run 或 migration script review。
- Secret/config validation。
- 部署後 smoke test：`/health/live`、`/health/ready`、核心 API happy path。

## 11. 最後確認

上線前請逐項打勾：

- [ ] JWT signing key 已更換並 secret 化。
- [ ] Connection string 已 secret 化，且不在 repository。
- [ ] Rate limit 已依 production traffic 調整。
- [ ] `/health/live` 與 `/health/ready` 已接到部署平台 probe。
- [ ] Migration 套用策略已確認。
- [ ] WebApi / Worker Docker image 可 build 並已掃描。
- [ ] Outbox dispatcher 已替換為正式 transport。
- [ ] Logging / tracing / metrics / alerting 已接到平台。
- [ ] Production settings 通過 options validation。
- [ ] Release 前 build/test/template verification 全部通過。
