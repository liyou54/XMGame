using System;
using XM.ConfigNew.Metadata;
using XM.Utils.Attribute;

namespace XM.ConfigNew.CodeGen.Builders
{
    /// <summary>
    /// 字段赋值代码生成器
    /// 统一处理从 Managed Config 到 Unmanaged Data 的字段赋值逻辑
    /// 消除 XmlHelperGenerator 中的硬编码类型判断
    /// </summary>
    public static class FieldAssignmentGenerator
    {
        /// <summary>
        /// 字段赋值类型分类
        /// </summary>
        public enum FieldAssignmentType
        {
            /// <summary>CfgS 类型（需要转换为 CfgI）</summary>
            CfgS,
            
            /// <summary>LabelS 类型（需要转换为 LabelI）</summary>
            LabelS,
            
            /// <summary>可空类型（需要 GetValueOrDefault）</summary>
            Nullable,
            
            /// <summary>枚举类型（直接赋值）</summary>
            Enum,
            
            /// <summary>字符串类型（根据 StringMode 转换）</summary>
            String,
            
            /// <summary>基本类型（直接赋值）</summary>
            Direct
        }
        
        /// <summary>
        /// 获取字段赋值类型分类
        /// </summary>
        public static FieldAssignmentType GetAssignmentType(ConfigFieldMetadata field)
        {
            var managedType = field.TypeInfo?.ManagedFieldType;
            
            if (managedType == null)
                return FieldAssignmentType.Direct;
            
            // 1. CfgS<T> 类型（优先级最高，包括 XMLLink）
            if (TypeHelper.IsCfgSType(managedType) || field.IsXmlLink)
                return FieldAssignmentType.CfgS;
            
            // 2. LabelS 类型
            if (managedType.Name == "LabelS")
                return FieldAssignmentType.LabelS;
            
            // 3. 可空类型
            if (field.TypeInfo.IsNullable)
                return FieldAssignmentType.Nullable;
            
            // 4. 枚举类型
            if (field.TypeInfo.IsEnum)
                return FieldAssignmentType.Enum;
            
            // 5. 字符串类型
            if (managedType == typeof(string))
                return FieldAssignmentType.String;
            
            // 6. 其他（基本类型、结构体等）
            return FieldAssignmentType.Direct;
        }
        
        /// <summary>
        /// 生成字段赋值代码（统一入口）
        /// </summary>
        public static void GenerateAssignment(CodeBuilder builder, ConfigFieldMetadata field, Func<Type, string> getUnmanagedTypeNameFunc)
        {
            var assignmentType = GetAssignmentType(field);
            
            switch (assignmentType)
            {
                case FieldAssignmentType.CfgS:
                    GenerateCfgSAssignment(builder, field, getUnmanagedTypeNameFunc);
                    break;
                
                case FieldAssignmentType.LabelS:
                    GenerateLabelSAssignment(builder, field);
                    break;
                
                case FieldAssignmentType.Nullable:
                    GenerateNullableAssignment(builder, field);
                    break;
                
                case FieldAssignmentType.Enum:
                    GenerateEnumAssignment(builder, field);
                    break;
                
                case FieldAssignmentType.String:
                    GenerateStringAssignment(builder, field);
                    break;
                
                case FieldAssignmentType.Direct:
                default:
                    GenerateDirectAssignment(builder, field);
                    break;
            }
        }
        
        #region 各类型赋值实现
        
        /// <summary>
        /// 生成 CfgS 类型字段赋值（CfgS -> CfgI 转换）
        /// </summary>
        private static void GenerateCfgSAssignment(CodeBuilder builder, ConfigFieldMetadata field, Func<Type, string> getUnmanagedTypeNameFunc)
        {
            var configFieldAccess = CodeBuilder.BuildConfigFieldAccess(field.FieldName);
            builder.BeginIfBlock($"{CodeGenConstants.TryGetCfgIMethod}({configFieldAccess}, out var {field.FieldName}CfgI)");
            
            // 获取目标非托管类型名称
            Type targetType = field.XmlLinkTargetType;
            if (targetType == null && field.TypeInfo?.ManagedFieldType != null)
            {
                // 从 CfgS<T> 获取 T
                targetType = TypeHelper.GetContainerElementType(field.TypeInfo.ManagedFieldType);
            }
            
            var targetUnmanagedTypeName = getUnmanagedTypeNameFunc(targetType);
            builder.AppendAssignment(
                CodeBuilder.BuildDataFieldAccess(field.FieldName), 
                $"{field.FieldName}CfgI.As<{targetUnmanagedTypeName}>()");
            builder.EndBlock();
        }
        
