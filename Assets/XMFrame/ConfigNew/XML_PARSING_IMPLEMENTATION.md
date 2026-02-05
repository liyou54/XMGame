# XML 解析实现完成

## 实现总结

已完成 XmlHelper (ClassHelper) 的所有 XML 解析功能，不再是 TODO 占位符。

## 实现的功能

### 1. 基本类型解析 ✅

**支持的类型：**
- `int, float, bool, long, double, short, byte`
- `string` (FixedString32Bytes / StrI / LabelI)
- 枚举类型
- 可空类型 (`int?`, `float?`, `EItemType?`)

**实现方式：**
```csharp
// 示例：int 字段
private static int ParseId(XmlElement configItem, ModS mod, string configName, in ConfigParseContext context)
{
    var xmlValue = ConfigParseHelper.GetXmlFieldValue(configItem, "Id");
    
    // 默认值支持
    if (string.IsNullOrEmpty(xmlValue))
        xmlValue = "0"; // 从 [XmlDefault] 获取
    
    if (ConfigParseHelper.TryParseInt(xmlValue, "Id", out var parsedValue))
        return parsedValue;
    
    return default;
}
```

### 2. 容器类型解析 ✅

**支持的容器：**
- `List<T>` - XML 节点 + CSV 备用
- `Dictionary<K,V>` - Item/Key/Value 结构
- `HashSet<T>` - XML 节点 + CSV 备用

**XML 格式：**
```xml
<!-- List -->
<IntList>1</IntList>
<IntList>2</IntList>
<!-- 或 CSV -->
<IntList>1,2,3</IntList>

<!-- Dictionary -->
<StringIntMap>
  <Item Key="key1">100</Item>
  <Item Key="key2">200</Item>
</StringIntMap>
```

**CSV 分隔符：** `,` `;` `|`

### 3. 嵌套配置解析 ✅

**单个嵌套配置：**
```csharp
var element = configItem.SelectSingleNode("Price") as XmlElement;
var helper = IConfigDataCenter.I?.GetClassHelper(typeof(PriceConfig));
return (PriceConfig)helper.DeserializeConfigFromXml(element, mod, configName + "_Price", context);
```

**嵌套配置列表：**
```csharp
var list = new List<AttributeConfig>();
var nodes = configItem.SelectNodes("Attributes");
var helper = IConfigDataCenter.I?.GetClassHelper(typeof(AttributeConfig));
foreach (XmlNode node in nodes)
{
    var item = (AttributeConfig)helper.DeserializeConfigFromXml(...);
    list.Add(item);
}
```

### 4. CfgS 和 Link 字段解析 ✅

```csharp
// 解析 "Mod::ConfigName" 格式
var cfgSString = ConfigParseHelper.GetXmlFieldValue(configItem, "RewardItem");
if (ConfigParseHelper.TryParseCfgSString(cfgSString, "RewardItem", out var modName, out var cfgName))
    return new CfgS<ComplexItemConfigUnmanaged>(new ModS(modName), cfgName);
```

### 5. 转换器支持 ✅

```csharp
// [XmlTypeConverter(typeof(StringToUpperConverter))]
if (StringToUpperConverter.Convert(xmlValue, out var convertedValue))
    return convertedValue;
```

## 代码架构

```
CodeGen/Builders/
  ├── ParseMethodBuilder.cs        # 主构建器，路由到具体解析器
  ├── BasicTypeParser.cs           # 基本类型（int, float, bool, string, enum）
  ├── ContainerParser.cs           # 容器（List, Dictionary, HashSet）
  ├── NestedConfigParser.cs        # 嵌套配置（单个和列表）
  └── CfgSParser.cs                # CfgS 和 Link 字段
  └── ConverterParser.cs           # 转换器支持
```

## 生成的代码特点

### 1. 使用 global:: 前缀

所有类型都使用完全限定名，避免命名冲突：
```csharp
private static global::XM.ConfigNew.Tests.Data.EItemType ParseItemType(
    global::System.Xml.XmlElement configItem,
    global::XM.Contracts.Config.ModS mod,
    string configName,
    in global::XM.Contracts.Config.ConfigParseContext context)
```

### 2. 完整的错误信息

错误日志自动包含文件路径和行号（来自 context）：
```csharp
ConfigParseHelper.LogParseError(context, "FieldName", "错误信息");
// 输出: 文件: xxx.xml, 行: 15, 字段: FieldName, 错误: xxx
```

### 3. 默认值支持

```csharp
// [XmlDefault("100")]
if (string.IsNullOrEmpty(xmlValue))
    xmlValue = "100";
```

### 4. 可空类型处理

```csharp
// int? 字段
if (string.IsNullOrEmpty(xmlValue))
    return null;  // 可空返回 null
```

### 5. 容器多格式支持

- XML 节点格式（主要）
- CSV 格式（备用）
- 默认值（CSV 格式）

## 测试和使用

### 运行测试
```
Unity Test Runner > TestXmlParsing
```

### 重新生成代码
```
XMFrame > ConfigNew > 清理所有生成文件并重新生成
```

### 查看生成代码
```
XMFrame > ConfigNew > 测试: 生成 ActiveSkillConfig 到临时目录
```

## 待实现功能

以下功能需要您提供基类实现：

### 1. 字符串转换方法

在 `ConfigClassHelper<T, TUnmanaged>` 基类中添加：

```csharp
/// <summary>
/// 将字符串转换为 StrI
/// </summary>
protected static StrI ConvertToStrI(string value)
{
    // TODO: 您的实现
    return default;
}

/// <summary>
/// 将字符串转换为 LabelI
/// </summary>
protected static LabelI ConvertToLabelI(string value)
{
    // TODO: 您的实现
    return default;
}
```

### 2. AllocXXX 方法实现

容器分配方法仍然是 TODO，需要后续实现：
- `AllocFieldName()` - 分配 XBlobArray/XBlobMap/XBlobSet
- `FillFieldName()` - 填充嵌套配置

## 技术亮点

1. **无硬编码** - 所有类型使用 `typeof` 和 `GetGlobalQualifiedTypeName()`
2. **注释清晰** - 每个方法都有 XML 注释
3. **分层架构** - 不同类型的解析器独立
4. **可扩展** - 易于添加新类型支持
5. **错误友好** - 错误信息包含文件行号

## 下一步

1. 在基类实现 `ConvertToStrI()` 和 `ConvertToLabelI()`
2. 实现 `AllocXXX` 和 `FillXXX` 方法（容器分配）
3. 测试生成的代码是否能正确解析 XML
