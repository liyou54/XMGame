using NUnit.Framework;
using XM.Editor.Tests.Fakes;
using XM.Editor.Tests.Fixtures;

namespace XM.Editor.Tests
{
    /// <summary>
    /// 所有测试的基类，提供通用的Setup/Teardown和辅助方法
    /// 职责：
    /// 1. 提供MockFactory、TestDataBuilder、AssertHelpers等工具
    /// 2. 统一测试生命周期管理
    /// 3. 提供Given-When-Then结构化测试方法
    /// </summary>
    [TestFixture]
    public abstract class TestBase
    {
        /// <summary>Mock工厂，用于创建预配置的Mock/Fake对象</summary>
        protected MockFactory MockFactory { get; private set; }
        
        /// <summary>测试数据构建器，用于创建测试数据</summary>
        protected TestDataBuilder DataBuilder { get; private set; }
        
        /// <summary>增强断言辅助类</summary>
        protected AssertHelpers AssertEx { get; private set; }
        
        /// <summary>
        /// 测试初始化，在每个测试方法执行前运行
        /// 子类可以重写此方法添加额外的初始化逻辑，但必须调用base.Setup()
        /// </summary>
        [SetUp]
        public virtual void Setup()
        {
            MockFactory = new MockFactory();
            DataBuilder = new TestDataBuilder();
            AssertEx = new AssertHelpers();
        }
        
        /// <summary>
        /// 测试清理，在每个测试方法执行后运行
        /// 子类可以重写此方法添加额外的清理逻辑，但必须调用base.Teardown()
        /// </summary>
        [TearDown]
        public virtual void Teardown()
        {
            MockFactory?.Dispose();
            MockFactory = null;
            DataBuilder = null;
            AssertEx = null;
        }
    }
}
