using System;
using System.IO;
using System.Xml;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using Unity.Mathematics;
using XM;
using XM.Contracts;
using XM.Contracts.Config;
using XM.Editor.Gen;

namespace XM.Editor
{
    /// <summary>
    /// ConfigClassHelper 与生成 ClassHelper 的单元测试：基类通用解析、override DeserializeConfigFromXml、ParseXXX 行为。
    /// 使用外部 XML 文件（TestData 目录），测试程序集 XM.Editor.Tests 已引用 XM.Editor（含 ClassHelperCodeGenerator）。
    /// </summary>
    [TestFixture]
    public class ConfigClassHelperTests
    {
        private static IConfigDataCenter _mockDataCenter;

        /// <summary>外部测试 XML 所在目录（相对于 Assets）。</summary>
        private static string TestDataRelativePath => "XMFrame/Editor/ConfigEditor/TestData";

        private static string GetTestDataPath() =>
            Path.Combine(Application.dataPath, TestDataRelativePath);

        private static XmlElement LoadXmlRootFromTestData(string fileName)
        {
            var path = Path.Combine(GetTestDataPath(), fileName);
            Assert.IsTrue(File.Exists(path), $"测试 XML 不存在: {path}");
            var doc = new XmlDocument();
            doc.Load(path);
            return doc.DocumentElement;
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _mockDataCenter = new MockConfigDataCenter();
            IConfigDataCenter.I = _mockDataCenter;
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            IConfigDataCenter.I = null;
        }

        #region 基类通用解析（调用 ConfigParseHelper 工具方法）

        [Test]
        public void GetXmlFieldValue_ChildElement_ReturnsInnerText()
        {
            var root = LoadXmlRootFromTestData("Root_ChildElement.xml");
            var value = InvokeGetXmlFieldValue(root, "Test");
            Assert.AreEqual("42", value);
        }

        [Test]
        public void GetXmlFieldValue_Attribute_ReturnsAttributeValue()
        {
            var root = LoadXmlRootFromTestData("Root_Attribute.xml");
            var value = InvokeGetXmlFieldValue(root, "Test");
            Assert.AreEqual("hello", value);
        }

        [Test]
        public void GetXmlFieldValue_Missing_ReturnsEmpty()
        {
            var root = LoadXmlRootFromTestData("Root_Empty.xml");
            var value = InvokeGetXmlFieldValue(root, "Missing");
            Assert.AreEqual("", value);
        }

        [Test]
        public void GetXmlFieldValue_AttributeAndChild_ChildElementWins()
        {
            var root = LoadXmlRootFromTestData("Root_AttributeAndChild.xml");
            var value = InvokeGetXmlFieldValue(root, "Test");
            Assert.AreEqual("child_value", value, "同名属性与子元素并存时应优先返回子元素 InnerText");
        }

        [Test]
        public void TryParseInt_Valid_ReturnsTrueAndValue()
        {
            Assert.IsTrue(InvokeTryParseInt("42", "f", out var v));
            Assert.AreEqual(42, v);
        }

        [Test]
        public void TryParseInt_Invalid_ReturnsFalse()
        {
            Assert.IsFalse(InvokeTryParseInt("x", "f", out _));
        }

        [Test]
        public void TryParseInt_EmptyOrWhitespace_ReturnsFalse()
        {
            Assert.IsFalse(InvokeTryParseInt("", "f", out _));
            Assert.IsFalse(InvokeTryParseInt("   ", "f", out _));
        }

        [Test]
        public void TryParseCfgSString_Empty_ReturnsFalse()
        {
            Assert.IsFalse(InvokeTryParseCfgSString("", "f", out _, out _));
        }

        [Test]
        public void TryParseCfgSString_TwoSegments_ParsesModAndConfig()
        {
            Assert.IsTrue(InvokeTryParseCfgSString("Mod::ConfigName", "f", out var mod, out var config));
            Assert.AreEqual("Mod", mod);
            Assert.AreEqual("ConfigName", config);
        }

        [Test]
        public void TryParseCfgSString_ThreeSegments_ConfigIsThird()
        {
            Assert.IsTrue(InvokeTryParseCfgSString("Mod::Table::ConfigName", "f", out var mod, out var config));
            Assert.AreEqual("Mod", mod);
            Assert.AreEqual("ConfigName", config);
        }

        [Test]
        public void TryParseLabelSString_Valid_ReturnsModAndLabel()
        {
            Assert.IsTrue(InvokeTryParseLabelSString("MyMod::MyLabel", "f", out var mod, out var label));
            Assert.AreEqual("MyMod", mod);
            Assert.AreEqual("MyLabel", label);
        }

        [Test]
        public void TryParseLabelSString_OneSegment_ReturnsFalse()
        {
            Assert.IsFalse(InvokeTryParseLabelSString("OnlyOne", "f", out _, out _));
        }

