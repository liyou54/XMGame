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
        const string __modName = "Default";
        CfgS<TestConfigUnManaged>.Table = new TblS(new ModS(__modName), __tableName);
        TblS = new TblS(new ModS(__modName), __tableName);
    }

    public TestConfigClassHelper(IConfigDataCenter dataCenter)
        : base(dataCenter)
    {
    }

    public override TblS GetTblS()
    {
        return TblS;
    }

    /// <summary>由 TblI 分配时一并确定，无需单独字段。</summary>
    public static ModI DefinedInMod => TblI.DefinedMod;

    public override void SetTblIDefinedInMod(TblI c)
    {
        TblI = c;
    }

    public override IXConfig DeserializeConfigFromXml(XmlElement configItem, ModS mod, string configName)
    {
        return DeserializeConfigFromXml(configItem, mod, configName, default);
    }

    public override void ParseAndFillFromXml(IXConfig target, XmlElement configItem, ModS mod, string configName)
    {
        ParseAndFillFromXml(target, configItem, mod, configName, default);
    }

    public override void ParseAndFillFromXml(IXConfig target, XmlElement configItem, ModS mod, string configName, in ConfigParseContext context)
    {
        var config = (TestConfig)target;
        config.Id = ParseId(configItem, mod, configName, context);
        config.TestInt = ParseTestInt(configItem, mod, configName, context);
        config.TestSample = ParseTestSample(configItem, mod, configName, context);
        config.TestDictSample = ParseTestDictSample(configItem, mod, configName, context);
        config.TestKeyList = ParseTestKeyList(configItem, mod, configName, context);
        config.TestKeyList1 = ParseTestKeyList1(configItem, mod, configName, context);
        config.TestKeyHashSet = ParseTestKeyHashSet(configItem, mod, configName, context);
        config.TestKeyDict = ParseTestKeyDict(configItem, mod, configName, context);
        config.TestSetKey = ParseTestSetKey(configItem, mod, configName, context);
        config.TestSetSample = ParseTestSetSample(configItem, mod, configName, context);
        config.TestNested = ParseTestNested(configItem, mod, configName, context);
        config.TestNestedConfig = ParseTestNestedConfig(configItem, mod, configName, context);
        config.Foreign = ParseForeign(configItem, mod, configName, context);
        config.TestIndex1 = ParseTestIndex1(configItem, mod, configName, context);
        config.TestIndex2 = ParseTestIndex2(configItem, mod, configName, context);
        config.TestIndex3 = ParseTestIndex3(configItem, mod, configName, context);
    }

    #region 字段解析 (ParseXXX)

    private static CfgS<TestConfigUnManaged> ParseId(XmlElement configItem, ModS mod, string configName, in ConfigParseContext context)
        {
            try
            {
                var s = ConfigParseHelper.GetXmlFieldValue(configItem, "Id");
            if (string.IsNullOrEmpty(s)) return default;
            if (!ConfigParseHelper.TryParseCfgSString(s, "Id", out var modName, out var cfgName)) return default;
            return new CfgS<TestConfigUnManaged>(new ModS(modName), cfgName);
            }
            catch (Exception ex)
            {
                if (ConfigParseHelper.IsStrictMode(context)) ConfigParseHelper.LogParseError(context, "Id", ex); else ConfigParseHelper.LogParseWarning("Id", ConfigParseHelper.GetXmlFieldValue(configItem, "Id"), ex);
                return default;
            }
        }

    private static int ParseTestInt(XmlElement configItem, ModS mod, string configName, in ConfigParseContext context)
        {
            var s = ConfigParseHelper.GetXmlFieldValue(configItem, "TestInt");
            if (string.IsNullOrEmpty(s)) return default;
            return ConfigParseHelper.TryParseInt(s, "TestInt", out var v) ? v : default;
        }

    private static List<int> ParseTestSample(XmlElement configItem, ModS mod, string configName, in ConfigParseContext context)
        {
            try
            {
                var list = new List<int>();
            var nodes = configItem.SelectNodes("TestSample");
            if (nodes != null)
            foreach (System.Xml.XmlNode n in nodes) { var t = (n as System.Xml.XmlElement)?.InnerText?.Trim(); if (!string.IsNullOrEmpty(t) && ConfigParseHelper.TryParseInt(t, "TestSample", out var vi)) list.Add(vi); }
            if (list.Count == 0) { var csv = ConfigParseHelper.GetXmlFieldValue(configItem, "TestSample"); if (!string.IsNullOrEmpty(csv)) foreach (var p in csv.Split(',', ';')) if (!string.IsNullOrWhiteSpace(p) && ConfigParseHelper.TryParseInt(p.Trim(), "TestSample", out var vi)) list.Add(vi); }
            return list;
            }
            catch (Exception ex)
            {
                if (ConfigParseHelper.IsStrictMode(context)) ConfigParseHelper.LogParseError(context, "TestSample", ex); else ConfigParseHelper.LogParseWarning("TestSample", null, ex);
                return new List<int>();
            }
        }

    private static Dictionary<int, int> ParseTestDictSample(XmlElement configItem, ModS mod, string configName, in ConfigParseContext context)
        {
            try
            {
                var dict = new Dictionary<int, int>();
            var dictNodes = configItem.SelectNodes("TestDictSample/Item");
            if (dictNodes != null)
            foreach (System.Xml.XmlNode n in dictNodes) { var el = n as System.Xml.XmlElement; if (el == null) continue; var k = el.GetAttribute("Key"); var v = el.InnerText?.Trim(); if (!string.IsNullOrEmpty(k) && !string.IsNullOrEmpty(v) && ConfigParseHelper.TryParseInt(k, "TestDictSample.Key", out var kv) && ConfigParseHelper.TryParseInt(v, "TestDictSample.Value", out var vv)) dict[kv] = vv; }
            return dict;
            }
            catch (Exception ex)
            {
                if (ConfigParseHelper.IsStrictMode(context)) ConfigParseHelper.LogParseError(context, "TestDictSample", ex); else ConfigParseHelper.LogParseWarning("TestDictSample", null, ex);
                return new Dictionary<int, int>();
            }
        }

    private static List<CfgS<TestConfigUnManaged>> ParseTestKeyList(XmlElement configItem, ModS mod, string configName, in ConfigParseContext context)
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
                if (ConfigParseHelper.IsStrictMode(context)) ConfigParseHelper.LogParseError(context, "TestKeyList", ex); else ConfigParseHelper.LogParseWarning("TestKeyList", null, ex);
                return new List<CfgS<TestConfigUnManaged>>();
            }
        }

    private static Dictionary<int, List<List<CfgS<TestConfigUnManaged>>>> ParseTestKeyList1(XmlElement configItem, ModS mod, string configName, in ConfigParseContext context)
        {
            try
            {
                var dict = new Dictionary<int, List<List<CfgS<TestConfigUnManaged>>>>();
            var dictNodes = configItem.SelectNodes("TestKeyList1/Item");
            if (dictNodes != null)
            foreach (System.Xml.XmlNode keyNode in dictNodes) { var keyEl = keyNode as System.Xml.XmlElement; if (keyEl == null) continue; var kStr = keyEl.GetAttribute("Key"); if (!string.IsNullOrEmpty(kStr) && ConfigParseHelper.TryParseInt(kStr, "TestKeyList1.Key", out var key)) { var outerList = new List<List<CfgS<TestConfigUnManaged>>>(); var midNodes = keyEl.SelectNodes("Item"); if (midNodes != null) foreach (System.Xml.XmlNode midNode in midNodes) { var midEl = midNode as System.Xml.XmlElement; if (midEl == null) continue; var innerList = new List<CfgS<TestConfigUnManaged>>(); var leafNodes = midEl.SelectNodes("Item"); if (leafNodes != null) foreach (System.Xml.XmlNode leafNode in leafNodes) { var leafText = (leafNode as System.Xml.XmlElement)?.InnerText?.Trim(); if (!string.IsNullOrEmpty(leafText) && ConfigParseHelper.TryParseCfgSString(leafText, "TestKeyList1", out var lm, out var lc)) innerList.Add(new CfgS<TestConfigUnManaged>(new ModS(lm), lc)); } outerList.Add(innerList); } dict[key] = outerList; } }
            return dict;
            }
            catch (Exception ex)
            {
                if (ConfigParseHelper.IsStrictMode(context)) ConfigParseHelper.LogParseError(context, "TestKeyList1", ex); else ConfigParseHelper.LogParseWarning("TestKeyList1", null, ex);
                return new Dictionary<int, List<List<CfgS<TestConfigUnManaged>>>>();
            }
        }

    private static HashSet<int> ParseTestKeyHashSet(XmlElement configItem, ModS mod, string configName, in ConfigParseContext context)
        {
            try
            {
                var set = new HashSet<int>();
            var nodes = configItem.SelectNodes("TestKeyHashSet");
            if (nodes != null)
            foreach (System.Xml.XmlNode n in nodes) { var t = (n as System.Xml.XmlElement)?.InnerText?.Trim(); if (!string.IsNullOrEmpty(t) && ConfigParseHelper.TryParseInt(t, "TestKeyHashSet", out var vi)) set.Add(vi); }
            if (set.Count == 0) { var csv = ConfigParseHelper.GetXmlFieldValue(configItem, "TestKeyHashSet"); if (!string.IsNullOrEmpty(csv)) foreach (var p in csv.Split(',', ';')) if (!string.IsNullOrWhiteSpace(p) && ConfigParseHelper.TryParseInt(p.Trim(), "TestKeyHashSet", out var vi)) set.Add(vi); }
            return set;
            }
            catch (Exception ex)
            {
                if (ConfigParseHelper.IsStrictMode(context)) ConfigParseHelper.LogParseError(context, "TestKeyHashSet", ex); else ConfigParseHelper.LogParseWarning("TestKeyHashSet", ConfigParseHelper.GetXmlFieldValue(configItem, "TestKeyHashSet"), ex);
                return new HashSet<int>();
            }
        }

    private static Dictionary<CfgS<TestConfigUnManaged>, CfgS<TestConfigUnManaged>> ParseTestKeyDict(XmlElement configItem, ModS mod, string configName, in ConfigParseContext context)
        {
            try
            {
                var dict = new Dictionary<CfgS<TestConfigUnManaged>, CfgS<TestConfigUnManaged>>();
            var dictNodes = configItem.SelectNodes("TestKeyDict/Item");
            if (dictNodes != null)
            foreach (System.Xml.XmlNode n in dictNodes) { var el = n as System.Xml.XmlElement; if (el == null) continue; var kStr = el.GetAttribute("Key") ?? (el.SelectSingleNode("Key") as System.Xml.XmlElement)?.InnerText?.Trim(); var vStr = el.GetAttribute("Value") ?? (el.SelectSingleNode("Value") as System.Xml.XmlElement)?.InnerText?.Trim() ?? el.InnerText?.Trim(); if (!string.IsNullOrEmpty(kStr) && ConfigParseHelper.TryParseCfgSString(kStr, "TestKeyDict.Key", out var km, out var kc) && !string.IsNullOrEmpty(vStr) && ConfigParseHelper.TryParseCfgSString(vStr, "TestKeyDict.Value", out var vm, out var vc)) dict[new CfgS<TestConfigUnManaged>(new ModS(km), kc)] = new CfgS<TestConfigUnManaged>(new ModS(vm), vc); }
            return dict;
            }
            catch (Exception ex)
            {
                if (ConfigParseHelper.IsStrictMode(context)) ConfigParseHelper.LogParseError(context, "TestKeyDict", ex); else ConfigParseHelper.LogParseWarning("TestKeyDict", null, ex);
                return new Dictionary<CfgS<TestConfigUnManaged>, CfgS<TestConfigUnManaged>>();
            }
        }

    private static HashSet<CfgS<TestConfigUnManaged>> ParseTestSetKey(XmlElement configItem, ModS mod, string configName, in ConfigParseContext context)
        {
            try
            {
                var set = new HashSet<CfgS<TestConfigUnManaged>>();
            var nodes = configItem.SelectNodes("TestSetKey");
            if (nodes != null)
            foreach (System.Xml.XmlNode n in nodes) { var t = (n as System.Xml.XmlElement)?.InnerText?.Trim(); if (!string.IsNullOrEmpty(t) && ConfigParseHelper.TryParseCfgSString(t, "TestSetKey", out var mn, out var cn)) set.Add(new CfgS<TestConfigUnManaged>(new ModS(mn), cn)); }
            return set;
            }
            catch (Exception ex)
            {
                if (ConfigParseHelper.IsStrictMode(context)) ConfigParseHelper.LogParseError(context, "TestSetKey", ex); else ConfigParseHelper.LogParseWarning("TestSetKey", ConfigParseHelper.GetXmlFieldValue(configItem, "TestSetKey"), ex);
                return new HashSet<CfgS<TestConfigUnManaged>>();
            }
        }

    private static HashSet<int> ParseTestSetSample(XmlElement configItem, ModS mod, string configName, in ConfigParseContext context)
        {
            try
            {
                var set = new HashSet<int>();
            var nodes = configItem.SelectNodes("TestSetSample");
            if (nodes != null)
            foreach (System.Xml.XmlNode n in nodes) { var t = (n as System.Xml.XmlElement)?.InnerText?.Trim(); if (!string.IsNullOrEmpty(t) && ConfigParseHelper.TryParseInt(t, "TestSetSample", out var vi)) set.Add(vi); }
            if (set.Count == 0) { var csv = ConfigParseHelper.GetXmlFieldValue(configItem, "TestSetSample"); if (!string.IsNullOrEmpty(csv)) foreach (var p in csv.Split(',', ';')) if (!string.IsNullOrWhiteSpace(p) && ConfigParseHelper.TryParseInt(p.Trim(), "TestSetSample", out var vi)) set.Add(vi); }
            return set;
            }
            catch (Exception ex)
            {
                if (ConfigParseHelper.IsStrictMode(context)) ConfigParseHelper.LogParseError(context, "TestSetSample", ex); else ConfigParseHelper.LogParseWarning("TestSetSample", ConfigParseHelper.GetXmlFieldValue(configItem, "TestSetSample"), ex);
                return new HashSet<int>();
            }
        }

    private static NestedConfig ParseTestNested(XmlElement configItem, ModS mod, string configName, in ConfigParseContext context)
        {
            try
            {
                var el = configItem.SelectSingleNode("TestNested") as System.Xml.XmlElement;
            if (el == null) return null;
            var helper = XM.Contracts.IConfigDataCenter.I?.GetClassHelper(typeof(NestedConfig));
            return helper != null ? (NestedConfig)helper.DeserializeConfigFromXml(el, mod, configName + "_TestNested", in context) : null;
            }
            catch (Exception ex)
            {
                if (ConfigParseHelper.IsStrictMode(context)) ConfigParseHelper.LogParseError(context, "TestNested", ex); else ConfigParseHelper.LogParseWarning("TestNested", null, ex);
                return null;
            }
        }

    private static List<NestedConfig> ParseTestNestedConfig(XmlElement configItem, ModS mod, string configName, in ConfigParseContext context)
        {
            try
            {
                var list = new List<NestedConfig>();
            var dc = XM.Contracts.IConfigDataCenter.I; if (dc == null) return list;
            var nodes = configItem.SelectNodes("TestNestedConfig");
            if (nodes != null)
            foreach (System.Xml.XmlNode n in nodes) { var el = n as System.Xml.XmlElement; if (el == null) continue; var helper = dc.GetClassHelper(typeof(NestedConfig)); if (helper != null) { var item = (NestedConfig)helper.DeserializeConfigFromXml(el, mod, configName + "_TestNestedConfig_" + list.Count, in context); if (item != null) list.Add(item); } }
            return list;
            }
            catch (Exception ex)
            {
                if (ConfigParseHelper.IsStrictMode(context)) ConfigParseHelper.LogParseError(context, "TestNestedConfig", ex); else ConfigParseHelper.LogParseWarning("TestNestedConfig", null, ex);
                return new List<NestedConfig>();
            }
        }

    private static CfgS<TestConfigUnManaged> ParseForeign(XmlElement configItem, ModS mod, string configName, in ConfigParseContext context)
        {
            try
            {
                var s = ConfigParseHelper.GetXmlFieldValue(configItem, "Foreign");
            if (string.IsNullOrEmpty(s)) return default;
            if (!ConfigParseHelper.TryParseCfgSString(s, "Foreign", out var modName, out var cfgName)) return default;
            return new CfgS<TestConfigUnManaged>(new ModS(modName), cfgName);
            }
            catch (Exception ex)
            {
                if (ConfigParseHelper.IsStrictMode(context)) ConfigParseHelper.LogParseError(context, "Foreign", ex); else ConfigParseHelper.LogParseWarning("Foreign", ConfigParseHelper.GetXmlFieldValue(configItem, "Foreign"), ex);
                return default;
            }
        }

    private static int ParseTestIndex1(XmlElement configItem, ModS mod, string configName, in ConfigParseContext context)
        {
            var s = ConfigParseHelper.GetXmlFieldValue(configItem, "TestIndex1");
            if (string.IsNullOrEmpty(s)) return default;
            return ConfigParseHelper.TryParseInt(s, "TestIndex1", out var v) ? v : default;
        }

    private static CfgS<TestConfigUnManaged> ParseTestIndex2(XmlElement configItem, ModS mod, string configName, in ConfigParseContext context)
        {
            try
            {
                var s = ConfigParseHelper.GetXmlFieldValue(configItem, "TestIndex2");
            if (string.IsNullOrEmpty(s)) return default;
            if (!ConfigParseHelper.TryParseCfgSString(s, "TestIndex2", out var modName, out var cfgName)) return default;
            return new CfgS<TestConfigUnManaged>(new ModS(modName), cfgName);
            }
            catch (Exception ex)
            {
                if (ConfigParseHelper.IsStrictMode(context)) ConfigParseHelper.LogParseError(context, "TestIndex2", ex); else ConfigParseHelper.LogParseWarning("TestIndex2", ConfigParseHelper.GetXmlFieldValue(configItem, "TestIndex2"), ex);
                return default;
            }
        }

    private static CfgS<TestConfigUnManaged> ParseTestIndex3(XmlElement configItem, ModS mod, string configName, in ConfigParseContext context)
        {
            try
            {
                var s = ConfigParseHelper.GetXmlFieldValue(configItem, "TestIndex3");
            if (string.IsNullOrEmpty(s)) return default;
            if (!ConfigParseHelper.TryParseCfgSString(s, "TestIndex3", out var modName, out var cfgName)) return default;
            return new CfgS<TestConfigUnManaged>(new ModS(modName), cfgName);
            }
            catch (Exception ex)
            {
                if (ConfigParseHelper.IsStrictMode(context)) ConfigParseHelper.LogParseError(context, "TestIndex3", ex); else ConfigParseHelper.LogParseWarning("TestIndex3", ConfigParseHelper.GetXmlFieldValue(configItem, "TestIndex3"), ex);
                return default;
            }
        }

    #endregion
}

