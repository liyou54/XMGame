using System.Xml;

namespace XMFrame.Interfaces.ConfigMananger
{
    /// <summary>
    /// 配置加载辅助类非泛型基接口
    /// </summary>
    public interface IConfigClassHelper
    {
        /// <summary>
        /// 从 XML 元素加载配置并注册到管理器
        /// </summary>
        void RegisterToManager(XmlElement element);
        
        /// <summary>
        /// 从 XML 文件加载所有配置并注册到管理器
        /// </summary>
        void LoadFromXml(string xmlFilePath);
    }
    
    /// <summary>
    /// 配置加载辅助类泛型基接口
    /// </summary>
    public interface IConfigClassHelper<T> : IConfigClassHelper where T : XMFrame.XConfig
    {
        /// <summary>
        /// 从 XML 元素加载单个配置项，返回配置对象
        /// </summary>
        T LoadFromXmlElement(XmlElement element);
    }
}
