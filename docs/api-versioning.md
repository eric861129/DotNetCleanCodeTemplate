# API Versioning

本範本使用輕量 route prefix 示範 API versioning，不引入大型 versioning 套件。目標是讓正式專案一開始就有清楚的 URL 邊界，同時保持 Minimal API 設定容易理解。

## 目前規則

| 項目 | 規則 |
| --- | --- |
| API prefix | `/api/v1` |
| Orders route | `/api/v1/orders` |
| Swagger group | `v1` |
| Swagger document | `/swagger/v1/swagger.json` |

Health check 不放進 versioned API，維持平台 probe 常用路徑：

```text
/health/live
/health/ready
```

## Endpoint 寫法

Endpoint group 以版本前綴建立：

```csharp
private const string ApiVersion = "v1";
private const string OrdersRoutePrefix = $"/api/{ApiVersion}/orders";

var group = app.MapGroup(OrdersRoutePrefix)
    .WithTags("Orders")
    .WithGroupName(ApiVersion)
    .RequireRateLimiting("global");
```

`WithGroupName("v1")` 會讓 Swagger 把 endpoint 放到 `v1` document。

## Swagger 分版本

Swagger 設定在 `WebApiServiceCollectionExtensions`：

```csharp
using Microsoft.OpenApi;

services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Template API",
        Version = "v1"
    });
});
```

Swagger UI 設定在 `WebApiApplicationBuilderExtensions`：

```csharp
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Template API v1");
});
```

## 新增 v2 的建議流程

如果未來要新增 `v2`，建議先複製 endpoint group，而不是在同一個 handler 裡塞版本判斷：

```text
src/Template.WebApi/Endpoints/
├── OrderEndpoints.cs
└── OrderV2Endpoints.cs
```

概念上：

```csharp
var group = app.MapGroup("/api/v2/orders")
    .WithTags("Orders")
    .WithGroupName("v2");
```

然後新增 Swagger document：

```csharp
options.SwaggerDoc("v2", new OpenApiInfo
{
    Title = "Template API",
    Version = "v2"
});
```

Swagger UI 也新增一組 endpoint：

```csharp
options.SwaggerEndpoint("/swagger/v2/swagger.json", "Template API v2");
```

## 何時需要正式套件

目前的 route prefix 適合：

- 新專案起步。
- 版本數量少。
- 主要用 URL path 管理版本。
- 不需要 deprecation header、API explorer policy 或複雜 negotiated versioning。

當你需要以下能力時，再評估引入正式 API versioning 套件：

- Header 或 query string versioning。
- 自動 deprecated / sunset metadata。
- 更複雜的 Swagger grouping。
- 同一 endpoint 需要多版本共存策略。
- 大量版本與相容性政策。

本範本刻意先用簡單 route prefix，是為了讓新團隊可以直接看懂路由結構，之後再依需求升級。
