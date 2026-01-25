using XMFrame.Utils;
using System;
using XMFrame;
public partial struct TestConfigUnManaged : IConfigUnManaged<TestConfigUnManaged>
{
}public partial struct TestConfigUnManaged
{
    public CfgId<TestConfigUnManaged> Id;
    public XBlobPtr<TestConfigUnManaged> Id_Ref;
    public Int32 TestInt;
    public XBlobArray<Int32> TestSample;
    public XBlobMap<Int32, Int32> TestDictSample;
    public XBlobArray<CfgId<TestConfigUnManaged>> TestKeyList;
    public XBlobMap<Int32, XBlobArray<XBlobArray<CfgId<TestConfigUnManaged>>>> TestKeyList1;
    public XBlobSet<Int32> TestKeyHashSet;
    public XBlobMap<CfgId<TestConfigUnManaged>, CfgId<TestConfigUnManaged>> TestKeyDict;
    public XBlobSet<CfgId<TestConfigUnManaged>> TestSetKey;
    public XBlobSet<Int32> TestSetSample;
    public NestedConfigUnManaged TestNested;
    public XBlobArray<NestedConfigUnManaged> TestNestedConfig;
    public CfgId<TestConfigUnManaged> Foreign;
    public XBlobPtr<TestConfigUnManaged> Foreign_Ref;
    public TypeId ConfigType;
    public Int32 TestIndex1;
    public CfgId<TestConfigUnManaged> TestIndex2;
    public XBlobPtr<TestConfigUnManaged> TestIndex2_Ref;
    public CfgId<TestConfigUnManaged> TestIndex3;
    public XBlobPtr<TestConfigUnManaged> TestIndex3_Ref;
}public partial struct TestConfigUnManaged
{
    public struct Index1Index : IConfigIndexGroup<TestConfigUnManaged>, IEquatable<Index1Index>
    {
        public Int32 TestIndex1;
        public CfgId<TestConfigUnManaged> TestIndex2;

        public Index1Index(Int32 testIndex1,CfgId<TestConfigUnManaged> testIndex2)
        {
            TestIndex1 = testIndex1;
            TestIndex2 = testIndex2;
        }

        public bool Equals(Index1Index other)
        {
            return TestIndex1.Equals(other.TestIndex1) && TestIndex2.Equals(other.TestIndex2);
        }

        public override bool Equals(object obj)
        {
            return obj is Index1Index other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TestIndex1,TestIndex2);
        }
    }
}
public partial struct TestConfigUnManaged
{
    public struct Index2Index : IConfigIndexGroup<TestConfigUnManaged>, IEquatable<Index2Index>
    {
        public CfgId<TestConfigUnManaged> TestIndex3;

        public Index2Index(CfgId<TestConfigUnManaged> testIndex3)
        {
            TestIndex3 = testIndex3;
        }

        public bool Equals(Index2Index other)
        {
            return TestIndex3.Equals(other.TestIndex3);
        }

        public override bool Equals(object obj)
        {
            return obj is Index2Index other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TestIndex3);
        }
    }
}

