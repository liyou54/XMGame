using System;
using System.Diagnostics;
using NUnit.Framework;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
#if UNITY_BURST
using Unity.Burst;
#endif

namespace XM.Utils.Tests
{
    /// <summary>
    /// XBlob 容器性能测试
    /// 专注于性能测试，不包含功能验证
    /// 测试基准：100万数据
    /// </summary>
    [TestFixture]
    public class XBlobPerformanceTests
    {
        private const int ElementCount = 1000000; // 100万数据基准
        private const int ContainerSize = 15000000; // 15MB空间

        [Test]
        public void Array_1M_Insert_Performance()
        {
            var container = new XBlobContainer();
            container.Create(Allocator.Temp, ContainerSize);
            
            var array = container.AllocArray<int>(ElementCount);
            
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < ElementCount; i++)
            {
                array[container, i] = i * 10;
            }
            sw.Stop();
            
            UnityEngine.Debug.Log($"Array 插入100万条数据: {sw.ElapsedMilliseconds}ms");
            
            container.Dispose();
        }

        [Test]
        public void Array_1M_Iterate_Performance()
        {
            var container = new XBlobContainer();
            container.Create(Allocator.Temp, ContainerSize);
            
            var array = container.AllocArray<int>(ElementCount);
            
            for (int i = 0; i < ElementCount; i++)
            {
                array[container, i] = i * 10;
            }
            
            var sw = Stopwatch.StartNew();
            int count = 0;
            foreach (var value in array.GetEnumerator(container))
            {
                count++;
            }
            sw.Stop();
            
            UnityEngine.Debug.Log($"Array 迭代100万条数据: {sw.ElapsedMilliseconds}ms ({count}/{ElementCount})");
            
            container.Dispose();
        }

        [Test]
        public void Map_1M_Insert_Performance()
        {
            var container = new XBlobContainer();
            container.Create(Allocator.Temp, ContainerSize);
            
            var map = container.AllocMap<int, int>(ElementCount);
            
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < ElementCount; i++)
            {
                map.AddOrUpdate(container, i, i * 10);
            }
            sw.Stop();
            
            UnityEngine.Debug.Log($"Map 插入100万条数据: {sw.ElapsedMilliseconds}ms");
            
