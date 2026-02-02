# TestRuntimeManager 集成测试说明

## 概述

TestRuntimeManager 是一个完整的运行时管理器集成测试套件，用于验证所有管理器的初始化流程和配置系统的完整性。

## 文件结构

```
Assets/XMFrame/Editor/ConfigEditor/
├── Tests/
│   └── TestRuntimeManager.cs          # 运行时管理器集成测试
└── ConfigOutputHelper.cs              # 配置输出辅助工具（独立文件）
```

## 条件编译说明

### 编译符号

这些功能使用条件编译控制，支持以下编译符号：

1. **UNITY_INCLUDE_TESTS** (Unity 内置)
   - Unity 测试框架自动定义
   - 在测试程序集中默认启用
   - 确保测试代码只在测试环境中编译

2. **ENABLE_CONFIG_OUTPUT** (自定义)
   - 需要手动定义来启用配置输出功能
   - 可以单独启用配置输出，而不启用所有测试

### 如何配置条件编译

#### 方法 1: 在 Player Settings 中添加全局定义

1. 打开 Unity 编辑器
2. 进入 `Edit` → `Project Settings` → `Player`
3. 在 `Other Settings` → `Scripting Define Symbols` 中添加：
   ```
   ENABLE_CONFIG_OUTPUT
   ```
4. 点击 `Apply` 保存

#### 方法 2: 在代码中使用条件编译

测试代码已包含条件编译指令：

```csharp
#if UNITY_INCLUDE_TESTS || ENABLE_CONFIG_OUTPUT
// 测试代码
#endif
```

配置输出功能：

```csharp
#if UNITY_EDITOR && (UNITY_INCLUDE_TESTS || ENABLE_CONFIG_OUTPUT)
// 配置输出代码
#endif
```

#### 方法 3: 使用 asmdef 的 defineConstraints

在测试程序集的 asmdef 文件中已配置：

```json
{
  "defineConstraints": [
    "UNITY_INCLUDE_TESTS"
  ]
}
```

## 测试功能

### TestRuntimeManager 测试套件

#### 1. Test_AllManagersInitialized
- 验证所有管理器是否成功初始化
- 检查关键管理器（ModManager、ConfigDataCenter）是否可用

#### 2. Test_ModManagerLoadedMods
- 测试 ModManager 是否正确加载 Mod
- 验证已启用的 Mod 列表

#### 3. Test_ConfigDataCenterRegisteredTypes
- 测试 ConfigDataCenter 是否正确注册配置类型
- 验证配置 Helper 的可用性

#### 4. Test_ManagerDependencies
- 测试管理器之间的依赖关系
- 验证初始化顺序正确

#### 5. Test_ManagerLifecycle
- 测试管理器生命周期（OnCreate 和 OnInit）
- 确保所有管理器实例有效

#### 6. Test_ConfigDataLoaded
- 测试配置数据是否正确加载
- 验证 Mod 配置加载情况

#### 7. Test_ManagerInitializationPerformance
- 性能测试：测量管理器初始化时间
- 记录管理器列表和性能数据

#### 8. Test_OutputConfigData
- **条件测试**：仅在定义 `ENABLE_CONFIG_OUTPUT` 时执行
- 触发配置输出功能
- 将配置数据输出到文件

## 配置输出功能 (ConfigOutputHelper)

### 功能说明

ConfigOutputHelper 是一个独立的工具类，用于将运行时配置数据输出到文件，便于调试和验证。

### 输出内容

1. **Managers.txt**
   - 所有管理器的列表
   - 管理器类型、完整名称、程序集信息

2. **Mods.txt**
   - 已启用的 Mod 列表
   - Mod 配置详情（版本、作者、描述、DLL 路径等）
   - 配置文件列表

3. **ConfigTypes.txt**
   - 所有配置类型信息
   - 每个 Mod 的配置类型列表
   - ClassHelper 信息

4. **ConfigStatistics.txt**
   - 配置数据统计信息
   - 系统状态

### 使用方法

#### 在测试中使用

```csharp
[Test]
public void Test_OutputConfigData()
{
    var outputHelper = new ConfigOutputHelper();
    string outputPath = "d:/ConfigOutput";
    
    // 输出所有配置
    outputHelper.OutputAllConfigs(outputPath);
    
    // 输出特定配置的详情
    outputHelper.OutputConfigDetail<TestConfig>(outputPath);
}
```

#### 在编辑器脚本中使用

```csharp
#if ENABLE_CONFIG_OUTPUT
using XM.Editor;

public class ConfigDebugWindow : EditorWindow
{
    [MenuItem("Tools/Output Config Data")]
    static void OutputConfigData()
    {
        var outputHelper = new ConfigOutputHelper();
        string outputPath = "Assets/ConfigOutput";
        
        outputHelper.OutputAllConfigs(outputPath);
        
        Debug.Log($"配置已输出到: {outputPath}");
    }
}
#endif
```

