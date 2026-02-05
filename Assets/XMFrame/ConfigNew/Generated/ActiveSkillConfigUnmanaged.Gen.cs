using System;
using Unity.Collections;
using XM;
using XM.ConfigNew.CodeGen;
using XM.ConfigNew.Tests.Data;
using XM.Contracts.Config;

namespace XM.ConfigNew.Tests.Data
{
    /// <summary>
    /// ActiveSkillConfig 的非托管数据结构 (代码生成)
    /// </summary>
    public partial struct ActiveSkillConfigUnmanaged : IConfigUnManaged<global::XM.ConfigNew.Tests.Data.ActiveSkillConfigUnmanaged>
    {
        // 字段

        public int ManaCost;
        public float Cooldown;
        /// <summary>容器, 嵌套配置</summary>
        public global::XBlobArray<global::XM.ConfigNew.Tests.Data.EffectConfigUnmanaged> SkillEffects;
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
            return "ActiveSkillConfig";
        }
    }
}
