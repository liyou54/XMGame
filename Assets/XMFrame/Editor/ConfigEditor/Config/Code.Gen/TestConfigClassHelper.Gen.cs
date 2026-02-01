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
        return new TblS(new ModS("Default"), "TestConfig");
    }

    public override void SetTblIDefinedInMod(TblI tbl)
    {
        _definedInMod = tbl;
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

    private static CfgS<TestConfigUnManaged> ParseId(XmlElement configItem, ModS mod, string configName,
        in ConfigParseContext context)
    {
        try
        {
            var s = ConfigParseHelper.GetXmlFieldValue(configItem, "Id");
            if (string.IsNullOrEmpty(s)) return default;
            if (!ConfigParseHelper.TryParseCfgSString(s, "Id", out var modName, out var cfgName))
                return default;
            return new CfgS<TestConfigUnManaged>(new ModS(modName), cfgName);
        }
        catch (Exception ex)
        {
            if (ConfigParseHelper.IsStrictMode(context))
                ConfigParseHelper.LogParseError(context, "Id", ex);
            else
                ConfigParseHelper.LogParseWarning("Id",
                    ConfigParseHelper.GetXmlFieldValue(configItem, "Id"), ex);
            return default;
        }
    }

    private static int ParseTestInt(XmlElement configItem, ModS mod, string configName,
        in ConfigParseContext context)
    {
        var s = ConfigParseHelper.GetXmlFieldValue(configItem, "TestInt");
        if (string.IsNullOrEmpty(s)) return default;
        return ConfigParseHelper.TryParseInt(s, "TestInt", out var v) ? v : default;
    }

    private static List<int> ParseTestSample(XmlElement configItem, ModS mod, string configName,
        in ConfigParseContext context)
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
            if (ConfigParseHelper.IsStrictMode(context))
                ConfigParseHelper.LogParseError(context, "TestSample", ex);
            else
                ConfigParseHelper.LogParseWarning("TestSample",
                    null, ex);
            return new List<int>();
        }
    }

    private static Dictionary<int, int> ParseTestDictSample(XmlElement configItem, ModS mod, string configName,
        in ConfigParseContext context)
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
            if (ConfigParseHelper.IsStrictMode(context))
                ConfigParseHelper.LogParseError(context, "TestDictSample", ex);
            else
                ConfigParseHelper.LogParseWarning("TestDictSample",
                    null, ex);
            return new Dictionary<int, int>();
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

    private static Dictionary<int, List<List<CfgS<TestConfigUnManaged>>>> ParseTestKeyList1(XmlElement configItem, ModS mod, string configName,
        in ConfigParseContext context)
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
            if (ConfigParseHelper.IsStrictMode(context))
                ConfigParseHelper.LogParseError(context, "TestKeyList1", ex);
            else
                ConfigParseHelper.LogParseWarning("TestKeyList1",
                    null, ex);
            return new Dictionary<int, List<List<CfgS<TestConfigUnManaged>>>>();
        }
    }

    private static HashSet<int> ParseTestKeyHashSet(XmlElement configItem, ModS mod, string configName,
        in ConfigParseContext context)
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
            if (ConfigParseHelper.IsStrictMode(context))
                ConfigParseHelper.LogParseError(context, "TestKeyHashSet", ex);
            else
                ConfigParseHelper.LogParseWarning("TestKeyHashSet",
                    ConfigParseHelper.GetXmlFieldValue(configItem, "TestKeyHashSet"), ex);
            return new HashSet<int>();
        }
    }

    private static Dictionary<CfgS<TestConfigUnManaged>, CfgS<TestConfigUnManaged>> ParseTestKeyDict(XmlElement configItem, ModS mod, string configName,
        in ConfigParseContext context)
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
            if (ConfigParseHelper.IsStrictMode(context))
                ConfigParseHelper.LogParseError(context, "TestKeyDict", ex);
            else
                ConfigParseHelper.LogParseWarning("TestKeyDict",
                    null, ex);
            return new Dictionary<CfgS<TestConfigUnManaged>, CfgS<TestConfigUnManaged>>();
        }
    }

    private static HashSet<CfgS<TestConfigUnManaged>> ParseTestSetKey(XmlElement configItem, ModS mod, string configName,
        in ConfigParseContext context)
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
            if (ConfigParseHelper.IsStrictMode(context))
                ConfigParseHelper.LogParseError(context, "TestSetKey", ex);
            else
                ConfigParseHelper.LogParseWarning("TestSetKey",
                    ConfigParseHelper.GetXmlFieldValue(configItem, "TestSetKey"), ex);
            return new HashSet<CfgS<TestConfigUnManaged>>();
        }
    }

    private static HashSet<int> ParseTestSetSample(XmlElement configItem, ModS mod, string configName,
        in ConfigParseContext context)
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
            if (ConfigParseHelper.IsStrictMode(context))
                ConfigParseHelper.LogParseError(context, "TestSetSample", ex);
            else
                ConfigParseHelper.LogParseWarning("TestSetSample",
                    ConfigParseHelper.GetXmlFieldValue(configItem, "TestSetSample"), ex);
            return new HashSet<int>();
        }
    }

    private static NestedConfig ParseTestNested(XmlElement configItem, ModS mod, string configName,
        in ConfigParseContext context)
    {
        try
        {
            var el = configItem.SelectSingleNode("TestNested") as System.Xml.XmlElement;
            if (el == null) return null;
            var helper = XM.Contracts.IConfigDataCenter.I?.GetClassHelper(typeof(NestedConfig));
            return helper != null
                ? (NestedConfig)helper.DeserializeConfigFromXml(el, mod, configName + "_TestNested", context)
                : null;
        }
        catch (Exception ex)
        {
            if (ConfigParseHelper.IsStrictMode(context))
                ConfigParseHelper.LogParseError(context, "TestNested", ex);
            else
                ConfigParseHelper.LogParseWarning("TestNested",
                    null, ex);
            return null;
        }
    }

    private static List<NestedConfig> ParseTestNestedConfig(XmlElement configItem, ModS mod, string configName,
        in ConfigParseContext context)
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
            if (ConfigParseHelper.IsStrictMode(context))
                ConfigParseHelper.LogParseError(context, "TestNestedConfig", ex);
            else
                ConfigParseHelper.LogParseWarning("TestNestedConfig",
                    null, ex);
            return new List<NestedConfig>();
        }
    }

    private static CfgS<TestConfigUnManaged> ParseForeign(XmlElement configItem, ModS mod, string configName,
        in ConfigParseContext context)
    {
        try
        {
            var s = ConfigParseHelper.GetXmlFieldValue(configItem, "Foreign");
            if (string.IsNullOrEmpty(s)) return default;
            if (!ConfigParseHelper.TryParseCfgSString(s, "Foreign", out var modName, out var cfgName))
                return default;
            return new CfgS<TestConfigUnManaged>(new ModS(modName), cfgName);
        }
        catch (Exception ex)
        {
            if (ConfigParseHelper.IsStrictMode(context))
                ConfigParseHelper.LogParseError(context, "Foreign", ex);
            else
                ConfigParseHelper.LogParseWarning("Foreign",
                    ConfigParseHelper.GetXmlFieldValue(configItem, "Foreign"), ex);
            return default;
        }
    }

    private static int ParseTestIndex1(XmlElement configItem, ModS mod, string configName,
        in ConfigParseContext context)
    {
        var s = ConfigParseHelper.GetXmlFieldValue(configItem, "TestIndex1");
        if (string.IsNullOrEmpty(s)) return default;
        return ConfigParseHelper.TryParseInt(s, "TestIndex1", out var v) ? v : default;
    }

    private static CfgS<TestConfigUnManaged> ParseTestIndex2(XmlElement configItem, ModS mod, string configName,
        in ConfigParseContext context)
    {
        try
        {
            var s = ConfigParseHelper.GetXmlFieldValue(configItem, "TestIndex2");
            if (string.IsNullOrEmpty(s)) return default;
            if (!ConfigParseHelper.TryParseCfgSString(s, "TestIndex2", out var modName, out var cfgName))
                return default;
            return new CfgS<TestConfigUnManaged>(new ModS(modName), cfgName);
        }
        catch (Exception ex)
        {
            if (ConfigParseHelper.IsStrictMode(context))
                ConfigParseHelper.LogParseError(context, "TestIndex2", ex);
            else
                ConfigParseHelper.LogParseWarning("TestIndex2",
                    ConfigParseHelper.GetXmlFieldValue(configItem, "TestIndex2"), ex);
            return default;
        }
    }

    private static CfgS<TestConfigUnManaged> ParseTestIndex3(XmlElement configItem, ModS mod, string configName,
        in ConfigParseContext context)
    {
        try
        {
            var s = ConfigParseHelper.GetXmlFieldValue(configItem, "TestIndex3");
            if (string.IsNullOrEmpty(s)) return default;
            if (!ConfigParseHelper.TryParseCfgSString(s, "TestIndex3", out var modName, out var cfgName))
                return default;
            return new CfgS<TestConfigUnManaged>(new ModS(modName), cfgName);
        }
        catch (Exception ex)
        {
            if (ConfigParseHelper.IsStrictMode(context))
                ConfigParseHelper.LogParseError(context, "TestIndex3", ex);
            else
                ConfigParseHelper.LogParseWarning("TestIndex3",
                    ConfigParseHelper.GetXmlFieldValue(configItem, "TestIndex3"), ex);
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
        var config = (TestConfig)value;
        // 在最外层获取 unmanaged 数据的值（不是引用）
        var map = configHolderData.Data.GetMap<CfgI, TestConfigUnManaged>(_definedInMod);
        if (!map.TryGetValue(configHolderData.Data.BlobContainer, cfgi, out var data))
        {
            return;
        }

        AllocTestSample(config, ref data, cfgi, configHolderData);
        AllocTestDictSample(config, ref data, cfgi, configHolderData);
        AllocTestKeyList(config, ref data, cfgi, configHolderData);
        AllocTestKeyList1(config, ref data, cfgi, configHolderData);
        AllocTestKeyList2(config, ref data, cfgi, configHolderData);
        AllocTestKeyHashSet(config, ref data, cfgi, configHolderData);
        AllocTestKeyDict(config, ref data, cfgi, configHolderData);
        AllocTestSetKey(config, ref data, cfgi, configHolderData);
        AllocTestSetSample(config, ref data, cfgi, configHolderData);
        AllocTestNestedConfig(config, ref data, cfgi, configHolderData);
        AllocConfigDict(config, ref data, cfgi, configHolderData);

        // 将修改后的 unmanaged 数据写回容器
        map[configHolderData.Data.BlobContainer, cfgi] = data;
    }

    #region 容器分配辅助方法

    private void AllocTestSample(
        TestConfig config,
        ref TestConfigUnManaged data,
        CfgI cfgi,
        XM.ConfigDataCenter.ConfigDataHolder configHolderData)
    {
        if (config.TestSample != null && config.TestSample.Count > 0)
        {
            var allocated = configHolderData.Data.BlobContainer.AllocArray<Int32>(config.TestSample.Count);
            data.TestSample = allocated;
        }
    }

    private void AllocTestDictSample(
        TestConfig config,
        ref TestConfigUnManaged data,
        CfgI cfgi,
        XM.ConfigDataCenter.ConfigDataHolder configHolderData)
    {
        if (config.TestDictSample != null && config.TestDictSample.Count > 0)
        {
            var allocated = configHolderData.Data.BlobContainer.AllocMap<Int32, Int32>(config.TestDictSample.Count);
            data.TestDictSample = allocated;
        }
    }

    private void AllocTestKeyList(
        TestConfig config,
        ref TestConfigUnManaged data,
        CfgI cfgi,
        XM.ConfigDataCenter.ConfigDataHolder configHolderData)
    {
        if (config.TestKeyList != null && config.TestKeyList.Count > 0)
        {
            var allocated = configHolderData.Data.BlobContainer.AllocArray<CfgI<TestConfigUnManaged>>(config.TestKeyList.Count);
            data.TestKeyList = allocated;
        }
    }

    private void AllocTestKeyList1(
        TestConfig config,
        ref TestConfigUnManaged data,
        CfgI cfgi,
        XM.ConfigDataCenter.ConfigDataHolder configHolderData)
    {
        if (config.TestKeyList1 != null && config.TestKeyList1.Count > 0)
        {
            var allocated = configHolderData.Data.BlobContainer.AllocMap<Int32, XBlobArray<XBlobArray<CfgI<TestConfigUnManaged>>>>(config.TestKeyList1.Count);
            data.TestKeyList1 = allocated;

            // 分配嵌套容器
            foreach (var kvp0 in config.TestKeyList1)
            {
            if (kvp0.Value != null && kvp0.Value.Count > 0)
            {
                var nested1 = configHolderData.Data.BlobContainer.AllocArray<XBlobArray<CfgI<TestConfigUnManaged>>>(kvp0.Value.Count);

                // 分配更深层的嵌套容器
                for (int i1 = 0; i1 < kvp0.Value.Count; i1++)
                {
                if (kvp0.Value[i1] != null && kvp0.Value[i1].Count > 0)
                {
                    var nested2 = configHolderData.Data.BlobContainer.AllocArray<CfgI<TestConfigUnManaged>>(kvp0.Value[i1].Count);
                    nested1[configHolderData.Data.BlobContainer, i1] = nested2;
                }
                }

                // 将分配的容器赋值到顶层数据
                var map1 = configHolderData.Data.GetMap<CfgI, TestConfigUnManaged>(_definedInMod);
                if (!map1.TryGetValue(configHolderData.Data.BlobContainer, cfgi, out var data1))
                {
                    XM.XLog.Error($"[Config] 配置 {cfgi} 不存在于表中，无法分配嵌套容器");
                    return;
                }
                data1.TestKeyList1[configHolderData.Data.BlobContainer, kvp0.Key] = nested1;
                map1[configHolderData.Data.BlobContainer, cfgi] = data1;
            }
            }
        }
    }

    private void AllocTestKeyList2(
        TestConfig config,
        ref TestConfigUnManaged data,
        CfgI cfgi,
        XM.ConfigDataCenter.ConfigDataHolder configHolderData)
    {
        if (config.TestKeyList2 != null && config.TestKeyList2.Count > 0)
        {
            var allocated = configHolderData.Data.BlobContainer.AllocMap<CfgI<TestConfigUnManaged>, XBlobArray<XBlobArray<CfgI<TestConfigUnManaged>>>>(config.TestKeyList2.Count);
            data.TestKeyList2 = allocated;

            // 分配嵌套容器
            foreach (var kvp0 in config.TestKeyList2)
            {
                if (!IConfigDataCenter.I.TryGetCfgI(kvp0.Key.AsNonGeneric(), out var kvp0_cfgI))
                {
                    XM.XLog.Error($"[Config] 无法找到配置 {kvp0.Key.ConfigName}, 跳过该项嵌套容器分配");
                    continue;
                }
            if (kvp0.Value != null && kvp0.Value.Count > 0)
            {
                var nested1 = configHolderData.Data.BlobContainer.AllocArray<XBlobArray<CfgI<TestConfigUnManaged>>>(kvp0.Value.Count);

                // 分配更深层的嵌套容器
                for (int i1 = 0; i1 < kvp0.Value.Count; i1++)
                {
                if (kvp0.Value[i1] != null && kvp0.Value[i1].Count > 0)
                {
                    var nested2 = configHolderData.Data.BlobContainer.AllocArray<CfgI<TestConfigUnManaged>>(kvp0.Value[i1].Count);
                    nested1[configHolderData.Data.BlobContainer, i1] = nested2;
                }
                }

                // 将分配的容器赋值到顶层数据
                var map1 = configHolderData.Data.GetMap<CfgI, TestConfigUnManaged>(_definedInMod);
                if (!map1.TryGetValue(configHolderData.Data.BlobContainer, cfgi, out var data1))
                {
                    XM.XLog.Error($"[Config] 配置 {cfgi} 不存在于表中，无法分配嵌套容器");
                    return;
                }
                data1.TestKeyList2[configHolderData.Data.BlobContainer, kvp0_cfgI.As<TestConfigUnManaged>()] = nested1;
                map1[configHolderData.Data.BlobContainer, cfgi] = data1;
            }
            }
        }
    }

    private void AllocTestKeyHashSet(
        TestConfig config,
        ref TestConfigUnManaged data,
        CfgI cfgi,
        XM.ConfigDataCenter.ConfigDataHolder configHolderData)
    {
        if (config.TestKeyHashSet != null && config.TestKeyHashSet.Count > 0)
        {
            var allocated = configHolderData.Data.BlobContainer.AllocSet<Int32>(config.TestKeyHashSet.Count);
            data.TestKeyHashSet = allocated;
        }
    }

    private void AllocTestKeyDict(
        TestConfig config,
        ref TestConfigUnManaged data,
        CfgI cfgi,
        XM.ConfigDataCenter.ConfigDataHolder configHolderData)
    {
        if (config.TestKeyDict != null && config.TestKeyDict.Count > 0)
        {
            var allocated = configHolderData.Data.BlobContainer.AllocMap<CfgI<TestConfigUnManaged>, CfgI<TestConfigUnManaged>>(config.TestKeyDict.Count);
            data.TestKeyDict = allocated;
        }
    }

    private void AllocTestSetKey(
        TestConfig config,
        ref TestConfigUnManaged data,
        CfgI cfgi,
        XM.ConfigDataCenter.ConfigDataHolder configHolderData)
    {
        if (config.TestSetKey != null && config.TestSetKey.Count > 0)
        {
            var allocated = configHolderData.Data.BlobContainer.AllocSet<CfgI<TestConfigUnManaged>>(config.TestSetKey.Count);
            data.TestSetKey = allocated;
        }
    }

    private void AllocTestSetSample(
        TestConfig config,
        ref TestConfigUnManaged data,
        CfgI cfgi,
        XM.ConfigDataCenter.ConfigDataHolder configHolderData)
    {
        if (config.TestSetSample != null && config.TestSetSample.Count > 0)
        {
            var allocated = configHolderData.Data.BlobContainer.AllocSet<Int32>(config.TestSetSample.Count);
            data.TestSetSample = allocated;
        }
    }

    private void AllocTestNestedConfig(
        TestConfig config,
        ref TestConfigUnManaged data,
        CfgI cfgi,
        XM.ConfigDataCenter.ConfigDataHolder configHolderData)
    {
        if (config.TestNestedConfig != null && config.TestNestedConfig.Count > 0)
        {
            var allocated = configHolderData.Data.BlobContainer.AllocArray<NestedConfigUnManaged>(config.TestNestedConfig.Count);
            data.TestNestedConfig = allocated;
        }
    }

    private void AllocConfigDict(
        TestConfig config,
        ref TestConfigUnManaged data,
        CfgI cfgi,
        XM.ConfigDataCenter.ConfigDataHolder configHolderData)
    {
        if (config.ConfigDict != null && config.ConfigDict.Count > 0)
        {
            var allocated = configHolderData.Data.BlobContainer.AllocMap<TypeI, XBlobMap<TypeI, XBlobArray<NestedConfigUnManaged>>>(config.ConfigDict.Count);
            data.ConfigDict = allocated;

            // 分配嵌套容器
            foreach (var kvp0 in config.ConfigDict)
            {
                var converter_kvp0 = XM.Contracts.IConfigDataCenter.I?.GetConverterByType<Type, TypeI>();
                if (converter_kvp0 == null)
                {
                    XM.XLog.Error($"[Config] 无法找到类型转换器 Type -> TypeI");
                    continue;
                }
                if (!converter_kvp0.Convert(kvp0.Key, out var kvp0_typeId)) continue;
            if (kvp0.Value != null && kvp0.Value.Count > 0)
            {
                var nested1 = configHolderData.Data.BlobContainer.AllocMap<TypeI, XBlobArray<NestedConfigUnManaged>>(kvp0.Value.Count);

                // 分配更深层的嵌套容器
                foreach (var kvp1 in kvp0.Value)
                {
                    var converter_kvp1 = XM.Contracts.IConfigDataCenter.I?.GetConverterByType<Type, TypeI>();
                    if (converter_kvp1 == null)
                    {
                        XM.XLog.Error($"[Config] 无法找到类型转换器 Type -> TypeI");
                        continue;
                    }
                    if (!converter_kvp1.Convert(kvp1.Key, out var kvp1_typeId)) continue;
                if (kvp1.Value != null && kvp1.Value.Count > 0)
                {
                    var nested2 = configHolderData.Data.BlobContainer.AllocArray<NestedConfigUnManaged>(kvp1.Value.Count);
                    nested1[configHolderData.Data.BlobContainer, kvp1_typeId] = nested2;
                }
                }

                // 将分配的容器赋值到顶层数据
                var map1 = configHolderData.Data.GetMap<CfgI, TestConfigUnManaged>(_definedInMod);
                if (!map1.TryGetValue(configHolderData.Data.BlobContainer, cfgi, out var data1))
                {
                    XM.XLog.Error($"[Config] 配置 {cfgi} 不存在于表中，无法分配嵌套容器");
                    return;
                }
                data1.ConfigDict[configHolderData.Data.BlobContainer, kvp0_typeId] = nested1;
                map1[configHolderData.Data.BlobContainer, cfgi] = data1;
            }
            }
        }
    }

    #endregion

    public override void FillBasicDataImpl(XM.ConfigDataCenter.ConfigDataHolder configHolderData, CfgS key, IXConfig value, XBlobMap<CfgI, TestConfigUnManaged> tableMap)
    {
        // TODO: 实现基础数据填充逻辑
    }

    private TblI _definedInMod;
}

