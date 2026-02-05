using System;

namespace XM.ConfigNew.CodeGen.Builders
{
    /// <summary>
    /// Dictionary Key/Value 处理辅助类
    /// 封装 Dictionary 中 Key 和 Value 的转换逻辑，消除重复代码块
    /// </summary>
    public static class DictionaryKeyValueHelper
    {
        /// <summary>
        /// Key 处理结果
        /// </summary>
        public class KeyProcessResult
        {
            /// <summary>Key 表达式变量名</summary>
            public string KeyExprVar { get; set; }
            
            /// <summary>是否需要关闭 if 块（string 类型的 TryGetStrI）</summary>
            public bool NeedsCloseBlock { get; set; }
        }
        
        /// <summary>
        /// 生成 Dictionary Key 的处理代码
        /// </summary>
        /// <param name="builder">代码构建器</param>
        /// <param name="keyType">Key 的类型</param>
        /// <param name="kvpKeyAccess">kvp.Key 访问表达式</param>
        /// <param name="suffix">变量名后缀（用于区分不同嵌套层级）</param>
        /// <returns>Key 处理结果</returns>
        public static KeyProcessResult GenerateKeyProcessing(CodeBuilder builder, Type keyType, string kvpKeyAccess, string suffix = "")
        {
            var result = new KeyProcessResult();
            
            // 获取实际类型（处理可空）
            var isNullable = TypeHelper.IsNullableType(keyType);
            var actualKeyType = isNullable ? Nullable.GetUnderlyingType(keyType) ?? keyType : keyType;
            var keyAccess = isNullable ? CodeBuilder.BuildGetValueOrDefault(kvpKeyAccess) : kvpKeyAccess;
            
            if (actualKeyType == typeof(string))
            {
                // string 类型需要转换为 StrI
                var keyStrVar = string.IsNullOrEmpty(suffix) ? "keyStrI" : $"keyStr{suffix}";
                builder.BeginIfBlock($"{CodeGenConstants.TryGetStrIMethod}({kvpKeyAccess}, out var {keyStrVar})");
                result.KeyExprVar = keyStrVar;
                result.NeedsCloseBlock = true;
            }
            else if (actualKeyType.IsEnum)
            {
                // enum 类型需要包装
                var enumTypeName = TypeHelper.GetGlobalQualifiedTypeName(actualKeyType);
                var keyVar = string.IsNullOrEmpty(suffix) ? "key" : $"key{suffix}";
                builder.AppendVarDeclaration(keyVar, CodeBuilder.BuildEnumWrapper(enumTypeName, keyAccess));
                result.KeyExprVar = keyVar;
                result.NeedsCloseBlock = false;
            }
            else
            {
                // 基本类型直接使用
                var keyVar = string.IsNullOrEmpty(suffix) ? "key" : $"key{suffix}";
                builder.AppendVarDeclaration(keyVar, keyAccess);
                result.KeyExprVar = keyVar;
                result.NeedsCloseBlock = false;
            }
            
            return result;
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
