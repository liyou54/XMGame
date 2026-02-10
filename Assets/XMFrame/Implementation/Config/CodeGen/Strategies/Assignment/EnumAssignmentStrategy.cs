using XM.ConfigNew.Metadata;

namespace XM.ConfigNew.CodeGen.Strategies.Assignment
{
    /// <summary>
    /// 枚举类型字段赋值策略（直接赋值）
    /// </summary>
    public class EnumAssignmentStrategy : IAssignmentStrategy
    {
        public bool CanHandle(ConfigFieldMetadata field)
        {
            return field?.TypeInfo?.IsEnum == true;
        }
        
        public void Generate(CodeGenContext ctx)
        {
            var field = ctx.FieldMetadata;
            var builder = ctx.Builder;
            
            builder.AppendAssignment(
                CodeBuilder.BuildDataFieldAccess(field.FieldName), 
                CodeBuilder.BuildConfigFieldAccess(field.FieldName));
        }
    }
}
