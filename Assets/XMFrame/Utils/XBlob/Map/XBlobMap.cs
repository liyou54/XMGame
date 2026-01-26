using System;
using System.Collections.Generic;
using Unity.Burst;
#if UNITY_BURST
using Unity.Burst;
#endif
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

[BurstCompile]
// Key 和 Entry 合并在一起的结构
public struct XBlobHashMapKeyEntry<TKey>
    where TKey : unmanaged
{
    public TKey Key;
    public int HashCode; // 用于快速比较和冲突检测，0 表示空槽
}

[BurstCompile]
public struct XBlobKeyValuePair<TKey, TValue>
    where TKey : unmanaged
    where TValue : unmanaged
{
    public TKey Key;
    public TValue Value;
}

[BurstCompile]
public ref struct XBlobHashMapEntryRef<TKey, TValue>
    where TKey : unmanaged
    where TValue : unmanaged
{
    private readonly XBlobHashMapView<TKey, TValue> _view;
    private readonly int _index;

    internal XBlobHashMapEntryRef(XBlobHashMapView<TKey, TValue> view, int index)
    {
        _view = view;
        _index = index;
    }

    public ref XBlobHashMapKeyEntry<TKey> KeyEntry => ref _view.KeyEntries[_index];
    public ref TKey Key => ref _view.KeyEntries[_index].Key;
    public ref int HashCode => ref _view.KeyEntries[_index].HashCode;
    public ref TValue Value => ref _view.Values[_index];
}

[BurstCompile]
public ref struct XBlobHashMapView<TKey, TValue>
    where TKey : unmanaged
    where TValue : unmanaged
{
    internal int Count;
    internal int BucketCount;
    internal Span<XBlobHashMapKeyEntry<TKey>> KeyEntries; // Key 和 Entry 合并在一起
    internal Span<TValue> Values;
}

// Burst 优化的核心查找方法
[BurstCompile]
internal static unsafe class XBlobMapBurst
{
    private const int EmptyHashCode = int.MinValue;

    [BurstCompile]
    public static int FindEntry<TKey>(
        XBlobHashMapKeyEntry<TKey>* keyEntries,
        int bucketCount,
        in TKey key,
        int hashCode)
        where TKey : unmanaged, IEquatable<TKey>
    {
        int startIndex = hashCode % bucketCount;
        if (startIndex < 0) startIndex += bucketCount;

        // 使用线性探测（开放寻址法）
        for (int i = 0; i < bucketCount; i++)
        {
            int index = (startIndex + i) % bucketCount;
            ref var keyEntry = ref keyEntries[index];

            // EmptyHashCode 表示空槽
            if (keyEntry.HashCode == EmptyHashCode)
            {
                return -1; // 未找到
            }

            // 检查是否匹配
            if (keyEntry.HashCode == hashCode && keyEntry.Key.Equals(key))
            {
                return index;
            }
        }

        return -1; // 表已满，未找到
    }

    [BurstCompile]
    public static int FindEmptySlot<TKey>(
        XBlobHashMapKeyEntry<TKey>* keyEntries,
        int bucketCount,
        int hashCode)
        where TKey : unmanaged
    {
        int startIndex = hashCode % bucketCount;
        if (startIndex < 0) startIndex += bucketCount;

        // 使用线性探测找到空槽
        for (int i = 0; i < bucketCount; i++)
        {
            int index = (startIndex + i) % bucketCount;
            if (keyEntries[index].HashCode == EmptyHashCode)
            {
                return index;
            }
        }

        return -1; // 表已满
    }
}

