using System;
using System.Collections.Generic;
using XM.Utils.Attribute;

namespace XM.ConfigNew.Metadata
{
    /// <summary>
    /// 配置字段元数据 - 描述配置类中单个字段的完整信息
    /// 包括类型信息、转换器、解析规则、索引信息、Link信息等
    /// </summary>
    public class ConfigFieldMetadata
    {
        #region 基础信息
        
        /// <summary>
        /// 字段名称
        /// </summary>
        public string FieldName;
        
        /// <summary>
        /// 字段类型信息(完整的类型结构)
        /// </summary>
        public FieldTypeInfo TypeInfo;
        
        /// <summary>
        /// 字段的反射信息(用于运行时访问)
        /// </summary>
        public System.Reflection.FieldInfo FieldReflectionInfo;
        
        /// <summary>
        /// 字段的源码注释(从源码中读取)
        /// </summary>
        public string SourceComment;
        
        #endregion
        
        #region 转换器信息
        
        /// <summary>
        /// 转换器信息(字段级/Mod级/全局级)
        /// 对于标量字段: 使用 Converter.Registrations
        /// 对于容器字段: 使用 Converter.KeyRegistrations 和 Converter.ValueRegistrations
        /// </summary>
        public ConverterInfo Converter;
        
        #endregion
        
        #region XML解析规则
        
        /// <summary>
        /// 是否为必填字段(标记了[XmlNotNull])
        /// XML中缺失时会打告警
        /// </summary>
        public bool IsNotNull;
        
        /// <summary>
        /// 默认值字符串(来自[XmlDefault]特性)
        /// XML中缺失或为空时使用此值
        /// 
        /// 标量类型: "100", "true", "MyEnum.Value1"
        /// 容器类型: "1,2,3" (逗号分隔,代替XML子元素)
        /// </summary>
        public string DefaultValue;
        
        /// <summary>
        /// 容器默认值的分隔符(默认为逗号)
        /// 用于解析容器类型的默认值字符串
        /// </summary>
        public string DefaultValueSeparator;
        
        /// <summary>
        /// 字符串模式(来自[XmlStringMode]特性)
        /// 用于指定字符串的存储方式(FixedString32/64/128, StrI, LabelI等)
        /// </summary>
        public EXmlStrMode StringMode;
        
        /// <summary>
        /// XML中的字段名(来自[XmlDefined]特性,默认为字段名)
        /// </summary>
        public string XmlName;
        
        #endregion
        
        #region 索引信息
        
        /// <summary>
        /// 是否参与索引
        /// </summary>
        public bool IsIndexField;
        
        /// <summary>
        /// 参与的索引名称列表
        /// 一个字段可以参与多个索引
        /// </summary>
        public List<(int,ConfigIndexMetadata)> IndexNames;
        
        
        #endregion
        
        #region Link信息
        
        /// <summary>
        /// 是否为XMLLink字段(标记了[XMLLink])
        /// Link字段表示配置间的引用关系
        /// </summary>
        public bool IsXmlLink;
        
        /// <summary>
        /// Link目标配置类型（父节点类型）
        /// 例如: CfgS&lt;TargetConfig&gt; 中的TargetConfig
        /// </summary>
        public Type XmlLinkTargetType;
        
        /// <summary>
        /// 父节点对该类型子Link是否唯一(来自[XMLLink(isUnique=true)])
        /// true: 一个父节点只能有一个该类型的子Link（使用唯一索引）
        /// false: 一个父节点可以有多个该类型的子Link（使用多值索引）
        /// </summary>
        public bool IsUniqueLinkToParent;
        
        /// <summary>
        /// 自动生成的父节点索引名称
        /// 格式: ParentLinkIndexPrefix + ParentTypeName
        /// 例如: ByParent_TestConfig
        /// </summary>
        public string ParentLinkIndexName;
        
        #endregion
        
        #region Key/Nested 信息
        
        /// <summary>
        /// 是否为 XmlKey 字段(标记了[XmlKey])
        /// XmlKey 字段从 configName 读取,而不是从 XML field 读取
        /// </summary>
        public bool IsXmlKey;
        
        /// <summary>
        /// 是否为 XmlNested 字段(标记了[XmlNested])
        /// XmlNested 字段表示嵌套配置
        /// </summary>
        public bool IsXmlNested;
        
        #endregion
        
        #region 代码生成预计算字段
        
        /// <summary>
        /// Parse方法名称(预计算)
        /// 例如: "ParseFieldName"
        /// </summary>
        public string ParseMethodName;
        
        /// <summary>
        /// Alloc方法名称(预计算,用于容器分配)
        /// 例如: "AllocFieldName"
        /// </summary>
        public string AllocMethodName;
        
        /// <summary>
        /// Fill方法名称(预计算,用于嵌套配置填充)
        /// 例如: "FillFieldName"
        /// </summary>
        public string FillMethodName;
        
        /// <summary>
        /// 非托管字段类型名称(预计算,用于代码生成)
        /// 例如: "int", "XBlobArray<int>", "BaseItemConfigUnmanaged"
        /// </summary>
        public string UnmanagedFieldTypeName;
        
        /// <summary>
        /// 托管字段类型名称(预计算,用于代码生成)
        /// 例如: "int", "List<int>", "BaseItemConfig"
        /// </summary>
        public string ManagedFieldTypeName;
        
        #endregion
        
        #region 属性
        
        /// <summary>
        /// 是否需要类型转换
        /// </summary>
        public bool NeedsConverter => Converter != null && Converter.NeedsConverter;
        
        /// <summary>
        /// 是否为容器类型
        /// </summary>
        public bool IsContainer => TypeInfo != null && TypeInfo.IsContainer;
        
        /// <summary>
        /// 是否为嵌套配置类型
        /// </summary>
        public bool IsNestedConfig => TypeInfo != null && TypeInfo.IsNestedConfig;
        
        #endregion
    }
}
