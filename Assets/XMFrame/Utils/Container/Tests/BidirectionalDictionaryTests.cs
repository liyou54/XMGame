using System;
using System.Linq;
using NUnit.Framework;

namespace XM.Utils.Tests
{
    /// <summary>
    /// BidirectionalDictionary 双向字典测试
    /// 目标：98%+ 分支覆盖率
    /// </summary>
    [TestFixture]
    [Category("Pure")]
    public class BidirectionalDictionaryTests
    {
        #region 构造和基础操作
        
        [Test]
        public void Constructor_Default_CreatesEmpty()
        {
            // Act
            var dict = new BidirectionalDictionary<int, string>();
            
            // Assert
            Assert.AreEqual(0, dict.Count);
            Assert.IsEmpty(dict.Keys);
            Assert.IsEmpty(dict.Values);
        }
        
        [Test]
        public void Add_NewPair_AddsSuccessfully()
        {
            // Arrange
            var dict = new BidirectionalDictionary<int, string>();
            
            // Act
            dict.Add(1, "one");
            
            // Assert
            Assert.AreEqual(1, dict.Count);
            Assert.AreEqual("one", dict.GetByKey(1));
            Assert.AreEqual(1, dict.GetByValue("one"));
        }
        
        [Test]
        public void Add_ExistingKey_ThrowsException()
        {
            // Arrange
            var dict = new BidirectionalDictionary<int, string>();
            dict.Add(1, "one");
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => dict.Add(1, "uno"));
        }
        
        [Test]
        public void Add_ExistingValue_ThrowsException()
        {
            // Arrange
            var dict = new BidirectionalDictionary<int, string>();
            dict.Add(1, "one");
            
            // Act & Assert
            Assert.Throws<ArgumentException>(() => dict.Add(100, "one"));
        }
        
        #endregion
        
        #region AddOrUpdate 四种场景
        
        [Test]
        public void AddOrUpdate_NewPair_Adds()
        {
            // Arrange
            var dict = new BidirectionalDictionary<int, string>();
            
            // Act
            dict.AddOrUpdate(1, "one");
            
            // Assert
            Assert.AreEqual(1, dict.Count);
            Assert.AreEqual("one", dict.GetByKey(1));
        }
        
        [Test]
        public void AddOrUpdate_ExistingKeyValuePair_NoChange()
        {
            // Arrange
            var dict = new BidirectionalDictionary<int, string>();
            dict.Add(1, "one");
            
            // Act
            dict.AddOrUpdate(1, "one");
            
            // Assert
            Assert.AreEqual(1, dict.Count);
            Assert.AreEqual("one", dict.GetByKey(1));
        }
        
        [Test]
        public void AddOrUpdate_ExistingKeyDifferentValue_Updates()
        {
            // Arrange
            var dict = new BidirectionalDictionary<int, string>();
            dict.Add(1, "one");
            
            // Act
            dict.AddOrUpdate(1, "uno");
            
            // Assert
            Assert.AreEqual(1, dict.Count);
            Assert.AreEqual("uno", dict.GetByKey(1));
            Assert.AreEqual(1, dict.GetByValue("uno"));
            Assert.AreEqual(default(int), dict.GetByValue("one")); // 旧值应该被移除
        }
        
        [Test]
        public void AddOrUpdate_DifferentKeyExistingValue_Updates()
        {
            // Arrange
            var dict = new BidirectionalDictionary<int, string>();
            dict.Add(1, "one");
            
            // Act
            dict.AddOrUpdate(100, "one");
            
            // Assert
            Assert.AreEqual(1, dict.Count);
            Assert.AreEqual("one", dict.GetByKey(100));
            Assert.AreEqual(100, dict.GetByValue("one"));
            Assert.AreEqual(default(string), dict.GetByKey(1)); // 旧键应该被移除
        }
        
        [Test]
        public void AddOrUpdate_ConflictingKeysAndValues_RemovesOldEntries()
        {
            // Arrange
            var dict = new BidirectionalDictionary<int, string>();
            dict.Add(1, "one");
            dict.Add(2, "two");
            
            // Act - key=1已存在映射"one"，value="two"已存在映射2
            dict.AddOrUpdate(1, "two");
            
            // Assert
            Assert.AreEqual(1, dict.Count);
            Assert.AreEqual("two", dict.GetByKey(1));
            Assert.AreEqual(1, dict.GetByValue("two"));
            Assert.IsFalse(dict.ContainsKey(2));
        }
        
        #endregion
        
        #region 查询操作
        
