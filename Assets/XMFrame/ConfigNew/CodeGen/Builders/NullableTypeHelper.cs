using System;

namespace XM.ConfigNew.CodeGen.Builders
{
    /// <summary>
    /// 可空类型处理辅助类
    /// 封装可空类型的判断和访问表达式构建，消除重复逻辑
    /// </summary>
    public static class NullableTypeHelper
    {
        /// <summary>
        /// 可空类型信息
        /// </summary>
        public class NullableInfo
        {
            /// <summary>是否是可空类型</summary>
            public bool IsNullable { get; set; }
            
            /// <summary>实际类型（去除可空后）</summary>
            public Type ActualType { get; set; }
            
            /// <summary>源表达式</summary>
            public string SourceExpr { get; set; }
            
            /// <summary>值访问表达式（自动处理 GetValueOrDefault）</summary>
            public string ValueAccessExpr { get; set; }
        }
        
        /// <summary>
        /// 分析可空类型并构建访问表达式
        /// </summary>
        /// <param name="type">要分析的类型</param>
        /// <param name="sourceExpr">源表达式（如 config.field[i]）</param>
        /// <returns>可空类型信息</returns>
        public static NullableInfo Analyze(Type type, string sourceExpr)
        {
            var isNullable = TypeHelper.IsNullableType(type);
            var actualType = isNullable 
                ? Nullable.GetUnderlyingType(type) ?? type 
                : type;
            
            return new NullableInfo
            {
                IsNullable = isNullable,
                ActualType = actualType,
                SourceExpr = sourceExpr,
                ValueAccessExpr = isNullable 
                    ? CodeBuilder.BuildGetValueOrDefault(sourceExpr) 
                    : sourceExpr
            };
        }
        
        /// <summary>
        /// 获取实际类型（去除可空）
        /// </summary>
        public static Type GetActualType(Type type)
        {
            return TypeHelper.IsNullableType(type)
                ? Nullable.GetUnderlyingType(type) ?? type
                : type;
        }
        
        /// <summary>
        /// 构建值访问表达式（自动处理可空）
        /// </summary>
        public static string BuildValueAccess(Type type, string sourceExpr)
        {
            return TypeHelper.IsNullableType(type)
                ? CodeBuilder.BuildGetValueOrDefault(sourceExpr)
                : sourceExpr;
        }
    }
}
