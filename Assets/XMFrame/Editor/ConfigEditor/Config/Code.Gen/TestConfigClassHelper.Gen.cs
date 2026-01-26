using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Xml;
using XMFrame;
using XMFrame.Interfaces;
using XMFrame.Utils;
using XMFrame.Utils.Attribute;
using XMFrame.Interfaces.ConfigMananger;
using XMFrame.Utils;
using System;
using XMFrame;
using System.Collections.Generic;
using System.Xml;
using XMFrame.Interfaces;
using XMFrame.Utils.Attribute;

/// <summary>
/// TestConfig 的 XML 加载辅助类
/// </summary>
public class TestConfigClassHelper : IConfigClassHelper<TestConfig>
{
    private readonly IConfigDataCenter _dataCenter;

    /// <summary>
    /// 构造函数
    /// </summary>
    public TestConfigClassHelper(IConfigDataCenter dataCenter)
    {
        _dataCenter = dataCenter ?? throw new ArgumentNullException(nameof(dataCenter));
    }

    /// <summary>
    /// 从 XML 文件加载配置并注册到管理器
    /// </summary>
    public void LoadFromXml(string xmlFilePath)
    {
        var xmlDoc = new XmlDocument();
        xmlDoc.Load(xmlFilePath);
        
        var root = xmlDoc.DocumentElement;
        if (root == null)
        {
            throw new InvalidOperationException($"XML文件根节点为空: {xmlFilePath}");
        }

        // 遍历所有配置项
        var configItems = root.SelectNodes("ConfigItem");
        if (configItems != null)
        {
            foreach (XmlElement itemElement in configItems)
            {
                RegisterToManager(itemElement);
            }
        }
    }

    /// <summary>
    /// 从 XML 元素加载配置并注册到管理器
    /// </summary>
    public void RegisterToManager(XmlElement element)
    {
        var config = LoadFromXmlElement(element);
        if (config != null)
        {
            _dataCenter.RegisterData(config);
        }
    }

    /// <summary>
    /// 从 XML 元素加载单个配置项，返回配置对象
    /// </summary>
    public TestConfig LoadFromXmlElement(XmlElement element)
    {
        if (element == null)
        {
            throw new ArgumentNullException(nameof(element));
        }

        // 创建配置对象
        var config = new TestConfig();

        // 读取 overwrite 属性
        var overwriteStr = element.GetAttribute("overwrite");
        var overwriteMode = EXmlOverwriteMode.Override;
        if (!string.IsNullOrEmpty(overwriteStr))
        {
            if (Enum.TryParse<EXmlOverwriteMode>(overwriteStr, true, out var parsedMode))
            {
                overwriteMode = parsedMode;
            }
            else
            {
                throw new InvalidOperationException($"无效的 overwrite 模式: {overwriteStr}");
            }
        }

        // 应用 Overwrite 模式
        if (overwriteMode == EXmlOverwriteMode.ClearAll)
        {
            // 清空所有字段（使用默认值）
            config = new TestConfig();
        }

        // 解析各个字段
        ParseId(element, config, overwriteMode);
        ParseTestInt(element, config, overwriteMode);
        ParseTestSample(element, config, overwriteMode);
        ParseTestDictSample(element, config, overwriteMode);
        ParseTestKeyList(element, config, overwriteMode);
        ParseTestKeyList1(element, config, overwriteMode);
        ParseTestKeyHashSet(element, config, overwriteMode);
        ParseTestKeyDict(element, config, overwriteMode);
        ParseTestSetKey(element, config, overwriteMode);
        ParseTestSetSample(element, config, overwriteMode);
        ParseTestNested(element, config, overwriteMode);
        ParseTestNestedConfig(element, config, overwriteMode);
        ParseForeign(element, config, overwriteMode);
        ParseConfigType(element, config, overwriteMode);
        ParseTestIndex1(element, config, overwriteMode);
        ParseTestIndex2(element, config, overwriteMode);
        ParseTestIndex3(element, config, overwriteMode);

        return config;
    }

    /// <summary>
    /// 解析字段 Id
    /// </summary>
    private void ParseId(XmlElement parent, TestConfig config, EXmlOverwriteMode rootOverwriteMode)
    {
        var fieldElement = parent.SelectSingleNode("Id") as XmlElement;
        if (fieldElement == null)
        {
            return; // 字段不存在，跳过
        }

        // 读取字段级别的 overwrite 属性
        var fieldOverwriteStr = fieldElement.GetAttribute("overwrite");
        var fieldOverwriteMode = rootOverwriteMode;
        if (!string.IsNullOrEmpty(fieldOverwriteStr))
        {
            if (Enum.TryParse<EXmlOverwriteMode>(fieldOverwriteStr, true, out var parsedMode))
            {
                fieldOverwriteMode = parsedMode;
            }
        }

        // ConfigKey 类型处理
        var configKeyStr = fieldElement.InnerText.Trim();
        if (!string.IsNullOrEmpty(configKeyStr))
        {
            var parts = configKeyStr.Split(new[] { "::" }, StringSplitOptions.None);
            if (parts.Length == 2)
            {
                var modKey = new ModKey(parts[0]);
                var configName = parts[1];
                config.Id = new ConfigKey<TestConfigUnManaged>(modKey, configName);
            }
            else if (parts.Length == 1)
            {
                // ModKey 省略，使用默认或当前 ModKey
                // TODO: 从上下文获取 ModKey
                var modKey = new ModKey("DefaultMod"); // 临时实现
                config.Id = new ConfigKey<TestConfigUnManaged>(modKey, parts[0]);
            }
        }
    }

