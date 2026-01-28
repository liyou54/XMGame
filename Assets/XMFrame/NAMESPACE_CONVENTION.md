# XM 项目命名空间规范

精炼简短，符合 C# 与 Unity 惯例。

## 1. 根命名空间

- **XM**：项目根，替代原 `XMFrame`，用于核心运行时类型（如 `ConfigDataCenter`、`ConfigData`、`IXConfig`、日志等）。

## 2. 程序集与 rootNamespace 对应

| 程序集名 (asmdef name) | rootNamespace | 说明 |
|------------------------|---------------|------|
| **XM.Contracts** | XM.Contracts | 契约层：接口、配置契约、Mod/UI 契约等 |
| **XM.Runtime** | XM | 运行时实现：Config/Asset/UI/Mod/Pool/Save 等管理器 |
| **XM.Editor** | XM.Editor | 编辑器：配置代码生成、窗口、仅 Editor 工具 |
| **XM.Utils** | XM.Utils | 工具：XBlob、容器、算法、类型系统等 |
| **XM.ModAPI** | XM.ModAPI | Mod 对外 API |

## 3. 子命名空间（精炼、PascalCase）

- **XM.Contracts.Config**：配置契约（原 `ConfigMananger` 文件夹，拼写保留；命名空间用 Config）。含 `IClassHelper`、`IConfigDataCenter`、`CfgId`（ModS/TblS/CfgS/CfgI）、`XmlConvertBase` 等。
- **XM.Editor.Gen**：编辑器生成的配置/Unmanaged 代码（原 `Config.Code.Gen`）。
- **XM.Utils.Tests**：Utils 下单元测试（如 XBlob.Tests）。

其余按文件夹适度划分子命名空间（如 `XM.Contracts` 下 Asset、UIManager、ModManager 等可不强制子空间，保持 `XM.Contracts` 即可，除非类型很多再分子空间）。

## 4. C# 规范要点

- 命名空间与标识符使用 **PascalCase**。
- 命名空间与程序集名一一对应，避免跨程序集同名根命名空间。
- 类型全名尽量简短：`XM.IXConfig`、`XM.Contracts.Config.ConfigClassHelper`、`XM.Editor.Gen.NestedConfigClassHelper`。
- 不使用的 `using` 不保留；同一命名空间内不写 `using` 本命名空间。

## 5. 映射表（迁移用）

| 原命名空间 | 新命名空间 |
|------------|------------|
| XMFrame | XM |
| XMFrame.Implementation | XM |
| XMFrame.Interfaces | XM.Contracts |
| XMFrame.Interfaces.ConfigMananger | XM.Contracts.Config |
| XMFrame.Editor | XM.Editor |
| XMFrame.Editor.ConfigEditor | XM.Editor |
| XMFrame.Editor.ConfigEditor.Config.Code.Gen | XM.Editor.Gen |
| XMFrame.Utils | XM.Utils |
| XMFrame.Utils.Attribute | XM.Utils.Attribute |
| XMFrame.XBlob.Tests | XM.Utils.Tests |
| XMFrame.ModAPI | XM.ModAPI |
| XMFrame.Example | XM.Example |
