using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Collections.LowLevel.Unsafe;

public struct XBlobHashMapEntry<TKey, TValue>
    where TKey : unmanaged
    where TValue : unmanaged
{
    public int HashCode;
    public int Next;
}

public struct XBlobKeyValuePair<TKey, TValue>
    where TKey : unmanaged
    where TValue : unmanaged
{
    public TKey Key;
    public TValue Value;
}

public ref struct XBlobHashMapEntryRef<TKey, TValue>
    where TKey : unmanaged, IEquatable<TKey>
    where TValue : unmanaged
{
    private readonly XBlobHashMapView<TKey, TValue> _view;
    private readonly int _index;

    internal XBlobHashMapEntryRef(XBlobHashMapView<TKey, TValue> view, int index)
    {
        _view = view;
        _index = index;
    }

    public ref XBlobHashMapEntry<TKey, TValue> Entry => ref _view.Entries[_index];
    public ref TKey Key => ref _view.Keys[_index];
    public ref TValue Value => ref _view.Values[_index];
}

/// <summary>Map 的只读视图，读逻辑在此；写（含 Count）经 container。链地址法，桶空=-1。</summary>
public ref struct XBlobHashMapView<TKey, TValue>
    where TKey : unmanaged, IEquatable<TKey>
    where TValue : unmanaged
{
    internal int Count;
    internal int BucketCount;
    internal Span<int> Buckets;
    internal Span<XBlobHashMapEntry<TKey, TValue>> Entries;
    internal Span<TKey> Keys;
    internal Span<TValue> Values;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal int FindEntry(in TKey key, int hashCode)
    {
        int bi = hashCode % BucketCount;
        if (bi < 0) bi += BucketCount;
        for (int i = Buckets[bi]; i >= 0; i = Entries[i].Next)
            if (Entries[i].HashCode == hashCode && Keys[i].Equals(key))
                return i;
        return -1;
    }
}

