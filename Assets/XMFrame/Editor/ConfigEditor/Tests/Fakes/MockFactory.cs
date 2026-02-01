using System;
using System.Collections.Generic;
using XM.Contracts;
using XM.Contracts.Config;

namespace XM.Editor.Tests.Fakes
{
    /// <summary>
    /// Mock工厂，提供预配置的常见Mock/Fake对象
    /// 职责：
    /// 1. 统一创建Mock对象，减少重复代码
    /// 2. 提供常见测试场景的预配置
    /// 3. 管理Mock对象的生命周期
    /// </summary>
    public class MockFactory : IDisposable
    {
        private readonly List<IDisposable> _disposables = new List<IDisposable>();
        
        /// <summary>
        /// 创建带有单个Mod的FakeModManager
        /// </summary>
        /// <param name="modName">Mod名称，默认"TestMod"</param>
        /// <param name="modId">Mod ID，默认1</param>
        /// <returns>预配置的FakeModManager</returns>
        public FakeModManager CreateModManagerWithSingleMod(
            string modName = "TestMod", 
            short modId = 1)
        {
            var modIdStruct = new ModI(modId);
            var fake = new FakeModManager()
                .WithMod(modName, modIdStruct, $"TestData/{modName}/config.xml");
            
            return fake;
        }
        
        /// <summary>
        /// 创建带有多个Mod的FakeModManager（测试Mod优先级）
        /// </summary>
        /// <param name="modConfigs">Mod配置列表，格式：(名称, ID, 优先级)</param>
        public FakeModManager CreateModManagerWithMultipleMods(
            params (string name, short id, int sortIndex)[] modConfigs)
        {
            var fake = new FakeModManager();
            
            foreach (var (name, id, sortIndex) in modConfigs)
            {
                var modIdStruct = new ModI(id);
                fake.WithMod(name, modIdStruct, $"TestData/{name}/config.xml", sortIndex);
            }
            
            return fake;
        }
        
        /// <summary>
        /// 创建返回特定配置的FakeConfigClassHelper
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="config">要返回的配置对象</param>
        public FakeConfigClassHelper CreateHelperReturning(TblS table, IXConfig config)
        {
            return new FakeConfigClassHelper
            {
                TblSToReturn = table,
                ConfigToReturn = config
            };
        }
        
        /// <summary>
        /// 创建InMemoryConfigData（内存版ConfigData，测试友好）
        /// </summary>
        public InMemoryConfigData CreateInMemoryConfigData()
        {
            return new InMemoryConfigData();
        }
        
        /// <summary>
        /// 释放所有创建的Mock对象
        /// </summary>
        public void Dispose()
        {
            foreach (var disposable in _disposables)
            {
                disposable?.Dispose();
            }
            _disposables.Clear();
        }
        
        /// <summary>
        /// 注册需要自动释放的对象
        /// </summary>
        protected void RegisterDisposable(IDisposable disposable)
        {
            if (disposable != null)
                _disposables.Add(disposable);
        }
    }
}
