namespace XMFrame.Interfaces.ConfigMananger
{
    /// <summary>
    /// 非托管类型转换基类，用于将托管类型转换为非托管类型
    /// </summary>
    /// <typeparam name="T">源类型（托管类型）</typeparam>
    /// <typeparam name="TUnmanaged">目标类型（非托管类型）</typeparam>
    public abstract class XmlUnManagedConvert<T, TUnmanaged> where TUnmanaged : unmanaged
    {
        /// <summary>
        /// 将源类型转换为目标类型
        /// </summary>
        /// <param name="source">源类型实例</param>
        /// <returns>转换后的非托管类型</returns>
        public abstract TUnmanaged Convert(T source);
    }
}
