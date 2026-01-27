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
using Unity.Mathematics;
using XMFrame;
using Unity.Collections;
using System;
using System.Collections.Generic;
using System.Xml;
using XMFrame.Interfaces;
using XMFrame.Utils;
using XMFrame.Utils.Attribute;

/// <summary>
/// NestedConfig 的 XML 加载辅助类
/// </summary>
public class NestedConfigClassHelper : IConfigClassHelper<NestedConfig>
{
    private readonly IConfigDataCenter _dataCenter;

    /// <summary>
    /// 构造函数
    /// </summary>
    public NestedConfigClassHelper(IConfigDataCenter dataCenter)
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

    public TableDefine GetTableDefine()
    {
        return new TableDefine(new ModKey("DefaultMod"), ConfigKey<NestedConfigUnManaged>.TableName ?? "NestedConfig");
    }

    public (ModKey mod, string configName) GetPrimaryKey(XMFrame.XConfig config)
    {
        var c = (NestedConfig)config;
        return (new ModKey("DefaultMod"), "Nested");
    }

    public void SetCfgId(XMFrame.XConfig config, CfgId cfgId)
    {
        ((NestedConfig)config).Data = cfgId;
    }

    public void FillToUnmanaged(IConfigDataWriter writer, TableHandle tableHandle, XMFrame.XConfig config, CfgId cfgId)
    {
        var dest = new NestedConfigUnManaged();
        var c = (NestedConfig)config;
        dest.Test = c.Test;
        dest.TestCustom = c.TestCustom;
        dest.TestGlobalConvert = c.TestGlobalConvert;
        writer.AddOrUpdateRow<NestedConfigUnManaged>(tableHandle, cfgId, dest);
    }

    public void AllocTableMap(IConfigDataWriter writer, TableHandle tableHandle, int capacity)
    {
        writer.AllocTableMap<NestedConfigUnManaged>(tableHandle, capacity);
    }

    public void AddPrimaryKeyOnly(IConfigDataWriter writer, TableHandle tableHandle, CfgId cfgId)
    {
        writer.AddPrimaryKeyOnly<NestedConfigUnManaged>(tableHandle, cfgId);
    }

    /// <summary>
    /// 从 XML 元素加载单个配置项，返回配置对象
    /// </summary>
    public NestedConfig LoadFromXmlElement(XmlElement element)
    {
        if (element == null)
        {
            throw new ArgumentNullException(nameof(element));
        }

        // 创建配置对象
        var config = new NestedConfig();

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
            config = new NestedConfig();
        }

        // 解析各个字段
        ParseTest(element, config, overwriteMode);
        ParseTestCustom(element, config, overwriteMode);
        ParseTestGlobalConvert(element, config, overwriteMode);
        ParseTestKeyList(element, config, overwriteMode);
        ParseStrIndex(element, config, overwriteMode);
        ParseStr32(element, config, overwriteMode);
        ParseStr64(element, config, overwriteMode);
        ParseStr(element, config, overwriteMode);
        ParseStrLabel(element, config, overwriteMode);

