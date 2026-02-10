using XM.ConfigNew.CodeGen.Builders;
using XM.ConfigNew.Metadata;

namespace XM.ConfigNew.CodeGen.Strategies.Parse
{
    /// <summary>
    /// CfgS/XMLLink 解析策略
    /// 支持: CfgS<T>、XMLLink 字段
    /// </summary>
    public class CfgSParseStrategy : IParseStrategy
    {
        public bool CanHandle(ConfigFieldMetadata field)
        {
            return field?.IsXmlLink == true;
        }
        
        public void Generate(CodeGenContext ctx)
        {
            // 委托给原有的 CfgSParser 实现
            CfgSParser.GenerateParseLogic(ctx.Builder, ctx.FieldMetadata);
        }
    }
}
