# Troubleshooting

這份文件整理本範本常見問題與排查順序。遇到問題時，請先確認正在操作的是「template source repository」還是「由 template 產出的新專案」，兩者的路徑與專案名稱可能不同。

## Docker SQL Server 起不來

常見症狀：

- `docker compose up -d sqlserver` 後 container 反覆重啟。
- `/health/ready` 顯示 unhealthy。
- API log 出現 SQL Server connection failed。

排查步驟：

```powershell
docker compose ps
docker compose logs sqlserver
```

檢查項目：

- Docker Desktop 已啟動。
- Port `1433` 沒有被本機其他 SQL Server 佔用。
- `MSSQL_SA_PASSWORD` 符合 SQL Server 密碼複雜度。
- 第一次啟動 SQL Server 需要一些時間，請等 health check 完成。
- 若改過 compose password，`ConnectionStrings__DefaultConnection` 也要同步更新。

如果本機已有 SQL Server 使用 `1433`，可以修改 `docker-compose.yml`：

```yaml
ports:
  - "11433:1433"
```

然後把本機連線字串改成：

```text
Server=localhost,11433;Database=CleanCodeTemplate;User Id=sa;Password=Your_strong_password123;TrustServerCertificate=True
```

## API 回傳 JWT 401

常見症狀：

- `POST /api/v1/orders` 回傳 `401 Unauthorized`。
- `GET /api/v1/orders/{id}` 回傳 `401 Unauthorized`。
- Swagger 可以開，但呼叫 Orders endpoint 失敗。

原因：

- 預設 `--auth jwt` 時 Orders endpoint 需要 Bearer token。
- Token issuer、audience 或 signing key 與 API 設定不一致。
- Token 過期或格式不是 JWT Bearer。

檢查設定：

```json
{
  "Jwt": {
    "Issuer": "CleanCodeTemplate",
    "Audience": "CleanCodeTemplate",
    "SigningKey": "local-development-signing-key-please-change"
  }
}
```

檢查 request header：

```text
Authorization: Bearer <token>
```

如果你只是要快速建立無 auth 的 prototype，可以用：

```powershell
dotnet new clean-code-api-worker -n MinimalService --auth none
```

如果是 production，請不要關閉 auth；應改為接入正式 identity provider，並同步更新 issuer、audience 與 signing key。

## Template Install Cache 沒更新

常見症狀：

- 修改 template 後，`dotnet new clean-code-api-worker` 產出的內容仍是舊版。
- 新增的檔案沒有出現在 generated project。
- option 行為與目前 `.template.config/template.json` 不一致。

處理方式：

```powershell
dotnet new uninstall .
dotnet new install . --force
```

確認目前安裝的 template：

```powershell
dotnet new list clean-code-api-worker
```

如果仍然異常，可以清除 dotnet new cache：

```powershell
dotnet new --debug:reinit
dotnet new install . --force
```

注意：

- 請在 template source repository 根目錄執行 install。
- 驗證產出時請輸出到 repository 外的 temp path。
- 不要把 generated project 放回 template source 裡，避免被下次 template install 打包。

## EF Migration 問題

常見症狀：

- `dotnet ef` 找不到 command。
- migration 產生失敗。
- app 啟動後查不到資料表。
- SQLite / SQL Server provider 設定與預期不同。

確認工具：

```powershell
dotnet tool list --global
dotnet ef --version
```

如果尚未安裝：

```powershell
dotnet tool install --global dotnet-ef
```

從 repository 根目錄新增 migration：

```powershell
dotnet ef migrations add InitialCreate --project src/Template.Infrastructure --startup-project src/Template.WebApi
```

套用 migration：

```powershell
dotnet ef database update --project src/Template.Infrastructure --startup-project src/Template.WebApi
```

檢查項目：

- `Database:Provider` 是否符合目前環境。
- `ConnectionStrings:DefaultConnection` 是否指向正確資料庫。
- SQL Server container 是否 healthy。
- `src/Template.Infrastructure` 是否仍包含 `Persistence` 資料夾。
- 如果產出時使用 `--database none`，就不會有 EF Core persistence 與 migration path。

