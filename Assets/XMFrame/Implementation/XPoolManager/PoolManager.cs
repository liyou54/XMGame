using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using XMFrame.Interfaces;

namespace XMFrame.Implementation
{
    /// <summary>
    /// 对象池实现类
    /// 注意：此实现不支持 UnityEngine.Object 类型，仅用于普通 C# 对象
    /// 使用 Stack 存储空闲对象，HashSet 跟踪激活对象
    /// </summary>
    public class Pool<T> : IPool<T>
    {
        private readonly Stack<T> _pool;            // 存储空闲对象的栈
        private readonly HashSet<T> _activeObjects; // 跟踪当前激活的对象
        private readonly PoolConfig<T> _config;     // 对象池配置

        /// <summary>
        /// 池中可用对象数量
        /// </summary>
        public int Count => _pool.Count;

        /// <summary>
        /// 当前激活的对象数量
        /// </summary>
        public int ActiveCount => _activeObjects.Count;

        /// <summary>
        /// 构造函数，创建对象池并预热（如果配置了初始容量）
        /// </summary>
        public Pool(PoolConfig<T> config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _pool = new Stack<T>(_config.InitialCapacity);
            _activeObjects = new HashSet<T>();

            // 预热对象池：如果配置了初始容量，提前创建对象
            if (_config.InitialCapacity > 0 && _config.OnCreate != null)
            {
                for (int i = 0; i < _config.InitialCapacity; i++)
                {
                    var obj = _config.OnCreate();
                    _pool.Push(obj);
                }
            }
        }

        /// <summary>
        /// 从池中获取对象
        /// 如果池中有空闲对象则直接返回，否则创建新对象
        /// </summary>
        public T Get()
        {
            T item;

            // 优先从池中获取空闲对象
            if (_pool.Count > 0)
            {
                item = _pool.Pop();
            }
            else
            {
                // 池中没有空闲对象，创建新对象
                if (_config.OnCreate == null)
                {
                    throw new InvalidOperationException("对象池创建回调为空，无法创建新对象");
                }
                item = _config.OnCreate();
            }

            // 标记为激活状态
            _activeObjects.Add(item);
            
            // 触发获取回调（用于重置对象状态等）
            _config.OnGet?.Invoke(item);

            return item;
        }

        /// <summary>
        /// 释放对象回池
        /// 如果超过最大容量则销毁对象
        /// </summary>
        public void Release(T item)
        {
            if (item == null)
            {
                XLog.Warning("尝试释放空对象到池中");
                return;
            }

            // 检查对象是否属于此池
            if (!_activeObjects.Remove(item))
            {
                XLog.Warning("尝试释放不属于此池的对象或重复释放");
                return;
            }

            // 触发释放回调（用于清理对象状态等）
            _config.OnRelease?.Invoke(item);

            // 检查是否超过最大容量
            if (_config.MaxCapacity > 0 && _pool.Count >= _config.MaxCapacity)
            {
                // 超过最大容量，销毁对象而不是放回池中
                _config.OnDestroy?.Invoke(item);
                return;
            }

            // 放回池中
            _pool.Push(item);
        }

        /// <summary>
        /// 清空池中所有对象（包括激活的和空闲的）
        /// </summary>
        public void Clear()
        {
            // 销毁所有激活的对象
            foreach (var item in _activeObjects)
            {
                _config.OnDestroy?.Invoke(item);
            }
            _activeObjects.Clear();

            // 销毁池中所有空闲对象
            while (_pool.Count > 0)
            {
                var item = _pool.Pop();
                _config.OnDestroy?.Invoke(item);
            }
        }

        /// <summary>
        /// 释放对象池资源
        /// </summary>
        public void Dispose()
        {
            Clear();
        }
    }

    /// <summary>
    /// 对象池管理器
    /// 管理多个不同类型的对象池，支持创建、获取、销毁对象池
    /// 注意：此对象池不存储 UnityEngine.Object 类型，仅用于普通 C# 对象
    /// </summary>
    [AutoCreate]
    public class PoolManager : ManagerBase<IPoolManager>, IPoolManager
    {
        // 使用类型全名作为key来存储不同类型的对象池
        private readonly Dictionary<string, object> _pools = new Dictionary<string, object>();

        public override UniTask OnCreate()
        {
            XLog.Info("PoolManager 创建完成");
            return UniTask.CompletedTask;
        }

        public override UniTask OnInit()
        {
            XLog.Info("PoolManager 初始化完成");
            return UniTask.CompletedTask;
        }

        public override UniTask OnDestroy()
        {
            DestroyAllPools();
            XLog.Info("PoolManager 已销毁所有对象池");
            return UniTask.CompletedTask;
        }

