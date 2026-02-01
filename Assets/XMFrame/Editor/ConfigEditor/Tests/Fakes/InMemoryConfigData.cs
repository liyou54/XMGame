using System.Collections.Generic;
using XM.Contracts.Config;

namespace XM.Editor.Tests.Fakes
{
    /// <summary>
    /// 内存版ConfigData，无需Blob分配，测试友好
    /// 特点：
    /// 1. 使用内存字典存储数据
    /// 2. 无需Unity Blob API
    /// 3. 简化了内存管理逻辑
    /// </summary>
    public class InMemoryConfigData
    {
        private readonly Dictionary<(TblI table, CfgI config), byte[]> _data = 
            new Dictionary<(TblI, CfgI), byte[]>();
        
        private readonly Dictionary<TblI, int> _tableConfigCounts = 
            new Dictionary<TblI, int>();
        
        /// <summary>
        /// 设置配置数据
        /// </summary>
        public void Set(TblI table, CfgI config, byte[] data)
        {
            _data[(table, config)] = data;
            
            if (!_tableConfigCounts.ContainsKey(table))
                _tableConfigCounts[table] = 0;
            _tableConfigCounts[table]++;
        }
        
        /// <summary>
        /// 获取配置数据
        /// </summary>
        public byte[] Get(TblI table, CfgI config)
        {
            return _data.TryGetValue((table, config), out var data) ? data : null;
        }
        
        /// <summary>
        /// 检查配置是否存在
        /// </summary>
        public bool Contains(TblI table, CfgI config)
        {
            return _data.ContainsKey((table, config));
        }
        
        /// <summary>
        /// 获取表中的配置数量
        /// </summary>
        public int GetConfigCount(TblI table)
        {
            return _tableConfigCounts.TryGetValue(table, out var count) ? count : 0;
        }
        
        /// <summary>
        /// 清空所有数据
        /// </summary>
        public void Clear()
        {
            _data.Clear();
            _tableConfigCounts.Clear();
        }
        
        /// <summary>
        /// 获取总配置数量
        /// </summary>
        public int TotalCount => _data.Count;
    }
}
