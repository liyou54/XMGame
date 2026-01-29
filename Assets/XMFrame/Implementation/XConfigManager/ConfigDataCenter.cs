using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using Cysharp.Threading.Tasks;
using UnityEngine;
using XM;
using XM.Contracts;
using Unity.Collections;
using UnityEngine.Pool;
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

        /// <summary>待添加项：CfgS -> (TblS + XmlElement)，用于 Apply 时反序列化并注册</summary>
        private struct PendingAddItem
        {
            public TblS TableDefine;
            public XmlElement XmlElement;
        }

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


        /// <summary>从 XML 读取配置。待处理列表使用并发安全容器，便于后续多线程填充。</summary>
        private void ReadConfigFromXml()
        {
            // 并发安全版本：CfgS 为键以区分同一 Mod 下不同配置；Modify 存 XmlElement 便于 Apply 时 FillFromXml
            var pendingAdds = new ConcurrentDictionary<CfgS, PendingAddItem>();
            var pendingModifies = new ConcurrentDictionary<CfgS, XmlElement>();
            var pendingDeletes = new ConcurrentDictionary<CfgS, byte>(); // CfgS 集合（value 占位）

            var enableMods = IModManager.I.GetEnabledModConfigs();
            foreach (var modConfig in enableMods)
            {
                var modName = modConfig.ModName;
                var modId = IModManager.I.GetModId(modConfig.ModName);
                var files = ListPool<string>.Get();
                try
                {
                    files.AddRange(IModManager.I.GetModXmlFilePathByModId(modId));
                    ProcessXmlFile(files, modName, modId, pendingAdds, pendingModifies, pendingDeletes);
                }
                catch (XmlException xmlEx)
                {
                    XLog.ErrorFormat("处理 XML 文件失败: {0} 行 {1}:{2} - {3}",
                        xmlEx.SourceUri ?? "(unknown)", xmlEx.LineNumber, xmlEx.LinePosition, xmlEx.Message);
                }
                catch (Exception ex)
                {
                    XLog.ErrorFormat("处理 Mod XML 失败: {0}, 错误: {1}", modName, ex.Message);
                }
                finally
                {
                    ListPool<string>.Release(files);
                }
            }

            ApplyPendingConfigs(pendingAdds, pendingModifies, pendingDeletes);
        }

        /// <summary>
        /// 处理多个 XML 文件；使用并发安全容器，便于后续 UniTask 多线程后台加载。
        /// </summary>
        private void ProcessXmlFile(
            List<string> xmlFilePaths,
            string modName,
            ModI modId,
            ConcurrentDictionary<CfgS, PendingAddItem> pendingAdds,
            ConcurrentDictionary<CfgS, XmlElement> pendingModifies,
            ConcurrentDictionary<CfgS, byte> pendingDeletes)
        {
            foreach (var xmlFilePath in xmlFilePaths)
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlFilePath);

                var root = xmlDoc.DocumentElement;
                if (root == null)
                {
                    Debug.LogWarning($"XML 文件根节点为空: {xmlFilePath}");
                    continue;
                }

                var configItems = root.SelectNodes("ConfigItem");
                if (configItems == null || configItems.Count == 0)
                    continue;

                foreach (XmlElement configItem in configItems)
                {
                    try
                    {
                        ProcessConfigItem(configItem, xmlFilePath, modName, modId, pendingAdds, pendingModifies, pendingDeletes);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"处理 ConfigItem 失败: {xmlFilePath}, 错误: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// 处理单个 ConfigItem；将 OverrideMode 传入 Helper，不同 override 可有不同错误处理。调用 Helper 前设置 CurrentParseContext（文件、行、模式）。
        /// </summary>
        private void ProcessConfigItem(
            XmlElement configItem,
            string xmlFilePath,
            string modName,
            ModI modId,
            ConcurrentDictionary<CfgS, PendingAddItem> pendingAdds,
            ConcurrentDictionary<CfgS, XmlElement> pendingModifies,
            ConcurrentDictionary<CfgS, byte> pendingDeletes)
        {
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

            var idParts = id.Split(new[] { "::" }, StringSplitOptions.None);
            string idModName;
            string configName;
            if (idParts.Length == 1)
            {
                idModName = modName;
                configName = idParts[0];
            }
            else if (idParts.Length == 2)
            {
                idModName = idParts[0];
                configName = idParts[1];
            }
            else
            {
                Debug.LogError($"ConfigItem id 格式错误，应为 modName::configName 或 configName: {id}");
                return;
            }

            // 非当前模组且无 override 时不允许定义其他模组内容
            if (!string.Equals(idModName, modName, StringComparison.Ordinal) && string.IsNullOrEmpty(overrideAttr))
            {
                Debug.LogError($"非当前模组不允许定义其他模组配置: 当前 mod={modName}, id 指定 mod={idModName}, id={id}");
                return;
            }

            var overrideMode = ParseOverrideMode(overrideAttr);

            var configType = ResolveTypeFromCls(cls);
            if (configType == null)
            {
                Debug.LogError($"无法解析 cls 类型: {cls}");
                return;
            }

            var tableDefine = GetTblSFromType(configType, idModName);
            var helper = GetClassHelperByTable(tableDefine);
            if (helper == null)
            {
                Debug.LogError($"找不到 TblS 对应的 ClassHelper: {tableDefine}");
                return;
            }

            var modKey = new ModS(idModName);
            var cfgKey = new CfgS(modKey, tableDefine.TableName, configName);

            if (overrideMode == OverrideMode.None)
            {
                if (helper.TryExistsInHierarchy(modKey, configName, out _))
                {
                    Debug.LogError($"配置已存在（或在父类中存在），不能重复添加: {id}");
                    return;
                }

                if (!_typeLookUp.TryGetValueByKey(tableDefine, out var tableHandle))
                {
                    Debug.LogError($"找不到 TblS 对应的 TblI: {tableDefine}");
                    return;
                }

                var prev = ConfigClassHelper.CurrentParseContext;
                try
                {
                    ConfigClassHelper.CurrentParseContext = new ConfigParseContext { FilePath = xmlFilePath ?? "", Line = 0, Mode = overrideMode };
                    var config = helper.DeserializeConfigFromXml(configItem, modKey, configName, overrideMode);
                    if (config != null)
                        RegisterConfigInternal(config, tableHandle, modKey, configName);
                }
                finally
                {
                    ConfigClassHelper.CurrentParseContext = prev;
                }
            }
            else
            {
                switch (overrideMode)
                {
                    case OverrideMode.ReWrite:
                        pendingAdds[cfgKey] = new PendingAddItem { TableDefine = tableDefine, XmlElement = configItem };
                        break;
                    case OverrideMode.Modify:
                        pendingModifies[cfgKey] = configItem;
                        break;
                    case OverrideMode.Delete:
                        pendingDeletes[cfgKey] = 0;
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
                case "rewrite":
                case "add":
                    return OverrideMode.ReWrite;
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
        /// 应用并发安全待处理列表（ReWrite → 注册，Modify → FillFromXml，Delete → 移除）
        /// </summary>
        private void ApplyPendingConfigs(
            ConcurrentDictionary<CfgS, PendingAddItem> pendingAdds,
            ConcurrentDictionary<CfgS, XmlElement> pendingModifies,
            ConcurrentDictionary<CfgS, byte> pendingDeletes)
        {
            foreach (var kv in pendingAdds)
            {
                var cfgKey = kv.Key;
                var item = kv.Value;
                if (item.XmlElement == null) continue;
                var helper = GetClassHelperByTable(item.TableDefine);
                if (helper == null) { Debug.LogError($"应用 ReWrite 时找不到 Helper: {item.TableDefine}"); continue; }
                if (!_typeLookUp.TryGetValueByKey(item.TableDefine, out var tableHandle))
                { Debug.LogError($"应用 ReWrite 时找不到 TblI: {item.TableDefine}"); continue; }
                if (helper.TryExistsInHierarchy(cfgKey.Mod, cfgKey.ConfigName, out _))
                { Debug.LogError($"应用 ReWrite 时配置已存在: {cfgKey}"); continue; }
                var config = helper.DeserializeConfigFromXml(item.XmlElement, cfgKey.Mod, cfgKey.ConfigName, OverrideMode.ReWrite);
                if (config != null)
                    RegisterConfigInternal(config, tableHandle, cfgKey.Mod, cfgKey.ConfigName);
            }

            foreach (var kv in pendingModifies)
            {
                var cfgKey = kv.Key;
                var modifyXml = kv.Value;
                if (modifyXml == null) continue;
                if (!_typeLookUp.TryGetValueByKey(GetTblSFromCfgS(cfgKey), out var tableHandle))
                { Debug.LogError($"应用 Modify 时找不到 TblI: {cfgKey}"); continue; }
                var key = (tableHandle, cfgKey.Mod, cfgKey.ConfigName);
                if (!_registeredConfigs.TryGetValue(key, out var existing))
                { Debug.LogError($"应用 Modify 时配置不存在: {cfgKey}"); continue; }
                var helper = GetClassHelperByTable(GetTblSFromCfgS(cfgKey));
                if (helper == null) { Debug.LogError($"应用 Modify 时找不到 Helper: {cfgKey}"); continue; }
                helper.FillFromXml(existing, modifyXml, cfgKey.Mod, cfgKey.ConfigName);
            }

            foreach (var cfgKey in pendingDeletes.Keys)
            {
                var tableDefine = new TblS(cfgKey.Mod, cfgKey.TableName);
                if (!_typeLookUp.TryGetValueByKey(tableDefine, out var tableHandle))
                { Debug.LogError($"应用 Delete 时找不到 TblI: {cfgKey}"); continue; }
                var key = (tableHandle, cfgKey.Mod, cfgKey.ConfigName);
                if (!_registeredConfigs.Remove(key, out var removed))
                { Debug.LogWarning($"应用 Delete 时配置不存在: {cfgKey}"); continue; }
                if (_configsByTable.TryGetValue(tableHandle, out var list))
                    list.Remove(removed);
                _configKeyToCfgI.Remove(key);
            }
        }

        private static TblS GetTblSFromCfgS(CfgS cfg)
        {
            return new TblS(cfg.Mod, cfg.TableName);
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

        public ITypeConverter<TSource, TTarget> GetConverterByType<TSource, TTarget>()
        {
            return TypeConverterRegistry.GetConverterByType<TSource, TTarget>();
        }

        public bool HasConverter<TSource, TTarget>(string domain = "")
        {
            return TypeConverterRegistry.HasConverter<TSource, TTarget>(domain);
        }

        public bool HasConverterByType<TSource, TTarget>()
        {
            return TypeConverterRegistry.GetConverterByType<TSource, TTarget>() != null;
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