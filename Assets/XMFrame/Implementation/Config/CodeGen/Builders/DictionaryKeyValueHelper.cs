using System;

namespace XM.ConfigNew.CodeGen.Builders
{
    /// <summary>
    /// Dictionary Key/Value 处理辅助类
    /// 使用统一值转换器处理所有类型转换
    /// </summary>
    public static class DictionaryKeyValueHelper
    {
        /// <summary>
        /// Key/Value 处理结果（使用统一转换结果）
        /// </summary>
        public class KeyProcessResult
        {
            /// <summary>Key 表达式变量名</summary>
            public string KeyExprVar { get; set; }
            
            /// <summary>是否需要关闭 if 块</summary>
            public bool NeedsCloseBlock { get; set; }
        }
        
        /// <summary>
        /// 生成 Dictionary Key 的处理代码（使用统一值转换器）
        /// </summary>
        /// <param name="builder">代码构建器</param>
        /// <param name="keyType">Key 的类型（Managed）</param>
        /// <param name="kvpKeyAccess">kvp.Key 访问表达式</param>
        /// <param name="suffix">变量名后缀（用于区分不同嵌套层级）</param>
        /// <returns>Key 处理结果</returns>
        public static KeyProcessResult GenerateKeyProcessing(
            CodeBuilder builder, 
            Type keyType, 
            string kvpKeyAccess, 
            string suffix = "")
        {
            var resultPrefix = string.IsNullOrEmpty(suffix) ? "key" : $"key{suffix}";
            
            // 使用统一转换器（自动处理所有类型）
            var conversion = UnifiedValueConverter.GenerateConversion(
                builder, keyType, kvpKeyAccess, resultPrefix, UnifiedValueConverter.UsageContext.Alloc);
            
            return new KeyProcessResult
            {
                KeyExprVar = conversion.ConvertedValueVar,
                NeedsCloseBlock = conversion.NeedsCloseBlock
            };
        }
        
        /// <summary>
        /// 关闭 Key 处理的 if 块（如果需要）
        /// </summary>
        public static void CloseKeyProcessingBlock(CodeBuilder builder, KeyProcessResult keyResult)
        {
            if (keyResult.NeedsCloseBlock)
            {
                builder.EndBlock();
            }
        }
    }
}
