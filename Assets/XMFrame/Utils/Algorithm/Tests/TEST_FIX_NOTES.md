# TopologicalSorterTests 测试修复记录

## 🐛 修复的测试

**测试名称**: `Sort_BothGetters_CombinedDependencies`  
**文件**: `Assets/XMFrame/Utils/Algorithm/Tests/TopologicalSorterTests.cs`  
**行号**: 377-407

---

## 📋 问题描述

### 失败信息
```
Sort_BothGetters_CombinedDependencies (0.085s)
---
  A应该在C之前
  Expected: less than 1
  But was:  2
---
```

### 根本原因

测试中的断言逻辑错误。测试定义的依赖关系与预期结果不符。

---

## 🔍 依赖关系分析

### 原始测试设置

```csharp
var dependencies = new Dictionary<string, string[]>
{
    ["A"] = new[] { "B" }, // A依赖B
    ["B"] = new string[0],
    ["C"] = new string[0]
};

var depended = new Dictionary<string, string[]>
{
    ["C"] = new[] { "A" }, // C被A依赖
    ["A"] = new string[0],
    ["B"] = new string[0]
};
```

### 依赖关系解读

根据TopologicalSorter的API文档：

1. **getDependence**: 返回"当前节点依赖的节点"
   - `dependencies["A"] = ["B"]` → A依赖B → **B必须在A之前**

2. **getDepended**: 返回"依赖当前节点的节点"  
   - `depended["C"] = ["A"]` → C被A依赖 → A依赖C → **C必须在A之前**

### 合并后的依赖链

- B → A (B必须在A之前)
- C → A (C必须在A之前)

**正确的顺序**: `B, C, A` 或 `C, B, A`

---

## ❌ 错误的断言

```csharp
// 原始断言（错误）
Assert.Less(list.IndexOf("A"), list.IndexOf("C"), "A应该在C之前");
```

这个断言是错误的，因为：
- A依赖C（从`depended["C"] = ["A"]`推导）
- 所以C应该在A之前，不是A在C之前

---

## ✅ 修复后的代码

```csharp
/// <summary>
/// Given: 同时使用GetDependence和GetDepended
/// When: 调用Sort(items, getDependence, getDepended)
/// Then: 合并两个依赖关系
/// 依赖关系：A依赖B, A依赖C -> 顺序应为 B,C,A 或 C,B,A
/// </summary>
[Test]
public void Sort_BothGetters_CombinedDependencies()
{
    // Arrange
    var items = new[] { "A", "B", "C" };
    var dependencies = new Dictionary<string, string[]>
    {
        ["A"] = new[] { "B" }, // A依赖B -> B必须在A之前
        ["B"] = new string[0],
        ["C"] = new string[0]
    };
    var depended = new Dictionary<string, string[]>
    {
        ["C"] = new[] { "A" }, // C被A依赖 -> A依赖C -> C必须在A之前
        ["A"] = new string[0],
        ["B"] = new string[0]
    };
    
    // Act
    var result = TopologicalSorter.Sort(
        items, 
        x => dependencies[x], 
        x => depended[x]);
    
    // Assert
    Assert.IsTrue(result.IsSuccess);
    Assert.IsEmpty(result.CycleNodes);
    var list = result.SortedItems.ToList();
    // B应该在A之前（因为A依赖B）
    Assert.Less(list.IndexOf("B"), list.IndexOf("A"), "B应该在A之前");
    // C应该在A之前（因为A依赖C，从depended推导出来）✅
    Assert.Less(list.IndexOf("C"), list.IndexOf("A"), "C应该在A之前");
}
```

---

## 📝 变更总结

### 修改的部分

1. **注释更新** - 添加了详细的依赖关系说明
2. **断言修复** - 将`Assert.Less(list.IndexOf("A"), list.IndexOf("C"), ...)`改为`Assert.Less(list.IndexOf("C"), list.IndexOf("A"), ...)`
3. **断言消息** - 更新为"C应该在A之前"

### 测试验证

修复后，测试应该通过，验证：
- ✅ B在A之前
- ✅ C在A之前
- ✅ A在最后

---

## 🎓 经验教训

### getDepended的理解

`getDepended(X)` 返回"依赖X的节点集合"：
- 如果 `getDepended(C) = [A]`
- 意思是：A依赖C
- 推导：C → A（C必须在A之前）

**记忆技巧**：
- `getDependence(A) = [B]` → "A需要B" → B在前
- `getDepended(C) = [A]` → "A需要C" → C在前

---

## ✅ 修复状态

- **修复时间**: 2026-02-01
- **测试状态**: ✅ 应该通过
- **编译状态**: ✅ 无错误
- **影响范围**: 单个测试方法

---

*此修复确保测试正确验证TopologicalSorter在混合使用getDependence和getDepended时的行为。*
