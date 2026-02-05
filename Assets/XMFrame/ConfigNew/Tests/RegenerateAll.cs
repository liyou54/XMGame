using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using XM.ConfigNew.CodeGen;
using XM.ConfigNew.Tests.Data;
using XM.ConfigNew.Tools;

namespace XM.ConfigNew.Tests
{
    /// <summary>
    /// 重新生成所有配置类的 Unmanaged 和 Helper
    /// </summary>
    public static class RegenerateAll
    {
        [MenuItem("XMFrame/ConfigNew/重新生成所有测试配置")]
        public static void RegenerateAllTestConfigs()
        {
            var outputDir = Path.Combine(Application.dataPath, "XMFrame", "ConfigNew", "Generated");
            Directory.CreateDirectory(outputDir);
            
            var configTypes = new[]
            {
                typeof(AttributeConfig),
                typeof(PriceConfig),
                typeof(EffectConfig),
                typeof(BaseItemConfig),
                typeof(BaseSkillConfig),
                typeof(ActiveSkillConfig),
                typeof(PassiveSkillConfig),
                typeof(ComplexItemConfig),
                typeof(QuestConfig),
                typeof(LinkParentConfig),
                typeof(SingleLinkChildConfig),
                typeof(ListLinkChildConfig),
                typeof(ContainerConverterConfig)
            };
            
            var manager = new CodeGenerationManager();
            int successCount = 0;
            int errorCount = 0;
            
            foreach (var configType in configTypes)
            {
                try
                {
                    Debug.Log($"开始生成: {configType.Name}");
                    var files = manager.GenerateForType(configType, outputDir);
                    Debug.Log($"  ✅ 生成成功: {files.Count} 个文件");
                    foreach (var file in files)
                    {
                        Debug.Log($"    - {Path.GetFileName(file)}");
                    }
                    successCount++;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"  ❌ 生成失败: {configType.Name}\n{ex.Message}");
                    errorCount++;
                }
            }
            
            AssetDatabase.Refresh();
            
            Debug.Log($"\n========== 生成完成 ==========");
            Debug.Log($"成功: {successCount} 个");
            Debug.Log($"失败: {errorCount} 个");
        }
        
        [MenuItem("XMFrame/ConfigNew/只生成 ActiveSkillConfig")]
        public static void RegenerateActiveSkillConfig()
        {
            var outputDir = Path.Combine(Application.dataPath, "XMFrame", "ConfigNew", "Generated");
            var manager = new CodeGenerationManager();
            
            try
            {
                Debug.Log("========== 生成 ActiveSkillConfig ==========");
                
                // 先分析元数据
                var metadata = TypeAnalyzer.AnalyzeConfigType(typeof(ActiveSkillConfig));
                Debug.Log($"ManagedType: {metadata.ManagedType?.Name}");
                Debug.Log($"UnmanagedType: {metadata.UnmanagedType?.Name}");
                Debug.Log($"ManagedTypeName: {metadata.ManagedTypeName}");
                Debug.Log($"UnmanagedTypeName: {metadata.UnmanagedTypeName}");
                Debug.Log($"字段数: {metadata.Fields?.Count ?? 0}");
                
                if (metadata.Fields != null)
                {
                    foreach (var field in metadata.Fields)
                    {
                        var declaringType = field.FieldReflectionInfo?.DeclaringType?.Name ?? "?";
                        Debug.Log($"  - {field.FieldName} (from {declaringType}): {field.ManagedFieldTypeName} -> {field.UnmanagedFieldTypeName}");
                    }
                }
                
                // 生成文件
                var files = manager.GenerateForType(typeof(ActiveSkillConfig), outputDir);
                
                Debug.Log($"\n生成的文件:");
                foreach (var file in files)
                {
                    Debug.Log($"  - {Path.GetFileName(file)}");
                    
                    // 显示部分内容
                    if (file.EndsWith("ClassHelper.Gen.cs"))
                    {
                        var content = File.ReadAllText(file);
                        var lines = content.Split('\n');
                        Debug.Log("\n前50行:");
                        for (int i = 0; i < Math.Min(50, lines.Length); i++)
                        {
                            Debug.Log($"{i + 1}: {lines[i]}");
                        }
                    }
                }
                
                AssetDatabase.Refresh();
                Debug.Log("\n✅ 生成成功！");
            }
            catch (Exception ex)
            {
                Debug.LogError($"❌ 生成失败: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
