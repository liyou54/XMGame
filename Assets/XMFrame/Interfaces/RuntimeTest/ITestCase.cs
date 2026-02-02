using Cysharp.Threading.Tasks;

namespace XM.RuntimeTest
{
    /// <summary>
    /// 运行时测试用例接口
    /// </summary>
    public interface ITestCase
    {
        /// <summary>测试用例名称</summary>
        string Name { get; }
        
        /// <summary>测试用例描述</summary>
        string Description { get; }
        
        /// <summary>执行测试</summary>
        UniTask<TestResult> ExecuteAsync();
    }
}
