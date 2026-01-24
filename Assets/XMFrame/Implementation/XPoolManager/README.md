# XMFrame 对象池管理器 (IPoolManager)

## 概述

IPoolManager 是一个高性能的对象池管理系统，用于管理和复用 C# 对象，减少内存分配和 GC 压力。

**重要提示：此对象池不存储 UnityEngine.Object 类型，仅用于普通 C# 对象。**

## 核心特性

- ✅ **接口与实现分离**：遵循项目架构规范
- ✅ **泛型支持**：支持任意 C# 类型
- ✅ **自动管理**：通过 `[AutoCreate]` 属性自动创建
- ✅ **生命周期回调**：支持创建、销毁、获取、释放回调
- ✅ **容量控制**：支持初始容量和最大容量限制
- ✅ **线程安全**：使用 HashSet 跟踪激活对象，防止重复释放
- ✅ **完善的日志**：关键操作都有日志记录

## 架构设计

### 接口层 (Interfaces)
```
IPoolManager           - 对象池管理器接口
  ├─ GetOrCreatePool<T>()                  - 获取或创建默认对象池
  ├─ GetOrCreatePool<T>(name, config)      - 获取或创建自定义对象池
  ├─ GetPool<T>(name)                      - 获取对象池
  ├─ HasPool<T>(name)                      - 检查对象池是否存在
  ├─ DestroyPool<T>(name)                  - 销毁指定对象池
  └─ DestroyAllPools()                     - 销毁所有对象池

IPool<T>               - 对象池接口
  ├─ Get()                                 - 从池中获取对象
  ├─ Release(item)                         - 释放对象回池
  ├─ Clear()                               - 清空池
  ├─ Count                                 - 池中可用对象数
  └─ ActiveCount                           - 激活对象数

PoolConfig<T>          - 对象池配置类
  ├─ OnCreate                              - 创建对象回调
  ├─ OnDestroy                             - 销毁对象回调
  ├─ OnGet                                 - 获取对象回调
  ├─ OnRelease                             - 释放对象回调
  ├─ InitialCapacity                       - 初始容量
  └─ MaxCapacity                           - 最大容量
```

### 实现层 (Implementation)
```
PoolManager            - 对象池管理器实现
  └─ 管理多个不同类型的对象池

Pool<T>                - 具体对象池实现
  ├─ Stack<T>          - 存储空闲对象
  └─ HashSet<T>        - 跟踪激活对象
```

## 使用方式

### 1. 基础使用（默认对象池）

```csharp
// 获取默认对象池（自动使用 new() 创建对象）
var pool = IPoolManager.I.GetOrCreatePool<MyClass>();

// 从池中获取对象
var obj = pool.Get();
obj.DoSomething();

// 使用完毕后释放回池
pool.Release(obj);
```

### 2. 自定义配置

```csharp
var config = new PoolConfig<MyClass>
{
    OnCreate = () => new MyClass(),
    OnDestroy = (obj) => obj.Cleanup(),
    OnGet = (obj) => obj.Reset(),
    OnRelease = (obj) => obj.Clear(),
    InitialCapacity = 10,  // 预创建 10 个对象
    MaxCapacity = 100      // 最多保留 100 个对象
};

var pool = IPoolManager.I.GetOrCreatePool("MyCustomPool", config);
```

### 3. 带容量限制的对象池

```csharp
var config = new PoolConfig<List<int>>
{
    OnCreate = () => new List<int>(),
    OnRelease = (list) => list.Clear(),
    InitialCapacity = 5,   // 预热：提前创建 5 个对象
    MaxCapacity = 20       // 限制：最多保留 20 个空闲对象
};

var pool = IPoolManager.I.GetOrCreatePool("ListPool", config);
```

### 4. 使用回调管理对象状态

```csharp
var config = new PoolConfig<PlayerData>
{
    OnCreate = () =>
    {
        Debug.Log("创建新的 PlayerData");
        return new PlayerData();
    },
    OnGet = (obj) =>
    {
        obj.IsActive = true;
        Debug.Log("对象被获取");
    },
    OnRelease = (obj) =>
    {
        obj.Reset();
        obj.IsActive = false;
        Debug.Log("对象被释放");
    },
    OnDestroy = (obj) =>
    {
        obj.Dispose();
        Debug.Log("对象被销毁");
    }
};

var pool = IPoolManager.I.GetOrCreatePool("PlayerPool", config);
```

