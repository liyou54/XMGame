using System;

namespace XM.Utils
{
    /// <summary>
    /// 异常工具。反射调用（Invoke、CreateInstance、LoadFrom 等）抛出的异常常包含 InnerException，需一并输出以便排查。
    /// </summary>
    public static class ExceptionUtil
    {
        /// <summary>
        /// 获取异常消息，若存在内部异常则拼接 " | 内部: ..." 链，便于反射等场景下看到真实原因。
        /// </summary>
        public static string GetMessageWithInner(Exception ex)
        {
            if (ex == null) return string.Empty;
            var msg = ex.Message ?? "";
            var inner = ex.InnerException;
            while (inner != null)
            {
                msg += " | 内部: " + (inner.Message ?? "");
                inner = inner.InnerException;
            }
            return msg;
        }
    }
}
