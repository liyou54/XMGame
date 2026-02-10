using XM.ConfigNew.Metadata;

namespace XM.ConfigNew.CodeGen.Strategies.Assignment
{
    /// <summary>
    /// 直接赋值策略（基本类型、结构体等）
    /// 兜底策略，优先级最低
    /// </summary>
    public class DirectAssignmentStrategy : IAssignmentStrategy
    {
        public bool CanHandle(ConfigFieldMetadata field)
        {
            // 兜底策略，总是返回 true
            return true;
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
