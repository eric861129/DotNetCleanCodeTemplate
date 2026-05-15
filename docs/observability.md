# Observability

本範本使用 OpenTelemetry 提供平台中立的 tracing 與 metrics 接法。預設不輸出到任何後端，使用時透過設定開關啟用 Console exporter 或 OTLP exporter。

## 目標

Observability 用來回答 production 常見問題：

- API 慢在哪一段？
- Worker 是否有處理 outbox messages？
- Outbox pending / failed message 是否累積？
- 某次 request 的 log、trace 與 database call 能否串起來？
- 部署後錯誤率或延遲是否變差？

## 設定總覽

WebApi 與 Worker 都支援同一段設定：

```json
{
  "Observability": {
    "Enabled": false,
    "ServiceName": "CleanCodeTemplate.WebApi",
    "ServiceVersion": "1.0.0",
    "Tracing": {
      "Enabled": true
    },
    "Metrics": {
      "Enabled": true
    },
    "ConsoleExporter": {
      "Enabled": false
    },
    "OtlpExporter": {
      "Enabled": false,
      "Endpoint": ""
    }
  }
}
```

| 設定 | 用途 |
| --- | --- |
| `Enabled` | Observability 總開關。 |
| `ServiceName` | OpenTelemetry resource service name。WebApi / Worker 建議使用不同名稱。 |
| `ServiceVersion` | 服務版本，可放 release version 或 image tag。 |
| `Tracing:Enabled` | 是否收集 traces。 |
| `Metrics:Enabled` | 是否收集 metrics。 |
| `ConsoleExporter:Enabled` | 是否輸出到 console，適合本機測試。 |
| `OtlpExporter:Enabled` | 是否透過 OTLP 輸出到 OpenTelemetry Collector 或相容平台。 |
| `OtlpExporter:Endpoint` | OTLP endpoint，例如 `http://localhost:4317`。空值時使用 OpenTelemetry SDK 預設值。 |

`Observability` options 會在啟動時驗證。當 `Enabled=false` 時，不會註冊 OpenTelemetry pipeline。

## 本機測試 Console Exporter

WebApi：

```powershell
$env:Observability__Enabled = "true"
$env:Observability__ConsoleExporter__Enabled = "true"
dotnet run --project src/Template.WebApi
```

Worker：

```powershell
$env:Observability__Enabled = "true"
$env:Observability__ConsoleExporter__Enabled = "true"
dotnet run --project src/Template.Worker
```

呼叫 API 後，console 會看到 traces 或 metrics 輸出。

## 本機觀測 Stack

本範本提供一組可選的本機觀測工具：

```text
deploy/observability/
├── docker-compose.observability.yml
├── otel-collector-config.yml
├── prometheus.yml
└── README.md
```

啟動：

```powershell
docker compose -f deploy/observability/docker-compose.observability.yml up -d
```

服務入口：

| 工具 | URL | 用途 |
| --- | --- | --- |
| Jaeger UI | `http://localhost:16686` | 查看 traces。 |
| Prometheus | `http://localhost:9090` | 查詢 metrics。 |
| Grafana | `http://localhost:3000` | 視覺化 metrics，預設帳號密碼為 `admin / admin`。 |

本機執行 WebApi / Worker 時，設定 OTLP endpoint：

```powershell
$env:Observability__Enabled = "true"
$env:Observability__OtlpExporter__Enabled = "true"
$env:Observability__OtlpExporter__Endpoint = "http://localhost:4317"
```

如果是用 Docker Compose 啟動 API / Worker，且 observability stack 獨立啟動，endpoint 請使用 Docker host：

```yaml
Observability__Enabled: "true"
Observability__OtlpExporter__Enabled: "true"
Observability__OtlpExporter__Endpoint: http://host.docker.internal:4317
```

若你把主服務與 observability stack 放在同一個 Docker network，才改用 `http://otel-collector:4317`。

詳細操作請看 [Local Observability Stack](../deploy/observability/README.md)。

## 接 OpenTelemetry Collector

範例設定：

```powershell
$env:Observability__Enabled = "true"
$env:Observability__OtlpExporter__Enabled = "true"
$env:Observability__OtlpExporter__Endpoint = "http://localhost:4317"
```

這個方式可以接到：

- OpenTelemetry Collector
- Grafana Tempo / Mimir / Prometheus pipeline
- Jaeger / Zipkin 相容 pipeline
- Azure Monitor / AWS / GCP / Datadog 等支援 OTLP 的後端

本範本不綁定特定平台，正式環境建議透過 Collector 統一處理 sampling、export、retry 與 backend credentials。

## WebApi Instrumentation

WebApi 啟用時會收集：

- ASP.NET Core request traces / metrics。
- `HttpClient` outgoing calls。
- Runtime metrics。
- App 自訂 meter。
- EF Core traces。此 instrumentation 目前使用 OpenTelemetry 官方 prerelease package。

註冊位置：

```text
src/Template.WebApi/Extensions/WebApiObservabilityExtensions.cs
```

## Worker Instrumentation

Worker 啟用時會收集：

- Worker 自訂 outbox dispatch traces。
- `HttpClient` outgoing calls。
- Runtime metrics。
- App 自訂 meter。
- EF Core traces。此 instrumentation 目前使用 OpenTelemetry 官方 prerelease package。

註冊位置：

```text
src/Template.Worker/WorkerObservabilityExtensions.cs
```

## Outbox Metrics

本範本提供 outbox 自訂 metrics：

| Metric | 說明 |
| --- | --- |
| `outbox.dispatch.success.count` | 成功 dispatch 的 outbox message 次數。 |
| `outbox.dispatch.failure.count` | dispatch 失敗次數。 |
| `outbox.dispatch.duration.ms` | dispatch 單筆 message 花費毫秒數。 |
| `outbox.batch.messages.count` | 每輪讀出的 message 數量累計。 |
| `outbox.pending.count` | Worker 最近觀察到的 pending message 數。 |
| `outbox.failed.count` | Worker 最近觀察到已失敗且仍 pending 的 message 數。 |

自訂 diagnostics 定義位置：

```text
src/Template.Infrastructure/Observability/AppDiagnostics.cs
```

## Tracing 範例

一次建立訂單的 trace 可能長這樣：

```text
HTTP POST /api/v1/orders
  -> EF Core INSERT Orders
  -> EF Core INSERT OutboxMessages
```

Worker dispatch trace 可能長這樣：

```text
outbox.dispatch
  message.id = ...
  message.type = OrderCreated
```

如果 dispatch 失敗，activity status 會被標示為 error，並附上 exception。

## Production 建議

- `ServiceName` 使用明確名稱，例如 `BillingService.WebApi` 與 `BillingService.Worker`。
- `ServiceVersion` 綁定 release version 或 container image tag。
- 本機可用 Console exporter，production 建議使用 OTLP exporter。
- 透過 OpenTelemetry Collector 管理 sampling 與後端 credentials。
- 避免在 traces/logs 中放入 JWT、connection string、password 或完整個資。
- Outbox pending / failed metrics 建議設定告警。

## 注意事項

EF Core instrumentation 目前使用 OpenTelemetry 官方 prerelease package。若團隊不接受 prerelease dependency，可以先移除 `OpenTelemetry.Instrumentation.EntityFrameworkCore`，保留 ASP.NET Core、HTTP、Runtime 與自訂 outbox metrics，之後等穩定版再加回。
