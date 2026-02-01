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
    /// <summary>
    /// 配置数据中心：管理 Mod 下所有配置类型的注册、XML 解析、待处理配置应用及查询。
    /// </summary>
    [AutoCreate]
    [ManagerDependency(typeof(IModManager))]
    public class ConfigDataCenter : ManagerBase<IConfigDataCenter>, IConfigDataCenter
    {
        #region 内部类型

        /// <summary>持有 ConfigData，供 ClassHelper 在 AllocUnManaged 时访问 Blob 与表映射</summary>
        public sealed class ConfigDataHolder
        {
            public ConfigData Data;
        }

        #endregion

        #region 私有字段

        /// <summary>多键缓存：TblS / HelperType / ConfigType / UnmanagedType -> ConfigClassHelper</summary>
        private readonly MultiKeyDictionary<TblS, Type, Type, Type, ConfigClassHelper> _classHelperCache = new();

        /// <summary>TblS 与 TblI 双向映射</summary>
        private readonly BidirectionalDictionary<TblS, TblI> _typeLookUp = new();

        /// <summary>TblS 与 TblI 双向映射</summary>
        private readonly BidirectionalDictionary<CfgS, CfgI> _cfgLookUp = new();
        
        /// <summary>配置数据容器持有者</summary>
        private readonly ConfigDataHolder _configHolder = new();

        /// <summary>按 (表, Mod) 分配的下一个 CfgI 序号（预留）</summary>
        private readonly Dictionary<(TblI table, ModI mod), short> _nextCfgIByTableMod = new();

        /// <summary>单条 ConfigItem 处理器，懒创建</summary>
        private ConfigItemProcessor _configItemProcessor;

        private ConfigItemProcessor GetConfigItemProcessor() =>
            _configItemProcessor ??= new ConfigItemProcessor(new ConfigItemProcessorContextImpl(this));

        #endregion

        #region 生命周期

        /// <remarks>主要步骤：1. 创建 ConfigData；2. 以 Persistent 分配器初始化 Blob（4MB）。</remarks>
        public override UniTask OnCreate()
        {
            // 创建配置数据容器并初始化 Blob 与表映射
            _configHolder.Data = new ConfigData();
            _configHolder.Data.Create(Allocator.Persistent, 4 * 1024 * 1024);

            return UniTask.CompletedTask;
        }

        #endregion

        #region Mod / Helper 注册与解析

        /// <summary>从已加载 Mod 程序集中扫描并注册所有 ConfigClassHelper</summary>
        /// <remarks>主要步骤：1. 遍历已启用 Mod 的程序集；2. 安全获取程序集内类型；3. 筛选继承 ConfigClassHelper&lt;,&gt; 的类；4. 触发静态构造并创建实例；5. 写入 _classHelperCache。</remarks>
        private void RegisterModHelper()
        {
            // 获取当前已加载的 Mod 运行时列表
            var enabledMods = IModManager.I.GetModRuntime();

            foreach (var modConfig in enabledMods)
            {
                Assembly assembly = modConfig.Assembly;

                // 跳过未加载或无效的程序集
                if (assembly == null)
                    continue;
                // 安全获取程序集内所有类型，避免 ReflectionTypeLoadException 导致整段失败
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types ?? Array.Empty<Type>();
                }

                // 以 ConfigClassHelper<,> 为基准筛选 Helper 类型
                var helperBaseDef = typeof(ConfigClassHelper<,>);
                foreach (var helperType in types)
                {
                    // 跳过 null、非类、抽象类
                    if (helperType == null || !helperType.IsClass || helperType.IsAbstract)
                        continue;
                    var baseType = helperType.BaseType;
                    if (baseType?.IsGenericType != true || baseType.GetGenericTypeDefinition() != helperBaseDef)
                        continue;
                    var args = baseType.GetGenericArguments();
                    if (args.Length != 2)
                        continue;

                    var configType = args[0];
                    var unmanagedType = args[1];
                    try
                    {
                        // 先触发静态构造，保证 CfgS&lt;T&gt;.TableName 等生成代码已赋值
                        RuntimeHelpers.RunClassConstructor(helperType.TypeHandle);
                        // 创建 Helper 实例并写入多键缓存
                        var instance = (ConfigClassHelper)Activator.CreateInstance(helperType, (IConfigDataCenter)this);
                        var tbls = instance.GetTblS();
                        _classHelperCache.Set(instance, tbls, helperType, configType, unmanagedType);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(
                            $"注册 ClassHelper 失败: {helperType.Name}, 错误: {ExceptionUtil.GetMessageWithInner(ex)}");
                    }
                }
            }
        }

        /// <remarks>主要步骤：1. 注册 Mod 配置（Helper、SubLink、动态类型、读 XML）；2. 解析配置间引用（预留）。</remarks>
        public override async UniTask OnInit()
        {
            // 注册 Helper、构建 SubLink、注册动态类型、从 XML 读取并应用
            await RegisterModConfig();
            // 预留：解析配置间引用
            SolveConfigReference();
        } 

        /// <summary>解析配置间引用（预留）</summary>
        private void SolveConfigReference()
        {
            // 先预注册
        }

        /// <summary>为每个表分配 Unmanaged 内存并初始化</summary>
        /// <remarks>主要步骤：1. 遍历所有表的待添加配置；2. 获取对应的 ClassHelper；3. 调用 AllocUnManagedAndInitHeadVal 分配内存并初始化。</remarks>
        private async UniTask FillUnmanagedData(ConcurrentDictionary<TblS, ConcurrentDictionary<CfgS, IXConfig>> pendingAdds)
        {
            foreach (var tableEntry in pendingAdds)
            {
                var tbls = tableEntry.Key;
                var kvValue = tableEntry.Value;

                // 获取该表对应的 ClassHelper
                var helper = GetClassHelperByTable(tbls);
                if (helper == null)
                {
                    XLog.Error($"[Config] FillUnmanagedData: 未找到表 {tbls} 的 ClassHelper");
                    continue;
                }

                // 获取该表的 TblI
                var tblI = GetTblI(tbls);
                if (!tblI.Valid)
                {
                    XLog.Error($"[Config] FillUnmanagedData: 表 {tbls} 的 TblI 无效");
                    continue;
                }

                // 调用 Helper 分配 Unmanaged 内存并初始化
                helper.AllocUnManagedAndInitHeadVal(tblI, kvValue, _configHolder);
            }
            
            foreach (var tableEntry in pendingAdds)
            {
                var tbls = tableEntry.Key;
                var kvValue = tableEntry.Value;

                // 获取该表对应的 ClassHelper
                var helper = GetClassHelperByTable(tbls);
                if (helper == null)
                {
                    XLog.Error($"[Config] FillUnmanagedData: 未找到表 {tbls} 的 ClassHelper");
                    continue;
                }

                // 获取该表的 TblI
                var tblI = GetTblI(tbls);
                if (!tblI.Valid)
                {
                    XLog.Error($"[Config] FillUnmanagedData: 表 {tbls} 的 TblI 无效");
                    continue;
                }

                // 调用 Helper 分配 Unmanaged 内存并初始化
                helper.AllocContainerWithoutFill(tblI,tbls, kvValue, pendingAdds, _configHolder);
            }

            foreach (var tableEntry in pendingAdds)
            {
                var tbls = tableEntry.Key;
                var kvValue = tableEntry.Value;
                // 获取该表的 TblI
                var tblI = GetTblI(tbls);
                if (!tblI.Valid)
                {
                    XLog.Error($"[Config] FillUnmanagedData: 表 {tbls} 的 TblI 无效");
                    continue;
                }
                var helper = GetClassHelperByTable(tbls);
                // 多线程
                helper.FillBasicData(tblI, kvValue, _configHolder);
            }
        }

        /// <remarks>主要步骤：1. 注册 Mod 内 Helper；2. 构建 SubLink 反向表；3. 注册动态配置类型（TblI）；4. 异步读 XML 并应用。</remarks>
        private async UniTask RegisterModConfig()
        {
            // 从 Mod 程序集扫描并注册所有 ConfigClassHelper
            RegisterModHelper();
            // 为每个 Helper 注册“谁链接到我”的 SubLinkHelper
            BuildSubLinkHelpers();
            // 为每个 Helper 分配 TblI 并写入 _typeLookUp
            RegisterDynamicConfigType();
            // 多文件并行解析 XML，结果写入 pending 容器后统一应用
            var paddingAdd = await ReadConfigFromXmlAsync();
            // 为每个表分配 Unmanaged 内存并初始化头部值
            await FillUnmanagedData(paddingAdd);
        }

        /// <summary>
        /// 构建“谁链接到我”的反向表：对每个 Helper，将 LinkHelperType 指向它的其它 Helper 注册为其 SubLinkHelper。
        /// </summary>
        /// <remarks>主要步骤：1. 遍历缓存中每个 Helper 作为 target；2. 遍历其它 Helper，若其 LinkHelperType 指向 target；3. 则注册为 target 的 SubLinkHelper。</remarks>
        private void BuildSubLinkHelpers()
        {
            foreach (var entry in _classHelperCache.Pairs)
            {
                var targetHelper = entry.value;
                var targetType = targetHelper.GetType();
                // 查找所有“链接到 target”的 Helper 并注册为 SubLinkHelper
                foreach (var otherEntry in _classHelperCache.Pairs)
                {
                    if (otherEntry.value == targetHelper) continue;
                    var linkType = otherEntry.value.GetLinkHelperType();
                    if (linkType != null && linkType == targetType)
                        targetHelper.RegisterSubLinkHelper(otherEntry.value);
                }
            }
        }

        #endregion

        #region XML 解析

        /// <summary>从 XML 读取配置。每个文件在线程池执行，结果写入并发容器，减少 GC。</summary>
        /// <remarks>主要步骤：1. 收集所有 Mod 的 XML 文件信息；2. 创建 pending 并发容器；3. 线程池并行解析每个文件；4. 切回主线程；5. 输出解析错误；6. 应用待处理配置。</remarks>
        private async UniTask<ConcurrentDictionary<TblS, ConcurrentDictionary<CfgS, IXConfig>>> ReadConfigFromXmlAsync()
        {
            // 按 Mod 顺序收集 (ModName, ModId, XmlFilePath)
            var fileInfos = new List<(string ModName, ModI ModId, string XmlFilePath)>();
            var sortedModConfigs = IModManager.I.GetSortedModConfigs();
            foreach (var modConfig in sortedModConfigs)
            {
                var modName = modConfig.ModConfig.ModName;
                var modId = IModManager.I.GetModId(modConfig.ModConfig.ModName);
                var xmlFilePaths = IModManager.I.GetModXmlFilePathByModId(modId);
                foreach (var path in xmlFilePaths)
                    fileInfos.Add((modName, modId, path));
            }

            XLog.DebugFormat("[Config] ReadConfigFromXmlAsync 开始, 文件数: {0}", fileInfos.Count);

            // 待添加 / 待删除 / 待修改 的并发容器
            var pendingAdds = new ConcurrentDictionary<TblS, ConcurrentDictionary<CfgS, IXConfig>>();
            var pendingDeletes = new ConcurrentDictionary<CfgS, byte>();
            var pendingModifies = new ConcurrentDictionary<CfgS, XmlElement>();

            // 每个文件在线程池执行 ProcessSingleXmlFile，结果写入上述容器
            var parseResults = await UniTask.WhenAll(
                fileInfos.Select(f => UniTask.RunOnThreadPool(() =>
                    ProcessSingleXmlFile(f.XmlFilePath, f.ModName, f.ModId, pendingAdds, pendingDeletes,
                        pendingModifies))));

            // 回到主线程后再写日志和应用
            await UniTask.SwitchToMainThread();

            // 输出各文件解析过程中的错误信息
            foreach (var errorMessage in parseResults)
            {
                if (!string.IsNullOrEmpty(errorMessage))
                    XLog.Error(errorMessage);
            }

            XLog.DebugFormat("[Config] 解析完成 pending: Add={0}, Modify={1}, Delete={2}",
                pendingAdds.Sum(t => t.Value.Count), pendingModifies.Count, pendingDeletes.Count);
            // 将 pending 结果应用到 ConfigData 与 Helper（删除、Modify、Add 顺序已处理）
            ApplyPendingConfigs(pendingAdds, pendingDeletes, pendingModifies);
            return pendingAdds;
        }

        /// <summary>单文件在线程池执行：加载 XML、解析 ConfigItem，结果写入 pendingAdds / pendingDeletes / pendingModifies。</summary>
        /// <remarks>主要步骤：1. 加载 XML 文档；2. 取根节点与 ConfigItem 列表；3. 逐条交给 ConfigItemProcessor 处理；4. 收集单条异常到 errorBuilder；5. 返回错误字符串（无错误为 null）。</remarks>
        private string ProcessSingleXmlFile(
            string xmlFilePath,
            string modName,
            ModI modId,
            ConcurrentDictionary<TblS, ConcurrentDictionary<CfgS, IXConfig>> pendingAdds,
            ConcurrentDictionary<CfgS, byte> pendingDeletes,
            ConcurrentDictionary<CfgS, XmlElement> pendingModifies)
        {
            var errorBuilder = default(System.Text.StringBuilder);
            try
            {
                XLog.DebugFormat("[Config] ProcessSingleXmlFile 开始: {0}, mod={1}", xmlFilePath, modName);
                // 加载 XML 文件
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(xmlFilePath);

                var root = xmlDoc.DocumentElement;
                if (root == null)
                    return $"XML 文件根节点为空: {xmlFilePath}";

                // 取所有 ConfigItem 子节点
                var configItems = root.SelectNodes("ConfigItem");
                if (configItems == null || configItems.Count == 0)
                {
                    XLog.DebugFormat("[Config] 无 ConfigItem: {0}", xmlFilePath);
                    return null;
                }

                XLog.DebugFormat("[Config] ConfigItem 数量: {0}, 文件: {1}", configItems.Count, xmlFilePath);

                // 逐条解析并写入 pending 容器，单条异常不中断，累积到 errorBuilder
                foreach (XmlElement configItem in configItems)
                {
                    try
                    {
                        GetConfigItemProcessor().Process(configItem, xmlFilePath ?? "", modName, modId,
                            pendingAdds, pendingDeletes, pendingModifies);
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

        #endregion

        #region 待处理配置应用

        /// <summary>从 cls 字符串解析 Helper。支持 "MyMod::MyItemConfig" 形式，取 "::" 后 Trim 作为类型名。</summary>
        /// <remarks>主要步骤：1. 校验 cls 非空；2. 若有 "::" 则取其后部分作为类型名；3. 构造 TblS 并用 Key1 查 Helper。</remarks>
        private ConfigClassHelper ResolveTypeFromCls(string cls, string configInMod)
        {
            if (string.IsNullOrWhiteSpace(cls))
                return null;
            // 规范化：Trim，若有 "::" 则取后半段作为类型名
            var normalized = cls.Trim();
            var idx = normalized.IndexOf("::", StringComparison.Ordinal);
            if (idx >= 0)
                normalized = normalized.Substring(idx + 2).Trim();

            // 用 (configInMod, normalized) 构造 TblS，按 Key1 查缓存
            var tbls = new TblS(configInMod, normalized);
            return _classHelperCache.GetByKey1(tbls);
        }

        /// <summary>将解析阶段的待添加/待删除/待修改配置应用到 ConfigData 与 Helper。</summary>
        /// <remarks>主要步骤：1. 处理删除（从 pendingAdds 移除、通知 SubLink 表移除引用、清空 pendingDeletes）；2. 处理 Modify（从 pendingAdds 取源配置、ParseAndFillFromXml、清空 pendingModifies）；3. 打日志。</remarks>
        private void ApplyPendingConfigs(
            ConcurrentDictionary<TblS, ConcurrentDictionary<CfgS, IXConfig>> pendingAdds,
            ConcurrentDictionary<CfgS, byte> pendingDeletes,
            ConcurrentDictionary<CfgS, XmlElement> pendingModifies)
        {
            // 1. 处理删除：从 pendingAdds 中移除该键，并让“链接到本表”的子表也移除对已删配置的引用
            foreach (var cfgKey in pendingDeletes.Keys)
            {
                RemovePendingAdd(pendingAdds, cfgKey.Table, cfgKey);
                var helper = _classHelperCache.GetByKey1(cfgKey.Table);
                if (helper != null && helper.GetSubLinkHelper() is { Count: > 0 } subHelpers)
                {
                    foreach (var subHelper in subHelpers)
                    {
                        var subTbls = subHelper.GetTblS();
                        if (pendingAdds.TryGetValue(subTbls, out var tableDict))
                        {
                            tableDict.Remove(new CfgS(cfgKey.ConfigInMod, subTbls, cfgKey.ConfigName), out _);
                        }
                    }
                }
            }

            pendingDeletes.Clear();

            // 2. 处理 Modify：从 pendingAdds 取出对应表的字典与源配置，用 Xml 合并到已有配置后清空 pendingModifies
            foreach (var modifyEntry in pendingModifies)
            {
                var tbls = GetTblSFromCfgS(modifyEntry.Key);
                var helper = GetClassHelperByTable(tbls);
                if (helper == null)
                {
                    XLog.Error($"[Config] Modify 未找到 Helper: tbls={tbls}, key={modifyEntry.Key}");
                    continue;
                }
                if (pendingAdds.TryGetValue(tbls, out var tableDict) && tableDict.TryGetValue(modifyEntry.Key, out var sourceConfig))
                {
                    helper.ParseAndFillFromXml(sourceConfig, modifyEntry.Value, modifyEntry.Key.ConfigInMod, modifyEntry.Key.ConfigName);
                }
            }

            pendingModifies.Clear();

            XLog.Debug("[Config] ApplyPendingConfigs 完成");
        }

        /// <summary>从 pendingAdds 中移除指定表下的 existingKey 登记（用于 newIndex &gt; oriIndex 时覆盖前清理）</summary>
        /// <remarks>主要步骤：1. 用 existingKey 与 tbls 构造 CfgS；2. 若该表在 pendingAdds 中则移除该键。</remarks>
        private void RemovePendingAdd(
            ConcurrentDictionary<TblS, ConcurrentDictionary<CfgS, IXConfig>> pendingAdds,
            TblS tbls,
            CfgS existingKey)
        {
            var cfgKey = new CfgS(existingKey.ConfigInMod, tbls, existingKey.ConfigName);
            if (pendingAdds.TryGetValue(tbls, out var tableDict))
                tableDict.Remove(cfgKey, out _);
        }

        /// <remarks>主要步骤：从 CfgS 取 Table 字段（即 TblS）。</remarks>
        private static TblS GetTblSFromCfgS(CfgS cfg)
        {
            return cfg.Table;
        }

        #endregion

        #region 动态配置类型注册

        /// <summary>为每个已注册的 ClassHelper 分配 TblI 并写入 _typeLookUp</summary>
        /// <remarks>主要步骤：1. 遍历 _classHelperCache；2. 从 Config 类型取 Unmanaged 类型；3. 取 Mod 名并转 ModI；4. 分配递增 TblI 并写入 _typeLookUp；5. 通知 Helper SetTblIDefinedInMod。</remarks>
        public void RegisterDynamicConfigType()
        {
            short tableIdCounter = 1;

            foreach (var entry in _classHelperCache.Pairs)
            {
                var helper = entry.value;
                var tableDefine = entry.key1;
                var configType = entry.key2;
                try
                {
                    // 从 Config 类型解析出 Unmanaged 类型，解析不到则跳过
                    var unmanagedType = GetUnmanagedTypeFromConfig(configType);
                    if (unmanagedType == null)
                    {
                        Debug.LogWarning($"无法获取配置类型 {configType.Name} 的 Unmanaged 类型");
                        continue;
                    }

                    // 用 TblS 的 Mod 名取 ModI，分配 TblI 并登记
                    var modName = tableDefine.DefinedInMod.Name;
                    var modHandle = IModManager.I.GetModId(modName);
                    var tableHandle = new TblI(tableIdCounter++, modHandle);
                    _typeLookUp.AddOrUpdate(tableDefine, tableHandle);
                    helper.SetTblIDefinedInMod(tableHandle);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"注册配置类型失败: {tableDefine}, 错误: {ex.Message}");
                }
            }
        }

        /// <summary>从 XConfig 类型获取 Unmanaged 类型（支持基类链或 IXConfig&lt;T,TUnmanaged&gt; 接口）</summary>
        /// <remarks>主要步骤：1. 沿基类链查找泛型基类，取第二个泛型参数为 TUnmanaged；2. 若未找到则从实现的 IXConfig`2 接口取第二个泛型参数；3. 都没有则返回 null。</remarks>
        private Type GetUnmanagedTypeFromConfig(Type configType)
        {
            // 1. 从基类链查找 ConfigClassHelper&lt;T, TUnmanaged&gt; 等，取 [1] 为 TUnmanaged
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

            // 2. 从实现的 IXConfig&lt;T, TUnmanaged&gt; 取第二个泛型参数
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

        #endregion

        #region 公开 API：配置查询与转换器

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

        /// <remarks>主要步骤：按 domain 从 TypeConverterRegistry 取转换器。</remarks>
        public ITypeConverter<TSource, TTarget> GetConverter<TSource, TTarget>(string domain = "")
        {
            return TypeConverterRegistry.GetConverter<TSource, TTarget>(domain);
        }

        /// <remarks>主要步骤：按类型从 TypeConverterRegistry 取转换器。</remarks>
        public ITypeConverter<TSource, TTarget> GetConverterByType<TSource, TTarget>()
        {
            return TypeConverterRegistry.GetConverterByType<TSource, TTarget>();
        }

        /// <remarks>主要步骤：按 domain 查询是否存在转换器。</remarks>
        public bool HasConverter<TSource, TTarget>(string domain = "")
        {
            return TypeConverterRegistry.HasConverter<TSource, TTarget>(domain);
        }

        /// <remarks>主要步骤：按类型查询是否存在转换器。</remarks>
        public bool HasConverterByType<TSource, TTarget>()
        {
            return TypeConverterRegistry.GetConverterByType<TSource, TTarget>() != null;
        }

        /// <remarks>主要步骤：根据表定义、Mod、配置名构造 CfgS，然后从 _cfgLookUp 查询 CfgI。</remarks>
        public bool TryGetCfgI(TblS tableDefine, ModS mod, string configName, out CfgI cfgI)
        {
            // 构造 CfgS 作为查询键
            var cfgS = new CfgS(mod, tableDefine, configName);
            
            // 从双向字典中查询
            return _cfgLookUp.TryGetValueByKey(cfgS, out cfgI);
        }

        /// <summary>从 CfgS 查询 CfgI</summary>
        /// <remarks>主要步骤：直接用 CfgS 从 _cfgLookUp 查询 CfgI。</remarks>
        public bool TryGetCfgI(CfgS cfgS, out CfgI cfgI)
        {
            return _cfgLookUp.TryGetValueByKey(cfgS, out cfgI);
        }

        /// <remarks>主要步骤：根据表句柄、Mod、配置名构造查询键，检查 ConfigData 中是否存在该配置。</remarks>
        public bool TryExistsConfig(TblI table, ModS mod, string configName)
        {
            // 从 TblI 反查 TblS（使用双向字典的正确方法）
            if (!_typeLookUp.TryGetKeyByValue(table, out var tableDefine))
                return false;

            // 构造 CfgS 查询键
            var cfgS = new CfgS(mod, tableDefine, configName);
            
            // 检查是否在 _cfgLookUp 中存在
            if (_cfgLookUp.TryGetValueByKey(cfgS, out var cfgI))
            {
                // 进一步检查 ConfigData 中是否真实存在该配置
                return _configHolder.Data.IsConfigExist(cfgI);
            }

            return false;
        }

        /// <remarks>主要步骤：预留接口，当前无实现。</remarks>
        public void UpdateData<T>(T data) where T : IXConfig
        {
        }

        /// <remarks>主要步骤：预留接口，当前无实现。</remarks>
        public void RegisterData<T>(T data) where T : IXConfig
        {
        }

        #endregion

        #region 公开 API：ClassHelper 与表句柄

        /// <remarks>主要步骤：取 T 的 Type 后调用 GetClassHelper(Type)。</remarks>
        public ConfigClassHelper GetClassHelper<T>() where T : IXConfig, new()
        {
            var configType = typeof(T);
            return GetClassHelper(configType);
        }

        /// <remarks>主要步骤：1. 校验 configType 非空；2. 按 Key3（Config 类型）查 _classHelperCache 返回 Helper。</remarks>
        public ConfigClassHelper GetClassHelper(Type configType)
        {
            if (configType == null)
            {
                throw new ArgumentNullException(nameof(configType));
            }

            return _classHelperCache.TryGetValueByKey3(configType, out var helper) ? helper : null;
        }

        /// <remarks>主要步骤：按 Key2（Helper 类型）查 _classHelperCache 返回 Helper。</remarks>
        public ConfigClassHelper GetClassHelperByHelpType(Type configType)
        {
            return _classHelperCache.TryGetValueByKey2(configType, out var helper) ? helper : null;
        }

        /// <remarks>主要步骤：按 Key1（TblS）查 _classHelperCache 返回 Helper。</remarks>
        public ConfigClassHelper GetClassHelperByTable(TblS tableDefine)
        {
            return _classHelperCache.GetByKey1(tableDefine);
        }

        /// <summary>从 TblS 获取 TblI</summary>
        /// <remarks>主要步骤：用 _typeLookUp 按 TblS 查 TblI，查不到返回 default。</remarks>
        public TblI GetTblI(TblS tableDefine)
        {
            if (_typeLookUp.TryGetValueByKey(tableDefine, out var tableHandle))
                return tableHandle;
            return default;
        }

        /// <summary>为配置分配唯一的 CfgI 索引</summary>
        /// <remarks>主要步骤：1. 从 CfgS 获取 ModI；2. 查询或初始化该(表,Mod)的下一个ID；3. 构造并返回 CfgI；4. 递增计数器；5. 注册到 _cfgLookUp。</remarks>
        public CfgI AllocCfgIndex(CfgS cfgS, TblI table)
        {
            // 从配置键获取 Mod 名并转为 ModI
            var modName = cfgS.ConfigInMod.Name;
            var modHandle = IModManager.I.GetModId(modName);

            // 获取该 (表, Mod) 组合的下一个可用 ID
            var key = (table, modHandle);
            if (!_nextCfgIByTableMod.TryGetValue(key, out var nextId))
            {
                nextId = 1; // 从 1 开始分配（0 通常表示无效）
            }

            // 构造 CfgI
            var cfgI = new CfgI(nextId, modHandle, table);

            // 递增并保存下一个 ID
            _nextCfgIByTableMod[key] = (short)(nextId + 1);

            // 将 CfgS 和 CfgI 的映射关系注册到双向字典
            _cfgLookUp.AddOrUpdate(cfgS, cfgI);

            return cfgI;
        }

        #endregion

        #region ConfigItemProcessor 上下文实现

        /// <summary>ConfigItemProcessor 的上下文实现，委托到当前 ConfigDataCenter，便于测试与解耦。</summary>
        private sealed class ConfigItemProcessorContextImpl : IConfigItemProcessorContext
        {
            private readonly ConfigDataCenter _owner;

            internal ConfigItemProcessorContextImpl(ConfigDataCenter owner) => _owner = owner;

            public bool HasTable(TblS tbls) => _owner._typeLookUp.TryGetValueByKey(tbls, out _);

            public ConfigClassHelper ResolveHelper(string cls, string configInMod) =>
                _owner.ResolveTypeFromCls(cls, configInMod);

            public int GetModSortIndex(string modName) => IModManager.I.GetModSortIndex(modName);

            public void RemovePendingAdd(
                ConcurrentDictionary<TblS, ConcurrentDictionary<CfgS, IXConfig>> pendingAdds,
                TblS tbls,
                CfgS existingKey) =>
                _owner.RemovePendingAdd(pendingAdds, tbls, existingKey);
        }

        #endregion
    }
}