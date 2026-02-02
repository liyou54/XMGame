using System.Text;

namespace XM.RuntimeTest
{
    /// <summary>
    /// 测试结果
    /// </summary>
    public class TestResult
    {
        /// <summary>测试是否成功</summary>
        public bool Success { get; set; }
        
        /// <summary>测试消息</summary>
        public string Message { get; set; }
        
        /// <summary>执行时间（秒）</summary>
        public float ExecutionTime { get; set; }
        
        /// <summary>详细日志</summary>
        public StringBuilder DetailLog { get; set; }
        
        /// <summary>测试统计信息（如配置数量等）</summary>
        public string Statistics { get; set; }

        public TestResult()
        {
            Success = false;
            Message = string.Empty;
            ExecutionTime = 0f;
            DetailLog = new StringBuilder();
            Statistics = string.Empty;
        }

        public override string ToString()
        {
            var status = Success ? "✓ 成功" : "✗ 失败";
            return $"{status} - {Message} ({ExecutionTime:F2}s) {Statistics}";
        }
    }
}
