using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using Cysharp.Threading.Tasks;
using UnityEngine;
using XM;
using XM.Contracts;
using Unity.Collections;
using XM.Contracts.Config;
using XM.Utils;

namespace XM
{
    [ManagerDependency(typeof(IModManager))]
    public class ConfigDataCenter : ManagerBase<IConfigDataCenter>, IConfigDataCenter
    {
        static ConfigDataCenter()
        {
            // 默认将配置解析告警输出到 Unity 控制台
            if (ConfigClassHelper.OnParseWarning == null)
                ConfigClassHelper.OnParseWarning = msg => Debug.LogWarning(msg);
        }

        // ClassHelper 实例缓存：TblS、HelperType -> IConfigClassHelper 实例
        private readonly MultiKeyDictionary<TblS, Type, ConfigClassHelper> _classHelperCache = new();

        private readonly BidirectionalDictionary<TblS, TblI> _typeLookUp = new();

        /// <summary>持有 ConfigData 以便 IConfigDataWriter 通过 ref 写入</summary>
        private sealed class ConfigDataHolder
        {
            public ConfigData Data;
        }

        private readonly ConfigDataHolder _configHolder = new ConfigDataHolder();

        /// <summary>按表保存已注册的 config，供 InitUnManagedData 按数量申请与填充</summary>
        private readonly Dictionary<TblI, List<IXConfig>> _configsByTable = new();

        /// <summary>已注册的配置：(TblI, ModS, configName) -> IXConfig，用于检查重复和后续分配 CfgI</summary>
        private readonly Dictionary<(TblI table, ModS mod, string configName), IXConfig> _registeredConfigs =
            new();

        /// <summary>CfgS (tableHandle, mod, configName) -> CfgI，用于 overwrite 复用与 FillToUnmanaged 外键解析（在 InitUnManagedData 时填充）</summary>
        private readonly Dictionary<(TblI table, ModS mod, string configName), CfgI>
            _configKeyToCfgI = new();

        /// <summary>(TblI, ModI) 自增 Id，用于分配 CfgI</summary>
        private readonly Dictionary<(TblI table, ModI mod), short> _nextCfgIByTableMod = new();

        public override UniTask OnCreate()
        {
            _configHolder.Data = new ConfigData();
            _configHolder.Data.Create(Allocator.Persistent, 4 * 1024 * 1024);
            return UniTask.CompletedTask;
        }


