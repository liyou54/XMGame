using System;
using System.Collections.Generic;
using System.Xml;
using XM;
using XM.Contracts;
using XM.Contracts.Config;
using XM.Utils;


/// <summary>
/// TestConfig 的配置加载辅助类，用于从 XML 反序列化（静态代码生成，无反射）。
/// </summary>
public sealed class TestConfigClassHelper : ConfigClassHelper<TestConfig, TestConfigUnManaged>
{
    public static TblI TblI { get; private set; }
    public static TblS TblS { get; private set; }

    static TestConfigClassHelper()
    {
        const string __tableName = "TestConfig";
        CfgS<TestConfigUnManaged>.TableName = __tableName;
        TblS = new TblS(new ModS("Default"), __tableName);
    }

    public TestConfigClassHelper(IConfigDataCenter dataCenter)
        : base(dataCenter)
    {
    }

    public static ModI DefinedInMod => TblI.Mod;

    public override TblS GetTblS()
    {
        return TblS;
    }

    public override void SetTblIDefinedInMod(TblI c)
    {
        TblI = c;
    }

    public override IXConfig DeserializeConfigFromXml(XmlElement configItem, ModS mod, string configName)
    {
        return DeserializeConfigFromXml(configItem, mod, configName, OverrideMode.None);
    }

    public override IXConfig DeserializeConfigFromXml(XmlElement configItem, ModS mod, string configName, OverrideMode overrideMode)
    {
        var config = (TestConfig)Create();
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
        var config = (TestConfig)target;
        config.Id = ParseId(configItem, mod, configName);
        config.TestInt = ParseTestInt(configItem, mod, configName);
        config.TestSample = ParseTestSample(configItem, mod, configName);
        config.TestDictSample = ParseTestDictSample(configItem, mod, configName);
        config.TestKeyList = ParseTestKeyList(configItem, mod, configName);
        config.TestKeyList1 = ParseTestKeyList1(configItem, mod, configName);
        config.TestKeyHashSet = ParseTestKeyHashSet(configItem, mod, configName);
        config.TestKeyDict = ParseTestKeyDict(configItem, mod, configName);
        config.TestSetKey = ParseTestSetKey(configItem, mod, configName);
        config.TestSetSample = ParseTestSetSample(configItem, mod, configName);
        config.TestNested = ParseTestNested(configItem, mod, configName);
        config.TestNestedConfig = ParseTestNestedConfig(configItem, mod, configName);
        config.Foreign = ParseForeign(configItem, mod, configName);
        config.TestIndex1 = ParseTestIndex1(configItem, mod, configName);
        config.TestIndex2 = ParseTestIndex2(configItem, mod, configName);
        config.TestIndex3 = ParseTestIndex3(configItem, mod, configName);
    }

    #region 字段解析 (ParseXXX)

    private static CfgS<TestConfigUnManaged> ParseId(XmlElement configItem, ModS mod, string configName)
        {
            try
            {
                var s = ConfigClassHelper.GetXmlFieldValue(configItem, "Id");
            if (string.IsNullOrEmpty(s)) return default;
            if (!ConfigClassHelper.TryParseCfgSString(s, "Id", out var modName, out var cfgName)) return default;
            return new CfgS<TestConfigUnManaged>(new ModS(modName), cfgName);
            }
            catch (Exception ex)
            {
                if (ConfigClassHelper.IsStrictMode) ConfigClassHelper.LogParseError(ConfigClassHelper.CurrentParseContext.FilePath, ConfigClassHelper.CurrentParseContext.Line, "Id", ex); else ConfigClassHelper.LogParseWarning("Id", ConfigClassHelper.GetXmlFieldValue(configItem, "Id"), ex);
                return default;
            }
        }

    private static int ParseTestInt(XmlElement configItem, ModS mod, string configName)
        {
            var s = ConfigClassHelper.GetXmlFieldValue(configItem, "TestInt");
            if (string.IsNullOrEmpty(s)) return default;
            return ConfigClassHelper.TryParseInt(s, "TestInt", out var v) ? v : default;
        }

