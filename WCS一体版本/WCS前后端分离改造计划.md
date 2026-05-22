
# WCS 前后端分离改造计划

## 技术选型
- **后端**: ASP.NET Core Web API (.NET 8)
- **前端**: Vue 3 + TypeScript + Vite
- **UI组件库**: Element Plus
- **实时通信**: SignalR
- **ORM**: 保留 NHibernate（逐步迁移）
- **设备通信**: 保留现有 TCP 协议栈

---

## 阶段一：后端服务搭建

### Task 1: 创建 ASP.NET Core Web API 项目
- 新建 `Wcs.WebApi` 项目（.NET 8），作为后端服务入口
- 配置依赖注入、中间件管道、CORS 策略
- 集成 Swagger/OpenAPI 文档
- 集成 SignalR Hub

### Task 2: 迁移核心框架层 (Wcs + Wcs.Framework)
- 将 `Wcs.Framework` 从 .NET Framework 4.0 升级到 .NET 8
- 将 `Wcs` 项目升级到 .NET 8
- 去除 WinForms/DevExpress 依赖（WcsPlugin 中的 BarButtonItem 等）
- 保留核心逻辑：WcsContext、EventBus、TaskHelper、LocationConverter、IocContainer
- NHibernate 配置适配 .NET 8（替换 System.Configuration 为 appsettings.json）
- 替换 NLog 为兼容 .NET 8 的版本

### Task 3: 迁移数据访问层
- 将 `NHUnitOfWork`、`NHRepository` 升级到 .NET 8
- 将所有 NHibernate 映射文件（.hbm.xml）迁移
- 保持数据库 schema 不变

### Task 4: 迁移业务逻辑层
- 将 `Wcs.DefaultImplementCollection.*` 全部升级到 .NET 8
- 去除所有 WinForms 窗体代码（保留设备控制核心逻辑）
- 将 `Wcs.FrameworkExtend` 升级到 .NET 8
- 将 `ZHQXC(业务代码)` 中的业务逻辑提取到服务层

### Task 5: 迁移 WCF TaskService → Web API Controller
- 将 `IWcsTaskService` 的所有操作（暂停/完成/取消/归档/恢复任务）改为 RESTful API
- 将 `WCSController`、`WCSForWMSWebApiController` 从 SelfHost 迁移到 ASP.NET Core

### Task 6: 改造插件系统
- WcsPlugin 去除 DevExpress 依赖，改用纯 C# 接口
- 插件元数据（菜单名称、分组、排序）改为 JSON/Attribute 配置
- 插件加载逻辑保留（反射扫描DLL）

### Task 7: 实现 SignalR 实时推送
- 创建 `DeviceHub`：推送设备状态变更、连接/断开事件
- 创建 `TaskHub`：推送任务状态变更、进度更新
- 创建 `AlarmHub`：推送设备报警、故障事件
- 后端 EventBus 事件订阅 → SignalR 广播

### Task 8: 设备通信层保留与适配
- TCP 连接管理、数据收发逻辑保留在后端服务
- `Device`、`TaskableDevice`、`TcpProtocolDevice` 等核心类保留
- 数据接收器（IDataReceiver）架构不变
- 设备状态变更通过 SignalR 推送到前端

---

## 阶段二：前端应用搭建

### Task 9: 初始化 Vue 3 项目
- 使用 Vite + Vue 3 + TypeScript 创建项目
- 集成 Element Plus、Vue Router、Pinia、Axios
- 配置路由、状态管理、请求拦截器
- 前端项目结构：
  ```
  wcs-web/
  ├── src/
  │   ├── api/          # API 请求封装
  │   ├── components/   # 公共组件
  │   ├── composables/  # 组合式函数
  │   ├── layouts/      # 布局组件
  │   ├── plugins/      # 插件模块
  │   ├── router/       # 路由配置
  │   ├── stores/       # Pinia 状态
  │   ├── utils/        # 工具函数
  │   │   └── signalr.ts  # SignalR 客户端
  │   └── views/        # 页面视图
  │       ├── dashboard/    # 欢迎页/仪表盘
  │       ├── device/       # 设备管理
  │       ├── task/         # 任务管理(当前/历史/手工)
  │       ├── alarm/        # 报警管理
  │       ├── monitor/      # 设备监控(实时)
  │       ├── config/       # 系统配置
  │       └── log/          # 日志管理
  ```

### Task 10: 实现布局与导航
- 侧边栏 + 顶部导航栏布局（替代 DevExpress RibbonForm）
- 多标签页管理（替代 XtraTabControl）
- 基于插件元数据动态生成菜单
- 权限控制（路由守卫）

### Task 11: 实现核心页面
- **仪表盘**：系统概览、设备统计、任务统计、实时报警
- **设备管理**：设备列表、设备详情、设备状态实时展示
- **任务管理**：当前任务列表、历史任务、手工任务创建
- **设备监控**：实时设备状态图、2D/3D 仓库可视化
- **报警管理**：报警列表、报警确认、故障统计
- **系统配置**：设备配置、路径配置、参数配置

### Task 12: 集成 SignalR 实时数据
- 封装 SignalR 客户端工具类（自动重连、心跳检测）
- 设备状态实时刷新（设备连接、运行状态、故障状态）
- 任务进度实时追踪
- 报警消息实时弹窗

---

## 阶段三：集成与优化

### Task 13: 前后端联调
- API 接口对接与测试
- SignalR 实时通信测试
- 设备通信端到端测试

### Task 14: 部署方案
- 后端：ASP.NET Core 自托管或 IIS 部署
- 前端：Nginx 静态资源部署或集成到 ASP.NET Core
- 配置管理：appsettings.json 替代原 XML 配置
- Windows Service 方式运行后端服务

---

## 关键文件映射

| 现有文件 | 改造后归属 |
|---------|-----------|
| `WCS.DEVAPP/Program.cs` | 后端 `Program.cs` (ASP.NET Core) |
| `WCS.DEVAPP/frmMain.cs` | 前端 `layouts/MainLayout.vue` |
| `WCS.DEVAPP/frmStarting.cs` | 前端 `views/login/LoginView.vue` |
| `TaskService/IWcsTaskService.cs` | 后端 `Controllers/TaskController.cs` |
| `业务代码/WebAPI/Controller/WCSController.cs` | 后端 `Controllers/WCSController.cs` |
| `Wcs/WcsPlugin/WcsContext.cs` | 后端 `Services/PluginService.cs` |
| `Wcs/WcsPlugin/WcsPlugin.cs` | 后端 `Abstractions/WcsPlugin.cs` (去除UI依赖) |
| `Wcs.Framework/EventBus/` | 后端 `EventBus/` (保留) |
| `Wcs.Framework/任务相关/` | 后端 `Services/TaskService.cs` + `Models/` |
| `Wcs.Framework/设备相关/` | 后端 `Services/DeviceService.cs` + `Models/` |

## 改造优先级
1. **P0**: 后端核心框架迁移（Task 1-4）— 所有其他工作依赖此步
2. **P0**: Web API 接口层（Task 5）— 前端开发依赖此步
3. **P0**: 前端项目搭建与核心页面（Task 9-11）
4. **P1**: SignalR 实时推送（Task 7, 12）
5. **P1**: 插件系统改造（Task 6）
6. **P2**: 集成优化与部署（Task 13-14）
