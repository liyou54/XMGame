# IndexInitializationBuilder - 索引初始化代码生成器

## 功能概述

`IndexInitializationBuilder` 负责为配置管理器生成索引的初始化和填充逻辑。

## 生成的方法

### InitializeIndexes 方法

初始化所有索引容器并填充数据。这个方法将索引初始化和填充合并在一起。

**参数：**
- `configData`: 配置数据容器（包含 BlobContainer）
- `tableMap`: 表的主数据 Map (CfgI -> TUnmanaged)

**生成逻辑：**
1. **获取配置数量**：从 tableMap 获取配置数量
2. **申请索引容器**：在 configData.BlobContainer 中申请索引容器
   - 唯一索引：使用 `XBlobMap<IndexStruct, CfgI>`
   - 多值索引：使用 `XBlobMultiMap<IndexStruct, CfgI>`
   - **注意**：索引值存储的是 `CfgI`（配置ID），而不是指针
3. **遍历所有配置**：使用 for 循环遍历 tableMap 中的所有配置
4. **填充索引**：对每个配置
   - 通过 tableMap.GetKey() 获取配置ID
   - 通过 tableMap.GetRef() 获取配置数据引用
   - 从配置数据中提取索引字段值
   - 构建索引键（IndexStruct）
   - 将配置ID添加到对应的索引容器中
   - 唯一索引：如果键已存在，打印警告（使用 AddOrUpdate）
   - 多值索引：直接添加（使用 Add）

## 特殊处理

### CfgS 自动索引

如果索引字段只有一个且类型为 `CfgS<T>`，会自动标记为索引并生成注释说明。

**示例：**
```csharp
[XmlIndex("Index2", true, 0)] 
public CfgS<TestConfig> TestIndex3;
```

生成的代码会包含注释：
```csharp
// 索引字段 TestIndex3 为 CfgS 类型，自动作为索引
```

## 索引容器存储在 ConfigData 中

索引容器存储在 `ConfigData.BlobContainer` 中，作为方法的局部变量使用：

```csharp
var indexIndex1Map = configData.BlobContainer.AllocMap<IndexStruct, CfgI>(configCount);
var indexIndex2Map = configData.BlobContainer.AllocMultiMap<IndexStruct, CfgI>(configCount);
```

**关键设计决策：**
- 索引值存储 `CfgI`（配置ID）而不是 `XBlobPtr<TUnmanaged>`
- 通过 CfgI 可以从 tableMap 中查询到完整的配置数据
- 索引容器与配置数据共享同一个 BlobContainer，统一管理内存

**优点：**
- 减少类的状态管理
- 索引容器的生命周期更清晰
- 避免字段命名冲突
- 索引和配置数据统一存储在 ConfigData 中

## 使用示例

### 定义索引

```csharp
[XmlDefined()]
public class TestConfig : IXConfig<TestConfig, TestConfigUnmanaged>
{
    [XmlKey]
    public CfgS<TestConfig> Id;
    
    // 复合索引（非唯一）
    [XmlIndex("Index1", false, 0)] public int TestIndex1;
    [XmlIndex("Index1", false, 1)] public CfgS<TestConfig> TestIndex2;
    
    // 单字段索引（唯一）
    [XmlIndex("Index2", true, 0)] public CfgS<TestConfig> TestIndex3;
}
```

### 生成的代码

```csharp
/// <summary>
/// 初始化索引并填充数据
/// </summary>
/// <param name="configData">配置数据容器</param>
/// <param name="tableMap">表的主数据 Map (CfgI -> TUnmanaged)</param>
public void InitializeIndexes(
    ref XM.ConfigData configData,
    XBlobMap<CfgI, TestConfigUnmanaged> tableMap)
{
    // 获取配置数量
    int configCount = tableMap.GetLength(configData.BlobContainer);

    // 初始化索引: Index1
    // 申请 Map 容器，容量为配置数量
    var indexIndex1Map = configData.BlobContainer.AllocMap<TestConfigUnmanaged.Index1Index, CfgI>(configCount);

    // 初始化索引: Index2
    // 索引字段 TestIndex3 为 CfgS 类型，自动作为索引
    // 申请 MultiMap 容器，容量为配置数量
    var indexIndex2Map = configData.BlobContainer.AllocMultiMap<TestConfigUnmanaged.Index2Index, CfgI>(configCount);

    // 遍历所有配置，填充索引
    for (int i = 0; i < configCount; i++)
    {
        var cfgId = tableMap.GetKey(configData.BlobContainer, i);
        ref var data = ref tableMap.GetRef(configData.BlobContainer, cfgId, out bool exists);
        if (!exists) continue;

        // 填充索引: Index1
        var indexKeyIndex1 = new TestConfigUnmanaged.Index1Index(data.TestIndex1, data.TestIndex2);
        if (!indexIndex1Map.AddOrUpdate(configData.BlobContainer, indexKeyIndex1, cfgId))
        {
            UnityEngine.Debug.LogWarning($"索引 Index1 存在重复键: {indexKeyIndex1}");
        }

        // 填充索引: Index2
        var indexKeyIndex2 = new TestConfigUnmanaged.Index2Index(data.TestIndex3);
        indexIndex2Map.Add(configData.BlobContainer, indexKeyIndex2, cfgId);
    }
}
```

## 注意事项

1. **容量设置**：索引容器的容量设置为配置数量，确保不会发生容量溢出
2. **唯一性检查**：唯一索引使用 `AddOrUpdate`，如果键已存在会打印警告但不会抛出异常
3. **多值索引**：多值索引允许同一个键对应多个值，使用 `Add` 方法
4. **索引结构体**：每个索引都会生成对应的索引结构体（在单独的文件中）
5. **变量命名**：索引容器变量名为 `index{IndexName}Map`，索引键变量名为 `indexKey{IndexName}`，避免命名冲突
6. **方法合并**：索引初始化和填充逻辑合并在一个方法中，简化调用流程

## 集成到 XmlHelperGenerator

`IndexInitializationBuilder` 被集成到 `XmlHelperGenerator` 中：

1. 在构造函数中创建 `IndexInitializationBuilder` 实例
2. 在 `GenerateIndexMethods()` 中调用 `GenerateIndexInitializationMethod()` 生成索引初始化方法
3. 不再需要生成私有字段，索引容器作为局部变量

## 相关文件

- `IndexStructGenerator.cs`: 生成索引结构体
- `XmlHelperGenerator.cs`: 集成索引初始化逻辑
- `ConfigIndexMetadata.cs`: 索引元数据定义
