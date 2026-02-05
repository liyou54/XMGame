using System;
using System.Collections.Generic;
using XM;
using XM.Contracts.Config;
using XM.Utils.Attribute;
using Unity.Collections;

namespace XM.ConfigNew.Tests.Simple
{
    #region 枚举定义
    
    public enum EItemType : byte
    {
        None = 0,
        Weapon = 1,
        Armor = 2
    }
    
    public enum EQuality
    {
        Common = 0,
        Rare = 1,
        Epic = 2
    }
    
    #endregion
    
    #region 简单配置(测试基本功能)
    
    /// <summary>
    /// 简单配置 - 测试基本类型、枚举、可空、默认值
    /// </summary>
    public partial class SimpleConfig : IXConfig<SimpleConfig, SimpleConfigUnmanaged>
    {
        [XmlNotNull]
        [XmlIndex("IdIndex", false, 0)]
        public int Id;
        
        public string Name;
        
        public EItemType Type;
        
        [XmlDefault("100")]
        public int Value;
        
        public float? OptionalFloat;
    }
    
    public partial struct SimpleConfigUnmanaged : IConfigUnManaged<SimpleConfigUnmanaged>
    {
    }
    
    #endregion
    
    #region 容器配置(测试容器和嵌套)
    
    /// <summary>
    /// 容器配置 - 测试List、Dictionary、HashSet、嵌套容器
    /// </summary>
    public partial class ContainerConfig : IXConfig<ContainerConfig, ContainerConfigUnmanaged>
    {
        // 基本容器
        [XmlDefault("1,2,3")]
        public List<int> IntList;
        
        public Dictionary<string, int> StringIntMap;
        
        public HashSet<EItemType> TypeSet;
        
        // 嵌套容器
        public List<List<int>> Matrix;
    }
    
    public partial struct ContainerConfigUnmanaged : IConfigUnManaged<ContainerConfigUnmanaged>
    {
    }
    
    #endregion
    
    #region 嵌套配置(测试嵌套类型)
    
    /// <summary>
    /// 简单属性配置 - 嵌套配置测试专用（已改名避免冲突）
    /// </summary>
    public partial class SimpleAttrConfig : IXConfig<SimpleAttrConfig, SimpleAttrConfigUnmanaged>
    {
        public int Value;
        public float Multiplier;
    }
    
    public partial struct SimpleAttrConfigUnmanaged : IConfigUnManaged<SimpleAttrConfigUnmanaged>
    {
    }
    
    /// <summary>
    /// 嵌套配置测试 - 测试嵌套配置和容器中的嵌套配置
    /// </summary>
    public partial class NestedTestConfig : IXConfig<NestedTestConfig, NestedTestConfigUnmanaged>
    {
        public SimpleAttrConfig Attribute;
        
        public List<SimpleAttrConfig> AttributeList;
    }
    
    public partial struct NestedTestConfigUnmanaged : IConfigUnManaged<NestedTestConfigUnmanaged>
    {
    }
    
    #endregion
    
    #region Link配置(测试Link关系)
    
    /// <summary>
    /// Link父配置
    /// </summary>
    public partial class ParentConfig : IXConfig<ParentConfig, ParentConfigUnmanaged>
    {
        [XmlIndex("ParentIdIndex", false, 0)]
        public int ParentId;
        
        public string ParentName;
    }
    
    public partial struct ParentConfigUnmanaged : IConfigUnManaged<ParentConfigUnmanaged>
    {
    }
    
    /// <summary>
    /// Link子配置 - 测试Link字段生成
    /// </summary>
    public partial class ChildConfig : IXConfig<ChildConfig, ChildConfigUnmanaged>
    {
        public int ChildId;
        
        [XMLLinkAttribute(false)]
        public CfgS<ParentConfig> Parent;
    }
    
    public partial struct ChildConfigUnmanaged : IConfigUnManaged<ChildConfigUnmanaged>
    {
    }
    
    #endregion
    
    #region 字符串模式配置(测试字符串类型)
    
    /// <summary>
    /// 字符串模式配置 - 测试不同的字符串存储方式
    /// </summary>
    public partial class StringModeConfig : IXConfig<StringModeConfig, StringModeConfigUnmanaged>
    {
        [XmlStringMode(EXmlStrMode.EFix32)]
        public string ShortName;
        
        [XmlStringMode(EXmlStrMode.EStrI)]
        public string LocalizedName;
        
        [XmlStringMode(EXmlStrMode.ELabelI)]
        public string LabelName;
        
        public string DefaultName;  // 默认StrI
    }
    
    public partial struct StringModeConfigUnmanaged : IConfigUnManaged<StringModeConfigUnmanaged>
    {
    }
    
    #endregion
    
    #region 复合索引配置(测试索引)
    
    /// <summary>
    /// 复合索引配置 - 测试单字段索引和复合索引
    /// </summary>
    public partial class IndexConfig : IXConfig<IndexConfig, IndexConfigUnmanaged>
    {
        [XmlIndex("IdIndex", false, 0)]
        public int Id;
        
        [XmlIndex("TypeIndex", true, 0)]
        public EItemType Type;
        
        [XmlIndex("CompositeIndex", false, 0)]
        public string Name;
        
        [XmlIndex("CompositeIndex", false, 1)]
        public int Level;
    }
    
    public partial struct IndexConfigUnmanaged : IConfigUnManaged<IndexConfigUnmanaged>
    {
    }
    
    #endregion
}
