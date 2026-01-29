using System;
using System.Collections.Generic;
using System.Xml;
using Unity.Collections;
using Unity.Mathematics;
using XM;
using XM.Contracts;
using XM.Contracts.Config;
using XM.Utils;
public partial struct NestedConfigUnManaged : global::XM.IConfigUnManaged<NestedConfigUnManaged>
{
}public partial struct NestedConfigUnManaged
{
    public Int32 RequiredId;
    public StrI OptionalWithDefault;
    public Int32 Test;
    public int2 TestCustom;
    public int2 TestGlobalConvert;
    public XBlobArray<CfgI<TestConfigUnManaged>> TestKeyList;
    public LabelI StrIndex;
    public FixedString32Bytes Str32;
    public FixedString64Bytes Str64;
    public StrI Str;
    public LabelI LabelS;
}public partial struct NestedConfigUnManaged
{
    /// <summary>
    /// 将 TestCustom 从 String 转换为 int2，转换器从 IConfigDataCenter 按域获取。
    /// </summary>
    public static int2 ConvertTestCustom(String source)
    {
        var c = global::XM.Contracts.IConfigDataCenter.I?.GetConverter<String, int2>("");
        return c != null ? c.Convert(source) : default;
    }
}
public partial struct NestedConfigUnManaged
{
    /// <summary>
    /// 将 TestGlobalConvert 从 String 转换为 int2，转换器从 IConfigDataCenter 按域获取。
    /// </summary>
    public static int2 ConvertTestGlobalConvert(String source)
    {
        var c = global::XM.Contracts.IConfigDataCenter.I?.GetConverter<String, int2>("global");
        return c != null ? c.Convert(source) : default;
    }
}

