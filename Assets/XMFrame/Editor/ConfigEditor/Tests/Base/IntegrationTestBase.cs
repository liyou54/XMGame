using System;
using System.IO;
using NUnit.Framework;
using XM.Contracts;

namespace XM.Editor.Tests
{
    /// <summary>
    /// 集成测试基类
    /// 特点：
    /// 1. 使用真实依赖，不使用Mock
    /// 2. 测试多个模块协作
    /// 3. 执行时间较长（秒级）
    /// 4. 提供临时测试数据目录
    /// 
    /// 适用场景：
    /// - 完整配置加载流程测试
    /// - Mod优先级和配置覆盖测试
    /// - 配置引用链完整性测试
    /// </summary>
    [Category(TestCategories.Integration)]
    public abstract class IntegrationTestBase : TestBase
    {
        /// <summary>临时测试数据目录，每个测试独立</summary>
        protected string TestDataDirectory { get; private set; }
        
        /// <summary>真实的ConfigDataCenter实例（非Mock）</summary>
        protected XM.ConfigDataCenter ConfigDataCenter { get; private set; }
        
        /// <summary>真实的ModManager实例（非Mock）</summary>
        protected IModManager ModManager { get; private set; }
        
        [SetUp]
        public override void Setup()
        {
            base.Setup();
            
            // 创建临时测试数据目录
            TestDataDirectory = Path.Combine(
                Path.GetTempPath(), 
                $"XMFrameTest_{Guid.NewGuid():N}");
            Directory.CreateDirectory(TestDataDirectory);
            
            // TODO: 初始化真实组件（等待实现ConfigDataCenter的构造方法）
            // ModManager = CreateRealModManager();
            // ConfigDataCenter = CreateRealConfigDataCenter(ModManager);
        }
        
        [TearDown]
        public override void Teardown()
        {
            // 清理测试数据
            if (Directory.Exists(TestDataDirectory))
            {
                try
                {
                    Directory.Delete(TestDataDirectory, recursive: true);
                }
                catch
                {
                    // 忽略删除失败（可能被占用）
                }
            }
            
            ConfigDataCenter = null;
            ModManager = null;
            
            base.Teardown();
        }
        
        /// <summary>
        /// 将测试XML内容写入临时文件
        /// </summary>
        /// <param name="relativePath">相对于TestDataDirectory的路径</param>
        /// <param name="content">XML内容</param>
        protected void WriteTestXmlFile(string relativePath, string content)
        {
            var fullPath = Path.Combine(TestDataDirectory, relativePath);
            var directory = Path.GetDirectoryName(fullPath);
            
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            
            File.WriteAllText(fullPath, content);
        }
        
        /// <summary>
        /// 读取测试XML文件内容
        /// </summary>
        protected string ReadTestXmlFile(string relativePath)
        {
            var fullPath = Path.Combine(TestDataDirectory, relativePath);
            return File.Exists(fullPath) ? File.ReadAllText(fullPath) : null;
        }
    }
}
