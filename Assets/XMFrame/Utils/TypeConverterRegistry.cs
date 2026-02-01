using System;
using System.Collections.Generic;
using XM.Contracts;
using XM;

namespace XM.Utils
{
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
        /// 仅按类型获取转换器：先查全局，再按任意域查找 (TSource, TTarget)，返回第一个匹配。供生成代码直接通过类型获取正确转换器，无需传 domain。
        /// </summary>
        public static ITypeConverter<TSource, TTarget> GetConverterByType<TSource, TTarget>()
        {
            var src = typeof(TSource);
            var tgt = typeof(TTarget);
            if (_globalConverters.TryGetValue(src, out var globalTargetDict) && globalTargetDict.TryGetValue(tgt, out var globalConverter))
                return (ITypeConverter<TSource, TTarget>)globalConverter;
            foreach (var domainConverters in _localConverters.Values)
            {
                if (domainConverters.TryGetValue(src, out var targetDict) && targetDict.TryGetValue(tgt, out var converter))
                    return (ITypeConverter<TSource, TTarget>)converter;
            }
            return null;
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
