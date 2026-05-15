# CleanCodeTemplate 文件總覽

這份文件集說明如何使用、擴充、測試與維護這個 .NET Clean Architecture 範本專案。

## 文件地圖

| 文件 | 用途 |
| --- | --- |
| [Clean Code 專案規範](clean-code-standards.md) | 本範本的程式碼與專案組織規範。 |
| [架構說明](architecture.md) | 分層責任、依賴方向與 request flow。 |
| [快速開始](getting-started.md) | 安裝工具、執行範本、建立新服務。 |
| [開發流程](development-workflow.md) | 如何新增功能，同時維持架構邊界。 |
| [新增功能食譜](feature-cookbook.md) | 用 Customers 範例示範 Domain、Use Case、Validator、Repository、Endpoint 與測試如何落位。 |
| [測試指南](testing.md) | Unit test、integration test 與 template 驗證策略。 |
| [API 說明](api.md) | 目前 endpoint、驗證需求與 response pattern。 |
| [API Versioning](api-versioning.md) | `/api/v1` route prefix、Swagger v1 grouping 與未來 v2 擴充方式。 |
| [設定說明](configuration.md) | App settings、connection string、JWT、outbox 與環境設定。 |
| [Observability](observability.md) | OpenTelemetry tracing、metrics、OTLP / Console exporter 與 outbox metrics。 |
| [Template Options](template-options.md) | `--include-worker`、`--auth`、`--database`、`--sample-domain` 的產出差異矩陣。 |
| [範本維護指南](template-authoring.md) | `dotnet new` template replacement 的運作方式。 |
| [維運指南](operations.md) | 本機維運、health check、CI 與部署注意事項。 |
| [Production Checklist](production-checklist.md) | 上線前檢查 JWT、secret、migration、Docker、outbox、observability 與 release gate。 |
| [Troubleshooting](troubleshooting.md) | Docker SQL Server、JWT 401、template cache、EF migration 與 CI 常見問題。 |

## 建議閱讀順序

1. 先閱讀 [快速開始](getting-started.md)。
2. 新增功能前閱讀 [架構說明](architecture.md)。
3. 實作新功能時照 [新增功能食譜](feature-cookbook.md)。
4. 建立新專案前確認 [Template Options](template-options.md)。
5. Code review 時使用 [Clean Code 專案規範](clean-code-standards.md) 作為團隊共識。
6. 修改行為前閱讀 [測試指南](testing.md)。
7. 上線前逐項確認 [Production Checklist](production-checklist.md)。
8. 上線監控前閱讀 [Observability](observability.md)。
9. 遇到環境或 template 問題時看 [Troubleshooting](troubleshooting.md)。
