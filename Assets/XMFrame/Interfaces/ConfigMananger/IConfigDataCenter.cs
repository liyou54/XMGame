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
       bool  Convert(TSource source,out TTarget target);
    }

    public interface IConfigDataCenter : IManager<IConfigDataCenter>
    {
        bool TryGetConfigBySingleIndex<TData ,TIndex>(in TIndex index, out TData data)
            where TIndex : IConfigIndexGroup<TData>
            where TData : unmanaged, IConfigUnManaged<TData>;

        bool TryGetConfig<T>(out T data) where T : unmanaged, IConfigUnManaged<T>;

        /// <summary>
        /// 从配置中心获取转换器
        /// </summary>
        ITypeConverter<TSource, TTarget> GetConverter<TSource, TTarget>(string domain = "");

        /// <summary>
        /// 根据类型获取转换器（不需要domain）
        /// </summary>
        ITypeConverter<TSource, TTarget> GetConverterByType<TSource, TTarget>();

        T GetConverter<T>();
        
        /// <summary>
        /// 检查是否存在转换器
        /// </summary>
        bool HasConverter<TSource, TTarget>(string domain = "");

        /// <summary>
        /// 根据类型检查是否存在转换器
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
        /// 通过 TblS 获取 ClassHelper 实例
        /// </summary>
        ConfigClassHelper GetClassHelperByTable(TblS tableDefine);

        public void RegisterData<T>(T data) where T : IXConfig;


        /// <summary>
        /// 根据 (TblS, ModS, ConfigName) 解析已分配的 CfgI，供 FillToUnmanaged 外键解析。
        /// </summary>
        bool TryGetCfgI(TblS tableDefine, ModS mod, string configName, out CfgI cfgI);

        /// <summary>
        /// 从 CfgS 查询 CfgI
        /// </summary>
        bool TryGetCfgI(CfgS cfgS, out CfgI cfgI);
        
        /// <summary>
        /// 从 CfgI 反查 CfgS（用于 ToString 等调试功能）
        /// </summary>
        bool TryGetCfgS(CfgI cfgI, out CfgS cfgS);
        
        /// <summary>
        /// 检查指定表中是否存在指定配置（供 Helper 的递归判断父类是否存在使用）
        /// </summary>
        bool TryExistsConfig(TblI table, ModS mod, string configName);

        /// <summary>
        /// 从 TblS 获取 TblI
        /// </summary>
        TblI GetTblI(TblS tableDefine);

        /// <summary>
        /// 为配置分配唯一的 CfgI 索引
        /// </summary>
        /// <param name="cfgS">配置键</param>
        /// <param name="table">表句柄</param>
        /// <returns>分配的配置索引</returns>
        CfgI AllocCfgIndex(CfgS cfgS, TblI table);

    }
}