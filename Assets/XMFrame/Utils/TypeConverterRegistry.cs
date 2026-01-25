using System;
using System.Collections.Generic;
using XMFrame.Interfaces;
using XMFrame.Implementation;

namespace XMFrame.Utils
{
    /// <summary>
    /// 类型转换器接口
    /// </summary>
    public interface ITypeConverter<TSource, TTarget>
    {
        /// <summary>
        /// 将源类型转换为目标类型
        /// </summary>
        TTarget Convert(TSource source);
    }

    /// <summary>
    /// 类型转换器注册表，支持全局和局部转换器
    /// </summary>
    public static class TypeConverterRegistry
    {
        // 全局转换器字典：SourceType -> TargetType -> Converter
        private static readonly Dictionary<Type, Dictionary<Type, object>> _globalConverters = 
            new Dictionary<Type, Dictionary<Type, object>>();

        // 局部转换器字典：Domain -> SourceType -> TargetType -> Converter
        private static readonly Dictionary<string, Dictionary<Type, Dictionary<Type, object>>> _localConverters = 
            new Dictionary<string, Dictionary<Type, Dictionary<Type, object>>>();

        /// <summary>
        /// 注册全局转换器
        /// </summary>
        public static void RegisterGlobalConverter<TSource, TTarget>(ITypeConverter<TSource, TTarget> converter)
        {
            if (!_globalConverters.ContainsKey(typeof(TSource)))
            {
                _globalConverters[typeof(TSource)] = new Dictionary<Type, object>();
            }
            _globalConverters[typeof(TSource)][typeof(TTarget)] = converter;
        }

        /// <summary>
        /// 注册局部转换器（指定域）
        /// </summary>
        public static void RegisterLocalConverter<TSource, TTarget>(string domain, ITypeConverter<TSource, TTarget> converter)
        {
            if (string.IsNullOrEmpty(domain))
            {
                RegisterGlobalConverter(converter);
                return;
            }

            if (!_localConverters.ContainsKey(domain))
            {
                _localConverters[domain] = new Dictionary<Type, Dictionary<Type, object>>();
            }

            var domainConverters = _localConverters[domain];
            if (!domainConverters.ContainsKey(typeof(TSource)))
            {
                domainConverters[typeof(TSource)] = new Dictionary<Type, object>();
            }
            domainConverters[typeof(TSource)][typeof(TTarget)] = converter;
        }

        /// <summary>
        /// 获取转换器（优先查找局部转换器，然后查找全局转换器）
        /// </summary>
        public static ITypeConverter<TSource, TTarget> GetConverter<TSource, TTarget>(string domain = "")
        {
            // 先查找局部转换器
            if (!string.IsNullOrEmpty(domain) && _localConverters.TryGetValue(domain, out var domainConverters))
            {
                if (domainConverters.TryGetValue(typeof(TSource), out var targetDict))
                {
                    if (targetDict.TryGetValue(typeof(TTarget), out var converter))
                    {
                        return (ITypeConverter<TSource, TTarget>)converter;
                    }
                }
            }

            // 查找全局转换器
            if (_globalConverters.TryGetValue(typeof(TSource), out var globalTargetDict))
            {
                if (globalTargetDict.TryGetValue(typeof(TTarget), out var globalConverter))
                {
                    return (ITypeConverter<TSource, TTarget>)globalConverter;
                }
            }

            return null;
        }

        /// <summary>
        /// 执行转换
        /// </summary>
        public static TTarget Convert<TSource, TTarget>(TSource source, string domain = "")
        {
            var converter = GetConverter<TSource, TTarget>(domain);
            if (converter == null)
            {
                throw new InvalidOperationException($"未找到从 {typeof(TSource).Name} 到 {typeof(TTarget).Name} 的转换器（域: {domain ?? "全局"}）");
            }
            return converter.Convert(source);
        }

        /// <summary>
        /// 检查是否存在转换器
        /// </summary>
        public static bool HasConverter<TSource, TTarget>(string domain = "")
        {
            return GetConverter<TSource, TTarget>(domain) != null;
        }

        /// <summary>
        /// 清除所有转换器
        /// </summary>
        public static void Clear()
        {
            _globalConverters.Clear();
            _localConverters.Clear();
        }

        /// <summary>
        /// 清除指定域的转换器
        /// </summary>
        public static void ClearDomain(string domain)
        {
            if (_localConverters.ContainsKey(domain))
            {
                _localConverters.Remove(domain);
            }
        }
    }

 
}