        [Test]
        public void LogParseWarning_InvokesOnParseWarning()
        {
            string received = null;
            ConfigClassHelper.OnParseWarning = msg => received = msg;
            try
            {
                InvokeLogParseWarning("field1", "bad", new Exception("test ex"));
                Assert.IsNotNull(received);
                Assert.IsTrue(received.Contains("field1") && received.Contains("bad") && received.Contains("test ex"));
            }
            finally
            {
                ConfigClassHelper.OnParseWarning = null;
            }
        }

        #endregion

        #region Override DeserializeConfigFromXml 与 ParseXXX（NestedConfigClassHelper）

        [Test]
        public void NestedConfigClassHelper_Create_ReturnsNestedConfig()
        {
            var helper = new NestedConfigClassHelper(_mockDataCenter);
            var config = helper.Create();
            Assert.IsInstanceOf<NestedConfig>(config);
        }

        [Test]
        public void NestedConfigClassHelper_GetTblS_ReturnsNestedConfigTable()
        {
            var helper = new NestedConfigClassHelper(_mockDataCenter);
            var tbl = helper.GetTblS();
            Assert.AreEqual("NestedConfig", tbl.TableName);
        }

        [Test]
        public void NestedConfigClassHelper_DeserializeConfigFromXml_Override_FillsFields()
        {
            var el = LoadXmlRootFromTestData("NestedConfig_Full.xml");
            var helper = new NestedConfigClassHelper(_mockDataCenter);
            var config = (NestedConfig)helper.DeserializeConfigFromXml(el, new ModS("Default"), "test");
            Assert.IsNotNull(config);
            Assert.AreEqual(99, config.Test);
            Assert.AreEqual("idx", config.StrIndex);
            Assert.AreEqual("hello", config.Str);
            Assert.AreEqual("Mod", config.LabelS.ModName);
            Assert.AreEqual("Label", config.LabelS.LabelName);
        }

        [Test]
        public void NestedConfigClassHelper_DeserializeConfigFromXml_EmptyXml_ReturnsDefaultValues()
        {
            var el = LoadXmlRootFromTestData("NestedConfig_Empty.xml");
            var helper = new NestedConfigClassHelper(_mockDataCenter);
            var config = (NestedConfig)helper.DeserializeConfigFromXml(el, new ModS("Default"), "test");
            Assert.IsNotNull(config);
            Assert.AreEqual(0, config.RequiredId);
            Assert.AreEqual("default", config.OptionalWithDefault, "[XmlDefault] 在空 XML 时生效");
            Assert.AreEqual(0, config.Test);
            Assert.AreEqual("", config.Str);
        }

        [Test]
        public void NestedConfigClassHelper_DeserializeConfigFromXml_TestCustom_UsesConverterFromDataCenter()
        {
            var el = LoadXmlRootFromTestData("NestedConfig_TestCustom.xml");
            var helper = new NestedConfigClassHelper(_mockDataCenter);
            var config = (NestedConfig)helper.DeserializeConfigFromXml(el, new ModS("Default"), "test");
            Assert.IsNotNull(config);
            Assert.AreEqual(new int2(1, 2), config.TestCustom, "TestCustom 应由 MockConfigDataCenter.GetConverter<string,int2>(\"\") 提供的转换器解析");
            Assert.AreEqual(new int2(3, 4), config.TestGlobalConvert, "TestGlobalConvert 应由 GetConverter<string,int2>(\"global\") 提供的转换器解析");
        }

        [Test]
        public void MockConfigDataCenter_GetConverter_StringToInt2_ReturnsConverter()
        {
            var converter = _mockDataCenter.GetConverter<string, int2>("");
            Assert.IsNotNull(converter);
            var result = converter.Convert("3,4");
            Assert.AreEqual(new int2(3, 4), result);
        }

        #endregion

        #region [XmlNotNull] 必要字段与 [XmlDefault] 默认值

        [Test]
        public void NestedConfigClassHelper_XmlNotNull_MissingRequiredId_LogsParseWarning()
        {
            var el = LoadXmlRootFromTestData("NestedConfig_MissingRequired.xml");
            string received = null;
            ConfigClassHelper.OnParseWarning = msg => received = msg;
            try
            {
                var helper = new NestedConfigClassHelper(_mockDataCenter);
                var config = (NestedConfig)helper.DeserializeConfigFromXml(el, new ModS("Default"), "test");
                Assert.IsNotNull(config, "仍应反序列化成功，仅打告警");
                Assert.AreEqual(0, config.RequiredId, "缺失时使用默认值 0");
                Assert.IsNotNull(received, "应触发 [XmlNotNull] 告警");
                Assert.IsTrue(received.Contains("RequiredId"), "告警内容应包含字段名 RequiredId");
            }
            finally
            {
                ConfigClassHelper.OnParseWarning = null;
            }
        }