    /// <summary>
    /// 解析字段 TestInt
    /// </summary>
    private void ParseTestInt(XmlElement parent, TestConfig config, EXmlOverwriteMode rootOverwriteMode)
    {
        var fieldElement = parent.SelectSingleNode("TestInt") as XmlElement;
        if (fieldElement == null)
        {
            return; // 字段不存在，跳过
        }

        // 读取字段级别的 overwrite 属性
        var fieldOverwriteStr = fieldElement.GetAttribute("overwrite");
        var fieldOverwriteMode = rootOverwriteMode;
        if (!string.IsNullOrEmpty(fieldOverwriteStr))
        {
            if (Enum.TryParse<EXmlOverwriteMode>(fieldOverwriteStr, true, out var parsedMode))
            {
                fieldOverwriteMode = parsedMode;
            }
        }

        // 基本类型处理
        config.TestInt = ParseValue<Int32>(fieldElement);
    }

    /// <summary>
    /// 解析字段 TestSample
    /// </summary>
    private void ParseTestSample(XmlElement parent, TestConfig config, EXmlOverwriteMode rootOverwriteMode)
    {
        var fieldElement = parent.SelectSingleNode("TestSample") as XmlElement;
        if (fieldElement == null)
        {
            return; // 字段不存在，跳过
        }

        // 读取字段级别的 overwrite 属性
        var fieldOverwriteStr = fieldElement.GetAttribute("overwrite");
        var fieldOverwriteMode = rootOverwriteMode;
        if (!string.IsNullOrEmpty(fieldOverwriteStr))
        {
            if (Enum.TryParse<EXmlOverwriteMode>(fieldOverwriteStr, true, out var parsedMode))
            {
                fieldOverwriteMode = parsedMode;
            }
        }

        // List 类型处理
        if (fieldOverwriteMode == EXmlOverwriteMode.ContainerClearAdd || fieldOverwriteMode == EXmlOverwriteMode.ContainerOverride)
        {
            config.TestSample = new List<Int32>();
        }
        else if (fieldOverwriteMode == EXmlOverwriteMode.ContainerRemove)
        {
            // 删除模式：从现有列表中删除指定元素
            if (config.TestSample == null)
            {
                config.TestSample = new List<Int32>();
            }
            var removeItems = fieldElement.SelectNodes("Item");
            if (removeItems != null)
            {
                foreach (XmlElement itemElement in removeItems)
                {
                    var itemValue = ParseValue<Int32>(itemElement);
                    config.TestSample.Remove(itemValue);
                }
            }
            return;
        }
        else if (fieldOverwriteMode == EXmlOverwriteMode.ContainerAdd)
        {
            // 添加模式：添加到现有列表
            if (config.TestSample == null)
            {
                config.TestSample = new List<Int32>();
            }
        }
        else
        {
            // Override 模式：覆盖整个列表
            config.TestSample = new List<Int32>();
        }

        var addItems = fieldElement.SelectNodes("Item");
        if (addItems != null)
        {
            foreach (XmlElement itemElement in addItems)
            {
                var itemValue = ParseValue<Int32>(itemElement);
                config.TestSample.Add(itemValue);
            }
        }
    }

    /// <summary>
    /// 解析字段 TestDictSample
    /// </summary>
    private void ParseTestDictSample(XmlElement parent, TestConfig config, EXmlOverwriteMode rootOverwriteMode)
    {
        var fieldElement = parent.SelectSingleNode("TestDictSample") as XmlElement;
        if (fieldElement == null)
        {
            return; // 字段不存在，跳过
        }

        // 读取字段级别的 overwrite 属性
        var fieldOverwriteStr = fieldElement.GetAttribute("overwrite");
        var fieldOverwriteMode = rootOverwriteMode;
        if (!string.IsNullOrEmpty(fieldOverwriteStr))
        {
            if (Enum.TryParse<EXmlOverwriteMode>(fieldOverwriteStr, true, out var parsedMode))
            {
                fieldOverwriteMode = parsedMode;
            }
        }

        // Dictionary 类型处理
        if (fieldOverwriteMode == EXmlOverwriteMode.ContainerClearAdd || fieldOverwriteMode == EXmlOverwriteMode.ContainerOverride)
        {
            config.TestDictSample = new Dictionary<Int32, Int32>();
        }
        else if (fieldOverwriteMode == EXmlOverwriteMode.ContainerRemove)
        {
            // 删除模式：从现有字典中删除指定键
            if (config.TestDictSample == null)
            {
                config.TestDictSample = new Dictionary<Int32, Int32>();
            }
            var removeItems = fieldElement.SelectNodes("Item");
            if (removeItems != null)
            {
                foreach (XmlElement itemElement in removeItems)
                {
                    var keyValue = ParseValue<Int32>(itemElement.SelectSingleNode("Key") as XmlElement);
                    config.TestDictSample.Remove(keyValue);
                }
            }
            return;
        }
        else if (fieldOverwriteMode == EXmlOverwriteMode.ContainerAdd)
        {
            // 添加模式：添加到现有字典
            if (config.TestDictSample == null)
            {
                config.TestDictSample = new Dictionary<Int32, Int32>();
            }
        }
        else
        {
            // Override 模式：覆盖整个字典
            config.TestDictSample = new Dictionary<Int32, Int32>();
        }

        var addItems = fieldElement.SelectNodes("Item");
        if (addItems != null)
        {
            foreach (XmlElement itemElement in addItems)
            {
                var keyElement = itemElement.SelectSingleNode("Key") as XmlElement;
                var valueElement = itemElement.SelectSingleNode("Value") as XmlElement;
                if (keyElement != null && valueElement != null)
                {
                    var keyValue = ParseValue<Int32>(keyElement);
                    var valueValue = ParseValue<Int32>(valueElement);
                    config.TestDictSample[keyValue] = valueValue;
                }
            }
        }
    }

