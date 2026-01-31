using System;
using System.Collections.Generic;
using System.Xml;
using TestConfigLargenum;
using XM;
using XM.Contracts;
using XM.Contracts.Config;
namespace TestConfigLargenum
{
public partial struct PerfConfig7UnManaged : global::XM.IConfigUnManaged<PerfConfig7UnManaged>
{
}public partial struct PerfConfig7UnManaged
{
    public CfgI<PerfConfig7UnManaged> Id;
    public global::XBlobPtr<PerfConfig7UnManaged> Id_Ref;
    public StrI Name;
    public Int32 Level;
    public XBlobArray<Int32> Tags;
}
}
