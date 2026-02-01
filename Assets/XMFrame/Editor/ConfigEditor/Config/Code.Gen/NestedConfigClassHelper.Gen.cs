using System;
using System.Collections.Generic;
using System.Xml;
using Unity.Collections;
using Unity.Mathematics;
using XM;
using XM.Contracts;
using XM.Contracts.Config;
using XM.Utils;


/// <summary>
/// NestedConfig 的配置加载辅助类，用于从 XML 反序列化（静态代码生成，无反射）。
/// </summary>
public sealed class NestedConfigClassHelper : ConfigClassHelper<NestedConfig, NestedConfigUnManaged>
{
    public static TblI TblI { get; private set; }
    public static TblS TblS { get; private set; }

    static NestedConfigClassHelper()
    {
        const string __tableName = "NestedConfig";
        const string __modName = "Default";
        CfgS<NestedConfigUnManaged>.Table = new TblS(new ModS(__modName), __tableName);
        TblS = new TblS(new ModS(__modName), __tableName);
    }

    public NestedConfigClassHelper(IConfigDataCenter dataCenter)
        : base(dataCenter)
    {
        TypeConverterRegistry.RegisterLocalConverter<String, int2>("", new TestInt2Convert());
    }

    public override TblS GetTblS()
    {
        return new TblS(new ModS("Default"), "NestedConfig");
    }

    public override void SetTblIDefinedInMod(TblI tbl)
    {
        _definedInMod = tbl;
    }

