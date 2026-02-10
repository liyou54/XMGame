using XM.ConfigNew.CodeGen;
using XM.ConfigNew.Metadata;

namespace XM.ConfigNew.CodeGen.Strategies.Assignment
{
    /// <summary>
    /// 基于 XmlTypeConverter 的托管->非托管转换赋值策略
    /// 通过分析 [assembly: XmlTypeConverter(typeof(Xxx), true)] 得到 UnmanagedConverterType，
    /// 生成调用 converter.Convert(source, modName, out target) 的代码
    /// </summary>
    public class XAssetPathAssignmentStrategy : IAssignmentStrategy
    {
        public bool CanHandle(ConfigFieldMetadata field)
        {
            return field?.Converter?.UnmanagedConverterType != null;
        }
        
        public void Generate(CodeGenContext ctx)
        {
            var field = ctx.FieldMetadata;
            var builder = ctx.Builder;
            
            var converterType = field.Converter.UnmanagedConverterType;
            var converterTypeName = TypeHelper.GetGlobalQualifiedTypeName(converterType);
            var configFieldAccess = CodeBuilder.BuildConfigFieldAccess(field.FieldName);
            var dataFieldAccess = CodeBuilder.BuildDataFieldAccess(field.FieldName);
            var tempVarName = field.FieldName + "Unmanaged";
            
            // XAssetPathToIConvert.I.Convert(config.AssetPath, cfgi.Mod.GetModName(), out var AssetPathUnmanaged)
            var modNameExpr = $"{CodeGenConstants.CfgIVar}.Mod.GetModName()";
            var converterInvoke = TypeHelper.IsTypeConverterBase(converterType)
                ? $"{converterTypeName}.I.Convert({configFieldAccess}, {modNameExpr}, out var {tempVarName})"
                : $"new {converterTypeName}().Convert({configFieldAccess}, {modNameExpr}, out var {tempVarName})";
            builder.BeginIfBlock(converterInvoke);
            builder.AppendAssignment(dataFieldAccess, tempVarName);
            builder.EndBlock();
        }
    }
}
