# TestRuntimeManager 快速入门指南

## 快速开始

### 步骤 1: 启用条件编译符号（可选）

如果需要使用配置输出功能：

1. 打开 Unity 编辑器
2. 进入 `Edit` → `Project Settings` → `Player`
3. 在 `Other Settings` → `Scripting Define Symbols` 中添加：
   ```
   ENABLE_CONFIG_OUTPUT
   ```
4. 点击 `Apply` 并等待编译完成

> **注意**: 如果只运行测试，不需要配置输出功能，可以跳过此步骤。测试会自动忽略配置输出相关测试。

### 步骤 2: 运行测试

#### 方法 A: 使用 Unity Test Runner

1. 打开 `Window` → `General` → `Test Runner`
2. 切换到 `EditMode` 标签
3. 找到 `XM.Editor.Tests.Integration` → `TestRuntimeManager`
4. 点击 `Run All` 运行所有测试

#### 方法 B: 运行单个测试

1. 在 Test Runner 中展开 `TestRuntimeManager`
2. 选择要运行的测试，例如：
   - `Test_AllManagersInitialized` - 验证管理器初始化
   - `Test_ModManagerLoadedMods` - 验证 Mod 加载
   - `Test_ConfigDataCenterRegisteredTypes` - 验证配置注册
3. 点击 `Run Selected` 运行选中的测试

### 步骤 3: 查看测试结果

测试结果会在 Test Runner 窗口中显示：
- ✅ 绿色对勾：测试通过
- ❌ 红色叉号：测试失败
- ⚠️ 黄色警告：测试被跳过或有警告

### 步骤 4: 查看日志输出

1. 打开 `Window` → `General` → `Console`
2. 查看详细的测试日志：
   ```
   [TestRuntimeManager] 所有管理器初始化完成
   [Test] 已初始化 5 个管理器
   [Test] 已启用 2 个 Mod
   ...
   ```

## 使用配置输出功能

### 前提条件

确保已定义 `ENABLE_CONFIG_OUTPUT` 编译符号（见步骤 1）。

### 在测试中使用

运行 `Test_OutputConfigData` 测试：

1. 在 Test Runner 中找到 `Test_OutputConfigData`
2. 点击运行
3. 测试完成后，查看输出目录中的文件

输出文件位置：
- 临时目录：`C:\Users\<用户名>\AppData\Local\Temp\XMFrameTest_xxxxx\ConfigOutput\`

输出文件包括：
- `Managers.txt` - 管理器信息
- `Mods.txt` - Mod 信息
- `ConfigTypes.txt` - 配置类型信息
- `ConfigStatistics.txt` - 配置统计信息

### 在编辑器中手动使用

创建一个编辑器窗口脚本：

```csharp
#if UNITY_EDITOR && ENABLE_CONFIG_OUTPUT
using UnityEditor;
using UnityEngine;
using XM.Editor;

public class ConfigOutputWindow : EditorWindow
{
    [MenuItem("Tools/Debug/Output Config Data")]
    static void OutputConfigData()
    {
        var outputHelper = new ConfigOutputHelper();
        string outputPath = "Assets/ConfigDebug";
        
        outputHelper.OutputAllConfigs(outputPath);
        
        AssetDatabase.Refresh();
        Debug.Log($"配置已输出到: {outputPath}");
    }
}
#endif
```

使用方法：
1. 运行游戏（确保管理器已初始化）
2. 点击 `Tools` → `Debug` → `Output Config Data`
3. 查看 `Assets/ConfigDebug` 目录中的输出文件

## 测试说明

### 测试执行顺序

测试按以下顺序执行：

1. **OneTimeSetUp** - 初始化 GameMain 和所有管理器
2. **Test_AllManagersInitialized** - 验证初始化成功
3. **Test_ModManagerLoadedMods** - 验证 Mod 加载
4. **Test_ConfigDataCenterRegisteredTypes** - 验证配置注册
5. **Test_ManagerDependencies** - 验证依赖关系
6. **Test_ManagerLifecycle** - 验证生命周期
7. **Test_ConfigDataLoaded** - 验证配置加载
8. **Test_ManagerInitializationPerformance** - 性能测试
9. **Test_OutputConfigData** - 配置输出（条件执行）
10. **OneTimeTearDown** - 清理资源

### 测试分类

- **Category**: `Integration` - 集成测试
- **Performance**: 性能测试标记

可以按分类运行测试：
1. 在 Test Runner 中点击右上角的过滤图标
2. 选择 `Integration` 类别
3. 点击 `Run All`

## 常见场景

### 场景 1: 验证新管理器是否正确集成

1. 创建新管理器并添加 `[AutoCreate]` 特性
2. 运行 `Test_AllManagersInitialized` 测试
3. 查看日志，确认新管理器已被初始化

### 场景 2: 调试 Mod 加载问题

1. 运行 `Test_ModManagerLoadedMods` 测试
2. 查看日志输出的 Mod 列表
3. 确认问题 Mod 是否在列表中

### 场景 3: 验证配置是否正确注册

1. 运行 `Test_ConfigDataCenterRegisteredTypes` 测试
2. 如果启用了配置输出，运行 `Test_OutputConfigData`
3. 查看 `ConfigTypes.txt` 文件，确认配置类型已注册

### 场景 4: 性能分析

1. 运行 `Test_ManagerInitializationPerformance` 测试
2. 查看日志输出的管理器列表和初始化时间
3. 分析是否有性能瓶颈

### 场景 5: 导出配置数据供外部分析

1. 定义 `ENABLE_CONFIG_OUTPUT` 编译符号
2. 运行完整测试套件
3. 从临时目录复制输出文件到项目文档目录
4. 使用文本编辑器或其他工具分析配置数据

## 调试技巧

### 技巧 1: 调整日志级别

在 `GameMain.cs` 中修改日志级别：

```csharp
// 详细日志（调试用）
XLog.CurrentLogLevel = LogLevel.Debug;

