using NUnit.Framework;
using System;
using XM.ConfigNew.CodeGen;
using XM.ConfigNew.Tests.Data;
using XM.ConfigNew.Tools;

namespace XM.ConfigNew.Tests
{
    /// <summary>
    /// 全局限定名测试 - 验证生成的代码使用 global:: 前缀避免命名冲突
    /// </summary>
    [TestFixture]
    public class GlobalQualifiedNameTest
    {
        [Test]
        public void Test_TypeHelper_GetGlobalQualifiedTypeName()
        {
            // 基本类型
            Assert.AreEqual("int", TypeHelper.GetGlobalQualifiedTypeName(typeof(int)));
            Assert.AreEqual("string", TypeHelper.GetGlobalQualifiedTypeName(typeof(string)));
            Assert.AreEqual("float", TypeHelper.GetGlobalQualifiedTypeName(typeof(float)));
            
            // 可空类型
            Assert.AreEqual("int?", TypeHelper.GetGlobalQualifiedTypeName(typeof(int?)));
            Assert.AreEqual("float?", TypeHelper.GetGlobalQualifiedTypeName(typeof(float?)));
            
            // 带命名空间的类型
            var qualifiedName = TypeHelper.GetGlobalQualifiedTypeName(typeof(AttributeConfig));
            Assert.IsTrue(qualifiedName.StartsWith("global::"), $"应该以 global:: 开头: {qualifiedName}");
            Assert.IsTrue(qualifiedName.Contains("AttributeConfig"), $"应该包含类型名: {qualifiedName}");
            
            // 泛型类型
            var listType = typeof(System.Collections.Generic.List<int>);
            var qualifiedListName = TypeHelper.GetGlobalQualifiedTypeName(listType);
            Assert.IsTrue(qualifiedListName.StartsWith("global::"), $"泛型类型应该以 global:: 开头: {qualifiedListName}");
            Assert.IsTrue(qualifiedListName.Contains("List"), $"应该包含 List: {qualifiedListName}");
        }
        
        [Test]
        public void Test_GeneratedHelper_UsesGlobalQualifiedNames()
        {
            // Arrange
            var metadata = TypeAnalyzer.AnalyzeConfigType(typeof(AttributeConfig));
            var generator = new XmlHelperGenerator(metadata);
            
            // Act
            var code = generator.Generate();
            
            // Assert
            Assert.IsTrue(code.Contains("global::"), "生成的代码应该包含 global:: 前缀");
            
            // 验证关键位置使用了全局限定名
            Assert.IsTrue(code.Contains("ConfigClassHelper<global::"), 
                "类继承应该使用全局限定名");
            
            Console.WriteLine("========== 生成的代码片段 ==========");
            var lines = code.Split('\n');
            for (int i = 0; i < Math.Min(50, lines.Length); i++)
            {
                Console.WriteLine(lines[i]);
            }
        }
        
        [Test]
        public void Test_GeneratedUnmanaged_UsesGlobalQualifiedNames()
        {
            // Arrange
            var metadata = TypeAnalyzer.AnalyzeConfigType(typeof(ComplexItemConfig));
            var generator = new UnmanagedGenerator(metadata);
            
            // Act
            var code = generator.Generate();
            
            // Assert
            Assert.IsTrue(code.Contains("global::"), "Unmanaged 结构体应该包含 global:: 前缀");
            
            Console.WriteLine("========== Unmanaged 代码片段 ==========");
            var lines = code.Split('\n');
            for (int i = 0; i < Math.Min(30, lines.Length); i++)
            {
                Console.WriteLine(lines[i]);
            }
        }
        
        [Test]
        public void Test_ManagedFieldTypeName_UsesGlobalQualifiedName()
        {
            // Arrange
            var metadata = TypeAnalyzer.AnalyzeConfigType(typeof(ComplexItemConfig));
            
            // Act & Assert
            Assert.IsNotNull(metadata.Fields, "字段列表不应为null");
            
            foreach (var field in metadata.Fields)
            {
                Assert.IsNotNull(field.ManagedFieldTypeName, $"字段 {field.FieldName} 的 ManagedFieldTypeName 不应为null");
                
                // 如果是非基本类型，应该包含 global:: 或者是基本类型关键字
                if (field.TypeInfo?.ManagedFieldType != null && 
                    !IsPrimitiveKeyword(field.ManagedFieldTypeName))
                {
                    // 复杂类型应该使用全局限定名
                    if (field.TypeInfo.IsContainer || field.IsNestedConfig)
                    {
                        var hasGlobal = field.ManagedFieldTypeName.IndexOf("global::", StringComparison.Ordinal) >= 0;
                        Assert.IsTrue(hasGlobal || IsPrimitiveKeyword(field.ManagedFieldTypeName),
                            $"字段 {field.FieldName} 的类型应使用全局限定名: {field.ManagedFieldTypeName}");
                    }
                }
            }
        }
        
        private bool IsPrimitiveKeyword(string typeName)
        {
            var primitives = new[] { "int", "float", "double", "bool", "string", "long", "short", "byte", "decimal", "object", "void" };
            var baseTypeName = typeName.Split('<')[0].Trim();
            return System.Array.Exists(primitives, p => p == baseTypeName);
        }
    }
}
