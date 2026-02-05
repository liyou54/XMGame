# Global 限定名实现总结

## 修复完成

为了避免类型名称冲突，所有代码生成器现在都使用 `global::` 前缀来生成完全限定的类型名称。

## 实现的功能

### 1. TypeHelper 新方法

```csharp
/// <summary>
/// 获取类型的全局限定名称(使用 global:: 前缀，避免命名冲突)
/// </summary>
public static string GetGlobalQualifiedTypeName(Type type)
```

**功能：**
- 基本类型使用关键字：`int`, `float`, `string` 等
- 普通类型使用全局限定名：`global::XM.ConfigNew.TestConfigs.AttributeConfig`
- 泛型类型递归处理：`global::System.Collections.Generic.List<int>`
- 可空类型支持：`int?`, `global::XM.ConfigNew.TestConfigs.EItemType?`

### 2. 修改的生成器

| 生成器 | 修改内容 |
|--------|---------|
| **XmlHelperGenerator** | ✅ 类声明、方法签名、变量声明都使用全局限定名 |
| **UnmanagedGenerator** | ✅ 接口实现使用全局限定名 |
| **IndexStructGenerator** | ✅ 接口实现、方法签名使用全局限定名 |
| **TypeAnalyzer** | ✅ `ManagedFieldTypeName` 使用全局限定名 |

### 3. 修改的其他问题

| 问题 | 修复 |
|------|------|
| **可空类型转换** | `int?` → `int` (使用 `.GetValueOrDefault()`) |
| **继承字段支持** | 使用 `BindingFlags.FlattenHierarchy` 获取父类字段 |
| **静态构造函数** | 移除 `public` 访问修饰符 |
| **测试代码冲突** | 重命名 `AttributeConfig` → `SimpleAttributeConfig` |

## 生成的代码示例

### Before (可能有冲突):
```csharp
public class AttributeConfigClassHelper : ConfigClassHelper<AttributeConfig, AttributeConfigUnmanaged>
{
    static AttributeConfigClassHelper()
    {
        CfgS<AttributeConfigUnmanaged>.Table = ...;
    }
    
    public override void AllocContainerWithFillImpl(
        IXConfig value,
        TblI tbli,
        CfgI cfgi,
        ref AttributeConfigUnmanaged data,  // ⚠️ 可能冲突
        ...)
    {
        var config = (AttributeConfig)value;  // ⚠️ 可能冲突
    }
}
```

### After (使用 global::):
```csharp
public class AttributeConfigClassHelper : ConfigClassHelper<global::XM.ConfigNew.TestConfigs.AttributeConfig, global::XM.ConfigNew.TestConfigs.AttributeConfigUnmanaged>
{
    static AttributeConfigClassHelper()
    {
        CfgS<global::XM.ConfigNew.TestConfigs.AttributeConfigUnmanaged>.Table = ...;
    }
    
    public override void AllocContainerWithFillImpl(
        IXConfig value,
        TblI tbli,
        CfgI cfgi,
        ref global::XM.ConfigNew.TestConfigs.AttributeConfigUnmanaged data,  // ✅ 完全限定
        ...)
    {
        var config = (global::XM.ConfigNew.TestConfigs.AttributeConfig)value;  // ✅ 完全限定
    }
}
```

## 可空类型处理

### Before (编译错误):
```csharp
// PriceConfig 有字段: public int? Diamond;
public struct PriceConfigUnmanaged
{
    public int? Diamond;  // ❌ Unmanaged 不支持可空类型
}

public override void AllocContainerWithFillImpl(...)
{
    data.Diamond = config.Diamond;  // ❌ int? -> int? 不匹配
}
```

### After (正确):
```csharp
// PriceConfig 有字段: public int? Diamond;
public struct PriceConfigUnmanaged
{
    public int Diamond;  // ✅ 使用基础类型
}

public override void AllocContainerWithFillImpl(...)
{
    data.Diamond = config.Diamond.GetValueOrDefault();  // ✅ int? -> int
}
```

## 继承字段处理

### Before (缺少字段):
```csharp
// ActiveSkillConfig : BaseSkillConfig
// BaseSkillConfig 有: SkillId, SkillName, MaxLevel
// ActiveSkillConfig 有: ManaCost, Cooldown

public struct ActiveSkillConfigUnmanaged
{
    public int ManaCost;     // ✅ 自己的字段
    public float Cooldown;   // ✅ 自己的字段
    // ❌ 缺少父类字段！
}
```

### After (包含继承字段):
```csharp
public struct ActiveSkillConfigUnmanaged
{
    // 继承自 BaseSkillConfig
    public int SkillId;            // ✅ 父类字段
    public FixedString32Bytes SkillName;   // ✅ 父类字段
    public int MaxLevel;           // ✅ 父类字段
    
    // ActiveSkillConfig 自己的字段
    public int ManaCost;           // ✅ 自己的字段
    public float Cooldown;         // ✅ 自己的字段
}
```

## 使用方式

### 重新生成代码
在 Unity 编辑器中：
1. 打开 `XMFrame > ConfigNew > 代码生成器(测试)`
2. 选择需要生成的配置类型
3. 点击"生成 Unmanaged 代码"

### 调试生成
使用调试菜单：
- `XMFrame > ConfigNew > 调试: 生成 ActiveSkillConfig` - 查看生成代码
- `XMFrame > ConfigNew > 测试: 生成 ActiveSkillConfig 到临时目录` - 生成到临时目录
- `XMFrame > ConfigNew > 调试: 检查继承字段` - 检查字段分析

## 验证测试

运行以下测试验证功能：
- `GlobalQualifiedNameTest` - 验证 global:: 前缀生成
- `MetadataOptimizationTest` - 验证预计算字段
- `XmlHelperGeneratorTest` - 验证 Helper 生成

## 技术要点

1. **global:: 前缀规则**
   - 基本类型使用关键字：`int`, `string`, `float`
   - 用户类型使用完全限定名：`global::Namespace.Type`
   - 泛型参数递归处理

2. **可空类型规则**
   - 托管：`int?` (C# 可空类型)
   - 非托管：`int` (不支持可空，使用基础类型)
   - 赋值：使用 `.GetValueOrDefault()` 转换

3. **继承字段规则**
   - 使用 `BindingFlags.FlattenHierarchy` 获取所有字段
   - 只包含声明在 `IXConfig` 类型中的字段
   - 去重处理避免重复字段

## 已知问题

- ⚠️ 警告：`AttributeConfig` 类型冲突（已通过重命名为 `SimpleAttributeConfig` 修复）
- ✅ 所有编译错误已修复
- ✅ 生成器可以正常工作

## 下一步

现在框架已经完善，可以开始实现字段解析逻辑：
1. 实现 `ParseXXX` 方法（从 XML 解析字段）
2. 实现 `AllocXXX` 方法（分配容器）
3. 实现 `FillXXX` 方法（填充嵌套配置）
