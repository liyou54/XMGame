using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Collections;
using Unity.Mathematics;
using XM;
using XM.Contracts;
using XM.Utils;
using XM.Utils.Attribute;
using XM.Contracts.Config;




[XmlDefined()]
public class NestedConfig : IXConfig<NestedConfig, NestedConfigUnManaged>
{
    /// <summary>必要字段：XML 缺失时打告警，仍使用默认值 0。</summary>
    [XmlNotNull]
    public int RequiredId;

    /// <summary>可选字段：XML 缺失或空时使用 [XmlDefault] 默认值。</summary>
    [XmlDefault("default")]
    public string OptionalWithDefault;

    public int Test;

    public int2 TestCustom;

    public int2 TestGlobalConvert;
    public List<CfgS<TestConfigUnManaged>> TestKeyList;

    [XmlStringMode(EXmlStrMode.ELabelI)]
    public string StrIndex;

    [XmlStringMode(EXmlStrMode.EFix32)] public string Str32;
    [XmlStringMode(EXmlStrMode.EFix64)] public string Str64;

    [XmlStringMode(EXmlStrMode.EStrI)]
    public string Str;

    public LabelS LabelS;
    public CfgI Data { get; set; }
}

public partial struct NestedConfigUnManaged : IConfigUnManaged<NestedConfigUnManaged>
{

}


[XmlDefined()]
public class TestConfig : IXConfig<TestConfig, TestConfigUnManaged>
{
    public CfgI Data { get; set; }

    public CfgS<TestConfigUnManaged> Id;
    public int TestInt;
    public List<int> TestSample;
    public Dictionary<int, int> TestDictSample;
    public List<CfgS<TestConfigUnManaged>> TestKeyList;
    public Dictionary<int, List<List<CfgS<TestConfigUnManaged>>>> TestKeyList1;
    public Dictionary<CfgS<TestConfigUnManaged>, List<List<CfgS<TestConfigUnManaged>>>> TestKeyList2;
    public HashSet<int> TestKeyHashSet;
    public Dictionary<CfgS<TestConfigUnManaged>, CfgS<TestConfigUnManaged>> TestKeyDict;
    public HashSet<CfgS<TestConfigUnManaged>> TestSetKey;
    public HashSet<int> TestSetSample;
    public NestedConfig TestNested;
    public List<NestedConfig> TestNestedConfig;
    public CfgS<TestConfigUnManaged> Foreign;
    
    public Dictionary<int, Dictionary<int, List<NestedConfig>>> ConfigDict;
    
    [XmlIndex("Index1", false, 0)] public int TestIndex1;
    [XmlIndex("Index1", false, 1)] public CfgS<TestConfigUnManaged> TestIndex2;
    [XmlIndex("Index2", true, 0)] public CfgS<TestConfigUnManaged> TestIndex3;
}

public partial struct TestConfigUnManaged : IConfigUnManaged<TestConfigUnManaged>
{

}