using System.Xml;
using XMFrame;

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

        /// <summary>
        /// 返回该 Helper 对应的表定义，供 DataCenter 解析 TableHandle。
        /// </summary>
        TableDefine GetTableDefine();

        /// <summary>
        /// 从 config 取主键 (ModKey, ConfigName)，供 DataCenter 分配 CfgId 与 ConfigKey→CfgId 映射。
        /// </summary>
        (ModKey mod, string configName) GetPrimaryKey(XConfig config);

        /// <summary>
        /// 将分配好的 CfgId 写回 config.Data。
        /// </summary>
        void SetCfgId(XConfig config, CfgId cfgId);

        /// <summary>
        /// 由 helper 申请 TUnmanaged、按 config 与 cfgId 填充并注册到 writer。传递 CfgId，无装箱。
        /// </summary>
        void FillToUnmanaged(IConfigDataWriter writer, TableHandle tableHandle, XConfig config, CfgId cfgId);

        /// <summary>
        /// 向 writer 申请该表的 blob Map 容量。
        /// </summary>
        void AllocTableMap(IConfigDataWriter writer, TableHandle tableHandle, int capacity);

        /// <summary>
        /// 向 writer 仅插入主键，值为 default(TUnmanaged)。
        /// </summary>
        void AddPrimaryKeyOnly(IConfigDataWriter writer, TableHandle tableHandle, CfgId cfgId);
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