// 关闭日志（性能测试用）
XLog.CurrentLogLevel = LogLevel.PerformanceTest;
```

### 技巧 2: 单步调试测试

1. 在测试代码中设置断点
2. 在 Test Runner 中右键点击测试
3. 选择 `Debug` 运行
4. Visual Studio 会在断点处暂停

### 技巧 3: 过滤日志

在 Console 窗口中：
- 搜索 `[TestRuntimeManager]` - 查看测试框架日志
- 搜索 `[Test]` - 查看测试用例日志
- 搜索 `[ConfigOutput]` - 查看配置输出日志

### 技巧 4: 保存测试输出

如果需要保存配置输出：

```csharp
// 修改 Test_OutputConfigData 中的输出路径
string outputPath = "D:/ConfigOutput"; // 固定路径，不会被清理
```

## 故障排除

### 问题 1: 测试编译错误

**症状**: 代码无法编译，提示找不到 TestRuntimeManager 或 ConfigOutputHelper

**解决方案**:
- 检查文件是否正确放置在 `Assets/XMFrame/Editor/ConfigEditor/` 目录
- 确保 .meta 文件存在
- 尝试刷新 Unity（Ctrl+R 或 Assets → Refresh）

### 问题 2: 测试一直失败

**症状**: 所有测试都失败，提示管理器初始化失败

**可能原因**:
- Mod 配置文件缺失或损坏
- DLL 文件不存在
- 依赖关系配置错误

**解决方案**:
1. 检查日志中的具体错误信息
2. 验证 Mod 配置文件存在于 `Assets/../Mods/` 目录
3. 确保 DLL 文件存在且可访问
4. 检查管理器的依赖关系配置

### 问题 3: 配置输出测试被跳过

**症状**: `Test_OutputConfigData` 显示 "Ignored"

**原因**: `ENABLE_CONFIG_OUTPUT` 未定义

**解决方案**:
- 这是正常的，该测试是可选的
- 如果需要启用，在 Player Settings 中添加 `ENABLE_CONFIG_OUTPUT` 编译符号

### 问题 4: 找不到输出文件

**症状**: 测试通过，但找不到输出文件

**原因**: 输出到临时目录，可能已被清理

**解决方案**:
1. 在测试日志中查找输出路径：
   ```
   [ConfigOutput] 配置输出完成，路径: C:\Users\xxx\AppData\Local\Temp\XMFrameTest_xxxxx\ConfigOutput
   ```
2. 立即访问该目录（测试完成后会清理）
3. 或者修改代码使用固定路径

### 问题 5: 性能测试结果不准确

**症状**: 性能测试时间不稳定

**原因**: 初始化在 OneTimeSetUp 中完成，不在测试方法内

**说明**: 
- `Test_ManagerInitializationPerformance` 只记录管理器列表，不测量实际初始化时间
- 如需准确测量，需要在测试方法内部进行初始化

## 高级用法

### 自定义配置输出

扩展 ConfigOutputHelper 添加自定义输出：

```csharp
public class CustomConfigOutputHelper : ConfigOutputHelper
{
    public void OutputCustomData(string outputDirectory)
    {
        // 自定义输出逻辑
        var builder = new StringBuilder();
        builder.AppendLine("========== 自定义数据 ==========");
        
        // 添加数据
        
        string filePath = Path.Combine(outputDirectory, "CustomData.txt");
        File.WriteAllText(filePath, builder.ToString(), Encoding.UTF8);
    }
}
```

### 创建自定义测试

继承 IntegrationTestBase 创建新测试：

```csharp
[TestFixture]
public class MyCustomIntegrationTest : IntegrationTestBase
{
    [Test]
    public void Test_MyFeature()
    {
        // 可以使用 TestDataDirectory 属性
        string testFile = Path.Combine(TestDataDirectory, "test.xml");
        
        // 编写测试逻辑
    }
}
```

## 相关文件

- `TestRuntimeManager.cs` - 主测试文件
- `ConfigOutputHelper.cs` - 配置输出工具
- `TEST_RUNTIME_MANAGER_README.md` - 详细文档
- `IntegrationTestBase.cs` - 集成测试基类

## 更多帮助

如有问题，请查看：
1. 完整文档：[TEST_RUNTIME_MANAGER_README.md](Assets/XMFrame/Editor/ConfigEditor/Tests/TEST_RUNTIME_MANAGER_README.md)
2. 测试基类：[IntegrationTestBase.cs](Assets/XMFrame/Editor/ConfigEditor/Tests/Base/IntegrationTestBase.cs)
3. 日志输出：Unity Console 窗口

---

**最后更新**: 2026-02-03
