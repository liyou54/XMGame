using System;
using XM;
using XM.Contracts.Config;
using Unity.Collections;

namespace XM.ConfigNew.Tests.Data
{
    // BaseItemConfigUnmanaged 的部分类 - NameIndex索引
    public partial struct BaseItemConfigUnmanaged
    {
        /// <summary>
        /// NameIndex 索引结构体
        /// </summary>
        public struct NameIndexIndex : IConfigIndexGroup<global::XM.ConfigNew.Tests.Data.BaseItemConfigUnmanaged>, IEquatable<NameIndexIndex>
        {
            // 索引字段
            /// <summary>索引字段: Name</summary>
            public global::Unity.Collections.FixedString32Bytes Name;

            /// <summary>
            /// 构造方法
            /// </summary>
            /// <param name="name">Name值</param>
            public NameIndexIndex(global::Unity.Collections.FixedString32Bytes name)
            {
                this.Name = name;
            }

            /// <summary>
            /// 判断索引是否相等
            /// </summary>
            public bool Equals(NameIndexIndex other)
            {
                return this.Name == other.Name;
            }

            /// <summary>
            /// 获取哈希码
            /// </summary>
            public int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 31 + Name.GetHashCode();
                    return hash;
                }
            }

        }
    }

    /// <summary>
    /// NameIndex 索引扩展方法
    /// </summary>
    public static class BaseItemConfigUnmanaged_NameIndexIndexExtensions
    {
        /// <summary>
        /// 获取索引对应的配置数据(唯一索引)
        /// </summary>
        /// <param name="index">索引值</param>
        /// <param name="returns">配置数据</param>
        public static global::XM.ConfigNew.Tests.Data.BaseItemConfigUnmanaged GetVal(this BaseItemConfigUnmanaged.NameIndexIndex index)
        {
            // TODO: 实现索引查询逻辑
            return default(global::XM.ConfigNew.Tests.Data.BaseItemConfigUnmanaged);
        }
    }
}