            container.Dispose();
        }

        [Test]
        public void Map_1M_Query_Performance()
        {
            var container = new XBlobContainer();
            container.Create(Allocator.Temp, ContainerSize);
            
            var map = container.AllocMap<int, int>(ElementCount);
            
            for (int i = 0; i < ElementCount; i++)
            {
                map.AddOrUpdate(container, i, i * 10);
            }
            
            var random = new System.Random(42);
            var sw = Stopwatch.StartNew();
            int queryCount = ElementCount;
            int foundCount = 0;
            for (int i = 0; i < queryCount; i++)
            {
                int key = random.Next(0, ElementCount);
                if (map.TryGetValue(container, key, out int value))
                {
                    foundCount++;
                }
            }
            sw.Stop();
            
            UnityEngine.Debug.Log($"Map TryGetValue查询100万次: {sw.ElapsedMilliseconds}ms ({foundCount}/{queryCount})");
            
            sw.Restart();
            int hasKeyCount = 0;
            for (int i = 0; i < queryCount; i++)
            {
                int key = random.Next(0, ElementCount);
                if (map.HasKey(container, key))
                {
                    hasKeyCount++;
                }
            }
            sw.Stop();
            
            UnityEngine.Debug.Log($"Map HasKey查询100万次: {sw.ElapsedMilliseconds}ms ({hasKeyCount}/{queryCount})");
            
            var keyView = map.AsKeyView();
            sw.Restart();
            int keyViewCount = 0;
            for (int i = 0; i < queryCount; i++)
            {
                int key = random.Next(0, ElementCount);
                if (keyView.HasKey(container, key))
                {
                    keyViewCount++;
                }
            }
            sw.Stop();
            
            UnityEngine.Debug.Log($"Map KeyView查询100万次: {sw.ElapsedMilliseconds}ms ({keyViewCount}/{queryCount})");
            
            container.Dispose();
        }

        [Test]
        public void Map_1M_Iterate_Performance()
        {
            var container = new XBlobContainer();
            container.Create(Allocator.Temp, ContainerSize);
            
            var map = container.AllocMap<int, int>(ElementCount);
            
            for (int i = 0; i < ElementCount; i++)
            {
                map.AddOrUpdate(container, i, i * 10);
            }
            
            var sw = Stopwatch.StartNew();
            int count = 0;
            foreach (var kvp in map.GetEnumerator(container))
            {
                count++;
            }
            sw.Stop();
            
            UnityEngine.Debug.Log($"Map 迭代100万条数据: {sw.ElapsedMilliseconds}ms ({count}/{ElementCount})");
            
            sw.Restart();
            int keyCount = 0;
            foreach (var key in map.GetKeysEnumerator(container))
            {
                keyCount++;
            }
            sw.Stop();
            
            UnityEngine.Debug.Log($"Map Keys迭代100万条数据: {sw.ElapsedMilliseconds}ms ({keyCount}/{ElementCount})");
            
            sw.Restart();
            int valueCount = 0;
            foreach (var value in map.GetValuesEnumerator(container))
            {
                valueCount++;
            }
            sw.Stop();
            
            UnityEngine.Debug.Log($"Map Values迭代100万条数据: {sw.ElapsedMilliseconds}ms ({valueCount}/{ElementCount})");
            
            container.Dispose();
        }

        [Test]
        public void Map_1M_Update_Performance()
        {
            var container = new XBlobContainer();
            container.Create(Allocator.Temp, ContainerSize);
            
            var map = container.AllocMap<int, int>(ElementCount);
            
            for (int i = 0; i < ElementCount; i++)
            {
                map.AddOrUpdate(container, i, i * 10);
            }
            
            var random = new System.Random(42);
            var sw = Stopwatch.StartNew();
            int updateCount = 10000;
            for (int i = 0; i < updateCount; i++)
            {
                int key = random.Next(0, ElementCount);
                map.AddOrUpdate(container, key, key * 20);
            }
            sw.Stop();
            
            UnityEngine.Debug.Log($"Map 更新1万条数据: {sw.ElapsedMilliseconds}ms");
            
            container.Dispose();
        }

        [Test]
        public void MultiMap_1M_Insert_Performance()
        {
            var container = new XBlobContainer();
            container.Create(Allocator.Temp, ContainerSize);
            
            var multiMap = container.AllocMultiMap<int, int>(ElementCount);
            
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < ElementCount; i++)
            {
                multiMap.Add(container, i % 10000, i * 10); // 1万个不同的key，每个key有100个value
            }
            sw.Stop();
            
            UnityEngine.Debug.Log($"MultiMap 插入100万条数据: {sw.ElapsedMilliseconds}ms");
            
            container.Dispose();
        }

        [Test]
        public void MultiMap_1M_Iterate_Performance()
        {
            var container = new XBlobContainer();
            container.Create(Allocator.Temp, ContainerSize);
            
            var multiMap = container.AllocMultiMap<int, int>(ElementCount);
            
            for (int i = 0; i < ElementCount; i++)
            {
                multiMap.Add(container, i % 10000, i * 10);
            }
            
            var sw = Stopwatch.StartNew();
            int count = 0;
            foreach (var kvp in multiMap.GetEnumerator(container))
            {
                count++;
            }
            sw.Stop();
            
            UnityEngine.Debug.Log($"MultiMap 迭代100万条数据: {sw.ElapsedMilliseconds}ms ({count}/{ElementCount})");
            
            sw.Restart();
            int keyCount = 0;
            foreach (var key in multiMap.GetKeysEnumerator(container))
            {
                keyCount++;
            }
            sw.Stop();
            
            UnityEngine.Debug.Log($"MultiMap Keys迭代: {sw.ElapsedMilliseconds}ms ({keyCount} unique keys)");
            
            sw.Restart();
            int valueCount = 0;
            foreach (var value in multiMap.GetValuesEnumerator(container))
            {
                valueCount++;
            }
            sw.Stop();
            
            UnityEngine.Debug.Log($"MultiMap Values迭代: {sw.ElapsedMilliseconds}ms ({valueCount}/{ElementCount})");
            
            container.Dispose();
        }

        [Test]
        public void Set_1M_Insert_Performance()
        {
            var container = new XBlobContainer();
            container.Create(Allocator.Temp, ContainerSize);
            
            var set = container.AllocSet<int>(ElementCount);
            
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < ElementCount; i++)
            {
                set.Add(container, i);
            }
            sw.Stop();
            
            UnityEngine.Debug.Log($"Set 插入100万条数据: {sw.ElapsedMilliseconds}ms");
            
            container.Dispose();
        }

        [Test]
        public void Set_1M_Query_Performance()
        {
            var container = new XBlobContainer();
            container.Create(Allocator.Temp, ContainerSize);
            
            var set = container.AllocSet<int>(ElementCount);
            
            for (int i = 0; i < ElementCount; i++)
            {
                set.Add(container, i);
            }
            
            var random = new System.Random(42);
            var sw = Stopwatch.StartNew();
            int queryCount = ElementCount;
            int foundCount = 0;
            for (int i = 0; i < queryCount; i++)
            {
                int value = random.Next(0, ElementCount);
                if (set.Contains(container, value))
                {
                    foundCount++;
                }
            }
            sw.Stop();
            
            UnityEngine.Debug.Log($"Set Contains查询100万次: {sw.ElapsedMilliseconds}ms ({foundCount}/{queryCount})");
            
            container.Dispose();
        }

        [Test]
        public void Set_1M_Iterate_Performance()
        {
            var container = new XBlobContainer();
            container.Create(Allocator.Temp, ContainerSize);
            
            var set = container.AllocSet<int>(ElementCount);
            
            for (int i = 0; i < ElementCount; i++)
            {
                set.Add(container, i);
            }
            
            var sw = Stopwatch.StartNew();
            int count = 0;
            foreach (var value in set.GetEnumerator(container))
            {
                count++;
            }
            sw.Stop();
            
            UnityEngine.Debug.Log($"Set 迭代100万条数据: {sw.ElapsedMilliseconds}ms ({count}/{ElementCount})");
            
            container.Dispose();
        }

        // ========== Job 只读访问性能测试 ==========

