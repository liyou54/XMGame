using System;
using System.Collections.Generic;
using XM;
using XM.Contracts.Config;
using XM.Utils.Attribute;
using Unity.Collections;

namespace XM.ConfigNew.Tests.Data
{
    #region 枚举定义
    
    /// <summary>
    /// 物品类型枚举
    /// </summary>
    public enum EItemType 
    {
        None = 0,
        Weapon = 1,
        Armor = 2,
        Consumable = 3,
        Material = 4,
        Quest = 5
    }
    
    /// <summary>
    /// 物品品质枚举
    /// </summary>
    public enum EItemQuality
    {
        Common = 0,
        Uncommon = 1,
        Rare = 2,
        Epic = 3,
        Legendary = 4,
        Mythic = 5
    }
    
    /// <summary>
    /// 属性类型枚举
    /// </summary>
    public enum EAttributeType
    {
        Health = 1,
        Mana = 2,
        Attack = 3,
        Defense = 4,
        Speed = 5
    }
    
    #endregion
    
    #region 嵌套配置类
    
    /// <summary>
    /// 属性配置(嵌套配置)
    /// </summary>
    [XmlDefined("Attribute")]
    public class AttributeConfig : IXConfig<AttributeConfig, AttributeConfigUnmanaged>
    {
        [XmlNotNull]
        public EAttributeType Type;
        
        [XmlDefault("0")]
        public int BaseValue;
        
        [XmlDefault("1.0")]
        public float Multiplier;
        
        public int? BonusValue;
    }
    
    public struct AttributeConfigUnmanaged : IConfigUnManaged<AttributeConfigUnmanaged>
    {

    }
    
    /// <summary>
    /// 价格配置(嵌套配置)
    /// </summary>
    public class PriceConfig : IXConfig<PriceConfig, PriceConfigUnmanaged>
    {
        [XmlNotNull]
        public int Gold;
        
        [XmlDefault("0")]
        public int Silver;
        
        public int? Diamond;
    }
    
    public struct PriceConfigUnmanaged : IConfigUnManaged<PriceConfigUnmanaged>
    {

    }
    
    /// <summary>
    /// 效果配置(嵌套配置,用于容器中)
    /// </summary>
    public class EffectConfig : IXConfig<EffectConfig, EffectConfigUnmanaged>
    {
        public string EffectName;
        
        [XmlDefault("1")]
        public int Duration;
        
        public float Value;
    }
    
    public struct EffectConfigUnmanaged : IConfigUnManaged<EffectConfigUnmanaged>
    {
        public int EffectNameId;
        public int Duration;
        public float Value;
        
        public string ToString(object dataContainer) => $"Effect[{EffectNameId}:{Value}]";
    }
    
    #endregion
    
    #region Link测试专用配置类
    
    /// <summary>
    /// Link父配置(用于测试Link关系)
    /// </summary>
    [XmlDefined("LinkParent")]
    public class LinkParentConfig : IXConfig<LinkParentConfig, LinkParentConfigUnmanaged>
    {
        [XmlNotNull]
        [XmlIndex("ParentIdIndex", false, 0)]
        public int ParentId;
        
        [XmlNotNull]
        public string ParentName;
        
        public string ParentData;
    }
    
    public struct LinkParentConfigUnmanaged : IConfigUnManaged<LinkParentConfigUnmanaged>
    {

    }
    
    /// <summary>
    /// SingleLink子配置(一对一关系)
    /// 多个子配置可以引用同一个父配置,但每个子配置只能引用一个父配置
    /// </summary>
    [XmlDefined("SingleLinkChild")]
    public class SingleLinkChildConfig : IXConfig<SingleLinkChildConfig, SingleLinkChildConfigUnmanaged>
    {
        [XmlNotNull]
        [XmlIndex("ChildIdIndex", false, 0)]
        public int ChildId;
        
        public string ChildName;
        
