using System;

public readonly struct XBlobPtr
{
    internal readonly int Offset;
    internal XBlobPtr(int offset) => Offset = offset;

    public XBlobPtr<T> As<T>() where T : unmanaged
    {
        return new XBlobPtr<T>(Offset);
    }

    public XBlobArray<T> AsArray<T>() where T : unmanaged
    {
        return new XBlobArray<T>(Offset);
    }
}

public readonly struct XBlobPtr<T> where T : unmanaged
{
    internal readonly int Offset;
    internal XBlobPtr(int offset) => Offset = offset;

    public XBlobPtr As()
    {
        return new XBlobPtr(Offset);
    }

    public T Get(XBlobContainer container)
    {
       return container.Get<T>(Offset);
    }
}
