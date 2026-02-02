using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using XM.Contracts;
using XM.RuntimeTest;

namespace XM
{
#if UNITY_EDITOR
    /// <summary>
    /// 运行时测试管理器：管理测试用例的注册、执行和结果记录
    /// </summary>
    [AutoCreate(priority: 2000)] // 较低优先级，确保在所有业务管理器之后初始化
    [ManagerDependency(typeof(IConfigDataCenter))]
    public class RuntimeTestManager : ManagerBase<IRuntimeTestManager>, IRuntimeTestManager
    {
        /// <summary>已注册的测试用例</summary>
        private Dictionary<string, ITestCase> _testCases = new Dictionary<string, ITestCase>();
        
        /// <summary>测试历史记录</summary>
        private List<TestResult> _testHistory = new List<TestResult>();

        public override UniTask OnCreate()
        {
            XLog.Info("RuntimeTestManager 创建");
            return UniTask.CompletedTask;
        }

        public override UniTask OnInit()
        {
            XLog.Info("RuntimeTestManager 初始化完成，等待测试用例注册");
            return UniTask.CompletedTask;
        }

        #region 测试用例管理

        /// <summary>注册测试用例</summary>
        public void RegisterTestCase(ITestCase testCase)
        {
            if (testCase == null)
            {
                XLog.Error("尝试注册空的测试用例");
                return;
            }

            if (string.IsNullOrEmpty(testCase.Name))
            {
                XLog.Error("测试用例名称不能为空");
                return;
            }

            if (_testCases.ContainsKey(testCase.Name))
            {
                XLog.WarningFormat("测试用例 '{0}' 已注册，跳过重复注册", testCase.Name);
                return;
            }

            _testCases[testCase.Name] = testCase;
            XLog.InfoFormat("注册测试用例: {0} - {1}", testCase.Name, testCase.Description);
        }

        /// <summary>检查测试用例是否已注册</summary>
        public bool IsTestCaseRegistered(string testName)
        {
            return _testCases.ContainsKey(testName);
        }

        /// <summary>获取所有测试用例</summary>
        public IReadOnlyList<ITestCase> GetAllTests()
        {
            return _testCases.Values.ToList();
        }

        #endregion

        #region 测试执行

        /// <summary>运行指定测试</summary>
        public async UniTask<TestResult> RunTestAsync(string testName)
        {
            if (!_testCases.TryGetValue(testName, out var testCase))
            {
                var errorResult = new TestResult
                {
                    Success = false,
                    Message = $"未找到测试用例: {testName}",
                    ExecutionTime = 0f
                };
                _testHistory.Add(errorResult);
                return errorResult;
            }

            XLog.InfoFormat("=================================================================");
            XLog.InfoFormat("开始执行测试: {0}", testCase.Name);
            XLog.InfoFormat("描述: {0}", testCase.Description);
            XLog.InfoFormat("=================================================================");

            var stopwatch = Stopwatch.StartNew();
            TestResult result;

            try
            {
                result = await testCase.ExecuteAsync();
                stopwatch.Stop();
                result.ExecutionTime = (float)stopwatch.Elapsed.TotalSeconds;

                if (result.Success)
                {
                    XLog.InfoFormat("✓ 测试 '{0}' 完成 - {1} ({2:F2}s)", 
                        testCase.Name, result.Message, result.ExecutionTime);
                }
                else
                {
                    XLog.ErrorFormat("✗ 测试 '{0}' 失败 - {1} ({2:F2}s)", 
                        testCase.Name, result.Message, result.ExecutionTime);
                }

                // 输出详细日志
                if (result.DetailLog != null && result.DetailLog.Length > 0)
                {
                    UnityEngine.Debug.Log($"[{testCase.Name}] 详细日志:\n{result.DetailLog}");
                }
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result = new TestResult
                {
                    Success = false,
                    Message = $"测试执行异常: {ex.Message}",
                    ExecutionTime = (float)stopwatch.Elapsed.TotalSeconds
                };
                result.DetailLog.AppendLine($"异常堆栈: {ex.StackTrace}");
                
                XLog.ErrorFormat("✗ 测试 '{0}' 异常 - {1}", testCase.Name, ex.Message);
                UnityEngine.Debug.LogException(ex);
            }

            _testHistory.Add(result);
            XLog.InfoFormat("=================================================================\n");

            return result;
        }

        /// <summary>运行所有测试</summary>
        public async UniTask<List<TestResult>> RunAllTestsAsync()
        {
            var results = new List<TestResult>();
            
            if (_testCases.Count == 0)
            {
                XLog.Warning("没有已注册的测试用例");
                return results;
            }

            XLog.InfoFormat("\n=================================================================");
            XLog.InfoFormat("开始批量执行 {0} 个测试用例", _testCases.Count);
            XLog.InfoFormat("=================================================================\n");

            var totalStopwatch = Stopwatch.StartNew();

            foreach (var testCase in _testCases.Values)
            {
                var result = await RunTestAsync(testCase.Name);
                results.Add(result);
                
                // 每个测试之间稍作延迟，避免连续执行造成性能问题
                await UniTask.Yield();
            }

            totalStopwatch.Stop();

            // 输出汇总
            var successCount = results.Count(r => r.Success);
            var failCount = results.Count - successCount;
            var totalTime = (float)totalStopwatch.Elapsed.TotalSeconds;

            XLog.InfoFormat("\n=================================================================");
            XLog.InfoFormat("批量测试完成");
            XLog.InfoFormat("总测试数: {0}, 成功: {1}, 失败: {2}", results.Count, successCount, failCount);
            XLog.InfoFormat("总耗时: {0:F2}s", totalTime);
            XLog.InfoFormat("=================================================================\n");

            return results;
        }

        #endregion

        #region 历史记录管理

        /// <summary>获取测试历史</summary>
        public IReadOnlyList<TestResult> GetTestHistory()
        {
            return _testHistory.AsReadOnly();
        }

        /// <summary>清除测试历史</summary>
        public void ClearHistory()
        {
            _testHistory.Clear();
            XLog.Info("测试历史已清除");
        }

        #endregion

        public override UniTask OnDestroy()
        {
            _testCases.Clear();
            _testHistory.Clear();
            XLog.Info("RuntimeTestManager 已销毁");
            return UniTask.CompletedTask;
        }
    }
#endif
}
