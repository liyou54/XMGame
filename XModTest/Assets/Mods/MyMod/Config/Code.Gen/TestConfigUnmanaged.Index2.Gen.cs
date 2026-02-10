using System;
using XM;
using XM.Contracts.Config;
using Unity.Collections;

// TestConfigUnmanaged 的部分类 - Index2索引
public partial struct TestConfigUnmanaged
{
    /// <summary>
    /// Index2 索引结构体
    /// </summary>
    public struct Index2Index : IConfigIndexGroup<TestConfigUnmanaged>, IEquatable<Index2Index>
    {
        private static IndexType? indexType;

        /// <summary>
        /// 索引类型标识（实现 IConfigIndexGroup 接口要求）
        /// </summary>
        public static IndexType IndexType
        {
            get
            {
                if (indexType == null)
                {
                    indexType = new IndexType
                    {
                        Tbl = TestConfigClassHelper.TblI,
                        Index = 1
                    };
                }

                return indexType.Value;
            }
        }

        // 索引字段
        /// <summary>索引字段: TestIndex3</summary>
        public CfgI<TestConfigUnmanaged> TestIndex3;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="testindex3">TestIndex3值</param>
        public Index2Index(CfgI<TestConfigUnmanaged> testindex3)
        {
            this.TestIndex3 = testindex3;
        }

        /// <summary>
        /// 判断索引是否相等
        /// </summary>
        public bool Equals(Index2Index other)
        {
            return this.TestIndex3 == other.TestIndex3;
        }

        /// <summary>
        /// 获取哈希码
        /// </summary>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + TestIndex3.GetHashCode();
                return hash;
            }
        }

    }
}

/// <summary>
/// Index2 索引扩展方法
/// </summary>
public static class TestConfigUnmanaged_Index2IndexExtensions
{
    /// <summary>
    /// 获取索引对应的配置引用列表(多值索引)
    /// </summary>
    /// <param name="index">索引值</param>
    /// <param name="data">配置数据容器</param>
    /// <param name="allocator">内存分配器</param>
    /// <param name="returns">配置引用 CfgI 数组</param>
    public static NativeArray<CfgI<TestConfigUnmanaged>> GetVals(this TestConfigUnmanaged.Index2Index index, in XM.ConfigData data, Allocator allocator)
    {
        // 从 ConfigData 获取多值索引容器
        var indexMultiMap = data.GetMultiIndex<TestConfigUnmanaged.Index2Index, TestConfigUnmanaged>(TestConfigUnmanaged.Index2Index.IndexType);
        if (!indexMultiMap.Valid)
        {
            return new NativeArray<CfgI<TestConfigUnmanaged>>(0, allocator);
        }

        // 查询索引获取数量
        var count = indexMultiMap.GetValueCount(data.BlobContainer, index);
        if (count == 0)
        {
            return new NativeArray<CfgI<TestConfigUnmanaged>>(0, allocator);
        }

        // 遍历并转换为泛型 CfgI 数组
        var results = new NativeArray<CfgI<TestConfigUnmanaged>>(count, allocator);
        var i = 0;
        foreach (var cfgI in indexMultiMap.GetValuesPerKeyEnumerator(data.BlobContainer, index))
        {
            results[i++] = cfgI.As<TestConfigUnmanaged>();
        }

        return results;
    }
}
