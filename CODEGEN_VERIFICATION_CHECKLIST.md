# 代码生成验证清单

重新生成 ClassHelper 代码后，请验证以下关键变化：

## 1. AllocContainerWithFillImpl 方法签名

### 预期变化
方法签名应该包含 `ref TUnmanaged data` 参数：

```csharp
protected override void AllocContainerWithFillImpl(
    IXConfig value,
    TblI tbli,
    CfgI cfgi,
    ref TestConfigUnManaged data,  // ✓ 添加了 ref data 参数
    System.Collections.Concurrent.ConcurrentDictionary<TblS, System.Collections.Concurrent.ConcurrentDictionary<CfgS, IXConfig>> allData,
    XM.ConfigDataCenter.ConfigDataHolder configHolderData)
```

### ❌ 旧版本（需要更新）
```csharp
protected override void AllocContainerWithFillImpl(
    IXConfig value,
    TblI tbli,
    CfgI cfgi,
    // 缺少 ref data 参数
    System.Collections.Concurrent.ConcurrentDictionary<TblS, System.Collections.Concurrent.ConcurrentDictionary<CfgS, IXConfig>> allData,
    XM.ConfigDataCenter.ConfigDataHolder configHolderData)
```

## 2. AllocContainerWithFillImpl 方法体

### 预期变化
- ❌ **移除**：从 map 读取 data 的代码
- ❌ **移除**：将 data 写回 map 的代码
- ✓ **直接使用**：通过 ref 参数传入的 data

```csharp
protected override void AllocContainerWithFillImpl(...)
{
    var config = (TestConfig)value;
    // ❌ 旧代码：不应该从 map 读取
    // var map = configHolderData.Data.GetMap<CfgI, TestConfigUnManaged>(_definedInMod);
    // if (!map.TryGetValue(..., out var data)) return;

    // ✓ 直接操作 ref data 参数
    AllocTestSample(config, ref data, cfgi, allData, configHolderData);
    AllocTestDictSample(config, ref data, cfgi, allData, configHolderData);
    // ...

    // 填充基本类型字段
    data.TestInt = config.TestInt;
    // ...

    // ❌ 旧代码：不应该写回 map
    // map[configHolderData.Data.BlobContainer, cfgi] = data;
}
```

## 3. 辅助方法签名（Alloc/Fill 方法）

### 预期变化
所有辅助方法应该包含 `allData` 参数：

```csharp
private void AllocTestSample(
    TestConfig config,
    ref TestConfigUnManaged data,
    CfgI cfgi,
    System.Collections.Concurrent.ConcurrentDictionary<TblS, System.Collections.Concurrent.ConcurrentDictionary<CfgS, IXConfig>> allData,  // ✓ 新增
    XM.ConfigDataCenter.ConfigDataHolder configHolderData)
```

### ❌ 旧版本
```csharp
private void AllocTestSample(
    TestConfig config,
    ref TestConfigUnManaged data,
    CfgI cfgi,
    // 缺少 allData 参数
    XM.ConfigDataCenter.ConfigDataHolder configHolderData)
```

## 4. 嵌套配置字段的 Fill 方法

### 预期变化
`FillTestNested` 方法应该递归调用 Helper 的 AllocContainerWithFillImpl：

```csharp
private void FillTestNested(
    TestConfig config,
    ref TestConfigUnManaged data,
    CfgI cfgi,
    System.Collections.Concurrent.ConcurrentDictionary<TblS, System.Collections.Concurrent.ConcurrentDictionary<CfgS, IXConfig>> allData,
    XM.ConfigDataCenter.ConfigDataHolder configHolderData)
{
    if (config.TestNested != null)
    {
        var nestedHelper = IConfigManager.I.GetClassHelper<NestedConfig>() as NestedConfigClassHelper;
        if (nestedHelper != null)
        {
            // ✓ 递归调用，使用 ref 传递嵌套字段
            nestedHelper.AllocContainerWithFillImpl(
                config.TestNested,
                _definedInMod,
                cfgi,
                ref data.TestNested,  // ✓ 使用 ref 传递
                allData,
                configHolderData);
        }
    }
}
```

