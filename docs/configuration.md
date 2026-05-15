# 設定說明

此範本使用標準 ASP.NET Core configuration，來源包含 `appsettings.json`、環境專用設定檔、user secrets、environment variables 與 command-line arguments。

## ConnectionStrings

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=CleanCodeTemplate;User Id=sa;Password=Your_strong_password123;TrustServerCertificate=True"
  }
}
```

`DefaultConnection` 由 `Template.Infrastructure` 用來設定 `AppDbContext`。

## Database

```json
{
  "Database": {
    "Provider": "SqlServer"
  }
}
```

支援 provider：

- `SqlServer`：本機開發與接近 production 的預設選項。
- `Sqlite`：integration test 使用。
- `none`：只在 `dotnet new --database none` 產生的骨架中使用，不註冊 EF Core persistence。

`Database:Provider` 會在啟動時驗證；目前完整範本允許 `SqlServer` 與 `Sqlite`。

## JWT

```json
{
  "Jwt": {
    "Issuer": "CleanCodeTemplate",
    "Audience": "CleanCodeTemplate",
    "SigningKey": "local-development-signing-key-please-change"
  }
}
```

Production 建議：

- 將 `SigningKey` 放在 secret storage。
- 使用足夠強度與長度的 key；範本啟動時要求至少 32 個字元。
- 每個服務維持穩定的 issuer 與 audience。

## Outbox

```json
{
  "Outbox": {
    "PollingIntervalSeconds": 5,
    "BatchSize": 20
  }
}
```

| 設定 | 用途 |
| --- | --- |
| `PollingIntervalSeconds` | Worker 每次 polling 之間的延遲秒數。 |
| `BatchSize` | 每輪最多處理的 pending outbox messages 數量，允許範圍為 `1` 到 `100`。 |

`Outbox` options 會在 Worker 啟動時驗證，避免部署後才發現 polling 設定無效。

## RateLimiting

```json
{
  "RateLimiting": {
    "PermitLimit": 100,
    "WindowSeconds": 60,
    "QueueLimit": 0
  }
}
```

| 設定 | 用途 |
| --- | --- |
| `PermitLimit` | 每個 fixed window 允許的 request 數量。 |
| `WindowSeconds` | fixed window 秒數。 |
| `QueueLimit` | 超過限制時可排隊的 request 數量；預設 `0` 代表直接回 `429`。 |

`RateLimiting` options 會在 API 啟動時驗證，`PermitLimit` 與 `WindowSeconds` 必須大於 `0`，`QueueLimit` 不可小於 `0`。

## Environment Variables

巢狀設定使用雙底線：

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=..."
$env:Jwt__SigningKey = "replace-with-secret"
$env:Outbox__BatchSize = "50"
$env:RateLimiting__PermitLimit = "200"
```
