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
    }

    public TestInhertClassHelper(IConfigDataCenter dataCenter)
        : base(dataCenter)
    {
    }

    public override TblS GetTblS()
    {
        return new TblS(new ModS("Default"), "TestInhert");
    }

    public override void SetTblIDefinedInMod(TblI tbl)
    {
        _definedInMod = tbl;
    }

    public override void ParseAndFillFromXml(IXConfig target, XmlElement configItem, ModS mod, string configName, in ConfigParseContext context)
    {
        var config = (TestInhert)target;
        config.Link = ParseLink(configItem, mod, configName, context);
        config.xxxx = Parsexxxx(configItem, mod, configName, context);
    }

    #region 字段解析 (ParseXXX)

    private static CfgS<TestConfig> ParseLink(XmlElement configItem, ModS mod, string configName,
        in ConfigParseContext context)
    {
        try
        {
            var s = ConfigParseHelper.GetXmlFieldValue(configItem, "Link");
            if (string.IsNullOrEmpty(s)) return default;
            if (!ConfigParseHelper.TryParseCfgSString(s, "Link", out var modName, out var cfgName))
                return default;
            return new CfgS<TestConfig>(new ModS(modName), cfgName);
        }
        catch (Exception ex)
        {
            if (ConfigParseHelper.IsStrictMode(context))
                ConfigParseHelper.LogParseError(context, "Link", ex);
            else
                ConfigParseHelper.LogParseWarning("Link",
                    ConfigParseHelper.GetXmlFieldValue(configItem, "Link"), ex);
            return default;
        }
    }

    private static int Parsexxxx(XmlElement configItem, ModS mod, string configName,
        in ConfigParseContext context)
    {
        var s = ConfigParseHelper.GetXmlFieldValue(configItem, "xxxx");
        if (string.IsNullOrEmpty(s)) return default;
        return ConfigParseHelper.TryParseInt(s, "xxxx", out var v) ? v : default;
    }

    #endregion

    protected override void AllocContainerWithoutFillImpl(
        IXConfig value,
        TblI tbli,
        CfgI cfgi,
        System.Collections.Concurrent.ConcurrentDictionary<TblS, System.Collections.Concurrent.ConcurrentDictionary<CfgS, IXConfig>> allData,
        XM.ConfigDataCenter.ConfigDataHolder configHolderData)
    {
        // 无容器字段需要分配
    }


    public override void FillBasicDataImpl(XM.ConfigDataCenter.ConfigDataHolder configHolderData, CfgS key, IXConfig value, XBlobMap<CfgI, TestInhertUnmanaged> tableMap)
    {
        // TODO: 实现基础数据填充逻辑
    }

    private TblI _definedInMod;
}

}
