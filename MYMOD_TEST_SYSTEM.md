# MyMod 测试系统完整文档

## 📋 概述

本文档描述了为MyMod创建的完整测试系统，包括测试XML数据和自动化测试管理器。

## 🎯 系统目标

1. **全面测试** - 覆盖配置系统的各种使用场景
2. **自动验证** - 启动时自动检测配置是否正确加载
3. **详细报告** - 提供清晰的测试结果反馈
4. **易于扩展** - 便于添加新的测试用例

## 📦 系统组成

### 1. 测试XML文件（XModTest工程）
位置：`XModTest/Assets/Mods/MyMod/Xml/`

| 文件名 | 测试目标 | 数据量 | 说明 |
|--------|---------|--------|------|
| `MyModConfig_Basic.xml` | 基础配置 | 10条 | 简单字段、列表、边界值、中文、特殊字符 |
| `MyModConfig_TestConfig.xml` | 复杂配置 | 10条 | 字典、集合、HashSet、配置键引用、索引 |
| `MyModConfig_Nested.xml` | 嵌套结构 | 10条 | 单层嵌套、嵌套列表、嵌套字典、自定义类型 |
| `MyModConfig_Link.xml` | XMLLink链接 | 10条 | 链接继承、外键引用 |
| `MyModConfig_Edge.xml` | 边界情况 | 10条 | 空值、XML转义、Unicode、Emoji、极限值 |
| **总计** | - | **50条** | - |

### 2. 测试管理器（主工程）
位置：`Assets/XMFrame/Implementation/XConfigManager/ConfigTestManager.cs`

**功能：**
- 自动在配置系统初始化后运行
- 验证50条配置数据是否成功加载
- 生成详细的测试报告
- 支持扩展为字段值验证

## 🏗️ 架构设计

### 数据流程
```
XML文件 (XModTest工程)
    ↓
打包到 Mods/MyMod/ 目录
    ↓
ModManager 加载Mod
    ↓
ConfigDataCenter 解析XML并注册配置
    ↓
ConfigTestManager 验证配置数据
    ↓
输出测试报告
```

### 依赖关系
```
GameMain
  ├─ ModManager (IModManager)
  │    ↓
  ├─ ConfigDataCenter (IConfigManager)
  │    ↓
  └─ ConfigTestManager (IConfigTestManager) [priority: 1000]
```

## 📊 测试覆盖矩阵

### 配置类型覆盖
| 配置类 | Basic | TestConfig | Nested | Link | Edge | 总计 |
|--------|-------|------------|--------|------|------|------|
| MyItemConfig | ✓ | - | - | - | ✓ | 19条 |
| TestConfig | - | ✓ | ✓ | - | ✓ | 21条 |
| NestedConfig | - | - | ✓ | - | - | - |
| TestInhert | - | - | - | ✓ | - | 10条 |

### 功能特性覆盖
| 特性 | 是否覆盖 | 测试文件 | 测试项数 |
|------|----------|----------|----------|
| 基础类型（int, string） | ✅ | All | 50 |
| List集合 | ✅ | Basic, TestConfig | 20 |
| Dictionary字典 | ✅ | TestConfig, Nested | 15 |
| HashSet集合 | ✅ | TestConfig | 10 |
| 配置键引用（CfgS） | ✅ | TestConfig, Nested | 15 |
| XMLLink链接 | ✅ | Link | 10 |
| 嵌套结构 | ✅ | Nested | 10 |
| XmlIndex索引 | ✅ | TestConfig, Nested | 20 |
| XmlStringMode | ✅ | Nested | 10 |
| XmlNotNull | ✅ | Nested | 10 |
| XmlDefault | ✅ | Nested | 10 |
| 自定义类型（int2） | ✅ | Nested | 10 |
| 边界值测试 | ✅ | Basic, Edge | 15 |
| 空值测试 | ✅ | Basic, TestConfig, Edge | 10 |
| 中文字符 | ✅ | Basic, Edge | 5 |
| 特殊字符 | ✅ | Basic, Edge | 5 |
| Unicode/Emoji | ✅ | Edge | 1 |
| XML转义 | ✅ | Edge | 1 |

## 🚀 使用指南

### 1. 运行测试

**方法1：Unity Editor中运行**
1. 打开Unity项目
2. 运行游戏场景
3. 查看Console窗口的测试输出

**方法2：查看日志文件**
```
位置：项目根目录/Logs/
文件：Player.log 或 Editor.log
搜索：[ConfigTestManager] 或 "配置测试"
```

### 2. 解读测试结果

**测试通过示例：**
```
=================================================================
✓ 所有测试通过！配置系统运行正常。
总测试数: 50
通过: 50 (100.0%)
失败: 0
=================================================================
```

**测试失败示例：**
```
【MyModConfig_Basic.xml - 基础配置测试】
-----------------------------------------------------------------
✓ basic_001: 配置存在
✗ basic_002: 配置不存在 (表:MyItemConfig, ID:basic_002)
✓ basic_003: 配置存在
...

【测试总结】
总测试数: 50
通过: 49 (98.0%)
失败: 1
⚠ 有 1 个测试失败，请检查配置数据！
```

