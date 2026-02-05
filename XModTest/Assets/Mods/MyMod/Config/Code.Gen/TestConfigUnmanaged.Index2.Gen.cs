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
        public int GetHashCode()
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
    /// 获取索引对应的配置数据列表(多值索引)
    /// </summary>
    /// <param name="index">索引值</param>
    /// <param name="allocator">内存分配器</param>
    /// <param name="returns">配置数据数组</param>
    public static NativeArray<TestConfigUnmanaged> GetVals(this TestConfigUnmanaged.Index2Index index, Allocator allocator)
    {
        // TODO: 实现多值索引查询逻辑
        return new NativeArray<TestConfigUnmanaged>(0, allocator);
    }
}
