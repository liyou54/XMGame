using System;
using System.Collections.Generic;
using System.Xml;
using Unity.Collections;
using Unity.Mathematics;
using XM;
using XM.Contracts;
using XM.Contracts.Config;


public partial struct NestedConfigUnManaged : global::XM.IConfigUnManaged<NestedConfigUnManaged>
{
}

public partial struct NestedConfigUnManaged
{
    public Int32 RequiredId;
    public StrI OptionalWithDefault;
    public Int32 Test;
    public int2 TestCustom;
    public int2 TestGlobalConvert;
    public XBlobArray<CfgI<TestConfigUnManaged>> TestKeyList;
    public LabelI StrIndex;
    public FixedString32Bytes Str32;
    public FixedString64Bytes Str64;
    public StrI Str;
    public LabelI LabelS;
}




#if DEBUG || UNITY_EDITOR
public partial struct NestedConfigUnManaged
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
        sb.Append("NestedConfigUnManaged {");
        // 普通字段：直接打印
        sb.Append("RequiredId=" + RequiredId);
        sb.Append(", ");
        // 普通字段：直接打印
        sb.Append("OptionalWithDefault=" + OptionalWithDefault);
        sb.Append(", ");
        // 普通字段：直接打印
        sb.Append("Test=" + Test);
        sb.Append(", ");
        // 普通字段：直接打印
        sb.Append("TestCustom=" + TestCustom);
        sb.Append(", ");
        // 普通字段：直接打印
        sb.Append("TestGlobalConvert=" + TestGlobalConvert);
        sb.Append(", ");
        // XBlobArray
        sb.Append("TestKeyList=");
        if (container.HasValue)
            AppendArray_TestKeyList(sb, container.Value, TestKeyList);
        else
            sb.Append("XBlobArray<CfgI<TestConfigUnManaged>>[?]");
        sb.Append(", ");
        // 普通字段：直接打印
        sb.Append("StrIndex=" + StrIndex);
        sb.Append(", ");
        // 普通字段：直接打印
        sb.Append("Str32=" + Str32);
        sb.Append(", ");
        // 普通字段：直接打印
        sb.Append("Str64=" + Str64);
        sb.Append(", ");
        // 普通字段：直接打印
        sb.Append("Str=" + Str);
        sb.Append(", ");
        // 普通字段：直接打印
        sb.Append("LabelS=" + LabelS);
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
}

#endif

