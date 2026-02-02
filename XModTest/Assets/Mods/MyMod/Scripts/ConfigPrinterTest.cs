using UnityEngine;
using XM;

namespace MyMod
{
#if UNITY_EDITOR
    /// <summary>
    /// 配置打印测试脚本 - 可以在运行时通过菜单或代码调用
    /// </summary>
    public static class ConfigPrinterTest
    {
        /// <summary>
        /// 测试打印所有配置 - 可以从外部调用
        /// </summary>
        [UnityEditor.MenuItem("MyMod/测试/打印所有配置到控制台")]
        public static void TestPrintAllToConsole()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("请先运行游戏!");
                return;
            }

            Debug.Log("开始打印所有 MyMod 配置...");
            
            try
            {
                string result = ConfigPrinter.PrintAllMyModConfigs();
                Debug.Log(result);
                XLog.Info("配置打印完成");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"打印配置失败: {ex}");
                XLog.Error($"打印配置失败: {ex}");
            }
        }

        /// <summary>
        /// 测试打印 MyItemConfig
        /// </summary>
        [UnityEditor.MenuItem("MyMod/测试/打印 MyItemConfig")]
        public static void TestPrintMyItemConfig()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("请先运行游戏!");
                return;
            }

            Debug.Log("开始打印 MyItemConfig...");
            
            try
            {
                string result = ConfigPrinter.PrintMyItemConfigs();
                Debug.Log(result);
                XLog.Info("MyItemConfig 打印完成");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"打印 MyItemConfig 失败: {ex}");
                XLog.Error($"打印 MyItemConfig 失败: {ex}");
            }
        }

        /// <summary>
        /// 测试打印 TestConfig
        /// </summary>
        [UnityEditor.MenuItem("MyMod/测试/打印 TestConfig")]
        public static void TestPrintTestConfig()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("请先运行游戏!");
                return;
            }

            Debug.Log("开始打印 TestConfig...");
            
            try
            {
                string result = ConfigPrinter.PrintTestConfigs();
                Debug.Log(result);
                XLog.Info("TestConfig 打印完成");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"打印 TestConfig 失败: {ex}");
                XLog.Error($"打印 TestConfig 失败: {ex}");
            }
        }

        /// <summary>
        /// 保存打印结果到文件
        /// </summary>
        [UnityEditor.MenuItem("MyMod/测试/打印并保存到文件")]
        public static void TestPrintAndSaveToFile()
        {
            if (!Application.isPlaying)
            {
                Debug.LogWarning("请先运行游戏!");
                return;
            }

            Debug.Log("开始打印并保存配置...");
            
            try
            {
                string result = ConfigPrinter.PrintAllMyModConfigs();
                
                // 保存到项目根目录
                string filePath = System.IO.Path.Combine(
                    UnityEngine.Application.dataPath, 
                    "..", 
                    $"MyMod_Config_Print_{System.DateTime.Now:yyyyMMdd_HHmmss}.txt"
                );
                
                System.IO.File.WriteAllText(filePath, result);
                
                Debug.Log($"配置已保存到: {filePath}");
                XLog.Info($"配置已保存到: {filePath}");
                
                // 在资源管理器中显示文件
                UnityEditor.EditorUtility.RevealInFinder(filePath);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"保存配置失败: {ex}");
                XLog.Error($"保存配置失败: {ex}");
            }
        }
    }
#endif
}
