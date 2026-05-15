# Changelog

所有重要變更都會記錄在此文件。

## [v1.0.0] - 2026-05-15

### Added

- 建立 .NET 10 LTS Clean Code / Pragmatic Clean Architecture 範本。
- 提供 `dotnet new clean-code-api-worker` template。
- 支援 template options：
  - `--include-worker true|false`
  - `--auth jwt|none`
  - `--database sqlserver|sqlite|none`
  - `--sample-domain orders|minimal`
- 建立分層 solution：
  - `Template.Domain`
  - `Template.Application`
  - `Template.Infrastructure`
  - `Template.WebApi`
  - `Template.Worker`
  - Unit / integration tests
- 提供 Orders 範例 domain、use cases、validators、repository、API endpoints 與 tests。
- WebApi 支援：
  - Minimal API endpoint groups
  - `/api/v1` route prefix
  - Swagger v1 document
  - JWT Bearer authentication
  - ProblemDetails / ValidationProblemDetails
  - request validation
  - pagination example
  - correlation id
  - security headers
  - rate limiting
  - `/health/live`、`/health/ready`
- Infrastructure 支援：
  - EF Core SQL Server
  - SQLite template option
  - database options validation
  - migration-ready `AppDbContext`
  - in-memory path for minimal / database none scenarios
- Worker 支援：
  - Outbox polling
  - replaceable message dispatcher
  - processed / failed / retry count handling
- Observability 支援：
  - OpenTelemetry tracing / metrics
  - ASP.NET Core / HttpClient / Runtime / EF Core instrumentation
  - Outbox custom traces and metrics
  - Console exporter
  - OTLP exporter
  - optional local observability stack with Collector, Jaeger, Prometheus, and Grafana
- Docker 支援：
  - WebApi Dockerfile
  - Worker Dockerfile
  - SQL Server docker compose
  - optional app profile
  - `.dockerignore`
- CI 支援：
  - restore / build / test
  - default / minimal / sqlite template generation matrix
- 文件：
  - README
  - Clean Code 專案規範
  - 架構說明
  - 快速開始
  - 開發流程
  - 新增功能食譜
  - API 說明
  - API Versioning
  - 設定說明
  - Observability
  - Template Options
  - 範本維護指南
  - 維運指南
  - Production Checklist
  - Troubleshooting

### Notes

- EF Core OpenTelemetry instrumentation 使用 OpenTelemetry 官方 prerelease package；若團隊不接受 prerelease dependency，可先移除 EF Core instrumentation，保留其他 OpenTelemetry 能力。
