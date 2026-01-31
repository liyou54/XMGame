using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace XM.Utils
{
    /// <summary>
    /// 迭代器输出类型标志，用于 Enumerate 方法指定迭代内容。
    /// 可组合使用（通过 | 运算符）来指定返回多个元素的元组。
    /// </summary>
    [Flags]
    public enum MultiKeyIterFlags
    {
        /// <summary>迭代 Value</summary>
        Value = 1,
        /// <summary>迭代 Key1</summary>
        Key1 = 2,
        /// <summary>迭代 Key2</summary>
        Key2 = 4,
        /// <summary>迭代 Key3</summary>
        Key3 = 8,
        /// <summary>迭代 Key4</summary>
        Key4 = 16,
    }

    /// <summary>
    /// 哈希表辅助工具类：提供质数表和容量扩展功能，用于确保哈希桶数量为质数以减少哈希冲突。
    /// </summary>
    internal static class MultiKeyHashHelpers
    {
        /// <summary>预定义质数表，用于快速查找合适的哈希桶大小</summary>
        private static readonly int[] Primes = { 3, 7, 11, 17, 23, 29, 37, 47, 59, 71, 89, 107, 131, 163, 197, 239, 293, 353, 431, 521, 631, 761, 919, 1103, 1327, 1597, 1931, 2333, 2801, 3371, 4049, 4861, 5839, 7013, 8419, 10103, 12143, 14591, 17519, 21023, 25229, 30293, 36353, 43627, 52361, 62851, 75431, 90523, 108631, 130363, 156437, 187751, 225307, 270371, 324449, 389357, 467237, 560689, 672827, 807403, 968897, 1162687, 1395263, 1674319, 2009191, 2411033, 2893249, 3471899, 4166287, 4999559, 5999471, 7199369 };

        /// <summary>
        /// 获取大于等于 min 的最小质数。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetPrime(int min)
        {
            foreach (int prime in Primes)
            {
                if (prime >= min) return prime;
            }
            return min;
        }

        /// <summary>
        /// 计算扩容后的质数大小（约为原大小的 2 倍）。
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ExpandPrime(int oldSize)
        {
            int newSize = 2 * oldSize;
            return (uint)newSize > 0x7FEFFFFD ? 0x7FEFFFFD : GetPrime(newSize);
        }
    }

    /// <summary>
    /// 双键字典：支持通过两个不同的 key 访问同一个 value。
    /// <para>
    /// 架构说明：每个 key 维护独立的 Entry 数组（_entries1、_entries2），
    /// 通过 linkTo1/linkTo2 字段相互关联，value 只在 _values 数组中存储一份。
    /// 使用独立的 freeList 管理各 Entry 数组和 value 数组的空闲槽位，支持高效的增删改查。
    /// </para>
    /// </summary>
    /// <typeparam name="TKey1">第一个键的类型</typeparam>
    /// <typeparam name="TKey2">第二个键的类型</typeparam>
    /// <typeparam name="TValue">值的类型</typeparam>
    public class MultiKeyDictionary<TKey1, TKey2, TValue> : IEnumerable<TValue>
        where TKey1 : notnull
        where TKey2 : notnull
    {
        /// <summary>freeList 链表的起始标记（用于编码 freeList 指针）</summary>
        private const int StartOfFreeList = -3;

        // 哈希桶数组，存储每个 key 的桶头索引（值为 entry 索引 + 1，0 表示空桶）
        private int[] _buckets1, _buckets2;
        // Entry 数组，每个 key 独立维护
        private Entry<TKey1>[] _entries1;
        private Entry<TKey2>[] _entries2;
        // Value 数组，共享存储，通过 Entry.valueIndex 引用
        private TValue[] _values;
        // Value 的 freeList 链表（_valueFreeNext[i] 指向下一个空闲槽位）
        private int[] _valueFreeNext;
        // Entry1 的计数与 freeList 头（空闲槽位由 freeList 链表表示，无需单独 freeCount）
        private int _count1, _freeList1;
        // Entry2 的计数与 freeList 头
        private int _count2, _freeList2;
        // Value 的计数、freeList 头和空闲数量
        private int _valueCount, _valueFreeList, _valueFreeCount;
        private readonly IEqualityComparer<TKey1> _comparer1;
        private readonly IEqualityComparer<TKey2> _comparer2;

        /// <summary>
        /// Entry 结构：存储单个 key 的哈希表节点信息。
        /// </summary>
        /// <typeparam name="T">key 的类型</typeparam>
        private struct Entry<T>
        {
            /// <summary>freeList 链表指针（空闲时使用，编码为 StartOfFreeList - nextFreeIndex）</summary>
            public int next;
            /// <summary>bucket 链表指针（指向同一桶内的下一个 Entry 索引，-1 表示末尾）</summary>
            public int link;
            /// <summary>关联的 _entries1 索引</summary>
            public int linkTo1;
            /// <summary>关联的 _entries2 索引</summary>
            public int linkTo2;
            /// <summary>key 的哈希码（缓存以避免重复计算）</summary>
            public int hashCode;
            /// <summary>key 值</summary>
            public T key;
            /// <summary>关联的 _values 数组索引</summary>
            public int valueIndex;
        }

        /// <summary>
        /// 创建双键字典实例。
        /// </summary>
        /// <param name="capacity">初始容量（会自动调整为大于等于 3 的质数）</param>
        public MultiKeyDictionary(int capacity = 0)
        {
            capacity = capacity < 3 ? 3 : MultiKeyHashHelpers.GetPrime(capacity);
            _buckets1 = new int[capacity];
            _buckets2 = new int[capacity];
            _entries1 = new Entry<TKey1>[capacity];
            _entries2 = new Entry<TKey2>[capacity];
            _values = new TValue[capacity];
            _valueFreeNext = new int[capacity];
            _comparer1 = EqualityComparer<TKey1>.Default;
            _comparer2 = EqualityComparer<TKey2>.Default;
            _freeList1 = _freeList2 = _valueFreeList = -1;
        }

        /// <summary>
        /// 获取字典中有效的 value 数量（不包括已删除的空闲槽位）。
        /// </summary>
        public int Count => _valueCount - _valueFreeCount;

        /// <summary>计算 key1 的哈希码对应的桶索引</summary>
        private int B1(int h) => (int)((uint)h % (uint)_buckets1.Length);
        /// <summary>计算 key2 的哈希码对应的桶索引</summary>
        private int B2(int h) => (int)((uint)h % (uint)_buckets2.Length);

        /// <summary>
        /// 确保 _entries1 有足够容量，容量不足时扩容并重建哈希桶。
        /// </summary>
        private void EnsureCapacity1()
        {
            if (_count1 < _entries1.Length) return;
            int n = MultiKeyHashHelpers.ExpandPrime(_entries1.Length);
            var b = new int[n];
            var e = new Entry<TKey1>[n];
            Array.Copy(_entries1, e, _count1);
            // 重建哈希桶链表
            for (int i = 0; i < _count1; i++)
            {
                if (e[i].next >= -1) // 跳过 freeList 中的节点
                {
                    int x = (int)((uint)e[i].hashCode % (uint)n);
                    e[i].link = b[x] - 1;
                    b[x] = i + 1;
                }
            }
            _buckets1 = b;
            _entries1 = e;
        }

        /// <summary>
        /// 确保 _entries2 有足够容量，容量不足时扩容并重建哈希桶。
        /// </summary>
        private void EnsureCapacity2()
        {
            if (_count2 < _entries2.Length) return;
            int n = MultiKeyHashHelpers.ExpandPrime(_entries2.Length);
            var b = new int[n];
            var e = new Entry<TKey2>[n];
            Array.Copy(_entries2, e, _count2);
            for (int i = 0; i < _count2; i++)
            {
                if (e[i].next >= -1)
                {
                    int x = (int)((uint)e[i].hashCode % (uint)n);
                    e[i].link = b[x] - 1;
                    b[x] = i + 1;
                }
            }
            _buckets2 = b;
            _entries2 = e;
        }

        /// <summary>
        /// 确保 _values 数组有足够容量，容量不足时扩容。
        /// </summary>
        private void EnsureValueCapacity()
        {
            if (_valueCount < _values.Length) return;
            int n = MultiKeyHashHelpers.ExpandPrime(_values.Length);
            var v = new TValue[n];
            var fn = new int[n];
            Array.Copy(_values, v, _valueCount);
            Array.Copy(_valueFreeNext, fn, _valueCount);
            _values = v;
            _valueFreeNext = fn;
        }

        /// <summary>
        /// 通过 key1 查找对应的 Entry 索引和 value 索引。
        /// </summary>
        /// <returns>找到返回 true，否则返回 false</returns>
        private bool TryFindByKey1(TKey1 key1, out int idx1, out int valueIdx)
        {
            int h = _comparer1.GetHashCode(key1);
            int i = _buckets1[B1(h)] - 1; // 获取桶头（-1 表示空桶）
            // 沿着 bucket 链表查找
            while (i >= 0)
            {
                ref var e = ref _entries1[i];
                if (e.hashCode == h && _comparer1.Equals(e.key, key1))
                {
                    idx1 = i;
                    valueIdx = e.valueIndex;
                    return true;
                }
                i = e.link; // 下一个节点
            }
            idx1 = -1;
            valueIdx = -1;
            return false;
        }

        /// <summary>
        /// 通过 key2 查找对应的 Entry 索引和 value 索引。
        /// </summary>
        /// <returns>找到返回 true，否则返回 false</returns>
        private bool TryFindByKey2(TKey2 key2, out int idx2, out int valueIdx)
        {
            int h = _comparer2.GetHashCode(key2);
            int i = _buckets2[B2(h)] - 1;
            while (i >= 0)
            {
                ref var e = ref _entries2[i];
                if (e.hashCode == h && _comparer2.Equals(e.key, key2))
                {
                    idx2 = i;
                    valueIdx = e.valueIndex;
                    return true;
                }
                i = e.link;
            }
            idx2 = -1;
            valueIdx = -1;
            return false;
        }

        /// <summary>
        /// 从 key1 的哈希桶链表中移除指定 Entry（不释放 Entry 本身）。
        /// </summary>
        private void RemoveFromBucket1(int idx1)
        {
            int h = _entries1[idx1].hashCode;
            int b = B1(h);
            int i = _buckets1[b] - 1, last = -1;
            while (i >= 0)
            {
                if (i == idx1)
                {
                    // 从链表中摘除节点
                    if (last < 0) _buckets1[b] = _entries1[i].link + 1; // 更新桶头
                    else _entries1[last].link = _entries1[i].link; // 更新前驱的 link
                    return;
                }
                last = i;
                i = _entries1[i].link;
            }
        }

        /// <summary>
        /// 从 key2 的哈希桶链表中移除指定 Entry（不释放 Entry 本身）。
        /// </summary>
        private void RemoveFromBucket2(int idx2)
        {
            int h = _entries2[idx2].hashCode;
            int b = B2(h);
            int i = _buckets2[b] - 1, last = -1;
            while (i >= 0)
            {
                if (i == idx2)
                {
                    if (last < 0) _buckets2[b] = _entries2[i].link + 1;
                    else _entries2[last].link = _entries2[i].link;
                    return;
                }
                last = i;
                i = _entries2[i].link;
            }
        }

        /// <summary>
        /// 分配一个 Entry1 槽位（优先从 freeList 复用，否则扩容）。
        /// </summary>
        private int AllocEntry1()
        {
            if (_freeList1 != -1)
            {
                int idx = _freeList1;
                _freeList1 = StartOfFreeList - _entries1[idx].next;
                return idx;
            }
            EnsureCapacity1();
            return _count1++;
        }

        /// <summary>
        /// 分配一个 Entry2 槽位（优先从 freeList 复用，否则扩容）。
        /// </summary>
        private int AllocEntry2()
        {
            if (_freeList2 != -1)
            {
                int idx = _freeList2;
                _freeList2 = StartOfFreeList - _entries2[idx].next;
                return idx;
            }
            EnsureCapacity2();
            return _count2++;
        }

        /// <summary>
        /// 分配一个 Value 槽位（优先从 freeList 复用，否则扩容）。
        /// </summary>
        private int AllocValue()
        {
            int idx;
            if (_valueFreeCount > 0)
            {
                idx = _valueFreeList;
                _valueFreeList = _valueFreeNext[idx];
                _valueFreeCount--;
            }
            else
            {
                EnsureValueCapacity();
                idx = _valueCount++;
            }
            return idx;
        }

        /// <summary>
        /// 释放 Entry1 槽位到 freeList（编码 next 指针）。
        /// </summary>
        private void FreeEntry1(int idx)
        {
            _entries1[idx].next = StartOfFreeList - _freeList1;
            _freeList1 = idx;
        }

        /// <summary>
        /// 释放 Entry2 槽位到 freeList。
        /// </summary>
        private void FreeEntry2(int idx)
        {
            _entries2[idx].next = StartOfFreeList - _freeList2;
            _freeList2 = idx;
        }

        /// <summary>
        /// 释放 Value 槽位到 freeList，并清空 value。
        /// </summary>
        private void FreeValue(int valueIdx)
        {
            _values[valueIdx] = default;
            _valueFreeNext[valueIdx] = _valueFreeList;
            _valueFreeList = valueIdx;
            _valueFreeCount++;
        }

        /// <summary>
        /// 按完整 key 组设置：先按 key1/key2 移除可能冲突的项，再添加 (value, key1, key2)。对同一 (key1, key2) 多次调用结果一致（幂等）。
        /// </summary>
        public void Set(TValue value, TKey1 key1, TKey2 key2)
        {
            int h1 = _comparer1.GetHashCode(key1);
            int h2 = _comparer2.GetHashCode(key2);
            RemoveByKey1(key1);
            RemoveByKey2(key2);
            int vi = AllocValue();
            int i1 = AllocEntry1();
            int i2 = AllocEntry2();
            try
            {
                _values[vi] = value;
                ref var e1 = ref _entries1[i1];
                e1.next = -1;
                e1.linkTo1 = i1;
                e1.linkTo2 = i2;
                e1.hashCode = h1;
                e1.key = key1;
                e1.valueIndex = vi;
                ref var e2 = ref _entries2[i2];
                e2.next = -1;
                e2.linkTo1 = i1;
                e2.linkTo2 = i2;
                e2.hashCode = h2;
                e2.key = key2;
                e2.valueIndex = vi;
                int x = B1(h1);
                e1.link = _buckets1[x] - 1;
                _buckets1[x] = i1 + 1;
                x = B2(h2);
                e2.link = _buckets2[x] - 1;
                _buckets2[x] = i2 + 1;
            }
            catch
            {
                FreeEntry1(i1);
                FreeEntry2(i2);
                FreeValue(vi);
                throw;
            }
        }

        /// <summary>通过 key1 获取 value，不存在则返回 default</summary>
        public TValue GetByKey1(TKey1 key1) => TryGetByKey1(key1, out var v) ? v : default;
        /// <summary>通过 key2 获取 value，不存在则返回 default</summary>
        public TValue GetByKey2(TKey2 key2) => TryGetByKey2(key2, out var v) ? v : default;
        /// <summary>尝试通过 key1 获取 value</summary>
        public bool TryGetValueByKey1(TKey1 key1, out TValue value) => TryGetByKey1(key1, out value);
        /// <summary>尝试通过 key2 获取 value</summary>
        public bool TryGetValueByKey2(TKey2 key2, out TValue value) => TryGetByKey2(key2, out value);

        private bool TryGetByKey1(TKey1 key1, out TValue value)
        {
            if (TryFindByKey1(key1, out _, out int vi))
            {
                value = _values[vi];
                return true;
            }
            value = default;
            return false;
        }

        private bool TryGetByKey2(TKey2 key2, out TValue value)
        {
            if (TryFindByKey2(key2, out _, out int vi))
            {
                value = _values[vi];
                return true;
            }
            value = default;
            return false;
        }

        /// <summary>检查 key1 是否存在</summary>
        public bool ContainsKey1(TKey1 key1) => TryFindByKey1(key1, out _, out _);
        /// <summary>检查 key2 是否存在</summary>
        public bool ContainsKey2(TKey2 key2) => TryFindByKey2(key2, out _, out _);

        /// <summary>检查 value 是否存在（线性查找）</summary>
        public bool ContainsValue(TValue value)
        {
            var cmp = EqualityComparer<TValue>.Default;
            for (int i = 0; i < _count1; i++)
            {
                if (_entries1[i].next >= -1 && cmp.Equals(_values[_entries1[i].valueIndex], value))
                    return true;
            }
            return false;
        }

        /// <summary>通过 key1 移除项，同时移除关联的 key2 和 value</summary>
        public bool RemoveByKey1(TKey1 key1)
        {
            if (!TryFindByKey1(key1, out int idx1, out int valueIdx)) return false;
            int idx2 = _entries1[idx1].linkTo2;
            RemoveFromBucket1(idx1);
            RemoveFromBucket2(idx2);
            FreeEntry1(idx1);
            FreeEntry2(idx2);
            FreeValue(valueIdx);
            return true;
        }

        /// <summary>通过 key2 移除项，同时移除关联的 key1 和 value</summary>
        public bool RemoveByKey2(TKey2 key2)
        {
            if (!TryFindByKey2(key2, out int idx2, out int valueIdx)) return false;
            int idx1 = _entries2[idx2].linkTo1;
            RemoveFromBucket1(idx1);
            RemoveFromBucket2(idx2);
            FreeEntry1(idx1);
            FreeEntry2(idx2);
            FreeValue(valueIdx);
            return true;
        }

        /// <summary>通过 value 移除项（线性查找），同时移除关联的两个 key</summary>
        public bool Remove(TValue value)
        {
            var cmp = EqualityComparer<TValue>.Default;
            for (int i = 0; i < _count1; i++)
            {
                if (_entries1[i].next >= -1 && cmp.Equals(_values[_entries1[i].valueIndex], value))
                {
                    int idx1 = i;
                    int idx2 = _entries1[idx1].linkTo2;
                    int valueIdx = _entries1[idx1].valueIndex;
                    RemoveFromBucket1(idx1);
                    RemoveFromBucket2(idx2);
                    FreeEntry1(idx1);
                    FreeEntry2(idx2);
                    FreeValue(valueIdx);
                    return true;
                }
            }
            return false;
        }

        /// <summary>根据 value 获取对应的两个 key（线性查找）</summary>
        public (TKey1 key1, TKey2 key2) GetKeys(TValue value)
        {
            var cmp = EqualityComparer<TValue>.Default;
            for (int i = 0; i < _count1; i++)
            {
                if (_entries1[i].next >= -1 && cmp.Equals(_values[_entries1[i].valueIndex], value))
                    return (_entries1[i].key, _entries2[_entries1[i].linkTo2].key);
            }
            return default;
        }

        /// <summary>清空字典，重置所有状态</summary>
        public void Clear()
        {
            Array.Fill(_buckets1, 0);
            Array.Fill(_buckets2, 0);
            Array.Clear(_entries1, 0, _count1);
            Array.Clear(_entries2, 0, _count2);
            Array.Clear(_values, 0, _valueCount);
            Array.Clear(_valueFreeNext, 0, _valueCount);
            _count1 = _count2 = _valueCount = 0;
            _freeList1 = _freeList2 = _valueFreeList = -1;
            _valueFreeCount = 0;
        }

        /// <summary>通过 key1 索引访问 value</summary>
        public TValue this[TKey1 key1] { get => GetByKey1(key1); }
        /// <summary>通过 key2 索引访问 value</summary>
        public TValue this[TKey2 key2] { get => GetByKey2(key2); }

        /// <summary>迭代所有 value（按 _entries1 顺序）。0=val, 1=k1, 2=k2，排列组合如 GetIter01=val&amp;k1</summary>
        public IEnumerable<TValue> GetIter0() { for (int i = 0; i < _count1; i++) if (_entries1[i].next >= -1) yield return _values[_entries1[i].valueIndex]; }
        /// <summary>迭代所有 key1（按 _entries1 顺序）</summary>
        public IEnumerable<TKey1> GetIter1() { for (int i = 0; i < _count1; i++) if (_entries1[i].next >= -1) yield return _entries1[i].key; }
        /// <summary>迭代所有 key2（按 _entries2 顺序）</summary>
        public IEnumerable<TKey2> GetIter2() { for (int i = 0; i < _count2; i++) if (_entries2[i].next >= -1) yield return _entries2[i].key; }
        /// <summary>迭代 (value, key1) 元组（按 _entries1 顺序）</summary>
        public IEnumerable<(TValue, TKey1)> GetIter01() { for (int i = 0; i < _count1; i++) if (_entries1[i].next >= -1) yield return (_values[_entries1[i].valueIndex], _entries1[i].key); }
        /// <summary>迭代 (value, key2) 元组（按 _entries1 顺序）</summary>
        public IEnumerable<(TValue, TKey2)> GetIter02() { for (int i = 0; i < _count1; i++) if (_entries1[i].next >= -1) yield return (_values[_entries1[i].valueIndex], _entries2[_entries1[i].linkTo2].key); }
        /// <summary>迭代 (key1, key2) 元组（按 _entries1 顺序）</summary>
        public IEnumerable<(TKey1, TKey2)> GetIter12() { for (int i = 0; i < _count1; i++) if (_entries1[i].next >= -1) yield return (_entries1[i].key, _entries2[_entries1[i].linkTo2].key); }
        /// <summary>迭代 (value, key1, key2) 元组（按 _entries1 顺序）</summary>
        public IEnumerable<(TValue, TKey1, TKey2)> GetIter012() { for (int i = 0; i < _count1; i++) if (_entries1[i].next >= -1) yield return (_values[_entries1[i].valueIndex], _entries1[i].key, _entries2[_entries1[i].linkTo2].key); }

        /// <summary>获取所有 value</summary>
        public IEnumerable<TValue> Values => GetIter0();
        /// <summary>获取所有 key1</summary>
        public IEnumerable<TKey1> Keys1 => GetIter1();
        /// <summary>获取所有 key2</summary>
        public IEnumerable<TKey2> Keys2 => GetIter2();
        /// <summary>获取所有 (value, key1, key2) 元组</summary>
        public IEnumerable<(TValue value, TKey1 key1, TKey2 key2)> Pairs => GetIter012();

        /// <summary>泛型单元素迭代（通过 flags 指定迭代 Value/Key1/Key2）</summary>
        public IEnumerable<T> Enumerate<T>(MultiKeyIterFlags flags)
        {
            if (flags == MultiKeyIterFlags.Value) return (IEnumerable<T>)GetIter0();
            if (flags == MultiKeyIterFlags.Key1) return (IEnumerable<T>)GetIter1();
            if (flags == MultiKeyIterFlags.Key2) return (IEnumerable<T>)GetIter2();
            throw new ArgumentException($"单元素迭代请使用 Value/Key1/Key2，当前 flags={flags}");
        }

        /// <summary>泛型双元素迭代（仅支持 Key1|Key2 组合）</summary>
        public IEnumerable<(T1, T2)> Enumerate<T1, T2>(MultiKeyIterFlags flags)
        {
            if (flags != (MultiKeyIterFlags.Key1 | MultiKeyIterFlags.Key2))
                throw new ArgumentException($"双元素迭代需 flags=Key1|Key2，当前 flags={flags}");
            return (IEnumerable<(T1, T2)>)GetIter12();
        }

        /// <summary>泛型三元素迭代（需 Value|Key1|Key2）</summary>
        public IEnumerable<(T1, T2, T3)> Enumerate<T1, T2, T3>(MultiKeyIterFlags flags)
        {
            var full = MultiKeyIterFlags.Value | MultiKeyIterFlags.Key1 | MultiKeyIterFlags.Key2;
            if (flags != full)
                throw new ArgumentException($"三元素迭代需 flags=Value|Key1|Key2，当前 flags={flags}");
            return (IEnumerable<(T1, T2, T3)>)GetIter012();
        }

        /// <summary>实现 IEnumerable，迭代所有 value</summary>
        public IEnumerator<TValue> GetEnumerator() => Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>RemoveByKey1 的别名</summary>
        public bool RemoveKey1(TKey1 key1) => RemoveByKey1(key1);
        /// <summary>RemoveByKey2 的别名</summary>
        public bool RemoveKey2(TKey2 key2) => RemoveByKey2(key2);
    }

    /// <summary>
    /// 三键字典：支持通过三个不同的 key 访问同一个 value。
    /// <para>
    /// 架构说明：每个 key 维护独立的 Entry 数组（_entries1/2/3），
    /// 通过 linkTo1/2/3 字段相互关联，value 只在 _values 数组中存储一份。
    /// </para>
    /// </summary>
    public class MultiKeyDictionary<TKey1, TKey2, TKey3, TValue> : IEnumerable<TValue>
        where TKey1 : notnull
        where TKey2 : notnull
        where TKey3 : notnull
    {
        private const int StartOfFreeList = -3;

        private int[] _buckets1, _buckets2, _buckets3;
        private Entry3<TKey1>[] _entries1;
        private Entry3<TKey2>[] _entries2;
        private Entry3<TKey3>[] _entries3;
        private TValue[] _values;
        private int[] _valueFreeNext;
        // 各 key 的 entry 数量与空闲数保持同步（1/2/3 必须相同）；freeList 按 key 分开（不同 key 的槽位索引空间不同）
        /// <summary>行数（每行占 entries1/2/3 各一槽，同一索引）</summary>
        private int _count;
        private int _freeList1;
        private int _freeCount;
        private int _valueCount, _valueFreeList, _valueFreeCount;
        private readonly IEqualityComparer<TKey1> _c1;
        private readonly IEqualityComparer<TKey2> _c2;
        private readonly IEqualityComparer<TKey3> _c3;

        /// <summary>
        /// Entry 结构：存储单个 key 的哈希表节点信息。
        /// </summary>
        private struct Entry3<T>
        {
            public int next;       // freeList 链表指针
            public int link;       // bucket 链表指针
            public int linkTo1, linkTo2, linkTo3; // 关联的 _entries1/2/3 索引
            public int hashCode;   // 缓存的哈希码
            public T key;          // key 值
            public int valueIndex; // 关联的 _values 数组索引
        }

        /// <summary>
        /// 创建三键字典实例。
        /// </summary>
        /// <param name="capacity">初始容量（会自动调整为大于等于 3 的质数）</param>
        public MultiKeyDictionary(int capacity = 0)
        {
            capacity = capacity < 3 ? 3 : MultiKeyHashHelpers.GetPrime(capacity);
            _buckets1 = new int[capacity];
            _buckets2 = new int[capacity];
            _buckets3 = new int[capacity];
            _entries1 = new Entry3<TKey1>[capacity];
            _entries2 = new Entry3<TKey2>[capacity];
            _entries3 = new Entry3<TKey3>[capacity];
            _values = new TValue[capacity];
            _valueFreeNext = new int[capacity];
            _c1 = EqualityComparer<TKey1>.Default;
            _c2 = EqualityComparer<TKey2>.Default;
            _c3 = EqualityComparer<TKey3>.Default;
            _freeList1 = _valueFreeList = -1;
        }

        /// <summary>获取字典中有效的 value 数量</summary>
        public int Count => _valueCount - _valueFreeCount;

        private int B1(int h) => (int)((uint)h % (uint)_buckets1.Length);
        private int B2(int h) => (int)((uint)h % (uint)_buckets2.Length);
        private int B3(int h) => (int)((uint)h % (uint)_buckets3.Length);

        private void EnsureCapacity1() { if (_count >= _entries1.Length) Resize1(); }
        private void EnsureCapacity2() { if (_count >= _entries2.Length) Resize2(); }
        private void EnsureCapacity3() { if (_count >= _entries3.Length) Resize3(); }
        private void EnsureValueCapacity()
        {
            if (_valueCount >= _values.Length)
            {
                int n = MultiKeyHashHelpers.ExpandPrime(_values.Length);
                var v = new TValue[n];
                var fn = new int[n];
                Array.Copy(_values, v, _valueCount);
                Array.Copy(_valueFreeNext, fn, _valueCount);
                _values = v;
                _valueFreeNext = fn;
            }
        }

        private void Resize1()
        {
            int n = MultiKeyHashHelpers.ExpandPrime(_entries1.Length);
            var b = new int[n];
            var e = new Entry3<TKey1>[n];
            Array.Copy(_entries1, e, _count);
            for (int i = 0; i < _count; i++)
            {
                if (e[i].next >= -1)
                {
                    int x = (int)((uint)e[i].hashCode % (uint)n);
                    e[i].link = b[x] - 1; b[x] = i + 1;
                }
            }
            _buckets1 = b; _entries1 = e;
        }
        private void Resize2()
        {
            int n = MultiKeyHashHelpers.ExpandPrime(_entries2.Length);
            var b = new int[n];
            var e = new Entry3<TKey2>[n];
            Array.Copy(_entries2, e, _count);
            for (int i = 0; i < _count; i++)
            {
                if (e[i].next >= -1)
                {
                    int x = (int)((uint)e[i].hashCode % (uint)n);
                    e[i].link = b[x] - 1; b[x] = i + 1;
                }
            }
            _buckets2 = b; _entries2 = e;
        }
        private void Resize3()
        {
            int n = MultiKeyHashHelpers.ExpandPrime(_entries3.Length);
            var b = new int[n];
            var e = new Entry3<TKey3>[n];
            Array.Copy(_entries3, e, _count);
            for (int i = 0; i < _count; i++)
            {
                if (e[i].next >= -1)
                {
                    int x = (int)((uint)e[i].hashCode % (uint)n);
                    e[i].link = b[x] - 1; b[x] = i + 1;
                }
            }
            _buckets3 = b; _entries3 = e;
        }

        private bool TryFindByKey1(TKey1 key1, out int idx1, out int valueIdx)
        {
            int h = _c1.GetHashCode(key1);
            int i = _buckets1[B1(h)] - 1;
            while (i >= 0)
            {
                ref var e = ref _entries1[i];
                if (e.hashCode == h && _c1.Equals(e.key, key1)) { idx1 = i; valueIdx = e.valueIndex; return true; }
                i = e.link;
            }
            idx1 = -1; valueIdx = -1; return false;
        }
        private bool TryFindByKey2(TKey2 key2, out int idx2, out int valueIdx)
        {
            int h = _c2.GetHashCode(key2);
            int i = _buckets2[B2(h)] - 1;
            while (i >= 0)
            {
                ref var e = ref _entries2[i];
                if (e.hashCode == h && _c2.Equals(e.key, key2)) { idx2 = i; valueIdx = e.valueIndex; return true; }
                i = e.link;
            }
            idx2 = -1; valueIdx = -1; return false;
        }
        private bool TryFindByKey3(TKey3 key3, out int idx3, out int valueIdx)
        {
            int h = _c3.GetHashCode(key3);
            int i = _buckets3[B3(h)] - 1;
            while (i >= 0)
            {
                ref var e = ref _entries3[i];
                if (e.hashCode == h && _c3.Equals(e.key, key3)) { idx3 = i; valueIdx = e.valueIndex; return true; }
                i = e.link;
            }
            idx3 = -1; valueIdx = -1; return false;
        }

        private void RemoveFromBucket1(int idx) { int h = _entries1[idx].hashCode; int b = B1(h); int i = _buckets1[b] - 1, last = -1; while (i >= 0) { if (i == idx) { if (last < 0) _buckets1[b] = _entries1[i].link + 1; else _entries1[last].link = _entries1[i].link; return; } last = i; i = _entries1[i].link; } }
        private void RemoveFromBucket2(int idx) { int h = _entries2[idx].hashCode; int b = B2(h); int i = _buckets2[b] - 1, last = -1; while (i >= 0) { if (i == idx) { if (last < 0) _buckets2[b] = _entries2[i].link + 1; else _entries2[last].link = _entries2[i].link; return; } last = i; i = _entries2[i].link; } }
        private void RemoveFromBucket3(int idx) { int h = _entries3[idx].hashCode; int b = B3(h); int i = _buckets3[b] - 1, last = -1; while (i >= 0) { if (i == idx) { if (last < 0) _buckets3[b] = _entries3[i].link + 1; else _entries3[last].link = _entries3[i].link; return; } last = i; i = _entries3[i].link; } }

        /// <summary>分配一行（三数组同索引），返回行索引</summary>
        private int AllocRow()
        {
            if (_freeCount > 0)
            {
                int i = _freeList1;
                _freeList1 = StartOfFreeList - _entries1[i].next;
                _freeCount--;
                return i;
            }
            EnsureCapacity1();
            EnsureCapacity2();
            EnsureCapacity3();
            return _count++;
        }
        private void FreeRow(int idx)
        {
            _entries1[idx].next = StartOfFreeList - _freeList1;
            _freeList1 = idx;
            _freeCount++;
        }
        private int AllocValue() { if (_valueFreeCount > 0) { int i = _valueFreeList; _valueFreeList = _valueFreeNext[i]; _valueFreeCount--; return i; } EnsureValueCapacity(); return _valueCount++; }
        private void FreeValue(int vi) { _values[vi] = default; _valueFreeNext[vi] = _valueFreeList; _valueFreeList = vi; _valueFreeCount++; }

        /// <summary>
        /// 按完整 key 组设置：先按 key1/key2/key3 移除可能冲突的项，再添加 (value, key1, key2, key3)。对同一 (key1, key2, key3) 多次调用结果一致（幂等）。
        /// </summary>
        public void Set(TValue value, TKey1 key1, TKey2 key2, TKey3 key3)
        {
            int h1 = _c1.GetHashCode(key1), h2 = _c2.GetHashCode(key2), h3 = _c3.GetHashCode(key3);
            RemoveByKey1(key1); RemoveByKey2(key2); RemoveByKey3(key3);
            int row = AllocRow(), vi = AllocValue();
            try
            {
                _values[vi] = value;
                ref var e1 = ref _entries1[row]; e1.next = -1; e1.linkTo1 = row; e1.linkTo2 = row; e1.linkTo3 = row; e1.hashCode = h1; e1.key = key1; e1.valueIndex = vi;
                ref var e2 = ref _entries2[row]; e2.next = -1; e2.linkTo1 = row; e2.linkTo2 = row; e2.linkTo3 = row; e2.hashCode = h2; e2.key = key2; e2.valueIndex = vi;
                ref var e3 = ref _entries3[row]; e3.next = -1; e3.linkTo1 = row; e3.linkTo2 = row; e3.linkTo3 = row; e3.hashCode = h3; e3.key = key3; e3.valueIndex = vi;
                int bx = B1(h1); e1.link = _buckets1[bx] - 1; _buckets1[bx] = row + 1;
                bx = B2(h2); e2.link = _buckets2[bx] - 1; _buckets2[bx] = row + 1;
                bx = B3(h3); e3.link = _buckets3[bx] - 1; _buckets3[bx] = row + 1;
            }
            catch
            {
                FreeRow(row); FreeValue(vi);
                throw;
            }
        }

        /// <summary>通过 key1 获取 value</summary>
        public TValue GetByKey1(TKey1 key1) => TryGetByKey1(key1, out var v) ? v : default;
        /// <summary>通过 key2 获取 value</summary>
        public TValue GetByKey2(TKey2 key2) => TryGetByKey2(key2, out var v) ? v : default;
        /// <summary>通过 key3 获取 value</summary>
        public TValue GetByKey3(TKey3 key3) => TryGetByKey3(key3, out var v) ? v : default;
        /// <summary>尝试通过 key1 获取 value</summary>
        public bool TryGetValueByKey1(TKey1 key1, out TValue value) => TryGetByKey1(key1, out value);
        /// <summary>尝试通过 key2 获取 value</summary>
        public bool TryGetValueByKey2(TKey2 key2, out TValue value) => TryGetByKey2(key2, out value);
        /// <summary>尝试通过 key3 获取 value</summary>
        public bool TryGetValueByKey3(TKey3 key3, out TValue value) => TryGetByKey3(key3, out value);

        private bool TryGetByKey1(TKey1 key1, out TValue value) { if (TryFindByKey1(key1, out _, out int vi)) { value = _values[vi]; return true; } value = default; return false; }
        private bool TryGetByKey2(TKey2 key2, out TValue value) { if (TryFindByKey2(key2, out _, out int vi)) { value = _values[vi]; return true; } value = default; return false; }
        private bool TryGetByKey3(TKey3 key3, out TValue value) { if (TryFindByKey3(key3, out _, out int vi)) { value = _values[vi]; return true; } value = default; return false; }

        /// <summary>检查 key1 是否存在</summary>
        public bool ContainsKey1(TKey1 key1) => TryFindByKey1(key1, out _, out _);
        /// <summary>检查 key2 是否存在</summary>
        public bool ContainsKey2(TKey2 key2) => TryFindByKey2(key2, out _, out _);
        /// <summary>检查 key3 是否存在</summary>
        public bool ContainsKey3(TKey3 key3) => TryFindByKey3(key3, out _, out _);

        /// <summary>检查 value 是否存在（线性查找）</summary>
        public bool ContainsValue(TValue value)
        {
            var cmp = EqualityComparer<TValue>.Default;
            for (int i = 0; i < _count; i++)
                if (_entries1[i].next >= -1 && cmp.Equals(_values[_entries1[i].valueIndex], value)) return true;
            return false;
        }

        /// <summary>通过 key1 移除项</summary>
        public bool RemoveByKey1(TKey1 key1) { if (!TryFindByKey1(key1, out int idx1, out int vi)) return false; int row = idx1; RemoveFromBucket1(idx1); RemoveFromBucket2(row); RemoveFromBucket3(row); FreeRow(row); FreeValue(vi); return true; }
        /// <summary>通过 key2 移除项</summary>
        public bool RemoveByKey2(TKey2 key2) { if (!TryFindByKey2(key2, out int idx2, out int vi)) return false; int row = idx2; RemoveFromBucket1(row); RemoveFromBucket2(idx2); RemoveFromBucket3(row); FreeRow(row); FreeValue(vi); return true; }
        /// <summary>通过 key3 移除项</summary>
        public bool RemoveByKey3(TKey3 key3) { if (!TryFindByKey3(key3, out int idx3, out int vi)) return false; int row = idx3; RemoveFromBucket1(row); RemoveFromBucket2(row); RemoveFromBucket3(idx3); FreeRow(row); FreeValue(vi); return true; }

        /// <summary>通过 value 移除项（线性查找）</summary>
        public bool Remove(TValue value)
        {
            var cmp = EqualityComparer<TValue>.Default;
            for (int i = 0; i < _count; i++)
            {
                if (_entries1[i].next >= -1 && cmp.Equals(_values[_entries1[i].valueIndex], value))
                {
                    int row = i, vi = _entries1[i].valueIndex;
                    RemoveFromBucket1(row); RemoveFromBucket2(row); RemoveFromBucket3(row);
                    FreeRow(row); FreeValue(vi);
                    return true;
                }
            }
            return false;
        }

        /// <summary>根据 value 获取对应的三个 key（线性查找）</summary>
        public (TKey1, TKey2, TKey3) GetKeys(TValue value)
        {
            var cmp = EqualityComparer<TValue>.Default;
            for (int i = 0; i < _count; i++)
                if (_entries1[i].next >= -1 && cmp.Equals(_values[_entries1[i].valueIndex], value))
                    return (_entries1[i].key, _entries2[i].key, _entries3[i].key);
            return default;
        }

        /// <summary>清空字典</summary>
        public void Clear()
        {
            Array.Fill(_buckets1, 0); Array.Fill(_buckets2, 0); Array.Fill(_buckets3, 0);
            Array.Clear(_entries1, 0, _count); Array.Clear(_entries2, 0, _count); Array.Clear(_entries3, 0, _count);
            Array.Clear(_values, 0, _valueCount); Array.Clear(_valueFreeNext, 0, _valueCount);
            _count = _valueCount = 0;
            _freeList1 = _valueFreeList = -1;
            _freeCount = _valueFreeCount = 0;
        }

        /// <summary>通过 key1 索引访问</summary>
        public TValue this[TKey1 key1] { get => GetByKey1(key1); }
        /// <summary>通过 key2 索引访问</summary>
        public TValue this[TKey2 key2] { get => GetByKey2(key2); }
        /// <summary>通过 key3 索引访问</summary>
        public TValue this[TKey3 key3] { get => GetByKey3(key3); }

        /// <summary>迭代器排列组合：0=val, 1=k1, 2=k2, 3=k3（按 _entries1 顺序）</summary>
        public IEnumerable<TValue> GetIter0() { for (int i = 0; i < _count; i++) if (_entries1[i].next >= -1) yield return _values[_entries1[i].valueIndex]; }
        public IEnumerable<TKey1> GetIter1() { for (int i = 0; i < _count; i++) if (_entries1[i].next >= -1) yield return _entries1[i].key; }
        public IEnumerable<TKey2> GetIter2() { for (int i = 0; i < _count; i++) if (_entries2[i].next >= -1) yield return _entries2[i].key; }
        public IEnumerable<TKey3> GetIter3() { for (int i = 0; i < _count; i++) if (_entries3[i].next >= -1) yield return _entries3[i].key; }
        public IEnumerable<(TValue, TKey1)> GetIter01() { for (int i = 0; i < _count; i++) if (_entries1[i].next >= -1) yield return (_values[_entries1[i].valueIndex], _entries1[i].key); }
        public IEnumerable<(TValue, TKey2)> GetIter02() { for (int i = 0; i < _count; i++) if (_entries1[i].next >= -1) yield return (_values[_entries1[i].valueIndex], _entries2[_entries1[i].linkTo2].key); }
        public IEnumerable<(TValue, TKey3)> GetIter03() { for (int i = 0; i < _count; i++) if (_entries1[i].next >= -1) yield return (_values[_entries1[i].valueIndex], _entries3[_entries1[i].linkTo3].key); }
        public IEnumerable<(TKey1, TKey2)> GetIter12() { for (int i = 0; i < _count; i++) if (_entries1[i].next >= -1) yield return (_entries1[i].key, _entries2[_entries1[i].linkTo2].key); }
        public IEnumerable<(TKey1, TKey3)> GetIter13() { for (int i = 0; i < _count; i++) if (_entries1[i].next >= -1) yield return (_entries1[i].key, _entries3[_entries1[i].linkTo3].key); }
        public IEnumerable<(TKey2, TKey3)> GetIter23() { for (int i = 0; i < _count; i++) if (_entries1[i].next >= -1) yield return (_entries2[_entries1[i].linkTo2].key, _entries3[_entries1[i].linkTo3].key); }
        public IEnumerable<(TValue, TKey1, TKey2)> GetIter012() { for (int i = 0; i < _count; i++) if (_entries1[i].next >= -1) yield return (_values[_entries1[i].valueIndex], _entries1[i].key, _entries2[_entries1[i].linkTo2].key); }
        public IEnumerable<(TValue, TKey1, TKey3)> GetIter013() { for (int i = 0; i < _count; i++) if (_entries1[i].next >= -1) yield return (_values[_entries1[i].valueIndex], _entries1[i].key, _entries3[_entries1[i].linkTo3].key); }
        public IEnumerable<(TValue, TKey2, TKey3)> GetIter023() { for (int i = 0; i < _count; i++) if (_entries1[i].next >= -1) yield return (_values[_entries1[i].valueIndex], _entries2[_entries1[i].linkTo2].key, _entries3[_entries1[i].linkTo3].key); }
        public IEnumerable<(TKey1, TKey2, TKey3)> GetIter123() { for (int i = 0; i < _count; i++) if (_entries1[i].next >= -1) yield return (_entries1[i].key, _entries2[_entries1[i].linkTo2].key, _entries3[_entries1[i].linkTo3].key); }
        public IEnumerable<(TValue, TKey1, TKey2, TKey3)> GetIter0123() { for (int i = 0; i < _count; i++) if (_entries1[i].next >= -1) yield return (_values[_entries1[i].valueIndex], _entries1[i].key, _entries2[_entries1[i].linkTo2].key, _entries3[_entries1[i].linkTo3].key); }

        public IEnumerable<TValue> Values => GetIter0();
        public IEnumerable<TKey1> Keys1 => GetIter1();
        public IEnumerable<TKey2> Keys2 => GetIter2();
        public IEnumerable<TKey3> Keys3 => GetIter3();
        public IEnumerable<(TValue value, TKey1 key1, TKey2 key2, TKey3 key3)> Pairs => GetIter0123();

        public IEnumerable<T> Enumerate<T>(MultiKeyIterFlags flags)
        {
            if (flags == MultiKeyIterFlags.Value) return (IEnumerable<T>)GetIter0();
            if (flags == MultiKeyIterFlags.Key1) return (IEnumerable<T>)GetIter1();
            if (flags == MultiKeyIterFlags.Key2) return (IEnumerable<T>)GetIter2();
            if (flags == MultiKeyIterFlags.Key3) return (IEnumerable<T>)GetIter3();
            throw new ArgumentException($"单元素迭代请使用 Value/Key1/Key2/Key3，当前 flags={flags}");
        }

        public IEnumerable<(T1, T2)> Enumerate<T1, T2>(MultiKeyIterFlags flags)
        {
            if (flags == (MultiKeyIterFlags.Key1 | MultiKeyIterFlags.Key2)) return (IEnumerable<(T1, T2)>)GetIter12();
            throw new ArgumentException($"双元素迭代需 flags=Key1|Key2，当前 flags={flags}");
        }

        public IEnumerable<(T1, T2, T3)> Enumerate<T1, T2, T3>(MultiKeyIterFlags flags)
        {
            if (flags == (MultiKeyIterFlags.Key1 | MultiKeyIterFlags.Key2 | MultiKeyIterFlags.Key3)) return (IEnumerable<(T1, T2, T3)>)GetIter123();
            throw new ArgumentException($"三元素迭代需 flags=Key1|Key2|Key3，当前 flags={flags}");
        }

        public IEnumerable<(T1, T2, T3, T4)> Enumerate<T1, T2, T3, T4>(MultiKeyIterFlags flags)
        {
            var full = MultiKeyIterFlags.Value | MultiKeyIterFlags.Key1 | MultiKeyIterFlags.Key2 | MultiKeyIterFlags.Key3;
            if (flags != full) throw new ArgumentException($"四元素迭代需 flags=Value|Key1|Key2|Key3，当前 flags={flags}");
            return (IEnumerable<(T1, T2, T3, T4)>)GetIter0123();
        }

        public IEnumerator<TValue> GetEnumerator() => Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool RemoveKey1(TKey1 key1) => RemoveByKey1(key1);
        public bool RemoveKey2(TKey2 key2) => RemoveByKey2(key2);
        public bool RemoveKey3(TKey3 key3) => RemoveByKey3(key3);
    }

    /// <summary>
    /// 四键字典：支持通过四个不同的 key 访问同一个 value。
    /// <para>
    /// 架构说明：每个 key 维护独立的 Entry 数组（_entries1/2/3/4），
    /// 通过 linkTo1/2/3/4 字段相互关联，value 只在 _values 数组中存储一份。
    /// </para>
    /// </summary>
    public class MultiKeyDictionary<TKey1, TKey2, TKey3, TKey4, TValue> : IEnumerable<TValue>
        where TKey1 : notnull
        where TKey2 : notnull
        where TKey3 : notnull
        where TKey4 : notnull
    {
        private const int StartOfFreeList = -3;

        private int[] _buckets1, _buckets2, _buckets3, _buckets4;
        private Entry4<TKey1>[] _entries1;
        private Entry4<TKey2>[] _entries2;
        private Entry4<TKey3>[] _entries3;
        private Entry4<TKey4>[] _entries4;
        private TValue[] _values;
        private int[] _valueFreeNext;
        /// <summary>行数（每行占 entries1/2/3/4 各一槽，同一索引）</summary>
        private int _count;
        private int _freeList1;
        private int _freeCount;
        private int _valueCount, _valueFreeList, _valueFreeCount;
        private readonly IEqualityComparer<TKey1> _c1;
        private readonly IEqualityComparer<TKey2> _c2;
        private readonly IEqualityComparer<TKey3> _c3;
        private readonly IEqualityComparer<TKey4> _c4;

        /// <summary>
        /// Entry 结构：存储单个 key 的哈希表节点信息。
        /// </summary>
        private struct Entry4<T>
        {
            public int next;       // freeList 链表指针
            public int link;       // bucket 链表指针
            public int linkTo1, linkTo2, linkTo3, linkTo4; // 关联的 _entries1/2/3/4 索引
            public int hashCode;   // 缓存的哈希码
            public T key;          // key 值
            public int valueIndex; // 关联的 _values 数组索引
        }

        /// <summary>
        /// 创建四键字典实例。
        /// </summary>
        /// <param name="capacity">初始容量（会自动调整为大于等于 3 的质数）</param>
        public MultiKeyDictionary(int capacity = 0)
        {
            capacity = capacity < 3 ? 3 : MultiKeyHashHelpers.GetPrime(capacity);
            _buckets1 = new int[capacity];
            _buckets2 = new int[capacity];
            _buckets3 = new int[capacity];
            _buckets4 = new int[capacity];
            _entries1 = new Entry4<TKey1>[capacity];
            _entries2 = new Entry4<TKey2>[capacity];
            _entries3 = new Entry4<TKey3>[capacity];
            _entries4 = new Entry4<TKey4>[capacity];
            _values = new TValue[capacity];
            _valueFreeNext = new int[capacity];
            _c1 = EqualityComparer<TKey1>.Default;
            _c2 = EqualityComparer<TKey2>.Default;
            _c3 = EqualityComparer<TKey3>.Default;
            _c4 = EqualityComparer<TKey4>.Default;
            _freeList1 = _valueFreeList = -1;
        }

        /// <summary>获取字典中有效的 value 数量</summary>
        public int Count => _valueCount - _valueFreeCount;

        private int B(int h, int[] b) => (int)((uint)h % (uint)b.Length);

        private void EnsureCapacity1() { if (_count >= _entries1.Length) Resize1(); }
        private void EnsureCapacity2() { if (_count >= _entries2.Length) Resize2(); }
        private void EnsureCapacity3() { if (_count >= _entries3.Length) Resize3(); }
        private void EnsureCapacity4() { if (_count >= _entries4.Length) Resize4(); }
        private void EnsureValueCapacity() { if (_valueCount >= _values.Length) { int n = MultiKeyHashHelpers.ExpandPrime(_values.Length); var v = new TValue[n]; var fn = new int[n]; Array.Copy(_values, v, _valueCount); Array.Copy(_valueFreeNext, fn, _valueCount); _values = v; _valueFreeNext = fn; } }

        private void Resize1() { int n = MultiKeyHashHelpers.ExpandPrime(_entries1.Length); var b = new int[n]; var e = new Entry4<TKey1>[n]; Array.Copy(_entries1, e, _count); for (int i = 0; i < _count; i++) { if (e[i].next >= -1) { int x = B(e[i].hashCode, b); e[i].link = b[x] - 1; b[x] = i + 1; } } _buckets1 = b; _entries1 = e; }
        private void Resize2() { int n = MultiKeyHashHelpers.ExpandPrime(_entries2.Length); var b = new int[n]; var e = new Entry4<TKey2>[n]; Array.Copy(_entries2, e, _count); for (int i = 0; i < _count; i++) { if (e[i].next >= -1) { int x = B(e[i].hashCode, b); e[i].link = b[x] - 1; b[x] = i + 1; } } _buckets2 = b; _entries2 = e; }
        private void Resize3() { int n = MultiKeyHashHelpers.ExpandPrime(_entries3.Length); var b = new int[n]; var e = new Entry4<TKey3>[n]; Array.Copy(_entries3, e, _count); for (int i = 0; i < _count; i++) { if (e[i].next >= -1) { int x = B(e[i].hashCode, b); e[i].link = b[x] - 1; b[x] = i + 1; } } _buckets3 = b; _entries3 = e; }
        private void Resize4() { int n = MultiKeyHashHelpers.ExpandPrime(_entries4.Length); var b = new int[n]; var e = new Entry4<TKey4>[n]; Array.Copy(_entries4, e, _count); for (int i = 0; i < _count; i++) { if (e[i].next >= -1) { int x = B(e[i].hashCode, b); e[i].link = b[x] - 1; b[x] = i + 1; } } _buckets4 = b; _entries4 = e; }

        private bool TryFindByKey1(TKey1 k, out int idx1, out int vi) { int h = _c1.GetHashCode(k); int i = _buckets1[B(h, _buckets1)] - 1; while (i >= 0) { if (_entries1[i].hashCode == h && _c1.Equals(_entries1[i].key, k)) { idx1 = i; vi = _entries1[i].valueIndex; return true; } i = _entries1[i].link; } idx1 = -1; vi = -1; return false; }
        private bool TryFindByKey2(TKey2 k, out int idx2, out int vi) { int h = _c2.GetHashCode(k); int i = _buckets2[B(h, _buckets2)] - 1; while (i >= 0) { if (_entries2[i].hashCode == h && _c2.Equals(_entries2[i].key, k)) { idx2 = i; vi = _entries2[i].valueIndex; return true; } i = _entries2[i].link; } idx2 = -1; vi = -1; return false; }
        private bool TryFindByKey3(TKey3 k, out int idx3, out int vi) { int h = _c3.GetHashCode(k); int i = _buckets3[B(h, _buckets3)] - 1; while (i >= 0) { if (_entries3[i].hashCode == h && _c3.Equals(_entries3[i].key, k)) { idx3 = i; vi = _entries3[i].valueIndex; return true; } i = _entries3[i].link; } idx3 = -1; vi = -1; return false; }
        private bool TryFindByKey4(TKey4 k, out int idx4, out int vi) { int h = _c4.GetHashCode(k); int i = _buckets4[B(h, _buckets4)] - 1; while (i >= 0) { if (_entries4[i].hashCode == h && _c4.Equals(_entries4[i].key, k)) { idx4 = i; vi = _entries4[i].valueIndex; return true; } i = _entries4[i].link; } idx4 = -1; vi = -1; return false; }

        private void Rm1(int idx) { int h = _entries1[idx].hashCode; int x = B(h, _buckets1); int i = _buckets1[x] - 1, last = -1; while (i >= 0) { if (i == idx) { if (last < 0) _buckets1[x] = _entries1[i].link + 1; else _entries1[last].link = _entries1[i].link; return; } last = i; i = _entries1[i].link; } }
        private void Rm2(int idx) { int h = _entries2[idx].hashCode; int x = B(h, _buckets2); int i = _buckets2[x] - 1, last = -1; while (i >= 0) { if (i == idx) { if (last < 0) _buckets2[x] = _entries2[i].link + 1; else _entries2[last].link = _entries2[i].link; return; } last = i; i = _entries2[i].link; } }
        private void Rm3(int idx) { int h = _entries3[idx].hashCode; int x = B(h, _buckets3); int i = _buckets3[x] - 1, last = -1; while (i >= 0) { if (i == idx) { if (last < 0) _buckets3[x] = _entries3[i].link + 1; else _entries3[last].link = _entries3[i].link; return; } last = i; i = _entries3[i].link; } }
        private void Rm4(int idx) { int h = _entries4[idx].hashCode; int x = B(h, _buckets4); int i = _buckets4[x] - 1, last = -1; while (i >= 0) { if (i == idx) { if (last < 0) _buckets4[x] = _entries4[i].link + 1; else _entries4[last].link = _entries4[i].link; return; } last = i; i = _entries4[i].link; } }

        /// <summary>分配一行（四数组同索引），返回行索引</summary>
        private int AllocRow()
        {
            if (_freeCount > 0)
            {
                int i = _freeList1;
                _freeList1 = StartOfFreeList - _entries1[i].next;
                _freeCount--;
                return i;
            }
            EnsureCapacity1();
            EnsureCapacity2();
            EnsureCapacity3();
            EnsureCapacity4();
            return _count++;
        }
        private void FreeRow(int idx)
        {
            _entries1[idx].next = StartOfFreeList - _freeList1;
            _freeList1 = idx;
            _freeCount++;
        }
        private int AllocV() { if (_valueFreeCount > 0) { int i = _valueFreeList; _valueFreeList = _valueFreeNext[i]; _valueFreeCount--; return i; } EnsureValueCapacity(); return _valueCount++; }
        private void FreeV(int vi) { _values[vi] = default; _valueFreeNext[vi] = _valueFreeList; _valueFreeList = vi; _valueFreeCount++; }

        /// <summary>
        /// 按完整 key 组设置：先按 key1/key2/key3/key4 移除可能冲突的项，再添加 (value, key1, key2, key3, key4)。对同一 (key1, key2, key3, key4) 多次调用结果一致（幂等）。
        /// </summary>
        public void Set(TValue value, TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4)
        {
            int h1 = _c1.GetHashCode(key1), h2 = _c2.GetHashCode(key2), h3 = _c3.GetHashCode(key3), h4 = _c4.GetHashCode(key4);
            RemoveByKey1(key1); RemoveByKey2(key2); RemoveByKey3(key3); RemoveByKey4(key4);
            int row = AllocRow(), vi = AllocV();
            try
            {
                _values[vi] = value;
                ref var e1r = ref _entries1[row]; e1r.next = -1; e1r.linkTo1 = row; e1r.linkTo2 = row; e1r.linkTo3 = row; e1r.linkTo4 = row; e1r.hashCode = h1; e1r.key = key1; e1r.valueIndex = vi;
                ref var e2r = ref _entries2[row]; e2r.next = -1; e2r.linkTo1 = row; e2r.linkTo2 = row; e2r.linkTo3 = row; e2r.linkTo4 = row; e2r.hashCode = h2; e2r.key = key2; e2r.valueIndex = vi;
                ref var e3r = ref _entries3[row]; e3r.next = -1; e3r.linkTo1 = row; e3r.linkTo2 = row; e3r.linkTo3 = row; e3r.linkTo4 = row; e3r.hashCode = h3; e3r.key = key3; e3r.valueIndex = vi;
                ref var e4r = ref _entries4[row]; e4r.next = -1; e4r.linkTo1 = row; e4r.linkTo2 = row; e4r.linkTo3 = row; e4r.linkTo4 = row; e4r.hashCode = h4; e4r.key = key4; e4r.valueIndex = vi;
                int bx = B(h1, _buckets1); e1r.link = _buckets1[bx] - 1; _buckets1[bx] = row + 1;
                bx = B(h2, _buckets2); e2r.link = _buckets2[bx] - 1; _buckets2[bx] = row + 1;
                bx = B(h3, _buckets3); e3r.link = _buckets3[bx] - 1; _buckets3[bx] = row + 1;
                bx = B(h4, _buckets4); e4r.link = _buckets4[bx] - 1; _buckets4[bx] = row + 1;
            }
            catch
            {
                FreeRow(row); FreeV(vi);
                throw;
            }
        }

        /// <summary>通过 key1 获取 value</summary>
        public TValue GetByKey1(TKey1 k) => TryGetByKey1(k, out var v) ? v : default;
        /// <summary>通过 key2 获取 value</summary>
        public TValue GetByKey2(TKey2 k) => TryGetByKey2(k, out var v) ? v : default;
        /// <summary>通过 key3 获取 value</summary>
        public TValue GetByKey3(TKey3 k) => TryGetByKey3(k, out var v) ? v : default;
        /// <summary>通过 key4 获取 value</summary>
        public TValue GetByKey4(TKey4 k) => TryGetByKey4(k, out var v) ? v : default;
        /// <summary>尝试通过 key1 获取 value</summary>
        public bool TryGetValueByKey1(TKey1 k, out TValue v) => TryGetByKey1(k, out v);
        /// <summary>尝试通过 key2 获取 value</summary>
        public bool TryGetValueByKey2(TKey2 k, out TValue v) => TryGetByKey2(k, out v);
        /// <summary>尝试通过 key3 获取 value</summary>
        public bool TryGetValueByKey3(TKey3 k, out TValue v) => TryGetByKey3(k, out v);
        /// <summary>尝试通过 key4 获取 value</summary>
        public bool TryGetValueByKey4(TKey4 k, out TValue v) => TryGetByKey4(k, out v);

        private bool TryGetByKey1(TKey1 k, out TValue v) { if (TryFindByKey1(k, out _, out int vi)) { v = _values[vi]; return true; } v = default; return false; }
        private bool TryGetByKey2(TKey2 k, out TValue v) { if (TryFindByKey2(k, out _, out int vi)) { v = _values[vi]; return true; } v = default; return false; }
        private bool TryGetByKey3(TKey3 k, out TValue v) { if (TryFindByKey3(k, out _, out int vi)) { v = _values[vi]; return true; } v = default; return false; }
        private bool TryGetByKey4(TKey4 k, out TValue v) { if (TryFindByKey4(k, out _, out int vi)) { v = _values[vi]; return true; } v = default; return false; }

        /// <summary>检查 key1 是否存在</summary>
        public bool ContainsKey1(TKey1 k) => TryFindByKey1(k, out _, out _);
        /// <summary>检查 key2 是否存在</summary>
        public bool ContainsKey2(TKey2 k) => TryFindByKey2(k, out _, out _);
        /// <summary>检查 key3 是否存在</summary>
        public bool ContainsKey3(TKey3 k) => TryFindByKey3(k, out _, out _);
        /// <summary>检查 key4 是否存在</summary>
        public bool ContainsKey4(TKey4 k) => TryFindByKey4(k, out _, out _);

        /// <summary>检查 value 是否存在（线性查找）</summary>
        public bool ContainsValue(TValue v) { var cmp = EqualityComparer<TValue>.Default; for (int i = 0; i < _count; i++) if (_entries1[i].next >= -1 && cmp.Equals(_values[_entries1[i].valueIndex], v)) return true; return false; }

        /// <summary>通过 key1 移除项</summary>
        public bool RemoveByKey1(TKey1 k) { if (!TryFindByKey1(k, out int i1, out int vi)) return false; int row = i1; Rm1(i1); Rm2(row); Rm3(row); Rm4(row); FreeRow(row); FreeV(vi); return true; }
        /// <summary>通过 key2 移除项</summary>
        public bool RemoveByKey2(TKey2 k) { if (!TryFindByKey2(k, out int i2, out int vi)) return false; int row = i2; Rm1(row); Rm2(i2); Rm3(row); Rm4(row); FreeRow(row); FreeV(vi); return true; }
        /// <summary>通过 key3 移除项</summary>
        public bool RemoveByKey3(TKey3 k) { if (!TryFindByKey3(k, out int i3, out int vi)) return false; int row = i3; Rm1(row); Rm2(row); Rm3(i3); Rm4(row); FreeRow(row); FreeV(vi); return true; }
        /// <summary>通过 key4 移除项</summary>
        public bool RemoveByKey4(TKey4 k) { if (!TryFindByKey4(k, out int i4, out int vi)) return false; int row = i4; Rm1(row); Rm2(row); Rm3(row); Rm4(i4); FreeRow(row); FreeV(vi); return true; }

        /// <summary>通过 value 移除项（线性查找）</summary>
        public bool Remove(TValue v) { var cmp = EqualityComparer<TValue>.Default; for (int i = 0; i < _count; i++) { if (_entries1[i].next >= -1 && cmp.Equals(_values[_entries1[i].valueIndex], v)) { int row = i; Rm1(row); Rm2(row); Rm3(row); Rm4(row); FreeRow(row); FreeV(_entries1[i].valueIndex); return true; } } return false; }

        /// <summary>根据 value 获取对应的四个 key（线性查找）</summary>
        public (TKey1, TKey2, TKey3, TKey4) GetKeys(TValue v) { var cmp = EqualityComparer<TValue>.Default; for (int i = 0; i < _count; i++) if (_entries1[i].next >= -1 && cmp.Equals(_values[_entries1[i].valueIndex], v)) return (_entries1[i].key, _entries2[i].key, _entries3[i].key, _entries4[i].key); return default; }

        /// <summary>清空字典</summary>
        public void Clear() { Array.Fill(_buckets1, 0); Array.Fill(_buckets2, 0); Array.Fill(_buckets3, 0); Array.Fill(_buckets4, 0); Array.Clear(_entries1, 0, _count); Array.Clear(_entries2, 0, _count); Array.Clear(_entries3, 0, _count); Array.Clear(_entries4, 0, _count); Array.Clear(_values, 0, _valueCount); Array.Clear(_valueFreeNext, 0, _valueCount); _count = _valueCount = 0; _freeList1 = _valueFreeList = -1; _freeCount = _valueFreeCount = 0; }

        /// <summary>通过 key1 索引访问</summary>
        public TValue this[TKey1 k] { get => GetByKey1(k); }
        /// <summary>通过 key2 索引访问</summary>
        public TValue this[TKey2 k] { get => GetByKey2(k); }
        /// <summary>通过 key3 索引访问</summary>
        public TValue this[TKey3 k] { get => GetByKey3(k); }
        /// <summary>通过 key4 索引访问</summary>
        public TValue this[TKey4 k] { get => GetByKey4(k); }

        /// <summary>迭代器排列组合：0=val, 1=k1, 2=k2, 3=k3, 4=k4（按 _entries1 顺序）</summary>
        public IEnumerable<TValue> GetIter0() { for (int i = 0; i < _count; i++) if (_entries1[i].next >= -1) yield return _values[_entries1[i].valueIndex]; }
        public IEnumerable<TKey1> GetIter1() { for (int i = 0; i < _count; i++) if (_entries1[i].next >= -1) yield return _entries1[i].key; }
        public IEnumerable<TKey2> GetIter2() { for (int i = 0; i < _count; i++) if (_entries2[i].next >= -1) yield return _entries2[i].key; }
        public IEnumerable<TKey3> GetIter3() { for (int i = 0; i < _count; i++) if (_entries3[i].next >= -1) yield return _entries3[i].key; }
        public IEnumerable<TKey4> GetIter4() { for (int i = 0; i < _count; i++) if (_entries4[i].next >= -1) yield return _entries4[i].key; }
        public IEnumerable<(TValue, TKey1)> GetIter01() { for (int i = 0; i < _count; i++) if (_entries1[i].next >= -1) yield return (_values[_entries1[i].valueIndex], _entries1[i].key); }
        public IEnumerable<(TValue, TKey2)> GetIter02() { for (int i = 0; i < _count; i++) if (_entries1[i].next >= -1) yield return (_values[_entries1[i].valueIndex], _entries2[_entries1[i].linkTo2].key); }
        public IEnumerable<(TValue, TKey3)> GetIter03() { for (int i = 0; i < _count; i++) if (_entries1[i].next >= -1) yield return (_values[_entries1[i].valueIndex], _entries3[_entries1[i].linkTo3].key); }
        public IEnumerable<(TValue, TKey4)> GetIter04() { for (int i = 0; i < _count; i++) if (_entries1[i].next >= -1) yield return (_values[_entries1[i].valueIndex], _entries4[_entries1[i].linkTo4].key); }
        public IEnumerable<(TKey1, TKey2)> GetIter12() { for (int i = 0; i < _count; i++) if (_entries1[i].next >= -1) yield return (_entries1[i].key, _entries2[_entries1[i].linkTo2].key); }
        public IEnumerable<(TKey1, TKey3)> GetIter13() { for (int i = 0; i < _count; i++) if (_entries1[i].next >= -1) yield return (_entries1[i].key, _entries3[_entries1[i].linkTo3].key); }
        public IEnumerable<(TKey1, TKey4)> GetIter14() { for (int i = 0; i < _count; i++) if (_entries1[i].next >= -1) yield return (_entries1[i].key, _entries4[_entries1[i].linkTo4].key); }
        public IEnumerable<(TKey2, TKey3)> GetIter23() { for (int i = 0; i < _count; i++) if (_entries1[i].next >= -1) yield return (_entries2[_entries1[i].linkTo2].key, _entries3[_entries1[i].linkTo3].key); }
        public IEnumerable<(TKey2, TKey4)> GetIter24() { for (int i = 0; i < _count; i++) if (_entries1[i].next >= -1) yield return (_entries2[_entries1[i].linkTo2].key, _entries4[_entries1[i].linkTo4].key); }
        public IEnumerable<(TKey3, TKey4)> GetIter34() { for (int i = 0; i < _count; i++) if (_entries1[i].next >= -1) yield return (_entries3[_entries1[i].linkTo3].key, _entries4[_entries1[i].linkTo4].key); }
        public IEnumerable<(TValue, TKey1, TKey2)> GetIter012() { for (int i = 0; i < _count; i++) if (_entries1[i].next >= -1) yield return (_values[_entries1[i].valueIndex], _entries1[i].key, _entries2[_entries1[i].linkTo2].key); }
        public IEnumerable<(TValue, TKey1, TKey3)> GetIter013() { for (int i = 0; i < _count; i++) if (_entries1[i].next >= -1) yield return (_values[_entries1[i].valueIndex], _entries1[i].key, _entries3[_entries1[i].linkTo3].key); }
        public IEnumerable<(TValue, TKey1, TKey4)> GetIter014() { for (int i = 0; i < _count; i++) if (_entries1[i].next >= -1) yield return (_values[_entries1[i].valueIndex], _entries1[i].key, _entries4[_entries1[i].linkTo4].key); }
        public IEnumerable<(TValue, TKey2, TKey3)> GetIter023() { for (int i = 0; i < _count; i++) if (_entries1[i].next >= -1) yield return (_values[_entries1[i].valueIndex], _entries2[_entries1[i].linkTo2].key, _entries3[_entries1[i].linkTo3].key); }
        public IEnumerable<(TValue, TKey2, TKey4)> GetIter024() { for (int i = 0; i < _count; i++) if (_entries1[i].next >= -1) yield return (_values[_entries1[i].valueIndex], _entries2[_entries1[i].linkTo2].key, _entries4[_entries1[i].linkTo4].key); }
        public IEnumerable<(TValue, TKey3, TKey4)> GetIter034() { for (int i = 0; i < _count; i++) if (_entries1[i].next >= -1) yield return (_values[_entries1[i].valueIndex], _entries3[_entries1[i].linkTo3].key, _entries4[_entries1[i].linkTo4].key); }
        public IEnumerable<(TKey1, TKey2, TKey3)> GetIter123() { for (int i = 0; i < _count; i++) if (_entries1[i].next >= -1) yield return (_entries1[i].key, _entries2[_entries1[i].linkTo2].key, _entries3[_entries1[i].linkTo3].key); }
        public IEnumerable<(TKey1, TKey2, TKey4)> GetIter124() { for (int i = 0; i < _count; i++) if (_entries1[i].next >= -1) yield return (_entries1[i].key, _entries2[_entries1[i].linkTo2].key, _entries4[_entries1[i].linkTo4].key); }
        public IEnumerable<(TKey1, TKey3, TKey4)> GetIter134() { for (int i = 0; i < _count; i++) if (_entries1[i].next >= -1) yield return (_entries1[i].key, _entries3[_entries1[i].linkTo3].key, _entries4[_entries1[i].linkTo4].key); }
        public IEnumerable<(TKey2, TKey3, TKey4)> GetIter234() { for (int i = 0; i < _count; i++) if (_entries1[i].next >= -1) yield return (_entries2[_entries1[i].linkTo2].key, _entries3[_entries1[i].linkTo3].key, _entries4[_entries1[i].linkTo4].key); }
        public IEnumerable<(TValue, TKey1, TKey2, TKey3)> GetIter0123() { for (int i = 0; i < _count; i++) if (_entries1[i].next >= -1) yield return (_values[_entries1[i].valueIndex], _entries1[i].key, _entries2[_entries1[i].linkTo2].key, _entries3[_entries1[i].linkTo3].key); }
        public IEnumerable<(TValue, TKey1, TKey2, TKey4)> GetIter0124() { for (int i = 0; i < _count; i++) if (_entries1[i].next >= -1) yield return (_values[_entries1[i].valueIndex], _entries1[i].key, _entries2[_entries1[i].linkTo2].key, _entries4[_entries1[i].linkTo4].key); }
        public IEnumerable<(TValue, TKey1, TKey3, TKey4)> GetIter0134() { for (int i = 0; i < _count; i++) if (_entries1[i].next >= -1) yield return (_values[_entries1[i].valueIndex], _entries1[i].key, _entries3[_entries1[i].linkTo3].key, _entries4[_entries1[i].linkTo4].key); }
        public IEnumerable<(TValue, TKey2, TKey3, TKey4)> GetIter0234() { for (int i = 0; i < _count; i++) if (_entries1[i].next >= -1) yield return (_values[_entries1[i].valueIndex], _entries2[_entries1[i].linkTo2].key, _entries3[_entries1[i].linkTo3].key, _entries4[_entries1[i].linkTo4].key); }
        public IEnumerable<(TKey1, TKey2, TKey3, TKey4)> GetIter1234() { for (int i = 0; i < _count; i++) if (_entries1[i].next >= -1) yield return (_entries1[i].key, _entries2[_entries1[i].linkTo2].key, _entries3[_entries1[i].linkTo3].key, _entries4[_entries1[i].linkTo4].key); }
        public IEnumerable<(TValue, TKey1, TKey2, TKey3, TKey4)> GetIter01234() { for (int i = 0; i < _count; i++) if (_entries1[i].next >= -1) yield return (_values[_entries1[i].valueIndex], _entries1[i].key, _entries2[_entries1[i].linkTo2].key, _entries3[_entries1[i].linkTo3].key, _entries4[_entries1[i].linkTo4].key); }

        public IEnumerable<(TValue value, TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4)> Pairs => GetIter01234();
        public IEnumerable<TValue> Values => GetIter0();
        public IEnumerable<TKey1> Keys1 => GetIter1();
        public IEnumerable<TKey2> Keys2 => GetIter2();
        public IEnumerable<TKey3> Keys3 => GetIter3();
        public IEnumerable<TKey4> Keys4 => GetIter4();

        public IEnumerable<T> Enumerate<T>(MultiKeyIterFlags flags) { if (flags == MultiKeyIterFlags.Value) return (IEnumerable<T>)GetIter0(); if (flags == MultiKeyIterFlags.Key1) return (IEnumerable<T>)GetIter1(); if (flags == MultiKeyIterFlags.Key2) return (IEnumerable<T>)GetIter2(); if (flags == MultiKeyIterFlags.Key3) return (IEnumerable<T>)GetIter3(); if (flags == MultiKeyIterFlags.Key4) return (IEnumerable<T>)GetIter4(); throw new ArgumentException($"单元素迭代请使用 Value/Key1/Key2/Key3/Key4，当前 flags={flags}"); }
        public IEnumerable<(T1, T2)> Enumerate<T1, T2>(MultiKeyIterFlags flags) { if (flags == (MultiKeyIterFlags.Key1 | MultiKeyIterFlags.Key2)) return (IEnumerable<(T1, T2)>)GetIter12(); throw new ArgumentException($"双元素迭代需 flags=Key1|Key2，当前 flags={flags}"); }
        public IEnumerable<(T1, T2, T3)> Enumerate<T1, T2, T3>(MultiKeyIterFlags flags) { if (flags == (MultiKeyIterFlags.Key1 | MultiKeyIterFlags.Key2 | MultiKeyIterFlags.Key3)) return (IEnumerable<(T1, T2, T3)>)GetIter123(); throw new ArgumentException($"三元素迭代需 flags=Key1|Key2|Key3，当前 flags={flags}"); }
        public IEnumerable<(T1, T2, T3, T4)> Enumerate<T1, T2, T3, T4>(MultiKeyIterFlags flags) { if (flags == (MultiKeyIterFlags.Key1 | MultiKeyIterFlags.Key2 | MultiKeyIterFlags.Key3 | MultiKeyIterFlags.Key4)) return (IEnumerable<(T1, T2, T3, T4)>)GetIter1234(); throw new ArgumentException($"四元素迭代需 flags=Key1|Key2|Key3|Key4，当前 flags={flags}"); }
        public IEnumerable<(T1, T2, T3, T4, T5)> Enumerate<T1, T2, T3, T4, T5>(MultiKeyIterFlags flags) { var full = MultiKeyIterFlags.Value | MultiKeyIterFlags.Key1 | MultiKeyIterFlags.Key2 | MultiKeyIterFlags.Key3 | MultiKeyIterFlags.Key4; if (flags != full) throw new ArgumentException($"五元素迭代需 flags=Value|Key1|Key2|Key3|Key4，当前 flags={flags}"); return (IEnumerable<(T1, T2, T3, T4, T5)>)GetIter01234(); }

        public IEnumerator<TValue> GetEnumerator() => Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool RemoveKey1(TKey1 k) => RemoveByKey1(k);
        public bool RemoveKey2(TKey2 k) => RemoveByKey2(k);
        public bool RemoveKey3(TKey3 k) => RemoveByKey3(k);
        public bool RemoveKey4(TKey4 k) => RemoveByKey4(k);
    }
}