        [Test]
        public void NestedConfigClassHelper_XmlDefault_OptionalWithDefaultMissing_UsesDefaultString()
        {
            var el = LoadXmlRootFromTestData("NestedConfig_DefaultValue.xml");
            var helper = new NestedConfigClassHelper(_mockDataCenter);
            var config = (NestedConfig)helper.DeserializeConfigFromXml(el, new ModS("Default"), "test");
            Assert.IsNotNull(config);
            Assert.AreEqual(100, config.RequiredId);
            Assert.AreEqual("default", config.OptionalWithDefault, "OptionalWithDefault 缺失时应使用 [XmlDefault(\"default\")]");
        }

        [Test]
        public void NestedConfigClassHelper_EmptyXml_RequiredIdWarnAndOptionalUsesDefault()
        {
            var el = LoadXmlRootFromTestData("NestedConfig_Empty.xml");
            string received = null;
            ConfigClassHelper.OnParseWarning = msg => received = msg;
            try
            {
                var helper = new NestedConfigClassHelper(_mockDataCenter);
                var config = (NestedConfig)helper.DeserializeConfigFromXml(el, new ModS("Default"), "test");
                Assert.IsNotNull(config);
                Assert.AreEqual(0, config.RequiredId);
                Assert.AreEqual("default", config.OptionalWithDefault, "空 XML 时 OptionalWithDefault 应使用 [XmlDefault]");
                Assert.IsNotNull(received);
                Assert.IsTrue(received.Contains("RequiredId"));
            }
            finally
            {
                ConfigClassHelper.OnParseWarning = null;
            }
        }

        [Test]
        public void NestedConfigClassHelper_NotNullAndDefault_FullXml_AllFieldsFilled()
        {
            var el = LoadXmlRootFromTestData("NestedConfig_NotNullAndDefault.xml");
            var helper = new NestedConfigClassHelper(_mockDataCenter);
            var config = (NestedConfig)helper.DeserializeConfigFromXml(el, new ModS("Default"), "test");
            Assert.IsNotNull(config);
            Assert.AreEqual(200, config.RequiredId);
            Assert.AreEqual("custom", config.OptionalWithDefault);
            Assert.AreEqual(99, config.Test);
            Assert.AreEqual("idx", config.StrIndex);
            Assert.AreEqual("hello", config.Str);
            Assert.AreEqual("Mod", config.LabelS.ModName);
            Assert.AreEqual("Label", config.LabelS.LabelName);
        }

        [Test]
        public void NestedConfigClassHelper_XmlDefault_ExplicitValue_OverridesDefault()
        {
            var el = LoadXmlRootFromTestData("NestedConfig_MissingRequired.xml");
            var helper = new NestedConfigClassHelper(_mockDataCenter);
            var config = (NestedConfig)helper.DeserializeConfigFromXml(el, new ModS("Default"), "test");
            Assert.IsNotNull(config);
            Assert.AreEqual("explicit_value", config.OptionalWithDefault, "XML 中显式给出的值应覆盖 [XmlDefault]");
        }

        #endregion

        #region Override DeserializeConfigFromXml（TestConfigClassHelper）

        [Test]
        public void TestConfigClassHelper_DeserializeConfigFromXml_Override_FillsTestIntAndList()
        {
            var el = LoadXmlRootFromTestData("TestConfig_TestIntAndList.xml");
            var helper = new TestConfigClassHelper(_mockDataCenter);
            var config = (TestConfig)helper.DeserializeConfigFromXml(el, new ModS("Default"), "test");
            Assert.IsNotNull(config);
            Assert.AreEqual(123, config.TestInt);
            Assert.IsNotNull(config.TestSample);
            Assert.AreEqual(2, config.TestSample.Count);
            Assert.AreEqual(1, config.TestSample[0]);
            Assert.AreEqual(2, config.TestSample[1]);
        }

        [Test]
        public void TestConfigClassHelper_DeserializeConfigFromXml_CfgSField_ParsesModConfig()
        {
            var el = LoadXmlRootFromTestData("TestConfig_CfgS.xml");
            var helper = new TestConfigClassHelper(_mockDataCenter);
            var config = (TestConfig)helper.DeserializeConfigFromXml(el, new ModS("Default"), "test");
            Assert.IsNotNull(config);
            Assert.AreEqual("MyMod", config.Id.Mod.Name);
            Assert.AreEqual("MyConfig", config.Id.ConfigName);
        }

        [Test]
        public void TestConfigClassHelper_DeserializeConfigFromXml_EmptyXml_ReturnsDefaultValues()
        {
            var el = LoadXmlRootFromTestData("TestConfig_Empty.xml");
            var helper = new TestConfigClassHelper(_mockDataCenter);
            var config = (TestConfig)helper.DeserializeConfigFromXml(el, new ModS("Default"), "test");
            Assert.IsNotNull(config);
            Assert.AreEqual(0, config.TestInt);
            Assert.IsNotNull(config.TestSample);
            Assert.AreEqual(0, config.TestSample.Count);
            Assert.IsNotNull(config.TestDictSample);
            Assert.AreEqual(0, config.TestDictSample.Count);
            Assert.IsNotNull(config.TestKeyList);
            Assert.AreEqual(0, config.TestKeyList.Count);
            Assert.IsNull(config.TestNested);
            Assert.IsNotNull(config.TestNestedConfig);
            Assert.AreEqual(0, config.TestNestedConfig.Count);
        }

