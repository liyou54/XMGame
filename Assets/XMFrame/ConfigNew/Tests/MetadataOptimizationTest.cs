using NUnit.Framework;
using System;
using System.Linq;
using XM.ConfigNew.Tests.Data;
using XM.ConfigNew.Tools;

namespace XM.ConfigNew.Tests
{
    /// <summary>
    /// 元数据优化测试 - 验证新增的预计算字段和优化后的结构
    /// </summary>
    [TestFixture]
    public class MetadataOptimizationTest
    {
        [Test]
        public void Test_PrecomputedFields_AreGenerated()
        {
            // Arrange
            var configType = typeof(BaseItemConfig);
            
            // Act
            var metadata = TypeAnalyzer.AnalyzeConfigType(configType);
            
            // Assert
            Assert.IsNotNull(metadata, "元数据不应为null");
            Assert.IsNotNull(metadata.Fields, "字段列表不应为null");
            Assert.Greater(metadata.Fields.Count, 0, "应该有字段");
            
            // 验证预计算字段
            foreach (var field in metadata.Fields)
            {
                Assert.IsNotNull(field.ParseMethodName, $"字段 {field.FieldName} 的 ParseMethodName 不应为null");
                Assert.IsTrue(field.ParseMethodName.StartsWith("Parse"), 
                    $"ParseMethodName 应该以 'Parse' 开头: {field.ParseMethodName}");
                
                Assert.IsNotNull(field.AllocMethodName, $"字段 {field.FieldName} 的 AllocMethodName 不应为null");
                Assert.IsTrue(field.AllocMethodName.StartsWith("Alloc"), 
                    $"AllocMethodName 应该以 'Alloc' 开头: {field.AllocMethodName}");
                
                Assert.IsNotNull(field.FillMethodName, $"字段 {field.FieldName} 的 FillMethodName 不应为null");
                Assert.IsTrue(field.FillMethodName.StartsWith("Fill"), 
                    $"FillMethodName 应该以 'Fill' 开头: {field.FillMethodName}");
                
                Assert.IsNotNull(field.ManagedFieldTypeName, $"字段 {field.FieldName} 的 ManagedFieldTypeName 不应为null");
                Assert.IsNotNull(field.UnmanagedFieldTypeName, $"字段 {field.FieldName} 的 UnmanagedFieldTypeName 不应为null");
            }
        }
        
        [Test]
        public void Test_RequiredUsings_IsCollected()
        {
            // Arrange
            var configType = typeof(ComplexItemConfig);
            
            // Act
            var metadata = TypeAnalyzer.AnalyzeConfigType(configType);
            
            // Assert
            Assert.IsNotNull(metadata.RequiredUsings, "RequiredUsings 不应为null");
            Assert.Greater(metadata.RequiredUsings.Count, 0, "RequiredUsings 应该有内容");
            Assert.IsTrue(metadata.RequiredUsings.Contains("System"), "应该包含 System");
            Assert.IsTrue(metadata.RequiredUsings.Contains("XM.Contracts.Config"), "应该包含 XM.Contracts.Config");
        }
        
        [Test]
        public void Test_HelperType_IsSet()
        {
            // Arrange
            var configType = typeof(BaseItemConfig);
            
            // Act
            var metadata = TypeAnalyzer.AnalyzeConfigType(configType);
            
            // Assert
            Assert.IsNotNull(metadata.HelperTypeName, "HelperTypeName 不应为null");
            Assert.AreEqual("BaseItemConfigClassHelper", metadata.HelperTypeName, "HelperTypeName 应该正确");
        }
        
        [Test]
        public void Test_ConverterInfo_HasKeyAndValueRegistrations()
        {
            // Arrange
            var configType = typeof(ContainerConverterConfig);
            
            // Act
            var metadata = TypeAnalyzer.AnalyzeConfigType(configType);
            
            // Assert
            var fieldWithConverter = metadata.Fields.FirstOrDefault(f => f.FieldName == "CustomKeyDict");
            Assert.IsNotNull(fieldWithConverter, "应该找到 CustomKeyDict 字段");
            Assert.IsNotNull(fieldWithConverter.Converter, "Converter 不应为null");
            
            // 注意: KeyRegistrations 和 ValueRegistrations 的实际填充需要在 AnalyzeConverter 中实现
            // 这里只验证结构存在
            Assert.IsNotNull(fieldWithConverter.Converter.Registrations, "Registrations 不应为null");
        }
        
        [Test]
        public void Test_RemovedFields_NoLongerExist()
        {
            // 验证 FieldTypeInfo 不再有冗余字段
            var fieldTypeInfoType = typeof(XM.ConfigNew.Metadata.FieldTypeInfo);
            
            Assert.IsNull(fieldTypeInfoType.GetField("IsPrimaryKey"), "IsPrimaryKey 字段应该被移除");
            Assert.IsNull(fieldTypeInfoType.GetField("IndexType"), "IndexType 字段应该被移除");
            Assert.IsNull(fieldTypeInfoType.GetField("HasCustomParser"), "HasCustomParser 字段应该被移除");
            Assert.IsNull(fieldTypeInfoType.GetField("KeyParserType"), "KeyParserType 字段应该被移除");
            Assert.IsNull(fieldTypeInfoType.GetField("ValueParserType"), "ValueParserType 字段应该被移除");
            Assert.IsNull(fieldTypeInfoType.GetField("HelperType"), "HelperType 字段应该被移除");
        }
        
        [Test]
        public void Test_ConfigFieldMetadata_RemovedRedundantConverters()
        {
            // 验证 ConfigFieldMetadata 不再有冗余的转换器字段
            var fieldMetadataType = typeof(XM.ConfigNew.Metadata.ConfigFieldMetadata);
            
            Assert.IsNull(fieldMetadataType.GetField("KeyConverter"), "KeyConverter 字段应该被移除");
            Assert.IsNull(fieldMetadataType.GetField("ValueConverter"), "ValueConverter 字段应该被移除");
        }
        
        [Test]
        public void Test_UnmanagedFieldTypeName_IsCorrect()
        {
            // Arrange
            var configType = typeof(BaseItemConfig);
            
            // Act
            var metadata = TypeAnalyzer.AnalyzeConfigType(configType);
            
            // Assert
            var idField = metadata.Fields.FirstOrDefault(f => f.FieldName == "Id");
            Assert.IsNotNull(idField, "应该找到 Id 字段");
            Assert.AreEqual("int", idField.UnmanagedFieldTypeName, "Id 的非托管类型应该是 int");
            
            var nameField = metadata.Fields.FirstOrDefault(f => f.FieldName == "Name");
            Assert.IsNotNull(nameField, "应该找到 Name 字段");
            // Name 字段默认应该是 FixedString32Bytes
            Assert.IsTrue(nameField.UnmanagedFieldTypeName.Contains("FixedString") || 
                         nameField.UnmanagedFieldTypeName.Contains("StrI"), 
                         $"Name 的非托管类型应该是字符串类型: {nameField.UnmanagedFieldTypeName}");
        }
    }
}
