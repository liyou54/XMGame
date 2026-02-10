using System;
using System.Collections.Generic;
using System.Linq;

namespace XM.ConfigNew.Metadata
{
    /// <summary>
    /// 配置类元数据 - 描述一个配置类的完整结构信息
    /// 这是元数据系统的核心类,包含类型、字段、索引、Link等所有信息
    /// </summary>
    public class ConfigClassMetadata
    {
        #region 类型信息
        
        /// <summary>
        /// 托管配置类型(C#类,实现IXConfig)
        /// </summary>
        public Type ManagedType;
        
        /// <summary>
        /// 非托管配置类型(struct,用于XBlob存储)
        /// </summary>
        public Type UnmanagedType;
        
        
        /// <summary>
        /// 命名空间
        /// </summary>
        public string Namespace;
        
        /// <summary>
        /// 托管类型名称
        /// </summary>
        public string ManagedTypeName;
        
        /// <summary>
        /// 非托管类型名称
        /// </summary>
        public string UnmanagedTypeName;
        
        /// <summary>
        /// Helper类型名称
        /// </summary>
        public string HelperTypeName;
        
        /// <summary>
        /// Helper类型(用于嵌套配置的递归调用)
        /// </summary>
        public Type HelperType;
        
        #endregion
        
        #region 表信息
        
        /// <summary>
        /// 表名(来自[XmlDefined]特性,默认为类型名)
        /// </summary>
        public string TableName;
        
        /// <summary>
        /// Mod名称(来自程序集的[ModName]特性)
        /// </summary>
        public string ModName;
        
        #endregion
        
        #region 字段信息
        
        /// <summary>
        /// 所有字段的元数据列表
        /// </summary>
        public List<ConfigFieldMetadata> Fields;
        
        /// <summary>
        /// 字段快速查找表(字段名 -> 字段元数据)
        /// </summary>
        public Dictionary<string, ConfigFieldMetadata> FieldByName;
        
        #endregion
        
        #region 索引信息
        
        /// <summary>
        /// 所有索引的元数据列表
        /// </summary>
        public List<ConfigIndexMetadata> Indexes;
        
        /// <summary>
        /// 索引快速查找表(索引名 -> 索引元数据)
        /// </summary>
        public Dictionary<string, ConfigIndexMetadata> IndexByName;
        
        #endregion
        
        #region Link信息
        
        /// <summary>
        /// Link元数据(继承关系和引用关系)
        /// </summary>
        public ConfigLinkMetadata Link;
        
        #endregion
        
        #region 程序集信息
        
        /// <summary>
        /// 所属程序集
        /// </summary>
        public System.Reflection.Assembly Assembly;
        
        /// <summary>
        /// 程序集名称
        /// </summary>
        public string AssemblyName;
        
        #endregion
        
        #region 代码生成所需信息
        
        /// <summary>
        /// 所需的using命名空间列表(用于代码生成)
        /// </summary>
        public List<string> RequiredUsings;
        
        #endregion
        
        #region XmlNested 信息
        
        /// <summary>
        /// 是否为嵌套配置容器（标记了[XmlNested]）
        /// 嵌套配置不需要 XmlKey 字段
        /// </summary>
        public bool IsXmlNested;
        
        #endregion
        
        #region 属性
        
        /// <summary>
        /// 是否有Link关系
        /// </summary>
        public bool HasLink => Link != null && Link.HasLinkFields;
        
        /// <summary>
        /// 是否有索引
        /// </summary>
        public bool HasIndexes => Indexes != null && Indexes.Count > 0;
        
        #endregion
    }
}
