# 維運指南

此文件說明使用此範本建立服務後的本機維運與部署注意事項。

## Health Check

API 提供：

```text
GET /health
```

可用於本機檢查、container probe 與部署後 smoke test。

## 本機 SQL Server

啟動 SQL Server：

```powershell
docker compose up -d
```

停止 SQL Server：

```powershell
docker compose stop
```

此專案的 AGENTS.md 禁止批量遞迴刪除。清理本機產物時，不要使用 recursive delete 指令。

## CI

GitHub Actions workflow：

```text
.github/workflows/ci.yml
```

流程內容：

1. Checkout repository。
2. 安裝 .NET 10。
3. 執行 `dotnet restore`。
4. 執行 `dotnet build --no-restore`。
5. 執行 `dotnet test --no-build --verbosity normal`。

## Logging

API 與 Worker 使用標準 .NET logging。預設 Worker dispatcher 只會記錄 outbox message，不會發送到外部系統。

Production 可以用真正的 transport 實作替換 `LoggingOutboxMessageDispatcher`。

## 部署注意事項

部署產生出的服務前：

- 更換 development JWT signing key。
- 使用 managed secret storage 保存 connection string 與 JWT 設定。
- 加入 EF Core migrations。
- 決定 Worker 要以獨立 process、container 或 hosted service 方式執行。
- 視需求用實際 integration 替換 logging outbox dispatcher。
