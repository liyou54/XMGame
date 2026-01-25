using System;

namespace XMFrame.Utils.Attribute
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

        public XmlDefinedAttribute(string xmlName)
        {
            XmlName = xmlName;
        }
    }

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
        EStrHandle,
        EStrLabel,
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