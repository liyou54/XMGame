namespace XMFrame
{
    public abstract class XConfig
    {
    }

    public abstract class XConfig<T, TUnmanaged>
        : XConfig
        where T : XConfig<T, TUnmanaged>
        where TUnmanaged : unmanaged
    {
    }

    public interface IConfigUnManaged<T>
        where T : unmanaged, IConfigUnManaged<T>
    {
    }

    public interface IConfigIndexGroup<TData>
        where TData : unmanaged, IConfigUnManaged<TData>
    {
    }
}