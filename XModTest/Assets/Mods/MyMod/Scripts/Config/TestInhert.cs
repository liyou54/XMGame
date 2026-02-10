using XM;
using XM.Contracts.Config;
using XM.Utils.Attribute;

namespace XM.Editor.Gen
{
    /// <summary>
    /// 组合配置（使用 [XMLLink] 代替继承），Unmanaged 由代码生成器生成（TestInhertUnmanaged.Gen.cs）。
    /// </summary>
    [XmlDefined]
    public class TestInhert : IXConfig<TestInhert, TestInhertUnmanaged>
    {
        [XmlKey]
        [XMLLink] public CfgS<TestConfig> Link;
        public int xxxx; 
        public CfgI Data { get; set; }
    }

    /// <summary>
    /// 占位定义，供编译通过；字段由 Unmanaged 代码生成器在 TestInhertUnmanaged.Gen.cs 中生成。
    /// [XMLLink] CfgS&lt;TestConfig&gt; Link 将生成：Link_ParentDst、Link_ParentRef、Link 三字段。
    /// </summary>
    public partial struct TestInhertUnmanaged : IConfigUnManaged<TestInhertUnmanaged>
    {
    }
}