using System.Xml;
using XMFrame.Interfaces;
using XMFrame.Interfaces.ConfigMananger;

namespace XMFrame
{
    /// <summary>
    /// ClassHelper 泛型基类，持有 TableDefine 并实现 GetTableDefine，减少生成代码量。
    /// 其余与 unmanaged 桥接的方法由子类实现。
    /// </summary>
    public abstract class ConfigClassHelperBase<TConfig, TUnmanaged> : IConfigClassHelper<TConfig>
        where TConfig : XConfig<TConfig, TUnmanaged>
        where TUnmanaged : unmanaged, IConfigUnManaged<TUnmanaged>
    {
        protected readonly IConfigDataCenter DataCenter;
        protected readonly TableDefine TableDefine;

        protected ConfigClassHelperBase(IConfigDataCenter dataCenter, TableDefine tableDefine)
        {
            DataCenter = dataCenter ?? throw new System.ArgumentNullException(nameof(dataCenter));
            TableDefine = tableDefine;
        }

        public TableDefine GetTableDefine() => TableDefine;

        public abstract (ModKey mod, string configName) GetPrimaryKey(XConfig config);
        public abstract void SetCfgId(XConfig config, CfgId cfgId);
        public abstract void FillToUnmanaged(IConfigDataWriter writer, TableHandle tableHandle, XConfig config, CfgId cfgId);
        public abstract void AllocTableMap(IConfigDataWriter writer, TableHandle tableHandle, int capacity);
        public abstract void AddPrimaryKeyOnly(IConfigDataWriter writer, TableHandle tableHandle, CfgId cfgId);

        public abstract void RegisterToManager(XmlElement element);
        public abstract void LoadFromXml(string xmlFilePath);
        public abstract TConfig LoadFromXmlElement(XmlElement element);
    }
}
