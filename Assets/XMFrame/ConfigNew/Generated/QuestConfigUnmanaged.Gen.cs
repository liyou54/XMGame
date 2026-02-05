using System;
using Unity.Collections;
using XM;
using XM.ConfigNew.CodeGen;
using XM.Contracts.Config;

namespace XM.ConfigNew.Tests.Data
{
    /// <summary>
    /// QuestConfig 的非托管数据结构 (代码生成)
    /// </summary>
    public partial struct QuestConfigUnmanaged : IConfigUnManaged<global::XM.ConfigNew.Tests.Data.QuestConfigUnmanaged>
    {
        // 字段

        /// <summary>索引: QuestIdIndex</summary>
        public int QuestId;
        public global::Unity.Collections.FixedString32Bytes QuestName;
        public int MinLevel;
        /// <summary>Link</summary>
        public CfgI<global::XM.ConfigNew.Tests.Data.ComplexItemConfigUnmanaged> RewardItem;
        /// <summary>Link父节点指针 (指向ComplexItemConfig)</summary>
        public global::XBlobPtr<global::XM.ConfigNew.Tests.Data.ComplexItemConfigUnmanaged> RewardItem_ParentPtr;
        /// <summary>Link父节点索引 (指向ComplexItemConfig)</summary>
        public CfgI<global::XM.ConfigNew.Tests.Data.ComplexItemConfigUnmanaged> RewardItem_ParentIndex;
        /// <summary>Link</summary>
        public CfgI<global::XM.ConfigNew.Tests.Data.QuestConfigUnmanaged> PreQuest;
        /// <summary>Link父节点指针 (指向QuestConfig)</summary>
        public global::XBlobPtr<global::XM.ConfigNew.Tests.Data.QuestConfigUnmanaged> PreQuest_ParentPtr;
        /// <summary>Link父节点索引 (指向QuestConfig)</summary>
        public CfgI<global::XM.ConfigNew.Tests.Data.QuestConfigUnmanaged> PreQuest_ParentIndex;

        /// <summary>
        /// ToString方法
        /// </summary>
        /// <param name="dataContainer">数据容器</param>
        public string ToString(object dataContainer)
        {
            return "QuestConfig";
        }
    }
}
