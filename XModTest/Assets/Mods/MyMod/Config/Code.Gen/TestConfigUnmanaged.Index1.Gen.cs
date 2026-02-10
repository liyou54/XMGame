using System;
using XM;
using XM.Contracts.Config;
using Unity.Collections;

// TestConfigUnmanaged 的部分类 - Index1索引
public partial struct TestConfigUnmanaged
{
    /// <summary>
    /// Index1 索引结构体
    /// </summary>
    public struct Index1Index : IConfigIndexGroup<TestConfigUnmanaged>, IEquatable<Index1Index>
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
                        Index = 0
                    };
                }

                return indexType.Value;
            }
        }

        // 索引字段
        /// <summary>索引字段: TestIndex1</summary>
        public int TestIndex1;
        /// <summary>索引字段: TestIndex2</summary>
        public CfgI<TestConfigUnmanaged> TestIndex2;

        /// <summary>
        /// 构造方法
        /// </summary>
        /// <param name="testindex1">TestIndex1值</param>
        /// <param name="testindex2">TestIndex2值</param>
        public Index1Index(int testindex1, CfgI<TestConfigUnmanaged> testindex2)
        {
            this.TestIndex1 = testindex1;
            this.TestIndex2 = testindex2;
        }

        /// <summary>
        /// 判断索引是否相等
        /// </summary>
        public bool Equals(Index1Index other)
        {
            return this.TestIndex1 == other.TestIndex1 && this.TestIndex2 == other.TestIndex2;
        }

        /// <summary>
        /// 获取哈希码
        /// </summary>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + TestIndex1.GetHashCode();
                hash = hash * 31 + TestIndex2.GetHashCode();
                return hash;
            }
        }

    }
}

/// <summary>
/// Index1 索引扩展方法
/// </summary>
public static class TestConfigUnmanaged_Index1IndexExtensions
{
    /// <summary>
    /// 获取索引对应的配置引用(唯一索引)
    /// </summary>
    /// <param name="index">索引值</param>
    /// <param name="data">配置数据容器</param>
    /// <param name="returns">配置引用 CfgI</param>
    public static CfgI<TestConfigUnmanaged> GetVal(this TestConfigUnmanaged.Index1Index index, in XM.ConfigData data)
    {
        // 从 ConfigData 获取索引容器
        var indexMap = data.GetIndex<TestConfigUnmanaged.Index1Index, TestConfigUnmanaged>(TestConfigUnmanaged.Index1Index.IndexType);
        if (!indexMap.Valid)
        {
            return default(CfgI<TestConfigUnmanaged>);
        }

        // 查询索引获取 CfgI 并转换为泛型类型
        if (!indexMap.TryGetValue(data.BlobContainer, index, out var cfgI))
        {
            return default(CfgI<TestConfigUnmanaged>);
        }

        return cfgI.As<TestConfigUnmanaged>();
    }
}
