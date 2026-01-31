using System;
using System.Collections.Generic;
using System.Xml;
using TestConfigLargenum;
using XM;
using XM.Contracts;
using XM.Contracts.Config;
namespace TestConfigLargenum
{
public partial struct PerfConfig5UnManaged : global::XM.IConfigUnManaged<PerfConfig5UnManaged>
{
}public partial struct PerfConfig5UnManaged
{
    public CfgI<PerfConfig5UnManaged> Id;
    public global::XBlobPtr<PerfConfig5UnManaged> Id_Ref;
    public StrI Name;
    public Int32 Level;
    public XBlobArray<Int32> Tags;
}
}
