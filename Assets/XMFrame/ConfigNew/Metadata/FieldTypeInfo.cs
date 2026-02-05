using System;
using System.Collections.Generic;

namespace XM.ConfigNew.Metadata
{
    /// <summary>
    /// 字段类型信息 - 描述字段的完整类型结构
    /// 支持嵌套容器(如 List&lt;List&lt;Dictionary&lt;K,V&gt;&gt;&gt;)和嵌套配置类型
    /// </summary>
    public class FieldTypeInfo
    {
        #region 容器类型信息
        
        /// <summary>
        /// 最外层容器类型
        /// </summary>
        public EContainerType ContainerType;
        
        /// <summary>
        /// 嵌套层级(0表示非嵌套容器)
        /// 例如: List&lt;List&lt;int&gt;&gt; 的嵌套层级为2
        /// </summary>
        public int NestedLevel;
        
        /// <summary>
        /// 嵌套容器链(从外到内)
        /// 例如: List&lt;Dictionary&lt;string, int&gt;&gt; -> [List, Dictionary]
        /// </summary>
        public List<EContainerType> NestedContainerChain;
        
        #endregion
        
        #region 字典类型信息(仅Dictionary,HashSet,MultiValueHashMap(TODO)有效)
        
        /// <summary>
        /// 字典Key类型(仅当ContainerType为Dictionary时有效)
        /// </summary>
        public Type NestedKeyType;
        
        /// <summary>
        /// 字典Key是否为容器类型(不允许,用于验证)
        /// </summary>
        public bool IsKeyContainer;
        
        #endregion
        
        #region 容器Value类型信息
        
        /// <summary>
        /// 容器Value类型(List/Dictionary/HashSet的元素类型)
        /// 对于Dictionary,这是Value的类型
        /// </summary>
        public Type NestedValueType;
        
        /// <summary>
        /// Value是否为容器类型(用于嵌套容器)
        /// </summary>
        public bool IsValueContainer;
        
        /// <summary>
        /// Value的容器类型信息(递归,用于嵌套容器)
        /// 例如: List&lt;List&lt;int&gt;&gt; 中,内层List&lt;int&gt;的类型信息
        /// </summary>
        public FieldTypeInfo NestedValueTypeInfo;
        
        #endregion
        
        #region 最终元素类型
        
        /// <summary>
        /// 最终的单值类型(递归到最深层的非容器类型)
        /// 例如: List&lt;List&lt;int&gt;&gt; -> int
        /// </summary>
        public Type SingleValueType;
        
        /// <summary>
        /// 最终元素是否为嵌套配置类型(实现IXConfig)
        /// </summary>
        public bool IsNestedConfig;
        
        /// <summary>
        /// 嵌套配置的完整元数据(递归存储)
        /// 仅当IsNestedConfig为true时有效
        /// </summary>
        public ConfigClassMetadata NestedConfigMetadata;
        
        #endregion
        
        #region 可空类型
        
        /// <summary>
        /// 是否为可空类型(Nullable&lt;T&gt; 或 T?)
        /// </summary>
        public bool IsNullable;
        
        /// <summary>
        /// 可空类型的基础类型
        /// 例如: int? -> int
        /// </summary>
        public Type UnderlyingType;
        
        #endregion
        
        #region 枚举类型
        
        /// <summary>
        /// 是否为枚举类型
        /// </summary>
        public bool IsEnum;
        
        /// <summary>
        /// 枚举的基础类型(int, byte, long等)
        /// </summary>
        public Type EnumUnderlyingType;
        
        /// <summary>
        /// 枚举值名称列表(用于验证和代码生成)
        /// </summary>
        public List<string> EnumValueNames;
        
        /// <summary>
        /// 枚举值映射(名称 -> 数值)
        /// </summary>
        public Dictionary<string, object> EnumValues;
        
        #endregion
        
        #region 托管/非托管类型
        
        /// <summary>
        /// 托管字段类型(C#原始类型)
        /// </summary>
        public Type ManagedFieldType;
        
        /// <summary>
        /// 非托管字段类型(用于XBlob存储)
        /// </summary>
        public Type UnmanagedFieldType;
        
        #endregion
        
        #region 属性
        
        /// <summary>
        /// 是否为容器类型
        /// </summary>
        public bool IsContainer => ContainerType != EContainerType.None;
        
        /// <summary>
        /// 是否为嵌套容器(容器中包含容器)
        /// </summary>
        public bool IsNestedContainer => NestedLevel > 1;
        
        #endregion
    }
}