### 3. 故障排查

**配置未加载？**
1. 检查XML文件路径：`Mods/MyMod/Xml/*.xml`
2. 验证XML格式是否正确
3. 查看ModManager日志确认Mod已加载
4. 确认配置类有`[XmlDefined]`标记

**测试失败？**
1. 查看详细错误信息
2. 检查配置ID是否唯一
3. 验证cls属性与配置类名匹配
4. 确认ClassHelper已正确生成

## 🔧 配置选项

### 禁用测试管理器
如果不需要自动测试，编辑`ConfigTestManager.cs`：

```csharp
// 注释掉AutoCreate标记
// [AutoCreate(priority: 1000)]
public class ConfigTestManager : ManagerBase<IConfigTestManager>, IConfigTestManager
```

### 自定义测试范围
编辑`ConfigTestManager.OnInit()`方法：

```csharp
public override async UniTask OnInit()
{
    await TestBasicConfigs();      // 保留
    // await TestComplexConfigs(); // 禁用
    await TestNestedConfigs();     // 保留
    // await TestLinkConfigs();    // 禁用
    // await TestEdgeConfigs();    // 禁用
}
```

### 调整日志级别
在`GameMain.cs`中设置：

```csharp
// 详细日志（开发环境）
XLog.CurrentLogLevel = LogLevel.Debug;

// 性能测试（关闭大部分日志）
XLog.CurrentLogLevel = LogLevel.PerformanceTest;
```

## 📈 扩展建议

### 1. 添加字段值验证
当`ConfigDataCenter`查询API完善后：

```csharp
TestConfig("basic_001 - 完整验证", () =>
{
    var config = IConfigManager.I.GetConfig<MyItemConfig>("MyMod::basic_001");
    Assert(config.Name == "普通武器", "名称验证");
    Assert(config.Level == 1, "等级验证");
    Assert(config.Tags.Count == 3, "标签数量验证");
    return null;
});
```

### 2. 添加性能测试

```csharp
private async UniTask TestPerformance()
{
    var sw = System.Diagnostics.Stopwatch.StartNew();
    
    // 批量查询测试
    for (int i = 1; i <= 10; i++)
    {
        var config = IConfigManager.I.GetConfig<MyItemConfig>($"MyMod::basic_{i:D3}");
    }
    
    sw.Stop();
    XLog.InfoFormat("批量查询10个配置耗时: {0}ms", sw.ElapsedMilliseconds);
}
```

### 3. 添加引用完整性测试

```csharp
private async UniTask TestReferenceIntegrity()
{
    // 验证外键引用
    var config = IConfigManager.I.GetConfig<TestConfig>("MyMod::test_002");
    Assert(config.Foreign.Valid, "外键引用应有效");
    
    // 验证XMLLink链接
    var linkConfig = IConfigManager.I.GetConfig<TestInhert>("MyMod::link_001");
    Assert(linkConfig.Link.Valid, "XMLLink应有效");
}
```

## 📚 相关文档

1. **测试XML说明**：`XModTest/Assets/Mods/MyMod/Xml/TEST_XML_README.md`
2. **测试管理器说明**：`Assets/XMFrame/Implementation/XConfigManager/CONFIG_TEST_README.md`
3. **配置系统文档**：`Assets/XMFrame/Implementation/XConfigManager/README.md`（如有）

## ✅ 验收标准

系统验收需满足以下标准：

- [x] 5个测试XML文件创建完成，每个10条数据
- [x] 测试管理器创建完成并集成到GameMain
- [x] 测试覆盖所有配置类型
- [x] 测试报告输出清晰易读
- [x] 配置存在性验证功能正常
- [ ] 配置字段值验证（待API完善）
- [ ] 性能测试（可选）
- [ ] 引用完整性测试（可选）

## 📝 更新日志

### 2026-02-02 - v1.0
- ✅ 创建5个测试XML文件（共50条数据）
- ✅ 创建ConfigTestManager测试管理器
- ✅ 实现配置存在性验证
- ✅ 完善测试报告输出
- ✅ 编写完整文档

### 待开发功能
- ⏳ 配置字段值验证（等待ConfigDataCenter查询API）
- ⏳ 配置引用完整性验证
- ⏳ 性能基准测试
- ⏳ 压力测试（大量配置）

---

## 🎉 总结

本测试系统为MyMod配置提供了全面的自动化测试支持，覆盖了从基础类型到复杂嵌套结构的各种场景。通过50条精心设计的测试数据和自动化测试管理器，能够在每次启动时快速验证配置系统的正确性，大大提高了开发效率和系统可靠性。

**测试数据统计：**
- 📄 XML文件：5个
- 📊 测试数据：50条
- 🎯 配置类型：4种
- ✅ 功能覆盖：18项核心特性

**系统优势：**
1. 自动化 - 无需手动运行测试
2. 全面性 - 覆盖各种边界情况
3. 可扩展 - 易于添加新测试
4. 清晰度 - 报告直观易懂
