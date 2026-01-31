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
    public static TblI TblI { get; private set; }
    public static TblS TblS { get; private set; }

    static TestInhertClassHelper()
    {
        const string __tableName = "TestInhert";
        CfgS<TestInhertUnmanaged>.TableName = __tableName;
        TblS = new TblS(new ModS("Default"), __tableName);
    }

    public TestInhertClassHelper(IConfigDataCenter dataCenter)
        : base(dataCenter)
    {
    }

    public static ModI DefinedInMod => TblI.Mod;

    public override TblS GetTblS()
    {
        return TblS;
    }

    public override void SetTblIDefinedInMod(TblI c)
    {
        TblI = c;
    }

    public override IXConfig DeserializeConfigFromXml(XmlElement configItem, ModS mod, string configName)
    {
        return DeserializeConfigFromXml(configItem, mod, configName, OverrideMode.None);
    }

    public override IXConfig DeserializeConfigFromXml(XmlElement configItem, ModS mod, string configName, OverrideMode overrideMode)
    {
        var config = (TestInhert)Create();
        try
        {
            FillFromXml(config, configItem, mod, configName);
        }
        catch (Exception ex)
        {
            if (overrideMode == OverrideMode.None || overrideMode == OverrideMode.ReWrite)
                ConfigClassHelper.LogParseError(ConfigClassHelper.CurrentParseContext.FilePath, ConfigClassHelper.CurrentParseContext.Line, "(整体)", ex);
            else
                ConfigClassHelper.LogParseWarning("(整体)", configName, ex);
        }
        return config;
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

}

}