## 运行测试

### 在 Unity Test Runner 中运行

1. 打开 `Window` → `General` → `Test Runner`
2. 切换到 `EditMode` 标签
3. 找到 `XM.Editor.Tests.Integration` → `TestRuntimeManager`
4. 点击 `Run All` 或单独运行某个测试

### 通过命令行运行

```bash
Unity.exe -runTests -testPlatform EditMode -testFilter "TestRuntimeManager"
```

## 测试顺序

测试使用 `[Order]` 特性控制执行顺序：

1. Order(1): 验证管理器初始化
2. Order(2): 验证 Mod 加载
3. Order(3): 验证配置类型注册
4. Order(4): 验证管理器依赖关系
5. Order(5): 验证管理器生命周期
6. Order(6): 验证配置数据加载
7. Order(7): 性能测试
8. Order(8): 配置输出（条件执行）

## 调试建议

### 查看日志

测试过程中会输出详细的日志信息：

```csharp
XLog.Info("[TestRuntimeManager] 所有管理器初始化完成");
XLog.InfoFormat("[Test] 已初始化 {0} 个管理器", allManagers.Count);
```

### 配置日志级别

在 GameMain 中可以配置日志级别：

```csharp
// 常规测试：打开日志
XLog.CurrentLogLevel = LogLevel.Debug;

// 性能测试：关闭输出
XLog.CurrentLogLevel = LogLevel.PerformanceTest;
```

### 临时测试数据

测试使用 `IntegrationTestBase` 提供的临时目录：

```csharp
protected string TestDataDirectory { get; private set; }
```

测试完成后会自动清理。

## 常见问题

### Q1: 测试编译错误 - 找不到类型

**原因**: 条件编译符号未定义

**解决方案**:
- 确保在 Player Settings 中添加了 `ENABLE_CONFIG_OUTPUT`（如果需要配置输出）
- 或者在测试程序集的 asmdef 中定义了 `UNITY_INCLUDE_TESTS`

### Q2: Test_OutputConfigData 被跳过

**原因**: `ENABLE_CONFIG_OUTPUT` 未定义

**解决方案**:
- 在 Player Settings → Scripting Define Symbols 中添加 `ENABLE_CONFIG_OUTPUT`
- 或者接受跳过（该测试是可选的）

### Q3: 管理器初始化失败

**原因**: 可能的依赖关系问题或配置错误

**解决方案**:
- 检查日志输出，查看具体错误信息
- 验证 Mod 配置文件是否正确
- 确保 DLL 文件存在且可访问

### Q4: 配置输出路径不存在

**原因**: 输出目录不存在或没有写入权限

**解决方案**:
- ConfigOutputHelper 会自动创建目录
- 确保有足够的磁盘空间和写入权限

## 扩展开发

### 添加新的测试

```csharp
[Test]
[Order(9)]
public void Test_CustomFeature()
{
    // Arrange
    Assert.IsNotNull(IConfigDataCenter.I);
    
    // Act
    // 执行测试操作
    
    // Assert
    // 验证结果
}
```

### 添加新的输出功能

```csharp
private void OutputCustomInfo(string outputDirectory)
{
    _stringBuilder.Clear();
    _stringBuilder.AppendLine("========== 自定义信息 ==========");
    
    // 添加自定义输出逻辑
    
    string filePath = Path.Combine(outputDirectory, "CustomInfo.txt");
    File.WriteAllText(filePath, _stringBuilder.ToString(), Encoding.UTF8);
}
```

## 性能考虑

- 集成测试会初始化完整的管理器系统，执行时间较长（秒级）
- 建议在 CI/CD 流程中单独运行集成测试
- 性能测试使用 `[Performance]` 特性标记
- 可以通过日志级别控制输出详细程度

## 相关文档

- [IntegrationTestBase.cs](Base/IntegrationTestBase.cs) - 集成测试基类
- [TestBase.cs](Base/TestBase.cs) - 测试基类
- [TestCategories.cs](Base/TestCategories.cs) - 测试分类定义
- [GameMain.cs](../../../GameMain.cs) - 管理器初始化入口
- [ConfigDataCenter.cs](../../../Implementation/XConfigManager/ConfigDataCenter.cs) - 配置数据中心

## 更新日志

### v1.0.0 (2026-02-03)
- ✅ 创建 TestRuntimeManager 集成测试
- ✅ 实现 ConfigOutputHelper 配置输出工具
- ✅ 添加条件编译支持
- ✅ 完成所有测试用例
- ✅ 分离测试和输出功能到不同文件

---

**注意**: 这些功能主要用于开发和调试阶段，不应该在生产环境中启用。通过条件编译确保这些代码不会包含在最终构建中。
