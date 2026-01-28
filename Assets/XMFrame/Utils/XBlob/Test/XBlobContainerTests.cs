using System;
using NUnit.Framework;
using Unity.Collections;
using UnityEngine;

namespace XM.Utils.Tests
{
    [TestFixture]
    public class XBlobContainerTests
    {
        private XBlobContainer _container;

        [SetUp]
        public void SetUp()
        {
            _container = new XBlobContainer();
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
        public void Create_WithValidCapacity_ShouldInitialize()
        {
            // Arrange & Act
            _container.Create(Allocator.Temp, 1024);

            // Assert
            Assert.IsTrue(_container.IsValid);
        }

        [Test]
        public void Create_WithZeroCapacity_ShouldThrowException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _container.Create(Allocator.Temp, 0));
        }

        [Test]
        public void Create_WithNegativeCapacity_ShouldThrowException()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _container.Create(Allocator.Temp, -1));
        }

        [Test]
        public void Create_WhenAlreadyCreated_ShouldThrowException()
        {
            // Arrange
            _container.Create(Allocator.Temp, 512);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _container.Create(Allocator.Temp, 1024));
        }

        [Test]
        public void Get_WithValidOffset_ShouldReturnValue()
        {
            // Arrange
            _container.Create(Allocator.Temp, 1024);
            var ptr = _container.Alloc<int>();
            ref int valueRef = ref _container.GetRef<int>(ptr.Offset);
            valueRef = 42;

            // Act
            int value = ptr.Get(_container);

            // Assert
            Assert.AreEqual(42, value);
        }

        [Test]
        public void Get_WhenContainerInvalid_ShouldThrowException()
        {
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _container.Get<int>(0));
        }

        [Test]
        public void GetRef_WithValidOffset_ShouldReturnReference()
        {
            // Arrange
            _container.Create(Allocator.Temp, 1024);
            var ptr = _container.Alloc<int>();

            // Act
            ref int valueRef = ref _container.GetRef<int>(ptr.Offset);
            valueRef = 100;

            // Assert
            Assert.AreEqual(100, ptr.Get(_container));
        }

        [Test]
        public void Alloc_ShouldAllocateMemory()
        {
            // Arrange
            _container.Create(Allocator.Temp, 1024);

            // Act
            var intPtr = _container.Alloc<int>();
            var floatPtr = _container.Alloc<float>();

            // Assert - 返回 XBlobPtr，默认值为 0
            Assert.AreEqual(0, intPtr.Get(_container));
            Assert.AreEqual(0f, floatPtr.Get(_container));
        }

        [Test]
        public void Reserve_WithLargerCapacity_ShouldExpand()
        {
            // Arrange
            _container.Create(Allocator.Temp, 64);

            // Act
            _container.Reserve(256);

            // Assert
            Assert.IsTrue(_container.IsValid);
        }

        [Test]
        public void Reserve_WithSmallerCapacity_ShouldNotShrink()
        {
            // Arrange
            _container.Create(Allocator.Temp, 256);

            // Act
            _container.Reserve(128);

            // Assert
            Assert.IsTrue(_container.IsValid);
        }

        [Test]
        public void Reserve_WhenContainerInvalid_ShouldThrowException()
        {
            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _container.Reserve(256));
        }

        [Test]
        public void Dispose_ShouldReleaseResources()
        {
            // Arrange
            _container.Create(Allocator.Temp, 1024);

            // Act
            _container.Dispose();

            // Assert
            Assert.IsFalse(_container.IsValid);
        }

        [Test]
        public void Dispose_WhenAlreadyDisposed_ShouldNotThrow()
        {
            // Arrange
            _container.Create(Allocator.Temp, 1024);
            _container.Dispose();

            // Act & Assert
            Assert.DoesNotThrow(() => _container.Dispose());
        }

        [Test]
        public void Alloc_MultipleTypes_ShouldAllocateCorrectly()
        {
            // Arrange
            _container.Create(Allocator.Temp, 1024);

            // Act - Alloc 返回 XBlobPtr<T>
            var intPtr = _container.Alloc<int>();
            var floatPtr = _container.Alloc<float>();
            var doublePtr = _container.Alloc<double>();

            // 通过 GetRef 设值，通过 Ptr.Get 或 Get 读值验证
            ref int intRef = ref _container.GetRef<int>(intPtr.Offset);
            ref float floatRef = ref _container.GetRef<float>(floatPtr.Offset);
            ref double doubleRef = ref _container.GetRef<double>(doublePtr.Offset);
            intRef = 10;
            floatRef = 3.14f;
            doubleRef = 2.718;

            Assert.AreEqual(10, intPtr.Get(_container));
            Assert.AreEqual(3.14f, floatPtr.Get(_container), 0.001f);
            Assert.AreEqual(2.718, doublePtr.Get(_container), 0.001);
        }

        [Test]
        public void Alloc_WithCustomStruct_ShouldWork()
        {
            _container.Create(Allocator.Temp, 1024);
            var ptr = _container.Alloc<TestBlobStruct>();
            ref var r = ref _container.GetRef<TestBlobStruct>(ptr.Offset);
            r = new TestBlobStruct { A = 11, B = 22 };
            var got = ptr.Get(_container);
            Assert.AreEqual(11, got.A);
            Assert.AreEqual(22, got.B);
        }

        [Test]
        public void Ptr_Get_WithCustomStruct_ShouldReturnSetValue()
        {
            _container.Create(Allocator.Temp, 1024);
            var ptr = _container.Alloc<TestBlobStruct>();
            ref var r = ref _container.GetRef<TestBlobStruct>(ptr.Offset);
            r = new TestBlobStruct { A = 100, B = 200 };
            Assert.AreEqual(100, ptr.Get(_container).A);
            Assert.AreEqual(200, ptr.Get(_container).B);
        }
    }
}
