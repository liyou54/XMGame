using System;
using System.Collections.Generic;
using System.Xml;
using Unity.Collections;
using XM;
using XM.Contracts;
using XM.Contracts.Config;

/// <summary>
/// TestConfig 的配置加载辅助类，用于从 XML 反序列化（静态代码生成，无反射）。
/// </summary>
public class TestConfigClassHelper : ConfigClassHelper<TestConfig, TestConfigUnmanaged>
{
    public static TestConfigClassHelper Instance { get; private set; }
    public static TblI TblI { get; private set; }
    public static TblS TblS { get; private set; }
    private static readonly string __modName;

    /// <summary>
    /// 静态构造函数
    /// </summary>
    static TestConfigClassHelper()
    {
        const string __tableName = "TestConfig";
        __modName = "MyMod";
        CfgS<TestConfigUnmanaged>.Table = new TblS(new ModS(__modName), __tableName);
        TblS = new TblS(new ModS(__modName), __tableName);
        Instance = new TestConfigClassHelper();
    }
    /// <summary>
    /// 构造函数
    /// </summary>
    public TestConfigClassHelper()
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
        var config = (TestConfig)target;

        // 解析所有字段
        config.Id = ParseId(configItem, mod, configName, context);
        config.TestInt = ParseTestInt(configItem, mod, configName, context);
        config.TestSample = ParseTestSample(configItem, mod, configName, context);
        config.TestDictSample = ParseTestDictSample(configItem, mod, configName, context);
        config.TestKeyList = ParseTestKeyList(configItem, mod, configName, context);
        config.TestKeyList1 = ParseTestKeyList1(configItem, mod, configName, context);
        config.TestKeyList2 = ParseTestKeyList2(configItem, mod, configName, context);
        config.TestKeyHashSet = ParseTestKeyHashSet(configItem, mod, configName, context);
        config.TestKeyDict = ParseTestKeyDict(configItem, mod, configName, context);
        config.TestSetKey = ParseTestSetKey(configItem, mod, configName, context);
        config.TestSetSample = ParseTestSetSample(configItem, mod, configName, context);
        config.TestNested = ParseTestNested(configItem, mod, configName, context);
        config.TestNestedConfig = ParseTestNestedConfig(configItem, mod, configName, context);
        config.Foreign = ParseForeign(configItem, mod, configName, context);
        config.ConfigDict = ParseConfigDict(configItem, mod, configName, context);
        config.TestIndex1 = ParseTestIndex1(configItem, mod, configName, context);
        config.TestIndex2 = ParseTestIndex2(configItem, mod, configName, context);
        config.TestIndex3 = ParseTestIndex3(configItem, mod, configName, context);
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
    /// 解析 TestInt 字段
    /// </summary>
    private static int ParseTestInt(
        global::System.Xml.XmlElement configItem,
        global::XM.Contracts.Config.ModS mod,
        string configName,
        in global::XM.Contracts.Config.ConfigParseContext context)
    {
        var xmlValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "TestInt");

        if (string.IsNullOrEmpty(xmlValue))
        {
            return default;
        }

        if (global::XM.Contracts.Config.ConfigParseHelper.TryParseInt(xmlValue, "TestInt", out var parsedValue))
        {
            return parsedValue;
        }

