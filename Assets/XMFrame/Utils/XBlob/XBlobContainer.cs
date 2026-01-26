using System;
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
    
    public bool IsValid => Data != null;
    
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
    
    public T Get<T>(int offset) where T : unmanaged
    {
        ThrowIfInvalid();
        return Data->Get<T>(offset);
    }

    public ref T GetRef<T>(int offset) where T : unmanaged
    {
        ThrowIfInvalid();
        return ref Data->GetRef<T>(offset);
    }

    internal XBlobArrayView<T> GetArrayView<T>(int offset) where T : unmanaged
    {
        ThrowIfInvalid();
        return Data->GetArrayView<T>(offset);
    }

    internal byte* GetDataPointer(int offset)
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

    internal XBlobArray<T> AllocArray<T>(int capacity) where T : unmanaged
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

    internal XBlobMap<TKey, TValue> AllocMap<TKey, TValue>(int capacity) 
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
    {
        ThrowIfInvalid();
        if (capacity <= 0)
            throw new ArgumentException("Capacity must be positive", nameof(capacity));
        
        int offset = AllocHashMap<TKey, TValue>(capacity);
        return new XBlobMap<TKey, TValue>(offset);
    }

    internal XBlobSet<T> AllocSet<T>(int capacity) 
        where T : unmanaged, IEquatable<T>
    {
        ThrowIfInvalid();
        if (capacity <= 0)
            throw new ArgumentException("Capacity must be positive", nameof(capacity));
        
        int offset = AllocHashSet<T>(capacity);
        return new XBlobSet<T>(offset);
    }

    internal XBlobMultiMap<TKey, TValue> AllocMultiMap<TKey, TValue>(int capacity) 
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

    private void ThrowIfInvalid()
    {
        if (!IsValid)
            throw new InvalidOperationException("Container is not valid");
    }

    private int AllocHashMap<TKey, TValue>(int bucketCount) 
        where TKey : unmanaged
        where TValue : unmanaged
    {
        // Map 布局: [Count(int)] [BucketCount(int)] [KeyEntries: bucketCount * KeyEntry] [Values: bucketCount * TValue]
        // KeyEntry 结构: Key(TKey) + HashCode(int)
        int keyEntrySize = Marshal.SizeOf<TKey>() + sizeof(int);
        int valueSize = Marshal.SizeOf<TValue>();
        int size = KeyEntriesOffset + bucketCount * keyEntrySize + bucketCount * valueSize;
        int offset = Data->AllocBytes(size);
        
        // 初始化 Count = 0, BucketCount = bucketCount
        ref int countRef = ref GetRef<int>(offset + CountOffset);
        countRef = 0;
        ref int bucketCountRef = ref GetRef<int>(offset + BucketCountOffset);
        bucketCountRef = bucketCount;
        
        // 初始化所有 KeyEntry 的 HashCode 为 int.MinValue（表示空槽）
        // 因为 AllocBytes 会清零内存（HashCode 为 0），但我们需要 int.MinValue 来表示空槽
        int keyEntriesOffset = offset + KeyEntriesOffset;
        int hashCodeOffset = Marshal.SizeOf<TKey>(); // HashCode 在 KeyEntry 中的偏移量
        unsafe
        {
            byte* keyEntriesPtr = Data->GetDataPointer(keyEntriesOffset);
            for (int i = 0; i < bucketCount; i++)
            {
                int* hashCodePtr = (int*)(keyEntriesPtr + i * keyEntrySize + hashCodeOffset);
                *hashCodePtr = int.MinValue; // 设置为 EmptyHashCode
            }
        }
        
        return offset;
    }

    private int AllocHashSet<T>(int bucketCount) where T : unmanaged
    {
        // Set 布局: [Count(int)] [BucketCount(int)] [Buckets: bucketCount * int] [Entries: bucketCount * Entry]
        // Entry 结构: HashCode(int) + Next(int) + Value(T)
        int entrySize = sizeof(int) + sizeof(int) + Marshal.SizeOf<T>();
        int size = BucketsOffset + bucketCount * sizeof(int) + bucketCount * entrySize;
        int offset = Data->AllocBytes(size);
        
        // 初始化 Count = 0, BucketCount = bucketCount
        ref int countRef = ref GetRef<int>(offset + CountOffset);
        countRef = 0;
        ref int bucketCountRef = ref GetRef<int>(offset + BucketCountOffset);
        bucketCountRef = bucketCount;
        
        return offset;
    }

    private int AllocMultiHashMap<TKey, TValue>(int bucketCount) 
        where TKey : unmanaged
        where TValue : unmanaged
    {
        // MultiMap 布局: [Count(int)] [BucketCount(int)] [Buckets: bucketCount * int] [Entries: bucketCount * Entry] [Keys: bucketCount * TKey] [Values: bucketCount * TValue]
        // Entry 结构: HashCode(int) + Next(int) + ValueNext(int)
        int entrySize = sizeof(int) + sizeof(int) + sizeof(int);
        int keySize = Marshal.SizeOf<TKey>();
        int valueSize = Marshal.SizeOf<TValue>();
        int size = BucketsOffset + bucketCount * sizeof(int) + bucketCount * entrySize + bucketCount * keySize + bucketCount * valueSize;
        int offset = Data->AllocBytes(size);
        
        // 初始化 Count = 0, BucketCount = bucketCount
        ref int countRef = ref GetRef<int>(offset + CountOffset);
        countRef = 0;
        ref int bucketCountRef = ref GetRef<int>(offset + BucketCountOffset);
        bucketCountRef = bucketCount;
        
        return offset;
    }
}
