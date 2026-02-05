using System;
using XM;
using XM.Contracts.Config;
using Unity.Collections;

namespace XM.ConfigNew.Tests.Data
{
    // LinkParentConfigUnmanaged 的部分类 - ParentIdIndex索引
    public partial struct LinkParentConfigUnmanaged
    {
        /// <summary>
        /// ParentIdIndex 索引结构体
        /// </summary>
        public struct ParentIdIndexIndex : IConfigIndexGroup<global::XM.ConfigNew.Tests.Data.LinkParentConfigUnmanaged>, IEquatable<ParentIdIndexIndex>
        {
            // 索引字段
            /// <summary>索引字段: ParentId</summary>
            public int ParentId;

            /// <summary>
            /// 构造方法
            /// </summary>
            /// <param name="parentid">ParentId值</param>
            public ParentIdIndexIndex(int parentid)
            {
                this.ParentId = parentid;
            }

            /// <summary>
            /// 判断索引是否相等
            /// </summary>
            public bool Equals(ParentIdIndexIndex other)
            {
                return this.ParentId == other.ParentId;
            }

            /// <summary>
            /// 获取哈希码
            /// </summary>
            public int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 31 + ParentId.GetHashCode();
                    return hash;
                }
            }

        }
    }

    /// <summary>
    /// ParentIdIndex 索引扩展方法
    /// </summary>
    public static class LinkParentConfigUnmanaged_ParentIdIndexIndexExtensions
    {
        /// <summary>
        /// 获取索引对应的配置数据(唯一索引)
        /// </summary>
        /// <param name="index">索引值</param>
        /// <param name="returns">配置数据</param>
        public static global::XM.ConfigNew.Tests.Data.LinkParentConfigUnmanaged GetVal(this LinkParentConfigUnmanaged.ParentIdIndexIndex index)
        {
            // TODO: 实现索引查询逻辑
            return default(global::XM.ConfigNew.Tests.Data.LinkParentConfigUnmanaged);
        }
    }
}
