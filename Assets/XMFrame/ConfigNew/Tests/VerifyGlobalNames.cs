using NUnit.Framework;
using System;
using System.Linq;
using UnityEngine;
using XM.ConfigNew.CodeGen;
using XM.ConfigNew.Tests.Data;
using XM.ConfigNew.Tools;

namespace XM.ConfigNew.Tests
{
    /// <summary>
    /// 验证全局限定名是否正确生成
    /// </summary>
    [TestFixture]
    public class VerifyGlobalNames
    {
        [Test]
        public void Verify_EnumField_HasGlobalQualifiedName()
        {
            // Arrange
            var metadata = TypeAnalyzer.AnalyzeConfigType(typeof(ComplexItemConfig));
            
            // Act
            var indexField1 = metadata.Fields?.FirstOrDefault(f => f.FieldName == "IndexField1");
            
            // Assert
            Assert.IsNotNull(indexField1, "应该找到 IndexField1 字段");
            Assert.IsNotNull(indexField1.ManagedFieldTypeName, "ManagedFieldTypeName 不应为null");
            
            Debug.Log($"IndexField1.ManagedFieldTypeName = {indexField1.ManagedFieldTypeName}");
            Debug.Log($"IndexField1.TypeInfo.IsEnum = {indexField1.TypeInfo?.IsEnum}");
            Debug.Log($"IndexField1.TypeInfo.SingleValueType = {indexField1.TypeInfo?.SingleValueType?.FullName}");
            
            // 枚举类型应该有 global:: 前缀或者是简单名称
            Assert.IsTrue(
                indexField1.ManagedFieldTypeName.Contains("global::") || 
                indexField1.ManagedFieldTypeName == "EItemType",
                $"枚举类型应该使用全局限定名: {indexField1.ManagedFieldTypeName}");
        }
        
        [Test]
        public void Verify_AllFields_HaveCorrectManagedFieldTypeName()
        {
            // Arrange
            var metadata = TypeAnalyzer.AnalyzeConfigType(typeof(ComplexItemConfig));
            
            // Assert
            Assert.IsNotNull(metadata.Fields, "字段列表不应为null");
            
            Debug.Log($"\n所有字段的 ManagedFieldTypeName:");
            foreach (var field in metadata.Fields)
            {
                Debug.Log($"  {field.FieldName}: {field.ManagedFieldTypeName}");
                Assert.IsNotNull(field.ManagedFieldTypeName, $"字段 {field.FieldName} 的 ManagedFieldTypeName 不应为null");
            }
        }
        
        [Test]
        public void Verify_TypeHelper_EnumQualifiedName()
        {
            // Act
            var globalName = TypeHelper.GetGlobalQualifiedTypeName(typeof(EItemType));
            
            // Assert
            Debug.Log($"EItemType 的全局限定名: {globalName}");
            Assert.IsTrue(globalName.Contains("global::") || globalName == "EItemType",
                $"应该是全局限定名: {globalName}");
        }
        
        [Test]
        public void Generate_ComplexItemConfig_And_CheckCode()
        {
            // Arrange
            var metadata = TypeAnalyzer.AnalyzeConfigType(typeof(ComplexItemConfig));
            var generator = new XmlHelperGenerator(metadata);
            
            // Act
            var code = generator.Generate();
            
            // Assert
            Debug.Log("\n========== 生成的代码检查 ==========");
            
            // 查找 ParseIndexField1 方法定义
            var lines = code.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("ParseIndexField1") && lines[i].Contains("(XmlElement"))
                {
                    Debug.Log($"\nParseIndexField1 方法签名 (行 {i}):");
                    for (int j = Math.Max(0, i - 1); j < Math.Min(i + 5, lines.Length); j++)
                    {
                        Debug.Log($"  {lines[j].TrimEnd()}");
                    }
                    
                    // 验证返回类型
                    var methodLine = lines[i].Trim();
                    Assert.IsTrue(methodLine.Contains("global::") || methodLine.Contains("EItemType"),
                        $"ParseIndexField1 的返回类型应该使用全局限定名: {methodLine}");
                    break;
                }
            }
        }
    }
}
