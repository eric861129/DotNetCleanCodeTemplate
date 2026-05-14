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
- 使用足夠強度與長度的 key。
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
| `BatchSize` | 每輪最多處理的 pending outbox messages 數量。 |

## Environment Variables

巢狀設定使用雙底線：

```powershell
$env:ConnectionStrings__DefaultConnection = "Server=..."
$env:Jwt__SigningKey = "replace-with-secret"
$env:Outbox__BatchSize = "50"
```
