using System;
using XM;
using XM.Contracts.Config;
using Unity.Collections;

namespace XM.ConfigNew.Tests.Data
{
    // BaseItemConfigUnmanaged 的部分类 - IdIndex索引
    public partial struct BaseItemConfigUnmanaged
    {
        /// <summary>
        /// IdIndex 索引结构体
        /// </summary>
        public struct IdIndexIndex : IConfigIndexGroup<global::XM.ConfigNew.Tests.Data.BaseItemConfigUnmanaged>, IEquatable<IdIndexIndex>
        {
            // 索引字段
            /// <summary>索引字段: Id</summary>
            public int Id;

            /// <summary>
            /// 构造方法
            /// </summary>
            /// <param name="id">Id值</param>
            public IdIndexIndex(int id)
            {
                this.Id = id;
            }

            /// <summary>
            /// 判断索引是否相等
            /// </summary>
            public bool Equals(IdIndexIndex other)
            {
                return this.Id == other.Id;
            }

            /// <summary>
            /// 获取哈希码
            /// </summary>
            public int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 31 + Id.GetHashCode();
                    return hash;
                }
            }

        }
    }

    /// <summary>
    /// IdIndex 索引扩展方法
    /// </summary>
    public static class BaseItemConfigUnmanaged_IdIndexIndexExtensions
    {
        /// <summary>
        /// 获取索引对应的配置数据(唯一索引)
        /// </summary>
        /// <param name="index">索引值</param>
        /// <param name="returns">配置数据</param>
        public static global::XM.ConfigNew.Tests.Data.BaseItemConfigUnmanaged GetVal(this BaseItemConfigUnmanaged.IdIndexIndex index)
        {
            // TODO: 实现索引查询逻辑
            return default(global::XM.ConfigNew.Tests.Data.BaseItemConfigUnmanaged);
        }
    }
}
