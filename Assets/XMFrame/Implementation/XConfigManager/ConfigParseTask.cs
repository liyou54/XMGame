using System;
using System.Xml;
using XM.Contracts;
using XM.Contracts.Config;

namespace XM
{
    /// <summary>
    /// 配置解析任务代理类，封装单次解析所需的所有参数，支持懒执行与多线程安全。
    /// 在 Execute() 时设置 CurrentParseContext，避免调用方在多线程环境下设置上下文的风险。
    /// </summary>
    internal sealed class ConfigParseTask
    {
        public ConfigParseContext Context { get; }
        public XmlElement ConfigItem { get; }
        public ConfigClassHelper Helper { get; }
        public ModS ModKey { get; }
        public string ConfigName { get; }
        public OverrideMode OverrideMode { get; }
        public TblI TableHandle { get; }
        public string XmlFilePath { get; }

        public ConfigParseTask(
            ConfigParseContext context,
            XmlElement configItem,
            ConfigClassHelper helper,
            ModS modKey,
            string configName,
            OverrideMode overrideMode,
            TblI tableHandle,
            string xmlFilePath)
        {
            Context = context;
            ConfigItem = configItem;
            Helper = helper;
            ModKey = modKey;
            ConfigName = configName;
            OverrideMode = overrideMode;
            TableHandle = tableHandle;
            XmlFilePath = xmlFilePath;
        }

        /// <summary>
        /// 执行解析任务。在当前线程设置 CurrentParseContext，调用 Helper 解析，返回结果。
        /// 可在子线程调用，每个线程有独立的 ThreadStatic 上下文。
        /// </summary>
        public ConfigParseResult Execute()
        {
            var prevContext = ConfigClassHelper.CurrentParseContext;
            try
            {
                ConfigClassHelper.CurrentParseContext = Context;
                var config = Helper.DeserializeConfigFromXml(ConfigItem, ModKey, ConfigName, OverrideMode);
                return new ConfigParseResult(config, TableHandle, ModKey, ConfigName, XmlFilePath);
            }
            catch (Exception ex)
            {
                // 记录错误，返回空结果（主线程 merge 时会跳过）
                UnityEngine.Debug.LogError($"解析配置失败: {XmlFilePath}, 配置: {ConfigName}, 错误: {ex.Message}");
                return new ConfigParseResult(null, TableHandle, ModKey, ConfigName, XmlFilePath);
            }
            finally
            {
                ConfigClassHelper.CurrentParseContext = prevContext;
            }
        }
    }

    /// <summary>
    /// 配置解析结果，包含解析出的 IXConfig 与注册所需的元数据。
    /// </summary>
    internal readonly struct ConfigParseResult
    {
        public readonly IXConfig Config;
        public readonly TblI TableHandle;
        public readonly ModS ModKey;
        public readonly string ConfigName;
        public readonly string XmlFilePath;

        public ConfigParseResult(IXConfig config, TblI tableHandle, ModS modKey, string configName, string xmlFilePath)
        {
            Config = config;
            TableHandle = tableHandle;
            ModKey = modKey;
            ConfigName = configName;
            XmlFilePath = xmlFilePath;
        }

        public bool IsValid => Config != null;
    }
}
