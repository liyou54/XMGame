using System;
using System.Collections.Generic;
using System.Xml;
using XM;
using XM.Contracts;
using XM.Contracts.Config;

namespace XM.Editor.Gen
{

public partial struct TestInhertUnmanaged : global::XM.IConfigUnManaged<TestInhertUnmanaged>
{
}

public partial struct TestInhertUnmanaged
{
    public CfgI<TestConfigUnManaged> Link_ParentDst;
    public XBlobPtr<TestConfigUnManaged> Link_ParentRef;
    public CfgI<TestInhertUnmanaged> Link;
    public Int32 xxxx;
}




#if DEBUG || UNITY_EDITOR
public partial struct TestInhertUnmanaged
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
        sb.Append("TestInhertUnmanaged {");
        // CfgI：打印为 CfgS（模块::配置名）
        sb.Append("Link_ParentDst=");
        AppendCfgI(sb, Link_ParentDst);
        sb.Append(", ");
        // XBlobPtr 关联 CfgI：打印为 "Ptr->CfgS"
        sb.Append("Link_ParentRef=Ptr->");
        AppendCfgI(sb, Link_ParentDst);
        sb.Append(", ");
        // CfgI：打印为 CfgS（模块::配置名）
        sb.Append("Link=");
        AppendCfgI(sb, Link);
        sb.Append(", ");
        // 普通字段：直接打印
        sb.Append("xxxx=" + xxxx);
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
}

#endif

}
