using System;
using System.Collections.Generic;
using System.Xml;
using TestConfigLargenum;
using XM;
using XM.Contracts;
using XM.Contracts.Config;
namespace TestConfigLargenum
{
public partial struct PerfConfig4UnManaged : global::XM.IConfigUnManaged<PerfConfig4UnManaged>
{
}public partial struct PerfConfig4UnManaged
{
    public CfgI<PerfConfig4UnManaged> Id;
    public global::XBlobPtr<PerfConfig4UnManaged> Id_Ref;
    public StrI Name;
    public Int32 Level;
    public XBlobArray<Int32> Tags;
}
}
