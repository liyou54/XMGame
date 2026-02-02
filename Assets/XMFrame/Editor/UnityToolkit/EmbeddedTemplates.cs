namespace UnityToolkit
{
    /// <summary>内嵌 Scriban 模板内容，供 mod 工程等无模板文件时使用。</summary>
    internal static class EmbeddedTemplates
    {
        public const string ClassHelperSbncs = @"{{- # ConfigClassHelper<TC, TI> 静态代码生成模板，用于解析 XML（无反射）；using 根据 required_usings 按字段类型生成 -}}
{{~ for ns in required_usings ~}}
using {{ ns }};
{{~ end ~}}

{{~ if namespace != """" ~}}
namespace {{ namespace }}
{
{{~ end ~}}

/// <summary>
/// {{ managed_type_name }} 的配置加载辅助类，用于从 XML 反序列化（静态代码生成，无反射）。
/// </summary>
public sealed class {{ helper_class_name }} : ConfigClassHelper<{{ managed_type_name }}, {{ unmanaged_type_name }}>
{
    public static TblI TblI { get; private set; }
    public static TblS TblS { get; private set; }

    static {{ helper_class_name }}()
    {
        const string __tableName = ""{{ table_name }}"";
        const string __modName = ""{{ mod_name }}"";
        CfgS<{{ unmanaged_type_name }}>.Table = new TblS(new ModS(__modName), __tableName);
        TblS = new TblS(new ModS(__modName), __tableName);
    }

    public {{ helper_class_name }}(IConfigDataCenter dataCenter)
        : base(dataCenter)
    {
{{~ for reg in converter_registrations ~}}
        TypeConverterRegistry.RegisterLocalConverter<{{ reg.source_type }}, {{ reg.target_type }}>(""{{ reg.domain_escaped }}"", new {{ reg.converter_type_name }}());
{{~ end ~}}
    }

    public override TblS GetTblS()
    {
        return TblS;
    }

    public override void SetTblIDefinedInMod(TblI tbl)
    {
        _definedInMod = tbl;
    }

    public override void ParseAndFillFromXml(IXConfig target, XmlElement configItem, ModS mod, string configName, in ConfigParseContext context)
    {
        var config = ({{ managed_type_name }})target;
{{~ if has_base ~}}
        var baseHelper = ConfigDataCenter.GetClassHelper(typeof({{ base_managed_type_name }}));
        if (baseHelper != null) baseHelper.ParseAndFillFromXml(target, configItem, mod, configName, context);
{{~ end ~}}
{{~ for assign in field_assigns ~}}
        {{ assign.call_code }}
{{~ end ~}}
    }

    public override Type GetLinkHelperType()
    {
{{~ if link_helper_class_name != """" ~}}
        return typeof({{ link_helper_class_name }});
{{~ else ~}}
        return null;
{{~ end ~}}
    }

    #region 字段解析 (ParseXXX)

{{~ for assign in field_assigns ~}}
    {{ assign.method_code }}

{{~ end ~}}
    #endregion

    public override void AllocContainerWithFillImpl(
        IXConfig value,
        TblI tbli,
        CfgI cfgi,
        ref {{ unmanaged_type_name }} data,
        XM.ConfigDataCenter.ConfigDataHolder configHolderData,
        XBlobPtr? linkParent = null)
    {
        var config = ({{ managed_type_name }})value;
{{~ if container_alloc_code != """" ~}}
{{ container_alloc_code }}
{{~ else ~}}
        // 无字段需要处理
{{~ end ~}}
    }

{{~ if container_alloc_helper_methods != """" ~}}
    #region 容器分配辅助方法

{{ container_alloc_helper_methods }}

    #endregion
{{~ end ~}}

    private TblI _definedInMod;
}

{{~ if namespace != """" ~}}
}
{{~ end ~}}
";

        public const string UnmanagedStructSbncs = @"{{~ # 非托管结构体代码生成模板（所有类型使用 global:: 全局命名空间限定） ~}}
{{~ # 生成 using 语句 ~}}
{{~ for using in required_usings ~}}
using {{ using }};
{{~ end ~}}

{{~ # 生成命名空间 ~}}
{{~ if namespace != """" ~}}
namespace {{ namespace }}
{
{{~ end ~}}

{{~ # 第一部分：接口声明 ~}}
public partial struct {{ unmanaged_type_name }} : global::XM.IConfigUnManaged<{{ unmanaged_type_name }}>
{
}

{{~ # 第二部分：字段定义 ~}}
public partial struct {{ unmanaged_type_name }}
{
{{~ for field in fields ~}}
    public {{ field.unmanaged_type }} {{ field.name }};
{{~ if field.needs_ref_field ~}}
    public global::XBlobPtr<{{ unmanaged_type_name }}> {{ field.ref_field_name }};
{{~ end ~}}
{{~ end ~}}
}

{{~ # 第四部分：转换方法（从 IConfigDataCenter 获取转换器，不静态 new 转换器类型） ~}}
{{~ for field in fields ~}}
{{~ if field.needs_converter ~}}
public partial struct {{ unmanaged_type_name }}
{
    /// <summary>
    /// 将 {{ field.name }} 从 {{ field.source_type }} 转换为 {{ field.target_type }}，转换器从 IConfigDataCenter 按域获取。
    /// </summary>
    public static {{ field.target_type }} Convert{{ field.name }}({{ field.source_type }} source)
    {
        var c = global::XM.Contracts.IConfigDataCenter.I?.GetConverter<{{ field.source_type }}, {{ field.target_type }}>(""{{ field.converter_domain_escaped }}"");
        return c != null && c.Convert(source, out var result) ? result : default;
    }
}
{{~ end ~}}
{{~ end ~}}

{{~ # 第三部分：索引组定义 ~}}
{{~ for index_group in index_groups ~}}
public partial struct {{ unmanaged_type_name }}
{
    public struct {{ index_group.index_name }}Index : global::XM.IConfigIndexGroup<{{ unmanaged_type_name }}>, global::System.IEquatable<{{ index_group.index_name }}Index>
    {
{{~ for field in index_group.fields ~}}
        public {{ field.unmanaged_type }} {{ field.name }};
{{~ end ~}}

        public {{ index_group.index_name }}Index({{~ for field in index_group.fields ~}}{{~ if for.index > 0 ~}}, {{~ end ~}}{{ field.unmanaged_type }} {{ field.param_name }}{{~ end ~}})
        {
{{~ for field in index_group.fields ~}}
            {{ field.name }} = {{ field.param_name }};
{{~ end ~}}
        }

        public bool Equals({{ index_group.index_name }}Index other)
        {
            return {{ for field in index_group.fields }}{{ if for.index > 0 }} && {{ end }}{{ field.name }}.Equals(other.{{ field.name }}){{ end }};
        }

        public override bool Equals(global::System.Object obj)
        {
            return obj is {{ index_group.index_name }}Index other && Equals(other);
        }

        public override int GetHashCode()
        {
            return global::System.HashCode.Combine({{~ for field in index_group.fields ~}}{{~ if for.index > 0 ~}}, {{~ end ~}}{{ field.name }}{{~ end ~}});
        }
    }
}
{{~ end ~}}

{{~ # 第五部分：ToString 方法（条件编译） ~}}

#if DEBUG || UNITY_EDITOR
public partial struct {{ unmanaged_type_name }}
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
        sb.Append(""{{ unmanaged_type_name }} {"");
{{~ for field in fields ~}}
{{~ if !for.first ~}}
        sb.Append("", "");
{{~ end ~}}
{{~ if field.is_xblobptr ~}}
{{~ if field.associated_cfgi_field != """" ~}}
        // XBlobPtr 关联 CfgI：打印为 ""Ptr->CfgS""
        sb.Append(""{{ field.name }}=Ptr->"");
        AppendCfgI(sb, {{ field.associated_cfgi_field }});
{{~ else ~}}
        // 独立 XBlobPtr：只打印偏移量
        sb.Append(""{{ field.name }}=Ptr("" + {{ field.name }}.Offset + "")"");
{{~ end ~}}
{{~ else if field.is_cfgi ~}}
        // CfgI：打印为 CfgS（模块::配置名）
        sb.Append(""{{ field.name }}="");
        AppendCfgI(sb, {{ field.name }});
{{~ else if field.is_container ~}}
{{~ if field.container_kind == ""Array"" ~}}
        // XBlobArray
        sb.Append(""{{ field.name }}="");
        if (container.HasValue)
            AppendArray_{{ field.name }}(sb, container.Value, {{ field.name }});
        else
            sb.Append(""XBlobArray<{{ field.element_type }}>[?]"");
{{~ else if field.container_kind == ""Map"" ~}}
        // XBlobMap
        sb.Append(""{{ field.name }}="");
        if (container.HasValue)
            AppendMap_{{ field.name }}(sb, container.Value, {{ field.name }});
        else
            sb.Append(""XBlobMap<{{ field.key_type }}, {{ field.value_type }}>[?]"");
{{~ else if field.container_kind == ""Set"" ~}}
        // XBlobSet
        sb.Append(""{{ field.name }}="");
        if (container.HasValue)
            AppendSet_{{ field.name }}(sb, container.Value, {{ field.name }});
        else
            sb.Append(""XBlobSet<{{ field.element_type }}>[?]"");
{{~ end ~}}
{{~ else if field.needs_ref_field ~}}
        // 普通字段但有 Ref 字段：打印字段值和 Ref
        sb.Append(""{{ field.name }}="" + {{ field.name }});
        sb.Append("", {{ field.ref_field_name }}=Ptr->"");
        sb.Append({{ field.name }});
{{~ else ~}}
        // 普通字段：直接打印
        sb.Append(""{{ field.name }}="" + {{ field.name }});
{{~ end ~}}
{{~ end ~}}
        sb.Append("" }"");
        return sb.ToString();
    }

    /// <summary>辅助方法：打印 CfgI（尝试转为 CfgS）</summary>
    private static void AppendCfgI<T>(global::System.Text.StringBuilder sb, global::CfgI<T> cfgi)
        where T : unmanaged, global::XM.IConfigUnManaged<T>
    {
        var dataCenter = global::XM.Contracts.IConfigDataCenter.I;
        if (dataCenter != null && dataCenter.TryGetCfgS(cfgi.AsNonGeneric(), out var cfgs))
        {
            sb.Append(cfgs.ToString()); // ""模块::配置名""
        }
        else
        {
            sb.Append(cfgi.ToString()); // 回退到 ""CfgI(Id)""
        }
    }
{{~ for field in fields ~}}
{{~ if field.is_container && field.container_kind == ""Array"" ~}}

    /// <summary>辅助方法：打印 XBlobArray {{ field.name }}</summary>
    private static void AppendArray_{{ field.name }}(global::System.Text.StringBuilder sb, global::XBlobContainer container, {{ field.unmanaged_type }} arr)
    {
        int len = arr.GetLength(container);
        sb.Append(""["");
        int maxPrint = global::System.Math.Min(len, 10); // 最多打印 10 个元素
        for (int i = 0; i < maxPrint; i++)
        {
            if (i > 0) sb.Append("", "");
            var elem = arr[container, i];
{{~ if field.element_type_is_cfgi ~}}
            AppendCfgI(sb, elem);
{{~ else ~}}
            sb.Append(elem);
{{~ end ~}}
        }
        if (len > maxPrint) sb.Append($"", ...({len - maxPrint} more)"");
        sb.Append(""]"");
    }
{{~ end ~}}
{{~ if field.is_container && field.container_kind == ""Map"" ~}}

    /// <summary>辅助方法：打印 XBlobMap {{ field.name }}</summary>
    private static void AppendMap_{{ field.name }}(global::System.Text.StringBuilder sb, global::XBlobContainer container, {{ field.unmanaged_type }} map)
    {
        int len = map.GetLength(container);
        sb.Append(""{"");
        // Map 打印前 5 对
        int maxPrint = global::System.Math.Min(len, 5);
        int printed = 0;
        foreach (var kvp in map.GetEnumerator(container))
        {
            if (printed >= maxPrint) break;
            if (printed > 0) sb.Append("", "");
{{~ if field.key_type_is_cfgi ~}}
            AppendCfgI(sb, kvp.Key);
{{~ else ~}}
            sb.Append(kvp.Key);
{{~ end ~}}
            sb.Append("":"");
{{~ if field.value_type_is_cfgi ~}}
            AppendCfgI(sb, kvp.Value);
{{~ else ~}}
            sb.Append(kvp.Value);
{{~ end ~}}
            printed++;
        }
        if (len > maxPrint) sb.Append($"", ...({len - maxPrint} more)"");
        sb.Append("")}"");
    }
{{~ end ~}}
{{~ if field.is_container && field.container_kind == ""Set"" ~}}

    /// <summary>辅助方法：打印 XBlobSet {{ field.name }}</summary>
    private static void AppendSet_{{ field.name }}(global::System.Text.StringBuilder sb, global::XBlobContainer container, {{ field.unmanaged_type }} set)
    {
        int len = set.GetLength(container);
        sb.Append(""{"");
        int maxPrint = global::System.Math.Min(len, 10);
        for (int i = 0; i < maxPrint && i < len; i++)
        {
            if (i > 0) sb.Append("", "");
            var elem = set[container, i];
{{~ if field.element_type_is_cfgi ~}}
            AppendCfgI(sb, elem);
{{~ else ~}}
            sb.Append(elem);
{{~ end ~}}
        }
        if (len > maxPrint) sb.Append($"", ...({len - maxPrint} more)"");
        sb.Append("")}"");
    }
{{~ end ~}}
{{~ end ~}}
}

#endif

{{~ if namespace != """" ~}}
}
{{~ end ~}}
";
    }
}