        [Test]
        public void GetByKey_ExistingKey_ReturnsValue()
        {
            // Arrange
            var dict = new BidirectionalDictionary<int, string>();
            dict.Add(1, "one");
            
            // Act
            var result = dict.GetByKey(1);
            
            // Assert
            Assert.AreEqual("one", result);
        }
        
        [Test]
        public void GetByKey_NonExistingKey_ReturnsDefault()
        {
            // Arrange
            var dict = new BidirectionalDictionary<int, string>();
            
            // Act
            var result = dict.GetByKey(999);
            
            // Assert
            Assert.AreEqual(default(string), result);
        }
        
        [Test]
        public void GetByValue_ExistingValue_ReturnsKey()
        {
            // Arrange
            var dict = new BidirectionalDictionary<int, string>();
            dict.Add(1, "one");
            
            // Act
            var result = dict.GetByValue("one");
            
            // Assert
            Assert.AreEqual(1, result);
        }
        
        [Test]
        public void GetByValue_NonExistingValue_ReturnsDefault()
        {
            // Arrange
            var dict = new BidirectionalDictionary<int, string>();
            
            // Act
            var result = dict.GetByValue("nonexistent");
            
            // Assert
            Assert.AreEqual(default(int), result);
        }
        
        [Test]
        public void TryGetValueByKey_Existing_ReturnsTrue()
        {
            // Arrange
            var dict = new BidirectionalDictionary<int, string>();
            dict.Add(1, "one");
            
            // Act
            var found = dict.TryGetValueByKey(1, out var value);
            
            // Assert
            Assert.IsTrue(found);
            Assert.AreEqual("one", value);
        }
        
        [Test]
        public void TryGetValueByKey_NonExisting_ReturnsFalse()
        {
            // Arrange
            var dict = new BidirectionalDictionary<int, string>();
            
            // Act
            var found = dict.TryGetValueByKey(999, out var value);
            
            // Assert
            Assert.IsFalse(found);
            Assert.AreEqual(default(string), value);
        }
        
        [Test]
        public void ContainsKey_Existing_ReturnsTrue()
        {
            // Arrange
            var dict = new BidirectionalDictionary<int, string>();
            dict.Add(1, "one");
            
            // Assert
            Assert.IsTrue(dict.ContainsKey(1));
        }
        
        [Test]
        public void ContainsValue_Existing_ReturnsTrue()
        {
            // Arrange
            var dict = new BidirectionalDictionary<int, string>();
            dict.Add(1, "one");
            
            // Assert
            Assert.IsTrue(dict.ContainsValue("one"));
        }
        
        #endregion
        
        #region 删除操作
        
        [Test]
        public void RemoveByKey_Existing_RemovesBothMappings()
        {
            // Arrange
            var dict = new BidirectionalDictionary<int, string>();
            dict.Add(1, "one");
            
            // Act
            var removed = dict.RemoveByKey(1);
            
            // Assert
            Assert.IsTrue(removed);
            Assert.AreEqual(0, dict.Count);
            Assert.IsFalse(dict.ContainsKey(1));
            Assert.IsFalse(dict.ContainsValue("one"));
        }
        
        [Test]
        public void RemoveByKey_NonExisting_ReturnsFalse()
        {
            // Arrange
            var dict = new BidirectionalDictionary<int, string>();
            
            // Act
            var removed = dict.RemoveByKey(999);
            
            // Assert
            Assert.IsFalse(removed);
        }
        
        [Test]
        public void RemoveByValue_Existing_RemovesBothMappings()
        {
            // Arrange
            var dict = new BidirectionalDictionary<int, string>();
            dict.Add(1, "one");
            
            // Act
            var removed = dict.RemoveByValue("one");
            
            // Assert
            Assert.IsTrue(removed);
            Assert.AreEqual(0, dict.Count);
            Assert.IsFalse(dict.ContainsKey(1));
            Assert.IsFalse(dict.ContainsValue("one"));
        }
        
        [Test]
        public void Clear_RemovesAll()
        {
            // Arrange
            var dict = new BidirectionalDictionary<int, string>();
            dict.Add(1, "one");
            dict.Add(2, "two");
            dict.Add(3, "three");
            
            // Act
            dict.Clear();
            
            // Assert
            Assert.AreEqual(0, dict.Count);
            Assert.IsEmpty(dict.Keys);
            Assert.IsEmpty(dict.Values);
        }
        
        #endregion
        
        #region 双向一致性测试
        
