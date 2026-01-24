using System;
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
            Assert.Throws<IndexOutOfRangeException>(() => { var _ = set[_container, 1]; });
            Assert.Throws<IndexOutOfRangeException>(() => { var _ = set[_container, -1]; });
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
    }
}
