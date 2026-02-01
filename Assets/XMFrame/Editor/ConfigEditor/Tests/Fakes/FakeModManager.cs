using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using XM.Contracts;

namespace XM.Editor.Tests.Fakes
{
    /// <summary>
    /// Fake Mod管理器，使用内存数据结构，无外部依赖
    /// 特点：
    /// 1. 真实行为，无副作用
    /// 2. 支持Fluent API配置
    /// 3. 无需复杂的Mock设置
    /// 
    /// 与Mock的区别：
    /// - Mock：模拟对象，需要设置预期调用和返回值
    /// - Fake：轻量级实现，有真实行为但简化了复杂逻辑
    /// </summary>
    public class FakeModManager : IModManager
    {
        // 内存存储
        private readonly List<SortedModConfig> _configs = new List<SortedModConfig>();
        private readonly List<ModRuntime> _runtimes = new List<ModRuntime>();
        private readonly Dictionary<string, ModI> _modNameToId = new Dictionary<string, ModI>();
        private readonly Dictionary<ModI, List<string>> _modIdToXmlPaths = new Dictionary<ModI, List<string>>();
        private readonly Dictionary<string, int> _modNameToSortIndex = new Dictionary<string, int>();
        
        /// <summary>
        /// 添加一个Mod（Fluent API）
        /// </summary>
        /// <param name="name">Mod名称</param>
        /// <param name="id">Mod ID</param>
        /// <param name="xmlPath">XML文件路径</param>
        /// <param name="sortIndex">排序索引（优先级）</param>
        public FakeModManager WithMod(string name, ModI id, string xmlPath, int sortIndex = 0)
        {
            _modNameToId[name] = id;
            
            if (!_modIdToXmlPaths.ContainsKey(id))
                _modIdToXmlPaths[id] = new List<string>();
            _modIdToXmlPaths[id].Add(xmlPath);
            
            _modNameToSortIndex[name] = sortIndex;
            
            // 如果不存在对应的ModConfig，添加一个
            if (!_configs.Exists(c => c.ModConfig.ModName == name))
            {
                var modConfig = new ModConfig(
                    modName: name,
                    version: "1.0.0",
                    author: "Test",
                    description: "Test Mod",
                    dllPath: "",
                    packageName: name,
                    configFiles: new List<string> { xmlPath },
                    assetName: name
                );
                
                _configs.Add(new SortedModConfig(modConfig, isEnabled: true));
            }
            
            return this;
        }
        
        /// <summary>
        /// 添加一个SortedModConfig（Fluent API）
        /// </summary>
        public FakeModManager WithConfig(SortedModConfig config)
        {
            if (!_configs.Contains(config))
                _configs.Add(config);
            
            return this;
        }
        
        // 实现IModManager接口
        
        public IEnumerable<SortedModConfig> GetSortedModConfigs()
        {
            return _configs.OrderBy(c => _modNameToSortIndex.GetValueOrDefault(c.ModConfig.ModName, 0));
        }
        
        public IEnumerable<ModRuntime> GetModRuntime()
        {
            return _runtimes;
        }
        
        public IEnumerable<string> GetModXmlFilePathByModId(ModI modId)
        {
            return _modIdToXmlPaths.TryGetValue(modId, out var paths) ? paths : new List<string>();
        }
        
        public ModI GetModId(string modName)
        {
            return _modNameToId.TryGetValue(modName, out var id) ? id : default;
        }
        
        public int GetModSortIndex(string modName)
        {
            return _modNameToSortIndex.TryGetValue(modName, out var index) ? index : 0;
        }
        
        // 实现IManager接口（来自IManager<IModManager>）
        public UniTask OnCreate() => UniTask.CompletedTask;
        public UniTask OnInit() => UniTask.CompletedTask;
        public UniTask OnDestroy() => UniTask.CompletedTask;
        public UniTask OnEntryMainMenu() => UniTask.CompletedTask;
        public UniTask OnEntryWorld() => UniTask.CompletedTask;
        public UniTask OnStopWorld() => UniTask.CompletedTask;
        public UniTask OnResumeWorld() => UniTask.CompletedTask;
        public UniTask OnExitWorld() => UniTask.CompletedTask;
        public UniTask OnQuitGame() => UniTask.CompletedTask;
    }
}
