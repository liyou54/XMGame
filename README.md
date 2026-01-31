# XMGame

Unity 主项目，基于 XMFrame 框架，支持 Mod 热更与资源配置（YooAsset）。

## 项目结构

- **Assets/XMFrame**：框架代码（接口、实现、编辑器工具）
- **XModTest**：Mod 开发子工程，用于创建与打包 Mod
- **Mods**：主项目运行时从根目录 `Mods/` 加载已导出的 Mod（与 `XModManager.ModsFolder` 一致）

## Mod 系统

- 主项目运行时从 `BaseDirectory/Mods` 读取各 Mod 的 `ModDefine.xml`，加载对应 DLL 与资源。
- Mod 入口：继承 `XM.Contracts.ModBase`，实现 `OnCreate`、`OnInit` 等。
- 主项目通过 `XModManager` 管理 Mod 配置与生命周期。

## XModTest 子工程

用于开发、打包并导出 Mod 到主项目：

1. **创建 Mod**：菜单 `Mod/创建 Mod` 或 `Assets/Mod/创建 Mod`，在 `Assets/Mods/<ModName>` 下生成 ModDefine.xml、Xml、Asset、Scripts、YooAssetPackage.xml。
2. **打包并导出**：菜单 `Mod/打包并导出到主项目 Mods` 或右键 Mod 目录选「打包并导出到主项目」：
   - 先请求编译，编译完成后同步执行 YooAsset 打包（BuiltinBuildPipeline），再将 Bundles 等导出到主项目 `Mods/<ModName>/`（含 Asset、ModDefine.xml、DLL 等）。

主项目 Mods 路径规则：若当前工程在 XMGame 内（如 `XMGame/XModTest`）则主项目为父目录；若当前工程名为 XModTest 且与 XMGame 同级，则主项目为同级目录 `XMGame`。

## 环境要求

- Unity 2021.3+（推荐）
- 主项目引用 XMFrame；XModTest 通过 Package 引用 YooAsset，通过拷贝 DLL 引用 XM.Contracts、XM.ModAPI 等（见主项目 `CopyDllsToXModTest`）。

## 文档

- **Unity MCP 接入**：`docs/MCP_SETUP.md`（Cursor/Claude 等 AI 客户端与 Unity Editor 联动）
- 对象池：`Assets/XMFrame/Implementation/XPoolManager/README.md`
- 命名与命名空间：`Assets/XMFrame/ID_NAMING_CONVENTION.md`、`NAMESPACE_CONVENTION.md`
