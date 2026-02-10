using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

public unsafe struct XBlobContainer : IDisposable
{
    private const int CountOffset = 0;
    private const int BucketCountOffset = sizeof(int);
    private const int KeyEntriesOffset = sizeof(int) * 2;  // KeyEntries 的偏移量（用于 HashMap）
    private const int BucketsOffset = sizeof(int) * 2;  // 保留用于 HashSet 和 MultiMap（它们仍使用 Buckets）

    internal XBlobData* Data;
    
    public readonly bool IsValid => Data != null;
    
    private Allocator _allocator;

    public void Create(Allocator allocator, int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentException("Capacity must be positive", nameof(capacity));

        // 如果已经初始化过，直接抛出异常
        if (Data != null)
        {
            throw new InvalidOperationException("XBlobData has already been initialized. Call Dispose() before reinitializing.");
        }
        
        _allocator = allocator;
        
        // 分配 XBlobData 结构的内存并清零，确保 _data、_allocator 等字段为默认值
        int dataSize = UnsafeUtility.SizeOf<XBlobData>();
        Data = (XBlobData*)UnsafeUtility.Malloc(dataSize, UnsafeUtility.AlignOf<XBlobData>(), allocator);
        UnsafeUtility.MemClear(Data, dataSize);
        
        // 初始化 XBlobData
        Data->Initialize(allocator, capacity);
    }
    
    public readonly T Get<T>(int offset) where T : unmanaged
    {
        ThrowIfInvalid();
        return Data->Get<T>(offset);
    }

    public readonly ref T GetRef<T>(int offset) where T : unmanaged
    {
        ThrowIfInvalid();
        return ref Data->GetRef<T>(offset);
    }

    internal XBlobArrayView<T> GetArrayView<T>(int offset) where T : unmanaged
    {
        ThrowIfInvalid();
        return Data->GetArrayView<T>(offset);
    }

    internal readonly byte* GetDataPointer(int offset)
    {
        ThrowIfInvalid();
        return Data->GetDataPointer(offset);
    }
    
    internal XBlobPtr<T> Alloc<T>() where T : unmanaged
    {
        ThrowIfInvalid();
        int offset = Data->Alloc<T>();
        return new XBlobPtr<T>(offset);
    }

    public XBlobArray<T> AllocArray<T>(int capacity) where T : unmanaged
    {
        ThrowIfInvalid();
        if (capacity < 0)
            throw new ArgumentException("Capacity must be non-negative", nameof(capacity));
        
        // Array 布局: [length(int)] [capacity * T]
        int size = sizeof(int) + capacity * Marshal.SizeOf<T>();
        int offset = Data->AllocBytes(size);
        
        // 初始化 length
        ref int lengthRef = ref GetRef<int>(offset);
        lengthRef = capacity;
        
        return new XBlobArray<T>(offset);
    }

    public readonly XBlobMap<TKey, TValue> AllocMap<TKey, TValue>(int capacity) 
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
    {
        ThrowIfInvalid();
        if (capacity <= 0)
            throw new ArgumentException("Capacity must be positive", nameof(capacity));
        
        int offset = AllocHashMap<TKey, TValue>(capacity);
        return new XBlobMap<TKey, TValue>(offset);
    }

    /// <summary>
    /// 泛型分配 Map 并返回偏移，供 ConfigData 等无反射调用。
    /// </summary>
    public readonly int AllocMapOffset<TKey, TValue>(int capacity)
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
    {
        var map = AllocMap<TKey, TValue>(capacity);
        return map.Offset; 
    }

