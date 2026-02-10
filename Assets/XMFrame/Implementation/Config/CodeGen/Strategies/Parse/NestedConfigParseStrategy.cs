using XM.ConfigNew.CodeGen.Builders;
using XM.ConfigNew.Metadata;

namespace XM.ConfigNew.CodeGen.Strategies.Parse
{
    /// <summary>
    /// 嵌套配置解析策略
    /// 支持: 单个嵌套配置、嵌套配置列表
    /// </summary>
    public class NestedConfigParseStrategy : IParseStrategy
    {
        public bool CanHandle(ConfigFieldMetadata field)
        {
            return field?.TypeInfo?.IsNestedConfig == true;
        }
        
        public void Generate(CodeGenContext ctx)
        {
            var field = ctx.FieldMetadata;
            
            // 判断是单个还是容器中的嵌套配置
            if (field.TypeInfo.IsContainer)
            {
                // 容器中的嵌套配置
                NestedConfigParser.GenerateListParse(ctx.Builder, field);
            }
            else
            {
                // 单个嵌套配置
                NestedConfigParser.GenerateSingleParse(ctx.Builder, field);
            }
        }
    }
}
