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
        /// 从 XML 文件加载配置
        /// </summary>
        void LoadConfigFromXml<T>(string xmlFilePath) where T : XMFrame.XConfig;

        /// <summary>
        /// 从 XML 元素加载单个配置项
        /// </summary>
        void LoadConfigFromXmlElement<T>(System.Xml.XmlElement element) where T : XMFrame.XConfig;

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
    }
}