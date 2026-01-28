using XM;

namespace XM.Editor.Gen
{
    /// <summary>
    /// 派生配置，Unmanaged 由代码生成器生成（TestInhertUnmanaged.Gen.cs）。
    /// </summary>
    public class TestInhert : TestConfig, IXConfig<TestInhert, TestInhertUnmanaged>
    {
        public int xxxx;
    }

    /// <summary>
    /// 占位定义，供编译通过；字段由 Unmanaged 代码生成器在 TestInhertUnmanaged.Gen.cs 中生成。
    /// </summary>
    public partial struct TestInhertUnmanaged : IConfigUnManaged<TestInhertUnmanaged>
    {
    }
}