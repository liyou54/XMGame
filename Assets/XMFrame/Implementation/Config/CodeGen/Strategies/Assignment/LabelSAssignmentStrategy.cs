using XM.ConfigNew.Metadata;

namespace XM.ConfigNew.CodeGen.Strategies.Assignment
{
    /// <summary>
    /// LabelS 类型字段赋值策略（LabelS -> LabelI 转换）
    /// </summary>
    public class LabelSAssignmentStrategy : IAssignmentStrategy
    {
        public bool CanHandle(ConfigFieldMetadata field)
        {
            var managedType = field.TypeInfo?.ManagedFieldType;
            return managedType != null && managedType.Name == "LabelS";
        }
        
        public void Generate(CodeGenContext ctx)
        {
            var field = ctx.FieldMetadata;
            var builder = ctx.Builder;
            
            var configFieldAccess = CodeBuilder.BuildConfigFieldAccess(field.FieldName);
            builder.BeginIfBlock($"TryGetLabelI({configFieldAccess}, out var {field.FieldName}LabelI)");
            builder.AppendAssignment(CodeBuilder.BuildDataFieldAccess(field.FieldName), $"{field.FieldName}LabelI");
            builder.EndBlock();
        }
    }
}
