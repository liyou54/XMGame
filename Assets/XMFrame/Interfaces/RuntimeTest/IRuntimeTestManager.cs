using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using XM.Contracts;

namespace XM.RuntimeTest
{
#if UNITY_EDITOR
    /// <summary>
    /// 运行时测试管理器接口
    /// </summary>
    public interface IRuntimeTestManager : IManager<IRuntimeTestManager>
    {
        /// <summary>注册测试用例</summary>
        void RegisterTestCase(ITestCase testCase);
        
        /// <summary>检查测试用例是否已注册</summary>
        bool IsTestCaseRegistered(string testName);
        
        /// <summary>运行指定测试</summary>
        UniTask<TestResult> RunTestAsync(string testName);
        
        /// <summary>运行所有测试</summary>
        UniTask<List<TestResult>> RunAllTestsAsync();
        
        /// <summary>获取所有测试用例</summary>
        IReadOnlyList<ITestCase> GetAllTests();
        
        /// <summary>获取测试历史</summary>
        IReadOnlyList<TestResult> GetTestHistory();
        
        /// <summary>清除测试历史</summary>
        void ClearHistory();
    }
#endif
}
