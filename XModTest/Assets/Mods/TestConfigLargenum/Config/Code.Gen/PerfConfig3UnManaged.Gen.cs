using System;
using System.Collections.Generic;
using System.Xml;
using TestConfigLargenum;
using XM;
using XM.Contracts;
using XM.Contracts.Config;
namespace TestConfigLargenum
{
public partial struct PerfConfig3UnManaged : global::XM.IConfigUnManaged<PerfConfig3UnManaged>
{
}public partial struct PerfConfig3UnManaged
{
    public CfgI<PerfConfig3UnManaged> Id;
    public global::XBlobPtr<PerfConfig3UnManaged> Id_Ref;
    public StrI Name;
    public Int32 Level;
    public XBlobArray<Int32> Tags;
}
}
