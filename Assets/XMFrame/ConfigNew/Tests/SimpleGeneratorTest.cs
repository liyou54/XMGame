using System;
using System.IO;
using UnityEngine;
using XM.ConfigNew.CodeGen;
using XM.ConfigNew.Tests.Data;
using XM.ConfigNew.Tools;

namespace XM.ConfigNew.Tests
{
    /// <summary>
    /// 简单生成器测试 - 输出生成代码到临时文件
    /// </summary>
    public static class SimpleGeneratorTest
    {
        [UnityEditor.MenuItem("XMFrame/ConfigNew/测试: 生成 ActiveSkillConfig 到临时目录")]
        public static void GenerateActiveSkillToTemp()
        {
            try
            {
                var tempDir = Path.Combine(Path.GetTempPath(), "XMConfigNew_Test");
                Directory.CreateDirectory(tempDir);
                
                Debug.Log($"临时目录: {tempDir}");
                
                // 1. 分析元数据
                var metadata = TypeAnalyzer.AnalyzeConfigType(typeof(ActiveSkillConfig));
                Debug.Log($"元数据分析完成:");
                Debug.Log($"  ManagedTypeName: {metadata.ManagedTypeName}");
                Debug.Log($"  UnmanagedTypeName: {metadata.UnmanagedTypeName}");
                Debug.Log($"  HelperTypeName: {metadata.HelperTypeName}");
                Debug.Log($"  字段数: {metadata.Fields?.Count ?? 0}");
                
                // 2. 生成 Unmanaged
                var unmanagedGen = new UnmanagedGenerator(metadata);
                var unmanagedCode = unmanagedGen.Generate();
                var unmanagedPath = Path.Combine(tempDir, $"{metadata.UnmanagedTypeName}.Gen.cs");
                File.WriteAllText(unmanagedPath, unmanagedCode);
                Debug.Log($"\nUnmanaged 文件: {unmanagedPath}");
                Debug.Log($"文件大小: {new FileInfo(unmanagedPath).Length} 字节");
                
                // 3. 生成 Helper
                var helperGen = new XmlHelperGenerator(metadata);
                var helperCode = helperGen.Generate();
                var helperPath = Path.Combine(tempDir, $"{metadata.HelperTypeName}.Gen.cs");
                File.WriteAllText(helperPath, helperCode);
                Debug.Log($"\nHelper 文件: {helperPath}");
                Debug.Log($"文件大小: {new FileInfo(helperPath).Length} 字节");
                
                // 4. 检查关键代码
                Debug.Log("\n========== 检查 AllocContainerWithFillImpl 签名 ==========");
                var lines = helperCode.Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains("AllocContainerWithFillImpl"))
                    {
                        // 打印前后几行
                        for (int j = Math.Max(0, i - 2); j < Math.Min(lines.Length, i + 8); j++)
                        {
                            Debug.Log($"{j}: {lines[j]}");
                        }
                        break;
                    }
                }
                
                Debug.Log("\n========== 生成完成 ==========");
                Debug.Log($"请查看临时文件: {tempDir}");
                
                // 在文件资源管理器中打开
                System.Diagnostics.Process.Start("explorer.exe", tempDir);
            }
            catch (Exception ex)
            {
                Debug.LogError($"生成失败: {ex.Message}\n{ex.StackTrace}");
            }
        }
        
        [UnityEditor.MenuItem("XMFrame/ConfigNew/测试: 检查元数据的 UnmanagedTypeName")]
        public static void CheckUnmanagedTypeName()
        {
            Debug.Log("========== 检查各配置类的 UnmanagedTypeName ==========");
            
            var types = new[]
            {
                typeof(BaseSkillConfig),
                typeof(ActiveSkillConfig),
                typeof(PassiveSkillConfig)
            };
            
            foreach (var type in types)
            {
                var metadata = TypeAnalyzer.AnalyzeConfigType(type);
                Debug.Log($"\n{type.Name}:");
                Debug.Log($"  ManagedTypeName: {metadata.ManagedTypeName}");
                Debug.Log($"  UnmanagedTypeName: {metadata.UnmanagedTypeName}");
                Debug.Log($"  UnmanagedType: {metadata.UnmanagedType?.Name}");
            }
        }
    }
}
