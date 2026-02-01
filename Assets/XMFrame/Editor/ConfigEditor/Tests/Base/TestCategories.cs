namespace XM.Editor.Tests
{
    /// <summary>
    /// 测试类别常量，用于NUnit的Category特性
    /// 支持通过类别筛选运行测试
    /// </summary>
    public static class TestCategories
    {
        /// <summary>纯函数测试 - 无副作用，快速执行</summary>
        public const string Pure = "Pure";
        
        /// <summary>单元测试 - 有副作用，使用Mock/Fake对象</summary>
        public const string Unit = "Unit";
        
        /// <summary>集成测试 - 多模块协作，使用真实依赖</summary>
        public const string Integration = "Integration";
        
        /// <summary>性能测试 - 验证性能指标</summary>
        public const string Performance = "Performance";
        
        /// <summary>边界条件测试 - 特殊输入、异常场景</summary>
        public const string EdgeCase = "EdgeCase";
    }
}
