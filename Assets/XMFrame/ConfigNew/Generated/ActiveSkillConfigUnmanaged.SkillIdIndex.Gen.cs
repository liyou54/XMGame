using System;
using XM;
using XM.Contracts.Config;
using Unity.Collections;

namespace XM.ConfigNew.Tests.Data
{
    // ActiveSkillConfigUnmanaged 的部分类 - SkillIdIndex索引
    public partial struct ActiveSkillConfigUnmanaged
    {
        /// <summary>
        /// SkillIdIndex 索引结构体
        /// </summary>
        public struct SkillIdIndexIndex : IConfigIndexGroup<global::XM.ConfigNew.Tests.Data.ActiveSkillConfigUnmanaged>, IEquatable<SkillIdIndexIndex>
        {
            // 索引字段
            /// <summary>索引字段: SkillId</summary>
            public int SkillId;

            /// <summary>
            /// 构造方法
            /// </summary>
            /// <param name="skillid">SkillId值</param>
            public SkillIdIndexIndex(int skillid)
            {
                this.SkillId = skillid;
            }

            /// <summary>
            /// 判断索引是否相等
            /// </summary>
            public bool Equals(SkillIdIndexIndex other)
            {
                return this.SkillId == other.SkillId;
            }

            /// <summary>
            /// 获取哈希码
            /// </summary>
            public int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 31 + SkillId.GetHashCode();
                    return hash;
                }
            }

        }
    }

    /// <summary>
    /// SkillIdIndex 索引扩展方法
    /// </summary>
    public static class ActiveSkillConfigUnmanaged_SkillIdIndexIndexExtensions
    {
        /// <summary>
        /// 获取索引对应的配置数据(唯一索引)
        /// </summary>
        /// <param name="index">索引值</param>
        /// <param name="returns">配置数据</param>
        public static global::XM.ConfigNew.Tests.Data.ActiveSkillConfigUnmanaged GetVal(this ActiveSkillConfigUnmanaged.SkillIdIndexIndex index)
        {
            // TODO: 实现索引查询逻辑
            return default(global::XM.ConfigNew.Tests.Data.ActiveSkillConfigUnmanaged);
        }
    }
}
