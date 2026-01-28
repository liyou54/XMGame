using System;
using Cysharp.Threading.Tasks;
using XM;

namespace XM.Contracts
{
    public interface IAssetId
    {
        public ModI Mod { get; set; }
        public int Id { get; set; }

        public AssetI GetAssetId()
        {
            return new AssetI(Mod, Id);
        }
    }

        /// <summary>
        /// 资源ID，用于标识资源（只是一个key，不能直接获取对象）
        /// </summary>
        public readonly struct AssetI : IEquatable<AssetI>, IAssetId
        {
            public ModI Mod { get; }
            public int Id { get; }
            
            public bool Valid => Mod.Valid && Id > 0;

            public AssetI(ModI mod, int id)
            {
                Mod = mod;
                Id = id;
            }

            ModI IAssetId.Mod 
            { 
                get => Mod; 
                set => throw new NotSupportedException("AssetI is immutable"); 
            }

            int IAssetId.Id 
            { 
                get => Id; 
                set => throw new NotSupportedException("AssetI is immutable"); 
            }

            public bool Equals(AssetI other)
            {
                return Mod.Equals(other.Mod) && Id == other.Id;
            }

            public override bool Equals(object obj)
            {
                return obj is AssetI other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return (Mod.GetHashCode() * 397) ^ Id;
                }
            }

            public static bool operator ==(AssetI left, AssetI right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(AssetI left, AssetI right)
            {
                return !left.Equals(right);
            }

            /// <summary>
            /// 创建资源句柄（通过AssetManager，异步，如果资源未加载会自动加载）
            /// </summary>
            public async UniTask<XAssetHandle> CreateHandleAsync()
            {
                if (IAssetManager.I == null)
                {
                    return null;
                }
                return await IAssetManager.I.CreateAssetHandleAsync(this);
            }
        }

    public interface IAssetAddress
    {
        public int AddressId { get; set; }
    }

