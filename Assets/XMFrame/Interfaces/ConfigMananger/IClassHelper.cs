using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Xml;
using XM;

namespace XM.Contracts.Config
{
    /// <summary>
    /// 配置解析上下文（文件路径、行号、覆盖模式），用于严格/宽松错误处理。
    /// </summary>
    public struct ConfigParseContext
    {
        public string FilePath;
        public int Line;
        public OverrideMode Mode;
    }

    /// <summary>
    /// 配置加载辅助类非泛型基类。XML 反序列化由生成的 *ClassHelper 静态代码实现，无反射。
    /// TryParse/GetXmlFieldValue/Log 等通用解析方法已迁至 <see cref="ConfigParseHelper"/>，本类仅做委托。
    /// </summary>
    public abstract class ConfigClassHelper
    {
        /// <summary>
        /// 解析告警回调（可由外部设置为 UnityEngine.Debug.LogWarning 等）。委托给 <see cref="ConfigParseHelper.OnParseWarning"/>。
        /// </summary>
        public static Action<string> OnParseWarning
        {
            get => ConfigParseHelper.OnParseWarning;
            set => ConfigParseHelper.OnParseWarning = value;
        }

        /// <summary>
        /// 解析错误回调（严格模式下解析失败时调用，含 文件、行、字段）。委托给 <see cref="ConfigParseHelper.OnParseError"/>。
        /// </summary>
        public static Action<string> OnParseError
        {
            get => ConfigParseHelper.OnParseError;
            set => ConfigParseHelper.OnParseError = value;
        }

        public abstract TblS GetTblS();
        public abstract IXConfig Create();

        public abstract void SetTblIDefinedInMod(TblI tbl);

        /// <summary>
        /// 从 XML 反序列化配置（无上下文，兼容用）。默认委托给带 context 重载并传入 default。
        /// </summary>
        public virtual IXConfig DeserializeConfigFromXml(XmlElement configItem, ModS mod, string configName)
        {
            return DeserializeConfigFromXml(configItem, mod, configName, default);
        }

        /// <summary>
        /// 从 XML 反序列化配置（带解析上下文）。基类默认实现：先 Create，再 ParseAndFillFromXml，context 会一路传递到 ParseAndFillFromXml/ParseXXX，避免线程切换导致上下文错乱。
        /// </summary>
        public virtual IXConfig DeserializeConfigFromXml(XmlElement configItem, ModS mod, string configName,
            in ConfigParseContext context)
        {
            var config = Create();
            ParseAndFillFromXml(config, configItem, mod, configName, context);
            return config;
        }

        /// <summary>
        /// 将 XML 节点解析并填入已有配置实例（无上下文，兼容用）。默认委托给带 context 重载并传入 default。
        /// </summary>
        public virtual void ParseAndFillFromXml(IXConfig target, XmlElement configItem, ModS mod, string configName)
        {
            ParseAndFillFromXml(target, configItem, mod, configName, default);
        }

        /// <summary>
        /// 将 XML 节点解析并填入已有配置实例，并传入解析上下文（供 ParseXXX 打 Error/Warning 使用）。
        /// 由生成的 *ClassHelper 重写，按字段调用 ParseXXX 并赋值。
        /// </summary>
        public abstract void ParseAndFillFromXml(IXConfig target, XmlElement configItem, ModS mod, string configName,
            in ConfigParseContext context);

        public abstract void AllocUnManagedAndInitHeadVal(TblI table, ConcurrentDictionary<CfgS, IXConfig> kvValue,
            object configHolder);

        /// <summary>
        /// 获取“链接到本表”的 ClassHelper 列表（其 LinkHelperType 指向本 Helper 类型）。由 ConfigDataCenter 在初始化时通过 RegisterSubLinkHelper 填充。
        /// </summary>
        private readonly List<ConfigClassHelper> _subLinkHelpers = new();

        public virtual IReadOnlyList<ConfigClassHelper> GetSubLinkHelper() => _subLinkHelpers;

        /// <summary>
        /// 由 ConfigDataCenter 在初始化时调用，注册一个将本表作为链接目标的 Helper。
        /// </summary>
        public void RegisterSubLinkHelper(ConfigClassHelper sub)
        {
            if (sub != null && !_subLinkHelpers.Contains(sub))
                _subLinkHelpers.Add(sub);
        }

        public abstract Type GetLinkHelperType();

        public abstract void FillBasicData(TblI tblI, ConcurrentDictionary<CfgS, IXConfig> kvValue,
            object configHolder);

        public abstract void AllocContainerWithoutFill(TblI tblI,TblS tblS, ConcurrentDictionary<CfgS, IXConfig> kvValue,
            ConcurrentDictionary<TblS, ConcurrentDictionary<CfgS, IXConfig>> allData,
            object configHolder);
    }
}