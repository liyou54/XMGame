using System;
using System.Text;

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

        /// <summary>
        /// 获取完整诊断信息（消息链 + 堆栈），便于排查 NotImplemented 等异常的具体位置。
        /// </summary>
        public static string GetFullDiagnostic(Exception ex)
        {
            if (ex == null) return string.Empty;
            var sb = new StringBuilder();
            sb.Append(GetMessageWithInner(ex));
            var e = ex;
            while (e != null)
            {
                if (!string.IsNullOrEmpty(e.StackTrace))
                {
                    sb.Append("\n[").Append(e.GetType().Name).Append("] 堆栈:\n").Append(e.StackTrace);
                }
                e = e.InnerException;
            }
            return sb.ToString();
        }
    }
}
