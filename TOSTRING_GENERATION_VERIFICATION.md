# ToString 生成功能验证指南

## 概述

本文档说明如何验证新实现的 UnManaged ToString 方法生成功能。

## 重新生成代码

### 方法 1: 使用 Unity Editor 菜单

1. 打开 Unity Editor
2. 在菜单栏中选择 `Tools > XMFrame > Generate Unmanaged Code`（具体菜单路径可能不同，需要查看 UnmanagedCodeGenWindow.cs）
3. 等待代码生成完成

### 方法 2: 通过代码调用

在 Unity Editor 中执行以下代码：

```csharp
UnityToolkit.UnmanagedCodeGenerator.GenerateAllUnmanagedCode();
```

## 验证生成的代码

### 1. 检查条件编译指令

打开生成的文件，例如：
- `Assets/XMFrame/Editor/ConfigEditor/Config/Code.Gen/TestConfigUnManaged.Gen.cs`

验证文件末尾是否包含：

```csharp
#if DEBUG || UNITY_EDITOR
public partial struct TestConfigUnManaged
{
    // ToString 方法应该在这里
}
#endif
```

### 2. 验证 ToString 方法

检查生成的 ToString 方法是否包含以下功能：

#### 2.1 无参数 ToString

```csharp
public override string ToString()
{
    return ToStringInternal(null);
}
```

#### 2.2 带容器参数的 ToString

```csharp
public string ToString(global::XBlobContainer container)
{
    return ToStringInternal(container);
}
```

### 3. 验证 CfgI 字段打印

对于 `CfgI<T>` 类型字段，应生成调用 `AppendCfgI` 的代码：

```csharp
sb.Append("Id=");
AppendCfgI(sb, Id);
```

验证是否存在 `AppendCfgI` 辅助方法：

```csharp
private static void AppendCfgI<T>(global::System.Text.StringBuilder sb, global::CfgI<T> cfgi)
    where T : unmanaged, global::XM.IConfigUnManaged<T>
{
    var dataCenter = global::XM.Contracts.IConfigDataCenter.I;
    if (dataCenter != null && dataCenter.TryGetCfgS(cfgi.AsNonGeneric(), out var cfgs))
    {
        sb.Append(cfgs.ToString()); // "模块::配置名"
    }
    else
    {
        sb.Append(cfgi.ToString()); // 回退到 "CfgI(Id)"
    }
}
```

### 4. 验证容器字段打印

#### 4.1 XBlobArray

对于 `XBlobArray<T>` 字段，应生成专用的辅助方法：

```csharp
private static void AppendArray_FieldName(global::System.Text.StringBuilder sb, global::XBlobContainer container, XBlobArray<T> arr)
{
    int len = arr.GetLength(container);
    sb.Append("[");
    // ... 打印逻辑
    sb.Append("]");
}
```

#### 4.2 XBlobMap

对于 `XBlobMap<K, V>` 字段，应生成专用的辅助方法：

```csharp
private static void AppendMap_FieldName(global::System.Text.StringBuilder sb, global::XBlobContainer container, XBlobMap<K, V> map)
{
    int len = map.GetLength(container);
    sb.Append("{");
    // ... 打印键值对
    sb.Append("}");
}
```

#### 4.3 XBlobSet

对于 `XBlobSet<T>` 字段，应生成专用的辅助方法：

```csharp
private static void AppendSet_FieldName(global::System.Text.StringBuilder sb, global::XBlobContainer container, XBlobSet<T> set)
{
    int len = set.GetLength(container);
    sb.Append("{");
    // ... 打印元素
    sb.Append("}");
}
```

### 5. 验证 XBlobPtr 字段打印

对于关联 CfgI 的 XBlobPtr 字段（如 `Id_Ref`），应该打印为：

```csharp
sb.Append("Id_Ref=Ptr->");
AppendCfgI(sb, Id);
```

对于独立的 XBlobPtr 字段，应该打印为：

```csharp
sb.Append("FieldName=Ptr(" + FieldName.Offset + ")");
```

## 运行时验证

### 1. 测试无容器参数的 ToString

在 Unity Editor 中编写测试代码：

```csharp
var config = new TestConfigUnManaged
{
    Id = new CfgI<TestConfigUnManaged>(1, new ModI(1), new TblI<TestConfigUnManaged>(1, new ModI(1))),
    TestInt = 42
};

Debug.Log(config.ToString());
// 预期输出类似：TestConfigUnManaged {Id=MyMod::TestConfig1, Id_Ref=Ptr->MyMod::TestConfig1, TestInt=42, ...}
```

### 2. 测试带容器参数的 ToString

```csharp
// 假设 container 已经初始化并包含数据
var output = config.ToString(container);
Debug.Log(output);
// 预期输出应包含容器的完整内容
```

### 3. 验证 CfgI 转 CfgS

验证输出中是否显示了配置名而不是 CfgI(Id)：

- **正确**：`Id=MyMod::TestConfig1`
- **错误**：`Id=CfgI(1)`

### 4. 验证容器内容打印

对于包含容器的配置，验证容器内容是否正确打印：

```csharp
// XBlobArray
TestSample=[1, 2, 3, 4, 5]

// XBlobMap
TestDictSample={1:10, 2:20, 3:30}

// XBlobSet
TestKeyHashSet={1, 2, 3, 4, 5}
```

## 常见问题

### 1. ToString 方法未生成

**原因**：可能是条件编译未满足或代码生成失败。

**解决**：
- 检查是否定义了 `DEBUG` 或 `UNITY_EDITOR` 宏
- 重新生成代码
- 检查控制台是否有错误信息

### 2. CfgI 显示为 CfgI(Id) 而不是配置名

**原因**：`IConfigDataCenter.TryGetCfgS` 方法未正确实现或配置未注册。

**解决**：
- 验证 `ConfigDataCenter.TryGetCfgS` 方法已正确实现
- 确保配置已经加载并注册到 ConfigDataCenter

### 3. 容器打印显示为 [?]

**原因**：调用了无参数的 `ToString()`，容器需要 `XBlobContainer` 才能访问数据。

**解决**：
- 使用 `ToString(container)` 方法并传入有效的容器

## 性能考虑

ToString 方法仅在 DEBUG 或 UNITY_EDITOR 模式下编译，不会影响发布版本的性能。

对于大型容器，ToString 方法会限制打印的元素数量：
- XBlobArray/XBlobSet：最多 10 个元素
- XBlobMap：最多 5 个键值对

超出部分会显示为 `...(N more)`。

## 总结

完成以上验证步骤后，确认：

- ✅ 所有生成的 UnManaged 文件都包含条件编译的 ToString 方法
- ✅ CfgI 字段能正确转换为 CfgS 显示
- ✅ 容器字段能正确打印内容（当提供 container 参数时）
- ✅ XBlobPtr 字段能正确显示关联信息
- ✅ ToString 方法仅在 DEBUG/UNITY_EDITOR 下编译

如有问题，请检查：
1. 模板文件是否正确更新
2. ConfigDataCenter 是否实现了 TryGetCfgS 方法
3. 代码生成过程是否有错误信息
