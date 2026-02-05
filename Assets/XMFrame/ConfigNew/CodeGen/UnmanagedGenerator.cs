using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework.Internal;
using XM.ConfigNew.Metadata;
using XM.Utils.Attribute;

namespace XM.ConfigNew.CodeGen
{
    /// <summary>
    /// Unmanaged结构体代码生成器
    /// 按照1.1-1.4的规则生成代码
    /// </summary>
    public class UnmanagedGenerator
    {
        private readonly ConfigClassMetadata _metadata;
        private readonly CodeBuilder _builder;
        
        
        public UnmanagedGenerator(ConfigClassMetadata metadata)
        {
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _builder = new CodeBuilder();
        }
        
        #region 主生成方法
        
        /// <summary>
        /// 生成Unmanaged结构体代码
        /// </summary>
        public string Generate()
        {
            _builder.Clear();
            
            // 1. 生成文件头
            GenerateFileHeader();
            
            // 2. 生成命名空间（如果有）
            if (!string.IsNullOrEmpty(_metadata.Namespace))
            {
                _builder.BeginNamespace(_metadata.Namespace);
            }
            
            // 3. 生成结构体
            GenerateStruct();
            
            // 4. 结束命名空间（如果有）
            if (!string.IsNullOrEmpty(_metadata.Namespace))
            {
                _builder.EndNamespace();
            }
            
            return _builder.Build();
        }
        
        #endregion
        
        #region 文件头生成
        
        /// <summary>
        /// 生成文件头(using语句)
        /// </summary>
        private void GenerateFileHeader()
        {
            var usings = GetRequiredUsings();
            foreach (var usingNamespace in usings)
            {
                _builder.AppendUsing(usingNamespace);
            }
            _builder.AppendLine();
        }
        
        /// <summary>
        /// 获取需要的using语句
        /// </summary>
        private List<string> GetRequiredUsings()
        {
            var usings = new HashSet<string>
            {
                "System",
                "XM",
                "XM.Contracts.Config",
                "XM.ConfigNew.CodeGen",
                "Unity.Collections"
            };
            
            // 根据字段类型添加额外的using
            if (_metadata.Fields != null)
            {
                foreach (var field in _metadata.Fields)
                {
                    if (field.TypeInfo?.IsNestedConfig == true)
                    {
                        var nestedNamespace = field.TypeInfo.NestedConfigMetadata?.Namespace;
                        if (!string.IsNullOrEmpty(nestedNamespace))
                            usings.Add(nestedNamespace);
                    }
                }
            }
            
            return usings.OrderBy(u => u).ToList();
        }
        
        #endregion
        
        #region 结构体生成
        
        /// <summary>
        /// 生成Unmanaged结构体
        /// </summary>
        private void GenerateStruct()
        {
            var structName = _metadata.UnmanagedTypeName;
            var unmanagedTypeName = TypeHelper.GetGlobalQualifiedTypeName(_metadata.UnmanagedType);
            var interfaceName = $"IConfigUnManaged<{unmanagedTypeName}>";
            
            _builder.AppendXmlComment($"{_metadata.ManagedTypeName} 的非托管数据结构 (代码生成)");
            _builder.BeginClass(structName, interfaceName, isPartial: true, isStruct: true);
            
            // 生成字段
            GenerateFields();
            
            // 生成ToString方法
            GenerateToStringMethod();
            
            _builder.EndClass();
        }
        
        #endregion
        
        #region 字段生成
        
        /// <summary>
        /// 生成所有字段
        /// </summary>
        private void GenerateFields()
        {
            if (_metadata.Fields == null || _metadata.Fields.Count == 0)
                return;
            
            _builder.AppendComment(CodeGenConstants.FieldsComment);
            _builder.AppendLine();
            
            foreach (var field in _metadata.Fields)
            {
                GenerateField(field);
            }
            
            _builder.AppendLine();
        }
        
        /// <summary>
        /// 生成单个字段
        /// </summary>
        private void GenerateField(ConfigFieldMetadata field)
        {
            // 优先使用预计算的类型名，如果没有则回退到动态计算
            var fieldType = !string.IsNullOrEmpty(field.UnmanagedFieldTypeName) 
                ? field.UnmanagedFieldTypeName 
                : GetUnmanagedFieldType(field);
            var fieldName = field.FieldName;
            
            // 优先使用源码注释,否则使用生成的注释
            var comment = field.SourceComment ?? GetFieldComment(field);
            
            _builder.AppendField(fieldType, fieldName, comment);
            
            // 1.4 Link类型生成额外字段
            if (field.IsXmlLink)
            {
                GenerateLinkFields(field);
            }
        }
        
        /// <summary>
        /// 获取字段的非托管类型字符串（委托给 TypeHelper 统一方法）
        /// </summary>
        private string GetUnmanagedFieldType(ConfigFieldMetadata field)
        {
            return TypeHelper.GetUnmanagedFieldTypeName(field, GetStringModeTypeNameQualified);
        }
        
        /// <summary>
        /// 根据字符串模式获取全局限定的类型名称
        /// 注意：FixedString32/64Bytes 只在明确标记时使用，默认 fallback 到 StrI
        /// </summary>
        private string GetStringModeTypeNameQualified(EXmlStrMode mode)
        {
            switch (mode)
            {
                case EXmlStrMode.EFix32:
                    return TypeHelper.GetGlobalQualifiedTypeName(typeof(Unity.Collections.FixedString32Bytes));
                case EXmlStrMode.EFix64:
                    return TypeHelper.GetGlobalQualifiedTypeName(typeof(Unity.Collections.FixedString64Bytes));
                case EXmlStrMode.ELabelI:
                    return TypeHelper.GetGlobalQualifiedTypeName(typeof(LabelI));
                case EXmlStrMode.EStrI:
                default:
                    // 默认使用 StrI，这是最常见的字符串索引类型
                    // 不再自动使用 FixedString32Bytes，因为它有长度限制且不支持动态字符串池
                    return TypeHelper.GetGlobalQualifiedTypeName(typeof(StrI));
            }
        }
        