/// <summary>基于 XBlob 的键值映射，链地址法、无 Remove，下一槽=Count。读由 View 负责，写（含 Count）经 container。</summary>
public struct XBlobMap<TKey, TValue>
    where TKey : unmanaged, IEquatable<TKey>
    where TValue : unmanaged
{
    internal readonly int Offset;
    private TValue _nullValue; // 实例字段，用于GetRef找不到键时返回的默认值引用
    
    public XBlobMap(int offset)
    {
        Offset = offset;
        _nullValue = default;
    }

    private const int CountOffset = 0;
    private const int BucketCountOffset = sizeof(int);
    private const int BucketsOffset = sizeof(int) * 2; // 布局: [Count][BucketCount][Buckets][Entries][Keys][Values]
    public bool Valid => Offset > 0;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetBucketCount(in XBlobContainer container)
    {
        return container.Get<int>(Offset + BucketCountOffset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetCount(in XBlobContainer container)
    {
        return container.Get<int>(Offset + CountOffset);
    }

    private unsafe XBlobHashMapView<TKey, TValue> GetView(in XBlobContainer container)
    {
        int bucketCount = GetBucketCount(container);
        int count = GetCount(container);
        
        int bucketsOffset = Offset + BucketsOffset;
        int entriesOffset = bucketsOffset + bucketCount * sizeof(int);
        int entrySize = sizeof(int) + sizeof(int); // HashCode + Next
        int keysOffset = entriesOffset + bucketCount * entrySize;
        int valuesOffset = keysOffset + bucketCount * UnsafeUtility.SizeOf<TKey>();
        
        // 直接通过数据指针创建 Span
        byte* dataPtr = container.GetDataPointer(bucketsOffset);
        int* bucketsPtr = (int*)dataPtr;
        XBlobHashMapEntry<TKey, TValue>* entriesPtr = (XBlobHashMapEntry<TKey, TValue>*)(dataPtr + bucketCount * sizeof(int));
        TKey* keysPtr = (TKey*)(dataPtr + bucketCount * sizeof(int) + bucketCount * entrySize);
        TValue* valuesPtr = (TValue*)(dataPtr + bucketCount * sizeof(int) + bucketCount * entrySize + bucketCount * UnsafeUtility.SizeOf<TKey>());
        
        return new XBlobHashMapView<TKey, TValue>
        {
            Count = count,
            BucketCount = bucketCount,
            Buckets = new Span<int>(bucketsPtr, bucketCount),
            Entries = new Span<XBlobHashMapEntry<TKey, TValue>>(entriesPtr, bucketCount),
            Keys = new Span<TKey>(keysPtr, bucketCount),
            Values = new Span<TValue>(valuesPtr, bucketCount)
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetHashCode(in TKey key)
    {
        return key.GetHashCode();
    }

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
        set
        {
            AddOrUpdate(container, key, value);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(in XBlobContainer container, in TKey key, out TValue value)
    {
        // 超级快速路径：最小化内存访问和函数调用
        unsafe
        {
            byte* dataPtr = container.Data->GetDataPointer(Offset);
            int bucketCount = *(int*)(dataPtr + BucketCountOffset);
            int hashCode = GetHashCode(key);
            int bi = hashCode % bucketCount;
            if (bi < 0) bi += bucketCount;

            // 一次计算所有指针，避免重复运算
            byte* bucketsPtr = dataPtr + BucketsOffset;
            int* buckets = (int*)bucketsPtr;
            int entrySize = sizeof(int) + sizeof(int);
            int bucketsSize = bucketCount * sizeof(int);
            XBlobHashMapEntry<TKey, TValue>* entries = (XBlobHashMapEntry<TKey, TValue>*)(bucketsPtr + bucketsSize);
            TKey* keys = (TKey*)((byte*)entries + bucketCount * entrySize);
            TValue* values = (TValue*)((byte*)keys + bucketCount * UnsafeUtility.SizeOf<TKey>());

            // 查找循环
            for (int i = buckets[bi]; i >= 0; i = entries[i].Next)
            {
                if (entries[i].HashCode == hashCode && keys[i].Equals(key))
                {
                    value = values[i];
                    return true;
                }
            }
        }

        value = default;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool HasKey(in XBlobContainer container, in TKey key)
    {
        // 超级快速路径：最小化内存访问和函数调用
        unsafe
        {
            byte* dataPtr = container.Data->GetDataPointer(Offset);
            int bucketCount = *(int*)(dataPtr + BucketCountOffset);
            int hashCode = GetHashCode(key);
            int bi = hashCode % bucketCount;
            if (bi < 0) bi += bucketCount;

            byte* bucketsPtr = dataPtr + BucketsOffset;
            int* buckets = (int*)bucketsPtr;
            int entrySize = sizeof(int) + sizeof(int);
            int bucketsSize = bucketCount * sizeof(int);
            XBlobHashMapEntry<TKey, TValue>* entries = (XBlobHashMapEntry<TKey, TValue>*)(bucketsPtr + bucketsSize);
            TKey* keys = (TKey*)((byte*)entries + bucketCount * entrySize);

            for (int i = buckets[bi]; i >= 0; i = entries[i].Next)
            {
                if (entries[i].HashCode == hashCode && keys[i].Equals(key))
                    return true;
            }
        }

        return false;
    }

    public TKey GetKey(in XBlobContainer container, int index)
    {
        var view = GetView(container);
        XBlobHashCommon.ThrowIfIndexOutOfRange(index, view.Count, nameof(index));
        return view.Keys[index];
    }

    public TValue GetValue(in XBlobContainer container, int index)
    {
        var view = GetView(container);
        XBlobHashCommon.ThrowIfIndexOutOfRange(index, view.Count, nameof(index));
        return view.Values[index];
    }

    public Span<TKey> GetKeys(in XBlobContainer container)
    {
        var view = GetView(container);
        return view.Keys.Slice(0, view.Count);
    }

    public Span<TValue> GetValues(in XBlobContainer container)
    {
        var view = GetView(container);
        return view.Values.Slice(0, view.Count);
    }

    /// <summary>
    /// 获取指定键对应值的引用，通过 out exists 参数指示键是否存在
    /// </summary>
    /// <param name="exists">输出参数，指示键是否存在</param>
    /// <returns>如果键存在返回其引用，否则返回结构体实例的 _nullValue 字段引用</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public unsafe ref TValue GetRef(in XBlobContainer container, in TKey key, out bool exists)
    {
        int bucketCount = container.Get<int>(Offset + BucketCountOffset);
        int hashCode = GetHashCode(key);
        int bi = hashCode % bucketCount;
        if (bi < 0) bi += bucketCount;

        int bucketsOffset = Offset + BucketsOffset;
        byte* basePtr = container.GetDataPointer(bucketsOffset);
        int* buckets = (int*)basePtr;
        int entrySize = sizeof(int) + sizeof(int);
        int bucketsSize = bucketCount * sizeof(int);
        XBlobHashMapEntry<TKey, TValue>* entries = (XBlobHashMapEntry<TKey, TValue>*)(basePtr + bucketsSize);
        TKey* keys = (TKey*)((byte*)entries + bucketCount * entrySize);
        TValue* values = (TValue*)((byte*)keys + bucketCount * UnsafeUtility.SizeOf<TKey>());

        // 查找键
        for (int i = buckets[bi]; i >= 0; i = entries[i].Next)
        {
            if (entries[i].HashCode == hashCode && keys[i].Equals(key))
            {
                exists = true;
                return ref values[i];
            }
        }

        // 键不存在，返回结构体实例的默认值字段引用
        exists = false;
#pragma warning disable CS9084 // Struct member returns 'this' or other instance members by reference
        return ref _nullValue;
#pragma warning restore CS9084 // Struct member returns 'this' or other instance members by reference
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AddOrUpdate(in XBlobContainer container, in TKey key, in TValue value)
    {
        // 写操作：使用安全路径，带验证以确保数据一致性
        int count = container.Get<int>(Offset + CountOffset);
        int bucketCount = container.Get<int>(Offset + BucketCountOffset);
        int hashCode = GetHashCode(key);
        int bi = hashCode % bucketCount;
        if (bi < 0) bi += bucketCount;

        unsafe
        {
            int bucketsOffset = Offset + BucketsOffset;
            byte* basePtr = container.GetDataPointer(bucketsOffset);
            int* buckets = (int*)basePtr;
            int entrySize = sizeof(int) + sizeof(int);
            int bucketsSize = bucketCount * sizeof(int);
            XBlobHashMapEntry<TKey, TValue>* entries = (XBlobHashMapEntry<TKey, TValue>*)(basePtr + bucketsSize);
            TKey* keys = (TKey*)((byte*)entries + bucketCount * entrySize);
            TValue* values = (TValue*)((byte*)keys + bucketCount * UnsafeUtility.SizeOf<TKey>());

            // 查找已存在的 key
            for (int i = buckets[bi]; i >= 0; i = entries[i].Next)
            {
                if (entries[i].HashCode == hashCode && keys[i].Equals(key))
                {
                    values[i] = value;
                    return false; // 已存在，更新
                }
            }

            // 添加新元素
            if (count >= bucketCount)
                throw new InvalidOperationException(XBlobHashCommon.FullMessage);

            entries[count].HashCode = hashCode;
            entries[count].Next = buckets[bi];
            buckets[bi] = count;
            keys[count] = key;
            values[count] = value;
            container.GetRef<int>(Offset + CountOffset) = count + 1; // 安全写入
            return true; // 新增
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

        internal Enumerator(XBlobMap<TKey, TValue> map, XBlobContainer container)
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
                    Key = _view.Keys[_index],
                    Value = _view.Values[_index]
                };
            }
        }

        public bool MoveNext()
        {
            _index++;
            // 只遍历有效的条目（在 count 范围内）
            return _index < _view.Count;
        }
    }

    public readonly ref struct Enumerable
    {
        private readonly XBlobMap<TKey, TValue> _map;
        private readonly XBlobContainer _container;

        internal Enumerable(XBlobMap<TKey, TValue> map, XBlobContainer container)
        {
            _map = map;
            _container = container;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(_map, _container);
        }
    }

    public ref struct EnumeratorRef
    {
        private XBlobHashMapView<TKey, TValue> _view;
        private int _index;
        private XBlobHashMapEntryRef<TKey, TValue> _current;

        internal EnumeratorRef(XBlobMap<TKey, TValue> map, XBlobContainer container)
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
                return  _current;
            }
        }

        public bool MoveNext()
        {
            _index++;
            // 只遍历有效的条目（在 count 范围内）
            return _index < _view.Count;
        }
    }

    public readonly ref struct EnumerableRef
    {
        private readonly XBlobMap<TKey, TValue> _map;
        private readonly XBlobContainer _container;

        internal EnumerableRef(XBlobMap<TKey, TValue> map, XBlobContainer container)
        {
            _map = map;
            _container = container;
        }

        public EnumeratorRef GetEnumerator()
        {
            return new EnumeratorRef(_map, _container);
        }
    }

    public ref struct KeysEnumerator
    {
        private Enumerator _inner;

        internal KeysEnumerator(XBlobMap<TKey, TValue> map, XBlobContainer container)
        {
            _inner = new Enumerator(map, container);
        }

        public TKey Current => _inner.Current.Key;
        public bool MoveNext() => _inner.MoveNext();
    }

    public readonly ref struct KeysEnumerable
    {
        private readonly XBlobMap<TKey, TValue> _map;
        private readonly XBlobContainer _container;

        internal KeysEnumerable(XBlobMap<TKey, TValue> map, XBlobContainer container)
        {
            _map = map;
            _container = container;
        }

        public KeysEnumerator GetEnumerator() => new KeysEnumerator(_map, _container);
    }

    public ref struct ValuesEnumerator
    {
        private Enumerator _inner;

        internal ValuesEnumerator(XBlobMap<TKey, TValue> map, XBlobContainer container)
        {
            _inner = new Enumerator(map, container);
        }

        public TValue Current => _inner.Current.Value;
        public bool MoveNext() => _inner.MoveNext();
    }

    public readonly ref struct ValuesEnumerable
    {
        private readonly XBlobMap<TKey, TValue> _map;
        private readonly XBlobContainer _container;

        internal ValuesEnumerable(XBlobMap<TKey, TValue> map, XBlobContainer container)
        {
            _map = map;
            _container = container;
        }

        public ValuesEnumerator GetEnumerator() => new ValuesEnumerator(_map, _container);
    }
}

/// <summary>仅包含 Key 的视图，用于 XBlobMapKey 内部，不包含 Values。</summary>
internal ref struct XBlobMapKeyView<TKey>
    where TKey : unmanaged, IEquatable<TKey>
{
    internal int Count;
    internal int BucketCount;
    internal Span<int> Buckets;
    internal Span<XBlobHashEntry> Entries;
    internal Span<TKey> Keys;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal int FindEntry(in TKey key, int hashCode)
    {
        int bi = hashCode % BucketCount;
        if (bi < 0) bi += BucketCount;
        for (int i = Buckets[bi]; i >= 0; i = Entries[i].Next)
            if (Entries[i].HashCode == hashCode && Keys[i].Equals(key))
                return i;
        return -1;
    }
}

/// <summary>Map 的键视图，仅读键、HasKey、按索引取 Key，与 Map 共享同一块内存。</summary>
public struct XBlobMapKey<TKey>
    where TKey : unmanaged, IEquatable<TKey>
{
    internal readonly int Offset;

    internal XBlobMapKey(int offset) => Offset = offset;

    private static unsafe XBlobMapKeyView<TKey> GetView(in XBlobContainer container, int offset)
    {
        int bucketCount = container.Get<int>(offset + sizeof(int));
        int count = container.Get<int>(offset);
        int bucketsOffset = offset + sizeof(int) * 2;
        int entrySize = sizeof(int) * 2;
        byte* dataPtr = container.GetDataPointer(bucketsOffset);
        return new XBlobMapKeyView<TKey>
        {
            Count = count,
            BucketCount = bucketCount,
            Buckets = new Span<int>((int*)dataPtr, bucketCount),
            Entries = new Span<XBlobHashEntry>((XBlobHashEntry*)(dataPtr + bucketCount * sizeof(int)), bucketCount),
            Keys = new Span<TKey>((TKey*)(dataPtr + bucketCount * sizeof(int) + bucketCount * entrySize), bucketCount)
        };
    }

    private static int GetHashCode(in TKey key) => key.GetHashCode();

    public int GetLength(in XBlobContainer container) => container.Get<int>(Offset);

    public bool HasKey(in XBlobContainer container, in TKey key)
    {
        var view = GetView(container, Offset);
        return view.FindEntry(key, GetHashCode(key)) >= 0;
    }

    public TKey GetKey(in XBlobContainer container, int index)
    {
        var view = GetView(container, Offset);
        XBlobHashCommon.ThrowIfIndexOutOfRange(index, view.Count, nameof(index));
        return view.Keys[index];
    }

    public Span<TKey> GetKeys(in XBlobContainer container)
    {
        // KeyView 的 GetKeys 返回空 Span，建议使用 GetKeysEnumerator 进行遍历
        return Span<TKey>.Empty;
    }

    public KeyViewKeysEnumerable GetKeysEnumerator(in XBlobContainer container)
    {
        return new KeyViewKeysEnumerable(container, Offset);
    }

    public ref struct KeyViewKeysEnumerator
    {
        private XBlobMapKeyView<TKey> _view;
        private int _index;

        internal KeyViewKeysEnumerator(in XBlobContainer container, int offset)
        {
            _view = GetView(container, offset);
            _index = -1;
        }

        public TKey Current => _view.Keys[_index];
        public bool MoveNext() { _index++; return _index < _view.Count; }
    }

    public readonly ref struct KeyViewKeysEnumerable
    {
        private readonly XBlobContainer _container;
        private readonly int _offset;

        internal KeyViewKeysEnumerable(XBlobContainer container, int offset)
        {
            _container = container;
            _offset = offset;
        }

        public KeyViewKeysEnumerator GetEnumerator() => new KeyViewKeysEnumerator(_container, _offset);
    }
}
