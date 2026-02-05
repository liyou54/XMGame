using NUnit.Framework;
using System;
using System.IO;
using UnityEngine;
using XM.ConfigNew.CodeGen;
using XM.ConfigNew.Tools;
using XM.ConfigNew.Tests.Data;

namespace XM.ConfigNew.Tests
{
    /// <summary>
    /// 测试 XML 解析功能
    /// </summary>
    [TestFixture]
    public class TestXmlParsing
    {
        [Test]
        public void Test_GenerateParseMethod_BasicTypes()
        {
            // Arrange
            var metadata = TypeAnalyzer.AnalyzeConfigType(typeof(BaseItemConfig));
            var generator = new XmlHelperGenerator(metadata);
            
            // Act
            var code = generator.Generate();
            
            // Assert
            Assert.IsTrue(code.Contains("ParseId"), "应生成 ParseId 方法");
            Assert.IsTrue(code.Contains("ParseName"), "应生成 ParseName 方法");
            Assert.IsTrue(code.Contains("ConfigParseHelper.TryParseInt"), "应使用 TryParseInt");
            Assert.IsTrue(code.Contains("GetXmlFieldValue"), "应使用 GetXmlFieldValue");
            
            Debug.Log("========== 生成的 Parse 方法示例 ==========");
            var lines = code.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("private static") && lines[i].Contains("ParseId"))
                {
                    for (int j = i; j < Math.Min(i + 15, lines.Length); j++)
                    {
                        Debug.Log(lines[j]);
                    }
                    break;
                }
            }
        }
        
        [Test]
        public void Test_GenerateParseMethod_WithEnum()
        {
            // Arrange
            var metadata = TypeAnalyzer.AnalyzeConfigType(typeof(ComplexItemConfig));
            var generator = new XmlHelperGenerator(metadata);
            
            // Act
            var code = generator.Generate();
            
            // Assert
            Assert.IsTrue(code.Contains("ParseItemType"), "应生成枚举解析方法");
            Assert.IsTrue(code.Contains("Enum.TryParse"), "应使用 Enum.TryParse");
        }
        
        [Test]
        public void Test_GenerateParseMethod_WithContainer()
        {
            // Arrange
            var metadata = TypeAnalyzer.AnalyzeConfigType(typeof(ComplexItemConfig));
            var generator = new XmlHelperGenerator(metadata);
            
            // Act
            var code = generator.Generate();
            
            // Assert
            Assert.IsTrue(code.Contains("SelectNodes"), "容器应使用 SelectNodes");
            Assert.IsTrue(code.Contains("new List<"), "应创建 List 实例");
        }
        
        [Test]
        public void Test_FullGeneration_NoErrors()
        {
            // Arrange
            var types = new[]
            {
                typeof(BaseItemConfig),
                typeof(ComplexItemConfig),
                typeof(BaseSkillConfig)
            };
            
            // Act & Assert
            foreach (var type in types)
            {
                try
                {
                    var metadata = TypeAnalyzer.AnalyzeConfigType(type);
                    var generator = new XmlHelperGenerator(metadata);
                    var code = generator.Generate();
                    
                    Assert.IsNotNull(code);
                    Assert.IsNotEmpty(code);
                    Assert.IsFalse(code.Contains("TODO"), $"{type.Name} 不应包含 TODO 标记");
                    
                    Debug.Log($"✅ {type.Name} 生成成功，代码长度: {code.Length}");
                }
                catch (Exception ex)
                {
                    Assert.Fail($"❌ {type.Name} 生成失败: {ex.Message}");
                }
            }
        }
    }
}
