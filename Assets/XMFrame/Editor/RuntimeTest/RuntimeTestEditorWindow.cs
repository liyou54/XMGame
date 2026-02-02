using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using XM.RuntimeTest;
using XM.RuntimeTest.Tests;

namespace XM.Editor.RuntimeTest
{
    /// <summary>
    /// 运行时测试管理器编辑器窗口
    /// </summary>
    public class RuntimeTestEditorWindow : EditorWindow
    {
        private Vector2 _scrollPosition;
        private Dictionary<string, bool> _testSelections = new Dictionary<string, bool>();
        private Dictionary<string, bool> _testFoldouts = new Dictionary<string, bool>();
        private bool _isRunning = false;
        private string _currentRunningTest = "";
        
        // 历史记录相关
        private Vector2 _historyScrollPosition;
        private bool _showHistory = true;

        [MenuItem("XMFrame/Tests/Runtime Test Manager")]
        public static void ShowWindow()
        {
            var window = GetWindow<RuntimeTestEditorWindow>("运行时测试管理器");
            window.minSize = new Vector2(600, 400);
            window.Show();
        }

        private void OnEnable()
        {
            // 当窗口打开时注册内置测试用例（如果游戏正在运行）
            RegisterBuiltInTests();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(10);

            // 标题
            EditorGUILayout.LabelField("运行时测试管理器", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("在游戏运行时执行各种测试用例，验证系统功能", MessageType.Info);
            
            EditorGUILayout.Space(5);

            // 检查游戏是否在运行
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("请先启动游戏（进入播放模式）", MessageType.Warning);
                return;
            }

            // 检查 RuntimeTestManager 是否可用
            if (IRuntimeTestManager.I == null)
            {
                EditorGUILayout.HelpBox("RuntimeTestManager 未初始化，请等待游戏初始化完成", MessageType.Warning);
                
                if (GUILayout.Button("刷新", GUILayout.Width(100)))
                {
                    RegisterBuiltInTests();
                }
                return;
            }

            // 注册内置测试（如果还没注册）
            RegisterBuiltInTests();

            DrawTestCaseList();
            
            EditorGUILayout.Space(10);
            
            DrawControlButtons();
            
            EditorGUILayout.Space(10);
            
            DrawTestHistory();
        }

        /// <summary>注册内置测试用例</summary>
        private void RegisterBuiltInTests()
        {
            if (!Application.isPlaying || IRuntimeTestManager.I == null)
                return;

            var manager = IRuntimeTestManager.I;

            // 注册 ConfigUnmanaged 测试
            if (!manager.IsTestCaseRegistered("ConfigUnmanaged数据打印"))
            {
                manager.RegisterTestCase(new ConfigUnmanagedTestCase());
            }

            // 在此可以添加更多内置测试用例
        }

        /// <summary>绘制测试用例列表</summary>
        private void DrawTestCaseList()
        {
            EditorGUILayout.LabelField("已注册测试用例", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            var tests = IRuntimeTestManager.I.GetAllTests();

            if (tests == null || tests.Count == 0)
            {
                EditorGUILayout.LabelField("  (无可用测试用例)", EditorStyles.miniLabel);
            }
            else
            {
                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(200));

                foreach (var testCase in tests)
                {
                    DrawTestCaseItem(testCase);
                }

                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>绘制单个测试用例</summary>
        private void DrawTestCaseItem(ITestCase testCase)
        {
            EditorGUILayout.BeginHorizontal();

            // 复选框
            if (!_testSelections.ContainsKey(testCase.Name))
            {
                _testSelections[testCase.Name] = false;
            }
            _testSelections[testCase.Name] = EditorGUILayout.Toggle(_testSelections[testCase.Name], GUILayout.Width(20));

            // 折叠箭头和名称
            if (!_testFoldouts.ContainsKey(testCase.Name))
            {
                _testFoldouts[testCase.Name] = false;
            }
            
            _testFoldouts[testCase.Name] = EditorGUILayout.Foldout(_testFoldouts[testCase.Name], testCase.Name, true);

            // 运行按钮
            GUI.enabled = !_isRunning;
            if (GUILayout.Button("运行", GUILayout.Width(60)))
            {
                RunTest(testCase.Name);
            }
            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            // 展开显示描述
            if (_testFoldouts[testCase.Name])
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("描述:", EditorStyles.miniLabel);
                EditorGUILayout.LabelField(testCase.Description, EditorStyles.wordWrappedMiniLabel);
                EditorGUI.indentLevel--;
            }
        }

        /// <summary>绘制控制按钮</summary>
        private void DrawControlButtons()
        {
            EditorGUILayout.BeginHorizontal();

            GUI.enabled = !_isRunning;
            
            // 全选/取消全选
            if (GUILayout.Button("全选", GUILayout.Width(80)))
            {
                var tests = IRuntimeTestManager.I.GetAllTests();
                foreach (var test in tests)
                {
                    _testSelections[test.Name] = true;
                }
            }

            if (GUILayout.Button("取消全选", GUILayout.Width(80)))
            {
                var keys = _testSelections.Keys.ToList();
                foreach (var key in keys)
                {
                    _testSelections[key] = false;
                }
            }

            GUILayout.FlexibleSpace();

            // 运行选中的测试
            var selectedCount = _testSelections.Count(kvp => kvp.Value);
            if (GUILayout.Button($"运行选中 ({selectedCount})", GUILayout.Width(120)))
            {
                RunSelectedTests();
            }

            // 运行所有测试
            if (GUILayout.Button("运行所有", GUILayout.Width(80)))
            {
                RunAllTests();
            }

            GUI.enabled = true;

            EditorGUILayout.EndHorizontal();

            // 显示运行状态
            if (_isRunning)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"正在运行: {_currentRunningTest}...", EditorStyles.boldLabel);
                if (GUILayout.Button("强制停止", GUILayout.Width(80)))
                {
                    _isRunning = false;
                    _currentRunningTest = "";
                }
                EditorGUILayout.EndHorizontal();
            }
        }

