using System;
using System.Xml;
using NUnit.Framework;
using XM.Contracts.Config;
using XM.Editor.Tests;

namespace XM.Editor.Tests.Unit
{
    /// <summary>
    /// ConfigParseHelper 配置解析辅助类测试
    /// 目标：98%+ 分支覆盖率
    /// 测试所有TryParse方法和XML处理方法
    /// </summary>
    [TestFixture]
    [Category(TestCategories.Pure)]
    public class ConfigParseHelperTests : PureFunctionTestBase
    {
        #region TryParseInt 测试
        
        [Test]
        public void TryParseInt_ValidString_ReturnsTrue()
        {
            Assert.IsTrue(ConfigParseHelper.TryParseInt("123", "field", out var value));
            Assert.AreEqual(123, value);
        }
        
        [Test]
        public void TryParseInt_NegativeNumber_ReturnsTrue()
        {
            Assert.IsTrue(ConfigParseHelper.TryParseInt("-456", "field", out var value));
            Assert.AreEqual(-456, value);
        }
        
        [Test]
        public void TryParseInt_NullOrEmpty_ReturnsFalse()
        {
            Assert.IsFalse(ConfigParseHelper.TryParseInt(null, "field", out var value1));
            Assert.IsFalse(ConfigParseHelper.TryParseInt("", "field", out var value2));
            Assert.AreEqual(default(int), value1);
            Assert.AreEqual(default(int), value2);
        }
        
        [Test]
        public void TryParseInt_InvalidFormat_ReturnsFalse()
        {
            Assert.IsFalse(ConfigParseHelper.TryParseInt("abc", "field", out var value));
            Assert.AreEqual(default(int), value);
        }
        
        #endregion
        
        #region TryParseLong 测试
        
        [Test]
        public void TryParseLong_ValidString_ReturnsTrue()
        {
            Assert.IsTrue(ConfigParseHelper.TryParseLong("9223372036854775807", "field", out var value));
            Assert.AreEqual(9223372036854775807L, value);
        }
        
        [Test]
        public void TryParseLong_NullOrEmpty_ReturnsFalse()
        {
            Assert.IsFalse(ConfigParseHelper.TryParseLong(null, "field", out var value));
            Assert.AreEqual(default(long), value);
        }
        
        #endregion
        
        #region TryParseShort 测试
        
        [Test]
        public void TryParseShort_ValidString_ReturnsTrue()
        {
            Assert.IsTrue(ConfigParseHelper.TryParseShort("32767", "field", out var value));
            Assert.AreEqual((short)32767, value);
        }
        
        #endregion
        
        #region TryParseByte 测试
        
        [Test]
        public void TryParseByte_ValidString_ReturnsTrue()
        {
            Assert.IsTrue(ConfigParseHelper.TryParseByte("255", "field", out var value));
            Assert.AreEqual((byte)255, value);
        }
        
        #endregion
        
        #region TryParseFloat 测试
        
        [Test]
        public void TryParseFloat_ValidString_ReturnsTrue()
        {
            Assert.IsTrue(ConfigParseHelper.TryParseFloat("3.14", "field", out var value));
            Assert.AreEqual(3.14f, value, 0.0001f);
        }
        
        #endregion
        
        #region TryParseDouble 测试
        
        [Test]
        public void TryParseDouble_ValidString_ReturnsTrue()
        {
            Assert.IsTrue(ConfigParseHelper.TryParseDouble("3.141592653589793", "field", out var value));
            Assert.AreEqual(3.141592653589793, value, 0.0000001);
        }
        
        #endregion
        
        #region TryParseBool 测试
        
        [TestCase("1", true)]
        [TestCase("true", true)]
        [TestCase("TRUE", true)]
        [TestCase("True", true)]
        [TestCase("yes", true)]
        [TestCase("YES", true)]
        [TestCase("0", false)]
        [TestCase("false", false)]
        [TestCase("no", false)]
        [TestCase("anything", false)]
        public void TryParseBool_VariousInputs_ReturnsCorrectValue(string input, bool expected)
        {
            Assert.IsTrue(ConfigParseHelper.TryParseBool(input, "field", out var value));
            Assert.AreEqual(expected, value);
        }
        
