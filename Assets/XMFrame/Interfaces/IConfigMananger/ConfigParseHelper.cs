using System;
using System.Globalization;
using System.Xml;

namespace XM.Contracts.Config
{
    /// <summary>
    /// 配置 XML 解析通用方法：TryParse 数值/字符串、GetXmlFieldValue、Log 与 IsStrictMode。
    /// 由 ConfigClassHelper 委托调用，供生成的 *ClassHelper 与模板直接引用，保持 IClassHelper 职责单一。
    /// </summary>
    public static class ConfigParseHelper
    {
        #region 回调

        /// <summary>
        /// 解析告警回调（可由外部设置为 UnityEngine.Debug.LogWarning 等），用于 TryParse 异常/失败时打日志。
        /// </summary>
        public static Action<string> OnParseWarning;

        /// <summary>
        /// 解析错误回调（严格模式下解析失败时调用，含 文件、行、字段）。
        /// </summary>
        public static Action<string> OnParseError;

        #endregion

        #region XML 与日志

        /// <summary>
        /// 从 XML 节点获取字段值：优先取同名子元素 InnerText，否则取同名属性。
        /// </summary>
        public static string GetXmlFieldValue(XmlElement parent, string fieldName)
        {
            var el = parent?.SelectSingleNode(fieldName) as XmlElement;
            if (el != null && !string.IsNullOrEmpty(el.InnerText))
                return el.InnerText.Trim();
            var attr = parent?.GetAttribute(fieldName);
            return attr?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// 通用解析失败时打 Warning。
        /// </summary>
        public static void LogParseWarning(string fieldName, string value, Exception ex)
        {
            OnParseWarning?.Invoke($"[Config] 解析字段 {fieldName} 失败 value='{value ?? ""}' {ex?.Message ?? ""}");
        }

        /// <summary>
        /// 严格模式下解析失败时打 Error（文件、行、字段、错误）。
        /// </summary>
        public static void LogParseError(string file, int line, string field, Exception ex)
        {
            var msg = $"文件: {file ?? ""}, 行: {line}, 字段: {field ?? ""}, 错误: {ex?.Message ?? ""}";
            OnParseError?.Invoke(msg);
        }

        /// <summary>
        /// 严格模式下解析失败时打 Error（文件、行、字段、消息）。
        /// </summary>
        public static void LogParseError(string file, int line, string field, string message)
        {
            var msg = $"文件: {file ?? ""}, 行: {line}, 字段: {field ?? ""}, 错误: {message ?? ""}";
            OnParseError?.Invoke(msg);
        }

        /// <summary>
        /// 严格模式下解析失败时打 Error（使用 context 中的文件、行）。
        /// </summary>
        public static void LogParseError(in ConfigParseContext context, string field, Exception ex)
        {
            LogParseError(context.FilePath, context.Line, field, ex);
        }

        /// <summary>
        /// 严格模式下解析失败时打 Error（使用 context 中的文件、行）。
        /// </summary>
        public static void LogParseError(in ConfigParseContext context, string field, string message)
        {
            LogParseError(context.FilePath, context.Line, field, message);
        }

        /// <summary>
        /// 判断解析上下文是否为严格模式（None 或 ReWrite）。
        /// </summary>
        public static bool IsStrictMode(in ConfigParseContext context) =>
            context.Mode == OverrideMode.None || context.Mode == OverrideMode.ReWrite;

        #endregion

        #region TryParse 数值与布尔

        public static bool TryParseInt(string s, string fieldName, out int value)
        {
            value = default;
            if (string.IsNullOrEmpty(s)) return false;
            try
            {
                value = int.Parse(s, CultureInfo.InvariantCulture);
                return true;
            }
            catch (Exception ex)
            {
                LogParseWarning(fieldName, s, ex);
                return false;
            }
        }

        public static bool TryParseLong(string s, string fieldName, out long value)
        {
            value = default;
            if (string.IsNullOrEmpty(s)) return false;
            try
            {
                value = long.Parse(s, CultureInfo.InvariantCulture);
                return true;
            }
            catch (Exception ex)
            {
                LogParseWarning(fieldName, s, ex);
                return false;
            }
        }

        public static bool TryParseShort(string s, string fieldName, out short value)
        {
            value = default;
            if (string.IsNullOrEmpty(s)) return false;
            try
            {
                value = short.Parse(s, CultureInfo.InvariantCulture);
                return true;
            }
            catch (Exception ex)
            {
                LogParseWarning(fieldName, s, ex);
                return false;
            }
        }

        public static bool TryParseByte(string s, string fieldName, out byte value)
        {
            value = default;
            if (string.IsNullOrEmpty(s)) return false;
            try
            {
                value = byte.Parse(s, CultureInfo.InvariantCulture);
                return true;
            }
            catch (Exception ex)
            {
                LogParseWarning(fieldName, s, ex);
                return false;
            }
        }

        public static bool TryParseFloat(string s, string fieldName, out float value)
        {
            value = default;
            if (string.IsNullOrEmpty(s)) return false;
            try
            {
                value = float.Parse(s, CultureInfo.InvariantCulture);
                return true;
            }
            catch (Exception ex)
            {
                LogParseWarning(fieldName, s, ex);
                return false;
            }
        }

        public static bool TryParseDouble(string s, string fieldName, out double value)
        {
            value = default;
            if (string.IsNullOrEmpty(s)) return false;
            try
            {
                value = double.Parse(s, CultureInfo.InvariantCulture);
                return true;
            }
            catch (Exception ex)
            {
                LogParseWarning(fieldName, s, ex);
                return false;
            }
        }

        public static bool TryParseBool(string s, string fieldName, out bool value)
        {
            value = false;
            if (string.IsNullOrEmpty(s)) return false;
            try
            {
                var b = s.Trim().ToLowerInvariant();
                value = b == "1" || b == "true" || b == "yes";
                return true;
            }
            catch (Exception ex)
            {
                LogParseWarning(fieldName, s, ex);
                return false;
            }
        }

        public static bool TryParseDecimal(string s, string fieldName, out decimal value)
        {
            value = default;
            if (string.IsNullOrEmpty(s)) return false;
            try
            {
                value = decimal.Parse(s, CultureInfo.InvariantCulture);
                return true;
            }
            catch (Exception ex)
            {
                LogParseWarning(fieldName, s, ex);
                return false;
            }
        }

        #endregion

        #region TryParse 字符串（CfgS / LabelS）

        /// <summary>
        /// 解析 "Mod::ConfigName" 或 "Mod::TableName::ConfigName"，返回 modName 与 configName（三段时 configName 为第三段）。
        /// </summary>
        public static bool TryParseCfgSString(string s, string fieldName, out string modName, out string configName)
        {
            modName = null;
            configName = null;
            if (string.IsNullOrEmpty(s)) return false;
            try
            {
                var p = s.Split(new[] { "::" }, StringSplitOptions.None);
                if (p.Length < 2) return false;
                modName = p[0].Trim();
                configName = p.Length >= 3 ? p[2].Trim() : p[1].Trim();
                
                // 验证解析结果不能为空
                if (string.IsNullOrEmpty(modName) || string.IsNullOrEmpty(configName))
                {
                    LogParseWarning(fieldName, s, new Exception($"配置引用格式错误：模块名或配置名为空 (input: '{s}')"));
                    modName = null;
                    configName = null;
                    return false;
                }
                
                return true;
            }
            catch (Exception ex)
            {
                LogParseWarning(fieldName, s, ex);
                return false;
            }
        }

        /// <summary>
        /// 解析 "ModName::LabelName" 为两个字符串。
        /// </summary>
        public static bool TryParseLabelSString(string s, string fieldName, out string modName, out string labelName)
        {
            modName = null;
            labelName = null;
            if (string.IsNullOrEmpty(s)) return false;
            try
            {
                var p = s.Split(new[] { "::" }, StringSplitOptions.None);
                if (p.Length != 2) return false;
                modName = p[0].Trim();
                labelName = p[1].Trim();
                return true;
            }
            catch (Exception ex)
            {
                LogParseWarning(fieldName, s, ex);
                return false;
            }
        }

        #endregion
    }
}