        return config;
    }

    /// <summary>
    /// 解析字段 Test
    /// </summary>
    private void ParseTest(XmlElement parent, NestedConfig config, EXmlOverwriteMode rootOverwriteMode)
    {
        var fieldElement = parent.SelectSingleNode("Test") as XmlElement;
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
        config.Test = ParseValue<Int32>(fieldElement);
    }

    /// <summary>
    /// 解析字段 TestCustom
    /// </summary>
    private void ParseTestCustom(XmlElement parent, NestedConfig config, EXmlOverwriteMode rootOverwriteMode)
    {
        var fieldElement = parent.SelectSingleNode("TestCustom") as XmlElement;
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

        // Unity.Mathematics 类型处理（使用全局转换器）
        config.TestCustom = Parseint2Value(fieldElement);
    }

    /// <summary>
    /// 解析字段 TestGlobalConvert
    /// </summary>
    private void ParseTestGlobalConvert(XmlElement parent, NestedConfig config, EXmlOverwriteMode rootOverwriteMode)
    {
        var fieldElement = parent.SelectSingleNode("TestGlobalConvert") as XmlElement;
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

        // Unity.Mathematics 类型处理（使用全局转换器）
        // 没有全局转换器，尝试使用默认解析
        config.TestGlobalConvert = ParseValue<int2>(fieldElement);
    }

    /// <summary>
    /// 解析字段 TestKeyList
    /// </summary>
    private void ParseTestKeyList(XmlElement parent, NestedConfig config, EXmlOverwriteMode rootOverwriteMode)
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
    /// 解析字段 StrIndex
    /// </summary>
    private void ParseStrIndex(XmlElement parent, NestedConfig config, EXmlOverwriteMode rootOverwriteMode)
    {
        var fieldElement = parent.SelectSingleNode("StrIndex") as XmlElement;
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

        // String 类型处理（根据字符串模式）
        // 托管类型中仍然是 string，直接赋值
        config.StrIndex = ParseStringValue(fieldElement);
    }

    /// <summary>
    /// 解析字段 Str32
    /// </summary>
    private void ParseStr32(XmlElement parent, NestedConfig config, EXmlOverwriteMode rootOverwriteMode)
    {
        var fieldElement = parent.SelectSingleNode("Str32") as XmlElement;
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

        // String 类型处理（根据字符串模式）
        // 托管类型中仍然是 string，直接赋值
        config.Str32 = ParseStringValue(fieldElement);
    }

    /// <summary>
    /// 解析字段 Str64
    /// </summary>
    private void ParseStr64(XmlElement parent, NestedConfig config, EXmlOverwriteMode rootOverwriteMode)
    {
        var fieldElement = parent.SelectSingleNode("Str64") as XmlElement;
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

        // String 类型处理（根据字符串模式）
        // 托管类型中仍然是 string，直接赋值
        config.Str64 = ParseStringValue(fieldElement);
    }

    /// <summary>
    /// 解析字段 Str
    /// </summary>
    private void ParseStr(XmlElement parent, NestedConfig config, EXmlOverwriteMode rootOverwriteMode)
    {
        var fieldElement = parent.SelectSingleNode("Str") as XmlElement;
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

        // String 类型处理（根据字符串模式）
        // 托管类型中仍然是 string，直接赋值
        config.Str = ParseStringValue(fieldElement);
    }

    /// <summary>
    /// 解析字段 StrLabel
    /// </summary>
    private void ParseStrLabel(XmlElement parent, NestedConfig config, EXmlOverwriteMode rootOverwriteMode)
    {
        var fieldElement = parent.SelectSingleNode("StrLabel") as XmlElement;
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

        // StrLabel 类型处理
        var strLabelStr = fieldElement.InnerText.Trim();
        if (!string.IsNullOrEmpty(strLabelStr))
        {
            var parts = strLabelStr.Split(new[] { "::" }, StringSplitOptions.None);
            if (parts.Length == 2)
            {
                config.StrLabel = new StrLabel
                {
                    ModName = parts[0],
                    LabelName = parts[1]
                };
            }
            else if (parts.Length == 1)
            {
                // ModName 省略
                config.StrLabel = new StrLabel
                {
                    ModName = "DefaultMod", // 临时实现
                    LabelName = parts[0]
                };
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
    /// 解析 int2 类型值（使用全局转换器）
    /// </summary>
    private int2 Parseint2Value(XmlElement element)
    {
        if (element == null)
        {
            return default(int2);
        }

        var valueStr = element.InnerText?.Trim() ?? string.Empty;
        if (string.IsNullOrEmpty(valueStr))
        {
            return default(int2);
        }

        var converter = TestInt2Convert.Instance;
        if (converter.TryGetData(valueStr, out var result))
        {
            return result;
        }

        return default(int2);
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
            if (type.FullName == "Unity.Mathematics.int2" || type.Name == "int2")
            {
                return (T)(object)Parseint2Value(element);
            }
        }

        // 嵌套 XConfig 类型解析（使用预生成的解析方法）

        // 嵌套容器类型解析（使用预生成的解析方法）

        // 默认：尝试使用 TypeConverter
        return ParsePrimitiveValue<T>(element);
    }
}

