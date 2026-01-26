using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Cysharp.Threading.Tasks;
using XMFrame.Interfaces;
using XMFrame.Utils;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace XMFrame.Implementation
{
    [AutoCreate]
    [ManagerDependency(typeof(ISaveManager))]
    public class XModManager : ManagerBase<IModManager>, IModManager
    {
        public const string ModsFolder = "Mods";
        public const string ModDefineXmlName = "ModDefine.xml";

        public MultiKeyDictionary<string, int, ModConfig> ModConfigDict =
            new MultiKeyDictionary<string, int, ModConfig>();

        public BidirectionalDictionary<int, ModKey> ModStaticToRuntimeDict = new BidirectionalDictionary<int, ModKey>();

        private Dictionary<ModKey, ModRuntime> _modRuntimeDict = new Dictionary<ModKey, ModRuntime>();
        private Dictionary<string, ModKey> _modNameToKeyDict = new Dictionary<string, ModKey>();
        private int _nextStaticModId = 1;

        // 排序后的Mod配置列表
        private List<SortedModConfig> _sortedModConfigs = new List<SortedModConfig>();
        private bool _isInitialized = false;

        /// <summary>
        /// 读取所有Mod配置文件
        /// </summary>
        public void ReadAllModConfigs()
        {
            // 获取Mods文件夹的完整路径
            var modsFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ModsFolder);

            // 检查Mods文件夹是否存在
            if (!Directory.Exists(modsFolderPath))
            {
                XLog.WarningFormat("Mods文件夹不存在: {0}", modsFolderPath);
                return;
            }

            // 遍历Mods文件夹下的所有子文件夹
            var subFolders = Directory.GetDirectories(modsFolderPath);

            foreach (var subFolder in subFolders)
            {
                // 查找当前子文件夹下所有名为ModDefine.xml的文件
                var xmlFiles = Directory.GetFiles(subFolder, ModDefineXmlName, SearchOption.AllDirectories);

                foreach (var xmlFile in xmlFiles)
                {
                    try
                    {
                        XLog.InfoFormat("找到Mod定义文件: {0}", xmlFile);
                        var modConfig = ReadModDefineXml(xmlFile, subFolder);
                        if (modConfig != null)
                        {
                            // 使用ModName作为第一个键，版本号哈希作为第二个键
                            int versionHash = modConfig.Version?.GetHashCode() ?? 0;
                            ModConfigDict.AddOrUpdate(modConfig, modConfig.ModName, versionHash);

                            // 建立ModName到ModKey的映射
                            var modKey = new ModKey(modConfig.ModName);
                            _modNameToKeyDict[modConfig.ModName] = modKey;

                            XLog.InfoFormat("成功加载Mod配置: {0} v{1}", modConfig.ModName, modConfig.Version);
                        }
                    }
                    catch (Exception ex)
                    {
                        XLog.ErrorFormat("读取Mod配置文件失败: {0}, 错误: {1}", xmlFile, ex.Message);
                    }
                }
            }

            // 读取完所有配置后，初始化SortedModConfig
            if (!_isInitialized)
            {
                InitializeSortedModConfigs();
            }
        }

        /// <summary>
        /// 读取XML子元素的值
        /// </summary>
        private string GetXmlElementValue(XmlElement parent, string elementName)
        {
            var element = parent.SelectSingleNode(elementName);
            return element?.InnerText?.Trim() ?? "";
        }

        /// <summary>
        /// 读取Mod定义XML文件
        /// </summary>
        private ModConfig ReadModDefineXml(string xmlFilePath, string modFolder)
        {
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlFilePath);

                var root = xmlDoc.DocumentElement;
                if (root == null)
                {
                    XLog.ErrorFormat("XML文件根节点为空: {0}", xmlFilePath);
                    return null;
                }

                // 从子元素读取值
                var modName = GetXmlElementValue(root, "Name");
                var modVersion = GetXmlElementValue(root, "Version");
                var modAuthor = GetXmlElementValue(root, "Author");
                var modDescription = GetXmlElementValue(root, "Description");
                var modDllPath = GetXmlElementValue(root, "DllPath");
                var modIconPath = GetXmlElementValue(root, "IconPath");
                var modHomePageLink = GetXmlElementValue(root, "HomePageLink");
                var modImagePath = GetXmlElementValue(root, "ImagePath");

                if (string.IsNullOrEmpty(modName))
                {
                    XLog.ErrorFormat("Mod名称不能为空: {0}", xmlFilePath);
                    return null;
                }

                // 如果DllPath是相对路径，则相对于modFolder解析
                if (!string.IsNullOrEmpty(modDllPath) && !Path.IsPathRooted(modDllPath))
                {
                    modDllPath = Path.Combine(modFolder, modDllPath);
                }

                // 如果IconPath是相对路径，则相对于modFolder解析
                if (!string.IsNullOrEmpty(modIconPath) && !Path.IsPathRooted(modIconPath))
                {
                    modIconPath = Path.Combine(modFolder, modIconPath);
                }

                // 如果ImagePath是相对路径，则相对于modFolder解析
                if (!string.IsNullOrEmpty(modImagePath) && !Path.IsPathRooted(modImagePath))
                {
                    modImagePath = Path.Combine(modFolder, modImagePath);
                }

                var modConfig = new ModConfig(modName,
                    modVersion ?? "1.0.0",
                    modAuthor ?? "",
                    modDescription ?? "",
                    modDllPath ?? "",
                    modIconPath ?? "",
                    modHomePageLink ?? "",
                    modImagePath ?? "");
                return modConfig;
            }
            catch (Exception ex)
            {
                XLog.ErrorFormat("解析Mod定义XML失败: {0}, 错误: {1}", xmlFilePath, ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 启用指定的Mod
        /// </summary>
        public bool EnableMod(string modName)
        {
            if (string.IsNullOrEmpty(modName))
            {
                XLog.Warning("Mod名称不能为空");
                return false;
            }

            // 检查Mod是否已启用
            if (_modNameToKeyDict.TryGetValue(modName, out var modKey))
            {
                if (_modRuntimeDict.ContainsKey(modKey))
                {
                    XLog.WarningFormat("Mod已启用: {0}", modName);
                    return true;
                }
            }

            // 查找Mod配置
            if (!ModConfigDict.TryGetValueByKey1(modName, out var modConfig))
            {
                XLog.ErrorFormat("未找到Mod配置: {0}", modName);
                return false;
            }

            // 加载DLL
            if (string.IsNullOrEmpty(modConfig.DllPath))
            {
                XLog.WarningFormat("Mod未指定DLL路径: {0}", modName);
                return false;
            }

            if (!File.Exists(modConfig.DllPath))
            {
                XLog.ErrorFormat("Mod DLL文件不存在: {0}", modConfig.DllPath);
                return false;
            }

            try
            {
                // 加载程序集
                var assembly = Assembly.LoadFrom(modConfig.DllPath);

                // 查找并实例化ModBase
                ModBase modEntry = null;
                try
                {
                    // 查找所有继承自ModBase的类型
                    var modBaseTypes = assembly.GetTypes()
                        .Where(t => typeof(ModBase).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface)
                        .ToList();

                    // 如果没有找到ModBase实现类也允许正常继续，不视为错误，只做调试提示
                    if (modBaseTypes.Count == 0)
                    {
                        XLog.DebugFormat("Mod程序集中未找到ModBase实现类（可正常继续）: {0}", modName);
                    }
                    else if (modBaseTypes.Count > 1)
                    {
                        XLog.WarningFormat("Mod程序集中找到多个ModBase实现类，将使用第一个: {0}", modName);
                    }

                    if (modBaseTypes.Count > 0)
                    {
                        var modBaseType = modBaseTypes[0];
                        modEntry = Activator.CreateInstance(modBaseType) as ModBase;

                        if (modEntry != null)
                        {
                            XLog.InfoFormat("成功创建Mod入口点: {0}", modBaseType.Name);
                        }
                        else
                        {
                            XLog.ErrorFormat("无法实例化ModBase类型: {0}", modBaseType.Name);
                        }
                    }
                }
                catch (Exception ex)
                {
                    XLog.ErrorFormat("创建Mod入口点失败: {0}, 错误: {1}", modName, ex.Message);
                }

                // 创建ModKey
                modKey = new ModKey(modName);

                // 创建ModRuntime
                var modRuntime = new ModRuntime(modKey, modConfig, assembly, modEntry);

                // 存储到字典
                _modRuntimeDict[modKey] = modRuntime;
                _modNameToKeyDict[modName] = modKey;

                // 分配静态ID并建立映射
                int staticId = _nextStaticModId++;
                ModStaticToRuntimeDict.AddOrUpdate(staticId, modKey);

                XLog.InfoFormat("成功启用Mod: {0} (静态ID: {1})", modName, staticId);
                return true;
            }
            catch (Exception ex)
            {
                XLog.ErrorFormat("加载Mod DLL失败: {0}, 错误: {1}", modConfig.DllPath, ex.Message);
                return false;
            }
        }

        private void SolveConfigDefine()
        {
            foreach (var modRuntime in _modRuntimeDict.Values)
            {
                if (modRuntime.Assembly == null) continue;
                var assembly = modRuntime.Assembly;
                var configDefineTypes = assembly.GetTypes()
                    .Where(t => typeof(XConfig).IsAssignableFrom(t) && !t.IsAbstract && !t.IsInterface);
                modRuntime.AddConfigDefine(configDefineTypes);
            }

            foreach (var modRuntime in _modRuntimeDict.Values)
            {
                foreach (var configDefineType in modRuntime.ConfigDefineTypes)
                {
                }
            }
        }


        public void PreInitConfig()
        {
            SolveConfigDefine();
        }

        /// <summary>
        /// 禁用指定的Mod（运行时不允许禁用，修改配置会触发游戏重启）
        /// </summary>
        public bool DisableMod(string modName)
        {
            XLog.WarningFormat("运行时不允许禁用Mod: {0}。如需修改配置，请修改SortedModConfig，游戏将自动重启。", modName);
            return false;
        }

        /// <summary>
        /// 获取排序后的Mod配置列表
        /// </summary>
        public List<SortedModConfig> GetSortedModConfigs()
        {
            return new List<SortedModConfig>(_sortedModConfigs);
        }

        /// <summary>
        /// 重置Mod配置并重启游戏（运行时修改会触发游戏重启）
        /// </summary>
        public void ResetModConfigsRestartGame(List<ModConfig> modConfigs)
        {
            if (modConfigs == null)
            {
                XLog.Error("ModConfig列表不能为null");
                return;
            }

            // 将 ModConfig 列表转换为 SortedModConfig 列表
            var sortedConfigs = new List<SortedModConfig>();
            foreach (var modConfig in modConfigs)
            {
                if (modConfig == null)
                {
                    XLog.Warning("发现null的ModConfig，跳过");
                    continue;
                }

                // 查找现有配置中是否已启用
                bool isEnabled = false;
                if (_isInitialized)
                {
                    var existingConfig = _sortedModConfigs.FirstOrDefault(sc => 
                        sc.ModConfig.ModName == modConfig.ModName && 
                        sc.ModConfig.Version == modConfig.Version);
                    isEnabled = existingConfig?.IsEnabled ?? false;
                }

                var sortedConfig = new SortedModConfig(modConfig, isEnabled);
                sortedConfigs.Add(sortedConfig);
            }

            // 检测运行时修改
            if (_isInitialized && HasSortedConfigChanged(sortedConfigs))
            {
                // 持久化模组列表
                try
                {
                    PersistSortedModConfigs(sortedConfigs);
                }
                catch (Exception ex)
                {
                    XLog.ErrorFormat("持久化SortedModConfig失败: {0}", ex.Message);
                }

                XLog.Warning("检测到运行时修改ModConfig，已持久化并将重启游戏...");
                RestartGame();
                return;
            }

            _sortedModConfigs = sortedConfigs;

            // 根据排序后的配置启用Mod
            EnableModsFromSortedConfig();
        }

        /// <summary>
        /// 初始化排序后的Mod配置（从已读取的配置创建）
        /// </summary>
        public void InitializeSortedModConfigs()
        {
            if (_isInitialized)
            {
                XLog.Warning("SortedModConfig已经初始化");
                return;
            }

            _sortedModConfigs.Clear();

            // 从存档读取已保存的Mod配置
            List<SavedModInfo> savedModInfos = null;
            if ( ISaveManager.I != null)
            {
                try
                {
                    savedModInfos = ISaveManager.I.LoadModConfigs();
                    XLog.InfoFormat("从存档读取到 {0} 个Mod配置", savedModInfos?.Count ?? 0);
                }
                catch (Exception ex)
                {
                    XLog.WarningFormat("从存档读取Mod配置失败: {0}，将使用默认配置", ex.Message);
                }
            }

            // 从ModConfigDict创建SortedModConfig列表
            foreach (var kvp in ModConfigDict)
            {
                bool isEnabled = false;

                // 如果存档中有该Mod的配置，使用存档中的启用状态
                if (savedModInfos != null)
                {
                    var savedInfo = savedModInfos.FirstOrDefault(s => 
                        s.ModName == kvp.ModName && s.Version == kvp.Version);
                    if (savedInfo != null)
                    {
                        isEnabled = savedInfo.IsEnabled;
                    }
                }

                var sortedConfig = new SortedModConfig(kvp, isEnabled);
                _sortedModConfigs.Add(sortedConfig);
            }

            // 按Mod名称排序
            _sortedModConfigs.Sort((a, b) =>
                string.Compare(a.ModConfig.ModName, b.ModConfig.ModName, StringComparison.Ordinal));

            _isInitialized = true;
            XLog.InfoFormat("已初始化SortedModConfig，共 {0} 个Mod", _sortedModConfigs.Count);
        }

        /// <summary>
        /// 根据排序后的配置启用Mod
        /// </summary>
        private void EnableModsFromSortedConfig()
        {
            foreach (var sortedConfig in _sortedModConfigs)
            {
                if (sortedConfig.IsEnabled)
                {
                    EnableMod(sortedConfig.ModConfig.ModName);
                }
            }
            
        }

        /// <summary>
        /// 检测SortedModConfig是否发生变化
        /// </summary>
        private bool HasSortedConfigChanged(List<SortedModConfig> newConfigs)
        {
            if (newConfigs.Count != _sortedModConfigs.Count)
            {
                return true;
            }

            for (int i = 0; i < newConfigs.Count; i++)
            {
                if (!ConfigEquals(_sortedModConfigs[i], newConfigs[i]))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 比较两个SortedModConfig是否相等
        /// </summary>
        private bool ConfigEquals(SortedModConfig a, SortedModConfig b)
        {
            if (a == null && b == null) return true;
            if (a == null || b == null) return false;

            return a.IsEnabled == b.IsEnabled &&
                   a.ModConfig.ModName == b.ModConfig.ModName &&
                   a.ModConfig.Version == b.ModConfig.Version;
        }

        /// <summary>
        /// 持久化排序后的Mod配置列表到存档
        /// </summary>
        private void PersistSortedModConfigs(List<SortedModConfig> sortedConfigs)
        {
            try
            {
                // 转换为存档格式
                var savedModInfos = new List<SavedModInfo>();
                foreach (var sortedConfig in sortedConfigs)
                {
                    var savedInfo = new SavedModInfo(
                        sortedConfig.ModConfig.ModName,
                        sortedConfig.ModConfig.Version ?? "",
                        sortedConfig.IsEnabled);
                    savedModInfos.Add(savedInfo);
                }

                // 使用存档接口保存
                if (ISaveManager.I != null)
                {
                    ISaveManager.I.SaveModConfigs(savedModInfos);
                    XLog.InfoFormat("已通过存档系统保存 {0} 个Mod配置", savedModInfos.Count);
                }
                else
                {
                    XLog.Warning("SaveManager不可用，无法保存Mod配置");
                }
            }
            catch (Exception ex)
            {
                XLog.ErrorFormat("持久化SortedModConfig失败: {0}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 重启游戏
        /// </summary>
        private void RestartGame()
        {
            XLog.Warning("正在重启游戏...");

#if UNITY_EDITOR
            // 编辑器模式下停止播放
            UnityEditor.EditorApplication.isPlaying = false;
#else
        // 运行时退出游戏（实际应用中可能需要重新启动）
        Application.Quit();
#endif
        }

        /// <summary>
        /// 获取Mod运行时信息
        /// </summary>
        public ModRuntime GetModRuntime(string modName)
        {
            if (_modNameToKeyDict.TryGetValue(modName, out var modKey))
            {
                _modRuntimeDict.TryGetValue(modKey, out var modRuntime);
                return modRuntime;
            }

            return null;
        }

        /// <summary>
        /// 获取Mod运行时信息（通过ModKey）
        /// </summary>
        public ModRuntime GetModRuntime(ModKey modKey)
        {
            _modRuntimeDict.TryGetValue(modKey, out var modRuntime);
            return modRuntime;
        }

        /// <summary>
        /// 检查Mod是否已启用
        /// </summary>
        public bool IsModEnabled(string modName)
        {
            return _modNameToKeyDict.ContainsKey(modName) &&
                   _modNameToKeyDict.TryGetValue(modName, out var modKey) &&
                   _modRuntimeDict.ContainsKey(modKey);
        }

        /// <summary>
        /// 获取所有已启用的Mod名称列表
        /// </summary>
        public List<string> GetEnabledModNames()
        {
            return new List<string>(_modNameToKeyDict.Keys);
        }

        /// <summary>
        /// 通过Mod名称获取ModId
        /// </summary>
        public ModHandle GetModId(string modName)
        {
            if (string.IsNullOrEmpty(modName))
            {
                return default;
            }

            // 通过ModName获取ModKey
            if (!_modNameToKeyDict.TryGetValue(modName, out var modKey))
            {
                return default;
            }

            // 通过ModKey从双向字典获取静态ID
            if (!ModStaticToRuntimeDict.TryGetKeyByValue(modKey, out int staticId))
            {
                return default;
            }

            // 将int转换为ModHandle（short）
            return new ModHandle((short)staticId);
        }

        public IEnumerable<ModConfig> GetEnabledModConfigs()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 通过ModHandle获取Mod的XML文件路径列表
        /// </summary>
        public IEnumerable<string> GetModXmlFilePathByModId(ModHandle modId)
        {
            if (!modId.Valid)
            {
                return Enumerable.Empty<string>();
            }

            // 通过ModHandle的ModId获取ModKey
            int staticId = modId.ModId;
            if (!ModStaticToRuntimeDict.TryGetValueByKey(staticId, out var modKey))
            {
                return Enumerable.Empty<string>();
            }

            // 通过ModKey获取ModRuntime
            if (!_modRuntimeDict.TryGetValue(modKey, out var modRuntime))
            {
                return Enumerable.Empty<string>();
            }

            // 获取Mod配置
            var modConfig = modRuntime.Config;
            if (modConfig == null)
            {
                return Enumerable.Empty<string>();
            }

            // 获取Mod文件夹路径（从ModDefine.xml的路径推断）
            var modsFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ModsFolder);
            var modFolder = Path.Combine(modsFolderPath, modConfig.ModName);

            // 如果Mod文件夹不存在，返回空列表
            if (!Directory.Exists(modFolder))
            {
                return Enumerable.Empty<string>();
            }

            // 查找所有XML文件（递归搜索）
            var xmlFiles = Directory.GetFiles(modFolder, "*.xml", SearchOption.AllDirectories);
            return xmlFiles;
        }

        public override UniTask OnCreate()
        {
            return UniTask.CompletedTask;
        }

        public override UniTask OnInit()
        {
            return UniTask.CompletedTask;
        }
    }
}