#if UNITY_BURST
        [BurstCompile]
#endif
        struct ArrayReadJob : IJob
        {
            [NativeDisableUnsafePtrRestriction]
            public XBlobContainer Container;
            public XBlobArray<int> Array;
            public int ElementCount;
            public long Sum;

            public void Execute()
            {
                long sum = 0;
                foreach (var value in Array.GetEnumerator(Container))
                {
                    sum += value;
                }
                Sum = sum;
            }
        }

#if UNITY_BURST
        [BurstCompile]
#endif
        struct ArrayReadParallelJob : IJobParallelFor
        {
            [ReadOnly]
            [NativeDisableUnsafePtrRestriction]
            public XBlobContainer Container;
            [ReadOnly] public XBlobArray<int> Array;
            public NativeArray<long> Sums;

            public void Execute(int index)
            {
                // 每个线程处理一部分数据
                int start = index * (Array.GetLength(Container) / Sums.Length);
                int end = (index + 1) * (Array.GetLength(Container) / Sums.Length);
                if (index == Sums.Length - 1) end = Array.GetLength(Container);
                
                long sum = 0;
                for (int i = start; i < end; i++)
                {
                    sum += Array[Container, i];
                }
                Sums[index] = sum;
            }
        }

#if UNITY_BURST
        [BurstCompile]
#endif
        struct MapReadJob : IJob
        {
            [NativeDisableUnsafePtrRestriction]
            public XBlobContainer Container;
            public XBlobMap<int, int> Map;
            public long Sum;

            public void Execute()
            {
                long sum = 0;
                foreach (var kvp in Map.GetEnumerator(Container))
                {
                    sum += kvp.Value;
                }
                Sum = sum;
            }
        }

#if UNITY_BURST
        [BurstCompile]
#endif
        struct MapQueryJob : IJobParallelFor
        {
            [ReadOnly]
            [NativeDisableUnsafePtrRestriction]
            public XBlobContainer Container;
            [ReadOnly] public XBlobMap<int, int> Map;
            [ReadOnly] public NativeArray<int> QueryKeys;
            public NativeArray<int> Results;

            public void Execute(int index)
            {
                if (Map.TryGetValue(Container, QueryKeys[index], out int value))
                {
                    Results[index] = value;
                }
                else
                {
                    Results[index] = -1;
                }
            }
        }

