using System.Collections.Generic;
using XM;
using XM.Contracts.Config;
using XM.Utils.Attribute;

namespace MyMod
{
    /// <summary>
    /// MyMod 测试配置：物品/条目配置，用于验证配置系统。
    /// </summary>
    [XmlDefined]
    public partial class MyItemConfig : IXConfig<MyItemConfig, MyItemConfigUnManaged>
    {
        public CfgI Data { get; set; }

        /// <summary>配置键，格式一般为 ModName::ConfigName</summary>
        public CfgS<MyItemConfigUnManaged> Id;

        /// <summary>显示名称</summary>
        public string Name;

        /// <summary>等级/数值</summary>
        public int Level;

        /// <summary>标签 ID 列表</summary>
        public List<int> Tags;
    }

    /// <summary>
    /// MyItemConfig 对应的非托管结构（当前为空，仅满足接口约束）。
    /// </summary>
    public partial  struct MyItemConfigUnManaged : IConfigUnManaged<MyItemConfigUnManaged>
    {
    }
}
