using System;
using XM.ConfigNew.CodeGen.Builders;
using XM.ConfigNew.Metadata;

namespace XM.ConfigNew.CodeGen.Strategies.Parse
{
    /// <summary>
    /// 基本类型解析策略
    /// 支持: int, float, bool, string, 枚举, 可空类型
    /// </summary>
    public class BasicTypeParseStrategy : IParseStrategy
    {
        public bool CanHandle(ConfigFieldMetadata field)
        {
            if (field?.TypeInfo == null)
                return false;
            
            // 容器、嵌套配置、CfgS、XmlKey 都不由本策略处理
            if (field.IsContainer || field.IsNestedConfig || field.IsXmlLink || field.IsXmlKey)
                return false;
            
            // 其他都视为基本类型（兜底策略）
            return true;
        }
        
        public void Generate(CodeGenContext ctx)
        {
            // 委托给原有的 BasicTypeParser 实现
            BasicTypeParser.GenerateParseLogic(ctx.Builder, ctx.FieldMetadata);
        }
    }
}
