using System;
using System.Collections.Generic;
using System.Xml;
using XM;
using XM.Contracts;
using XM.Contracts.Config;


public partial struct TestConfigUnManaged : global::XM.IConfigUnManaged<TestConfigUnManaged>
{
}

public partial struct TestConfigUnManaged
{
    public CfgI<TestConfigUnManaged> Id;
    public global::XBlobPtr<TestConfigUnManaged> Id_Ref;
    public Int32 TestInt;
    public XBlobArray<Int32> TestSample;
    public XBlobMap<Int32, Int32> TestDictSample;
    public XBlobArray<CfgI<TestConfigUnManaged>> TestKeyList;
    public XBlobMap<Int32, XBlobArray<XBlobArray<CfgI<TestConfigUnManaged>>>> TestKeyList1;
    public XBlobMap<CfgI<TestConfigUnManaged>, XBlobArray<XBlobArray<CfgI<TestConfigUnManaged>>>> TestKeyList2;
    public XBlobSet<Int32> TestKeyHashSet;
    public XBlobMap<CfgI<TestConfigUnManaged>, CfgI<TestConfigUnManaged>> TestKeyDict;
    public XBlobSet<CfgI<TestConfigUnManaged>> TestSetKey;
    public XBlobSet<Int32> TestSetSample;
    public NestedConfigUnManaged TestNested;
    public XBlobArray<NestedConfigUnManaged> TestNestedConfig;
    public CfgI<TestConfigUnManaged> Foreign;
    public global::XBlobPtr<TestConfigUnManaged> Foreign_Ref;
    public Int32 TestIndex1;
    public CfgI<TestConfigUnManaged> TestIndex2;
    public global::XBlobPtr<TestConfigUnManaged> TestIndex2_Ref;
    public CfgI<TestConfigUnManaged> TestIndex3;
    public global::XBlobPtr<TestConfigUnManaged> TestIndex3_Ref;
}


public partial struct TestConfigUnManaged
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


#if DEBUG || UNITY_EDITOR
public partial struct TestConfigUnManaged
{
    /// <summary>无容器参数的 ToString：容器只显示类型和长度</summary>
    public override string ToString()
    {
        return ToString(null);
    }

    /// <summary>带容器参数的 ToString：打印完整容器内容（实现接口方法）</summary>
    public string ToString(object dataContainer)
    {
        var container = dataContainer as global::XBlobContainer?;
        var sb = new global::System.Text.StringBuilder();
        sb.Append("TestConfigUnManaged {");
        // CfgI：打印为 CfgS（模块::配置名）
        sb.Append("Id=");
        AppendCfgI(sb, Id);
        sb.Append(", ");
        // 普通字段：直接打印
        sb.Append("TestInt=" + TestInt);
        sb.Append(", ");
        // XBlobArray
        sb.Append("TestSample=");
        if (container.HasValue)
            AppendArray_TestSample(sb, container.Value, TestSample);
        else
            sb.Append("XBlobArray<Int32>[?]");
        sb.Append(", ");
        // XBlobMap
        sb.Append("TestDictSample=");
        if (container.HasValue)
            AppendMap_TestDictSample(sb, container.Value, TestDictSample);
        else
            sb.Append("XBlobMap<Int32, Int32>[?]");
        sb.Append(", ");
        // XBlobArray
        sb.Append("TestKeyList=");
        if (container.HasValue)
            AppendArray_TestKeyList(sb, container.Value, TestKeyList);
        else
            sb.Append("XBlobArray<CfgI<TestConfigUnManaged>>[?]");
        sb.Append(", ");
        // XBlobMap
        sb.Append("TestKeyList1=");
        if (container.HasValue)
            AppendMap_TestKeyList1(sb, container.Value, TestKeyList1);
        else
            sb.Append("XBlobMap<Int32, XBlobArray<XBlobArray<CfgI<TestConfigUnManaged>>>>[?]");
        sb.Append(", ");
        // XBlobMap
        sb.Append("TestKeyList2=");
        if (container.HasValue)
            AppendMap_TestKeyList2(sb, container.Value, TestKeyList2);
        else
            sb.Append("XBlobMap<CfgI<TestConfigUnManaged>, XBlobArray<XBlobArray<CfgI<TestConfigUnManaged>>>>[?]");
        sb.Append(", ");
        // XBlobSet
        sb.Append("TestKeyHashSet=");
        if (container.HasValue)
            AppendSet_TestKeyHashSet(sb, container.Value, TestKeyHashSet);
        else
            sb.Append("XBlobSet<Int32>[?]");
        sb.Append(", ");
        // XBlobMap
        sb.Append("TestKeyDict=");
        if (container.HasValue)
            AppendMap_TestKeyDict(sb, container.Value, TestKeyDict);
        else
            sb.Append("XBlobMap<CfgI<TestConfigUnManaged>, CfgI<TestConfigUnManaged>>[?]");
        sb.Append(", ");
        // XBlobSet
        sb.Append("TestSetKey=");
        if (container.HasValue)
            AppendSet_TestSetKey(sb, container.Value, TestSetKey);
        else
            sb.Append("XBlobSet<CfgI<TestConfigUnManaged>>[?]");
        sb.Append(", ");
        // XBlobSet
        sb.Append("TestSetSample=");
        if (container.HasValue)
            AppendSet_TestSetSample(sb, container.Value, TestSetSample);
        else
            sb.Append("XBlobSet<Int32>[?]");
        sb.Append(", ");
        // 普通字段：直接打印
        sb.Append("TestNested=" + TestNested);
        sb.Append(", ");
        // XBlobArray
        sb.Append("TestNestedConfig=");
        if (container.HasValue)
            AppendArray_TestNestedConfig(sb, container.Value, TestNestedConfig);
        else
            sb.Append("XBlobArray<NestedConfigUnManaged>[?]");
        sb.Append(", ");
        // CfgI：打印为 CfgS（模块::配置名）
        sb.Append("Foreign=");
        AppendCfgI(sb, Foreign);
        sb.Append(", ");
        // 普通字段：直接打印
        sb.Append("TestIndex1=" + TestIndex1);
        sb.Append(", ");
        // CfgI：打印为 CfgS（模块::配置名）
        sb.Append("TestIndex2=");
        AppendCfgI(sb, TestIndex2);
        sb.Append(", ");
        // CfgI：打印为 CfgS（模块::配置名）
        sb.Append("TestIndex3=");
        AppendCfgI(sb, TestIndex3);
        sb.Append(" }");
        return sb.ToString();
    }

