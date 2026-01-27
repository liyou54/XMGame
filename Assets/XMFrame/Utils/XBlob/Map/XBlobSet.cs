using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;

[BurstCompile]
internal struct XBlobHashSetEntry
{
    public int HashCode;
    public int Next;
}

[BurstCompile]
/// <summary>只读视图，读逻辑全在此层；写（含 Count）由 container 负责。</summary>
internal ref struct XBlobHashSetView<T>
    where T : unmanaged, IEquatable<T>
{
    internal int Count;
    internal int BucketCount;
    internal Span<int> Buckets;
    internal Span<XBlobHashSetEntry> Entries;
    internal Span<T> Values;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal int FindEntry(in T value, int hashCode)
    {
        int bucketIndex = hashCode % BucketCount;
        if (bucketIndex < 0) bucketIndex += BucketCount;
        for (int i = Buckets[bucketIndex]; i >= 0; i = Entries[i].Next)
        {
            if (Entries[i].HashCode == hashCode && Values[i].Equals(value))
                return i;
        }
        return -1;
    }
}

/// <summary>基于 XBlob 的集合，链地址法、无 Remove，下一槽=Count。读由 View 负责，写（含 Count）经 container。</summary>
[BurstCompile]
public readonly struct XBlobSet<T> where T : unmanaged, IEquatable<T>
{
    internal readonly int Offset;
    internal XBlobSet(int offset) => Offset = offset;

    private const int CountOffset = 0;
    private const int BucketCountOffset = sizeof(int);
    private const int BucketsOffset = sizeof(int) * 2; // 布局: [Count][BucketCount][Buckets][Entries][Values]

    [BurstCompile]
    private unsafe XBlobHashSetView<T> GetView(in XBlobContainer container)
    {
        int bucketCount = container.Get<int>(Offset + BucketCountOffset);
        int count = container.Get<int>(Offset + CountOffset);
        int entrySize = sizeof(int) * 2;
        int bucketsOffset = Offset + BucketsOffset;
        byte* dataPtr = container.GetDataPointer(bucketsOffset);

        return new XBlobHashSetView<T>
        {
            Count = count,
            BucketCount = bucketCount,
            Buckets = new Span<int>((int*)dataPtr, bucketCount),
            Entries = new Span<XBlobHashSetEntry>((XBlobHashSetEntry*)(dataPtr + bucketCount * sizeof(int)), bucketCount),
            Values = new Span<T>((T*)(dataPtr + bucketCount * sizeof(int) + bucketCount * entrySize), bucketCount)
        };
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetHashCode(in T value)
    {
        return value.GetHashCode();
    }

    [BurstCompile]
    public int GetLength(in XBlobContainer container)
    {
        return container.Get<int>(Offset + CountOffset);
    }

    public T this[in XBlobContainer container, int index]
    {
        get
        {
            var view = GetView(container);
            XBlobHashCommon.ThrowIfIndexOutOfRange(index, view.Count, nameof(index));
            return view.Values[index];
        }
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(in XBlobContainer container, in T value)
    {
        // 快速路径：直接访问内存，避免创建完整 View
        int bucketCount = container.Get<int>(Offset + BucketCountOffset);
        int hashCode = GetHashCode(value);
        int bi = hashCode % bucketCount;
        if (bi < 0) bi += bucketCount;

        unsafe
        {
            int bucketsOffset = Offset + BucketsOffset;
            byte* basePtr = container.GetDataPointer(bucketsOffset);
            int* buckets = (int*)basePtr;
            int entrySize = sizeof(int) + sizeof(int);
            XBlobHashSetEntry* entries = (XBlobHashSetEntry*)(basePtr + bucketCount * sizeof(int));
            T* values = (T*)(basePtr + bucketCount * sizeof(int) + bucketCount * entrySize);

            for (int i = buckets[bi]; i >= 0; i = entries[i].Next)
            {
                if (entries[i].HashCode == hashCode && values[i].Equals(value))
                    return true;
            }
        }

        return false;
    }

    [BurstCompile]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Add(in XBlobContainer container, in T value)
    {
        // 快速路径：直接访问内存，避免创建完整 View
        int count = container.Get<int>(Offset + CountOffset);
        int bucketCount = container.Get<int>(Offset + BucketCountOffset);
        int hashCode = GetHashCode(value);
        int bi = hashCode % bucketCount;
        if (bi < 0) bi += bucketCount;

        unsafe
        {
            int bucketsOffset = Offset + BucketsOffset;
            byte* basePtr = container.GetDataPointer(bucketsOffset);
            int* buckets = (int*)basePtr;
            int entrySize = sizeof(int) + sizeof(int);
            XBlobHashSetEntry* entries = (XBlobHashSetEntry*)(basePtr + bucketCount * sizeof(int));
            T* values = (T*)(basePtr + bucketCount * sizeof(int) + bucketCount * entrySize);

            // 检查是否已存在
            for (int i = buckets[bi]; i >= 0; i = entries[i].Next)
            {
                if (entries[i].HashCode == hashCode && values[i].Equals(value))
                    return false; // 已存在
            }

            // 添加新元素
            if (count >= bucketCount)
                throw new InvalidOperationException(XBlobHashCommon.FullMessage);

            entries[count].HashCode = hashCode;
            entries[count].Next = buckets[bi];
            buckets[bi] = count;
            values[count] = value;
            container.GetRef<int>(Offset + CountOffset) = count + 1;
            return true;
        }
    }

    [BurstCompile]
    public Enumerable GetEnumerator(in XBlobContainer container)
    {
        return new Enumerable(this, container);
    }

    [BurstCompile]
    public ref struct Enumerator
    {
        private XBlobHashSetView<T> _view;
        private int _bucketIndex;
        private int _currentIndex;

        internal Enumerator(in XBlobSet<T> set, in XBlobContainer container)
        {
            _view = set.GetView(container);
            _bucketIndex = 0;
            _currentIndex = -1;
        }

        public T Current => _view.Values[_currentIndex];

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
    public ref struct Enumerable
    {
        private readonly XBlobSet<T> _set;
        private readonly XBlobContainer _container;

        internal Enumerable(in XBlobSet<T> set, in XBlobContainer container)
        {
            _set = set;
            _container = container;
        }

        [BurstCompile]
        public Enumerator GetEnumerator()
        {
            return new Enumerator(_set, _container);
        }
    }

}