    /// <summary>
    /// 解析字段 TestKeyList
    /// </summary>
    private void ParseTestKeyList(XmlElement parent, TestConfig config, EXmlOverwriteMode rootOverwriteMode)
    {
        var fieldElement = parent.SelectSingleNode("TestKeyList") as XmlElement;
        if (fieldElement == null)
        {
            return; // 字段不存在，跳过
        }

        // 读取字段级别的 overwrite 属性
        var fieldOverwriteStr = fieldElement.GetAttribute("overwrite");
        var fieldOverwriteMode = rootOverwriteMode;
        if (!string.IsNullOrEmpty(fieldOverwriteStr))
        {
            if (Enum.TryParse<EXmlOverwriteMode>(fieldOverwriteStr, true, out var parsedMode))
            {
                fieldOverwriteMode = parsedMode;
            }
        }

        // List 类型处理
        if (fieldOverwriteMode == EXmlOverwriteMode.ContainerClearAdd || fieldOverwriteMode == EXmlOverwriteMode.ContainerOverride)
        {
            config.TestKeyList = new List<ConfigKey<TestConfigUnManaged>>();
        }
        else if (fieldOverwriteMode == EXmlOverwriteMode.ContainerRemove)
        {
            // 删除模式：从现有列表中删除指定元素
            if (config.TestKeyList == null)
            {
                config.TestKeyList = new List<ConfigKey<TestConfigUnManaged>>();
            }
            var removeItems = fieldElement.SelectNodes("Item");
            if (removeItems != null)
            {
                foreach (XmlElement itemElement in removeItems)
                {
                    var itemValue = ParseValue<ConfigKey<TestConfigUnManaged>>(itemElement);
                    config.TestKeyList.Remove(itemValue);
                }
            }
            return;
        }
        else if (fieldOverwriteMode == EXmlOverwriteMode.ContainerAdd)
        {
            // 添加模式：添加到现有列表
            if (config.TestKeyList == null)
            {
                config.TestKeyList = new List<ConfigKey<TestConfigUnManaged>>();
            }
        }
        else
        {
            // Override 模式：覆盖整个列表
            config.TestKeyList = new List<ConfigKey<TestConfigUnManaged>>();
        }

        var addItems = fieldElement.SelectNodes("Item");
        if (addItems != null)
        {
            foreach (XmlElement itemElement in addItems)
            {
                var itemValue = ParseValue<ConfigKey<TestConfigUnManaged>>(itemElement);
                config.TestKeyList.Add(itemValue);
            }
        }
    }

    /// <summary>
    /// 解析字段 TestKeyList1
    /// </summary>
    private void ParseTestKeyList1(XmlElement parent, TestConfig config, EXmlOverwriteMode rootOverwriteMode)
    {
        var fieldElement = parent.SelectSingleNode("TestKeyList1") as XmlElement;
        if (fieldElement == null)
        {
            return; // 字段不存在，跳过
        }

        // 读取字段级别的 overwrite 属性
        var fieldOverwriteStr = fieldElement.GetAttribute("overwrite");
        var fieldOverwriteMode = rootOverwriteMode;
        if (!string.IsNullOrEmpty(fieldOverwriteStr))
        {
            if (Enum.TryParse<EXmlOverwriteMode>(fieldOverwriteStr, true, out var parsedMode))
            {
                fieldOverwriteMode = parsedMode;
            }
        }

        // Dictionary 类型处理
        if (fieldOverwriteMode == EXmlOverwriteMode.ContainerClearAdd || fieldOverwriteMode == EXmlOverwriteMode.ContainerOverride)
        {
            config.TestKeyList1 = new Dictionary<Int32, List<List<ConfigKey<TestConfigUnManaged>>>>();
        }
        else if (fieldOverwriteMode == EXmlOverwriteMode.ContainerRemove)
        {
            // 删除模式：从现有字典中删除指定键
            if (config.TestKeyList1 == null)
            {
                config.TestKeyList1 = new Dictionary<Int32, List<List<ConfigKey<TestConfigUnManaged>>>>();
            }
            var removeItems = fieldElement.SelectNodes("Item");
            if (removeItems != null)
            {
                foreach (XmlElement itemElement in removeItems)
                {
                    var keyValue = ParseValue<Int32>(itemElement.SelectSingleNode("Key") as XmlElement);
                    config.TestKeyList1.Remove(keyValue);
                }
            }
            return;
        }
        else if (fieldOverwriteMode == EXmlOverwriteMode.ContainerAdd)
        {
            // 添加模式：添加到现有字典
            if (config.TestKeyList1 == null)
            {
                config.TestKeyList1 = new Dictionary<Int32, List<List<ConfigKey<TestConfigUnManaged>>>>();
            }
        }
        else
        {
            // Override 模式：覆盖整个字典
            config.TestKeyList1 = new Dictionary<Int32, List<List<ConfigKey<TestConfigUnManaged>>>>();
        }

        var addItems = fieldElement.SelectNodes("Item");
        if (addItems != null)
        {
            foreach (XmlElement itemElement in addItems)
            {
                var keyElement = itemElement.SelectSingleNode("Key") as XmlElement;
                var valueElement = itemElement.SelectSingleNode("Value") as XmlElement;
                if (keyElement != null && valueElement != null)
                {
                    var keyValue = ParseValue<Int32>(keyElement);
                    var valueValue = ParseValue<List<List<ConfigKey<TestConfigUnManaged>>>>(valueElement);
                    config.TestKeyList1[keyValue] = valueValue;
                }
            }
        }
    }

