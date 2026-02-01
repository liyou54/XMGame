using System;
using Unity.Collections;

namespace XM
{
    /// <summary>
    /// 配置数据容器，使用 XBlob 存储配置数据。
    /// 内部使用 NativeHashMap 管理不同表类型的 XBlobMap 指针，支持泛型、无反射的 Unmanaged 表操作。
    /// </summary>
    public struct ConfigData : IDisposable
    {
        #region 私有字段

        /// <summary>XBlob 容器，用于存储所有配置的二进制数据</summary>
        private XBlobContainer _blobContainer;

        /// <summary>表句柄到 XBlobMap 指针的映射。每个表对应 XBlobMap&lt;CfgI, TUnmanaged&gt; 的外观</summary>
        private NativeHashMap<TblI, XBlobPtr> _typeBlobMap;

        #endregion

        #region 创建与分配

        /// <summary>
        /// 创建配置数据容器。
        /// </summary>
        /// <remarks>主要步骤：1. 创建并初始化 Blob 容器；2. 创建表句柄到 Blob 指针的 NativeHashMap。</remarks>
        /// <param name="allocator">内存分配器（通常为 Allocator.Persistent）</param>
        /// <param name="capacity">Blob 初始容量（字节），默认 4MB</param>
        public void Create(Allocator allocator, int capacity = 4 * 1024 * 1024)
        {
            // 创建 Blob 容器并分配初始容量
            _blobContainer = new XBlobContainer();
            _blobContainer.Create(allocator, capacity);
            // 创建表句柄 -> XBlobPtr 的映射表，预留 512 个表槽位
            _typeBlobMap = new NativeHashMap<TblI, XBlobPtr>(512, allocator);
        }

        /// <summary>
        /// 为指定表在 Unmanaged 中分配 CfgI -> TUnmanaged 的 Map（泛型、无反射）。
        /// </summary>
        /// <remarks>主要步骤：1. 校验 capacity 有效；2. 校验表尚未分配；3. 在 Blob 中分配 Map 并登记到 _typeBlobMap；4. 返回 Map 外观。</remarks>
        /// <param name="tableHandle">表句柄</param>
        /// <param name="capacity">预估条目数</param>
        /// <returns>可用的 XBlobMap 外观；容量无效或表已存在时返回 default</returns>
        public XBlobMap<CfgI, TUnmanaged> AllocTableMap<TUnmanaged>(TblI tableHandle, int capacity)
            where TUnmanaged : unmanaged, IConfigUnManaged<TUnmanaged>
        {
            // 拒绝无效容量
            if (capacity <= 0)
            {
                // TODO LogError
                return default;
            }

            // 避免同一表重复分配
            if (_typeBlobMap.ContainsKey(tableHandle))
            {
                // TODO logerror
                return default;
            }

            // 在 Blob 中分配 Map 并登记，返回可读写的外观
            var blobPtr = XBlobPtr.AllocMapFrom<CfgI, TUnmanaged>(_blobContainer, capacity);
            _typeBlobMap[tableHandle] = blobPtr;
            return blobPtr.AsMap<CfgI, TUnmanaged>();
        }

        #endregion

        #region 行操作

        /// <summary>
        /// 仅插入主键，值为 default(TUnmanaged)（泛型、无反射）。
        /// </summary>
        /// <remarks>主要步骤：1. 按表句柄取 Blob 指针；2. 转为 Map 外观并写入 default 值。</remarks>
        /// <param name="tableHandle">表句柄</param>
        /// <param name="cfgId">配置 ID</param>
        public void AddPrimaryKeyOnly<TUnmanaged>(TblI tableHandle, CfgI cfgId)
            where TUnmanaged : unmanaged, IConfigUnManaged<TUnmanaged>
        {
            // 表不存在或 Blob 无效则直接返回
            if (!_typeBlobMap.TryGetValue(tableHandle, out var blobPtr) || !blobPtr.Valid)
                return;
            // 取得 Map 外观并插入/更新该键，值为 default
            var map = blobPtr.AsMap<CfgI, TUnmanaged>();
            map.AddOrUpdate(_blobContainer, cfgId, default);
        }

        /// <summary>
        /// 写入整行 TUnmanaged（泛型、无反射）。
        /// </summary>
        /// <remarks>主要步骤：1. 按表句柄取 Blob 指针；2. 转为 Map 外观并写入整行值。</remarks>
        /// <param name="tableHandle">表句柄</param>
        /// <param name="cfgId">配置 ID</param>
        /// <param name="value">Unmanaged 值</param>
        public void AddOrUpdateRow<TUnmanaged>(TblI tableHandle, CfgI cfgId, TUnmanaged value)
            where TUnmanaged : unmanaged, IConfigUnManaged<TUnmanaged>
        {
            // 表不存在或 Blob 无效则直接返回
            if (!_typeBlobMap.TryGetValue(tableHandle, out var blobPtr) || !blobPtr.Valid)
                return;
            // 取得 Map 外观并插入/更新该键的整行数据
            var map = blobPtr.AsMap<CfgI, TUnmanaged>();
            map.AddOrUpdate(_blobContainer, cfgId, value);
        }

        #endregion

        #region 查询

        /// <summary>
        /// 检查指定表是否存在。
        /// </summary>
        /// <remarks>主要步骤：查表句柄映射并校验对应 Blob 有效。</remarks>
        /// <param name="tableHandle">表句柄</param>
        /// <returns>表存在且 Blob 有效时返回 true</returns>
        public bool IsTableExist(TblI tableHandle)
        {
            // 能取到指针且指针有效即认为表存在
            return _typeBlobMap.TryGetValue(tableHandle, out var blobHandle) && blobHandle.Valid;
        }

        /// <summary>
        /// 检查指定配置是否存在。
        /// 使用 AsMapKey&lt;CfgI&gt; 仅查询键，不访问值，性能更优。
        /// </summary>
        /// <remarks>主要步骤：1. 按 cfgId.Table 取 Blob；2. 用 AsMapKey 仅查键是否存在。</remarks>
        /// <param name="cfgId">配置 ID（含 Table）</param>
        /// <returns>配置存在返回 true</returns>
        public bool IsConfigExist(CfgI cfgId)
        {
            // 表不存在或 Blob 无效则配置不存在
            if (!_typeBlobMap.TryGetValue(cfgId.Table, out var blobHandle) || !blobHandle.Valid)
                return false;

            // 使用键外观只查键，不读值，减少访问
            var mapKey = blobHandle.AsMapKey<CfgI>();
            return mapKey.HasKey(_blobContainer, cfgId);
        }

        #endregion

        #region 释放

        /// <summary>
        /// 释放 Blob 与表映射占用的 Native 内存。
        /// </summary>
        /// <remarks>主要步骤：1. 释放 Blob 容器；2. 释放表句柄映射表。</remarks>
        public void Dispose()
        {
            // 先释放 Blob，再释放映射，避免悬空引用
            _blobContainer.Dispose();
            _typeBlobMap.Dispose();
        }

        #endregion
    }
}
