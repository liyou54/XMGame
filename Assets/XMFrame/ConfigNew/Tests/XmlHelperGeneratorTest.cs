using NUnit.Framework;
using System;
using System.IO;
using System.Linq;
using XM.ConfigNew.CodeGen;
using XM.ConfigNew.Tests.Data;
using XM.ConfigNew.Tools;

namespace XM.ConfigNew.Tests
{
    /// <summary>
    /// XmlHelper 生成器测试
    /// 验证生成器能够生成基本的 ClassHelper 框架
    /// </summary>
    [TestFixture]
    public class XmlHelperGeneratorTest
    {
        [Test]
        public void Test_GenerateSimpleHelper_Structure()
        {
            // Arrange
            var configType = typeof(BaseItemConfig);
            var metadata = TypeAnalyzer.AnalyzeConfigType(configType);
            var generator = new XmlHelperGenerator(metadata);
            
            // Act
            var code = generator.Generate();
            
            // Assert
            Assert.IsNotNull(code, "生成的代码不应为null");
            Assert.IsNotEmpty(code, "生成的代码不应为空");
            
            // 验证基本结构
            Assert.IsTrue(code.Contains("using System"), "应包含 using System");
            Assert.IsTrue(code.Contains("using XM.Contracts.Config"), "应包含 using XM.Contracts.Config");
            Assert.IsTrue(code.Contains($"class {metadata.HelperTypeName}"), "应包含类声明");
            Assert.IsTrue(code.Contains("ConfigClassHelper<"), "应继承 ConfigClassHelper");
            
            // 验证静态字段
            Assert.IsTrue(code.Contains("public static TblS TblS"), "应包含 TblS 静态字段");
            
            // 验证方法
            Assert.IsTrue(code.Contains("GetTblS()"), "应包含 GetTblS 方法");
            Assert.IsTrue(code.Contains("ParseAndFillFromXml"), "应包含 ParseAndFillFromXml 方法");
            Assert.IsTrue(code.Contains("AllocContainerWithFillImpl"), "应包含 AllocContainerWithFillImpl 方法");
            
            // 验证 Region
            Assert.IsTrue(code.Contains("#region 字段解析方法"), "应包含字段解析方法 Region");
            
            // 输出到控制台查看
            Console.WriteLine("========== 生成的代码 ==========");
            Console.WriteLine(code);
        }
        
        [Test]
        public void Test_GenerateHelper_WithFields()
        {
            // Arrange
            var configType = typeof(BaseItemConfig);
            var metadata = TypeAnalyzer.AnalyzeConfigType(configType);
            var generator = new XmlHelperGenerator(metadata);
            
            // Act
            var code = generator.Generate();
            
            // Assert - 验证为每个字段生成了 Parse 方法
            foreach (var field in metadata.Fields)
            {
                Assert.IsTrue(code.Contains(field.ParseMethodName), 
                    $"应包含 {field.FieldName} 的解析方法: {field.ParseMethodName}");
            }
        }
        
        [Test]
        public void Test_GenerateHelper_WithContainers()
        {
            // Arrange
            var configType = typeof(ComplexItemConfig);
            var metadata = TypeAnalyzer.AnalyzeConfigType(configType);
            var generator = new XmlHelperGenerator(metadata);
            
            // Act
            var code = generator.Generate();
            
            // Assert - 验证为容器字段生成了 Alloc 方法
            var containerFields = metadata.Fields.Where(f => f.IsContainer).ToList();
            
            foreach (var field in containerFields)
            {
                Assert.IsTrue(code.Contains(field.AllocMethodName), 
                    $"应包含 {field.FieldName} 的分配方法: {field.AllocMethodName}");
            }
        }
        
        [Test]
        public void Test_GenerateHelper_ToFile()
        {
            // Arrange
            var configType = typeof(BaseItemConfig);
            var metadata = TypeAnalyzer.AnalyzeConfigType(configType);
            var outputPath = Path.Combine(Path.GetTempPath(), "ConfigNewTest");
            Directory.CreateDirectory(outputPath);
            
            var manager = new CodeGenerationManager();
            
            try
            {
                // Act
                var files = manager.GenerateForType(configType, outputPath);
                
                // Assert
                Assert.IsNotNull(files, "生成的文件列表不应为null");
                Assert.Greater(files.Count, 0, "应该生成至少一个文件");
                
                // 查找 Helper 文件
                var helperFile = files.FirstOrDefault(f => f.Contains("ClassHelper"));
                Assert.IsNotNull(helperFile, "应该生成 ClassHelper 文件");
                Assert.IsTrue(File.Exists(helperFile), $"Helper 文件应该存在: {helperFile}");
                
                // 读取并验证内容
                var content = File.ReadAllText(helperFile);
                Assert.IsNotEmpty(content, "Helper 文件内容不应为空");
                
                Console.WriteLine($"生成的 Helper 文件: {helperFile}");
                Console.WriteLine($"文件大小: {new FileInfo(helperFile).Length} 字节");
            }
            finally
            {
                // 清理
                if (Directory.Exists(outputPath))
                {
                    try
                    {
                        Directory.Delete(outputPath, true);
                    }
                    catch
                    {
                        // 忽略清理错误
                    }
                }
            }
        }
        
        [Test]
        public void Test_GeneratedCode_HasTODOMarkers()
        {
            // Arrange
            var configType = typeof(BaseItemConfig);
            var metadata = TypeAnalyzer.AnalyzeConfigType(configType);
            var generator = new XmlHelperGenerator(metadata);
            
            // Act
            var code = generator.Generate();
            
            // Assert - 验证 TODO 标记存在（因为还没实现细节）
            Assert.IsTrue(code.Contains("TODO"), "应包含 TODO 标记");
        }
    }
}
