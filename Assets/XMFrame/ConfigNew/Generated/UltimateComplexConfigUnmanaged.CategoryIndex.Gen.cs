using System;
using XM;
using XM.Contracts.Config;
using Unity.Collections;

namespace XM.ConfigNew.Tests.Data
{
    // UltimateComplexConfigUnmanaged 的部分类 - CategoryIndex索引
    public partial struct UltimateComplexConfigUnmanaged
    {
        /// <summary>
        /// CategoryIndex 索引结构体
        /// </summary>
        public struct CategoryIndexIndex : IConfigIndexGroup<global::XM.ConfigNew.Tests.Data.UltimateComplexConfigUnmanaged>, IEquatable<CategoryIndexIndex>
        {
            // 索引字段
            /// <summary>索引字段: Category</summary>
            public global::Unity.Collections.FixedString32Bytes Category;

            /// <summary>
            /// 构造方法
            /// </summary>
            /// <param name="category">Category值</param>
            public CategoryIndexIndex(global::Unity.Collections.FixedString32Bytes category)
            {
                this.Category = category;
            }

            /// <summary>
            /// 判断索引是否相等
            /// </summary>
            public bool Equals(CategoryIndexIndex other)
            {
                return this.Category == other.Category;
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
                    return hash;
                }
            }

        }
    }

    /// <summary>
    /// CategoryIndex 索引扩展方法
    /// </summary>
    public static class UltimateComplexConfigUnmanaged_CategoryIndexIndexExtensions
    {
        /// <summary>
        /// 获取索引对应的配置数据(唯一索引)
        /// </summary>
        /// <param name="index">索引值</param>
        /// <param name="returns">配置数据</param>
        public static global::XM.ConfigNew.Tests.Data.UltimateComplexConfigUnmanaged GetVal(this UltimateComplexConfigUnmanaged.CategoryIndexIndex index)
        {
            // TODO: 实现索引查询逻辑
            return default(global::XM.ConfigNew.Tests.Data.UltimateComplexConfigUnmanaged);
        }
    }
}
