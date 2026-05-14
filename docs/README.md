# CleanCodeTemplate 文件總覽

這份文件集說明如何使用、擴充、測試與維護這個 .NET Clean Architecture 範本專案。

## 文件地圖

| 文件 | 用途 |
| --- | --- |
| [Clean Code 專案規範](clean-code-standards.md) | 本範本的程式碼與專案組織規範。 |
| [架構說明](architecture.md) | 分層責任、依賴方向與 request flow。 |
| [快速開始](getting-started.md) | 安裝工具、執行範本、建立新服務。 |
| [開發流程](development-workflow.md) | 如何新增功能，同時維持架構邊界。 |
| [測試指南](testing.md) | Unit test、integration test 與 template 驗證策略。 |
| [API 說明](api.md) | 目前 endpoint、驗證需求與 response pattern。 |
| [設定說明](configuration.md) | App settings、connection string、JWT、outbox 與環境設定。 |
| [範本維護指南](template-authoring.md) | `dotnet new` template replacement 的運作方式。 |
| [維運指南](operations.md) | 本機維運、health check、CI 與部署注意事項。 |

## 建議閱讀順序

1. 先閱讀 [快速開始](getting-started.md)。
2. 新增功能前閱讀 [架構說明](architecture.md)。
3. Code review 時使用 [Clean Code 專案規範](clean-code-standards.md) 作為團隊共識。
4. 修改行為前閱讀 [測試指南](testing.md)。