    /// <summary>
    /// 解析字段 TestKeyHashSet
    /// </summary>
    private void ParseTestKeyHashSet(XmlElement parent, TestConfig config, EXmlOverwriteMode rootOverwriteMode)
    {
        var fieldElement = parent.SelectSingleNode("TestKeyHashSet") as XmlElement;
        if (fieldElement == null)
        {
            return; // 字段不存在，跳过
        }

        // 读取字段级别的 overwrite 属性
        var fieldOverwriteStr = fieldElement.GetAttribute("overwrite");
        var fieldOverwriteMode = rootOverwriteMode;
        if (!string.IsNullOrEmpty(fieldOverwriteStr))
        {
            if (Enum.TryParse<EXmlOverwriteMode>(fieldOverwriteStr, true, out var parsedMode))
            {
                fieldOverwriteMode = parsedMode;
            }
        }

        // HashSet 类型处理
        if (fieldOverwriteMode == EXmlOverwriteMode.ContainerClearAdd || fieldOverwriteMode == EXmlOverwriteMode.ContainerOverride)
        {
            config.TestKeyHashSet = new HashSet<Int32>();
        }
        else if (fieldOverwriteMode == EXmlOverwriteMode.ContainerRemove)
        {
            // 删除模式：从现有集合中删除指定元素
            if (config.TestKeyHashSet == null)
            {
                config.TestKeyHashSet = new HashSet<Int32>();
            }
            var removeItems = fieldElement.SelectNodes("Item");
            if (removeItems != null)
            {
                foreach (XmlElement itemElement in removeItems)
                {
                    var itemValue = ParseValue<Int32>(itemElement);
                    config.TestKeyHashSet.Remove(itemValue);
                }
            }
            return;
        }
        else if (fieldOverwriteMode == EXmlOverwriteMode.ContainerAdd)
        {
            // 添加模式：添加到现有集合
            if (config.TestKeyHashSet == null)
            {
                config.TestKeyHashSet = new HashSet<Int32>();
            }
        }
        else
        {
            // Override 模式：覆盖整个集合
            config.TestKeyHashSet = new HashSet<Int32>();
        }

        var addItems = fieldElement.SelectNodes("Item");
        if (addItems != null)
        {
            foreach (XmlElement itemElement in addItems)
            {
                var itemValue = ParseValue<Int32>(itemElement);
                config.TestKeyHashSet.Add(itemValue);
            }
        }
    }