Production migration 策略請參考 [Production Checklist](production-checklist.md)。

## `/health/ready` 不健康

常見症狀：

- `/health/live` 回 `Healthy`。
- `/health/ready` 回 `Unhealthy`。
- Docker compose 中 WebApi 等待 SQL Server 或啟動後 ready check 失敗。

解讀方式：

- `live` 只代表 process alive。
- `ready` 代表服務依賴也可用，目前預設會檢查 database。

排查順序：

1. 確認 SQL Server container 狀態。
2. 確認 connection string。
3. 確認 database provider。
4. 確認 migration 是否已套用。
5. 查看 WebApi log 中的 database health check exception。

如果你用 `--database none` 產生專案，ready check 不會檢查 DB。

## Docker Build 失敗

常見症狀：

- `docker build` 找不到 `.slnx` 或 `.csproj`。
- `dotnet restore` 在 image build 內失敗。
- build context 很大或包含不必要檔案。

建議指令：

```powershell
docker build -f src/Template.WebApi/Dockerfile -t clean-code-template-webapi:local .
docker build -f src/Template.Worker/Dockerfile -t clean-code-template-worker:local .
```

檢查項目：

- 指令最後的 build context 是 repository 根目錄的 `.`。
- `.dockerignore` 沒有排除 Dockerfile 需要 `COPY` 的 source files。
- 已先在本機跑過 `dotnet restore` / `dotnet build` 確認不是程式碼本身錯誤。
- 如果使用 generated project，請確認 Dockerfile 中的 project path 已被 template 正確改名。

## Template Option 產出不符合預期

常見症狀：

- `--include-worker false` 仍看到 Worker project。
- `--auth none` 還有 JWT 設定。
- `--sample-domain minimal` 還看到 Orders endpoint。
- `--database none` 還有 EF Core persistence。

排查步驟：

```powershell
dotnet new install . --force

$out = Join-Path $env:TEMP "CleanCodeTemplateCheck"
dotnet new clean-code-api-worker -n CheckService -o $out --include-worker false --auth none --database none --sample-domain minimal
```

檢查 generated solution：

```powershell
dotnet build "$out/CheckService.slnx"
dotnet test "$out/CheckService.slnx" --no-build
```

如果產出仍不正確，請檢查：

- `.template.config/template.json` 的 `symbols` 與 `sources.modifiers`。
- `.template.config/dotnetcli.host.json` 的 alias。
- `.csproj`、`.slnx`、`.cs` 中的 conditional comment。
- 是否安裝到舊的 template cache。

詳細矩陣請看 [Template Options](template-options.md)。

## 測試在本機通過但 CI 失敗

常見原因：

- 本機有環境變數或 user secrets，CI 沒有。
- 本機 template cache 是最新版，CI 重新 install 後暴露 conditional source 問題。
- OS path 或 line ending 差異。
- Docker 或 SQL Server 啟動時間在 CI 較慢。

建議：

- CI 中明確執行 `dotnet restore`、`dotnet build --no-restore`、`dotnet test --no-build`。
- template matrix 至少跑 default、minimal、sqlite。
- Integration tests 使用 `TemplateWebApplicationFactory` 與 SQLite in-memory，避免依賴本機 SQL Server。
- 將必要設定放在 workflow env，而不是依賴本機設定。

## 修改後不知道要跑哪些驗證

可依變更範圍選擇：

| 變更類型 | 建議驗證 |
| --- | --- |
| Domain / Application | `dotnet test Template.slnx --filter Template.UnitTests` 或完整 `dotnet test`。 |
| WebApi endpoint / middleware | `dotnet test Template.slnx --filter Template.IntegrationTests` 或完整 `dotnet test`。 |
| Infrastructure / database | `dotnet build`、integration tests、必要時跑 EF migration。 |
| Worker / outbox | Worker integration tests。 |
| Template option / project structure | default、minimal、sqlite template generation + build + test。 |
| 文件-only | `git diff --check` 與連結檢查。 |

不確定時，使用完整驗證：

```powershell
dotnet restore Template.slnx
dotnet build Template.slnx --no-restore
dotnet test Template.slnx --no-build
```