        /// <summary>单个父配置Link(multilink=false)</summary>
        [XMLLinkAttribute(false)]
        public CfgS<LinkParentConfig> Parent;
    }
    
    public partial struct SingleLinkChildConfigUnmanaged : IConfigUnManaged<SingleLinkChildConfigUnmanaged>
    {

    }
    
    /// <summary>
    /// SingleLink子配置(一对一关系)
    /// 多个子配置可以引用同一个父配置,但每个子配置只能引用一个父配置
    /// </summary>
    [XmlDefined("SingleLinkChild")]
    public class ListLinkChildConfig : IXConfig<ListLinkChildConfig, ListLinkChildConfigUnmanaged>
    {
        [XmlNotNull]
        [XmlIndex("ChildIdIndex", false, 0)]
        public int ChildId;
        
        public string ChildName;
        
        /// <summary>单个父配置Link(multilink=false)</summary>
        [XMLLinkAttribute(false)]
        public List< CfgS<LinkParentConfig>> Parent;
    }
    
    public struct ListLinkChildConfigUnmanaged : IConfigUnManaged<ListLinkChildConfigUnmanaged>
    {

    }
    
    #endregion
    
    #region 父配置类(用于继承)
    
    /// <summary>
    /// 基础物品配置(父类)
    /// </summary>
    [XmlDefined("BaseItem")]
    public class BaseItemConfig : IXConfig<BaseItemConfig, BaseItemConfigUnmanaged>
    {
        [XmlNotNull]
        [XmlIndex("IdIndex", false, 0)]
        public int Id;
        
        [XmlNotNull]
        [XmlIndex("NameIndex", false, 0)]
        public string Name;
        
        [XmlDefault("Common")]
        public EItemQuality Quality;
        
        [XmlDefault("1")]
        public int StackSize;
        
        public string Description;
    }
    
    public struct BaseItemConfigUnmanaged : IConfigUnManaged<BaseItemConfigUnmanaged>
    {

    }
    
    #endregion
    
    #region 复杂配置类(包含所有特性)
    
    /// <summary>
    /// 超级复杂的物品配置类
    /// 包含: 继承、枚举、可空类型、容器、嵌套容器、嵌套配置、Link、多个索引、转换器、默认值等
    /// </summary>
    [XmlDefined("ComplexItem")]
    public class ComplexItemConfig : IXConfig<ComplexItemConfig, ComplexItemConfigUnmanaged>
    {
        #region 基本字段
        
        /// <summary>物品类型</summary>
        [XmlNotNull]
        [XmlIndex("TypeIndex", true, 0)]
        public EItemType ItemType;
        
        /// <summary>等级要求(可空)</summary>
        [XmlIndex("LevelIndex", true, 0)]
        public int? RequiredLevel;
        
        /// <summary>价格(嵌套配置)</summary>
        [XmlNotNull]
        public AttributeConfig Price;
        
        
        #endregion
        
        #region 容器字段
        
        /// <summary>标签列表</summary>
        [XmlDefault("common,item")]
        public List<string> Tags;
        
        /// <summary>属性列表(嵌套配置容器)</summary>
        public List<AttributeConfig> Attributes;
        
        /// <summary>效果列表(嵌套配置容器)</summary>
        [XmlDefault("Buff1,Buff2")]
        public List<EffectConfig> Effects;
        
        /// <summary>整数列表</summary>
        [XmlDefault("1,2,3,4,5")]
        public List<int> IntValues;
        
        /// <summary>枚举列表</summary>
        [XmlDefault("Health,Attack,Defense")]
        public List<EAttributeType> AttributeTypes;
        
        #endregion
        
        #region 嵌套容器字段
        
        /// <summary>二维整数数组</summary>
        public List<List<int>> Matrix;
        
        /// <summary>字典容器</summary>
        public Dictionary<string, int> StringIntMap;
        