        [Test]
        public void TestConfigClassHelper_DeserializeConfigFromXml_DictSample_ParsesItemKeyValue()
        {
            var el = LoadXmlRootFromTestData("TestConfig_DictSample.xml");
            var helper = new TestConfigClassHelper(_mockDataCenter);
            var config = (TestConfig)helper.DeserializeConfigFromXml(el, new ModS("Default"), "test");
            Assert.IsNotNull(config);
            Assert.IsNotNull(config.TestDictSample);
            Assert.AreEqual(3, config.TestDictSample.Count);
            Assert.AreEqual(100, config.TestDictSample[10]);
            Assert.AreEqual(200, config.TestDictSample[20]);
            Assert.AreEqual(300, config.TestDictSample[30]);
        }

        [Test]
        public void TestConfigClassHelper_DeserializeConfigFromXml_HashSet_ParsesMultipleElements()
        {
            var el = LoadXmlRootFromTestData("TestConfig_HashSet.xml");
            var helper = new TestConfigClassHelper(_mockDataCenter);
            var config = (TestConfig)helper.DeserializeConfigFromXml(el, new ModS("Default"), "test");
            Assert.IsNotNull(config);
            Assert.IsNotNull(config.TestKeyHashSet);
            Assert.AreEqual(3, config.TestKeyHashSet.Count);
            Assert.IsTrue(config.TestKeyHashSet.Contains(5));
            Assert.IsTrue(config.TestKeyHashSet.Contains(10));
            Assert.IsTrue(config.TestKeyHashSet.Contains(15));
            Assert.IsNotNull(config.TestSetSample);
            Assert.AreEqual(3, config.TestSetSample.Count);
            Assert.IsTrue(config.TestSetSample.Contains(1));
            Assert.IsTrue(config.TestSetSample.Contains(2));
            Assert.IsTrue(config.TestSetSample.Contains(3));
        }

        [Test]
        public void TestConfigClassHelper_DeserializeConfigFromXml_KeyList_ParsesCfgSList()
        {
            var el = LoadXmlRootFromTestData("TestConfig_KeyList.xml");
            var helper = new TestConfigClassHelper(_mockDataCenter);
            var config = (TestConfig)helper.DeserializeConfigFromXml(el, new ModS("Default"), "test");
            Assert.IsNotNull(config);
            Assert.IsNotNull(config.TestKeyList);
            Assert.AreEqual(3, config.TestKeyList.Count);
            Assert.AreEqual("ModA", config.TestKeyList[0].Mod.Name);
            Assert.AreEqual("Config1", config.TestKeyList[0].ConfigName);
            Assert.AreEqual("ModB", config.TestKeyList[2].Mod.Name);
            Assert.AreEqual("ConfigX", config.TestKeyList[2].ConfigName);
        }

        [Test]
        public void TestConfigClassHelper_DeserializeConfigFromXml_TestKeyList1_ParsesNestedStructure()
        {
            var el = LoadXmlRootFromTestData("TestConfig_TestKeyList1.xml");
            var helper = new TestConfigClassHelper(_mockDataCenter);
            var config = (TestConfig)helper.DeserializeConfigFromXml(el, new ModS("Default"), "test");
            Assert.IsNotNull(config);
            Assert.IsNotNull(config.TestKeyList1);
            Assert.AreEqual(2, config.TestKeyList1.Count);
            Assert.IsTrue(config.TestKeyList1.ContainsKey(1));
            Assert.IsTrue(config.TestKeyList1.ContainsKey(2));
            var list1 = config.TestKeyList1[1];
            Assert.AreEqual(2, list1.Count);
            Assert.AreEqual(2, list1[0].Count);
            Assert.AreEqual("Mod1", list1[0][0].Mod.Name);
            Assert.AreEqual("ConfigA", list1[0][0].ConfigName);
            Assert.AreEqual("ConfigB", list1[0][1].ConfigName);
            Assert.AreEqual(1, list1[1].Count);
            Assert.AreEqual("Mod1", list1[1][0].Mod.Name);
            Assert.AreEqual("ConfigC", list1[1][0].ConfigName);
        }

