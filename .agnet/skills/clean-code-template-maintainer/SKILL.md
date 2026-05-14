---
name: clean-code-template-maintainer
description: Use this skill when modifying dotnet new template behavior in this CleanCodeTemplate repository, including .template.config/template.json, sourceName replacement, solution/project naming, README template instructions, generated project verification, or local template installation problems.
---

# Clean Code Template Maintainer

使用此 skill 維護 `dotnet new clean-code-api-worker` 範本。重點是讓此 repository 同時保持「可直接開發」與「可產生新專案」。

## 必讀文件

開始前閱讀：

1. `docs/template-authoring.md`
2. `docs/getting-started.md`
3. `.template.config/template.json`

## Template 原則

- `sourceName` 是 `Template`。
- 使用者執行 `dotnet new clean-code-api-worker -n SampleService` 後，所有 `Template` 應替換為 `SampleService`。
- Solution 檔名使用 `Template.slnx`，讓產出檔案變成 `<ProjectName>.slnx`。
- 不要把產出的 sample project 留在 repository 裡。
- 不要將 `bin/`、`obj/`、`.git/`、`.vs/`、`.vscode/` 包進 template。

## 修改時檢查

修改 template 後檢查：

- `.template.config/template.json` JSON 格式正確。
- `sourceName` 沒有被誤改。
- 新增檔案中的 namespace 使用 `Template...`，讓 template 能替換。
- README 的指令仍與 `shortName` 一致。
- 產出的 project reference 可 restore/build/test。

## 驗證命令

在 repository 外產生：

```powershell
dotnet new install .
$out = Join-Path $env:TEMP ("CleanCodeTemplateSample-" + [DateTimeOffset]::UtcNow.ToUnixTimeSeconds())
dotnet new clean-code-api-worker -n SampleService -o $out
dotnet restore "$out/SampleService.slnx"
dotnet build "$out/SampleService.slnx" --no-restore
dotnet test "$out/SampleService.slnx" --no-build
```

如果發生 duplicate shortName，通常代表 repository 內有巢狀 template 產物。不要批量刪除；依 AGENT/AGENTS 指示請使用者手動清理，或一次只處理明確單一檔案。

## 回報重點

- 說明 template 變更會如何影響產出專案。
- 附上實際驗證命令與結果。
- 若 template 已安裝到本機，提醒使用者必要時可 `dotnet new uninstall .`。
