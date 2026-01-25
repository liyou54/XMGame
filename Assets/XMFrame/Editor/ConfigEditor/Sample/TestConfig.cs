using System;
using System.Collections.Generic;
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


public class NestedConfig : XConfig<NestedConfig, TestConfigUnManaged>
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
    public int Test;
    public int2 Test1;
    public int2 TestGlobalConvert;
    public XBlobArray<CfgId<TestConfigUnManaged>> TestKeyList;
    public FixedString32Bytes Str32;
    public FixedString64Bytes Str64;
    public StrHandle Str;
    public StrLabelHandle StrLabel;
}



public class TestConfig : XConfig<TestConfig, TestConfigUnManaged>
{
    public ConfigKey<TestConfigUnManaged> Id;
    public int TestInt;
    public List<int> TestSample;
    public Dictionary<int, int> TestDictSample;
    public List<ConfigKey<TestConfigUnManaged>> TestKeyList;
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

// public partial struct TestConfigUnManaged
// {
//     public CfgId<TestConfigUnManaged> Id;
//     public XBlobArray<int> TestSample;
//     public int TestInt;
//     public XBlobMap<int, int> TestDictSample;
//     public XBlobArray<CfgId<TestConfigUnManaged>> TestKeyList;
//     public XBlobArray<XBlobPtr<TestConfigUnManaged>> TestKeyList_Ref;
//     public XBlobSet<int> TestKeyHashSet;
//     public XBlobMap<CfgId<TestConfigUnManaged>, CfgId<TestConfigUnManaged>> TestKeyDict;
//     public XBlobSet<CfgId<TestConfigUnManaged>> TestSetKey;
//     public XBlobSet<int> TestSetSample;
//     public NestedConfigUnManaged TestNested;
//     public XBlobArray<NestedConfigUnManaged> TestNestedConfig;
//     public CfgId<TestConfigUnManaged> Foreign;
//     public XBlobPtr<TestConfigUnManaged> Foreign_Ref;
//     public TypeId ConfigType;
//
//     public int TestIndex1;
//     public CfgId<TestConfigUnManaged> TestIndex2;
//     public CfgId<TestConfigUnManaged> TestIndex3;
// }

// public partial struct TestConfigUnManaged
// {
//     public struct Index1Index:IConfigIndexGroup<TestConfigUnManaged>,IEquatable<Index1Index>
//     {
//         public int TestIndex1;
//         public CfgId<TestConfigUnManaged> TestIndex2;
//
//         public Index1Index(int testIndex1, CfgId<TestConfigUnManaged> testIndex2)
//         {
//             TestIndex1 = testIndex1;
//             TestIndex2 = testIndex2;
//         }
//
//
//         public bool Equals(Index1Index other)
//         {
//             return TestIndex1 == other.TestIndex1 && TestIndex2.Equals(other.TestIndex2);
//         }
//
//         public override bool Equals(object obj)
//         {
//             return obj is Index1Index other && Equals(other);
//         }
//
//         public override int GetHashCode()
//         {
//             return HashCode.Combine(TestIndex1, TestIndex2);
//         }
//     }
// }
//
// public partial struct TestConfigUnManaged
// {
//     public struct Index2Index:IConfigIndexGroup<TestConfigUnManaged>,IEquatable<Index2Index>
//     {
//         public CfgId<TestConfigUnManaged> TestIndex3;
//
//         public Index2Index(CfgId<TestConfigUnManaged> testIndex3)
//         {
//             TestIndex3 = testIndex3;
//         }
//         public bool Equals(Index2Index other)
//         {
//             return TestIndex3.Equals(other.TestIndex3);
//         }
//
//         public override bool Equals(object obj)
//         {
//             return obj is Index2Index other && Equals(other);
//         }
//
//         public override int GetHashCode()
//         {
//             return TestIndex3.GetHashCode();
//         }
//     }
// }