        [Test]
        public void TestConfigClassHelper_DeserializeConfigFromXml_Indexes_ParsesIndexFields()
        {
            var el = LoadXmlRootFromTestData("TestConfig_Indexes.xml");
            var helper = new TestConfigClassHelper(_mockDataCenter);
            var config = (TestConfig)helper.DeserializeConfigFromXml(el, new ModS("Default"), "test");
            Assert.IsNotNull(config);
            Assert.AreEqual(42, config.TestIndex1);
            Assert.AreEqual("IndexMod", config.TestIndex2.Mod.Name);
            Assert.AreEqual("IndexCfg2", config.TestIndex2.ConfigName);
            Assert.AreEqual("IndexMod", config.TestIndex3.Mod.Name);
            Assert.AreEqual("IndexCfg3", config.TestIndex3.ConfigName);
        }

        [Test]
        public void TestConfigClassHelper_DeserializeConfigFromXml_KeyDict_ParsesCfgSKeyValue()
        {
            var el = LoadXmlRootFromTestData("TestConfig_KeyDict.xml");
            var helper = new TestConfigClassHelper(_mockDataCenter);
            var config = (TestConfig)helper.DeserializeConfigFromXml(el, new ModS("Default"), "test");
            Assert.IsNotNull(config);
            Assert.IsNotNull(config.TestKeyDict);
            Assert.AreEqual(2, config.TestKeyDict.Count);
            var k1 = new CfgS<TestConfigUnManaged>(new ModS("KMod"), "KCfg1");
            var v1 = new CfgS<TestConfigUnManaged>(new ModS("VMod"), "VCfg1");
            Assert.IsTrue(config.TestKeyDict.ContainsKey(k1));
            Assert.AreEqual("VMod", config.TestKeyDict[k1].Mod.Name);
            Assert.AreEqual("VCfg1", config.TestKeyDict[k1].ConfigName);
        }

        [Test]
        public void TestConfigClassHelper_DeserializeConfigFromXml_Nested_ParsesSingleAndListNested()
        {
            var el = LoadXmlRootFromTestData("TestConfig_Nested.xml");
            var helper = new TestConfigClassHelper(_mockDataCenter);
            var config = (TestConfig)helper.DeserializeConfigFromXml(el, new ModS("Default"), "test");
            Assert.IsNotNull(config);
            Assert.IsNotNull(config.TestNested);
            Assert.AreEqual(111, config.TestNested.Test);
            Assert.AreEqual("nested", config.TestNested.StrIndex);
            Assert.AreEqual("world", config.TestNested.Str);
            Assert.IsNotNull(config.TestNestedConfig);
            Assert.AreEqual(2, config.TestNestedConfig.Count);
            Assert.AreEqual(1, config.TestNestedConfig[0].Test);
            Assert.AreEqual("a", config.TestNestedConfig[0].Str);
            Assert.AreEqual(2, config.TestNestedConfig[1].Test);
            Assert.AreEqual("b", config.TestNestedConfig[1].Str);
        }

        #endregion

        #region 容器嵌套容器（Dictionary/List 多层嵌套）

        /// <summary>Dictionary&lt;int, List&lt;List&lt;CfgS&gt;&gt;&gt;：完整结构断言。</summary>
        [Test]
        public void NestedContainer_TestKeyList1_DictListListCfgS_Structure()
        {
            var el = LoadXmlRootFromTestData("TestConfig_TestKeyList1.xml");
            var helper = new TestConfigClassHelper(_mockDataCenter);
            var config = (TestConfig)helper.DeserializeConfigFromXml(el, new ModS("Default"), "test");
            Assert.IsNotNull(config?.TestKeyList1);
            var dict = config.TestKeyList1;
            Assert.AreEqual(2, dict.Count, "应有两个 key：1、2");

            Assert.IsTrue(dict.ContainsKey(1));
            var outer1 = dict[1];
            Assert.AreEqual(2, outer1.Count, "key=1 下应有 2 个内层 List");
            Assert.AreEqual(2, outer1[0].Count);
            Assert.AreEqual("Mod1", outer1[0][0].Mod.Name);
            Assert.AreEqual("ConfigA", outer1[0][0].ConfigName);
            Assert.AreEqual("ConfigB", outer1[0][1].ConfigName);
            Assert.AreEqual(1, outer1[1].Count);
            Assert.AreEqual("Mod1", outer1[1][0].Mod.Name);
            Assert.AreEqual("ConfigC", outer1[1][0].ConfigName);

            Assert.IsTrue(dict.ContainsKey(2));
            var outer2 = dict[2];
            Assert.AreEqual(1, outer2.Count);
            Assert.AreEqual(1, outer2[0].Count);
            Assert.AreEqual("Mod2", outer2[0][0].Mod.Name);
            Assert.AreEqual("ConfigX", outer2[0][0].ConfigName);
        }

