using System;
using System.Collections.Generic;

internal struct XBlobHashSetEntry<T>
    where T : unmanaged
{
    public int HashCode;
    public int Next;
    public T Value;
}

internal ref struct XBlobHashSetView<T>
    where T : unmanaged
{
    internal int Count;
    internal int BucketCount;
    internal Span<int> Buckets;
    internal Span<XBlobHashSetEntry<T>> Entries;
}

internal readonly struct XBlobSet<T> where T : unmanaged, IEquatable<T>
{
    internal readonly int Offset;
    internal XBlobSet(int offset) => Offset = offset;

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

    private unsafe XBlobHashSetView<T> GetView(XBlobContainer container)
    {
        int bucketCount = GetBucketCount(container);
        int count = GetCount(container);
        
        int bucketsOffset = Offset + BucketsOffset;
        int entriesOffset = bucketsOffset + bucketCount * sizeof(int);
        
        byte* dataPtr = container.GetDataPointer(bucketsOffset);
        int* bucketsPtr = (int*)dataPtr;
        XBlobHashSetEntry<T>* entriesPtr = (XBlobHashSetEntry<T>*)(dataPtr + bucketCount * sizeof(int));
        
        return new XBlobHashSetView<T>
        {
            Count = count,
            BucketCount = bucketCount,
            Buckets = new Span<int>(bucketsPtr, bucketCount),
            Entries = new Span<XBlobHashSetEntry<T>>(entriesPtr, bucketCount)
        };
    }

    private static int GetHashCode(in T value)
    {
        return value.GetHashCode();
    }

    private int FindEntry(XBlobContainer container, in T value, int hashCode)
    {
        var view = GetView(container);
        int bucketIndex = hashCode % view.BucketCount;
        if (bucketIndex < 0) bucketIndex += view.BucketCount;
        
        for (int i = view.Buckets[bucketIndex] - 1; i >= 0; i = view.Entries[i].Next)
        {
            if (view.Entries[i].HashCode == hashCode && view.Entries[i].Value.Equals(value))
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

    public T this[XBlobContainer container, int index]
    {
        get
        {
            var view = GetView(container);
            if (index < 0 || index >= view.Count)
                throw new IndexOutOfRangeException($"Index {index} is out of range [0, {view.Count})");
            return view.Entries[index].Value;
        }
    }

    public bool Contains(XBlobContainer container, in T value)
    {
        int hashCode = GetHashCode(value);
        return FindEntry(container, value, hashCode) >= 0;
    }

    public bool Add(XBlobContainer container, in T value)
    {
        int hashCode = GetHashCode(value);
        int index = FindEntry(container, value, hashCode);
        
        if (index >= 0)
        {
            return false; // 已存在，返回 false
        }
        else
        {
            // 添加新值
            int count = GetCount(container);
            int bucketCount = GetBucketCount(container);
            
            if (count >= bucketCount)
            {
                throw new InvalidOperationException("Set is full, cannot add more elements");
            }
            
            var view = GetView(container);
            int bucketIndex = hashCode % bucketCount;
            if (bucketIndex < 0) bucketIndex += bucketCount;
            
            // 创建新条目
            ref var entry = ref view.Entries[count];
            entry.HashCode = hashCode;
            entry.Next = view.Buckets[bucketIndex] - 1;
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

    public ref struct Enumerator
    {
        private XBlobHashSetView<T> _view;
        private int _index;

        internal Enumerator(XBlobSet<T> set, XBlobContainer container)
        {
            _view = set.GetView(container);
            _index = -1;
        }

        public T Current => _view.Entries[_index].Value;

        public bool MoveNext()
        {
            _index++;
            return _index < _view.Count;
        }
    }

    public ref struct Enumerable
    {
        private readonly XBlobSet<T> _set;
        private readonly XBlobContainer _container;

        internal Enumerable(XBlobSet<T> set, XBlobContainer container)
        {
            _set = set;
            _container = container;
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(_set, _container);
        }
    }

}
