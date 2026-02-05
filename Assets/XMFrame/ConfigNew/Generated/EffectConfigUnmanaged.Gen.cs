using System;
using Unity.Collections;
using XM;
using XM.ConfigNew.CodeGen;
using XM.Contracts.Config;

namespace XM.ConfigNew.Tests.Data
{
    /// <summary>
    /// EffectConfig 的非托管数据结构 (代码生成)
    /// </summary>
    public partial struct EffectConfigUnmanaged : IConfigUnManaged<global::XM.ConfigNew.Tests.Data.EffectConfigUnmanaged>
    {
        // 字段

        public global::Unity.Collections.FixedString32Bytes EffectName;
        public int Duration;
        public float Value;

        /// <summary>
        /// ToString方法
        /// </summary>
        /// <param name="dataContainer">数据容器</param>
        public string ToString(object dataContainer)
        {
            return "EffectConfig";
        }
    }
}
