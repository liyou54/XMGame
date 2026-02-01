using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using XM.Utils;

namespace XM.Editor.Tests.Fixtures
{
    /// <summary>
    /// 增强断言辅助类
    /// 职责：
    /// 1. 提供领域特定的断言方法
    /// 2. 封装复杂的验证逻辑
    /// 3. 提供更清晰的错误消息
    /// </summary>
    public class AssertHelpers : Assert
    {
        /// <summary>
        /// 断言拓扑排序无环
        /// </summary>
        public void AssertNoCycles<T>(TopologicalSorter.SortResult<T> result)
        {
            IsTrue(result.IsSuccess, "排序应该成功（无环）");
            IsEmpty(result.CycleNodes, "不应该有环节点");
            IsNotNull(result.SortedItems, "排序结果不应为null");
        }
        
        /// <summary>
        /// 断言拓扑排序检测到环
        /// </summary>
        public void AssertHasCycles<T>(TopologicalSorter.SortResult<T> result, int expectedCycleNodeCount)
        {
            IsFalse(result.IsSuccess, "排序应该失败（存在环）");
            IsNotEmpty(result.CycleNodes, "应该有环节点");
            AreEqual(expectedCycleNodeCount, result.CycleNodes.Count, 
                $"环节点数量应该是{expectedCycleNodeCount}");
        }
        
        /// <summary>
        /// 断言集合顺序（支持部分顺序验证）
        /// 用法：AssertOrder(actual, "A", "B", "C") - 验证A在B前，B在C前
        /// </summary>
        public void AssertOrder<T>(IEnumerable<T> actual, params T[] expected)
        {
            var actualList = actual.ToList();
            
            // 验证所有期望的元素都存在
            foreach (var item in expected)
            {
                IsTrue(actualList.Contains(item), $"结果应包含 {item}");
            }
            
            // 验证相对顺序
            for (int i = 0; i < expected.Length - 1; i++)
            {
                var idx1 = actualList.IndexOf(expected[i]);
                var idx2 = actualList.IndexOf(expected[i + 1]);
                Less(idx1, idx2, $"{expected[i]} 应该在 {expected[i + 1]} 之前");
            }
        }
        
        /// <summary>
        /// 断言双向字典一致性
        /// </summary>
        public void AssertBidirectionalConsistency<TKey, TValue>(
            BidirectionalDictionary<TKey, TValue> dict)
            where TKey : notnull
            where TValue : notnull
        {
            // 验证键→值→键一致性
            foreach (var key in dict.Keys)
            {
                var value = dict.GetByKey(key);
                var reversedKey = dict.GetByValue(value);
                AreEqual(key, reversedKey, "键→值→键应该保持一致");
            }
            
            // 验证值→键→值一致性
            foreach (var value in dict.Values)
            {
                var key = dict.GetByValue(value);
                var reversedValue = dict.GetByKey(key);
                AreEqual(value, reversedValue, "值→键→值应该保持一致");
            }
        }
        
        /// <summary>
        /// 断言覆盖率达标
        /// </summary>
        public void AssertCoverageTarget(
            double actualCoverage, 
            double targetCoverage, 
            string moduleName)
        {
            GreaterOrEqual(actualCoverage, targetCoverage, 
                $"{moduleName} 覆盖率应达到 {targetCoverage}%，实际 {actualCoverage}%");
        }
        
        /// <summary>
        /// 断言集合包含所有期望元素（忽略顺序）
        /// </summary>
        public void AssertContainsAll<T>(IEnumerable<T> actual, params T[] expected)
        {
            var actualList = actual.ToList();
            
            foreach (var item in expected)
            {
                IsTrue(actualList.Contains(item), 
                    $"集合应包含元素 {item}");
            }
        }
        
        /// <summary>
        /// 断言集合等价（相同元素，忽略顺序）
        /// </summary>
        public void AssertEquivalent<T>(IEnumerable<T> expected, IEnumerable<T> actual)
        {
            var expectedList = expected.ToList();
            var actualList = actual.ToList();
            
            AreEqual(expectedList.Count, actualList.Count, "集合元素数量应相等");
            
            foreach (var item in expectedList)
            {
                IsTrue(actualList.Contains(item), 
                    $"实际集合应包含期望元素 {item}");
            }
        }
        
        /// <summary>
        /// 断言字符串包含指定子串（忽略大小写）
        /// </summary>
        public void AssertContainsIgnoreCase(string actual, string expected)
        {
            IsTrue(actual?.ToLower().Contains(expected?.ToLower() ?? "") ?? false,
                $"字符串应包含（忽略大小写）'{expected}'，实际：'{actual}'");
        }
    }
}
