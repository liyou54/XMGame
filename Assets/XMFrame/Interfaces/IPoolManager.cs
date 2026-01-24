using System;
using System.Collections.Generic;
using XMFrame.Implementation;

namespace XMFrame.Interfaces
{
    /// <summary>
    /// 对象池配置
    /// </summary>
    public class PoolConfig<T>
    {
        /// <summary>
        /// 创建对象的回调
        /// </summary>
        public Func<T> OnCreate { get; set; }

        /// <summary>
        /// 销毁对象的回调
        /// </summary>
        public Action<T> OnDestroy { get; set; }

        /// <summary>
        /// 从池中获取对象时的回调
        /// </summary>
        public Action<T> OnGet { get; set; }

        /// <summary>
        /// 释放对象回池时的回调
        /// </summary>
        public Action<T> OnRelease { get; set; }

        /// <summary>
        /// 初始化容量
        /// </summary>
        public int InitialCapacity { get; set; } = 0;

        /// <summary>
        /// 最大容量（0表示无限制）
        /// </summary>
        public int MaxCapacity { get; set; } = 0;
    }

    /// <summary>
    /// 对象池接口
    /// </summary>
    public interface IPool<T> : IDisposable
    {
        /// <summary>
        /// 从池中获取对象
        /// </summary>
        T Get();

        /// <summary>
        /// 释放对象回池
        /// </summary>
        void Release(T item);

        /// <summary>
        /// 清空池中所有对象
        /// </summary>
        void Clear();

        /// <summary>
        /// 当前池中可用对象数量
        /// </summary>
        int Count { get; }

        /// <summary>
        /// 当前激活的对象数量
        /// </summary>
        int ActiveCount { get; }
    }

    /// <summary>
    /// 对象池管理器接口
    /// </summary>
    public interface IPoolManager : IManager<IPoolManager>
    {
        /// <summary>
        /// 获取或创建默认对象池（使用new()创建对象）
        /// </summary>
        IPool<T> GetOrCreatePool<T>() where T : new();

        /// <summary>
        /// 获取或创建自定义配置的对象池
        /// </summary>
        IPool<T> GetOrCreatePool<T>(string poolName, PoolConfig<T> config);

        /// <summary>
        /// 获取对象池（如果不存在返回null）
        /// </summary>
        IPool<T> GetPool<T>(string poolName);

        /// <summary>
        /// 检查对象池是否存在
        /// </summary>
        bool HasPool<T>(string poolName);

        /// <summary>
        /// 销毁指定对象池
        /// </summary>
        void DestroyPool<T>(string poolName);
        
        /// <summary>
        /// 销毁所有对象池
        /// </summary>
        void DestroyAllPools();
    }
}