using System;
using System.IO;
using UnityEngine;
using XM.ConfigNew.CodeGen;
using XM.ConfigNew.Tests.Data;
using XM.ConfigNew.Tools;

namespace XM.ConfigNew.Tests
{
    /// <summary>
    /// 重新生成技能配置的 Unmanaged 和 Helper 文件
    /// 用于修复继承字段问题
    /// </summary>
    public static class RegenerateSkillConfigs
    {
        [UnityEditor.MenuItem("XMFrame/ConfigNew/重新生成技能配置")]
        public static void Regenerate()
        {
            var outputDir = Path.Combine(UnityEngine.Application.dataPath, "XMFrame", "ConfigNew", "Generated");
            Directory.CreateDirectory(outputDir);
            
            var manager = new CodeGenerationManager();
            
            try
            {
                // 生成 BaseSkillConfig
                Debug.Log("开始生成 BaseSkillConfig...");
                var baseFiles = manager.GenerateForType(typeof(BaseSkillConfig), outputDir);
                Debug.Log($"BaseSkillConfig 生成完成: {baseFiles.Count} 个文件");
                foreach (var file in baseFiles)
                    Debug.Log($"  - {Path.GetFileName(file)}");
                
                // 生成 ActiveSkillConfig
                Debug.Log("开始生成 ActiveSkillConfig...");
                var activeFiles = manager.GenerateForType(typeof(ActiveSkillConfig), outputDir);
                Debug.Log($"ActiveSkillConfig 生成完成: {activeFiles.Count} 个文件");
                foreach (var file in activeFiles)
                    Debug.Log($"  - {Path.GetFileName(file)}");
                
                // 生成 PassiveSkillConfig
                Debug.Log("开始生成 PassiveSkillConfig...");
                var passiveFiles = manager.GenerateForType(typeof(PassiveSkillConfig), outputDir);
                Debug.Log($"PassiveSkillConfig 生成完成: {passiveFiles.Count} 个文件");
                foreach (var file in passiveFiles)
                    Debug.Log($"  - {Path.GetFileName(file)}");
                
                UnityEditor.AssetDatabase.Refresh();
                
                Debug.Log("所有技能配置生成完成！");
            }
            catch (Exception ex)
            {
                Debug.LogError($"生成失败: {ex.Message}\n{ex.StackTrace}");
            }
        }
        
        [UnityEditor.MenuItem("XMFrame/ConfigNew/测试字段分析")]
        public static void TestFieldAnalysis()
        {
            Debug.Log("========== BaseSkillConfig 字段分析 ==========");
            var baseMetadata = TypeAnalyzer.AnalyzeConfigType(typeof(BaseSkillConfig));
            Debug.Log($"字段数: {baseMetadata.Fields?.Count ?? 0}");
            if (baseMetadata.Fields != null)
            {
                foreach (var field in baseMetadata.Fields)
                {
                    Debug.Log($"  - {field.FieldName}: {field.ManagedFieldTypeName}");
                }
            }
            
            Debug.Log("\n========== ActiveSkillConfig 字段分析 ==========");
            var activeMetadata = TypeAnalyzer.AnalyzeConfigType(typeof(ActiveSkillConfig));
            Debug.Log($"字段数: {activeMetadata.Fields?.Count ?? 0}");
            if (activeMetadata.Fields != null)
            {
                foreach (var field in activeMetadata.Fields)
                {
                    Debug.Log($"  - {field.FieldName}: {field.ManagedFieldTypeName}");
                }
            }
            
            Debug.Log("\n========== PassiveSkillConfig 字段分析 ==========");
            var passiveMetadata = TypeAnalyzer.AnalyzeConfigType(typeof(PassiveSkillConfig));
            Debug.Log($"字段数: {passiveMetadata.Fields?.Count ?? 0}");
            if (passiveMetadata.Fields != null)
            {
                foreach (var field in passiveMetadata.Fields)
                {
                    Debug.Log($"  - {field.FieldName}: {field.ManagedFieldTypeName}");
                }
            }
        }
    }
}
