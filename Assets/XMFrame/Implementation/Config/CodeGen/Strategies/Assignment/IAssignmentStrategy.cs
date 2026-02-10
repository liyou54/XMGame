using XM.ConfigNew.Metadata;

namespace XM.ConfigNew.CodeGen.Strategies.Assignment
{
    /// <summary>
    /// Assignment 字段赋值策略接口
    /// 用于生成从托管配置到非托管数据的字段赋值代码
    /// </summary>
    public interface IAssignmentStrategy
    {
        /// <summary>
        /// 判断是否可以处理该字段
        /// </summary>
        /// <param name="field">字段元数据</param>
        /// <returns>true 表示可以处理</returns>
        bool CanHandle(ConfigFieldMetadata field);
        
        /// <summary>
        /// 生成字段赋值代码
        /// </summary>
        /// <param name="ctx">代码生成上下文</param>
        void Generate(CodeGenContext ctx);
    }
}