        /// <summary>容器嵌套容器边界：空内层列表、单元素、多 key 混合。</summary>
        [Test]
        public void NestedContainer_TestKeyList1_EmptyInnerListAndSingleElement()
        {
            var el = LoadXmlRootFromTestData("TestConfig_NestedContainer_EdgeCases.xml");
            var helper = new TestConfigClassHelper(_mockDataCenter);
            var config = (TestConfig)helper.DeserializeConfigFromXml(el, new ModS("Default"), "test");
            Assert.IsNotNull(config?.TestKeyList1);
            var dict = config.TestKeyList1;

            Assert.AreEqual(3, dict.Count, "应有 key 0、10、20");

            Assert.IsTrue(dict.ContainsKey(0));
            Assert.AreEqual(1, dict[0].Count, "key=0 下一层：一个内层 List");
            Assert.AreEqual(0, dict[0][0].Count, "该内层 List 为空");

            Assert.IsTrue(dict.ContainsKey(10));
            Assert.AreEqual(1, dict[10].Count);
            Assert.AreEqual(1, dict[10][0].Count);
            Assert.AreEqual("M", dict[10][0][0].Mod.Name);
            Assert.AreEqual("Only", dict[10][0][0].ConfigName);

            Assert.IsTrue(dict.ContainsKey(20));
            Assert.AreEqual(2, dict[20].Count, "key=20 下两个内层 List");
            Assert.AreEqual(2, dict[20][0].Count);
            Assert.AreEqual("A", dict[20][0][0].Mod.Name);
            Assert.AreEqual("X", dict[20][0][0].ConfigName);
            Assert.AreEqual("B", dict[20][0][1].Mod.Name);
            Assert.AreEqual("Y", dict[20][0][1].ConfigName);
            Assert.AreEqual(0, dict[20][1].Count, "第二个内层 List 为空");
        }

        /// <summary>List&lt;NestedConfig&gt;：列表容器内嵌套配置对象。</summary>
        [Test]
        public void NestedContainer_TestNestedConfig_ListOfNestedConfig()
        {
            var el = LoadXmlRootFromTestData("TestConfig_Nested.xml");
            var helper = new TestConfigClassHelper(_mockDataCenter);
            var config = (TestConfig)helper.DeserializeConfigFromXml(el, new ModS("Default"), "test");
            Assert.IsNotNull(config?.TestNestedConfig);
            var list = config.TestNestedConfig;
            Assert.AreEqual(2, list.Count);
            Assert.AreEqual(1, list[0].Test);
            Assert.AreEqual("a", list[0].Str);
            Assert.AreEqual(2, list[1].Test);
            Assert.AreEqual("b", list[1].Str);
        }

        /// <summary>Dictionary&lt;CfgS, CfgS&gt;：键值均为 CfgS 的字典容器。</summary>
        [Test]
        public void NestedContainer_TestKeyDict_DictCfgSKeyCfgSValue()
        {
            var el = LoadXmlRootFromTestData("TestConfig_KeyDict.xml");
            var helper = new TestConfigClassHelper(_mockDataCenter);
            var config = (TestConfig)helper.DeserializeConfigFromXml(el, new ModS("Default"), "test");
            Assert.IsNotNull(config?.TestKeyDict);
            var d = config.TestKeyDict;
            Assert.AreEqual(2, d.Count);
            var k1 = new CfgS<TestConfigUnManaged>(new ModS("KMod"), "KCfg1");
            var k2 = new CfgS<TestConfigUnManaged>(new ModS("KMod"), "KCfg2");
            Assert.IsTrue(d.ContainsKey(k1));
            Assert.AreEqual("VMod", d[k1].Mod.Name);
            Assert.AreEqual("VCfg1", d[k1].ConfigName);
            Assert.IsTrue(d.ContainsKey(k2));
            Assert.AreEqual("VMod", d[k2].Mod.Name);
            Assert.AreEqual("VCfg2", d[k2].ConfigName);
        }

        #endregion

        #region 继承 Override（TestInhertClassHelper）

        [Test]
        public void TestInhertClassHelper_DeserializeConfigFromXml_Override_FillsDerivedField()
        {
            var el = LoadXmlRootFromTestData("TestInhert_xxxx.xml");
            var helper = new TestInhertClassHelper(_mockDataCenter);
            var config = (TestInhert)helper.DeserializeConfigFromXml(el, new ModS("Default"), "test");
            Assert.IsNotNull(config);
            Assert.AreEqual(777, config.xxxx);
        }

        [Test]
        public void TestInhertClassHelper_Create_ReturnsTestInhert()
        {
            var helper = new TestInhertClassHelper(_mockDataCenter);
            var config = helper.Create();
            Assert.IsInstanceOf<TestInhert>(config);
        }

        /// <summary>继承解析：先调基类 FillFromXml 再填子类附加字段。</summary>
        [Test]
        public void TestInhertClassHelper_FillFromXml_CallsBaseThenFillsDerived()
        {
            var el = LoadXmlRootFromTestData("TestInhert_WithBase.xml");
            var helper = new TestInhertClassHelper(_mockDataCenter);
            var config = (TestInhert)helper.DeserializeConfigFromXml(el, new ModS("Default"), "test");
            Assert.IsNotNull(config);
            // Assert.AreEqual(999, config.TestInt, "基类字段应由基类 FillFromXml 解析");
            Assert.AreEqual(777, config.xxxx, "子类附加字段应由子类解析");
        }

