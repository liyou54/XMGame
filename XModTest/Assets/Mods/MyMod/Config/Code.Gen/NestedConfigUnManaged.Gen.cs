using System;
using Unity.Collections;
using XM;
using XM.ConfigNew.CodeGen;
using XM.Contracts.Config;

/// <summary>
/// NestedConfig 的非托管数据结构 (代码生成)
/// </summary>
public partial struct NestedConfigUnManaged : IConfigUnManaged<NestedConfigUnManaged>
{
    // 字段

    public int RequiredId;
    public global::Unity.Collections.FixedString32Bytes OptionalWithDefault;
    public int Test;
    public global::Unity.Mathematics.int2 TestCustom;
    public global::Unity.Mathematics.int2 TestGlobalConvert;
    /// <summary>容器</summary>
    public global::XBlobArray<CfgI<TestConfigUnmanaged>> TestKeyList;
    public global::XM.LabelI StrIndex;
    public global::Unity.Collections.FixedString32Bytes Str32;
    public global::Unity.Collections.FixedString64Bytes Str64;
    public global::XM.StrI Str;
    public global::XM.LabelI LabelS;

    /// <summary>
    /// ToString方法
    /// </summary>
    /// <param name="dataContainer">数据容器</param>
    public string ToString(object dataContainer)
    {
        return "NestedConfig";
    }
}
