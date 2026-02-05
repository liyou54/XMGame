using System;
using XM;
using XM.Contracts.Config;
using Unity.Collections;

namespace XM.ConfigNew.Tests.Data
{
    // SingleLinkChildConfigUnmanaged 的部分类 - ChildIdIndex索引
    public partial struct SingleLinkChildConfigUnmanaged
    {
        /// <summary>
        /// ChildIdIndex 索引结构体
        /// </summary>
        public struct ChildIdIndexIndex : IConfigIndexGroup<global::XM.ConfigNew.Tests.Data.SingleLinkChildConfigUnmanaged>, IEquatable<ChildIdIndexIndex>
        {
            // 索引字段
            /// <summary>索引字段: ChildId</summary>
            public int ChildId;

            /// <summary>
            /// 构造方法
            /// </summary>
            /// <param name="childid">ChildId值</param>
            public ChildIdIndexIndex(int childid)
            {
                this.ChildId = childid;
            }

            /// <summary>
            /// 判断索引是否相等
            /// </summary>
            public bool Equals(ChildIdIndexIndex other)
            {
                return this.ChildId == other.ChildId;
            }

            /// <summary>
            /// 获取哈希码
            /// </summary>
            public int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 31 + ChildId.GetHashCode();
                    return hash;
                }
            }

        }
    }

    /// <summary>
    /// ChildIdIndex 索引扩展方法
    /// </summary>
    public static class SingleLinkChildConfigUnmanaged_ChildIdIndexIndexExtensions
    {
        /// <summary>
        /// 获取索引对应的配置数据(唯一索引)
        /// </summary>
        /// <param name="index">索引值</param>
        /// <param name="returns">配置数据</param>
        public static global::XM.ConfigNew.Tests.Data.SingleLinkChildConfigUnmanaged GetVal(this SingleLinkChildConfigUnmanaged.ChildIdIndexIndex index)
        {
            // TODO: 实现索引查询逻辑
            return default(global::XM.ConfigNew.Tests.Data.SingleLinkChildConfigUnmanaged);
        }
    }
}
