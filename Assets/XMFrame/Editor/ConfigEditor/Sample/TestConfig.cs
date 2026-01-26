using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Collections;
using Unity.Mathematics;
using XMFrame;
using XMFrame.Utils;
using XMFrame.Utils.Attribute;
using XMFrame.Interfaces.ConfigMananger;

[assembly: XmlGlobalConvert(typeof(TestGlobalInt2Convert), "global")]



/// <summary>
/// Type 到 TypeId 的转换器
/// </summary>
public class TypeToTypeIdConverter : XmlUnManagedConvert<Type, TypeId>
{
    public override TypeId Convert(Type source)
    {
        if (source == null)
        {
            return new TypeId(0);
        }
        return TypeSystem.RegisterType(source);
    }
}

public class TestGlobalInt2Convert : XmlConvertBase<int2,TestInt2Convert>
{
    public override bool TryGetData(string str, out int2 data)
    {
        throw new System.NotImplementedException();
    }
}

public class TestInt2Convert : XmlConvertBase<int2,TestInt2Convert>
{
    
    public override bool TryGetData(string str, out int2 data)
    {
        throw new System.NotImplementedException();
    }
}

[XmlDefined()]
public class NestedConfig : XConfig<NestedConfig, NestedConfigUnManaged>
{
    public int Test;

    [XmlGlobalConvert(typeof(TestInt2Convert))]
    public int2 TestCustom;

    public int2 TestGlobalConvert;
    public List<ConfigKey<TestConfigUnManaged>> TestKeyList;

    [XmlStringMode(EXmlStrMode.EStrHandle)]
    public string StrIndex;

    [XmlStringMode(EXmlStrMode.EFix32)] public string Str32;
    [XmlStringMode(EXmlStrMode.EFix64)] public string Str64;

    [XmlStringMode(EXmlStrMode.EStrHandle)]
    public string Str;

    public StrLabel StrLabel;
}

public partial struct NestedConfigUnManaged : IConfigUnManaged<NestedConfigUnManaged>
{

}


[XmlDefined()]
public class TestConfig : XConfig<TestConfig, TestConfigUnManaged>
{
    public ConfigKey<TestConfigUnManaged> Id;
    public int TestInt;
    public List<int> TestSample;
    public Dictionary<int, int> TestDictSample;
    public List<ConfigKey<TestConfigUnManaged>> TestKeyList;
    public Dictionary<int, List<List<ConfigKey<TestConfigUnManaged>>>> TestKeyList1;
    public HashSet<int> TestKeyHashSet;
    public Dictionary<ConfigKey<TestConfigUnManaged>, ConfigKey<TestConfigUnManaged>> TestKeyDict;
    public HashSet<ConfigKey<TestConfigUnManaged>> TestSetKey;
    public HashSet<int> TestSetSample;
    public NestedConfig TestNested;
    public List<NestedConfig> TestNestedConfig;
    public ConfigKey<TestConfigUnManaged> Foreign;
    public Type ConfigType;

    [XmlIndex("Index1", false, 0)] public int TestIndex1;
    [XmlIndex("Index1", false, 1)] public ConfigKey<TestConfigUnManaged> TestIndex2;
    [XmlIndex("Index2", true, 0)] public ConfigKey<TestConfigUnManaged> TestIndex3;
}

public partial struct TestConfigUnManaged : IConfigUnManaged<TestConfigUnManaged>
{
}