    /// <summary>
    /// 解析字段 TestKeyDict
    /// </summary>
    private void ParseTestKeyDict(XmlElement parent, TestConfig config, EXmlOverwriteMode rootOverwriteMode)
    {
        var fieldElement = parent.SelectSingleNode("TestKeyDict") as XmlElement;
        if (fieldElement == null)
        {
            return; // 字段不存在，跳过
        }

        // 读取字段级别的 overwrite 属性
        var fieldOverwriteStr = fieldElement.GetAttribute("overwrite");
        var fieldOverwriteMode = rootOverwriteMode;
        if (!string.IsNullOrEmpty(fieldOverwriteStr))
        {
            if (Enum.TryParse<EXmlOverwriteMode>(fieldOverwriteStr, true, out var parsedMode))
            {
                fieldOverwriteMode = parsedMode;
            }
        }

        // Dictionary 类型处理
        if (fieldOverwriteMode == EXmlOverwriteMode.ContainerClearAdd || fieldOverwriteMode == EXmlOverwriteMode.ContainerOverride)
        {
            config.TestKeyDict = new Dictionary<ConfigKey<TestConfigUnManaged>, ConfigKey<TestConfigUnManaged>>();
        }
        else if (fieldOverwriteMode == EXmlOverwriteMode.ContainerRemove)
        {
            // 删除模式：从现有字典中删除指定键
            if (config.TestKeyDict == null)
            {
                config.TestKeyDict = new Dictionary<ConfigKey<TestConfigUnManaged>, ConfigKey<TestConfigUnManaged>>();
            }
            var removeItems = fieldElement.SelectNodes("Item");
            if (removeItems != null)
            {
                foreach (XmlElement itemElement in removeItems)
                {
                    var keyValue = ParseValue<ConfigKey<TestConfigUnManaged>>(itemElement.SelectSingleNode("Key") as XmlElement);
                    config.TestKeyDict.Remove(keyValue);
                }
            }
            return;
        }
        else if (fieldOverwriteMode == EXmlOverwriteMode.ContainerAdd)
        {
            // 添加模式：添加到现有字典
            if (config.TestKeyDict == null)
            {
                config.TestKeyDict = new Dictionary<ConfigKey<TestConfigUnManaged>, ConfigKey<TestConfigUnManaged>>();
            }
        }
        else
        {
            // Override 模式：覆盖整个字典
            config.TestKeyDict = new Dictionary<ConfigKey<TestConfigUnManaged>, ConfigKey<TestConfigUnManaged>>();
        }

        var addItems = fieldElement.SelectNodes("Item");
        if (addItems != null)
        {
            foreach (XmlElement itemElement in addItems)
            {
                var keyElement = itemElement.SelectSingleNode("Key") as XmlElement;
                var valueElement = itemElement.SelectSingleNode("Value") as XmlElement;
                if (keyElement != null && valueElement != null)
                {
                    var keyValue = ParseValue<ConfigKey<TestConfigUnManaged>>(keyElement);
                    var valueValue = ParseValue<ConfigKey<TestConfigUnManaged>>(valueElement);
                    config.TestKeyDict[keyValue] = valueValue;
                }
            }
        }
    }

    /// <summary>
    /// 解析字段 TestSetKey
    /// </summary>
    private void ParseTestSetKey(XmlElement parent, TestConfig config, EXmlOverwriteMode rootOverwriteMode)
    {
        var fieldElement = parent.SelectSingleNode("TestSetKey") as XmlElement;
        if (fieldElement == null)
        {
            return; // 字段不存在，跳过
        }

        // 读取字段级别的 overwrite 属性
        var fieldOverwriteStr = fieldElement.GetAttribute("overwrite");
        var fieldOverwriteMode = rootOverwriteMode;
        if (!string.IsNullOrEmpty(fieldOverwriteStr))
        {
            if (Enum.TryParse<EXmlOverwriteMode>(fieldOverwriteStr, true, out var parsedMode))
            {
                fieldOverwriteMode = parsedMode;
            }
        }

        // HashSet 类型处理
        if (fieldOverwriteMode == EXmlOverwriteMode.ContainerClearAdd || fieldOverwriteMode == EXmlOverwriteMode.ContainerOverride)
        {
            config.TestSetKey = new HashSet<ConfigKey<TestConfigUnManaged>>();
        }
        else if (fieldOverwriteMode == EXmlOverwriteMode.ContainerRemove)
        {
            // 删除模式：从现有集合中删除指定元素
            if (config.TestSetKey == null)
            {
                config.TestSetKey = new HashSet<ConfigKey<TestConfigUnManaged>>();
            }
            var removeItems = fieldElement.SelectNodes("Item");
            if (removeItems != null)
            {
                foreach (XmlElement itemElement in removeItems)
                {
                    var itemValue = ParseValue<ConfigKey<TestConfigUnManaged>>(itemElement);
                    config.TestSetKey.Remove(itemValue);
                }
            }
            return;
        }
        else if (fieldOverwriteMode == EXmlOverwriteMode.ContainerAdd)
        {
            // 添加模式：添加到现有集合
            if (config.TestSetKey == null)
            {
                config.TestSetKey = new HashSet<ConfigKey<TestConfigUnManaged>>();
            }
        }
        else
        {
            // Override 模式：覆盖整个集合
            config.TestSetKey = new HashSet<ConfigKey<TestConfigUnManaged>>();
        }

        var addItems = fieldElement.SelectNodes("Item");
        if (addItems != null)
        {
            foreach (XmlElement itemElement in addItems)
            {
                var itemValue = ParseValue<ConfigKey<TestConfigUnManaged>>(itemElement);
                config.TestSetKey.Add(itemValue);
            }
        }
    }

