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
    /// UltimateComplexConfig 的配置加载辅助类，用于从 XML 反序列化（静态代码生成，无反射）。
    /// </summary>
    public class UltimateComplexConfigClassHelper : ConfigClassHelper<global::XM.ConfigNew.Tests.Data.UltimateComplexConfig, global::XM.ConfigNew.Tests.Data.UltimateComplexConfigUnmanaged>
    {
        public static UltimateComplexConfigClassHelper Instance { get; private set; }
        public static TblI TblI { get; private set; }
        public static TblS TblS { get; private set; }

        /// <summary>
        /// 静态构造函数
        /// </summary>
        static UltimateComplexConfigClassHelper()
        {
            const string __tableName = "UltimateComplex";
            const string __modName = "Default";
            CfgS<global::XM.ConfigNew.Tests.Data.UltimateComplexConfigUnmanaged>.Table = new TblS(new ModS(__modName), __tableName);
            TblS = new TblS(new ModS(__modName), __tableName);
            Instance = new UltimateComplexConfigClassHelper();
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        public UltimateComplexConfigClassHelper()
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
            var config = (global::XM.ConfigNew.Tests.Data.UltimateComplexConfig)target;

            // 解析所有字段
            config.Version = ParseVersion(configItem, mod, configName, context);
            config.IsEnabled = ParseIsEnabled(configItem, mod, configName, context);
            config.Weight = ParseWeight(configItem, mod, configName, context);
            config.IntSet = ParseIntSet(configItem, mod, configName, context);
            config.TypeSet = ParseTypeSet(configItem, mod, configName, context);
            config.NullableIntList = ParseNullableIntList(configItem, mod, configName, context);
            config.FourLevelNested = ParseFourLevelNested(configItem, mod, configName, context);
            config.DictInDict = ParseDictInDict(configItem, mod, configName, context);
            config.ComplexNested = ParseComplexNested(configItem, mod, configName, context);
            config.UnlockSkill = ParseUnlockSkill(configItem, mod, configName, context);
            config.PassiveSkill = ParsePassiveSkill(configItem, mod, configName, context);
            config.IndexField1 = ParseIndexField1(configItem, mod, configName, context);
            config.IndexField2 = ParseIndexField2(configItem, mod, configName, context);
            config.IndexField3 = ParseIndexField3(configItem, mod, configName, context);
            config.IndexField4 = ParseIndexField4(configItem, mod, configName, context);
            config.CustomDict = ParseCustomDict(configItem, mod, configName, context);
            config.CustomKeyDict = ParseCustomKeyDict(configItem, mod, configName, context);
            config.CustomList = ParseCustomList(configItem, mod, configName, context);
            config.CustomNestedList = ParseCustomNestedList(configItem, mod, configName, context);
            config.ItemType = ParseItemType(configItem, mod, configName, context);
            config.RequiredLevel = ParseRequiredLevel(configItem, mod, configName, context);
            config.Price = ParsePrice(configItem, mod, configName, context);
            config.Tags = ParseTags(configItem, mod, configName, context);
            config.Attributes = ParseAttributes(configItem, mod, configName, context);
            config.Effects = ParseEffects(configItem, mod, configName, context);
            config.IntValues = ParseIntValues(configItem, mod, configName, context);
            config.AttributeTypes = ParseAttributeTypes(configItem, mod, configName, context);
            config.Matrix = ParseMatrix(configItem, mod, configName, context);
            config.StringIntMap = ParseStringIntMap(configItem, mod, configName, context);
            config.AttributeValueMap = ParseAttributeValueMap(configItem, mod, configName, context);
            config.DeepNestedContainer = ParseDeepNestedContainer(configItem, mod, configName, context);
            config.UniqueIds = ParseUniqueIds(configItem, mod, configName, context);
            config.CustomData = ParseCustomData(configItem, mod, configName, context);
            config.GlobalValue = ParseGlobalValue(configItem, mod, configName, context);
            config.ComplexList = ParseComplexList(configItem, mod, configName, context);
            config.Category = ParseCategory(configItem, mod, configName, context);
            config.SubType = ParseSubType(configItem, mod, configName, context);
            config.Level = ParseLevel(configItem, mod, configName, context);
            config.ShortName = ParseShortName(configItem, mod, configName, context);
            config.LocalizedName = ParseLocalizedName(configItem, mod, configName, context);
            config.LabelName = ParseLabelName(configItem, mod, configName, context);
        }
        /// <summary>获取 Link Helper 类型</summary>
        public override Type GetLinkHelperType()
        {
            return null;
        }
        #region 字段解析方法 (ParseXXX)

        /// <summary>
        /// 解析 Version 字段
        /// </summary>
        private static int ParseVersion(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var xmlValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "Version");

            if (string.IsNullOrEmpty(xmlValue))
            {
                return default;
            }

            if (global::XM.Contracts.Config.ConfigParseHelper.TryParseInt(xmlValue, "Version", out var parsedValue))
            {
                return parsedValue;
            }

            return default;
        }

        /// <summary>
        /// 解析 IsEnabled 字段
        /// </summary>
        private static bool ParseIsEnabled(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var xmlValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "IsEnabled");

            // 默认值: true
            if (string.IsNullOrEmpty(xmlValue))
            {
                xmlValue = "true";
            }

            if (global::XM.Contracts.Config.ConfigParseHelper.TryParseBool(xmlValue, "IsEnabled", out var parsedValue))
            {
                return parsedValue;
            }

            return default;
        }

        /// <summary>
        /// 解析 Weight 字段
        /// </summary>
        private static float? ParseWeight(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var xmlValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "Weight");

            if (string.IsNullOrEmpty(xmlValue))
            {
                return null;
            }

            if (global::XM.Contracts.Config.ConfigParseHelper.TryParseFloat(xmlValue, "Weight", out var parsedValue))
            {
                return parsedValue;
            }

            return null;
        }

        /// <summary>
        /// 解析 IntSet 字段
        /// </summary>
        private static global::System.Collections.Generic.HashSet<int> ParseIntSet(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var set = new global::System.Collections.Generic.HashSet<int>();

            // 从 XML 节点解析
            var nodes = configItem.SelectNodes("IntSet");
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    var element = node as global::System.Xml.XmlElement;
                    if (element == null)
                    {
                        continue;
                    }

                    var text = element.InnerText?.Trim();
                    if (string.IsNullOrEmpty(text))
                    {
                        continue;
                    }

                    if (global::XM.Contracts.Config.ConfigParseHelper.TryParseInt(text, "IntSet", out var parsedItem))
                    {
                        set.Add(parsedItem);
                    }
                }
            }

            // CSV 格式备用
            if (set.Count == 0)
            {
                var csvValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "IntSet");
                if (!string.IsNullOrEmpty(csvValue))
                {
                    var parts = csvValue.Split(new[] { ',', ';', '|' }, global::System.StringSplitOptions.RemoveEmptyEntries);
                    foreach (var part in parts)
                    {
                        var trimmed = part.Trim();
                        if (global::XM.Contracts.Config.ConfigParseHelper.TryParseInt(trimmed, "IntSet", out var parsedItem))
                        {
                            set.Add(parsedItem);
                        }
                    }
                }
            }

            return set;
        }

        /// <summary>
        /// 解析 TypeSet 字段
        /// </summary>
        private static global::System.Collections.Generic.HashSet<global::XM.ConfigNew.Tests.Data.EItemType> ParseTypeSet(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var set = new global::System.Collections.Generic.HashSet<global::XM.ConfigNew.Tests.Data.EItemType>();

            // 从 XML 节点解析
            var nodes = configItem.SelectNodes("TypeSet");
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    var element = node as global::System.Xml.XmlElement;
                    if (element == null)
                    {
                        continue;
                    }

                    var text = element.InnerText?.Trim();
                    if (string.IsNullOrEmpty(text))
                    {
                        continue;
                    }

                    if (global::System.Enum.TryParse<global::XM.ConfigNew.Tests.Data.EItemType>(text, out var parsedItem))
                    {
                        set.Add(parsedItem);
                    }
                }
            }

            // CSV 格式备用
            if (set.Count == 0)
            {
                var csvValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "TypeSet");
                if (!string.IsNullOrEmpty(csvValue))
                {
                    var parts = csvValue.Split(new[] { ',', ';', '|' }, global::System.StringSplitOptions.RemoveEmptyEntries);
                    foreach (var part in parts)
                    {
                        var trimmed = part.Trim();
                        if (global::System.Enum.TryParse<global::XM.ConfigNew.Tests.Data.EItemType>(trimmed, out var parsedItem))
                        {
                            set.Add(parsedItem);
                        }
                    }
                }
            }

            return set;
        }

        /// <summary>
        /// 解析 NullableIntList 字段
        /// </summary>
        private static global::System.Collections.Generic.List<int?> ParseNullableIntList(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var list = new global::System.Collections.Generic.List<int?>();

            // 尝试从 XML 节点解析
            var nodes = configItem.SelectNodes("NullableIntList");
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    var element = node as global::System.Xml.XmlElement;
                    if (element == null)
                    {
                        continue;
                    }

                    var text = element.InnerText?.Trim();
                    if (string.IsNullOrEmpty(text))
                    {
                        continue;
                    }

                    // 类型 Int32? 不支持从文本解析
                    continue;
                }
            }

            // 如果没有节点，尝试 CSV 格式
            if (list.Count == 0)
            {
                var csvValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "NullableIntList");
                if (!string.IsNullOrEmpty(csvValue))
                {
                    var parts = csvValue.Split(new[] { ',', ';', '|' }, global::System.StringSplitOptions.RemoveEmptyEntries);
                    foreach (var part in parts)
                    {
                        var trimmed = part.Trim();
                        if (string.IsNullOrEmpty(trimmed))
                        {
                            continue;
                        }

                        // 类型 Int32? 不支持从文本解析
                        continue;
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// 解析 FourLevelNested 字段
        /// </summary>
        private static global::System.Collections.Generic.List<global::System.Collections.Generic.List<global::System.Collections.Generic.Dictionary<string, global::System.Collections.Generic.List<int>>>> ParseFourLevelNested(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var list = new global::System.Collections.Generic.List<global::System.Collections.Generic.List<global::System.Collections.Generic.Dictionary<string, global::System.Collections.Generic.List<int>>>>();

            // 尝试从 XML 节点解析
            var nodes = configItem.SelectNodes("FourLevelNested");
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    var element = node as global::System.Xml.XmlElement;
                    if (element == null)
                    {
                        continue;
                    }

                    var text = element.InnerText?.Trim();
                    if (string.IsNullOrEmpty(text))
                    {
                        continue;
                    }

                    // 嵌套容器不支持从文本解析: List<Dictionary<String, List<Int32>>>
                    continue;
                }
            }

            return list;
        }

        /// <summary>
        /// 解析 DictInDict 字段
        /// </summary>
        private static global::System.Collections.Generic.Dictionary<string, global::System.Collections.Generic.Dictionary<int, float>> ParseDictInDict(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var dict = new global::System.Collections.Generic.Dictionary<string, global::System.Collections.Generic.Dictionary<int, float>>();

            // 解析 Dictionary Item 节点
            var dictNodes = configItem.SelectNodes("DictInDict/Item");
            if (dictNodes != null)
            {
                foreach (var node in dictNodes)
                {
                    var element = node as global::System.Xml.XmlElement;
                    if (element == null)
                    {
                        continue;
                    }

                    var keyText = element.GetAttribute("Key");
                    var valueText = element.InnerText?.Trim();

                    var parsedKey = keyText;
                    if (!string.IsNullOrEmpty(parsedKey))
                    {
                        // 嵌套容器不支持从文本解析: Dictionary<Int32, Single>
                        continue;
                    }
                }
            }

            return dict;
        }

        /// <summary>
        /// 解析 ComplexNested 字段
        /// </summary>
        private static global::System.Collections.Generic.List<global::System.Collections.Generic.Dictionary<global::XM.ConfigNew.Tests.Data.EItemType, global::System.Collections.Generic.List<global::XM.ConfigNew.Tests.Data.AttributeConfig>>> ParseComplexNested(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var list = new global::System.Collections.Generic.List<global::System.Collections.Generic.Dictionary<global::XM.ConfigNew.Tests.Data.EItemType, global::System.Collections.Generic.List<global::XM.ConfigNew.Tests.Data.AttributeConfig>>>();

            // 尝试从 XML 节点解析
            var nodes = configItem.SelectNodes("ComplexNested");
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    var element = node as global::System.Xml.XmlElement;
                    if (element == null)
                    {
                        continue;
                    }

                    var text = element.InnerText?.Trim();
                    if (string.IsNullOrEmpty(text))
                    {
                        continue;
                    }

                    // 嵌套容器不支持从文本解析: Dictionary<EItemType, List<AttributeConfig>>
                    continue;
                }
            }

            return list;
        }

        /// <summary>
        /// 解析 UnlockSkill 字段
        /// </summary>
        private static global::XM.Contracts.Config.CfgS<global::XM.ConfigNew.Tests.Data.ActiveSkillConfig> ParseUnlockSkill(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            // 解析 CfgS 引用字符串
            var cfgSString = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "UnlockSkill");
            if (string.IsNullOrEmpty(cfgSString))
            {
                return default;
            }

            if (global::XM.Contracts.Config.ConfigParseHelper.TryParseCfgSString(cfgSString, "UnlockSkill", out var modName, out var cfgName))
            {
                return new global::XM.Contracts.Config.CfgS<global::XM.ConfigNew.Tests.Data.ActiveSkillConfig>(new global::XM.Contracts.Config.ModS(modName), cfgName);
            }

            global::XM.Contracts.Config.ConfigParseHelper.LogParseError(context, "UnlockSkill", $"无法解析 CfgS 字符串: {cfgSString}");
            return default;
        }

        /// <summary>
        /// 解析 PassiveSkill 字段
        /// </summary>
        private static global::XM.Contracts.Config.CfgS<global::XM.ConfigNew.Tests.Data.PassiveSkillConfig> ParsePassiveSkill(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            // 解析 CfgS 引用字符串
            var cfgSString = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "PassiveSkill");
            if (string.IsNullOrEmpty(cfgSString))
            {
                return default;
            }

            if (global::XM.Contracts.Config.ConfigParseHelper.TryParseCfgSString(cfgSString, "PassiveSkill", out var modName, out var cfgName))
            {
                return new global::XM.Contracts.Config.CfgS<global::XM.ConfigNew.Tests.Data.PassiveSkillConfig>(new global::XM.Contracts.Config.ModS(modName), cfgName);
            }

            global::XM.Contracts.Config.ConfigParseHelper.LogParseError(context, "PassiveSkill", $"无法解析 CfgS 字符串: {cfgSString}");
            return default;
        }

        /// <summary>
        /// 解析 IndexField1 字段
        /// </summary>
        private static global::XM.ConfigNew.Tests.Data.EItemType ParseIndexField1(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var xmlValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "IndexField1");

            if (string.IsNullOrEmpty(xmlValue))
            {
                return default;
            }

            if (global::System.Enum.TryParse<global::XM.ConfigNew.Tests.Data.EItemType>(xmlValue, out var parsedValue))
            {
                return parsedValue;
            }

            global::XM.Contracts.Config.ConfigParseHelper.LogParseError(context, "IndexField1", $"无法解析枚举值: {xmlValue}");
            return default;
        }

        /// <summary>
        /// 解析 IndexField2 字段
        /// </summary>
        private static global::XM.ConfigNew.Tests.Data.EItemQuality ParseIndexField2(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var xmlValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "IndexField2");

            if (string.IsNullOrEmpty(xmlValue))
            {
                return default;
            }

            if (global::System.Enum.TryParse<global::XM.ConfigNew.Tests.Data.EItemQuality>(xmlValue, out var parsedValue))
            {
                return parsedValue;
            }

            global::XM.Contracts.Config.ConfigParseHelper.LogParseError(context, "IndexField2", $"无法解析枚举值: {xmlValue}");
            return default;
        }

        /// <summary>
        /// 解析 IndexField3 字段
        /// </summary>
        private static int ParseIndexField3(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var xmlValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "IndexField3");

            if (string.IsNullOrEmpty(xmlValue))
            {
                return default;
            }

            if (global::XM.Contracts.Config.ConfigParseHelper.TryParseInt(xmlValue, "IndexField3", out var parsedValue))
            {
                return parsedValue;
            }

            return default;
        }

        /// <summary>
        /// 解析 IndexField4 字段
        /// </summary>
        private static string ParseIndexField4(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var xmlValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "IndexField4");

            if (string.IsNullOrEmpty(xmlValue))
            {
                return default;
            }

            // 字符串类型直接返回
            return xmlValue ?? string.Empty;
        }

        /// <summary>
        /// 解析 CustomDict 字段
        /// </summary>
        private static global::System.Collections.Generic.Dictionary<string, int> ParseCustomDict(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var dict = new global::System.Collections.Generic.Dictionary<string, int>();

            // 解析 Dictionary Item 节点
            var dictNodes = configItem.SelectNodes("CustomDict/Item");
            if (dictNodes != null)
            {
                foreach (var node in dictNodes)
                {
                    var element = node as global::System.Xml.XmlElement;
                    if (element == null)
                    {
                        continue;
                    }

                    var keyText = element.GetAttribute("Key");
                    var valueText = element.InnerText?.Trim();

                    var parsedKey = keyText;
                    if (!string.IsNullOrEmpty(parsedKey))
                    {
                        if (global::XM.Contracts.Config.ConfigParseHelper.TryParseInt(valueText, "CustomDict.Value", out var parsedValue))
                        {
                            dict[parsedKey] = parsedValue;
                        }
                    }
                }
            }

            return dict;
        }

        /// <summary>
        /// 解析 CustomKeyDict 字段
        /// </summary>
        private static global::System.Collections.Generic.Dictionary<string, int> ParseCustomKeyDict(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var dict = new global::System.Collections.Generic.Dictionary<string, int>();

            // 解析 Dictionary Item 节点
            var dictNodes = configItem.SelectNodes("CustomKeyDict/Item");
            if (dictNodes != null)
            {
                foreach (var node in dictNodes)
                {
                    var element = node as global::System.Xml.XmlElement;
                    if (element == null)
                    {
                        continue;
                    }

                    var keyText = element.GetAttribute("Key");
                    var valueText = element.InnerText?.Trim();

                    var parsedKey = keyText;
                    if (!string.IsNullOrEmpty(parsedKey))
                    {
                        if (global::XM.Contracts.Config.ConfigParseHelper.TryParseInt(valueText, "CustomKeyDict.Value", out var parsedValue))
                        {
                            dict[parsedKey] = parsedValue;
                        }
                    }
                }
            }

            return dict;
        }

        /// <summary>
        /// 解析 CustomList 字段
        /// </summary>
        private static global::System.Collections.Generic.List<string> ParseCustomList(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var list = new global::System.Collections.Generic.List<string>();

            // 尝试从 XML 节点解析
            var nodes = configItem.SelectNodes("CustomList");
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    var element = node as global::System.Xml.XmlElement;
                    if (element == null)
                    {
                        continue;
                    }

                    var text = element.InnerText?.Trim();
                    if (string.IsNullOrEmpty(text))
                    {
                        continue;
                    }

                    var parsedItem = text;
                    if (!string.IsNullOrEmpty(parsedItem))
                    {
                        list.Add(parsedItem);
                    }
                }
            }

            // 如果没有节点，尝试 CSV 格式
            if (list.Count == 0)
            {
                var csvValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "CustomList");
                if (!string.IsNullOrEmpty(csvValue))
                {
                    var parts = csvValue.Split(new[] { ',', ';', '|' }, global::System.StringSplitOptions.RemoveEmptyEntries);
                    foreach (var part in parts)
                    {
                        var trimmed = part.Trim();
                        if (string.IsNullOrEmpty(trimmed))
                        {
                            continue;
                        }

                        var parsedItem = trimmed;
                        if (!string.IsNullOrEmpty(parsedItem))
                        {
                            list.Add(parsedItem);
                        }
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// 解析 CustomNestedList 字段
        /// </summary>
        private static global::System.Collections.Generic.List<global::System.Collections.Generic.List<string>> ParseCustomNestedList(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var list = new global::System.Collections.Generic.List<global::System.Collections.Generic.List<string>>();

            // 尝试从 XML 节点解析
            var nodes = configItem.SelectNodes("CustomNestedList");
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    var element = node as global::System.Xml.XmlElement;
                    if (element == null)
                    {
                        continue;
                    }

                    var text = element.InnerText?.Trim();
                    if (string.IsNullOrEmpty(text))
                    {
                        continue;
                    }

                    // 嵌套容器不支持从文本解析: List<String>
                    continue;
                }
            }

            return list;
        }

        /// <summary>
        /// 解析 ItemType 字段
        /// </summary>
        private static global::XM.ConfigNew.Tests.Data.EItemType ParseItemType(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var xmlValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "ItemType");

            if (string.IsNullOrEmpty(xmlValue))
            {
                return default;
            }

            if (global::System.Enum.TryParse<global::XM.ConfigNew.Tests.Data.EItemType>(xmlValue, out var parsedValue))
            {
                return parsedValue;
            }

            global::XM.Contracts.Config.ConfigParseHelper.LogParseError(context, "ItemType", $"无法解析枚举值: {xmlValue}");
            return default;
        }

        /// <summary>
        /// 解析 RequiredLevel 字段
        /// </summary>
        private static int? ParseRequiredLevel(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var xmlValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "RequiredLevel");

            if (string.IsNullOrEmpty(xmlValue))
            {
                return null;
            }

            if (global::XM.Contracts.Config.ConfigParseHelper.TryParseInt(xmlValue, "RequiredLevel", out var parsedValue))
            {
                return parsedValue;
            }

            return null;
        }

        /// <summary>
        /// 解析 Price 字段
        /// </summary>
        private static global::XM.ConfigNew.Tests.Data.AttributeConfig ParsePrice(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            // 解析嵌套配置
            var element = configItem.SelectSingleNode("Price") as global::System.Xml.XmlElement;
            if (element != null == false)
            {
                return null;
            }

            var helper = global::XM.Contracts.IConfigDataCenter.I?.GetClassHelper(typeof(global::XM.ConfigNew.Tests.Data.AttributeConfig));
            if (helper != null == false)
            {
                global::XM.Contracts.Config.ConfigParseHelper.LogParseError(context, "Price", "无法获取嵌套配置 Helper");
                return null;
            }

            var nestedConfigName = configName + "_Price";
            return (global::XM.ConfigNew.Tests.Data.AttributeConfig)helper.DeserializeConfigFromXml(element, mod, nestedConfigName, context);
        }

        /// <summary>
        /// 解析 Tags 字段
        /// </summary>
        private static global::System.Collections.Generic.List<string> ParseTags(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var list = new global::System.Collections.Generic.List<string>();

            // 尝试从 XML 节点解析
            var nodes = configItem.SelectNodes("Tags");
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    var element = node as global::System.Xml.XmlElement;
                    if (element == null)
                    {
                        continue;
                    }

                    var text = element.InnerText?.Trim();
                    if (string.IsNullOrEmpty(text))
                    {
                        continue;
                    }

                    var parsedItem = text;
                    if (!string.IsNullOrEmpty(parsedItem))
                    {
                        list.Add(parsedItem);
                    }
                }
            }

            // 如果没有节点，尝试 CSV 格式
            if (list.Count == 0)
            {
                var csvValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "Tags");
                if (!string.IsNullOrEmpty(csvValue))
                {
                    var parts = csvValue.Split(new[] { ',', ';', '|' }, global::System.StringSplitOptions.RemoveEmptyEntries);
                    foreach (var part in parts)
                    {
                        var trimmed = part.Trim();
                        if (string.IsNullOrEmpty(trimmed))
                        {
                            continue;
                        }

                        var parsedItem = trimmed;
                        if (!string.IsNullOrEmpty(parsedItem))
                        {
                            list.Add(parsedItem);
                        }
                    }
                }
            }

            // 如果仍为空，使用默认值: common,item
            if (list.Count == 0)
            {
                var defaultValue = "common,item";
                var parts = defaultValue.Split(new[] { ',', ';', '|' }, global::System.StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in parts)
                {
                    var trimmed = part.Trim();
                    if (string.IsNullOrEmpty(trimmed))
                    {
                        continue;
                    }

                    var parsedItem = trimmed;
                    if (!string.IsNullOrEmpty(parsedItem))
                    {
                        list.Add(parsedItem);
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// 解析 Attributes 字段
        /// </summary>
        private static global::System.Collections.Generic.List<global::XM.ConfigNew.Tests.Data.AttributeConfig> ParseAttributes(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var list = new global::System.Collections.Generic.List<global::XM.ConfigNew.Tests.Data.AttributeConfig>();

            // 尝试从 XML 节点解析
            var nodes = configItem.SelectNodes("Attributes");
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    var element = node as global::System.Xml.XmlElement;
                    if (element == null)
                    {
                        continue;
                    }

                    var text = element.InnerText?.Trim();
                    if (string.IsNullOrEmpty(text))
                    {
                        continue;
                    }

                    // 类型 AttributeConfig 不支持从文本解析
                    continue;
                }
            }

            // 如果没有节点，尝试 CSV 格式
            if (list.Count == 0)
            {
                var csvValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "Attributes");
                if (!string.IsNullOrEmpty(csvValue))
                {
                    var parts = csvValue.Split(new[] { ',', ';', '|' }, global::System.StringSplitOptions.RemoveEmptyEntries);
                    foreach (var part in parts)
                    {
                        var trimmed = part.Trim();
                        if (string.IsNullOrEmpty(trimmed))
                        {
                            continue;
                        }

                        // 类型 AttributeConfig 不支持从文本解析
                        continue;
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// 解析 Effects 字段
        /// </summary>
        private static global::System.Collections.Generic.List<global::XM.ConfigNew.Tests.Data.EffectConfig> ParseEffects(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var list = new global::System.Collections.Generic.List<global::XM.ConfigNew.Tests.Data.EffectConfig>();

            // 尝试从 XML 节点解析
            var nodes = configItem.SelectNodes("Effects");
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    var element = node as global::System.Xml.XmlElement;
                    if (element == null)
                    {
                        continue;
                    }

                    var text = element.InnerText?.Trim();
                    if (string.IsNullOrEmpty(text))
                    {
                        continue;
                    }

                    // 类型 EffectConfig 不支持从文本解析
                    continue;
                }
            }

            // 如果没有节点，尝试 CSV 格式
            if (list.Count == 0)
            {
                var csvValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "Effects");
                if (!string.IsNullOrEmpty(csvValue))
                {
                    var parts = csvValue.Split(new[] { ',', ';', '|' }, global::System.StringSplitOptions.RemoveEmptyEntries);
                    foreach (var part in parts)
                    {
                        var trimmed = part.Trim();
                        if (string.IsNullOrEmpty(trimmed))
                        {
                            continue;
                        }

                        // 类型 EffectConfig 不支持从文本解析
                        continue;
                    }
                }
            }

            // 如果仍为空，使用默认值: Buff1,Buff2
            if (list.Count == 0)
            {
                var defaultValue = "Buff1,Buff2";
                var parts = defaultValue.Split(new[] { ',', ';', '|' }, global::System.StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in parts)
                {
                    var trimmed = part.Trim();
                    if (string.IsNullOrEmpty(trimmed))
                    {
                        continue;
                    }

                    // 类型 EffectConfig 不支持从文本解析
                    continue;
                }
            }

            return list;
        }

        /// <summary>
        /// 解析 IntValues 字段
        /// </summary>
        private static global::System.Collections.Generic.List<int> ParseIntValues(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var list = new global::System.Collections.Generic.List<int>();

            // 尝试从 XML 节点解析
            var nodes = configItem.SelectNodes("IntValues");
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    var element = node as global::System.Xml.XmlElement;
                    if (element == null)
                    {
                        continue;
                    }

                    var text = element.InnerText?.Trim();
                    if (string.IsNullOrEmpty(text))
                    {
                        continue;
                    }

                    if (global::XM.Contracts.Config.ConfigParseHelper.TryParseInt(text, "IntValues", out var parsedItem))
                    {
                        list.Add(parsedItem);
                    }
                }
            }

            // 如果没有节点，尝试 CSV 格式
            if (list.Count == 0)
            {
                var csvValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "IntValues");
                if (!string.IsNullOrEmpty(csvValue))
                {
                    var parts = csvValue.Split(new[] { ',', ';', '|' }, global::System.StringSplitOptions.RemoveEmptyEntries);
                    foreach (var part in parts)
                    {
                        var trimmed = part.Trim();
                        if (string.IsNullOrEmpty(trimmed))
                        {
                            continue;
                        }

                        if (global::XM.Contracts.Config.ConfigParseHelper.TryParseInt(trimmed, "IntValues", out var parsedItem))
                        {
                            list.Add(parsedItem);
                        }
                    }
                }
            }

            // 如果仍为空，使用默认值: 1,2,3,4,5
            if (list.Count == 0)
            {
                var defaultValue = "1,2,3,4,5";
                var parts = defaultValue.Split(new[] { ',', ';', '|' }, global::System.StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in parts)
                {
                    var trimmed = part.Trim();
                    if (string.IsNullOrEmpty(trimmed))
                    {
                        continue;
                    }

                    if (global::XM.Contracts.Config.ConfigParseHelper.TryParseInt(trimmed, "IntValues", out var parsedItem))
                    {
                        list.Add(parsedItem);
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// 解析 AttributeTypes 字段
        /// </summary>
        private static global::System.Collections.Generic.List<global::XM.ConfigNew.Tests.Data.EAttributeType> ParseAttributeTypes(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var list = new global::System.Collections.Generic.List<global::XM.ConfigNew.Tests.Data.EAttributeType>();

            // 尝试从 XML 节点解析
            var nodes = configItem.SelectNodes("AttributeTypes");
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    var element = node as global::System.Xml.XmlElement;
                    if (element == null)
                    {
                        continue;
                    }

                    var text = element.InnerText?.Trim();
                    if (string.IsNullOrEmpty(text))
                    {
                        continue;
                    }

                    if (global::System.Enum.TryParse<global::XM.ConfigNew.Tests.Data.EAttributeType>(text, out var parsedItem))
                    {
                        list.Add(parsedItem);
                    }
                }
            }

            // 如果没有节点，尝试 CSV 格式
            if (list.Count == 0)
            {
                var csvValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "AttributeTypes");
                if (!string.IsNullOrEmpty(csvValue))
                {
                    var parts = csvValue.Split(new[] { ',', ';', '|' }, global::System.StringSplitOptions.RemoveEmptyEntries);
                    foreach (var part in parts)
                    {
                        var trimmed = part.Trim();
                        if (string.IsNullOrEmpty(trimmed))
                        {
                            continue;
                        }

                        if (global::System.Enum.TryParse<global::XM.ConfigNew.Tests.Data.EAttributeType>(trimmed, out var parsedItem))
                        {
                            list.Add(parsedItem);
                        }
                    }
                }
            }

            // 如果仍为空，使用默认值: Health,Attack,Defense
            if (list.Count == 0)
            {
                var defaultValue = "Health,Attack,Defense";
                var parts = defaultValue.Split(new[] { ',', ';', '|' }, global::System.StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in parts)
                {
                    var trimmed = part.Trim();
                    if (string.IsNullOrEmpty(trimmed))
                    {
                        continue;
                    }

                    if (global::System.Enum.TryParse<global::XM.ConfigNew.Tests.Data.EAttributeType>(trimmed, out var parsedItem))
                    {
                        list.Add(parsedItem);
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// 解析 Matrix 字段
        /// </summary>
        private static global::System.Collections.Generic.List<global::System.Collections.Generic.List<int>> ParseMatrix(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var list = new global::System.Collections.Generic.List<global::System.Collections.Generic.List<int>>();

            // 尝试从 XML 节点解析
            var nodes = configItem.SelectNodes("Matrix");
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    var element = node as global::System.Xml.XmlElement;
                    if (element == null)
                    {
                        continue;
                    }

                    var text = element.InnerText?.Trim();
                    if (string.IsNullOrEmpty(text))
                    {
                        continue;
                    }

                    // 嵌套容器不支持从文本解析: List<Int32>
                    continue;
                }
            }

            return list;
        }

        /// <summary>
        /// 解析 StringIntMap 字段
        /// </summary>
        private static global::System.Collections.Generic.Dictionary<string, int> ParseStringIntMap(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var dict = new global::System.Collections.Generic.Dictionary<string, int>();

            // 解析 Dictionary Item 节点
            var dictNodes = configItem.SelectNodes("StringIntMap/Item");
            if (dictNodes != null)
            {
                foreach (var node in dictNodes)
                {
                    var element = node as global::System.Xml.XmlElement;
                    if (element == null)
                    {
                        continue;
                    }

                    var keyText = element.GetAttribute("Key");
                    var valueText = element.InnerText?.Trim();

                    var parsedKey = keyText;
                    if (!string.IsNullOrEmpty(parsedKey))
                    {
                        if (global::XM.Contracts.Config.ConfigParseHelper.TryParseInt(valueText, "StringIntMap.Value", out var parsedValue))
                        {
                            dict[parsedKey] = parsedValue;
                        }
                    }
                }
            }

            return dict;
        }

        /// <summary>
        /// 解析 AttributeValueMap 字段
        /// </summary>
        private static global::System.Collections.Generic.Dictionary<global::XM.ConfigNew.Tests.Data.EAttributeType, global::System.Collections.Generic.List<int>> ParseAttributeValueMap(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var dict = new global::System.Collections.Generic.Dictionary<global::XM.ConfigNew.Tests.Data.EAttributeType, global::System.Collections.Generic.List<int>>();

            // 解析 Dictionary Item 节点
            var dictNodes = configItem.SelectNodes("AttributeValueMap/Item");
            if (dictNodes != null)
            {
                foreach (var node in dictNodes)
                {
                    var element = node as global::System.Xml.XmlElement;
                    if (element == null)
                    {
                        continue;
                    }

                    var keyText = element.GetAttribute("Key");
                    var valueText = element.InnerText?.Trim();

                    if (global::System.Enum.TryParse<global::XM.ConfigNew.Tests.Data.EAttributeType>(keyText, out var parsedKey))
                    {
                        // 嵌套容器不支持从文本解析: List<Int32>
                        continue;
                    }
                }
            }

            return dict;
        }

        /// <summary>
        /// 解析 DeepNestedContainer 字段
        /// </summary>
        private static global::System.Collections.Generic.List<global::System.Collections.Generic.Dictionary<string, global::System.Collections.Generic.List<float>>> ParseDeepNestedContainer(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var list = new global::System.Collections.Generic.List<global::System.Collections.Generic.Dictionary<string, global::System.Collections.Generic.List<float>>>();

            // 尝试从 XML 节点解析
            var nodes = configItem.SelectNodes("DeepNestedContainer");
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    var element = node as global::System.Xml.XmlElement;
                    if (element == null)
                    {
                        continue;
                    }

                    var text = element.InnerText?.Trim();
                    if (string.IsNullOrEmpty(text))
                    {
                        continue;
                    }

                    // 嵌套容器不支持从文本解析: Dictionary<String, List<Single>>
                    continue;
                }
            }

            return list;
        }

        /// <summary>
        /// 解析 UniqueIds 字段
        /// </summary>
        private static global::System.Collections.Generic.HashSet<int> ParseUniqueIds(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var set = new global::System.Collections.Generic.HashSet<int>();

            // 从 XML 节点解析
            var nodes = configItem.SelectNodes("UniqueIds");
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    var element = node as global::System.Xml.XmlElement;
                    if (element == null)
                    {
                        continue;
                    }

                    var text = element.InnerText?.Trim();
                    if (string.IsNullOrEmpty(text))
                    {
                        continue;
                    }

                    if (global::XM.Contracts.Config.ConfigParseHelper.TryParseInt(text, "UniqueIds", out var parsedItem))
                    {
                        set.Add(parsedItem);
                    }
                }
            }

            // CSV 格式备用
            if (set.Count == 0)
            {
                var csvValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "UniqueIds");
                if (!string.IsNullOrEmpty(csvValue))
                {
                    var parts = csvValue.Split(new[] { ',', ';', '|' }, global::System.StringSplitOptions.RemoveEmptyEntries);
                    foreach (var part in parts)
                    {
                        var trimmed = part.Trim();
                        if (global::XM.Contracts.Config.ConfigParseHelper.TryParseInt(trimmed, "UniqueIds", out var parsedItem))
                        {
                            set.Add(parsedItem);
                        }
                    }
                }
            }

            return set;
        }

        /// <summary>
        /// 解析 CustomData 字段
        /// </summary>
        private static string ParseCustomData(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var xmlValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "CustomData");

            if (string.IsNullOrEmpty(xmlValue))
            {
                return default;
            }

            // 字符串类型直接返回
            return xmlValue ?? string.Empty;
        }

        /// <summary>
        /// 解析 GlobalValue 字段
        /// </summary>
        private static float ParseGlobalValue(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var xmlValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "GlobalValue");

            if (string.IsNullOrEmpty(xmlValue))
            {
                return default;
            }

            if (global::XM.Contracts.Config.ConfigParseHelper.TryParseFloat(xmlValue, "GlobalValue", out var parsedValue))
            {
                return parsedValue;
            }

            return default;
        }

        /// <summary>
        /// 解析 ComplexList 字段
        /// </summary>
        private static global::System.Collections.Generic.List<string> ParseComplexList(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var list = new global::System.Collections.Generic.List<string>();

            // 尝试从 XML 节点解析
            var nodes = configItem.SelectNodes("ComplexList");
            if (nodes != null)
            {
                foreach (var node in nodes)
                {
                    var element = node as global::System.Xml.XmlElement;
                    if (element == null)
                    {
                        continue;
                    }

                    var text = element.InnerText?.Trim();
                    if (string.IsNullOrEmpty(text))
                    {
                        continue;
                    }

                    var parsedItem = text;
                    if (!string.IsNullOrEmpty(parsedItem))
                    {
                        list.Add(parsedItem);
                    }
                }
            }

            // 如果没有节点，尝试 CSV 格式
            if (list.Count == 0)
            {
                var csvValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "ComplexList");
                if (!string.IsNullOrEmpty(csvValue))
                {
                    var parts = csvValue.Split(new[] { ',', ';', '|' }, global::System.StringSplitOptions.RemoveEmptyEntries);
                    foreach (var part in parts)
                    {
                        var trimmed = part.Trim();
                        if (string.IsNullOrEmpty(trimmed))
                        {
                            continue;
                        }

                        var parsedItem = trimmed;
                        if (!string.IsNullOrEmpty(parsedItem))
                        {
                            list.Add(parsedItem);
                        }
                    }
                }
            }

            return list;
        }

        /// <summary>
        /// 解析 Category 字段
        /// </summary>
        private static string ParseCategory(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var xmlValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "Category");

            if (string.IsNullOrEmpty(xmlValue))
            {
                return default;
            }

            // 字符串类型直接返回
            return xmlValue ?? string.Empty;
        }

        /// <summary>
        /// 解析 SubType 字段
        /// </summary>
        private static int ParseSubType(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var xmlValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "SubType");

            if (string.IsNullOrEmpty(xmlValue))
            {
                return default;
            }

            if (global::XM.Contracts.Config.ConfigParseHelper.TryParseInt(xmlValue, "SubType", out var parsedValue))
            {
                return parsedValue;
            }

            return default;
        }

        /// <summary>
        /// 解析 Level 字段
        /// </summary>
        private static int ParseLevel(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var xmlValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "Level");

            if (string.IsNullOrEmpty(xmlValue))
            {
                return default;
            }

            if (global::XM.Contracts.Config.ConfigParseHelper.TryParseInt(xmlValue, "Level", out var parsedValue))
            {
                return parsedValue;
            }

            return default;
        }

        /// <summary>
        /// 解析 ShortName 字段
        /// </summary>
        private static string ParseShortName(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var xmlValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "ShortName");

            if (string.IsNullOrEmpty(xmlValue))
            {
                return default;
            }

            // 字符串类型直接返回
            return xmlValue ?? string.Empty;
        }

        /// <summary>
        /// 解析 LocalizedName 字段
        /// </summary>
        private static string ParseLocalizedName(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var xmlValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "LocalizedName");

            if (string.IsNullOrEmpty(xmlValue))
            {
                return default;
            }

            // 字符串类型直接返回
            return xmlValue ?? string.Empty;
        }

        /// <summary>
        /// 解析 LabelName 字段
        /// </summary>
        private static string ParseLabelName(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var xmlValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "LabelName");

            if (string.IsNullOrEmpty(xmlValue))
            {
                return default;
            }

            // 字符串类型直接返回
            return xmlValue ?? string.Empty;
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
            ref global::XM.ConfigNew.Tests.Data.UltimateComplexConfigUnmanaged data,
            XM.ConfigDataCenter.ConfigDataHolder configHolderData,
            XBlobPtr? linkParent = null)
        {
            var config = (global::XM.ConfigNew.Tests.Data.UltimateComplexConfig)value;

            // 分配容器和嵌套配置
            AllocIntSet(config, ref data, cfgi, configHolderData);
            AllocTypeSet(config, ref data, cfgi, configHolderData);
            AllocNullableIntList(config, ref data, cfgi, configHolderData);
            AllocFourLevelNested(config, ref data, cfgi, configHolderData);
            AllocDictInDict(config, ref data, cfgi, configHolderData);
            AllocComplexNested(config, ref data, cfgi, configHolderData);
            AllocCustomDict(config, ref data, cfgi, configHolderData);
            AllocCustomKeyDict(config, ref data, cfgi, configHolderData);
            AllocCustomList(config, ref data, cfgi, configHolderData);
            AllocCustomNestedList(config, ref data, cfgi, configHolderData);
            FillPrice(config, ref data, cfgi, configHolderData);
            AllocTags(config, ref data, cfgi, configHolderData);
            AllocAttributes(config, ref data, cfgi, configHolderData);
            AllocEffects(config, ref data, cfgi, configHolderData);
            AllocIntValues(config, ref data, cfgi, configHolderData);
            AllocAttributeTypes(config, ref data, cfgi, configHolderData);
            AllocMatrix(config, ref data, cfgi, configHolderData);
            AllocStringIntMap(config, ref data, cfgi, configHolderData);
            AllocAttributeValueMap(config, ref data, cfgi, configHolderData);
            AllocDeepNestedContainer(config, ref data, cfgi, configHolderData);
            AllocUniqueIds(config, ref data, cfgi, configHolderData);
            AllocComplexList(config, ref data, cfgi, configHolderData);

            // 填充基本类型字段
            data.Version = config.Version;
            data.IsEnabled = config.IsEnabled;
            data.Weight = config.Weight.GetValueOrDefault();
            // TODO: UnlockSkill - CfgS 转 CfgI (链接阶段解析)
            if (TryGetCfgI(config.UnlockSkill, out var UnlockSkillCfgI))
            {
                data.UnlockSkill = UnlockSkillCfgI.As<global::XM.ConfigNew.Tests.Data.ActiveSkillConfigUnmanaged>();
            }
            // TODO: PassiveSkill - CfgS 转 CfgI (链接阶段解析)
            if (TryGetCfgI(config.PassiveSkill, out var PassiveSkillCfgI))
            {
                data.PassiveSkill = PassiveSkillCfgI.As<global::XM.ConfigNew.Tests.Data.PassiveSkillConfigUnmanaged>();
            }
            data.IndexField1 = config.IndexField1;
            data.IndexField2 = config.IndexField2;
            data.IndexField3 = config.IndexField3;
            data.IndexField4 = new global::Unity.Collections.FixedString32Bytes(config.IndexField4 ?? string.Empty);
            data.ItemType = config.ItemType;
            data.RequiredLevel = config.RequiredLevel.GetValueOrDefault();
            data.CustomData = new global::Unity.Collections.FixedString32Bytes(config.CustomData ?? string.Empty);
            data.GlobalValue = config.GlobalValue;
            data.Category = new global::Unity.Collections.FixedString32Bytes(config.Category ?? string.Empty);
            data.SubType = config.SubType;
            data.Level = config.Level;
            data.ShortName = new global::Unity.Collections.FixedString32Bytes(config.ShortName ?? string.Empty);
            if (TryGetStrI(config.LocalizedName, out var LocalizedNameStrI))
            {
                data.LocalizedName = LocalizedNameStrI;
            }
            if (TryGetLabelI(config.LabelName, out var LabelNameLabelI))
            {
                data.LabelName = LabelNameLabelI;
            }
        }
        /// <summary>
        /// 建立 Link 双向引用（链接阶段调用）
        /// </summary>
        /// <param name="config">托管配置对象</param>
        /// <param name="data">非托管数据结构（ref 传递）</param>
        /// <param name="configHolderData">配置数据持有者</param>
        public virtual void EstablishLinks(
            global::XM.ConfigNew.Tests.Data.UltimateComplexConfig config,
            ref global::XM.ConfigNew.Tests.Data.UltimateComplexConfigUnmanaged data,
            XM.ConfigDataCenter.ConfigDataHolder configHolderData)
        {
            // TODO: 实现 Link 双向引用
            // 父→子: 通过 CfgI 查找子配置，填充 XBlobPtr
            // 子→父: 通过 CfgI 查找父配置，填充引用
        }
        #region 容器分配和嵌套配置填充方法

        /// <summary>
        /// 分配 IntSet 容器
        /// </summary>
        private void AllocIntSet(global::XM.ConfigNew.Tests.Data.UltimateComplexConfig config, ref global::XM.ConfigNew.Tests.Data.UltimateComplexConfigUnmanaged data, CfgI cfgi, XM.ConfigDataCenter.ConfigDataHolder configHolderData)
        {
            if (config.IntSet == null || config.IntSet.Count == 0)
            {
                return;
            }

            var set = configHolderData.Data.BlobContainer.AllocSet<int>(config.IntSet.Count);
            foreach (var item in config.IntSet)
            {
                set.Add(configHolderData.Data.BlobContainer, item);
            }

            data.IntSet = set;
        }
        /// <summary>
        /// 分配 TypeSet 容器
        /// </summary>
        private void AllocTypeSet(global::XM.ConfigNew.Tests.Data.UltimateComplexConfig config, ref global::XM.ConfigNew.Tests.Data.UltimateComplexConfigUnmanaged data, CfgI cfgi, XM.ConfigDataCenter.ConfigDataHolder configHolderData)
        {
            if (config.TypeSet == null || config.TypeSet.Count == 0)
            {
                return;
            }

            var set = configHolderData.Data.BlobContainer.AllocSet<global::XM.ConfigNew.CodeGen.EnumWrapper<global::XM.ConfigNew.Tests.Data.EItemType>>(config.TypeSet.Count);
            foreach (var item in config.TypeSet)
            {
                set.Add(configHolderData.Data.BlobContainer, new global::XM.ConfigNew.CodeGen.EnumWrapper<global::XM.ConfigNew.Tests.Data.EItemType>(item));
            }

            data.TypeSet = set;
        }
        /// <summary>
        /// 分配 NullableIntList 容器
        /// </summary>
        private void AllocNullableIntList(global::XM.ConfigNew.Tests.Data.UltimateComplexConfig config, ref global::XM.ConfigNew.Tests.Data.UltimateComplexConfigUnmanaged data, CfgI cfgi, XM.ConfigDataCenter.ConfigDataHolder configHolderData)
        {
            if (config.NullableIntList == null || config.NullableIntList.Count == 0)
            {
                return;
            }

            var array = configHolderData.Data.BlobContainer.AllocArray<int>(config.NullableIntList.Count);
            for (int i = 0; i < config.NullableIntList.Count; i++)
            {
                array[configHolderData.Data.BlobContainer, i] = config.NullableIntList[i].GetValueOrDefault();
            }

            data.NullableIntList = array;
        }
        /// <summary>
        /// 分配 FourLevelNested 容器
        /// </summary>
        private void AllocFourLevelNested(global::XM.ConfigNew.Tests.Data.UltimateComplexConfig config, ref global::XM.ConfigNew.Tests.Data.UltimateComplexConfigUnmanaged data, CfgI cfgi, XM.ConfigDataCenter.ConfigDataHolder configHolderData)
        {
            if (config.FourLevelNested == null || config.FourLevelNested.Count == 0)
            {
                return;
            }

            var outerArray = configHolderData.Data.BlobContainer.AllocArray<global::XBlobArray<global::XBlobMap<StrI, global::XBlobArray<int>>>>(config.FourLevelNested.Count);
            for (int i = 0; i < config.FourLevelNested.Count; i++)
            {
                var innerList = config.FourLevelNested[i];
                if (innerList == null || innerList.Count == 0)
                {
                    continue;
                }

                var arr = configHolderData.Data.BlobContainer.AllocArray<global::XBlobMap<StrI, global::XBlobArray<int>>>(innerList.Count);
                for (int j = 0; j < innerList.Count; j++)
                {
                    var inner0 = innerList[j];
                    if (inner0 != null && inner0.Count > 0)
                    {
                        var temp_0 = default(global::XBlobMap<StrI, global::XBlobArray<int>>);
                        var innerArr_1 = configHolderData.Data.BlobContainer.AllocMap<StrI, global::XBlobArray<int>>(inner0.Count);
                        foreach (var kvp_1 in inner0)
                        {
                            if (TryGetStrI(kvp_1.Key, out var keyStr_1))
                            {
                                var innerVal1 = kvp_1.Value;
                                if (innerVal1 != null && innerVal1.Count > 0)
                                {
                                    var tempVal_1 = default(global::XBlobArray<int>);
                                    var innerArr_2 = configHolderData.Data.BlobContainer.AllocArray<int>(innerVal1.Count);
                                    for (int n2 = 0; n2 < innerVal1.Count; n2++)
                                    {
                                        innerArr_2[configHolderData.Data.BlobContainer, n2] = innerVal1[n2];
                                    }
                                    tempVal_1 = innerArr_2;
                                    innerArr_1[configHolderData.Data.BlobContainer, keyStr_1] = tempVal_1;
                                }
                            }
                        }
                        temp_0 = innerArr_1;
                        arr[configHolderData.Data.BlobContainer, j] = temp_0;
                    }
                }
                outerArray[configHolderData.Data.BlobContainer, i] = arr;
            }

            data.FourLevelNested = outerArray;
        }
        /// <summary>
        /// 分配 DictInDict 容器
        /// </summary>
        private void AllocDictInDict(global::XM.ConfigNew.Tests.Data.UltimateComplexConfig config, ref global::XM.ConfigNew.Tests.Data.UltimateComplexConfigUnmanaged data, CfgI cfgi, XM.ConfigDataCenter.ConfigDataHolder configHolderData)
        {
            if (config.DictInDict == null || config.DictInDict.Count == 0)
            {
                return;
            }

            var map = configHolderData.Data.BlobContainer.AllocMap<StrI, global::XBlobMap<int, float>>(config.DictInDict.Count);
            foreach (var kvp in config.DictInDict)
            {
                if (TryGetStrI(kvp.Key, out var keyStrI))
                {
                    var innerVal0 = kvp.Value;
                    if (innerVal0 != null && innerVal0.Count > 0)
                    {
                        var tempVal = default(global::XBlobMap<int, float>);
                        var innerArr_1 = configHolderData.Data.BlobContainer.AllocMap<int, float>(innerVal0.Count);
                        foreach (var kvp_1 in innerVal0)
                        {
                            var key_1 = kvp_1.Key;
                            innerArr_1[configHolderData.Data.BlobContainer, key_1] = kvp_1.Value;
                        }
                        tempVal = innerArr_1;
                        map[configHolderData.Data.BlobContainer, keyStrI] = tempVal;
                    }
                }
            }

            data.DictInDict = map;
        }
        /// <summary>
        /// 分配 ComplexNested 容器
        /// </summary>
        private void AllocComplexNested(global::XM.ConfigNew.Tests.Data.UltimateComplexConfig config, ref global::XM.ConfigNew.Tests.Data.UltimateComplexConfigUnmanaged data, CfgI cfgi, XM.ConfigDataCenter.ConfigDataHolder configHolderData)
        {
            if (config.ComplexNested == null || config.ComplexNested.Count == 0)
            {
                return;
            }

            var outerArray = configHolderData.Data.BlobContainer.AllocArray<global::XBlobMap<global::XM.ConfigNew.CodeGen.EnumWrapper<global::XM.ConfigNew.Tests.Data.EItemType>, global::XBlobArray<global::XM.ConfigNew.Tests.Data.AttributeConfigUnmanaged>>>(config.ComplexNested.Count);
            for (int i = 0; i < config.ComplexNested.Count; i++)
            {
                var innerList = config.ComplexNested[i];
                if (innerList == null || innerList.Count == 0)
                {
                    continue;
                }

                var arr = configHolderData.Data.BlobContainer.AllocMap<global::XM.ConfigNew.CodeGen.EnumWrapper<global::XM.ConfigNew.Tests.Data.EItemType>, global::XBlobArray<global::XM.ConfigNew.Tests.Data.AttributeConfigUnmanaged>>(innerList.Count);
                foreach (var kvp in innerList)
                {
                    var key = new global::XM.ConfigNew.CodeGen.EnumWrapper<global::XM.ConfigNew.Tests.Data.EItemType>(kvp.Key);
                    var innerVal0 = kvp.Value;
                    if (innerVal0 != null && innerVal0.Count > 0)
                    {
                        var tempVal = default(global::XBlobArray<global::XM.ConfigNew.Tests.Data.AttributeConfigUnmanaged>);
                        var innerArr_1 = configHolderData.Data.BlobContainer.AllocArray<global::XM.ConfigNew.Tests.Data.AttributeConfigUnmanaged>(innerVal0.Count);
                        for (int n1 = 0; n1 < innerVal0.Count; n1++)
                        {
                            if (innerVal0[n1] != null)
                            {
                                var leafHelper_n1 = global::XM.ConfigNew.Tests.Data.AttributeConfigClassHelper.Instance;
                                if (leafHelper_n1 != null)
                                {
                                    var leafItemData_n1 = new global::XM.ConfigNew.Tests.Data.AttributeConfigUnmanaged();
                                    leafHelper_n1.AllocContainerWithFillImpl(innerVal0[n1], default(TblI), cfgi, ref leafItemData_n1, configHolderData);
                                    innerArr_1[configHolderData.Data.BlobContainer, n1] = leafItemData_n1;
                                }
                            }
                        }
                        tempVal = innerArr_1;
                        arr[configHolderData.Data.BlobContainer, key] = tempVal;
                    }
                }
                outerArray[configHolderData.Data.BlobContainer, i] = arr;
            }

            data.ComplexNested = outerArray;
        }
        /// <summary>
        /// 分配 CustomDict 容器
        /// </summary>
        private void AllocCustomDict(global::XM.ConfigNew.Tests.Data.UltimateComplexConfig config, ref global::XM.ConfigNew.Tests.Data.UltimateComplexConfigUnmanaged data, CfgI cfgi, XM.ConfigDataCenter.ConfigDataHolder configHolderData)
        {
            if (config.CustomDict == null || config.CustomDict.Count == 0)
            {
                return;
            }

            var map = configHolderData.Data.BlobContainer.AllocMap<StrI, int>(config.CustomDict.Count);
            foreach (var kvp in config.CustomDict)
            {
                if (TryGetStrI(kvp.Key, out var keyStrI))
                {
                    map[configHolderData.Data.BlobContainer, keyStrI] = kvp.Value;
                }
            }

            data.CustomDict = map;
        }
        /// <summary>
        /// 分配 CustomKeyDict 容器
        /// </summary>
        private void AllocCustomKeyDict(global::XM.ConfigNew.Tests.Data.UltimateComplexConfig config, ref global::XM.ConfigNew.Tests.Data.UltimateComplexConfigUnmanaged data, CfgI cfgi, XM.ConfigDataCenter.ConfigDataHolder configHolderData)
        {
            if (config.CustomKeyDict == null || config.CustomKeyDict.Count == 0)
            {
                return;
            }

            var map = configHolderData.Data.BlobContainer.AllocMap<StrI, int>(config.CustomKeyDict.Count);
            foreach (var kvp in config.CustomKeyDict)
            {
                if (TryGetStrI(kvp.Key, out var keyStrI))
                {
                    map[configHolderData.Data.BlobContainer, keyStrI] = kvp.Value;
                }
            }

            data.CustomKeyDict = map;
        }
        /// <summary>
        /// 分配 CustomList 容器
        /// </summary>
        private void AllocCustomList(global::XM.ConfigNew.Tests.Data.UltimateComplexConfig config, ref global::XM.ConfigNew.Tests.Data.UltimateComplexConfigUnmanaged data, CfgI cfgi, XM.ConfigDataCenter.ConfigDataHolder configHolderData)
        {
            if (config.CustomList == null || config.CustomList.Count == 0)
            {
                return;
            }

            var array = configHolderData.Data.BlobContainer.AllocArray<StrI>(config.CustomList.Count);
            for (int i = 0; i < config.CustomList.Count; i++)
            {
                if (TryGetStrI(config.CustomList[i], out var strI))
                {
                    array[configHolderData.Data.BlobContainer, i] = strI;
                }
            }

            data.CustomList = array;
        }
        /// <summary>
        /// 分配 CustomNestedList 容器
        /// </summary>
        private void AllocCustomNestedList(global::XM.ConfigNew.Tests.Data.UltimateComplexConfig config, ref global::XM.ConfigNew.Tests.Data.UltimateComplexConfigUnmanaged data, CfgI cfgi, XM.ConfigDataCenter.ConfigDataHolder configHolderData)
        {
            if (config.CustomNestedList == null || config.CustomNestedList.Count == 0)
            {
                return;
            }

            var outerArray = configHolderData.Data.BlobContainer.AllocArray<global::XBlobArray<StrI>>(config.CustomNestedList.Count);
            for (int i = 0; i < config.CustomNestedList.Count; i++)
            {
                var innerList = config.CustomNestedList[i];
                if (innerList == null || innerList.Count == 0)
                {
                    continue;
                }

                var arr = configHolderData.Data.BlobContainer.AllocArray<StrI>(innerList.Count);
                for (int j = 0; j < innerList.Count; j++)
                {
                    if (TryGetStrI(innerList[j], out var strI))
                    {
                        arr[configHolderData.Data.BlobContainer, j] = strI;
                    }
                }
                outerArray[configHolderData.Data.BlobContainer, i] = arr;
            }

            data.CustomNestedList = outerArray;
        }
        /// <summary>
        /// 填充 Price 嵌套配置
        /// </summary>
        private void FillPrice(global::XM.ConfigNew.Tests.Data.UltimateComplexConfig config, ref global::XM.ConfigNew.Tests.Data.UltimateComplexConfigUnmanaged data, CfgI cfgi, XM.ConfigDataCenter.ConfigDataHolder configHolderData)
        {
            if (config.Price != null == false)
            {
                return;
            }

            var helper = global::XM.ConfigNew.Tests.Data.AttributeConfigClassHelper.Instance;
            if (helper != null)
            {
                var nestedData = new global::XM.ConfigNew.Tests.Data.AttributeConfigUnmanaged();
                helper.AllocContainerWithFillImpl(config.Price, default(TblI), cfgi, ref nestedData, configHolderData);
                data.Price = nestedData;
            }
        }
        /// <summary>
        /// 分配 Tags 容器
        /// </summary>
        private void AllocTags(global::XM.ConfigNew.Tests.Data.UltimateComplexConfig config, ref global::XM.ConfigNew.Tests.Data.UltimateComplexConfigUnmanaged data, CfgI cfgi, XM.ConfigDataCenter.ConfigDataHolder configHolderData)
        {
            if (config.Tags == null || config.Tags.Count == 0)
            {
                return;
            }

            var array = configHolderData.Data.BlobContainer.AllocArray<StrI>(config.Tags.Count);
            for (int i = 0; i < config.Tags.Count; i++)
            {
                if (TryGetStrI(config.Tags[i], out var strI))
                {
                    array[configHolderData.Data.BlobContainer, i] = strI;
                }
            }

            data.Tags = array;
        }
        /// <summary>
        /// 分配 Attributes 容器
        /// </summary>
        private void AllocAttributes(global::XM.ConfigNew.Tests.Data.UltimateComplexConfig config, ref global::XM.ConfigNew.Tests.Data.UltimateComplexConfigUnmanaged data, CfgI cfgi, XM.ConfigDataCenter.ConfigDataHolder configHolderData)
        {
            if (config.Attributes == null || config.Attributes.Count == 0)
            {
                return;
            }

            var array = configHolderData.Data.BlobContainer.AllocArray<global::XM.ConfigNew.Tests.Data.AttributeConfigUnmanaged>(config.Attributes.Count);
            var helper = global::XM.ConfigNew.Tests.Data.AttributeConfigClassHelper.Instance;
            if (helper != null)
            {
                for (int i = 0; i < config.Attributes.Count; i++)
                {
                    if (config.Attributes[i] != null)
                    {
                        var itemData = new global::XM.ConfigNew.Tests.Data.AttributeConfigUnmanaged();
                        helper.AllocContainerWithFillImpl(config.Attributes[i], default(TblI), cfgi, ref itemData, configHolderData);
                        array[configHolderData.Data.BlobContainer, i] = itemData;
                    }
                }
            }

            data.Attributes = array;
        }
        /// <summary>
        /// 分配 Effects 容器
        /// </summary>
        private void AllocEffects(global::XM.ConfigNew.Tests.Data.UltimateComplexConfig config, ref global::XM.ConfigNew.Tests.Data.UltimateComplexConfigUnmanaged data, CfgI cfgi, XM.ConfigDataCenter.ConfigDataHolder configHolderData)
        {
            if (config.Effects == null || config.Effects.Count == 0)
            {
                return;
            }

            var array = configHolderData.Data.BlobContainer.AllocArray<global::XM.ConfigNew.Tests.Data.EffectConfigUnmanaged>(config.Effects.Count);
            var helper = global::XM.ConfigNew.Tests.Data.EffectConfigClassHelper.Instance;
            if (helper != null)
            {
                for (int i = 0; i < config.Effects.Count; i++)
                {
                    if (config.Effects[i] != null)
                    {
                        var itemData = new global::XM.ConfigNew.Tests.Data.EffectConfigUnmanaged();
                        helper.AllocContainerWithFillImpl(config.Effects[i], default(TblI), cfgi, ref itemData, configHolderData);
                        array[configHolderData.Data.BlobContainer, i] = itemData;
                    }
                }
            }

            data.Effects = array;
        }
        /// <summary>
        /// 分配 IntValues 容器
        /// </summary>
        private void AllocIntValues(global::XM.ConfigNew.Tests.Data.UltimateComplexConfig config, ref global::XM.ConfigNew.Tests.Data.UltimateComplexConfigUnmanaged data, CfgI cfgi, XM.ConfigDataCenter.ConfigDataHolder configHolderData)
        {
            if (config.IntValues == null || config.IntValues.Count == 0)
            {
                return;
            }

            var array = configHolderData.Data.BlobContainer.AllocArray<int>(config.IntValues.Count);
            for (int i = 0; i < config.IntValues.Count; i++)
            {
                array[configHolderData.Data.BlobContainer, i] = config.IntValues[i];
            }

            data.IntValues = array;
        }
        /// <summary>
        /// 分配 AttributeTypes 容器
        /// </summary>
        private void AllocAttributeTypes(global::XM.ConfigNew.Tests.Data.UltimateComplexConfig config, ref global::XM.ConfigNew.Tests.Data.UltimateComplexConfigUnmanaged data, CfgI cfgi, XM.ConfigDataCenter.ConfigDataHolder configHolderData)
        {
            if (config.AttributeTypes == null || config.AttributeTypes.Count == 0)
            {
                return;
            }

            var array = configHolderData.Data.BlobContainer.AllocArray<global::XM.ConfigNew.CodeGen.EnumWrapper<global::XM.ConfigNew.Tests.Data.EAttributeType>>(config.AttributeTypes.Count);
            for (int i = 0; i < config.AttributeTypes.Count; i++)
            {
                array[configHolderData.Data.BlobContainer, i] = new global::XM.ConfigNew.CodeGen.EnumWrapper<global::XM.ConfigNew.Tests.Data.EAttributeType>(config.AttributeTypes[i]);
            }

            data.AttributeTypes = array;
        }
        /// <summary>
        /// 分配 Matrix 容器
        /// </summary>
        private void AllocMatrix(global::XM.ConfigNew.Tests.Data.UltimateComplexConfig config, ref global::XM.ConfigNew.Tests.Data.UltimateComplexConfigUnmanaged data, CfgI cfgi, XM.ConfigDataCenter.ConfigDataHolder configHolderData)
        {
            if (config.Matrix == null || config.Matrix.Count == 0)
            {
                return;
            }

            var outerArray = configHolderData.Data.BlobContainer.AllocArray<global::XBlobArray<int>>(config.Matrix.Count);
            for (int i = 0; i < config.Matrix.Count; i++)
            {
                var innerList = config.Matrix[i];
                if (innerList == null || innerList.Count == 0)
                {
                    continue;
                }

                var arr = configHolderData.Data.BlobContainer.AllocArray<int>(innerList.Count);
                for (int j = 0; j < innerList.Count; j++)
                {
                    arr[configHolderData.Data.BlobContainer, j] = innerList[j];
                }
                outerArray[configHolderData.Data.BlobContainer, i] = arr;
            }

            data.Matrix = outerArray;
        }
        /// <summary>
        /// 分配 StringIntMap 容器
        /// </summary>
        private void AllocStringIntMap(global::XM.ConfigNew.Tests.Data.UltimateComplexConfig config, ref global::XM.ConfigNew.Tests.Data.UltimateComplexConfigUnmanaged data, CfgI cfgi, XM.ConfigDataCenter.ConfigDataHolder configHolderData)
        {
            if (config.StringIntMap == null || config.StringIntMap.Count == 0)
            {
                return;
            }

            var map = configHolderData.Data.BlobContainer.AllocMap<StrI, int>(config.StringIntMap.Count);
            foreach (var kvp in config.StringIntMap)
            {
                if (TryGetStrI(kvp.Key, out var keyStrI))
                {
                    map[configHolderData.Data.BlobContainer, keyStrI] = kvp.Value;
                }
            }

            data.StringIntMap = map;
        }
        /// <summary>
        /// 分配 AttributeValueMap 容器
        /// </summary>
        private void AllocAttributeValueMap(global::XM.ConfigNew.Tests.Data.UltimateComplexConfig config, ref global::XM.ConfigNew.Tests.Data.UltimateComplexConfigUnmanaged data, CfgI cfgi, XM.ConfigDataCenter.ConfigDataHolder configHolderData)
        {
            if (config.AttributeValueMap == null || config.AttributeValueMap.Count == 0)
            {
                return;
            }

            var map = configHolderData.Data.BlobContainer.AllocMap<global::XM.ConfigNew.CodeGen.EnumWrapper<global::XM.ConfigNew.Tests.Data.EAttributeType>, global::XBlobArray<int>>(config.AttributeValueMap.Count);
            foreach (var kvp in config.AttributeValueMap)
            {
                var key = new global::XM.ConfigNew.CodeGen.EnumWrapper<global::XM.ConfigNew.Tests.Data.EAttributeType>(kvp.Key);
                var innerVal0 = kvp.Value;
                if (innerVal0 != null && innerVal0.Count > 0)
                {
                    var tempVal = default(global::XBlobArray<int>);
                    var innerArr_1 = configHolderData.Data.BlobContainer.AllocArray<int>(innerVal0.Count);
                    for (int n1 = 0; n1 < innerVal0.Count; n1++)
                    {
                        innerArr_1[configHolderData.Data.BlobContainer, n1] = innerVal0[n1];
                    }
                    tempVal = innerArr_1;
                    map[configHolderData.Data.BlobContainer, key] = tempVal;
                }
            }

            data.AttributeValueMap = map;
        }
        /// <summary>
        /// 分配 DeepNestedContainer 容器
        /// </summary>
        private void AllocDeepNestedContainer(global::XM.ConfigNew.Tests.Data.UltimateComplexConfig config, ref global::XM.ConfigNew.Tests.Data.UltimateComplexConfigUnmanaged data, CfgI cfgi, XM.ConfigDataCenter.ConfigDataHolder configHolderData)
        {
            if (config.DeepNestedContainer == null || config.DeepNestedContainer.Count == 0)
            {
                return;
            }

            var outerArray = configHolderData.Data.BlobContainer.AllocArray<global::XBlobMap<StrI, global::XBlobArray<float>>>(config.DeepNestedContainer.Count);
            for (int i = 0; i < config.DeepNestedContainer.Count; i++)
            {
                var innerList = config.DeepNestedContainer[i];
                if (innerList == null || innerList.Count == 0)
                {
                    continue;
                }

                var arr = configHolderData.Data.BlobContainer.AllocMap<StrI, global::XBlobArray<float>>(innerList.Count);
                foreach (var kvp in innerList)
                {
                    if (TryGetStrI(kvp.Key, out var keyStrI))
                    {
                        var innerVal0 = kvp.Value;
                        if (innerVal0 != null && innerVal0.Count > 0)
                        {
                            var tempVal = default(global::XBlobArray<float>);
                            var innerArr_1 = configHolderData.Data.BlobContainer.AllocArray<float>(innerVal0.Count);
                            for (int n1 = 0; n1 < innerVal0.Count; n1++)
                            {
                                innerArr_1[configHolderData.Data.BlobContainer, n1] = innerVal0[n1];
                            }
                            tempVal = innerArr_1;
                            arr[configHolderData.Data.BlobContainer, keyStrI] = tempVal;
                        }
                    }
                }
                outerArray[configHolderData.Data.BlobContainer, i] = arr;
            }

            data.DeepNestedContainer = outerArray;
        }
        /// <summary>
        /// 分配 UniqueIds 容器
        /// </summary>
        private void AllocUniqueIds(global::XM.ConfigNew.Tests.Data.UltimateComplexConfig config, ref global::XM.ConfigNew.Tests.Data.UltimateComplexConfigUnmanaged data, CfgI cfgi, XM.ConfigDataCenter.ConfigDataHolder configHolderData)
        {
            if (config.UniqueIds == null || config.UniqueIds.Count == 0)
            {
                return;
            }

            var set = configHolderData.Data.BlobContainer.AllocSet<int>(config.UniqueIds.Count);
            foreach (var item in config.UniqueIds)
            {
                set.Add(configHolderData.Data.BlobContainer, item);
            }

            data.UniqueIds = set;
        }
        /// <summary>
        /// 分配 ComplexList 容器
        /// </summary>
        private void AllocComplexList(global::XM.ConfigNew.Tests.Data.UltimateComplexConfig config, ref global::XM.ConfigNew.Tests.Data.UltimateComplexConfigUnmanaged data, CfgI cfgi, XM.ConfigDataCenter.ConfigDataHolder configHolderData)
        {
            if (config.ComplexList == null || config.ComplexList.Count == 0)
            {
                return;
            }

            var array = configHolderData.Data.BlobContainer.AllocArray<StrI>(config.ComplexList.Count);
            for (int i = 0; i < config.ComplexList.Count; i++)
            {
                if (TryGetStrI(config.ComplexList[i], out var strI))
                {
                    array[configHolderData.Data.BlobContainer, i] = strI;
                }
            }

            data.ComplexList = array;
        }

        #endregion


        /// <summary>配置定义所属的 Mod</summary>
        public TblI _definedInMod;
    }
}