        return default;
    }

    /// <summary>
    /// 解析 TestSample 字段
    /// </summary>
    private static global::System.Collections.Generic.List<int> ParseTestSample(
        global::System.Xml.XmlElement configItem,
        global::XM.Contracts.Config.ModS mod,
        string configName,
        in global::XM.Contracts.Config.ConfigParseContext context)
    {
        var list = new global::System.Collections.Generic.List<int>();

        // 尝试从 XML 节点解析
        var nodes = configItem.SelectNodes("TestSample");
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

                if (global::XM.Contracts.Config.ConfigParseHelper.TryParseInt(text, "TestSample", out var parsedItem))
                {
                    list.Add(parsedItem);
                }
            }
        }

        // 如果没有节点，尝试 CSV 格式
        if (list.Count == 0)
        {
            var csvValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "TestSample");
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

                    if (global::XM.Contracts.Config.ConfigParseHelper.TryParseInt(trimmed, "TestSample", out var parsedItem))
                    {
                        list.Add(parsedItem);
                    }
                }
            }
        }

        return list;
    }

    /// <summary>
    /// 解析 TestDictSample 字段
    /// </summary>
    private static global::System.Collections.Generic.Dictionary<int, int> ParseTestDictSample(
        global::System.Xml.XmlElement configItem,
        global::XM.Contracts.Config.ModS mod,
        string configName,
        in global::XM.Contracts.Config.ConfigParseContext context)
    {
        var dict = new global::System.Collections.Generic.Dictionary<int, int>();

        // 解析 Dictionary Item 节点
        var dictNodes = configItem.SelectNodes("TestDictSample/Item");
        if (dictNodes != null)
        {
            foreach (var node in dictNodes)
            {
                var element = node as global::System.Xml.XmlElement;
                if (element == null)
                {
                    continue;
                }

                var keyText = element.GetAttribute("Key");
                var valueText = element.InnerText?.Trim();

                if (global::XM.Contracts.Config.ConfigParseHelper.TryParseInt(keyText, "TestDictSample.Key", out var parsedKey))
                {
                    if (global::XM.Contracts.Config.ConfigParseHelper.TryParseInt(valueText, "TestDictSample.Value", out var parsedValue))
                    {
                        dict[parsedKey] = parsedValue;
                    }
                }
            }
        }

        return dict;
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

                // CfgS 类型不支持从文本解析（应该从 id 属性读取）: CfgS<TestConfig>
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

                    // CfgS 类型不支持从文本解析（应该从 id 属性读取）: CfgS<TestConfig>
                    continue;
                }
            }
        }

        return list;
    }

    /// <summary>
    /// 解析 TestKeyList1 字段
    /// </summary>
    private static global::System.Collections.Generic.Dictionary<int, global::System.Collections.Generic.List<global::System.Collections.Generic.List<global::XM.Contracts.Config.CfgS<TestConfig>>>> ParseTestKeyList1(
        global::System.Xml.XmlElement configItem,
        global::XM.Contracts.Config.ModS mod,
        string configName,
        in global::XM.Contracts.Config.ConfigParseContext context)
    {
        var dict = new global::System.Collections.Generic.Dictionary<int, global::System.Collections.Generic.List<global::System.Collections.Generic.List<global::XM.Contracts.Config.CfgS<TestConfig>>>>();

        // 解析 Dictionary Item 节点
        var dictNodes = configItem.SelectNodes("TestKeyList1/Item");
        if (dictNodes != null)
        {
            foreach (var node in dictNodes)
            {
                var element = node as global::System.Xml.XmlElement;
                if (element == null)
                {
                    continue;
                }

                var keyText = element.GetAttribute("Key");
                var valueText = element.InnerText?.Trim();

                if (global::XM.Contracts.Config.ConfigParseHelper.TryParseInt(keyText, "TestKeyList1.Key", out var parsedKey))
                {
                    // 嵌套容器不支持从文本解析: List<List<CfgS<TestConfig>>>
                    continue;
                }
            }
        }

        return dict;
    }

    /// <summary>
    /// 解析 TestKeyList2 字段
    /// </summary>
    private static global::System.Collections.Generic.Dictionary<global::XM.Contracts.Config.CfgS<TestConfig>, global::System.Collections.Generic.List<global::System.Collections.Generic.List<global::XM.Contracts.Config.CfgS<TestConfig>>>> ParseTestKeyList2(
        global::System.Xml.XmlElement configItem,
        global::XM.Contracts.Config.ModS mod,
        string configName,
        in global::XM.Contracts.Config.ConfigParseContext context)
    {
        var dict = new global::System.Collections.Generic.Dictionary<global::XM.Contracts.Config.CfgS<TestConfig>, global::System.Collections.Generic.List<global::System.Collections.Generic.List<global::XM.Contracts.Config.CfgS<TestConfig>>>>();

        // 解析 Dictionary Item 节点
        var dictNodes = configItem.SelectNodes("TestKeyList2/Item");
        if (dictNodes != null)
        {
            foreach (var node in dictNodes)
            {
                var element = node as global::System.Xml.XmlElement;
                if (element == null)
                {
                    continue;
                }

                var keyText = element.GetAttribute("Key");
                var valueText = element.InnerText?.Trim();

                // CfgS 类型不支持从文本解析（应该从 id 属性读取）: CfgS<TestConfig>
                continue;
            }
        }

        return dict;
    }

    /// <summary>
    /// 解析 TestKeyHashSet 字段
    /// </summary>
    private static global::System.Collections.Generic.HashSet<int> ParseTestKeyHashSet(
        global::System.Xml.XmlElement configItem,
        global::XM.Contracts.Config.ModS mod,
        string configName,
        in global::XM.Contracts.Config.ConfigParseContext context)
    {
        var set = new global::System.Collections.Generic.HashSet<int>();

        // 从 XML 节点解析
        var nodes = configItem.SelectNodes("TestKeyHashSet");
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

                if (global::XM.Contracts.Config.ConfigParseHelper.TryParseInt(text, "TestKeyHashSet", out var parsedItem))
                {
                    set.Add(parsedItem);
                }
            }
        }

        // CSV 格式备用
        if (set.Count == 0)
        {
            var csvValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "TestKeyHashSet");
            if (!string.IsNullOrEmpty(csvValue))
            {
                var parts = csvValue.Split(new[] { ',', ';', '|' }, global::System.StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in parts)
                {
                    var trimmed = part.Trim();
                    if (global::XM.Contracts.Config.ConfigParseHelper.TryParseInt(trimmed, "TestKeyHashSet", out var parsedItem))
                    {
                        set.Add(parsedItem);
                    }
                }
            }
        }

        return set;
    }

    /// <summary>
    /// 解析 TestKeyDict 字段
    /// </summary>
    private static global::System.Collections.Generic.Dictionary<global::XM.Contracts.Config.CfgS<TestConfig>, global::XM.Contracts.Config.CfgS<TestConfig>> ParseTestKeyDict(
        global::System.Xml.XmlElement configItem,
        global::XM.Contracts.Config.ModS mod,
        string configName,
        in global::XM.Contracts.Config.ConfigParseContext context)
    {
        var dict = new global::System.Collections.Generic.Dictionary<global::XM.Contracts.Config.CfgS<TestConfig>, global::XM.Contracts.Config.CfgS<TestConfig>>();

        // 解析 Dictionary Item 节点
        var dictNodes = configItem.SelectNodes("TestKeyDict/Item");
        if (dictNodes != null)
        {
            foreach (var node in dictNodes)
            {
                var element = node as global::System.Xml.XmlElement;
                if (element == null)
                {
                    continue;
                }

                var keyText = element.GetAttribute("Key");
                var valueText = element.InnerText?.Trim();

                // CfgS 类型不支持从文本解析（应该从 id 属性读取）: CfgS<TestConfig>
                continue;
            }
        }

        return dict;
    }

    /// <summary>
    /// 解析 TestSetKey 字段
    /// </summary>
    private static global::System.Collections.Generic.HashSet<global::XM.Contracts.Config.CfgS<TestConfig>> ParseTestSetKey(
        global::System.Xml.XmlElement configItem,
        global::XM.Contracts.Config.ModS mod,
        string configName,
        in global::XM.Contracts.Config.ConfigParseContext context)
    {
        var set = new global::System.Collections.Generic.HashSet<global::XM.Contracts.Config.CfgS<TestConfig>>();

        // 从 XML 节点解析
        var nodes = configItem.SelectNodes("TestSetKey");
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

                // CfgS 类型不支持从文本解析（应该从 id 属性读取）: CfgS<TestConfig>
                continue;
            }
        }

        // CSV 格式备用
        if (set.Count == 0)
        {
            var csvValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "TestSetKey");
            if (!string.IsNullOrEmpty(csvValue))
            {
                var parts = csvValue.Split(new[] { ',', ';', '|' }, global::System.StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in parts)
                {
                    var trimmed = part.Trim();
                    // CfgS 类型不支持从文本解析（应该从 id 属性读取）: CfgS<TestConfig>
                    continue;
                }
            }
        }

        return set;
    }

    /// <summary>
    /// 解析 TestSetSample 字段
    /// </summary>
    private static global::System.Collections.Generic.HashSet<int> ParseTestSetSample(
        global::System.Xml.XmlElement configItem,
        global::XM.Contracts.Config.ModS mod,
        string configName,
        in global::XM.Contracts.Config.ConfigParseContext context)
    {
        var set = new global::System.Collections.Generic.HashSet<int>();

        // 从 XML 节点解析
        var nodes = configItem.SelectNodes("TestSetSample");
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

                if (global::XM.Contracts.Config.ConfigParseHelper.TryParseInt(text, "TestSetSample", out var parsedItem))
                {
                    set.Add(parsedItem);
                }
            }
        }

        // CSV 格式备用
        if (set.Count == 0)
        {
            var csvValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "TestSetSample");
            if (!string.IsNullOrEmpty(csvValue))
            {
                var parts = csvValue.Split(new[] { ',', ';', '|' }, global::System.StringSplitOptions.RemoveEmptyEntries);
                foreach (var part in parts)
                {
                    var trimmed = part.Trim();
                    if (global::XM.Contracts.Config.ConfigParseHelper.TryParseInt(trimmed, "TestSetSample", out var parsedItem))
                    {
                        set.Add(parsedItem);
                    }
                }
            }
        }

        return set;
    }

    /// <summary>
    /// 解析 TestNested 字段
    /// </summary>
    private static NestedConfig ParseTestNested(
        global::System.Xml.XmlElement configItem,
        global::XM.Contracts.Config.ModS mod,
        string configName,
        in global::XM.Contracts.Config.ConfigParseContext context)
    {
        // 解析嵌套配置
        var element = configItem.SelectSingleNode("TestNested") as global::System.Xml.XmlElement;
        if (element != null == false)
        {
            return null;
        }

        var helper = global::XM.Contracts.IConfigManager.I?.GetClassHelper(typeof(NestedConfig));
        if (helper != null == false)
        {
            global::XM.Contracts.Config.ConfigParseHelper.LogParseError(context, "TestNested", "无法获取嵌套配置 Helper");
            return null;
        }

        var nestedConfigName = configName + "_TestNested";
        return (NestedConfig)helper.DeserializeConfigFromXml(element, mod, nestedConfigName, context);
    }

    /// <summary>
    /// 解析 TestNestedConfig 字段
    /// </summary>
    private static global::System.Collections.Generic.List<NestedConfig> ParseTestNestedConfig(
        global::System.Xml.XmlElement configItem,
        global::XM.Contracts.Config.ModS mod,
        string configName,
        in global::XM.Contracts.Config.ConfigParseContext context)
    {
        var list = new global::System.Collections.Generic.List<NestedConfig>();

        // 尝试从 XML 节点解析
        var nodes = configItem.SelectNodes("TestNestedConfig");
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

                // 类型 NestedConfig 不支持从文本解析
                continue;
            }
        }

        // 如果没有节点，尝试 CSV 格式
        if (list.Count == 0)
        {
            var csvValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "TestNestedConfig");
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

                    // 类型 NestedConfig 不支持从文本解析
                    continue;
                }
            }
        }

        return list;
    }

    /// <summary>
    /// 解析 Foreign 字段
    /// </summary>
    private static global::XM.Contracts.Config.CfgS<TestConfig> ParseForeign(
        global::System.Xml.XmlElement configItem,
        global::XM.Contracts.Config.ModS mod,
        string configName,
        in global::XM.Contracts.Config.ConfigParseContext context)
    {
        var xmlValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "Foreign");

        if (string.IsNullOrEmpty(xmlValue))
        {
            return default;
        }

        // 未知类型: XM.Contracts.Config.CfgS`1[[TestConfig, MyMod, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]]
        return default;
    }

    /// <summary>
    /// 解析 ConfigDict 字段
    /// </summary>
    private static global::System.Collections.Generic.Dictionary<int, global::System.Collections.Generic.Dictionary<int, global::System.Collections.Generic.List<NestedConfig>>> ParseConfigDict(
        global::System.Xml.XmlElement configItem,
        global::XM.Contracts.Config.ModS mod,
        string configName,
        in global::XM.Contracts.Config.ConfigParseContext context)
    {
        var dict = new global::System.Collections.Generic.Dictionary<int, global::System.Collections.Generic.Dictionary<int, global::System.Collections.Generic.List<NestedConfig>>>();

        // 解析 Dictionary Item 节点
        var dictNodes = configItem.SelectNodes("ConfigDict/Item");
        if (dictNodes != null)
        {
            foreach (var node in dictNodes)
            {
                var element = node as global::System.Xml.XmlElement;
                if (element == null)
                {
                    continue;
                }

                var keyText = element.GetAttribute("Key");
                var valueText = element.InnerText?.Trim();

                if (global::XM.Contracts.Config.ConfigParseHelper.TryParseInt(keyText, "ConfigDict.Key", out var parsedKey))
                {
                    // 嵌套容器不支持从文本解析: Dictionary<Int32, List<NestedConfig>>
                    continue;
                }
            }
        }

        return dict;
    }

    /// <summary>
    /// 解析 TestIndex1 字段
    /// </summary>
    private static int ParseTestIndex1(
        global::System.Xml.XmlElement configItem,
        global::XM.Contracts.Config.ModS mod,
        string configName,
        in global::XM.Contracts.Config.ConfigParseContext context)
    {
        var xmlValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "TestIndex1");

        if (string.IsNullOrEmpty(xmlValue))
        {
            return default;
        }

        if (global::XM.Contracts.Config.ConfigParseHelper.TryParseInt(xmlValue, "TestIndex1", out var parsedValue))
        {
            return parsedValue;
        }

        return default;
    }

    /// <summary>
    /// 解析 TestIndex2 字段
    /// </summary>
    private static global::XM.Contracts.Config.CfgS<TestConfig> ParseTestIndex2(
        global::System.Xml.XmlElement configItem,
        global::XM.Contracts.Config.ModS mod,
        string configName,
        in global::XM.Contracts.Config.ConfigParseContext context)
    {
        var xmlValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "TestIndex2");

        if (string.IsNullOrEmpty(xmlValue))
        {
            return default;
        }

        // 未知类型: XM.Contracts.Config.CfgS`1[[TestConfig, MyMod, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]]
        return default;
    }

    /// <summary>
    /// 解析 TestIndex3 字段
    /// </summary>
    private static global::XM.Contracts.Config.CfgS<TestConfig> ParseTestIndex3(
        global::System.Xml.XmlElement configItem,
        global::XM.Contracts.Config.ModS mod,
        string configName,
        in global::XM.Contracts.Config.ConfigParseContext context)
    {
        var xmlValue = global::XM.Contracts.Config.ConfigParseHelper.GetXmlFieldValue(configItem, "TestIndex3");

        if (string.IsNullOrEmpty(xmlValue))
        {
            return default;
        }

        // 未知类型: XM.Contracts.Config.CfgS`1[[TestConfig, MyMod, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null]]
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
        ref TestConfigUnmanaged data,
        XM.ConfigDataCenter.ConfigDataHolder configHolderData,
        XBlobPtr? linkParent = null)
    {
        var config = (TestConfig)value;

        // 分配容器和嵌套配置
        AllocTestSample(config, ref data, cfgi, configHolderData);
        AllocTestDictSample(config, ref data, cfgi, configHolderData);
        AllocTestKeyList(config, ref data, cfgi, configHolderData);
        AllocTestKeyList1(config, ref data, cfgi, configHolderData);
        AllocTestKeyList2(config, ref data, cfgi, configHolderData);
        AllocTestKeyHashSet(config, ref data, cfgi, configHolderData);
        AllocTestKeyDict(config, ref data, cfgi, configHolderData);
        AllocTestSetKey(config, ref data, cfgi, configHolderData);
        AllocTestSetSample(config, ref data, cfgi, configHolderData);
        FillTestNested(config, ref data, cfgi, configHolderData);
        AllocTestNestedConfig(config, ref data, cfgi, configHolderData);
        AllocConfigDict(config, ref data, cfgi, configHolderData);

        // 填充基本类型字段
        if (TryGetCfgI(config.Id, out var IdCfgI))
        {
            data.Id = IdCfgI.As<TestConfigUnmanaged>();
        }
        data.TestInt = config.TestInt;
        if (TryGetCfgI(config.Foreign, out var ForeignCfgI))
        {
            data.Foreign = ForeignCfgI.As<TestConfigUnmanaged>();
        }
        data.TestIndex1 = config.TestIndex1;
        if (TryGetCfgI(config.TestIndex2, out var TestIndex2CfgI))
        {
            data.TestIndex2 = TestIndex2CfgI.As<TestConfigUnmanaged>();
        }
        if (TryGetCfgI(config.TestIndex3, out var TestIndex3CfgI))
        {
            data.TestIndex3 = TestIndex3CfgI.As<TestConfigUnmanaged>();
        }
    }
    #region 容器分配和嵌套配置填充方法

    /// <summary>
    /// 分配 TestSample 容器
    /// </summary>
    private void AllocTestSample(TestConfig config, ref TestConfigUnmanaged data, CfgI cfgi, XM.ConfigDataCenter.ConfigDataHolder configHolderData)
    {
        if (config.TestSample == null || config.TestSample.Count == 0)
        {
            return;
        }

        var array = configHolderData.Data.BlobContainer.AllocArray<int>(config.TestSample.Count);
        for (int i = 0; i < config.TestSample.Count; i++)
        {
            var elemiDirect = config.TestSample[i];
            array[configHolderData.Data.BlobContainer, i] = elemiDirect;
        }

        data.TestSample = array;
    }
    /// <summary>
    /// 分配 TestDictSample 容器
    /// </summary>
    private void AllocTestDictSample(TestConfig config, ref TestConfigUnmanaged data, CfgI cfgi, XM.ConfigDataCenter.ConfigDataHolder configHolderData)
    {
        if (config.TestDictSample == null || config.TestDictSample.Count == 0)
        {
            return;
        }

        var map = configHolderData.Data.BlobContainer.AllocMap<int, int>(config.TestDictSample.Count);
        foreach (var kvp in config.TestDictSample)
        {
            var keyDirect = kvp.Key;
            var valueDirect = kvp.Value;
            map[configHolderData.Data.BlobContainer, keyDirect] = valueDirect;
        }

        data.TestDictSample = map;
    }
    /// <summary>
    /// 分配 TestKeyList 容器
    /// </summary>
    private void AllocTestKeyList(TestConfig config, ref TestConfigUnmanaged data, CfgI cfgi, XM.ConfigDataCenter.ConfigDataHolder configHolderData)
    {
        if (config.TestKeyList == null || config.TestKeyList.Count == 0)
        {
            return;
        }

        var array = configHolderData.Data.BlobContainer.AllocArray<CfgI<TestConfigUnmanaged>>(config.TestKeyList.Count);
        for (int i = 0; i < config.TestKeyList.Count; i++)
        {
            if (TryGetCfgI(config.TestKeyList[i], out var elemiCfgI))
            {
                var elemiConverted = elemiCfgI.As<TestConfigUnmanaged>();
                array[configHolderData.Data.BlobContainer, i] = elemiConverted;
            }
        }

        data.TestKeyList = array;
    }
    /// <summary>
    /// 分配 TestKeyList1 容器
    /// </summary>
    private void AllocTestKeyList1(TestConfig config, ref TestConfigUnmanaged data, CfgI cfgi, XM.ConfigDataCenter.ConfigDataHolder configHolderData)
    {
        if (config.TestKeyList1 == null || config.TestKeyList1.Count == 0)
        {
            return;
        }

        var map = configHolderData.Data.BlobContainer.AllocMap<int, global::XBlobArray<global::XBlobArray<CfgI<TestConfigUnmanaged>>>>(config.TestKeyList1.Count);
        foreach (var kvp in config.TestKeyList1)
        {
            var keyDirect = kvp.Key;
            var innerVal0 = kvp.Value;
            if (innerVal0 != null && innerVal0.Count > 0)
            {
                var tempVal = default(global::XBlobArray<global::XBlobArray<CfgI<TestConfigUnmanaged>>>);
                var innerArr_1 = configHolderData.Data.BlobContainer.AllocArray<global::XBlobArray<CfgI<TestConfigUnmanaged>>>(innerVal0.Count);
                for (int n1 = 0; n1 < innerVal0.Count; n1++)
                {
                    var inner1 = innerVal0[n1];
                    if (inner1 != null && inner1.Count > 0)
                    {
                        var temp_1 = default(global::XBlobArray<CfgI<TestConfigUnmanaged>>);
                        var innerArr_2 = configHolderData.Data.BlobContainer.AllocArray<CfgI<TestConfigUnmanaged>>(inner1.Count);
                        for (int n2 = 0; n2 < inner1.Count; n2++)
                        {
                            if (TryGetCfgI(inner1[n2], out var elemn2CfgI))
                            {
                                var elemn2Converted = elemn2CfgI.As<TestConfigUnmanaged>();
                                innerArr_2[configHolderData.Data.BlobContainer, n2] = elemn2Converted;
                            }
                        }
                        temp_1 = innerArr_2;
                        innerArr_1[configHolderData.Data.BlobContainer, n1] = temp_1;
                    }
                }
                tempVal = innerArr_1;
                map[configHolderData.Data.BlobContainer, keyDirect] = tempVal;
            }
        }

        data.TestKeyList1 = map;
    }
    /// <summary>
    /// 分配 TestKeyList2 容器
    /// </summary>
    private void AllocTestKeyList2(TestConfig config, ref TestConfigUnmanaged data, CfgI cfgi, XM.ConfigDataCenter.ConfigDataHolder configHolderData)
    {
        if (config.TestKeyList2 == null || config.TestKeyList2.Count == 0)
        {
            return;
        }

        var map = configHolderData.Data.BlobContainer.AllocMap<CfgI<TestConfigUnmanaged>, global::XBlobArray<global::XBlobArray<CfgI<TestConfigUnmanaged>>>>(config.TestKeyList2.Count);
        foreach (var kvp in config.TestKeyList2)
        {
            if (TryGetCfgI(kvp.Key, out var keyCfgI))
            {
                var keyConverted = keyCfgI.As<TestConfigUnmanaged>();
                var innerVal0 = kvp.Value;
                if (innerVal0 != null && innerVal0.Count > 0)
                {
                    var tempVal = default(global::XBlobArray<global::XBlobArray<CfgI<TestConfigUnmanaged>>>);
                    var innerArr_1 = configHolderData.Data.BlobContainer.AllocArray<global::XBlobArray<CfgI<TestConfigUnmanaged>>>(innerVal0.Count);
                    for (int n1 = 0; n1 < innerVal0.Count; n1++)
                    {
                        var inner1 = innerVal0[n1];
                        if (inner1 != null && inner1.Count > 0)
                        {
                            var temp_1 = default(global::XBlobArray<CfgI<TestConfigUnmanaged>>);
                            var innerArr_2 = configHolderData.Data.BlobContainer.AllocArray<CfgI<TestConfigUnmanaged>>(inner1.Count);
                            for (int n2 = 0; n2 < inner1.Count; n2++)
                            {
                                if (TryGetCfgI(inner1[n2], out var elemn2CfgI))
                                {
                                    var elemn2Converted = elemn2CfgI.As<TestConfigUnmanaged>();
                                    innerArr_2[configHolderData.Data.BlobContainer, n2] = elemn2Converted;
                                }
                            }
                            temp_1 = innerArr_2;
                            innerArr_1[configHolderData.Data.BlobContainer, n1] = temp_1;
                        }
                    }
                    tempVal = innerArr_1;
                    map[configHolderData.Data.BlobContainer, keyConverted] = tempVal;
                }
            }
        }

        data.TestKeyList2 = map;
    }
    /// <summary>
    /// 分配 TestKeyHashSet 容器
    /// </summary>
    private void AllocTestKeyHashSet(TestConfig config, ref TestConfigUnmanaged data, CfgI cfgi, XM.ConfigDataCenter.ConfigDataHolder configHolderData)
    {
        if (config.TestKeyHashSet == null || config.TestKeyHashSet.Count == 0)
        {
            return;
        }

        var set = configHolderData.Data.BlobContainer.AllocSet<int>(config.TestKeyHashSet.Count);
        foreach (var item in config.TestKeyHashSet)
        {
            var itemDirect = item;
            set.Add(configHolderData.Data.BlobContainer, itemDirect);
        }

        data.TestKeyHashSet = set;
    }
    /// <summary>
    /// 分配 TestKeyDict 容器
    /// </summary>
    private void AllocTestKeyDict(TestConfig config, ref TestConfigUnmanaged data, CfgI cfgi, XM.ConfigDataCenter.ConfigDataHolder configHolderData)
    {
        if (config.TestKeyDict == null || config.TestKeyDict.Count == 0)
        {
            return;
        }

        var map = configHolderData.Data.BlobContainer.AllocMap<CfgI<TestConfigUnmanaged>, CfgI<TestConfigUnmanaged>>(config.TestKeyDict.Count);
        foreach (var kvp in config.TestKeyDict)
        {
            if (TryGetCfgI(kvp.Key, out var keyCfgI))
            {
                var keyConverted = keyCfgI.As<TestConfigUnmanaged>();
                if (TryGetCfgI(kvp.Value, out var valueCfgI))
                {
                    var valueConverted = valueCfgI.As<TestConfigUnmanaged>();
                    map[configHolderData.Data.BlobContainer, keyConverted] = valueConverted;
                }
            }
        }

        data.TestKeyDict = map;
    }
    /// <summary>
    /// 分配 TestSetKey 容器
    /// </summary>
    private void AllocTestSetKey(TestConfig config, ref TestConfigUnmanaged data, CfgI cfgi, XM.ConfigDataCenter.ConfigDataHolder configHolderData)
    {
        if (config.TestSetKey == null || config.TestSetKey.Count == 0)
        {
            return;
        }

        var set = configHolderData.Data.BlobContainer.AllocSet<CfgI<TestConfigUnmanaged>>(config.TestSetKey.Count);
        foreach (var item in config.TestSetKey)
        {
            if (TryGetCfgI(item, out var itemCfgI))
            {
                var itemConverted = itemCfgI.As<TestConfigUnmanaged>();
                set.Add(configHolderData.Data.BlobContainer, itemConverted);
            }
        }

        data.TestSetKey = set;
    }
    /// <summary>
    /// 分配 TestSetSample 容器
    /// </summary>
    private void AllocTestSetSample(TestConfig config, ref TestConfigUnmanaged data, CfgI cfgi, XM.ConfigDataCenter.ConfigDataHolder configHolderData)
    {
        if (config.TestSetSample == null || config.TestSetSample.Count == 0)
        {
            return;
        }

        var set = configHolderData.Data.BlobContainer.AllocSet<int>(config.TestSetSample.Count);
        foreach (var item in config.TestSetSample)
        {
            var itemDirect = item;
            set.Add(configHolderData.Data.BlobContainer, itemDirect);
        }

        data.TestSetSample = set;
    }
    /// <summary>
    /// 填充 TestNested 嵌套配置
    /// </summary>
    private void FillTestNested(TestConfig config, ref TestConfigUnmanaged data, CfgI cfgi, XM.ConfigDataCenter.ConfigDataHolder configHolderData)
    {
        if (config.TestNested != null == false)
        {
            return;
        }

        var helper = NestedConfigClassHelper.Instance;
        if (helper != null)
        {
            var nestedData = new NestedConfigUnManaged();
            helper.AllocContainerWithFillImpl(config.TestNested, default(TblI), cfgi, ref nestedData, configHolderData);
            data.TestNested = nestedData;
        }
    }
    /// <summary>
    /// 分配 TestNestedConfig 容器
    /// </summary>
    private void AllocTestNestedConfig(TestConfig config, ref TestConfigUnmanaged data, CfgI cfgi, XM.ConfigDataCenter.ConfigDataHolder configHolderData)
    {
        if (config.TestNestedConfig == null || config.TestNestedConfig.Count == 0)
        {
            return;
        }

        var array = configHolderData.Data.BlobContainer.AllocArray<NestedConfigUnManaged>(config.TestNestedConfig.Count);
        for (int i = 0; i < config.TestNestedConfig.Count; i++)
        {
            if (config.TestNestedConfig[i] != null)
            {
                var helper_i = NestedConfigClassHelper.Instance;
                if (helper_i != null)
                {
                    var itemData_i = new NestedConfigUnManaged();
                    helper_i.AllocContainerWithFillImpl(config.TestNestedConfig[i], default(TblI), cfgi, ref itemData_i, configHolderData);
                    array[configHolderData.Data.BlobContainer, i] = itemData_i;
                }
            }
        }

        data.TestNestedConfig = array;
    }
    /// <summary>
    /// 分配 ConfigDict 容器
    /// </summary>
    private void AllocConfigDict(TestConfig config, ref TestConfigUnmanaged data, CfgI cfgi, XM.ConfigDataCenter.ConfigDataHolder configHolderData)
    {
        if (config.ConfigDict == null || config.ConfigDict.Count == 0)
        {
            return;
        }

        var map = configHolderData.Data.BlobContainer.AllocMap<int, global::XBlobMap<int, global::XBlobArray<NestedConfigUnManaged>>>(config.ConfigDict.Count);
        foreach (var kvp in config.ConfigDict)
        {
            var keyDirect = kvp.Key;
            var innerVal0 = kvp.Value;
            if (innerVal0 != null && innerVal0.Count > 0)
            {
                var tempVal = default(global::XBlobMap<int, global::XBlobArray<NestedConfigUnManaged>>);
                var innerArr_1 = configHolderData.Data.BlobContainer.AllocMap<int, global::XBlobArray<NestedConfigUnManaged>>(innerVal0.Count);
                foreach (var kvp_1 in innerVal0)
                {
                    var key_1Direct = kvp_1.Key;
                    var innerVal1 = kvp_1.Value;
                    if (innerVal1 != null && innerVal1.Count > 0)
                    {
                        var tempVal_1 = default(global::XBlobArray<NestedConfigUnManaged>);
                        var innerArr_2 = configHolderData.Data.BlobContainer.AllocArray<NestedConfigUnManaged>(innerVal1.Count);
                        for (int n2 = 0; n2 < innerVal1.Count; n2++)
                        {
                            if (innerVal1[n2] != null)
                            {
                                var helper_n2 = NestedConfigClassHelper.Instance;
                                if (helper_n2 != null)
                                {
                                    var itemData_n2 = new NestedConfigUnManaged();
                                    helper_n2.AllocContainerWithFillImpl(innerVal1[n2], default(TblI), cfgi, ref itemData_n2, configHolderData);
                                    innerArr_2[configHolderData.Data.BlobContainer, n2] = itemData_n2;
                                }
                            }
                        }
                        tempVal_1 = innerArr_2;
                        innerArr_1[configHolderData.Data.BlobContainer, key_1Direct] = tempVal_1;
                    }
                }
                tempVal = innerArr_1;
                map[configHolderData.Data.BlobContainer, keyDirect] = tempVal;
            }
        }

        data.ConfigDict = map;
    }

    #endregion

    #region 索引初始化和查询方法

    /// <summary>
    /// 初始化索引并填充数据
    /// </summary>
    /// <param name="configData">配置数据容器</param>
    /// <param name="tableMap">表的主数据 Map (CfgI -> TUnmanaged)</param>
    public void InitializeIndexes(
        ref XM.ConfigData configData,
        XBlobMap<CfgI, TestConfigUnmanaged> tableMap)
    {
        // 获取配置数量
        int configCount = tableMap.GetLength(configData.BlobContainer);

        // 初始化索引: Index1
        // 申请 Map 容器，容量为配置数量
        var indexIndex1Map = configData.AllocIndex<TestConfigUnmanaged.Index1Index, TestConfigUnmanaged>(TestConfigUnmanaged.Index1Index.IndexType, configCount);

        // 初始化索引: Index2
        // 索引字段 TestIndex3 为 CfgS 类型，自动作为索引
        // 申请 MultiMap 容器，容量为配置数量
        var indexIndex2Map = configData.AllocMultiIndex<TestConfigUnmanaged.Index2Index, TestConfigUnmanaged>(TestConfigUnmanaged.Index2Index.IndexType, configCount);

        // 遍历所有配置，填充索引
        for (int i = 0; i < configCount; i++)
        {
            var cfgId = tableMap.GetKey(configData.BlobContainer, i);
            ref var data = ref tableMap.GetRef(configData.BlobContainer, cfgId, out bool exists);
            if (!exists) continue;

            // 填充索引: Index1
            var indexKeyIndex1 = new TestConfigUnmanaged.Index1Index(data.TestIndex1, data.TestIndex2);
            if (!indexIndex1Map.AddOrUpdate(configData.BlobContainer, indexKeyIndex1, cfgId))
            {
                UnityEngine.Debug.LogWarning($"索引 Index1 存在重复键: {indexKeyIndex1}");
            }

            // 填充索引: Index2
            var indexKeyIndex2 = new TestConfigUnmanaged.Index2Index(data.TestIndex3);
            indexIndex2Map.Add(configData.BlobContainer, indexKeyIndex2, cfgId);

        }
    }

    #endregion


    /// <summary>配置定义所属的 Mod</summary>
    public TblI _definedInMod;
}
