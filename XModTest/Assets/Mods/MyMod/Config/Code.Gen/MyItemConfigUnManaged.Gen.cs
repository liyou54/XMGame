using System;
using Unity.Collections;
using XM;
using XM.Contracts.Config;

namespace MyMod
{
    /// <summary>
    /// MyItemConfig 的非托管数据结构 (代码生成)
    /// </summary>
    public partial struct MyItemConfigUnManaged : IConfigUnManaged<global::MyMod.MyItemConfigUnManaged>
    {
        // 字段

        public CfgI<TestConfigUnmanaged> Id;
        public global::Unity.Collections.FixedString32Bytes Name;
        public int Level;
        /// <summary>容器</summary>
        public global::XBlobArray<int> Tags;

        /// <summary>
        /// ToString方法
        /// </summary>
        /// <param name="dataContainer">数据容器</param>
        public string ToString(object dataContainer)
        {
            return "MyItemConfig";
        }
    }
}