    public override void ParseAndFillFromXml(IXConfig target, XmlElement configItem, ModS mod, string configName, in ConfigParseContext context)
    {
        var config = (NestedConfig)target;
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

    #region 字段解析 (ParseXXX)

    private static int ParseRequiredId(XmlElement configItem, ModS mod, string configName,
        in ConfigParseContext context)
    {
        var s = ConfigParseHelper.GetXmlFieldValue(configItem, "RequiredId");
        if (string.IsNullOrEmpty(s)) { ConfigParseHelper.LogParseWarning("RequiredId", s ?? "", null); }
        if (string.IsNullOrEmpty(s)) return default;
        return ConfigParseHelper.TryParseInt(s, "RequiredId", out var v) ? v : default;
    }

    private static string ParseOptionalWithDefault(XmlElement configItem, ModS mod, string configName,
        in ConfigParseContext context)
    {
        var s = ConfigParseHelper.GetXmlFieldValue(configItem, "OptionalWithDefault");
        if (string.IsNullOrEmpty(s)) { s = "default"; }
        return s ?? "";
    }

    private static int ParseTest(XmlElement configItem, ModS mod, string configName,
        in ConfigParseContext context)
    {
        var s = ConfigParseHelper.GetXmlFieldValue(configItem, "Test");
        if (string.IsNullOrEmpty(s)) return default;
        return ConfigParseHelper.TryParseInt(s, "Test", out var v) ? v : default;
    }

    private static int2 ParseTestCustom(XmlElement configItem, ModS mod, string configName,
        in ConfigParseContext context)
    {
        try
        {
            var s = ConfigParseHelper.GetXmlFieldValue(configItem, "TestCustom");
            if (string.IsNullOrEmpty(s)) return default;
            var converter = XM.Contracts.IConfigDataCenter.I?.GetConverterByType<string, int2>();
            return converter != null && converter.Convert(s, out var result) ? result : default;
        }
        catch (Exception ex)
        {
            if (ConfigParseHelper.IsStrictMode(context))
                ConfigParseHelper.LogParseError(context, "TestCustom", ex);
            else
                ConfigParseHelper.LogParseWarning("TestCustom",
                    ConfigParseHelper.GetXmlFieldValue(configItem, "TestCustom"), ex);
            return default;
        }
    }

    private static int2 ParseTestGlobalConvert(XmlElement configItem, ModS mod, string configName,
        in ConfigParseContext context)
    {
        try
        {
            var s = ConfigParseHelper.GetXmlFieldValue(configItem, "TestGlobalConvert");
            if (string.IsNullOrEmpty(s)) return default;
            var converter = XM.Contracts.IConfigDataCenter.I?.GetConverter<string, int2>("global");
            return converter != null && converter.Convert(s, out var result) ? result : default;
        }
        catch (Exception ex)
        {
            if (ConfigParseHelper.IsStrictMode(context))
                ConfigParseHelper.LogParseError(context, "TestGlobalConvert", ex);
            else
                ConfigParseHelper.LogParseWarning("TestGlobalConvert",
                    ConfigParseHelper.GetXmlFieldValue(configItem, "TestGlobalConvert"), ex);
            return default;
        }
    }

    private static List<CfgS<TestConfigUnManaged>> ParseTestKeyList(XmlElement configItem, ModS mod, string configName,
        in ConfigParseContext context)
    {
        try
        {
            var list = new List<CfgS<TestConfigUnManaged>>();
            var nodes = configItem.SelectNodes("TestKeyList");
            if (nodes != null)
            foreach (System.Xml.XmlNode n in nodes) { var t = (n as System.Xml.XmlElement)?.InnerText?.Trim(); if (!string.IsNullOrEmpty(t) && ConfigParseHelper.TryParseCfgSString(t, "TestKeyList", out var mn, out var cn)) list.Add(new CfgS<TestConfigUnManaged>(new ModS(mn), cn)); }
            return list;
        }
        catch (Exception ex)
        {
            if (ConfigParseHelper.IsStrictMode(context))
                ConfigParseHelper.LogParseError(context, "TestKeyList", ex);
            else
                ConfigParseHelper.LogParseWarning("TestKeyList",
                    null, ex);
            return new List<CfgS<TestConfigUnManaged>>();
        }
    }

    private static string ParseStrIndex(XmlElement configItem, ModS mod, string configName,
        in ConfigParseContext context)
    {
        var s = ConfigParseHelper.GetXmlFieldValue(configItem, "StrIndex");
        return s ?? "";
    }

    private static string ParseStr32(XmlElement configItem, ModS mod, string configName,
        in ConfigParseContext context)
    {
        var s = ConfigParseHelper.GetXmlFieldValue(configItem, "Str32");
        return s ?? "";
    }

    private static string ParseStr64(XmlElement configItem, ModS mod, string configName,
        in ConfigParseContext context)
    {
        var s = ConfigParseHelper.GetXmlFieldValue(configItem, "Str64");
        return s ?? "";
    }

    private static string ParseStr(XmlElement configItem, ModS mod, string configName,
        in ConfigParseContext context)
    {
        var s = ConfigParseHelper.GetXmlFieldValue(configItem, "Str");
        return s ?? "";
    }

    private static LabelS ParseLabelS(XmlElement configItem, ModS mod, string configName,
        in ConfigParseContext context)
    {
        try
        {
            var s = ConfigParseHelper.GetXmlFieldValue(configItem, "LabelS");
            if (string.IsNullOrEmpty(s)) return default;
            if (!ConfigParseHelper.TryParseLabelSString(s, "LabelS", out var modName, out var labelName))
                return default;
            return new LabelS { ModName = modName, LabelName = labelName };
        }
        catch (Exception ex)
        {
            if (ConfigParseHelper.IsStrictMode(context))
                ConfigParseHelper.LogParseError(context, "LabelS", ex);
            else
                ConfigParseHelper.LogParseWarning("LabelS",
                    ConfigParseHelper.GetXmlFieldValue(configItem, "LabelS"), ex);
            return default;
        }
    }

    #endregion

    protected override void AllocContainerWithoutFillImpl(
        IXConfig value,
        TblI tbli,
        CfgI cfgi,
        System.Collections.Concurrent.ConcurrentDictionary<TblS, System.Collections.Concurrent.ConcurrentDictionary<CfgS, IXConfig>> allData,
        XM.ConfigDataCenter.ConfigDataHolder configHolderData)
    {
        var config = (NestedConfig)value;
        // 在最外层获取 unmanaged 数据的值（不是引用）
        var map = configHolderData.Data.GetMap<CfgI, NestedConfigUnManaged>(_definedInMod);
        if (!map.TryGetValue(configHolderData.Data.BlobContainer, cfgi, out var data))
        {
            return;
        }

        AllocTestKeyList(config, ref data, cfgi, configHolderData);

        // 将修改后的 unmanaged 数据写回容器
        map[configHolderData.Data.BlobContainer, cfgi] = data;
    }

    #region 容器分配辅助方法

    private void AllocTestKeyList(
        NestedConfig config,
        ref NestedConfigUnManaged data,
        CfgI cfgi,
        XM.ConfigDataCenter.ConfigDataHolder configHolderData)
    {
        if (config.TestKeyList != null && config.TestKeyList.Count > 0)
        {
            var allocated = configHolderData.Data.BlobContainer.AllocArray<CfgI<TestConfigUnManaged>>(config.TestKeyList.Count);
            data.TestKeyList = allocated;
        }
    }

    #endregion

    public override void FillBasicDataImpl(XM.ConfigDataCenter.ConfigDataHolder configHolderData, CfgS key, IXConfig value, XBlobMap<CfgI, NestedConfigUnManaged> tableMap)
    {
        // TODO: 实现基础数据填充逻辑
    }

    private TblI _definedInMod;
}

