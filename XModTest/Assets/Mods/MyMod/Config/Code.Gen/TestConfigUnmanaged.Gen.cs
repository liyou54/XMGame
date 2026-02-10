using System;
using Unity.Collections;
using XM;
using XM.Contracts.Config;

/// <summary>
/// TestConfig 的非托管数据结构 (代码生成)
/// </summary>
public partial struct TestConfigUnmanaged : IConfigUnManaged<TestConfigUnmanaged>
{
    // 字段

    public CfgI<TestConfigUnmanaged> Id;
    public int TestInt;
    /// <summary>容器</summary>
    public global::XBlobArray<int> TestSample;
    /// <summary>容器</summary>
    public global::XBlobMap<int, int> TestDictSample;
    /// <summary>容器</summary>
    public global::XBlobArray<CfgI<TestConfigUnmanaged>> TestKeyList;
    /// <summary>容器</summary>
    public global::XBlobMap<int, global::XBlobArray<global::XBlobArray<CfgI<TestConfigUnmanaged>>>> TestKeyList1;
    /// <summary>容器</summary>
    public global::XBlobMap<CfgI<TestConfigUnmanaged>, global::XBlobArray<global::XBlobArray<CfgI<TestConfigUnmanaged>>>> TestKeyList2;
    /// <summary>容器</summary>
    public global::XBlobSet<int> TestKeyHashSet;
    /// <summary>容器</summary>
    public global::XBlobMap<CfgI<TestConfigUnmanaged>, CfgI<TestConfigUnmanaged>> TestKeyDict;
    /// <summary>容器</summary>
    public global::XBlobSet<CfgI<TestConfigUnmanaged>> TestSetKey;
    /// <summary>容器</summary>
    public global::XBlobSet<int> TestSetSample;
    /// <summary>嵌套配置</summary>
    public NestedConfigUnManaged TestNested;
    /// <summary>容器, 嵌套配置</summary>
    public global::XBlobArray<NestedConfigUnManaged> TestNestedConfig;
    public CfgI<TestConfigUnmanaged> Foreign;
    /// <summary>容器, 嵌套配置</summary>
    public global::XBlobMap<int, global::XBlobMap<int, global::XBlobArray<NestedConfigUnManaged>>> ConfigDict;
    /// <summary>索引: Index1</summary>
    public int TestIndex1;
    /// <summary>索引: Index1</summary>
    public CfgI<TestConfigUnmanaged> TestIndex2;
    /// <summary>索引: Index2</summary>
    public CfgI<TestConfigUnmanaged> TestIndex3;

    /// <summary>
    /// ToString方法
    /// </summary>
    /// <param name="dataContainer">数据容器</param>
    public string ToString(object dataContainer)
    {
        return "TestConfig";
    }
}
