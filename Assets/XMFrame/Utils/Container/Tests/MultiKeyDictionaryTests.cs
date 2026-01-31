using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using XM.Utils;

namespace XM.Utils.Tests
{
    /// <summary>
    /// MultiKeyDictionary 双键版本单元测试
    /// </summary>
    [TestFixture]
    public class MultiKeyDictionaryTwoKeyTests
    {
        [Test]
        public void Constructor_Default_CreatesEmptyDictionary()
        {
            // Arrange & Act
            var dict = new MultiKeyDictionary<int, string, float>();

            // Assert
            Assert.AreEqual(0, dict.Count);
        }

        [Test]
        public void Constructor_WithCapacity_CreatesEmptyDictionary()
        {
            // Arrange & Act
            var dict = new MultiKeyDictionary<int, string, float>(10);

            // Assert
            Assert.AreEqual(0, dict.Count);
        }

        [Test]
        public void Set_NewEntry_AddsSuccessfully()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, float>();

            // Act
            dict.Set(3.14f, 1, "one");

            // Assert
            Assert.AreEqual(1, dict.Count);
            Assert.AreEqual(3.14f, dict.GetByKey1(1));
            Assert.AreEqual(3.14f, dict.GetByKey2("one"));
        }

        [Test]
        public void Set_ExistingKey1_UpdatesValue()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, float>();
            dict.Set(3.14f, 1, "one");

            // Act
            dict.Set(2.71f, 1, "uno");

