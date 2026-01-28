namespace XM
{
    /// <summary>
    /// 配置覆盖模式
    /// </summary>
    public enum OverrideMode
    {
        /// <summary>无覆盖，新增配置</summary>
        None,
        /// <summary>追加</summary>
        Add,
        /// <summary>删除</summary>
        Delete,
        /// <summary>修改</summary>
        Modify
    }

    public interface IXConfig
    {
        /// <summary>由 ConfigDataCenter 在 RegisterData 时分配并写回，用于 CfgI→TUnmanaged 映射</summary>
        public CfgI Data{get;set;}
    }

    public interface IXConfig<T, TUnmanaged>
        : IXConfig
        where T : IXConfig<T, TUnmanaged>
        where TUnmanaged : unmanaged, IConfigUnManaged<TUnmanaged>
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

    public struct StrI
    {
        public int Id;
    }

    public struct LabelI
    {
        public ModI DefinedModId;
        public int labelId;
    }

    public struct LabelS
    {
        public string ModName;

        public string LabelName;
    }
}