using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using XM.ConfigNew.CodeGen;
using XM.ConfigNew.Tests.Data;
using XM.ConfigNew.Tools;

namespace XM.ConfigNew.Tests
{
    public static class TestGeneratorOutput
    {
        [MenuItem("XMFrame/ConfigNew/测试当前生成器输出")]
        public static void TestCurrentGeneratorOutput()
        {
            Debug.Log("========== 测试当前生成器 ==========\n");
            
            // 测试 ActiveSkillConfig
            var metadata = TypeAnalyzer.AnalyzeConfigType(typeof(ActiveSkillConfig));
            
            Debug.Log($"元数据信息:");
            Debug.Log($"  ManagedType: {metadata.ManagedType?.FullName}");
            Debug.Log($"  UnmanagedType: {metadata.UnmanagedType?.FullName}");
            Debug.Log($"  ManagedTypeName: {metadata.ManagedTypeName}");
            Debug.Log($"  UnmanagedTypeName: {metadata.UnmanagedTypeName}");
            
            // 测试 TypeHelper
            var qualifiedManaged = TypeHelper.GetGlobalQualifiedTypeName(metadata.ManagedType);
            var qualifiedUnmanaged = TypeHelper.GetGlobalQualifiedTypeName(metadata.UnmanagedType);
            
            Debug.Log($"\n全局限定名:");
            Debug.Log($"  Managed: {qualifiedManaged}");
            Debug.Log($"  Unmanaged: {qualifiedUnmanaged}");
            
            // 生成 Helper 代码
            var generator = new XmlHelperGenerator(metadata);
            var code = generator.Generate();
            
            // 查找关键行
            Debug.Log($"\n========== 检查生成的代码 ==========");
            var lines = code.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                
                // 查找类声明
                if (line.Contains("class") && line.Contains("ClassHelper"))
                {
                    Debug.Log($"[{i}] 类声明: {line}");
                }
                
                // 查找 AllocContainerWithFillImpl
                if (line.Contains("AllocContainerWithFillImpl"))
                {
                    Debug.Log($"\n[{i}] 找到 AllocContainerWithFillImpl:");
                    for (int j = i; j < Math.Min(i + 10, lines.Length); j++)
                    {
                        Debug.Log($"  [{j}] {lines[j]}");
                    }
                    break;
                }
            }
            
            // 保存到临时文件查看
            var tempPath = Path.Combine(Path.GetTempPath(), "ActiveSkillConfigClassHelper_Test.cs");
            File.WriteAllText(tempPath, code);
            Debug.Log($"\n完整代码已保存到: {tempPath}");
            System.Diagnostics.Process.Start("notepad.exe", tempPath);
        }
        
        [MenuItem("XMFrame/ConfigNew/生成三个技能配置")]
        public static void GenerateSkillConfigs()
        {
            var outputDir = Path.Combine(Application.dataPath, "XMFrame", "ConfigNew", "Generated");
            var manager = new CodeGenerationManager();
            
            var types = new[]
            {
                typeof(BaseSkillConfig),
                typeof(ActiveSkillConfig),
                typeof(PassiveSkillConfig)
            };
            
            foreach (var type in types)
            {
                try
                {
                    Debug.Log($"\n生成 {type.Name}...");
                    var files = manager.GenerateForType(type, outputDir);
                    
                    foreach (var file in files)
                    {
                        Debug.Log($"  ✅ {Path.GetFileName(file)}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"  ❌ {type.Name} 失败: {ex.Message}");
                }
            }
            
            AssetDatabase.Refresh();
            Debug.Log("\n生成完成！");
        }
    }
}
