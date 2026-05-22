# Wcs.WebApi

`Wcs.WebApi` 是 WCS 前后端分离改造的后端入口项目，目标是逐步替代原 WinForms SelfHost/WCF 入口，同时保留现有设备通信、任务调度和 NHibernate 数据模型。

## 当前职责

- ASP.NET Core Web API 服务入口
- CORS 策略配置
- 健康检查接口：`GET /health`
- OpenAPI 文档：`GET /openapi/v1.json`
- Swagger UI：`GET /swagger`
- SignalR Hub：
  - `/hubs/device`
  - `/hubs/task`
  - `/hubs/alarm`

## 后续迁移方向

1. 将 `业务代码/WebAPI/Controller/WCSController.cs` 中的 SelfHost Controller 迁移到 `Controllers/`。
2. 将 `TaskService/IWcsTaskService.cs` 的 WCF 操作拆成 RESTful Controller。
3. 抽象旧 `Wcs.Framework` 中与 WinForms/DevExpress 无关的核心服务，逐步接入依赖注入。
4. 将设备、任务、报警事件通过 EventBus 订阅后转发到 SignalR Hub。

## 本地运行

当前云环境未安装 .NET SDK。具备 .NET 8 SDK 的开发环境中可执行：

```bash
dotnet run --project Wcs.WebApi/Wcs.WebApi.csproj
```

默认开发地址见 `Properties/launchSettings.json`。
