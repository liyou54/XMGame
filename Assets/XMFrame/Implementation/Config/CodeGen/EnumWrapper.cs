using System;

namespace XM.ConfigNew.CodeGen
{
    /// <summary>
    /// 枚举包装器 - 为枚举类型提供IEquatable支持
    /// 内部存储int,使用时转换为枚举,Burst兼容
    /// </summary>
    /// <typeparam name="TEnum">枚举类型</typeparam>
    public struct EnumWrapper<TEnum> : IEquatable<EnumWrapper<TEnum>> 
        where TEnum : unmanaged, Enum
    {
        /// <summary>
        /// 内部存储为int值
        /// </summary>
        public int Value;
        
        public EnumWrapper(TEnum enumValue)
        {
            unsafe
            {
                Value = *(int*)(&enumValue);
            }
        }
        
        public EnumWrapper(int intValue)
        {
            Value = intValue;
        }
        
        /// <summary>
        /// 转换为枚举类型
        /// </summary>
        public TEnum ToEnum()
        {
            unsafe
            {
                int temp = Value;
                return *(TEnum*)(&temp);
            }
        }
        
        public bool Equals(EnumWrapper<TEnum> other)
        {
            return Value == other.Value;
        }
        
        public override bool Equals(object obj)
        {
            return obj is EnumWrapper<TEnum> other && Equals(other);
        }
        
        public override int GetHashCode()
        {
            return Value;
        }
        
        public static bool operator ==(EnumWrapper<TEnum> left, EnumWrapper<TEnum> right)
        {
            return left.Value == right.Value;
        }
        
        public static bool operator !=(EnumWrapper<TEnum> left, EnumWrapper<TEnum> right)
        {
            return left.Value != right.Value;
        }
        
        /// <summary>
        /// 隐式转换: EnumWrapper → TEnum
        /// </summary>
        public static implicit operator TEnum(EnumWrapper<TEnum> wrapper)
        {
            return wrapper.ToEnum();
        }
        
        /// <summary>
        /// 隐式转换: TEnum → EnumWrapper
        /// </summary>
        public static implicit operator EnumWrapper<TEnum>(TEnum value)
        {
            return new EnumWrapper<TEnum>(value);
        }
    }
}
