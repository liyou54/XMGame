using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using UnityEngine;
using XMFrame.Implementation;
using XMFrame.Interfaces;
using System.Xml;
using Unity.Collections;
using XMFrame.Interfaces.ConfigMananger;
using XMFrame.Utils;

namespace XMFrame
{
    [ManagerDependency(typeof(IModManager))]
    public class ConfigDataCenter : ManagerBase<IConfigDataCenter>, IConfigDataCenter
    {
        // ClassHelper 实例缓存：TableDefine、HelperType -> IConfigClassHelper 实例
        private readonly MultiKeyDictionary<TableDefine, Type, IConfigClassHelper> _classHelperCache = new();

        private readonly BidirectionalDictionary<TableDefine, TableHandle> _typeLookUp = new();
        
        private XBlobContainer BlobContainer { get; set; }
        
        public override UniTask OnCreate()
        {
            BlobContainer = new XBlobContainer();
            // 预分配4m内存
            BlobContainer.Create(Allocator.Persistent,4*1024*1024);
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
                        // 获取 TableDefine（从 ConfigKey<T>.TableName 静态属性）
                        var tableDefine = GetTableDefineFromType(configType, modConfig.ModName);

                        // 创建 ClassHelper 实例
                        var helperTypeName = configType.FullName + "ClassHelper";
                        var helperType = assembly.GetType(helperTypeName);

                        if (helperType == null)
                        {
                            Debug.LogWarning($"找不到 ClassHelper: {helperTypeName}");
                            continue;
                        }

                        var helper = (IConfigClassHelper)Activator.CreateInstance(helperType, this);

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
        /// 从类型获取 TableDefine
        /// </summary>
        private TableDefine GetTableDefineFromType(Type configType, string modName)
        {
            // 从 XConfig<T, TUnmanaged> 泛型参数获取 TUnmanaged
            var baseType = configType.BaseType;
            while (baseType != null && baseType.IsGenericType)
            {
                var genericArgs = baseType.GetGenericArguments();
                if (genericArgs.Length >= 2)
                {
                    var unmanagedType = genericArgs[1];
                    // 获取 ConfigKey<TUnmanaged>.TableName
                    var configKeyType = typeof(ConfigKey<>).MakeGenericType(unmanagedType);
                    var tableNameProp =
                        configKeyType.GetProperty("TableName", BindingFlags.Public | BindingFlags.Static);
                    if (tableNameProp != null)
                    {
                        var tableName = tableNameProp.GetValue(null) as string;
                        if (!string.IsNullOrEmpty(tableName))
                        {
                            return new TableDefine(new ModKey(modName), tableName);
                        }
                    }
                }

                baseType = baseType.BaseType;
            }

            // 如果无法获取，使用配置类型名作为表名
            return new TableDefine(new ModKey(modName), configType.Name);
        }

        public override UniTask OnInit()
        {
            RegisterModConfig();
            ReadAllConfigFromMods();
            SolveConfigReference();
            return UniTask.CompletedTask;
        }

        private void SolveConfigReference()
        {
            // 先预注册
        }

        private void ReadAllConfigFromMods()
        {
            // 按 Mod 分组处理 XML 文件，避免重复读取
            var processedMods = new HashSet<ModHandle>(128);
            
            foreach (var kvp in _typeLookUp.Pairs)
            {
                var tableDefine = kvp.Key;
                var tableHandle = kvp.Value;
                
                // 如果该 Mod 已处理过，跳过
                if (!processedMods.Add(tableHandle.Mod))
                {
                    continue;
                }
                
                // 获取该 Mod 的所有 XML 配置文件
                var xmlFiles = IModManager.I.GetModXmlFilePathByModId(tableHandle.Mod);
                
                foreach (var xmlFile in xmlFiles)
                {
                    try
                    {
                        // 加载并解析 XML 文件
                        LoadXmlFile(xmlFile);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"加载 XML 文件失败: {xmlFile}, 错误: {ex.Message}");
                    }
                }
            }
        }

        /// <summary>
        /// 加载单个 XML 文件，根据配置项类型分发到对应的 ClassHelper
        /// </summary>
        private void LoadXmlFile(string xmlFilePath)
        {
            
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlFilePath);
            var root = xmlDoc.DocumentElement;
            if (root == null)
            {
                Debug.LogWarning($"XML 文件根节点为空: {xmlFilePath}");
                return;
            }

