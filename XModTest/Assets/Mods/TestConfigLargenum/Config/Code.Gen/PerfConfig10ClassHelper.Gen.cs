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
/// PerfConfig10 的配置加载辅助类，用于从 XML 反序列化（静态代码生成，无反射）。
/// </summary>
public sealed class PerfConfig10ClassHelper : ConfigClassHelper<PerfConfig10, PerfConfig10UnManaged>
{
    public static TblI TblI { get; private set; }
    public static TblS TblS { get; private set; }

    static PerfConfig10ClassHelper()
    {
        const string __tableName = "PerfConfig10";
        CfgS<PerfConfig10UnManaged>.TableName = __tableName;
        try
        {
            var __cfgSType = typeof(CfgS<>).MakeGenericType(typeof(PerfConfig10UnManaged));
            var __prop = __cfgSType.GetProperty("TableName", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            __prop?.SetValue(null, __tableName);
        }
        catch { }
        const string __modName = "TestConfigLargenum";
        TblS = new TblS(new ModS(__modName), __tableName);
    }

    public PerfConfig10ClassHelper(IConfigDataCenter dataCenter)
        : base(dataCenter)
    {
    }

    public override TblS GetTblS()
    {
        return TblS;
    }

    /// <summary>由 TblI 分配时一并确定，无需单独字段。</summary>
    public static ModI DefinedInMod => TblI.Mod;

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
        var config = (PerfConfig10)Create();
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
        var config = (PerfConfig10)target;
        config.Id = ParseId(configItem, mod, configName);
        config.Name = ParseName(configItem, mod, configName);
        config.Level = ParseLevel(configItem, mod, configName);
        config.Tags = ParseTags(configItem, mod, configName);
    }

    #region 字段解析 (ParseXXX)

    private static CfgS<PerfConfig10UnManaged> ParseId(XmlElement configItem, ModS mod, string configName)
        {
            try
            {
                var s = ConfigClassHelper.GetXmlFieldValue(configItem, "Id");
            if (string.IsNullOrEmpty(s)) return default;
            if (!ConfigClassHelper.TryParseCfgSString(s, "Id", out var modName, out var cfgName)) return default;
            return new CfgS<PerfConfig10UnManaged>(new ModS(modName), cfgName);
            }
            catch (Exception ex)
            {
                if (ConfigClassHelper.IsStrictMode) ConfigClassHelper.LogParseError(ConfigClassHelper.CurrentParseContext.FilePath, ConfigClassHelper.CurrentParseContext.Line, "Id", ex); else ConfigClassHelper.LogParseWarning("Id", ConfigClassHelper.GetXmlFieldValue(configItem, "Id"), ex);
                return default;
            }
        }

    private static string ParseName(XmlElement configItem, ModS mod, string configName)
        {
            var s = ConfigClassHelper.GetXmlFieldValue(configItem, "Name");
            return s ?? "";
        }

    private static int ParseLevel(XmlElement configItem, ModS mod, string configName)
        {
            var s = ConfigClassHelper.GetXmlFieldValue(configItem, "Level");
            if (string.IsNullOrEmpty(s)) return default;
            return ConfigClassHelper.TryParseInt(s, "Level", out var v) ? v : default;
        }

    private static List<int> ParseTags(XmlElement configItem, ModS mod, string configName)
        {
            try
            {
                var list = new List<int>();
            var nodes = configItem.SelectNodes("Tags");
            if (nodes != null)
            foreach (System.Xml.XmlNode n in nodes) { var t = (n as System.Xml.XmlElement)?.InnerText?.Trim(); if (!string.IsNullOrEmpty(t) && ConfigClassHelper.TryParseInt(t, "Tags", out var vi)) list.Add(vi); }
            if (list.Count == 0) { var csv = ConfigClassHelper.GetXmlFieldValue(configItem, "Tags"); if (!string.IsNullOrEmpty(csv)) foreach (var p in csv.Split(',', ';')) if (!string.IsNullOrWhiteSpace(p) && ConfigClassHelper.TryParseInt(p.Trim(), "Tags", out var vi)) list.Add(vi); }
            return list;
            }
            catch (Exception ex)
            {
                if (ConfigClassHelper.IsStrictMode) ConfigClassHelper.LogParseError(ConfigClassHelper.CurrentParseContext.FilePath, ConfigClassHelper.CurrentParseContext.Line, "Tags", ex); else ConfigClassHelper.LogParseWarning("Tags", null, ex);
                return new List<int>();
            }
        }

    #endregion
}

}