## 工作流程

### 获取对象流程
```
Get() 调用
  ↓
池中有空闲对象？
  ├─ 是 → 从 Stack 弹出对象
  └─ 否 → 调用 OnCreate 创建新对象
  ↓
添加到 ActiveObjects (HashSet)
  ↓
调用 OnGet 回调
  ↓
返回对象
```

### 释放对象流程
```
Release(obj) 调用
  ↓
对象属于此池？(检查 ActiveObjects)
  ├─ 否 → 警告并返回
  └─ 是 → 继续
  ↓
从 ActiveObjects 移除
  ↓
调用 OnRelease 回调
  ↓
是否超过最大容量？
  ├─ 是 → 调用 OnDestroy 销毁
  └─ 否 → 压入 Stack
```

## 最佳实践

### ✅ 推荐做法

1. **为频繁创建销毁的对象使用对象池**
```csharp
// 游戏中频繁创建的临时数据
var pool = IPoolManager.I.GetOrCreatePool<BulletData>();
```

2. **使用 OnRelease 重置对象状态**
```csharp
OnRelease = (obj) =>
{
    obj.Reset();        // 重置所有字段
    obj.ClearCache();   // 清理缓存
}
```

3. **设置合理的容量限制**
```csharp
InitialCapacity = 10,   // 根据常用数量设置
MaxCapacity = 100       // 防止内存无限增长
```

4. **使用 using 语句自动释放**
```csharp
var pool = IPoolManager.I.GetOrCreatePool<MyClass>();
var obj = pool.Get();
// 使用对象...
pool.Release(obj);
```

### ❌ 避免的做法

1. **不要对 UnityEngine.Object 使用此对象池**
```csharp
// ❌ 错误：此对象池不支持 Unity 对象
var pool = IPoolManager.I.GetOrCreatePool<GameObject>();
```

2. **不要忘记释放对象**
```csharp
// ❌ 错误：对象永远不会被回收
var obj = pool.Get();
// 使用完后没有调用 pool.Release(obj)
```

3. **不要重复释放同一对象**
```csharp
// ❌ 错误：会触发警告
pool.Release(obj);
pool.Release(obj);  // 第二次释放会被阻止
```

4. **不要在释放后继续使用对象**
```csharp
// ❌ 错误：对象可能被其他地方获取并修改
pool.Release(obj);
obj.DoSomething();  // 危险！
```

## 性能优化建议

1. **预热对象池**
```csharp
InitialCapacity = 20  // 游戏启动时预创建，避免运行时卡顿
```

2. **设置最大容量**
```csharp
MaxCapacity = 100  // 防止内存无限增长
```

3. **复用容器对象**
```csharp
// List、Dictionary 等容器特别适合使用对象池
var listPool = IPoolManager.I.GetOrCreatePool<List<int>>();
```

4. **监控对象池状态**
```csharp
Debug.Log($"空闲: {pool.Count}, 激活: {pool.ActiveCount}");
```

## 示例场景

详细的使用示例请查看：`PoolManagerExample.cs`

包含以下示例：
- 示例 1：默认对象池基础使用
- 示例 2：自定义配置的对象池
- 示例 3：带容量限制的对象池
- 示例 4：使用完整回调的对象池

## 与其他 Manager 的集成

IPoolManager 遵循 XMFrame 的 Manager 架构规范：

- 继承自 `ManagerBase<IPoolManager>`
- 使用 `[AutoCreate]` 自动创建
- 实现 `IManager<IPoolManager>` 接口
- 支持生命周期回调（OnCreate, OnInit, OnDestroy 等）
- 通过 `IPoolManager.I` 全局访问

## 注意事项

1. **线程安全**：当前实现不是线程安全的，仅在主线程使用
2. **类型限制**：不支持 UnityEngine.Object 及其派生类
3. **内存管理**：记得在不需要时调用 `DestroyPool` 释放资源
4. **命名规范**：池名称应具有描述性，便于调试

## 版本历史

- v1.0 - 初始实现
  - 接口与实现分离
  - 支持泛型对象池
  - 支持生命周期回调
  - 支持容量控制
