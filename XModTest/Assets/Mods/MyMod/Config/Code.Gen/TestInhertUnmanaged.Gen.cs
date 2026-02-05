using System;
using Unity.Collections;
using XM;
using XM.ConfigNew.CodeGen;
using XM.Contracts.Config;

namespace XM.Editor.Gen
{
    /// <summary>
    /// TestInhert 的非托管数据结构 (代码生成)
    /// </summary>
    public partial struct TestInhertUnmanaged : IConfigUnManaged<global::XM.Editor.Gen.TestInhertUnmanaged>
    {
        // 字段

        /// <summary>Link</summary>
        public CfgI<TestConfigUnmanaged> Link;
        /// <summary>Link父节点指针 (指向TestConfig)</summary>
        public global::XBlobPtr<TestConfigUnmanaged> Link_ParentPtr;
        /// <summary>Link父节点索引 (指向TestConfig)</summary>
        public CfgI<TestConfigUnmanaged> Link_ParentIndex;
        public int xxxx;

        /// <summary>
        /// ToString方法
        /// </summary>
        /// <param name="dataContainer">数据容器</param>
        public string ToString(object dataContainer)
        {
            return "TestInhert";
        }
    }
}
