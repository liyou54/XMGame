using System;
using System.Collections.Generic;
using System.Xml;
using TestConfigLargenum;
using XM;
using XM.Contracts;
using XM.Contracts.Config;
namespace TestConfigLargenum
{
public partial struct PerfConfig8UnManaged : global::XM.IConfigUnManaged<PerfConfig8UnManaged>
{
}public partial struct PerfConfig8UnManaged
{
    public CfgI<PerfConfig8UnManaged> Id;
    public global::XBlobPtr<PerfConfig8UnManaged> Id_Ref;
    public StrI Name;
    public Int32 Level;
    public XBlobArray<Int32> Tags;
}
}
