using System;
using XM.ConfigNew.Metadata;

namespace XM.ConfigNew.CodeGen.Strategies.Assignment
{
    /// <summary>
    /// CfgS 类型字段赋值策略（CfgS -> CfgI 转换）
    /// </summary>
    public class CfgSAssignmentStrategy : IAssignmentStrategy
    {
        public bool CanHandle(ConfigFieldMetadata field)
        {
            var managedType = field.TypeInfo?.ManagedFieldType;
            
            if (managedType == null)
                return false;
            
            // CfgS<T> 类型或者 XMLLink 字段
            return TypeHelper.IsCfgSType(managedType) || field.IsXmlLink;
        }
        
        public void Generate(CodeGenContext ctx)
        {
            var field = ctx.FieldMetadata;
            var builder = ctx.Builder;
            
            var configFieldAccess = CodeBuilder.BuildConfigFieldAccess(field.FieldName);
            builder.BeginIfBlock($"{CodeGenConstants.TryGetCfgIMethod}({configFieldAccess}, out var {field.FieldName}CfgI)");
            
            // 获取目标非托管类型名称
            Type targetType = field.XmlLinkTargetType;
            if (targetType == null && field.TypeInfo?.ManagedFieldType != null)
            {
                // 从 CfgS<T> 获取 T
                targetType = TypeHelper.GetContainerElementType(field.TypeInfo.ManagedFieldType);
            }
            
            var targetUnmanagedTypeName = ctx.GetXmlLinkUnmanagedTypeName(targetType);
            
            // XMLLink 字段和普通 CfgS 字段都赋值到原字段
            // XMLLink 字段的原字段在 Unmanaged 中已经是 CfgI 类型
            builder.AppendAssignment(
                CodeBuilder.BuildDataFieldAccess(field.FieldName), 
                $"{field.FieldName}CfgI.As<{targetUnmanagedTypeName}>()");
            builder.EndBlock();
        }
    }
}
