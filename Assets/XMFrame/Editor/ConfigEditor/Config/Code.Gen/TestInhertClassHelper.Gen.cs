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
        const string __modName = "Default";
        CfgS<TestInhertUnmanaged>.Table = new TblS(new ModS(__modName), __tableName);
        TblS = new TblS(new ModS(__modName), __tableName);
        LinkHelperType = typeof(TestConfigClassHelper);
    }

    public TestInhertClassHelper(IConfigDataCenter dataCenter)
        : base(dataCenter)
    {
    }

    public override TblS GetTblS()
    {
        return TblS;
    }

    /// <summary>由 TblI 分配时一并确定，无需单独字段。</summary>
    public static ModI DefinedInMod => TblI.DefinedMod;

    public override void SetTblIDefinedInMod(TblI c)
    {
        TblI = c;
    }

    public override IXConfig DeserializeConfigFromXml(XmlElement configItem, ModS mod, string configName)
    {
        return DeserializeConfigFromXml(configItem, mod, configName, default);
    }

    public override void ParseAndFillFromXml(IXConfig target, XmlElement configItem, ModS mod, string configName)
    {
        ParseAndFillFromXml(target, configItem, mod, configName, default);
    }

    public override void ParseAndFillFromXml(IXConfig target, XmlElement configItem, ModS mod, string configName, in ConfigParseContext context)
    {
        var config = (TestInhert)target;
        config.Link = ParseLink(configItem, mod, configName, context);
        config.xxxx = Parsexxxx(configItem, mod, configName, context);
    }

    #region 字段解析 (ParseXXX)

    private static CfgS<TestConfig> ParseLink(XmlElement configItem, ModS mod, string configName, in ConfigParseContext context)
        {
            try
            {
                var s = ConfigParseHelper.GetXmlFieldValue(configItem, "Link");
            if (string.IsNullOrEmpty(s)) return default;
            if (!ConfigParseHelper.TryParseCfgSString(s, "Link", out var modName, out var cfgName)) return default;
            return new CfgS<TestConfig>(new ModS(modName), cfgName);
            }
            catch (Exception ex)
            {
                if (ConfigParseHelper.IsStrictMode(context)) ConfigParseHelper.LogParseError(context, "Link", ex); else ConfigParseHelper.LogParseWarning("Link", ConfigParseHelper.GetXmlFieldValue(configItem, "Link"), ex);
                return default;
            }
        }

    private static int Parsexxxx(XmlElement configItem, ModS mod, string configName, in ConfigParseContext context)
        {
            var s = ConfigParseHelper.GetXmlFieldValue(configItem, "xxxx");
            if (string.IsNullOrEmpty(s)) return default;
            return ConfigParseHelper.TryParseInt(s, "xxxx", out var v) ? v : default;
        }

    #endregion
}

}
