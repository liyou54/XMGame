using MyMod;
using System;
using System.Collections.Generic;
using System.Xml;
using XM;
using XM.Contracts;
using XM.Contracts.Config;

namespace MyMod
{

/// <summary>
/// MyItemConfig 的配置加载辅助类，用于从 XML 反序列化（静态代码生成，无反射）。
/// </summary>
public sealed class MyItemConfigClassHelper : ConfigClassHelper<MyItemConfig, MyItemConfigUnManaged>
{
    public static TblI TblI { get; private set; }
    public static TblS TblS { get; private set; }

    static MyItemConfigClassHelper()
    {
        const string __tableName = "MyItemConfig";
        const string __modName = "MyMod";
        CfgS<MyItemConfigUnManaged>.Table = new TblS(new ModS(__modName), __tableName);
        TblS = new TblS(new ModS(__modName), __tableName);
    }

    public MyItemConfigClassHelper(IConfigDataCenter dataCenter)
        : base(dataCenter)
    {
    }

    public override TblS GetTblS()
    {
        return TblS;
    }

    public override void SetTblIDefinedInMod(TblI tbl)
    {
        _definedInMod = tbl;
    }

    public override void ParseAndFillFromXml(IXConfig target, XmlElement configItem, ModS mod, string configName, in ConfigParseContext context)
    {
        var config = (MyItemConfig)target;
        config.Id = ParseId(configItem, mod, configName, context);
        config.Name = ParseName(configItem, mod, configName, context);
        config.Level = ParseLevel(configItem, mod, configName, context);
        config.Tags = ParseTags(configItem, mod, configName, context);
    }

    public override Type GetLinkHelperType()
    {
        return null;
    }

    #region 字段解析 (ParseXXX)

    private static CfgS<MyItemConfigUnManaged> ParseId(XmlElement configItem, ModS mod, string configName,
        in ConfigParseContext context)
    {
        try
        {
            var s = ConfigParseHelper.GetXmlFieldValue(configItem, "Id");
            if (string.IsNullOrEmpty(s)) return default;
            if (!ConfigParseHelper.TryParseCfgSString(s, "Id", out var modName, out var cfgName))
                return default;
            return new CfgS<MyItemConfigUnManaged>(new ModS(modName), cfgName);
        }
        catch (Exception ex)
        {
            if (ConfigParseHelper.IsStrictMode(context))
                ConfigParseHelper.LogParseError(context, "Id", ex);
            else
                ConfigParseHelper.LogParseWarning("Id",
                    ConfigParseHelper.GetXmlFieldValue(configItem, "Id"), ex);
            return default;
        }
    }

    private static string ParseName(XmlElement configItem, ModS mod, string configName,
        in ConfigParseContext context)
    {
        var s = ConfigParseHelper.GetXmlFieldValue(configItem, "Name");
        return s ?? "";
    }

    private static int ParseLevel(XmlElement configItem, ModS mod, string configName,
        in ConfigParseContext context)
    {
        var s = ConfigParseHelper.GetXmlFieldValue(configItem, "Level");
        if (string.IsNullOrEmpty(s)) return default;
        return ConfigParseHelper.TryParseInt(s, "Level", out var v) ? v : default;
    }

    private static List<int> ParseTags(XmlElement configItem, ModS mod, string configName,
        in ConfigParseContext context)
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
            if (ConfigParseHelper.IsStrictMode(context))
                ConfigParseHelper.LogParseError(context, "Tags", ex);
            else
                ConfigParseHelper.LogParseWarning("Tags",
                    null, ex);
            return new List<int>();
        }
    }

    #endregion

    public override void AllocContainerWithFillImpl(
        IXConfig value,
        TblI tbli,
        CfgI cfgi,
        ref MyItemConfigUnManaged data,
        XM.ConfigDataCenter.ConfigDataHolder configHolderData,
        XBlobPtr? linkParent = null)
    {
        var config = (MyItemConfig)value;
        AllocTags(config, ref data, cfgi, configHolderData);

        // 填充基本类型和引用类型字段
        if (IConfigDataCenter.I.TryGetCfgI(config.Id.AsNonGeneric(), out var cfgI_Id))
        {
            data.Id = cfgI_Id.As<MyItemConfigUnManaged>();
        }
        data.Name = ConvertToStrI(config.Name);
        data.Level = config.Level;
    }

    #region 容器分配辅助方法

    private void AllocTags(
        MyItemConfig config,
        ref MyItemConfigUnManaged data,
        CfgI cfgi,
        XM.ConfigDataCenter.ConfigDataHolder configHolderData)
    {
        if (config.Tags != null && config.Tags.Count > 0)
        {
            var allocated = configHolderData.Data.BlobContainer.AllocArray<Int32>(config.Tags.Count);
            data.Tags = allocated;

            // 填充数据
            for (int i0 = 0; i0 < config.Tags.Count; i0++)
            {
                allocated[configHolderData.Data.BlobContainer, i0] = config.Tags[i0];
            }
        }
    }

    #endregion

    private TblI _definedInMod;
}

}
