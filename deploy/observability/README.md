# Local Observability Stack

這個資料夾提供本機 OpenTelemetry 觀測範例。它是可選工具，不是 production 監控平台，也不會影響主服務預設啟動。

## 內容

| Service | Port | 用途 |
| --- | --- | --- |
| OpenTelemetry Collector | `4317` / `4318` | 接收 WebApi / Worker 的 OTLP traces 與 metrics。 |
| Jaeger UI | `16686` | 查看 distributed traces。 |
| Prometheus | `9090` | Scrape Collector 暴露的 metrics。 |
| Grafana | `3000` | 查詢與視覺化 Prometheus metrics。 |

## 啟動觀測工具

從 repository 根目錄執行：

```powershell
docker compose -f deploy/observability/docker-compose.observability.yml up -d
```

打開：

```text
Jaeger:     http://localhost:16686
Prometheus: http://localhost:9090
Grafana:    http://localhost:3000
```

Grafana 預設帳號密碼：

```text
admin / admin
```

第一次使用 Grafana 時，新增 Prometheus datasource：

```text
URL: http://prometheus:9090
```

## 啟用 WebApi / Worker OTLP 輸出

本機直接執行：

```powershell
$env:Observability__Enabled = "true"
$env:Observability__OtlpExporter__Enabled = "true"
$env:Observability__OtlpExporter__Endpoint = "http://localhost:4317"

dotnet run --project src/Template.WebApi
```

Worker：

```powershell
$env:Observability__Enabled = "true"
$env:Observability__OtlpExporter__Enabled = "true"
$env:Observability__OtlpExporter__Endpoint = "http://localhost:4317"

dotnet run --project src/Template.Worker
```

如果使用主 `docker-compose.yml` 的 `app` profile，並且 observability stack 是用本文件的指令獨立啟動，請把 API / Worker 的環境變數改成：

```yaml
Observability__Enabled: "true"
Observability__OtlpExporter__Enabled: "true"
Observability__OtlpExporter__Endpoint: http://host.docker.internal:4317
```

`host.docker.internal` 適合 Docker Desktop。若你的環境沒有支援這個 host name，請改成 Docker host 的實際 IP，或把主服務與 observability stack 放在同一個 Docker network 後改用 `http://otel-collector:4317`。

## 產生 trace

啟動 API 後呼叫：

```powershell
curl http://localhost:5007/health/live
```

或呼叫 Orders API：

```powershell
curl http://localhost:5007/api/v1/orders
```

接著到 Jaeger UI 搜尋 service：

```text
CleanCodeTemplate.WebApi
CleanCodeTemplate.Worker
```

## 查詢 metrics

Prometheus expression 範例：

```text
outbox_pending_count
outbox_failed_count
outbox_dispatch_success_count_total
outbox_dispatch_failure_count_total
```

實際 metric 名稱可能會依 OpenTelemetry Prometheus exporter 命名規則轉成底線格式。

## 停止

```powershell
docker compose -f deploy/observability/docker-compose.observability.yml down
```
