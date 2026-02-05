using System;
using XM;
using XM.Contracts.Config;
using Unity.Collections;

namespace XM.ConfigNew.Tests.Data
{
    // ComplexItemConfigUnmanaged 的部分类 - TypeIndex索引
    public partial struct ComplexItemConfigUnmanaged
    {
        /// <summary>
        /// TypeIndex 索引结构体
        /// </summary>
        public struct TypeIndexIndex : IConfigIndexGroup<global::XM.ConfigNew.Tests.Data.ComplexItemConfigUnmanaged>, IEquatable<TypeIndexIndex>
        {
            // 索引字段
            /// <summary>索引字段: ItemType</summary>
            public global::XM.ConfigNew.Tests.Data.EItemType ItemType;

            /// <summary>
            /// 构造方法
            /// </summary>
            /// <param name="itemtype">ItemType值</param>
            public TypeIndexIndex(global::XM.ConfigNew.Tests.Data.EItemType itemtype)
            {
                this.ItemType = itemtype;
            }

            /// <summary>
            /// 判断索引是否相等
            /// </summary>
            public bool Equals(TypeIndexIndex other)
            {
                return this.ItemType == other.ItemType;
            }

            /// <summary>
            /// 获取哈希码
            /// </summary>
            public int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 31 + ItemType.GetHashCode();
                    return hash;
                }
            }

        }
    }

    /// <summary>
    /// TypeIndex 索引扩展方法
    /// </summary>
    public static class ComplexItemConfigUnmanaged_TypeIndexIndexExtensions
    {
        /// <summary>
        /// 获取索引对应的配置数据列表(多值索引)
        /// </summary>
        /// <param name="index">索引值</param>
        /// <param name="allocator">内存分配器</param>
        /// <param name="returns">配置数据数组</param>
        public static NativeArray<global::XM.ConfigNew.Tests.Data.ComplexItemConfigUnmanaged> GetVals(this ComplexItemConfigUnmanaged.TypeIndexIndex index, Allocator allocator)
        {
            // TODO: 实现多值索引查询逻辑
            return new NativeArray<global::XM.ConfigNew.Tests.Data.ComplexItemConfigUnmanaged>(0, allocator);
        }
    }
}
