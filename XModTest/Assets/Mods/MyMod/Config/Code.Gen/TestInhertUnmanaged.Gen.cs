using System;
using Unity.Collections;
using XM;
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
