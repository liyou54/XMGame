using System;
using System.Collections.Generic;
using System.Xml;
using TestConfigLargenum;
using XM;
using XM.Contracts;
using XM.Contracts.Config;
namespace TestConfigLargenum
{
public partial struct PerfConfig10UnManaged : global::XM.IConfigUnManaged<PerfConfig10UnManaged>
{
}public partial struct PerfConfig10UnManaged
{
    public CfgI<PerfConfig10UnManaged> Id;
    public global::XBlobPtr<PerfConfig10UnManaged> Id_Ref;
    public StrI Name;
    public Int32 Level;
    public XBlobArray<Int32> Tags;
}
}
