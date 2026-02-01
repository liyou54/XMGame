using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace XM.Utils.Tests
{
    /// <summary>
    /// TopologicalSorter 拓扑排序测试
    /// 目标：98%+ 分支覆盖率
    /// </summary>
    [TestFixture]
    [Category("Pure")]
    public class TopologicalSorterTests
    {
        #region 基础场景测试
        
        /// <summary>
        /// Given: 空集合
        /// When: 调用Sort
        /// Then: 返回空结果，IsSuccess=true
        /// </summary>
        [Test]
        public void Sort_EmptyCollection_ReturnsEmptySuccess()
        {
            // Arrange
            var items = new string[0];
            
            // Act
            var result = TopologicalSorter.Sort(items, x => new string[0]);
            
            // Assert
            Assert.IsTrue(result.IsSuccess, "排序应该成功（无环）");
            Assert.IsEmpty(result.CycleNodes, "不应该有环节点");
            Assert.IsEmpty(result.SortedItems);
        }
        
        /// <summary>
        /// Given: 单个节点，无依赖
        /// When: 调用Sort
        /// Then: 返回该节点
        /// </summary>
        [Test]
        public void Sort_SingleNode_ReturnsNode()
        {
            // Arrange
            var items = new[] { "A" };
            
            // Act
            var result = TopologicalSorter.Sort(items, x => new string[0]);
            
            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsEmpty(result.CycleNodes);
            CollectionAssert.AreEquivalent(new[] { "A" }, result.SortedItems.ToList());
        }
        
        /// <summary>
        /// Given: 两个节点 A→B (A依赖B)
        /// When: 调用Sort
        /// Then: B在A之前
        /// </summary>
        [Test]
        public void Sort_TwoNodesSimpleChain_CorrectOrder()
        {
            // Arrange
            var items = new[] { "A", "B" };
            var dependencies = new Dictionary<string, string[]>
            {
                ["A"] = new[] { "B" },
                ["B"] = new string[0]
            };
            
            // Act
            var result = TopologicalSorter.Sort(items, x => dependencies[x]);
            
            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsEmpty(result.CycleNodes);
            var list = result.SortedItems.ToList();
            Assert.Less(list.IndexOf("B"), list.IndexOf("A"), "B应该在A之前");
        }
        
        #endregion
        
        #region 依赖关系测试
        
        /// <summary>
        /// Given: 线性链 A→B→C→D
        /// When: 调用Sort
        /// Then: D,C,B,A 顺序
        /// </summary>
        [Test]
        public void Sort_LinearChain_ReturnsCorrectOrder()
        {
            // Arrange
            var items = new[] { "A", "B", "C", "D" };
            var dependencies = new Dictionary<string, string[]>
            {
                ["A"] = new[] { "B" },
                ["B"] = new[] { "C" },
                ["C"] = new[] { "D" },
                ["D"] = new string[0]
            };
            
            // Act
            var result = TopologicalSorter.Sort(items, x => dependencies[x]);
            
            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsEmpty(result.CycleNodes);
            var list = result.SortedItems.ToList();
            Assert.Less(list.IndexOf("D"), list.IndexOf("C"));
            Assert.Less(list.IndexOf("C"), list.IndexOf("B"));
            Assert.Less(list.IndexOf("B"), list.IndexOf("A"));
        }
        
        /// <summary>
        /// Given: 多分支依赖 A→B, A→C (B和C无依赖关系)
        /// When: 调用Sort
        /// Then: A在B和C之后，B和C顺序任意
        /// </summary>
        [Test]
        public void Sort_MultipleBranches_ValidOrder()
        {
            // Arrange
            var items = new[] { "A", "B", "C" };
            var dependencies = new Dictionary<string, string[]>
            {
                ["A"] = new[] { "B", "C" },
                ["B"] = new string[0],
                ["C"] = new string[0]
            };
            
            // Act
            var result = TopologicalSorter.Sort(items, x => dependencies[x]);
            
            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsEmpty(result.CycleNodes);
            var list = result.SortedItems.ToList();
            
            // A应该在最后
            Assert.AreEqual("A", list.Last());
            
            // B和C应该在A之前（顺序任意）
            Assert.Less(list.IndexOf("B"), list.IndexOf("A"));
            Assert.Less(list.IndexOf("C"), list.IndexOf("A"));
        }
        
        /// <summary>
        /// Given: 菱形依赖 A→B, A→C, B→D, C→D
        /// When: 调用Sort
        /// Then: D在最前，A在最后
        /// </summary>
        [Test]
        public void Sort_DiamondDependency_HandlesCorrectly()
        {
            // Arrange
            var items = new[] { "A", "B", "C", "D" };
            var dependencies = new Dictionary<string, string[]>
            {
                ["A"] = new[] { "B", "C" },
                ["B"] = new[] { "D" },
                ["C"] = new[] { "D" },
                ["D"] = new string[0]
            };
            
            // Act
            var result = TopologicalSorter.Sort(items, x => dependencies[x]);
            
            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsEmpty(result.CycleNodes);
            var list = result.SortedItems.ToList();
            Assert.Less(list.IndexOf("D"), list.IndexOf("A"), "D应该在A之前");
        }
        
        #endregion
        
        #region 循环检测测试
        
        /// <summary>
        /// Given: 简单环 A→B→A
        /// When: 调用Sort
        /// Then: IsSuccess=false，CycleNodes包含A和B
        /// </summary>
        [Test]
        [Category("EdgeCase")]
        public void Sort_SimpleCycle_DetectsAndReturnsFailure()
        {
            // Arrange
            var items = new[] { "A", "B" };
            var dependencies = new Dictionary<string, string[]>
            {
                ["A"] = new[] { "B" },
                ["B"] = new[] { "A" }
            };
            
            // Act
            var result = TopologicalSorter.Sort(items, x => dependencies[x]);
            
            // Assert
            Assert.IsFalse(result.IsSuccess, "排序应该失败（存在环）");
            Assert.AreEqual(2, result.CycleNodes.Count);
            Assert.Contains("A", result.CycleNodes.ToList());
            Assert.Contains("B", result.CycleNodes.ToList());
        }
        
        /// <summary>
        /// Given: 复杂环 A→B→C→A
        /// When: 调用Sort
        /// Then: IsSuccess=false，CycleNodes包含A、B、C
        /// </summary>
        [Test]
        [Category("EdgeCase")]
        public void Sort_ComplexCycle_DetectsAllCycleNodes()
        {
            // Arrange
            var items = new[] { "A", "B", "C" };
            var dependencies = new Dictionary<string, string[]>
            {
                ["A"] = new[] { "B" },
                ["B"] = new[] { "C" },
                ["C"] = new[] { "A" }
            };
            
            // Act
            var result = TopologicalSorter.Sort(items, x => dependencies[x]);
            
            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(3, result.CycleNodes.Count);
            Assert.Contains("A", result.CycleNodes.ToList());
            Assert.Contains("B", result.CycleNodes.ToList());
            Assert.Contains("C", result.CycleNodes.ToList());
        }
        
        /// <summary>
        /// Given: 部分环 A→B→C(成功), D→E→D(环)
        /// When: 调用Sort
        /// Then: IsSuccess=false，CycleNodes只包含D和E
        /// </summary>
        [Test]
        [Category("EdgeCase")]
        public void Sort_PartialCycle_DetectsOnlyCycleNodes()
        {
            // Arrange
            var items = new[] { "A", "B", "C", "D", "E" };
            var dependencies = new Dictionary<string, string[]>
            {
                ["A"] = new[] { "B" },
                ["B"] = new[] { "C" },
                ["C"] = new string[0],
                ["D"] = new[] { "E" },
                ["E"] = new[] { "D" }
            };
            
            // Act
            var result = TopologicalSorter.Sort(items, x => dependencies[x]);
            
            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(2, result.CycleNodes.Count);
            Assert.Contains("D", result.CycleNodes.ToList());
            Assert.Contains("E", result.CycleNodes.ToList());
        }
        
        #endregion
        
        #region 依赖关系为空/Null测试
        
        /// <summary>
        /// Given: GetDependence返回null
        /// When: 调用Sort
        /// Then: 视为无依赖，正常排序
        /// </summary>
        [Test]
        public void Sort_GetDependenceReturnsNull_TreatAsNoDependency()
        {
            // Arrange
            var items = new[] { "A", "B" };
            
            // Act
            var result = TopologicalSorter.Sort(items, x => null);
            
            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsEmpty(result.CycleNodes);
            Assert.AreEqual(2, result.SortedItems.Count());
        }
        
        /// <summary>
        /// Given: GetDependence返回空数组
        /// When: 调用Sort
        /// Then: 视为无依赖，正常排序
        /// </summary>
        [Test]
        public void Sort_GetDependenceReturnsEmpty_TreatAsNoDependency()
        {
            // Arrange
            var items = new[] { "A", "B", "C" };
            
            // Act
            var result = TopologicalSorter.Sort(items, x => new string[0]);
            
            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsEmpty(result.CycleNodes);
            Assert.AreEqual(3, result.SortedItems.Count());
        }
        
        #endregion
        
        #region SortByDepended 模式测试
        
        /// <summary>
        /// Given: B被A依赖 (使用SortByDepended)
        /// When: 调用SortByDepended
        /// Then: 正确排序
        /// </summary>
        [Test]
        public void SortByDepended_ReverseDependency_CorrectOrder()
        {
            // Arrange
            var items = new[] { "A", "B" };
            var depended = new Dictionary<string, string[]>
            {
                ["B"] = new[] { "A" }, // B被A依赖
                ["A"] = new string[0]
            };
            
            // Act
            var result = TopologicalSorter.SortByDepended(items, x => depended[x]);
            
            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsEmpty(result.CycleNodes);
            var list = result.SortedItems.ToList();
            Assert.Less(list.IndexOf("B"), list.IndexOf("A"), "B应该在A之前");
        }
        
        /// <summary>
        /// Given: 使用SortByDepended，存在环
        /// When: 调用SortByDepended
        /// Then: 正确检测环
        /// </summary>
        [Test]
        [Category("EdgeCase")]
        public void SortByDepended_Cycle_DetectsCorrectly()
        {
            // Arrange
            var items = new[] { "A", "B" };
            var depended = new Dictionary<string, string[]>
            {
                ["A"] = new[] { "B" },
                ["B"] = new[] { "A" }
            };
            
            // Act
            var result = TopologicalSorter.SortByDepended(items, x => depended[x]);
            
            // Assert
            Assert.IsFalse(result.IsSuccess);
            Assert.AreEqual(2, result.CycleNodes.Count);
        }
        
        #endregion
        
        #region 混合模式测试
        
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
            // C应该在A之前（因为A依赖C，从depended推导出来）
            Assert.Less(list.IndexOf("C"), list.IndexOf("A"), "C应该在A之前");
        }
        
        #endregion
        
        #region 性能测试
        
        /// <summary>
        /// Given: 100个节点的大图
        /// When: 调用Sort
        /// Then: 在合理时间内完成（5秒）
        /// </summary>
        [Test]
        [Category("Performance")]
        [Timeout(5000)]
        public void Sort_LargeGraph100Nodes_CompletesInReasonableTime()
        {
            // Arrange
            var items = Enumerable.Range(0, 100).Select(i => $"Node{i}").ToArray();
            var dependencies = new Dictionary<string, string[]>();
            
            // 创建线性依赖链
            for (int i = 0; i < 100; i++)
            {
                if (i < 99)
                    dependencies[$"Node{i}"] = new[] { $"Node{i + 1}" };
                else
                    dependencies[$"Node{i}"] = new string[0];
            }
            
            // Act
            var result = TopologicalSorter.Sort(items, x => dependencies[x]);
            
            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.IsEmpty(result.CycleNodes);
            Assert.AreEqual(100, result.SortedItems.Count());
        }
        
        #endregion
    }
}
