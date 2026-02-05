using System;
using UnityEngine;
using XM.ConfigNew.CodeGen;
using XM.ConfigNew.Tests.Data;
using XM.ConfigNew.Tools;

namespace XM.ConfigNew.Tests
{
    /// <summary>
    /// 调试生成器 - 输出生成的代码到控制台
    /// </summary>
    public static class DebugGenerator
    {
        [UnityEditor.MenuItem("XMFrame/ConfigNew/调试: 生成 ActiveSkillConfig")]
        public static void DebugActiveSkillConfig()
        {
            try
            {
                Debug.Log("========== 开始调试 ActiveSkillConfig ==========");
                
                // 1. 分析元数据
                var metadata = TypeAnalyzer.AnalyzeConfigType(typeof(ActiveSkillConfig));
                Debug.Log($"配置类型: {metadata.ManagedTypeName}");
                Debug.Log($"Unmanaged类型: {metadata.UnmanagedTypeName}");
                Debug.Log($"Helper类型: {metadata.HelperTypeName}");
                Debug.Log($"字段数: {metadata.Fields?.Count ?? 0}");
                
                if (metadata.Fields != null)
                {
                    Debug.Log("\n字段列表:");
                    foreach (var field in metadata.Fields)
                    {
                        Debug.Log($"  - {field.FieldName}: {field.ManagedFieldTypeName} -> {field.UnmanagedFieldTypeName}");
                    }
                }
                
                // 2. 生成 Unmanaged 结构体
                Debug.Log("\n========== 生成 Unmanaged 结构体 ==========");
                var unmanagedGen = new UnmanagedGenerator(metadata);
                var unmanagedCode = unmanagedGen.Generate();
                Debug.Log(unmanagedCode);
                
                // 3. 生成 XmlHelper
                Debug.Log("\n========== 生成 XmlHelper (ClassHelper) ==========");
                var helperGen = new XmlHelperGenerator(metadata);
                var helperCode = helperGen.Generate();
                Debug.Log(helperCode);
                
                Debug.Log("\n========== 调试完成 ==========");
            }
            catch (Exception ex)
            {
                Debug.LogError($"调试失败: {ex.Message}\n{ex.StackTrace}");
            }
        }
        
        [UnityEditor.MenuItem("XMFrame/ConfigNew/调试: 生成 BaseItemConfig")]
        public static void DebugBaseItemConfig()
        {
            try
            {
                Debug.Log("========== 开始调试 BaseItemConfig ==========");
                
                var metadata = TypeAnalyzer.AnalyzeConfigType(typeof(BaseItemConfig));
                Debug.Log($"配置类型: {metadata.ManagedTypeName}");
                Debug.Log($"Unmanaged类型: {metadata.UnmanagedTypeName}");
                Debug.Log($"字段数: {metadata.Fields?.Count ?? 0}");
                
                var helperGen = new XmlHelperGenerator(metadata);
                var helperCode = helperGen.Generate();
                Debug.Log("\n生成的 Helper 代码:");
                Debug.Log(helperCode);
            }
            catch (Exception ex)
            {
                Debug.LogError($"调试失败: {ex.Message}\n{ex.StackTrace}");
            }
        }
        
        [UnityEditor.MenuItem("XMFrame/ConfigNew/调试: 检查继承字段")]
        public static void DebugInheritedFields()
        {
            Debug.Log("========== ActiveSkillConfig 分析 ==========");
            
            var metadata = TypeAnalyzer.AnalyzeConfigType(typeof(ActiveSkillConfig));
            Debug.Log($"ActiveSkillConfig.BaseType = {typeof(ActiveSkillConfig).BaseType?.Name}");
            
            if (metadata.Fields != null)
            {
                Debug.Log($"\n总字段数: {metadata.Fields.Count}");
                Debug.Log("\n字段详情:");
                foreach (var field in metadata.Fields)
                {
                    var declaringType = field.FieldReflectionInfo?.DeclaringType?.Name ?? "Unknown";
                    Debug.Log($"  - {field.FieldName} (声明于: {declaringType})");
                }
            }
        }
    }
}
