using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;

[BurstCompile]
public ref struct XBlobMultiMapEntryRef<TKey, TValue>
    where TKey : unmanaged
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
internal ref struct XBlobMultiMapView<TKey, TValue>
    where TKey : unmanaged
    where TValue : unmanaged
{
    internal int Count;
    internal int BucketCount;
    internal Span<int> Buckets;
    internal Span<XBlobMultiMapEntry<TKey, TValue>> Entries;
    internal Span<TKey> Keys;
    internal Span<TValue> Values;
}

// Burst 优化的 MultiMap 查找方法
[BurstCompile]
internal static unsafe class XBlobMultiMapBurst
{
    [BurstCompile]
    public static int FindFirstEntry<TKey>(
        int* buckets,
        XBlobMultiMapEntry<TKey, int>* entries,
        TKey* keys,
        int bucketCount,
        in TKey key,
        int hashCode)
        where TKey : unmanaged, IEquatable<TKey>
    {
        int bucketIndex = hashCode % bucketCount;
        if (bucketIndex < 0) bucketIndex += bucketCount;
        
        for (int i = buckets[bucketIndex] - 1; i >= 0; i = entries[i].Next)
        {
            if (entries[i].HashCode == hashCode && keys[i].Equals(key))
            {
                return i;
            }
        }
        return -1;
    }
}

