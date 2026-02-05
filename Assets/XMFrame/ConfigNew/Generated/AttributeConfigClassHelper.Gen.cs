using System;
using System.Collections.Generic;
using System.Xml;
using Unity.Collections;
using XM;
using XM.ConfigNew.Tests.Data;
using XM.Contracts;
using XM.Contracts.Config;

namespace XM.ConfigNew.Tests.Data
{
    /// <summary>
    /// AttributeConfig 的配置加载辅助类，用于从 XML 反序列化（静态代码生成，无反射）。
    /// </summary>
    public class AttributeConfigClassHelper : ConfigClassHelper<global::XM.ConfigNew.Tests.Data.AttributeConfig, global::XM.ConfigNew.Tests.Data.AttributeConfigUnmanaged>
    {
        public static AttributeConfigClassHelper Instance { get; private set; }
        public static TblI TblI { get; private set; }
        public static TblS TblS { get; private set; }

        /// <summary>
        /// 静态构造函数
        /// </summary>
        static AttributeConfigClassHelper()
        {
            const string __tableName = "Attribute";
            const string __modName = "Default";
            CfgS<global::XM.ConfigNew.Tests.Data.AttributeConfigUnmanaged>.Table = new TblS(new ModS(__modName), __tableName);
            TblS = new TblS(new ModS(__modName), __tableName);
            Instance = new AttributeConfigClassHelper();
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        public AttributeConfigClassHelper()
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
            var config = (global::XM.ConfigNew.Tests.Data.AttributeConfig)target;

            // 解析所有字段
            config.Type = ParseType(configItem, mod, configName, context);
            config.BaseValue = ParseBaseValue(configItem, mod, configName, context);
            config.Multiplier = ParseMultiplier(configItem, mod, configName, context);
            config.BonusValue = ParseBonusValue(configItem, mod, configName, context);
        }
        /// <summary>获取 Link Helper 类型</summary>
        public override Type GetLinkHelperType()
        {
            return null;
        }
        #region 字段解析方法 (ParseXXX)

        /// <summary>
        /// 解析 Type 字段
        /// </summary>
        private static global::XM.ConfigNew.Tests.Data.EAttributeType ParseType(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var xmlValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "Type");

            if (string.IsNullOrEmpty(xmlValue))
            {
                return default;
            }

            if (global::System.Enum.TryParse<global::XM.ConfigNew.Tests.Data.EAttributeType>(xmlValue, out var parsedValue))
            {
                return parsedValue;
            }

            global::XM.Contracts.Config.ConfigParseHelper.LogParseError(context, "Type", $"无法解析枚举值: {xmlValue}");
            return default;
        }

        /// <summary>
        /// 解析 BaseValue 字段
        /// </summary>
        private static int ParseBaseValue(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var xmlValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "BaseValue");

            // 默认值: 0
            if (string.IsNullOrEmpty(xmlValue))
            {
                xmlValue = "0";
            }

            if (global::XM.Contracts.Config.ConfigParseHelper.TryParseInt(xmlValue, "BaseValue", out var parsedValue))
            {
                return parsedValue;
            }

            return default;
        }

        /// <summary>
        /// 解析 Multiplier 字段
        /// </summary>
        private static float ParseMultiplier(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var xmlValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "Multiplier");

            // 默认值: 1.0
            if (string.IsNullOrEmpty(xmlValue))
            {
                xmlValue = "1.0";
            }

            if (global::XM.Contracts.Config.ConfigParseHelper.TryParseFloat(xmlValue, "Multiplier", out var parsedValue))
            {
                return parsedValue;
            }

            return default;
        }

        /// <summary>
        /// 解析 BonusValue 字段
        /// </summary>
        private static int? ParseBonusValue(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var xmlValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "BonusValue");

            if (string.IsNullOrEmpty(xmlValue))
            {
                return null;
            }

            if (global::XM.Contracts.Config.ConfigParseHelper.TryParseInt(xmlValue, "BonusValue", out var parsedValue))
            {
                return parsedValue;
            }

            return null;
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
            ref global::XM.ConfigNew.Tests.Data.AttributeConfigUnmanaged data,
            XM.ConfigDataCenter.ConfigDataHolder configHolderData,
            XBlobPtr? linkParent = null)
        {
            var config = (global::XM.ConfigNew.Tests.Data.AttributeConfig)value;

            // 填充基本类型字段
            data.Type = config.Type;
            data.BaseValue = config.BaseValue;
            data.Multiplier = config.Multiplier;
            data.BonusValue = config.BonusValue.GetValueOrDefault();
        }
        /// <summary>
        /// 建立 Link 双向引用（链接阶段调用）
        /// </summary>
        /// <param name="config">托管配置对象</param>
        /// <param name="data">非托管数据结构（ref 传递）</param>
        /// <param name="configHolderData">配置数据持有者</param>
        public virtual void EstablishLinks(
            global::XM.ConfigNew.Tests.Data.AttributeConfig config,
            ref global::XM.ConfigNew.Tests.Data.AttributeConfigUnmanaged data,
            XM.ConfigDataCenter.ConfigDataHolder configHolderData)
        {
            // TODO: 实现 Link 双向引用
            // 父→子: 通过 CfgI 查找子配置，填充 XBlobPtr
            // 子→父: 通过 CfgI 查找父配置，填充引用
        }

        /// <summary>配置定义所属的 Mod</summary>
        public TblI _definedInMod;
    }
}