    /// <summary>辅助方法：打印 CfgI（尝试转为 CfgS）</summary>
    private static void AppendCfgI<T>(global::System.Text.StringBuilder sb, global::CfgI<T> cfgi)
        where T : unmanaged, global::XM.IConfigUnManaged<T>
    {
        var dataCenter = global::XM.Contracts.IConfigDataCenter.I;
        if (dataCenter != null && dataCenter.TryGetCfgS(cfgi.AsNonGeneric(), out var cfgs))
        {
            sb.Append(cfgs.ToString()); // "模块::配置名"
        }
        else
        {
            sb.Append(cfgi.ToString()); // 回退到 "CfgI(Id)"
        }
    }

    /// <summary>辅助方法：打印 XBlobArray TestSample</summary>
    private static void AppendArray_TestSample(global::System.Text.StringBuilder sb, global::XBlobContainer container, XBlobArray<Int32> arr)
    {
        int len = arr.GetLength(container);
        sb.Append("[");
        int maxPrint = global::System.Math.Min(len, 10); // 最多打印 10 个元素
        for (int i = 0; i < maxPrint; i++)
        {
            if (i > 0) sb.Append(", ");
            var elem = arr[container, i];
            sb.Append(elem);
        }
        if (len > maxPrint) sb.Append($", ...({len - maxPrint} more)");
        sb.Append("]");
    }

    /// <summary>辅助方法：打印 XBlobMap TestDictSample</summary>
    private static void AppendMap_TestDictSample(global::System.Text.StringBuilder sb, global::XBlobContainer container, XBlobMap<Int32, Int32> map)
    {
        int len = map.GetLength(container);
        sb.Append("{");
        // Map 打印前 5 对
        int maxPrint = global::System.Math.Min(len, 5);
        int printed = 0;
        foreach (var kvp in map.GetEnumerator(container))
        {
            if (printed >= maxPrint) break;
            if (printed > 0) sb.Append(", ");
            sb.Append(kvp.Key);
            sb.Append(":");
            sb.Append(kvp.Value);
            printed++;
        }
        if (len > maxPrint) sb.Append($", ...({len - maxPrint} more)");
        sb.Append("}");
    }

    /// <summary>辅助方法：打印 XBlobArray TestKeyList</summary>
    private static void AppendArray_TestKeyList(global::System.Text.StringBuilder sb, global::XBlobContainer container, XBlobArray<CfgI<TestConfigUnManaged>> arr)
    {
        int len = arr.GetLength(container);
        sb.Append("[");
        int maxPrint = global::System.Math.Min(len, 10); // 最多打印 10 个元素
        for (int i = 0; i < maxPrint; i++)
        {
            if (i > 0) sb.Append(", ");
            var elem = arr[container, i];
            AppendCfgI(sb, elem);
        }
        if (len > maxPrint) sb.Append($", ...({len - maxPrint} more)");
        sb.Append("]");
    }

    /// <summary>辅助方法：打印 XBlobMap TestKeyList1</summary>
    private static void AppendMap_TestKeyList1(global::System.Text.StringBuilder sb, global::XBlobContainer container, XBlobMap<Int32, XBlobArray<XBlobArray<CfgI<TestConfigUnManaged>>>> map)
    {
        int len = map.GetLength(container);
        sb.Append("{");
        // Map 打印前 5 对
        int maxPrint = global::System.Math.Min(len, 5);
        int printed = 0;
        foreach (var kvp in map.GetEnumerator(container))
        {
            if (printed >= maxPrint) break;
            if (printed > 0) sb.Append(", ");
            sb.Append(kvp.Key);
            sb.Append(":");
            sb.Append(kvp.Value);
            printed++;
        }
        if (len > maxPrint) sb.Append($", ...({len - maxPrint} more)");
        sb.Append("}");
    }