    /// <summary>
    /// 可以通过这个找到 ResourceId 方便进行变量复写
    /// </summary>
    public readonly struct AssetAddress : IAssetAddress
    {
        public int AddressId { get; }

        public AssetAddress(int addressId)
        {
            AddressId = addressId;
        }

        int IAssetAddress.AddressId 
        { 
            get => AddressId; 
            set => throw new NotSupportedException("AssetAddress is immutable"); 
        }

        /// <summary>
        /// 获取资源ID
        /// </summary>
        public AssetI? GetAssetId()
        {
            if (IAssetManager.I == null )
            {
                return null;
            }
            return IAssetManager.I.GetAssetByAddress<AssetI, AssetAddress>(this);
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public async UniTask ReleaseAsync()
        {
            var assetId = GetAssetId();
            if (assetId.HasValue)
            {
                var handle = await assetId.Value.CreateHandleAsync();
                if (handle != null)
                {
                    handle.Release();
                }
            }
        }

    }

    /// <summary>
    /// 资源句柄，用于持有和访问实际资源对象（引用类型，支持using自动释放）
    /// 使用引用计数机制管理资源生命周期
    /// </summary>
    public sealed class XAssetHandle : IDisposable
    {
        private static IPool<XAssetHandle> _pool;
        private const string PoolName = "XAssetHandlePool";
        
        public AssetI Id { get; set; }
        public EAssetStatus Status;

        private XAssetHandle()
        {
        }
        
        /// <summary>
        /// 初始化对象池（由 AssetManager 在 OnCreate 时调用）
        /// </summary>
        public static void InitializePool()
        {
            if (_pool != null)
            {
                XLog.Warning("XAssetHandle 对象池已经初始化");
                return;
            }
            
            if (IPoolManager.I == null)
            {
                XLog.Error("IPoolManager.I 为 null，无法初始化 XAssetHandle 对象池");
                return;
            }
            
            var poolConfig = new PoolConfig<XAssetHandle>
            {
                OnCreate = () => new XAssetHandle(),
                OnGet = handle =>
                {
                    // 从池中获取时重置状态
                    handle.Status = EAssetStatus.None;
                    handle.Id = default;
                },
                OnRelease = handle =>
                {
                    // 归还到池时重置状态和 Id
                    handle.Id = default;
                    handle.Status = EAssetStatus.Released;
                },
                OnDestroy = handle =>
                {
                    // 销毁时无需额外操作
                },
                InitialCapacity = 100,  // 初始容量
                MaxCapacity = -1      // 最大容量
            };
            
            _pool = IPoolManager.I.GetOrCreatePool(PoolName, poolConfig);
            XLog.Info("XAssetHandle 对象池初始化完成");
        }
        
        /// <summary>
        /// 从对象池获取 XAssetHandle（内部使用）
        /// </summary>
        public static XAssetHandle GetFromPool()
        {
            if (_pool == null)
            {
                XLog.Error("XAssetHandle 对象池未初始化");
                return new XAssetHandle();
            }
            return _pool.Get();
        }
        
        /// <summary>
        /// 将 XAssetHandle 归还到对象池（内部使用）
        /// </summary>
        public static void ReleaseToPool(XAssetHandle handle)
        {
            if (_pool == null)
            {
                XLog.Warning("XAssetHandle 对象池未初始化，无法归还");
                return;
            }
            _pool.Release(handle);
        }
        
        /// <summary>
        /// 清理对象池（由 AssetManager 在 OnDestroy 时调用）
        /// </summary>
        public static void CleanupPool()
        {
            if (_pool != null)
            {
                _pool.Dispose();
                _pool = null;
            }
            
            if (IPoolManager.I != null)
            {
                IPoolManager.I.DestroyPool<XAssetHandle>(PoolName);
            }
            
            XLog.Info("XAssetHandle 对象池已清理");
        }

        /// <summary>
        /// 获取资源对象
        /// </summary>
        public T Get<T>() where T : UnityEngine.Object
        {
            if (Status != EAssetStatus.Success)
            {
                XLog.Warning($"尝试从已释放的句柄获取资源，HandleId: {Id}");
                return null;
            }
            if (IAssetManager.I == null)
            {
                return null;
            }
            return IAssetManager.I.GetAssetObject<T>(Id);
        }

        /// <summary>
        /// 释放资源句柄（减少引用计数）
        /// </summary>
        public void Release()
        {
            if (Status != EAssetStatus.Success || Status != EAssetStatus.Loading) return;
            
            if (IAssetManager.I != null)
            {
                IAssetManager.I.ReleaseAssetHandle(this);
            }
            Status = EAssetStatus.Released;
        }

        /// <summary>
        /// IDisposable实现，支持using语句
        /// </summary>
        public void Dispose()
        {
            Release();
        }

        ~XAssetHandle()
        {
            if (Status != EAssetStatus.Loading || Status != EAssetStatus.Success) return;
            XLog.Warning($"XAssetHandle 未正确释放就被GC回收，HandleId: {Id}，这可能导致内存泄漏");
        }
    }

    public enum EAssetStatus
    {
        None,
        Loading,
        Success,
        Failed,
        Cancelled,
        ErrorAndRelease,
        Released
    }

    public interface IAssetManager : IManager<IAssetManager>
    {
        public UniTask<bool> CreateResPackage(ModI modId, string modName, string path);
        public AssetAddress CreateAssetAddress(string resAddress, AssetI? defaultResId = null);
        public UniTask<Address> LoadAssetAsync<Address>(ModI modId, string path) where Address : IAssetId;
        public TAssetId LoadAsset<TAssetId>(ModI modId, string path) where TAssetId : IAssetId;
        public TAssetId CreateAssetId<TAssetId>(ModI modId, string path) where TAssetId : IAssetId;

        /// <summary>
        /// 通过AssetI创建XAssetHandle（异步，如果资源未加载会自动加载）
        /// 使用引用计数机制，每次创建引用计数+1
        /// </summary>
        public UniTask<XAssetHandle> CreateAssetHandleAsync(AssetI xAssetId);

        public TAsset GetAssetByAddress<TAsset, TAddress>( TAddress address)
            where TAddress : IAssetAddress where TAsset : IAssetId;

        public TTAddress UpdateAssetAddress<TAsset, TTAddress>(TTAddress address, TAsset asset)
            where TTAddress : IAssetAddress where TAsset : IAssetId;

        /// <summary>
        /// 释放资源句柄（引用计数-1，当引用计数为0时加入待回收队列）
        /// </summary>
        public void ReleaseAssetHandle(XAssetHandle handle);
        
        /// <summary>
        /// 通过AssetId获取资源对象（内部使用，外部应通过XAssetHandle获取）
        /// </summary>
        public T GetAssetObject<T>(AssetI xAsset) where T : UnityEngine.Object;

        AssetI GetAsstIdByModIdAndPath(ModI modId, string path);
    }

}