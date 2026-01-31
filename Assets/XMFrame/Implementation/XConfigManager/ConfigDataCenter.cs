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

        public override async UniTask OnInit()
        {
            await RegisterModConfig();
            SolveConfigReference();
        }

        private void SolveConfigReference()
        {
            // 先预注册
        }

        private async UniTask RegisterModConfig()
        {
            RegisterModHelper();
            RegisterDynamicConfigType();
            await ReadConfigFromXmlAsync();
            InitUnManagedData();
        }


        /// <summary>从 XML 读取配置。从 xmlDoc.Load 起多线程（一个文件一个线程），传入并发容器直接写入，减少 GC。</summary>
        private async UniTask ReadConfigFromXmlAsync()
        {
            var fileInfos = new List<(string ModName, ModI ModId, string XmlFilePath)>();
            var enableMods = IModManager.I.GetSortedModConfigs();
            foreach (var modConfig in enableMods)
            {
                var modName = modConfig.ModConfig.ModName;
                var modId = IModManager.I.GetModId(modConfig.ModConfig.ModName);
                var files = IModManager.I.GetModXmlFilePathByModId(modId);
                foreach (var path in files)
                    fileInfos.Add((modName, modId, path));
            }

            XLog.DebugFormat("[Config] ReadConfigFromXmlAsync 开始, 文件数: {0}", fileInfos.Count);

            var pendingAdds = new ConcurrentDictionary<CfgS, IXConfig>();
            var pendingReWrite = new ConcurrentDictionary<CfgS, IXConfig>();
            var pendingDeletes = new ConcurrentDictionary<CfgS, byte>();
            var pendingModifies = new ConcurrentDictionary<CfgS, XmlElement>();

            var parseResults = await UniTask.WhenAll(
                fileInfos.Select(f => UniTask.RunOnThreadPool(() =>
                    ProcessSingleXmlFile(f.XmlFilePath, f.ModName, f.ModId, pendingAdds, pendingReWrite,
                        pendingDeletes, pendingModifies))));

            await UniTask.SwitchToMainThread();

            foreach (var errorMessage in parseResults)
            {
                if (!string.IsNullOrEmpty(errorMessage))
                    XLog.Error(errorMessage);
            }

            XLog.DebugFormat("[Config] 解析完成 pending: Add={0}, Modify={1}, ReWrite={2}, Delete={3}",
                pendingAdds.Count, pendingModifies.Count, pendingReWrite.Count, pendingDeletes.Count);
            ApplyPendingConfigs(pendingAdds, pendingReWrite, pendingDeletes, pendingModifies);
        }

        /// <summary>单文件从 xmlDoc.Load 起在线程池执行：加载 XML、解析 ConfigItem；结果写入 pendingAdds/pendingReWrite/pendingDeletes，Modify 仅写入 pendingModifies(XmlElement) 留待最后合并。多线程仅由外层 WhenAll 保证。</summary>
        private string ProcessSingleXmlFile(
            string xmlFilePath,
            string modName,
            ModI modId,
            ConcurrentDictionary<CfgS, IXConfig> pendingAdds,
            ConcurrentDictionary<CfgS, IXConfig> pendingReWrite,
            ConcurrentDictionary<CfgS, byte> pendingDeletes,
            ConcurrentDictionary<CfgS, XmlElement> pendingModifies)
        {
            var errorBuilder = default(System.Text.StringBuilder);
            try
            {
                XLog.DebugFormat("[Config] ProcessSingleXmlFile 开始: {0}, mod={1}", xmlFilePath, modName);
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlFilePath);

                var root = xmlDoc.DocumentElement;
                if (root == null)
                    return $"XML 文件根节点为空: {xmlFilePath}";

                var configItems = root.SelectNodes("ConfigItem");
                if (configItems == null || configItems.Count == 0)
                {
                    XLog.DebugFormat("[Config] 无 ConfigItem: {0}", xmlFilePath);
                    return null;
                }

                XLog.DebugFormat("[Config] ConfigItem 数量: {0}, 文件: {1}", configItems.Count, xmlFilePath);

                foreach (XmlElement configItem in configItems)
                {
                    try
                    {
                        ProcessConfigItemToConcurrent(configItem, xmlFilePath ?? "", modName, modId,
                            pendingAdds, pendingReWrite, pendingDeletes, pendingModifies);
                    }
                    catch (Exception ex)
                    {
                        if (errorBuilder == null) errorBuilder = new System.Text.StringBuilder();
                        errorBuilder.Append($"[ConfigItem] {xmlFilePath}: {ExceptionUtil.GetMessageWithInner(ex)}; ");
                    }
                }

                if (errorBuilder == null)
                    XLog.DebugFormat("[Config] ProcessSingleXmlFile 完成(无错误): {0}", xmlFilePath);
                return errorBuilder?.ToString();
            }
            catch (XmlException xmlEx)
            {
                return $"处理 XML 文件失败: {xmlFilePath} 行 {xmlEx.LineNumber}:{xmlEx.LinePosition} - {xmlEx.Message}";
            }
            catch (Exception ex)
            {
                return $"处理 XML 文件失败: {xmlFilePath} - {ExceptionUtil.GetMessageWithInner(ex)}";
            }
        }

        /// <summary>与 ProcessConfigItem 逻辑一致。None/ReWrite 解析后写入 pendingAdds/pendingReWrite，Modify 仅写入 pendingModifies(XmlElement) 留待最后合并，Delete 写入 pendingDeletes。</summary>
        private void ProcessConfigItemToConcurrent(
            XmlElement configItem,
            string xmlFilePath,
            string modName,
            ModI modId,
            ConcurrentDictionary<CfgS, IXConfig> pendingAdds,
            ConcurrentDictionary<CfgS, IXConfig> pendingReWrite,
            ConcurrentDictionary<CfgS, byte> pendingDeletes,
            ConcurrentDictionary<CfgS, XmlElement> pendingModifies)
        {
            var cls = configItem.GetAttribute("cls");
            var id = configItem.GetAttribute("id");
            var overrideAttr = configItem.GetAttribute("override");

            if (string.IsNullOrEmpty(cls) || string.IsNullOrEmpty(id))
            {
                XLog.DebugFormat("[Config] 跳过 ConfigItem cls 或 id 为空: cls={0}, id={1}, file={2}", cls ?? "", id ?? "",
                    xmlFilePath);
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
                return;

            if (!string.Equals(idModName, modName, StringComparison.Ordinal) && string.IsNullOrEmpty(overrideAttr))
                return;

            var overrideMode = ParseOverrideMode(overrideAttr);
            var helper = ResolveTypeFromCls(cls, modName);

            if (helper == null)
            {
                XLog.DebugFormat("[Config] 未解析到 Helper: cls={0}, mod={1}, id={2}", cls, modName, configName);
                return;
            }

            var tbls = helper.GetTblS();
            var modKey = new ModS(idModName);
            var cfgKey = new CfgS(modKey, tbls.TableName, configName);
            XLog.DebugFormat("[Config] ProcessConfigItem: key={0}, override={1}", cfgKey, overrideMode);

            if (overrideMode == OverrideMode.Modify)
            {
                if (!_typeLookUp.TryGetValueByKey(tbls, out _))
                {
                    XLog.DebugFormat("[Config] Modify 未找到表跳过: tbls={0}, key={1}", tbls, cfgKey);
                    return;
                }
                pendingModifies[cfgKey] = configItem;
                XLog.DebugFormat("[Config] pending Modify(Xml): {0}", cfgKey);
                return;
            }

            if (overrideMode == OverrideMode.None || overrideMode == OverrideMode.ReWrite)
            {
                if (helper.TryExistsInHierarchy(modKey, configName, out _))
                {
                    XLog.DebugFormat("[Config] 已存在跳过: {0}, mode={1}", cfgKey, overrideMode);
                    return;
                }

                if (!_typeLookUp.TryGetValueByKey(tbls, out var tableHandle))
                {
                    XLog.DebugFormat("[Config] 未找到表跳过: tbls={0}, key={1}", tbls, cfgKey);
                    return;
                }

                var context = new ConfigParseContext { FilePath = xmlFilePath ?? "", Line = 0, Mode = overrideMode };
                var task = new ConfigParseTask(context, configItem, helper, modKey, configName, overrideMode,
                    tableHandle, xmlFilePath ?? "");
                var res = task.Execute();
                if (res.IsValid)
                {
                    if (overrideMode == OverrideMode.None)
                    {
                        pendingAdds[cfgKey] = res.Config;
                        XLog.DebugFormat("[Config] pending Add: {0}", cfgKey);
                    }
                    else
                    {
                        pendingReWrite[cfgKey] = res.Config;
                        XLog.DebugFormat("[Config] pending ReWrite: {0}", cfgKey);
                    }
                }
                else
                    XLog.DebugFormat("[Config] 解析无效跳过: {0}, mode={1}", cfgKey, overrideMode);
            }
            else if (overrideMode == OverrideMode.Delete)
            {
                pendingDeletes[cfgKey] = 0;
                XLog.DebugFormat("[Config] pending Delete: {0}", cfgKey);
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
        /// 从 cls 字符串解析 Type。支持 "MyMod:: MyItemConfig" 形式，取 "::" 后并 Trim 作为类型名解析。
        /// </summary>
        private ConfigClassHelper ResolveTypeFromCls(string cls, string configInMod)
        {
            if (string.IsNullOrWhiteSpace(cls))
                return null;
            var normalized = cls.Trim();
            var idx = normalized.IndexOf("::", StringComparison.Ordinal);
            if (idx >= 0)
                normalized = normalized.Substring(idx + 2).Trim();

            var tbls = new TblS(configInMod, normalized);
            return _classHelperCache.GetByKey1(tbls);
        }

        /// <summary>
        /// 从 XConfig 类型获取表名（与 RegisterModHelper 中 TblS 使用的表名一致）。
        /// </summary>
        private string GetTableNameFromConfigType(Type configType)
        {
            var unmanagedType = GetUnmanagedTypeFromConfig(configType);
            return unmanagedType != null ? GetTableNameFromUnmanagedType(unmanagedType) : null;
        }

        /// <summary>
        /// </summary>
        private void ApplyPendingConfigs(
            ConcurrentDictionary<CfgS, IXConfig> pendingAdds,
            ConcurrentDictionary<CfgS, IXConfig> pendingReWrite,
            ConcurrentDictionary<CfgS, byte> pendingDeletes,
            ConcurrentDictionary<CfgS, XmlElement> pendingModifies)
        {
            XLog.DebugFormat("[Config] ApplyPendingConfigs 开始: Add={0}, Modify={1}, ReWrite={2}, Delete={3}",
                pendingAdds.Count, pendingModifies.Count, pendingReWrite.Count, pendingDeletes.Count);

            // 1. 先处理 ReWrite：合并或覆盖到 pendingAdds
            foreach (var kv in pendingReWrite)
            {
                var tbls = GetTblSFromCfgS(kv.Key);
                var helper = GetClassHelperByTable(tbls);
                IXConfig dst = null;
                if (pendingAdds.TryGetValue(kv.Key, out var fromAdd))
                    dst = fromAdd;
                else if (_typeLookUp.TryGetValueByKey(tbls, out var tableHandle) &&
                         _configsByTable.TryGetValue(tableHandle, out var dict) &&
                         dict.TryGetValue(kv.Key, out var fromTable))
                    dst = fromTable;
                if (dst != null && helper != null)
                {
                    helper.MergeConfig(kv.Value, dst);
                    pendingAdds[kv.Key] = dst;
                }
                else
                    pendingAdds[kv.Key] = kv.Value;
            }

            pendingReWrite.Clear();

            // 2. 处理删除：从 _configLookUp 与 _configsByTable 移除
            foreach (var cfgKey in pendingDeletes.Keys)
            {
                _configLookUp.RemoveByKey(cfgKey);
                var tbls = GetTblSFromCfgS(cfgKey);
                if (_typeLookUp.TryGetValueByKey(tbls, out var tableHandle) &&
                    _configsByTable.TryGetValue(tableHandle, out var dict))
                    dict.Remove(cfgKey);
            }

            pendingDeletes.Clear();

            // 3. 最后处理 Modify：用 Xml 合并到已有配置（FillFromXml）
            foreach (var kv in pendingModifies)
            {
                var tbls = GetTblSFromCfgS(kv.Key);
                var helper = GetClassHelperByTable(tbls);
                IXConfig dst = null;
                if (pendingAdds.TryGetValue(kv.Key, out var fromAdd))
                    dst = fromAdd;
                else if (_typeLookUp.TryGetValueByKey(tbls, out var tableHandle) &&
                         _configsByTable.TryGetValue(tableHandle, out var dict) &&
                         dict.TryGetValue(kv.Key, out var fromTable))
                    dst = fromTable;
                if (dst != null && helper != null)
                {
                    helper.FillFromXml(dst, kv.Value, kv.Key.Mod, kv.Key.ConfigName);
                    pendingAdds[kv.Key] = dst;
                }
                else
                {
                    XLog.WarningFormat("[Config] Modify 目标配置不存在 {0}", kv.Key);
                }
            }

            pendingModifies.Clear();

            XLog.Debug("[Config] ApplyPendingConfigs 完成");
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