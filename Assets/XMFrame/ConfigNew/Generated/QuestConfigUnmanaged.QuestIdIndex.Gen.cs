using System;
using XM;
using XM.Contracts.Config;
using Unity.Collections;

namespace XM.ConfigNew.Tests.Data
{
    // QuestConfigUnmanaged 的部分类 - QuestIdIndex索引
    public partial struct QuestConfigUnmanaged
    {
        /// <summary>
        /// QuestIdIndex 索引结构体
        /// </summary>
        public struct QuestIdIndexIndex : IConfigIndexGroup<global::XM.ConfigNew.Tests.Data.QuestConfigUnmanaged>, IEquatable<QuestIdIndexIndex>
        {
            // 索引字段
            /// <summary>索引字段: QuestId</summary>
            public int QuestId;

            /// <summary>
            /// 构造方法
            /// </summary>
            /// <param name="questid">QuestId值</param>
            public QuestIdIndexIndex(int questid)
            {
                this.QuestId = questid;
            }

            /// <summary>
            /// 判断索引是否相等
            /// </summary>
            public bool Equals(QuestIdIndexIndex other)
            {
                return this.QuestId == other.QuestId;
            }

            /// <summary>
            /// 获取哈希码
            /// </summary>
            public int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;
                    hash = hash * 31 + QuestId.GetHashCode();
                    return hash;
                }
            }

        }
    }

    /// <summary>
    /// QuestIdIndex 索引扩展方法
    /// </summary>
    public static class QuestConfigUnmanaged_QuestIdIndexIndexExtensions
    {
        /// <summary>
        /// 获取索引对应的配置数据(唯一索引)
        /// </summary>
        /// <param name="index">索引值</param>
        /// <param name="returns">配置数据</param>
        public static global::XM.ConfigNew.Tests.Data.QuestConfigUnmanaged GetVal(this QuestConfigUnmanaged.QuestIdIndexIndex index)
        {
            // TODO: 实现索引查询逻辑
            return default(global::XM.ConfigNew.Tests.Data.QuestConfigUnmanaged);
        }
    }
}
