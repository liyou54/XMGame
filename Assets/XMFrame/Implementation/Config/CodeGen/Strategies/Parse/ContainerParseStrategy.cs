using XM.ConfigNew.CodeGen.Builders;
using XM.ConfigNew.Metadata;

namespace XM.ConfigNew.CodeGen.Strategies.Parse
{
    /// <summary>
    /// 容器解析策略
    /// 支持: List, Dictionary, HashSet
    /// </summary>
    public class ContainerParseStrategy : IParseStrategy
    {
        public bool CanHandle(ConfigFieldMetadata field)
        {
            return field?.TypeInfo?.IsContainer == true;
        }
        
        public void Generate(CodeGenContext ctx)
        {
            // 委托给原有的 ContainerParser 实现
            ContainerParser.GenerateParseLogic(ctx.Builder, ctx.FieldMetadata);
        }
    }
}
