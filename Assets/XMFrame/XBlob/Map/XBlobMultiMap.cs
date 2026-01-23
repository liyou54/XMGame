using System;
using System.Collections.Generic;

internal struct XBlobMultiMapEntry<TKey, TValue>
    where TKey : unmanaged
    where TValue : unmanaged
{
    public int HashCode;
    public int Next; // 下一个相同键的条目索引
    public int ValueNext; // 下一个值的索引（用于链表）
    public TKey Key;
    public TValue Value;
}

internal ref struct XBlobMultiMapView<TKey, TValue>
    where TKey : unmanaged
    where TValue : unmanaged
{
    internal int Count;
    internal int BucketCount;
    internal Span<int> Buckets;
    internal Span<XBlobMultiMapEntry<TKey, TValue>> Entries;
}

internal readonly struct XBlobMultiMap<TKey, TValue>
    where TKey : unmanaged, IEquatable<TKey>
    where TValue : unmanaged
{
    internal readonly int Offset;
    internal XBlobMultiMap(int offset) => Offset = offset;

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

    private unsafe XBlobMultiMapView<TKey, TValue> GetView(XBlobContainer container)
    {
        int bucketCount = GetBucketCount(container);
        int count = GetCount(container);
        
        int bucketsOffset = Offset + BucketsOffset;
        int entriesOffset = bucketsOffset + bucketCount * sizeof(int);
        
        byte* dataPtr = container.GetDataPointer(bucketsOffset);
        int* bucketsPtr = (int*)dataPtr;
        XBlobMultiMapEntry<TKey, TValue>* entriesPtr = (XBlobMultiMapEntry<TKey, TValue>*)(dataPtr + bucketCount * sizeof(int));
        
        return new XBlobMultiMapView<TKey, TValue>
        {
            Count = count,
            BucketCount = bucketCount,
            Buckets = new Span<int>(bucketsPtr, bucketCount),
            Entries = new Span<XBlobMultiMapEntry<TKey, TValue>>(entriesPtr, bucketCount)
        };
    }

    private static int GetHashCode(in TKey key)
    {
        return key.GetHashCode();
    }

    private int FindFirstEntry(XBlobContainer container, in TKey key, int hashCode)
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

    public int GetValueCount(XBlobContainer container, in TKey key)
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

    public bool ContainsKey(XBlobContainer container, in TKey key)
    {
        int hashCode = GetHashCode(key);
        return FindFirstEntry(container, key, hashCode) >= 0;
    }

    public bool ContainsValue(XBlobContainer container, in TKey key, in TValue value)
    {
        int hashCode = GetHashCode(key);
        int firstIndex = FindFirstEntry(container, key, hashCode);
        if (firstIndex < 0) return false;
        
        var view = GetView(container);
        // 检查第一个值
        if (view.Entries[firstIndex].Value.Equals(value))
            return true;
        
        // 检查链表中的其他值
        for (int i = view.Entries[firstIndex].ValueNext; i >= 0; i = view.Entries[i].ValueNext)
        {
            if (view.Entries[i].Value.Equals(value))
                return true;
        }
        return false;
    }

    public void Add(XBlobContainer container, in TKey key, in TValue value)
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
            newEntry.Key = key;
            newEntry.Value = value;
            
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
            newEntry.Key = key;
            newEntry.Value = value;
            
            view.Buckets[bucketIndex] = count + 1;
        }
        
        // 更新计数
        ref int countRef = ref container.GetRef<int>(Offset + CountOffset);
        countRef = count + 1;
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
        private XBlobMultiMapView<TKey, TValue> _view;
        private int _index;

        internal Enumerator(XBlobMultiMap<TKey, TValue> map, XBlobContainer container)
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
            return _index < _view.Count;
        }
    }

    public ref struct Enumerable
    {
        private readonly XBlobMultiMap<TKey, TValue> _map;
        private readonly XBlobContainer _container;

        internal Enumerable(XBlobMultiMap<TKey, TValue> map, XBlobContainer container)
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
        private XBlobMultiMapView<TKey, TValue> _view;
        private int _index;

        internal EnumeratorRef(XBlobMultiMap<TKey, TValue> map, XBlobContainer container)
        {
            _view = map.GetView(container);
            _index = -1;
        }

        public ref XBlobMultiMapEntry<TKey, TValue> Current => ref _view.Entries[_index];

        public bool MoveNext()
        {
            _index++;
            return _index < _view.Count;
        }
    }

    public ref struct EnumerableRef
    {
        private readonly XBlobMultiMap<TKey, TValue> _map;
        private readonly XBlobContainer _container;

        internal EnumerableRef(XBlobMultiMap<TKey, TValue> map, XBlobContainer container)
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
