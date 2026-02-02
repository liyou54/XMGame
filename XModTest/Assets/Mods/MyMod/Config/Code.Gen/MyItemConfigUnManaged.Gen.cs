using MyMod;
using System;
using System.Collections.Generic;
using System.Xml;
using XM;
using XM.Contracts;
using XM.Contracts.Config;

namespace MyMod
{

public partial struct MyItemConfigUnManaged : global::XM.IConfigUnManaged<MyItemConfigUnManaged>
{
}

public partial struct MyItemConfigUnManaged
{
    public CfgI<MyItemConfigUnManaged> Id;
    public global::XBlobPtr<MyItemConfigUnManaged> Id_Ref;
    public StrI Name;
    public Int32 Level;
    public XBlobArray<Int32> Tags;
}




#if DEBUG || UNITY_EDITOR
public partial struct MyItemConfigUnManaged
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
        sb.Append("MyItemConfigUnManaged {");
        // CfgI：打印为 CfgS（模块::配置名）
        sb.Append("Id=");
        AppendCfgI(sb, Id);
        sb.Append(", ");
        // 普通字段：直接打印
        sb.Append("Name=" + Name);
        sb.Append(", ");
        // 普通字段：直接打印
        sb.Append("Level=" + Level);
        sb.Append(", ");
        // XBlobArray
        sb.Append("Tags=");
        if (container.HasValue)
            AppendArray_Tags(sb, container.Value, Tags);
        else
            sb.Append("XBlobArray<Int32>[?]");
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

    /// <summary>辅助方法：打印 XBlobArray Tags</summary>
    private static void AppendArray_Tags(global::System.Text.StringBuilder sb, global::XBlobContainer container, XBlobArray<Int32> arr)
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

}
