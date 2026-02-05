using System;
using Unity.Collections;
using XM;
using XM.ConfigNew.CodeGen;
using XM.Contracts.Config;

namespace XM.ConfigNew.Tests.Data
{
    /// <summary>
    /// AttributeConfig 的非托管数据结构 (代码生成)
    /// </summary>
    public partial struct AttributeConfigUnmanaged : IConfigUnManaged<global::XM.ConfigNew.Tests.Data.AttributeConfigUnmanaged>
    {
        // 字段

        /// <summary>枚举</summary>
        public global::XM.ConfigNew.Tests.Data.EAttributeType Type;
        public int BaseValue;
        public float Multiplier;
        /// <summary>可空</summary>
        public int BonusValue;

        /// <summary>
        /// ToString方法
        /// </summary>
        /// <param name="dataContainer">数据容器</param>
        public string ToString(object dataContainer)
        {
            return "AttributeConfig";
        }
    }
}
