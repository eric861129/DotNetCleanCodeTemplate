# 開發流程

新增或修改行為時，請使用此流程。

## 1. 從行為開始

修改 production code 前，先用測試描述期望行為。

- Domain rule：新增或更新 unit test。
- Use case flow：新增或更新 Application 層級 unit test。
- HTTP behavior：新增或更新 integration test。
- Worker behavior：新增或更新 integration test。

## 2. 把程式放在正確層級

| 變更內容 | 應放位置 |
| --- | --- |
| 業務 invariant | Domain |
| Use case orchestration | Application |
| Database query 或 EF mapping | Infrastructure |
| HTTP route 或 status code | WebApi |
| Background polling 或 scheduling | Worker |

如果一個 class 同時需要 SQL、HTTP 與業務規則，它很可能做太多事。

## 3. 保持 Use Case 明確

Application use case 實作：

```csharp
IUseCase<TRequest, TResponse>
```

使用 request/response record 讓輸入與輸出清楚可見。

## 4. 有意識地註冊依賴

- Application service 註冊在 `Template.Application.DependencyInjection`。
- Infrastructure service 註冊在 `Template.Infrastructure.DependencyInjection`。
- WebApi 與 Worker 在 startup 組裝 application。

避免在隨機 feature file 中分散 service registration。

## 5. 完成前驗證

執行：

```powershell
dotnet build
dotnet test
```

如果修改 template 行為，也要驗證產生專案：

```powershell
dotnet new install .
dotnet new clean-code-api-worker -n SampleService -o <temp-path>
dotnet restore <temp-path>/SampleService.slnx
dotnet build <temp-path>/SampleService.slnx --no-restore
dotnet test <temp-path>/SampleService.slnx --no-build
```

請將 `<temp-path>` 放在 repository 外，避免產出的 template 內容被 `dotnet new install .` 掃描成巢狀 template。