    /// <summary>
    /// 按运行时类型分配 Map，用于建立 CfgI->TUnmanaged 等映射。
    /// 返回 Map 在容器内的偏移（可用 XBlobPtr.FromOffset 转成指针）。
    /// </summary>
    public readonly int AllocMapByTypes(Type keyType, Type valueType, int capacity)
    {
        ThrowIfInvalid();
        if (capacity <= 0)
            throw new ArgumentException("Capacity must be positive", nameof(capacity));
        var m = typeof(XBlobContainer).GetMethod("AllocMap", BindingFlags.NonPublic | BindingFlags.Instance);
        if (m == null)
            throw new MissingMethodException(nameof(XBlobContainer), "AllocMap");
        var generic = m.MakeGenericMethod(keyType, valueType);
        object boxed = this;
        var map = generic.Invoke(boxed, new object[] { capacity });
        var offsetField = map.GetType().GetField("Offset", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (offsetField == null)
            throw new MissingFieldException("XBlobMap", "Offset");
        return (int)offsetField.GetValue(map);
    }

    public XBlobSet<T> AllocSet<T>(int capacity) 
        where T : unmanaged, IEquatable<T>
    {
        ThrowIfInvalid();
        if (capacity <= 0)
            throw new ArgumentException("Capacity must be positive", nameof(capacity));
        
        int offset = AllocHashSet<T>(capacity);
        return new XBlobSet<T>(offset);
    }

    public XBlobMultiMap<TKey, TValue> AllocMultiMap<TKey, TValue>(int capacity) 
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
    {
        ThrowIfInvalid();
        if (capacity <= 0)
            throw new ArgumentException("Capacity must be positive", nameof(capacity));
        
        int offset = AllocMultiHashMap<TKey, TValue>(capacity);
        return new XBlobMultiMap<TKey, TValue>(offset);
    }

    public void Reserve(int newCapacity)
    {
        ThrowIfInvalid();
        if (newCapacity <= 0)
            throw new ArgumentException("Capacity must be positive", nameof(newCapacity));
        
        Data->Reserve(newCapacity);
    }

    public void Dispose()
    {
        if (Data != null)
        {
            Data->Dispose();
            UnsafeUtility.Free(Data, _allocator);
            Data = null;
        }
    }

    private readonly void ThrowIfInvalid()
    {
        if (!IsValid)
            throw new InvalidOperationException("Container is not valid");
    }

    private readonly int AllocHashMap<TKey, TValue>(int bucketCount) 
        where TKey : unmanaged
        where TValue : unmanaged
    {
        // Map 布局（链地址 + Key/Value 线性，无 Remove 下一槽=Count）: [Count][BucketCount][Buckets][Entries][Keys][Values]
        int entrySize = sizeof(int) * 2;
        int keySize = Marshal.SizeOf<TKey>();
        int valueSize = Marshal.SizeOf<TValue>();
        int headerSize = sizeof(int) * 2; // Count + BucketCount
        int bucketsSize = bucketCount * sizeof(int);
        int entriesSize = bucketCount * entrySize;
        int keysSize = bucketCount * keySize;
        int valuesSize = bucketCount * valueSize;
        int size = headerSize + bucketsSize + entriesSize + keysSize + valuesSize;
        int offset = Data->AllocBytes(size);
        
        ref int countRef = ref GetRef<int>(offset + CountOffset);
        countRef = 0;
        ref int bucketCountRef = ref GetRef<int>(offset + BucketCountOffset);
        bucketCountRef = bucketCount;

        unsafe
        {
            int bucketsOffset = offset + headerSize;
            byte* bucketsPtr = Data->GetDataPointer(bucketsOffset);
            int* buckets = (int*)bucketsPtr;
            for (int i = 0; i < bucketCount; i++)
                buckets[i] = -1;
        }
        
        return offset;
    }

    private int AllocHashSet<T>(int bucketCount) where T : unmanaged
    {
        // Set 布局（链地址 + Value 线性，下一槽=Count）: [Count][BucketCount][Buckets][Entries][Values]
        int entrySize = sizeof(int) * 2;
        int valueSize = Marshal.SizeOf<T>();
        int headerSize = sizeof(int) * 2;
        int bucketsSize = bucketCount * sizeof(int);
        int entriesSize = bucketCount * entrySize;
        int valuesSize = bucketCount * valueSize;
        int size = headerSize + bucketsSize + entriesSize + valuesSize;
        int offset = Data->AllocBytes(size);
        
        ref int countRef = ref GetRef<int>(offset + CountOffset);
        countRef = 0;
        ref int bucketCountRef = ref GetRef<int>(offset + BucketCountOffset);
        bucketCountRef = bucketCount;

        unsafe
        {
            int bucketsOffset = offset + headerSize;
            byte* bucketsPtr = Data->GetDataPointer(bucketsOffset);
            int* buckets = (int*)bucketsPtr;
            for (int i = 0; i < bucketCount; i++)
                buckets[i] = -1;
        }
        
        return offset;
    }

    private int AllocMultiHashMap<TKey, TValue>(int bucketCount) 
        where TKey : unmanaged
        where TValue : unmanaged
    {
        // MultiMap 布局（链地址 + Key/Value 线性，下一槽=Count）: [Count][BucketCount][Buckets][Entries][Keys][Values]
        int entrySize = sizeof(int) * 3;
        int keySize = Marshal.SizeOf<TKey>();
        int valueSize = Marshal.SizeOf<TValue>();
        int headerSize = sizeof(int) * 2;
        int bucketsSize = bucketCount * sizeof(int);
        int entriesSize = bucketCount * entrySize;
        int keysSize = bucketCount * keySize;
        int valuesSize = bucketCount * valueSize;
        int size = headerSize + bucketsSize + entriesSize + keysSize + valuesSize;
        int offset = Data->AllocBytes(size);
        
        ref int countRef = ref GetRef<int>(offset + CountOffset);
        countRef = 0;
        ref int bucketCountRef = ref GetRef<int>(offset + BucketCountOffset);
        bucketCountRef = bucketCount;

        unsafe
        {
            int bucketsOffset = offset + headerSize;
            byte* bucketsPtr = Data->GetDataPointer(bucketsOffset);
            int* buckets = (int*)bucketsPtr;
            for (int i = 0; i < bucketCount; i++)
                buckets[i] = -1;
        }
        
        return offset;
    }
}