        /// <summary>
        /// 获取或创建默认对象池
        /// 使用类型的 new() 约束自动创建对象
        /// </summary>
        public IPool<T> GetOrCreatePool<T>() where T : new()
        {
            string poolName = GetDefaultPoolName<T>();
            
            if (_pools.TryGetValue(poolName, out var pool))
            {
                return pool as IPool<T>;
            }

            var config = new PoolConfig<T>
            {
                OnCreate = () => new T(),
                OnDestroy = null,
                OnGet = null,
                OnRelease = null,
                InitialCapacity = 0,
                MaxCapacity = 0
            };

            var newPool = new Pool<T>(config);
            _pools[poolName] = newPool;
            
            XLog.Info($"创建默认对象池: {typeof(T).Name}");
            return newPool;
        }

        /// <summary>
        /// 获取或创建自定义配置的对象池
        /// 支持自定义创建、销毁、获取、释放回调
        /// </summary>
        public IPool<T> GetOrCreatePool<T>(string poolName, PoolConfig<T> config)
        {
            if (string.IsNullOrEmpty(poolName))
            {
                XLog.Error("池名称不能为空");
                throw new ArgumentException("池名称不能为空", nameof(poolName));
            }

            if (config == null)
            {
                XLog.Error("对象池配置不能为空");
                throw new ArgumentNullException(nameof(config));
            }

            if (config.OnCreate == null)
            {
                XLog.Error($"对象池 {poolName} 的创建回调不能为空");
                throw new ArgumentException("对象池配置的 OnCreate 回调不能为空", nameof(config));
            }

            string fullPoolName = GetFullPoolName<T>(poolName);

            if (_pools.TryGetValue(fullPoolName, out var pool))
            {
                XLog.Info($"获取已存在的对象池: {typeof(T).Name}_{poolName}");
                return pool as IPool<T>;
            }

            var newPool = new Pool<T>(config);
            _pools[fullPoolName] = newPool;
            
            XLog.Info($"创建自定义对象池: {typeof(T).Name}_{poolName}, 初始容量: {config.InitialCapacity}, 最大容量: {config.MaxCapacity}");
            return newPool;
        }

        /// <summary>
        /// 获取对象池（如果不存在返回 null）
        /// </summary>
        public IPool<T> GetPool<T>(string poolName)
        {
            if (string.IsNullOrEmpty(poolName))
            {
                XLog.Warning("池名称为空，无法获取对象池");
                return null;
            }

            string fullPoolName = GetFullPoolName<T>(poolName);

            if (_pools.TryGetValue(fullPoolName, out var pool))
            {
                return pool as IPool<T>;
            }

            XLog.Warning($"对象池不存在: {typeof(T).Name}_{poolName}");
            return null;
        }

        /// <summary>
        /// 销毁指定对象池
        /// </summary>
        public void DestroyPool<T>(string poolName)
        {
            if (string.IsNullOrEmpty(poolName))
            {
                XLog.Warning("池名称为空，无法销毁对象池");
                return;
            }

            string fullPoolName = GetFullPoolName<T>(poolName);

            if (_pools.TryGetValue(fullPoolName, out var pool))
            {
                (pool as IDisposable)?.Dispose();
                _pools.Remove(fullPoolName);
                XLog.Info($"已销毁对象池: {typeof(T).Name}_{poolName}");
            }
            else
            {
                XLog.Warning($"对象池不存在，无法销毁: {typeof(T).Name}_{poolName}");
            }
        }

        /// <summary>
        /// 销毁所有对象池
        /// </summary>
        public void DestroyAllPools()
        {
            int count = _pools.Count;
            foreach (var pool in _pools.Values)
            {
                (pool as IDisposable)?.Dispose();
            }
            _pools.Clear();
            
            if (count > 0)
            {
                XLog.Info($"已销毁所有对象池，共 {count} 个");
            }
        }

        /// <summary>
        /// 检查对象池是否存在
        /// </summary>
        public bool HasPool<T>(string poolName)
        {
            if (string.IsNullOrEmpty(poolName))
            {
                return false;
            }

            string fullPoolName = GetFullPoolName<T>(poolName);
            return _pools.ContainsKey(fullPoolName);
        }

        /// <summary>
        /// 获取默认对象池名称
        /// </summary>
        private string GetDefaultPoolName<T>()
        {
            return $"DefaultPool_{typeof(T).FullName}";
        }

        /// <summary>
        /// 获取完整的对象池名称（类型+自定义名称）
        /// </summary>
        private string GetFullPoolName<T>(string poolName)
        {
            return $"{typeof(T).FullName}_{poolName}";
        }
    }
}
