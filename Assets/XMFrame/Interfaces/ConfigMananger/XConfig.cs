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

    public struct StrHandle
    {
        public int Id;
    }

    public struct StrLabelHandle
    {
        public ModId DefinedModId;
        public int labelId;
    }

    public struct StrLabel
    {
        public string ModName;

        public string LabelName;
    }
}