        [Test]
        public void TryParseBool_NullOrEmpty_ReturnsFalse()
        {
            Assert.IsFalse(ConfigParseHelper.TryParseBool(null, "field", out var value1));
            Assert.IsFalse(ConfigParseHelper.TryParseBool("", "field", out var value2));
        }
        
        #endregion
        
        #region TryParseDecimal 测试
        
        [Test]
        public void TryParseDecimal_ValidString_ReturnsTrue()
        {
            Assert.IsTrue(ConfigParseHelper.TryParseDecimal("123.456", "field", out var value));
            Assert.AreEqual(123.456m, value);
        }
        
        #endregion
        
        #region TryParseCfgSString 测试
        
        [Test]
        public void TryParseCfgSString_TwoSegments_ParsesCorrectly()
        {
            // Given: "ModName::ConfigName" 格式
            var result = ConfigParseHelper.TryParseCfgSString(
                "TestMod::Config1", 
                "field", 
                out var modName, 
                out var configName);
            
            // Then
            Assert.IsTrue(result);
            Assert.AreEqual("TestMod", modName);
            Assert.AreEqual("Config1", configName);
        }
        
        [Test]
        public void TryParseCfgSString_ThreeSegments_ParsesThirdAsConfigName()
        {
            // Given: "ModName::TableName::ConfigName" 格式
            var result = ConfigParseHelper.TryParseCfgSString(
                "TestMod::TestTable::Config1", 
                "field", 
                out var modName, 
                out var configName);
            
            // Then
            Assert.IsTrue(result);
            Assert.AreEqual("TestMod", modName);
            Assert.AreEqual("Config1", configName); // 第三段作为ConfigName
        }
        
        [Test]
        public void TryParseCfgSString_NullOrEmpty_ReturnsFalse()
        {
            Assert.IsFalse(ConfigParseHelper.TryParseCfgSString(null, "field", out _, out _));
            Assert.IsFalse(ConfigParseHelper.TryParseCfgSString("", "field", out _, out _));
        }
        
        [Test]
        public void TryParseCfgSString_SingleSegment_ReturnsFalse()
        {
            Assert.IsFalse(ConfigParseHelper.TryParseCfgSString("OnlyOnePart", "field", out _, out _));
        }
        
        [Test]
        public void TryParseCfgSString_WithWhitespace_TrimsCorrectly()
        {
            var result = ConfigParseHelper.TryParseCfgSString(
                "  TestMod  ::  Config1  ", 
                "field", 
                out var modName, 
                out var configName);
            
            Assert.IsTrue(result);
            Assert.AreEqual("TestMod", modName);
            Assert.AreEqual("Config1", configName);
        }
        
        #endregion
        
        #region TryParseLabelSString 测试
        
        [Test]
        public void TryParseLabelSString_TwoSegments_ParsesCorrectly()
        {
            var result = ConfigParseHelper.TryParseLabelSString(
                "TestMod::Label1", 
                "field", 
                out var modName, 
                out var labelName);
            
            Assert.IsTrue(result);
            Assert.AreEqual("TestMod", modName);
            Assert.AreEqual("Label1", labelName);
        }
        
        [Test]
        public void TryParseLabelSString_NullOrEmpty_ReturnsFalse()
        {
            Assert.IsFalse(ConfigParseHelper.TryParseLabelSString(null, "field", out _, out _));
            Assert.IsFalse(ConfigParseHelper.TryParseLabelSString("", "field", out _, out _));
        }
        
        [Test]
        public void TryParseLabelSString_SingleSegment_ReturnsFalse()
        {
            Assert.IsFalse(ConfigParseHelper.TryParseLabelSString("OnlyOnePart", "field", out _, out _));
        }
        
