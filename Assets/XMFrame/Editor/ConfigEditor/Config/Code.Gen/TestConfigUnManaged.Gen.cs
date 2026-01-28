using System;
using System.Collections.Generic;
using System.Xml;
using XM;
using XM.Contracts;
using XM.Contracts.Config;
using XM.Utils;
public partial struct TestConfigUnManaged : global::XM.IConfigUnManaged<TestConfigUnManaged>
{
}public partial struct TestConfigUnManaged
{
    public CfgI<TestConfigUnManaged> Id;
    public global::XBlobPtr<TestConfigUnManaged> Id_Ref;
    public Int32 TestInt;
    public XBlobArray<Int32> TestSample;
    public XBlobMap<Int32, Int32> TestDictSample;
    public XBlobArray<CfgI<TestConfigUnManaged>> TestKeyList;
    public XBlobMap<Int32, XBlobArray<XBlobArray<CfgI<TestConfigUnManaged>>>> TestKeyList1;
    public XBlobSet<Int32> TestKeyHashSet;
    public XBlobMap<CfgI<TestConfigUnManaged>, CfgI<TestConfigUnManaged>> TestKeyDict;
    public XBlobSet<CfgI<TestConfigUnManaged>> TestSetKey;
    public XBlobSet<Int32> TestSetSample;
    public NestedConfigUnManaged TestNested;
    public XBlobArray<NestedConfigUnManaged> TestNestedConfig;
    public CfgI<TestConfigUnManaged> Foreign;
    public global::XBlobPtr<TestConfigUnManaged> Foreign_Ref;
    public TypeI ConfigType;
    public Int32 TestIndex1;
    public CfgI<TestConfigUnManaged> TestIndex2;
    public global::XBlobPtr<TestConfigUnManaged> TestIndex2_Ref;
    public CfgI<TestConfigUnManaged> TestIndex3;
    public global::XBlobPtr<TestConfigUnManaged> TestIndex3_Ref;
}public partial struct TestConfigUnManaged
{
    public struct Index1Index : global::XM.IConfigIndexGroup<TestConfigUnManaged>, global::System.IEquatable<Index1Index>
    {
        public Int32 TestIndex1;
        public CfgI<TestConfigUnManaged> TestIndex2;

        public Index1Index(Int32 testIndex1,CfgI<TestConfigUnManaged> testIndex2)
        {
            TestIndex1 = testIndex1;
            TestIndex2 = testIndex2;
        }

        public bool Equals(Index1Index other)
        {
            return TestIndex1.Equals(other.TestIndex1) && TestIndex2.Equals(other.TestIndex2);
        }

        public override bool Equals(global::System.Object obj)
        {
            return obj is Index1Index other && Equals(other);
        }

        public override int GetHashCode()
        {
            return global::System.HashCode.Combine(TestIndex1,TestIndex2);
        }
    }
}
public partial struct TestConfigUnManaged
{
    public struct Index2Index : global::XM.IConfigIndexGroup<TestConfigUnManaged>, global::System.IEquatable<Index2Index>
    {
        public CfgI<TestConfigUnManaged> TestIndex3;

        public Index2Index(CfgI<TestConfigUnManaged> testIndex3)
        {
            TestIndex3 = testIndex3;
        }

        public bool Equals(Index2Index other)
        {
            return TestIndex3.Equals(other.TestIndex3);
        }

        public override bool Equals(global::System.Object obj)
        {
            return obj is Index2Index other && Equals(other);
        }

        public override int GetHashCode()
        {
            return global::System.HashCode.Combine(TestIndex3);
        }
    }
}

