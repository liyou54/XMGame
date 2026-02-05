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
    /// QuestConfig 的配置加载辅助类，用于从 XML 反序列化（静态代码生成，无反射）。
    /// </summary>
    public class QuestConfigClassHelper : ConfigClassHelper<global::XM.ConfigNew.Tests.Data.QuestConfig, global::XM.ConfigNew.Tests.Data.QuestConfigUnmanaged>
    {
        public static QuestConfigClassHelper Instance { get; private set; }
        public static TblI TblI { get; private set; }
        public static TblS TblS { get; private set; }

        /// <summary>
        /// 静态构造函数
        /// </summary>
        static QuestConfigClassHelper()
        {
            const string __tableName = "Quest";
            const string __modName = "Default";
            CfgS<global::XM.ConfigNew.Tests.Data.QuestConfigUnmanaged>.Table = new TblS(new ModS(__modName), __tableName);
            TblS = new TblS(new ModS(__modName), __tableName);
            Instance = new QuestConfigClassHelper();
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        public QuestConfigClassHelper()
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
            var config = (global::XM.ConfigNew.Tests.Data.QuestConfig)target;

            // 解析所有字段
            config.QuestId = ParseQuestId(configItem, mod, configName, context);
            config.QuestName = ParseQuestName(configItem, mod, configName, context);
            config.MinLevel = ParseMinLevel(configItem, mod, configName, context);
            config.RewardItem = ParseRewardItem(configItem, mod, configName, context);
            config.PreQuest = ParsePreQuest(configItem, mod, configName, context);
        }
        /// <summary>获取 Link Helper 类型</summary>
        public override Type GetLinkHelperType()
        {
            return null;
        }
        #region 字段解析方法 (ParseXXX)

        /// <summary>
        /// 解析 QuestId 字段
        /// </summary>
        private static int ParseQuestId(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var xmlValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "QuestId");

            if (string.IsNullOrEmpty(xmlValue))
            {
                return default;
            }

            if (global::XM.Contracts.Config.ConfigParseHelper.TryParseInt(xmlValue, "QuestId", out var parsedValue))
            {
                return parsedValue;
            }

            return default;
        }

        /// <summary>
        /// 解析 QuestName 字段
        /// </summary>
        private static string ParseQuestName(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var xmlValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "QuestName");

            if (string.IsNullOrEmpty(xmlValue))
            {
                return default;
            }

            // 字符串类型直接返回
            return xmlValue ?? string.Empty;
        }

        /// <summary>
        /// 解析 MinLevel 字段
        /// </summary>
        private static int ParseMinLevel(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var xmlValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "MinLevel");

            // 默认值: 1
            if (string.IsNullOrEmpty(xmlValue))
            {
                xmlValue = "1";
            }

            if (global::XM.Contracts.Config.ConfigParseHelper.TryParseInt(xmlValue, "MinLevel", out var parsedValue))
            {
                return parsedValue;
            }

            return default;
        }

        /// <summary>
        /// 解析 RewardItem 字段
        /// </summary>
        private static global::XM.Contracts.Config.CfgS<global::XM.ConfigNew.Tests.Data.ComplexItemConfig> ParseRewardItem(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            // 解析 CfgS 引用字符串
            var cfgSString = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "RewardItem");
            if (string.IsNullOrEmpty(cfgSString))
            {
                return default;
            }

            if (global::XM.Contracts.Config.ConfigParseHelper.TryParseCfgSString(cfgSString, "RewardItem", out var modName, out var cfgName))
            {
                return new global::XM.Contracts.Config.CfgS<global::XM.ConfigNew.Tests.Data.ComplexItemConfig>(new global::XM.Contracts.Config.ModS(modName), cfgName);
            }

            global::XM.Contracts.Config.ConfigParseHelper.LogParseError(context, "RewardItem", $"无法解析 CfgS 字符串: {cfgSString}");
            return default;
        }

        /// <summary>
        /// 解析 PreQuest 字段
        /// </summary>
        private static global::XM.Contracts.Config.CfgS<global::XM.ConfigNew.Tests.Data.QuestConfig> ParsePreQuest(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            // 解析 CfgS 引用字符串
            var cfgSString = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "PreQuest");
            if (string.IsNullOrEmpty(cfgSString))
            {
                return default;
            }

            if (global::XM.Contracts.Config.ConfigParseHelper.TryParseCfgSString(cfgSString, "PreQuest", out var modName, out var cfgName))
            {
                return new global::XM.Contracts.Config.CfgS<global::XM.ConfigNew.Tests.Data.QuestConfig>(new global::XM.Contracts.Config.ModS(modName), cfgName);
            }

            global::XM.Contracts.Config.ConfigParseHelper.LogParseError(context, "PreQuest", $"无法解析 CfgS 字符串: {cfgSString}");
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
            ref global::XM.ConfigNew.Tests.Data.QuestConfigUnmanaged data,
            XM.ConfigDataCenter.ConfigDataHolder configHolderData,
            XBlobPtr? linkParent = null)
        {
            var config = (global::XM.ConfigNew.Tests.Data.QuestConfig)value;

            // 填充基本类型字段
            data.QuestId = config.QuestId;
            data.QuestName = new global::Unity.Collections.FixedString32Bytes(config.QuestName ?? string.Empty);
            data.MinLevel = config.MinLevel;
            // TODO: RewardItem - CfgS 转 CfgI (链接阶段解析)
            if (TryGetCfgI(config.RewardItem, out var RewardItemCfgI))
            {
                data.RewardItem = RewardItemCfgI.As<global::XM.ConfigNew.Tests.Data.ComplexItemConfigUnmanaged>();
            }
            // TODO: PreQuest - CfgS 转 CfgI (链接阶段解析)
            if (TryGetCfgI(config.PreQuest, out var PreQuestCfgI))
            {
                data.PreQuest = PreQuestCfgI.As<global::XM.ConfigNew.Tests.Data.QuestConfigUnmanaged>();
            }
        }
        /// <summary>
        /// 建立 Link 双向引用（链接阶段调用）
        /// </summary>
        /// <param name="config">托管配置对象</param>
        /// <param name="data">非托管数据结构（ref 传递）</param>
        /// <param name="configHolderData">配置数据持有者</param>
        public virtual void EstablishLinks(
            global::XM.ConfigNew.Tests.Data.QuestConfig config,
            ref global::XM.ConfigNew.Tests.Data.QuestConfigUnmanaged data,
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
