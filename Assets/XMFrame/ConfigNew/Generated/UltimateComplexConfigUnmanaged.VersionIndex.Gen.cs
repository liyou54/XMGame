using System;
using XM;
using XM.Contracts.Config;
using Unity.Collections;

namespace XM.ConfigNew.Tests.Data
{
    // UltimateComplexConfigUnmanaged 的部分类 - VersionIndex索引
    public partial struct UltimateComplexConfigUnmanaged
    {
        /// <summary>
        /// VersionIndex 索引结构体
        /// </summary>
        public struct VersionIndexIndex : IConfigIndexGroup<global::XM.ConfigNew.Tests.Data.UltimateComplexConfigUnmanaged>, IEquatable<VersionIndexIndex>
        {
            // 索引字段
            /// <summary>索引字段: Version</summary>
            public int Version;

            /// <summary>
            /// 构造方法
            /// </summary>
            /// <param name="version">Version值</param>
            public VersionIndexIndex(int version)
            {
                this.Version = version;
            }

            /// <summary>
            /// 判断索引是否相等
            /// </summary>
            public bool Equals(VersionIndexIndex other)
            {
                return this.Version == other.Version;
            }

            /// <summary>
            /// 获取哈希码
            /// </summary>
            public int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 31 + Version.GetHashCode();
                    return hash;
                }
            }

        }
    }

    /// <summary>
    /// VersionIndex 索引扩展方法
    /// </summary>
    public static class UltimateComplexConfigUnmanaged_VersionIndexIndexExtensions
    {
        /// <summary>
        /// 获取索引对应的配置数据(唯一索引)
        /// </summary>
        /// <param name="index">索引值</param>
        /// <param name="returns">配置数据</param>
        public static global::XM.ConfigNew.Tests.Data.UltimateComplexConfigUnmanaged GetVal(this UltimateComplexConfigUnmanaged.VersionIndexIndex index)
        {
            // TODO: 实现索引查询逻辑
            return default(global::XM.ConfigNew.Tests.Data.UltimateComplexConfigUnmanaged);
        }
    }
}