        /// <summary>字典+列表嵌套</summary>
        public Dictionary<EAttributeType, List<int>> AttributeValueMap;
        
        /// <summary>三层嵌套容器</summary>
        public List<Dictionary<string, List<float>>> DeepNestedContainer;
        
        /// <summary>HashSet容器</summary>
        public HashSet<int> UniqueIds;
        
        #endregion
        
        #endregion
        
        #region 带转换器的字段
        
        /// <summary>自定义转换器字段</summary>
        [XmlTypeConverter(typeof(CustomItemDataConverter) )]
        public string CustomData;
        
        /// <summary>全局转换器字段</summary>
        [XmlTypeConverter(typeof(GlobalFloatConverter) )]
        public float GlobalValue;
        
        #endregion
        
        #region 带容器解析器的字段
        
        public List<string> ComplexList;
        
        #endregion
        
        #region 复合索引字段
        
        /// <summary>分类</summary>
        [XmlIndex("CategoryIndex", false, 0)]
        [XmlIndex("FullIndex", false, 0)]
        public string Category;
        
        /// <summary>子类型</summary>
        [XmlIndex("FullIndex", false, 1)]
        public int SubType;
        
        /// <summary>等级</summary>
        [XmlIndex("FullIndex", false, 2)]
        public int Level;
        
        #endregion
        
        #region 字符串模式字段
        
        /// <summary>固定字符串32</summary>
        [XmlStringMode(EXmlStrMode.EFix32)]
        public string ShortName;
        
        /// <summary>字符串ID</summary>
        [XmlStringMode(EXmlStrMode.EStrI)]
        public string LocalizedName;
        
        /// <summary>标签ID</summary>
        [XmlStringMode(EXmlStrMode.ELabelI)]
        public string LabelName;
        
        #endregion
    }
    
    public partial struct ComplexItemConfigUnmanaged : IConfigUnManaged<ComplexItemConfigUnmanaged>
    {

    }
    
    
    #region 任务配置(用于Link测试)
    
    /// <summary>
    /// 任务配置
    /// </summary>
    [XmlDefined("Quest")]
    public class QuestConfig : IXConfig<QuestConfig, QuestConfigUnmanaged>
    {
        [XmlNotNull]
        [XmlIndex("QuestIdIndex", false, 0)]
        public int QuestId;
        
        [XmlNotNull]
        public string QuestName;
        
        [XmlDefault("1")]
        public int MinLevel;
        
        /// <summary>奖励物品Link(一对一)</summary>
        [XMLLinkAttribute(false)]
        public CfgS<ComplexItemConfig> RewardItem;
        
        /// <summary>前置任务Link</summary>
        [XMLLinkAttribute(false)]
        public CfgS<QuestConfig> PreQuest;
    }
    
    public struct QuestConfigUnmanaged : IConfigUnManaged<QuestConfigUnmanaged>
    {
  
    }
    
    #endregion
    
    #region 技能配置(多层继承测试)
    
    /// <summary>
    /// 基础技能配置
    /// </summary>
    public class BaseSkillConfig : IXConfig<BaseSkillConfig, BaseSkillConfigUnmanaged>
    {
        [XmlNotNull]
        [XmlIndex("SkillIdIndex", false, 0)]
        public int SkillId;
        
        public string SkillName;
        
        [XmlDefault("1")]
        public int MaxLevel;
    }
    
    public struct BaseSkillConfigUnmanaged : IConfigUnManaged<BaseSkillConfigUnmanaged>
    {
        public int SkillId;
        public int SkillNameId;
        public int MaxLevel;
        
        public string ToString(object dataContainer) => $"BaseSkill[{SkillId}]";
    }
    
    /// <summary>
    /// 主动技能配置(继承BaseSkill)
    /// </summary>
    public class ActiveSkillConfig : BaseSkillConfig, IXConfig<ActiveSkillConfig, ActiveSkillConfigUnmanaged>
    {
        [XmlDefault("10")]
        public int ManaCost;
        