        /// <summary>绘制测试历史</summary>
        private void DrawTestHistory()
        {
            _showHistory = EditorGUILayout.Foldout(_showHistory, "测试历史", true, EditorStyles.foldoutHeader);

            if (!_showHistory)
                return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            var history = IRuntimeTestManager.I.GetTestHistory();

            if (history == null || history.Count == 0)
            {
                EditorGUILayout.LabelField("  (无历史记录)", EditorStyles.miniLabel);
            }
            else
            {
                // 清除按钮
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"共 {history.Count} 条记录", EditorStyles.miniLabel);
                if (GUILayout.Button("清除历史", GUILayout.Width(80)))
                {
                    IRuntimeTestManager.I.ClearHistory();
                }
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(5);

                _historyScrollPosition = EditorGUILayout.BeginScrollView(_historyScrollPosition, GUILayout.Height(150));

                // 倒序显示（最新的在上面）
                for (int i = history.Count - 1; i >= 0; i--)
                {
                    var result = history[i];
                    DrawTestResult(result, i + 1);
                }

                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.EndVertical();
        }

        /// <summary>绘制测试结果</summary>
        private void DrawTestResult(TestResult result, int index)
        {
            var style = new GUIStyle(EditorStyles.helpBox);
            if (result.Success)
            {
                style.normal.textColor = Color.green;
            }
            else
            {
                style.normal.textColor = Color.red;
            }

            EditorGUILayout.BeginVertical(style);

            // 状态图标和消息
            var statusIcon = result.Success ? "✓" : "✗";
            EditorGUILayout.LabelField($"[{index}] {statusIcon} {result.Message}", EditorStyles.boldLabel);

            // 详细信息
            if (!string.IsNullOrEmpty(result.Statistics))
            {
                EditorGUILayout.LabelField($"  统计: {result.Statistics}", EditorStyles.miniLabel);
            }
            EditorGUILayout.LabelField($"  耗时: {result.ExecutionTime:F2}s", EditorStyles.miniLabel);

            // 显示详细日志按钮
            if (result.DetailLog != null && result.DetailLog.Length > 0)
            {
                if (GUILayout.Button("查看详细日志", GUILayout.Width(100)))
                {
                    Debug.Log($"=== 测试详细日志 ===\n{result.DetailLog}");
                }
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(3);
        }

        #region 测试执行

        /// <summary>运行单个测试</summary>
        private async void RunTest(string testName)
        {
            if (_isRunning)
            {
                Debug.LogWarning("已有测试正在运行，请等待完成");
                return;
            }

            _isRunning = true;
            _currentRunningTest = testName;

            try
            {
                var result = await IRuntimeTestManager.I.RunTestAsync(testName);
                
                // 刷新窗口显示结果
                Repaint();

                if (result.Success)
                {
                    Debug.Log($"测试 '{testName}' 完成: {result.Message}");
                }
                else
                {
                    Debug.LogError($"测试 '{testName}' 失败: {result.Message}");
                }
            }
            finally
            {
                _isRunning = false;
                _currentRunningTest = "";
                Repaint();
            }
        }

        /// <summary>运行选中的测试</summary>
        private async void RunSelectedTests()
        {
            if (_isRunning)
            {
                Debug.LogWarning("已有测试正在运行，请等待完成");
                return;
            }

            var selectedTests = _testSelections.Where(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();
            if (selectedTests.Count == 0)
            {
                Debug.LogWarning("请先选择要运行的测试用例");
                return;
            }

            _isRunning = true;

            try
            {
                foreach (var testName in selectedTests)
                {
                    _currentRunningTest = testName;
                    Repaint();

                    await IRuntimeTestManager.I.RunTestAsync(testName);
                }

                Debug.Log($"批量测试完成，共运行 {selectedTests.Count} 个测试");
            }
            finally
            {
                _isRunning = false;
                _currentRunningTest = "";
                Repaint();
            }
        }

        /// <summary>运行所有测试</summary>
        private async void RunAllTests()
        {
            if (_isRunning)
            {
                Debug.LogWarning("已有测试正在运行，请等待完成");
                return;
            }

            _isRunning = true;
            _currentRunningTest = "所有测试";

            try
            {
                var results = await IRuntimeTestManager.I.RunAllTestsAsync();
                
                var successCount = results.Count(r => r.Success);
                var failCount = results.Count - successCount;

                Debug.Log($"所有测试完成 - 成功: {successCount}, 失败: {failCount}");
                Repaint();
            }
            finally
            {
                _isRunning = false;
                _currentRunningTest = "";
                Repaint();
            }
        }

        #endregion
    }
}
