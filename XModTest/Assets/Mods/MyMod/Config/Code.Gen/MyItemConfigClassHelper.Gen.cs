using MyMod;
using System;
using System.Collections.Generic;
using System.Xml;
using Unity.Collections;
using XM;
using XM.Contracts;
using XM.Contracts.Config;

namespace MyMod
{
    /// <summary>
    /// MyItemConfig 的配置加载辅助类，用于从 XML 反序列化（静态代码生成，无反射）。
    /// </summary>
    public class MyItemConfigClassHelper : ConfigClassHelper<global::MyMod.MyItemConfig, global::MyMod.MyItemConfigUnManaged>
    {
        public static MyItemConfigClassHelper Instance { get; private set; }
        public static TblI TblI { get; private set; }
        public static TblS TblS { get; private set; }
        private static readonly string __modName;

        /// <summary>
        /// 静态构造函数
        /// </summary>
        static MyItemConfigClassHelper()
        {
            const string __tableName = "MyItemConfig";
            __modName = "MyMod";
            CfgS<global::MyMod.MyItemConfigUnManaged>.Table = new TblS(new ModS(__modName), __tableName);
            TblS = new TblS(new ModS(__modName), __tableName);
            Instance = new MyItemConfigClassHelper();
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        public MyItemConfigClassHelper()
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
            var config = (global::MyMod.MyItemConfig)target;

            // 解析所有字段
            config.Id = ParseId(configItem, mod, configName, context);
            config.Name = ParseName(configItem, mod, configName, context);
            config.Level = ParseLevel(configItem, mod, configName, context);
            config.Tags = ParseTags(configItem, mod, configName, context);
        }
        #region 字段解析方法 (ParseXXX)

        /// <summary>
        /// 解析 Id 字段
        /// </summary>
        private static global::XM.Contracts.Config.CfgS<TestConfig> ParseId(
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
                return new global::XM.Contracts.Config.CfgS<TestConfig>(new global::XM.Contracts.Config.ModS(modName), cfgName);
            }

            // 如果 configName 不包含 :: 分隔符，使用当前 mod.Name 补充
            return new global::XM.Contracts.Config.CfgS<TestConfig>(mod, configName);
        }

        /// <summary>
        /// 解析 Name 字段
        /// </summary>
        private static string ParseName(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var xmlValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "Name");

            if (string.IsNullOrEmpty(xmlValue))
            {
                return default;
            }

            // 字符串类型直接返回
            return xmlValue ?? string.Empty;
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
        /// 解析 Tags 字段
        /// </summary>
        private static global::System.Collections.Generic.List<int> ParseTags(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var list = new global::System.Collections.Generic.List<int>();

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

                    if (global::XM.Contracts.Config.ConfigParseHelper.TryParseInt(text, "Tags", out var parsedItem))
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

                        if (global::XM.Contracts.Config.ConfigParseHelper.TryParseInt(trimmed, "Tags", out var parsedItem))
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
            ref global::MyMod.MyItemConfigUnManaged data,
            XM.ConfigDataCenter.ConfigDataHolder configHolderData,
            XBlobPtr? linkParent = null)
        {
            var config = (global::MyMod.MyItemConfig)value;

            // 分配容器和嵌套配置
            AllocTags(config, ref data, cfgi, configHolderData);

            // 填充基本类型字段
            if (TryGetCfgI(config.Id, out var IdCfgI))
            {
                data.Id = IdCfgI.As<TestConfigUnmanaged>();
            }
            data.Name = SafeConvertToFixedString32(config.Name ?? string.Empty);
            data.Level = config.Level;
        }
        #region 容器分配和嵌套配置填充方法

        /// <summary>
        /// 分配 Tags 容器
        /// </summary>
        private void AllocTags(global::MyMod.MyItemConfig config, ref global::MyMod.MyItemConfigUnManaged data, CfgI cfgi, XM.ConfigDataCenter.ConfigDataHolder configHolderData)
        {
            if (config.Tags == null || config.Tags.Count == 0)
            {
                return;
            }

            var array = configHolderData.Data.BlobContainer.AllocArray<int>(config.Tags.Count);
            for (int i = 0; i < config.Tags.Count; i++)
            {
                var elemiDirect = config.Tags[i];
                array[configHolderData.Data.BlobContainer, i] = elemiDirect;
            }

            data.Tags = array;
        }

        #endregion


        /// <summary>配置定义所属的 Mod</summary>
        public TblI _definedInMod;
    }
}