        private void RegisterModHelper()
        {
            var enabledMods = IModManager.I.GetEnabledModConfigs();

            foreach (var modConfig in enabledMods)
            {
                // 从 DllPath 加载程序集
                if (string.IsNullOrEmpty(modConfig.DllPath))
                    continue;

                Assembly assembly;
                try
                {
                    assembly = Assembly.LoadFrom(modConfig.DllPath);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"加载模块程序集失败: {modConfig.ModName}, 路径: {modConfig.DllPath}, 错误: {ex.Message}");
                    continue;
                }

                // 查找所有 XConfig 类型
                var configTypes = assembly.GetTypes()
                    .Where(t => !t.IsAbstract && IsXConfigType(t))
                    .ToList();

                foreach (var configType in configTypes)
                {
                    try
                    {
                        // 获取 TblS（从 CfgS<T>.TableName 静态属性）
                        var tableDefine = GetTblSFromType(configType, modConfig.ModName);

                        // 创建 ClassHelper 实例
                        var helperTypeName = configType.FullName + "ClassHelper";
                        var helperType = assembly.GetType(helperTypeName);

                        if (helperType == null)
                        {
                            Debug.LogWarning($"找不到 ClassHelper: {helperTypeName}");
                            continue;
                        }

                        var helper = (ConfigClassHelper)Activator.CreateInstance(helperType, this);

                        // 添加到缓存（双键）
                        _classHelperCache.AddOrUpdate(helper, tableDefine, configType);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"注册 ClassHelper 失败: {configType.Name}, 错误: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// 判断是否是 XConfig 类型
        /// </summary>
        private bool IsXConfigType(Type type)
        {
            var baseType = type.BaseType;
            while (baseType != null)
            {
                if (baseType.IsGenericType && baseType.GetGenericTypeDefinition().Name.Contains("XConfig"))
                {
                    return true;
                }

                baseType = baseType.BaseType;
            }

            return false;
        }

        /// <summary>
        /// 从类型获取 TblS TODO Helper获取
        /// </summary>
        private TblS GetTblSFromType(Type configType, string modName)
        {
            // 从 XConfig<T, TUnmanaged> 泛型参数获取 TUnmanaged
            var baseType = configType.BaseType;
            while (baseType != null && baseType.IsGenericType)
            {
                var genericArgs = baseType.GetGenericArguments();
                if (genericArgs.Length >= 2)
                {
                    var unmanagedType = genericArgs[1];
                    // 获取 CfgS<TUnmanaged>.TableName
                    var configKeyType = typeof(CfgS<>).MakeGenericType(unmanagedType);
                    var tableNameProp =
                        configKeyType.GetProperty("TableName", BindingFlags.Public | BindingFlags.Static);
                    if (tableNameProp != null)
                    {
                        var tableName = tableNameProp.GetValue(null) as string;
                        if (!string.IsNullOrEmpty(tableName))
                        {
                            return new TblS(new ModS(modName), tableName);
                        }
                    }
                }

                baseType = baseType.BaseType;
            }

            // 如果无法获取，使用配置类型名作为表名
            return new TblS(new ModS(modName), configType.Name);
        }

        public override UniTask OnInit()
        {
            RegisterModConfig();
            InitUnManagedData();
            SolveConfigReference();
            return UniTask.CompletedTask;
        }

        private void SolveConfigReference()
        {
            // 先预注册
        }

        private void RegisterModConfig()
        {
            RegisterModHelper();
            RegisterDynamicConfigType();
            ReadConfigFromXml();
            InitUnManagedData();
        }

        /// <summary>
        /// 待处理配置项
        /// </summary>
        private class PendingConfigItem
        {
            public ConfigClassHelper Helper;
            public ModS ModS;
            public string ConfigName;
            public XmlElement Node;
            public OverrideMode Mode;
        }

        private void ReadConfigFromXml()
        {
            // 待处理列表
            var pendingAdds = new List<PendingConfigItem>();
            var pendingModifies = new List<PendingConfigItem>();
            var pendingDeletes = new List<PendingConfigItem>();

            var enableMods = IModManager.I.GetEnabledModConfigs();
            foreach (var modConfig in enableMods)
            {
                var modName = modConfig.ModName;
                var modId = IModManager.I.GetModId(modConfig.ModName);
                foreach (var file in IModManager.I.GetModXmlFilePathByModId(modId))
                {
                    try
                    {
                        ProcessXmlFile(file, modName, modId, pendingAdds, pendingModifies, pendingDeletes);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"处理 XML 文件失败: {file}, 错误: {ex.Message}\n{ex.StackTrace}");
                    }
                }
            }

            // 应用待处理列表
            ApplyPendingConfigs(pendingAdds, pendingModifies, pendingDeletes);
        }

        /// <summary>
        /// 处理单个 XML 文件
        /// </summary>
        private void ProcessXmlFile(
            string xmlFilePath,
            string modName,
            ModI modId,
            List<PendingConfigItem> pendingAdds,
            List<PendingConfigItem> pendingModifies,
            List<PendingConfigItem> pendingDeletes)
        {
            // 加载 XML 文档
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlFilePath);

            var root = xmlDoc.DocumentElement;
            if (root == null)
            {
                Debug.LogWarning($"XML 文件根节点为空: {xmlFilePath}");
                return;
            }

            // 获取 Root 下所有 ConfigItem
            var configItems = root.SelectNodes("ConfigItem");
            if (configItems == null || configItems.Count == 0)
            {
                // 没有 ConfigItem，可能不是配置文件或格式不同
                return;
            }

            foreach (XmlElement configItem in configItems)
            {
                try
                {
                    ProcessConfigItem(configItem, modName, modId, pendingAdds, pendingModifies, pendingDeletes);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"处理 ConfigItem 失败: {xmlFilePath}, 错误: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 处理单个 ConfigItem
        /// </summary>
        private void ProcessConfigItem(
            XmlElement configItem,
            string modName,
            ModI modId,
            List<PendingConfigItem> pendingAdds,
            List<PendingConfigItem> pendingModifies,
            List<PendingConfigItem> pendingDeletes)
        {
            // 读取 cls、id、override 属性
            var cls = configItem.GetAttribute("cls");
            var id = configItem.GetAttribute("id");
            var overrideAttr = configItem.GetAttribute("override");

            if (string.IsNullOrEmpty(cls))
            {
                Debug.LogWarning("ConfigItem 缺少 cls 属性，跳过");
                return;
            }

            if (string.IsNullOrEmpty(id))
            {
                Debug.LogWarning("ConfigItem 缺少 id 属性，跳过");
                return;
            }

            // 解析 id 为 modName 和 configName（格式：modName::configName）
            var idParts = id.Split(new[] { "::" }, StringSplitOptions.None);
            if (idParts.Length != 2)
            {
                Debug.LogError($"id 格式错误（应为 modName::configName）: {id}");
                return;
            }

            var idModName = idParts[0];
            var configName = idParts[1];

            // 解析 override 属性
            var overrideMode = ParseOverrideMode(overrideAttr);

            // cls → Type
            var configType = ResolveTypeFromCls(cls);
            if (configType == null)
            {
                Debug.LogError($"无法解析 cls 类型: {cls}");
                return;
            }

            // Type + modName → TblS
            var tableDefine = GetTblSFromType(configType, idModName);

            // TblS → Helper
            var helper = GetClassHelperByTable(tableDefine);
            if (helper == null)
            {
                Debug.LogError($"找不到 TblS 对应的 ClassHelper: {tableDefine}");
                return;
            }

            var modKey = new ModS(idModName);

            // 根据 override 模式分支处理
            if (overrideMode == OverrideMode.None)
            {
                // 新增配置：检查是否存在（递归检查父类）
                if (helper.TryExistsInHierarchy(modKey, configName, out var key))
                {
                    Debug.LogError($"配置已存在（或在父类中存在），不能重复添加: {id}");
                    return;
                }

                // 获取 TblI
                if (!_typeLookUp.TryGetValueByKey(tableDefine, out var tableHandle))
                {
                    Debug.LogError($"找不到 TblS 对应的 TblI: {tableDefine}");
                    return;
                }

                // TODO: 从 XML 节点反序列化出 IXConfig 实例
                var config = helper.DeserializeConfigFromXml(configItem,modKey, configName);

                // TODO: 继承类时设置 BaseClass+Id 的 CfgS（由 Helper 或生成代码填充）
                // 例如：config.Id_Base = new CfgS<BaseUnmanaged>(modKey, baseConfigName);

                // 注册配置（不分配 CfgI，CfgI 在 InitUnManagedData 时分配）
                // RegisterConfigInternal(config, tableHandle, modKey, configName);

                Debug.Log($"[TODO] 新增配置: {id}，注册到 _registeredConfigs 和 _configsByTable");
                throw new NotImplementedException($"从 XML 反序列化配置、设置 BaseClass 的 CfgS、注册配置 等逻辑待实现");
            }
            else
            {
                // 加入待处理列表
                var pendingItem = new PendingConfigItem
                {
                    Helper = helper,
                    ModS = modKey,
                    ConfigName = configName,
                    Node = configItem,
                    Mode = overrideMode
                };

                switch (overrideMode)
                {
                    case OverrideMode.Add:
                        pendingAdds.Add(pendingItem);
                        break;
                    case OverrideMode.Modify:
                        pendingModifies.Add(pendingItem);
                        break;
                    case OverrideMode.Delete:
                        pendingDeletes.Add(pendingItem);
                        break;
                }
            }
        }

        /// <summary>
        /// 解析 override 属性
        /// </summary>
        private OverrideMode ParseOverrideMode(string overrideAttr)
        {
            if (string.IsNullOrEmpty(overrideAttr))
                return OverrideMode.None;

            switch (overrideAttr.ToLowerInvariant())
            {
                case "add":
                    return OverrideMode.Add;
                case "delete":
                case "del":
                    return OverrideMode.Delete;
                case "modify":
                    return OverrideMode.Modify;
                default:
                    Debug.LogWarning($"未知的 override 模式: {overrideAttr}，将视为 None");
                    return OverrideMode.None;
            }
        }

        /// <summary>
        /// 从 cls 字符串解析 Type
        /// </summary>
        private Type ResolveTypeFromCls(string cls)
        {
            // 优先使用 Type.GetType
            var type = Type.GetType(cls);
            if (type != null)
                return type;

            // 在已加载的程序集中按 FullName 查找
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = assembly.GetType(cls);
                if (type != null)
                    return type;
            }

            return null;
        }

        /// <summary>
        /// 应用待处理列表
        /// </summary>
        private void ApplyPendingConfigs(
            List<PendingConfigItem> pendingAdds,
            List<PendingConfigItem> pendingModifies,
            List<PendingConfigItem> pendingDeletes)
        {
            // TODO: 实现应用增加
            if (pendingAdds.Count > 0)
            {
                Debug.Log($"[TODO] 应用 {pendingAdds.Count} 个 Add 操作");
                // throw new NotImplementedException("应用 Add 操作待实现");
            }

            // TODO: 实现应用修改
            if (pendingModifies.Count > 0)
            {
                Debug.Log($"[TODO] 应用 {pendingModifies.Count} 个 Modify 操作");
                // throw new NotImplementedException("应用 Modify 操作待实现");
            }

            // TODO: 实现应用删除
            if (pendingDeletes.Count > 0)
            {
                Debug.Log($"[TODO] 应用 {pendingDeletes.Count} 个 Delete 操作");
                // throw new NotImplementedException("应用 Delete 操作待实现");
            }
        }

        private void InitUnManagedData()
        {
            // TODO: 遍历 _registeredConfigs，为每个配置分配 CfgI
            // TODO: 调用 AllocateCfgIForConfig 分配 CfgI 并写回 config.Data
            // TODO: 将配置填充到 Unmanaged 数据（FillToUnmanaged）
        }


        public void RegisterDynamicConfigType()
        {
            // 为每个收集到的配置类型创建 TblI 并注册
            short tableIdCounter = 1;

            // 从 _classHelperCache 遍历所有已注册的 ClassHelper
            foreach (var (helper, tableDefine, configType) in _classHelperCache.Pairs)
            {
                try
                {
                    // 从 XConfig 类型获取 Unmanaged 类型
                    var unmanagedType = GetUnmanagedTypeFromConfig(configType);
                    if (unmanagedType == null)
                    {
                        Debug.LogWarning($"无法获取配置类型 {configType.Name} 的 Unmanaged 类型");
                        continue;
                    }

                    // 从 TblS 获取 ModName
                    var modName = tableDefine.DefinedInMod.Name;
                    // 获取 ModI
                    var modHandle = IModManager.I.GetModId(modName);
                    // 创建 TblI
                    var tableHandle = new TblI(tableIdCounter++, modHandle);
                    // 注册到 _typeLookUp
                    _typeLookUp.AddOrUpdate(tableDefine, tableHandle);
                    helper.SetTblIDefinedInMod(modHandle);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"注册配置类型失败: {tableDefine}, 错误: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 从 XConfig 类型获取 Unmanaged 类型
        /// </summary>
        private Type GetUnmanagedTypeFromConfig(Type configType)
        {
            var baseType = configType.BaseType;
            while (baseType != null && baseType.IsGenericType)
            {
                var genericArgs = baseType.GetGenericArguments();
                if (genericArgs.Length >= 2)
                {
                    return genericArgs[1]; // TUnmanaged
                }

                baseType = baseType.BaseType;
            }

            return null;
        }


        public bool TryGetConfigBySingleIndex<TData, TIndex>(in TIndex index, out TData data)
            where TData : unmanaged, IConfigUnManaged<TData>
            where TIndex : IConfigIndexGroup<TData>
        {
            // 找到Root下所有ConfigItem 
            throw new NotImplementedException();
        }

        public bool TryGetConfig<T>(out T data) where T : unmanaged, IConfigUnManaged<T>
        {
            throw new NotImplementedException();
        }


        public void RegisterConfigTable<T>() where T : IXConfig
        {
        }

        public ITypeConverter<TSource, TTarget> GetConverter<TSource, TTarget>(string domain = "")
        {
            return TypeConverterRegistry.GetConverter<TSource, TTarget>(domain);
        }

        public bool HasConverter<TSource, TTarget>(string domain = "")
        {
            return TypeConverterRegistry.HasConverter<TSource, TTarget>(domain);
        }

        public bool TryGetCfgI(TblS tableDefine, ModS mod, string configName, out CfgI cfgI)
        {
            cfgI = default;
            if (!_typeLookUp.TryGetValueByKey(tableDefine, out var tableI))
                return false;
            var key = (tableI, mod, configName);
            return _configKeyToCfgI.TryGetValue(key, out cfgI);
        }

        public bool TryExistsConfig(TblI table, ModS mod, string configName)
        {
            var key = (table, mod, configName);
            return _registeredConfigs.ContainsKey(key);
        }

        /// <summary>
        /// 注册配置到内部字典（XML 读取阶段使用，不分配 CfgI）
        /// </summary>
        private void RegisterConfigInternal(IXConfig config, TblI tableHandle, ModS modKey, string configName)
        {
            var key = (tableHandle, modKey, configName);

            // 注册到 _registeredConfigs
            _registeredConfigs[key] = config;

            // 添加到 _configsByTable
            if (!_configsByTable.TryGetValue(tableHandle, out var configList))
            {
                configList = new List<IXConfig>();
                _configsByTable[tableHandle] = configList;
            }

            configList.Add(config);
        }

        /// <summary>
        /// 为已注册的配置分配 CfgI（在 InitUnManagedData 时使用）
        /// </summary>
        private CfgI AllocateCfgIForConfig(TblI table, ModI mod, ModS modKey, string configName)
        {
            // 获取或初始化自增 Id
            var key = (table, mod);
            if (!_nextCfgIByTableMod.TryGetValue(key, out var nextId))
            {
                nextId = 1;
            }

            var cfgId = new CfgI(nextId, mod, table);
            _nextCfgIByTableMod[key] = (short)(nextId + 1);

            // 写入 _configKeyToCfgI
            var lookupKey = (table, modKey, configName);
            _configKeyToCfgI[lookupKey] = cfgId;

            return cfgId;
        }

        /// <summary>
        /// 从 ModI 获取 ModS
        /// </summary>
        private ModS GetModSFromHandle(ModI modHandle)
        {
            // TODO: 需要从 IModManager 或内部映射获取 ModS
            // 临时实现：遍历 _typeLookUp 查找匹配的 ModI
            foreach (var (tableDefine, tableHandle) in _typeLookUp.Pairs)
            {
                if (tableHandle.Mod.Equals(modHandle))
                {
                    return tableDefine.DefinedInMod;
                }
            }

            throw new InvalidOperationException($"无法找到 ModI {modHandle.ModId} 对应的 ModS");
        }

        public void RegisterData<T>(T data) where T : IXConfig
        {
            throw new NotImplementedException();
        }

        public void UpdateData<T>(T data) where T : IXConfig
        {
        }

        public ConfigClassHelper GetClassHelper<T>() where T : IXConfig, new()
        {
            var configType = typeof(T);
            return (ConfigClassHelper)GetClassHelper(configType);
        }

        public ConfigClassHelper GetClassHelper(Type configType)
        {
            if (configType == null)
            {
                throw new ArgumentNullException(nameof(configType));
            }

            // 先尝试从缓存中通过 Type 获取
            var cached = _classHelperCache.GetByKey2(configType);
            if (cached != null)
            {
                return cached;
            }

            // 如果缓存中没有，创建新实例
            // 构造 ClassHelper 类型名称（例如：TestConfig -> TestConfigClassHelper）
            var helperTypeName = configType.FullName + "ClassHelper";

            // 尝试在同一程序集中查找 ClassHelper 类型
            var helperType = configType.Assembly.GetType(helperTypeName);

            if (helperType == null)
            {
                // 如果在同一程序集中找不到，尝试在所有程序集中查找
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    helperType = assembly.GetType(helperTypeName);
                    if (helperType != null)
                    {
                        break;
                    }
                }
            }

            if (helperType == null)
            {
                throw new InvalidOperationException($"找不到配置类型 {configType.Name} 的 ClassHelper: {helperTypeName}");
            }

            // 创建 ClassHelper 实例（传入 this 作为 IConfigDataCenter）
            var helper = (ConfigClassHelper)Activator.CreateInstance(helperType, this);

            // 获取 TblS 并添加到缓存（双键）
            // 注意：这里使用默认的 ModS，因为没有模块上下文信息
            var tableDefine = GetTblSFromType(configType, "Default");
            _classHelperCache.AddOrUpdate(helper, tableDefine, configType);

            return helper;
        }

        public ConfigClassHelper GetClassHelperByTable(TblS tableDefine)
        {
            return _classHelperCache.GetByKey1(tableDefine);
        }

        /// <summary>
        /// 从 TblS 获取 TblI
        /// </summary>
        public TblI GetTblI(TblS tableDefine)
        {
            if (_typeLookUp.TryGetValueByKey(tableDefine, out var tableHandle))
            {
                return tableHandle;
            }

            return default;
        }
    }
}