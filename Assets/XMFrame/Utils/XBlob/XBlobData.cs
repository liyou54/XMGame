using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

internal unsafe struct XBlobData : IDisposable
{
    private const int MinInitialCapacity = 64;
    private const int GrowthFactor = 2;

    private byte* _data;
    private Allocator _allocator;
    
    internal int UsedSize { get; private set; }
    internal int Capacity { get; private set; }
    internal bool IsValid => _data != null;
    
    internal void Initialize(Allocator allocator, int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentException("Capacity must be positive", nameof(capacity));
        // 如果已经初始化过，先释放旧资源
        if (_data != null)
        {
            throw new InvalidOperationException("XBlobData has already been initialized. Call Dispose() before reinitializing.");
        }
        _allocator = allocator;
        Capacity = capacity;
        UsedSize = 0;
        _data = (byte*)UnsafeUtility.Malloc(capacity, UnsafeUtility.AlignOf<byte>(), allocator);
    }

    public T Get<T>(int offset) where T : unmanaged
    {
        return *((T*)(_data + offset));
    }

    public ref T GetRef<T>(int offset) where T : unmanaged
    {
        return ref *((T*)(_data + offset));
    }

    internal XBlobArrayView<T> GetArrayView<T>(int offset) where T : unmanaged
    {
        int length = *(int*)(_data + offset);
        T* dataPtr = (T*)(_data + offset + sizeof(int));
        Span<T> dataSpan = new Span<T>(dataPtr, length);
        return new XBlobArrayView<T>
        {
            Length = length,
            Data = dataSpan
        };
    }

    internal byte* GetDataPointer(int offset)
    {
        return _data + offset;
    }

    internal int Alloc<T>() where T : unmanaged
    {
        int size = sizeof(T);
        EnsureCapacity(UsedSize + size);
        
        int offset = UsedSize;
        T* ptr = (T*)(_data + offset);
        *ptr = default;
        UsedSize += size;
        return offset;
    }

    internal int AllocBytes(int size)
    {
        if (size < 0)
            throw new ArgumentException("Size must be non-negative", nameof(size));

        EnsureCapacity(UsedSize + size);
        
        int offset = UsedSize;
        // 清零分配的内存
        UnsafeUtility.MemClear(_data + offset, size);
        UsedSize += size;
        return offset;
    }

    internal void Reserve(int newCapacity)
    {
        if (newCapacity <= Capacity)
            return;
        
        if (_data == null)
            throw new InvalidOperationException("Data is not initialized");
        
        byte* newData = (byte*)UnsafeUtility.Malloc(newCapacity, UnsafeUtility.AlignOf<byte>(), _allocator);
        
        if (UsedSize > 0)
        {
            UnsafeUtility.MemCpy(newData, _data, UsedSize);
        }
        
        UnsafeUtility.Free(_data, _allocator);
        _data = newData;
        Capacity = newCapacity;
    }

    private void EnsureCapacity(int requiredSize)
    {
        if (requiredSize > Capacity)
        {
            int newCapacity = CalculateNewCapacity(requiredSize);
            Reserve(newCapacity);
        }
    }

    private int CalculateNewCapacity(int requiredSize)
    {
        if (Capacity == 0)
        {
            return Math.Max(requiredSize, MinInitialCapacity);
        }
        
        return Math.Max(requiredSize, Capacity * GrowthFactor);
    }

    public void Dispose()
    {
        if (_data != null)
        {
            UnsafeUtility.Free(_data, _allocator);
            _data = null;
            Capacity = 0;
            UsedSize = 0;
            _allocator = Allocator.Invalid;
        }
    }
}