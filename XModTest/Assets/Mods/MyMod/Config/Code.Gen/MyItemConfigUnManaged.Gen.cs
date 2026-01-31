using MyMod;
using System;
using System.Collections.Generic;
using System.Xml;
using XM;
using XM.Contracts;
using XM.Contracts.Config;
namespace MyMod
{
public partial struct MyItemConfigUnManaged : global::XM.IConfigUnManaged<MyItemConfigUnManaged>
{
}public partial struct MyItemConfigUnManaged
{
    public CfgI<MyItemConfigUnManaged> Id;
    public global::XBlobPtr<MyItemConfigUnManaged> Id_Ref;
    public StrI Name;
    public Int32 Level;
    public XBlobArray<Int32> Tags;
}
}