        /// <summary>
        /// 生成 LabelS 类型字段赋值（LabelS -> LabelI 转换）
        /// </summary>
        private static void GenerateLabelSAssignment(CodeBuilder builder, ConfigFieldMetadata field)
        {
            var configFieldAccess = CodeBuilder.BuildConfigFieldAccess(field.FieldName);
            builder.BeginIfBlock($"TryGetLabelI({configFieldAccess}, out var {field.FieldName}LabelI)");
            builder.AppendAssignment(CodeBuilder.BuildDataFieldAccess(field.FieldName), $"{field.FieldName}LabelI");
            builder.EndBlock();
        }
        
        /// <summary>
        /// 生成可空类型字段赋值（T? -> T 转换）
        /// </summary>
        private static void GenerateNullableAssignment(CodeBuilder builder, ConfigFieldMetadata field)
        {
            var configFieldAccess = CodeBuilder.BuildConfigFieldAccess(field.FieldName);
            builder.AppendAssignment(
                CodeBuilder.BuildDataFieldAccess(field.FieldName), 
                CodeBuilder.BuildGetValueOrDefault(configFieldAccess));
        }
        
        /// <summary>
        /// 生成枚举类型字段赋值（直接赋值）
        /// </summary>
        private static void GenerateEnumAssignment(CodeBuilder builder, ConfigFieldMetadata field)
        {
            builder.AppendAssignment(
                CodeBuilder.BuildDataFieldAccess(field.FieldName), 
                CodeBuilder.BuildConfigFieldAccess(field.FieldName));
        }
        
        /// <summary>
        /// 生成字符串类型字段赋值（根据 StringMode 转换）
        /// </summary>
        private static void GenerateStringAssignment(CodeBuilder builder, ConfigFieldMetadata field)
        {
            var configFieldAccess = CodeBuilder.BuildConfigFieldAccess(field.FieldName);
            var dataFieldAccess = CodeBuilder.BuildDataFieldAccess(field.FieldName);
            
            switch (field.StringMode)
            {
                case EXmlStrMode.EFix32:
                    // FixedString32Bytes
                    builder.AppendAssignment(dataFieldAccess, 
                        $"new global::Unity.Collections.FixedString32Bytes({configFieldAccess} ?? string.Empty)");
                    break;
                
                case EXmlStrMode.EFix64:
                    // FixedString64Bytes
                    builder.AppendAssignment(dataFieldAccess, 
                        $"new global::Unity.Collections.FixedString64Bytes({configFieldAccess} ?? string.Empty)");
                    break;
                
                case EXmlStrMode.ELabelI:
                    // LabelI 需要转换
                    builder.BeginIfBlock($"TryGetLabelI({configFieldAccess}, out var {field.FieldName}LabelI)");
                    builder.AppendAssignment(dataFieldAccess, $"{field.FieldName}LabelI");
                    builder.EndBlock();
                    break;
                
                case EXmlStrMode.EStrI:
                default:
                    // StrI 需要转换（默认）
                    builder.BeginIfBlock($"{CodeGenConstants.TryGetStrIMethod}({configFieldAccess}, out var {field.FieldName}StrI)");
                    builder.AppendAssignment(dataFieldAccess, $"{field.FieldName}StrI");
                    builder.EndBlock();
                    break;
            }
        }
        
        /// <summary>
        /// 生成直接赋值（基本类型、结构体等）
        /// </summary>
        private static void GenerateDirectAssignment(CodeBuilder builder, ConfigFieldMetadata field)
        {
            builder.AppendAssignment(
                CodeBuilder.BuildDataFieldAccess(field.FieldName), 
                CodeBuilder.BuildConfigFieldAccess(field.FieldName));
        }
        
        #endregion
    }
}
