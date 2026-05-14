# AI Agent 使用指南

本文件提供 AI Agent 在此 repository 中協助開發時的共同規範。請先閱讀本文件，再進行修改、審查或規劃。

## 語言與溝通

- 回覆以繁體中文為主。
- 保留必要英文技術名詞、程式碼、指令、API route、型別名稱與路徑。
- 說明要精準，避免空泛描述。

## 安全限制

禁止批量刪除文件或目錄。

不要使用：

- `del /s`
- `rd /s`
- `rmdir /s`
- `Remove-Item -Recurse`
- `rm -rf`

需要刪除文件時，只能一次刪除一個明確路徑的文件。

正確示範：

```powershell
Remove-Item "C:\path\to\file.txt"
```

如果需要批量刪除文件，停止操作，並請使用者手動刪除。

## 必讀文件

開始任何程式碼變更前，依任務類型閱讀相關文件：

- `docs/README.md`
- `docs/clean-code-standards.md`
- `docs/architecture.md`
- `docs/development-workflow.md`
- `docs/testing.md`
- `docs/template-authoring.md`

## 本地 Skills

本 repository 的專用 skills 放在：

```text
.agnet/skills/
```

> 注意：此目錄名稱依使用者指定為 `.agnet/skills`。

可用 skills：

| Skill | 使用時機 |
| --- | --- |
| `clean-code-architecture-guardian` | 架構審查、Clean Code 檢查、分層判斷、refactor。 |
| `clean-code-feature-builder` | 新增或修改 API、use case、domain behavior、repository、Worker job。 |
| `clean-code-testing-verifier` | 完成前驗證、測試失敗排查、template generation 驗證。 |
| `clean-code-template-maintainer` | 修改 `.template.config`、template replacement、solution/project 命名。 |
| `clean-code-outbox-worker` | 修改 Outbox、Worker、background processing、dispatcher、retry 行為。 |

## Skill 使用方式

當任務符合 skill 描述時：

1. 讀取對應 `.agnet/skills/<skill-name>/SKILL.md`。
2. 依 skill 指示閱讀必要文件。
3. 執行或規劃變更。
4. 回報時列出重要檔案與驗證結果。

若多個 skill 同時適用，優先順序建議：

1. `clean-code-architecture-guardian`
2. `clean-code-feature-builder`
3. 任務專用 skill，例如 `clean-code-outbox-worker` 或 `clean-code-template-maintainer`
4. `clean-code-testing-verifier`

## 架構規則摘要

- Domain 不可 reference Application、Infrastructure、WebApi、Worker。
- Application 可以 reference Domain。
- Infrastructure 可以 reference Application 與 Domain。
- WebApi 可以 reference Application 與 Infrastructure。
- Worker 可以 reference Application 與 Infrastructure。
- Tests 可以 reference 被驗證的層級。

## 開發守則

- 先理解現有結構，再修改。
- 新行為先補測試。
- 不把業務規則放進 API endpoint、EF mapping 或 Worker loop。
- 不新增模糊命名，例如 `Helper`、`Utility`、`Manager`、`Misc`、`Temp`。
- 不為了形式建立過度抽象。
- 修改 template 時，產生測試專案要放在 repository 外的 temp 路徑。

## 常用驗證

一般變更：

```powershell
dotnet build
dotnet test
```

Template 變更：

```powershell
dotnet new install .
$out = Join-Path $env:TEMP ("CleanCodeTemplateSample-" + [DateTimeOffset]::UtcNow.ToUnixTimeSeconds())
dotnet new clean-code-api-worker -n SampleService -o $out
dotnet restore "$out/SampleService.slnx"
dotnet build "$out/SampleService.slnx" --no-restore
dotnet test "$out/SampleService.slnx" --no-build
```

## 完成回報

完成工作時，回報：

- 變更摘要
- 新增或修改的主要檔案
- 已執行的驗證命令與結果
- 尚未處理或需要使用者決策的項目
