using XM.Contracts.Config;

namespace XM.Contracts
{
    /// <summary>
    /// 类型转换器接口
    /// </summary>
    public interface ITypeConverter<TSource, TTarget>
    {
        /// <summary>
        /// 将源类型转换为目标类型
        /// </summary>
        TTarget Convert(TSource source);
    }

    public interface IConfigDataCenter : IManager<IConfigDataCenter>
    {
        bool TryGetConfigBySingleIndex<TData ,TIndex>(in TIndex index, out TData data)
            where TIndex : IConfigIndexGroup<TData>
            where TData : unmanaged, IConfigUnManaged<TData>;

        bool TryGetConfig<T>(out T data) where T : unmanaged, IConfigUnManaged<T>;

        /// <summary>
        /// 从配置中心获取转换器（按域）
        /// </summary>
        ITypeConverter<TSource, TTarget> GetConverter<TSource, TTarget>(string domain = "");

        /// <summary>
        /// 仅按类型获取转换器：先全局再任意域，返回第一个匹配。供生成代码直接通过类型获取正确转换器，无需传 domain。
        /// </summary>
        ITypeConverter<TSource, TTarget> GetConverterByType<TSource, TTarget>();

        /// <summary>
        /// 检查是否存在转换器
        /// </summary>
        bool HasConverter<TSource, TTarget>(string domain = "");

        /// <summary>
        /// 检查是否存在转换器（按类型，任意域）
        /// </summary>
        bool HasConverterByType<TSource, TTarget>();

        /// <summary>
        /// 获取指定类型的 ClassHelper 实例（泛型版本）
        /// </summary>
        ConfigClassHelper GetClassHelper<T>() where T : XM.IXConfig, new();

        /// <summary>
        /// 通过 Type 获取 ClassHelper 实例（非泛型版本）
        /// </summary>
        ConfigClassHelper GetClassHelper(System.Type configType);

        /// <summary>
        /// 通过 Type 获取 ClassHelper 实例（非泛型版本）
        /// </summary>
        ConfigClassHelper GetClassHelperByHelpType(System.Type configType);
        
        /// <summary>
        /// 通过 TblS 获取 ClassHelper 实例
        /// </summary>
        ConfigClassHelper GetClassHelperByTable(TblS tableDefine);

        public void UpdateData<T>(T data) where T : IXConfig;

        /// <summary>
        /// 根据 (TblS, ModS, ConfigName) 解析已分配的 CfgI，供 FillToUnmanaged 外键解析。
        /// </summary>
        bool TryGetCfgI(TblS tableDefine, ModS mod, string configName, out CfgI cfgI);

        /// <summary>
        /// 从 TblS 获取 TblI
        /// </summary>
        TblI GetTblI(TblS tableDefine);

    }
}