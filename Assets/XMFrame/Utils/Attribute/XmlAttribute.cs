using System;
using System.Diagnostics.CodeAnalysis;

namespace XM.Utils.Attribute
{
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

    public class XmlDefinedAttribute : System.Attribute
    {
        public string XmlName;

        public XmlDefinedAttribute(string xmlName = null)
        {
            XmlName = xmlName;
        }
    }

    public class XMLLinkAttribute : System.Attribute
    {
        public bool Multilink;

        public XMLLinkAttribute(bool multilink = false)
        {
            Multilink = multilink;
        }
    }

    /// <summary>
    /// 标记字段为必要：XML 中缺失时打告警（LogParseWarning），仍使用默认值或 [XmlDefault]。
    /// 容器类型暂不参与默认值逻辑，仅支持缺失告警。
    /// </summary>
    public class XmlNotNullAttribute : System.Attribute { }

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

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class XmlGlobalConvertAttribute : System.Attribute
    {
        public Type ConverterType;
        public string Domain;

        public XmlGlobalConvertAttribute(Type converterType, string domain = "")
        {
            ConverterType = converterType;
            Domain = domain;
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
    public class XmlTypeConverterAttribute : System.Attribute
    {
        /// <summary>
        /// 转换器类型（继承自 XmlUnManagedConvert）
        /// </summary>
        public Type ConverterType { get; set; }
        
        /// <summary>
        /// 转换域（用于区分全局和局部转换器，空字符串表示全局）
        /// </summary>
        public string Domain { get; set; }

        public XmlTypeConverterAttribute(Type converterType, string domain = "")
        {
            ConverterType = converterType;
            Domain = domain ?? "";
        }
    }
}