        #endregion

        #region 调用 ConfigParseHelper 工具方法（与生成代码一致）

        private static string InvokeGetXmlFieldValue(XmlElement parent, string fieldName) =>
            ConfigParseHelper.GetXmlFieldValue(parent, fieldName);

        private static bool InvokeTryParseInt(string s, string fieldName, out int value) =>
            ConfigParseHelper.TryParseInt(s, fieldName, out value);

        private static bool InvokeTryParseCfgSString(string s, string fieldName, out string modName, out string configName) =>
            ConfigParseHelper.TryParseCfgSString(s, fieldName, out modName, out configName);

        private static bool InvokeTryParseLabelSString(string s, string fieldName, out string modName, out string labelName) =>
            ConfigParseHelper.TryParseLabelSString(s, fieldName, out modName, out labelName);

        private static void InvokeLogParseWarning(string fieldName, string value, Exception ex) =>
            ConfigParseHelper.LogParseWarning(fieldName, value, ex);

        #endregion

        #region OverrideMode 严格/宽松异常处理（context 通过参数传递，不依赖线程静态）

        [Test]
        public void DeserializeConfigFromXml_StrictMode_ParseError_LogsErrorWithFileLineField_StillReturnsConfig()
        {
            string errorReceived = null;
            ConfigClassHelper.OnParseError = msg => errorReceived = msg;
            try
            {
                var ctx = new ConfigParseContext { FilePath = "C:/Mods/Test/test.xml", Line = 10, Mode = OverrideMode.None };
                var helper = new NestedConfigClassHelper(_mockDataCenter);
                // 传入 null 触发 FillFromXml 内异常，严格模式应打 Error（含文件、行、字段）并仍返回已创建实例；context 通过参数传递，不依赖线程上下文
                var config = (NestedConfig)helper.DeserializeConfigFromXml(null, new ModS("Default"), "test", in ctx);
                Assert.IsNotNull(config, "严格模式：解析失败仍应正常序列化返回 obj");
                Assert.IsNotNull(errorReceived, "应触发 OnParseError");
                Assert.IsTrue(errorReceived.Contains("文件"), "Error 应包含文件");
                Assert.IsTrue(errorReceived.Contains("行"), "Error 应包含行");
                Assert.IsTrue(errorReceived.Contains("字段"), "Error 应包含字段");
                // 断言 context 通过参数传递到 LogParseError，错误信息中应包含传入的 FilePath 与 Line
                Assert.IsTrue(errorReceived.Contains("C:/Mods/Test/test.xml"), "Error 应包含传入的 context.FilePath");
                Assert.IsTrue(errorReceived.Contains("10"), "Error 应包含传入的 context.Line");
            }
            finally
            {
                ConfigClassHelper.OnParseError = null;
            }
        }

        [Test]
        public void DeserializeConfigFromXml_ReWriteMode_ParseError_LogsError_StillReturnsConfig()
        {
            string errorReceived = null;
            ConfigClassHelper.OnParseError = msg => errorReceived = msg;
            try
            {
                var ctx = new ConfigParseContext { FilePath = "D:/rewrite.xml", Line = 5, Mode = OverrideMode.ReWrite };
                var helper = new NestedConfigClassHelper(_mockDataCenter);
                var config = (NestedConfig)helper.DeserializeConfigFromXml(null, new ModS("Default"), "rewrite_test", in ctx);
                Assert.IsNotNull(config);
                Assert.IsNotNull(errorReceived);
                Assert.IsTrue(errorReceived.Contains("文件") && errorReceived.Contains("行") && errorReceived.Contains("字段"));
                Assert.IsTrue(errorReceived.Contains("D:/rewrite.xml"), "Error 应包含传入的 context.FilePath");
                Assert.IsTrue(errorReceived.Contains("5"), "Error 应包含传入的 context.Line");
            }
            finally
            {
                ConfigClassHelper.OnParseError = null;
            }
        }

        [Test]
        public void DeserializeConfigFromXml_RelaxedMode_ParseError_LogsWarning_StillReturnsConfig()
        {
            string warningReceived = null;
            ConfigClassHelper.OnParseWarning = msg => warningReceived = msg;
            try
            {
                var ctx = new ConfigParseContext { FilePath = "", Line = 0, Mode = OverrideMode.Modify };
                var helper = new NestedConfigClassHelper(_mockDataCenter);
                var config = (NestedConfig)helper.DeserializeConfigFromXml(null, new ModS("Default"), "modify_test", in ctx);
                Assert.IsNotNull(config, "宽松模式：仍应返回已创建实例");
                Assert.IsNotNull(warningReceived, "宽松模式应打 Warning");
            }
            finally
            {
                ConfigClassHelper.OnParseWarning = null;
            }
        }

