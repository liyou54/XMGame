using System;
using System.Collections.Generic;
using System.Xml;
using TestConfigLargenum;
using XM;
using XM.Contracts;
using XM.Contracts.Config;
namespace TestConfigLargenum
{
public partial struct PerfConfig1UnManaged : global::XM.IConfigUnManaged<PerfConfig1UnManaged>
{
}public partial struct PerfConfig1UnManaged
{
    public CfgI<PerfConfig1UnManaged> Id;
    public global::XBlobPtr<PerfConfig1UnManaged> Id_Ref;
    public StrI Name;
    public Int32 Level;
    public XBlobArray<Int32> Tags;
}
}
