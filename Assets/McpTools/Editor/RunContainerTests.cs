using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace McpTools.Editor
{
    /// <summary>
    /// 用于 MCP 或菜单触发的 Container 测试运行器。
    /// 可程序化运行 XM.Utils.Container.Tests 并输出结果到控制台或文件。
    /// </summary>
    public static class RunContainerTests
    {
        private const string AssemblyName = "XM.Utils.Container.Tests";
        private const string MenuRun = "Tools/MCP/Run Container Tests";
        private const string MenuRunAndSave = "Tools/MCP/Run Container Tests (Save Results)";

        [MenuItem(MenuRun)]
        public static void RunFromMenu()
        {
            Run(writeResultsToFile: false);
        }

        [MenuItem(MenuRunAndSave)]
        public static void RunAndSaveFromMenu()
        {
            Run(writeResultsToFile: true);
        }

        /// <summary>
        /// 程序化运行 Container 测试。
        /// </summary>
        /// <param name="writeResultsToFile">为 true 时把结果写入项目根目录的 TestResults_Container.txt</param>
        public static void Run(bool writeResultsToFile = false)
        {
            var api = ScriptableObject.CreateInstance<TestRunnerApi>();
            var filter = new Filter
            {
                testMode = TestMode.EditMode,
                assemblyNames = new[] { AssemblyName }
            };
            var settings = new ExecutionSettings(filter);

            var collector = new ResultCollector(writeResultsToFile);
            api.RegisterCallbacks(collector, 0);

            try
            {
                string runId = api.Execute(settings);
                Debug.Log($"[McpTools] Container 测试已启动，RunId: {runId}");
            }
            finally
            {
                api.UnregisterCallbacks(collector);
            }
        }

        private class ResultCollector : ICallbacks
        {
            private readonly bool _writeToFile;
            private readonly List<string> _lines = new List<string>();

            public ResultCollector(bool writeToFile)
            {
                _writeToFile = writeToFile;
            }

            public void RunStarted(ITestAdaptor testsToRun)
            {
                _lines.Clear();
                _lines.Add($"RunStarted: {testsToRun.Name}");
            }

            public void RunFinished(ITestResultAdaptor result)
            {
                int passed = result.PassCount;
                int failed = result.FailCount;
                int skipped = result.SkipCount;
                int total = passed + failed + skipped + result.InconclusiveCount;
                _lines.Add($"RunFinished: Total={total}, Passed={passed}, Failed={failed}, Skipped={skipped}");
                if (failed > 0)
                    AppendFailures(result);
                string text = string.Join(Environment.NewLine, _lines);
                Debug.Log($"[McpTools] {text}");
                if (_writeToFile)
                {
                    string path = Path.Combine(Application.dataPath, "..", "TestResults_Container.txt");
                    File.WriteAllText(path, text);
                    Debug.Log($"[McpTools] 结果已写入: {path}");
                }
            }

            private void AppendFailures(ITestResultAdaptor result)
            {
                if (result.HasChildren)
                {
                    foreach (var child in result.Children)
                        AppendFailures(child);
                    return;
                }
                if (result.ResultState != "Passed" && !string.IsNullOrEmpty(result.Message))
                {
                    _lines.Add($"  FAIL: {result.FullName}");
                    _lines.Add($"    {result.Message}");
                    if (!string.IsNullOrEmpty(result.StackTrace))
                        _lines.Add($"    {result.StackTrace?.Replace("\n", "\n    ")}");
                }
            }

            public void TestStarted(ITestAdaptor test) { }
            public void TestFinished(ITestResultAdaptor result) { }
        }
    }
}
