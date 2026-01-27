using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;

[BurstCompile]
public ref struct XBlobMultiMapEntryRef<TKey, TValue>
    where TKey : unmanaged, IEquatable<TKey>
    where TValue : unmanaged
{
    private readonly XBlobMultiMapView<TKey, TValue> _view;
    private readonly int _index;

    internal XBlobMultiMapEntryRef(XBlobMultiMapView<TKey, TValue> view, int index)
    {
        _view = view;
        _index = index;
    }

    public ref XBlobMultiMapEntry<TKey, TValue> Entry => ref _view.Entries[_index];
    public ref TKey Key => ref _view.Keys[_index];
    public ref TValue Value => ref _view.Values[_index];
}

[BurstCompile]
public struct XBlobMultiMapEntry<TKey, TValue>
    where TKey : unmanaged
    where TValue : unmanaged
{
    public int HashCode;
    public int Next; // 下一个相同键的条目索引
    public int ValueNext; // 下一个值的索引（用于链表）
}

[BurstCompile]
/// <summary>只读视图，读逻辑全在此层；写（含 Count）由 container 负责。</summary>
internal ref struct XBlobMultiMapView<TKey, TValue>
    where TKey : unmanaged, IEquatable<TKey>
    where TValue : unmanaged
{
    internal int Count;
    internal int BucketCount;
    internal Span<int> Buckets;
    internal Span<XBlobMultiMapEntry<TKey, TValue>> Entries;
    internal Span<TKey> Keys;
    internal Span<TValue> Values;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal int FindFirstEntry(in TKey key, int hashCode)
    {
        int bucketIndex = hashCode % BucketCount;
        if (bucketIndex < 0) bucketIndex += BucketCount;
        for (int i = Buckets[bucketIndex]; i >= 0; i = Entries[i].Next)
        {
            if (Entries[i].HashCode == hashCode && Keys[i].Equals(key))
                return i;
        }
        return -1;
    }
}

