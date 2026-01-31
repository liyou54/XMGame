using System;
using System.Collections.Generic;
using System.Xml;
using TestConfigLargenum;
using XM;
using XM.Contracts;
using XM.Contracts.Config;
namespace TestConfigLargenum
{
public partial struct PerfConfig2UnManaged : global::XM.IConfigUnManaged<PerfConfig2UnManaged>
{
}public partial struct PerfConfig2UnManaged
{
    public CfgI<PerfConfig2UnManaged> Id;
    public global::XBlobPtr<PerfConfig2UnManaged> Id_Ref;
    public StrI Name;
    public Int32 Level;
    public XBlobArray<Int32> Tags;
}
}
