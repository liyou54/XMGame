using System;
using System.Collections.Generic;
using System.Xml;
using TestConfigLargenum;
using XM;
using XM.Contracts;
using XM.Contracts.Config;
namespace TestConfigLargenum
{
public partial struct PerfConfig6UnManaged : global::XM.IConfigUnManaged<PerfConfig6UnManaged>
{
}public partial struct PerfConfig6UnManaged
{
    public CfgI<PerfConfig6UnManaged> Id;
    public global::XBlobPtr<PerfConfig6UnManaged> Id_Ref;
    public StrI Name;
    public Int32 Level;
    public XBlobArray<Int32> Tags;
}
}