[BurstCompile]
public readonly struct XBlobMultiMap<TKey, TValue>
    where TKey : unmanaged, IEquatable<TKey>
    where TValue : unmanaged
{
    internal readonly int Offset;
    internal XBlobMultiMap(int offset) => Offset = offset;

    private const int CountOffset = 0;
    private const int BucketCountOffset = sizeof(int);
    private const int BucketsOffset = sizeof(int) * 2;

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
    private unsafe XBlobMultiMapView<TKey, TValue> GetView(in XBlobContainer container)
    {
        int bucketCount = GetBucketCount(container);
        int count = GetCount(container);
        
        int bucketsOffset = Offset + BucketsOffset;
        int entriesOffset = bucketsOffset + bucketCount * sizeof(int);
        int entrySize = sizeof(int) + sizeof(int) + sizeof(int); // HashCode + Next + ValueNext
        int keysOffset = entriesOffset + bucketCount * entrySize;
        int valuesOffset = keysOffset + bucketCount * System.Runtime.InteropServices.Marshal.SizeOf<TKey>();
        
        byte* dataPtr = container.GetDataPointer(bucketsOffset);
        int* bucketsPtr = (int*)dataPtr;
        XBlobMultiMapEntry<TKey, TValue>* entriesPtr = (XBlobMultiMapEntry<TKey, TValue>*)(dataPtr + bucketCount * sizeof(int));
        TKey* keysPtr = (TKey*)(dataPtr + bucketCount * sizeof(int) + bucketCount * entrySize);
        TValue* valuesPtr = (TValue*)(dataPtr + bucketCount * sizeof(int) + bucketCount * entrySize + bucketCount * System.Runtime.InteropServices.Marshal.SizeOf<TKey>());
        
        return new XBlobMultiMapView<TKey, TValue>
        {
            Count = count,
            BucketCount = bucketCount,
            Buckets = new Span<int>(bucketsPtr, bucketCount),
            Entries = new Span<XBlobMultiMapEntry<TKey, TValue>>(entriesPtr, bucketCount),
            Keys = new Span<TKey>(keysPtr, bucketCount),
            Values = new Span<TValue>(valuesPtr, bucketCount)
        };
    }

    [BurstCompile]
    private static int GetHashCode(in TKey key)
    {
        return key.GetHashCode();
    }

    [BurstCompile]
    private int FindFirstEntry(in XBlobContainer container, in TKey key, int hashCode)
    {
        var view = GetView(container);
        int bucketIndex = hashCode % view.BucketCount;
        if (bucketIndex < 0) bucketIndex += view.BucketCount;
        
        for (int i = view.Buckets[bucketIndex] - 1; i >= 0; i = view.Entries[i].Next)
        {
            if (view.Entries[i].HashCode == hashCode && view.Keys[i].Equals(key))
            {
                return i;
            }
        }
        return -1;
    }

    [BurstCompile]
    public int GetLength(in XBlobContainer container)
    {
        return GetCount(container);
    }

    [BurstCompile]
    public int GetValueCount(in XBlobContainer container, in TKey key)
    {
        int hashCode = GetHashCode(key);
        int firstIndex = FindFirstEntry(container, key, hashCode);
        if (firstIndex < 0) return 0;
        
        int count = 1;
        var view = GetView(container);
        for (int i = view.Entries[firstIndex].ValueNext; i >= 0; i = view.Entries[i].ValueNext)
        {
            count++;
        }
        return count;
    }

    [BurstCompile]
    public bool ContainsKey(in XBlobContainer container, in TKey key)
    {
        int hashCode = GetHashCode(key);
        return FindFirstEntry(container, key, hashCode) >= 0;
    }

    [BurstCompile]
    public TKey GetKey(in XBlobContainer container, int index)
    {
        if (index < 0 || index >= GetCount(container))
            throw new ArgumentOutOfRangeException(nameof(index));
        var view = GetView(container);
        return view.Keys[index];
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
        return view.Keys.Slice(0, view.Count);
    }

    [BurstCompile]
    public Span<TValue> GetValues(in XBlobContainer container)
    {
        var view = GetView(container);
        return view.Values.Slice(0, view.Count);
    }

    [BurstCompile]
    public bool ContainsValue(in XBlobContainer container, in TKey key, in TValue value)
    {
        int hashCode = GetHashCode(key);
        int firstIndex = FindFirstEntry(container, key, hashCode);
        if (firstIndex < 0) return false;
        
        var view = GetView(container);
        // 检查第一个值
        if (view.Values[firstIndex].Equals(value))
            return true;
        
        // 检查链表中的其他值
        for (int i = view.Entries[firstIndex].ValueNext; i >= 0; i = view.Entries[i].ValueNext)
        {
            if (view.Values[i].Equals(value))
                return true;
        }
        return false;
    }

    [BurstCompile]
    public void Add(in XBlobContainer container, in TKey key, in TValue value)
    {
        int hashCode = GetHashCode(key);
        int firstIndex = FindFirstEntry(container, key, hashCode);
        
        int count = GetCount(container);
        int bucketCount = GetBucketCount(container);
        
        if (count >= bucketCount)
        {
            throw new InvalidOperationException("MultiMap is full, cannot add more elements");
        }
        
        var view = GetView(container);
        
        if (firstIndex >= 0)
        {
            // 键已存在，添加到值的链表
            ref var newEntry = ref view.Entries[count];
            newEntry.HashCode = hashCode;
            newEntry.Next = -1; // 不在桶链表中
            newEntry.ValueNext = view.Entries[firstIndex].ValueNext;
            view.Keys[count] = key;
            view.Values[count] = value;
            
            view.Entries[firstIndex].ValueNext = count;
        }
        else
        {
            // 新键，添加到桶链表
            int bucketIndex = hashCode % bucketCount;
            if (bucketIndex < 0) bucketIndex += bucketCount;
            
            ref var newEntry = ref view.Entries[count];
            newEntry.HashCode = hashCode;
            newEntry.Next = view.Buckets[bucketIndex] - 1;
            newEntry.ValueNext = -1; // 第一个值，没有下一个值
            view.Keys[count] = key;
            view.Values[count] = value;
            
            view.Buckets[bucketIndex] = count + 1;
        }
        
        // 更新计数
        ref int countRef = ref container.GetRef<int>(Offset + CountOffset);
        countRef = count + 1;
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
        private int _index;

        internal Enumerator(in XBlobMultiMap<TKey, TValue> map, in XBlobContainer container)
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
            return _index < _view.Count;
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
        private int _index;
        private XBlobMultiMapEntryRef<TKey, TValue> _current;

        internal EnumeratorRef(in XBlobMultiMap<TKey, TValue> map, in XBlobContainer container)
        {
            _view = map.GetView(container);
            _index = -1;
            _current = default;
        }

        public  XBlobMultiMapEntryRef<TKey, TValue> Current
        {
            get
            {
                // 更新 _current 以反映当前的索引
                _current = new XBlobMultiMapEntryRef<TKey, TValue>(_view, _index);
                // 使用 unsafe 来返回引用
                return  _current;
            }
        }

        [BurstCompile]
        public bool MoveNext()
        {
            _index++;
            return _index < _view.Count;
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
        private int _index;

        internal KeysEnumerator(in XBlobMultiMap<TKey, TValue> map, in XBlobContainer container)
        {
            _view = map.GetView(container);
            _index = -1;
        }

        public TKey Current => _view.Keys[_index];

        [BurstCompile]
        public bool MoveNext()
        {
            // 遍历所有条目，只返回那些 ValueNext == -1 的条目（即每个 key 的第一个值对应的条目）
            // 这样可以确保每个唯一的 key 只返回一次
            while (true)
            {
                _index++;
                if (_index >= _view.Count)
                {
                    return false;
                }

                // 如果这个条目的 ValueNext == -1，说明这是某个 key 的第一个值
                if (_view.Entries[_index].ValueNext == -1)
                {
                    return true;
                }
            }
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
        private int _index;

        internal ValuesEnumerator(in XBlobMultiMap<TKey, TValue> map, in XBlobContainer container)
        {
            _view = map.GetView(container);
            _index = -1;
        }

        public TValue Current => _view.Values[_index];

        [BurstCompile]
        public bool MoveNext()
        {
            _index++;
            return _index < _view.Count;
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
            // 查找第一个条目
            int hashCode = key.GetHashCode();
            int bucketIndex = hashCode % _view.BucketCount;
            if (bucketIndex < 0) bucketIndex += _view.BucketCount;
            
            int firstIndex = -1;
            for (int i = _view.Buckets[bucketIndex] - 1; i >= 0; i = _view.Entries[i].Next)
            {
                if (_view.Entries[i].HashCode == hashCode && _view.Keys[i].Equals(key))
                {
                    firstIndex = i;
                    break;
                }
            }
            
            _currentIndex = firstIndex;
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
