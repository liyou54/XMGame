using System;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.Collections;
using UnityEngine;

namespace XMFrame.XBlob.Tests
{
    [TestFixture]
    public class XBlobSetTests
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
        public void AllocSet_WithValidCapacity_ShouldCreateSet()
        {
            // Act
            var set = _container.AllocSet<int>(10);

            // Assert
            Assert.IsNotNull(set);
            Assert.AreEqual(0, set.GetLength(_container));
        }

        [Test]
        public void AllocSet_WithZeroCapacity_ShouldThrowException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _container.AllocSet<int>(0));
        }

        [Test]
        public void Set_Add_ShouldAddNewValue()
        {
            // Arrange
            var set = _container.AllocSet<int>(10);

            // Act
            bool added = set.Add(_container, 42);

            // Assert
            Assert.IsTrue(added);
            Assert.AreEqual(1, set.GetLength(_container));
            Assert.IsTrue(set.Contains(_container, 42));
        }

        [Test]
        public void Set_Add_ShouldNotAddDuplicateValue()
        {
            // Arrange
            var set = _container.AllocSet<int>(10);
            set.Add(_container, 42);

            // Act
            bool added = set.Add(_container, 42);

            // Assert
            Assert.IsFalse(added);
            Assert.AreEqual(1, set.GetLength(_container));
        }

        [Test]
        public void Set_Contains_ShouldReturnCorrectResult()
        {
            // Arrange
            var set = _container.AllocSet<int>(10);
            set.Add(_container, 42);
            set.Add(_container, 100);

            // Act & Assert
            Assert.IsTrue(set.Contains(_container, 42));
            Assert.IsTrue(set.Contains(_container, 100));
            Assert.IsFalse(set.Contains(_container, 999));
        }

        [Test]
        public void Set_Indexer_ShouldReturnValue()
        {
            // Arrange
            var set = _container.AllocSet<int>(10);
            set.Add(_container, 42);
            set.Add(_container, 100);

            // Act & Assert
            Assert.AreEqual(42, set[_container, 0]);
            Assert.AreEqual(100, set[_container, 1]);
        }

        [Test]
        public void Set_Indexer_OutOfRange_ShouldThrowException()
        {
            // Arrange
            var set = _container.AllocSet<int>(10);
            set.Add(_container, 42);

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => { var _ = set[_container, 1]; });
            Assert.Throws<ArgumentOutOfRangeException>(() => { var _ = set[_container, -1]; });
        }

        [Test]
        public void Set_GetEnumerator_ShouldIterateAllValues()
        {
            // Arrange
            var set = _container.AllocSet<int>(10);
            set.Add(_container, 10);
            set.Add(_container, 20);
            set.Add(_container, 30);

            // Act & Assert
            int count = 0;
            foreach (var value in set.GetEnumerator(_container))
            {
                count++;
                Assert.IsTrue(value == 10 || value == 20 || value == 30);
            }
            Assert.AreEqual(3, count);
        }

        [Test]
        public void Set_WhenFull_ShouldThrowException()
        {
            // Arrange
            var set = _container.AllocSet<int>(2);
            set.Add(_container, 1);
            set.Add(_container, 2);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => set.Add(_container, 3));
        }

        [Test]
        public void Set_WhenFull_AddDuplicate_ShouldNotThrowException()
        {
            // Arrange - 创建容量为 2 的 Set 并填满
            var set = _container.AllocSet<int>(2);
            set.Add(_container, 1);
            set.Add(_container, 2);
            Assert.AreEqual(2, set.GetLength(_container), "Set 应该已满");

            // Act - 添加已存在的值不应触发满容异常
            bool added1 = set.Add(_container, 1);
            bool added2 = set.Add(_container, 2);

            // Assert
            Assert.IsFalse(added1, "添加重复值应该返回 false");
            Assert.IsFalse(added2, "添加重复值应该返回 false");
            Assert.AreEqual(2, set.GetLength(_container), "Set 长度不应变化");
        }

        [Test]
        public void Set_FillToCapacity_ShouldWorkCorrectly()
        {
            // Arrange
            const int capacity = 5;
            var set = _container.AllocSet<int>(capacity);

            // Act - 填充到容量上限
            for (int i = 0; i < capacity; i++)
            {
                bool added = set.Add(_container, i * 10);
                Assert.IsTrue(added, $"添加第 {i} 个元素应该成功");
            }

            // Assert
            Assert.AreEqual(capacity, set.GetLength(_container), "Set 应该已满");
            for (int i = 0; i < capacity; i++)
            {
                Assert.IsTrue(set.Contains(_container, i * 10), $"应该包含值 {i * 10}");
            }
        }

        [Test]
        public void Set_ExceedCapacity_ShouldThrowInvalidOperationException()
        {
            // Arrange
            const int capacity = 3;
            var set = _container.AllocSet<int>(capacity);
            for (int i = 0; i < capacity; i++)
            {
                set.Add(_container, i * 100);
            }

            // Act & Assert - 尝试添加超过容量的元素
            var exception = Assert.Throws<InvalidOperationException>(() => 
                set.Add(_container, capacity * 100));
            Assert.That(exception.Message, Does.Contain("full").Or.Contain("满"));
        }

        [Test]
        public void Set_SingleCapacity_ShouldWorkAndThrowOnSecond()
        {
            // Arrange - 容量为 1 的极限情况
            var set = _container.AllocSet<int>(1);

            // Act & Assert - 第一个元素应该成功
            bool added = set.Add(_container, 100);
            Assert.IsTrue(added, "第一个元素应该添加成功");
            Assert.AreEqual(1, set.GetLength(_container));

            // 第二个元素应该失败
            Assert.Throws<InvalidOperationException>(() => 
                set.Add(_container, 200));
        }

        [Test]
        public void Set_WithIntValues_ShouldWork()
        {
            // Arrange
            var set = _container.AllocSet<int>(10);

            // Act
            set.Add(_container, 1);
            set.Add(_container, 2);

            // Assert
            Assert.IsTrue(set.Contains(_container, 1));
            Assert.IsTrue(set.Contains(_container, 2));
            Assert.AreEqual(2, set.GetLength(_container));
        }

        [Test]
        public void Set_GetEnumerator_WithEmptySet_ShouldReturnNoValues()
        {
            var set = _container.AllocSet<int>(10);
            int count = 0;
            foreach (var _ in set.GetEnumerator(_container))
                count++;
            Assert.AreEqual(0, count);
        }

        [Test]
        public void Set_Indexer_InsertionOrder_MatchesGetEnumerator_Count()
        {
            var set = _container.AllocSet<int>(10);
            set.Add(_container, 5);
            set.Add(_container, 3);
            set.Add(_container, 7);
            int indexerCount = set.GetLength(_container);
            int enumCount = 0;
            foreach (var _ in set.GetEnumerator(_container))
                enumCount++;
            Assert.AreEqual(indexerCount, enumCount, "迭代数量应与 Count 一致");
            Assert.AreEqual(3, indexerCount);
        }

        [Test]
        public void Set_HashCollision_ShouldHandleCorrectly()
        {
            var largeContainer = new XBlobContainer();
            largeContainer.Create(Allocator.Temp, 20000);
            const int bucketCount = 10;
            const int elementCount = 100;
            var set = largeContainer.AllocSet<int>(elementCount);

            for (int i = 0; i < elementCount; i++)
            {
                int value = i * bucketCount;
                bool added = set.Add(largeContainer, value);
                Assert.IsTrue(added, $"值 {value} 应成功添加");
            }
            Assert.AreEqual(elementCount, set.GetLength(largeContainer));

            for (int i = 0; i < elementCount; i++)
            {
                int value = i * bucketCount;
                Assert.IsTrue(set.Contains(largeContainer, value), $"应包含 {value}");
                Assert.AreEqual(value, set[largeContainer, i], $"索引 {i} 应为插入顺序的值");
            }

            var visited = new System.Collections.Generic.HashSet<int>();
            foreach (var v in set.GetEnumerator(largeContainer))
            {
                Assert.IsTrue(visited.Add(v), $"迭代不应重复 {v}");
            }
            Assert.AreEqual(elementCount, visited.Count);
            largeContainer.Dispose();
        }

        [Test]
        public void Set_Add1000Elements_ShouldWorkCorrectly()
        {
            var largeContainer = new XBlobContainer();
            largeContainer.Create(Allocator.Temp, 50000);
            const int n = 1000;
            var set = largeContainer.AllocSet<int>(n);

            for (int i = 0; i < n; i++)
            {
                bool added = set.Add(largeContainer, i);
                Assert.IsTrue(added, $"元素 {i} 应被添加");
            }
            Assert.AreEqual(n, set.GetLength(largeContainer));

            for (int i = 0; i < n; i++)
            {
                Assert.IsTrue(set.Contains(largeContainer, i), $"应包含 {i}");
                Assert.AreEqual(i, set[largeContainer, i], $"set[{i}] 应按插入顺序为 {i}");
            }

            var iterated = new HashSet<int>();
            foreach (var v in set.GetEnumerator(largeContainer))
                iterated.Add(v);
            Assert.AreEqual(n, iterated.Count, "迭代应覆盖全部 1000 个元素");
            for (int i = 0; i < n; i++)
                Assert.IsTrue(iterated.Contains(i), $"迭代结果应包含 {i}");
            largeContainer.Dispose();
        }

        [Test]
        public void Set_WithCustomStruct_ShouldWork()
        {
            var set = _container.AllocSet<TestKey>(16);
            var k1 = new TestKey { Id = 1, Tag = 10 };
            var k2 = new TestKey { Id = 2, Tag = 20 };
            Assert.IsTrue(set.Add(_container, k1));
            Assert.IsTrue(set.Add(_container, k2));
            Assert.IsFalse(set.Add(_container, k1));
            Assert.AreEqual(2, set.GetLength(_container));
            Assert.IsTrue(set.Contains(_container, k1));
            Assert.IsTrue(set.Contains(_container, k2));
            Assert.IsFalse(set.Contains(_container, new TestKey { Id = 99, Tag = 0 }));
            int n = 0;
            foreach (var k in set.GetEnumerator(_container))
            {
                Assert.IsTrue(k.Id == 1 || k.Id == 2);
                n++;
            }
            Assert.AreEqual(2, n);
        }
    }
}
