using System;
using Unity.Burst;

[BurstCompile]
public readonly struct XBlobPtr
{
    internal readonly int Offset;
    internal XBlobPtr(int offset) => Offset = offset;

    /// <summary>从 blob 内偏移构造指针，用于 AllocMapByTypes 等返回的 offset。</summary>
    public static XBlobPtr FromOffset(int offset) => new XBlobPtr(offset);

    /// <summary>向 Container 申请：在容器上分配一块 HashMap，返回指向该 blob HashMap 容器的指针。</summary>
    /// <param name="container">XBlob 容器，从中申请</param>
    /// <param name="keyType">键类型（如 typeof(CfgId)）</param>
    /// <param name="valueType">值类型（如 TUnmanaged）</param>
    /// <param name="capacity">初始容量</param>
    /// <returns>指向新分配的 XBlobMap 的指针，可用 AsMap / AsMapKey 操作</returns>
    public static XBlobPtr AllocMapFrom(in XBlobContainer container, Type keyType, Type valueType, int capacity)
    {
        int offset = container.AllocMapByTypes(keyType, valueType, capacity);
        return FromOffset(offset);
    }

    /// <summary>泛型分配 Map 并返回指针，供 ConfigData 等无反射调用。</summary>
    public static XBlobPtr AllocMapFrom<TKey, TValue>(in XBlobContainer container, int capacity)
        where TKey : unmanaged, IEquatable<TKey>
        where TValue : unmanaged
    {
        int offset = container.AllocMapOffset<TKey, TValue>(capacity);
        return FromOffset(offset);
    }

    /// <summary>向 blob HashMap 容器申请：同 AllocMapFrom，语义为“向 blob 内的 HashMap 容器申请一块新 Map”。</summary>
    public static XBlobPtr AllocHashMapFrom(in XBlobContainer container, Type keyType, Type valueType, int capacity)
        => AllocMapFrom(container, keyType, valueType, capacity);

    /// <summary>blob 内偏移，供 ConfigData 等按类型操作 Map 时使用。</summary>
    public int OffsetValue => Offset;

    public bool Valid => Offset > 0;

    [BurstCompile]
    public XBlobPtr<T> As<T>() where T : unmanaged
    {
        return new XBlobPtr<T>(Offset);
    }

    [BurstCompile]
    public XBlobArray<T> AsArray<T>() where T : unmanaged
    {
        return new XBlobArray<T>(Offset);
    }
    
    [BurstCompile]
    public XBlobMap<T,TV> AsMap<T,TV>() where T : unmanaged, IEquatable<T> where TV : unmanaged
    {
        return new XBlobMap<T,TV>(Offset);
    }
    
    /// <summary>
    /// 将当前指针转换为 XBlobMapKey 外观，用于只操作键的映射（外观模式）
    /// 适用于只需要检查键是否存在，而不需要访问值的场景，性能更优
    /// </summary>
    /// <typeparam name="TKey">键类型，必须满足 unmanaged 和 IEquatable 约束</typeparam>
    /// <returns>XBlobMapKey 外观实例，可用于键的查询操作</returns>
    [BurstCompile]
    public XBlobMapKey<TKey> AsMapKey<TKey>() 
    where TKey : unmanaged, IEquatable<TKey>
    {
        return new XBlobMapKey<TKey>(Offset);
    }
}

[BurstCompile]
public readonly struct XBlobPtr<T> where T : unmanaged
{
    internal readonly int Offset;
    internal XBlobPtr(int offset) => Offset = offset;

    [BurstCompile]
    public XBlobPtr As()
    {
        return new XBlobPtr(Offset);
    }

    [BurstCompile]
    public T Get(in XBlobContainer container)
    {
        return container.Get<T>(Offset);
    }
}