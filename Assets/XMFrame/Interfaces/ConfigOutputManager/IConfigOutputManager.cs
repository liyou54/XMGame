#if UNITY_EDITOR && ENABLE_CONFIG_OUTPUT
using XM.Contracts;

namespace XM.Contracts
{
    /// <summary>
    /// 配置输出管理器接口
    /// 用于在所有管理器初始化完成后输出配置数据，便于调试和验证
    /// 
    /// 条件编译：
    /// - UNITY_EDITOR: 仅在编辑器中可用
    /// - ENABLE_CONFIG_OUTPUT: 需要显式启用（在 Player Settings 的 Scripting Define Symbols 中添加）
    /// </summary>
    public interface IConfigOutputManager : IManager<IConfigOutputManager>
    {
        /// <summary>
        /// 输出所有配置数据到指定目录
        /// </summary>
        /// <param name="outputDirectory">输出目录路径，为空则使用默认路径</param>
        void OutputAllConfigs(string outputDirectory = null);

        /// <summary>
        /// 输出管理器信息
        /// </summary>
        /// <param name="outputDirectory">输出目录路径</param>
        void OutputManagerInfo(string outputDirectory = null);

        /// <summary>
        /// 输出 Mod 信息
        /// </summary>
        /// <param name="outputDirectory">输出目录路径</param>
        void OutputModInfo(string outputDirectory = null);

        /// <summary>
        /// 输出配置类型信息
        /// </summary>
        /// <param name="outputDirectory">输出目录路径</param>
        void OutputConfigTypeInfo(string outputDirectory = null);

        /// <summary>
        /// 获取默认输出目录
        /// </summary>
        string GetDefaultOutputDirectory();
    }
}
#endif
