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
    static NestedConfigClassHelper()
    {
        CfgS<NestedConfigUnManaged>.TableName = "NestedConfig";
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

    public override void SetTblIDefinedInMod(ModI modHandle)
    {
        _definedInMod = modHandle;
    }

    public override IXConfig DeserializeConfigFromXml(XmlElement configItem, ModS mod, string configName)
    {
        return DeserializeConfigFromXml(configItem, mod, configName, OverrideMode.None);
    }

    public override IXConfig DeserializeConfigFromXml(XmlElement configItem, ModS mod, string configName, OverrideMode overrideMode)
    {
        var config = (NestedConfig)Create();
        try
        {
            FillFromXml(config, configItem, mod, configName);
        }
        catch (Exception ex)
        {
            if (overrideMode == OverrideMode.None || overrideMode == OverrideMode.ReWrite)
                ConfigClassHelper.LogParseError(ConfigClassHelper.CurrentParseContext.FilePath, ConfigClassHelper.CurrentParseContext.Line, "(整体)", ex);
            else
                ConfigClassHelper.LogParseWarning("(整体)", configName, ex);
        }
        return config;
    }

    public override void FillFromXml(IXConfig target, XmlElement configItem, ModS mod, string configName)
    {
        var config = (NestedConfig)target;
        config.RequiredId = ParseRequiredId(configItem, mod, configName);
        config.OptionalWithDefault = ParseOptionalWithDefault(configItem, mod, configName);
        config.Test = ParseTest(configItem, mod, configName);
        config.TestCustom = ParseTestCustom(configItem, mod, configName);
        config.TestGlobalConvert = ParseTestGlobalConvert(configItem, mod, configName);
        config.TestKeyList = ParseTestKeyList(configItem, mod, configName);
        config.StrIndex = ParseStrIndex(configItem, mod, configName);
        config.Str32 = ParseStr32(configItem, mod, configName);
        config.Str64 = ParseStr64(configItem, mod, configName);
        config.Str = ParseStr(configItem, mod, configName);
        config.LabelS = ParseLabelS(configItem, mod, configName);
    }

    #region 字段解析 (ParseXXX)

    private static int ParseRequiredId(XmlElement configItem, ModS mod, string configName)
        {
            var s = ConfigClassHelper.GetXmlFieldValue(configItem, "RequiredId");
            if (string.IsNullOrEmpty(s)) { ConfigClassHelper.LogParseWarning("RequiredId", s ?? "", null); }
            if (string.IsNullOrEmpty(s)) return default;
            return ConfigClassHelper.TryParseInt(s, "RequiredId", out var v) ? v : default;
        }

    private static string ParseOptionalWithDefault(XmlElement configItem, ModS mod, string configName)
        {
            var s = ConfigClassHelper.GetXmlFieldValue(configItem, "OptionalWithDefault");
            if (string.IsNullOrEmpty(s)) { s = "default"; }
            return s ?? "";
        }

    private static int ParseTest(XmlElement configItem, ModS mod, string configName)
        {
            var s = ConfigClassHelper.GetXmlFieldValue(configItem, "Test");
            if (string.IsNullOrEmpty(s)) return default;
            return ConfigClassHelper.TryParseInt(s, "Test", out var v) ? v : default;
        }

    private static int2 ParseTestCustom(XmlElement configItem, ModS mod, string configName)
        {
            try
            {
                var s = ConfigClassHelper.GetXmlFieldValue(configItem, "TestCustom");
            if (string.IsNullOrEmpty(s)) return default;
            var converter = XM.Contracts.IConfigDataCenter.I?.GetConverterByType<string, int2>();
            return converter != null ? converter.Convert(s) : default;
            }
            catch (Exception ex)
            {
                if (ConfigClassHelper.IsStrictMode) ConfigClassHelper.LogParseError(ConfigClassHelper.CurrentParseContext.FilePath, ConfigClassHelper.CurrentParseContext.Line, "TestCustom", ex); else ConfigClassHelper.LogParseWarning("TestCustom", ConfigClassHelper.GetXmlFieldValue(configItem, "TestCustom"), ex);
                return default;
            }
        }

    private static int2 ParseTestGlobalConvert(XmlElement configItem, ModS mod, string configName)
        {
            try
            {
                var s = ConfigClassHelper.GetXmlFieldValue(configItem, "TestGlobalConvert");
            if (string.IsNullOrEmpty(s)) return default;
            var converter = XM.Contracts.IConfigDataCenter.I?.GetConverter<string, int2>("global");
            return converter != null ? converter.Convert(s) : default;
            }
            catch (Exception ex)
            {
                if (ConfigClassHelper.IsStrictMode) ConfigClassHelper.LogParseError(ConfigClassHelper.CurrentParseContext.FilePath, ConfigClassHelper.CurrentParseContext.Line, "TestGlobalConvert", ex); else ConfigClassHelper.LogParseWarning("TestGlobalConvert", ConfigClassHelper.GetXmlFieldValue(configItem, "TestGlobalConvert"), ex);
                return default;
            }
        }

    private static List<CfgS<TestConfigUnManaged>> ParseTestKeyList(XmlElement configItem, ModS mod, string configName)
        {
            try
            {
                var list = new List<CfgS<TestConfigUnManaged>>();
            var nodes = configItem.SelectNodes("TestKeyList");
            if (nodes != null)
            foreach (System.Xml.XmlNode n in nodes) { var t = (n as System.Xml.XmlElement)?.InnerText?.Trim(); if (!string.IsNullOrEmpty(t) && ConfigClassHelper.TryParseCfgSString(t, "TestKeyList", out var mn, out var cn)) list.Add(new CfgS<TestConfigUnManaged>(new ModS(mn), cn)); }
            return list;
            }
            catch (Exception ex)
            {
                if (ConfigClassHelper.IsStrictMode) ConfigClassHelper.LogParseError(ConfigClassHelper.CurrentParseContext.FilePath, ConfigClassHelper.CurrentParseContext.Line, "TestKeyList", ex); else ConfigClassHelper.LogParseWarning("TestKeyList", null, ex);
                return new List<CfgS<TestConfigUnManaged>>();
            }
        }

    private static string ParseStrIndex(XmlElement configItem, ModS mod, string configName)
        {
            var s = ConfigClassHelper.GetXmlFieldValue(configItem, "StrIndex");
            return s ?? "";
        }

    private static string ParseStr32(XmlElement configItem, ModS mod, string configName)
        {
            var s = ConfigClassHelper.GetXmlFieldValue(configItem, "Str32");
            return s ?? "";
        }

    private static string ParseStr64(XmlElement configItem, ModS mod, string configName)
        {
            var s = ConfigClassHelper.GetXmlFieldValue(configItem, "Str64");
            return s ?? "";
        }

    private static string ParseStr(XmlElement configItem, ModS mod, string configName)
        {
            var s = ConfigClassHelper.GetXmlFieldValue(configItem, "Str");
            return s ?? "";
        }

    private static LabelS ParseLabelS(XmlElement configItem, ModS mod, string configName)
        {
            try
            {
                var s = ConfigClassHelper.GetXmlFieldValue(configItem, "LabelS");
            if (string.IsNullOrEmpty(s)) return default;
            if (!ConfigClassHelper.TryParseLabelSString(s, "LabelS", out var modName, out var labelName)) return default;
            return new LabelS { ModName = modName, LabelName = labelName };
            }
            catch (Exception ex)
            {
                if (ConfigClassHelper.IsStrictMode) ConfigClassHelper.LogParseError(ConfigClassHelper.CurrentParseContext.FilePath, ConfigClassHelper.CurrentParseContext.Line, "LabelS", ex); else ConfigClassHelper.LogParseWarning("LabelS", ConfigClassHelper.GetXmlFieldValue(configItem, "LabelS"), ex);
                return default;
            }
        }

    #endregion

    private ModI _definedInMod;
}

