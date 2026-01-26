using XMFrame.Interfaces.ConfigMananger;

namespace XMFrame.Interfaces
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
        /// <summary>
        /// 注册配置表
        /// </summary>
        void RegisterConfigTable();

        
        bool TryGetConfigBySingleIndex<TData ,TIndex>(in TIndex index, out TData data)
            where TIndex : IConfigIndexGroup<TData>
            where TData : unmanaged, IConfigUnManaged<TData>;

        bool TryGetConfig<T>(out T data) where T : unmanaged, IConfigUnManaged<T>;

        /// <summary>
        /// 注册配置表（泛型版本）
        /// </summary>
        void RegisterConfigTable<T>() where T : XMFrame.XConfig;

        /// <summary>
        /// 从配置中心获取转换器
        /// </summary>
        ITypeConverter<TSource, TTarget> GetConverter<TSource, TTarget>(string domain = "");

        /// <summary>
        /// 检查是否存在转换器
        /// </summary>
        bool HasConverter<TSource, TTarget>(string domain = "");

        /// <summary>
        /// 获取指定类型的 ClassHelper 实例（泛型版本）
        /// </summary>
        IConfigClassHelper<T> GetClassHelper<T>() where T : XMFrame.XConfig;

        /// <summary>
        /// 通过 Type 获取 ClassHelper 实例（非泛型版本）
        /// </summary>
        IConfigClassHelper GetClassHelper(System.Type configType);

        /// <summary>
        /// 通过 TableDefine 获取 ClassHelper 实例
        /// </summary>
        IConfigClassHelper GetClassHelperByTable(TableDefine tableDefine);

        public void RegisterData<T>(T data)where T : XConfig;
        
        public void UpdateData<T>(T data)where T : XConfig;
        
        
    }
}