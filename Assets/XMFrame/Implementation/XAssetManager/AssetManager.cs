using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using XMFrame.Interfaces;
using XMFrame.Utils;
using YooAsset;

namespace XMFrame.Implementation
{
    /// <summary>
    /// 资源管理器实现（基于YooAsset）
    /// </summary>
    [AutoCreate]
    [ManagerDependency(typeof(IPoolManager))]
    public class AssetManager : ManagerBase<IAssetManager>, IAssetManager
    {
        #region 内部类型定义

        /// <summary>
        /// 资源包信息
        /// </summary>
        private class ResPackageInfo
        {
            public ModHandle ModId { get; set; }
            public string PackageName { get; set; }
            public string Path { get; set; }
            public ResourcePackage ResourcePackage { get; set; }
            public bool IsInitialized { get; set; }
        }

        /// <summary>
        /// 资源地址信息
        /// </summary>
        private class AssetAddressInfo
        {
            public int AddressId { get; set; }
            public string ResAddress { get; set; }
            public XAssetId? DefaultAssetId { get; set; }
            public XAssetId? CurrentAssetId { get; set; }
        }

        /// <summary>
        /// 已加载的资源信息
        /// </summary>
        private class LoadedAssetInfo
        {
            public XAssetId XAssetId { get; set; }
            public YooAsset.AssetHandle YooAssetHandle { get; set; }
            public UnityEngine.Object Asset { get; set; }

            // 引用计数（记录有多少个XAssetHandle引用此资源）
            public int RefCount { get; set; }
        }

        #endregion

        #region 私有字段

        // ModId -> ResPackageInfo 映射
        private Dictionary<ModHandle, ResPackageInfo> _resPackages = new Dictionary<ModHandle, ResPackageInfo>();

        // AddressId -> AssetAddressInfo 映射
        private Dictionary<int, AssetAddressInfo> _assetAddresses = new Dictionary<int, AssetAddressInfo>();
        private int _nextAddressId = 1;

        // AssetId -> LoadedAssetInfo 映射
        private Dictionary<XAssetId, LoadedAssetInfo> _loadedAssets = new Dictionary<XAssetId, LoadedAssetInfo>();

        // 资源路径 -> AssetId 映射（用于快速查找）
        private Dictionary<string, Dictionary<ModHandle, XAssetId>> _pathToAssetId =
            new Dictionary<string, Dictionary<ModHandle, XAssetId>>();

        // AssetId -> (ModId, Path) 映射（用于通过AssetId查找路径信息，用于延迟加载）
        private Dictionary<XAssetId, (ModHandle modId, string path)> _assetIdToPathInfo =
            new Dictionary<XAssetId, (ModHandle, string)>();

        // 下一个资源ID（全局计数器）
        private int _nextAssetId = 1;

        // 待回收的资源集合（用于定期回收，使用HashSet提高查找效率）
        private HashSet<XAssetId> _assetsToRecycle = new HashSet<XAssetId>();

        // 回收协程的取消令牌
        private CancellationTokenSource _recycleCancellationTokenSource;

        #endregion

        #region 公共 API - 资源包管理

        /// <summary>
        /// 创建资源包 时用modId注册 使用 modName为包名 
        /// </summary>
        public async UniTask<bool> CreateResPackage(ModHandle modId, string modName, string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                XLog.ErrorFormat("资源包路径不能为空，ModId: {0}", modId.ModId);
                return false;
            }

            if (_resPackages.ContainsKey(modId))
            {
                XLog.WarningFormat("资源包已存在，ModId: {0}, Path: {1}", modId.ModId, path);
                return false;
            }

            // 检查路径是否存在
            if (!File.Exists(path) && !Directory.Exists(path))
            {
                XLog.ErrorFormat("资源包路径不存在，ModId: {0}, Path: {1}", modId.ModId, path);
                return false;
            }

