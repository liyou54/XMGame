using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Cysharp.Threading.Tasks;
using XM.Contracts;
using XM.Utils;
using XM;
using XM;

namespace XM
{
    /// <summary>
    /// 游戏主入口，负责管理器的查找、排序、创建和生命周期管理
    /// </summary>
    public class GameMain : MonoBehaviour
    {
        /// <summary>
        /// 所有已创建的管理器实例字典（类型 -> 实例）
        /// </summary>
        private Dictionary<Type, IManager> _managers = new Dictionary<Type, IManager>();

    /// <summary>
    /// 管理器创建顺序列表
    /// </summary>
    private List<Type> _managerCreationOrder = new List<Type>();

    /// <summary>
    /// 是否已初始化
    /// </summary>
    private bool _isInitialized = false;

    /// <summary>
    /// Awake 时初始化所有管理器
    /// </summary>
    private async void Awake()
    {
        DontDestroyOnLoad(gameObject);
        await OnAwake();
    }

    /// <summary>
    /// 初始化所有管理器
    /// </summary>
    public async UniTask OnAwake()
    {
        if (_isInitialized)
        {
            XLog.Warning("GameMain 已经初始化，跳过重复初始化");
            return;
        }

        try
        {
            // 1. 查找所有激活管理器类型
            var managerTypes = FindAllManagerTypes();
            XLog.InfoFormat("找到 {0} 个管理器类型", managerTypes.Count);

            // 2. 过滤出需要自动创建的管理器
            var autoCreateTypes = managerTypes.Where(t => t.IsAutoCreate()).ToList();
            XLog.InfoFormat("需要自动创建的管理器: {0} 个", autoCreateTypes.Count);

            // 3. 根据依赖关系进行拓扑排序
            var sortedTypes = SortManagersByDependencies(autoCreateTypes);
            XLog.InfoFormat("拓扑排序完成，排序后的管理器数量: {0}", sortedTypes.Count);

            // 4. 创建管理器实例
            await CreateManagers(sortedTypes);

            // 5. 初始化所有管理器
            await InitializeManagers(sortedTypes);

            _isInitialized = true;
            XLog.Info("所有管理器初始化完成");
        }
        catch (Exception ex)
        {
            XLog.ErrorFormat("管理器初始化失败: {0}", ex);
            throw;
        }
    }

    /// <summary>
    /// 查找所有管理器类型（继承自 ManagerBase 且实现 IManager）
    /// </summary>
    private List<Type> FindAllManagerTypes()
    {
        var managerTypes = new List<Type>();
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (var assembly in assemblies)
        {
            try
            {
                var types = assembly.GetTypes()
                    .Where(t =>
                        t.IsClass &&
                        !t.IsAbstract &&
                        IsManagerBaseType(t))
                    .ToList();

                managerTypes.AddRange(types);
            }
            catch (ReflectionTypeLoadException ex)
            {
                XLog.WarningFormat("加载程序集 {0} 的类型时出错: {1}", assembly.FullName, ex.Message);
            }
        }

        return managerTypes;
    }

    /// <summary>
    /// 检查类型是否继承自 ManagerBase
    /// </summary>
    private bool IsManagerBaseType(Type type)
    {
        while (type != null && type != typeof(object))
        {
            if (type.IsGenericType)
            {
                var genericTypeDef = type.GetGenericTypeDefinition();
                if (genericTypeDef == typeof(ManagerBase<>))
                {
                    return true;
                }
            }
            type = type.BaseType;
        }
        return false;
    }

