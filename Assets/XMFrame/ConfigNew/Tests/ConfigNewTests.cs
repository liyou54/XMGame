using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using XM.ConfigNew.CodeGen;
using XM.ConfigNew.Metadata;
using XM.ConfigNew.Tests.Data;
using XM.ConfigNew.Tools;

namespace XM.ConfigNew.Tests
{
    /// <summary>
    /// ConfigNew 核心功能测试
    /// </summary>
    [TestFixture]
    public class ConfigNewTests
    {
        private string _testOutputDir;
        
        [SetUp]
        public void Setup()
        {
            _testOutputDir = Path.Combine(Path.GetTempPath(), "ConfigNewTests", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testOutputDir);
        }
        
        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_testOutputDir))
            {
                try { Directory.Delete(_testOutputDir, true); }
                catch { }
            }
        }
        
        #region TypeAnalyzer 基础测试
        
        [Test]
        public void Test_Analyze_BasicMetadata()
        {
            var metadata = TypeAnalyzer.AnalyzeConfigType(typeof(AttributeConfig));
            
            Assert.IsNotNull(metadata);
            Assert.AreEqual("AttributeConfig", metadata.ManagedTypeName);
            Assert.AreEqual("AttributeConfigUnmanaged", metadata.UnmanagedTypeName);
            Assert.AreEqual(4, metadata.Fields.Count);
        }
        
        [Test]
        public void Test_Analyze_EnumAndNullable()
        {
            var metadata = TypeAnalyzer.AnalyzeConfigType(typeof(AttributeConfig));
            var typeField = metadata.Fields.First(f => f.FieldName == "Type");
            var bonusField = metadata.Fields.First(f => f.FieldName == "BonusValue");
            
            Assert.IsTrue(typeField.TypeInfo.IsEnum);
            Assert.IsTrue(bonusField.TypeInfo.IsNullable);
        }
        
        [Test]
        public void Test_Analyze_LinkAndIndex()
        {
            var metadata = TypeAnalyzer.AnalyzeConfigType(typeof(SingleLinkChildConfig));
            var parentField = metadata.Fields.First(f => f.FieldName == "Parent");
            
            Assert.IsTrue(parentField.IsXmlLink);
            Assert.AreEqual(1, metadata.Indexes.Count);
        }
        
        #endregion
        
        #region 代码生成测试
        
        [Test]
        public void Test_Generate_BasicStruct()
        {
            var metadata = TypeAnalyzer.AnalyzeConfigType(typeof(AttributeConfig));
            var generator = new UnmanagedGenerator(metadata);
            var code = generator.Generate();
            
            StringAssert.Contains("public partial struct AttributeConfigUnmanaged", code);
            StringAssert.Contains("IConfigUnManaged<AttributeConfigUnmanaged>", code);
            StringAssert.Contains("string ToString(object dataContainer)", code);
        }
        
        [Test]
        public void Test_Generate_LinkFields()
        {
            var metadata = TypeAnalyzer.AnalyzeConfigType(typeof(SingleLinkChildConfig));
            var generator = new UnmanagedGenerator(metadata);
            var code = generator.Generate();
            
            StringAssert.Contains("public CfgI<LinkParentConfigUnmanaged> Parent;", code);
            StringAssert.Contains("public XBlobPtr<LinkParentConfigUnmanaged> Parent_ParentPtr;", code);
            StringAssert.Contains("public CfgI<LinkParentConfigUnmanaged> Parent_ParentIndex;", code);
        }
        
        [Test]
        public void Test_Generate_IndexStruct()
        {
            var metadata = TypeAnalyzer.AnalyzeConfigType(typeof(BaseItemConfig));
            var idIndex = metadata.Indexes.First(i => i.IndexName == "IdIndex");
            var generator = new IndexStructGenerator(metadata, idIndex);
            var code = generator.Generate();
            
            StringAssert.Contains("public struct IdIndexIndex", code);
            StringAssert.Contains("IEquatable<IdIndexIndex>", code);
            StringAssert.Contains("public IdIndexIndex(int id)", code);
            StringAssert.Contains("public static class", code);
        }
        
        #endregion
        
        #region 类型映射测试
        
        [Test]
        public void Test_TypeMapping_String_To_StrI()
        {
            var metadata = TypeAnalyzer.AnalyzeConfigType(typeof(BaseItemConfig));
            var generator = new UnmanagedGenerator(metadata);
            var code = generator.Generate();
            
            StringAssert.Contains("public StrI Name;", code);
        }
        
        [Test]
        public void Test_TypeMapping_StringMode()
        {
            var metadata = TypeAnalyzer.AnalyzeConfigType(typeof(ComplexItemConfig));
            var generator = new UnmanagedGenerator(metadata);
            var code = generator.Generate();
            
            StringAssert.Contains("public FixedString32Bytes ShortName;", code);
            StringAssert.Contains("public StrI LocalizedName;", code);
            StringAssert.Contains("public LabelI LabelName;", code);
        }
        
        [Test]
        public void Test_TypeMapping_Containers()
        {
            var metadata = TypeAnalyzer.AnalyzeConfigType(typeof(ComplexItemConfig));
            var generator = new UnmanagedGenerator(metadata);
            var code = generator.Generate();
            
            StringAssert.Contains("public XBlobArray<StrI> Tags;", code);
            StringAssert.Contains("public XBlobArray<XBlobArray<int>> Matrix;", code);
            StringAssert.Contains("public XBlobMap<StrI, int> StringIntMap;", code);
        }
        
        [Test]
        public void Test_TypeMapping_NestedConfig()
        {
            var metadata = TypeAnalyzer.AnalyzeConfigType(typeof(ComplexItemConfig));
            var generator = new UnmanagedGenerator(metadata);
            var code = generator.Generate();
            
            StringAssert.Contains("public AttributeConfigUnmanaged Price;", code);
            StringAssert.Contains("public XBlobArray<AttributeConfigUnmanaged> Attributes;", code);
        }
        
        #endregion
        
        #region 完整生成流程测试
        
        [Test]
        public void Test_Manager_GenerateAll()
        {
            var manager = new CodeGenerationManager();
            var files = manager.GenerateForType(typeof(BaseItemConfig), _testOutputDir);
            
            Assert.AreEqual(3, files.Count); // 1个Unmanaged + 2个索引
            Assert.IsTrue(files.All(f => File.Exists(f)));
        }
        
        #endregion
    }
}
