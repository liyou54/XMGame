# ConfigUnmanaged ToString 方法生成说明

## 概述

为 ConfigUnmanaged 结构体自动生成 ToString 方法，用于调试和日志输出。

## 功能特性

1. **基本字段**: 直接输出字段值
2. **CfgI 字段**: 使用 CfgI 的 ToString 方法（显示 "CfgI(Id)"）
3. **XBlobPtr 指针字段**: 
   - 如果关联了 CfgI 字段（如 `Id` 和 `Id_Ref`），显示为 `Id_Ref=Ptr->Id`，表示指针指向 Id 的配置
   - 如果是独立的指针字段，显示为 `FieldName=Ptr(offset)`

## 实现原理

### 1. 字段元数据标记

在 `UnmanagedFieldDto` 中添加了以下字段：
- `IsCfgI`: 标记字段是否是 CfgI 类型
- `IsXBlobPtr`: 标记字段是否是 XBlobPtr 类型
- `AssociatedCfgIField`: 对于 XBlobPtr 字段，关联的 CfgI 字段名

### 2. 代码生成逻辑

在 `UnmanagedCodeGenerator.ToUnmanagedDto()` 中：
- 自动识别 CfgI 类型字段
- 为 XMLLink 字段生成的 XBlobPtr 标记关联的 CfgI 字段

### 3. 模板生成

在 `UnmanagedStruct.sbncs` 模板中添加了 ToString 方法生成：

```csharp
public override string ToString()
{
    var sb = new global::System.Text.StringBuilder();
    sb.Append("TypeName {");
    // 遍历字段
    sb.Append("}");
    return sb.ToString();
}
```

## 输出示例

对于 TestConfigUnManaged，生成的 ToString 输出可能如下：

```
TestConfigUnManaged {Id=CfgI(1), Id_Ref=Ptr->Id, TestInt=100, TestSample=..., Foreign=CfgI(5), Foreign_Ref=Ptr->Foreign}
```

## 使用方法

1. 在 Unity Editor 中运行配置代码生成工具
2. 所有 ConfigUnmanaged 结构体会自动生成 ToString 方法
3. 在代码中可以直接调用 `configInstance.ToString()` 进行调试

## 注意事项

- XBlobPtr 指针字段需要 XBlobContainer 才能解引用，ToString 只显示关联关系
- 对于复杂容器类型（如 XBlobArray、XBlobMap），直接使用默认的 ToString 输出
- CfgI 的详细配置名需要通过 IConfigManager 查询

## 相关文件

- `Assets/XMFrame/Editor/Toolkit/Config/ConfigCodeGenDto.cs`
- `Assets/XMFrame/Editor/Toolkit/Config/UnmanagedModelBuilder.cs`
- `Assets/XMFrame/Editor/UnityToolkit/UnmanagedCodeGenerator.cs`
- `Assets/XMFrame/Editor/UnityToolkit/EmbeddedTemplates.cs`