    private static List<int> ParseTestSample(XmlElement configItem, ModS mod, string configName)
        {
            try
            {
                var list = new List<int>();
            var nodes = configItem.SelectNodes("TestSample");
            if (nodes != null)
            foreach (System.Xml.XmlNode n in nodes) { var t = (n as System.Xml.XmlElement)?.InnerText?.Trim(); if (!string.IsNullOrEmpty(t) && ConfigClassHelper.TryParseInt(t, "TestSample", out var vi)) list.Add(vi); }
            if (list.Count == 0) { var csv = ConfigClassHelper.GetXmlFieldValue(configItem, "TestSample"); if (!string.IsNullOrEmpty(csv)) foreach (var p in csv.Split(',', ';')) if (!string.IsNullOrWhiteSpace(p) && ConfigClassHelper.TryParseInt(p.Trim(), "TestSample", out var vi)) list.Add(vi); }
            return list;
            }
            catch (Exception ex)
            {
                if (ConfigClassHelper.IsStrictMode) ConfigClassHelper.LogParseError(ConfigClassHelper.CurrentParseContext.FilePath, ConfigClassHelper.CurrentParseContext.Line, "TestSample", ex); else ConfigClassHelper.LogParseWarning("TestSample", null, ex);
                return new List<int>();
            }
        }

