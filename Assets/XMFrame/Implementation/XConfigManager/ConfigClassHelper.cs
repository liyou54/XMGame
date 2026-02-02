using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using XM;
using XM.Contracts;
using XM.Contracts.Config;

/// <summary>
/// 配置加载辅助类泛型基接口
/// </summary>
public abstract class ConfigClassHelper<T, TUnmanaged> :
    ConfigClassHelper where T : XM.IXConfig, new()
    where TUnmanaged : unmanaged, IConfigUnManaged<TUnmanaged>
{
    public static Type LinkHelperType { get; protected set; }

    /// <summary>子类 Helper 类型（如果有继承，由代码生成器或反射初始化）</summary>
    public static List<Type> SubClasses { get; } = new List<Type>();

    private readonly IConfigDataCenter _configDataCenter;
    protected IConfigDataCenter ConfigDataCenter => _configDataCenter;


    /// <summary>由生成的 *ClassHelper 调用，传入 IConfigDataCenter</summary>
    protected ConfigClassHelper(IConfigDataCenter dataCenter)
    {
        _configDataCenter = dataCenter ?? throw new ArgumentNullException(nameof(dataCenter));
    }

    public override IXConfig Create()
    {
        return new T();
    }

    public override void AllocUnManagedAndInitHeadVal(
        TblI tbli,
        ConcurrentDictionary<CfgS, IXConfig> kvValue,
        object configHolder)
    {
        if (configHolder is not XM.ConfigDataCenter.ConfigDataHolder configHolderData)
        {
            XLog.Error("ConfigClassHelper.AllocUnManaged: configHolder is not XM.ConfigDataHolder");
            return;
        }

        var map = configHolderData.Data.AllocTableMap<TUnmanaged>(tbli, kvValue.Count);
        if (!map.Vaild)
        {
            return;
        }

        foreach (var kv in kvValue)
        {
            var cfgI = IConfigDataCenter.I.AllocCfgIndex(kv.Key, tbli);
            map[configHolderData.Data.BlobContainer, cfgI] = new TUnmanaged();
        }
    }

    public override void AllocContainerWithFill(
        TblI tbli,
        TblS tblS,
        ConcurrentDictionary<CfgS, IXConfig> kvValue,
        ConcurrentDictionary<TblS, ConcurrentDictionary<CfgS, IXConfig>> allData,
        object configHolder)
    {
        if (configHolder is not XM.ConfigDataCenter.ConfigDataHolder configHolderData)
        {
            XLog.Error("ConfigClassHelper.AllocUnManaged: configHolder is not XM.ConfigDataHolder");
            return;
        }

        foreach (var kv in kvValue)
        {
            if (IConfigDataCenter.I.TryGetCfgI(kv.Key, out var cfgI))
            {
                var val = new TUnmanaged();
                var map = configHolderData.Data.GetMap<CfgI, TUnmanaged>(tbli);
                if (map.Vaild)
                {
                    AllocContainerWithFillImpl(
                        kv.Value,
                        tbli,
                        cfgI,
                        ref val,
                        configHolderData
                    );
                    map[configHolderData.Data.BlobContainer, cfgI] = val;
                    Debug.Log(val.ToString(configHolderData.Data.BlobContainer));
                }
            }
        }
    }

    /// <summary>
    /// 递归填充容器和嵌套配置的 unmanaged 数据。使用 ref 传递 data 参数以避免值拷贝。
    /// 由生成的子类实现，支持嵌套配置的递归调用。
    /// </summary>
    /// <param name="value">托管配置对象</param>
    /// <param name="tbli">表ID</param>
    /// <param name="cfgi">配置ID</param>
    /// <param name="data">unmanaged 数据结构（使用 ref 传递）</param>
    /// <param name="configHolderData">配置数据持有者</param>
    /// <param name="linkParent">链接父节点 </param>
    public abstract void AllocContainerWithFillImpl(
        IXConfig value,
        TblI tbli,
        CfgI cfgi,
        ref TUnmanaged data,
        XM.ConfigDataCenter.ConfigDataHolder configHolderData,
        XBlobPtr? linkParent = null);

    #region 类型转换辅助方法

    protected static StrI ConvertToStrI(string value)
    {
        if (string.IsNullOrEmpty(value))
            return default;

        var converter = XM.Utils.TypeConverterRegistry.GetConverter<string, StrI>();
        if (converter != null && converter.Convert(value, out var result))
        {
            return result;
        }

        return default;
    }

    protected static Unity.Collections.FixedString32Bytes ConvertToFixedString32(string value)
    {
        return string.IsNullOrEmpty(value) ? default : new Unity.Collections.FixedString32Bytes(value);
    }

    protected static Unity.Collections.FixedString64Bytes ConvertToFixedString64(string value)
    {
        return string.IsNullOrEmpty(value) ? default : new Unity.Collections.FixedString64Bytes(value);
    }

    protected static Unity.Collections.FixedString128Bytes ConvertToFixedString128(string value)
    {
        return string.IsNullOrEmpty(value) ? default : new Unity.Collections.FixedString128Bytes(value);
    }

    protected static LabelI ConvertToLabelI(LabelS labelS)
    {
        // TODO: 实现 LabelS 到 LabelI 的转换逻辑
        return default;
    }

    protected static LabelI ConvertToLabelI(string value)
    {
        if (string.IsNullOrEmpty(value))
            return default;

        // 解析字符串格式：ModName::LabelName
        if (ConfigParseHelper.TryParseLabelSString(value, "", out var modName, out var labelName))
        {
            // TODO: 实现从 modName 和 labelName 到 LabelI 的转换逻辑
            // 这需要查找模块ID和标签ID
            return default;
        }

        return default;
    }

    #endregion
}