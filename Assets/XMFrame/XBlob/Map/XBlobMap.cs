using System;
using System.Collections.Generic;

internal struct XBlobHashMapEntry<TKey, TValue>
    where TKey : unmanaged
    where TValue : unmanaged
{
    public int HashCode;
    public int Next;
    public TKey Key;
    public TValue Value;
}

internal struct XBlobKeyValuePair<TKey, TValue>
    where TKey : unmanaged
    where TValue : unmanaged
{
    public TKey Key;
    public TValue Value;
}

internal ref struct XBlobHashMapView<TKey, TValue>
    where TKey : unmanaged
    where TValue : unmanaged
{
    internal int Count;
    internal int BucketCount;
    internal Span<int> Buckets;
    internal Span<XBlobHashMapEntry<TKey, TValue>> Entries;
}

internal struct XBlobMap<TKey, TValue>
    where TKey : unmanaged, IEquatable<TKey>
    where TValue : unmanaged
{
    internal readonly int Offset;
    internal XBlobMap(int offset) => Offset = offset;

    private const int CountOffset = 0;
    private const int BucketCountOffset = sizeof(int);
    private const int BucketsOffset = sizeof(int) * 2;

    private int GetBucketCount(XBlobContainer container)
    {
        return container.Get<int>(Offset + BucketCountOffset);
    }

    private int GetCount(XBlobContainer container)
    {
        return container.Get<int>(Offset + CountOffset);
    }

    private unsafe XBlobHashMapView<TKey, TValue> GetView(XBlobContainer container)
    {
        int bucketCount = GetBucketCount(container);
        int count = GetCount(container);
        
        int bucketsOffset = Offset + BucketsOffset;
        int entriesOffset = bucketsOffset + bucketCount * sizeof(int);
        
        // 直接通过数据指针创建 Span
        byte* dataPtr = container.GetDataPointer(bucketsOffset);
        int* bucketsPtr = (int*)dataPtr;
        XBlobHashMapEntry<TKey, TValue>* entriesPtr = (XBlobHashMapEntry<TKey, TValue>*)(dataPtr + bucketCount * sizeof(int));
        
        return new XBlobHashMapView<TKey, TValue>
        {
            Count = count,
            BucketCount = bucketCount,
            Buckets = new Span<int>(bucketsPtr, bucketCount),
            Entries = new Span<XBlobHashMapEntry<TKey, TValue>>(entriesPtr, bucketCount)
        };
    }

    private static int GetHashCode(in TKey key)
    {
        // 使用默认的哈希码生成
        return key.GetHashCode();
    }

    private int FindEntry(XBlobContainer container, in TKey key, int hashCode)
    {
        var view = GetView(container);
        int bucketIndex = hashCode % view.BucketCount;
        if (bucketIndex < 0) bucketIndex += view.BucketCount;
        
        for (int i = view.Buckets[bucketIndex] - 1; i >= 0; i = view.Entries[i].Next)
        {
            if (view.Entries[i].HashCode == hashCode && view.Entries[i].Key.Equals(key))
            {
                return i;
            }
        }
        return -1;
    }

    public int GetLength(XBlobContainer container)
    {
        return GetCount(container);
    }

    public TValue this[XBlobContainer container, in TKey key]
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

    public bool TryGetValue(XBlobContainer container, in TKey key, out TValue value)
    {
        int hashCode = GetHashCode(key);
        int index = FindEntry(container, key, hashCode);
        if (index >= 0)
        {
            var view = GetView(container);
            value = view.Entries[index].Value;
            return true;
        }
        value = default;
        return false;
    }

    public bool HasKey(XBlobContainer container, in TKey key)
    {
        int hashCode = GetHashCode(key);
        return FindEntry(container, key, hashCode) >= 0;
    }

    public bool AddOrUpdate(XBlobContainer container, in TKey key, in TValue value)
    {
        int hashCode = GetHashCode(key);
        int index = FindEntry(container, key, hashCode);
        
        if (index >= 0)
        {
            // 更新现有值
            var view = GetView(container);
            view.Entries[index].Value = value;
            return false; // 返回 false 表示更新，不是新增
        }
        else
        {
            // 添加新键值对
            int count = GetCount(container);
            int bucketCount = GetBucketCount(container);
            
            if (count >= bucketCount)
            {
                throw new InvalidOperationException("Map is full, cannot add more elements");
            }
            
            var view = GetView(container);
            int bucketIndex = hashCode % bucketCount;
            if (bucketIndex < 0) bucketIndex += bucketCount;
            
            // 创建新条目
            ref var entry = ref view.Entries[count];
            entry.HashCode = hashCode;
            entry.Next = view.Buckets[bucketIndex] - 1;
            entry.Key = key;
            entry.Value = value;
            
            // 更新桶
            view.Buckets[bucketIndex] = count + 1;
            
            // 更新计数
            ref int countRef = ref container.GetRef<int>(Offset + CountOffset);
            countRef = count + 1;
            
            return true; // 返回 true 表示新增
        }
    }

    public Enumerable GetEnumerator(XBlobContainer container)
    {
        return new Enumerable(this, container);
    }

    public EnumerableRef GetEnumeratorRef(XBlobContainer container)
    {
        return new EnumerableRef(this, container);
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
                var entry = _view.Entries[_index];
                return new XBlobKeyValuePair<TKey, TValue>
                {
                    Key = entry.Key,
                    Value = entry.Value
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

        internal EnumeratorRef(XBlobMap<TKey, TValue> map, XBlobContainer container)
        {
            _view = map.GetView(container);
            _index = -1;
        }

        public ref XBlobHashMapEntry<TKey, TValue> Current => ref _view.Entries[_index];

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
}