            // 遍历所有 ConfigItem
            var configItems = root.SelectNodes("ConfigItem");
            if (configItems == null || configItems.Count == 0)
            {
                return;
            }

            foreach (XmlElement itemElement in configItems)
            {
                try
                {
                    // 从 XML 元素获取配置类型信息（可能通过 class 属性或其他方式）
                    var configTypeName = itemElement.GetAttribute("class");
                    if (string.IsNullOrEmpty(configTypeName))
                    {
                        Debug.LogWarning($"ConfigItem 缺少 class 属性: {xmlFilePath}");
                        continue;
                    }

                    // 根据类型名查找对应的 ClassHelper
                    var helper = FindClassHelperByTypeName(configTypeName);
                    if (helper == null)
                    {
                        Debug.LogWarning($"找不到类型 {configTypeName} 的 ClassHelper");
                        continue;
                    }

                    // 使用 ClassHelper 注册配置到管理器
                    helper.RegisterToManager(itemElement);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"处理 ConfigItem 失败: {xmlFilePath}, 错误: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 根据配置类型名查找对应的 ClassHelper
        /// </summary>
        private IConfigClassHelper FindClassHelperByTypeName(string typeName)
        {
            // 遍历 _classHelperCache 查找匹配的类型
            foreach (var (helper, tableDefine, configType) in _classHelperCache.Pairs)
            {
                if (configType.Name == typeName || configType.FullName == typeName)
                {
                    return helper;
                }
            }
            
            return null;
        }

        private void RegisterModConfig()
        {
            RegisterModHelper();
            RegisterDynamicConfigType();
        }


        public void RegisterDynamicConfigType()
        {
            // 为每个收集到的配置类型创建 TableHandle 并注册
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

                    // 从 TableDefine 获取 ModName
                    var modName = tableDefine.DefinedInMod.Name;

                    // 获取 ModHandle
                    var modHandle = IModManager.I.GetModId(modName);

                    // 创建 TableHandle
                    var tableHandle = new TableHandle(tableIdCounter++, modHandle);

                    // 注册到 _typeLookUp
                    _typeLookUp.AddOrUpdate(tableDefine, tableHandle);

                    // 设置 TableHandle<TUnmanaged>.DefinedInMod 静态字段
                    // 注意：这里暂时使用反射，后续可以通过代码生成优化
                    var tableHandleGenericType = typeof(TableHandle<>).MakeGenericType(unmanagedType);
                    var definedInModField = tableHandleGenericType.GetField("DefinedInMod",
                        BindingFlags.Public | BindingFlags.Static);

                    if (definedInModField != null)
                    {
                        definedInModField.SetValue(null, modHandle);
                    }
                    else
                    {
                        Debug.LogWarning($"无法找到 TableHandle<{unmanagedType.Name}>.DefinedInMod 字段");
                    }
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


        public void LoadConfigFromXmlElement(XmlElement element)
        {
            throw new NotImplementedException();
        }

        public void RegisterConfigTable()
        {
            // 此方法由 SoleModConfig 调用，已合并到 RegisterDynamicConfigType
            RegisterDynamicConfigType();
        }

        public void RegisterConfigTable<T>() where T : XConfig
        {
            throw new NotImplementedException();
        }

        public void LoadConfigFromXmlElement<T>(XmlElement element) where T : XConfig
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

        public void RegisterData<T>(T data) where T : XConfig
        {
        }

        public void UpdateData<T>(T data) where T : XConfig
        {
        }

        public IConfigClassHelper<T> GetClassHelper<T>() where T : XConfig
        {
            var configType = typeof(T);
            return (IConfigClassHelper<T>)GetClassHelper(configType);
        }

        public IConfigClassHelper GetClassHelper(Type configType)
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
            var helper = (IConfigClassHelper)Activator.CreateInstance(helperType, this);

            // 获取 TableDefine 并添加到缓存（双键）
            // 注意：这里使用默认的 ModKey，因为没有模块上下文信息
            var tableDefine = GetTableDefineFromType(configType, "Default");
            _classHelperCache.AddOrUpdate(helper, tableDefine, configType);

            return helper;
        }

        public IConfigClassHelper GetClassHelperByTable(TableDefine tableDefine)
        {
            return _classHelperCache.GetByKey1(tableDefine);
        }
    }
}