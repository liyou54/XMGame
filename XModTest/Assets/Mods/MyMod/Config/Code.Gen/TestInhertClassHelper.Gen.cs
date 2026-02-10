using System;
using System.Collections.Generic;
using System.Xml;
using Unity.Collections;
using XM;
using XM.Contracts;
using XM.Contracts.Config;
using XM.Editor.Gen;

namespace XM.Editor.Gen
{
    /// <summary>
    /// TestInhert 的配置加载辅助类，用于从 XML 反序列化（静态代码生成，无反射）。
    /// </summary>
    public class TestInhertClassHelper : ConfigClassHelper<global::XM.Editor.Gen.TestInhert, global::XM.Editor.Gen.TestInhertUnmanaged>
    {
        public static TestInhertClassHelper Instance { get; private set; }
        public static TblI TblI { get; private set; }
        public static TblS TblS { get; private set; }
        private static readonly string __modName;

        /// <summary>
        /// 静态构造函数
        /// </summary>
        static TestInhertClassHelper()
        {
            const string __tableName = "TestInhert";
            __modName = "MyMod";
            CfgS<global::XM.Editor.Gen.TestInhertUnmanaged>.Table = new TblS(new ModS(__modName), __tableName);
            TblS = new TblS(new ModS(__modName), __tableName);
            Instance = new TestInhertClassHelper();
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        public TestInhertClassHelper()
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
            var config = (global::XM.Editor.Gen.TestInhert)target;

            // 解析所有字段
            config.Link = ParseLink(configItem, mod, configName, context);
            config.xxxx = Parsexxxx(configItem, mod, configName, context);
        }
        #region 字段解析方法 (ParseXXX)

        /// <summary>
        /// 解析 Link 字段
        /// </summary>
        private static global::XM.Contracts.Config.CfgS<TestConfig> ParseLink(
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
            if (global::XM.Contracts.Config.ConfigParseHelper.TryParseCfgSString(configName, "Link", out var modName, out var cfgName))
            {
                return new global::XM.Contracts.Config.CfgS<TestConfig>(new global::XM.Contracts.Config.ModS(modName), cfgName);
            }

            // 如果 configName 不包含 :: 分隔符，使用当前 mod.Name 补充
            return new global::XM.Contracts.Config.CfgS<TestConfig>(mod, configName);
        }

        /// <summary>
        /// 解析 xxxx 字段
        /// </summary>
        private static int Parsexxxx(
            global::System.Xml.XmlElement configItem,
            global::XM.Contracts.Config.ModS mod,
            string configName,
            in global::XM.Contracts.Config.ConfigParseContext context)
        {
            var xmlValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "xxxx");

            if (string.IsNullOrEmpty(xmlValue))
            {
                return default;
            }

            if (global::XM.Contracts.Config.ConfigParseHelper.TryParseInt(xmlValue, "xxxx", out var parsedValue))
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
            ref global::XM.Editor.Gen.TestInhertUnmanaged data,
            XM.ConfigDataCenter.ConfigDataHolder configHolderData,
            XBlobPtr? linkParent = null)
        {
            var config = (global::XM.Editor.Gen.TestInhert)value;

            // 填充基本类型字段
            if (TryGetCfgI(config.Link, out var LinkCfgI))
            {
                data.Link = LinkCfgI.As<TestConfigUnmanaged>();
            }
            data.xxxx = config.xxxx;
        }
        #region 索引初始化和查询方法

        /// <summary>
        /// 初始化索引并填充数据
        /// </summary>
        /// <param name="configData">配置数据容器</param>
        /// <param name="tableMap">表的主数据 Map (CfgI -> TUnmanaged)</param>
        public void InitializeIndexes(
            ref XM.ConfigData configData,
            XBlobMap<CfgI, global::XM.Editor.Gen.TestInhertUnmanaged> tableMap)
        {
            // 获取配置数量
            int configCount = tableMap.GetLength(configData.BlobContainer);

            // 第一遍：统计 XMLLink 索引容量
            int capacity_ByParent_TestConfig = 0;

            for (int i = 0; i < configCount; i++)
            {
                var cfgId = tableMap.GetKey(configData.BlobContainer, i);
                ref var data = ref tableMap.GetRef(configData.BlobContainer, cfgId, out bool exists);
                if (!exists) continue;

                // 统计索引 ByParent_TestConfig 的有效引用数
                if (data.Link.Valid)
                    capacity_ByParent_TestConfig++;

            }

            // 第二遍：创建索引容器
            // 索引: ByParent_TestConfig (XMLLink 自动生成)
            var indexByParent_TestConfigMap = configData.AllocMultiIndex<TestInhertUnmanaged.ByParent_TestConfigIndex, global::XM.Editor.Gen.TestInhertUnmanaged>(TestInhertUnmanaged.ByParent_TestConfigIndex.IndexType, capacity_ByParent_TestConfig);

            // 遍历所有配置，填充索引数据
            for (int i = 0; i < configCount; i++)
            {
                var cfgId = tableMap.GetKey(configData.BlobContainer, i);
                ref var data = ref tableMap.GetRef(configData.BlobContainer, cfgId, out bool exists);
                if (!exists) continue;

                // 填充索引: ByParent_TestConfig (XMLLink)
                if (data.Link.Valid)
                {
                    var indexKeyByParent_TestConfig = new TestInhertUnmanaged.ByParent_TestConfigIndex(data.Link);
                    // 多值索引：允许一个父节点有多个子Link
                    indexByParent_TestConfigMap.Add(configData.BlobContainer, indexKeyByParent_TestConfig, cfgId);
                }

            }
        }

        #endregion


        /// <summary>配置定义所属的 Mod</summary>
        public TblI _definedInMod;
    }
}
