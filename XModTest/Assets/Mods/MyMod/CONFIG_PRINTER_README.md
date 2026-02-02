# MyMod 配置打印工具使用说明

## 问题背景

在运行时测试系统 `ConfigUnmanagedTestCase` 中,使用反射调用 `ToString(XBlobContainer)` 方法时会遇到以下错误:

```
Cannot invoke method with stack pointers via reflection
```

这是因为 `XBlobContainer` 是包含栈指针的结构体,反射无法处理这种类型的参数。

## 解决方案

在 MyMod 中创建专门的配置打印模块,**直接调用** `ToString(container)` 方法而不使用反射,从而避免栈指针问题。

## 文件说明

### 1. ConfigPrinter.cs
位置: `XModTest/Assets/Mods/MyMod/Scripts/ConfigPrinter.cs`

核心打印工具类,提供以下方法:

- `PrintMyItemConfigs()` - 打印 MyItemConfig 表的所有配置
- `PrintTestConfigs()` - 打印 TestConfig 表的所有配置
- `PrintAllMyModConfigs()` - 打印所有 MyMod 配置表

**关键实现**:
```csharp
// 直接调用 ToString(container) - 不使用反射
string configStr = config.ToString(container);
```

### 2. ConfigPrinterWindow.cs
位置: `XModTest/Assets/Mods/MyMod/Editor/ConfigPrinterWindow.cs`

Unity 编辑器窗口,提供可视化界面来调用打印工具。

## 使用方法

### 方式一: 使用编辑器窗口 (推荐)

1. **运行游戏** - 必须在 Play Mode 下使用
2. 打开菜单: `MyMod > 配置打印工具`
3. 点击对应按钮:
   - "打印 MyItemConfig" - 打印物品配置
   - "打印 TestConfig" - 打印测试配置
   - "打印所有配置" - 打印所有 MyMod 配置
4. 查看输出窗口中的结果
5. 可以点击"复制到剪贴板"保存结果

### 方式二: 代码调用

在任何需要打印配置的地方直接调用:

```csharp
#if UNITY_EDITOR
using MyMod;

// 打印 MyItemConfig
string result = ConfigPrinter.PrintMyItemConfigs();
UnityEngine.Debug.Log(result);

// 打印 TestConfig
string result2 = ConfigPrinter.PrintTestConfigs();
UnityEngine.Debug.Log(result2);

// 打印所有配置
string result3 = ConfigPrinter.PrintAllMyModConfigs();
UnityEngine.Debug.Log(result3);
#endif
```

## 输出示例

```
=== MyItemConfig 配置打印 ===

表名: MyMod::MyItemConfig
TableId: 123
--------------------------------------------------------------------------------
配置数量: 5

[1] CfgI=CfgI(1)
    MyItemConfigUnManaged {Id=MyMod::Item_Sword, Name=剑, Level=10, Tags=[1, 2, 3] }

[2] CfgI=CfgI(2)
    MyItemConfigUnManaged {Id=MyMod::Item_Shield, Name=盾, Level=5, Tags=[4, 5] }

...
```

## 扩展其他配置表

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
        
        // 获取 ClassHelper
        var helper = dataCenter.GetClassHelper<YourConfig, YourConfigUnManaged>();
        var tblS = helper.GetTblS();
        var tblI = dataCenter.GetTblI(tblS);
        
        // 获取 Map
        var map = configData.Value.GetMap<CfgI<YourConfigUnManaged>, YourConfigUnManaged>(tblI);
        var container = configData.Value.BlobContainer;
        
        // 遍历打印
        foreach (var kvp in map.GetEnumerator(container))
        {
            var config = kvp.Value;
            // 直接调用 ToString(container) - 不使用反射
            string configStr = config.ToString(container);
            sb.AppendLine(configStr);
        }
    }
    catch (Exception ex)
    {
        sb.AppendLine($"异常: {ex.Message}");
    }

    return sb.ToString();
}
```

## 注意事项

1. **必须在运行时使用** - 配置数据只在 Play Mode 下加载
2. **仅在 UNITY_EDITOR 下可用** - 生产环境不包含此工具
3. **性能考虑** - 打印大量配置可能耗时,建议限制打印数量
4. **类型安全** - 直接调用避免了反射,编译时就能发现类型错误

## 技术细节

### 为什么反射会失败?

`XBlobContainer` 是一个包含栈指针的结构体:
```csharp
public struct XBlobContainer
{
    private unsafe byte* _ptr;  // 栈指针
    // ...
}
```

C# 反射不支持调用包含栈指针参数的方法,会抛出异常:
```
Cannot invoke method with stack pointers via reflection
```

### 解决方案的关键

通过泛型和直接调用,避免使用反射:

```csharp
// ❌ 错误: 使用反射调用
var toStringMethod = unmanagedType.GetMethod("ToString", new[] { typeof(XBlobContainer) });
string result = (string)toStringMethod.Invoke(configValue, new object[] { container });

// ✅ 正确: 直接调用
var map = configData.GetMap<CfgI<MyItemConfigUnManaged>, MyItemConfigUnManaged>(tblI);
foreach (var kvp in map.GetEnumerator(container))
{
    string result = kvp.Value.ToString(container);  // 直接调用,无反射
}
```

## 相关文件

- 配置打印工具: `XModTest/Assets/Mods/MyMod/Scripts/ConfigPrinter.cs`
- 编辑器窗口: `XModTest/Assets/Mods/MyMod/Editor/ConfigPrinterWindow.cs`
- 原始测试用例: `Assets/XMFrame/Implementation/RuntimeTest/Tests/ConfigUnmanagedTestCase.cs`

## 更新日志

- 2026-02-03: 创建配置打印工具,解决反射栈指针问题