[BurstCompile]
public struct XBlobMap<TKey, TValue>
    where TKey : unmanaged, IEquatable<TKey>
    where TValue : unmanaged
{
    internal readonly int Offset;
    internal XBlobMap(int offset) => Offset = offset;

    private const int CountOffset = 0;
    private const int BucketCountOffset = sizeof(int);
    private const int KeyEntriesOffset = sizeof(int) * 2; // 直接是 KeyEntries，没有 Buckets
    private const int EmptyHashCode = int.MinValue; // 使用 int.MinValue 表示空槽，避免与 HashCode == 0 的键冲突

    [BurstCompile]
    private int GetBucketCount(in XBlobContainer container)
    {
        return container.Get<int>(Offset + BucketCountOffset);
    }

    [BurstCompile]
    private int GetCount(in XBlobContainer container)
    {
        return container.Get<int>(Offset + CountOffset);
    }

    [BurstCompile]
    private unsafe XBlobHashMapView<TKey, TValue> GetView(in XBlobContainer container)
    {
        int bucketCount = GetBucketCount(container);
        int count = GetCount(container);

        int keyEntriesOffset = Offset + KeyEntriesOffset;
        int keyEntrySize = System.Runtime.InteropServices.Marshal.SizeOf<XBlobHashMapKeyEntry<TKey>>();
        int valuesOffset = keyEntriesOffset + bucketCount * keyEntrySize;

        // 新布局: [Count] [BucketCount] [KeyEntries] [Values]
        // KeyEntries 包含 Key 和 HashCode，使用开放寻址法
        byte* dataPtr = container.GetDataPointer(keyEntriesOffset);
        XBlobHashMapKeyEntry<TKey>* keyEntriesPtr = (XBlobHashMapKeyEntry<TKey>*)dataPtr;
        TValue* valuesPtr = (TValue*)(dataPtr + bucketCount * keyEntrySize);

        return new XBlobHashMapView<TKey, TValue>
        {
            Count = count,
            BucketCount = bucketCount,
            KeyEntries = new Span<XBlobHashMapKeyEntry<TKey>>(keyEntriesPtr, bucketCount),
            Values = new Span<TValue>(valuesPtr, bucketCount)
        };
    }

    [BurstCompile]
    private static int GetHashCode(in TKey key)
    {
        // 使用默认的哈希码生成
        return key.GetHashCode();
    }

    [BurstCompile]
    private unsafe int FindEntry(in XBlobContainer container, in TKey key, int hashCode)
    {
        var view = GetView(container);
        int bucketCount = view.BucketCount;

        // 使用 Burst 优化的查找方法
        fixed (XBlobHashMapKeyEntry<TKey>* keyEntriesPtr = view.KeyEntries)
        {
            return XBlobMapBurst.FindEntry(keyEntriesPtr, bucketCount, key, hashCode);
        }
    }

    [BurstCompile]
    public int GetLength(in XBlobContainer container)
    {
        return GetCount(container);
    }

    public TValue this[in XBlobContainer container, in TKey key]
    {
        get
        {
            if (TryGetValue(container, key, out TValue value))
            {
                return value;
            }

            throw new KeyNotFoundException($"Key not found in map");
        }
        set { AddOrUpdate(container, key, value); }
    }

    [BurstCompile]
    public bool TryGetValue(in XBlobContainer container, in TKey key, out TValue value)
    {
        int hashCode = GetHashCode(key);
        int index = FindEntry(container, key, hashCode);
        if (index >= 0)
        {
            var view = GetView(container);
            value = view.Values[index];
            return true;
        }

        value = default;
        return false;
    }

    [BurstCompile]
    public bool HasKey(in XBlobContainer container, in TKey key)
    {
        int hashCode = GetHashCode(key);
        return FindEntry(container, key, hashCode) >= 0;
    }

    [BurstCompile]
    public TKey GetKey(in XBlobContainer container, int index)
    {
        if (index < 0 || index >= GetCount(container))
            throw new ArgumentOutOfRangeException(nameof(index));
        var view = GetView(container);
        return view.KeyEntries[index].Key;
    }

    [BurstCompile]
    public TValue GetValue(in XBlobContainer container, int index)
    {
        if (index < 0 || index >= GetCount(container))
            throw new ArgumentOutOfRangeException(nameof(index));
        var view = GetView(container);
        return view.Values[index];
    }

    [BurstCompile]
    public Span<TKey> GetKeys(in XBlobContainer container)
    {
        var view = GetView(container);
        // 需要创建一个临时的 Key 数组，因为 KeyEntries 是结构数组
        // 或者返回一个自定义的 Span，但这比较复杂
        // 暂时返回空 Span，建议使用 GetKeysEnumerator
        return Span<TKey>.Empty;
    }

    [BurstCompile]
    public Span<TValue> GetValues(in XBlobContainer container)
    {
        var view = GetView(container);
        return view.Values.Slice(0, view.Count);
    }

    [BurstCompile]
    public unsafe bool AddOrUpdate(in XBlobContainer container, in TKey key, in TValue value)
    {
        int hashCode = GetHashCode(key);
        int index = FindEntry(container, key, hashCode);

        if (index >= 0)
        {
            // 更新现有值
            var view = GetView(container);
            view.Values[index] = value;
            return false; // 返回 false 表示更新，不是新增
        }
        else
        {
            // 添加新键值对，使用开放寻址法找到空槽
            int bucketCount = GetBucketCount(container);
            var view = GetView(container);

            // 使用 Burst 优化的查找空槽方法
            int slotIndex;
            fixed (XBlobHashMapKeyEntry<TKey>* keyEntriesPtr = view.KeyEntries)
            {
                slotIndex = XBlobMapBurst.FindEmptySlot(keyEntriesPtr, bucketCount, hashCode);
            }

            if (slotIndex >= 0)
            {
                // 找到空槽，插入新条目
                ref var keyEntry = ref view.KeyEntries[slotIndex];
                keyEntry.Key = key;
                keyEntry.HashCode = hashCode;
                view.Values[slotIndex] = value;

                // 更新计数
                int count = GetCount(container);
                ref int countRef = ref container.GetRef<int>(Offset + CountOffset);
                countRef = count + 1;

                return true; // 返回 true 表示新增
            }
            else
            {
                // 表已满
                throw new InvalidOperationException("Map is full, cannot add more elements");
            }
        }
    }

    public Enumerable GetEnumerator(in XBlobContainer container)
    {
        return new Enumerable(this, container);
    }

    public EnumerableRef GetEnumeratorRef(in XBlobContainer container)
    {
        return new EnumerableRef(this, container);
    }

    public KeysEnumerable GetKeysEnumerator(in XBlobContainer container)
    {
        return new KeysEnumerable(this, container);
    }

    public ValuesEnumerable GetValuesEnumerator(in XBlobContainer container)
    {
        return new ValuesEnumerable(this, container);
    }

    public XBlobMapKey<TKey> AsKeyView()
    {
        return new XBlobMapKey<TKey>(Offset);
    }

    public ref struct Enumerator
    {
        private XBlobHashMapView<TKey, TValue> _view;
        private int _index;

        internal Enumerator(in XBlobMap<TKey, TValue> map, in XBlobContainer container)
        {
            _view = map.GetView(container);
            _index = -1;
        }

        public XBlobKeyValuePair<TKey, TValue> Current
        {
            get
            {
                return new XBlobKeyValuePair<TKey, TValue>
                {
                    Key = _view.KeyEntries[_index].Key,
                    Value = _view.Values[_index]
                };
            }
        }

        public bool MoveNext()
        {
            // 使用线性探测遍历所有非空槽
            _index++;
            while (_index < _view.BucketCount)
            {
                if (_view.KeyEntries[_index].HashCode != EmptyHashCode)
                {
                    return true;
                }

                _index++;
            }

            return false;
        }
    }

    [BurstCompile]
    public readonly ref struct Enumerable
    {
        private readonly XBlobMap<TKey, TValue> _map;
        private readonly XBlobContainer _container;

        internal Enumerable(in XBlobMap<TKey, TValue> map, in XBlobContainer container)
        {
            _map = map;
            _container = container;
        }

        [BurstCompile]
        public Enumerator GetEnumerator()
        {
            return new Enumerator(_map, _container);
        }
    }

    [BurstCompile]
    public ref struct EnumeratorRef
    {
        private XBlobHashMapView<TKey, TValue> _view;
        private int _index;
        private XBlobHashMapEntryRef<TKey, TValue> _current;

        internal EnumeratorRef(in XBlobMap<TKey, TValue> map, in XBlobContainer container)
        {
            _view = map.GetView(container);
            _index = -1;
            _current = default;
        }

        public XBlobHashMapEntryRef<TKey, TValue> Current
        {
            get
            {
                // 更新 _current 以反映当前的索引
                _current = new XBlobHashMapEntryRef<TKey, TValue>(_view, _index);
                // 使用 unsafe 来返回引用
                return _current;
            }
        }

        [BurstCompile]
        public bool MoveNext()
        {
            // 使用线性探测遍历所有非空槽
            _index++;
            while (_index < _view.BucketCount)
            {
                if (_view.KeyEntries[_index].HashCode != EmptyHashCode)
                {
                    return true;
                }

                _index++;
            }

            return false;
        }
    }

    [BurstCompile]
    public readonly ref struct EnumerableRef
    {
        private readonly XBlobMap<TKey, TValue> _map;
        private readonly XBlobContainer _container;

        internal EnumerableRef(in XBlobMap<TKey, TValue> map, in XBlobContainer container)
        {
            _map = map;
            _container = container;
        }

        [BurstCompile]
        public EnumeratorRef GetEnumerator()
        {
            return new EnumeratorRef(_map, _container);
        }
    }

    [BurstCompile]
    public ref struct KeysEnumerator
    {
        private XBlobHashMapView<TKey, TValue> _view;
        private int _index;

        internal KeysEnumerator(in XBlobMap<TKey, TValue> map, in XBlobContainer container)
        {
            _view = map.GetView(container);
            _index = -1;
        }

        public TKey Current => _view.KeyEntries[_index].Key;

        public bool MoveNext()
        {
            // 使用线性探测遍历所有非空槽
            _index++;
            while (_index < _view.BucketCount)
            {
                if (_view.KeyEntries[_index].HashCode != EmptyHashCode)
                {
                    return true;
                }

                _index++;
            }

            return false;
        }
    }

    [BurstCompile]
    public readonly ref struct KeysEnumerable
    {
        private readonly XBlobMap<TKey, TValue> _map;
        private readonly XBlobContainer _container;

        internal KeysEnumerable(in XBlobMap<TKey, TValue> map, in XBlobContainer container)
        {
            _map = map;
            _container = container;
        }

        public KeysEnumerator GetEnumerator()
        {
            return new KeysEnumerator(_map, _container);
        }
    }

    [BurstCompile]
    public ref struct ValuesEnumerator
    {
        private XBlobHashMapView<TKey, TValue> _view;
        private int _index;

        internal ValuesEnumerator(in XBlobMap<TKey, TValue> map, in XBlobContainer container)
        {
            _view = map.GetView(container);
            _index = -1;
        }

        public TValue Current => _view.Values[_index];

        public bool MoveNext()
        {
            // 使用线性探测遍历所有非空槽
            _index++;
            while (_index < _view.BucketCount)
            {
                if (_view.KeyEntries[_index].HashCode != EmptyHashCode)
                {
                    return true;
                }

                _index++;
            }

            return false;
        }
    }

    [BurstCompile]
    public readonly ref struct ValuesEnumerable
    {
        private readonly XBlobMap<TKey, TValue> _map;
        private readonly XBlobContainer _container;

        internal ValuesEnumerable(in XBlobMap<TKey, TValue> map, in XBlobContainer container)
        {
            _map = map;
            _container = container;
        }

        public ValuesEnumerator GetEnumerator()
        {
            return new ValuesEnumerator(_map, _container);
        }
    }
}

