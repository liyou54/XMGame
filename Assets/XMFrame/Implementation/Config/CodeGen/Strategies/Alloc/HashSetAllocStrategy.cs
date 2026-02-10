using XM.ConfigNew.CodeGen.Builders;
using XM.ConfigNew.Metadata;

namespace XM.ConfigNew.CodeGen.Strategies.Alloc
{
    /// <summary>
    /// HashSet 容器分配策略
    /// </summary>
    public class HashSetAllocStrategy : IAllocStrategy
    {
        public bool CanHandle(ConfigFieldMetadata field)
        {
            return field?.TypeInfo?.ContainerType == EContainerType.HashSet;
        }
        
        public void GenerateAllocMethod(CodeGenContext ctx)
        {
            // 委托给原有的 ContainerAllocBuilder 实现
            ContainerAllocBuilder.GenerateAllocMethod(
                ctx.Builder, 
                ctx.FieldMetadata, 
                ctx.ManagedTypeName, 
                ctx.UnmanagedTypeName);
        }
    }
}