            try
            {
                // 检查modName是否为空
                if (string.IsNullOrEmpty(modName))
                {
                    XLog.ErrorFormat("资源包名称不能为空，ModId: {0}", modId.ModId);
                    return false;
                }

                // 创建资源包名称（使用modName作为包名）
                string packageName = modName;

                // 创建YooAsset资源包
                var resourcePackage = YooAssets.CreatePackage(packageName);


                var resPackageInfo = new ResPackageInfo
                {
                    ModId = modId,
                    PackageName = packageName,
                    Path = path,
                    ResourcePackage = resourcePackage,
                    IsInitialized = false
                };

                // 初始化并加载资源包
                await InitializeResourcePackageAsync(resPackageInfo);

                // 检查初始化是否成功
                if (!resPackageInfo.IsInitialized)
                {
                    XLog.ErrorFormat("资源包初始化失败，ModId: {0}, PackageName: {1}, Path: {2}", modId.ModId, packageName,
                        path);
                    // 清理已创建的ResourcePackage
                    try
                    {
                        YooAssets.RemovePackage(resPackageInfo.ResourcePackage);
                    }
                    catch (Exception ex)
                    {
                        XLog.WarningFormat("清理失败的资源包时出错，ModId: {0}, 错误: {1}", modId.ModId, ex.Message);
                    }

                    return false;
                }

                _resPackages[modId] = resPackageInfo;
                XLog.InfoFormat("成功创建并加载资源包，ModId: {0}, PackageName: {1}, Path: {2}", modId.ModId, packageName, path);
                return true;
            }
            catch (Exception ex)
            {
                XLog.ErrorFormat("创建资源包失败，ModId: {0}, Path: {1}, 错误: {2}", modId.ModId, path, ex.Message);
                // 如果已经创建了ResourcePackage，需要清理
                if (_resPackages.TryGetValue(modId, out var failedPackageInfo))
                {
                    try
                    {
                        YooAssets.RemovePackage(failedPackageInfo.ResourcePackage);
                    }
                    catch (Exception cleanupEx)
                    {
                        XLog.WarningFormat("清理失败的资源包时出错，ModId: {0}, 错误: {1}", modId.ModId, cleanupEx.Message);
                    }

                    _resPackages.Remove(modId);
                }

                return false;
            }
        }

        #endregion

        #region 公共 API - 资源ID管理

        /// <summary>
        /// 创建并注册资源ID（不加载资源）
        /// </summary>
        public TAssetId CreateAssetId<TAssetId>(ModHandle modId, string path) where TAssetId : IAssetId
        {
            if (string.IsNullOrEmpty(path))
            {
                XLog.ErrorFormat("资源路径不能为空，ModId: {0}", modId.ModId);
                return default(TAssetId);
            }

            try
            {
                // 检查资源包是否存在
                if (!_resPackages.TryGetValue(modId, out var resPackageInfo))
                {
                    XLog.ErrorFormat("资源包不存在，ModId: {0}", modId.ModId);
                    return default(TAssetId);
                }

                // 检查是否已经存在该路径的 AssetId
                XAssetId xAssetIdStruct;
                if (_pathToAssetId.TryGetValue(path, out var modIdToAssetId) &&
                    modIdToAssetId.TryGetValue(modId, out var existingAssetId))
                {
                    // 已存在，直接返回现有的AssetId
                    xAssetIdStruct = existingAssetId;
                    XLog.DebugFormat("AssetId已存在，直接返回，ModId: {0}, Path: {1}, AssetId: {2}",
                        modId.ModId, path, xAssetIdStruct.Id);
                }
                else
                {
                    // 创建新的AssetId并注册
                    int assetId = _nextAssetId++;
                    xAssetIdStruct = new XAssetId(modId, assetId);

                    // 建立路径映射
                    if (!_pathToAssetId.ContainsKey(path))
                    {
                        _pathToAssetId[path] = new Dictionary<ModHandle, XAssetId>();
                    }
                    _pathToAssetId[path][modId] = xAssetIdStruct;

                    // 建立AssetId到路径信息的映射（用于延迟加载）
                    _assetIdToPathInfo[xAssetIdStruct] = (modId, path);

                    XLog.InfoFormat("成功创建并注册AssetId，ModId: {0}, Path: {1}, AssetId: {2}", 
                        modId.ModId, path, assetId);
                }

                // 转换为TAssetId类型
                if (typeof(TAssetId) == typeof(XAssetId))
                {
                    return (TAssetId)(object)xAssetIdStruct;
                }

                return default(TAssetId);
            }
            catch (Exception ex)
            {
                XLog.ErrorFormat("创建AssetId异常，ModId: {0}, Path: {1}, 错误: {2}", modId.ModId, path, ex.Message);
                return default(TAssetId);
            }
        }

        /// <summary>
        /// 通过ModId和Path获取AssetId
        /// </summary>
        public XAssetId GetAsstIdByModIdAndPath(ModHandle modId, string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return default;
            }

            if (!_resPackages.TryGetValue(modId, out var resPackageInfo) || resPackageInfo.IsInitialized == false)
            {
                return default;
            }

            // 查找已存在的AssetId
            if (_pathToAssetId.TryGetValue(path, out var modIdToAssetId) &&
                modIdToAssetId.TryGetValue(modId, out var existingAssetId))
            {
                return existingAssetId;
            }

