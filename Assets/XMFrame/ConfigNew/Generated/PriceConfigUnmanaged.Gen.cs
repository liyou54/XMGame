using System;
using Unity.Collections;
using XM;
using XM.ConfigNew.CodeGen;
using XM.Contracts.Config;

namespace XM.ConfigNew.Tests.Data
{
    /// <summary>
    /// PriceConfig 的非托管数据结构 (代码生成)
    /// </summary>
    public partial struct PriceConfigUnmanaged : IConfigUnManaged<global::XM.ConfigNew.Tests.Data.PriceConfigUnmanaged>
    {
        // 字段

        public int Gold;
        public int Silver;
        /// <summary>可空</summary>
        public int Diamond;

        /// <summary>
        /// ToString方法
        /// </summary>
        /// <param name="dataContainer">数据容器</param>
        public string ToString(object dataContainer)
        {
            return "PriceConfig";
        }
    }
}
