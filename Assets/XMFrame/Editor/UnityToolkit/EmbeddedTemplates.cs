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

    #region 字段解析 (ParseXXX)

{{~ for assign in field_assigns ~}}
    {{ assign.method_code }}

{{~ end ~}}
    #endregion

    protected override void AllocContainerWithoutFillImpl(
        IXConfig value,
        TblI tbli,
        CfgI cfgi,
        System.Collections.Concurrent.ConcurrentDictionary<TblS, System.Collections.Concurrent.ConcurrentDictionary<CfgS, IXConfig>> allData,
        XM.ConfigDataCenter.ConfigDataHolder configHolderData)
    {
{{~ if container_alloc_code != """" ~}}
        var config = ({{ managed_type_name }})value;
{{ container_alloc_code }}
{{~ else ~}}
        // 无容器字段需要分配
{{~ end ~}}
    }

{{~ if container_alloc_helper_methods != """" ~}}
    #region 容器分配辅助方法

{{ container_alloc_helper_methods }}

    #endregion
{{~ end ~}}

    public override void FillBasicDataImpl(XM.ConfigDataCenter.ConfigDataHolder configHolderData, CfgS key, IXConfig value, XBlobMap<CfgI, {{ unmanaged_type_name }}> tableMap)
    {
        // TODO: 实现基础数据填充逻辑
    }

    private TblI _definedInMod;
}

{{~ if namespace != """" ~}}
}
{{~ end ~}}
";

        public const string UnmanagedStructSbncs = @"{{- # 非托管结构体代码生成模板（所有类型使用 global:: 全局命名空间限定） -}}
{{- # 生成 using 语句 -}}
{{~ for using in required_usings ~}}
using {{ using }};
{{~ end ~}}

{{- # 生成命名空间 -}}
{{~ if namespace != """" ~}}
namespace {{ namespace }}
{
{{~ end ~}}

{{- # 第一部分：接口声明 -}}
public partial struct {{ unmanaged_type_name }} : global::XM.IConfigUnManaged<{{ unmanaged_type_name }}>
{
}

{{- # 第二部分：字段定义 -}}
public partial struct {{ unmanaged_type_name }}
{
{{~ for field in fields ~}}
    public {{ field.unmanaged_type }} {{ field.name }};
{{~ if field.needs_ref_field ~}}
    public global::XBlobPtr<{{ unmanaged_type_name }}> {{ field.ref_field_name }};
{{~ end ~}}
{{~ end ~}}
}

{{- # 第四部分：转换方法（从 IConfigDataCenter 获取转换器，不静态 new 转换器类型） -}}
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

{{- # 第三部分：索引组定义 -}}
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

{{~ if namespace != """" ~}}
}
{{~ end ~}}
";
    }
}
