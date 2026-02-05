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
    /// ContainerConverterConfig 的配置加载辅助类，用于从 XML 反序列化（静态代码生成，无反射）。
    /// </summary>
    public class ContainerConverterConfigClassHelper : ConfigClassHelper<global::XM.ConfigNew.Tests.Data.ContainerConverterConfig, global::XM.ConfigNew.Tests.Data.ContainerConverterConfigUnmanaged>
    {
        public static ContainerConverterConfigClassHelper Instance { get; private set; }
        public static TblI TblI { get; private set; }
        public static TblS TblS { get; private set; }

        /// <summary>
        /// 静态构造函数
        /// </summary>
        static ContainerConverterConfigClassHelper()
        {
            const string __tableName = "ContainerConverterConfig";
            const string __modName = "Default";
            CfgS<global::XM.ConfigNew.Tests.Data.ContainerConverterConfigUnmanaged>.Table = new TblS(new ModS(__modName), __tableName);
            TblS = new TblS(new ModS(__modName), __tableName);
            Instance = new ContainerConverterConfigClassHelper();
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        public ContainerConverterConfigClassHelper()
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
            var config = (global::XM.ConfigNew.Tests.Data.ContainerConverterConfig)target;

            // 解析所有字段
            config.CustomStringList = ParseCustomStringList(configItem, mod, configName, context);
            config.CustomValueDict = ParseCustomValueDict(configItem, mod, configName, context);
            config.CustomKeyDict = ParseCustomKeyDict(configItem, mod, configName, context);
            config.CustomBothDict = ParseCustomBothDict(configItem, mod, configName, context);
            config.NestedCustomList = ParseNestedCustomList(configItem, mod, configName, context);
            config.EnumList = ParseEnumList(configItem, mod, configName, context);
        }
        /// <summary>获取 Link Helper 类型</summary>
        public override Type GetLinkHelperType()
        {
            return null;
        }
        #region 字段解析方法 (ParseXXX)

        /// <summary>
        /// 解析 CustomStringList 字段
        /// </summary>
        private static global::System.Collections.Generic.List<string> ParseCustomStringList(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var list = new global::System.Collections.Generic.List<string>();

            // 尝试从 XML 节点解析
            var nodes = configItem.SelectNodes("CustomStringList");
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
                var csvValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "CustomStringList");
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
        /// 解析 CustomValueDict 字段
        /// </summary>
        private static global::System.Collections.Generic.Dictionary<string, int> ParseCustomValueDict(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var dict = new global::System.Collections.Generic.Dictionary<string, int>();

            // 解析 Dictionary Item 节点
            var dictNodes = configItem.SelectNodes("CustomValueDict/Item");
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
                        if (global::XM.Contracts.Config.ConfigParseHelper.TryParseInt(valueText, "CustomValueDict.Value", out var parsedValue))
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
        /// 解析 CustomBothDict 字段
        /// </summary>
        private static global::System.Collections.Generic.Dictionary<string, int> ParseCustomBothDict(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var dict = new global::System.Collections.Generic.Dictionary<string, int>();

            // 解析 Dictionary Item 节点
            var dictNodes = configItem.SelectNodes("CustomBothDict/Item");
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
                        if (global::XM.Contracts.Config.ConfigParseHelper.TryParseInt(valueText, "CustomBothDict.Value", out var parsedValue))
                        {
                            dict[parsedKey] = parsedValue;
                        }
                    }
                }
            }

            return dict;
        }

        /// <summary>
        /// 解析 NestedCustomList 字段
        /// </summary>
        private static global::System.Collections.Generic.List<global::System.Collections.Generic.List<string>> ParseNestedCustomList(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var list = new global::System.Collections.Generic.List<global::System.Collections.Generic.List<string>>();

            // 尝试从 XML 节点解析
            var nodes = configItem.SelectNodes("NestedCustomList");
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
        /// 解析 EnumList 字段
        /// </summary>
        private static global::System.Collections.Generic.List<global::XM.ConfigNew.Tests.Data.EItemType> ParseEnumList(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var list = new global::System.Collections.Generic.List<global::XM.ConfigNew.Tests.Data.EItemType>();

            // 尝试从 XML 节点解析
            var nodes = configItem.SelectNodes("EnumList");
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
                        list.Add(parsedItem);
                    }
                }
            }

            // 如果没有节点，尝试 CSV 格式
            if (list.Count == 0)
            {
                var csvValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "EnumList");
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

                        if (global::System.Enum.TryParse<global::XM.ConfigNew.Tests.Data.EItemType>(trimmed, out var parsedItem))
                        {
                            list.Add(parsedItem);
                        }
                    }
                }
            }

            return list;
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
            ref global::XM.ConfigNew.Tests.Data.ContainerConverterConfigUnmanaged data,
            XM.ConfigDataCenter.ConfigDataHolder configHolderData,
            XBlobPtr? linkParent = null)
        {
            var config = (global::XM.ConfigNew.Tests.Data.ContainerConverterConfig)value;

            // 分配容器和嵌套配置
            AllocCustomStringList(config, ref data, cfgi, configHolderData);
            AllocCustomValueDict(config, ref data, cfgi, configHolderData);
            AllocCustomKeyDict(config, ref data, cfgi, configHolderData);
            AllocCustomBothDict(config, ref data, cfgi, configHolderData);
            AllocNestedCustomList(config, ref data, cfgi, configHolderData);
            AllocEnumList(config, ref data, cfgi, configHolderData);

        }
        /// <summary>
        /// 建立 Link 双向引用（链接阶段调用）
        /// </summary>
        /// <param name="config">托管配置对象</param>
        /// <param name="data">非托管数据结构（ref 传递）</param>
        /// <param name="configHolderData">配置数据持有者</param>
        public virtual void EstablishLinks(
            global::XM.ConfigNew.Tests.Data.ContainerConverterConfig config,
            ref global::XM.ConfigNew.Tests.Data.ContainerConverterConfigUnmanaged data,
            XM.ConfigDataCenter.ConfigDataHolder configHolderData)
        {
            // TODO: 实现 Link 双向引用
            // 父→子: 通过 CfgI 查找子配置，填充 XBlobPtr
            // 子→父: 通过 CfgI 查找父配置，填充引用
        }
        #region 容器分配和嵌套配置填充方法

        /// <summary>
        /// 分配 CustomStringList 容器
        /// </summary>
        private void AllocCustomStringList(global::XM.ConfigNew.Tests.Data.ContainerConverterConfig config, ref global::XM.ConfigNew.Tests.Data.ContainerConverterConfigUnmanaged data, CfgI cfgi, XM.ConfigDataCenter.ConfigDataHolder configHolderData)
        {
            if (config.CustomStringList == null || config.CustomStringList.Count == 0)
            {
                return;
            }

            var array = configHolderData.Data.BlobContainer.AllocArray<StrI>(config.CustomStringList.Count);
            for (int i = 0; i < config.CustomStringList.Count; i++)
            {
                if (TryGetStrI(config.CustomStringList[i], out var strI))
                {
                    array[configHolderData.Data.BlobContainer, i] = strI;
                }
            }

            data.CustomStringList = array;
        }
        /// <summary>
        /// 分配 CustomValueDict 容器
        /// </summary>
        private void AllocCustomValueDict(global::XM.ConfigNew.Tests.Data.ContainerConverterConfig config, ref global::XM.ConfigNew.Tests.Data.ContainerConverterConfigUnmanaged data, CfgI cfgi, XM.ConfigDataCenter.ConfigDataHolder configHolderData)
        {
            if (config.CustomValueDict == null || config.CustomValueDict.Count == 0)
            {
                return;
            }

            var map = configHolderData.Data.BlobContainer.AllocMap<StrI, int>(config.CustomValueDict.Count);
            foreach (var kvp in config.CustomValueDict)
            {
                if (TryGetStrI(kvp.Key, out var keyStrI))
                {
                    map[configHolderData.Data.BlobContainer, keyStrI] = kvp.Value;
                }
            }

            data.CustomValueDict = map;
        }
        /// <summary>
        /// 分配 CustomKeyDict 容器
        /// </summary>
        private void AllocCustomKeyDict(global::XM.ConfigNew.Tests.Data.ContainerConverterConfig config, ref global::XM.ConfigNew.Tests.Data.ContainerConverterConfigUnmanaged data, CfgI cfgi, XM.ConfigDataCenter.ConfigDataHolder configHolderData)
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
        /// 分配 CustomBothDict 容器
        /// </summary>
        private void AllocCustomBothDict(global::XM.ConfigNew.Tests.Data.ContainerConverterConfig config, ref global::XM.ConfigNew.Tests.Data.ContainerConverterConfigUnmanaged data, CfgI cfgi, XM.ConfigDataCenter.ConfigDataHolder configHolderData)
        {
            if (config.CustomBothDict == null || config.CustomBothDict.Count == 0)
            {
                return;
            }

            var map = configHolderData.Data.BlobContainer.AllocMap<StrI, int>(config.CustomBothDict.Count);
            foreach (var kvp in config.CustomBothDict)
            {
                if (TryGetStrI(kvp.Key, out var keyStrI))
                {
                    map[configHolderData.Data.BlobContainer, keyStrI] = kvp.Value;
                }
            }

            data.CustomBothDict = map;
        }
        /// <summary>
        /// 分配 NestedCustomList 容器
        /// </summary>
        private void AllocNestedCustomList(global::XM.ConfigNew.Tests.Data.ContainerConverterConfig config, ref global::XM.ConfigNew.Tests.Data.ContainerConverterConfigUnmanaged data, CfgI cfgi, XM.ConfigDataCenter.ConfigDataHolder configHolderData)
        {
            if (config.NestedCustomList == null || config.NestedCustomList.Count == 0)
            {
                return;
            }

            var outerArray = configHolderData.Data.BlobContainer.AllocArray<global::XBlobArray<StrI>>(config.NestedCustomList.Count);
            for (int i = 0; i < config.NestedCustomList.Count; i++)
            {
                var innerList = config.NestedCustomList[i];
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

            data.NestedCustomList = outerArray;
        }
        /// <summary>
        /// 分配 EnumList 容器
        /// </summary>
        private void AllocEnumList(global::XM.ConfigNew.Tests.Data.ContainerConverterConfig config, ref global::XM.ConfigNew.Tests.Data.ContainerConverterConfigUnmanaged data, CfgI cfgi, XM.ConfigDataCenter.ConfigDataHolder configHolderData)
        {
            if (config.EnumList == null || config.EnumList.Count == 0)
            {
                return;
            }

            var array = configHolderData.Data.BlobContainer.AllocArray<global::XM.ConfigNew.CodeGen.EnumWrapper<global::XM.ConfigNew.Tests.Data.EItemType>>(config.EnumList.Count);
            for (int i = 0; i < config.EnumList.Count; i++)
            {
                array[configHolderData.Data.BlobContainer, i] = new global::XM.ConfigNew.CodeGen.EnumWrapper<global::XM.ConfigNew.Tests.Data.EItemType>(config.EnumList[i]);
            }

            data.EnumList = array;
        }

        #endregion


        /// <summary>配置定义所属的 Mod</summary>
        public TblI _definedInMod;
    }
}