    /// <summary>
    /// 解析字段 TestSetSample
    /// </summary>
    private void ParseTestSetSample(XmlElement parent, TestConfig config, EXmlOverwriteMode rootOverwriteMode)
    {
        var fieldElement = parent.SelectSingleNode("TestSetSample") as XmlElement;
        if (fieldElement == null)
        {
            return; // 字段不存在，跳过
        }

        // 读取字段级别的 overwrite 属性
        var fieldOverwriteStr = fieldElement.GetAttribute("overwrite");
        var fieldOverwriteMode = rootOverwriteMode;
        if (!string.IsNullOrEmpty(fieldOverwriteStr))
        {
            if (Enum.TryParse<EXmlOverwriteMode>(fieldOverwriteStr, true, out var parsedMode))
            {
                fieldOverwriteMode = parsedMode;
            }
        }

        // HashSet 类型处理
        if (fieldOverwriteMode == EXmlOverwriteMode.ContainerClearAdd || fieldOverwriteMode == EXmlOverwriteMode.ContainerOverride)
        {
            config.TestSetSample = new HashSet<Int32>();
        }
        else if (fieldOverwriteMode == EXmlOverwriteMode.ContainerRemove)
        {
            // 删除模式：从现有集合中删除指定元素
            if (config.TestSetSample == null)
            {
                config.TestSetSample = new HashSet<Int32>();
            }
            var removeItems = fieldElement.SelectNodes("Item");
            if (removeItems != null)
            {
                foreach (XmlElement itemElement in removeItems)
                {
                    var itemValue = ParseValue<Int32>(itemElement);
                    config.TestSetSample.Remove(itemValue);
                }
            }
            return;
        }
        else if (fieldOverwriteMode == EXmlOverwriteMode.ContainerAdd)
        {
            // 添加模式：添加到现有集合
            if (config.TestSetSample == null)
            {
                config.TestSetSample = new HashSet<Int32>();
            }
        }
        else
        {
            // Override 模式：覆盖整个集合
            config.TestSetSample = new HashSet<Int32>();
        }

        var addItems = fieldElement.SelectNodes("Item");
        if (addItems != null)
        {
            foreach (XmlElement itemElement in addItems)
            {
                var itemValue = ParseValue<Int32>(itemElement);
                config.TestSetSample.Add(itemValue);
            }
        }
    }

    /// <summary>
    /// 解析字段 TestNested
    /// </summary>
    private void ParseTestNested(XmlElement parent, TestConfig config, EXmlOverwriteMode rootOverwriteMode)
    {
        var fieldElement = parent.SelectSingleNode("TestNested") as XmlElement;
        if (fieldElement == null)
        {
            return; // 字段不存在，跳过
        }

        // 读取字段级别的 overwrite 属性
        var fieldOverwriteStr = fieldElement.GetAttribute("overwrite");
        var fieldOverwriteMode = rootOverwriteMode;
        if (!string.IsNullOrEmpty(fieldOverwriteStr))
        {
            if (Enum.TryParse<EXmlOverwriteMode>(fieldOverwriteStr, true, out var parsedMode))
            {
                fieldOverwriteMode = parsedMode;
            }
        }

        // 嵌套配置类型处理
        if (fieldOverwriteMode == EXmlOverwriteMode.ClearAll || fieldOverwriteMode == EXmlOverwriteMode.Override)
        {
            config.TestNested = ParseNestedConfig(fieldElement);
        }
    }

    /// <summary>
    /// 解析字段 TestNestedConfig
    /// </summary>
    private void ParseTestNestedConfig(XmlElement parent, TestConfig config, EXmlOverwriteMode rootOverwriteMode)
    {
        var fieldElement = parent.SelectSingleNode("TestNestedConfig") as XmlElement;
        if (fieldElement == null)
        {
            return; // 字段不存在，跳过
        }

        // 读取字段级别的 overwrite 属性
        var fieldOverwriteStr = fieldElement.GetAttribute("overwrite");
        var fieldOverwriteMode = rootOverwriteMode;
        if (!string.IsNullOrEmpty(fieldOverwriteStr))
        {
            if (Enum.TryParse<EXmlOverwriteMode>(fieldOverwriteStr, true, out var parsedMode))
            {
                fieldOverwriteMode = parsedMode;
            }
        }

        // List 类型处理
        if (fieldOverwriteMode == EXmlOverwriteMode.ContainerClearAdd || fieldOverwriteMode == EXmlOverwriteMode.ContainerOverride)
        {
            config.TestNestedConfig = new List<NestedConfig>();
        }
        else if (fieldOverwriteMode == EXmlOverwriteMode.ContainerRemove)
        {
            // 删除模式：从现有列表中删除指定元素
            if (config.TestNestedConfig == null)
            {
                config.TestNestedConfig = new List<NestedConfig>();
            }
            var removeItems = fieldElement.SelectNodes("Item");
            if (removeItems != null)
            {
                foreach (XmlElement itemElement in removeItems)
                {
                    var itemValue = ParseValue<NestedConfig>(itemElement);
                    config.TestNestedConfig.Remove(itemValue);
                }
            }
            return;
        }
        else if (fieldOverwriteMode == EXmlOverwriteMode.ContainerAdd)
        {
            // 添加模式：添加到现有列表
            if (config.TestNestedConfig == null)
            {
                config.TestNestedConfig = new List<NestedConfig>();
            }
        }
        else
        {
            // Override 模式：覆盖整个列表
            config.TestNestedConfig = new List<NestedConfig>();
        }

        var addItems = fieldElement.SelectNodes("Item");
        if (addItems != null)
        {
            foreach (XmlElement itemElement in addItems)
            {
                var itemValue = ParseValue<NestedConfig>(itemElement);
                config.TestNestedConfig.Add(itemValue);
            }
        }
    }

