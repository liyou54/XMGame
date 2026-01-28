using XM.Contracts;

namespace XM.Contracts.Config
{
    /// <summary>XML 自定义转换基类，实现 ITypeConverter&lt;string, T&gt; 以便注册到 TypeConverterRegistry。</summary>
    public abstract class XmlConvertBase<T, This> : ITypeConverter<string, T> where This : XmlConvertBase<T, This>, new()
    {
        private static This _instance;
        public static This Instance => _instance ??= new This();

        public abstract bool TryGetData(string str, out T data);

        public T Convert(string source)
        {
            return TryGetData(source, out var data) ? data : default;
        }
    }
}