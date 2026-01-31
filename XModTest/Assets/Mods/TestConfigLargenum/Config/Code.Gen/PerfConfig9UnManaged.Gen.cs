using System;
using System.Collections.Generic;
using System.Xml;
using TestConfigLargenum;
using XM;
using XM.Contracts;
using XM.Contracts.Config;
namespace TestConfigLargenum
{
public partial struct PerfConfig9UnManaged : global::XM.IConfigUnManaged<PerfConfig9UnManaged>
{
}public partial struct PerfConfig9UnManaged
{
    public CfgI<PerfConfig9UnManaged> Id;
    public global::XBlobPtr<PerfConfig9UnManaged> Id_Ref;
    public StrI Name;
    public Int32 Level;
    public XBlobArray<Int32> Tags;
}
}