        [Test]
        public void BidirectionalConsistency_AfterMultipleOperations()
        {
            // Arrange
            var dict = new BidirectionalDictionary<int, string>();
            dict.Add(1, "one");
            dict.Add(2, "two");
            dict.Add(3, "three");
            
            dict.RemoveByKey(2);
            dict.AddOrUpdate(4, "four");
            dict.AddOrUpdate(1, "uno");
            
            // Assert - 手动验证双向一致性
            foreach (var key in dict.Keys)
            {
                var value = dict.GetByKey(key);
                var reversedKey = dict.GetByValue(value);
                Assert.AreEqual(key, reversedKey, "键→值→键应该保持一致");
            }
            
            foreach (var value in dict.Values)
            {
                var key = dict.GetByValue(value);
                var reversedValue = dict.GetByKey(key);
                Assert.AreEqual(value, reversedValue, "值→键→值应该保持一致");
            }
        }
        
        #endregion
        
        #region 迭代器测试
        
        [Test]
        public void Keys_ReturnsAllKeys()
        {
            // Arrange
            var dict = new BidirectionalDictionary<int, string>();
            dict.Add(1, "one");
            dict.Add(2, "two");
            
            // Act
            var keys = dict.Keys.ToList();
            
            // Assert
            Assert.AreEqual(2, keys.Count);
            Assert.Contains(1, keys);
            Assert.Contains(2, keys);
        }
        
        [Test]
        public void Values_ReturnsAllValues()
        {
            // Arrange
            var dict = new BidirectionalDictionary<int, string>();
            dict.Add(1, "one");
            dict.Add(2, "two");
            
            // Act
            var values = dict.Values.ToList();
            
            // Assert
            Assert.AreEqual(2, values.Count);
            Assert.Contains("one", values);
            Assert.Contains("two", values);
        }
        
        [Test]
        public void GetEnumerator_IteratesKeyValuePairs()
        {
            // Arrange
            var dict = new BidirectionalDictionary<int, string>();
            dict.Add(1, "one");
            dict.Add(2, "two");
            
            // Act
            var pairs = dict.ToList();
            
            // Assert
            Assert.AreEqual(2, pairs.Count);
            Assert.IsTrue(pairs.Any(p => p.Key == 1 && p.Value == "one"));
            Assert.IsTrue(pairs.Any(p => p.Key == 2 && p.Value == "two"));
        }
        
        [Test]
        public void EmptyDictionary_Iteration_DoesNotThrow()
        {
            // Arrange
            var dict = new BidirectionalDictionary<int, string>();
            
            // Act & Assert
            Assert.DoesNotThrow(() =>
            {
                var _ = dict.Keys.ToList();
                var __ = dict.Values.ToList();
                var ___ = dict.ToList();
            });
        }
        
        #endregion
        
        #region 边界和性能测试
        
        [Test]
        [Category("Performance")]
        [Timeout(3000)]
        public void LargeDataSet_1000Pairs_PerformanceTest()
        {
            // Arrange
            var dict = new BidirectionalDictionary<int, string>();
            
            // Act - 添加1000对
            for (int i = 0; i < 1000; i++)
            {
                dict.Add(i, $"value{i}");
            }
            
            // Assert
            Assert.AreEqual(1000, dict.Count);
            
            // 验证双向查询性能
            for (int i = 0; i < 1000; i++)
            {
                Assert.AreEqual($"value{i}", dict.GetByKey(i));
                Assert.AreEqual(i, dict.GetByValue($"value{i}"));
            }
        }
        
        [Test]
        public void MultipleAddRemoveOperations_MaintainsConsistency()
        {
            // Arrange
            var dict = new BidirectionalDictionary<int, string>();
            
            // Act - 复杂的添加删除序列
            dict.Add(1, "one");
            dict.Add(2, "two");
            dict.Add(3, "three");
            dict.RemoveByKey(2);
            dict.AddOrUpdate(4, "four");
            dict.AddOrUpdate(1, "uno");
            dict.RemoveByValue("three");
            dict.AddOrUpdate(5, "five");
            
            // Assert
            Assert.AreEqual(3, dict.Count);
            
            // 手动验证双向一致性
            foreach (var key in dict.Keys)
            {
                var value = dict.GetByKey(key);
                var reversedKey = dict.GetByValue(value);
                Assert.AreEqual(key, reversedKey);
            }
            
            Assert.AreEqual("uno", dict.GetByKey(1));
            Assert.AreEqual("four", dict.GetByKey(4));
            Assert.AreEqual("five", dict.GetByKey(5));
        }
        
        #endregion
    }
}
