using System;
using XM;
using XM.Contracts.Config;
using Unity.Collections;

namespace XM.ConfigNew.Tests.Data
{
    // UltimateComplexConfigUnmanaged 的部分类 - FullIndex索引
    public partial struct UltimateComplexConfigUnmanaged
    {
        /// <summary>
        /// FullIndex 索引结构体
        /// </summary>
        public struct FullIndexIndex : IConfigIndexGroup<global::XM.ConfigNew.Tests.Data.UltimateComplexConfigUnmanaged>, IEquatable<FullIndexIndex>
        {
            // 索引字段
            /// <summary>索引字段: Category</summary>
            public global::Unity.Collections.FixedString32Bytes Category;
            /// <summary>索引字段: SubType</summary>
            public int SubType;
            /// <summary>索引字段: Level</summary>
            public int Level;

            /// <summary>
            /// 构造方法
            /// </summary>
            /// <param name="category">Category值</param>
            /// <param name="subtype">SubType值</param>
            /// <param name="level">Level值</param>
            public FullIndexIndex(global::Unity.Collections.FixedString32Bytes category, int subtype, int level)
            {
                this.Category = category;
                this.SubType = subtype;
                this.Level = level;
            }

            /// <summary>
            /// 判断索引是否相等
            /// </summary>
            public bool Equals(FullIndexIndex other)
            {
                return this.Category == other.Category && this.SubType == other.SubType && this.Level == other.Level;
            }

            /// <summary>
            /// 获取哈希码
            /// </summary>
            public int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 31 + Category.GetHashCode();
                    hash = hash * 31 + SubType.GetHashCode();
                    hash = hash * 31 + Level.GetHashCode();
                    return hash;
                }
            }

        }
    }

    /// <summary>
    /// FullIndex 索引扩展方法
    /// </summary>
    public static class UltimateComplexConfigUnmanaged_FullIndexIndexExtensions
    {
        /// <summary>
        /// 获取索引对应的配置数据(唯一索引)
        /// </summary>
        /// <param name="index">索引值</param>
        /// <param name="returns">配置数据</param>
        public static global::XM.ConfigNew.Tests.Data.UltimateComplexConfigUnmanaged GetVal(this UltimateComplexConfigUnmanaged.FullIndexIndex index)
        {
            // TODO: 实现索引查询逻辑
            return default(global::XM.ConfigNew.Tests.Data.UltimateComplexConfigUnmanaged);
        }
    }
}
