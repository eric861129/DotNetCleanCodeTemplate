---
name: clean-code-outbox-worker
description: Use this skill when adding, changing, debugging, or reviewing Outbox, Worker, background processing, polling, message dispatching, retry behavior, or IOutboxMessageDispatcher implementations in this CleanCodeTemplate repository.
---

# Clean Code Outbox Worker

使用此 skill 維護 Outbox 與 Worker 相關功能。目標是讓背景處理清楚、可測、可替換，並且不污染 Domain/Application 邊界。

## 必讀文件

開始前閱讀：

1. `docs/architecture.md`
2. `docs/configuration.md`
3. `docs/operations.md`
4. `docs/testing.md`

## 現有責任分工

| 元件 | 責任 |
| --- | --- |
| `OutboxMessage` | Application contract，描述待處理訊息與處理狀態。 |
| `IOutboxMessageRepository` | Application port，讀寫 outbox messages。 |
| `OutboxMessageRepository` | Infrastructure adapter，透過 EF Core 存取資料。 |
| `OutboxDispatcher` | Worker orchestration，處理一批 pending messages。 |
| `OutboxDispatcherWorker` | BackgroundService loop，定期建立 scope 並呼叫 dispatcher。 |
| `IOutboxMessageDispatcher` | 可替換的實際 dispatch interface。 |
| `LoggingOutboxMessageDispatcher` | 本機開發用預設實作，只寫 log。 |

## 修改規則

- 不要讓 Worker 直接計算 domain 規則。
- 不要讓 Domain reference Worker 或 Infrastructure。
- 新的外部訊息系統應實作 `IOutboxMessageDispatcher`。
- Retry、failed message、processed message 的狀態更新要能測試。
- Polling interval 與 batch size 走 `OutboxOptions`，不要硬編碼。

## 測試策略

新增 Worker 行為時，優先補 integration test：

```text
tests/Template.IntegrationTests/Worker/
```

測試應涵蓋：

- pending message 被處理後會標記 `ProcessedAt`。
- dispatcher 失敗時會增加 retry count 並記錄 error。
- batch size 會限制單輪處理數量。

## 回報格式

```markdown
Outbox/Worker 變更：
- 變更摘要

架構邊界：
- 是否維持 Application port / Infrastructure adapter / Worker orchestration 分離

驗證：
- 測試命令與結果
```
