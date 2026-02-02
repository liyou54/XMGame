// using NUnit.Framework;
// using System.Text;
// using XM.Contracts.Config;
//
// namespace XM.Editor.ConfigEditor.Tests
// {
//     /// <summary>
//     /// ToString 方法生成测试
//     /// </summary>
//     [TestFixture]
//     [Category("CodeGen")]
//     public class ToStringGenerationTest
//     {
//         [Test]
//         [Description("测试基本字段的 ToString 输出")]
//         public void TestBasicFieldToString()
//         {
//             // Arrange
//             var config = new TestConfigUnManaged
//             {
//                 TestInt = 42
//             };
//
//             // Act
//             var result = config.ToString();
//
//             // Assert
//             Assert.That(result, Does.Contain("TestConfigUnManaged {"));
//             Assert.That(result, Does.Contain("TestInt=42"));
//         }
//
//         [Test]
//         [Description("测试 CfgI 字段的 ToString 输出")]
//         public void TestCfgIFieldToString()
//         {
//             // Arrange
//             var cfgI = new CfgI<TestConfigUnManaged>(1, new ModI(1), new TblI<TestConfigUnManaged>(1));
//             var config = new TestConfigUnManaged
//             {
//                 Id = cfgI
//             };
//
//             // Act
//             var result = config.ToString();
//
//             // Assert
//             Assert.That(result, Does.Contain("Id=CfgI(1)"));
//         }
//
//         [Test]
//         [Description("测试 XBlobPtr 字段的 ToString 输出")]
//         public void TestXBlobPtrFieldToString()
//         {
//             // Arrange
//             var cfgI = new CfgI<TestConfigUnManaged>(5, new ModI(1), new TblI<TestConfigUnManaged>(1));
//             var config = new TestConfigUnManaged
//             {
//                 Foreign = cfgI,
//                 Foreign_Ref = new global::XBlobPtr<TestConfigUnManaged>(100)
//             };
//
//             // Act
//             var result = config.ToString();
//
//             // Assert
//             Assert.That(result, Does.Contain("Foreign=CfgI(5)"));
//             Assert.That(result, Does.Contain("Foreign_Ref=Ptr->Foreign") | Does.Contain("Foreign_Ref=Ptr->" + config.Foreign));
//         }
//
//         [Test]
//         [Description("验证 ToString 方法已生成")]
//         public void TestToStringMethodExists()
//         {
//             // Arrange
//             var config = new TestConfigUnManaged();
//
//             // Act & Assert
//             Assert.DoesNotThrow(() => config.ToString());
//         }
//
//         [Test]
//         [Description("验证嵌套配置的 ToString")]
//         public void TestNestedConfigToString()
//         {
//             // Arrange
//             var nested = new NestedConfigUnManaged
//             {
//                 RequiredId = 123,
//                 Test = 456
//             };
//
//             // Act
//             var result = nested.ToString();
//
//             // Assert
//             Assert.That(result, Does.Contain("NestedConfigUnManaged {"));
//             Assert.That(result, Does.Contain("RequiredId=123"));
//             Assert.That(result, Does.Contain("Test=456"));
//         }
//     }
// }
