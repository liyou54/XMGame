# 配置打印解决方案 - 解决反射栈指针问题

## 问题描述

在 `ConfigUnmanagedTestCase` 运行时测试中,尝试使用反射调用 `ToString(XBlobContainer)` 方法时遇到错误:

```
[Warning] 打印表配置失败: Cannot invoke method with stack pointers via reflection
```

**错误位置**: `Assets/XMFrame/Implementation/RuntimeTest/Tests/ConfigUnmanagedTestCase.cs:336`

```csharp
// 第 306 行 - 问题代码
var toStringMethod = unmanagedType.GetMethod("ToString", new[] { typeof(XBlobContainer) });
string configStr = (string)toStringMethod.Invoke(configValue, new object[] { configData.BlobContainer });
```

## 根本原因

`XBlobContainer` 是一个包含 **栈指针** (`unsafe byte*`) 的结构体,C# 反射机制不支持调用包含栈指针参数的方法。

```csharp
public struct XBlobContainer
{
    private unsafe byte* _ptr;  // 栈指针,反射无法处理
    // ...
}
```

## 解决方案

在 MyMod 中创建专门的配置打印模块,**直接调用** `ToString(container)` 方法而不使用反射。

### 方案优势

✅ **避免反射** - 直接调用,无栈指针问题  
✅ **类型安全** - 编译时检查,不会出现运行时类型错误  
✅ **性能更好** - 直接调用比反射快得多  
✅ **易于扩展** - 添加新配置表只需复制模板代码  
✅ **调试友好** - 可以设置断点,查看具体值  

## 实现文件

### 1. 核心打印工具
**文件**: `XModTest/Assets/Mods/MyMod/Scripts/ConfigPrinter.cs`

```csharp
public static class ConfigPrinter
{
    // 打印 MyItemConfig 表
    public static string PrintMyItemConfigs()
    {
        var map = configData.Value.GetMap<CfgI<MyItemConfigUnManaged>, MyItemConfigUnManaged>(tblI);
        foreach (var kvp in map.GetEnumerator(container))
        {
            // ✅ 直接调用 - 无反射
            string configStr = kvp.Value.ToString(container);
        }
    }
    
    // 打印 TestConfig 表
    public static string PrintTestConfigs() { ... }
    
    // 打印所有配置
    public static string PrintAllMyModConfigs() { ... }
}
```

### 2. 编辑器窗口
**文件**: `XModTest/Assets/Mods/MyMod/Editor/ConfigPrinterWindow.cs`

提供可视化界面:
- 菜单: `MyMod > 配置打印工具`
- 按钮: 打印 MyItemConfig / TestConfig / 所有配置
- 功能: 清空输出、复制到剪贴板、自动滚动

### 3. 测试脚本
**文件**: `XModTest/Assets/Mods/MyMod/Scripts/ConfigPrinterTest.cs`

提供菜单命令:
- `MyMod > 测试 > 打印所有配置到控制台`
- `MyMod > 测试 > 打印 MyItemConfig`
- `MyMod > 测试 > 打印 TestConfig`
- `MyMod > 测试 > 打印并保存到文件`

### 4. 程序集定义
**文件**: `XModTest/Assets/Mods/MyMod/Editor/MyMod.Editor.asmdef`

确保编辑器代码能正确引用 MyMod 程序集。

## 使用方法

### 方式一: 编辑器窗口 (推荐)

1. **运行游戏** (必须在 Play Mode)
2. 打开菜单: `MyMod > 配置打印工具`
3. 点击对应按钮查看结果
4. 可以复制到剪贴板或保存到文件

### 方式二: 菜单命令

1. **运行游戏**
2. 点击菜单: `MyMod > 测试 > 打印所有配置到控制台`
3. 在 Unity Console 中查看输出

### 方式三: 代码调用

```csharp
#if UNITY_EDITOR
using MyMod;

// 在运行时任何地方调用
string result = ConfigPrinter.PrintAllMyModConfigs();
UnityEngine.Debug.Log(result);
#endif
```

## 输出示例

```
=== MyMod 所有配置表打印 ===

时间: 2026-02-03 01:36:00

================================================================================

=== MyItemConfig 配置打印 ===

表名: MyMod::MyItemConfig
TableId: 123
--------------------------------------------------------------------------------
配置数量: 5

[1] CfgI=CfgI(1)
    MyItemConfigUnManaged {Id=MyMod::Item_Sword, Name=剑, Level=10, Tags=[1, 2, 3] }

[2] CfgI=CfgI(2)
    MyItemConfigUnManaged {Id=MyMod::Item_Shield, Name=盾, Level=5, Tags=[4, 5] }

[3] CfgI=CfgI(3)
    MyItemConfigUnManaged {Id=MyMod::Item_Potion, Name=药水, Level=1, Tags=[6] }

...

================================================================================

=== TestConfig 配置打印 ===

表名: MyMod::TestConfig
TableId: 124
--------------------------------------------------------------------------------
配置数量: 3

[1] CfgI=CfgI(1)
    TestConfigUnManaged {Id=MyMod::Test_001, TestInt=100, TestSample=[1, 2, 3], 
    TestDictSample={1:10, 2:20}, TestKeyList=[MyMod::Test_002, MyMod::Test_003], ...}

...
```

