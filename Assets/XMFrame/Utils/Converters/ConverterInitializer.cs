using XM.Contracts;

namespace XM.Utils.Converters
{
    /// <summary>
    /// 转换器初始化器，确保所有全局转换器在使用前被注册
    /// </summary>
    public static class ConverterInitializer
    {
        private static bool _initialized = false;

        /// <summary>
        /// 初始化所有全局转换器
        /// 在配置系统启动前调用此方法
        /// </summary>
        public static void Initialize()
        {
            if (_initialized)
                return;

            _initialized = true;

            // 触发静态构造函数，注册转换器
            var _ = StringToStrIConverter.Instance;
            
            XLog.Info("[ConverterInitializer] 转换器初始化完成");
        }
    }
}
