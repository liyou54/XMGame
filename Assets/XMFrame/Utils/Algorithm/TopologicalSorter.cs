using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace XMFrame.Utils
{
    /// <summary>
    /// 拓扑排序工具类，支持依赖关系排序和成环检测
    /// </summary>
    public static class TopologicalSorter
{
    /// <summary>
    /// 拓扑排序结果
    /// </summary>
    public class SortResult<T>
    {
        /// <summary>
        /// 排序后的结果（如果成功）
        /// </summary>
        public IEnumerable<T> SortedItems { get; internal set; }

        /// <summary>
        /// 是否成功排序（无环）
        /// </summary>
        public bool IsSuccess { get; internal set; }

        /// <summary>
        /// 如果存在环，这里包含环中的节点
        /// </summary>
        public List<T> CycleNodes { get; internal set; }

        internal SortResult()
        {
            CycleNodes = new List<T>();
        }
    }

    /// <summary>
    /// 使用 GetDependence 进行拓扑排序
    /// GetDependence 返回当前节点依赖的所有节点（A依赖B，则A->B）
    /// </summary>
    /// <typeparam name="T">节点类型</typeparam>
    /// <param name="items">要排序的节点集合</param>
    /// <param name="getDependence">获取当前节点依赖的节点集合</param>
    /// <returns>排序结果</returns>
    public static SortResult<T> Sort<T>(
        IEnumerable<T> items,
        Func<T, IEnumerable<T>> getDependence)
        where T : notnull
    {
        return SortInternal(items, getDependence, null);
    }

    /// <summary>
    /// 使用 GetDepended 进行拓扑排序
    /// GetDepended 返回依赖当前节点的所有节点（A被B依赖，则B->A）
    /// </summary>
    /// <typeparam name="T">节点类型</typeparam>
    /// <param name="items">要排序的节点集合</param>
    /// <param name="getDepended">获取依赖当前节点的节点集合</param>
    /// <returns>排序结果</returns>
    public static SortResult<T> SortByDepended<T>(
        IEnumerable<T> items,
        Func<T, IEnumerable<T>> getDepended)
        where T : notnull
    {
        return SortInternal(items, null, getDepended);
    }

    /// <summary>
    /// 同时使用 GetDependence 和 GetDepended 进行拓扑排序
    /// </summary>
    /// <typeparam name="T">节点类型</typeparam>
    /// <param name="items">要排序的节点集合</param>
    /// <param name="getDependence">获取当前节点依赖的节点集合</param>
    /// <param name="getDepended">获取依赖当前节点的节点集合</param>
    /// <returns>排序结果</returns>
    public static SortResult<T> Sort<T>(
        IEnumerable<T> items,
        Func<T, IEnumerable<T>> getDependence,
        Func<T, IEnumerable<T>> getDepended)
        where T : notnull
    {
        return SortInternal(items, getDependence, getDepended);
    }

    /// <summary>
    /// 内部实现：拓扑排序核心算法（Kahn算法）
    /// </summary>
    private static SortResult<T> SortInternal<T>(
        IEnumerable<T> items,
        Func<T, IEnumerable<T>> getDependence,
        Func<T, IEnumerable<T>> getDepended)
        where T : notnull
    {
        var result = new SortResult<T>();
        var sortedList = new List<T>();
        var itemSet = new HashSet<T>(items);
        
        if (itemSet.Count == 0)
        {
            result.IsSuccess = true;
            result.SortedItems = sortedList;
            return result;
        }

        // 构建邻接表和入度表
        var adjacencyList = new Dictionary<T, HashSet<T>>();
        var inDegree = new Dictionary<T, int>();

        // 初始化
        foreach (var item in itemSet)
        {
            adjacencyList[item] = new HashSet<T>();
            inDegree[item] = 0;
        }

        // 构建依赖关系图
        foreach (var item in itemSet)
        {
            // 处理 GetDependence：item 依赖的节点
            if (getDependence != null)
            {
                var dependencies = getDependence(item);
                if (dependencies != null)
                {
                    foreach (var dep in dependencies)
                    {
                        if (itemSet.Contains(dep))
                        {
                            if (!adjacencyList[dep].Contains(item))
                            {
                                adjacencyList[dep].Add(item);
                                inDegree[item]++;
                            }
                        }
                    }
                }
            }

            // 处理 GetDepended：依赖 item 的节点
            if (getDepended != null)
            {
                var dependents = getDepended(item);
                if (dependents != null)
                {
                    foreach (var dep in dependents)
                    {
                        if (itemSet.Contains(dep))
                        {
                            if (!adjacencyList[item].Contains(dep))
                            {
                                adjacencyList[item].Add(dep);
                                inDegree[dep]++;
                            }
                        }
                    }
                }
            }
        }

        // Kahn算法：找到所有入度为0的节点
        var queue = new Queue<T>();
        foreach (var kvp in inDegree)
        {
            if (kvp.Value == 0)
            {
                queue.Enqueue(kvp.Key);
            }
        }

        // 拓扑排序
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            sortedList.Add(current);

            // 移除当前节点，更新依赖它的节点的入度
            foreach (var neighbor in adjacencyList[current])
            {
                inDegree[neighbor]--;
                if (inDegree[neighbor] == 0)
                {
                    queue.Enqueue(neighbor);
                }
            }
        }

        // 检查是否有环：如果排序后的节点数量小于总节点数量，说明存在环
        if (sortedList.Count < itemSet.Count)
        {
            result.IsSuccess = false;
            // 找出环中的节点（入度不为0的节点）
            foreach (var kvp in inDegree)
            {
                if (kvp.Value > 0)
                {
                    result.CycleNodes.Add(kvp.Key);
                }
            }
            result.SortedItems = sortedList; // 即使有环，也返回部分排序结果
        }
        else
        {
            result.IsSuccess = true;
            result.SortedItems = sortedList;
        }

        return result;
    }

    /// <summary>
    /// 直接返回排序后的迭代器（如果存在环则抛出异常）
    /// </summary>
    /// <typeparam name="T">节点类型</typeparam>
    /// <param name="items">要排序的节点集合</param>
    /// <param name="getDependence">获取当前节点依赖的节点集合</param>
    /// <returns>排序后的迭代器</returns>
    /// <exception cref="InvalidOperationException">如果存在环则抛出异常</exception>
    public static IEnumerable<T> SortOrThrow<T>(
        IEnumerable<T> items,
        Func<T, IEnumerable<T>> getDependence)
        where T : notnull
    {
        var result = Sort(items, getDependence);
        if (!result.IsSuccess)
        {
            throw new InvalidOperationException(
                $"拓扑排序失败：检测到循环依赖。环中的节点：{string.Join(", ", result.CycleNodes)}");
        }
        return result.SortedItems;
    }

    /// <summary>
    /// 直接返回排序后的迭代器（如果存在环则抛出异常）
    /// </summary>
    /// <typeparam name="T">节点类型</typeparam>
    /// <param name="items">要排序的节点集合</param>
    /// <param name="getDepended">获取依赖当前节点的节点集合</param>
    /// <returns>排序后的迭代器</returns>
    /// <exception cref="InvalidOperationException">如果存在环则抛出异常</exception>
    public static IEnumerable<T> SortByDependedOrThrow<T>(
        IEnumerable<T> items,
        Func<T, IEnumerable<T>> getDepended)
        where T : notnull
    {
        var result = SortByDepended(items, getDepended);
        if (!result.IsSuccess)
        {
            throw new InvalidOperationException(
                $"拓扑排序失败：检测到循环依赖。环中的节点：{string.Join(", ", result.CycleNodes)}");
        }
        return result.SortedItems;
    }

    /// <summary>
    /// 直接返回排序后的迭代器（如果存在环则抛出异常）
    /// </summary>
    /// <typeparam name="T">节点类型</typeparam>
    /// <param name="items">要排序的节点集合</param>
    /// <param name="getDependence">获取当前节点依赖的节点集合</param>
    /// <param name="getDepended">获取依赖当前节点的节点集合</param>
    /// <returns>排序后的迭代器</returns>
    /// <exception cref="InvalidOperationException">如果存在环则抛出异常</exception>
    public static IEnumerable<T> SortOrThrow<T>(
        IEnumerable<T> items,
        Func<T, IEnumerable<T>> getDependence,
        Func<T, IEnumerable<T>> getDepended)
        where T : notnull
    {
        var result = Sort(items, getDependence, getDepended);
        if (!result.IsSuccess)
        {
            throw new InvalidOperationException(
                $"拓扑排序失败：检测到循环依赖。环中的节点：{string.Join(", ", result.CycleNodes)}");
        }
        return result.SortedItems;
    }
    }
}
