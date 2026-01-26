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

namespace XMFrame.XBlob.Tests
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
    }
}
