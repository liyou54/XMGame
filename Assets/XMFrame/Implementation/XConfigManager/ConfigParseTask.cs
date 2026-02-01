using System;
using System.Xml;
using XM.Contracts;
using XM.Contracts.Config;

namespace XM
{
    #region ConfigParseTask

    /// <summary>
    /// 配置解析任务：封装单次解析所需参数，支持懒执行与多线程安全。
    /// Execute() 时将 Context 通过参数传入 DeserializeConfigFromXml，避免线程上下文切换导致错乱。
    /// </summary>
    internal sealed class ConfigParseTask
    {
        #region 属性

        /// <summary>解析上下文（文件路径、行号、Override 模式等）</summary>
        public ConfigParseContext Context { get; }

        /// <summary>XML ConfigItem 节点</summary>
        public XmlElement ConfigItem { get; }

        /// <summary>对应表的 ClassHelper</summary>
        public ConfigClassHelper Helper { get; }

        /// <summary>Mod 键（ModS）</summary>
        public ModS ModKey { get; }

        /// <summary>配置名（id 中 "::" 后或当前 Mod 下的 id）</summary>
        public string ConfigName { get; }

        /// <summary>Override 模式</summary>
        public OverrideMode OverrideMode { get; }

        /// <summary>XML 文件路径（用于日志与错误信息）</summary>
        public string XmlFilePath { get; }

        #endregion

        #region 构造

        /// <remarks>主要步骤：保存所有解析所需参数，供 Execute 使用。</remarks>
        public ConfigParseTask(
            ConfigParseContext context,
            XmlElement configItem,
            ConfigClassHelper helper,
            ModS modKey,
            string configName,
            OverrideMode overrideMode,
            string xmlFilePath)
        {
            Context = context;
            ConfigItem = configItem;
            Helper = helper;
            ModKey = modKey;
            ConfigName = configName;
            OverrideMode = overrideMode;
            XmlFilePath = xmlFilePath;
        }

        #endregion

        #region 执行

        /// <summary>
        /// 执行解析：将 Context 通过参数传入 Helper.DeserializeConfigFromXml，不依赖线程静态变量，多线程安全。
        /// </summary>
        /// <remarks>主要步骤：1. 调用 Helper.DeserializeConfigFromXml（传入 context，不含 overrideMode）；2. 成功则返回含 Config 的结果；3. 异常则打日志并返回无效结果。</remarks>
        /// <returns>解析结果；失败时 Config 为 null，IsValid 为 false</returns>
        public ConfigParseResult Execute()
        {
            try
            {
                var ctx = Context;
                var config = Helper.DeserializeConfigFromXml(ConfigItem, ModKey, ConfigName, in ctx);
                return new ConfigParseResult(config, ModKey, ConfigName, XmlFilePath);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"解析配置失败: {XmlFilePath}, 配置: {ConfigName}, 错误: {ex.Message}");
                return new ConfigParseResult(null, ModKey, ConfigName, XmlFilePath);
            }
        }

        #endregion
    }

    #endregion

    #region ConfigParseResult

    /// <summary>
    /// 配置解析结果：包含解析出的 IXConfig 与注册所需的元数据（ModKey、ConfigName、XmlFilePath）。
    /// </summary>
    internal readonly struct ConfigParseResult
    {
        /// <summary>解析出的配置实例，失败时为 null</summary>
        public readonly IXConfig Config;

        /// <summary>Mod 键</summary>
        public readonly ModS ModKey;

        /// <summary>配置名</summary>
        public readonly string ConfigName;

        /// <summary>XML 文件路径</summary>
        public readonly string XmlFilePath;

        /// <remarks>主要步骤：保存解析出的 Config 与元数据（ModKey、ConfigName、XmlFilePath）。</remarks>
        public ConfigParseResult(IXConfig config, ModS modKey, string configName, string xmlFilePath)
        {
            Config = config;
            ModKey = modKey;
            ConfigName = configName;
            XmlFilePath = xmlFilePath;
        }

        /// <summary>结果是否有效（Config 非 null）</summary>
        public bool IsValid => Config != null;
    }

    #endregion
}
