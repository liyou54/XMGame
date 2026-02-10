using System;
using System.Collections.Generic;
using System.Xml;
using Unity.Collections;
using XM;
using XM.Contracts;
using XM.Contracts.Config;

namespace XM
{
    /// <summary>
    /// UIConfig 的配置加载辅助类，用于从 XML 反序列化（静态代码生成，无反射）。
    /// </summary>
    public class UIConfigClassHelper : ConfigClassHelper<global::XM.UIConfig, global::XM.UIConfigUnManaged>
    {
        public static UIConfigClassHelper Instance { get; private set; }
        public static TblI TblI { get; private set; }
        public static TblS TblS { get; private set; }
        private static readonly string __modName;

        /// <summary>
        /// 静态构造函数
        /// </summary>
        static UIConfigClassHelper()
        {
            const string __tableName = "UIConfig";
            __modName = "Core";
            CfgS<global::XM.UIConfigUnManaged>.Table = new TblS(new ModS(__modName), __tableName);
            TblS = new TblS(new ModS(__modName), __tableName);
            Instance = new UIConfigClassHelper();
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        public UIConfigClassHelper()
        {
        }
        /// <summary>获取表静态标识</summary>
        public override TblS GetTblS()
        {
            return TblS;
        }
        /// <summary>设置表所属Mod</summary>
        public override void SetTblIDefinedInMod(TblI tbl)
        {
            _definedInMod = tbl;
        }
        /// <summary>
        /// 从 XML 解析并填充配置对象
        /// </summary>
        /// <param name="target">目标配置对象</param>
        /// <param name="configItem">XML 元素</param>
        /// <param name="mod">Mod 标识</param>
        /// <param name="configName">配置名称</param>
        /// <param name="context">解析上下文</param>
        public override void ParseAndFillFromXml(
            IXConfig target,
            XmlElement configItem,
            ModS mod,
            string configName,
            in ConfigParseContext context)
        {
            var config = (global::XM.UIConfig)target;

            // 解析所有字段
            config.Id = ParseId(configItem, mod, configName, context);
            config.UILayer = ParseUILayer(configItem, mod, configName, context);
            config.UIType = ParseUIType(configItem, mod, configName, context);
            config.IsFullScreen = ParseIsFullScreen(configItem, mod, configName, context);
            config.AssetPath = ParseAssetPath(configItem, mod, configName, context);
            config.type = Parsetype(configItem, mod, configName, context);
        }
        #region 字段解析方法 (ParseXXX)

        /// <summary>
        /// 解析 Id 字段
        /// </summary>
        private static global::XM.Contracts.Config.CfgS<global::XM.UIConfig> ParseId(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            // XmlKey 字段: 从 configName 参数读取
            // CfgS 类型：从 configName 参数读取并解析
            if (string.IsNullOrEmpty(configName))
            {
                return default;
            }

            // 尝试解析 CfgS 格式（ModName::ConfigName）
            if (global::XM.Contracts.Config.ConfigParseHelper.TryParseCfgSString(configName, "Id", out var modName, out var cfgName))
            {
                return new global::XM.Contracts.Config.CfgS<global::XM.UIConfig>(new global::XM.Contracts.Config.ModS(modName), cfgName);
            }

            // 如果 configName 不包含 :: 分隔符，使用当前 mod.Name 补充
            return new global::XM.Contracts.Config.CfgS<global::XM.UIConfig>(mod, configName);
        }

        /// <summary>
        /// 解析 UILayer 字段
        /// </summary>
        private static global::XM.Contracts.EUILayer ParseUILayer(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var xmlValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "UILayer");

            if (string.IsNullOrEmpty(xmlValue))
            {
                return default;
            }

            if (global::System.Enum.TryParse<global::XM.Contracts.EUILayer>(xmlValue, out var parsedValue))
            {
                return parsedValue;
            }

            global::XM.Contracts.Config.ConfigParseHelper.LogParseError(context, "UILayer", $"无法解析枚举值: {xmlValue}");
            return default;
        }

        /// <summary>
        /// 解析 UIType 字段
        /// </summary>
        private static global::XM.Contracts.EUIType ParseUIType(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var xmlValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "UIType");

            if (string.IsNullOrEmpty(xmlValue))
            {
                return default;
            }

            if (global::System.Enum.TryParse<global::XM.Contracts.EUIType>(xmlValue, out var parsedValue))
            {
                return parsedValue;
            }

            global::XM.Contracts.Config.ConfigParseHelper.LogParseError(context, "UIType", $"无法解析枚举值: {xmlValue}");
            return default;
        }

        /// <summary>
        /// 解析 IsFullScreen 字段
        /// </summary>
        private static bool ParseIsFullScreen(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var xmlValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "IsFullScreen");

            if (string.IsNullOrEmpty(xmlValue))
            {
                return default;
            }

            if (global::XM.Contracts.Config.ConfigParseHelper.TryParseBool(xmlValue, "IsFullScreen", out var parsedValue))
            {
                return parsedValue;
            }

            return default;
        }

        /// <summary>
        /// 解析 AssetPath 字段
        /// </summary>
        private static global::XM.XAssetPath ParseAssetPath(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var xmlValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "AssetPath");

            if (string.IsNullOrEmpty(xmlValue))
            {
                return default;
            }

            // 通过 XmlTypeConverter: XAssetPathConvert
            if (global::XM.XAssetPathConvert.I.Convert(xmlValue, mod.Name, out var parsedValue))
            {
                return parsedValue;
            }

            return default;
        }

        /// <summary>
        /// 解析 type 字段
        /// </summary>
        private static global::System.Type Parsetype(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var xmlValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "type");

            if (string.IsNullOrEmpty(xmlValue))
            {
                return default;
            }

            // 通过 XmlTypeConverter: TypeConvert
            if (global::XM.TypeConvert.I.Convert(xmlValue, mod.Name, out var parsedValue))
            {
                return parsedValue;
            }

            return default;
        }


        #endregion

        /// <summary>
        /// 分配容器并填充非托管数据
        /// </summary>
        /// <param name="value">托管配置对象</param>
        /// <param name="tbli">表ID</param>
        /// <param name="cfgi">配置ID</param>
        /// <param name="data">非托管数据结构（ref 传递）</param>
        /// <param name="configHolderData">配置数据持有者</param>
        /// <param name="linkParent">Link 父节点指针</param>
        public override void AllocContainerWithFillImpl(
            IXConfig value,
            TblI tbli,
            CfgI cfgi,
            ref global::XM.UIConfigUnManaged data,
            XM.ConfigDataCenter.ConfigDataHolder configHolderData,
            XBlobPtr? linkParent = null)
        {
            var config = (global::XM.UIConfig)value;

            // 填充基本类型字段
            if (TryGetCfgI(config.Id, out var IdCfgI))
            {
                data.Id = IdCfgI.As<global::XM.UIConfigUnManaged>();
            }
            data.UILayer = config.UILayer;
            data.UIType = config.UIType;
            data.IsFullScreen = config.IsFullScreen;
            if (global::XM.XAssetPathToIConvert.I.Convert(config.AssetPath, cfgi.Mod.GetModName(), out var AssetPathUnmanaged))
            {
                data.AssetPath = AssetPathUnmanaged;
            }
            if (global::XM.TypeConvertI.I.Convert(config.type, cfgi.Mod.GetModName(), out var typeUnmanaged))
            {
                data.type = typeUnmanaged;
            }
        }

        /// <summary>配置定义所属的 Mod</summary>
        public TblI _definedInMod;
    }
}