## 扩展到其他配置表

如果需要打印其他配置表,在 `ConfigPrinter.cs` 中添加新方法:

```csharp
public static string PrintYourConfig()
{
    var sb = new StringBuilder();
    sb.AppendLine("=== YourConfig 配置打印 ===\n");

    try
    {
        var dataCenter = IConfigDataCenter.I;
        var configData = GetConfigData(dataCenter);
        
        // 1. 获取 ClassHelper
        var helper = dataCenter.GetClassHelper<YourConfig, YourConfigUnManaged>();
        var tblS = helper.GetTblS();
        var tblI = dataCenter.GetTblI(tblS);
        
        // 2. 获取 Map
        var map = configData.Value.GetMap<CfgI<YourConfigUnManaged>, YourConfigUnManaged>(tblI);
        var container = configData.Value.BlobContainer;
        
        // 3. 遍历打印
        foreach (var kvp in map.GetEnumerator(container))
        {
            var cfgI = kvp.Key;
            var config = kvp.Value;
            
            // ✅ 直接调用 ToString(container) - 不使用反射
            string configStr = config.ToString(container);
            
            sb.AppendLine($"CfgI={cfgI}");
            sb.AppendLine($"  {configStr}");
        }
    }
    catch (Exception ex)
    {
        sb.AppendLine($"异常: {ex.Message}");
    }

    return sb.ToString();
}
```

然后在 `PrintAllMyModConfigs()` 中调用:

```csharp
public static string PrintAllMyModConfigs()
{
    var sb = new StringBuilder();
    sb.AppendLine(PrintMyItemConfigs());
    sb.AppendLine(PrintTestConfigs());
    sb.AppendLine(PrintYourConfig());  // 添加新配置
    return sb.ToString();
}
```

## 技术对比

### ❌ 原方案 (反射 - 失败)

```csharp
// ConfigUnmanagedTestCase.cs:306
var toStringMethod = unmanagedType.GetMethod("ToString", new[] { typeof(XBlobContainer) });
string configStr = (string)toStringMethod.Invoke(configValue, new object[] { container });
// ❌ 错误: Cannot invoke method with stack pointers via reflection
```

**问题**:
- 反射无法处理栈指针参数
- 运行时类型错误
- 性能较差

### ✅ 新方案 (直接调用 - 成功)

```csharp
// ConfigPrinter.cs
var map = configData.Value.GetMap<CfgI<MyItemConfigUnManaged>, MyItemConfigUnManaged>(tblI);
foreach (var kvp in map.GetEnumerator(container))
{
    string configStr = kvp.Value.ToString(container);  // ✅ 直接调用
}
```

**优势**:
- 无反射,无栈指针问题
- 编译时类型检查
- 性能更好
- 易于调试

## 注意事项

1. **必须在运行时使用** - 配置数据只在 Play Mode 下加载
2. **仅在 UNITY_EDITOR 下可用** - 生产环境不包含此工具
3. **性能考虑** - 打印大量配置可能耗时,已限制打印数量
4. **类型安全** - 每个配置表需要单独实现打印方法

## 相关文件清单

### MyMod 文件
```
XModTest/Assets/Mods/MyMod/
├── Scripts/
│   ├── ConfigPrinter.cs              # 核心打印工具
│   ├── ConfigPrinterTest.cs          # 测试脚本(菜单命令)
│   └── ...
├── Editor/
│   ├── ConfigPrinterWindow.cs        # 编辑器窗口
│   └── MyMod.Editor.asmdef           # 编辑器程序集定义
└── CONFIG_PRINTER_README.md          # 详细使用说明
```

### 原始测试文件
```
Assets/XMFrame/Implementation/RuntimeTest/Tests/
└── ConfigUnmanagedTestCase.cs        # 原始测试用例(有反射问题)
```

## 总结

通过在 MyMod 中创建专门的配置打印模块,我们成功解决了反射调用栈指针方法的问题。这个方案不仅解决了技术问题,还提供了更好的用户体验和性能。

**关键要点**:
- 问题: 反射无法调用包含栈指针参数的方法
- 解决: 直接调用 `ToString(container)`,避免反射
- 优势: 类型安全、性能更好、易于扩展
- 使用: 编辑器窗口、菜单命令、代码调用三种方式

---

**创建时间**: 2026-02-03  
**问题来源**: ConfigUnmanagedTestCase 运行时测试  
**解决方案**: MyMod 配置打印工具
