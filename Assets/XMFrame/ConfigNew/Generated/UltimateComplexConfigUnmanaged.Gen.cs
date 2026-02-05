using System;
using Unity.Collections;
using XM;
using XM.ConfigNew.CodeGen;
using XM.ConfigNew.Tests.Data;
using XM.Contracts.Config;

namespace XM.ConfigNew.Tests.Data
{
    /// <summary>
    /// UltimateComplexConfig 的非托管数据结构 (代码生成)
    /// </summary>
    public partial struct UltimateComplexConfigUnmanaged : IConfigUnManaged<global::XM.ConfigNew.Tests.Data.UltimateComplexConfigUnmanaged>
    {
        // 字段

        /// <summary>索引: VersionIndex</summary>
        public int Version;
        public bool IsEnabled;
        /// <summary>可空</summary>
        public float Weight;
        /// <summary>容器</summary>
        public global::XBlobSet<int> IntSet;
        /// <summary>容器</summary>
        public global::XBlobSet<global::XM.ConfigNew.CodeGen.EnumWrapper<global::XM.ConfigNew.Tests.Data.EItemType>> TypeSet;
        /// <summary>容器</summary>
        public global::XBlobArray<int> NullableIntList;
        /// <summary>容器</summary>
        public global::XBlobArray<global::XBlobArray<global::XBlobMap<StrI, global::XBlobArray<int>>>> FourLevelNested;
        /// <summary>容器</summary>
        public global::XBlobMap<StrI, global::XBlobMap<int, float>> DictInDict;
        /// <summary>容器, 嵌套配置</summary>
        public global::XBlobArray<global::XBlobMap<global::XM.ConfigNew.CodeGen.EnumWrapper<global::XM.ConfigNew.Tests.Data.EItemType>, global::XBlobArray<global::XM.ConfigNew.Tests.Data.AttributeConfigUnmanaged>>> ComplexNested;
        /// <summary>Link</summary>
        public CfgI<global::XM.ConfigNew.Tests.Data.ActiveSkillConfigUnmanaged> UnlockSkill;
        /// <summary>Link父节点指针 (指向ActiveSkillConfig)</summary>
        public global::XBlobPtr<global::XM.ConfigNew.Tests.Data.ActiveSkillConfigUnmanaged> UnlockSkill_ParentPtr;
        /// <summary>Link父节点索引 (指向ActiveSkillConfig)</summary>
        public CfgI<global::XM.ConfigNew.Tests.Data.ActiveSkillConfigUnmanaged> UnlockSkill_ParentIndex;
        /// <summary>Link</summary>
        public CfgI<global::XM.ConfigNew.Tests.Data.PassiveSkillConfigUnmanaged> PassiveSkill;
        /// <summary>Link父节点指针 (指向PassiveSkillConfig)</summary>
        public global::XBlobPtr<global::XM.ConfigNew.Tests.Data.PassiveSkillConfigUnmanaged> PassiveSkill_ParentPtr;
        /// <summary>Link父节点索引 (指向PassiveSkillConfig)</summary>
        public CfgI<global::XM.ConfigNew.Tests.Data.PassiveSkillConfigUnmanaged> PassiveSkill_ParentIndex;
        /// <summary>枚举, 索引: MegaIndex</summary>
        public global::XM.ConfigNew.Tests.Data.EItemType IndexField1;
        /// <summary>枚举, 索引: MegaIndex</summary>
        public global::XM.ConfigNew.Tests.Data.EItemQuality IndexField2;
        /// <summary>索引: MegaIndex</summary>
        public int IndexField3;
        /// <summary>索引: MegaIndex</summary>
        public global::Unity.Collections.FixedString32Bytes IndexField4;
        /// <summary>容器</summary>
        public global::XBlobMap<StrI, int> CustomDict;
        /// <summary>容器</summary>
        public global::XBlobMap<StrI, int> CustomKeyDict;
        /// <summary>容器</summary>
        public global::XBlobArray<StrI> CustomList;
        /// <summary>容器</summary>
        public global::XBlobArray<global::XBlobArray<StrI>> CustomNestedList;
        /// <summary>枚举, 索引: TypeIndex</summary>
        public global::XM.ConfigNew.Tests.Data.EItemType ItemType;
        /// <summary>可空, 索引: LevelIndex</summary>
        public int RequiredLevel;
        /// <summary>嵌套配置</summary>
        public global::XM.ConfigNew.Tests.Data.AttributeConfigUnmanaged Price;
        /// <summary>容器</summary>
        public global::XBlobArray<StrI> Tags;
        /// <summary>容器, 嵌套配置</summary>
        public global::XBlobArray<global::XM.ConfigNew.Tests.Data.AttributeConfigUnmanaged> Attributes;
        /// <summary>容器, 嵌套配置</summary>
        public global::XBlobArray<global::XM.ConfigNew.Tests.Data.EffectConfigUnmanaged> Effects;
        /// <summary>容器</summary>
        public global::XBlobArray<int> IntValues;
        /// <summary>容器</summary>
        public global::XBlobArray<global::XM.ConfigNew.CodeGen.EnumWrapper<global::XM.ConfigNew.Tests.Data.EAttributeType>> AttributeTypes;
        /// <summary>容器</summary>
        public global::XBlobArray<global::XBlobArray<int>> Matrix;
        /// <summary>容器</summary>
        public global::XBlobMap<StrI, int> StringIntMap;
        /// <summary>容器</summary>
        public global::XBlobMap<global::XM.ConfigNew.CodeGen.EnumWrapper<global::XM.ConfigNew.Tests.Data.EAttributeType>, global::XBlobArray<int>> AttributeValueMap;
        /// <summary>容器</summary>
        public global::XBlobArray<global::XBlobMap<StrI, global::XBlobArray<float>>> DeepNestedContainer;
        /// <summary>容器</summary>
        public global::XBlobSet<int> UniqueIds;
        public global::Unity.Collections.FixedString32Bytes CustomData;
        public float GlobalValue;
        /// <summary>容器</summary>
        public global::XBlobArray<StrI> ComplexList;
        /// <summary>索引: CategoryIndex, FullIndex</summary>
        public global::Unity.Collections.FixedString32Bytes Category;
        /// <summary>索引: FullIndex</summary>
        public int SubType;
        /// <summary>索引: FullIndex</summary>
        public int Level;
        public global::Unity.Collections.FixedString32Bytes ShortName;
        public global::XM.StrI LocalizedName;
        public global::XM.LabelI LabelName;

        /// <summary>
        /// ToString方法
        /// </summary>
        /// <param name="dataContainer">数据容器</param>
        public string ToString(object dataContainer)
        {
            return "UltimateComplexConfig";
        }
    }
}