[BurstCompile]
// XBlobMapKey 外观类型，用于从 XBlobMap 转换而来，专门用于判断 Key 是否存在
public struct XBlobMapKey<TKey>
    where TKey : unmanaged, IEquatable<TKey>
{
    internal readonly int Offset;
    internal XBlobMapKey(int offset) => Offset = offset;

    private const int CountOffset = 0;
    private const int BucketCountOffset = sizeof(int);
    private const int KeyEntriesOffset = sizeof(int) * 2; // 直接是 KeyEntries，没有 Buckets
    private const int EmptyHashCode = int.MinValue; // 使用 int.MinValue 表示空槽，避免与 HashCode == 0 的键冲突

    private int GetBucketCount(in XBlobContainer container)
    {
        return container.Get<int>(Offset + BucketCountOffset);
    }

    private int GetCount(in XBlobContainer container)
    {
        return container.Get<int>(Offset + CountOffset);
    }

    private unsafe XBlobHashMapKeyView<TKey> GetView(in XBlobContainer container)
    {
        int bucketCount = GetBucketCount(container);
        int count = GetCount(container);

        int keyEntriesOffset = Offset + KeyEntriesOffset;
        int keyEntrySize = System.Runtime.InteropServices.Marshal.SizeOf<XBlobHashMapKeyEntry<TKey>>();

        // 新布局: [Count] [BucketCount] [KeyEntries] [Values]
        // KeyEntries 包含 Key 和 HashCode
        byte* dataPtr = container.GetDataPointer(keyEntriesOffset);
        XBlobHashMapKeyEntry<TKey>* keyEntriesPtr = (XBlobHashMapKeyEntry<TKey>*)dataPtr;

        return new XBlobHashMapKeyView<TKey>
        {
            Count = count,
            BucketCount = bucketCount,
            KeyEntries = new Span<XBlobHashMapKeyEntry<TKey>>(keyEntriesPtr, bucketCount)
        };
    }

    private static int GetHashCode(in TKey key)
    {
        return key.GetHashCode();
    }

    private unsafe int FindEntry(in XBlobContainer container, in TKey key, int hashCode)
    {
        var view = GetView(container);
        int bucketCount = view.BucketCount;
        int startIndex = hashCode % bucketCount;
        if (startIndex < 0) startIndex += bucketCount;

        // 使用线性探测（开放寻址法）
        for (int i = 0; i < bucketCount; i++)
        {
            int index = (startIndex + i) % bucketCount;
            ref var keyEntry = ref view.KeyEntries[index];

            // EmptyHashCode 表示空槽
            if (keyEntry.HashCode == EmptyHashCode)
            {
                return -1; // 未找到
            }

            // 检查是否匹配
            if (keyEntry.HashCode == hashCode && keyEntry.Key.Equals(key))
            {
                return index;
            }
        }

        return -1; // 表已满，未找到
    }

    public int GetLength(in XBlobContainer container)
    {
        return GetCount(container);
    }

    public bool HasKey(in XBlobContainer container, in TKey key)
    {
        int hashCode = GetHashCode(key);
        return FindEntry(container, key, hashCode) >= 0;
    }

    public TKey GetKey(in XBlobContainer container, int index)
    {
        if (index < 0 || index >= GetBucketCount(container))
            throw new ArgumentOutOfRangeException(nameof(index));
        var view = GetView(container);
        return view.KeyEntries[index].Key;
    }

    public Span<TKey> GetKeys(in XBlobContainer container)
    {
        var view = GetView(container);
        // 返回空 Span，建议使用 GetKeysEnumerator
        return Span<TKey>.Empty;
    }

    public KeysEnumerable GetKeysEnumerator(in XBlobContainer container)
    {
        return new KeysEnumerable(this, container);
    }

    public ref struct KeysEnumerator
    {
        private XBlobHashMapKeyView<TKey> _view;
        private int _index;

        internal KeysEnumerator(in XBlobMapKey<TKey> mapKey, in XBlobContainer container)
        {
            _view = mapKey.GetView(container);
            _index = -1;
        }

        public TKey Current => _view.KeyEntries[_index].Key;

        public bool MoveNext()
        {
            // 使用线性探测遍历所有非空槽
            _index++;
            while (_index < _view.BucketCount)
            {
                if (_view.KeyEntries[_index].HashCode != EmptyHashCode)
                {
                    return true;
                }

                _index++;
            }

            return false;
        }
    }

    [BurstCompile]
    public readonly ref struct KeysEnumerable
    {
        private readonly XBlobMapKey<TKey> _mapKey;
        private readonly XBlobContainer _container;

        internal KeysEnumerable(in XBlobMapKey<TKey> mapKey, in XBlobContainer container)
        {
            _mapKey = mapKey;
            _container = container;
        }

        public KeysEnumerator GetEnumerator()
        {
            return new KeysEnumerator(_mapKey, _container);
        }
    }
}

[BurstCompile]
// 用于 XBlobMapKey 的视图结构（不包含 Values）
internal ref struct XBlobHashMapKeyView<TKey>
    where TKey : unmanaged
{
    internal int Count;
    internal int BucketCount;
    internal Span<XBlobHashMapKeyEntry<TKey>> KeyEntries;
}