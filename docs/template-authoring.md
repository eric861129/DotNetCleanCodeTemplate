# 範本維護指南

此 repository 同時是一個可執行範例，也是一個 `dotnet new` template。

## Template Metadata

Template 設定位於：

```text
.template.config/template.json
```

重要設定：

| 設定 | 用途 |
| --- | --- |
| `shortName` | CLI 名稱：`clean-code-api-worker`。 |
| `sourceName` | 文字替換來源：`Template`。 |
| `preferNameDirectory` | 依照使用者指定的專案名稱建立目錄。 |

## 命名替換

因為 `sourceName` 是 `Template`，執行：

```powershell
dotnet new clean-code-api-worker -n SampleService
```

會替換：

- `Template.slnx` 變成 `SampleService.slnx`
- `Template.Domain` 變成 `SampleService.Domain`
- `namespace Template...` 變成 `namespace SampleService...`
- project references 與 test namespaces

## Template 驗證

一定要產生到 repository 外的路徑：

```powershell
$out = Join-Path $env:TEMP "CleanCodeTemplateSample"
dotnet new clean-code-api-worker -n SampleService -o $out
dotnet restore "$out/SampleService.slnx"
dotnet build "$out/SampleService.slnx" --no-restore
dotnet test "$out/SampleService.slnx" --no-build
```

不要把產出的專案留在 repository 底下。巢狀產物可能被 `dotnet new install .` 掃到，造成重複 `shortName` 衝突。

## 產出專案排除項目

Template 會排除：

- `.git/**`
- `.vs/**`
- `.vscode/**`
- `**/bin/**`
- `**/obj/**`
- `.template-output/**`
