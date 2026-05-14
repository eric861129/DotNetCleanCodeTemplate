# CleanCodeTemplate

這是一個以 `.NET 10 LTS` 建立的 Clean Code / Pragmatic Clean Architecture 範本專案，適合快速建立同時需要 ASP.NET Core Web API 與背景 Worker 的後端服務。

## 內容包含

- `Template.Domain`：訂單聚合、業務規則、domain exception。
- `Template.Application`：use case contract、request/response record、repository 介面、unit of work、outbox contract。
- `Template.Infrastructure`：EF Core persistence、SQL Server 預設設定、repository 實作、outbox 儲存、migration-ready DbContext。
- `Template.WebApi`：Minimal API endpoint、Swagger、JWT Bearer 驗證、health check、problem details。
- `Template.Worker`：可替換 dispatcher 的 Outbox 背景處理服務。
- `tests`：Domain、Application、API、Worker 與 Outbox 行為測試。

## 本機執行

啟動 SQL Server：

```powershell
docker compose up -d
```

還原、建置、測試：

```powershell
dotnet restore
dotnet build
dotnet test
```

啟動 API：

```powershell
dotnet run --project src/Template.WebApi
```

啟動 Worker：

```powershell
dotnet run --project src/Template.Worker
```

## 作為 dotnet new 範本使用

從此 repository 安裝範本：

```powershell
dotnet new install .
```

建立新服務：

```powershell
dotnet new clean-code-api-worker -n SampleService
```

需要移除本機範本時：

```powershell
dotnet new uninstall .
```

## API

- `GET /health`
- `POST /api/orders`
- `GET /api/orders/{id}`

訂單相關 endpoint 需要 JWT Bearer authentication。開發用 signing key 僅供本機使用，上線前必須更換。

## 資料庫

預設使用 SQL Server：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=CleanCodeTemplate;User Id=sa;Password=Your_strong_password123;TrustServerCertificate=True"
  }
}
```

從 repository 根目錄建立 EF Core migration：

```powershell
dotnet ef migrations add InitialCreate --project src/Template.Infrastructure --startup-project src/Template.WebApi
```

## 文件

- [文件總覽](docs/README.md)
- [Clean Code 專案規範](docs/clean-code-standards.md)
- [架構說明](docs/architecture.md)
- [快速開始](docs/getting-started.md)
- [開發流程](docs/development-workflow.md)
- [測試指南](docs/testing.md)
- [API 說明](docs/api.md)
- [設定說明](docs/configuration.md)
- [範本維護指南](docs/template-authoring.md)
- [維運指南](docs/operations.md)
