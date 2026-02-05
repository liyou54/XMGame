using System;
using System.Collections.Generic;
using System.Xml;
using Unity.Collections;
using XM;
using XM.Contracts;
using XM.Contracts.Config;

/// <summary>
/// NestedConfig 的配置加载辅助类，用于从 XML 反序列化（静态代码生成，无反射）。
/// </summary>
public class NestedConfigClassHelper : ConfigClassHelper<NestedConfig, NestedConfigUnManaged>
{
    public static NestedConfigClassHelper Instance { get; private set; }
    public static TblI TblI { get; private set; }
    public static TblS TblS { get; private set; }

    /// <summary>
    /// 静态构造函数
    /// </summary>
    static NestedConfigClassHelper()
    {
        const string __tableName = "NestedConfig";
        const string __modName = "MyMod";
        CfgS<NestedConfigUnManaged>.Table = new TblS(new ModS(__modName), __tableName);
        TblS = new TblS(new ModS(__modName), __tableName);
        Instance = new NestedConfigClassHelper();
    }
    /// <summary>
    /// 构造函数
    /// </summary>
    public NestedConfigClassHelper()
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
        var config = (NestedConfig)target;

        // 解析所有字段
        config.RequiredId = ParseRequiredId(configItem, mod, configName, context);
        config.OptionalWithDefault = ParseOptionalWithDefault(configItem, mod, configName, context);
        config.Test = ParseTest(configItem, mod, configName, context);
        config.TestCustom = ParseTestCustom(configItem, mod, configName, context);
        config.TestGlobalConvert = ParseTestGlobalConvert(configItem, mod, configName, context);
        config.TestKeyList = ParseTestKeyList(configItem, mod, configName, context);
        config.StrIndex = ParseStrIndex(configItem, mod, configName, context);
        config.Str32 = ParseStr32(configItem, mod, configName, context);
        config.Str64 = ParseStr64(configItem, mod, configName, context);
        config.Str = ParseStr(configItem, mod, configName, context);
        config.LabelS = ParseLabelS(configItem, mod, configName, context);
    }
    /// <summary>获取 Link Helper 类型</summary>
    public override Type GetLinkHelperType()
    {
        return null;
    }
    #region 字段解析方法 (ParseXXX)

    /// <summary>
    /// 解析 RequiredId 字段
    /// </summary>
    private static int ParseRequiredId(
        global::System.Xml.XmlElement configItem,
        global::XM.Contracts.Config.ModS mod,
        string configName,
        in global::XM.Contracts.Config.ConfigParseContext context)
    {
        var xmlValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "RequiredId");

        if (string.IsNullOrEmpty(xmlValue))
        {
            return default;
        }

        if (global::XM.Contracts.Config.ConfigParseHelper.TryParseInt(xmlValue, "RequiredId", out var parsedValue))
        {
            return parsedValue;
        }

        return default;
    }

    /// <summary>
    /// 解析 OptionalWithDefault 字段
    /// </summary>
    private static string ParseOptionalWithDefault(
        global::System.Xml.XmlElement configItem,
        global::XM.Contracts.Config.ModS mod,
        string configName,
        in global::XM.Contracts.Config.ConfigParseContext context)
    {
        var xmlValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "OptionalWithDefault");

        // 默认值: default
        if (string.IsNullOrEmpty(xmlValue))
        {
            xmlValue = "default";
        }

        // 字符串类型直接返回
        return xmlValue ?? string.Empty;
    }

    /// <summary>
    /// 解析 Test 字段
    /// </summary>
    private static int ParseTest(
        global::System.Xml.XmlElement configItem,
        global::XM.Contracts.Config.ModS mod,
        string configName,
        in global::XM.Contracts.Config.ConfigParseContext context)
    {
        var xmlValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "Test");

        if (string.IsNullOrEmpty(xmlValue))
        {
            return default;
        }

        if (global::XM.Contracts.Config.ConfigParseHelper.TryParseInt(xmlValue, "Test", out var parsedValue))
        {
            return parsedValue;
        }

        return default;
    }

    /// <summary>
    /// 解析 TestCustom 字段
    /// </summary>
    private static global::Unity.Mathematics.int2 ParseTestCustom(
        global::System.Xml.XmlElement configItem,
        global::XM.Contracts.Config.ModS mod,
        string configName,
        in global::XM.Contracts.Config.ConfigParseContext context)
    {
        var xmlValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "TestCustom");

        if (string.IsNullOrEmpty(xmlValue))
        {
            return default;
        }

        // 未知类型: Unity.Mathematics.int2
        return default;
    }

    /// <summary>
    /// 解析 TestGlobalConvert 字段
    /// </summary>
    private static global::Unity.Mathematics.int2 ParseTestGlobalConvert(
        global::System.Xml.XmlElement configItem,
        global::XM.Contracts.Config.ModS mod,
        string configName,
        in global::XM.Contracts.Config.ConfigParseContext context)
    {
        var xmlValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "TestGlobalConvert");

        if (string.IsNullOrEmpty(xmlValue))
        {
            return default;
        }

        // 未知类型: Unity.Mathematics.int2
        return default;
    }

    /// <summary>
    /// 解析 TestKeyList 字段
    /// </summary>
    private static global::System.Collections.Generic.List<global::XM.Contracts.Config.CfgS<TestConfig>> ParseTestKeyList(
        global::System.Xml.XmlElement configItem,
        global::XM.Contracts.Config.ModS mod,
        string configName,
        in global::XM.Contracts.Config.ConfigParseContext context)
    {
        var list = new global::System.Collections.Generic.List<global::XM.Contracts.Config.CfgS<TestConfig>>();

        // 尝试从 XML 节点解析
        var nodes = configItem.SelectNodes("TestKeyList");
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

                // 类型 CfgS<TestConfig> 不支持从文本解析
                continue;
            }
        }

        // 如果没有节点，尝试 CSV 格式
        if (list.Count == 0)
        {
            var csvValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "TestKeyList");
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

                    // 类型 CfgS<TestConfig> 不支持从文本解析
                    continue;
                }
            }
        }

        return list;
    }

    /// <summary>
    /// 解析 StrIndex 字段
    /// </summary>
    private static string ParseStrIndex(
        global::System.Xml.XmlElement configItem,
        global::XM.Contracts.Config.ModS mod,
        string configName,
        in global::XM.Contracts.Config.ConfigParseContext context)
    {
        var xmlValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "StrIndex");

        if (string.IsNullOrEmpty(xmlValue))
        {
            return default;
        }

        // 字符串类型直接返回
        return xmlValue ?? string.Empty;
    }

    /// <summary>
    /// 解析 Str32 字段
    /// </summary>
    private static string ParseStr32(
        global::System.Xml.XmlElement configItem,
        global::XM.Contracts.Config.ModS mod,
        string configName,
        in global::XM.Contracts.Config.ConfigParseContext context)
    {
        var xmlValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "Str32");

        if (string.IsNullOrEmpty(xmlValue))
        {
            return default;
        }

        // 字符串类型直接返回
        return xmlValue ?? string.Empty;
    }

    /// <summary>
    /// 解析 Str64 字段
    /// </summary>
    private static string ParseStr64(
        global::System.Xml.XmlElement configItem,
        global::XM.Contracts.Config.ModS mod,
        string configName,
        in global::XM.Contracts.Config.ConfigParseContext context)
    {
        var xmlValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "Str64");

        if (string.IsNullOrEmpty(xmlValue))
        {
            return default;
        }

        // 字符串类型直接返回
        return xmlValue ?? string.Empty;
    }

    /// <summary>
    /// 解析 Str 字段
    /// </summary>
    private static string ParseStr(
        global::System.Xml.XmlElement configItem,
        global::XM.Contracts.Config.ModS mod,
        string configName,
        in global::XM.Contracts.Config.ConfigParseContext context)
    {
        var xmlValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "Str");

        if (string.IsNullOrEmpty(xmlValue))
        {
            return default;
        }

        // 字符串类型直接返回
        return xmlValue ?? string.Empty;
    }

    /// <summary>
    /// 解析 LabelS 字段
    /// </summary>
    private static global::XM.LabelS ParseLabelS(
        global::System.Xml.XmlElement configItem,
        global::XM.Contracts.Config.ModS mod,
        string configName,
        in global::XM.Contracts.Config.ConfigParseContext context)
    {
        var xmlValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "LabelS");

        if (string.IsNullOrEmpty(xmlValue))
        {
            return default;
        }

        // 未知类型: XM.LabelS
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
        ref NestedConfigUnManaged data,
        XM.ConfigDataCenter.ConfigDataHolder configHolderData,
        XBlobPtr? linkParent = null)
    {
        var config = (NestedConfig)value;

        // 分配容器和嵌套配置
        AllocTestKeyList(config, ref data, cfgi, configHolderData);

        // 填充基本类型字段
        data.RequiredId = config.RequiredId;
        data.OptionalWithDefault = new global::Unity.Collections.FixedString32Bytes(config.OptionalWithDefault ?? string.Empty);
        data.Test = config.Test;
        data.TestCustom = config.TestCustom;
        data.TestGlobalConvert = config.TestGlobalConvert;
        if (TryGetLabelI(config.StrIndex, out var StrIndexLabelI))
        {
            data.StrIndex = StrIndexLabelI;
        }
        data.Str32 = new global::Unity.Collections.FixedString32Bytes(config.Str32 ?? string.Empty);
        data.Str64 = new global::Unity.Collections.FixedString64Bytes(config.Str64 ?? string.Empty);
        if (TryGetStrI(config.Str, out var StrStrI))
        {
            data.Str = StrStrI;
        }
        if (TryGetLabelI(config.LabelS, out var LabelSLabelI))
        {
            data.LabelS = LabelSLabelI;
        }
    }
    /// <summary>
    /// 建立 Link 双向引用（链接阶段调用）
    /// </summary>
    /// <param name="config">托管配置对象</param>
    /// <param name="data">非托管数据结构（ref 传递）</param>
    /// <param name="configHolderData">配置数据持有者</param>
    public virtual void EstablishLinks(
        NestedConfig config,
        ref NestedConfigUnManaged data,
        XM.ConfigDataCenter.ConfigDataHolder configHolderData)
    {
        // TODO: 实现 Link 双向引用
        // 父→子: 通过 CfgI 查找子配置，填充 XBlobPtr
        // 子→父: 通过 CfgI 查找父配置，填充引用
    }
    #region 容器分配和嵌套配置填充方法

    /// <summary>
    /// 分配 TestKeyList 容器
    /// </summary>
    private void AllocTestKeyList(NestedConfig config, ref NestedConfigUnManaged data, CfgI cfgi, XM.ConfigDataCenter.ConfigDataHolder configHolderData)
    {
        if (config.TestKeyList == null || config.TestKeyList.Count == 0)
        {
            return;
        }

        var array = configHolderData.Data.BlobContainer.AllocArray<CfgI<TestConfigUnmanaged>>(config.TestKeyList.Count);
        for (int i = 0; i < config.TestKeyList.Count; i++)
        {
            if (TryGetCfgI(config.TestKeyList[i], out var cfgI))
            {
                array[configHolderData.Data.BlobContainer, i] = cfgI.As<TestConfigUnmanaged>();
            }
        }

        data.TestKeyList = array;
    }

    #endregion


    /// <summary>配置定义所属的 Mod</summary>
    public TblI _definedInMod;
}
