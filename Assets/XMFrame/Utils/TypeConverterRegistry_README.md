# 自定义类型转换注册系统使用指南

## 概述

该系统允许你为配置字段注册自定义类型转换器，支持全局和局部（域）转换器。

## 核心组件

### 1. 特性定义

**`XmlTypeConverterAttribute`** - 用于标记需要转换的字段

```csharp
[XmlTypeConverter("ConverterName", typeof(SourceType), typeof(TargetType), domain = "")]
```

参数说明：
- `converterName`: 转换器名称（用于代码生成）
- `sourceType`: 源类型（托管类型）
- `targetType`: 目标类型（非托管类型）
- `domain`: 转换域（空字符串表示全局，非空表示局部）

### 2. 转换器注册表

**`TypeConverterRegistry`** - 管理所有转换器

```csharp
// 注册全局转换器
TypeConverterRegistry.RegisterGlobalConverter<TSource, TTarget>(converter);

// 注册局部转换器（指定域）
TypeConverterRegistry.RegisterLocalConverter<TSource, TTarget>("DomainName", converter);

// 执行转换
var result = TypeConverterRegistry.Convert<TSource, TTarget>(source, domain);
```

### 3. 内置转换器

**`BuiltInConverters`** - 提供常用转换器

- `XAssetPath -> XAssetId`
- `Type -> TypeId`

使用方式：
```csharp
BuiltInConverters.RegisterAll(); // 注册所有内置转换器
```

## 使用示例

### 示例1：在配置类中使用全局转换器

```csharp
public class UIConfig : XConfig<UIConfig, UIConfigUnManaged>
{
    [XmlTypeConverter("XAssetPathToXAssetId", typeof(XAssetPath), typeof(XAssetId))]
    public XAssetPath Prefab;
    
    [XmlTypeConverter("TypeToTypeId", typeof(Type), typeof(TypeId))]
    public Type ConfigType;
}
```

生成的代码会包含转换方法：
```csharp
public partial struct UIConfigUnManaged
{
    public XAssetId ConvertPrefab()
    {
        return TypeConverterRegistry.Convert<XAssetPath, XAssetId>(Prefab);
    }
    
    public TypeId ConvertConfigType()
    {
        return TypeConverterRegistry.Convert<Type, TypeId>(ConfigType);
    }
}
```

### 示例2：使用局部转换器（指定域）

```csharp
public class MyConfig : XConfig<MyConfig, MyConfigUnManaged>
{
    [XmlTypeConverter("CustomConverter", typeof(MySourceType), typeof(MyTargetType), domain = "MyDomain")]
    public MySourceType MyField;
}
```

生成的代码：
```csharp
public partial struct MyConfigUnManaged
{
    public MyTargetType ConvertMyField()
    {
        return TypeConverterRegistry.Convert<MySourceType, MyTargetType>(MyField, "MyDomain");
    }
}
```

### 示例3：注册自定义转换器

```csharp
// 实现转换器接口
public class MyCustomConverter : ITypeConverter<MySourceType, MyTargetType>
{
    public MyTargetType Convert(MySourceType source)
    {
        // 实现转换逻辑
        return new MyTargetType { /* ... */ };
    }
}

// 注册全局转换器
TypeConverterRegistry.RegisterGlobalConverter<MySourceType, MyTargetType>(
    new MyCustomConverter());

// 或注册局部转换器
TypeConverterRegistry.RegisterLocalConverter<MySourceType, MyTargetType>(
    "MyDomain", 
    new MyCustomConverter());
```

## 代码生成流程

1. **分析阶段**：`TypeAnalyzer` 检测字段上的 `XmlTypeConverterAttribute`
2. **代码生成阶段**：`UnmanagedCodeGenerator` 为每个需要转换的字段生成转换方法
3. **运行时**：通过 `TypeConverterRegistry` 执行实际转换

## 注意事项

1. 转换器必须在运行时注册后才能使用
2. 局部转换器优先于全局转换器
3. 如果找不到转换器，会抛出 `InvalidOperationException`
4. 建议在应用启动时调用 `BuiltInConverters.RegisterAll()` 注册内置转换器
