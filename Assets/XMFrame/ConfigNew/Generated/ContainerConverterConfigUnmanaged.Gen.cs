using System;
using Unity.Collections;
using XM;
using XM.ConfigNew.CodeGen;
using XM.Contracts.Config;

namespace XM.ConfigNew.Tests.Data
{
    /// <summary>
    /// ContainerConverterConfig 的非托管数据结构 (代码生成)
    /// </summary>
    public partial struct ContainerConverterConfigUnmanaged : IConfigUnManaged<global::XM.ConfigNew.Tests.Data.ContainerConverterConfigUnmanaged>
    {
        // 字段

        /// <summary>容器</summary>
        public global::XBlobArray<StrI> CustomStringList;
        /// <summary>容器</summary>
        public global::XBlobMap<StrI, int> CustomValueDict;
        /// <summary>容器</summary>
        public global::XBlobMap<StrI, int> CustomKeyDict;
        /// <summary>容器</summary>
        public global::XBlobMap<StrI, int> CustomBothDict;
        /// <summary>容器</summary>
        public global::XBlobArray<global::XBlobArray<StrI>> NestedCustomList;
        /// <summary>容器</summary>
        public global::XBlobArray<global::XM.ConfigNew.CodeGen.EnumWrapper<global::XM.ConfigNew.Tests.Data.EItemType>> EnumList;

        /// <summary>
        /// ToString方法
        /// </summary>
        /// <param name="dataContainer">数据容器</param>
        public string ToString(object dataContainer)
        {
            return "ContainerConverterConfig";
        }
    }
}