            return default;
        }

        #endregion

        #region 公共 API - 资源加载

        /// <summary>
        /// 异步加载资源
        /// </summary>
        public async UniTask<Address> LoadAssetAsync<Address>(ModHandle modId, string path) where Address : IAssetId
        {
            if (string.IsNullOrEmpty(path))
            {
                XLog.ErrorFormat("资源路径不能为空，ModId: {0}", modId.ModId);
                return default(Address);
            }

            try
            {
                // 先创建或获取AssetId
                var xAssetId = CreateAssetId<XAssetId>(modId, path);
                if (xAssetId.Id == 0)
                {
                    return default(Address);
                }

                // 检查资源包是否存在
                if (!_resPackages.TryGetValue(modId, out var resPackageInfo))
                {
                    XLog.ErrorFormat("资源包不存在，ModId: {0}", modId.ModId);
                    return default(Address);
                }

                // 初始化资源包（如果尚未初始化）
                if (!resPackageInfo.IsInitialized)
                {
                    await InitializeResourcePackageAsync(resPackageInfo);
                    if (!resPackageInfo.IsInitialized)
                    {
                        XLog.ErrorFormat("资源包初始化失败，ModId: {0}", modId.ModId);
                        return default(Address);
                    }
                }

                // 检查资源是否已经加载过（避免重复加载）
                if (!_loadedAssets.ContainsKey(xAssetId))
                {
                    // 资源未加载，执行加载流程
                    var loadedAssetId = await LoadAssetInternalAsync(resPackageInfo, modId, path, xAssetId);
                    if (loadedAssetId.Id == 0)
                    {
                        return default(Address);
                    }
                    xAssetId = loadedAssetId;
                }
                else
                {
                    XLog.DebugFormat("资源已加载，直接返回现有AssetId，ModId: {0}, Path: {1}, AssetId: {2}",
                        modId.ModId, path, xAssetId.Id);
                }

                // 转换为Address类型
                if (typeof(Address) == typeof(XAssetId))
                {
                    return (Address)(object)xAssetId;
                }

                return default(Address);
            }
            catch (Exception ex)
            {
                XLog.ErrorFormat("异步加载资源异常，ModId: {0}, Path: {1}, 错误: {2}", modId.ModId, path, ex.Message);
                return default(Address);
            }
        }

        /// <summary>
        /// 同步加载资源
        /// </summary>
        public TAsset LoadAsset<TAsset>(ModHandle modId, string path) where TAsset : IAssetId
        {
            if (string.IsNullOrEmpty(path))
            {
                XLog.ErrorFormat("资源路径不能为空，ModId: {0}", modId.ModId);
                return default(TAsset);
            }

            try
            {
                // 先创建或获取AssetId
                var xAssetId = CreateAssetId<XAssetId>(modId, path);
                if (xAssetId.Id == 0)
                {
                    return default(TAsset);
                }

                // 检查资源包是否存在
                if (!_resPackages.TryGetValue(modId, out var resPackageInfo))
                {
                    XLog.ErrorFormat("资源包不存在，ModId: {0}", modId.ModId);
                    return default(TAsset);
                }

                // 初始化资源包（如果尚未初始化）
                if (!resPackageInfo.IsInitialized)
                {
                    InitializeResourcePackageSyncBlocking(resPackageInfo);
                    if (!resPackageInfo.IsInitialized)
                    {
                        XLog.ErrorFormat("资源包初始化失败，ModId: {0}", modId.ModId);
                        return default(TAsset);
                    }
                }

                // 检查资源是否已经加载过（避免重复加载）
                if (!_loadedAssets.ContainsKey(xAssetId))
                {
                    // 资源未加载，执行加载流程
                    var loadedAssetId = LoadAssetInternalSync(resPackageInfo, modId, path, xAssetId);
                    if (loadedAssetId.Id == 0)
                    {
                        return default(TAsset);
                    }
                    xAssetId = loadedAssetId;
                }
                else
                {
                    XLog.DebugFormat("资源已加载，直接返回现有AssetId，ModId: {0}, Path: {1}, AssetId: {2}",
                        modId.ModId, path, xAssetId.Id);
                }

                // 转换为Address类型
                if (typeof(TAsset) == typeof(XAssetId))
                {
                    return (TAsset)(object)xAssetId;
                }

                return default(TAsset);
            }
            catch (Exception ex)
            {
                XLog.ErrorFormat("同步加载资源异常，ModId: {0}, Path: {1}, 错误: {2}", modId.ModId, path, ex.Message);
                return default(TAsset);
            }
        }

        #endregion

        #region 公共 API - 资源地址管理

        /// <summary>
        /// 创建资源地址
        /// </summary>
        public AssetAddress CreateAssetAddress(string resAddress, XAssetId? defaultResId = null)
        {
            if (string.IsNullOrEmpty(resAddress))
            {
                XLog.Error("资源地址不能为空");
                return new AssetAddress(0);
            }

            int addressId = _nextAddressId++;
            var addressInfo = new AssetAddressInfo
            {
                AddressId = addressId,
                ResAddress = resAddress,
                DefaultAssetId = defaultResId,
                CurrentAssetId = defaultResId
            };

            _assetAddresses[addressId] = addressInfo;
            XLog.InfoFormat("成功创建资源地址，AddressId: {0}, ResAddress: {1}", addressId, resAddress);

            return new AssetAddress(addressId);
        }

        /// <summary>
        /// 通过资源地址获取资源ID
        /// </summary>
        public TAsset GetAssetByAddress<TAsset, TAddress>(TAddress address)
            where TAsset :  IAssetId
            where TAddress : IAssetAddress
        {
            int addressId = address.AddressId;
            if (addressId == 0)
            {
                return default(TAsset);
            }

            if (!_assetAddresses.TryGetValue(addressId, out var addressInfo))
            {
                return default(TAsset);
            }

            if (addressInfo.CurrentAssetId.HasValue)
            {
                var assetId = addressInfo.CurrentAssetId.Value;
                if (typeof(TAsset) == typeof(XAssetId))
                {
                    return (TAsset)(object)assetId;
                }
            }

            return default(TAsset);
        }

        /// <summary>
        /// 更新资源地址
        /// 将Address对应的资源更新为新的AssetId
        /// </summary>
        public Address UpdateAssetAddress<Asset, Address>(Address address, Asset asset)
            where Address : IAssetAddress
            where Asset : IAssetId
        {
            // 检查address是否为null（结构体可能无法直接判断）
            int addressId = address.AddressId;
            if (addressId == 0)
            {
                XLog.Error("资源地址ID无效");
                return default(Address);
            }

            var assetId = asset.GetAssetId();

            // Asset是结构体，检查Id是否有效
            if (asset.Id == 0)
            {
                XLog.Error("资源ID无效");
                return default(Address);
            }

            if (!_assetAddresses.TryGetValue(addressId, out var addressInfo))
            {
                XLog.ErrorFormat("资源地址不存在，AddressId: {0}", addressId);
                return default(Address);
            }

            // 如果之前有资源，直接更新（不使用引用计数）
            // 注意：旧资源不会自动释放，需要手动通过AssetHandle释放
            addressInfo.CurrentAssetId = assetId;

            XLog.InfoFormat("成功更新资源地址，AddressId: {0}, AssetId: {1}", addressId, assetId.Id);

            return address;
        }

        /// <summary>
        /// 通过资源地址获取资源ID
        /// </summary>
        public XAssetId? GetAssetIdByAddress(AssetAddress address)
        {
            if (address.AddressId == 0)
            {
                return null;
            }

            if (_assetAddresses.TryGetValue(address.AddressId, out var addressInfo))
            {
                return addressInfo.CurrentAssetId;
            }

            return null;
        }

        /// <summary>
        /// 通过资源地址获取资源对象
        /// </summary>
        public T GetAssetByAddress<T>(AssetAddress address) where T : UnityEngine.Object
        {
            if (address.AddressId == 0)
            {
                XLog.Warning("资源地址ID无效");
                return null;
            }

            if (!_assetAddresses.TryGetValue(address.AddressId, out var addressInfo))
            {
                XLog.WarningFormat("资源地址不存在，AddressId: {0}", address.AddressId);
                return null;
            }

            if (addressInfo.CurrentAssetId.HasValue)
            {
                return GetAssetObject<T>(addressInfo.CurrentAssetId.Value);
            }

            XLog.WarningFormat("资源地址没有关联的资源，AddressId: {0}", address.AddressId);
            return null;
        }

        #endregion

        #region 公共 API - 资源句柄管理

        /// <summary>
        /// 通过AssetId创建XAssetHandle（异步，如果资源未加载会自动加载）
        /// </summary>
        public async UniTask<XAssetHandle> CreateAssetHandleAsync(XAssetId xAssetId)
        {
            if (xAssetId.Id == 0)
            {
                return null;
            }

            // 检查资源是否已加载
            LoadedAssetInfo loadedAssetInfo;
            if (!_loadedAssets.TryGetValue(xAssetId, out loadedAssetInfo))
            {
                // 资源未加载，尝试通过AssetId查找路径信息并加载
                if (!_assetIdToPathInfo.TryGetValue(xAssetId, out var pathInfo))
                {
                    XLog.WarningFormat("未找到资源路径信息，AssetId: {0}", xAssetId.Id);
                    return null;
                }

                var modId = pathInfo.modId;
                var path = pathInfo.path;

                // 检查资源包是否存在
                if (!_resPackages.TryGetValue(modId, out var resPackageInfo))
                {
                    XLog.ErrorFormat("资源包不存在，ModId: {0}", modId.ModId);
                    return null;
                }

                // 初始化资源包（如果尚未初始化）
                if (!resPackageInfo.IsInitialized)
                {
                    await InitializeResourcePackageAsync(resPackageInfo);
                    if (!resPackageInfo.IsInitialized)
                    {
                        XLog.ErrorFormat("资源包初始化失败，无法加载资源，ModId: {0}", modId.ModId);
                        return null;
                    }
                }

                // 使用YooAsset异步加载资源
                YooAsset.AssetHandle assetHandle = null;
                UnityEngine.Object asset = null;

                try
                {
                    // 尝试加载资源
                    assetHandle = resPackageInfo.ResourcePackage.LoadAssetAsync<UnityEngine.Object>(path);

                    // 等待资源加载完成（使用YooAsset的UniTask扩展方法）
                    await assetHandle.ToUniTask();

                    if (assetHandle.IsValid && assetHandle.AssetObject != null)
                    {
                        asset = assetHandle.AssetObject;
                    }
                    else
                    {
                        XLog.ErrorFormat("加载资源失败，资源句柄无效，ModId: {0}, Path: {1}", modId.ModId, path);
                        assetHandle?.Release();
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    XLog.ErrorFormat("YooAsset加载资源异常，ModId: {0}, Path: {1}, 错误: {2}", modId.ModId, path, ex.Message);
                    assetHandle?.Release();
                    return null;
                }

                // 存储资源信息，初始引用计数为0
                loadedAssetInfo = new LoadedAssetInfo
                {
                    XAssetId = xAssetId,
                    YooAssetHandle = assetHandle,
                    Asset = asset,
                    RefCount = 0
                };
                _loadedAssets[xAssetId] = loadedAssetInfo;

                XLog.InfoFormat("延迟加载资源成功，ModId: {0}, Path: {1}, AssetId: {2}", modId.ModId, path, xAssetId.Id);
            }

            // 从对象池获取句柄
            var handle = GetHandleFromPool();
            handle.Id = xAssetId;
            
            // 引用计数+1
            loadedAssetInfo.RefCount++;
            
            // 如果资源在待回收队列中，移除它（资源被重新引用）
            if (_assetsToRecycle.Remove(xAssetId))
            {
                XLog.DebugFormat("资源被重新引用，从待回收队列中移除，AssetId: {0}", xAssetId.Id);
            }

            XLog.DebugFormat("创建资源句柄，AssetId: {0}, 引用计数: {1}", 
                xAssetId.Id, loadedAssetInfo.RefCount);

            return handle;
        }

        /// <summary>
        /// 释放资源句柄（引用计数-1）
        /// </summary>
        public  void ReleaseAssetHandle(XAssetHandle handle)
        {
            if (handle == null)
            {
                XLog.Warning("尝试释放null句柄");
                return;
            }

            var assetId = handle.Id;
            
            // 检查资源是否存在
            if (!_loadedAssets.TryGetValue(assetId, out var loadedAssetInfo))
            {
                XLog.WarningFormat("资源信息不存在，AssetId: {0}", assetId.Id);
                return;
            }

            // 引用计数-1
            loadedAssetInfo.RefCount--;
            
            XLog.DebugFormat("释放资源句柄，AssetId: {0}, 剩余引用计数: {1}", 
                assetId.Id, loadedAssetInfo.RefCount);

            // 如果引用计数降为0，加入待回收队列
            if (loadedAssetInfo.RefCount <= 0)
            {
                loadedAssetInfo.RefCount = 0; // 防止负数
                _assetsToRecycle.Add(assetId);
                XLog.DebugFormat("资源引用计数为0，加入待回收队列，AssetId: {0}", assetId.Id);
            }
            else
            {
                // 如果引用计数仍>0，确保不在待回收队列中
                _assetsToRecycle.Remove(assetId);
            }

            // 归还句柄到对象池
            ReturnHandleToPool(handle);
        }

        /// <summary>
        /// 通过AssetId获取资源对象（内部使用，外部应通过XAssetHandle获取）
        /// </summary>
        public T GetAssetObject<T>(XAssetId xAssetId) where T : UnityEngine.Object
        {
            if (xAssetId.Id == 0)
            {
                return null;
            }

            if (_loadedAssets.TryGetValue(xAssetId, out var loadedAssetInfo))
            {
                if (loadedAssetInfo.Asset != null)
                {
                    return loadedAssetInfo.Asset as T;
                }
            }
            else
            {
                // TODO 加载资源
            }

            return null;
        }

        #endregion

        #region 私有方法 - 资源加载

        /// <summary>
        /// 内部异步加载资源方法（提取公共逻辑）
        /// </summary>
        private async UniTask<XAssetId> LoadAssetInternalAsync(ResPackageInfo resPackageInfo, ModHandle modId, string path, XAssetId xAssetId)
        {
            // 使用已创建的AssetId
            var assetIdStruct = xAssetId;

            // 使用YooAsset异步加载资源
            AssetHandle assetHandle = null;
            UnityEngine.Object asset = null;

            try
            {
                // 尝试加载资源
                assetHandle = resPackageInfo.ResourcePackage.LoadAssetAsync<UnityEngine.Object>(path);

                // 等待资源加载完成（使用YooAsset的UniTask扩展方法）
                await assetHandle.ToUniTask();

                if (assetHandle.IsValid && assetHandle.AssetObject != null)
                {
                    asset = assetHandle.AssetObject;
                }
                else
                {
                    XLog.ErrorFormat("加载资源失败，资源句柄无效，ModId: {0}, Path: {1}", modId.ModId, path);
                    assetHandle?.Release();
                    return new XAssetId(assetIdStruct.ModHandle, 0); // 返回无效的AssetId
                }
            }
            catch (Exception ex)
            {
                XLog.ErrorFormat("YooAsset加载资源异常，ModId: {0}, Path: {1}, 错误: {2}", modId.ModId, path, ex.Message);
                assetHandle?.Release();
                return new XAssetId(assetIdStruct.ModHandle, 0); // 返回无效的AssetId
            }

            // 存储资源信息，初始引用计数为0
            var loadedAssetInfo = new LoadedAssetInfo
            {
                XAssetId = assetIdStruct,
                YooAssetHandle = assetHandle,
                Asset = asset,
                RefCount = 0
            };
            _loadedAssets[assetIdStruct] = loadedAssetInfo;
            
            // 如果资源在待回收Set中，移除它（资源被重新加载）
            _assetsToRecycle.Remove(assetIdStruct);

            XLog.InfoFormat("成功异步加载资源，ModId: {0}, Path: {1}, AssetId: {2}", modId.ModId, path, assetIdStruct.Id);
            return assetIdStruct;
        }

        /// <summary>
        /// 内部同步加载资源方法（提取公共逻辑）
        /// </summary>
        private XAssetId LoadAssetInternalSync(ResPackageInfo resPackageInfo, ModHandle modId, string path, XAssetId xAssetId)
        {
            // 使用已创建的AssetId
            var assetIdStruct = xAssetId;

            // 使用YooAsset同步加载资源
            AssetHandle assetHandle = null;
            UnityEngine.Object asset = null;

            try
            {
                // 同步加载资源
                assetHandle = resPackageInfo.ResourcePackage.LoadAssetSync<UnityEngine.Object>(path);

                if (assetHandle.IsValid && assetHandle.AssetObject != null)
                {
                    asset = assetHandle.AssetObject;
                }
                else
                {
                    XLog.ErrorFormat("加载资源失败，资源句柄无效，ModId: {0}, Path: {1}", modId.ModId, path);
                    assetHandle?.Release();
                    return new XAssetId(assetIdStruct.ModHandle, 0); // 返回无效的AssetId
                }
            }
            catch (Exception ex)
            {
                XLog.ErrorFormat("YooAsset加载资源异常，ModId: {0}, Path: {1}, 错误: {2}", modId.ModId, path, ex.Message);
                assetHandle?.Release();
                return new XAssetId(assetIdStruct.ModHandle, 0); // 返回无效的AssetId
            }

            // 存储资源信息，初始引用计数为0
            var loadedAssetInfo = new LoadedAssetInfo
            {
                XAssetId = assetIdStruct,
                YooAssetHandle = assetHandle,
                Asset = asset,
                RefCount = 0
            };
            _loadedAssets[assetIdStruct] = loadedAssetInfo;
            
            // 如果资源在待回收Set中，移除它（资源被重新加载）
            _assetsToRecycle.Remove(assetIdStruct);

            XLog.InfoFormat("成功同步加载资源，ModId: {0}, Path: {1}, AssetId: {2}", modId.ModId, path, assetIdStruct.Id);
            return assetIdStruct;
        }

        #endregion

        #region 私有方法 - 资源包初始化

        /// <summary>
        /// 异步初始化资源包
        /// </summary>
        private async UniTask InitializeResourcePackageAsync(ResPackageInfo resPackageInfo)
        {
            if (resPackageInfo.IsInitialized)
            {
                return;
            }

            try
            {
                // 创建初始化参数
                var initParameters = CreateInitParameters(resPackageInfo);

                // 异步初始化资源包
                var initOperation = resPackageInfo.ResourcePackage.InitializeAsync(initParameters);

                // 等待初始化完成（使用YooAsset的UniTask扩展方法）
                await initOperation.ToUniTask();

                if (initOperation.Status == EOperationStatus.Succeed)
                {
                    resPackageInfo.IsInitialized = true;
                    XLog.InfoFormat("成功初始化资源包，ModId: {0}, PackageName: {1}",
                        resPackageInfo.ModId.ModId, resPackageInfo.PackageName);
                }
                else
                {
                    XLog.ErrorFormat("初始化资源包失败，ModId: {0}, PackageName: {1}, Status: {2}",
                        resPackageInfo.ModId.ModId, resPackageInfo.PackageName, initOperation.Status);
                }
            }
            catch (Exception ex)
            {
                XLog.ErrorFormat("初始化资源包异常，ModId: {0}, PackageName: {1}, 错误: {2}",
                    resPackageInfo.ModId.ModId, resPackageInfo.PackageName, ex.Message);
                resPackageInfo.IsInitialized = false;
            }
        }

        /// <summary>
        /// 同步初始化资源包（用于同步方法中，使用轮询方式等待）
        /// 注意：在Unity主线程中调用，使用UniTask.Yield来避免阻塞
        /// </summary>
        private void InitializeResourcePackageSyncBlocking(ResPackageInfo resPackageInfo)
        {
            if (resPackageInfo.IsInitialized)
            {
                return;
            }

            try
            {
                // 创建初始化参数
                var initParameters = CreateInitParameters(resPackageInfo);

                // 初始化资源包
                var initOperation = resPackageInfo.ResourcePackage.InitializeAsync(initParameters);

                // 同步等待初始化完成（使用轮询方式，但减少Sleep时间以提高响应性）
                // 注意：在Unity中，Thread.Sleep会阻塞主线程，但这是同步方法的权衡
                // 更好的做法是使用异步方法，但为了保持API兼容性，这里使用最小延迟
                const int maxWaitTime = 30000; // 最大等待30秒
                int waitedTime = 0;
                while (!initOperation.IsDone && waitedTime < maxWaitTime)
                {
                    System.Threading.Thread.Sleep(1); // 减少到1ms以提高响应性
                    waitedTime += 1;
                }

                if (waitedTime >= maxWaitTime)
                {
                    XLog.ErrorFormat("初始化资源包超时，ModId: {0}, PackageName: {1}",
                        resPackageInfo.ModId.ModId, resPackageInfo.PackageName);
                    resPackageInfo.IsInitialized = false;
                    return;
                }

                if (initOperation.Status == EOperationStatus.Succeed)
                {
                    resPackageInfo.IsInitialized = true;
                    XLog.InfoFormat("成功初始化资源包，ModId: {0}, PackageName: {1}",
                        resPackageInfo.ModId.ModId, resPackageInfo.PackageName);
                }
                else
                {
                    XLog.ErrorFormat("初始化资源包失败，ModId: {0}, PackageName: {1}, Status: {2}",
                        resPackageInfo.ModId.ModId, resPackageInfo.PackageName, initOperation.Status);
                    resPackageInfo.IsInitialized = false;
                }
            }
            catch (Exception ex)
            {
                XLog.ErrorFormat("初始化资源包异常，ModId: {0}, PackageName: {1}, 错误: {2}",
                    resPackageInfo.ModId.ModId, resPackageInfo.PackageName, ex.Message);
                resPackageInfo.IsInitialized = false;
            }
        }

        /// <summary>
        /// 创建初始化参数
        /// </summary>
        private OfflinePlayModeParameters CreateInitParameters(ResPackageInfo resPackageInfo)
        {
            var initParameters = new OfflinePlayModeParameters();

            // 设置资源包路径
            if (!string.IsNullOrEmpty(resPackageInfo.Path))
            {
                // 如果是文件路径，使用其所在目录；如果是目录，直接使用
                string rootDirectory = Directory.Exists(resPackageInfo.Path)
                    ? resPackageInfo.Path
                    : System.IO.Path.GetDirectoryName(resPackageInfo.Path);

                if (!string.IsNullOrEmpty(rootDirectory))
                {
                    // 转换为Unity可用的路径格式（相对路径）
                    string relativePath = rootDirectory.Replace('\\', '/');
                    if (relativePath.StartsWith(Application.dataPath))
                    {
                        relativePath = "Assets" + relativePath.Substring(Application.dataPath.Length);
                    }

                    // 使用 BuildinFileSystemParameters 设置路径
                    var buildinFileSystemParams =
                        FileSystemParameters.CreateDefaultBuildinFileSystemParameters(packageRoot: relativePath);
                    initParameters.BuildinFileSystemParameters = buildinFileSystemParams;
                }
            }

            return initParameters;
        }

        #endregion

        #region 私有方法 - 资源回收

        /// <summary>
        /// 启动定期回收协程
        /// </summary>
        private async UniTaskVoid StartRecycleCoroutine(CancellationToken cancellationToken)
        {
            // 每5秒检查一次
            const float recycleIntervalSeconds = 5f;

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(recycleIntervalSeconds),
                        cancellationToken: cancellationToken);

                    // 执行回收
                    RecycleUnusedAssets();
                }
                catch (OperationCanceledException)
                {
                    // 协程被取消，正常退出
                    break;
                }
                catch (Exception ex)
                {
                    XLog.ErrorFormat("回收资源协程异常: {0}", ex.Message);
                }
            }
        }

        /// <summary>
        /// 回收未被引用的资源（引用计数为0的资源）
        /// </summary>
        private void RecycleUnusedAssets()
        {
            if (_loadedAssets.Count == 0)
            {
                return;
            }

            // 收集所有引用计数为0的资源
            _assetsToRecycle.Clear();

            foreach (var kvp in _loadedAssets)
            {
                var assetId = kvp.Key;
                var loadedAssetInfo = kvp.Value;

                // 检查引用计数是否为0
                if (loadedAssetInfo.RefCount <= 0)
                {
                    _assetsToRecycle.Add(assetId);
                }
            }

            // 回收未被引用的资源
            int recycledCount = 0;
            foreach (var assetId in _assetsToRecycle)
            {
                if (_loadedAssets.TryGetValue(assetId, out var loadedAssetInfo))
                {
                    // 释放YooAsset资源句柄
                    if (loadedAssetInfo.YooAssetHandle != null && loadedAssetInfo.YooAssetHandle.IsValid)
                    {
                        loadedAssetInfo.YooAssetHandle.Release();
                    }

                    // 获取路径信息（在删除前）
                    var pathInfo = _assetIdToPathInfo.TryGetValue(assetId, out var pi)
                        ? pi
                        : (modId: default(ModHandle), path: null);

                    // 清理资源信息
                    _loadedAssets.Remove(assetId);
                    _assetIdToPathInfo.Remove(assetId);

                    // 清理路径映射
                    if (!string.IsNullOrEmpty(pathInfo.path))
                    {
                        if (_pathToAssetId.TryGetValue(pathInfo.path, out var modIdToAssetId))
                        {
                            modIdToAssetId.Remove(pathInfo.modId);
                            if (modIdToAssetId.Count == 0)
                            {
                                _pathToAssetId.Remove(pathInfo.path);
                            }
                        }
                    }

                    recycledCount++;
                }
            }

            if (recycledCount > 0)
            {
                XLog.InfoFormat("定期回收完成，回收了 {0} 个引用计数为0的资源", recycledCount);
            }

            _assetsToRecycle.Clear();
        }

        #endregion

        #region 私有方法 - 对象池

        /// <summary>
        /// 从对象池获取或创建XAssetHandle
        /// </summary>
        private XAssetHandle GetHandleFromPool()
        {
            return XAssetHandle.GetFromPool();
        }

        /// <summary>
        /// 将XAssetHandle归还到对象池
        /// </summary>
        private void ReturnHandleToPool(XAssetHandle handle)
        {
            if (handle == null) return;
            XAssetHandle.ReleaseToPool(handle);
        }

        #endregion

        #region 生命周期方法

        public override UniTask OnCreate()
        {
            XLog.Info("AssetManager OnCreate");
            
            // 初始化 XAssetHandle 对象池
            XAssetHandle.InitializePool();
            
            return UniTask.CompletedTask;
        }

        public override UniTask OnInit()
        {
            XLog.Info("AssetManager OnInit");

            // 启动定期回收协程
            _recycleCancellationTokenSource = new CancellationTokenSource();
            StartRecycleCoroutine(_recycleCancellationTokenSource.Token).Forget();

            return UniTask.CompletedTask;
        }

        public override async UniTask OnDestroy()
        {
            // 停止回收协程
            _recycleCancellationTokenSource?.Cancel();
            _recycleCancellationTokenSource?.Dispose();
            _recycleCancellationTokenSource = null;

            // 释放所有资源
            foreach (var loadedAsset in _loadedAssets.Values)
            {
                if (loadedAsset.YooAssetHandle != null && loadedAsset.YooAssetHandle.IsValid)
                {
                    loadedAsset.YooAssetHandle.Release();
                }
            }

            _loadedAssets.Clear();

            // 卸载所有资源包（等待异步操作完成）
            var unloadTasks = new List<UniTask>();
            foreach (var resPackageInfo in _resPackages.Values)
            {
                if (resPackageInfo.ResourcePackage != null)
                {
                    try
                    {
                        var unloadOperation = resPackageInfo.ResourcePackage.UnloadUnusedAssetsAsync();
                        if (unloadOperation != null)
                        {
                            unloadTasks.Add(unloadOperation.ToUniTask());
                        }
                    }
                    catch (Exception ex)
                    {
                        XLog.WarningFormat("卸载资源包时出错，ModId: {0}, 错误: {1}",
                            resPackageInfo.ModId.ModId, ex.Message);
                    }
                }
            }

            // 等待所有卸载操作完成（最多等待5秒）
            if (unloadTasks.Count > 0)
            {
                try
                {
                    await UniTask.WhenAll(unloadTasks).Timeout(TimeSpan.FromSeconds(5));
                }
                catch (TimeoutException)
                {
                    XLog.Warning("卸载资源包超时，继续执行清理");
                }
                catch (Exception ex)
                {
                    XLog.WarningFormat("等待卸载资源包时出错: {0}", ex.Message);
                }
            }

            _resPackages.Clear();

            _assetAddresses.Clear();
            _pathToAssetId.Clear();
            _assetIdToPathInfo.Clear();
            _assetsToRecycle.Clear();
            
            // 清理 XAssetHandle 对象池
            XAssetHandle.CleanupPool();
            
            _nextAssetId = 1;

            XLog.Info("AssetManager OnDestroy");
        }

        #endregion
    }
}