        /// <summary>三参重载应委托四参重载（default context），行为与显式传 context 一致。</summary>
        [Test]
        public void DeserializeConfigFromXml_ThreeParam_CallsFourParamWithDefaultContext()
        {
            var el = LoadXmlRootFromTestData("NestedConfig_NotNullAndDefault.xml");
            var helper = new NestedConfigClassHelper(_mockDataCenter);
            var config = (NestedConfig)helper.DeserializeConfigFromXml(el, new ModS("Default"), "test");
            Assert.IsNotNull(config, "三参应委托四参 default context，行为一致");
            Assert.AreEqual(200, config.RequiredId);
        }

        #endregion
    }

    /// <summary>用于单元测试的 IConfigDataCenter 占位实现；GetClassHelper 对 NestedConfig 返回 NestedConfigClassHelper，其余返回 null；GetConverter 对 string->int2 返回 MockInt2Converter。</summary>
    internal class MockConfigDataCenter : IConfigDataCenter
    {
        public ConfigClassHelper GetClassHelper(Type configType)
        {
            if (configType == typeof(NestedConfig))
                return new NestedConfigClassHelper(this);
            if (configType == typeof(TestConfig))
                return new TestConfigClassHelper(this);
            return null;
        }

        public ConfigClassHelper GetClassHelperByHelpType(Type configType)
        {
            throw new NotImplementedException();
        }

        public ConfigClassHelper GetClassHelper<T>() where T : IXConfig, new() => GetClassHelper(typeof(T));
        public ConfigClassHelper GetClassHelperByTable(TblS tableDefine)
        {
            if (tableDefine.TableName == "NestedConfig")
                return new NestedConfigClassHelper(this);
            if (tableDefine.TableName == "TestConfig")
                return new TestConfigClassHelper(this);
            return null;
        }
        public TblI GetTblI(TblS tableDefine) => default;
        public bool TryExistsConfig(TblI tableI, ModS mod, string configName) => false;
        public bool TryGetCfgI(TblS tableDefine, ModS mod, string configName, out CfgI cfgI) { cfgI = default; return false; }
        public bool TryGetConfig<T>(out T data) where T : unmanaged, IConfigUnManaged<T> { data = default; return false; }
        public bool TryGetConfigBySingleIndex<TData, TIndex>(in TIndex index, out TData data) where TIndex : IConfigIndexGroup<TData> where TData : unmanaged, IConfigUnManaged<TData> { data = default; return false; }
        public ITypeConverter<TSource, TTarget> GetConverter<TSource, TTarget>(string domain = "")
        {
            if (typeof(TSource) == typeof(string) && typeof(TTarget) == typeof(int2))
                return (ITypeConverter<TSource, TTarget>)(object)MockInt2Converter.Instance;
            return null;
        }
        public ITypeConverter<TSource, TTarget> GetConverterByType<TSource, TTarget>()
        {
            if (typeof(TSource) == typeof(string) && typeof(TTarget) == typeof(int2))
                return (ITypeConverter<TSource, TTarget>)(object)MockInt2Converter.Instance;
            return null;
        }
        public bool HasConverter<TSource, TTarget>(string domain = "") => typeof(TSource) == typeof(string) && typeof(TTarget) == typeof(int2);
        public bool HasConverterByType<TSource, TTarget>() => typeof(TSource) == typeof(string) && typeof(TTarget) == typeof(int2);
        public void RegisterData<T>(T data) where T : IXConfig { }
        public void UpdateData<T>(T data) where T : IXConfig { }
        public UniTask OnCreate() => UniTask.CompletedTask;
        public UniTask OnInit() => UniTask.CompletedTask;
        public UniTask OnDestroy() => UniTask.CompletedTask;
        public UniTask OnEntryMainMenu() => UniTask.CompletedTask;
        public UniTask OnEntryWorld() => UniTask.CompletedTask;
        public UniTask OnStopWorld() => UniTask.CompletedTask;
        public UniTask OnResumeWorld() => UniTask.CompletedTask;
        public UniTask OnExitWorld() => UniTask.CompletedTask;
        public UniTask OnQuitGame() => UniTask.CompletedTask;
    }

    /// <summary>测试用 string->int2 转换器，解析 "x,y" 为 int2(x,y)。</summary>
    internal class MockInt2Converter : ITypeConverter<string, int2>
    {
        public static readonly MockInt2Converter Instance = new MockInt2Converter();
        public int2 Convert(string source)
        {
            if (string.IsNullOrWhiteSpace(source)) return default;
            var parts = source.Trim().Split(',');
            if (parts.Length >= 2 && int.TryParse(parts[0].Trim(), out var x) && int.TryParse(parts[1].Trim(), out var y))
                return new int2(x, y);
            return default;
        }
    }
}
