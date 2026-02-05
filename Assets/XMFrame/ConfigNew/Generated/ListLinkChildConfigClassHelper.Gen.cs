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
    /// ListLinkChildConfig 的配置加载辅助类，用于从 XML 反序列化（静态代码生成，无反射）。
    /// </summary>
    public class ListLinkChildConfigClassHelper : ConfigClassHelper<global::XM.ConfigNew.Tests.Data.ListLinkChildConfig, global::XM.ConfigNew.Tests.Data.ListLinkChildConfigUnmanaged>
    {
        public static ListLinkChildConfigClassHelper Instance { get; private set; }
        public static TblI TblI { get; private set; }
        public static TblS TblS { get; private set; }

        /// <summary>
        /// 静态构造函数
        /// </summary>
        static ListLinkChildConfigClassHelper()
        {
            const string __tableName = "SingleLinkChild";
            const string __modName = "Default";
            CfgS<global::XM.ConfigNew.Tests.Data.ListLinkChildConfigUnmanaged>.Table = new TblS(new ModS(__modName), __tableName);
            TblS = new TblS(new ModS(__modName), __tableName);
            Instance = new ListLinkChildConfigClassHelper();
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        public ListLinkChildConfigClassHelper()
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
            var config = (global::XM.ConfigNew.Tests.Data.ListLinkChildConfig)target;

            // 解析所有字段
            config.ChildId = ParseChildId(configItem, mod, configName, context);
            config.ChildName = ParseChildName(configItem, mod, configName, context);
            config.Parent = ParseParent(configItem, mod, configName, context);
        }
        /// <summary>获取 Link Helper 类型</summary>
        public override Type GetLinkHelperType()
        {
            return null;
        }
        #region 字段解析方法 (ParseXXX)

        /// <summary>
        /// 解析 ChildId 字段
        /// </summary>
        private static int ParseChildId(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var xmlValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "ChildId");

            if (string.IsNullOrEmpty(xmlValue))
            {
                return default;
            }

            if (global::XM.Contracts.Config.ConfigParseHelper.TryParseInt(xmlValue, "ChildId", out var parsedValue))
            {
                return parsedValue;
            }

            return default;
        }

        /// <summary>
        /// 解析 ChildName 字段
        /// </summary>
        private static string ParseChildName(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var xmlValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "ChildName");

            if (string.IsNullOrEmpty(xmlValue))
            {
                return default;
            }

            // 字符串类型直接返回
            return xmlValue ?? string.Empty;
        }

        /// <summary>
        /// 解析 Parent 字段
        /// </summary>
        private static global::System.Collections.Generic.List<global::XM.Contracts.Config.CfgS<global::XM.ConfigNew.Tests.Data.LinkParentConfig>> ParseParent(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var list = new global::System.Collections.Generic.List<global::XM.Contracts.Config.CfgS<global::XM.ConfigNew.Tests.Data.LinkParentConfig>>();

            // 尝试从 XML 节点解析
            var nodes = configItem.SelectNodes("Parent");
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

                    // 类型 CfgS<LinkParentConfig> 不支持从文本解析
                    continue;
                }
            }

            // 如果没有节点，尝试 CSV 格式
            if (list.Count == 0)
            {
                var csvValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "Parent");
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

                        // 类型 CfgS<LinkParentConfig> 不支持从文本解析
                        continue;
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
            ref global::XM.ConfigNew.Tests.Data.ListLinkChildConfigUnmanaged data,
            XM.ConfigDataCenter.ConfigDataHolder configHolderData,
            XBlobPtr? linkParent = null)
        {
            var config = (global::XM.ConfigNew.Tests.Data.ListLinkChildConfig)value;

            // 分配容器和嵌套配置
            AllocParent(config, ref data, cfgi, configHolderData);

            // 填充基本类型字段
            data.ChildId = config.ChildId;
            data.ChildName = new global::Unity.Collections.FixedString32Bytes(config.ChildName ?? string.Empty);
        }
        /// <summary>
        /// 建立 Link 双向引用（链接阶段调用）
        /// </summary>
        /// <param name="config">托管配置对象</param>
        /// <param name="data">非托管数据结构（ref 传递）</param>
        /// <param name="configHolderData">配置数据持有者</param>
        public virtual void EstablishLinks(
            global::XM.ConfigNew.Tests.Data.ListLinkChildConfig config,
            ref global::XM.ConfigNew.Tests.Data.ListLinkChildConfigUnmanaged data,
            XM.ConfigDataCenter.ConfigDataHolder configHolderData)
        {
            // TODO: 实现 Link 双向引用
            // 父→子: 通过 CfgI 查找子配置，填充 XBlobPtr
            // 子→父: 通过 CfgI 查找父配置，填充引用
        }
        #region 容器分配和嵌套配置填充方法

        /// <summary>
        /// 分配 Parent 容器
        /// </summary>
        private void AllocParent(global::XM.ConfigNew.Tests.Data.ListLinkChildConfig config, ref global::XM.ConfigNew.Tests.Data.ListLinkChildConfigUnmanaged data, CfgI cfgi, XM.ConfigDataCenter.ConfigDataHolder configHolderData)
        {
            if (config.Parent == null || config.Parent.Count == 0)
            {
                return;
            }

            var array = configHolderData.Data.BlobContainer.AllocArray<CfgI<global::XM.ConfigNew.Tests.Data.LinkParentConfigUnmanaged>>(config.Parent.Count);
            for (int i = 0; i < config.Parent.Count; i++)
            {
                if (TryGetCfgI(config.Parent[i], out var cfgI))
                {
                    array[configHolderData.Data.BlobContainer, i] = cfgI.As<global::XM.ConfigNew.Tests.Data.LinkParentConfigUnmanaged>();
                }
            }

            data.Parent = array;
        }

        #endregion


        /// <summary>配置定义所属的 Mod</summary>
        public TblI _definedInMod;
    }
}
