using System;
using NUnit.Framework;
using Unity.Collections;
using UnityEngine;

namespace XMFrame.XBlob.Tests
{
    [TestFixture]
    public class XBlobArrayTests
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
        public void AllocArray_WithValidCapacity_ShouldCreateArray()
        {
            // Act
            var array = _container.AllocArray<int>(10);

            // Assert
            Assert.IsNotNull(array);
            Assert.AreEqual(10, array.GetLength(_container));
        }

        [Test]
        public void AllocArray_WithZeroCapacity_ShouldCreateEmptyArray()
        {
            // Act
            var array = _container.AllocArray<int>(0);

            // Assert
            Assert.AreEqual(0, array.GetLength(_container));
        }

        [Test]
        public void AllocArray_WithNegativeCapacity_ShouldThrowException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _container.AllocArray<int>(-1));
        }

        [Test]
        public void ArrayIndexer_GetAndSet_ShouldWork()
        {
            // Arrange
            var array = _container.AllocArray<int>(5);

            // Act
            array[_container, 0] = 10;
            array[_container, 1] = 20;
            array[_container, 2] = 30;

            // Assert
            Assert.AreEqual(10, array[_container, 0]);
            Assert.AreEqual(20, array[_container, 1]);
            Assert.AreEqual(30, array[_container, 2]);
        }

        [Test]
        public void ArrayIndexer_OutOfRange_ShouldThrowException()
        {
            // Arrange
            var array = _container.AllocArray<int>(5);

            // Act & Assert
            Assert.Throws<IndexOutOfRangeException>(() => { var _ = array[_container, 5]; });
            Assert.Throws<IndexOutOfRangeException>(() => { var _ = array[_container, -1]; });
        }

        [Test]
        public void Array_GetEnumerator_ShouldIterateAllElements()
        {
            // Arrange
            var array = _container.AllocArray<int>(5);
            for (int i = 0; i < 5; i++)
            {
                array[_container, i] = i * 10;
            }

            // Act & Assert
            int index = 0;
            foreach (var value in array.GetEnumerator(_container))
            {
                Assert.AreEqual(index * 10, value);
                index++;
            }
            Assert.AreEqual(5, index);
        }

        [Test]
        public void Array_GetEnumeratorRef_ShouldIterateWithReferences()
        {
            // Arrange
            var array = _container.AllocArray<int>(3);
            array[_container, 0] = 1;
            array[_container, 1] = 2;
            array[_container, 2] = 3;

            // Act
            foreach (ref var value in array.GetEnumeratorRef(_container))
            {
                value *= 2;
            }

            // Assert
            Assert.AreEqual(2, array[_container, 0]);
            Assert.AreEqual(4, array[_container, 1]);
            Assert.AreEqual(6, array[_container, 2]);
        }

        [Test]
        public void AllocArray_DifferentTypes_ShouldWork()
        {
            // Act
            var intArray = _container.AllocArray<int>(5);
            var floatArray = _container.AllocArray<float>(3);
            var doubleArray = _container.AllocArray<double>(2);

            // Assert
            Assert.AreEqual(5, intArray.GetLength(_container));
            Assert.AreEqual(3, floatArray.GetLength(_container));
            Assert.AreEqual(2, doubleArray.GetLength(_container));
        }

        [Test]
        public void AllocArray_MultipleArrays_ShouldBeIndependent()
        {
            // Arrange
            var array1 = _container.AllocArray<int>(3);
            var array2 = _container.AllocArray<int>(3);

            // Act
            array1[_container, 0] = 100;
            array2[_container, 0] = 200;

            // Assert
            Assert.AreEqual(100, array1[_container, 0]);
            Assert.AreEqual(200, array2[_container, 0]);
        }
    }
}
