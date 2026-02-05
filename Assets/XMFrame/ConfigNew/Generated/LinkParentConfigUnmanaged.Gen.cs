using System;
using Unity.Collections;
using XM;
using XM.ConfigNew.CodeGen;
using XM.Contracts.Config;

namespace XM.ConfigNew.Tests.Data
{
    /// <summary>
    /// LinkParentConfig 的非托管数据结构 (代码生成)
    /// </summary>
    public partial struct LinkParentConfigUnmanaged : IConfigUnManaged<global::XM.ConfigNew.Tests.Data.LinkParentConfigUnmanaged>
    {
        // 字段

        /// <summary>索引: ParentIdIndex</summary>
        public int ParentId;
        public global::Unity.Collections.FixedString32Bytes ParentName;
        public global::Unity.Collections.FixedString32Bytes ParentData;

        /// <summary>
        /// ToString方法
        /// </summary>
        /// <param name="dataContainer">数据容器</param>
        public string ToString(object dataContainer)
        {
            return "LinkParentConfig";
        }
    }
}