    /// <summary>辅助方法：打印 XBlobMap TestKeyList2</summary>
    private static void AppendMap_TestKeyList2(global::System.Text.StringBuilder sb, global::XBlobContainer container, XBlobMap<CfgI<TestConfigUnManaged>, XBlobArray<XBlobArray<CfgI<TestConfigUnManaged>>>> map)
    {
        int len = map.GetLength(container);
        sb.Append("{");
        // Map 打印前 5 对
        int maxPrint = global::System.Math.Min(len, 5);
        int printed = 0;
        foreach (var kvp in map.GetEnumerator(container))
        {
            if (printed >= maxPrint) break;
            if (printed > 0) sb.Append(", ");
            AppendCfgI(sb, kvp.Key);
            sb.Append(":");
            sb.Append(kvp.Value);
            printed++;
        }
        if (len > maxPrint) sb.Append($", ...({len - maxPrint} more)");
        sb.Append("}");
    }

    /// <summary>辅助方法：打印 XBlobSet TestKeyHashSet</summary>
    private static void AppendSet_TestKeyHashSet(global::System.Text.StringBuilder sb, global::XBlobContainer container, XBlobSet<Int32> set)
    {
        int len = set.GetLength(container);
        sb.Append("{");
        int maxPrint = global::System.Math.Min(len, 10);
        for (int i = 0; i < maxPrint && i < len; i++)
        {
            if (i > 0) sb.Append(", ");
            var elem = set[container, i];
            sb.Append(elem);
        }
        if (len > maxPrint) sb.Append($", ...({len - maxPrint} more)");
        sb.Append("}");
    }

    /// <summary>辅助方法：打印 XBlobMap TestKeyDict</summary>
    private static void AppendMap_TestKeyDict(global::System.Text.StringBuilder sb, global::XBlobContainer container, XBlobMap<CfgI<TestConfigUnManaged>, CfgI<TestConfigUnManaged>> map)
    {
        int len = map.GetLength(container);
        sb.Append("{");
        // Map 打印前 5 对
        int maxPrint = global::System.Math.Min(len, 5);
        int printed = 0;
        foreach (var kvp in map.GetEnumerator(container))
        {
            if (printed >= maxPrint) break;
            if (printed > 0) sb.Append(", ");
            AppendCfgI(sb, kvp.Key);
            sb.Append(":");
            AppendCfgI(sb, kvp.Value);
            printed++;
        }
        if (len > maxPrint) sb.Append($", ...({len - maxPrint} more)");
        sb.Append("}");
    }

    /// <summary>辅助方法：打印 XBlobSet TestSetKey</summary>
    private static void AppendSet_TestSetKey(global::System.Text.StringBuilder sb, global::XBlobContainer container, XBlobSet<CfgI<TestConfigUnManaged>> set)
    {
        int len = set.GetLength(container);
        sb.Append("{");
        int maxPrint = global::System.Math.Min(len, 10);
        for (int i = 0; i < maxPrint && i < len; i++)
        {
            if (i > 0) sb.Append(", ");
            var elem = set[container, i];
            AppendCfgI(sb, elem);
        }
        if (len > maxPrint) sb.Append($", ...({len - maxPrint} more)");
        sb.Append("}");
    }

    /// <summary>辅助方法：打印 XBlobSet TestSetSample</summary>
    private static void AppendSet_TestSetSample(global::System.Text.StringBuilder sb, global::XBlobContainer container, XBlobSet<Int32> set)
    {
        int len = set.GetLength(container);
        sb.Append("{");
        int maxPrint = global::System.Math.Min(len, 10);
        for (int i = 0; i < maxPrint && i < len; i++)
        {
            if (i > 0) sb.Append(", ");
            var elem = set[container, i];
            sb.Append(elem);
        }
        if (len > maxPrint) sb.Append($", ...({len - maxPrint} more)");
        sb.Append("}");
    }

    /// <summary>辅助方法：打印 XBlobArray TestNestedConfig</summary>
    private static void AppendArray_TestNestedConfig(global::System.Text.StringBuilder sb, global::XBlobContainer container, XBlobArray<NestedConfigUnManaged> arr)
    {
        int len = arr.GetLength(container);
        sb.Append("[");
        int maxPrint = global::System.Math.Min(len, 10); // 最多打印 10 个元素
        for (int i = 0; i < maxPrint; i++)
        {
            if (i > 0) sb.Append(", ");
            var elem = arr[container, i];
            sb.Append(elem);
        }
        if (len > maxPrint) sb.Append($", ...({len - maxPrint} more)");
        sb.Append("]");
    }
}

#endif

