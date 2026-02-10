using XM.ConfigNew.Metadata;

namespace XM.ConfigNew.CodeGen.Strategies.Assignment
{
    /// <summary>
    /// 可空类型字段赋值策略（T? -> T 转换）
    /// </summary>
    public class NullableAssignmentStrategy : IAssignmentStrategy
    {
        public bool CanHandle(ConfigFieldMetadata field)
        {
            return field?.TypeInfo?.IsNullable == true;
        }
        
        public void Generate(CodeGenContext ctx)
        {
            var field = ctx.FieldMetadata;
            var builder = ctx.Builder;
            
            var configFieldAccess = CodeBuilder.BuildConfigFieldAccess(field.FieldName);
            builder.AppendAssignment(
                CodeBuilder.BuildDataFieldAccess(field.FieldName), 
                CodeBuilder.BuildGetValueOrDefault(configFieldAccess));
        }
    }
}