    /// <summary>
    /// 解析字段 Foreign
    /// </summary>
    private void ParseForeign(XmlElement parent, TestConfig config, EXmlOverwriteMode rootOverwriteMode)
    {
        var fieldElement = parent.SelectSingleNode("Foreign") as XmlElement;
        if (fieldElement == null)
        {
            return; // 字段不存在，跳过
        }

        // 读取字段级别的 overwrite 属性
        var fieldOverwriteStr = fieldElement.GetAttribute("overwrite");
        var fieldOverwriteMode = rootOverwriteMode;
        if (!string.IsNullOrEmpty(fieldOverwriteStr))
        {
            if (Enum.TryParse<EXmlOverwriteMode>(fieldOverwriteStr, true, out var parsedMode))
            {
                fieldOverwriteMode = parsedMode;
            }
        }

        // ConfigKey 类型处理
        var configKeyStr = fieldElement.InnerText.Trim();
        if (!string.IsNullOrEmpty(configKeyStr))
        {
            var parts = configKeyStr.Split(new[] { "::" }, StringSplitOptions.None);
            if (parts.Length == 2)
            {
                var modKey = new ModKey(parts[0]);
                var configName = parts[1];
                config.Foreign = new ConfigKey<TestConfigUnManaged>(modKey, configName);
            }
            else if (parts.Length == 1)
            {
                // ModKey 省略，使用默认或当前 ModKey
                // TODO: 从上下文获取 ModKey
                var modKey = new ModKey("DefaultMod"); // 临时实现
                config.Foreign = new ConfigKey<TestConfigUnManaged>(modKey, parts[0]);
            }
        }
    }

    /// <summary>
    /// 解析字段 ConfigType
    /// </summary>
    private void ParseConfigType(XmlElement parent, TestConfig config, EXmlOverwriteMode rootOverwriteMode)
    {
        var fieldElement = parent.SelectSingleNode("ConfigType") as XmlElement;
        if (fieldElement == null)
        {
            return; // 字段不存在，跳过
        }

        // 读取字段级别的 overwrite 属性
        var fieldOverwriteStr = fieldElement.GetAttribute("overwrite");
        var fieldOverwriteMode = rootOverwriteMode;
        if (!string.IsNullOrEmpty(fieldOverwriteStr))
        {
            if (Enum.TryParse<EXmlOverwriteMode>(fieldOverwriteStr, true, out var parsedMode))
            {
                fieldOverwriteMode = parsedMode;
            }
        }

        // Type 类型处理（需要转换器转换为 TypeId）
        var typeStr = fieldElement.InnerText?.Trim() ?? string.Empty;
        if (!string.IsNullOrEmpty(typeStr))
        {
            var type = Type.GetType(typeStr);
            if (type != null && IConfigDataCenter.I != null)
            {
                var converter = IConfigDataCenter.I.GetConverter<Type, TypeId>("");
                if (converter != null)
                {
                    config.ConfigType = type;
                    // 注意：TypeId 的转换会在后续处理中完成
                }
            }
            else
            {
                config.ConfigType = type;
            }
        }
    }

    /// <summary>
    /// 解析字段 TestIndex1
    /// </summary>
    private void ParseTestIndex1(XmlElement parent, TestConfig config, EXmlOverwriteMode rootOverwriteMode)
    {
        var fieldElement = parent.SelectSingleNode("TestIndex1") as XmlElement;
        if (fieldElement == null)
        {
            return; // 字段不存在，跳过
        }

        // 读取字段级别的 overwrite 属性
        var fieldOverwriteStr = fieldElement.GetAttribute("overwrite");
        var fieldOverwriteMode = rootOverwriteMode;
        if (!string.IsNullOrEmpty(fieldOverwriteStr))
        {
            if (Enum.TryParse<EXmlOverwriteMode>(fieldOverwriteStr, true, out var parsedMode))
            {
                fieldOverwriteMode = parsedMode;
            }
        }

        // 基本类型处理
        config.TestIndex1 = ParseValue<Int32>(fieldElement);
    }

    /// <summary>
    /// 解析字段 TestIndex2
    /// </summary>
    private void ParseTestIndex2(XmlElement parent, TestConfig config, EXmlOverwriteMode rootOverwriteMode)
    {
        var fieldElement = parent.SelectSingleNode("TestIndex2") as XmlElement;
        if (fieldElement == null)
        {
            return; // 字段不存在，跳过
        }

        // 读取字段级别的 overwrite 属性
        var fieldOverwriteStr = fieldElement.GetAttribute("overwrite");
        var fieldOverwriteMode = rootOverwriteMode;
        if (!string.IsNullOrEmpty(fieldOverwriteStr))
        {
            if (Enum.TryParse<EXmlOverwriteMode>(fieldOverwriteStr, true, out var parsedMode))
            {
                fieldOverwriteMode = parsedMode;
            }
        }

        // ConfigKey 类型处理
        var configKeyStr = fieldElement.InnerText.Trim();
        if (!string.IsNullOrEmpty(configKeyStr))
        {
            var parts = configKeyStr.Split(new[] { "::" }, StringSplitOptions.None);
            if (parts.Length == 2)
            {
                var modKey = new ModKey(parts[0]);
                var configName = parts[1];
                config.TestIndex2 = new ConfigKey<TestConfigUnManaged>(modKey, configName);
            }
            else if (parts.Length == 1)
            {
                // ModKey 省略，使用默认或当前 ModKey
                // TODO: 从上下文获取 ModKey
                var modKey = new ModKey("DefaultMod"); // 临时实现
                config.TestIndex2 = new ConfigKey<TestConfigUnManaged>(modKey, parts[0]);
            }
        }
    }

