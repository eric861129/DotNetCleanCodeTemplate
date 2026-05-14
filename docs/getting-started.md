# 快速開始

## 前置需求

- .NET 10 SDK
- Docker Desktop 或其他相容 Docker 的 runtime
- PowerShell

## 還原、建置、測試

```powershell
dotnet restore
dotnet build
dotnet test
```

## 啟動 SQL Server

```powershell
docker compose up -d
```

預設資料庫連線字串：

```text
Server=localhost,1433;Database=CleanCodeTemplate;User Id=sa;Password=Your_strong_password123;TrustServerCertificate=True
```

## 啟動 API

```powershell
dotnet run --project src/Template.WebApi
```

Development 環境會啟用 Swagger。

## 啟動 Worker

```powershell
dotnet run --project src/Template.Worker
```

Worker 會輪詢 pending outbox messages，並透過 `IOutboxMessageDispatcher` dispatch。

## 從範本建立新專案

安裝此 repository 為本機 template：

```powershell
dotnet new install .
```

建立新服務：

```powershell
dotnet new clean-code-api-worker -n SampleService
```

驗證產出的服務：

```powershell
dotnet restore SampleService/SampleService.slnx
dotnet build SampleService/SampleService.slnx --no-restore
dotnet test SampleService/SampleService.slnx --no-build
```

移除本機 template：

```powershell
dotnet new uninstall .
```
