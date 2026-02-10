using System;
using XM.ConfigNew.Metadata;

namespace XM.ConfigNew.CodeGen.Builders
{
    /// <summary>
    /// 嵌套配置填充代码生成器
    /// 负责生成 FillXXX 方法，将嵌套配置转换为非托管结构
    /// </summary>
    public static class NestedConfigAllocBuilder
    {
        /// <summary>
        /// 生成嵌套配置填充方法的完整实现
        /// </summary>
        public static void GenerateFillMethod(CodeBuilder builder, ConfigFieldMetadata field, string managedTypeName, string unmanagedTypeName)
        {
            // 使用统一方法获取类型名（确保全局限定名和正确的 Unmanaged 类型）
            var nestedManagedType = field.TypeInfo.ManagedFieldType;
            var nestedTypeName = TypeHelper.GetGlobalQualifiedTypeName(nestedManagedType);
            var helperTypeName = nestedTypeName + CodeGenConstants.ClassHelperSuffix;
            var nestedUnmanagedTypeName = TypeHelper.GetConfigUnmanagedTypeName(nestedManagedType);
            
            builder.AppendXmlComment($"填充 {field.FieldName} 嵌套配置");
            builder.BeginPrivateMethod(
                $"void {field.FillMethodName}({managedTypeName} config, ref {unmanagedTypeName} data, CfgI cfgi, XM.ConfigDataCenter.ConfigDataHolder configHolderData)",
                null);
            
            // null 检查
            builder.BeginIfBlock(CodeBuilder.BuildNotNullCondition(CodeBuilder.BuildConfigFieldAccess(field.FieldName)) + " == false");
            builder.AppendLine("return;");
            builder.EndBlock();
            builder.AppendLine();
            
            // 获取嵌套配置的 Helper（使用静态实例）
            builder.AppendVarDeclaration("helper", $"{helperTypeName}.{CodeGenConstants.InstanceProperty}");
            builder.BeginIfBlock(CodeBuilder.BuildNotNullCondition("helper"));
            
            // 创建非托管结构并填充
            builder.AppendNewVarDeclaration("nestedData", nestedUnmanagedTypeName);
            builder.AppendLine($"helper.{CodeGenConstants.AllocContainerWithFillImplMethod}({CodeBuilder.BuildConfigFieldAccess(field.FieldName)}, {CodeGenConstants.DefaultTblI}, {CodeGenConstants.CfgIVar}, ref nestedData, {CodeGenConstants.ConfigHolderDataVar});");
            builder.AppendAssignment(CodeBuilder.BuildDataFieldAccess(field.FieldName), "nestedData");
            
            builder.EndBlock(); // if helper != null
            
            builder.EndMethod();
        }
    }
}
