using XM.Contracts;
using XM.Contracts.Config;

namespace XM.Utils.Converters
{
    /// <summary>
    /// 字符串到 StrI 的转换器
    /// StrI 用于在配置系统中表示字符串的唯一ID
    /// </summary>
    public class StringToStrIConverter : ITypeConverter<string, StrI>
    {
        private static readonly StringToStrIConverter _instance = new StringToStrIConverter();
        
        public static StringToStrIConverter Instance => _instance;

        static StringToStrIConverter()
        {
            // 自动注册到全局转换器
            TypeConverterRegistry.RegisterGlobalConverter(_instance);
        }

        private StringToStrIConverter()
        {
            // 私有构造函数，确保单例
        }

        public bool Convert(string source, out StrI target)
        {
            target = default;
            
            if (string.IsNullOrEmpty(source))
            {
                return true; // 空字符串返回默认值
            }

            // TODO: 实现字符串到 StrI 的实际转换逻辑
            // 这里需要根据你的字符串存储策略来实现
            // 例如：
            // 1. 从字符串池中查找或注册字符串
            // 2. 返回对应的唯一ID
            
            // 临时实现：使用字符串的哈希码作为ID
            target = new StrI { Id = source.GetHashCode() };
            
            return true;
        }
    }
}
