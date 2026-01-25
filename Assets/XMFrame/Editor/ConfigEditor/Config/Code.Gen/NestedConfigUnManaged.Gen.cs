using Unity.Mathematics;
using XMFrame;
using Unity.Collections;
using System;
public partial struct NestedConfigUnManaged : IConfigUnManaged<NestedConfigUnManaged>
{
}public partial struct NestedConfigUnManaged
{
    public Int32 Test;
    public int2 TestCustom;
    public int2 TestGlobalConvert;
    public XBlobArray<CfgId<TestConfigUnManaged>> TestKeyList;
    public StrHandle StrIndex;
    public FixedString32Bytes Str32;
    public FixedString64Bytes Str64;
    public StrHandle Str;
    public StrLabelHandle StrLabel;
}