/// <summary>基于 XBlob 的一键多值映射，链地址法、无 Remove，下一槽=Count。读由 View 负责，写（含 Count）经 container。</summary>
[BurstCompile]
public readonly struct XBlobMultiMap<TKey, TValue>
    where TKey : unmanaged, IEquatable<TKey>
    where TValue : unmanaged
{
    internal readonly int Offset;
    internal XBlobMultiMap(int offset) => Offset = offset;

    private const int CountOffset = 0;
    private const int BucketCountOffset = sizeof(int);
    private const int BucketsOffset = sizeof(int) * 2; // 布局: [Count][BucketCount][Buckets][Entries][Keys][Values]

    [BurstCompile]
    private unsafe XBlobMultiMapView<TKey, TValue> GetView(in XBlobContainer container)
    {
        int bucketCount = container.Get<int>(Offset + BucketCountOffset);
        int count = container.Get<int>(Offset + CountOffset);
        int entrySize = sizeof(int) * 3;
        int keySize = UnsafeUtility.SizeOf<TKey>();
        int bucketsOffset = Offset + BucketsOffset;
        int entriesOffset = bucketsOffset + bucketCount * sizeof(int);
        int keysOffset = entriesOffset + bucketCount * entrySize;

        byte* dataPtr = container.GetDataPointer(bucketsOffset);
        return new XBlobMultiMapView<TKey, TValue>
        {
            Count = count,
            BucketCount = bucketCount,
            Buckets = new Span<int>((int*)dataPtr, bucketCount),
            Entries = new Span<XBlobMultiMapEntry<TKey, TValue>>((XBlobMultiMapEntry<TKey, TValue>*)(dataPtr + bucketCount * sizeof(int)), bucketCount),
            Keys = new Span<TKey>((TKey*)(dataPtr + bucketCount * sizeof(int) + bucketCount * entrySize), bucketCount),
            Values = new Span<TValue>((TValue*)(dataPtr + bucketCount * sizeof(int) + bucketCount * entrySize + bucketCount * keySize), bucketCount)
        };
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetHashCode(in TKey key)
    {
        return key.GetHashCode();
    }

    [BurstCompile]
    public int GetLength(in XBlobContainer container)
    {
        return container.Get<int>(Offset + CountOffset);
    }

    [BurstCompile]
    public int GetValueCount(in XBlobContainer container, in TKey key)
    {
        var view = GetView(container);
        int firstIndex = view.FindFirstEntry(key, GetHashCode(key));
        if (firstIndex < 0) return 0;
        int count = 1;
        for (int i = view.Entries[firstIndex].ValueNext; i >= 0; i = view.Entries[i].ValueNext)
            count++;
        return count;
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsKey(in XBlobContainer container, in TKey key)
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
            int entrySize = sizeof(int) * 3;
            int bucketsSize = bucketCount * sizeof(int);
            XBlobMultiMapEntry<TKey, TValue>* entries = (XBlobMultiMapEntry<TKey, TValue>*)(bucketsPtr + bucketsSize);
            TKey* keys = (TKey*)((byte*)entries + bucketCount * entrySize);

            for (int i = buckets[bi]; i >= 0; i = entries[i].Next)
            {
                if (entries[i].HashCode == hashCode && keys[i].Equals(key))
                    return true;
            }
        }

        return false;
    }

    [BurstCompile]
    public TKey GetKey(in XBlobContainer container, int index)
    {
        var view = GetView(container);
        XBlobHashCommon.ThrowIfIndexOutOfRange(index, view.Count, nameof(index));
        return view.Keys[index];
    }

    [BurstCompile]
    public TValue GetValue(in XBlobContainer container, int index)
    {
        var view = GetView(container);
        XBlobHashCommon.ThrowIfIndexOutOfRange(index, view.Count, nameof(index));
        return view.Values[index];
    }

    [BurstCompile]
    public Span<TKey> GetKeys(in XBlobContainer container)
    {
        return Span<TKey>.Empty;
    }

    [BurstCompile]
    public Span<TValue> GetValues(in XBlobContainer container)
    {
        return Span<TValue>.Empty;
    }

    [BurstCompile]
    public bool ContainsValue(in XBlobContainer container, in TKey key, in TValue value)
    {
        var view = GetView(container);
        int firstIndex = view.FindFirstEntry(key, GetHashCode(key));
        if (firstIndex < 0) return false;
        if (view.Values[firstIndex].Equals(value)) return true;
        for (int i = view.Entries[firstIndex].ValueNext; i >= 0; i = view.Entries[i].ValueNext)
        {
            if (view.Values[i].Equals(value)) return true;
        }
        return false;
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(in XBlobContainer container, in TKey key, in TValue value)
    {
        // 写操作：使用安全路径，带验证以确保数据一致性
        int count = container.Get<int>(Offset + CountOffset);
        int bucketCount = container.Get<int>(Offset + BucketCountOffset);
        
        if (count >= bucketCount)
            throw new InvalidOperationException(XBlobHashCommon.FullMessage);

        int hashCode = GetHashCode(key);
        int bi = hashCode % bucketCount;
        if (bi < 0) bi += bucketCount;

        unsafe
        {
            int bucketsOffset = Offset + BucketsOffset;
            byte* basePtr = container.GetDataPointer(bucketsOffset);
            int* buckets = (int*)basePtr;
            int entrySize = sizeof(int) * 3;
            int bucketsSize = bucketCount * sizeof(int);
            XBlobMultiMapEntry<TKey, TValue>* entries = (XBlobMultiMapEntry<TKey, TValue>*)(basePtr + bucketsSize);
            TKey* keys = (TKey*)((byte*)entries + bucketCount * entrySize);
            TValue* values = (TValue*)((byte*)keys + bucketCount * UnsafeUtility.SizeOf<TKey>());

            // 查找 firstIndex
            int firstIndex = -1;
            for (int i = buckets[bi]; i >= 0; i = entries[i].Next)
            {
                if (entries[i].HashCode == hashCode && keys[i].Equals(key))
                {
                    firstIndex = i;
                    break;
                }
            }

            entries[count].HashCode = hashCode;
            keys[count] = key;
            values[count] = value;

            if (firstIndex >= 0)
            {
                entries[count].Next = -1;
                entries[count].ValueNext = entries[firstIndex].ValueNext;
                entries[firstIndex].ValueNext = count;
            }
            else
            {
                entries[count].Next = buckets[bi];
                entries[count].ValueNext = -1;
                buckets[bi] = count;
            }

            container.GetRef<int>(Offset + CountOffset) = count + 1; // 安全写入
        }
    }

    [BurstCompile]
    public Enumerable GetEnumerator(in XBlobContainer container)
    {
        return new Enumerable(this, container);
    }

    [BurstCompile]
    public EnumerableRef GetEnumeratorRef(in XBlobContainer container)
    {
        return new EnumerableRef(this, container);
    }

    [BurstCompile]
    public KeysEnumerable GetKeysEnumerator(in XBlobContainer container)
    {
        return new KeysEnumerable(this, container);
    }

    [BurstCompile]
    public ValuesEnumerable GetValuesEnumerator(in XBlobContainer container)
    {
        return new ValuesEnumerable(this, container);
    }

    [BurstCompile]
    public ValuesPerKeyEnumerable GetValuesPerKeyEnumerator(in XBlobContainer container, in TKey key)
    {
        return new ValuesPerKeyEnumerable(this, container, key);
    }

    [BurstCompile]
    public ref struct Enumerator
    {
        private XBlobMultiMapView<TKey, TValue> _view;
        private int _bucketIndex;
        private int _chainHead;
        private int _currentIndex;

        internal Enumerator(in XBlobMultiMap<TKey, TValue> map, in XBlobContainer container)
        {
            _view = map.GetView(container);
            _bucketIndex = 0;
            _chainHead = -1;
            _currentIndex = -1;
        }

        public XBlobKeyValuePair<TKey, TValue> Current
        {
            get
            {
                return new XBlobKeyValuePair<TKey, TValue>
                {
                    Key = _view.Keys[_currentIndex],
                    Value = _view.Values[_currentIndex]
                };
            }
        }

        public bool MoveNext()
        {
            if (_currentIndex >= 0)
            {
                int valueNext = _view.Entries[_currentIndex].ValueNext;
                if (valueNext >= 0)
                {
                    _currentIndex = valueNext;
                    return true;
                }
                _chainHead = _view.Entries[_chainHead].Next;
            }
            else
            {
                _chainHead = -1;
            }
            if (_chainHead >= 0)
            {
                _currentIndex = _chainHead;
                return true;
            }
            for (; _bucketIndex < _view.BucketCount; _bucketIndex++)
            {
                _chainHead = _view.Buckets[_bucketIndex];
                if (_chainHead >= 0)
                {
                    _currentIndex = _chainHead;
                    _bucketIndex++;
                    return true;
                }
            }
            return false;
        }
    }

    [BurstCompile]
    public ref struct Enumerable
    {
        private readonly XBlobMultiMap<TKey, TValue> _map;
        private readonly XBlobContainer _container;

        internal Enumerable(in XBlobMultiMap<TKey, TValue> map, in XBlobContainer container)
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
        private XBlobMultiMapView<TKey, TValue> _view;
        private int _bucketIndex;
        private int _chainHead;
        private int _currentIndex;
        private XBlobMultiMapEntryRef<TKey, TValue> _current;

        internal EnumeratorRef(in XBlobMultiMap<TKey, TValue> map, in XBlobContainer container)
        {
            _view = map.GetView(container);
            _bucketIndex = 0;
            _chainHead = -1;
            _currentIndex = -1;
            _current = default;
        }

        public XBlobMultiMapEntryRef<TKey, TValue> Current
        {
            get
            {
                _current = new XBlobMultiMapEntryRef<TKey, TValue>(_view, _currentIndex);
                return _current;
            }
        }

        [BurstCompile]
        public bool MoveNext()
        {
            if (_currentIndex >= 0)
            {
                int valueNext = _view.Entries[_currentIndex].ValueNext;
                if (valueNext >= 0)
                {
                    _currentIndex = valueNext;
                    return true;
                }
                _chainHead = _view.Entries[_chainHead].Next;
            }
            else
            {
                _chainHead = -1;
            }
            if (_chainHead >= 0)
            {
                _currentIndex = _chainHead;
                return true;
            }
            for (; _bucketIndex < _view.BucketCount; _bucketIndex++)
            {
                _chainHead = _view.Buckets[_bucketIndex];
                if (_chainHead >= 0)
                {
                    _currentIndex = _chainHead;
                    _bucketIndex++;
                    return true;
                }
            }
            return false;
        }
    }

    [BurstCompile]
    public ref struct EnumerableRef
    {
        private readonly XBlobMultiMap<TKey, TValue> _map;
        private readonly XBlobContainer _container;

        internal EnumerableRef(in XBlobMultiMap<TKey, TValue> map, in XBlobContainer container)
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
        private XBlobMultiMapView<TKey, TValue> _view;
        private int _bucketIndex;
        private int _currentIndex;

        internal KeysEnumerator(in XBlobMultiMap<TKey, TValue> map, in XBlobContainer container)
        {
            _view = map.GetView(container);
            _bucketIndex = 0;
            _currentIndex = -1;
        }

        public TKey Current => _view.Keys[_currentIndex];

        [BurstCompile]
        public bool MoveNext()
        {
            if (_currentIndex >= 0)
            {
                _currentIndex = _view.Entries[_currentIndex].Next;
                if (_currentIndex >= 0) return true;
            }
            for (; _bucketIndex < _view.BucketCount; _bucketIndex++)
            {
                _currentIndex = _view.Buckets[_bucketIndex];
                if (_currentIndex >= 0)
                {
                    _bucketIndex++;
                    return true;
                }
            }
            return false;
        }
    }

    [BurstCompile]
    public readonly ref struct KeysEnumerable
    {
        private readonly XBlobMultiMap<TKey, TValue> _map;
        private readonly XBlobContainer _container;

        internal KeysEnumerable(in XBlobMultiMap<TKey, TValue> map, in XBlobContainer container)
        {
            _map = map;
            _container = container;
        }

        [BurstCompile]
        public KeysEnumerator GetEnumerator()
        {
            return new KeysEnumerator(_map, _container);
        }
    }

    [BurstCompile]
    public ref struct ValuesEnumerator
    {
        private XBlobMultiMapView<TKey, TValue> _view;
        private int _bucketIndex;
        private int _chainHead;
        private int _currentIndex;

        internal ValuesEnumerator(in XBlobMultiMap<TKey, TValue> map, in XBlobContainer container)
        {
            _view = map.GetView(container);
            _bucketIndex = 0;
            _chainHead = -1;
            _currentIndex = -1;
        }

        public TValue Current => _view.Values[_currentIndex];

        [BurstCompile]
        public bool MoveNext()
        {
            if (_currentIndex >= 0)
            {
                int valueNext = _view.Entries[_currentIndex].ValueNext;
                if (valueNext >= 0)
                {
                    _currentIndex = valueNext;
                    return true;
                }
                _chainHead = _view.Entries[_chainHead].Next;
            }
            else
            {
                _chainHead = -1;
            }

            if (_chainHead >= 0)
            {
                _currentIndex = _chainHead;
                return true;
            }
            for (; _bucketIndex < _view.BucketCount; _bucketIndex++)
            {
                _chainHead = _view.Buckets[_bucketIndex];
                if (_chainHead >= 0)
                {
                    _currentIndex = _chainHead;
                    _bucketIndex++;
                    return true;
                }
            }
            return false;
        }
    }

    [BurstCompile]
    public readonly ref struct ValuesEnumerable
    {
        private readonly XBlobMultiMap<TKey, TValue> _map;
        private readonly XBlobContainer _container;

        internal ValuesEnumerable(in XBlobMultiMap<TKey, TValue> map, in XBlobContainer container)
        {
            _map = map;
            _container = container;
        }

        [BurstCompile]
        public ValuesEnumerator GetEnumerator()
        {
            return new ValuesEnumerator(_map, _container);
        }
    }

    [BurstCompile]
    public ref struct ValuesPerKeyEnumerator
    {
        private XBlobMultiMapView<TKey, TValue> _view;
        private int _currentIndex;
        private bool _started;

        internal ValuesPerKeyEnumerator(in XBlobMultiMap<TKey, TValue> map, in XBlobContainer container, in TKey key)
        {
            _view = map.GetView(container);
            _currentIndex = _view.FindFirstEntry(key, key.GetHashCode());
            _started = false;
        }

        public TValue Current => _view.Values[_currentIndex];

        [BurstCompile]
        public bool MoveNext()
        {
            if (_currentIndex < 0)
            {
                return false;
            }

            if (!_started)
            {
                _started = true;
                return true;
            }

            _currentIndex = _view.Entries[_currentIndex].ValueNext;
            return _currentIndex >= 0;
        }
    }

    [BurstCompile]
    public readonly ref struct ValuesPerKeyEnumerable
    {
        private readonly XBlobMultiMap<TKey, TValue> _map;
        private readonly XBlobContainer _container;
        private readonly TKey _key;

        internal ValuesPerKeyEnumerable(in XBlobMultiMap<TKey, TValue> map, in XBlobContainer container, in TKey key)
        {
            _map = map;
            _container = container;
            _key = key;
        }

        [BurstCompile]
        public ValuesPerKeyEnumerator GetEnumerator()
        {
            return new ValuesPerKeyEnumerator(_map, _container, _key);
        }
    }
}
