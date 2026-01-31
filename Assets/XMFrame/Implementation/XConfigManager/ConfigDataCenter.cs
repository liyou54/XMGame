using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
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
    [AutoCreate]
    [ManagerDependency(typeof(IModManager))]
    public class ConfigDataCenter : ManagerBase<IConfigDataCenter>, IConfigDataCenter
    {
        private sealed class ConfigDataHolder
        {
            public ConfigData Data;
        }

        private readonly MultiKeyDictionary<TblS, Type, Type, Type, ConfigClassHelper> _classHelperCache = new();
        private readonly BidirectionalDictionary<TblS, TblI> _typeLookUp = new();

        private readonly BidirectionalDictionary<CfgS, CfgI> _configLookUp =
            new();

        private readonly ConfigDataHolder _configHolder = new ConfigDataHolder();
        private readonly Dictionary<TblI, Dictionary<CfgS, IXConfig>> _configsByTable = new();
        private readonly Dictionary<(TblI table, ModI mod), short> _nextCfgIByTableMod = new();

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
            var enabledMods = IModManager.I.GetModRuntime();

            foreach (var modConfig in enabledMods)
            {
                Assembly assembly = modConfig.Assembly;

                // 从 DllPath 加载程序集
                if (assembly == null)
                    continue;
                // 查找所有 XConfig 类型（安全获取类型，避免 ReflectionTypeLoadException 导致整段失败）
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types ?? Array.Empty<Type>();
                }

                // 以 ConfigClassHelper 为最外层驱动：没有 Helper 就无法进行配置解析与注册
                var helperBaseDef = typeof(ConfigClassHelper<,>);
                foreach (var helperType in types)
                {
                    if (helperType == null || !helperType.IsClass || helperType.IsAbstract)
                        continue;
                    var baseType = helperType.BaseType;
                    if (baseType?.IsGenericType != true || baseType.GetGenericTypeDefinition() != helperBaseDef)
                        continue;
                    var args = baseType.GetGenericArguments();
                    if (args.Length != 2)
                        continue;

                    var typeClass = args[0];
                    var typeUnmanaged = args[1];
                    try
                    {
                        // 先触发 Helper 的静态构造函数，否则 CfgS<T>.TableName 尚未被生成代码赋值，GetTableNameFromUnmanagedType 会得到 null
                        RuntimeHelpers.RunClassConstructor(helperType.TypeHandle);
                        var tableName = GetTableNameFromUnmanagedType(typeUnmanaged);
                        if (string.IsNullOrEmpty(tableName))
                            continue;
                        var tbls = new TblS(modConfig.ModS, tableName);
                        var instance = (ConfigClassHelper)Activator.CreateInstance(helperType, (IConfigDataCenter)this);
                        _classHelperCache.Set(instance, tbls, helperType, typeClass, typeUnmanaged);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(
                            $"注册 ClassHelper 失败: {helperType.Name}, 错误: {ExceptionUtil.GetMessageWithInner(ex)}");
                    }
                }
            }
        }


        private static string GetTableNameFromUnmanagedType(Type unmanagedType)
        {
            var configKeyType = typeof(CfgS<>).MakeGenericType(unmanagedType);
            var tableNameProp =
                configKeyType.GetProperty(nameof(CfgS.TableName), BindingFlags.Public | BindingFlags.Static);
            if (tableNameProp != null)
            {
                return tableNameProp.GetValue(null) as string;
            }

            return null;
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

            var enableMods = IModManager.I.GetSortedModConfigs();
            foreach (var modConfig in enableMods)
            {
                var modName = modConfig.ModConfig.ModName;
                var modId = IModManager.I.GetModId(modConfig.ModConfig.ModName);
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
                    XLog.ErrorFormat("处理 Mod XML 失败: {0}, 错误: {1}", modName, ExceptionUtil.GetMessageWithInner(ex));
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
                        ProcessConfigItem(configItem, xmlFilePath, modName, modId, pendingAdds, pendingModifies,
                            pendingDeletes);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"处理 ConfigItem 失败: {xmlFilePath}, 错误: {ExceptionUtil.GetMessageWithInner(ex)}");
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
                XLog.Error($"ConfigItem id 格式错误，应为 modName::configName 或 configName: {id}");
                return;
            }

            if (!string.Equals(idModName, modName, StringComparison.Ordinal) && string.IsNullOrEmpty(overrideAttr))
            {
                XLog.Error($"非当前模组不允许定义其他模组配置: 当前 mod={modName}, id 指定 mod={idModName}, id={id}");
                return;
            }

            var overrideMode = ParseOverrideMode(overrideAttr);

            var configType = ResolveTypeFromCls(cls);
            if (configType == null)
            {
                XLog.Error($"无法解析 cls 类型: {cls}");
                return;
            }

            var tbls = new TblS(idModName, configName);

            var helper = GetClassHelperByTable(tbls);
            if (helper == null)
            {
                XLog.Error($"找不到 TblS 对应的 ClassHelper: {tbls}");
                return;
            }

            var modKey = new ModS(idModName);
            var cfgKey = new CfgS(modKey, tbls.TableName, configName);

            if (overrideMode == OverrideMode.None)
            {
                if (helper.TryExistsInHierarchy(modKey, configName, out _))
                {
                    Debug.LogError($"配置已存在（或在父类中存在），不能重复添加: {id}");
                    return;
                }

                if (!_typeLookUp.TryGetValueByKey(tbls, out var tableHandle))
                {
                    Debug.LogError($"找不到 TblS 对应的 TblI: {tbls}");
                    return;
                }

                var prev = ConfigClassHelper.CurrentParseContext;
                try
                {
                    ConfigClassHelper.CurrentParseContext = new ConfigParseContext
                        { FilePath = xmlFilePath ?? "", Line = 0, Mode = overrideMode };
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
                        pendingAdds[cfgKey] = new PendingAddItem { TableDefine = tbls, XmlElement = configItem };
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
                if (helper == null)
                {
                    Debug.LogError($"应用 ReWrite 时找不到 Helper: {item.TableDefine}");
                    continue;
                }

                if (!_typeLookUp.TryGetValueByKey(item.TableDefine, out var tableHandle))
                {
                    Debug.LogError($"应用 ReWrite 时找不到 TblI: {item.TableDefine}");
                    continue;
                }

                if (helper.TryExistsInHierarchy(cfgKey.Mod, cfgKey.ConfigName, out _))
                {
                    Debug.LogError($"应用 ReWrite 时配置已存在: {cfgKey}");
                    continue;
                }

                var config = helper.DeserializeConfigFromXml(item.XmlElement, cfgKey.Mod, cfgKey.ConfigName,
                    OverrideMode.ReWrite);
                if (config != null)
                    RegisterConfigInternal(config, tableHandle, cfgKey.Mod, cfgKey.ConfigName);
            }

            foreach (var kv in pendingModifies)
            {
                var cfgKey = kv.Key;
                var modifyXml = kv.Value;
                if (modifyXml == null) continue;
                if (!_typeLookUp.TryGetValueByKey(GetTblSFromCfgS(cfgKey), out var tableHandle))
                {
                    Debug.LogError($"应用 Modify 时找不到 TblI: {cfgKey}");
                    continue;
                }

                if (!_configsByTable.TryGetValue(tableHandle, out var dictionary) || !dictionary.TryGetValue(cfgKey, out var config))
                {
                    Debug.LogError($"应用 Modify 时配置不存在: {cfgKey}");
                    continue;
                }

                var helper = GetClassHelperByTable(GetTblSFromCfgS(cfgKey));
                if (helper == null)
                {
                    Debug.LogError($"应用 Modify 时找不到 Helper: {cfgKey}");
                    continue;
                }

                helper.FillFromXml(config, modifyXml, cfgKey.Mod, cfgKey.ConfigName);
            }

            foreach (var cfgKey in pendingDeletes.Keys)
            {
                var tableDefine = new TblS(cfgKey.Mod, cfgKey.TableName);
                if (!_typeLookUp.TryGetValueByKey(tableDefine, out var tableHandle))
                {
                    Debug.LogError($"应用 Delete 时找不到 TblI: {cfgKey}");
                    continue;
                }

                if (_configsByTable.TryGetValue(tableHandle, out var dictionary))
                    dictionary.Remove(cfgKey);
            }
        }

        private static TblS GetTblSFromCfgS(CfgS cfg)
        {
            return new TblS(cfg.Mod, cfg.TableName);
        }

        private void InitUnManagedData()
        {
            // TODO: 遍历 _configsByTable，为每个配置分配 CfgI
            // TODO: 调用 AllocateCfgIForConfig 分配 CfgI 并写回 config.Data
            // TODO: 将配置填充到 Unmanaged 数据（FillToUnmanaged）
        }


        public void RegisterDynamicConfigType()
        {
            // 为每个收集到的配置类型创建 TblI 并注册
            short tableIdCounter = 1;

            // 从 _classHelperCache 遍历所有已注册的 ClassHelper
            foreach (var pair in _classHelperCache.Pairs)
            {
                var helper = pair.value;
                var tableDefine = pair.key1;
                var configType = pair.key2;
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
                    helper.SetTblIDefinedInMod(tableHandle);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"注册配置类型失败: {tableDefine}, 错误: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 从 XConfig 类型获取 Unmanaged 类型（支持基类链或实现的 IXConfig&lt;T,TUnmanaged&gt; 接口）
        /// </summary>
        private Type GetUnmanagedTypeFromConfig(Type configType)
        {
            // 1. 从基类链获取 TUnmanaged
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

            // 2. 从实现的 IXConfig<T, TUnmanaged> 接口获取 TUnmanaged
            foreach (var iface in configType.GetInterfaces())
            {
                if (iface.IsGenericType && iface.GetGenericTypeDefinition().Name == "IXConfig`2")
                {
                    var genericArgs = iface.GetGenericArguments();
                    if (genericArgs.Length >= 2)
                    {
                        return genericArgs[1];
                    }
                }
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
            var tbl = _typeLookUp.TryGetValueByKey(tableDefine, out var tableHandle);
            throw new NotImplementedException();
        }

        public bool TryExistsConfig(TblI table, ModS mod, string configName)
        {
            if (!_typeLookUp.TryGetKeyByValue(table, out var tbls))
                return false;
            var cfgKey = new CfgS(mod, tbls.TableName, configName);
            return _configsByTable.TryGetValue(table, out var dict) && dict.ContainsKey(cfgKey);
        }

        /// <summary>
        /// 注册配置到内部字典（XML 读取阶段使用，不分配 CfgI）
        /// </summary>
        private void RegisterConfigInternal(IXConfig config, TblI tableHandle, ModS modKey, string configName)
        {
            if (!_typeLookUp.TryGetKeyByValue(tableHandle, out var tbls))
                return;
            var cfgKey = new CfgS(modKey, tbls.TableName, configName);
            if (!_configsByTable.TryGetValue(tableHandle, out var dict))
            {
                dict = new Dictionary<CfgS, IXConfig>();
                _configsByTable[tableHandle] = dict;
            }
            dict[cfgKey] = config;
        }

        /// <summary>
        /// 为已注册的配置分配 CfgI（在 InitUnManagedData 时使用）
        /// </summary>
        private CfgI AllocateCfgIForConfig(TblI table, ModI mod, ModS modKey, string configName)
        {
            var key = (table, mod);
            if (!_nextCfgIByTableMod.TryGetValue(key, out var nextId))
                nextId = 1;

            var cfgId = new CfgI(nextId, mod, table);
            _nextCfgIByTableMod[key] = (short)(nextId + 1);

            if (_typeLookUp.TryGetKeyByValue(table, out var tbls))
                _configLookUp.AddOrUpdate(new CfgS(modKey, tbls.TableName, configName), cfgId);

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

            return _classHelperCache.TryGetValueByKey3(configType, out var helper) ? helper : default;
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