using NUnit.Framework;
using XM.Contracts;

namespace XM.Editor.Tests
{
    /// <summary>
    /// 带Mock的单元测试基类
    /// 特点：
    /// 1. 有副作用，需要Mock/Fake对象隔离依赖
    /// 2. 使用简化的Fake对象代替复杂Mock
    /// 3. 测试单个类的行为，隔离外部依赖
    /// 
    /// 适用场景：
    /// - ConfigDataCenter（需要Mock IModManager）
    /// - ClassHelperGenerator（需要Mock文件系统）
    /// - 任何有外部依赖的类
    /// </summary>
    [Category(TestCategories.Unit)]
    public abstract class UnitTestWithMocksBase : TestBase
    {
        /// <summary>预配置的FakeModManager，大多数单元测试会用到</summary>
        protected Fakes.FakeModManager FakeModManager { get; private set; }
        
        [SetUp]
        public override void Setup()
        {
            base.Setup();
            
            // 创建默认的FakeModManager（单个测试Mod）
            FakeModManager = MockFactory.CreateModManagerWithSingleMod();
        }
        
        [TearDown]
        public override void Teardown()
        {
            FakeModManager = null;
            base.Teardown();
        }
    }
}
