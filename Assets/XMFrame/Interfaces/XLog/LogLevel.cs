namespace XMFrame
{
    /// <summary>
    /// 日志级别
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// 调试信息 - 灰色
        /// </summary>
        Debug = 0,
        
        /// <summary>
        /// 普通信息 - 白色
        /// </summary>
        Info = 1,
        
        /// <summary>
        /// 警告信息 - 黄色
        /// </summary>
        Warning = 2,
        
        /// <summary>
        /// 错误信息 - 红色
        /// </summary>
        Error = 3,
        
        /// <summary>
        /// 严重错误 - 深红色
        /// </summary>
        Fatal = 4
    }

    /// <summary>
    /// 日志级别扩展方法，用于获取颜色
    /// </summary>
    public static class LogLevelExtensions
    {
        /// <summary>
        /// 获取日志级别对应的Unity颜色标签
        /// </summary>
        public static string GetColorTag(this LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Debug:
                    return "<color=#808080>"; // 灰色
                case LogLevel.Info:
                    return "<color=#FFFFFF>"; // 白色
                case LogLevel.Warning:
                    return "<color=#FFFF00>"; // 黄色
                case LogLevel.Error:
                    return "<color=#FF0000>"; // 红色
                case LogLevel.Fatal:
                    return "<color=#8B0000>"; // 深红色
                default:
                    return "<color=#FFFFFF>"; // 默认白色
            }
        }

        /// <summary>
        /// 获取日志级别对应的名称
        /// </summary>
        public static string GetName(this LogLevel level)
        {
            return level.ToString();
        }
    }
}
