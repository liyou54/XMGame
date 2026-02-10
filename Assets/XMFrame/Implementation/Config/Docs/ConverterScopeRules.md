# 转换器作用域规则

## 核心规则

转换器的作用域由**定义位置**和**Domain参数**共同决定:

### 1. 全局转换器
**定义**: 程序集级 + Domain为空字符串

```csharp
[assembly: XmlGlobalConvert(typeof(StringToIntConverter), "")]
```

**作用范围**: 所有Mod的所有字段

**优先级**: 最低 (Priority = 2)

### 2. Mod转换器
**定义**: 程序集级 + Domain为Mod名

```csharp
[assembly: XmlGlobalConvert(typeof(ModSpecificConverter), "MyMod")]
```

**作用范围**: 仅MyMod下的所有字段

**优先级**: 中等 (Priority = 1)

### 3. 字段转换器
**定义**: 字段级 + Domain为自定义值

```csharp
public class MyConfig
{
    [XmlTypeConverter(typeof(FieldConverter), "customDomain")]
    public string Field;
}
```

**作用范围**: 仅当前字段

**优先级**: 最高 (Priority = 0)

## 匹配规则

### 查找顺序
1. 查找字段级转换器 (Priority = 0)
2. 查找Mod级转换器 (Priority = 1, Domain = ModName)
3. 查找全局级转换器 (Priority = 2, Domain = "")

### Domain匹配
- 字段级: Domain可以是任意自定义值
- Mod级: Domain必须等于Mod名
- 全局级: Domain必须为空字符串

## 容器转换器

### Dictionary<K, V>
可以分别为Key和Value指定转换器:

```csharp
[XmlTypeConverter(typeof(KeyConverter), "key")]
[XmlTypeConverter(typeof(ValueConverter), "value")]
public Dictionary<string, int> Map;
```

**转换器自动匹配**:
- KeyConverter的签名 `Convert(string, out string)` → 匹配Key类型
- ValueConverter的签名 `Convert(string, out int)` → 匹配Value类型

### List<T> / HashSet<T>
为元素指定转换器:

```csharp
[XmlTypeConverter(typeof(ElementConverter), "element")]
public List<string> Items;
```

### 嵌套容器
转换器只作用于最内层元素:

```csharp
[XmlTypeConverter(typeof(IntConverter), "element")]
public List<List<int>> Matrix;
// 转换器只转换 int,框架处理 List<List<>> 结构
```

## 示例

### 完整示例
```csharp
// 全局转换器(所有Mod)
[assembly: XmlGlobalConvert(typeof(GlobalStringToInt), "")]

// Mod转换器(仅MyMod)
[assembly: XmlGlobalConvert(typeof(MyModConverter), "MyMod")]

namespace MyMod
{
    public class ItemConfig
    {
        // 使用字段级转换器(最高优先级)
        [XmlTypeConverter(typeof(CustomConverter), "item")]
        public string Name;
        
        // 使用Mod级转换器(中等优先级)
        public int Level;  // 如果MyMod注册了 string->int 转换器
        
        // 使用全局级转换器(最低优先级)
        public float Value;  // 如果全局注册了 string->float 转换器
        
        // 容器转换器
        [XmlTypeConverter(typeof(KeyConverter), "key")]
        [XmlTypeConverter(typeof(ValueConverter), "value")]
        public Dictionary<string, int> Map;
    }
}
```

## 优先级总结

| 级别 | 定义位置 | Domain | Priority | 作用范围 |
|------|---------|--------|----------|---------|
| 字段级 | 字段上 | 自定义值 | 0 | 当前字段 |
| Mod级 | 程序集上 | Mod名 | 1 | 当前Mod所有字段 |
| 全局级 | 程序集上 | 空字符串 | 2 | 所有Mod所有字段 |

**查找策略**: 按Priority从小到大查找,Priority相同时按Domain匹配
