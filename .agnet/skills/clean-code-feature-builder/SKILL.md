---
name: clean-code-feature-builder
description: Use this skill when adding or changing a feature in this CleanCodeTemplate repository, especially API endpoints, use cases, domain behavior, repositories, EF Core persistence, or Worker jobs. It guides AI agents to implement changes test-first, place code in the correct Clean Architecture layer, and follow the Traditional Chinese Clean Code standards.
---

# Clean Code Feature Builder

使用此 skill 新增或修改功能。目標是讓每個功能都能遵守 Clean Code、分層清楚、容易測試。

## 必讀文件

開始前閱讀：

1. `docs/clean-code-standards.md`
2. `docs/architecture.md`
3. `docs/development-workflow.md`
4. `docs/testing.md`

## 開發流程

1. 釐清行為
   - 功能要解決什麼使用情境？
   - 輸入、輸出、失敗狀況是什麼？
   - 是否需要 API、Worker、資料庫或外部系統？

2. 先寫測試
   - Domain rule：寫在 `tests/Template.UnitTests`。
   - Application use case：寫 use case unit test，可用 in-memory fake。
   - WebApi 行為：寫 `WebApplicationFactory<Program>` integration test。
   - Worker 行為：寫 Worker / dispatcher integration test。

3. 放在正確層級
   - Domain：entity、invariant、domain exception。
   - Application：request/response record、use case、interface。
   - Infrastructure：EF Core、repository、unit of work、外部服務實作。
   - WebApi：route、auth、HTTP status mapping。
   - Worker：background loop、polling、dispatch orchestration。

4. 實作最小可用功能
   - 先讓測試轉綠。
   - 再整理命名、重複與邊界。
   - 不加入目前需求沒有要求的功能。

5. 驗證
   - 至少執行 `dotnet build` 與 `dotnet test`。
   - 如果修改 template 行為，也要在 repository 外產生範本並 build/test。

## 新功能預設檔案模式

新增一個 use case 時，優先採用：

```text
src/Template.Domain/<Feature>/
src/Template.Application/<Feature>/
src/Template.Infrastructure/Persistence/
src/Template.WebApi/Endpoints/
tests/Template.UnitTests/<Feature>/
tests/Template.IntegrationTests/<Feature>/
```

不要建立模糊資料夾，例如 `Helpers`、`Utils`、`Managers`、`Misc`。

## 完成回報格式

```markdown
已完成：
- 變更摘要
- 重要檔案

驗證：
- `dotnet build`
- `dotnet test`

注意事項：
- 若有未完成或需使用者決策的項目，明確列出
```