        [Test]
        public void TryParseLabelSString_ThreeSegments_ReturnsFalse()
        {
            // LabelS只支持两段，不支持三段
            Assert.IsFalse(ConfigParseHelper.TryParseLabelSString(
                "Mod::Table::Label", 
                "field", 
                out _, 
                out _));
        }
        
        #endregion
        
        #region GetXmlFieldValue 测试
        
        [Test]
        public void GetXmlFieldValue_ChildElementExists_ReturnsInnerText()
        {
            // Arrange
            var doc = new XmlDocument();
            var parent = doc.CreateElement("Parent");
            var child = doc.CreateElement("Field1");
            child.InnerText = "ChildValue";
            parent.AppendChild(child);
            
            // Act
            var result = ConfigParseHelper.GetXmlFieldValue(parent, "Field1");
            
            // Assert
            Assert.AreEqual("ChildValue", result);
        }
        
        [Test]
        public void GetXmlFieldValue_ChildElementEmpty_FallsBackToAttribute()
        {
            // Arrange
            var doc = new XmlDocument();
            var parent = doc.CreateElement("Parent");
            var child = doc.CreateElement("Field1");
            child.InnerText = ""; // 空文本
            parent.AppendChild(child);
            parent.SetAttribute("Field1", "AttributeValue");
            
            // Act
            var result = ConfigParseHelper.GetXmlFieldValue(parent, "Field1");
            
            // Assert
            Assert.AreEqual("AttributeValue", result);
        }
        
        [Test]
        public void GetXmlFieldValue_OnlyAttribute_ReturnsAttribute()
        {
            // Arrange
            var doc = new XmlDocument();
            var parent = doc.CreateElement("Parent");
            parent.SetAttribute("Field1", "AttributeValue");
            
            // Act
            var result = ConfigParseHelper.GetXmlFieldValue(parent, "Field1");
            
            // Assert
            Assert.AreEqual("AttributeValue", result);
        }
        
        [Test]
        public void GetXmlFieldValue_NeitherExists_ReturnsEmpty()
        {
            // Arrange
            var doc = new XmlDocument();
            var parent = doc.CreateElement("Parent");
            
            // Act
            var result = ConfigParseHelper.GetXmlFieldValue(parent, "NonExistent");
            
            // Assert
            Assert.AreEqual(string.Empty, result);
        }
        
        [Test]
        public void GetXmlFieldValue_WithWhitespace_ReturnsTrimed()
        {
            // Arrange
            var doc = new XmlDocument();
            var parent = doc.CreateElement("Parent");
            var child = doc.CreateElement("Field1");
            child.InnerText = "  ValueWithSpaces  ";
            parent.AppendChild(child);
            
            // Act
            var result = ConfigParseHelper.GetXmlFieldValue(parent, "Field1");
            
            // Assert
            Assert.AreEqual("ValueWithSpaces", result);
        }
        
        #endregion
        
        #region IsStrictMode 测试
        
        [Test]
        public void IsStrictMode_NoneMode_ReturnsTrue()
        {
            var context = new ConfigParseContext { Mode = OverrideMode.None };
            Assert.IsTrue(ConfigParseHelper.IsStrictMode(in context));
        }
        
        [Test]
        public void IsStrictMode_ReWriteMode_ReturnsTrue()
        {
            var context = new ConfigParseContext { Mode = OverrideMode.ReWrite };
            Assert.IsTrue(ConfigParseHelper.IsStrictMode(in context));
        }
        
        [Test]
        public void IsStrictMode_ModifyMode_ReturnsFalse()
        {
            var context = new ConfigParseContext { Mode = OverrideMode.Modify };
            Assert.IsFalse(ConfigParseHelper.IsStrictMode(in context));
        }
        
        [Test]
        public void IsStrictMode_DeleteMode_ReturnsFalse()
        {
            var context = new ConfigParseContext { Mode = OverrideMode.Delete };
            Assert.IsFalse(ConfigParseHelper.IsStrictMode(in context));
        }
        
        #endregion
    }
}
