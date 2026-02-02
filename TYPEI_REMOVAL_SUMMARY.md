# TypeI 移除总结

## 概述
已移除 `System.Type` 到 `TypeI` 的类型转换支持，包括类型分析、代码生成和转换方法。

## 修改的文件

### 1. TypeAnalyzer.cs
**位置**: `Assets/XMFrame/Editor/UnityToolkit/TypeAnalyzer.cs`

**修改内容**:
- 移除了 `System.Type` -> `TypeI` 的类型映射逻辑（第 330-336 行）
- 移除了对 `TypeI` 命名空间的自动引用

**影响**:
- 现在配置类中的 `Type` 字段将不再自动转换为 `TypeI`
- 不会再为 `TypeI` 添加 `using` 语句

### 2. ClassHelperCodeGenerator.cs
**位置**: `Assets/XMFrame/Editor/UnityToolkit/ClassHelperCodeGenerator.cs`

**修改内容**:
- 移除了生成 `ConvertToTypeI` 调用的代码（第 291-295 行）
- 移除了对 `Type` 字段的特殊处理

**影响**:
- 生成的 `AllocContainerWithFillImpl` 方法中不再包含 `Type` -> `TypeI` 的转换代码
- `Type` 字段现在会被视为普通类型处理

### 3. TestConfig.cs (示例文件)
**位置**: `Assets/XMFrame/Editor/ConfigEditor/Sample/TestConfig.cs`

**修改内容**:
- 注释掉了 `public Type ConfigType;` 字段
- 注释掉了 `public Dictionary<Type, Dictionary<Type, List<NestedConfig>>> ConfigDict;` 字段

**影响**:
- 示例代码不再演示 `Type` 字段的使用
- 避免了在重新生成代码时产生编译错误

## 影响范围

### 不再支持的功能
1. **Type 字段直接映射**: 配置类中的 `System.Type` 字段不再自动转换为 `TypeI`
2. **Type 作为容器键**: 不再支持 `Dictionary<Type, ...>` 等以 `Type` 作为键的容器

### 仍然支持的功能
1. ✅ 所有基本类型（int, float, bool 等）
2. ✅ Unity.Mathematics 类型（int2, int3, float3, quaternion 等）
3. ✅ UnityEngine 类型（Vector2, Vector3, Color 等）
4. ✅ 字符串类型（StrI, FixedString 系列, LabelI）
5. ✅ 配置引用类型（CfgS, CfgI）
6. ✅ 容器类型（List, Dictionary, HashSet）
7. ✅ 嵌套配置
8. ✅ 自定义转换器

## 迁移指南

如果现有配置使用了 `Type` 字段，请考虑以下替代方案：

### 方案 1: 使用字符串存储类型名称
```csharp
// 之前
public Type ConfigType;

// 之后
[XmlStringMode(EXmlStrMode.EStrI)]
public string ConfigTypeName;

// 运行时可以通过反射获取类型
Type type = Type.GetType(config.ConfigTypeName);
```

### 方案 2: 使用枚举表示预定义类型
```csharp
public enum ConfigTypeEnum
{
    Type1,
    Type2,
    Type3
}

public ConfigTypeEnum ConfigType;
```

### 方案 3: 使用配置引用
```csharp
// 如果类型信息本身也是配置的一部分
public CfgS<TypeDefinitionConfig> ConfigType;
```

## 下一步

如果需要重新支持 `TypeI`，可以：
1. 实现 `ITypeConverter<string, TypeI>` 转换器
2. 使用 `[XmlGlobalConvert]` 或 `[XmlTypeConverter]` 特性标记字段
3. 在配置类中显式指定转换器

## 注意事项

⚠️ **已生成的代码可能需要重新生成**:
- 如果之前的配置类使用了 `Type` 字段，生成的代码可能包含 `TypeI` 引用
- 请删除或重新生成相关的 `*ClassHelper.Gen.cs` 和 `*UnManaged.Gen.cs` 文件
- 或者在配置类中注释掉 `Type` 字段后重新生成

⚠️ **XML 配置文件**:
- 如果 XML 中包含 `Type` 字段的数据，解析时可能会被忽略或报错
- 请根据新的字段类型更新 XML 配置

## 完成时间
2026-02-02
