---
name: clean-code-testing-verifier
description: Use this skill before claiming work is complete, when adding tests, debugging failing tests, changing behavior, or verifying generated template output in this CleanCodeTemplate repository. It ensures build, unit tests, integration tests, and dotnet new template generation are checked with concrete evidence.
---

# Clean Code Testing Verifier

使用此 skill 來驗證變更是否真的可建置、可測試、可作為 template 使用。不要在沒有新鮮驗證結果時宣稱完成。

## 必讀文件

開始前閱讀：

1. `docs/testing.md`
2. `docs/template-authoring.md`
3. `docs/development-workflow.md`

## 基本驗證

一般程式變更至少執行：

```powershell
dotnet build
dotnet test
```

如果已經 build 過，並且只改文件，可執行：

```powershell
dotnet test --no-build
```

但回報時要清楚說明這是文件變更後的快速驗證。

## Template 驗證

修改下列內容時必須驗證 template：

- `.template.config/template.json`
- solution/project 名稱
- namespace
- `.csproj` reference
- README 或 template 使用流程

請產生到 repository 外的 temp 路徑：

```powershell
$out = Join-Path $env:TEMP ("CleanCodeTemplateSample-" + [DateTimeOffset]::UtcNow.ToUnixTimeSeconds())
dotnet new clean-code-api-worker -n SampleService -o $out
dotnet restore "$out/SampleService.slnx"
dotnet build "$out/SampleService.slnx" --no-restore
dotnet test "$out/SampleService.slnx" --no-build
```

不要把產物放在 repository 裡，避免 `dotnet new install .` 掃到巢狀 template 造成 shortName 衝突。

## 失敗處理

如果驗證失敗：

1. 讀錯誤訊息，不要猜。
2. 判斷失敗屬於 build、unit test、integration test、template generation 或環境問題。
3. 用最小修正處理。
4. 重新跑失敗的命令。
5. 修好後再跑完整相關驗證。

## 回報格式

```markdown
驗證結果：
- `dotnet build`：通過/失敗
- `dotnet test`：通過/失敗，通過數量
- Template generation：通過/未執行/失敗

備註：
- 未執行的驗證與原因
```