### ❌ 旧版本
```csharp
private void FillTestNested(...)
{
    if (config.TestNested != null)
    {
        var nestedHelper = ...;
        if (nestedHelper != null)
        {
            // ❌ TODO 注释，未实现
            // TODO: 实现嵌套配置的递归填充
        }
    }
}
```

## 5. 容器中的嵌套配置（List<NestedConfig>）

### 预期变化
处理 `TestNestedConfig` 字段时，应该递归调用 Helper：

```csharp
private void AllocTestNestedConfig(
    TestConfig config,
    ref TestConfigUnManaged data,
    CfgI cfgi,
    System.Collections.Concurrent.ConcurrentDictionary<TblS, System.Collections.Concurrent.ConcurrentDictionary<CfgS, IXConfig>> allData,
    XM.ConfigDataCenter.ConfigDataHolder configHolderData)
{
    if (config.TestNestedConfig != null && config.TestNestedConfig.Count > 0)
    {
        var allocated = configHolderData.Data.BlobContainer.AllocArray<NestedConfigUnManaged>(config.TestNestedConfig.Count);
        data.TestNestedConfig = allocated;

        // ✓ 填充嵌套配置数据
        var nestedHelper0 = IConfigManager.I.GetClassHelper<NestedConfig>() as NestedConfigClassHelper;
        if (nestedHelper0 != null)
        {
            for (int i0 = 0; i0 < config.TestNestedConfig.Count; i0++)
            {
                if (config.TestNestedConfig[i0] != null)
                {
                    var nestedData0 = allocated[configHolderData.Data.BlobContainer, i0];
                    nestedHelper0.AllocContainerWithFillImpl(
                        config.TestNestedConfig[i0],
                        _definedInMod,
                        cfgi,
                        ref nestedData0,  // ✓ 使用 ref 传递
                        allData,
                        configHolderData);
                    allocated[configHolderData.Data.BlobContainer, i0] = nestedData0;
                }
            }
        }
    }
}
```

### ❌ 旧版本
```csharp
// ❌ TODO 注释，未实现递归填充
// TODO: 实现嵌套配置类型的递归填充
for (int i0 = 0; i0 < config.TestNestedConfig.Count; i0++)
{
    if (config.TestNestedConfig[i0] != null)
    {
        var nestedData0 = new NestedConfigUnManaged();
        // 这里应该递归填充 nestedData0 的所有字段
        allocated[configHolderData.Data.BlobContainer, i0] = nestedData0;
    }
}
```

## 验证步骤

1. ✅ 检查方法签名是否包含 `ref TUnmanaged data` 参数
2. ✅ 检查是否移除了从 map 读取和写回的代码
3. ✅ 检查辅助方法是否包含 `allData` 参数
4. ✅ 检查嵌套配置字段是否实现了递归调用
5. ✅ 检查容器中的嵌套配置是否实现了递归调用
6. ✅ 确保没有编译错误
7. ✅ 运行测试用例验证功能正常

## 如果遇到问题

如果生成的代码不符合预期，请检查：
1. 代码生成器是否已更新（ClassHelperCodeGenerator.cs）
2. 模板文件是否已更新（EmbeddedTemplates.cs）
3. 基类是否已更新（ConfigClassHelper.cs）
4. Unity 编辑器是否需要重启以重新加载代码

## 相关文件

- `Assets/XMFrame/Implementation/XConfigManager/ConfigClassHelper.cs` - 基类
- `Assets/XMFrame/Editor/UnityToolkit/ClassHelperCodeGenerator.cs` - 代码生成器
- `Assets/XMFrame/Editor/UnityToolkit/EmbeddedTemplates.cs` - 代码模板
- `Assets/XMFrame/Editor/ConfigEditor/Config/Code.Gen/*.Gen.cs` - 生成的代码
