using System;
using Unity.Collections;
using XM;
using XM.ConfigNew.CodeGen;
using XM.ConfigNew.Tests.Data;
using XM.Contracts.Config;

namespace XM.ConfigNew.Tests.Data
{
    /// <summary>
    /// PassiveSkillConfig 的非托管数据结构 (代码生成)
    /// </summary>
    public partial struct PassiveSkillConfigUnmanaged : IConfigUnManaged<global::XM.ConfigNew.Tests.Data.PassiveSkillConfigUnmanaged>
    {
        // 字段

        /// <summary>容器, 嵌套配置</summary>
        public global::XBlobArray<global::XM.ConfigNew.Tests.Data.AttributeConfigUnmanaged> AttributeBonuses;
        public global::Unity.Collections.FixedString32Bytes TriggerCondition;
        /// <summary>索引: SkillIdIndex</summary>
        public int SkillId;
        public global::Unity.Collections.FixedString32Bytes SkillName;
        public int MaxLevel;

        /// <summary>
        /// ToString方法
        /// </summary>
        /// <param name="dataContainer">数据容器</param>
        public string ToString(object dataContainer)
        {
            return "PassiveSkillConfig";
        }
    }
}