        [XmlDefault("5.0")]
        public float Cooldown;
        
        /// <summary>技能效果列表</summary>
        public List<EffectConfig> SkillEffects;
    }
    
    public struct ActiveSkillConfigUnmanaged : IConfigUnManaged<ActiveSkillConfigUnmanaged>
    {

    }
    
    /// <summary>
    /// 被动技能配置(继承BaseSkill)
    /// </summary>
    public class PassiveSkillConfig : BaseSkillConfig, IXConfig<PassiveSkillConfig, PassiveSkillConfigUnmanaged>
    {
        /// <summary>属性加成列表</summary>
        public List<AttributeConfig> AttributeBonuses;
        
        /// <summary>触发条件</summary>
        public string TriggerCondition;
    }
    
    public struct PassiveSkillConfigUnmanaged : IConfigUnManaged<PassiveSkillConfigUnmanaged>
    {

    }
    
    #endregion
    
    #region 终极复杂配置(包含所有特性)
    
    /// <summary>
    /// 终极复杂配置 - 包含所有可能的特性组合
    /// </summary>
    [XmlDefined("UltimateComplex")]
    public class UltimateComplexConfig : ComplexItemConfig, IXConfig<UltimateComplexConfig, UltimateComplexConfigUnmanaged>
    {
        #region 更多基本字段
        
        /// <summary>版本号</summary>
        [XmlNotNull]
        [XmlIndex("VersionIndex", false, 0)]
        public int Version;
        
        /// <summary>是否启用</summary>
        [XmlDefault("true")]
        public bool IsEnabled;
        
        /// <summary>权重(可空)</summary>
        public float? Weight;
        
        #endregion
        
        #region 更多容器字段
        
        /// <summary>整数集合</summary>
        [XmlDefault("10,20,30")]
        public HashSet<int> IntSet;
        
        /// <summary>枚举集合</summary>
        public HashSet<EItemType> TypeSet;
        
        /// <summary>可空整数列表</summary>
        public List<int?> NullableIntList;
        
        #endregion
        
        #region 极端嵌套容器
        
        /// <summary>四层嵌套: List&lt;List&lt;Dictionary&lt;string, List&lt;int&gt;&gt;&gt;&gt;</summary>
        public List<List<Dictionary<string, List<int>>>> FourLevelNested;
        
        /// <summary>字典嵌套字典: Dictionary&lt;string, Dictionary&lt;int, float&gt;&gt;</summary>
        public Dictionary<string, Dictionary<int, float>> DictInDict;
        
        /// <summary>列表嵌套字典嵌套列表: List&lt;Dictionary&lt;EItemType, List&lt;AttributeConfig&gt;&gt;&gt;</summary>
        public List<Dictionary<EItemType, List<AttributeConfig>>> ComplexNested;
        
        #endregion
        
        #region 更多Link字段
        
        /// <summary>解锁技能Link(一对一)</summary>
        [XMLLinkAttribute(false)]
        public CfgS<ActiveSkillConfig> UnlockSkill;
        
        /// <summary>被动技能Link(一对一)</summary>
        [XMLLinkAttribute(false)]
        public CfgS<PassiveSkillConfig> PassiveSkill;
        
        #endregion
        
        #region 复杂索引
        
        /// <summary>四字段复合索引</summary>
        [XmlIndex("MegaIndex", false, 0)]
        public EItemType IndexField1;
        
        [XmlIndex("MegaIndex", false, 1)]
        public EItemQuality IndexField2;
        
        [XmlIndex("MegaIndex", false, 2)]
        public int IndexField3;
        
        [XmlIndex("MegaIndex", false, 3)]
        public string IndexField4;
        
        #endregion
        
        #region 自定义解析器字段
        
