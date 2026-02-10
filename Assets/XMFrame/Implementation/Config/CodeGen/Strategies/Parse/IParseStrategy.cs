using XM.ConfigNew.Metadata;

namespace XM.ConfigNew.CodeGen.Strategies.Parse
{
    /// <summary>
    /// Parse 方法生成策略接口
    /// 每种字段类型（基本类型、容器、嵌套配置、CfgS等）实现一个策略
    /// </summary>
    public interface IParseStrategy
    {
        /// <summary>
        /// 判断是否可以处理该字段
        /// </summary>
        /// <param name="field">字段元数据</param>
        /// <returns>true 表示可以处理</returns>
        bool CanHandle(ConfigFieldMetadata field);
        
        /// <summary>
        /// 生成 Parse 方法体代码
        /// </summary>
        /// <param name="ctx">代码生成上下文</param>
        void Generate(CodeGenContext ctx);
    }
}
