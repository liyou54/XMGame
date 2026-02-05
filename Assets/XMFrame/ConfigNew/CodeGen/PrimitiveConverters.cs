using System;
using System.Runtime.CompilerServices;

namespace XM.ConfigNew.CodeGen
{
    /// <summary>
    /// 基本数据类型内联转换方法
    /// 提供高性能的内联转换,避免装箱和虚函数调用
    /// </summary>
    public static class PrimitiveConverters
    {
        #region 整数类型转换
        
        /// <summary>
        /// 字符串转int (内联)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ConvertToInt(string source, out int target)
        {
            return int.TryParse(source, out target);
        }
        
        /// <summary>
        /// 字符串转long (内联)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ConvertToLong(string source, out long target)
        {
            return long.TryParse(source, out target);
        }
        
        /// <summary>
        /// 字符串转short (内联)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ConvertToShort(string source, out short target)
        {
            return short.TryParse(source, out target);
        }
        
        /// <summary>
        /// 字符串转byte (内联)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ConvertToByte(string source, out byte target)
        {
            return byte.TryParse(source, out target);
        }
        
        /// <summary>
        /// 字符串转sbyte (内联)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ConvertToSByte(string source, out sbyte target)
        {
            return sbyte.TryParse(source, out target);
        }
        
        /// <summary>
        /// 字符串转uint (内联)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ConvertToUInt(string source, out uint target)
        {
            return uint.TryParse(source, out target);
        }
        
        /// <summary>
        /// 字符串转ulong (内联)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ConvertToULong(string source, out ulong target)
        {
            return ulong.TryParse(source, out target);
        }
        
        /// <summary>
        /// 字符串转ushort (内联)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ConvertToUShort(string source, out ushort target)
        {
            return ushort.TryParse(source, out target);
        }
        
        #endregion
        
        #region 浮点类型转换
        
        /// <summary>
        /// 字符串转float (内联)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ConvertToFloat(string source, out float target)
        {
            return float.TryParse(source, out target);
        }
        
        /// <summary>
        /// 字符串转double (内联)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ConvertToDouble(string source, out double target)
        {
            return double.TryParse(source, out target);
        }
        
        /// <summary>
        /// 字符串转decimal (内联)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ConvertToDecimal(string source, out decimal target)
        {
            return decimal.TryParse(source, out target);
        }
        
        #endregion
        
        #region 布尔类型转换
        
        /// <summary>
        /// 字符串转bool (内联)
        /// 支持: "true"/"false", "1"/"0", "yes"/"no"
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ConvertToBool(string source, out bool target)
        {
            if (string.IsNullOrEmpty(source))
            {
                target = false;
                return true;
            }
            
            var lower = source.ToLowerInvariant();
            if (lower == CodeGenConstants.TrueString || lower == CodeGenConstants.OneString || lower == CodeGenConstants.YesString)
            {
                target = true;
                return true;
            }
            
            if (lower == CodeGenConstants.FalseString || lower == CodeGenConstants.ZeroString || lower == CodeGenConstants.NoString)
            {
                target = false;
                return true;
            }
            
            return bool.TryParse(source, out target);
        }
        
        #endregion
        
        #region 字符串类型转换
        
        /// <summary>
        /// 字符串转字符串 (内联,直接返回)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ConvertToString(string source, out string target)
        {
            target = source ?? string.Empty;
            return true;
        }
        
        /// <summary>
        /// 字符串转char (内联)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ConvertToChar(string source, out char target)
        {
            if (!string.IsNullOrEmpty(source) && source.Length > 0)
            {
                target = source[0];
                return true;
            }
            
            target = '\0';
            return false;
        }
        
        #endregion
        
        #region 枚举类型转换
        
        /// <summary>
        /// 字符串转枚举 (内联,泛型)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ConvertToEnum<TEnum>(string source, out TEnum target) where TEnum : struct, Enum
        {
            return Enum.TryParse(source, true, out target);
        }
        
        /// <summary>
        /// 字符串转枚举 (非泛型版本)
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool ConvertToEnum(string source, Type enumType, out object target)
        {
            if (enumType == null || !enumType.IsEnum)
            {
                target = null;
                return false;
            }
            
            try
            {
                target = Enum.Parse(enumType, source, true);
                return true;
            }
            catch
            {
                target = Activator.CreateInstance(enumType);
                return false;
            }
        }
        
        #endregion
        
        #region 类型检测和分发
        
        /// <summary>
        /// 根据目标类型自动选择转换方法
        /// </summary>
        public static bool ConvertByType(string source, Type targetType, out object result)
        {
            result = null;
            
            if (targetType == typeof(int))
            {
                var success = ConvertToInt(source, out var value);
                result = value;
                return success;
            }
            
            if (targetType == typeof(float))
            {
                var success = ConvertToFloat(source, out var value);
                result = value;
                return success;
            }
            
            if (targetType == typeof(bool))
            {
                var success = ConvertToBool(source, out var value);
                result = value;
                return success;
            }
            
            if (targetType == typeof(long))
            {
                var success = ConvertToLong(source, out var value);
                result = value;
                return success;
            }
            
            if (targetType == typeof(double))
            {
                var success = ConvertToDouble(source, out var value);
                result = value;
                return success;
            }
            
            if (targetType == typeof(byte))
            {
                var success = ConvertToByte(source, out var value);
                result = value;
                return success;
            }
            
            if (targetType == typeof(short))
            {
                var success = ConvertToShort(source, out var value);
                result = value;
                return success;
            }
            
            if (targetType == typeof(string))
            {
                var success = ConvertToString(source, out var value);
                result = value;
                return success;
            }
            
            if (targetType == typeof(char))
            {
                var success = ConvertToChar(source, out var value);
                result = value;
                return success;
            }
            
            if (targetType.IsEnum)
            {
                return ConvertToEnum(source, targetType, out result);
            }
            
            return false;
        }
        
        /// <summary>
        /// 获取类型对应的转换方法名(用于代码生成)
        /// </summary>
        public static string GetConverterMethodName(Type targetType)
        {
            if (targetType == typeof(int)) return nameof(ConvertToInt);
            if (targetType == typeof(float)) return nameof(ConvertToFloat);
            if (targetType == typeof(bool)) return nameof(ConvertToBool);
            if (targetType == typeof(long)) return nameof(ConvertToLong);
            if (targetType == typeof(double)) return nameof(ConvertToDouble);
            if (targetType == typeof(byte)) return nameof(ConvertToByte);
            if (targetType == typeof(short)) return nameof(ConvertToShort);
            if (targetType == typeof(string)) return nameof(ConvertToString);
            if (targetType == typeof(char)) return nameof(ConvertToChar);
            if (targetType.IsEnum) return nameof(ConvertToEnum);
            
            return null;
        }
        
        #endregion
    }
}
