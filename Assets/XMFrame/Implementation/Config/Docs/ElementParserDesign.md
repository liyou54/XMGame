# 容器元素自定义解析器设计

## 核心概念

**元素解析器只解析最内层元素,容器结构由框架自动处理**

### 错误理解 ❌
```csharp
// 解析整个容器
public static bool Parse(XmlElement element, out List<string> result)
{
    result = new List<string>();
    // 手动遍历所有子节点...
}
```

### 正确理解 ✅
```csharp
// 只解析单个元素
public static bool ParseElement(XmlElement element, out string result)
{
    result = element.InnerText;
    return true;
}

// 框架自动处理容器结构:
// List<string> -> 框架遍历子节点,每个调用 ParseElement()
```

## 使用方式

### 1. List 元素解析

```csharp
// 配置类
public class MyConfig
{
    [XmlElementParser(typeof(CustomStringParser), "Element")]
    public List<string> Items;
}

// 解析器
public class CustomStringParser
{
    public static bool ParseElement(XmlElement element, out string result)
    {
        result = element.GetAttribute("value") ?? element.InnerText;
        return true;
    }
}
```

**XML示例**:
```xml
<Items>
    <Item value="custom1">text1</Item>
    <Item value="custom2">text2</Item>
</Items>
```

**解析流程**:
1. 框架遍历 `<Items>` 的子节点
2. 对每个 `<Item>` 调用 `CustomStringParser.ParseElement()`
3. 将结果添加到 `List<string>`

### 2. Dictionary Key 解析

```csharp
public class MyConfig
{
    [XmlElementParser(typeof(CustomKeyParser), "Key")]
    public Dictionary<string, int> Map;
}

public class CustomKeyParser
{
    public static bool ParseElement(XmlElement element, out string result)
    {
        result = element.GetAttribute("key");
        return true;
    }
}
```

### 3. Dictionary Value 解析

```csharp
public class MyConfig
{
    [XmlElementParser(typeof(CustomIntParser), "Value")]
    public Dictionary<string, int> Map;
}

public class CustomIntParser
{
    public static bool ParseElement(XmlElement element, out int result)
    {
        var text = element.GetAttribute("val");
        return int.TryParse(text, out result);
    }
}
```

### 4. 嵌套容器元素解析

```csharp
// 只解析最内层元素
[XmlElementParser(typeof(CustomStringParser), "Element")]
public List<List<string>> NestedList;

// 解析器只处理最内层的 string
// 框架自动处理 List<List<>> 的结构
```

**XML示例**:
```xml
<NestedList>
    <Row>
        <Cell>A1</Cell>
        <Cell>A2</Cell>
    </Row>
    <Row>
        <Cell>B1</Cell>
        <Cell>B2</Cell>
    </Row>
</NestedList>
```

**解析流程**:
1. 框架识别外层 List
2. 遍历 `<Row>` 节点,识别内层 List
3. 遍历 `<Cell>` 节点,调用 `ParseElement()` 解析 string
4. 构建完整的 `List<List<string>>`

## 解析器签名

### 标准签名
```csharp
public static bool ParseElement(XmlElement element, out T result)
```

### 参数说明
- `element`: 单个元素的XML节点
- `result`: 解析后的元素值
- 返回值: 解析是否成功

## Target 参数

### 对于 List<T> / HashSet<T>
- `Target = "Element"` - 解析元素 T

### 对于 Dictionary<K, V>
- `Target = "Key"` - 解析Key K
- `Target = "Value"` - 解析Value V
- `Target = "Both"` - 同时解析Key和Value(需要两个解析器)

## 嵌套容器处理

### 原则
**解析器只关注最内层元素,所有容器层级由框架自动处理**

### 示例

#### List<List<int>>
```csharp
[XmlElementParser(typeof(IntParser), "Element")]
public List<List<int>> Matrix;

// 解析器只解析 int,不关心 List 结构
public class IntParser
{
    public static bool ParseElement(XmlElement element, out int result)
    {
        return int.TryParse(element.InnerText, out result);
    }
}
```

#### Dictionary<string, List<int>>
```csharp
[XmlElementParser(typeof(IntParser), "Value")]
public Dictionary<string, List<int>> Map;

// 解析器只解析最内层的 int
// 框架自动处理: Dictionary -> List<int> -> int
```

#### List<Dictionary<K, List<V>>>
```csharp
[XmlElementParser(typeof(VParser), "Value")]
public List<Dictionary<string, List<int>>> Complex;

// 解析器只解析最内层的 int (V)
// 框架自动处理三层结构
```

## 优势

1. **简化解析器** - 只需关注单个元素
2. **复用性高** - 同一个解析器可用于不同容器
3. **框架统一处理** - 容器遍历逻辑由框架实现
4. **支持无限嵌套** - 解析器不关心嵌套层级

## 代码生成

生成的代码会根据容器结构自动生成遍历逻辑:

```csharp
// 生成的解析代码示例
foreach (XmlElement item in itemsElement.ChildNodes)
{
    if (CustomStringParser.ParseElement(item, out var value))
    {
        list.Add(value);
    }
}
```
