using System;
using System.Collections.Generic;
using System.Xml;
using XM;
using XM.Contracts;
using XM.Contracts.Config;

namespace XM.Editor.Gen
{

/// <summary>
/// TestInhert 的配置加载辅助类，用于从 XML 反序列化（静态代码生成，无反射）。
/// </summary>
public sealed class TestInhertClassHelper : ConfigClassHelper<TestInhert, TestInhertUnmanaged>
{
    static TestInhertClassHelper()
    {
        CfgS<TestInhertUnmanaged>.TableName = "TestInhert";
    }

    public TestInhertClassHelper(IConfigDataCenter dataCenter)
        : base(dataCenter)
    {
    }

    public override TblS GetTblS()
    {
        return new TblS(new ModS("Default"), "TestInhert");
    }

    public override void SetTblIDefinedInMod(ModI modHandle)
    {
        _definedInMod = modHandle;
    }

    public override IXConfig DeserializeConfigFromXml(XmlElement configItem, ModS mod, string configName)
    {
        try
        {
            var config = (TestInhert)Create();
            FillFromXml(config, configItem, mod, configName);
            return config;
        }
        catch (Exception ex)
        {
            ConfigClassHelper.LogParseWarning("(整体)", configName, ex);
            throw;
        }
    }

    public override void FillFromXml(IXConfig target, XmlElement configItem, ModS mod, string configName)
    {
        var config = (TestInhert)target;
        var baseHelper = ConfigDataCenter.GetClassHelper(typeof(TestConfig));
        if (baseHelper != null) baseHelper.FillFromXml(target, configItem, mod, configName);
        config.xxxx = Parsexxxx(configItem, mod, configName);
    }

    #region 字段解析 (ParseXXX)

    private static int Parsexxxx(XmlElement configItem, ModS mod, string configName)
        {
            var s = ConfigClassHelper.GetXmlFieldValue(configItem, "xxxx");
            if (string.IsNullOrEmpty(s)) return default;
            return ConfigClassHelper.TryParseInt(s, "xxxx", out var v) ? v : default;
        }

    #endregion

    private ModI _definedInMod;
}

}
