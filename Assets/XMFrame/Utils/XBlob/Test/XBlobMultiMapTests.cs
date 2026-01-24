using System;
using NUnit.Framework;
using Unity.Collections;
using UnityEngine;

namespace XMFrame.XBlob.Tests
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
            foreach (ref var entry in multiMap.GetEnumeratorRef(_container))
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
    }
}