#if UNITY_BURST
        [BurstCompile]
#endif
        struct MapKeyViewQueryJob : IJobParallelFor
        {
            [ReadOnly]
            [NativeDisableUnsafePtrRestriction]
            public XBlobContainer Container;
            [ReadOnly] public XBlobMapKey<int> KeyView;
            [ReadOnly] public NativeArray<int> QueryKeys;
            public NativeArray<bool> Results;

            public void Execute(int index)
            {
                Results[index] = KeyView.HasKey(Container, QueryKeys[index]);
            }
        }

        [Test]
        public void Array_1M_Job_Read_Performance()
        {
            var container = new XBlobContainer();
            container.Create(Allocator.TempJob, ContainerSize);
            
            var array = container.AllocArray<int>(ElementCount);
            
            for (int i = 0; i < ElementCount; i++)
            {
                array[container, i] = i * 10;
            }
            
            var job = new ArrayReadJob
            {
                Container = container,
                Array = array,
                ElementCount = ElementCount
            };
            
            var sw = Stopwatch.StartNew();
            var handle = job.Schedule();
            handle.Complete();
            sw.Stop();
            
            UnityEngine.Debug.Log($"Array Job只读迭代100万条数据: {sw.ElapsedMilliseconds}ms (Sum: {job.Sum})");
            
            container.Dispose();
        }

        [Test]
        public void Array_1M_Job_Parallel_Read_Performance()
        {
            var container = new XBlobContainer();
            container.Create(Allocator.TempJob, ContainerSize);
            
            var array = container.AllocArray<int>(ElementCount);
            
            for (int i = 0; i < ElementCount; i++)
            {
                array[container, i] = i * 10;
            }
            
            int threadCount = System.Environment.ProcessorCount;
            var sums = new NativeArray<long>(threadCount, Allocator.TempJob);
            
            var job = new ArrayReadParallelJob
            {
                Container = container,
                Array = array,
                Sums = sums
            };
            
            var sw = Stopwatch.StartNew();
            var handle = job.Schedule(threadCount, 1);
            handle.Complete();
            sw.Stop();
            
            long totalSum = 0;
            for (int i = 0; i < threadCount; i++)
            {
                totalSum += sums[i];
            }
            
            UnityEngine.Debug.Log($"Array Job并行只读迭代100万条数据 ({threadCount}线程): {sw.ElapsedMilliseconds}ms (Sum: {totalSum})");
            
            sums.Dispose();
            container.Dispose();
        }

        [Test]
        public void Map_1M_Job_Read_Performance()
        {
            var container = new XBlobContainer();
            container.Create(Allocator.TempJob, ContainerSize);
            
            var map = container.AllocMap<int, int>(ElementCount);
            
            for (int i = 0; i < ElementCount; i++)
            {
                map.AddOrUpdate(container, i, i * 10);
            }
            
            var job = new MapReadJob
            {
                Container = container,
                Map = map
            };
            
            var sw = Stopwatch.StartNew();
            var handle = job.Schedule();
            handle.Complete();
            sw.Stop();
            
            UnityEngine.Debug.Log($"Map Job只读迭代100万条数据: {sw.ElapsedMilliseconds}ms (Sum: {job.Sum})");
            
            container.Dispose();
        }

        [Test]
        public void Map_1M_Job_Query_Performance()
        {
            var container = new XBlobContainer();
            container.Create(Allocator.TempJob, ContainerSize);
            
            var map = container.AllocMap<int, int>(ElementCount);
            
            for (int i = 0; i < ElementCount; i++)
            {
                map.AddOrUpdate(container, i, i * 10);
            }
            
            var random = new System.Random(42);
            var queryKeys = new NativeArray<int>(ElementCount, Allocator.TempJob);
            for (int i = 0; i < ElementCount; i++)
            {
                queryKeys[i] = random.Next(0, ElementCount);
            }
            
            var results = new NativeArray<int>(ElementCount, Allocator.TempJob);
            
            var job = new MapQueryJob
            {
                Container = container,
                Map = map,
                QueryKeys = queryKeys,
                Results = results
            };
            
            var sw = Stopwatch.StartNew();
            var handle = job.Schedule(ElementCount, 1000);
            handle.Complete();
            sw.Stop();
            
            int foundCount = 0;
            for (int i = 0; i < ElementCount; i++)
            {
                if (results[i] >= 0) foundCount++;
            }
            
            UnityEngine.Debug.Log($"Map Job并行查询100万次: {sw.ElapsedMilliseconds}ms ({foundCount}/{ElementCount})");
            
            results.Dispose();
            queryKeys.Dispose();
            container.Dispose();
        }

        [Test]
        public void Map_1M_Job_KeyView_Query_Performance()
        {
            var container = new XBlobContainer();
            container.Create(Allocator.TempJob, ContainerSize);
            
            var map = container.AllocMap<int, int>(ElementCount);
            
            for (int i = 0; i < ElementCount; i++)
            {
                map.AddOrUpdate(container, i, i * 10);
            }
            
            var keyView = map.AsKeyView();
            
            var random = new System.Random(42);
            var queryKeys = new NativeArray<int>(ElementCount, Allocator.TempJob);
            for (int i = 0; i < ElementCount; i++)
            {
                queryKeys[i] = random.Next(0, ElementCount);
            }
            
            var results = new NativeArray<bool>(ElementCount, Allocator.TempJob);
            
            var job = new MapKeyViewQueryJob
            {
                Container = container,
                KeyView = keyView,
                QueryKeys = queryKeys,
                Results = results
            };
            
            var sw = Stopwatch.StartNew();
            var handle = job.Schedule(ElementCount, 1000);
            handle.Complete();
            sw.Stop();
            
            int foundCount = 0;
            for (int i = 0; i < ElementCount; i++)
            {
                if (results[i]) foundCount++;
            }
            
            UnityEngine.Debug.Log($"Map KeyView Job并行查询100万次: {sw.ElapsedMilliseconds}ms ({foundCount}/{ElementCount})");
            
            results.Dispose();
            queryKeys.Dispose();
            container.Dispose();
        }

        /// <summary>
        /// 综合基准对比测试：将 XBlob 容器与 Unity 原生容器进行全面性能对比
        /// 测试所有容器的所有关键操作，结果汇总到一条日志
        /// </summary>
        [Test]
        public void Benchmark_CompareAllContainersWithNative()
        {
            const int containerSize = 100000; // 10万容器大小
            const int testIterations = 1000000; // 100万次测试
            var results = new System.Text.StringBuilder();
            results.AppendLine("\n========== XBlob 容器 vs 原生容器 性能对比基准测试 ==========");
            results.AppendLine($"容器大小: {containerSize:N0} 元素");
            results.AppendLine($"测试次数: {testIterations:N0} 次");
            results.AppendLine("================================================================\n");

            // ==================== Map vs NativeHashMap ====================
            results.AppendLine("【Map vs NativeHashMap】");
            
            // Map 插入测试
            var container = new XBlobContainer();
            container.Create(Allocator.TempJob, 50000000); // 50MB容器
            var xblobMap = container.AllocMap<int, int>(containerSize);
            
            var sw = Stopwatch.StartNew();
            for (int i = 0; i < containerSize; i++)
            {
                xblobMap.AddOrUpdate(container, i, i * 10);
            }
            sw.Stop();
            long mapInsertMs = sw.ElapsedMilliseconds;
            
            // NativeHashMap 插入测试
            var nativeMap = new NativeHashMap<int, int>(containerSize, Allocator.TempJob);
            sw.Restart();
            for (int i = 0; i < containerSize; i++)
            {
                nativeMap.TryAdd(i, i * 10);
            }
            sw.Stop();
            long nativeMapInsertMs = sw.ElapsedMilliseconds;
            
            results.AppendLine($"  插入:   XBlobMap={mapInsertMs}ms | NativeHashMap={nativeMapInsertMs}ms | 比率={GetRatio(mapInsertMs, nativeMapInsertMs)}");
            
            // Map 查询测试 - 100万次查询
            sw.Restart();
            int mapQueryHit = 0;
            for (int i = 0; i < testIterations; i++)
            {
                int key = i % containerSize; // 循环查询容器内的键
                if (xblobMap.TryGetValue(container, key, out int val))
                    mapQueryHit++;
            }
            sw.Stop();
            long mapQueryMs = sw.ElapsedMilliseconds;
            
            // NativeHashMap 查询测试 - 100万次查询
            sw.Restart();
            int nativeQueryHit = 0;
            for (int i = 0; i < testIterations; i++)
            {
                int key = i % containerSize;
                if (nativeMap.TryGetValue(key, out int val))
                    nativeQueryHit++;
            }
            sw.Stop();
            long nativeMapQueryMs = sw.ElapsedMilliseconds;
            
            results.AppendLine($"  查询:   XBlobMap={mapQueryMs}ms | NativeHashMap={nativeMapQueryMs}ms | 比率={GetRatio(mapQueryMs, nativeMapQueryMs)}");
            
            // Map 更新测试 - 100万次更新
            sw.Restart();
            for (int i = 0; i < testIterations; i++)
            {
                int key = i % containerSize;
                xblobMap.AddOrUpdate(container, key, i * 20);
            }
            sw.Stop();
            long mapUpdateMs = sw.ElapsedMilliseconds;
            
            // NativeHashMap 更新测试 - 100万次更新
            sw.Restart();
            for (int i = 0; i < testIterations; i++)
            {
                int key = i % containerSize;
                nativeMap[key] = i * 20;
            }
            sw.Stop();
            long nativeMapUpdateMs = sw.ElapsedMilliseconds;
            
            results.AppendLine($"  更新:   XBlobMap={mapUpdateMs}ms | NativeHashMap={nativeMapUpdateMs}ms | 比率={GetRatio(mapUpdateMs, nativeMapUpdateMs)}");
            
            // Map HasKey 测试 - 100万次检查
            sw.Restart();
            int mapHasKeyCount = 0;
            for (int i = 0; i < testIterations; i++)
            {
                int key = i % containerSize;
                if (xblobMap.HasKey(container, key))
                    mapHasKeyCount++;
            }
            sw.Stop();
            long mapHasKeyMs = sw.ElapsedMilliseconds;
            
            // NativeHashMap ContainsKey 测试 - 100万次检查
            sw.Restart();
            int nativeContainsCount = 0;
            for (int i = 0; i < testIterations; i++)
            {
                int key = i % containerSize;
                if (nativeMap.ContainsKey(key))
                    nativeContainsCount++;
            }
            sw.Stop();
            long nativeMapContainsMs = sw.ElapsedMilliseconds;
            
            results.AppendLine($"  存在检查: XBlobMap={mapHasKeyMs}ms | NativeHashMap={nativeMapContainsMs}ms | 比率={GetRatio(mapHasKeyMs, nativeMapContainsMs)}");
            
            // Map 迭代测试
            sw.Restart();
            int mapIterCount = 0;
            foreach (var kvp in xblobMap.GetEnumerator(container))
            {
                mapIterCount++;
            }
            sw.Stop();
            long mapIterMs = sw.ElapsedMilliseconds;
            
            // NativeHashMap 迭代测试
            sw.Restart();
            int nativeIterCount = 0;
            foreach (var kvp in nativeMap)
            {
                nativeIterCount++;
            }
            sw.Stop();
            long nativeMapIterMs = sw.ElapsedMilliseconds;
            
            results.AppendLine($"  迭代:   XBlobMap={mapIterMs}ms | NativeHashMap={nativeMapIterMs}ms | 比率={GetRatio(mapIterMs, nativeMapIterMs)}");
            
            long mapTotalMs = mapInsertMs + mapQueryMs + mapUpdateMs + mapHasKeyMs + mapIterMs;
            long nativeMapTotalMs = nativeMapInsertMs + nativeMapQueryMs + nativeMapUpdateMs + nativeMapContainsMs + nativeMapIterMs;
            results.AppendLine($"  总计:   XBlobMap={mapTotalMs}ms | NativeHashMap={nativeMapTotalMs}ms | 比率={GetRatio(mapTotalMs, nativeMapTotalMs)}\n");
            
            nativeMap.Dispose();
            container.Dispose();

            // ==================== Set vs NativeHashSet ====================
            results.AppendLine("【Set vs NativeHashSet】");
            
            container = new XBlobContainer();
            container.Create(Allocator.TempJob, 50000000);
            var xblobSet = container.AllocSet<int>(containerSize);
            
            // Set 插入测试
            sw.Restart();
            for (int i = 0; i < containerSize; i++)
            {
                xblobSet.Add(container, i);
            }
            sw.Stop();
            long setInsertMs = sw.ElapsedMilliseconds;
            
            // NativeHashSet 插入测试
            var nativeSet = new NativeHashSet<int>(containerSize, Allocator.TempJob);
            sw.Restart();
            for (int i = 0; i < containerSize; i++)
            {
                nativeSet.Add(i);
            }
            sw.Stop();
            long nativeSetInsertMs = sw.ElapsedMilliseconds;
            
            results.AppendLine($"  插入:   XBlobSet={setInsertMs}ms | NativeHashSet={nativeSetInsertMs}ms | 比率={GetRatio(setInsertMs, nativeSetInsertMs)}");
            
            // Set Contains 测试 - 100万次查询
            sw.Restart();
            int setContainsCount = 0;
            for (int i = 0; i < testIterations; i++)
            {
                int val = i % containerSize;
                if (xblobSet.Contains(container, val))
                    setContainsCount++;
            }
            sw.Stop();
            long setContainsMs = sw.ElapsedMilliseconds;
            
            // NativeHashSet Contains 测试 - 100万次查询
            sw.Restart();
            int nativeSetContainsCount = 0;
            for (int i = 0; i < testIterations; i++)
            {
                int val = i % containerSize;
                if (nativeSet.Contains(val))
                    nativeSetContainsCount++;
            }
            sw.Stop();
            long nativeSetContainsMs = sw.ElapsedMilliseconds;
            
            results.AppendLine($"  查询:   XBlobSet={setContainsMs}ms | NativeHashSet={nativeSetContainsMs}ms | 比率={GetRatio(setContainsMs, nativeSetContainsMs)}");
            
            // Set 迭代测试
            sw.Restart();
            int setIterCount = 0;
            foreach (var val in xblobSet.GetEnumerator(container))
            {
                setIterCount++;
            }
            sw.Stop();
            long setIterMs = sw.ElapsedMilliseconds;
            
            // NativeHashSet 迭代测试
            sw.Restart();
            int nativeSetIterCount = 0;
            foreach (var val in nativeSet)
            {
                nativeSetIterCount++;
            }
            sw.Stop();
            long nativeSetIterMs = sw.ElapsedMilliseconds;
            
            results.AppendLine($"  迭代:   XBlobSet={setIterMs}ms | NativeHashSet={nativeSetIterMs}ms | 比率={GetRatio(setIterMs, nativeSetIterMs)}");
            
            long setTotalMs = setInsertMs + setContainsMs + setIterMs;
            long nativeSetTotalMs = nativeSetInsertMs + nativeSetContainsMs + nativeSetIterMs;
            results.AppendLine($"  总计:   XBlobSet={setTotalMs}ms | NativeHashSet={nativeSetTotalMs}ms | 比率={GetRatio(setTotalMs, nativeSetTotalMs)}\n");
            
            nativeSet.Dispose();
            container.Dispose();

            // ==================== MultiMap vs NativeParallelMultiHashMap ====================
            results.AppendLine("【MultiMap vs NativeParallelMultiHashMap】");
            
            container = new XBlobContainer();
            container.Create(Allocator.TempJob, 50000000);
            var xblobMultiMap = container.AllocMultiMap<int, int>(containerSize);
            
            // MultiMap 插入测试（每个键1个值）
            sw.Restart();
            for (int i = 0; i < containerSize; i++)
            {
                xblobMultiMap.Add(container, i, i * 10);
            }
            sw.Stop();
            long multiMapInsertMs = sw.ElapsedMilliseconds;
            
            // NativeParallelMultiHashMap 插入测试
            var nativeMultiMap = new NativeParallelMultiHashMap<int, int>(containerSize, Allocator.TempJob);
            sw.Restart();
            for (int i = 0; i < containerSize; i++)
            {
                nativeMultiMap.Add(i, i * 10);
            }
            sw.Stop();
            long nativeMultiMapInsertMs = sw.ElapsedMilliseconds;
            
            results.AppendLine($"  插入:   XBlobMultiMap={multiMapInsertMs}ms | NativeMultiHashMap={nativeMultiMapInsertMs}ms | 比率={GetRatio(multiMapInsertMs, nativeMultiMapInsertMs)}");
            
            // MultiMap ContainsKey 测试 - 100万次查询
            sw.Restart();
            int multiMapContainsCount = 0;
            for (int i = 0; i < testIterations; i++)
            {
                int key = i % containerSize;
                if (xblobMultiMap.ContainsKey(container, key))
                    multiMapContainsCount++;
            }
            sw.Stop();
            long multiMapContainsMs = sw.ElapsedMilliseconds;
            
            // NativeParallelMultiHashMap ContainsKey 测试 - 100万次查询
            sw.Restart();
            int nativeMultiContainsCount = 0;
            for (int i = 0; i < testIterations; i++)
            {
                int key = i % containerSize;
                if (nativeMultiMap.ContainsKey(key))
                    nativeMultiContainsCount++;
            }
            sw.Stop();
            long nativeMultiMapContainsMs = sw.ElapsedMilliseconds;
            
            results.AppendLine($"  查询:   XBlobMultiMap={multiMapContainsMs}ms | NativeMultiHashMap={nativeMultiMapContainsMs}ms | 比率={GetRatio(multiMapContainsMs, nativeMultiMapContainsMs)}");
            
            // MultiMap 迭代测试
            sw.Restart();
            int multiMapIterCount = 0;
            foreach (var kvp in xblobMultiMap.GetEnumerator(container))
            {
                multiMapIterCount++;
            }
            sw.Stop();
            long multiMapIterMs = sw.ElapsedMilliseconds;
            
            // NativeMultiHashMap 迭代测试
            sw.Restart();
            int nativeMultiIterCount = 0;
            foreach (var kvp in nativeMultiMap)
            {
                nativeMultiIterCount++;
            }
            sw.Stop();
            long nativeMultiMapIterMs = sw.ElapsedMilliseconds;
            
            results.AppendLine($"  迭代:   XBlobMultiMap={multiMapIterMs}ms | NativeMultiHashMap={nativeMultiMapIterMs}ms | 比率={GetRatio(multiMapIterMs, nativeMultiMapIterMs)}");
            
            long multiMapTotalMs = multiMapInsertMs + multiMapContainsMs + multiMapIterMs;
            long nativeMultiMapTotalMs = nativeMultiMapInsertMs + nativeMultiMapContainsMs + nativeMultiMapIterMs;
            results.AppendLine($"  总计:   XBlobMultiMap={multiMapTotalMs}ms | NativeParallelMultiHashMap={nativeMultiMapTotalMs}ms | 比率={GetRatio(multiMapTotalMs, nativeMultiMapTotalMs)}\n");
            
            nativeMultiMap.Dispose();
            container.Dispose();

            // ==================== 总结 ====================
            results.AppendLine("================================================================");
            results.AppendLine("【综合对比总结】");
            long xblobTotalMs = mapTotalMs + setTotalMs + multiMapTotalMs;
            long nativeTotalMs = nativeMapTotalMs + nativeSetTotalMs + nativeMultiMapTotalMs;
            results.AppendLine($"  所有容器总耗时: XBlob={xblobTotalMs}ms | Native={nativeTotalMs}ms | 比率={GetRatio(xblobTotalMs, nativeTotalMs)}");
            results.AppendLine("================================================================");

            // 输出完整报告到一条日志
            UnityEngine.Debug.Log(results.ToString());
        }

        /// <summary>
        /// 计算性能比率，返回格式化字符串
        /// </summary>
        private string GetRatio(long xblobMs, long nativeMs)
        {
            if (nativeMs == 0) return "N/A";
            double ratio = (double)xblobMs / nativeMs;
            string performance = ratio <= 1.1 ? "✓" : ratio <= 1.5 ? "~" : "✗";
            return $"{ratio:F2}x {performance}";
        }
    }
}
