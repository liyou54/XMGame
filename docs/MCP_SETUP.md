# Unity MCP 接入说明

本项目通过 **MCP for Unity**（[CoplayDev/unity-mcp](https://github.com/CoplayDev/unity-mcp)）将 Unity Editor 与 MCP（Model Context Protocol）打通，使 Cursor、Claude Desktop 等 AI 客户端能直接操作 Unity：管理资源、场景、脚本、控制编辑器等。

## 架构

```
[AI 客户端 Cursor/Claude] ↔ [HTTP: localhost:8080/mcp] ↔ [MCP for Unity (Unity Editor 内)]
```

- **MCP for Unity**：Unity 包（`com.coplaydev.unity-mcp`），在 Editor 内提供 HTTP 服务（默认 `localhost:8080`），对外提供 MCP 协议。
- **MCP 客户端**：通过 URL `http://localhost:8080/mcp` 连接，无需单独安装 Python Server。

## 环境要求

- **Unity**：2021.3 LTS 或更新
- **Python 3.10+** 与 **uv**（仅在使用 stdio 传输时需要）：[安装 uv](https://docs.astral.sh/uv/getting-started/installation/)
- **MCP 客户端**：Cursor、Claude Desktop、Windsurf、VS Code Copilot 等

## 安装步骤

### 1. 安装 Unity 包

依赖已写入 `Packages/manifest.json`：

```json
"com.coplaydev.unity-mcp": "https://github.com/CoplayDev/unity-mcp.git?path=/MCPForUnity#main"
```

在 Unity 中：**Window → Package Manager**，等待解析完成即可。若需最新 beta：将 URL 末尾改为 `#beta`。

### 2. 在 Unity 中启动服务并配置客户端

1. 在 Unity 中打开 **Window → MCP for Unity**。
2. 点击 **Start Server**（在本地启动 HTTP 服务，默认 `localhost:8080`）。
3. 在窗口中选择你的 MCP 客户端（如 Cursor），点击 **Configure**。
4. 看到 🟢 **"Connected ✓"** 即表示配置已写入。

### 3. 手动配置 MCP 客户端（可选）

若自动配置失败，可在 Cursor 等客户端的 MCP 配置中手动添加：

**HTTP（推荐，适用于 Claude Desktop、Cursor、Windsurf）：**

```json
{
  "mcpServers": {
    "unityMCP": {
      "url": "http://localhost:8080/mcp"
    }
  }
}
```

**VS Code：**

```json
{
  "servers": {
    "unityMCP": {
      "type": "http",
      "url": "http://localhost:8080/mcp"
    }
  }
}
```

保存后重启客户端。**使用前需在 Unity 中先点击 Start Server**。

### 4. 使用

1. **打开 Unity 项目**，在 **Window → MCP for Unity** 中点击 **Start Server**。
2. **启动 Cursor**（或其它已配置的 MCP 客户端）；部分客户端需在设置中开启 MCP。
3. 在 Cursor 中即可用自然语言操作 Unity，例如：
   - “创建红、蓝、黄三个立方体”
   - “做一个简单的玩家控制器”
   - “列出当前场景层级”

## 常用 MCP 工具（由 MCP for Unity 提供）

| 工具 | 说明 |
|------|------|
| `manage_asset` | 资源导入、创建、修改、删除等 |
| `manage_editor` | 查询/控制编辑器状态与设置 |
| `manage_gameobject` | GameObject 的创建、修改、删除、查找与组件操作 |
| `manage_components` | 组件管理 |
| `manage_material` / `manage_prefabs` | 材质、预制体 |
| `manage_scene` | 场景加载、保存、创建、获取层级等 |
| `manage_script` | 管理 C# 脚本（创建、读、改、删） |
| `read_console` | 读取或清空 Unity 控制台消息 |
| `execute_menu_item` | 通过菜单路径执行菜单项 |
| `batch_execute` | 批量执行（推荐，比单次调用快 10–100 倍） |

## 故障排查

- **Unity 内未连接**  
  - 确认 Unity Editor 已打开本项目。  
  - 在 **Window → MCP for Unity** 查看状态，必要时重启 Unity 后再次 **Start Server**。

- **客户端连不上**  
  - 确认已在 Unity 中点击 **Start Server**，且无防火墙拦截 8080 端口。  
  - 检查 MCP 配置中的 URL 是否为 `http://localhost:8080/mcp`。  
  - Cursor/Windsurf 等需在设置中开启 MCP 或对应服务器。

- **自动配置失败**  
  - 使用上文**手动配置**，确保 JSON 格式正确。

更多说明与排错见 [CoplayDev/unity-mcp 官方仓库](https://github.com/CoplayDev/unity-mcp) 与 [Wiki](https://github.com/CoplayDev/unity-mcp/wiki)。
