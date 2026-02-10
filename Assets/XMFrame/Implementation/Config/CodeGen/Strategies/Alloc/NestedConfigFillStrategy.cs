using XM.ConfigNew.CodeGen.Builders;
using XM.ConfigNew.Metadata;

namespace XM.ConfigNew.CodeGen.Strategies.Alloc
{
    /// <summary>
    /// 嵌套配置填充策略
    /// </summary>
    public class NestedConfigFillStrategy : IFillStrategy
    {
        public bool CanHandle(ConfigFieldMetadata field)
        {
            // 非容器的嵌套配置
            return field?.IsNestedConfig == true && !field.IsContainer;
        }
        
        public void GenerateFillMethod(CodeGenContext ctx)
        {
            // 委托给原有的 NestedConfigAllocBuilder 实现
            NestedConfigAllocBuilder.GenerateFillMethod(
                ctx.Builder, 
                ctx.FieldMetadata, 
                ctx.ManagedTypeName, 
                ctx.UnmanagedTypeName);
        }
    }
}
