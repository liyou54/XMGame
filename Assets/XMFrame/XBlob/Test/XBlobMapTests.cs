using System;
using System.Collections.Generic;
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
            foreach (ref var entry in map.GetEnumeratorRef(_container))
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
    }
}
