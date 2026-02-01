using NUnit.Framework;

namespace XM.Editor.Tests
{
    /// <summary>
    /// 纯函数测试基类
    /// 特点：
    /// 1. 无副作用，无需Mock
    /// 2. 执行速度快（毫秒级）
    /// 3. 测试确定性强，无外部依赖
    /// 
    /// 适用场景：
    /// - 算法类（如TopologicalSorter）
    /// - 工具函数（如ConfigParseHelper）
    /// - 类型分析器的纯函数部分
    /// </summary>
    [Category(TestCategories.Pure)]
    public abstract class PureFunctionTestBase : TestBase
    {
        [SetUp]
        public override void Setup()
        {
            base.Setup();
            // 纯函数测试通常无需额外Setup
        }
        
        [TearDown]
        public override void Teardown()
        {
            // 纯函数测试通常无需额外Teardown
            base.Teardown();
        }
    }
}
