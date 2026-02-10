using XM.ConfigNew.Metadata;

namespace XM.ConfigNew.CodeGen.Strategies.Alloc
{
    /// <summary>
    /// Alloc 容器分配策略接口
    /// 用于生成容器类型的分配方法（AllocXXX）
    /// </summary>
    public interface IAllocStrategy
    {
        /// <summary>
        /// 判断是否可以处理该字段
        /// </summary>
        /// <param name="field">字段元数据</param>
        /// <returns>true 表示可以处理</returns>
        bool CanHandle(ConfigFieldMetadata field);
        
        /// <summary>
        /// 生成容器分配方法
        /// </summary>
        /// <param name="ctx">代码生成上下文</param>
        void GenerateAllocMethod(CodeGenContext ctx);
    }
}
