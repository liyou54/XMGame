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
    /// 清理并重新生成 - 确保完全清理旧文件
    /// </summary>
    public static class CleanAndRegenerate
    {
        [MenuItem("XMFrame/ConfigNew/清理并重新生成 ComplexItemConfig")]
        public static void CleanAndRegenerateComplexItem()
        {
            var outputDir = Path.Combine(Application.dataPath, "XMFrame", "ConfigNew", "Generated");
            
            // 1. 删除所有 ComplexItemConfig 相关文件
            var filesToDelete = Directory.GetFiles(outputDir, "ComplexItemConfig*.cs")
                .Concat(Directory.GetFiles(outputDir, "ComplexItemConfig*.cs.meta"))
                .ToList();
            
            Debug.Log($"清理文件: {filesToDelete.Count} 个");
            foreach (var file in filesToDelete)
            {
                File.Delete(file);
                Debug.Log($"  删除: {Path.GetFileName(file)}");
            }
            
            AssetDatabase.Refresh();
            
            // 2. 等待一下让 Unity 更新
            System.Threading.Thread.Sleep(500);
            
            // 3. 重新生成
            try
            {
                Debug.Log("\n重新生成 ComplexItemConfig...");
                
                var metadata = TypeAnalyzer.AnalyzeConfigType(typeof(ComplexItemConfig));
                Debug.Log($"  字段数: {metadata.Fields?.Count ?? 0}");
                
                // 检查字段的类型名
                if (metadata.Fields != null)
                {
                    var indexField1 = metadata.Fields.FirstOrDefault(f => f.FieldName == "IndexField1");
                    if (indexField1 != null)
                    {
                        Debug.Log($"\n  IndexField1 类型分析:");
                        Debug.Log($"    ManagedFieldTypeName: {indexField1.ManagedFieldTypeName}");
                        Debug.Log($"    UnmanagedFieldTypeName: {indexField1.UnmanagedFieldTypeName}");
                        Debug.Log($"    IsEnum: {indexField1.TypeInfo?.IsEnum}");
                        Debug.Log($"    SingleValueType: {indexField1.TypeInfo?.SingleValueType?.FullName}");
                    }
                }
                
                var manager = new CodeGenerationManager();
                var files = manager.GenerateForType(typeof(ComplexItemConfig), outputDir);
                
                Debug.Log($"\n生成的文件: {files.Count} 个");
                foreach (var file in files)
                {
                    Debug.Log($"  ✅ {Path.GetFileName(file)}");
                }
                
                // 检查生成的 Helper 代码
                var helperFile = files.FirstOrDefault(f => f.Contains("ClassHelper"));
                if (helperFile != null && File.Exists(helperFile))
                {
                    var content = File.ReadAllText(helperFile);
                    var lines = content.Split('\n');
                    
                    Debug.Log("\n检查 ParseIndexField1 方法:");
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (lines[i].Contains("ParseIndexField1"))
                        {
                            for (int j = i; j < Math.Min(i + 10, lines.Length); j++)
                            {
                                Debug.Log($"  [{j}] {lines[j].TrimEnd()}");
                            }
                            break;
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
        
        [MenuItem("XMFrame/ConfigNew/测试枚举类型的全局限定名")]
        public static void TestEnumGlobalName()
        {
            Debug.Log("========== 测试枚举类型全局限定名 ==========\n");
            
            var enumType = typeof(EItemType);
            Debug.Log($"枚举类型: {enumType.FullName}");
            Debug.Log($"命名空间: {enumType.Namespace}");
            
            var globalName = TypeHelper.GetGlobalQualifiedTypeName(enumType);
            Debug.Log($"全局限定名: {globalName}");
            
            // 测试可空枚举
            var nullableEnumType = typeof(EItemType?);
            var nullableGlobalName = TypeHelper.GetGlobalQualifiedTypeName(nullableEnumType);
            Debug.Log($"可空枚举全局限定名: {nullableGlobalName}");
        }
        
        [MenuItem("XMFrame/ConfigNew/清理所有生成文件并重新生成")]
        public static void CleanAllAndRegenerate()
        {
            if (!EditorUtility.DisplayDialog(
                "确认清理",
                "这将删除 Generated 目录下的所有 .Gen.cs 文件并重新生成。\n确定继续吗？",
                "确定",
                "取消"))
            {
                return;
            }
            
            var outputDir = Path.Combine(Application.dataPath, "XMFrame", "ConfigNew", "Generated");
            
            // 1. 删除所有生成文件
            var genFiles = Directory.GetFiles(outputDir, "*.Gen.cs")
                .Concat(Directory.GetFiles(outputDir, "*.Gen.cs.meta"))
                .ToList();
            
            Debug.Log($"清理 {genFiles.Count} 个生成文件...");
            foreach (var file in genFiles)
            {
                try
                {
                    File.Delete(file);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"无法删除 {Path.GetFileName(file)}: {ex.Message}");
                }
            }
            
            AssetDatabase.Refresh();
            System.Threading.Thread.Sleep(1000);
            
            // 2. 重新生成所有配置
            RegenerateAll.RegenerateAllTestConfigs();
        }
    }
}
