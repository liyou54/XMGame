using System;
using System.Globalization;
using System.Xml;
using XM;

namespace XM.Contracts.Config
{
    /// <summary>
    /// 配置加载辅助类非泛型基类。XML 反序列化由生成的 *ClassHelper 静态代码实现，无反射。
    /// </summary>
    public abstract class ConfigClassHelper
    {
        /// <summary>
        /// 解析告警回调（可由外部设置为 UnityEngine.Debug.LogWarning 等），用于通用解析方法异常/失败时打日志。
        /// </summary>
        public static Action<string> OnParseWarning;

        public abstract TblS GetTblS();
        public abstract IXConfig Create();

        public abstract void SetTblIDefinedInMod(ModI modHandle);

        /// <summary>
        /// 递归判断本表或父类表中是否已存在该 (mod, configName)
        /// </summary>
        public abstract bool TryExistsInHierarchy(ModS mod, string configName, out CfgS key);

        /// <summary>
        /// 从 XML 反序列化配置。由生成的 *ClassHelper 实现：先 Create，再 FillFromXml，无反射。
        /// </summary>
        public abstract IXConfig DeserializeConfigFromXml(XmlElement configItem, ModS mod, string configName);

        /// <summary>
        /// 将 XML 节点解析并填入已有配置实例（构造与解析拆开）。子类先调基类 FillFromXml 再填本类字段；由生成的 *ClassHelper 实现。
        /// </summary>
        public virtual void FillFromXml(IXConfig target, XmlElement configItem, ModS mod, string configName)
        {
            throw new NotSupportedException($"FillFromXml 应由生成的 {GetType().Name} 实现。");
        }

        /// <summary>
        /// 从 XML 节点获取字段值：优先取同名子元素 InnerText，否则取同名属性。供生成代码调用。
        /// </summary>
        protected static string GetXmlFieldValue(XmlElement parent, string fieldName)
        {
            var el = parent?.SelectSingleNode(fieldName) as XmlElement;
            if (el != null && !string.IsNullOrEmpty(el.InnerText))
                return el.InnerText.Trim();
            var attr = parent?.GetAttribute(fieldName);
            return attr?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// 通用解析失败时打日志，供生成代码或通用解析方法调用。
        /// </summary>
        protected static void LogParseWarning(string fieldName, string value, Exception ex)
        {
            OnParseWarning?.Invoke($"[Config] 解析字段 {fieldName} 失败 value='{value ?? ""}' {ex?.Message ?? ""}");
        }

        #region 通用解析方法（异常处理 + 日志）

        protected static bool TryParseInt(string s, string fieldName, out int value)
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

        protected static bool TryParseLong(string s, string fieldName, out long value)
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

        protected static bool TryParseShort(string s, string fieldName, out short value)
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

        protected static bool TryParseByte(string s, string fieldName, out byte value)
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

        protected static bool TryParseFloat(string s, string fieldName, out float value)
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

        protected static bool TryParseDouble(string s, string fieldName, out double value)
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

        protected static bool TryParseBool(string s, string fieldName, out bool value)
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

        protected static bool TryParseDecimal(string s, string fieldName, out decimal value)
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

        /// <summary>
        /// 解析 "Mod::ConfigName" 或 "Mod::TableName::ConfigName"，返回 modName 与 configName（三段时 configName 为第三段）。
        /// </summary>
        protected static bool TryParseCfgSString(string s, string fieldName, out string modName, out string configName)
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
        protected static bool TryParseLabelSString(string s, string fieldName, out string modName, out string labelName)
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

    /// <summary>
    /// 配置加载辅助类泛型基接口
    /// </summary>
    public abstract class ConfigClassHelper<T, TUnmanaged> :
        ConfigClassHelper where T : XM.IXConfig, new()
        where TUnmanaged : unmanaged, IConfigUnManaged<TUnmanaged>
    {
        public static bool IsInitialized = false;

        /// <summary>基类 Helper 类型（如果有继承，由代码生成器或反射初始化）</summary>
        public static Type BaseHelpType { get; } = null;

        /// <summary>基类配置类型</summary>
        public static Type BasicClassType { get; } = null;

        /// <summary>基类 Unmanaged 类型</summary>
        public static Type BasicTUnmanaged { get; } = null;

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

        /// <summary>
        /// 递归判断本表或父类表中是否已存在该配置
        /// </summary>
        public override bool TryExistsInHierarchy(ModS mod, string configName, out CfgS key)
        {
            // 先检查当前表是否存在
            var tableS = GetTblS();
            var configDataCenter = XM.Contracts.IConfigDataCenter.I;
            
            // 获取 TableHandle
            var tableI = configDataCenter.GetTblI(tableS);
            if (tableI.Valid && configDataCenter.TryExistsConfig(tableI, mod, configName))
            {
                // 当前表中存在
                key = new CfgS(mod, tableS.TableName, configName);
                return true;
            }

            // 如果有基类，递归查找
            if (BaseHelpType != null)
            {
                var baseHelper = configDataCenter.GetClassHelper(BaseHelpType);
                if (baseHelper != null && baseHelper.TryExistsInHierarchy(mod, configName, out key))
                {
                    return true;
                }
            }

            // 不存在
            key = default;
            return false;
        }
    }
}