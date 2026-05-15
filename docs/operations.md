# 維運指南

此文件整理本範本在本機開發、容器化與 CI 的基本維運方式。

## Health Check

API 端點：

```text
GET /health
GET /health/live
GET /health/ready
```

建議用法：

| Endpoint | 用途 |
| --- | --- |
| `/health/live` | Liveness probe，只確認 process alive。 |
| `/health/ready` | Readiness probe，確認資料庫連線可用。 |
| `/health` | 相容入口，目前等同 ready check。 |

## Docker Compose

只啟動 SQL Server：

```powershell
docker compose up -d sqlserver
```

啟動 SQL Server、API 與 Worker：

```powershell
docker compose --profile app up -d --build
```

停止服務：

```powershell
docker compose stop
```

Dockerfile 位置：

```text
src/Template.WebApi/Dockerfile
src/Template.Worker/Dockerfile
```

## CI

GitHub Actions workflow：

```text
.github/workflows/ci.yml
```

目前流程：

1. Checkout repository。
2. 安裝 .NET 10。
3. 執行 `dotnet restore`。
4. 執行 `dotnet build --no-restore`。
5. 執行 `dotnet test --no-build --verbosity normal`。

## Logging

API 與 Worker 使用標準 .NET logging。API request log 會包含 HTTP method、path、status code、elapsed milliseconds 與 correlation id。Worker dispatcher 範例會記錄 outbox message dispatch；正式環境應替換 `LoggingOutboxMessageDispatcher`，串接實際 message broker、HTTP callback 或其他外部系統。

## Correlation Id

API 會接受並回傳 `X-Correlation-Id`：

```powershell
curl -H "X-Correlation-Id: local-debug-001" http://localhost:5000/health/live -i
```

如果 request 沒有提供，API 會自動產生一個新的 correlation id，並放入 response header 與 request logging scope。

## Rate Limiting

範本使用 ASP.NET Core 內建 fixed-window limiter。預設值適合本機開發，正式環境請依服務流量調整：

```json
{
  "RateLimiting": {
    "PermitLimit": 100,
    "WindowSeconds": 60,
    "QueueLimit": 0
  }
}
```

## 上線前檢查

- 更換 development JWT signing key。
- 使用 secret storage 管理 connection string 與 JWT 設定。
- 建立並套用 EF Core migrations。
- 規劃 Worker 是獨立 process、container，或由平台排程啟動。
- 將範例 dispatcher 替換成正式 message transport。
- 依部署平台設定 `/health/live` 與 `/health/ready` probe。
- 依 API 流量調整 `RateLimiting`。
- 將 reverse proxy / gateway 的 request id 對應到 `X-Correlation-Id`。
