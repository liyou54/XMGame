using System;
using Unity.Collections;
using XM;
using XM.ConfigNew.CodeGen;
using XM.Contracts.Config;

namespace XM.ConfigNew.Tests.Data
{
    /// <summary>
    /// BaseItemConfig 的非托管数据结构 (代码生成)
    /// </summary>
    public partial struct BaseItemConfigUnmanaged : IConfigUnManaged<global::XM.ConfigNew.Tests.Data.BaseItemConfigUnmanaged>
    {
        // 字段

        /// <summary>索引: IdIndex</summary>
        public int Id;
        /// <summary>索引: NameIndex</summary>
        public global::Unity.Collections.FixedString32Bytes Name;
        /// <summary>枚举</summary>
        public global::XM.ConfigNew.Tests.Data.EItemQuality Quality;
        public int StackSize;
        public global::Unity.Collections.FixedString32Bytes Description;

        /// <summary>
        /// ToString方法
        /// </summary>
        /// <param name="dataContainer">数据容器</param>
        public string ToString(object dataContainer)
        {
            return "BaseItemConfig";
        }
    }
}
