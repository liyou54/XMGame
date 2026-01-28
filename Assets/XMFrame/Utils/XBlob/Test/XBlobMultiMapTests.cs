using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Collections;
using UnityEngine;

namespace XM.Utils.Tests
{
    [TestFixture]
    public class XBlobMultiMapTests
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
        public void AllocMultiMap_WithValidCapacity_ShouldCreateMultiMap()
        {
            // Act
            var multiMap = _container.AllocMultiMap<int, int>(10);

            // Assert
            Assert.IsNotNull(multiMap);
            Assert.AreEqual(0, multiMap.GetLength(_container));
        }

        [Test]
        public void AllocMultiMap_WithZeroCapacity_ShouldThrowException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _container.AllocMultiMap<int, int>(0));
        }

        [Test]
        public void MultiMap_Add_ShouldAddKeyValuePair()
        {
            // Arrange
            var multiMap = _container.AllocMultiMap<int, int>(10);

            // Act
            multiMap.Add(_container, 1, 100);

            // Assert
            Assert.AreEqual(1, multiMap.GetLength(_container));
            Assert.IsTrue(multiMap.ContainsKey(_container, 1));
        }

        [Test]
        public void MultiMap_Add_ShouldAllowMultipleValuesForSameKey()
        {
            // Arrange
            var multiMap = _container.AllocMultiMap<int, int>(10);

            // Act
            multiMap.Add(_container, 1, 100);
            multiMap.Add(_container, 1, 200);
            multiMap.Add(_container, 1, 300);

            // Assert
            Assert.AreEqual(3, multiMap.GetLength(_container));
            Assert.AreEqual(3, multiMap.GetValueCount(_container, 1));
        }

        [Test]
        public void MultiMap_ContainsKey_ShouldReturnCorrectResult()
        {
            // Arrange
            var multiMap = _container.AllocMultiMap<int, int>(10);
            multiMap.Add(_container, 1, 100);

            // Act & Assert
            Assert.IsTrue(multiMap.ContainsKey(_container, 1));
            Assert.IsFalse(multiMap.ContainsKey(_container, 999));
        }

        [Test]
        public void MultiMap_ContainsValue_ShouldReturnCorrectResult()
        {
            // Arrange
            var multiMap = _container.AllocMultiMap<int, int>(10);
            multiMap.Add(_container, 1, 100);
            multiMap.Add(_container, 1, 200);

            // Act & Assert
            Assert.IsTrue(multiMap.ContainsValue(_container, 1, 100));
            Assert.IsTrue(multiMap.ContainsValue(_container, 1, 200));
            Assert.IsFalse(multiMap.ContainsValue(_container, 1, 300));
            Assert.IsFalse(multiMap.ContainsValue(_container, 999, 100));
        }

        [Test]
        public void MultiMap_GetValueCount_ShouldReturnCorrectCount()
        {
            // Arrange
            var multiMap = _container.AllocMultiMap<int, int>(10);
            multiMap.Add(_container, 1, 100);
            multiMap.Add(_container, 1, 200);
            multiMap.Add(_container, 2, 300);

            // Act & Assert
            Assert.AreEqual(2, multiMap.GetValueCount(_container, 1));
            Assert.AreEqual(1, multiMap.GetValueCount(_container, 2));
            Assert.AreEqual(0, multiMap.GetValueCount(_container, 999));
        }

        [Test]
        public void MultiMap_GetKey_WithInvalidIndex_ShouldThrowException()
        {
            // Arrange
            var multiMap = _container.AllocMultiMap<int, int>(10);
            multiMap.Add(_container, 1, 100);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => { var _ = multiMap.GetKey(_container, -1); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { var _ = multiMap.GetKey(_container, 1); });
        }

        [Test]
        public void MultiMap_GetValue_WithInvalidIndex_ShouldThrowException()
        {
            // Arrange
            var multiMap = _container.AllocMultiMap<int, int>(10);
            multiMap.Add(_container, 1, 100);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => { var _ = multiMap.GetValue(_container, -1); });
            Assert.Throws<ArgumentOutOfRangeException>(() => { var _ = multiMap.GetValue(_container, 1); });
        }

        [Test]
        public void MultiMap_GetEnumerator_ShouldIterateAllEntries()
        {
            // Arrange
            var multiMap = _container.AllocMultiMap<int, int>(10);
            multiMap.Add(_container, 1, 100);
            multiMap.Add(_container, 1, 200);
            multiMap.Add(_container, 2, 300);

            // Act & Assert
            int count = 0;
            foreach (var kvp in multiMap.GetEnumerator(_container))
            {
                count++;
                Assert.IsTrue(kvp.Key == 1 || kvp.Key == 2);
                Assert.IsTrue(kvp.Value >= 100 && kvp.Value <= 300);
            }
            Assert.AreEqual(3, count);
        }

        [Test]
        public void MultiMap_GetEnumeratorRef_ShouldAllowModification()
        {
            // Arrange
            var multiMap = _container.AllocMultiMap<int, int>(10);
            multiMap.Add(_container, 1, 10);

            // Act
            foreach ( var entry in multiMap.GetEnumeratorRef(_container))
            {
                entry.Value = 100;
            }

            // Assert
            Assert.IsTrue(multiMap.ContainsValue(_container, 1, 100));
        }

        [Test]
        public void MultiMap_WhenFull_ShouldThrowException()
        {
            // Arrange
            var multiMap = _container.AllocMultiMap<int, int>(2);
            multiMap.Add(_container, 1, 10);
            multiMap.Add(_container, 2, 20);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => multiMap.Add(_container, 3, 30));
        }

        [Test]
        public void MultiMap_WhenFull_AddToExistingKey_ShouldThrowException()
        {
            // Arrange - MultiMap 满容后，即使添加到已存在的键也应该失败
            var multiMap = _container.AllocMultiMap<int, int>(2);
            multiMap.Add(_container, 1, 10);
            multiMap.Add(_container, 2, 20);
            Assert.AreEqual(2, multiMap.GetLength(_container), "MultiMap 应该已满");

            // Act & Assert - MultiMap 添加到已存在的键也会占用新的 slot
            var exception = Assert.Throws<InvalidOperationException>(() => 
                multiMap.Add(_container, 1, 100));
            Assert.That(exception.Message, Does.Contain("full").Or.Contain("满"));
        }

        [Test]
        public void MultiMap_FillToCapacity_ShouldWorkCorrectly()
        {
            // Arrange
            const int capacity = 5;
            var multiMap = _container.AllocMultiMap<int, int>(capacity);

            // Act - 填充到容量上限（不同的键）
            for (int i = 0; i < capacity; i++)
            {
                multiMap.Add(_container, i, i * 100);
            }

            // Assert
            Assert.AreEqual(capacity, multiMap.GetLength(_container), "MultiMap 应该已满");
            for (int i = 0; i < capacity; i++)
            {
                Assert.IsTrue(multiMap.ContainsKey(_container, i), $"应该包含键 {i}");
                Assert.AreEqual(1, multiMap.GetValueCount(_container, i), $"键 {i} 应该有 1 个值");
            }
        }

        [Test]
        public void MultiMap_FillWithMultipleValuesPerKey_ShouldRespectCapacity()
        {
            // Arrange - 测试同一个键添加多个值时的容量限制
            const int capacity = 4;
            var multiMap = _container.AllocMultiMap<int, int>(capacity);

            // Act - 给同一个键添加多个值
            multiMap.Add(_container, 1, 10);
            multiMap.Add(_container, 1, 20);
            multiMap.Add(_container, 1, 30);
            multiMap.Add(_container, 2, 100);

            // Assert - 应该使用了 4 个 slot
            Assert.AreEqual(4, multiMap.GetLength(_container), "应该使用了 4 个 slot");
            Assert.AreEqual(3, multiMap.GetValueCount(_container, 1), "键 1 应该有 3 个值");
            Assert.AreEqual(1, multiMap.GetValueCount(_container, 2), "键 2 应该有 1 个值");

            // Act & Assert - 第 5 个元素应该失败
            Assert.Throws<InvalidOperationException>(() => 
                multiMap.Add(_container, 3, 200));
        }

        [Test]
        public void MultiMap_ExceedCapacity_ShouldThrowInvalidOperationException()
        {
            // Arrange
            const int capacity = 3;
            var multiMap = _container.AllocMultiMap<int, int>(capacity);
            multiMap.Add(_container, 1, 10);
            multiMap.Add(_container, 2, 20);
            multiMap.Add(_container, 3, 30);

            // Act & Assert - 尝试添加超过容量的元素
            var exception = Assert.Throws<InvalidOperationException>(() => 
                multiMap.Add(_container, 4, 40));
            Assert.That(exception.Message, Does.Contain("full").Or.Contain("满"));
        }

        [Test]
        public void MultiMap_SingleCapacity_ShouldWorkAndThrowOnSecond()
        {
            // Arrange - 容量为 1 的极限情况
            var multiMap = _container.AllocMultiMap<int, int>(1);

            // Act & Assert - 第一个元素应该成功
            multiMap.Add(_container, 1, 10);
            Assert.AreEqual(1, multiMap.GetLength(_container));

            // 第二个元素应该失败（即使是给同一个键添加）
            Assert.Throws<InvalidOperationException>(() => 
                multiMap.Add(_container, 1, 20));
            Assert.Throws<InvalidOperationException>(() => 
                multiMap.Add(_container, 2, 20));
        }

        [Test]
        public void MultiMap_MultipleKeys_ShouldWorkIndependently()
        {
            // Arrange
            var multiMap = _container.AllocMultiMap<int, int>(10);

            // Act
            multiMap.Add(_container, 1, 100);
            multiMap.Add(_container, 1, 200);
            multiMap.Add(_container, 2, 300);
            multiMap.Add(_container, 2, 400);

            // Assert
            Assert.AreEqual(2, multiMap.GetValueCount(_container, 1));
            Assert.AreEqual(2, multiMap.GetValueCount(_container, 2));
            Assert.AreEqual(4, multiMap.GetLength(_container));
        }

        [Test]
        public void MultiMap_Add1000Elements_ShouldWorkCorrectly()
        {
            // Arrange - 创建足够大的容器来容纳1000个元素
            // MultiMap<int, int> 需要: Count(4) + BucketCount(4) + Buckets(1000*4) + Entries(1000*12) + Keys(1000*4) + Values(1000*4) ≈ 24008 字节
            var largeContainer = new XBlobContainer();
            largeContainer.Create(Allocator.Temp, 50000);
            
            const int elementCount = 1000;
            var multiMap = largeContainer.AllocMultiMap<int, int>(elementCount);

            // Act - 添加1000个元素
            for (int i = 0; i < elementCount; i++)
            {
                multiMap.Add(largeContainer, i, i * 10);
            }

            // Assert - 验证元素数量
            Assert.AreEqual(elementCount, multiMap.GetLength(largeContainer));

            // Assert - 验证所有键都能正确访问
            for (int i = 0; i < elementCount; i++)
            {
                Assert.IsTrue(multiMap.ContainsKey(largeContainer, i), $"键 {i} 应该存在");
                Assert.AreEqual(1, multiMap.GetValueCount(largeContainer, i), $"键 {i} 应该有一个值");
                Assert.IsTrue(multiMap.ContainsValue(largeContainer, i, i * 10), $"键 {i} 应该包含值 {i * 10}");
            }

            // Assert - 验证迭代器能遍历所有元素
            var visitedPairs = new HashSet<(int key, int value)>();
            int iteratedCount = 0;
            foreach (var kvp in multiMap.GetEnumerator(largeContainer))
            {
                visitedPairs.Add((kvp.Key, kvp.Value));
                Assert.AreEqual(kvp.Key * 10, kvp.Value, $"迭代器返回的键值对应该匹配");
                iteratedCount++;
            }
            Assert.AreEqual(elementCount, iteratedCount, "迭代器应该遍历所有元素");
            Assert.AreEqual(elementCount, visitedPairs.Count, "所有键值对都应该被访问到");
            
            // Cleanup
            largeContainer.Dispose();
        }

        [Test]
        public void MultiMap_HashCollision_ShouldHandleCorrectly()
        {
            // Arrange - 创建足够大的容器
            var largeContainer = new XBlobContainer();
            largeContainer.Create(Allocator.Temp, 10000);
            
            // 使用小的 bucketCount 来强制产生哈希冲突
            const int bucketCount = 10;
            const int keysCount = 50; // 50个不同的键
            const int valuesPerKey = 3; // 每个键3个值
            int expectedTotalCount = keysCount * valuesPerKey; // 总共150个元素
            // MultiMap 的容量需要至少等于要添加的元素总数
            var multiMap = largeContainer.AllocMultiMap<int, int>(expectedTotalCount);

            // Act - 添加会产生哈希冲突的元素
            // 使用会产生冲突的键（它们模 bucketCount 会相同）
            for (int i = 0; i < keysCount; i++)
            {
                int key = i * bucketCount; // 这些键会产生哈希冲突
                for (int j = 0; j < valuesPerKey; j++)
                {
                    int value = key * 10 + j;
                    multiMap.Add(largeContainer, key, value);
                }
            }

            // Assert - 验证所有元素都能正确访问（即使存在哈希冲突）
            Assert.AreEqual(expectedTotalCount, multiMap.GetLength(largeContainer));

            for (int i = 0; i < keysCount; i++)
            {
                int key = i * bucketCount;
                Assert.IsTrue(multiMap.ContainsKey(largeContainer, key), $"键 {key} 应该存在（即使存在哈希冲突）");
                Assert.AreEqual(valuesPerKey, multiMap.GetValueCount(largeContainer, key), $"键 {key} 应该有 {valuesPerKey} 个值");

                // 验证每个值都存在
                for (int j = 0; j < valuesPerKey; j++)
                {
                    int expectedValue = key * 10 + j;
                    Assert.IsTrue(multiMap.ContainsValue(largeContainer, key, expectedValue), 
                        $"键 {key} 应该包含值 {expectedValue}（即使存在哈希冲突）");
                }
            }

            // Assert - 验证迭代器能正确遍历所有元素（包括哈希冲突的情况）
            var visitedPairs = new HashSet<(int key, int value)>();
            int iteratedCount = 0;
            foreach (var kvp in multiMap.GetEnumerator(largeContainer))
            {
                visitedPairs.Add((kvp.Key, kvp.Value));
                // 验证值的格式：key * 10 + j，其中 j 在 [0, valuesPerKey) 范围内
                int key = kvp.Key;
                int value = kvp.Value;
                int j = value - key * 10;
                Assert.IsTrue(j >= 0 && j < valuesPerKey, 
                    $"值 {value} 对于键 {key} 应该是有效的（j={j}）");
                iteratedCount++;
            }
            Assert.AreEqual(expectedTotalCount, iteratedCount, 
                "迭代器应该遍历所有元素（包括哈希冲突的情况）");
            Assert.AreEqual(expectedTotalCount, visitedPairs.Count, 
                "所有键值对都应该被访问到（包括哈希冲突的情况）");
            
            // Cleanup
            largeContainer.Dispose();
        }

        [Test]
        public void MultiMap_GetKeysEnumerator_ShouldIterateUniqueKeys()
        {
            // Arrange
            var multiMap = _container.AllocMultiMap<int, int>(10);
            multiMap.Add(_container, 1, 100);
            multiMap.Add(_container, 1, 200);
            multiMap.Add(_container, 2, 300);
            multiMap.Add(_container, 2, 400);
            multiMap.Add(_container, 3, 500);

            // Act & Assert
            var visitedKeys = new HashSet<int>();
            int count = 0;
            foreach (var key in multiMap.GetKeysEnumerator(_container))
            {
                visitedKeys.Add(key);
                Assert.IsTrue(key >= 1 && key <= 3, $"键 {key} 应该在有效范围内");
                count++;
            }
            Assert.AreEqual(3, count, "应该遍历所有唯一的键");
            Assert.AreEqual(3, visitedKeys.Count, "所有唯一的键都应该被访问到");
            Assert.IsTrue(visitedKeys.Contains(1), "应该包含键 1");
            Assert.IsTrue(visitedKeys.Contains(2), "应该包含键 2");
            Assert.IsTrue(visitedKeys.Contains(3), "应该包含键 3");
        }

        [Test]
        public void MultiMap_GetValuesEnumerator_ShouldIterateAllValues()
        {
            // Arrange
            var multiMap = _container.AllocMultiMap<int, int>(10);
            multiMap.Add(_container, 1, 100);
            multiMap.Add(_container, 1, 200);
            multiMap.Add(_container, 2, 300);
            multiMap.Add(_container, 2, 400);
            multiMap.Add(_container, 3, 500);

            // Act & Assert
            var visitedValues = new HashSet<int>();
            int count = 0;
            foreach (var value in multiMap.GetValuesEnumerator(_container))
            {
                visitedValues.Add(value);
                Assert.IsTrue(value >= 100 && value <= 500, $"值 {value} 应该在有效范围内");
                count++;
            }
            Assert.AreEqual(5, count, "应该遍历所有值");
            Assert.AreEqual(5, visitedValues.Count, "所有值都应该被访问到");
            Assert.IsTrue(visitedValues.Contains(100), "应该包含值 100");
            Assert.IsTrue(visitedValues.Contains(200), "应该包含值 200");
            Assert.IsTrue(visitedValues.Contains(300), "应该包含值 300");
            Assert.IsTrue(visitedValues.Contains(400), "应该包含值 400");
            Assert.IsTrue(visitedValues.Contains(500), "应该包含值 500");
        }

        [Test]
        public void MultiMap_GetValuesPerKeyEnumerator_ShouldIterateAllValuesForKey()
        {
            // Arrange
            var multiMap = _container.AllocMultiMap<int, int>(10);
            multiMap.Add(_container, 1, 100);
            multiMap.Add(_container, 1, 200);
            multiMap.Add(_container, 1, 300);
            multiMap.Add(_container, 2, 400);
            multiMap.Add(_container, 2, 500);

            // Act & Assert - 测试键 1 的所有值
            var valuesForKey1 = new HashSet<int>();
            int count1 = 0;
            foreach (var value in multiMap.GetValuesPerKeyEnumerator(_container, 1))
            {
                valuesForKey1.Add(value);
                Assert.IsTrue(value >= 100 && value <= 300, $"键 1 的值 {value} 应该在有效范围内");
                count1++;
            }
            Assert.AreEqual(3, count1, "键 1 应该有 3 个值");
            Assert.AreEqual(3, valuesForKey1.Count, "键 1 的所有值都应该被访问到");
            Assert.IsTrue(valuesForKey1.Contains(100), "应该包含值 100");
            Assert.IsTrue(valuesForKey1.Contains(200), "应该包含值 200");
            Assert.IsTrue(valuesForKey1.Contains(300), "应该包含值 300");

            // Act & Assert - 测试键 2 的所有值
            var valuesForKey2 = new HashSet<int>();
            int count2 = 0;
            foreach (var value in multiMap.GetValuesPerKeyEnumerator(_container, 2))
            {
                valuesForKey2.Add(value);
                Assert.IsTrue(value >= 400 && value <= 500, $"键 2 的值 {value} 应该在有效范围内");
                count2++;
            }
            Assert.AreEqual(2, count2, "键 2 应该有 2 个值");
            Assert.AreEqual(2, valuesForKey2.Count, "键 2 的所有值都应该被访问到");
            Assert.IsTrue(valuesForKey2.Contains(400), "应该包含值 400");
            Assert.IsTrue(valuesForKey2.Contains(500), "应该包含值 500");

            // Act & Assert - 测试不存在的键
            int count3 = 0;
            foreach (var value in multiMap.GetValuesPerKeyEnumerator(_container, 999))
            {
                count3++;
            }
            Assert.AreEqual(0, count3, "不存在的键不应该返回任何值");
        }

        [Test]
        public void MultiMap_GetKeysEnumerator_WithEmptyMap_ShouldReturnNoKeys()
        {
            // Arrange
            var multiMap = _container.AllocMultiMap<int, int>(10);

            // Act & Assert
            int count = 0;
            foreach (var key in multiMap.GetKeysEnumerator(_container))
            {
                count++;
            }
            Assert.AreEqual(0, count, "空 MultiMap 不应该返回任何键");
        }

        [Test]
        public void MultiMap_GetValuesEnumerator_WithEmptyMap_ShouldReturnNoValues()
        {
            // Arrange
            var multiMap = _container.AllocMultiMap<int, int>(10);

            // Act & Assert
            int count = 0;
            foreach (var value in multiMap.GetValuesEnumerator(_container))
            {
                count++;
            }
            Assert.AreEqual(0, count, "空 MultiMap 不应该返回任何值");
        }

        [Test]
        public void MultiMap_GetValuesPerKeyEnumerator_WithSingleValue_ShouldReturnOneValue()
        {
            // Arrange
            var multiMap = _container.AllocMultiMap<int, int>(10);
            multiMap.Add(_container, 1, 100);

            // Act & Assert
            int count = 0;
            int value = 0;
            foreach (var v in multiMap.GetValuesPerKeyEnumerator(_container, 1))
            {
                value = v;
                count++;
            }
            Assert.AreEqual(1, count, "应该返回一个值");
            Assert.AreEqual(100, value, "值应该是 100");
        }

        [Test]
        public void MultiMap_WithCustomStructKeyAndValue_ShouldWork()
        {
            var multiMap = _container.AllocMultiMap<TestKey, TestValue>(16);
            var k1 = new TestKey { Id = 1, Tag = 10 };
            var k2 = new TestKey { Id = 2, Tag = 20 };
            multiMap.Add(_container, k1, new TestValue { X = 100, Y = 200 });
            multiMap.Add(_container, k1, new TestValue { X = 101, Y = 201 });
            multiMap.Add(_container, k2, new TestValue { X = 300, Y = 400 });
            Assert.AreEqual(3, multiMap.GetLength(_container));
            Assert.IsTrue(multiMap.ContainsKey(_container, k1));
            Assert.IsTrue(multiMap.ContainsKey(_container, k2));
            Assert.AreEqual(2, multiMap.GetValueCount(_container, k1));
            Assert.AreEqual(1, multiMap.GetValueCount(_container, k2));
            int count = 0;
            foreach (var kvp in multiMap.GetEnumerator(_container))
            {
                count++;
                Assert.IsTrue(kvp.Key.Id == 1 || kvp.Key.Id == 2);
                Assert.IsTrue(kvp.Value.X >= 100 && kvp.Value.X <= 301);
            }
            Assert.AreEqual(3, count);
        }
    }
}
