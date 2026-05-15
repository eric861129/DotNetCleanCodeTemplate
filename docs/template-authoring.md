# 範本維護指南

此 repository 同時是可直接 clone 修改的專案，也是 `dotnet new` template。

## Template Metadata

Template 設定位置：

```text
.template.config/template.json
.template.config/dotnetcli.host.json
```

重要設定：

| 設定 | 用途 |
| --- | --- |
| `shortName` | CLI 使用名稱：`clean-code-api-worker`。 |
| `sourceName` | 專案與 namespace 取代來源：`Template`。 |
| `preferNameDirectory` | 使用 `-n` 時預設建立同名資料夾。 |
| `dotnetcli.host.json` | 設定 CLI option alias，例如 `--include-worker` 與 `--sample-domain`。 |

## Template Options

完整產出差異矩陣請看 [Template Options](template-options.md)；本節只保留維護 template 時需要知道的核心選項。

| Option | 預設 | 說明 |
| --- | --- | --- |
| `--include-worker true|false` | `true` | 是否產生 Worker 專案、outbox dispatcher 與 Worker integration tests。 |
| `--auth jwt|none` | `jwt` | 是否產生 JWT Bearer authentication 設定與保護 endpoint。 |
| `--database sqlserver|sqlite|none` | `sqlserver` | persistence provider；`none` 會移除 EF Core persistence，Orders 範例改用 in-memory repository。 |
| `--sample-domain orders|minimal` | `orders` | 是否包含 Orders 範例 domain、use cases、endpoints 與測試。 |

## 名稱替換

因為 `sourceName` 是 `Template`，執行：

```powershell
dotnet new clean-code-api-worker -n SampleService
```

會產生：

- `Template.slnx` 變成 `SampleService.slnx`
- `Template.Domain` 變成 `SampleService.Domain`
- `namespace Template...` 變成 `namespace SampleService...`
- project references 與 test namespaces 一併替換

## 條件式內容

Template 使用兩種方式控制產出：

- `sources.modifiers.exclude`：排除整個 project、folder 或 file。
- 條件式註解：在 `.cs`、`.csproj`、`.slnx` 中保留或移除片段。

維護時要確認：

- 排除 Worker 時，solution 與 integration test project 不再 reference Worker。
- 排除 JWT 時，package、using、middleware、endpoint authorization 與 auth tests 都要一起移除。
- 排除 database 時，EF Core persistence、DB health check 與 EF package 都要一起移除。
- 排除 Orders 範例時，Domain/Application/WebApi/Tests 的 Orders 檔案都要一起移除。

## Template 驗證

修改下列內容時，必須驗證 template：

- `.template.config/template.json`
- `.template.config/dotnetcli.host.json`
- project name / namespace / solution file
- conditional source 或 conditional code
- README template 使用說明

先重新安裝目前 repo：

```powershell
dotnet new install . --force
```

驗證預設完整組合：

```powershell
$out = Join-Path $env:TEMP "CleanCodeTemplateSample"
dotnet new clean-code-api-worker -n SampleService -o $out
dotnet restore "$out/SampleService.slnx"
dotnet build "$out/SampleService.slnx" --no-restore
dotnet test "$out/SampleService.slnx" --no-build
```

驗證 minimal 組合：

```powershell
$out = Join-Path $env:TEMP "CleanCodeTemplateMinimal"
dotnet new clean-code-api-worker -n MinimalService -o $out --include-worker false --auth none --database none --sample-domain minimal
dotnet restore "$out/MinimalService.slnx"
dotnet build "$out/MinimalService.slnx" --no-restore
dotnet test "$out/MinimalService.slnx" --no-build
```

驗證 SQLite 組合：

```powershell
$out = Join-Path $env:TEMP "CleanCodeTemplateSqlite"
dotnet new clean-code-api-worker -n SqliteService -o $out --database sqlite
dotnet restore "$out/SqliteService.slnx"
dotnet build "$out/SqliteService.slnx" --no-restore
dotnet test "$out/SqliteService.slnx" --no-build
```

請一律輸出到 repository 外的 temp path，避免把產生物混入 template source。

## 排除清單

Template 預設排除：

- `.git/**`
- `.vs/**`
- `.vscode/**`
- `**/bin/**`
- `**/obj/**`
- `.template-output/**`
