using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

namespace XM
{
    /// <summary>
    /// 日志系统，支持最多8个泛型参数的格式化输出
    /// 支持多线程StringBuilder复用、日志级别颜色、文件输出
    /// </summary>
    public static class XLog
    {
        // 线程本地StringBuilder，每个线程使用自己的StringBuilder，使用完毕后清空
        private static readonly ThreadLocal<StringBuilder> _threadLocalStringBuilder = 
            new ThreadLocal<StringBuilder>(() => new StringBuilder(256));

        /// <summary>
        /// 当前日志级别，低于此级别的日志将不会被输出
        /// </summary>
        public static LogLevel CurrentLogLevel { get; set; } = LogLevel.Debug;

        /// <summary>
        /// 是否启用日志输出
        /// </summary>
        public static bool Enabled { get; set; } = true;

        /// <summary>
        /// 是否启用颜色输出（Unity控制台支持富文本颜色）
        /// </summary>
        public static bool EnableColor { get; set; } = true;

        /// <summary>
        /// 日志文件输出路径，如果为null或空字符串则不输出到文件
        /// </summary>
        public static string LogFilePath { get; set; } = null;

        /// <summary>
        /// 文件输出流锁，用于多线程安全写入文件
        /// </summary>
        private static readonly object _fileLock = new object();

        #region Debug

        /// <summary>
        /// 输出Debug级别日志
        /// </summary>
        public static void Debug(string message)
        {
            Log(LogLevel.Debug, message);
        }

        /// <summary>
        /// 输出Debug级别日志（支持格式化，0个参数）
        /// </summary>
        public static void DebugFormat(string format)
        {
            LogFormat(LogLevel.Debug, format);
        }

        /// <summary>
        /// 输出Debug级别日志（支持格式化，1个参数）
        /// </summary>
        public static void DebugFormat<T1>(string format, T1 arg1)
        {
            LogFormat(LogLevel.Debug, format, arg1);
        }

        /// <summary>
        /// 输出Debug级别日志（支持格式化，2个参数）
        /// </summary>
        public static void DebugFormat<T1, T2>(string format, T1 arg1, T2 arg2)
        {
            LogFormat(LogLevel.Debug, format, arg1, arg2);
        }

        /// <summary>
        /// 输出Debug级别日志（支持格式化，3个参数）
        /// </summary>
        public static void DebugFormat<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
        {
            LogFormat(LogLevel.Debug, format, arg1, arg2, arg3);
        }

        /// <summary>
        /// 输出Debug级别日志（支持格式化，4个参数）
        /// </summary>
        public static void DebugFormat<T1, T2, T3, T4>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            LogFormat(LogLevel.Debug, format, arg1, arg2, arg3, arg4);
        }