    /// <summary>
    /// 解析字段 TestIndex3
    /// </summary>
    private void ParseTestIndex3(XmlElement parent, TestConfig config, EXmlOverwriteMode rootOverwriteMode)
    {
        var fieldElement = parent.SelectSingleNode("TestIndex3") as XmlElement;
        if (fieldElement == null)
        {
            return; // 字段不存在，跳过
        }

        // 读取字段级别的 overwrite 属性
        var fieldOverwriteStr = fieldElement.GetAttribute("overwrite");
        var fieldOverwriteMode = rootOverwriteMode;
        if (!string.IsNullOrEmpty(fieldOverwriteStr))
        {
            if (Enum.TryParse<EXmlOverwriteMode>(fieldOverwriteStr, true, out var parsedMode))
            {
                fieldOverwriteMode = parsedMode;
            }
        }

        // ConfigKey 类型处理
        var configKeyStr = fieldElement.InnerText.Trim();
        if (!string.IsNullOrEmpty(configKeyStr))
        {
            var parts = configKeyStr.Split(new[] { "::" }, StringSplitOptions.None);
            if (parts.Length == 2)
            {
                var modKey = new ModKey(parts[0]);
                var configName = parts[1];
                config.TestIndex3 = new ConfigKey<TestConfigUnManaged>(modKey, configName);
            }
            else if (parts.Length == 1)
            {
                // ModKey 省略，使用默认或当前 ModKey
                // TODO: 从上下文获取 ModKey
                var modKey = new ModKey("DefaultMod"); // 临时实现
                config.TestIndex3 = new ConfigKey<TestConfigUnManaged>(modKey, parts[0]);
            }
        }
    }


    /// <summary>
    /// 解析基本类型值
    /// </summary>
    private T ParsePrimitiveValue<T>(XmlElement element)
    {
        if (element == null)
        {
            return default(T);
        }

        var valueStr = element.InnerText?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(valueStr))
        {
            return default(T);
        }

        var type = typeof(T);
        
        // 使用 TypeConverter 进行转换
        var converter = System.ComponentModel.TypeDescriptor.GetConverter(type);
        if (converter != null && converter.CanConvertFrom(typeof(string)))
        {
            try
            {
                return (T)converter.ConvertFromString(valueStr);
            }
            catch
            {
                return default(T);
            }
        }

        return default(T);
    }

    /// <summary>
    /// 解析字符串值
    /// </summary>
    private string ParseStringValue(XmlElement element)
    {
        return element?.InnerText?.Trim() ?? string.Empty;
    }


    /// <summary>
    /// 解析 ConfigKey&lt;TestConfigUnManaged&gt; 类型值
    /// </summary>
    private ConfigKey<TestConfigUnManaged> ParseConfigKey_TestConfigUnManaged(XmlElement element)
    {
        if (element == null)
        {
            return default(ConfigKey<TestConfigUnManaged>);
        }

        var valueStr = element.InnerText?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(valueStr))
        {
            return default(ConfigKey<TestConfigUnManaged>);
        }

        var parts = valueStr.Split(new[] { "::" }, StringSplitOptions.None);
        if (parts.Length == 2)
        {
            var modKey = new ModKey(parts[0]);
            var configName = parts[1];
            return new ConfigKey<TestConfigUnManaged>(modKey, configName);
        }
        else if (parts.Length == 1)
        {
            var modKey = new ModKey("DefaultMod");
            return new ConfigKey<TestConfigUnManaged>(modKey, parts[0]);
        }

        return default(ConfigKey<TestConfigUnManaged>);
    }

    /// <summary>
    /// 解析 NestedConfig 类型值
    /// </summary>
    private NestedConfig ParseNestedConfig(XmlElement element)
    {
        if (element == null)
        {
            return null;
        }

        var configType = typeof(NestedConfig);
        var helper = (IConfigClassHelper<NestedConfig>)_dataCenter.GetClassHelper(configType);
        return helper.LoadFromXmlElement(element);
    }


    /// <summary>
    /// 通用值解析方法
    /// </summary>
    private T ParseValue<T>(XmlElement element)
    {
        if (element == null)
        {
            return default(T);
        }

        var valueStr = element.InnerText?.Trim() ?? string.Empty;

        // 基本类型解析（使用 ParsePrimitiveValue）
        var type = typeof(T);
        if (type == typeof(int) || type == typeof(long) || type == typeof(short) || 
            type == typeof(byte) || type == typeof(float) || type == typeof(double) || 
            type == typeof(bool) || type == typeof(string))
        {
            return ParsePrimitiveValue<T>(element);
        }

        // ConfigKey 类型解析（使用预生成的解析方法）
        if (typeof(T) == typeof(ConfigKey<TestConfigUnManaged>))
        {
            return (T)(object)ParseConfigKey_TestConfigUnManaged(element);
        }

        // Unity.Mathematics 类型解析（使用预生成的解析方法）
        if (type.Namespace == "Unity.Mathematics")
        {
        }

        // 嵌套 XConfig 类型解析（使用预生成的解析方法）
        if (typeof(T) == typeof(NestedConfig))
        {
            return (T)(object)ParseNestedConfig(element);
        }

        // 嵌套容器类型解析（使用预生成的解析方法）

        // 默认：尝试使用 TypeConverter
        return ParsePrimitiveValue<T>(element);
    }
}

