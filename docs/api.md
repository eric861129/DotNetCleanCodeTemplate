# API 說明

`Template.WebApi` 提供一組小型 Orders API，用來示範 endpoint mapping、JWT 保護、use case 呼叫與 response 處理。

## Endpoints

| Method | Route | Auth | 用途 |
| --- | --- | --- | --- |
| `GET` | `/health` | Anonymous | Health probe。 |
| `POST` | `/api/orders` | JWT Bearer | 建立訂單。 |
| `GET` | `/api/orders/{id}` | JWT Bearer | 依 id 取得單筆訂單。 |

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

- Domain validation 失敗時回傳 `400 Bad Request`。
- Bearer token 缺少或無效時回傳 `401 Unauthorized`。

## Authentication

訂單 endpoint 需要 JWT Bearer authentication。開發預設值位於 `src/Template.WebApi/appsettings.json`。

內建 signing key 僅供本機開發。部署前必須更換。

## Swagger

Development 環境會啟用 Swagger：

```text
/swagger
```