    /// <summary>
    /// 根据依赖关系对管理器进行拓扑排序
    /// </summary>
    private List<Type> SortManagersByDependencies(List<Type> managerTypes)
    {
        // 先按优先级排序
        var prioritySorted = managerTypes
            .OrderBy(t => t.GetAutoCreatePriority())
            .ToList();

        // 构建接口到实现类的映射
        var interfaceToImplementation = BuildInterfaceToImplementationMap(prioritySorted);

        // 然后进行拓扑排序，支持接口依赖解析
        var sortResult = TopologicalSorter.Sort(
            prioritySorted,
            managerType => 
            {
                var dependencies = managerType.GetDependencies();
                var resolvedDeps = new List<Type>();
                
                foreach (var dep in dependencies)
                {
                    // 如果依赖是接口类型，尝试解析为实现类
                    if (dep.IsInterface && interfaceToImplementation.TryGetValue(dep, out var implementation))
                    {
                        resolvedDeps.Add(implementation);
                        XLog.DebugFormat("依赖解析: {0} -> {1} (接口 {2})", 
                            managerType.Name, implementation.Name, dep.Name);
                    }
                    // 如果依赖是实现类，直接使用
                    else if (prioritySorted.Contains(dep))
                    {
                        resolvedDeps.Add(dep);
                    }
                    else
                    {
                        XLog.WarningFormat("无法解析依赖: {0} 依赖 {1}，但找不到对应的实现类", 
                            managerType.Name, dep.Name);
                    }
                }
                
                return resolvedDeps;
            }
        );

        if (!sortResult.IsSuccess)
        {
            XLog.ErrorFormat("管理器依赖关系存在循环依赖！环中的节点: {0}", 
                string.Join(", ", sortResult.CycleNodes.Select(t => t.Name)));
            throw new InvalidOperationException("管理器依赖关系存在循环依赖");
        }

        return sortResult.SortedItems.ToList();
    }

    /// <summary>
    /// 构建接口到实现类的映射
    /// </summary>
    private Dictionary<Type, Type> BuildInterfaceToImplementationMap(List<Type> managerTypes)
    {
        var map = new Dictionary<Type, Type>();
        
        foreach (var managerType in managerTypes)
        {
            // 获取管理器实现的所有接口
            var interfaces = managerType.GetInterfaces();
            
            foreach (var iface in interfaces)
            {
                // 只映射继承自 IManager 的接口（排除 IManager 本身）
                if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IManager<>))
                {
                    // 获取 IManager<TInterface> 的泛型参数
                    var genericArgs = iface.GetGenericArguments();
                    if (genericArgs.Length > 0)
                    {
                        var managerInterface = genericArgs[0];
                        
                        // 如果这个接口还没有映射，或者当前类型更合适，则添加映射
                        if (!map.ContainsKey(managerInterface))
                        {
                            map[managerInterface] = managerType;
                            XLog.DebugFormat("接口映射: {0} -> {1}", managerInterface.Name, managerType.Name);
                        }
                    }
                }
            }
        }
        
