using System;
using XM;
using XM.Contracts.Config;
using Unity.Collections;

namespace XM.ConfigNew.Tests.Data
{
    // UltimateComplexConfigUnmanaged 的部分类 - LevelIndex索引
    public partial struct UltimateComplexConfigUnmanaged
    {
        /// <summary>
        /// LevelIndex 索引结构体
        /// </summary>
        public struct LevelIndexIndex : IConfigIndexGroup<global::XM.ConfigNew.Tests.Data.UltimateComplexConfigUnmanaged>, IEquatable<LevelIndexIndex>
        {
            // 索引字段
            /// <summary>索引字段: RequiredLevel</summary>
            public int RequiredLevel;

            /// <summary>
            /// 构造方法
            /// </summary>
            /// <param name="requiredlevel">RequiredLevel值</param>
            public LevelIndexIndex(int requiredlevel)
            {
                this.RequiredLevel = requiredlevel;
            }

            /// <summary>
            /// 判断索引是否相等
            /// </summary>
            public bool Equals(LevelIndexIndex other)
            {
                return this.RequiredLevel == other.RequiredLevel;
            }

            /// <summary>
            /// 获取哈希码
            /// </summary>
            public int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 31 + RequiredLevel.GetHashCode();
                    return hash;
                }
            }

        }
    }

    /// <summary>
    /// LevelIndex 索引扩展方法
    /// </summary>
    public static class UltimateComplexConfigUnmanaged_LevelIndexIndexExtensions
    {
        /// <summary>
        /// 获取索引对应的配置数据列表(多值索引)
        /// </summary>
        /// <param name="index">索引值</param>
        /// <param name="allocator">内存分配器</param>
        /// <param name="returns">配置数据数组</param>
        public static NativeArray<global::XM.ConfigNew.Tests.Data.UltimateComplexConfigUnmanaged> GetVals(this UltimateComplexConfigUnmanaged.LevelIndexIndex index, Allocator allocator)
        {
            // TODO: 实现多值索引查询逻辑
            return new NativeArray<global::XM.ConfigNew.Tests.Data.UltimateComplexConfigUnmanaged>(0, allocator);
        }
    }
}
