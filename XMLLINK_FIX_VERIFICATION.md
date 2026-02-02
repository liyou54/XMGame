# XMLLink 自引用类型修复验证报告

## 修复概述

修复了代码生成器中 `[XMLLink]` 字段的 `Link` 字段类型错误，使其正确生成为自引用类型而非链接目标类型。

## 修复的文件

### 1. UnmanagedCodeGenerator.cs（第 137 行）

**修复前：**
```csharp
dto.Fields.Add(new UnmanagedFieldDto { 
    Name = field.Name, 
    UnmanagedType = $"CfgI<{field.XmlLinkDstUnmanagedType}>",  // ❌ 错误：使用链接目标类型
    ...
});
```

**修复后：**
```csharp
dto.Fields.Add(new UnmanagedFieldDto { 
    Name = field.Name, 
    UnmanagedType = $"CfgI<{currentUnmanaged}>",  // ✅ 正确：使用当前配置类型（自引用）
    ...
});
```

### 2. ClassHelperCodeGenerator.cs（第 236-241 行）

**修复前：**
```csharp
sb.AppendLine($"        if (IConfigDataCenter.I.TryGetCfgI({managedVar}.{fieldName}.AsNonGeneric(), out var cfgI_{fieldName}))");
sb.AppendLine($"        {{");
sb.AppendLine($"            {unmanagedVar}.{fieldName}_Dst = cfgI_{fieldName}.As<{dstType}>();");
sb.AppendLine($"            {unmanagedVar}.{fieldName} = cfgI_{fieldName}.As<{dstType}>();");  // ❌ 错误
sb.AppendLine($"        }}");
```

**修复后：**
```csharp
sb.AppendLine($"        if (IConfigDataCenter.I.TryGetCfgI({managedVar}.{fieldName}.AsNonGeneric(), out var cfgI_{fieldName}))");
sb.AppendLine($"        {{");
sb.AppendLine($"            {unmanagedVar}.{fieldName}_Dst = cfgI_{fieldName}.As<{dstType}>();");
sb.AppendLine($"        }}");
sb.AppendLine($"        {unmanagedVar}.{fieldName} = cfgi.As<{unmanagedTypeName}>();");  // ✅ 正确：使用 cfgi 参数
```

## 生成代码验证

### TestInhertUnmanaged.Gen.cs

**修复前（错误）：**
```csharp
public partial struct TestInhertUnmanaged
{
    public CfgI<TestConfigUnManaged> Link_Dst;
    public XBlobPtr<TestConfigUnManaged> Link_Ref;
    public CfgI<TestConfigUnManaged> Link;  // ❌ 错误：应该是 TestInhertUnmanaged
    public Int32 xxxx;
}
```

**修复后（正确）：**
```csharp
public partial struct TestInhertUnmanaged
{
    public CfgI<TestConfigUnManaged> Link_Dst;   // ✅ 指向链接目标的索引
    public XBlobPtr<TestConfigUnManaged> Link_Ref;  // ✅ 指向链接目标的数据指针
    public CfgI<TestInhertUnmanaged> Link;         // ✅ 自引用：指向当前配置项的索引
    public Int32 xxxx;
}
```

### TestInhertClassHelper.Gen.cs

**修复前（错误）：**
```csharp
if (IConfigDataCenter.I.TryGetCfgI(config.Link.AsNonGeneric(), out var cfgI_Link))
{
    data.Link_Dst = cfgI_Link.As<TestConfigUnManaged>();
    data.Link = cfgI_Link.As<TestConfigUnManaged>();  // ❌ 错误：指向链接目标
}
```

**修复后（正确）：**
```csharp
if (IConfigDataCenter.I.TryGetCfgI(config.Link.AsNonGeneric(), out var cfgI_Link))
{
    data.Link_Dst = cfgI_Link.As<TestConfigUnManaged>();
}
data.Link = cfgi.As<TestInhertUnmanaged>();  // ✅ 正确：指向当前配置项
```

## XMLLink 字段语义

对于 `[XMLLink] public CfgS<TestConfig> Link;` 字段，生成的三个 Unmanaged 字段语义如下：

| 字段名 | 类型 | 语义 | 用途 |
|--------|------|------|------|
| `Link_Dst` | `CfgI<TestConfigUnManaged>` | 链接目标的索引 | 查找链接到的 TestConfig 配置项 |
| `Link_Ref` | `XBlobPtr<TestConfigUnManaged>` | 链接目标的数据指针 | 直接访问链接到的 TestConfig 数据 |
| `Link` | `CfgI<TestInhertUnmanaged>` | **当前配置项的索引（自引用）** | 表示当前 TestInhert 配置项本身 |

## 为什么需要自引用？

`Link` 字段表示当前配置项自身的索引，这在以下场景中很有用：
- **组合配置模式**：TestInhert 通过 XMLLink 组合 TestConfig 的数据，但仍需保留自身标识
- **配置追溯**：在数据处理流程中，可以通过 Link 字段追溯到原始配置项
- **类型系统完整性**：保持配置项的类型信息，避免类型混淆

## 后续操作

1. ✅ 修改代码生成器源文件
2. ✅ 更新已生成的测试代码
3. ⏳ 在 Unity 编辑器中重新生成所有配置代码（手动操作）
4. ⏳ 运行单元测试验证修复效果

## 手动重新生成步骤

1. 打开 Unity 编辑器
2. 选择菜单：`XMFrame/Config/Generate Code (Select Assemblies)`
3. 勾选包含配置类的程序集（如 Assembly-CSharp）
4. 点击"生成代码"按钮
5. 等待代码生成完成
6. 验证生成的 `*Unmanaged.Gen.cs` 和 `*ClassHelper.Gen.cs` 文件

---

**修复完成时间**：2026-02-02  
**影响范围**：所有使用 `[XMLLink]` 特性的配置类