        /// <summary>自定义解析Value的字典</summary>
        [XmlTypeConverterAttribute(typeof(StringToUpperConverter))]
        [XmlTypeConverterAttribute(typeof(StringToIntConverter))]
        public Dictionary<string, int> CustomDict;
        
        /// <summary>自定义解析Key的字典</summary>
        [XmlTypeConverterAttribute(typeof(StringToUpperConverter))]
        public Dictionary<string, int> CustomKeyDict;
        
        /// <summary>自定义解析元素的列表</summary>
        [XmlTypeConverterAttribute(typeof(StringToUpperConverter))]
        public List<string> CustomList;
        
 
        public List<List<string>> CustomNestedList;
        
        #endregion
    }
    
    public struct UltimateComplexConfigUnmanaged : IConfigUnManaged<UltimateComplexConfigUnmanaged>
    {

    }
    
    #endregion
    
    #region 容器转换器测试配置
    
    /// <summary>
    /// 容器转换器测试配置
    /// 测试容器元素的自定义转换
    /// </summary>
    public class ContainerConverterConfig : IXConfig<ContainerConverterConfig, ContainerConverterConfigUnmanaged>
    {
        /// <summary>自定义转换元素的列表</summary>
        [XmlTypeConverter(typeof(StringToUpperConverter))]
        public List<string> CustomStringList;
        
        /// <summary>自定义转换Value的字典</summary>
        [XmlTypeConverter(typeof(StringToIntConverter))]
        public Dictionary<string, int> CustomValueDict;
        
        /// <summary>自定义转换Key的字典</summary>
        [XmlTypeConverter(typeof(StringToKeyConverter))]
        public Dictionary<string, int> CustomKeyDict;
        
        /// <summary>同时转换Key和Value的字典</summary> 
        [XmlTypeConverter(typeof(StringToKeyConverter))]
        [XmlTypeConverter(typeof(StringToIntConverter))]
        public Dictionary<string, int> CustomBothDict;
        
        /// <summary>嵌套容器的元素转换</summary>
        [XmlTypeConverter(typeof(StringToUpperConverter))]
        public List<List<string>> NestedCustomList;
        
        /// <summary>枚举列表的转换</summary>
        [XmlTypeConverter(typeof(StringToEnumConverter))]
        public List<EItemType> EnumList;
    }
    
    public struct ContainerConverterConfigUnmanaged : IConfigUnManaged<ContainerConverterConfigUnmanaged>
    {
    }
    
    #endregion
    
    #region 自定义转换器和解析器
    
    /// <summary>
    /// 自定义物品数据转换器
    /// </summary>
    public class CustomItemDataConverter
    {
        public static bool Convert(string source, out string target)
        {
            target = $"Custom_{source}";
            return true;
        }
    }
    
    /// <summary>
    /// 全局浮点数转换器
    /// </summary>
    public class GlobalFloatConverter
    {
        public static bool Convert(string source, out float target)
        {
            return float.TryParse(source, out target);
        }
    }
    
    /// <summary>
    /// 字符串转大写转换器(用于容器元素)
    /// </summary>
    public class StringToUpperConverter
    {
        public static bool Convert(string source, out string target)
        {
            target = source?.ToUpper();
            return true;
        }
    }
    
    /// <summary>
    /// 字符串转整数转换器(用于容器Value)
    /// </summary>
    public class StringToIntConverter
    {
        public static bool Convert(string source, out int target)
        {
            return int.TryParse(source, out target);
        }
    }
    
    /// <summary>
    /// 字符串转Key转换器(用于容器Key)
    /// </summary>
    public class StringToKeyConverter
    {
        public static bool Convert(string source, out string target)
        {
            target = $"KEY_{source}";
            return true;
        }
    }
    
    /// <summary>
    /// 字符串转枚举转换器
    /// </summary>
    public class StringToEnumConverter
    {
        public static bool Convert(string source, out EItemType target)
        {
            return Enum.TryParse(source, out target);
        }
    }
    
    #endregion
}
