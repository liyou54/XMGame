using System;
using XM.ConfigNew.Metadata;

namespace XM.ConfigNew.CodeGen
{
    /// <summary>
    /// 代码生成上下文 - 封装代码生成过程中需要的所有上下文信息
    /// 目的：减少参数传递，提供统一的访问接口
    /// </summary>
    public class CodeGenContext
    {
        /// <summary>
        /// 代码构建器
        /// </summary>
        public CodeBuilder Builder { get; }
        
        /// <summary>
        /// 配置类元数据
        /// </summary>
        public ConfigClassMetadata ClassMetadata { get; }
        
        /// <summary>
        /// 当前正在处理的字段元数据（可动态切换）
        /// </summary>
        public ConfigFieldMetadata FieldMetadata { get; set; }
        
        /// <summary>
        /// 托管类型名称（全局限定）
        /// </summary>
        public string ManagedTypeName { get; }
        
        /// <summary>
        /// 非托管类型名称（全局限定）
        /// </summary>
        public string UnmanagedTypeName { get; }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public CodeGenContext(CodeBuilder builder, ConfigClassMetadata classMetadata)
        {
            Builder = builder ?? throw new ArgumentNullException(nameof(builder));
            ClassMetadata = classMetadata ?? throw new ArgumentNullException(nameof(classMetadata));
            
            ManagedTypeName = TypeHelper.GetGlobalQualifiedTypeName(classMetadata.ManagedType);
            UnmanagedTypeName = TypeHelper.GetGlobalQualifiedTypeName(classMetadata.UnmanagedType);
        }
        
        /// <summary>
        /// 获取 XML Link 目标的非托管类型名称
        /// 优先从 IXConfig&lt;T, TUnmanaged&gt; 泛型参数获取，避免拼接导致的大小写问题（如 UIConfigUnManaged vs UIConfigUnmanaged）
        /// </summary>
        public string GetXmlLinkUnmanagedTypeName(Type targetManagedType)
        {
            if (targetManagedType == null)
                return "object";
            
            return TypeHelper.GetConfigUnmanagedTypeName(targetManagedType);
        }
        
        /// <summary>
        /// 获取指定类型的非托管类型名称（通用）
        /// </summary>
        public string GetUnmanagedTypeName(Type type)
        {
            if (type == null)
                return "object";
            
            return TypeHelper.GetGlobalQualifiedTypeName(type);
        }
    }
}
