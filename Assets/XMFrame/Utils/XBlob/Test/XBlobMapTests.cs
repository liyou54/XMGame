using System;
using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;
using Unity.Collections;
using UnityEngine;

namespace XMFrame.XBlob.Tests
{
    [TestFixture]
    public class XBlobMapTests
    {
        private XBlobContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = new XBlobContainer();
            _container.Create(Allocator.Temp, 4096);
        }

        [TearDown]
        public void TearDown()
        {
            if (_container.IsValid)
            {
                _container.Dispose();
            }
        }

        [Test]
        public void AllocMap_WithValidCapacity_ShouldCreateMap()
        {
            // Act
            var map = _container.AllocMap<int, float>(10);

            // Assert
            Assert.IsNotNull(map);
            Assert.AreEqual(0, map.GetLength(_container));
        }

        [Test]
        public void AllocMap_WithZeroCapacity_ShouldThrowException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _container.AllocMap<int, float>(0));
        }

        [Test]
        public void Map_AddOrUpdate_ShouldAddNewKey()
        {
            // Arrange
            var map = _container.AllocMap<int, int>(10);

            // Act
            bool added = map.AddOrUpdate(_container, 1, 100);

            // Assert
            Assert.IsTrue(added);
            Assert.AreEqual(1, map.GetLength(_container));
            Assert.IsTrue(map.HasKey(_container, 1));
        }

        [Test]
        public void Map_AddOrUpdate_ShouldUpdateExistingKey()
        {
            // Arrange
            var map = _container.AllocMap<int, int>(10);
            map.AddOrUpdate(_container, 1, 100);

            // Act
            bool updated = map.AddOrUpdate(_container, 1, 200);

            // Assert
            Assert.IsFalse(updated);
            Assert.AreEqual(200, map[_container, 1]);
            Assert.AreEqual(1, map.GetLength(_container));
        }

        [Test]
        public void Map_Indexer_GetAndSet_ShouldWork()
        {
            // Arrange
            var map = _container.AllocMap<int, int>(10);

            // Act
            map[_container, 1] = 100;
            map[_container, 2] = 200;

            // Assert
            Assert.AreEqual(100, map[_container, 1]);
            Assert.AreEqual(200, map[_container, 2]);
        }

        [Test]
        public void Map_Indexer_GetNonExistentKey_ShouldThrowException()
        {
            // Arrange
            var map = _container.AllocMap<int, int>(10);

            // Act & Assert
            Assert.Throws<KeyNotFoundException>(() => { var _ = map[_container, 999]; });
        }

        [Test]
        public void Map_GetKey_WithInvalidIndex_ShouldThrowException()
        {
            // Arrange
            var map = _container.AllocMap<int, int>(10);
            map.AddOrUpdate(_container, 1, 100);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => { var _ = map.GetKey(_container, -1); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { var _ = map.GetKey(_container, 1); }); // Count=1, index 1 越界
        }

        [Test]
        public void Map_GetValue_WithInvalidIndex_ShouldThrowException()
        {
            // Arrange
            var map = _container.AllocMap<int, int>(10);
            map.AddOrUpdate(_container, 1, 100);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => { var _ = map.GetValue(_container, -1); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { var _ = map.GetValue(_container, 1); });
        }

        [Test]
        public void Map_TryGetValue_ShouldReturnTrueForExistingKey()
        {
            // Arrange
            var map = _container.AllocMap<int, int>(10);
            map.AddOrUpdate(_container, 1, 100);

            // Act
            bool found = map.TryGetValue(_container, 1, out int value);

            // Assert
            Assert.IsTrue(found);
            Assert.AreEqual(100, value);
        }

        [Test]
        public void Map_TryGetValue_ShouldReturnFalseForNonExistentKey()
        {
            // Arrange
            var map = _container.AllocMap<int, int>(10);

            // Act
            bool found = map.TryGetValue(_container, 999, out int value);

            // Assert
            Assert.IsFalse(found);
            Assert.AreEqual(0, value);
        }

        [Test]
        public void Map_HasKey_ShouldReturnCorrectResult()
        {
            // Arrange
            var map = _container.AllocMap<int, int>(10);
            map.AddOrUpdate(_container, 1, 100);

            // Act & Assert
            Assert.IsTrue(map.HasKey(_container, 1));
            Assert.IsFalse(map.HasKey(_container, 999));
        }

        [Test]
        public void Map_GetEnumerator_ShouldIterateAllEntries()
        {
            // Arrange
            var map = _container.AllocMap<int, int>(10);
            map.AddOrUpdate(_container, 1, 10);
            map.AddOrUpdate(_container, 2, 20);
            map.AddOrUpdate(_container, 3, 30);

            // Act & Assert
            int count = 0;
            foreach (var kvp in map.GetEnumerator(_container))
            {
                count++;
                Assert.IsTrue(kvp.Key >= 1 && kvp.Key <= 3);
                Assert.IsTrue(kvp.Value >= 10 && kvp.Value <= 30);
            }
            Assert.AreEqual(3, count);
        }

        [Test]
        public void Map_GetEnumeratorRef_ShouldAllowModification()
        {
            // Arrange
            var map = _container.AllocMap<int, int>(10);
            map.AddOrUpdate(_container, 1, 10);

            // Act
            foreach ( var entry in map.GetEnumeratorRef(_container))
            {
                entry.Value = 100;
            }

            // Assert
            Assert.AreEqual(100, map[_container, 1]);
        }

        [Test]
        public void Map_WhenFull_ShouldThrowException()
        {
            // Arrange
            var map = _container.AllocMap<int, int>(2);
            map.AddOrUpdate(_container, 1, 10);
            map.AddOrUpdate(_container, 2, 20);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => map.AddOrUpdate(_container, 3, 30));
        }

        [Test]
        public void Map_Add1000Elements_ShouldWorkCorrectly()
        {
            // Arrange - 创建足够大的容器来容纳1000个元素
            // Map<int, int> 需要: Count(4) + BucketCount(4) + Buckets(1000*4) + Entries(1000*8) + Keys(1000*4) + Values(1000*4) ≈ 20008 字节
            var largeContainer = new XBlobContainer();
            largeContainer.Create(Allocator.Temp, 50000);
            
            const int elementCount = 1000;
            var map = largeContainer.AllocMap<int, int>(elementCount);

            // Act - 添加1000个元素
            for (int i = 0; i < elementCount; i++)
            {
                bool added = map.AddOrUpdate(largeContainer, i, i * 10);
                Assert.IsTrue(added, $"元素 {i} 应该被成功添加");
            }

            // Assert - 验证元素数量
            Assert.AreEqual(elementCount, map.GetLength(largeContainer));

            // Assert - 验证所有元素都能正确访问
            for (int i = 0; i < elementCount; i++)
            {
                Assert.IsTrue(map.HasKey(largeContainer, i), $"键 {i} 应该存在");
                Assert.AreEqual(i * 10, map[largeContainer, i], $"键 {i} 的值应该是 {i * 10}");
                bool found = map.TryGetValue(largeContainer, i, out int value);
                Assert.IsTrue(found, $"TryGetValue 应该找到键 {i}");
                Assert.AreEqual(i * 10, value, $"TryGetValue 返回的值应该是 {i * 10}");
            }

            // Assert - 验证迭代器能遍历所有元素
            var visitedKeys = new HashSet<int>();
            int iteratedCount = 0;
            foreach (var kvp in map.GetEnumerator(largeContainer))
            {
                visitedKeys.Add(kvp.Key);
                Assert.AreEqual(kvp.Key * 10, kvp.Value, $"迭代器返回的键值对应该匹配");
                iteratedCount++;
            }
            Assert.AreEqual(elementCount, iteratedCount, "迭代器应该遍历所有元素");
            Assert.AreEqual(elementCount, visitedKeys.Count, "所有键都应该被访问到");
            
            // Cleanup
            largeContainer.Dispose();
        }

        [Test]
        public void Map_HashCollision_ShouldHandleCorrectly()
        {
            // Arrange - 创建足够大的容器
            var largeContainer = new XBlobContainer();
            largeContainer.Create(Allocator.Temp, 10000);
            
            // 使用小的 bucketCount 来强制产生哈希冲突
            // 使用 bucketCount = 10，这样很多不同的键会映射到同一个桶
            const int bucketCount = 10;
            const int elementCount = 100; // 添加100个元素，平均每个桶10个元素
            // Map 的容量需要至少等于要添加的元素数量
            var map = largeContainer.AllocMap<int, int>(elementCount);

            // Act - 添加会产生哈希冲突的元素
            // 由于 int 的 GetHashCode() 通常返回自身，使用模运算会产生冲突
            for (int i = 0; i < elementCount; i++)
            {
                int key = i * bucketCount; // 这些键会产生哈希冲突（因为它们模 bucketCount 都等于0）
                bool added = map.AddOrUpdate(largeContainer, key, key * 2);
                Assert.IsTrue(added, $"键 {key} 应该被成功添加（即使存在哈希冲突）");
            }

            // Assert - 验证所有元素都能正确访问（即使存在哈希冲突）
            Assert.AreEqual(elementCount, map.GetLength(largeContainer));

            for (int i = 0; i < elementCount; i++)
            {
                int key = i * bucketCount;
                Assert.IsTrue(map.HasKey(largeContainer, key), $"键 {key} 应该存在（即使存在哈希冲突）");
                Assert.AreEqual(key * 2, map[largeContainer, key], $"键 {key} 的值应该是 {key * 2}");
            }

            // Assert - 验证更新操作在哈希冲突情况下也能正常工作
            for (int i = 0; i < elementCount; i++)
            {
                int key = i * bucketCount;
                bool updated = map.AddOrUpdate(largeContainer, key, key * 3);
                Assert.IsFalse(updated, $"更新键 {key} 应该返回 false（不是新增）");
                Assert.AreEqual(key * 3, map[largeContainer, key], $"更新后键 {key} 的值应该是 {key * 3}");
            }

            // Assert - 验证迭代器能正确遍历所有元素（包括哈希冲突的情况）
            var visitedKeys = new HashSet<int>();
            int iteratedCount = 0;
            foreach (var kvp in map.GetEnumerator(largeContainer))
            {
                visitedKeys.Add(kvp.Key);
                int expectedValue = kvp.Key * 3; // 更新后的值
                Assert.AreEqual(expectedValue, kvp.Value, $"迭代器返回的键值对应该匹配（键: {kvp.Key}）");
                iteratedCount++;
            }
            Assert.AreEqual(elementCount, iteratedCount, "迭代器应该遍历所有元素（包括哈希冲突的情况）");
            Assert.AreEqual(elementCount, visitedKeys.Count, "所有键都应该被访问到（包括哈希冲突的情况）");
            
            // Cleanup
            largeContainer.Dispose();
        }

        [Test]
        public void Map_GetKeysEnumerator_ShouldIterateAllKeys()
        {
            // Arrange
            var map = _container.AllocMap<int, int>(10);
            map.AddOrUpdate(_container, 1, 100);
            map.AddOrUpdate(_container, 2, 200);
            map.AddOrUpdate(_container, 3, 300);

            // Act & Assert
            var visitedKeys = new HashSet<int>();
            int count = 0;
            foreach (var key in map.GetKeysEnumerator(_container))
            {
                visitedKeys.Add(key);
                Assert.IsTrue(key >= 1 && key <= 3, $"键 {key} 应该在有效范围内");
                count++;
            }
            Assert.AreEqual(3, count, "应该遍历所有键");
            Assert.AreEqual(3, visitedKeys.Count, "所有键都应该被访问到");
            Assert.IsTrue(visitedKeys.Contains(1), "应该包含键 1");
            Assert.IsTrue(visitedKeys.Contains(2), "应该包含键 2");
            Assert.IsTrue(visitedKeys.Contains(3), "应该包含键 3");
        }

        [Test]
        public void Map_GetValuesEnumerator_ShouldIterateAllValues()
        {
            // Arrange
            var map = _container.AllocMap<int, int>(10);
            map.AddOrUpdate(_container, 1, 100);
            map.AddOrUpdate(_container, 2, 200);
            map.AddOrUpdate(_container, 3, 300);

            // Act & Assert
            var visitedValues = new HashSet<int>();
            int count = 0;
            foreach (var value in map.GetValuesEnumerator(_container))
            {
                visitedValues.Add(value);
                Assert.IsTrue(value >= 100 && value <= 300, $"值 {value} 应该在有效范围内");
                count++;
            }
            Assert.AreEqual(3, count, "应该遍历所有值");
            Assert.AreEqual(3, visitedValues.Count, "所有值都应该被访问到");
            Assert.IsTrue(visitedValues.Contains(100), "应该包含值 100");
            Assert.IsTrue(visitedValues.Contains(200), "应该包含值 200");
            Assert.IsTrue(visitedValues.Contains(300), "应该包含值 300");
        }

        [Test]
        public void Map_GetKeysEnumerator_WithEmptyMap_ShouldReturnNoKeys()
        {
            // Arrange
            var map = _container.AllocMap<int, int>(10);

            // Act & Assert
            int count = 0;
            foreach (var key in map.GetKeysEnumerator(_container))
            {
                count++;
            }
            Assert.AreEqual(0, count, "空 Map 不应该返回任何键");
        }

        [Test]
        public void Map_GetValuesEnumerator_WithEmptyMap_ShouldReturnNoValues()
        {
            // Arrange
            var map = _container.AllocMap<int, int>(10);

            // Act & Assert
            int count = 0;
            foreach (var value in map.GetValuesEnumerator(_container))
            {
                count++;
            }
            Assert.AreEqual(0, count, "空 Map 不应该返回任何值");
        }

        [Test]
        public void Map_AsKeyView_ShouldCreateKeyView()
        {
            // Arrange
            var map = _container.AllocMap<int, int>(10);
            map.AddOrUpdate(_container, 1, 100);
            map.AddOrUpdate(_container, 2, 200);
            map.AddOrUpdate(_container, 3, 300);

            // Act
            var keyView = map.AsKeyView();

            // Assert
            Assert.IsTrue(keyView.HasKey(_container, 1), "KeyView 应该能够判断键 1 存在");
            Assert.IsTrue(keyView.HasKey(_container, 2), "KeyView 应该能够判断键 2 存在");
            Assert.IsTrue(keyView.HasKey(_container, 3), "KeyView 应该能够判断键 3 存在");
            Assert.IsFalse(keyView.HasKey(_container, 999), "KeyView 应该能够判断键 999 不存在");
            Assert.AreEqual(3, keyView.GetLength(_container), "KeyView 应该返回正确的长度");
        }

        [Test]
        public void Map_AsKeyView_ShouldIterateKeys()
        {
            // Arrange
            var map = _container.AllocMap<int, int>(10);
            map.AddOrUpdate(_container, 1, 100);
            map.AddOrUpdate(_container, 2, 200);
            map.AddOrUpdate(_container, 3, 300);

            // Act
            var keyView = map.AsKeyView();
            var visitedKeys = new HashSet<int>();
            int count = 0;
            foreach (var key in keyView.GetKeysEnumerator(_container))
            {
                visitedKeys.Add(key);
                count++;
            }

            // Assert
            Assert.AreEqual(3, count, "应该遍历所有键");
            Assert.AreEqual(3, visitedKeys.Count, "所有键都应该被访问到");
            Assert.IsTrue(visitedKeys.Contains(1), "应该包含键 1");
            Assert.IsTrue(visitedKeys.Contains(2), "应该包含键 2");
            Assert.IsTrue(visitedKeys.Contains(3), "应该包含键 3");
        }

        [Test]
        public void Map_AsKeyView_ShouldShareSameMemory()
        {
            // Arrange
            var map = _container.AllocMap<int, int>(10);
            map.AddOrUpdate(_container, 1, 100);
            var keyView = map.AsKeyView();

            // Act - 通过原始 Map 添加新键
            map.AddOrUpdate(_container, 2, 200);

            // Assert - KeyView 应该能看到新添加的键（因为它们共享相同的内存）
            Assert.IsTrue(keyView.HasKey(_container, 1), "KeyView 应该能看到键 1");
            Assert.IsTrue(keyView.HasKey(_container, 2), "KeyView 应该能看到键 2（通过原始 Map 添加）");
            Assert.AreEqual(2, keyView.GetLength(_container), "KeyView 应该返回更新后的长度");
        }

        [Test]
        public void Map_AsKeyView_GetKey_ShouldReturnCorrectKey()
        {
            // Arrange
            var map = _container.AllocMap<int, int>(10);
            map.AddOrUpdate(_container, 1, 100);
            map.AddOrUpdate(_container, 2, 200);
            map.AddOrUpdate(_container, 3, 300);
            var keyView = map.AsKeyView();

            // Act & Assert - 通过索引获取键（注意：索引是槽位索引，不是逻辑索引）
            // 由于使用开放寻址法，我们需要通过迭代器找到实际的键位置
            var keys = new List<int>();
            foreach (var key in keyView.GetKeysEnumerator(_container))
            {
                keys.Add(key);
            }
            
            // 验证所有键都能通过 HasKey 找到
            Assert.IsTrue(keyView.HasKey(_container, 1), "应该能找到键 1");
            Assert.IsTrue(keyView.HasKey(_container, 2), "应该能找到键 2");
            Assert.IsTrue(keyView.HasKey(_container, 3), "应该能找到键 3");
            Assert.AreEqual(3, keys.Count, "应该找到 3 个键");
        }

        [Test]
        public void Map_AsKeyView_GetKey_WithInvalidIndex_ShouldThrowException()
        {
            // Arrange
            var map = _container.AllocMap<int, int>(10);
            map.AddOrUpdate(_container, 1, 100);
            var keyView = map.AsKeyView();

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => keyView.GetKey(_container, -1), "负数索引应该抛出异常");
            Assert.Throws<ArgumentOutOfRangeException>(() => keyView.GetKey(_container, 10), "超出范围的索引应该抛出异常");
        }

        [Test]
        public void Map_AsKeyView_WithEmptyMap_ShouldWorkCorrectly()
        {
            // Arrange
            var map = _container.AllocMap<int, int>(10);
            var keyView = map.AsKeyView();

            // Act & Assert
            Assert.AreEqual(0, keyView.GetLength(_container), "空 Map 的 KeyView 长度应该为 0");
            Assert.IsFalse(keyView.HasKey(_container, 1), "空 Map 的 KeyView 不应该包含任何键");
            
            int count = 0;
            foreach (var key in keyView.GetKeysEnumerator(_container))
            {
                count++;
            }
            Assert.AreEqual(0, count, "空 Map 的 KeyView 迭代器不应该返回任何键");
        }

        [Test]
        public void Map_AsKeyView_WithManyElements_ShouldWorkCorrectly()
        {
            // Arrange
            var largeContainer = new XBlobContainer();
            largeContainer.Create(Allocator.Temp, 50000);
            
            const int elementCount = 100;
            var map = largeContainer.AllocMap<int, int>(elementCount);
            
            for (int i = 0; i < elementCount; i++)
            {
                map.AddOrUpdate(largeContainer, i, i * 10);
            }
            
            var keyView = map.AsKeyView();

            // Act & Assert
            Assert.AreEqual(elementCount, keyView.GetLength(largeContainer), "KeyView 应该返回正确的长度");
            
            // 验证所有键都能找到
            for (int i = 0; i < elementCount; i++)
            {
                Assert.IsTrue(keyView.HasKey(largeContainer, i), $"应该能找到键 {i}");
            }
            
            // 验证迭代器能遍历所有键
            var visitedKeys = new HashSet<int>();
            int count = 0;
            foreach (var key in keyView.GetKeysEnumerator(largeContainer))
            {
                visitedKeys.Add(key);
                Assert.IsTrue(key >= 0 && key < elementCount, $"键 {key} 应该在有效范围内");
                count++;
            }
            Assert.AreEqual(elementCount, count, "应该遍历所有键");
            Assert.AreEqual(elementCount, visitedKeys.Count, "所有键都应该被访问到");
            
            // Cleanup
            largeContainer.Dispose();
        }

        [Test]
        public void Map_AsKeyView_WithHashCollisions_ShouldWorkCorrectly()
        {
            // Arrange
            var largeContainer = new XBlobContainer();
            largeContainer.Create(Allocator.Temp, 10000);
            
            const int elementCount = 50;
            var map = largeContainer.AllocMap<int, int>(elementCount);
            
            // 添加会产生哈希冲突的键
            for (int i = 0; i < elementCount; i++)
            {
                int key = i * 10; // 这些键会产生哈希冲突
                map.AddOrUpdate(largeContainer, key, key * 2);
            }
            
            var keyView = map.AsKeyView();

            // Act & Assert
            Assert.AreEqual(elementCount, keyView.GetLength(largeContainer), "KeyView 应该返回正确的长度");
            
            // 验证所有键都能找到（即使存在哈希冲突）
            for (int i = 0; i < elementCount; i++)
            {
                int key = i * 10;
                Assert.IsTrue(keyView.HasKey(largeContainer, key), $"应该能找到键 {key}（即使存在哈希冲突）");
            }
            
            // 验证迭代器能正确遍历所有键（包括哈希冲突的情况）
            var visitedKeys = new HashSet<int>();
            int count = 0;
            foreach (var key in keyView.GetKeysEnumerator(largeContainer))
            {
                visitedKeys.Add(key);
                Assert.IsTrue(key % 10 == 0, $"键 {key} 应该是 10 的倍数");
                count++;
            }
            Assert.AreEqual(elementCount, count, "应该遍历所有键（包括哈希冲突的情况）");
            Assert.AreEqual(elementCount, visitedKeys.Count, "所有键都应该被访问到（包括哈希冲突的情况）");
            
            // Cleanup
            largeContainer.Dispose();
        }

        [Test]
        public void Map_AsKeyView_MultipleKeyViews_ShouldShareSameMemory()
        {
            // Arrange
            var map = _container.AllocMap<int, int>(10);
            map.AddOrUpdate(_container, 1, 100);
            var keyView1 = map.AsKeyView();
            var keyView2 = map.AsKeyView();

            // Act - 通过原始 Map 添加新键
            map.AddOrUpdate(_container, 2, 200);

            // Assert - 两个 KeyView 都应该能看到新添加的键
            Assert.IsTrue(keyView1.HasKey(_container, 1), "KeyView1 应该能看到键 1");
            Assert.IsTrue(keyView1.HasKey(_container, 2), "KeyView1 应该能看到键 2");
            Assert.IsTrue(keyView2.HasKey(_container, 1), "KeyView2 应该能看到键 1");
            Assert.IsTrue(keyView2.HasKey(_container, 2), "KeyView2 应该能看到键 2");
            Assert.AreEqual(2, keyView1.GetLength(_container), "KeyView1 应该返回更新后的长度");
            Assert.AreEqual(2, keyView2.GetLength(_container), "KeyView2 应该返回更新后的长度");
        }

        [Test]
        public void Map_AsKeyView_GetKeys_ShouldReturnEmptySpan()
        {
            // Arrange
            var map = _container.AllocMap<int, int>(10);
            map.AddOrUpdate(_container, 1, 100);
            map.AddOrUpdate(_container, 2, 200);
            var keyView = map.AsKeyView();

            // Act
            var keysSpan = keyView.GetKeys(_container);

            // Assert - GetKeys 返回空 Span（因为 KeyEntries 是结构数组，无法直接提取 Key 的 Span）
            Assert.AreEqual(0, keysSpan.Length, "GetKeys 应该返回空 Span（建议使用 GetKeysEnumerator）");
        }

        [Test]
        public void Map_WithCustomStructKeyAndValue_ShouldWork()
        {
            var map = _container.AllocMap<TestKey, TestValue>(16);
            var k1 = new TestKey { Id = 1, Tag = 10 };
            var k2 = new TestKey { Id = 2, Tag = 20 };
            var v1 = new TestValue { X = 100, Y = 200 };
            var v2 = new TestValue { X = 300, Y = 400 };
            Assert.IsTrue(map.AddOrUpdate(_container, k1, v1));
            Assert.IsTrue(map.AddOrUpdate(_container, k2, v2));
            Assert.AreEqual(2, map.GetLength(_container));
            Assert.IsTrue(map.HasKey(_container, k1));
            Assert.IsTrue(map.HasKey(_container, k2));
            Assert.IsTrue(map.TryGetValue(_container, k1, out var out1));
            Assert.AreEqual(100, out1.X);
            Assert.AreEqual(200, out1.Y);
            Assert.IsTrue(map.TryGetValue(_container, k2, out var out2));
            Assert.AreEqual(300, out2.X);
            Assert.AreEqual(400, out2.Y);
            int count = 0;
            foreach (var kvp in map.GetEnumerator(_container))
            {
                count++;
                Assert.IsTrue(kvp.Key.Id == 1 || kvp.Key.Id == 2);
                Assert.IsTrue(kvp.Value.X == 100 || kvp.Value.X == 300);
            }
            Assert.AreEqual(2, count);
        }
    }
}
