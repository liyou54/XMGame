using System;
using Unity.Burst;

[BurstCompile]
public readonly struct XBlobPtr
{
    internal readonly int Offset;
    internal XBlobPtr(int offset) => Offset = offset;
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