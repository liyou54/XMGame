using System;
using System.Collections.Generic;
using System.Xml;
using XM;
using XM.Contracts;
using XM.Contracts.Config;
namespace XM.Editor.Gen
{
public partial struct TestInhertUnmanaged : global::XM.IConfigUnManaged<TestInhertUnmanaged>
{
}public partial struct TestInhertUnmanaged
{
    public CfgI<TestConfigUnManaged> Link_Dst;
    public CfgI<TestConfigUnManaged> Link_Ref;
    public CfgI<TestInhertUnmanaged> Link;
    public Int32 xxxx;
}
}
