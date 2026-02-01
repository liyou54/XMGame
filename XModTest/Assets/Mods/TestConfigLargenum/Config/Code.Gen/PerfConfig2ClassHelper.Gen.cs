using System;
using System.Collections.Generic;
using System.Xml;
using TestConfigLargenum;
using XM;
using XM.Contracts;
using XM.Contracts.Config;

namespace TestConfigLargenum
{

/// <summary>
/// PerfConfig2 的配置加载辅助类，用于从 XML 反序列化（静态代码生成，无反射）。
/// </summary>
public sealed class PerfConfig2ClassHelper : ConfigClassHelper<PerfConfig2, PerfConfig2UnManaged>
{
    public static TblI TblI { get; private set; }
    public static TblS TblS { get; private set; }

    static PerfConfig2ClassHelper()
    {
        const string __tableName = "PerfConfig2";
        const string __modName = "TestConfigLargenum";
        CfgS<PerfConfig2UnManaged>.Table = new TblS(new ModS(__modName), __tableName);
        TblS = new TblS(new ModS(__modName), __tableName);
    }

    public PerfConfig2ClassHelper(IConfigDataCenter dataCenter)
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
        var config = (PerfConfig2)target;
        config.Id = ParseId(configItem, mod, configName, context);
        config.Name = ParseName(configItem, mod, configName, context);
        config.Level = ParseLevel(configItem, mod, configName, context);
        config.Tags = ParseTags(configItem, mod, configName, context);
    }

    #region 字段解析 (ParseXXX)

    private static CfgS<PerfConfig2UnManaged> ParseId(XmlElement configItem, ModS mod, string configName, in ConfigParseContext context)
        {
            try
            {
                var s = ConfigParseHelper.GetXmlFieldValue(configItem, "Id");
            if (string.IsNullOrEmpty(s)) return default;
            if (!ConfigParseHelper.TryParseCfgSString(s, "Id", out var modName, out var cfgName)) return default;
            return new CfgS<PerfConfig2UnManaged>(new ModS(modName), cfgName);
            }
            catch (Exception ex)
            {
                if (ConfigParseHelper.IsStrictMode(context)) ConfigParseHelper.LogParseError(context, "Id", ex); else ConfigParseHelper.LogParseWarning("Id", ConfigParseHelper.GetXmlFieldValue(configItem, "Id"), ex);
                return default;
            }
        }

    private static string ParseName(XmlElement configItem, ModS mod, string configName, in ConfigParseContext context)
        {
            var s = ConfigParseHelper.GetXmlFieldValue(configItem, "Name");
            return s ?? "";
        }

    private static int ParseLevel(XmlElement configItem, ModS mod, string configName, in ConfigParseContext context)
        {
            var s = ConfigParseHelper.GetXmlFieldValue(configItem, "Level");
            if (string.IsNullOrEmpty(s)) return default;
            return ConfigParseHelper.TryParseInt(s, "Level", out var v) ? v : default;
        }

    private static List<int> ParseTags(XmlElement configItem, ModS mod, string configName, in ConfigParseContext context)
        {
            try
            {
                var list = new List<int>();
            var nodes = configItem.SelectNodes("Tags");
            if (nodes != null)
            foreach (System.Xml.XmlNode n in nodes) { var t = (n as System.Xml.XmlElement)?.InnerText?.Trim(); if (!string.IsNullOrEmpty(t) && ConfigParseHelper.TryParseInt(t, "Tags", out var vi)) list.Add(vi); }
            if (list.Count == 0) { var csv = ConfigParseHelper.GetXmlFieldValue(configItem, "Tags"); if (!string.IsNullOrEmpty(csv)) foreach (var p in csv.Split(',', ';')) if (!string.IsNullOrWhiteSpace(p) && ConfigParseHelper.TryParseInt(p.Trim(), "Tags", out var vi)) list.Add(vi); }
            return list;
            }
            catch (Exception ex)
            {
                if (ConfigParseHelper.IsStrictMode(context)) ConfigParseHelper.LogParseError(context, "Tags", ex); else ConfigParseHelper.LogParseWarning("Tags", null, ex);
                return new List<int>();
            }
        }

    #endregion
}

}