    private static Dictionary<int, int> ParseTestDictSample(XmlElement configItem, ModS mod, string configName)
        {
            try
            {
                var dict = new Dictionary<int, int>();
            var dictNodes = configItem.SelectNodes("TestDictSample/Item");
            if (dictNodes != null)
            foreach (System.Xml.XmlNode n in dictNodes) { var el = n as System.Xml.XmlElement; if (el == null) continue; var k = el.GetAttribute("Key"); var v = el.InnerText?.Trim(); if (!string.IsNullOrEmpty(k) && !string.IsNullOrEmpty(v) && ConfigClassHelper.TryParseInt(k, "TestDictSample.Key", out var kv) && ConfigClassHelper.TryParseInt(v, "TestDictSample.Value", out var vv)) dict[kv] = vv; }
            return dict;
            }
            catch (Exception ex)
            {
                if (ConfigClassHelper.IsStrictMode) ConfigClassHelper.LogParseError(ConfigClassHelper.CurrentParseContext.FilePath, ConfigClassHelper.CurrentParseContext.Line, "TestDictSample", ex); else ConfigClassHelper.LogParseWarning("TestDictSample", null, ex);
                return new Dictionary<int, int>();
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

    private static Dictionary<int, List<List<CfgS<TestConfigUnManaged>>>> ParseTestKeyList1(XmlElement configItem, ModS mod, string configName)
        {
            try
            {
                var dict = new Dictionary<int, List<List<CfgS<TestConfigUnManaged>>>>();
            var dictNodes = configItem.SelectNodes("TestKeyList1/Item");
            if (dictNodes != null)
            foreach (System.Xml.XmlNode keyNode in dictNodes) { var keyEl = keyNode as System.Xml.XmlElement; if (keyEl == null) continue; var kStr = keyEl.GetAttribute("Key"); if (!string.IsNullOrEmpty(kStr) && ConfigClassHelper.TryParseInt(kStr, "TestKeyList1.Key", out var key)) { var outerList = new List<List<CfgS<TestConfigUnManaged>>>(); var midNodes = keyEl.SelectNodes("Item"); if (midNodes != null) foreach (System.Xml.XmlNode midNode in midNodes) { var midEl = midNode as System.Xml.XmlElement; if (midEl == null) continue; var innerList = new List<CfgS<TestConfigUnManaged>>(); var leafNodes = midEl.SelectNodes("Item"); if (leafNodes != null) foreach (System.Xml.XmlNode leafNode in leafNodes) { var leafText = (leafNode as System.Xml.XmlElement)?.InnerText?.Trim(); if (!string.IsNullOrEmpty(leafText) && ConfigClassHelper.TryParseCfgSString(leafText, "TestKeyList1", out var lm, out var lc)) innerList.Add(new CfgS<TestConfigUnManaged>(new ModS(lm), lc)); } outerList.Add(innerList); } dict[key] = outerList; } }
            return dict;
            }
            catch (Exception ex)
            {
                if (ConfigClassHelper.IsStrictMode) ConfigClassHelper.LogParseError(ConfigClassHelper.CurrentParseContext.FilePath, ConfigClassHelper.CurrentParseContext.Line, "TestKeyList1", ex); else ConfigClassHelper.LogParseWarning("TestKeyList1", null, ex);
                return new Dictionary<int, List<List<CfgS<TestConfigUnManaged>>>>();
            }
        }

    private static HashSet<int> ParseTestKeyHashSet(XmlElement configItem, ModS mod, string configName)
        {
            try
            {
                var set = new HashSet<int>();
            var nodes = configItem.SelectNodes("TestKeyHashSet");
            if (nodes != null)
            foreach (System.Xml.XmlNode n in nodes) { var t = (n as System.Xml.XmlElement)?.InnerText?.Trim(); if (!string.IsNullOrEmpty(t) && ConfigClassHelper.TryParseInt(t, "TestKeyHashSet", out var vi)) set.Add(vi); }
            if (set.Count == 0) { var csv = ConfigClassHelper.GetXmlFieldValue(configItem, "TestKeyHashSet"); if (!string.IsNullOrEmpty(csv)) foreach (var p in csv.Split(',', ';')) if (!string.IsNullOrWhiteSpace(p) && ConfigClassHelper.TryParseInt(p.Trim(), "TestKeyHashSet", out var vi)) set.Add(vi); }
            return set;
            }
            catch (Exception ex)
            {
                if (ConfigClassHelper.IsStrictMode) ConfigClassHelper.LogParseError(ConfigClassHelper.CurrentParseContext.FilePath, ConfigClassHelper.CurrentParseContext.Line, "TestKeyHashSet", ex); else ConfigClassHelper.LogParseWarning("TestKeyHashSet", ConfigClassHelper.GetXmlFieldValue(configItem, "TestKeyHashSet"), ex);
                return new HashSet<int>();
            }
        }

    private static Dictionary<CfgS<TestConfigUnManaged>, CfgS<TestConfigUnManaged>> ParseTestKeyDict(XmlElement configItem, ModS mod, string configName)
        {
            try
            {
                var dict = new Dictionary<CfgS<TestConfigUnManaged>, CfgS<TestConfigUnManaged>>();
            var dictNodes = configItem.SelectNodes("TestKeyDict/Item");
            if (dictNodes != null)
            foreach (System.Xml.XmlNode n in dictNodes) { var el = n as System.Xml.XmlElement; if (el == null) continue; var kStr = el.GetAttribute("Key") ?? (el.SelectSingleNode("Key") as System.Xml.XmlElement)?.InnerText?.Trim(); var vStr = el.GetAttribute("Value") ?? (el.SelectSingleNode("Value") as System.Xml.XmlElement)?.InnerText?.Trim() ?? el.InnerText?.Trim(); if (!string.IsNullOrEmpty(kStr) && ConfigClassHelper.TryParseCfgSString(kStr, "TestKeyDict.Key", out var km, out var kc) && !string.IsNullOrEmpty(vStr) && ConfigClassHelper.TryParseCfgSString(vStr, "TestKeyDict.Value", out var vm, out var vc)) dict[new CfgS<TestConfigUnManaged>(new ModS(km), kc)] = new CfgS<TestConfigUnManaged>(new ModS(vm), vc); }
            return dict;
            }
            catch (Exception ex)
            {
                if (ConfigClassHelper.IsStrictMode) ConfigClassHelper.LogParseError(ConfigClassHelper.CurrentParseContext.FilePath, ConfigClassHelper.CurrentParseContext.Line, "TestKeyDict", ex); else ConfigClassHelper.LogParseWarning("TestKeyDict", null, ex);
                return new Dictionary<CfgS<TestConfigUnManaged>, CfgS<TestConfigUnManaged>>();
            }
        }

    private static HashSet<CfgS<TestConfigUnManaged>> ParseTestSetKey(XmlElement configItem, ModS mod, string configName)
        {
            try
            {
                var set = new HashSet<CfgS<TestConfigUnManaged>>();
            var nodes = configItem.SelectNodes("TestSetKey");
            if (nodes != null)
            foreach (System.Xml.XmlNode n in nodes) { var t = (n as System.Xml.XmlElement)?.InnerText?.Trim(); if (!string.IsNullOrEmpty(t) && ConfigClassHelper.TryParseCfgSString(t, "TestSetKey", out var mn, out var cn)) set.Add(new CfgS<TestConfigUnManaged>(new ModS(mn), cn)); }
            return set;
            }
            catch (Exception ex)
            {
                if (ConfigClassHelper.IsStrictMode) ConfigClassHelper.LogParseError(ConfigClassHelper.CurrentParseContext.FilePath, ConfigClassHelper.CurrentParseContext.Line, "TestSetKey", ex); else ConfigClassHelper.LogParseWarning("TestSetKey", ConfigClassHelper.GetXmlFieldValue(configItem, "TestSetKey"), ex);
                return new HashSet<CfgS<TestConfigUnManaged>>();
            }
        }

    private static HashSet<int> ParseTestSetSample(XmlElement configItem, ModS mod, string configName)
        {
            try
            {
                var set = new HashSet<int>();
            var nodes = configItem.SelectNodes("TestSetSample");
            if (nodes != null)
            foreach (System.Xml.XmlNode n in nodes) { var t = (n as System.Xml.XmlElement)?.InnerText?.Trim(); if (!string.IsNullOrEmpty(t) && ConfigClassHelper.TryParseInt(t, "TestSetSample", out var vi)) set.Add(vi); }
            if (set.Count == 0) { var csv = ConfigClassHelper.GetXmlFieldValue(configItem, "TestSetSample"); if (!string.IsNullOrEmpty(csv)) foreach (var p in csv.Split(',', ';')) if (!string.IsNullOrWhiteSpace(p) && ConfigClassHelper.TryParseInt(p.Trim(), "TestSetSample", out var vi)) set.Add(vi); }
            return set;
            }
            catch (Exception ex)
            {
                if (ConfigClassHelper.IsStrictMode) ConfigClassHelper.LogParseError(ConfigClassHelper.CurrentParseContext.FilePath, ConfigClassHelper.CurrentParseContext.Line, "TestSetSample", ex); else ConfigClassHelper.LogParseWarning("TestSetSample", ConfigClassHelper.GetXmlFieldValue(configItem, "TestSetSample"), ex);
                return new HashSet<int>();
            }
        }

    private static NestedConfig ParseTestNested(XmlElement configItem, ModS mod, string configName)
        {
            try
            {
                var el = configItem.SelectSingleNode("TestNested") as System.Xml.XmlElement;
            if (el == null) return null;
            var helper = XM.Contracts.IConfigDataCenter.I?.GetClassHelper(typeof(NestedConfig));
            return helper != null ? (NestedConfig)helper.DeserializeConfigFromXml(el, mod, configName + "_TestNested") : null;
            }
            catch (Exception ex)
            {
                if (ConfigClassHelper.IsStrictMode) ConfigClassHelper.LogParseError(ConfigClassHelper.CurrentParseContext.FilePath, ConfigClassHelper.CurrentParseContext.Line, "TestNested", ex); else ConfigClassHelper.LogParseWarning("TestNested", null, ex);
                return null;
            }
        }

    private static List<NestedConfig> ParseTestNestedConfig(XmlElement configItem, ModS mod, string configName)
        {
            try
            {
                var list = new List<NestedConfig>();
            var dc = XM.Contracts.IConfigDataCenter.I; if (dc == null) return list;
            var nodes = configItem.SelectNodes("TestNestedConfig");
            if (nodes != null)
            foreach (System.Xml.XmlNode n in nodes) { var el = n as System.Xml.XmlElement; if (el == null) continue; var helper = dc.GetClassHelper(typeof(NestedConfig)); if (helper != null) { var item = (NestedConfig)helper.DeserializeConfigFromXml(el, mod, configName + "_TestNestedConfig_" + list.Count); if (item != null) list.Add(item); } }
            return list;
            }
            catch (Exception ex)
            {
                if (ConfigClassHelper.IsStrictMode) ConfigClassHelper.LogParseError(ConfigClassHelper.CurrentParseContext.FilePath, ConfigClassHelper.CurrentParseContext.Line, "TestNestedConfig", ex); else ConfigClassHelper.LogParseWarning("TestNestedConfig", null, ex);
                return new List<NestedConfig>();
            }
        }

    private static CfgS<TestConfigUnManaged> ParseForeign(XmlElement configItem, ModS mod, string configName)
        {
            try
            {
                var s = ConfigClassHelper.GetXmlFieldValue(configItem, "Foreign");
            if (string.IsNullOrEmpty(s)) return default;
            if (!ConfigClassHelper.TryParseCfgSString(s, "Foreign", out var modName, out var cfgName)) return default;
            return new CfgS<TestConfigUnManaged>(new ModS(modName), cfgName);
            }
            catch (Exception ex)
            {
                if (ConfigClassHelper.IsStrictMode) ConfigClassHelper.LogParseError(ConfigClassHelper.CurrentParseContext.FilePath, ConfigClassHelper.CurrentParseContext.Line, "Foreign", ex); else ConfigClassHelper.LogParseWarning("Foreign", ConfigClassHelper.GetXmlFieldValue(configItem, "Foreign"), ex);
                return default;
            }
        }

    private static int ParseTestIndex1(XmlElement configItem, ModS mod, string configName)
        {
            var s = ConfigClassHelper.GetXmlFieldValue(configItem, "TestIndex1");
            if (string.IsNullOrEmpty(s)) return default;
            return ConfigClassHelper.TryParseInt(s, "TestIndex1", out var v) ? v : default;
        }

    private static CfgS<TestConfigUnManaged> ParseTestIndex2(XmlElement configItem, ModS mod, string configName)
        {
            try
            {
                var s = ConfigClassHelper.GetXmlFieldValue(configItem, "TestIndex2");
            if (string.IsNullOrEmpty(s)) return default;
            if (!ConfigClassHelper.TryParseCfgSString(s, "TestIndex2", out var modName, out var cfgName)) return default;
            return new CfgS<TestConfigUnManaged>(new ModS(modName), cfgName);
            }
            catch (Exception ex)
            {
                if (ConfigClassHelper.IsStrictMode) ConfigClassHelper.LogParseError(ConfigClassHelper.CurrentParseContext.FilePath, ConfigClassHelper.CurrentParseContext.Line, "TestIndex2", ex); else ConfigClassHelper.LogParseWarning("TestIndex2", ConfigClassHelper.GetXmlFieldValue(configItem, "TestIndex2"), ex);
                return default;
            }
        }

    private static CfgS<TestConfigUnManaged> ParseTestIndex3(XmlElement configItem, ModS mod, string configName)
        {
            try
            {
                var s = ConfigClassHelper.GetXmlFieldValue(configItem, "TestIndex3");
            if (string.IsNullOrEmpty(s)) return default;
            if (!ConfigClassHelper.TryParseCfgSString(s, "TestIndex3", out var modName, out var cfgName)) return default;
            return new CfgS<TestConfigUnManaged>(new ModS(modName), cfgName);
            }
            catch (Exception ex)
            {
                if (ConfigClassHelper.IsStrictMode) ConfigClassHelper.LogParseError(ConfigClassHelper.CurrentParseContext.FilePath, ConfigClassHelper.CurrentParseContext.Line, "TestIndex3", ex); else ConfigClassHelper.LogParseWarning("TestIndex3", ConfigClassHelper.GetXmlFieldValue(configItem, "TestIndex3"), ex);
                return default;
            }
        }

    #endregion

}