        /// <summary>
        /// 输出Debug级别日志（支持格式化，5个参数）
        /// </summary>
        public static void DebugFormat<T1, T2, T3, T4, T5>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            LogFormat(LogLevel.Debug, format, arg1, arg2, arg3, arg4, arg5);
        }

        /// <summary>
        /// 输出Debug级别日志（支持格式化，6个参数）
        /// </summary>
        public static void DebugFormat<T1, T2, T3, T4, T5, T6>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            LogFormat(LogLevel.Debug, format, arg1, arg2, arg3, arg4, arg5, arg6);
        }

        /// <summary>
        /// 输出Debug级别日志（支持格式化，7个参数）
        /// </summary>
        public static void DebugFormat<T1, T2, T3, T4, T5, T6, T7>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            LogFormat(LogLevel.Debug, format, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        }

        /// <summary>
        /// 输出Debug级别日志（支持格式化，8个参数）
        /// </summary>
        public static void DebugFormat<T1, T2, T3, T4, T5, T6, T7, T8>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            LogFormat(LogLevel.Debug, format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        }

        /// <summary>
        /// 输出Debug级别日志（支持格式化，1个参数，不带Format后缀）
        /// </summary>
        public static void Debug<T1>(string format, T1 arg1)
        {
            LogFormat(LogLevel.Debug, format, arg1);
        }

        /// <summary>
        /// 输出Debug级别日志（支持格式化，2个参数，不带Format后缀）
        /// </summary>
        public static void Debug<T1, T2>(string format, T1 arg1, T2 arg2)
        {
            LogFormat(LogLevel.Debug, format, arg1, arg2);
        }

        /// <summary>
        /// 输出Debug级别日志（支持格式化，3个参数，不带Format后缀）
        /// </summary>
        public static void Debug<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
        {
            LogFormat(LogLevel.Debug, format, arg1, arg2, arg3);
        }

        /// <summary>
        /// 输出Debug级别日志（支持格式化，4个参数，不带Format后缀）
        /// </summary>
        public static void Debug<T1, T2, T3, T4>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            LogFormat(LogLevel.Debug, format, arg1, arg2, arg3, arg4);
        }

        /// <summary>
        /// 输出Debug级别日志（支持格式化，5个参数，不带Format后缀）
        /// </summary>
        public static void Debug<T1, T2, T3, T4, T5>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            LogFormat(LogLevel.Debug, format, arg1, arg2, arg3, arg4, arg5);
        }

        /// <summary>
        /// 输出Debug级别日志（支持格式化，6个参数，不带Format后缀）
        /// </summary>
        public static void Debug<T1, T2, T3, T4, T5, T6>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            LogFormat(LogLevel.Debug, format, arg1, arg2, arg3, arg4, arg5, arg6);
        }

        /// <summary>
        /// 输出Debug级别日志（支持格式化，7个参数，不带Format后缀）
        /// </summary>
        public static void Debug<T1, T2, T3, T4, T5, T6, T7>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            LogFormat(LogLevel.Debug, format, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        }

        /// <summary>
        /// 输出Debug级别日志（支持格式化，8个参数，不带Format后缀）
        /// </summary>
        public static void Debug<T1, T2, T3, T4, T5, T6, T7, T8>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            LogFormat(LogLevel.Debug, format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        }

        /// <summary>
        /// 输出Debug级别日志（支持Join，连接数组或集合）
        /// </summary>
        public static void DebugJoin<T>(string separator, IEnumerable<T> values)
        {
            LogJoin(LogLevel.Debug, separator, values);
        }

        /// <summary>
        /// 输出Debug级别日志（支持Join，连接数组或集合，带前缀消息）
        /// </summary>
        public static void DebugJoin<T>(string message, string separator, IEnumerable<T> values)
        {
            LogJoin(LogLevel.Debug, message, separator, values);
        }

        #endregion

        #region Info

        /// <summary>
        /// 输出Info级别日志
        /// </summary>
        public static void Info(string message)
        {
            Log(LogLevel.Info, message);
        }

        /// <summary>
        /// 输出Info级别日志（支持格式化，0个参数）
        /// </summary>
        public static void InfoFormat(string format)
        {
            LogFormat(LogLevel.Info, format);
        }

        /// <summary>
        /// 输出Info级别日志（支持格式化，1个参数）
        /// </summary>
        public static void InfoFormat<T1>(string format, T1 arg1)
        {
            LogFormat(LogLevel.Info, format, arg1);
        }

        /// <summary>
        /// 输出Info级别日志（支持格式化，2个参数）
        /// </summary>
        public static void InfoFormat<T1, T2>(string format, T1 arg1, T2 arg2)
        {
            LogFormat(LogLevel.Info, format, arg1, arg2);
        }

        /// <summary>
        /// 输出Info级别日志（支持格式化，3个参数）
        /// </summary>
        public static void InfoFormat<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
        {
            LogFormat(LogLevel.Info, format, arg1, arg2, arg3);
        }

        /// <summary>
        /// 输出Info级别日志（支持格式化，4个参数）
        /// </summary>
        public static void InfoFormat<T1, T2, T3, T4>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            LogFormat(LogLevel.Info, format, arg1, arg2, arg3, arg4);
        }

        /// <summary>
        /// 输出Info级别日志（支持格式化，5个参数）
        /// </summary>
        public static void InfoFormat<T1, T2, T3, T4, T5>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            LogFormat(LogLevel.Info, format, arg1, arg2, arg3, arg4, arg5);
        }

        /// <summary>
        /// 输出Info级别日志（支持格式化，6个参数）
        /// </summary>
        public static void InfoFormat<T1, T2, T3, T4, T5, T6>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            LogFormat(LogLevel.Info, format, arg1, arg2, arg3, arg4, arg5, arg6);
        }

        /// <summary>
        /// 输出Info级别日志（支持格式化，7个参数）
        /// </summary>
        public static void InfoFormat<T1, T2, T3, T4, T5, T6, T7>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            LogFormat(LogLevel.Info, format, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        }

        /// <summary>
        /// 输出Info级别日志（支持格式化，8个参数）
        /// </summary>
        public static void InfoFormat<T1, T2, T3, T4, T5, T6, T7, T8>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            LogFormat(LogLevel.Info, format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        }

        /// <summary>
        /// 输出Info级别日志（支持格式化，1个参数，不带Format后缀）
        /// </summary>
        public static void Info<T1>(string format, T1 arg1)
        {
            LogFormat(LogLevel.Info, format, arg1);
        }

        /// <summary>
        /// 输出Info级别日志（支持格式化，2个参数，不带Format后缀）
        /// </summary>
        public static void Info<T1, T2>(string format, T1 arg1, T2 arg2)
        {
            LogFormat(LogLevel.Info, format, arg1, arg2);
        }

        /// <summary>
        /// 输出Info级别日志（支持格式化，3个参数，不带Format后缀）
        /// </summary>
        public static void Info<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
        {
            LogFormat(LogLevel.Info, format, arg1, arg2, arg3);
        }

        /// <summary>
        /// 输出Info级别日志（支持格式化，4个参数，不带Format后缀）
        /// </summary>
        public static void Info<T1, T2, T3, T4>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            LogFormat(LogLevel.Info, format, arg1, arg2, arg3, arg4);
        }

        /// <summary>
        /// 输出Info级别日志（支持格式化，5个参数，不带Format后缀）
        /// </summary>
        public static void Info<T1, T2, T3, T4, T5>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            LogFormat(LogLevel.Info, format, arg1, arg2, arg3, arg4, arg5);
        }

        /// <summary>
        /// 输出Info级别日志（支持格式化，6个参数，不带Format后缀）
        /// </summary>
        public static void Info<T1, T2, T3, T4, T5, T6>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            LogFormat(LogLevel.Info, format, arg1, arg2, arg3, arg4, arg5, arg6);
        }

        /// <summary>
        /// 输出Info级别日志（支持格式化，7个参数，不带Format后缀）
        /// </summary>
        public static void Info<T1, T2, T3, T4, T5, T6, T7>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            LogFormat(LogLevel.Info, format, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        }

        /// <summary>
        /// 输出Info级别日志（支持格式化，8个参数，不带Format后缀）
        /// </summary>
        public static void Info<T1, T2, T3, T4, T5, T6, T7, T8>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            LogFormat(LogLevel.Info, format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        }

        /// <summary>
        /// 输出Info级别日志（支持Join，连接数组或集合）
        /// </summary>
        public static void InfoJoin<T>(string separator, IEnumerable<T> values)
        {
            LogJoin(LogLevel.Info, separator, values);
        }

        /// <summary>
        /// 输出Info级别日志（支持Join，连接数组或集合，带前缀消息）
        /// </summary>
        public static void InfoJoin<T>(string message, string separator, IEnumerable<T> values)
        {
            LogJoin(LogLevel.Info, message, separator, values);
        }

        #endregion

        #region Warning

        /// <summary>
        /// 输出Warning级别日志
        /// </summary>
        public static void Warning(string message)
        {
            Log(LogLevel.Warning, message);
        }

        /// <summary>
        /// 输出Warning级别日志（支持格式化，0个参数）
        /// </summary>
        public static void WarningFormat(string format)
        {
            LogFormat(LogLevel.Warning, format);
        }

        /// <summary>
        /// 输出Warning级别日志（支持格式化，1个参数）
        /// </summary>
        public static void WarningFormat<T1>(string format, T1 arg1)
        {
            LogFormat(LogLevel.Warning, format, arg1);
        }

        /// <summary>
        /// 输出Warning级别日志（支持格式化，2个参数）
        /// </summary>
        public static void WarningFormat<T1, T2>(string format, T1 arg1, T2 arg2)
        {
            LogFormat(LogLevel.Warning, format, arg1, arg2);
        }

        /// <summary>
        /// 输出Warning级别日志（支持格式化，3个参数）
        /// </summary>
        public static void WarningFormat<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
        {
            LogFormat(LogLevel.Warning, format, arg1, arg2, arg3);
        }

        /// <summary>
        /// 输出Warning级别日志（支持格式化，4个参数）
        /// </summary>
        public static void WarningFormat<T1, T2, T3, T4>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            LogFormat(LogLevel.Warning, format, arg1, arg2, arg3, arg4);
        }

        /// <summary>
        /// 输出Warning级别日志（支持格式化，5个参数）
        /// </summary>
        public static void WarningFormat<T1, T2, T3, T4, T5>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            LogFormat(LogLevel.Warning, format, arg1, arg2, arg3, arg4, arg5);
        }

        /// <summary>
        /// 输出Warning级别日志（支持格式化，6个参数）
        /// </summary>
        public static void WarningFormat<T1, T2, T3, T4, T5, T6>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            LogFormat(LogLevel.Warning, format, arg1, arg2, arg3, arg4, arg5, arg6);
        }

        /// <summary>
        /// 输出Warning级别日志（支持格式化，7个参数）
        /// </summary>
        public static void WarningFormat<T1, T2, T3, T4, T5, T6, T7>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            LogFormat(LogLevel.Warning, format, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        }

        /// <summary>
        /// 输出Warning级别日志（支持格式化，8个参数）
        /// </summary>
        public static void WarningFormat<T1, T2, T3, T4, T5, T6, T7, T8>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            LogFormat(LogLevel.Warning, format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        }

        /// <summary>
        /// 输出Warning级别日志（支持格式化，1个参数，不带Format后缀）
        /// </summary>
        public static void Warning<T1>(string format, T1 arg1)
        {
            LogFormat(LogLevel.Warning, format, arg1);
        }

        /// <summary>
        /// 输出Warning级别日志（支持格式化，2个参数，不带Format后缀）
        /// </summary>
        public static void Warning<T1, T2>(string format, T1 arg1, T2 arg2)
        {
            LogFormat(LogLevel.Warning, format, arg1, arg2);
        }

        /// <summary>
        /// 输出Warning级别日志（支持格式化，3个参数，不带Format后缀）
        /// </summary>
        public static void Warning<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
        {
            LogFormat(LogLevel.Warning, format, arg1, arg2, arg3);
        }

        /// <summary>
        /// 输出Warning级别日志（支持格式化，4个参数，不带Format后缀）
        /// </summary>
        public static void Warning<T1, T2, T3, T4>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            LogFormat(LogLevel.Warning, format, arg1, arg2, arg3, arg4);
        }

        /// <summary>
        /// 输出Warning级别日志（支持格式化，5个参数，不带Format后缀）
        /// </summary>
        public static void Warning<T1, T2, T3, T4, T5>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            LogFormat(LogLevel.Warning, format, arg1, arg2, arg3, arg4, arg5);
        }

        /// <summary>
        /// 输出Warning级别日志（支持格式化，6个参数，不带Format后缀）
        /// </summary>
        public static void Warning<T1, T2, T3, T4, T5, T6>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            LogFormat(LogLevel.Warning, format, arg1, arg2, arg3, arg4, arg5, arg6);
        }

        /// <summary>
        /// 输出Warning级别日志（支持格式化，7个参数，不带Format后缀）
        /// </summary>
        public static void Warning<T1, T2, T3, T4, T5, T6, T7>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            LogFormat(LogLevel.Warning, format, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        }

        /// <summary>
        /// 输出Warning级别日志（支持格式化，8个参数，不带Format后缀）
        /// </summary>
        public static void Warning<T1, T2, T3, T4, T5, T6, T7, T8>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            LogFormat(LogLevel.Warning, format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        }

        /// <summary>
        /// 输出Warning级别日志（支持Join，连接数组或集合）
        /// </summary>
        public static void WarningJoin<T>(string separator, IEnumerable<T> values)
        {
            LogJoin(LogLevel.Warning, separator, values);
        }

        /// <summary>
        /// 输出Warning级别日志（支持Join，连接数组或集合，带前缀消息）
        /// </summary>
        public static void WarningJoin<T>(string message, string separator, IEnumerable<T> values)
        {
            LogJoin(LogLevel.Warning, message, separator, values);
        }

        #endregion

        #region Error

        /// <summary>
        /// 输出Error级别日志
        /// </summary>
        public static void Error(string message)
        {
            Log(LogLevel.Error, message);
        }

        /// <summary>
        /// 输出Error级别日志（支持格式化，0个参数）
        /// </summary>
        public static void ErrorFormat(string format)
        {
            LogFormat(LogLevel.Error, format);
        }

        /// <summary>
        /// 输出Error级别日志（支持格式化，1个参数）
        /// </summary>
        public static void ErrorFormat<T1>(string format, T1 arg1)
        {
            LogFormat(LogLevel.Error, format, arg1);
        }

        /// <summary>
        /// 输出Error级别日志（支持格式化，2个参数）
        /// </summary>
        public static void ErrorFormat<T1, T2>(string format, T1 arg1, T2 arg2)
        {
            LogFormat(LogLevel.Error, format, arg1, arg2);
        }

        /// <summary>
        /// 输出Error级别日志（支持格式化，3个参数）
        /// </summary>
        public static void ErrorFormat<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
        {
            LogFormat(LogLevel.Error, format, arg1, arg2, arg3);
        }

        /// <summary>
        /// 输出Error级别日志（支持格式化，4个参数）
        /// </summary>
        public static void ErrorFormat<T1, T2, T3, T4>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            LogFormat(LogLevel.Error, format, arg1, arg2, arg3, arg4);
        }

        /// <summary>
        /// 输出Error级别日志（支持格式化，5个参数）
        /// </summary>
        public static void ErrorFormat<T1, T2, T3, T4, T5>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            LogFormat(LogLevel.Error, format, arg1, arg2, arg3, arg4, arg5);
        }

        /// <summary>
        /// 输出Error级别日志（支持格式化，6个参数）
        /// </summary>
        public static void ErrorFormat<T1, T2, T3, T4, T5, T6>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            LogFormat(LogLevel.Error, format, arg1, arg2, arg3, arg4, arg5, arg6);
        }

        /// <summary>
        /// 输出Error级别日志（支持格式化，7个参数）
        /// </summary>
        public static void ErrorFormat<T1, T2, T3, T4, T5, T6, T7>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            LogFormat(LogLevel.Error, format, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        }

        /// <summary>
        /// 输出Error级别日志（支持格式化，8个参数）
        /// </summary>
        public static void ErrorFormat<T1, T2, T3, T4, T5, T6, T7, T8>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            LogFormat(LogLevel.Error, format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        }

        /// <summary>
        /// 输出Error级别日志（支持格式化，1个参数，不带Format后缀）
        /// </summary>
        public static void Error<T1>(string format, T1 arg1)
        {
            LogFormat(LogLevel.Error, format, arg1);
        }

        /// <summary>
        /// 输出Error级别日志（支持格式化，2个参数，不带Format后缀）
        /// </summary>
        public static void Error<T1, T2>(string format, T1 arg1, T2 arg2)
        {
            LogFormat(LogLevel.Error, format, arg1, arg2);
        }

        /// <summary>
        /// 输出Error级别日志（支持格式化，3个参数，不带Format后缀）
        /// </summary>
        public static void Error<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
        {
            LogFormat(LogLevel.Error, format, arg1, arg2, arg3);
        }

        /// <summary>
        /// 输出Error级别日志（支持格式化，4个参数，不带Format后缀）
        /// </summary>
        public static void Error<T1, T2, T3, T4>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            LogFormat(LogLevel.Error, format, arg1, arg2, arg3, arg4);
        }

        /// <summary>
        /// 输出Error级别日志（支持格式化，5个参数，不带Format后缀）
        /// </summary>
        public static void Error<T1, T2, T3, T4, T5>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            LogFormat(LogLevel.Error, format, arg1, arg2, arg3, arg4, arg5);
        }

        /// <summary>
        /// 输出Error级别日志（支持格式化，6个参数，不带Format后缀）
        /// </summary>
        public static void Error<T1, T2, T3, T4, T5, T6>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            LogFormat(LogLevel.Error, format, arg1, arg2, arg3, arg4, arg5, arg6);
        }

        /// <summary>
        /// 输出Error级别日志（支持格式化，7个参数，不带Format后缀）
        /// </summary>
        public static void Error<T1, T2, T3, T4, T5, T6, T7>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            LogFormat(LogLevel.Error, format, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        }

        /// <summary>
        /// 输出Error级别日志（支持格式化，8个参数，不带Format后缀）
        /// </summary>
        public static void Error<T1, T2, T3, T4, T5, T6, T7, T8>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            LogFormat(LogLevel.Error, format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        }

        /// <summary>
        /// 输出Error级别日志（支持Join，连接数组或集合）
        /// </summary>
        public static void ErrorJoin<T>(string separator, IEnumerable<T> values)
        {
            LogJoin(LogLevel.Error, separator, values);
        }

        /// <summary>
        /// 输出Error级别日志（支持Join，连接数组或集合，带前缀消息）
        /// </summary>
        public static void ErrorJoin<T>(string message, string separator, IEnumerable<T> values)
        {
            LogJoin(LogLevel.Error, message, separator, values);
        }

        #endregion

        #region Fatal

        /// <summary>
        /// 输出Fatal级别日志
        /// </summary>
        public static void Fatal(string message)
        {
            Log(LogLevel.Fatal, message);
        }

        /// <summary>
        /// 输出Fatal级别日志（支持格式化，0个参数）
        /// </summary>
        public static void FatalFormat(string format)
        {
            LogFormat(LogLevel.Fatal, format);
        }

        /// <summary>
        /// 输出Fatal级别日志（支持格式化，1个参数）
        /// </summary>
        public static void FatalFormat<T1>(string format, T1 arg1)
        {
            LogFormat(LogLevel.Fatal, format, arg1);
        }

        /// <summary>
        /// 输出Fatal级别日志（支持格式化，2个参数）
        /// </summary>
        public static void FatalFormat<T1, T2>(string format, T1 arg1, T2 arg2)
        {
            LogFormat(LogLevel.Fatal, format, arg1, arg2);
        }

        /// <summary>
        /// 输出Fatal级别日志（支持格式化，3个参数）
        /// </summary>
        public static void FatalFormat<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
        {
            LogFormat(LogLevel.Fatal, format, arg1, arg2, arg3);
        }

        /// <summary>
        /// 输出Fatal级别日志（支持格式化，4个参数）
        /// </summary>
        public static void FatalFormat<T1, T2, T3, T4>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            LogFormat(LogLevel.Fatal, format, arg1, arg2, arg3, arg4);
        }

        /// <summary>
        /// 输出Fatal级别日志（支持格式化，5个参数）
        /// </summary>
        public static void FatalFormat<T1, T2, T3, T4, T5>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            LogFormat(LogLevel.Fatal, format, arg1, arg2, arg3, arg4, arg5);
        }

        /// <summary>
        /// 输出Fatal级别日志（支持格式化，6个参数）
        /// </summary>
        public static void FatalFormat<T1, T2, T3, T4, T5, T6>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            LogFormat(LogLevel.Fatal, format, arg1, arg2, arg3, arg4, arg5, arg6);
        }

        /// <summary>
        /// 输出Fatal级别日志（支持格式化，7个参数）
        /// </summary>
        public static void FatalFormat<T1, T2, T3, T4, T5, T6, T7>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            LogFormat(LogLevel.Fatal, format, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        }

        /// <summary>
        /// 输出Fatal级别日志（支持格式化，8个参数）
        /// </summary>
        public static void FatalFormat<T1, T2, T3, T4, T5, T6, T7, T8>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            LogFormat(LogLevel.Fatal, format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        }

        /// <summary>
        /// 输出Fatal级别日志（支持格式化，1个参数，不带Format后缀）
        /// </summary>
        public static void Fatal<T1>(string format, T1 arg1)
        {
            LogFormat(LogLevel.Fatal, format, arg1);
        }

        /// <summary>
        /// 输出Fatal级别日志（支持格式化，2个参数，不带Format后缀）
        /// </summary>
        public static void Fatal<T1, T2>(string format, T1 arg1, T2 arg2)
        {
            LogFormat(LogLevel.Fatal, format, arg1, arg2);
        }

        /// <summary>
        /// 输出Fatal级别日志（支持格式化，3个参数，不带Format后缀）
        /// </summary>
        public static void Fatal<T1, T2, T3>(string format, T1 arg1, T2 arg2, T3 arg3)
        {
            LogFormat(LogLevel.Fatal, format, arg1, arg2, arg3);
        }

        /// <summary>
        /// 输出Fatal级别日志（支持格式化，4个参数，不带Format后缀）
        /// </summary>
        public static void Fatal<T1, T2, T3, T4>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            LogFormat(LogLevel.Fatal, format, arg1, arg2, arg3, arg4);
        }

        /// <summary>
        /// 输出Fatal级别日志（支持格式化，5个参数，不带Format后缀）
        /// </summary>
        public static void Fatal<T1, T2, T3, T4, T5>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            LogFormat(LogLevel.Fatal, format, arg1, arg2, arg3, arg4, arg5);
        }

        /// <summary>
        /// 输出Fatal级别日志（支持格式化，6个参数，不带Format后缀）
        /// </summary>
        public static void Fatal<T1, T2, T3, T4, T5, T6>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            LogFormat(LogLevel.Fatal, format, arg1, arg2, arg3, arg4, arg5, arg6);
        }

        /// <summary>
        /// 输出Fatal级别日志（支持格式化，7个参数，不带Format后缀）
        /// </summary>
        public static void Fatal<T1, T2, T3, T4, T5, T6, T7>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            LogFormat(LogLevel.Fatal, format, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
        }

        /// <summary>
        /// 输出Fatal级别日志（支持格式化，8个参数，不带Format后缀）
        /// </summary>
        public static void Fatal<T1, T2, T3, T4, T5, T6, T7, T8>(string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            LogFormat(LogLevel.Fatal, format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
        }

        /// <summary>
        /// 输出Fatal级别日志（支持Join，连接数组或集合）
        /// </summary>
        public static void FatalJoin<T>(string separator, IEnumerable<T> values)
        {
            LogJoin(LogLevel.Fatal, separator, values);
        }

        /// <summary>
        /// 输出Fatal级别日志（支持Join，连接数组或集合，带前缀消息）
        /// </summary>
        public static void FatalJoin<T>(string message, string separator, IEnumerable<T> values)
        {
            LogJoin(LogLevel.Fatal, message, separator, values);
        }

        #endregion

        #region 内部实现

        /// <summary>
        /// 内部日志输出方法
        /// </summary>
        private static void Log(LogLevel level, string message)
        {
            if (!Enabled || level < CurrentLogLevel)
            {
                return;
            }

            // 获取当前线程的StringBuilder
            var sb = _threadLocalStringBuilder.Value;
            sb.Clear();

            // 构建日志消息
            BuildLogMessage(sb, level, message);

            string formattedMessage = sb.ToString();
            
            // 清空StringBuilder以供下次使用
            sb.Clear();

            // 输出到Unity控制台
            OutputToUnityConsole(level, formattedMessage);

            // 输出到文件（如果启用）
            OutputToFile(level, formattedMessage);
        }

        /// <summary>
        /// 构建日志消息
        /// </summary>
        private static void BuildLogMessage(StringBuilder sb, LogLevel level, string message)
        {
            if (EnableColor)
            {
                sb.Append(level.GetColorTag());
            }
            // 时间戳
            sb.Append('[');
            sb.Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            sb.Append(']');

            // 日志级别
            sb.Append('[');
            sb.Append(level.GetName());
            sb.Append(']');

            // 线程ID
            sb.Append('[');
            sb.Append(Thread.CurrentThread.ManagedThreadId);
            sb.Append(']');

            // 消息内容
            sb.Append(' ');
            sb.Append(message);
            if (EnableColor)
            {
                sb.Append("</color>");
            }
        }

        /// <summary>
        /// 输出到Unity控制台
        /// </summary>
        private static void OutputToUnityConsole(LogLevel level, string message)
        {
            switch (level)
            {
                case LogLevel.Debug:
                case LogLevel.Info:
                    UnityEngine.Debug.Log(message);
                    break;
                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning(message);
                    break;
                case LogLevel.Error:
                case LogLevel.Fatal:
                    UnityEngine.Debug.LogError(message);
                    break;
                case LogLevel.PerformanceTest:
                    break; // 性能测试级别不输出
            }
        }

        /// <summary>
        /// 输出到文件
        /// </summary>
        private static void OutputToFile(LogLevel level, string message)
        {
            if (string.IsNullOrEmpty(LogFilePath))
            {
                return;
            }

            try
            {
                lock (_fileLock)
                {
                    // 确保目录存在
                    string directory = Path.GetDirectoryName(LogFilePath);
                    if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    // 写入文件（追加模式）
                    using (var writer = new StreamWriter(LogFilePath, true, Encoding.UTF8))
                    {
                        writer.WriteLine(message);
                    }
                }
            }
            catch (Exception ex)
            {
                // 文件写入失败时，输出到Unity控制台（避免无限递归）
                UnityEngine.Debug.LogError($"[XLog] 写入日志文件失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 内部格式化日志输出方法（0个参数）
        /// </summary>
        private static void LogFormat(LogLevel level, string format)
        {
            if (!Enabled || level < CurrentLogLevel)
            {
                return;
            }

            var sb = _threadLocalStringBuilder.Value;
            sb.Clear();
            BuildLogMessage(sb, level, format);
            string formattedMessage = sb.ToString();
            sb.Clear();

            OutputToUnityConsole(level, formattedMessage);
            OutputToFile(level, formattedMessage);
        }

        /// <summary>
        /// 内部格式化日志输出方法（1个参数）
        /// </summary>
        private static void LogFormat<T1>(LogLevel level, string format, T1 arg1)
        {
            if (!Enabled || level < CurrentLogLevel)
            {
                return;
            }

            var sb = _threadLocalStringBuilder.Value;
            sb.Clear();
            string formattedContent = string.Format(format, arg1);
            BuildLogMessage(sb, level, formattedContent);
            string formattedMessage = sb.ToString();
            sb.Clear();

            OutputToUnityConsole(level, formattedMessage);
            OutputToFile(level, formattedMessage);
        }

        /// <summary>
        /// 内部格式化日志输出方法（2个参数）
        /// </summary>
        private static void LogFormat<T1, T2>(LogLevel level, string format, T1 arg1, T2 arg2)
        {
            if (!Enabled || level < CurrentLogLevel)
            {
                return;
            }

            var sb = _threadLocalStringBuilder.Value;
            sb.Clear();
            string formattedContent = string.Format(format, arg1, arg2);
            BuildLogMessage(sb, level, formattedContent);
            string formattedMessage = sb.ToString();
            sb.Clear();

            OutputToUnityConsole(level, formattedMessage);
            OutputToFile(level, formattedMessage);
        }

        /// <summary>
        /// 内部格式化日志输出方法（3个参数）
        /// </summary>
        private static void LogFormat<T1, T2, T3>(LogLevel level, string format, T1 arg1, T2 arg2, T3 arg3)
        {
            if (!Enabled || level < CurrentLogLevel)
            {
                return;
            }

            var sb = _threadLocalStringBuilder.Value;
            sb.Clear();
            string formattedContent = string.Format(format, arg1, arg2, arg3);
            BuildLogMessage(sb, level, formattedContent);
            string formattedMessage = sb.ToString();
            sb.Clear();

            OutputToUnityConsole(level, formattedMessage);
            OutputToFile(level, formattedMessage);
        }

        /// <summary>
        /// 内部格式化日志输出方法（4个参数）
        /// </summary>
        private static void LogFormat<T1, T2, T3, T4>(LogLevel level, string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            if (!Enabled || level < CurrentLogLevel)
            {
                return;
            }

            var sb = _threadLocalStringBuilder.Value;
            sb.Clear();
            string formattedContent = string.Format(format, arg1, arg2, arg3, arg4);
            BuildLogMessage(sb, level, formattedContent);
            string formattedMessage = sb.ToString();
            sb.Clear();

            OutputToUnityConsole(level, formattedMessage);
            OutputToFile(level, formattedMessage);
        }

        /// <summary>
        /// 内部格式化日志输出方法（5个参数）
        /// </summary>
        private static void LogFormat<T1, T2, T3, T4, T5>(LogLevel level, string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5)
        {
            if (!Enabled || level < CurrentLogLevel)
            {
                return;
            }

            var sb = _threadLocalStringBuilder.Value;
            sb.Clear();
            string formattedContent = string.Format(format, arg1, arg2, arg3, arg4, arg5);
            BuildLogMessage(sb, level, formattedContent);
            string formattedMessage = sb.ToString();
            sb.Clear();

            OutputToUnityConsole(level, formattedMessage);
            OutputToFile(level, formattedMessage);
        }

        /// <summary>
        /// 内部格式化日志输出方法（6个参数）
        /// </summary>
        private static void LogFormat<T1, T2, T3, T4, T5, T6>(LogLevel level, string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6)
        {
            if (!Enabled || level < CurrentLogLevel)
            {
                return;
            }

            var sb = _threadLocalStringBuilder.Value;
            sb.Clear();
            string formattedContent = string.Format(format, arg1, arg2, arg3, arg4, arg5, arg6);
            BuildLogMessage(sb, level, formattedContent);
            string formattedMessage = sb.ToString();
            sb.Clear();

            OutputToUnityConsole(level, formattedMessage);
            OutputToFile(level, formattedMessage);
        }

        /// <summary>
        /// 内部格式化日志输出方法（7个参数）
        /// </summary>
        private static void LogFormat<T1, T2, T3, T4, T5, T6, T7>(LogLevel level, string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7)
        {
            if (!Enabled || level < CurrentLogLevel)
            {
                return;
            }

            var sb = _threadLocalStringBuilder.Value;
            sb.Clear();
            string formattedContent = string.Format(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7);
            BuildLogMessage(sb, level, formattedContent);
            string formattedMessage = sb.ToString();
            sb.Clear();

            OutputToUnityConsole(level, formattedMessage);
            OutputToFile(level, formattedMessage);
        }

        /// <summary>
        /// 内部格式化日志输出方法（8个参数）
        /// </summary>
        private static void LogFormat<T1, T2, T3, T4, T5, T6, T7, T8>(LogLevel level, string format, T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5, T6 arg6, T7 arg7, T8 arg8)
        {
            if (!Enabled || level < CurrentLogLevel)
            {
                return;
            }

            var sb = _threadLocalStringBuilder.Value;
            sb.Clear();
            string formattedContent = string.Format(format, arg1, arg2, arg3, arg4, arg5, arg6, arg7, arg8);
            BuildLogMessage(sb, level, formattedContent);
            string formattedMessage = sb.ToString();
            sb.Clear();

            OutputToUnityConsole(level, formattedMessage);
            OutputToFile(level, formattedMessage);
        }

        /// <summary>
        /// 内部Join日志输出方法（连接数组或集合）
        /// </summary>
        private static void LogJoin<T>(LogLevel level, string separator, IEnumerable<T> values)
        {
            if (!Enabled || level < CurrentLogLevel)
            {
                return;
            }

            if (values == null)
            {
                Log(level, "null");
                return;
            }

            var sb = _threadLocalStringBuilder.Value;
            sb.Clear();

            // 使用StringBuilder连接值
            bool first = true;
            foreach (var value in values)
            {
                if (!first)
                {
                    sb.Append(separator);
                }
                sb.Append(value);
                first = false;
            }

            string joinedMessage = sb.ToString();
            sb.Clear();

            // 构建完整的日志消息
            BuildLogMessage(sb, level, joinedMessage);
            string formattedMessage = sb.ToString();
            sb.Clear();

            OutputToUnityConsole(level, formattedMessage);
            OutputToFile(level, formattedMessage);
        }

        /// <summary>
        /// 内部Join日志输出方法（连接数组或集合，带前缀消息）
        /// </summary>
        private static void LogJoin<T>(LogLevel level, string message, string separator, IEnumerable<T> values)
        {
            if (!Enabled || level < CurrentLogLevel)
            {
                return;
            }

            if (values == null)
            {
                Log(level, $"{message}: null");
                return;
            }

            var sb = _threadLocalStringBuilder.Value;
            sb.Clear();

            // 添加前缀消息
            sb.Append(message);
            sb.Append(": ");

            // 使用StringBuilder连接值
            bool first = true;
            foreach (var value in values)
            {
                if (!first)
                {
                    sb.Append(separator);
                }
                sb.Append(value);
                first = false;
            }

            string joinedMessage = sb.ToString();
            sb.Clear();

            // 构建完整的日志消息
            BuildLogMessage(sb, level, joinedMessage);
            string formattedMessage = sb.ToString();
            sb.Clear();

            OutputToUnityConsole(level, formattedMessage);
            OutputToFile(level, formattedMessage);
        }

        #endregion
    }
}