        /// <summary>
        /// 根据字符串模式获取类型名称
        /// </summary>
        /// <summary>
        /// 根据字符串模式获取类型名称
        /// </summary>
        /// <param name="mode">字符串模式</param>
        /// <returns>对应的非托管类型名称</returns>
        private string GetStringModeTypeName(EXmlStrMode mode)
        {
            switch (mode)
            {
                case EXmlStrMode.EFix32:
                    return CodeGenConstants.FixedString32TypeName;
                case EXmlStrMode.EFix64:
                    return CodeGenConstants.FixedString64TypeName;
                case EXmlStrMode.EStrI:
                    return CodeGenConstants.StrITypeName;
                case EXmlStrMode.ELabelI:
                    return CodeGenConstants.LabelITypeName;
                default:
                    return CodeGenConstants.StrITypeName;
            }
        }
        
        /// <summary>
        /// 获取字段注释
        /// </summary>
        private string GetFieldComment(ConfigFieldMetadata field)
        {
            var parts = new List<string>();
            
            if (field.TypeInfo.IsContainer)
                parts.Add("容器");
            if (field.TypeInfo.IsNestedConfig)
                parts.Add("嵌套配置");
            if (field.IsXmlLink)
                parts.Add("Link");
            if (field.TypeInfo.IsNullable)
                parts.Add("可空");
            if (field.TypeInfo.IsEnum)
                parts.Add("枚举");
            if (field.IsIndexField)
                parts.Add($"索引: {string.Join(", ", field.IndexNames.Select(i => i.Item2.IndexName))}");
            
            return parts.Count > 0 ? string.Join(", ", parts) : null;
        }
        
        /// <summary>
        /// 生成Link字段的额外字段
        /// 单个Link: CfgS&lt;T&gt; LinkField → XBlobPtr&lt;T&gt; LinkField_ParentPtr + CfgI&lt;T&gt; LinkField_ParentIndex
        /// 列表Link: List&lt;CfgS&lt;T&gt;&gt; LinkField → XBlobArray&lt;XBlobPtr&lt;T&gt;&gt; LinkField_ParentPtr + XBlobArray&lt;CfgI&lt;T&gt;&gt; LinkField_ParentIndex
        /// </summary>
        private void GenerateLinkFields(ConfigFieldMetadata field)
        {
            var fieldName = field.FieldName;
            var targetTypeName = field.XmlLinkTargetType?.Name ?? CodeGenConstants.UnknownTypeName;
            
            // 获取目标类型的 Unmanaged 类型的全局限定名
            string targetUnmanagedTypeName;
            if (field.XmlLinkTargetType != null)
            {
                var unmanagedType = TypeHelper.EnsureUnmanagedSuffix(field.XmlLinkTargetType.Name);
                var targetNamespace = field.XmlLinkTargetType.Namespace;
                if (!string.IsNullOrEmpty(targetNamespace))
                    targetUnmanagedTypeName = $"global::{targetNamespace}.{unmanagedType}";
                else
                    targetUnmanagedTypeName = unmanagedType;
            }
            else
            {
                targetUnmanagedTypeName = CodeGenConstants.UnknownTypeName;
            }
            
            // 判断Link字段本身是否是容器类型(List<CfgS<T>>)
            var isListLink = field.TypeInfo.IsContainer;
            
            if (isListLink)
            {
                // List Link: 生成 XBlobArray<XBlobPtr<T>> 和 XBlobArray<CfgI<T>>
                // XBlobArray 在全局命名空间，使用 global:: 前缀
                _builder.AppendField(
                    $"global::XBlobArray<global::XBlobPtr<{targetUnmanagedTypeName}>>",
                    fieldName + TypeHelper.LinkParentPtrSuffix,
                    $"Link父节点指针 (指向{targetTypeName})"
                );
                
                _builder.AppendField(
                    $"global::XBlobArray<{TypeHelper.GetCfgITypeName(field.XmlLinkTargetType)}>",
                    fieldName + TypeHelper.LinkParentIndexSuffix,
                    $"Link父节点索引 (指向{targetTypeName})"
                );
            }
            else
            {
                // 单个Link: 生成 XBlobPtr<T> 和 CfgI<T>
                _builder.AppendField(
                    $"global::XBlobPtr<{targetUnmanagedTypeName}>",
                    fieldName + TypeHelper.LinkParentPtrSuffix,
                    $"Link父节点指针 (指向{targetTypeName})"
                );
                
                _builder.AppendField(
                    TypeHelper.GetCfgITypeName(field.XmlLinkTargetType),
                    fieldName + TypeHelper.LinkParentIndexSuffix,
                    $"Link父节点索引 (指向{targetTypeName})"
                );
            }
        }
        
        #endregion
        
        #region ToString方法生成
        
        /// <summary>
        /// 生成ToString方法
        /// </summary>
        private void GenerateToStringMethod()
        {
            _builder.AppendXmlComment(
                CodeGenConstants.ToStringMethodName + "方法",
                new Dictionary<string, string> { { "dataContainer", "数据容器" } }
            );
            
            _builder.BeginMethod("string ToString(object dataContainer)");
            
            // 简单实现: 返回类型名
            _builder.AppendLine($"return \"{_metadata.ManagedTypeName}\";");
            
            _builder.EndMethod();
        }
        
        #endregion
        
    }
}
