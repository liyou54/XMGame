using System;
using Unity.Collections;
using XM;
using XM.ConfigNew.CodeGen;
using XM.Contracts.Config;

namespace XM.ConfigNew.Tests.Data
{
    /// <summary>
    /// ListLinkChildConfig 的非托管数据结构 (代码生成)
    /// </summary>
    public partial struct ListLinkChildConfigUnmanaged : IConfigUnManaged<global::XM.ConfigNew.Tests.Data.ListLinkChildConfigUnmanaged>
    {
        // 字段

        /// <summary>索引: ChildIdIndex</summary>
        public int ChildId;
        public global::Unity.Collections.FixedString32Bytes ChildName;
        /// <summary>容器, Link</summary>
        public global::XBlobArray<CfgI<global::XM.ConfigNew.Tests.Data.LinkParentConfigUnmanaged>> Parent;
        /// <summary>Link父节点指针 (指向LinkParentConfig)</summary>
        public global::XBlobArray<global::XBlobPtr<global::XM.ConfigNew.Tests.Data.LinkParentConfigUnmanaged>> Parent_ParentPtr;
        /// <summary>Link父节点索引 (指向LinkParentConfig)</summary>
        public global::XBlobArray<CfgI<global::XM.ConfigNew.Tests.Data.LinkParentConfigUnmanaged>> Parent_ParentIndex;

        /// <summary>
        /// ToString方法
        /// </summary>
        /// <param name="dataContainer">数据容器</param>
        public string ToString(object dataContainer)
        {
            return "ListLinkChildConfig";
        }
    }
}
