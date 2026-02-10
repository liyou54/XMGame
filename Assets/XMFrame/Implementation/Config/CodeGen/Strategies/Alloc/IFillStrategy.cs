using XM.ConfigNew.Metadata;

namespace XM.ConfigNew.CodeGen.Strategies.Alloc
{
    /// <summary>
    /// Fill 嵌套配置填充策略接口
    /// 用于生成嵌套配置类型的填充方法（FillXXX）
    /// </summary>
    public interface IFillStrategy
    {
        /// <summary>
        /// 判断是否可以处理该字段
        /// </summary>
        /// <param name="field">字段元数据</param>
        /// <returns>true 表示可以处理</returns>
        bool CanHandle(ConfigFieldMetadata field);
        
        /// <summary>
        /// 生成嵌套配置填充方法
        /// </summary>
        /// <param name="ctx">代码生成上下文</param>
        void GenerateFillMethod(CodeGenContext ctx);
    }
}