        return map;
    }

    /// <summary>
    /// 创建所有管理器实例
    /// </summary>
    private async UniTask CreateManagers(List<Type> managerTypes)
    {
        foreach (var managerType in managerTypes)
        {
            try
            {
                // 创建 GameObject
                var go = new GameObject(managerType.Name);
                go.transform.SetParent(transform);

                // 添加管理器组件
                var manager = go.AddComponent(managerType) as IManager;
                if (manager == null)
                {
                    XLog.ErrorFormat("无法创建管理器实例: {0}", managerType.Name);
                    UnityEngine.Object.Destroy(go);
                    continue;
                }

                // 设置静态实例（通过反射）
                SetStaticInstance(managerType, manager);

                // 存储管理器实例
                _managers[managerType] = manager;
                _managerCreationOrder.Add(managerType);
                await manager.OnCreate();
                XLog.InfoFormat("成功创建管理器: {0}", managerType.Name);
            }
            catch (Exception ex)
            {
                XLog.ErrorFormat("创建管理器失败: {0}, 错误: {1}", managerType.Name, ex.Message);
            }
        }
    }

    /// <summary>
    /// 设置管理器的静态实例 I
    /// </summary>
    private void SetStaticInstance(Type managerType, IManager instance)
    {
        // 查找管理器类型实现的 IManager<TInterface> 接口
        var interfaces = managerType.GetInterfaces();
        foreach (var iface in interfaces)
        {
            if (iface.IsGenericType)
            {
                var genericTypeDef = iface.GetGenericTypeDefinition();
                if (genericTypeDef == typeof(IManager<>))
                {
                    // 找到 IManager<TInterface> 接口，设置其静态字段 I
                    var field = iface.GetField("I", BindingFlags.Public | BindingFlags.Static);
                    if (field != null)
                    {
                        // 获取字段类型 TI（例如 IAssetManager）
                        var fieldType = field.FieldType;
                        
                        // 将 instance 转换为字段类型 TI
                        // 由于管理器类现在显式实现了接口，instance 应该可以转换为 TI 类型
                        object valueToSet = instance;
                        
                        // 如果 instance 的类型可以赋值给字段类型，直接设置
                        if (fieldType.IsAssignableFrom(instance.GetType()))
                        {
                            field.SetValue(null, instance);
                        }
                        else
                        {
                            // 尝试使用反射进行类型转换
                            try
                            {
                                // 使用 Convert.ChangeType 进行转换
                                valueToSet = Convert.ChangeType(instance, fieldType);
                                field.SetValue(null, valueToSet);
                            }
                            catch
                            {
                                // 如果转换失败，直接设置（可能会抛出异常，但这是预期的）
                                field.SetValue(null, instance);
                            }
                        }
                        return;
                    }
                }
            }
        }
        
        XLog.WarningFormat("无法找到管理器 {0} 的静态字段 I", managerType.Name);
    }

    /// <summary>
    /// 清除管理器的静态实例 I
    /// </summary>
    private void ClearStaticInstance(Type managerType)
    {
        // 查找管理器类型实现的 IManager<TInterface> 接口
        var interfaces = managerType.GetInterfaces();
        foreach (var iface in interfaces)
        {
            if (iface.IsGenericType)
            {
                var genericTypeDef = iface.GetGenericTypeDefinition();
                if (genericTypeDef == typeof(IManager<>))
                {
                    // 找到 IManager<TInterface> 接口，清除其静态字段 I
                    var field = iface.GetField("I", BindingFlags.Public | BindingFlags.Static);
                    if (field != null)
                    {
                        field.SetValue(null, null);
                        return;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 初始化所有管理器（调用 OnInit）
    /// </summary>
    private async UniTask InitializeManagers(List<Type> managerTypes)
    {
        foreach (var managerType in managerTypes)
        {
            if (!_managers.TryGetValue(managerType, out var manager))
            {
                continue;
            }

            try
            {
                // 调用 OnInit
                await InvokeManagerMethod(manager, "OnInit");

                XLog.InfoFormat("成功初始化管理器: {0}", managerType.Name);
            }
            catch (Exception ex)
            {
                XLog.ErrorFormat("初始化管理器失败: {0}, 错误: {1}", managerType.Name, ex.Message);
            }
        }
    }

    /// <summary>
    /// 调用管理器的异步方法
    /// </summary>
    private async UniTask InvokeManagerMethod(IManager manager, string methodName)
    {
        var method = manager.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);
        if (method != null && method.ReturnType == typeof(UniTask))
        {
            var task = method.Invoke(manager, null) as UniTask?;
            if (task.HasValue)
            {
                await task.Value;
            }
        }
    }

    #region 管理器增删改查

    /// <summary>
    /// 获取指定类型的管理器实例
    /// </summary>
    public T GetManager<T>() where T : class, IManager
    {
        var type = typeof(T);
        if (_managers.TryGetValue(type, out var manager))
        {
            return manager as T;
        }
        return null;
    }

    /// <summary>
    /// 获取指定类型的管理器实例（通过类型）
    /// </summary>
    public IManager GetManager(Type managerType)
    {
        if (_managers.TryGetValue(managerType, out var manager))
        {
            return manager;
        }
        return null;
    }

    /// <summary>
    /// 检查指定类型的管理器是否存在
    /// </summary>
    public bool HasManager<T>() where T : class, IManager
    {
        return _managers.ContainsKey(typeof(T));
    }

    /// <summary>
    /// 检查指定类型的管理器是否存在
    /// </summary>
    public bool HasManager(Type managerType)
    {
        return _managers.ContainsKey(managerType);
    }

    /// <summary>
    /// 获取所有管理器类型
    /// </summary>
    public List<Type> GetAllManagerTypes()
    {
        return new List<Type>(_managers.Keys);
    }

    /// <summary>
    /// 获取所有管理器实例
    /// </summary>
    public List<IManager> GetAllManagers()
    {
        return new List<IManager>(_managers.Values);
    }

    /// <summary>
    /// 手动创建并注册管理器（运行时动态添加）
    /// </summary>
    public async UniTask<T> CreateManager<T>() where T : MonoBehaviour, IManager
    {
        var type = typeof(T);
        
        if (_managers.ContainsKey(type))
        {
            XLog.WarningFormat("管理器 {0} 已存在", type.Name);
            return GetManager<T>();
        }

        try
        {
            // 创建 GameObject
            var go = new GameObject(type.Name);
            go.transform.SetParent(transform);
            
            // 设置为 DontDestroyOnLoad，防止场景切换时被销毁
            DontDestroyOnLoad(go);

            // 添加管理器组件
            var manager = go.AddComponent<T>();
            
            // 设置静态实例
            SetStaticInstance(type, manager);

            // 存储管理器实例
            _managers[type] = manager;
            _managerCreationOrder.Add(type);

            // 调用 OnCreate
            await manager.OnCreate();

            // 初始化
            await InvokeManagerMethod(manager, "OnInit");

            XLog.InfoFormat("成功创建并初始化管理器: {0}", type.Name);
            return manager;
        }
        catch (Exception ex)
        {
            XLog.ErrorFormat("创建管理器失败: {0}, 错误: {1}", type.Name, ex.Message);
            throw;
        }
    }

    /// <summary>
    /// 销毁指定类型的管理器
    /// </summary>
    public async UniTask DestroyManager<T>() where T : class, IManager
    {
        var type = typeof(T);
        await DestroyManager(type);
    }

    /// <summary>
    /// 销毁指定类型的管理器
    /// </summary>
    public async UniTask DestroyManager(Type managerType)
    {
        if (!_managers.TryGetValue(managerType, out var manager))
        {
            XLog.WarningFormat("管理器 {0} 不存在", managerType.Name);
            return;
        }

        try
        {
            // 调用 OnDestroy
            await InvokeManagerMethod(manager, "OnDestroy");

            // 清除静态实例
            ClearStaticInstance(managerType);

            // 销毁 GameObject
            if (manager is MonoBehaviour mb)
            {
                UnityEngine.Object.Destroy(mb.gameObject);
            }

            // 从字典中移除
            _managers.Remove(managerType);
            _managerCreationOrder.Remove(managerType);

            XLog.InfoFormat("成功销毁管理器: {0}", managerType.Name);
        }
        catch (Exception ex)
        {
            XLog.ErrorFormat("销毁管理器失败: {0}, 错误: {1}", managerType.Name, ex.Message);
        }
    }

    /// <summary>
    /// 销毁所有管理器
    /// </summary>
    public async UniTask DestroyAllManagers()
    {
        // 按创建顺序的逆序销毁
        var reverseOrder = new List<Type>(_managerCreationOrder);
        reverseOrder.Reverse();

        foreach (var managerType in reverseOrder)
        {
            await DestroyManager(managerType);
        }

        _managers.Clear();
        _managerCreationOrder.Clear();
        _isInitialized = false;
    }

    #endregion

    /// <summary>
    /// 销毁时清理所有管理器
    /// </summary>
    private async void OnDestroy()
    {
        await DestroyAllManagers();
    }
    }
}