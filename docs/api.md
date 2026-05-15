# API 說明

`Template.WebApi` 提供一組小型 Orders API，用來示範 endpoint mapping、JWT 保護、use case 呼叫與 response 處理。

## Endpoints

| Method | Route | Auth | 用途 |
| --- | --- | --- | --- |
| `GET` | `/health` | Anonymous | 相容入口，等同 ready check。 |
| `GET` | `/health/live` | Anonymous | Liveness probe，只確認 process alive。 |
| `GET` | `/health/ready` | Anonymous | Readiness probe，檢查資料庫可用性。 |
| `GET` | `/api/orders?page=1&pageSize=20` | JWT Bearer | 分頁查詢訂單。 |
| `POST` | `/api/orders` | JWT Bearer | 建立訂單。 |
| `GET` | `/api/orders/{id}` | JWT Bearer | 依 id 取得單筆訂單。 |

若產生範本時使用 `--auth none`，Orders endpoint 不會套用 JWT middleware。

## Cross-Cutting Headers

所有 API response 都會帶有：

| Header | 用途 |
| --- | --- |
| `X-Correlation-Id` | 追蹤單次 request。若 request 已帶入此 header，response 會沿用同一個值。 |
| `X-Content-Type-Options: nosniff` | 降低瀏覽器 content sniffing 風險。 |
| `X-Frame-Options: DENY` | 避免 API 被 frame 嵌入。 |
| `Referrer-Policy: no-referrer` | 避免 referrer 洩漏。 |

當超過 rate limit 時會回傳 `429 Too Many Requests`。

## 分頁查詢訂單

Request：

```text
GET /api/orders?page=1&pageSize=20
```

成功回應：`200 OK`

```json
{
  "items": [],
  "page": 1,
  "pageSize": 20,
  "totalCount": 0,
  "totalPages": 0
}
```

`page` 最小值為 `1`；`pageSize` 會限制在 `1` 到 `100`。

## 建立訂單

Request：

```json
{
  "customerId": "customer-001",
  "items": [
    {
      "productName": "Clean Code",
      "quantity": 2,
      "unitPrice": 30
    }
  ]
}
```

成功回應：`201 Created`

```json
{
  "id": "00000000-0000-0000-0000-000000000000",
  "customerId": "customer-001",
  "status": "Pending",
  "totalAmount": 60,
  "createdAt": "2026-05-14T00:00:00+00:00",
  "items": [
    {
      "productName": "Clean Code",
      "quantity": 2,
      "unitPrice": 30,
      "lineTotal": 60
    }
  ]
}
```

預期失敗：

- Request validation 失敗時回傳 `400 ValidationProblemDetails`。
- Domain validation 失敗時回傳 `400 ProblemDetails`。
- 查無資料時回傳 `404 ProblemDetails`。
- Bearer token 缺少或無效時回傳 `401 Unauthorized`。
- 超過 rate limit 時回傳 `429 Too Many Requests`。

## Authentication

訂單 endpoint 需要 JWT Bearer authentication。開發預設值位於 `src/Template.WebApi/appsettings.json`。

內建 signing key 僅供本機開發。部署前必須更換。

## Swagger

Development 環境會啟用 Swagger：

```text
/swagger
```
