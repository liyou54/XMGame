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
    public static List<Type> SubLinkClasses { get; } = new List<Type>();

    /// <summary>字段级自定义转换器</summary>
    protected Dictionary<string, Func<string, object>> _fieldConverters;

    /// <summary>由生成的 *ClassHelper 调用，传入 IConfigDataCenter</summary>
    protected ConfigClassHelper()
    {
    }

    /// <summary>
    /// 注册字段级自定义转换器
    /// </summary>
    /// <param name="fieldName">字段名称</param>
    /// <param name="converter">转换函数</param>
    protected void RegisterFieldConverter(string fieldName, Func<string, object> converter)
    {
        _fieldConverters ??= new Dictionary<string, Func<string, object>>();
        _fieldConverters[fieldName] = converter;
    }

    /// <summary>
    /// 尝试使用自定义转换器转换字段值
    /// </summary>
    /// <typeparam name="TResult">目标类型</typeparam>
    /// <param name="fieldName">字段名称</param>
    /// <param name="value">字符串值</param>
    /// <param name="result">转换结果</param>
    /// <returns>是否转换成功</returns>
    protected bool TryConvertField<TResult>(string fieldName, string value, out TResult result)
    {
        // TODO: 实现字段级转换器查找和调用
        // 1. 检查是否有注册的转换器
        // 2. 调用转换器并验证结果类型
        result = default;
        return false;
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

    /// <summary>
    /// 尝试将字符串转换为 StrI（字符串索引）
    /// </summary>
    /// <param name="value">字符串值</param>
    /// <param name="strI">输出的字符串索引</param>
    /// <returns>是否转换成功</returns>
    protected static bool TryGetStrI(string value, out StrI strI)
    {
        // TODO: 实现字符串到 StrI 的转换逻辑
        // 需要将字符串注册到全局字符串池，获取对应的索引
        strI = default;
        return false;
    }

    /// <summary>
    /// 尝试将字符串转换为 LabelI（标签索引）
    /// </summary>
    /// <param name="value">字符串值</param>
    /// <param name="labelI">输出的标签索引</param>
    /// <returns>是否转换成功</returns>
    protected static bool TryGetLabelI(string value, out LabelI labelI)
    {
        // TODO: 实现字符串到 LabelI 的转换逻辑
        // 需要将标签字符串注册到全局标签池，获取对应的索引
        labelI = default;
        return false;
    }

    /// <summary>
    /// 尝试将字符串转换为 LabelI（标签索引）
    /// </summary>
    /// <param name="value">字符串值</param>
    /// <param name="labelI">输出的标签索引</param>
    /// <returns>是否转换成功</returns>
    protected static bool TryGetLabelI(LabelS value, out LabelI labelI)
    {
        // TODO: 实现字符串到 LabelI 的转换逻辑
        // 需要将标签字符串注册到全局标签池，获取对应的索引
        labelI = default;
        return false;
    }
    
    /// <summary>
    /// 尝试将 CfgS 转换为 CfgI（配置实例索引）
    /// </summary>
    /// <typeparam name="T">配置类型</typeparam>
    /// <param name="cfgS">配置静态引用</param>
    /// <param name="cfgI">输出的配置实例索引</param>
    /// <returns>是否转换成功</returns>
    protected static bool TryGetCfgI<T>(CfgS<T> cfgS, out CfgI cfgI) where T : IXConfig
    {
        // TODO: 链接阶段实现 CfgS 到 CfgI 的解析
        // 需要从配置数据中心查找对应的配置实例
        cfgI = default;
        return false;
    }

    protected static StrI ConvertToStrI(string value)
    {
        if (string.IsNullOrEmpty(value))
            return default;

        // TODO: 实现字符串到 StrI 的转换逻辑
        // 需要将字符串注册到全局字符串池，获取对应的索引
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