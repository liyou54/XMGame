using System.Collections.Generic;
using UnityEngine;
using XMFrame.Interfaces;

namespace XMFrame.Implementation
{
    /// <summary>
    /// PoolManager 使用示例
    /// </summary>
    public class PoolManagerExample : MonoBehaviour
    {
        /// <summary>
        /// 示例：一个简单的数据类
        /// </summary>
        public class PlayerData
        {
            public int Id;
            public string Name;
            public Vector3 Position;

            public void Reset()
            {
                Id = 0;
                Name = string.Empty;
                Position = Vector3.zero;
            }
        }

        /// <summary>
        /// 示例：一个列表池
        /// </summary>
        public class ListData
        {
            public List<int> Numbers = new List<int>();

            public void Reset()
            {
                Numbers.Clear();
            }
        }

        private void Start()
        {
            // 示例 1: 使用默认对象池（自动使用 new() 创建对象）
            Example1_DefaultPool();

            // 示例 2: 使用自定义配置的对象池
            Example2_CustomPool();

            // 示例 3: 带初始容量和最大容量的对象池
            Example3_PoolWithCapacity();

            // 示例 4: 使用回调函数的对象池
            Example4_PoolWithCallbacks();
        }

        /// <summary>
        /// 示例 1: 使用默认对象池
        /// </summary>
        private void Example1_DefaultPool()
        {
            XLog.Info("=== 示例 1: 默认对象池 ===");

            // 获取默认对象池（自动创建）
            var pool = IPoolManager.I.GetOrCreatePool<PlayerData>();

            // 从池中获取对象
            var player1 = pool.Get();
            player1.Id = 1;
            player1.Name = "Player1";

            var player2 = pool.Get();
            player2.Id = 2;
            player2.Name = "Player2";

            XLog.Info($"激活对象数: {pool.ActiveCount}, 池中空闲对象数: {pool.Count}");

            // 释放对象回池
            pool.Release(player1);
            pool.Release(player2);

            XLog.Info($"释放后 - 激活对象数: {pool.ActiveCount}, 池中空闲对象数: {pool.Count}");
        }

        /// <summary>
        /// 示例 2: 使用自定义配置的对象池
        /// </summary>
        private void Example2_CustomPool()
        {
            XLog.Info("=== 示例 2: 自定义对象池 ===");

            // 创建自定义配置
            var config = new PoolConfig<PlayerData>
            {
                OnCreate = () => new PlayerData(),
                OnDestroy = (obj) => XLog.Info($"销毁对象: {obj.Name}"),
                OnGet = (obj) => XLog.Info($"获取对象: {obj.Name}"),
                OnRelease = (obj) =>
                {
                    obj.Reset(); // 释放时重置对象
                    XLog.Info($"释放对象: {obj.Name}");
                }
            };

            // 创建命名对象池
            var pool = IPoolManager.I.GetOrCreatePool("CustomPlayerPool", config);

            // 使用对象池
            var player = pool.Get();
            player.Id = 100;
            player.Name = "CustomPlayer";

            pool.Release(player);
        }

        /// <summary>
        /// 示例 3: 带初始容量和最大容量的对象池
        /// </summary>
        private void Example3_PoolWithCapacity()
        {
            XLog.Info("=== 示例 3: 带容量限制的对象池 ===");

            var config = new PoolConfig<ListData>
            {
                OnCreate = () => new ListData(),
                OnDestroy = (obj) =>
                {
                    obj.Numbers.Clear();
                    XLog.Info("对象被销毁（超过最大容量）");
                },
                OnRelease = (obj) => obj.Reset(),
                InitialCapacity = 5,  // 预先创建 5 个对象
                MaxCapacity = 10      // 最多保留 10 个空闲对象
            };

            var pool = IPoolManager.I.GetOrCreatePool("ListPool", config);

            XLog.Info($"初始池容量: {pool.Count}");

            // 获取并释放多个对象
            var lists = new List<ListData>();
            for (int i = 0; i < 15; i++)
            {
                var list = pool.Get();
                list.Numbers.Add(i);
                lists.Add(list);
            }

            XLog.Info($"获取 15 个对象后 - 激活: {pool.ActiveCount}, 空闲: {pool.Count}");

            // 释放所有对象
            foreach (var list in lists)
            {
                pool.Release(list);
            }

            XLog.Info($"释放后 - 激活: {pool.ActiveCount}, 空闲: {pool.Count}（最多保留 10 个）");
        }

        /// <summary>
        /// 示例 4: 使用完整回调的对象池
        /// </summary>
        private void Example4_PoolWithCallbacks()
        {
            XLog.Info("=== 示例 4: 带完整回调的对象池 ===");

            int createCount = 0;
            int destroyCount = 0;

            var config = new PoolConfig<PlayerData>
            {
                OnCreate = () =>
                {
                    createCount++;
                    XLog.Info($"创建新对象 (总创建数: {createCount})");
                    return new PlayerData();
                },
                OnDestroy = (obj) =>
                {
                    destroyCount++;
                    XLog.Info($"销毁对象: {obj.Name} (总销毁数: {destroyCount})");
                },
                OnGet = (obj) =>
                {
                    XLog.Info($"从池中获取对象");
                },
                OnRelease = (obj) =>
                {
                    obj.Reset();
                    XLog.Info($"对象释放回池");
                },
                InitialCapacity = 2,
                MaxCapacity = 3
            };

            var pool = IPoolManager.I.GetOrCreatePool("CallbackPool", config);

            // 获取对象（从预创建的池中获取）
            var p1 = pool.Get();
            p1.Name = "P1";

            var p2 = pool.Get();
            p2.Name = "P2";

            // 获取对象（需要创建新对象）
            var p3 = pool.Get();
            p3.Name = "P3";

            // 释放对象
            pool.Release(p1);
            pool.Release(p2);
            pool.Release(p3);

            // 再次获取（从池中复用）
            var p4 = pool.Get();
            p4.Name = "P4";
            pool.Release(p4);

            // 清空对象池
            pool.Clear();
            XLog.Info($"清空后统计 - 总创建: {createCount}, 总销毁: {destroyCount}");
        }

        private void OnDestroy()
        {

            IPoolManager.I.DestroyPool<PlayerData>("CustomPlayerPool");
            IPoolManager.I.DestroyPool<ListData>("ListPool");
            IPoolManager.I.DestroyPool<PlayerData>("CallbackPool");
        }
    }
}
