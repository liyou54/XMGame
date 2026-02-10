using System;
using System.Diagnostics.CodeAnalysis;

namespace XM.Utils.Attribute
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    public class XmlIndexAttribute : System.Attribute
    {
        public string IndexName;
        public bool IsMultiValue;
        public int Position;

        public XmlIndexAttribute(string indexName, bool isMultiValue, int pos = 0)
        {
            IndexName = indexName;
            IsMultiValue = isMultiValue;
            Position = pos;
        }
    }

    /// <summary>
    /// 标记字段为配置主键（从 XML 的 id 属性读取）
    /// - 非嵌套容器必须至少有一个 XmlKey 字段
    /// - XmlKey 字段从 configName 参数读取（该参数已包含 id 属性值）
    /// - 支持的类型：int, long, float, double, bool, string, enum, StrI, LabelI
    /// </summary>
    public class XmlKeyAttribute : System.Attribute
    {
        
    }
    
    /// <summary>
    /// 标记配置类为嵌套容器（在类级别使用）
    /// - 嵌套容器作为其他配置的子对象，不需要独立的 id 属性
    /// - 嵌套容器可以没有 XmlKey 字段
    /// - 示例：[XmlNested] public class NestedConfig { ... }
    /// </summary>
    public class XmlNestedAttribute : System.Attribute
    {
        
    }
    
    public class XmlDefinedAttribute : System.Attribute
    {
        public string XmlName;

        public XmlDefinedAttribute(string xmlName = null)
        {
            XmlName = xmlName;
        }
    }

    /// <summary>
    /// 标记字段为 XMLLink（配置组合模式，用组合代替继承）
    /// - 子Link存储父节点的CfgI，父节点通过索引查询子Link
    /// - IsUnique=true: 一个父节点只能有一个该类型的子Link（使用唯一索引）
    /// - IsUnique=false（默认）: 一个父节点可以有多个该类型的子Link（使用多值索引）
    /// </summary>
    public class XMLLinkAttribute : System.Attribute
    {
        public bool IsUnique;

        public XMLLinkAttribute(bool isUnique = false)
        {
            IsUnique = isUnique;
        }
    }

    /// <summary>
    /// 标记字段为必要：XML 中缺失时打告警（LogParseWarning），仍使用默认值或 [XmlDefault]。
    /// 容器类型暂不参与默认值逻辑，仅支持缺失告警。
    /// </summary>
    public class XmlNotNullAttribute : System.Attribute
    {
    }

    /// <summary>
    /// 标量字段的默认值（XML 缺失或空时使用）。值为字符串，解析方式与从 XML 读取时一致。
    /// 容器类型暂不支持默认值。
    /// </summary>
    public class XmlDefaultAttribute : System.Attribute
    {
        public string Value { get; }

        public XmlDefaultAttribute(string value)
        {
            Value = value ?? "";
        }
    }


    public enum EXmlStrMode
    {
        EFix32,
        EFix64,
        EStrI,
        ELabelI,
    }

    public class XmlStringModeAttribute : System.Attribute
    {
        EXmlStrMode StrMode;

        public XmlStringModeAttribute(EXmlStrMode strMode)
        {
            StrMode = strMode;
        }
    }


    /// <summary>
    /// 类型转换特性，用于标记字段需要进行类型转换
    /// 使用 XmlUnManagedConvert 转换器进行转换
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class XmlTypeConverterAttribute : System.Attribute
    {
        /// <summary>
        /// 转换器类型(需要有静态Convert方法)
        /// </summary>
        public Type ConverterType { get; set; }

        public bool BGlobal;
        
        public XmlTypeConverterAttribute(Type converterType, bool bGlobal = false)
        {
            ConverterType = converterType;
            BGlobal = bGlobal;
        }
    }
    
    /// <summary>
    /// 程序集级全局转换器特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class XmlGlobalConvertAttribute : System.Attribute
    {
        public Type ConverterType { get; }
        public string Domain { get; }

        public XmlGlobalConvertAttribute(Type converterType, string domain = "")
        {
            ConverterType = converterType;
            Domain = domain ?? "";
        }
    }
    
}