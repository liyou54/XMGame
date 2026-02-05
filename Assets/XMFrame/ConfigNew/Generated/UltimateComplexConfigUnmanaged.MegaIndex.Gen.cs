using System;
using XM;
using XM.Contracts.Config;
using Unity.Collections;

namespace XM.ConfigNew.Tests.Data
{
    // UltimateComplexConfigUnmanaged 的部分类 - MegaIndex索引
    public partial struct UltimateComplexConfigUnmanaged
    {
        /// <summary>
        /// MegaIndex 索引结构体
        /// </summary>
        public struct MegaIndexIndex : IConfigIndexGroup<global::XM.ConfigNew.Tests.Data.UltimateComplexConfigUnmanaged>, IEquatable<MegaIndexIndex>
        {
            // 索引字段
            /// <summary>索引字段: IndexField1</summary>
            public global::XM.ConfigNew.Tests.Data.EItemType IndexField1;
            /// <summary>索引字段: IndexField2</summary>
            public global::XM.ConfigNew.Tests.Data.EItemQuality IndexField2;
            /// <summary>索引字段: IndexField3</summary>
            public int IndexField3;
            /// <summary>索引字段: IndexField4</summary>
            public global::Unity.Collections.FixedString32Bytes IndexField4;

            /// <summary>
            /// 构造方法
            /// </summary>
            /// <param name="indexfield1">IndexField1值</param>
            /// <param name="indexfield2">IndexField2值</param>
            /// <param name="indexfield3">IndexField3值</param>
            /// <param name="indexfield4">IndexField4值</param>
            public MegaIndexIndex(global::XM.ConfigNew.Tests.Data.EItemType indexfield1, global::XM.ConfigNew.Tests.Data.EItemQuality indexfield2, int indexfield3, global::Unity.Collections.FixedString32Bytes indexfield4)
            {
                this.IndexField1 = indexfield1;
                this.IndexField2 = indexfield2;
                this.IndexField3 = indexfield3;
                this.IndexField4 = indexfield4;
            }

            /// <summary>
            /// 判断索引是否相等
            /// </summary>
            public bool Equals(MegaIndexIndex other)
            {
                return this.IndexField1 == other.IndexField1 && this.IndexField2 == other.IndexField2 && this.IndexField3 == other.IndexField3 && this.IndexField4 == other.IndexField4;
            }

            /// <summary>
            /// 获取哈希码
            /// </summary>
            public int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 31 + IndexField1.GetHashCode();
                    hash = hash * 31 + IndexField2.GetHashCode();
                    hash = hash * 31 + IndexField3.GetHashCode();
                    hash = hash * 31 + IndexField4.GetHashCode();
                    return hash;
                }
            }

        }
    }

    /// <summary>
    /// MegaIndex 索引扩展方法
    /// </summary>
    public static class UltimateComplexConfigUnmanaged_MegaIndexIndexExtensions
    {
        /// <summary>
        /// 获取索引对应的配置数据(唯一索引)
        /// </summary>
        /// <param name="index">索引值</param>
        /// <param name="returns">配置数据</param>
        public static global::XM.ConfigNew.Tests.Data.UltimateComplexConfigUnmanaged GetVal(this UltimateComplexConfigUnmanaged.MegaIndexIndex index)
        {
            // TODO: 实现索引查询逻辑
            return default(global::XM.ConfigNew.Tests.Data.UltimateComplexConfigUnmanaged);
        }
    }
}
