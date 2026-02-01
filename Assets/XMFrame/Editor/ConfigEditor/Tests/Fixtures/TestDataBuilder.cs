using System;
using System.Xml;
using XM.Contracts.Config;

namespace XM.Editor.Tests.Fixtures
{
    /// <summary>
    /// 测试数据构建器，提供流畅API构建测试数据
    /// 职责：
    /// 1. 构建XML测试数据
    /// 2. 构建类型信息测试数据
    /// 3. 提供Builder模式的流畅API
    /// </summary>
    public class TestDataBuilder
    {
        /// <summary>
        /// 创建XML ConfigItem元素（支持流畅API）
        /// </summary>
        /// <param name="cls">cls属性值</param>
        /// <param name="id">id属性值</param>
        public XmlElementBuilder CreateConfigItem(string cls, string id)
        {
            return new XmlElementBuilder()
                .WithAttribute("cls", cls)
                .WithAttribute("id", id);
        }
        
        /// <summary>
        /// 创建Mock类型信息
        /// </summary>
        public ConfigTypeInfo CreateTypeInfo(Type managedType, Type unmanagedType)
        {
            return new ConfigTypeInfo
            {
                ManagedType = managedType,
                UnmanagedType = unmanagedType,
                // 其他字段根据需要初始化
            };
        }
        
        /// <summary>
        /// 创建测试用的TblS
        /// </summary>
        public TblS CreateTblS(string modName, string tableName)
        {
            return new TblS(modName, tableName);
        }
        
        /// <summary>
        /// 创建测试用的CfgS
        /// </summary>
        public CfgS CreateCfgS(string modName, string tableName, string configName)
        {
            var modS = new ModS(modName);
            var tblS = new TblS(modName, tableName);
            return new CfgS(modS, tblS, configName);
        }
    }
    
    /// <summary>
    /// XML元素构建器（流畅API）
    /// 用法示例：
    /// <code>
    /// var xml = builder.CreateConfigItem("TestMod::TestConfig", "cfg1")
    ///     .WithChild("Field1", "Value1")
    ///     .WithChild("Field2", "123")
    ///     .Build();
    /// </code>
    /// </summary>
    public class XmlElementBuilder
    {
        private readonly XmlDocument _doc = new XmlDocument();
        private readonly XmlElement _element;
        
        public XmlElementBuilder() 
        { 
            _element = _doc.CreateElement("ConfigItem");
            _doc.AppendChild(_element);
        }
        
        /// <summary>
        /// 添加属性
        /// </summary>
        public XmlElementBuilder WithAttribute(string name, string value)
        {
            if (!string.IsNullOrEmpty(value))
                _element.SetAttribute(name, value);
            return this;
        }
        
        /// <summary>
        /// 添加子元素（简单文本值）
        /// </summary>
        public XmlElementBuilder WithChild(string name, string value)
        {
            var child = _doc.CreateElement(name);
            child.InnerText = value ?? string.Empty;
            _element.AppendChild(child);
            return this;
        }
        
        /// <summary>
        /// 添加子元素（复杂XML）
        /// </summary>
        public XmlElementBuilder WithChildElement(XmlElement child)
        {
            var imported = _doc.ImportNode(child, deep: true);
            _element.AppendChild(imported);
            return this;
        }
        
        /// <summary>
        /// 设置内部文本
        /// </summary>
        public XmlElementBuilder WithInnerText(string text)
        {
            _element.InnerText = text ?? string.Empty;
            return this;
        }
        
        /// <summary>
        /// 构建最终的XmlElement
        /// </summary>
        public XmlElement Build() => _element;
        
        /// <summary>
        /// 构建并获取XML字符串
        /// </summary>
        public string BuildAsString()
        {
            return _element.OuterXml;
        }
    }
    
    /// <summary>
    /// 配置类型信息（简化版，用于测试）
    /// </summary>
    public class ConfigTypeInfo
    {
        public Type ManagedType { get; set; }
        public Type UnmanagedType { get; set; }
        // 可以根据需要添加更多字段
    }
}