            // Assert
            Assert.AreEqual(1, dict.Count);
            Assert.AreEqual(2.71f, dict.GetByKey1(1));
            Assert.AreEqual(2.71f, dict.GetByKey2("uno"));
            Assert.IsFalse(dict.ContainsKey2("one"));
        }

        [Test]
        public void Set_ExistingKey2_UpdatesValue()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, float>();
            dict.Set(3.14f, 1, "one");

            // Act
            dict.Set(2.71f, 100, "one");

            // Assert
            Assert.AreEqual(1, dict.Count);
            Assert.AreEqual(2.71f, dict.GetByKey1(100));
            Assert.AreEqual(2.71f, dict.GetByKey2("one"));
            Assert.IsFalse(dict.ContainsKey1(1));
        }

        [Test]
        public void Set_ConflictingKeys_RemovesOldEntries()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, float>();
            dict.Set(1.0f, 1, "one");
            dict.Set(2.0f, 2, "two");

            // Act - key1=1 exists with "one", key2="two" exists with 2
            dict.Set(3.0f, 1, "two");

            // Assert - old entries should be removed
            Assert.AreEqual(1, dict.Count);
            Assert.AreEqual(3.0f, dict.GetByKey1(1));
            Assert.AreEqual(3.0f, dict.GetByKey2("two"));
            Assert.IsFalse(dict.ContainsKey1(2));
        }

        [Test]
        public void GetByKey1_ExistingKey_ReturnsValue()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, float>();
            dict.Set(3.14f, 1, "one");

            // Act
            var result = dict.GetByKey1(1);

            // Assert
            Assert.AreEqual(3.14f, result);
        }

        [Test]
        public void GetByKey1_NonExistingKey_ReturnsDefault()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, float>();

            // Act
            var result = dict.GetByKey1(999);

            // Assert
            Assert.AreEqual(default(float), result);
        }

        [Test]
        public void GetByKey2_ExistingKey_ReturnsValue()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, float>();
            dict.Set(3.14f, 1, "one");

            // Act
            var result = dict.GetByKey2("one");

            // Assert
            Assert.AreEqual(3.14f, result);
        }

        [Test]
        public void TryGetValueByKey1_ExistingKey_ReturnsTrue()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, float>();
            dict.Set(3.14f, 1, "one");

            // Act
            var found = dict.TryGetValueByKey1(1, out var value);

            // Assert
            Assert.IsTrue(found);
            Assert.AreEqual(3.14f, value);
        }

        [Test]
        public void TryGetValueByKey1_NonExistingKey_ReturnsFalse()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, float>();

            // Act
            var found = dict.TryGetValueByKey1(999, out var value);

            // Assert
            Assert.IsFalse(found);
            Assert.AreEqual(default(float), value);
        }

        [Test]
        public void TryGetValueByKey2_ExistingKey_ReturnsTrue()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, float>();
            dict.Set(3.14f, 1, "one");

            // Act
            var found = dict.TryGetValueByKey2("one", out var value);

            // Assert
            Assert.IsTrue(found);
            Assert.AreEqual(3.14f, value);
        }

        [Test]
        public void ContainsKey1_ExistingKey_ReturnsTrue()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, float>();
            dict.Set(3.14f, 1, "one");

            // Act & Assert
            Assert.IsTrue(dict.ContainsKey1(1));
        }

        [Test]
        public void ContainsKey1_NonExistingKey_ReturnsFalse()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, float>();

            // Act & Assert
            Assert.IsFalse(dict.ContainsKey1(999));
        }

        [Test]
        public void ContainsKey2_ExistingKey_ReturnsTrue()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, float>();
            dict.Set(3.14f, 1, "one");

            // Act & Assert
            Assert.IsTrue(dict.ContainsKey2("one"));
        }

        [Test]
        public void ContainsValue_ExistingValue_ReturnsTrue()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, float>();
            dict.Set(3.14f, 1, "one");

            // Act & Assert
            Assert.IsTrue(dict.ContainsValue(3.14f));
        }

        [Test]
        public void ContainsValue_NonExistingValue_ReturnsFalse()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, float>();
            dict.Set(3.14f, 1, "one");

            // Act & Assert
            Assert.IsFalse(dict.ContainsValue(2.71f));
        }

        [Test]
        public void RemoveByKey1_ExistingKey_RemovesAndReturnsTrue()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, float>();
            dict.Set(3.14f, 1, "one");

            // Act
            var removed = dict.RemoveByKey1(1);

            // Assert
            Assert.IsTrue(removed);
            Assert.AreEqual(0, dict.Count);
            Assert.IsFalse(dict.ContainsKey1(1));
            Assert.IsFalse(dict.ContainsKey2("one"));
        }

        [Test]
        public void RemoveByKey1_NonExistingKey_ReturnsFalse()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, float>();

            // Act
            var removed = dict.RemoveByKey1(999);

            // Assert
            Assert.IsFalse(removed);
        }

        [Test]
        public void RemoveByKey2_ExistingKey_RemovesAndReturnsTrue()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, float>();
            dict.Set(3.14f, 1, "one");

            // Act
            var removed = dict.RemoveByKey2("one");

            // Assert
            Assert.IsTrue(removed);
            Assert.AreEqual(0, dict.Count);
            Assert.IsFalse(dict.ContainsKey1(1));
            Assert.IsFalse(dict.ContainsKey2("one"));
        }

        [Test]
        public void Remove_ExistingValue_RemovesAndReturnsTrue()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, float>();
            dict.Set(3.14f, 1, "one");

            // Act
            var removed = dict.Remove(3.14f);

            // Assert
            Assert.IsTrue(removed);
            Assert.AreEqual(0, dict.Count);
        }

        [Test]
        public void Remove_NonExistingValue_ReturnsFalse()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, float>();

            // Act
            var removed = dict.Remove(3.14f);

            // Assert
            Assert.IsFalse(removed);
        }

        [Test]
        public void GetKeys_ExistingValue_ReturnsKeys()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, float>();
            dict.Set(3.14f, 1, "one");

            // Act
            var (key1, key2) = dict.GetKeys(3.14f);

            // Assert
            Assert.AreEqual(1, key1);
            Assert.AreEqual("one", key2);
        }

        [Test]
        public void GetKeys_NonExistingValue_ReturnsDefault()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, float>();

            // Act
            var (key1, key2) = dict.GetKeys(3.14f);

            // Assert
            Assert.AreEqual(default(int), key1);
            Assert.AreEqual(default(string), key2);
        }

        [Test]
        public void Clear_RemovesAllEntries()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, float>();
            dict.Set(1.0f, 1, "one");
            dict.Set(2.0f, 2, "two");
            dict.Set(3.0f, 3, "three");

            // Act
            dict.Clear();

            // Assert
            Assert.AreEqual(0, dict.Count);
            Assert.IsFalse(dict.ContainsKey1(1));
            Assert.IsFalse(dict.ContainsKey2("one"));
        }

        [Test]
        public void Indexer_Key1_ReturnsValue()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, float>();
            dict.Set(3.14f, 1, "one");

            // Act
            var result = dict[1];

            // Assert
            Assert.AreEqual(3.14f, result);
        }

        [Test]
        public void Indexer_Key2_ReturnsValue()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, float>();
            dict.Set(3.14f, 1, "one");

            // Act
            var result = dict["one"];

            // Assert
            Assert.AreEqual(3.14f, result);
        }

        [Test]
        public void Values_ReturnsAllValues()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, float>();
            dict.Set(1.0f, 1, "one");
            dict.Set(2.0f, 2, "two");
            dict.Set(3.0f, 3, "three");

            // Act
            var values = dict.Values.ToList();

            // Assert
            Assert.AreEqual(3, values.Count);
            Assert.Contains(1.0f, values);
            Assert.Contains(2.0f, values);
            Assert.Contains(3.0f, values);
        }

        [Test]
        public void Keys1_ReturnsAllKey1s()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, float>();
            dict.Set(1.0f, 1, "one");
            dict.Set(2.0f, 2, "two");

            // Act
            var keys = dict.Keys1.ToList();

            // Assert
            Assert.AreEqual(2, keys.Count);
            Assert.Contains(1, keys);
            Assert.Contains(2, keys);
        }

        [Test]
        public void Keys2_ReturnsAllKey2s()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, float>();
            dict.Set(1.0f, 1, "one");
            dict.Set(2.0f, 2, "two");

            // Act
            var keys = dict.Keys2.ToList();

            // Assert
            Assert.AreEqual(2, keys.Count);
            Assert.Contains("one", keys);
            Assert.Contains("two", keys);
        }

        [Test]
        public void Pairs_ReturnsAllPairs()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, float>();
            dict.Set(1.0f, 1, "one");
            dict.Set(2.0f, 2, "two");

            // Act
            var pairs = dict.Pairs.ToList();

            // Assert
            Assert.AreEqual(2, pairs.Count);
            Assert.IsTrue(pairs.Any(p => p.value == 1.0f && p.key1 == 1 && p.key2 == "one"));
            Assert.IsTrue(pairs.Any(p => p.value == 2.0f && p.key1 == 2 && p.key2 == "two"));
        }

        [Test]
        public void GetIter0_ReturnsValues()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, float>();
            dict.Set(1.0f, 1, "one");
            dict.Set(2.0f, 2, "two");

            // Act
            var values = dict.GetIter0().ToList();

            // Assert
            Assert.AreEqual(2, values.Count);
            Assert.Contains(1.0f, values);
            Assert.Contains(2.0f, values);
        }

        [Test]
        public void GetIter01_ReturnsValueKey1Tuples()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, float>();
            dict.Set(1.0f, 1, "one");

            // Act
            var tuples = dict.GetIter01().ToList();

            // Assert
            Assert.AreEqual(1, tuples.Count);
            Assert.AreEqual((1.0f, 1), tuples[0]);
        }

        [Test]
        public void GetIter02_ReturnsValueKey2Tuples()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, float>();
            dict.Set(1.0f, 1, "one");

            // Act
            var tuples = dict.GetIter02().ToList();

            // Assert
            Assert.AreEqual(1, tuples.Count);
            Assert.AreEqual((1.0f, "one"), tuples[0]);
        }

        [Test]
        public void GetIter12_ReturnsKey1Key2Tuples()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, float>();
            dict.Set(1.0f, 1, "one");

            // Act
            var tuples = dict.GetIter12().ToList();

            // Assert
            Assert.AreEqual(1, tuples.Count);
            Assert.AreEqual((1, "one"), tuples[0]);
        }

        [Test]
        public void GetIter012_ReturnsValueKey1Key2Tuples()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, float>();
            dict.Set(1.0f, 1, "one");

            // Act
            var tuples = dict.GetIter012().ToList();

            // Assert
            Assert.AreEqual(1, tuples.Count);
            Assert.AreEqual((1.0f, 1, "one"), tuples[0]);
        }

        [Test]
        public void IEnumerable_Foreach_EnumeratesValues()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, float>();
            dict.Set(1.0f, 1, "one");
            dict.Set(2.0f, 2, "two");

            // Act
            var values = new List<float>();
            foreach (var value in dict)
            {
                values.Add(value);
            }

            // Assert
            Assert.AreEqual(2, values.Count);
            Assert.Contains(1.0f, values);
            Assert.Contains(2.0f, values);
        }

        [Test]
        public void EmptyDictionary_Iteration_DoesNotThrow()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, float>();

            // Act & Assert
            Assert.DoesNotThrow(() =>
            {
                var _ = dict.Values.ToList();
                var __ = dict.Keys1.ToList();
                var ___ = dict.Keys2.ToList();
            });
        }

        [Test]
        public void MultipleAddRemove_TriggersCapacityExpansion()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, float>(3);

            // Act - add more than initial capacity
            for (int i = 0; i < 20; i++)
            {
                dict.Set((float)i, i, $"key{i}");
            }

            // Assert
            Assert.AreEqual(20, dict.Count);
            for (int i = 0; i < 20; i++)
            {
                Assert.AreEqual((float)i, dict.GetByKey1(i));
            }
        }

        [Test]
        public void AddRemoveAdd_ReusesFreedEntries()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, float>();
            dict.Set(1.0f, 1, "one");
            dict.Set(2.0f, 2, "two");

            // Act
            dict.RemoveByKey1(1);
            dict.Set(3.0f, 3, "three");

            // Assert
            Assert.AreEqual(2, dict.Count);
            Assert.IsFalse(dict.ContainsKey1(1));
            Assert.IsTrue(dict.ContainsKey1(2));
            Assert.IsTrue(dict.ContainsKey1(3));
        }
    }

    /// <summary>
    /// MultiKeyDictionary 三键版本单元测试
    /// </summary>
    [TestFixture]
    public class MultiKeyDictionaryThreeKeyTests
    {
        [Test]
        public void Constructor_Default_CreatesEmptyDictionary()
        {
            // Arrange & Act
            var dict = new MultiKeyDictionary<int, string, long, float>();

            // Assert
            Assert.AreEqual(0, dict.Count);
        }

        [Test]
        public void Set_NewEntry_AddsSuccessfully()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, long, float>();

            // Act
            dict.Set(3.14f, 1, "one", 100L);

            // Assert
            Assert.AreEqual(1, dict.Count);
            Assert.AreEqual(3.14f, dict.GetByKey1(1));
            Assert.AreEqual(3.14f, dict.GetByKey2("one"));
            Assert.AreEqual(3.14f, dict.GetByKey3(100L));
        }

        [Test]
        public void Set_ExistingKey1_UpdatesValue()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, long, float>();
            dict.Set(3.14f, 1, "one", 100L);

            // Act
            dict.Set(2.71f, 1, "uno", 111L);

            // Assert
            Assert.AreEqual(1, dict.Count);
            Assert.AreEqual(2.71f, dict.GetByKey1(1));
            Assert.AreEqual(2.71f, dict.GetByKey2("uno"));
            Assert.AreEqual(2.71f, dict.GetByKey3(111L));
        }

        [Test]
        public void Set_ExistingKey3_UpdatesValue()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, long, float>();
            dict.Set(3.14f, 1, "one", 100L);

            // Act
            dict.Set(2.71f, 999, "nuevo", 100L);

            // Assert
            Assert.AreEqual(1, dict.Count);
            Assert.AreEqual(2.71f, dict.GetByKey1(999));
            Assert.AreEqual(2.71f, dict.GetByKey2("nuevo"));
            Assert.AreEqual(2.71f, dict.GetByKey3(100L));
            Assert.IsFalse(dict.ContainsKey1(1));
        }

        [Test]
        public void RemoveByKey3_ExistingKey_RemovesAndReturnsTrue()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, long, float>();
            dict.Set(3.14f, 1, "one", 100L);

            // Act
            var removed = dict.RemoveByKey3(100L);

            // Assert
            Assert.IsTrue(removed);
            Assert.AreEqual(0, dict.Count);
            Assert.IsFalse(dict.ContainsKey1(1));
            Assert.IsFalse(dict.ContainsKey2("one"));
            Assert.IsFalse(dict.ContainsKey3(100L));
        }

        [Test]
        public void GetKeys_ExistingValue_ReturnsThreeKeys()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, long, float>();
            dict.Set(3.14f, 1, "one", 100L);

            // Act
            var (k1, k2, k3) = dict.GetKeys(3.14f);

            // Assert
            Assert.AreEqual(1, k1);
            Assert.AreEqual("one", k2);
            Assert.AreEqual(100L, k3);
        }

        [Test]
        public void Keys3_ReturnsAllKey3s()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, long, float>();
            dict.Set(1.0f, 1, "one", 100L);
            dict.Set(2.0f, 2, "two", 200L);

            // Act
            var keys = dict.Keys3.ToList();

            // Assert
            Assert.AreEqual(2, keys.Count);
            Assert.Contains(100L, keys);
            Assert.Contains(200L, keys);
        }

        [Test]
        public void GetIter03_ReturnsValueKey3Tuples()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, long, float>();
            dict.Set(1.0f, 1, "one", 100L);

            // Act
            var tuples = dict.GetIter03().ToList();

            // Assert
            Assert.AreEqual(1, tuples.Count);
            Assert.AreEqual((1.0f, 100L), tuples[0]);
        }

        [Test]
        public void GetIter13_ReturnsKey1Key3Tuples()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, long, float>();
            dict.Set(1.0f, 1, "one", 100L);

            // Act
            var tuples = dict.GetIter13().ToList();

            // Assert
            Assert.AreEqual(1, tuples.Count);
            Assert.AreEqual((1, 100L), tuples[0]);
        }

        [Test]
        public void GetIter23_ReturnsKey2Key3Tuples()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, long, float>();
            dict.Set(1.0f, 1, "one", 100L);

            // Act
            var tuples = dict.GetIter23().ToList();

            // Assert
            Assert.AreEqual(1, tuples.Count);
            Assert.AreEqual(("one", 100L), tuples[0]);
        }

        [Test]
        public void GetIter013_ReturnsValueKey1Key3Tuples()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, long, float>();
            dict.Set(1.0f, 1, "one", 100L);

            // Act
            var tuples = dict.GetIter013().ToList();

            // Assert
            Assert.AreEqual(1, tuples.Count);
            Assert.AreEqual((1.0f, 1, 100L), tuples[0]);
        }

        [Test]
        public void GetIter023_ReturnsValueKey2Key3Tuples()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, long, float>();
            dict.Set(1.0f, 1, "one", 100L);

            // Act
            var tuples = dict.GetIter023().ToList();

            // Assert
            Assert.AreEqual(1, tuples.Count);
            Assert.AreEqual((1.0f, "one", 100L), tuples[0]);
        }

        [Test]
        public void GetIter123_ReturnsKey1Key2Key3Tuples()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, long, float>();
            dict.Set(1.0f, 1, "one", 100L);

            // Act
            var tuples = dict.GetIter123().ToList();

            // Assert
            Assert.AreEqual(1, tuples.Count);
            Assert.AreEqual((1, "one", 100L), tuples[0]);
        }

        [Test]
        public void GetIter0123_ReturnsAllFourElements()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, long, float>();
            dict.Set(1.0f, 1, "one", 100L);

            // Act
            var tuples = dict.GetIter0123().ToList();

            // Assert
            Assert.AreEqual(1, tuples.Count);
            Assert.AreEqual((1.0f, 1, "one", 100L), tuples[0]);
        }

        [Test]
        public void Pairs_ReturnsValueAndThreeKeys()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, long, float>();
            dict.Set(1.0f, 1, "one", 100L);

            // Act
            var pairs = dict.Pairs.ToList();

            // Assert
            Assert.AreEqual(1, pairs.Count);
            Assert.AreEqual(1.0f, pairs[0].value);
            Assert.AreEqual(1, pairs[0].key1);
            Assert.AreEqual("one", pairs[0].key2);
            Assert.AreEqual(100L, pairs[0].key3);
        }

        [Test]
        public void MultipleOperations_WorksCorrectly()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, long, float>();

            // Act
            dict.Set(1.0f, 1, "one", 100L);
            dict.Set(2.0f, 2, "two", 200L);
            dict.Set(3.0f, 3, "three", 300L);
            dict.RemoveByKey2("two");

            // Assert
            Assert.AreEqual(2, dict.Count);
            Assert.IsTrue(dict.ContainsKey1(1));
            Assert.IsTrue(dict.ContainsKey3(300L));
            Assert.IsFalse(dict.ContainsKey2("two"));
        }

        [Test]
        public void Clear_RemovesAllEntries()
        {
            var dict = new MultiKeyDictionary<int, string, long, float>();
            dict.Set(1.0f, 1, "one", 100L);
            dict.Set(2.0f, 2, "two", 200L);
            dict.Clear();
            Assert.AreEqual(0, dict.Count);
            Assert.IsFalse(dict.ContainsKey1(1));
            Assert.IsFalse(dict.ContainsKey3(100L));
        }

        [Test]
        public void CapacityExpansion_WithThreeKeys_WorksCorrectly()
        {
            var dict = new MultiKeyDictionary<int, string, long, float>(3);
            for (int i = 0; i < 15; i++)
                dict.Set((float)i, i, $"k{i}", (long)i * 10);
            Assert.AreEqual(15, dict.Count);
            for (int i = 0; i < 15; i++)
            {
                Assert.AreEqual((float)i, dict.GetByKey1(i));
                Assert.AreEqual((float)i, dict.GetByKey3((long)i * 10));
            }
        }

        [Test]
        public void EmptyDictionary_Iteration_DoesNotThrow()
        {
            var dict = new MultiKeyDictionary<int, string, long, float>();
            Assert.DoesNotThrow(() =>
            {
                var _ = dict.Keys3.ToList();
                var __ = dict.Pairs.ToList();
            });
        }
    }

    /// <summary>
    /// MultiKeyDictionary 四键版本单元测试
    /// </summary>
    [TestFixture]
    public class MultiKeyDictionaryFourKeyTests
    {
        [Test]
        public void Constructor_Default_CreatesEmptyDictionary()
        {
            // Arrange & Act
            var dict = new MultiKeyDictionary<int, string, long, double, float>();

            // Assert
            Assert.AreEqual(0, dict.Count);
        }

        [Test]
        public void Set_NewEntry_AddsSuccessfully()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, long, double, float>();

            // Act
            dict.Set(3.14f, 1, "one", 100L, 1.1);

            // Assert
            Assert.AreEqual(1, dict.Count);
            Assert.AreEqual(3.14f, dict.GetByKey1(1));
            Assert.AreEqual(3.14f, dict.GetByKey2("one"));
            Assert.AreEqual(3.14f, dict.GetByKey3(100L));
            Assert.AreEqual(3.14f, dict.GetByKey4(1.1));
        }

        [Test]
        public void Set_ExistingKey4_UpdatesValue()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, long, double, float>();
            dict.Set(3.14f, 1, "one", 100L, 1.1);

            // Act
            dict.Set(2.71f, 999, "nuevo", 999L, 1.1);

            // Assert
            Assert.AreEqual(1, dict.Count);
            Assert.AreEqual(2.71f, dict.GetByKey1(999));
            Assert.AreEqual(2.71f, dict.GetByKey4(1.1));
            Assert.IsFalse(dict.ContainsKey1(1));
        }

        [Test]
        public void RemoveByKey4_ExistingKey_RemovesAndReturnsTrue()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, long, double, float>();
            dict.Set(3.14f, 1, "one", 100L, 1.1);

            // Act
            var removed = dict.RemoveByKey4(1.1);

            // Assert
            Assert.IsTrue(removed);
            Assert.AreEqual(0, dict.Count);
            Assert.IsFalse(dict.ContainsKey4(1.1));
        }

        [Test]
        public void GetKeys_ExistingValue_ReturnsFourKeys()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, long, double, float>();
            dict.Set(3.14f, 1, "one", 100L, 1.1);

            // Act
            var (k1, k2, k3, k4) = dict.GetKeys(3.14f);

            // Assert
            Assert.AreEqual(1, k1);
            Assert.AreEqual("one", k2);
            Assert.AreEqual(100L, k3);
            Assert.AreEqual(1.1, k4);
        }

        [Test]
        public void ContainsKey4_ExistingKey_ReturnsTrue()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, long, double, float>();
            dict.Set(3.14f, 1, "one", 100L, 1.1);

            // Act & Assert
            Assert.IsTrue(dict.ContainsKey4(1.1));
        }

        [Test]
        public void Keys4_ReturnsAllKey4s()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, long, double, float>();
            dict.Set(1.0f, 1, "one", 100L, 1.1);
            dict.Set(2.0f, 2, "two", 200L, 2.2);

            // Act
            var keys = dict.Keys4.ToList();

            // Assert
            Assert.AreEqual(2, keys.Count);
            Assert.Contains(1.1, keys);
            Assert.Contains(2.2, keys);
        }

        [Test]
        public void GetIter04_ReturnsValueKey4Tuples()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, long, double, float>();
            dict.Set(1.0f, 1, "one", 100L, 1.1);

            // Act
            var tuples = dict.GetIter04().ToList();

            // Assert
            Assert.AreEqual(1, tuples.Count);
            Assert.AreEqual((1.0f, 1.1), tuples[0]);
        }

        [Test]
        public void GetIter14_ReturnsKey1Key4Tuples()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, long, double, float>();
            dict.Set(1.0f, 1, "one", 100L, 1.1);

            // Act
            var tuples = dict.GetIter14().ToList();

            // Assert
            Assert.AreEqual(1, tuples.Count);
            Assert.AreEqual((1, 1.1), tuples[0]);
        }

        [Test]
        public void GetIter01234_ReturnsAllFiveElements()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, long, double, float>();
            dict.Set(1.0f, 1, "one", 100L, 1.1);

            // Act
            var tuples = dict.GetIter01234().ToList();

            // Assert
            Assert.AreEqual(1, tuples.Count);
            Assert.AreEqual((1.0f, 1, "one", 100L, 1.1), tuples[0]);
        }

        [Test]
        public void Pairs_ReturnsValueAndFourKeys()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, long, double, float>();
            dict.Set(1.0f, 1, "one", 100L, 1.1);

            // Act
            var pairs = dict.Pairs.ToList();

            // Assert
            Assert.AreEqual(1, pairs.Count);
            Assert.AreEqual(1.0f, pairs[0].value);
            Assert.AreEqual(1, pairs[0].key1);
            Assert.AreEqual("one", pairs[0].key2);
            Assert.AreEqual(100L, pairs[0].key3);
            Assert.AreEqual(1.1, pairs[0].key4);
        }

        [Test]
        public void ComplexScenario_MultipleKeysAndOperations()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, long, double, float>();

            // Act
            dict.Set(1.0f, 1, "a", 10L, 0.1);
            dict.Set(2.0f, 2, "b", 20L, 0.2);
            dict.Set(3.0f, 3, "c", 30L, 0.3);
            dict.Set(4.0f, 4, "d", 40L, 0.4);

            dict.RemoveByKey3(20L);
            dict.Set(5.0f, 5, "e", 50L, 0.5);

            // Assert
            Assert.AreEqual(4, dict.Count);
            Assert.IsTrue(dict.ContainsKey1(1));
            Assert.IsFalse(dict.ContainsKey2("b"));
            Assert.IsTrue(dict.ContainsKey4(0.5));
            Assert.AreEqual(5.0f, dict.GetByKey3(50L));
        }

        [Test]
        public void CapacityExpansion_WithFourKeys_WorksCorrectly()
        {
            // Arrange
            var dict = new MultiKeyDictionary<int, string, long, double, float>(3);

            // Act
            for (int i = 0; i < 15; i++)
            {
                dict.Set((float)i, i, $"k{i}", (long)i * 10, (double)i * 0.1);
            }

            // Assert
            Assert.AreEqual(15, dict.Count);
            for (int i = 0; i < 15; i++)
            {
                Assert.AreEqual((float)i, dict.GetByKey1(i));
                Assert.AreEqual((float)i, dict.GetByKey2($"k{i}"));
                Assert.AreEqual((float)i, dict.GetByKey3((long)i * 10));
                Assert.AreEqual((float)i, dict.GetByKey4((double)i * 0.1));
            }
        }

        [Test]
        public void Clear_RemovesAllEntries()
        {
            var dict = new MultiKeyDictionary<int, string, long, double, float>();
            dict.Set(1.0f, 1, "one", 100L, 1.1);
            dict.Set(2.0f, 2, "two", 200L, 2.2);
            dict.Clear();
            Assert.AreEqual(0, dict.Count);
            Assert.IsFalse(dict.ContainsKey4(1.1));
        }

        [Test]
        public void EmptyDictionary_Iteration_DoesNotThrow()
        {
            var dict = new MultiKeyDictionary<int, string, long, double, float>();
            Assert.DoesNotThrow(() =>
            {
                var _ = dict.Keys4.ToList();
                var __ = dict.Pairs.ToList();
            });
        }
    }
}
