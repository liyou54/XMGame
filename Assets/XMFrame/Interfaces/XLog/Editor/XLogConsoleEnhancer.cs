using System;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace XM.Editor
{
    /// <summary>
    /// XLog控制台增强器，用于增强日志的点击跳转功能
    /// 使用OnOpenAsset特性拦截双击事件，实现跳转到实际调用位置
    /// </summary>
    internal sealed class XLogConsoleEnhancer
    {
        private static XLogConsoleEnhancer _current;
        private static XLogConsoleEnhancer Current
        {
            get
            {
                if (_current == null)
                {
                    _current = new XLogConsoleEnhancer();
                }
                return _current;
            }
        }

        private Type _consoleWindowType;
        private FieldInfo _activeTextInfo;
        private FieldInfo _consoleWindowInfo;
        private MethodInfo _setActiveEntry;
        private object[] _setActiveEntryArgs;
        private object _consoleWindow;

        private static readonly Regex XLogMessagePattern = new Regex(
            @"\[\d{4}-\d{2}-\d{2}\s+\d{2}:\d{2}:\d{2}\.\d{3}\]",
            RegexOptions.Compiled
        );

        private XLogConsoleEnhancer()
        {
            try
            {
                _consoleWindowType = Type.GetType("UnityEditor.ConsoleWindow,UnityEditor");
                if (_consoleWindowType != null)
                {
                    _activeTextInfo = _consoleWindowType.GetField("m_ActiveText", BindingFlags.Instance | BindingFlags.NonPublic);
                    _consoleWindowInfo = _consoleWindowType.GetField("ms_ConsoleWindow", BindingFlags.Static | BindingFlags.NonPublic);
                    _setActiveEntry = _consoleWindowType.GetMethod("SetActiveEntry", BindingFlags.Instance | BindingFlags.NonPublic);
                    _setActiveEntryArgs = new object[] { null };
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[XLogConsoleEnhancer] 初始化失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 拦截资源打开事件，当双击.cs文件时检查是否是XLog日志
        /// </summary>
        [OnOpenAsset(0)]
        private static bool OnOpenAsset(int instanceID, int line)
        {
            UnityEngine.Object instance = EditorUtility.InstanceIDToObject(instanceID);
            string assetPath = AssetDatabase.GetAssetOrScenePath(instance);
            
            // 只处理.cs文件
            if (assetPath.EndsWith(".cs"))
            {
                return Current.OpenAsset();
            }
            return false;
        }

        /// <summary>
        /// 打开资源文件
        /// </summary>
        private bool OpenAsset()
        {
            string stackTrace = GetStackTrace();
            if (string.IsNullOrEmpty(stackTrace))
            {
                return false;
            }

            // 检查是否是XLog输出的日志
            if (!IsXLogMessage(stackTrace))
            {
                return false;
            }

            // 解析堆栈跟踪，找到实际调用位置
            // 堆栈跟踪格式通常是：
            // XM.XLog:OutputToUnityConsole (at Assets/XM/Utils/XLog/XLog.cs:952)
            // XM.XLog:InfoFormat<string> (at Assets/XM/Utils/XLog/XLog.cs:233)
            // XM.GameMain/<CreateManagers>d__8:MoveNext () (at Assets/XM/GameMain.cs:189)
            // 我们需要找到第一个非XLog的调用位置（跳过XLog内部的方法）
            
            string[] paths = stackTrace.Split('\n');
            int xlogCallCount = 0;
            
            foreach (string path in paths)
            {
                if (path.Contains(" (at "))
                {
                    // 检查是否是XLog内部的方法调用
                    if (path.Contains("XM.XLog:") || path.Contains("XM.Utils.XLog:"))
                    {
                        xlogCallCount++;
                        continue;
                    }
                    
                    // 找到第一个非XLog的调用位置，这就是实际调用位置
                    return OpenScriptAsset(path);
                }
            }
            
            return false;
        }

        /// <summary>
        /// 打开脚本资源并跳转到指定行号
        /// </summary>
        private bool OpenScriptAsset(string path)
        {
            try
            {
                // 解析路径格式: "  at Assets/XM/GameMain.cs:189"
                int startIndex = path.IndexOf(" (at ") + 5;
                if (startIndex < 5) return false;
                
                int endIndex = path.IndexOf(".cs:");
                if (endIndex < 0) return false;
                
                string filePath = path.Substring(startIndex, endIndex - startIndex + 3); // 包含.cs
                string lineStr = path.Substring(endIndex + 4); // 跳过 ".cs:"
                
                // 移除可能的括号
                lineStr = lineStr.TrimEnd(')', ' ', '\r', '\n');
                
                TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(filePath);
                if (asset != null)
                {
                    if (int.TryParse(lineStr, out int line))
                    {
                        // 设置控制台窗口的激活条目
                        object consoleWindow = GetConsoleWindow();
                        if (consoleWindow != null && _setActiveEntry != null)
                        {
                            _setActiveEntry.Invoke(consoleWindow, _setActiveEntryArgs);
                        }

                        // 打开文件并跳转到指定行
                        EditorGUIUtility.PingObject(asset);
                        AssetDatabase.OpenAsset(asset, line);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[XLogConsoleEnhancer] 打开文件失败: {ex.Message}");
            }
            
            return false;
        }

        /// <summary>
        /// 获取当前控制台窗口的堆栈跟踪文本
        /// </summary>
        private string GetStackTrace()
        {
            object consoleWindow = GetConsoleWindow();
            if (consoleWindow != null && _activeTextInfo != null)
            {
                // 检查控制台窗口是否是当前焦点窗口
                if (consoleWindow == EditorWindow.focusedWindow as object)
                {
                    object value = _activeTextInfo.GetValue(consoleWindow);
                    return value != null ? value.ToString() : "";
                }
            }
            return "";
        }

        /// <summary>
        /// 获取控制台窗口实例
        /// </summary>
        private object GetConsoleWindow()
        {
            if (_consoleWindow == null && _consoleWindowInfo != null)
            {
                _consoleWindow = _consoleWindowInfo.GetValue(null);
            }
            return _consoleWindow;
        }

        /// <summary>
        /// 检查是否是XLog输出的日志消息
        /// </summary>
        private bool IsXLogMessage(string text)
        {
            return XLogMessagePattern.IsMatch(text);
        }

        /// <summary>
        /// 菜单项：显示XLog使用说明
        /// </summary>
        [MenuItem("Tools/XLog/使用说明")]
        private static void ShowUsageInfo()
        {
            EditorUtility.DisplayDialog("XLog 使用说明", 
                "XLog 日志系统已支持点击跳转功能！\n\n" +
                "使用方法：\n" +
                "1. 在Unity Console窗口中双击任意XLog日志\n" +
                "2. 系统会自动打开文件并跳转到实际调用位置\n" +
                "3. 跳过XLog内部方法，直接跳转到您的代码位置\n\n" +
                "工作原理：\n" +
                "- 使用OnOpenAsset特性拦截双击事件\n" +
                "- 解析堆栈跟踪，找到第一个非XLog的调用位置\n" +
                "- 使用AssetDatabase.OpenAsset打开文件\n\n" +
                "注意事项：\n" +
                "- 确保代码已编译（没有编译错误）\n" +
                "- 文件路径必须是相对于Assets的路径\n" +
                "- 行号必须正确对应代码位置", 
                "确定");
        }

        /// <summary>
        /// 菜单项：测试日志跳转功能
        /// </summary>
        [MenuItem("Tools/XLog/测试日志跳转")]
        private static void TestLogJump()
        {
            XM.XLog.InfoFormat("这是一条测试日志，双击此日志应该可以跳转到实际调用位置");
            Debug.Log("测试日志已输出，请在Console窗口中双击日志查看跳转效果");
        }
    }
}
