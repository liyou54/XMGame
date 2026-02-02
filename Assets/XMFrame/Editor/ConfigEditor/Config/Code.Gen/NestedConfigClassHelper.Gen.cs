using System;
using System.Collections.Generic;
using System.Xml;
using Unity.Collections;
using Unity.Mathematics;
using XM;
using XM.Contracts;
using XM.Contracts.Config;


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
    }

    public override TblS GetTblS()
    {
        return TblS;
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
        config.TestKeyList = ParseTestKeyList(configItem, mod, configName, context);
        config.StrIndex = ParseStrIndex(configItem, mod, configName, context);
        config.Str32 = ParseStr32(configItem, mod, configName, context);
        config.Str64 = ParseStr64(configItem, mod, configName, context);
        config.Str = ParseStr(configItem, mod, configName, context);
        config.LabelS = ParseLabelS(configItem, mod, configName, context);
    }

    public override Type GetLinkHelperType()
    {
        return null;
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

    public override void AllocContainerWithFillImpl(
        IXConfig value,
        TblI tbli,
        CfgI cfgi,
        ref NestedConfigUnManaged data,
        XM.ConfigDataCenter.ConfigDataHolder configHolderData,
        XBlobPtr? linkParent = null)
    {
        var config = (NestedConfig)value;
        AllocTestKeyList(config, ref data, cfgi, configHolderData);

        // 填充基本类型和引用类型字段
        data.RequiredId = config.RequiredId;
        data.OptionalWithDefault = ConvertToStrI(config.OptionalWithDefault);
        data.Test = config.Test;
        data.TestCustom = config.TestCustom;
        data.TestGlobalConvert = config.TestGlobalConvert;
        data.StrIndex = ConvertToLabelI(config.StrIndex);
        data.Str32 = ConvertToFixedString32(config.Str32);
        data.Str64 = ConvertToFixedString64(config.Str64);
        data.Str = ConvertToStrI(config.Str);
        data.LabelS = ConvertToLabelI(config.LabelS);
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

            // 填充数据
            for (int i0 = 0; i0 < config.TestKeyList.Count; i0++)
            {
                if (IConfigDataCenter.I.TryGetCfgI(config.TestKeyList[i0].AsNonGeneric(), out var cfgI0))
                {
                    allocated[configHolderData.Data.BlobContainer, i0] = cfgI0.As<TestConfigUnManaged>();
                }
            }
        }
    }

    #endregion

    private TblI _definedInMod;
}

