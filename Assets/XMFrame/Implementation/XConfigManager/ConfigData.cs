using System;
using Unity.Collections;

namespace XMFrame
{
    /// <summary>
    /// 配置数据容器，使用 XBlob 存储配置数据
    /// 内部使用 NativeHashMap 管理不同表类型的 XBlobMap 指针
    /// </summary>
    public struct ConfigData:IDisposable
    {
        /// <summary>
        /// XBlob 容器，用于存储所有配置数据
        /// </summary>
        XBlobContainer BlobContainer;
        
        /// <summary>
        /// 表类型到 XBlobMap 指针的映射
        /// 每个表实际上存储的是 XBlobMap&lt;CfgId, ConfigType&gt; 的外观
        /// </summary>
        private NativeHashMap<TableHandle, XBlobPtr> TypeBlobMap;

        /// <summary>
        /// 创建配置数据容器
        /// </summary>
        /// <param name="allocator">内存分配器</param>
        /// <param name="capacity">初始容量，默认 4MB</param>
        public void Create(Allocator allocator,int capacity = 4*1024*1024)
        {
            BlobContainer = new XBlobContainer();
            BlobContainer.Create(allocator, capacity);
            TypeBlobMap  = new NativeHashMap<TableHandle, XBlobPtr>(512, allocator);
        }

        /// <summary>
        /// 释放所有资源
        /// </summary>
        public void Dispose()
        {
            BlobContainer.Dispose();
            TypeBlobMap.Dispose();
        }

        /// <summary>
        /// 检查指定表是否存在
        /// </summary>
        /// <param name="tableHandle">表句柄</param>
        /// <returns>如果表存在且有效返回 true，否则返回 false</returns>
        public bool IsTableExist(TableHandle tableHandle)
        {
           return TypeBlobMap.TryGetValue(tableHandle, out var blobHandle) && blobHandle.Valid;
        }

        /// <summary>
        /// 检查指定配置是否存在
        /// 使用外观模式 AsMapKey&lt;CfgId&gt; 进行键查询，只检查键是否存在，不访问值数据，性能更优
        /// </summary>
        /// <param name="cfgId">配置ID</param>
        /// <returns>如果配置存在返回 true，否则返回 false</returns>
        public bool IsConfigExist(CfgId cfgId)
        {
            // 首先检查表是否存在
            if (!TypeBlobMap.TryGetValue(cfgId.Table, out var blobHandle) || !blobHandle.Valid)
            {
                return false;
            }
            // 使用外观模式 AsMapKey<CfgId> 获取键映射外观，只查询键是否存在
            var mapKey = blobHandle.AsMapKey<CfgId>();
            return mapKey.HasKey(BlobContainer, cfgId);
        }
    }
}