using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace XM
{
    /// <summary>
    /// 配置数据容器，使用 XBlob 存储配置数据
    /// 内部使用 NativeHashMap 管理不同表类型的 XBlobMap 指针
    /// </summary>
    public struct ConfigData : IDisposable
    {
        /// <summary>
        /// XBlob 容器，用于存储所有配置数据
        /// </summary>
        public XBlobContainer BlobContainer;

        /// <summary>
        /// 表类型到 XBlobMap 指针的映射
        /// 每个表实际上存储的是 XBlobMap&lt;CfgI, ConfigType&gt; 的外观
        /// </summary>
        private NativeHashMap<TblI, XBlobPtr> TypeBlobMap;
        
        /// <summary>
        /// 索引类型到 XBlobMap/XBlobMultiMap 指针的映射
        /// 存储索引容器的指针
        /// </summary>
        private NativeHashMap<IndexType, XBlobPtr> TypeIndexBlobMap;


        
        /// <summary>
        /// 创建配置数据容器
        /// </summary>
        /// <param name="allocator">内存分配器</param>
        /// <param name="capacity">初始容量，默认 4MB</param>
        public void Create(Allocator allocator, int capacity = 4 * 1024 * 1024)
        {
            BlobContainer = new XBlobContainer();
            BlobContainer.Create(allocator, capacity);
            TypeBlobMap = new NativeHashMap<TblI, XBlobPtr>(512, allocator);
            TypeIndexBlobMap = new NativeHashMap<IndexType, XBlobPtr>(512, allocator);
        }


        /// <summary>
        /// 为指定表在 unmanaged 中分配 CfgI->TUnmanaged 的 Map（泛型、无反射）。
        /// </summary>
        public XBlobMap<CfgI, TUnmanaged> AllocTableMap<TUnmanaged>(TblI tableHandle, int capacity)
            where TUnmanaged : unmanaged, IConfigUnManaged<TUnmanaged>
        {
            if (capacity <= 0)
            {
                // TODO LogError
                return default;
            }

            if (TypeBlobMap.ContainsKey(tableHandle))
            {
                // TODO logerror
                return default;
            }

            var data = XBlobPtr.AllocMapFrom<CfgI, TUnmanaged>(BlobContainer, capacity);
            TypeBlobMap[tableHandle] = data;
            return data.AsMap<CfgI, TUnmanaged>();
        }


        /// <summary>
        /// 仅插入主键，值为 default(TUnmanaged)（泛型、无反射）。
        /// </summary>
        public void AddPrimaryKeyOnly<TUnmanaged>(TblI tableHandle, CfgI cfgId)
            where TUnmanaged : unmanaged, IConfigUnManaged<TUnmanaged>
        {
            if (!TypeBlobMap.TryGetValue(tableHandle, out var blobPtr) || !blobPtr.Valid)
                return;
            var map = blobPtr.AsMap<CfgI, TUnmanaged>();
            map.AddOrUpdate(BlobContainer, cfgId, default);
        }

        /// <summary>
        /// 写入整行 TUnmanaged（泛型、无反射）。
        /// </summary>
        public void AddOrUpdateRow<TUnmanaged>(TblI tableHandle, CfgI cfgId, TUnmanaged value)
            where TUnmanaged : unmanaged, IConfigUnManaged<TUnmanaged>
        {
            if (!TypeBlobMap.TryGetValue(tableHandle, out var blobPtr) || !blobPtr.Valid)
                return;
            var map = blobPtr.AsMap<CfgI, TUnmanaged>();
            map.AddOrUpdate(BlobContainer, cfgId, value);
        }

        /// <summary>
        /// 释放所有资源
        /// </summary>
        public void Dispose()
        {
            BlobContainer.Dispose();
            TypeBlobMap.Dispose();
            TypeIndexBlobMap.Dispose();
        }

        /// <summary>
        /// 检查指定表是否存在
        /// </summary>
        /// <param name="tableHandle">表句柄</param>
        /// <returns>如果表存在且有效返回 true，否则返回 false</returns>
        public bool IsTableExist(TblI tableHandle)
        {
            return TypeBlobMap.TryGetValue(tableHandle, out var blobHandle) && blobHandle.Valid;
        }

        /// <summary>
        /// 检查指定配置是否存在
        /// 使用外观模式 AsMapKey&lt;CfgI&gt; 进行键查询，只检查键是否存在，不访问值数据，性能更优
        /// </summary>
        /// <param name="cfgId">配置ID</param>
        /// <returns>如果配置存在返回 true，否则返回 false</returns>
        public bool IsConfigExist(CfgI cfgId)
        {
            // 首先检查表是否存在
            if (!TypeBlobMap.TryGetValue(cfgId.Table, out var blobHandle) || !blobHandle.Valid)
            {
                return false;
            }

            // 使用外观模式 AsMapKey<CfgI> 获取键映射外观，只查询键是否存在
            var mapKey = blobHandle.AsMapKey<CfgI>();
            return mapKey.HasKey(BlobContainer, cfgId);
        }
        
        public  XBlobMap<T,TUnmanaged> GetMap<T, TUnmanaged>(TblI tblI) where TUnmanaged : unmanaged, IConfigUnManaged<TUnmanaged> where T : unmanaged, IEquatable<T>
        {
            // 表不存在或 Blob 无效则直接返回
            if (!TypeBlobMap.TryGetValue(tblI, out var blobPtr) || !blobPtr.Valid)
                return default;
            // 取得 Map 外观并插入/更新该键，值为 default
            return blobPtr.AsMap<T, TUnmanaged>();
        }

        /// <summary>
        /// 为指定索引分配 XBlobMap（唯一索引）
        /// </summary>
        /// <typeparam name="TK">索引键类型（索引结构体）</typeparam>
        /// <typeparam name="TV">配置数据类型</typeparam>
        /// <param name="indexType">索引类型标识</param>
        /// <param name="capacity">容量</param>
        /// <returns>索引 Map</returns>
        public XBlobMap<TK, CfgI> AllocIndex<TK, TV>(IndexType indexType, int capacity) 
            where TK : unmanaged, IConfigIndexGroup<TV>, IEquatable<TK>
            where TV : unmanaged, IConfigUnManaged<TV>
        {
            if (capacity <= 0)
            {
                UnityEngine.Debug.LogError($"[ConfigData.AllocIndex] 容量必须大于0: {capacity}");
                return default;
            }

            if (TypeIndexBlobMap.ContainsKey(indexType))
            {
                UnityEngine.Debug.LogError($"[ConfigData.AllocIndex] 索引已存在: Table={indexType.Tbl}, Index={indexType.Index}");
                return default;
            }

            // 分配 Map 并存储指针
            var data = XBlobPtr.AllocMapFrom<TK, CfgI>(BlobContainer, capacity);
            TypeIndexBlobMap[indexType] = data;
            return data.AsMap<TK, CfgI>();
        }
        
        /// <summary>
        /// 为指定索引分配 XBlobMultiMap（多值索引）
        /// </summary>
        /// <typeparam name="TK">索引键类型（索引结构体）</typeparam>
        /// <typeparam name="TV">配置数据类型</typeparam>
        /// <param name="indexType">索引类型标识</param>
        /// <param name="capacity">容量</param>
        /// <returns>索引 MultiMap</returns>
        public XBlobMultiMap<TK, CfgI> AllocMultiIndex<TK, TV>(IndexType indexType, int capacity) 
            where TK : unmanaged, IConfigIndexGroup<TV>, IEquatable<TK>
            where TV : unmanaged, IConfigUnManaged<TV>
        {
            if (capacity <= 0)
            {
                UnityEngine.Debug.LogError($"[ConfigData.AllocMultiIndex] 容量必须大于0: {capacity}");
                return default;
            }

            if (TypeIndexBlobMap.ContainsKey(indexType))
            {
                UnityEngine.Debug.LogError($"[ConfigData.AllocMultiIndex] 索引已存在: Table={indexType.Tbl}, Index={indexType.Index}");
                return default;
            }

            // 分配 MultiMap 并存储指针
            var data = XBlobPtr.AllocMultiMapFrom<TK, CfgI>(BlobContainer, capacity);
            TypeIndexBlobMap[indexType] = data;
            return data.AsMultiMap<TK, CfgI>();
        }
        
        /// <summary>
        /// 获取指定索引的 Map（唯一索引）
        /// </summary>
        /// <typeparam name="TK">索引键类型</typeparam>
        /// <typeparam name="TV">配置数据类型</typeparam>
        /// <param name="indexType">索引类型标识</param>
        /// <returns>索引 Map，如果不存在返回 default</returns>
        public XBlobMap<TK, CfgI> GetIndex<TK, TV>(IndexType indexType) 
            where TK : unmanaged, IConfigIndexGroup<TV>, IEquatable<TK>
            where TV : unmanaged, IConfigUnManaged<TV>
        {
            if (!TypeIndexBlobMap.TryGetValue(indexType, out var blobPtr) || !blobPtr.Valid)
                return default;
            return blobPtr.AsMap<TK, CfgI>();
        }
        
        /// <summary>
        /// 获取指定索引的 MultiMap（多值索引）
        /// </summary>
        /// <typeparam name="TK">索引键类型</typeparam>
        /// <typeparam name="TV">配置数据类型</typeparam>
        /// <param name="indexType">索引类型标识</param>
        /// <returns>索引 MultiMap，如果不存在返回 default</returns>
        public XBlobMultiMap<TK, CfgI> GetMultiIndex<TK, TV>(IndexType indexType) 
            where TK : unmanaged, IConfigIndexGroup<TV>, IEquatable<TK>
            where TV : unmanaged, IConfigUnManaged<TV>
        {
            if (!TypeIndexBlobMap.TryGetValue(indexType, out var blobPtr) || !blobPtr.Valid)
                return default;
            return blobPtr.AsMultiMap<TK, CfgI>();
        }
        
        /// <summary>
        /// 检查指定索引是否存在
        /// </summary>
        /// <param name="indexType">索引类型标识</param>
        /// <returns>如果索引存在且有效返回 true，否则返回 false</returns>
        public bool IsIndexExist(IndexType indexType)
        {
            return TypeIndexBlobMap.TryGetValue(indexType, out var blobPtr) && blobPtr.Valid;
        }

    }
}