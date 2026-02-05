using System;

namespace XM.ConfigNew.CodeGen.Builders
{
    /// <summary>
    /// 配置类型 Helper 调用生成器
    /// 统一处理配置类型的分配和填充逻辑：获取 Helper → null检查 → 创建Unmanaged → AllocContainerWithFillImpl → 赋值
    /// 消除 ElementValueGenerator 和 ContainerAllocBuilder 中的重复代码
    /// </summary>
    public static class ConfigHelperInvoker
    {
        /// <summary>
        /// 生成配置类型的索引赋值（List/Array场景）
        /// </summary>
        public static void GenerateIndexAssignment(
            CodeBuilder builder,
            Type configType,
            string sourceExpr,
            string targetArrayVar,
            string indexVar)
        {
            var (helperTypeName, unmanagedTypeName) = GetTypeNames(configType);
            var helperVar = $"helper_{indexVar}";
            var dataVar = $"itemData_{indexVar}";
            
            builder.BeginIfBlock(CodeBuilder.BuildNotNullCondition(sourceExpr));
            builder.AppendVarDeclaration(helperVar, $"{helperTypeName}.{CodeGenConstants.InstanceProperty}");
            builder.BeginIfBlock(CodeBuilder.BuildNotNullCondition(helperVar));
            builder.AppendNewVarDeclaration(dataVar, unmanagedTypeName);
            builder.AppendLine($"{helperVar}.{CodeGenConstants.AllocContainerWithFillImplMethod}({sourceExpr}, {CodeGenConstants.DefaultTblI}, {CodeGenConstants.CfgIVar}, ref {dataVar}, {CodeGenConstants.ConfigHolderDataVar});");
            builder.AppendBlobIndexAssign(targetArrayVar, indexVar, dataVar);
            builder.EndBlock();
            builder.EndBlock();
        }
        
        /// <summary>
        /// 生成配置类型的 Map 赋值（Dictionary Value场景）
        /// </summary>
        public static void GenerateMapAssignment(
            CodeBuilder builder,
            Type configType,
            string sourceExpr,
            string targetMapVar,
            string keyExpr,
            string suffix)
        {
            var (helperTypeName, unmanagedTypeName) = GetTypeNames(configType);
            var helperVar = $"cfgHelper{suffix}";
            var dataVar = $"cfgItemData{suffix}";
            
            builder.BeginIfBlock(CodeBuilder.BuildNotNullCondition(sourceExpr));
            builder.AppendVarDeclaration(helperVar, $"{helperTypeName}.{CodeGenConstants.InstanceProperty}");
            builder.BeginIfBlock(CodeBuilder.BuildNotNullCondition(helperVar));
            builder.AppendNewVarDeclaration(dataVar, unmanagedTypeName);
            builder.AppendLine($"{helperVar}.{CodeGenConstants.AllocContainerWithFillImplMethod}({sourceExpr}, {CodeGenConstants.DefaultTblI}, {CodeGenConstants.CfgIVar}, ref {dataVar}, {CodeGenConstants.ConfigHolderDataVar});");
            builder.AppendBlobMapAssign(targetMapVar, keyExpr, dataVar);
            builder.EndBlock();
            builder.EndBlock();
        }
        
        /// <summary>
        /// 获取配置类型的 Helper 和 Unmanaged 类型名（使用统一方法）
        /// </summary>
        private static (string helperTypeName, string unmanagedTypeName) GetTypeNames(Type configType)
        {
            var configTypeName = TypeHelper.GetGlobalQualifiedTypeName(configType);
            var helperTypeName = configTypeName + CodeGenConstants.ClassHelperSuffix;
            var unmanagedTypeName = TypeHelper.GetConfigUnmanagedTypeName(configType);
            
            return (helperTypeName, unmanagedTypeName);
        }
    }
}
