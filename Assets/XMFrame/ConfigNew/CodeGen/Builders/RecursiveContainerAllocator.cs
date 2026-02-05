using System;
using XM.ConfigNew.Metadata;

namespace XM.ConfigNew.CodeGen.Builders
{
    /// <summary>
    /// 递归容器分配器
    /// 统一处理 List/Dictionary/HashSet 的递归分配逻辑，消除三个递归方法之间的重复
    /// </summary>
    public static class RecursiveContainerAllocator
    {
        /// <summary>
        /// 赋值方式枚举
        /// </summary>
        public enum AssignmentMode
        {
            /// <summary>Blob 索引赋值: target[BlobContainer, index] = value</summary>
            BlobIndex,
            
            /// <summary>直接赋值: target = value</summary>
            Direct,
            
            /// <summary>Blob Map 赋值: target[BlobContainer, key] = value</summary>
            BlobMap
        }
        
        /// <summary>
        /// 生成递归容器分配代码（通用方法）
        /// </summary>
        /// <param name="builder">代码构建器</param>
        /// <param name="containerType">容器类型</param>
        /// <param name="sourceVar">源容器变量名</param>
        /// <param name="targetExpr">目标表达式（变量名 或 数组名）</param>
        /// <param name="targetIndex">目标索引（用于 BlobIndex 模式）或 Key（用于 BlobMap 模式）</param>
        /// <param name="depth">递归深度</param>
        /// <param name="varPrefix">变量名前缀（arr, innerArr, valArr 等）</param>
        /// <param name="loopVarBase">循环变量基础名（j, n, m 等）</param>
        /// <param name="assignMode">赋值方式</param>
        /// <param name="recursiveCallback">递归回调</param>
        public static void GenerateAllocation(
            CodeBuilder builder,
            Type containerType,
            string sourceVar,
            string targetExpr,
            string targetIndex,
            int depth,
            string varPrefix,
            string loopVarBase,
            AssignmentMode assignMode,
            Action<CodeBuilder, Type, string, string, int> recursiveCallback)
        {
            var elementType = TypeHelper.GetContainerElementType(containerType);
            var elementUnmanagedType = TypeHelper.GetUnmanagedElementTypeName(elementType);
            
            var suffix = depth == 0 ? "" : $"_{depth}";
            var loopVar = depth == 0 ? loopVarBase : $"{loopVarBase}{depth}";
            var containerVar = $"{varPrefix}{suffix}";
            
            if (TypeHelper.IsListType(containerType))
            {
                // List 分配
                builder.AppendAllocArray(containerVar, elementUnmanagedType, $"{sourceVar}.{CodeGenConstants.CountProperty}");
                builder.BeginCountLoop(loopVar, $"{sourceVar}.{CodeGenConstants.CountProperty}");
                
                var elementAccess = CodeBuilder.BuildIndexAccess(sourceVar, loopVar);
                ContainerElementHandler.GenerateListElementProcessing(
                    builder, elementType, elementAccess, containerVar, loopVar,
                    recursiveCallback, depth);
                
                builder.EndBlock(); // for
                AssignResult(builder, assignMode, targetExpr, targetIndex, containerVar);
            }
            else if (TypeHelper.IsDictionaryType(containerType))
            {
                // Dictionary 分配
                var keyType = TypeHelper.GetDictionaryKeyType(containerType);
                var valueType = TypeHelper.GetDictionaryValueType(containerType);
                var keyUnmanagedType = TypeHelper.GetUnmanagedElementTypeName(keyType);
                var valueUnmanagedType = TypeHelper.GetUnmanagedElementTypeName(valueType);
                
                var mapVar = containerVar;
                var kvpVar = $"kvp{suffix}";
                
                builder.AppendAllocMap(mapVar, keyUnmanagedType, valueUnmanagedType, $"{sourceVar}.{CodeGenConstants.CountProperty}");
                builder.BeginForeachLoop("var", kvpVar, sourceVar);
                
                // 处理 Key（使用统一转换器）
                var kvpKeyAccess = $"{kvpVar}.Key";
                var keyResult = DictionaryKeyValueHelper.GenerateKeyProcessing(builder, keyType, kvpKeyAccess, suffix);
                var keyExpr = keyResult.KeyExprVar;
                
                // 处理 Value
                ContainerElementHandler.GenerateDictionaryValueProcessing(
                    builder, valueType, $"{kvpVar}.Value", mapVar, keyExpr, suffix,
                    recursiveCallback, depth);
                
                DictionaryKeyValueHelper.CloseKeyProcessingBlock(builder, keyResult);
                
                builder.EndBlock(); // foreach
                AssignResult(builder, assignMode, targetExpr, targetIndex, containerVar);
            }
            else if (TypeHelper.IsHashSetType(containerType))
            {
                // HashSet 分配
                var setVar = containerVar;
                builder.AppendAllocSet(setVar, elementUnmanagedType, $"{sourceVar}.{CodeGenConstants.CountProperty}");
                builder.BeginForeachLoop("var", $"item{suffix}", sourceVar);
                
                ElementValueGenerator.GenerateSetAdd(builder, elementType, $"item{suffix}", setVar);
                
                builder.EndBlock(); // foreach
                AssignResult(builder, assignMode, targetExpr, targetIndex, containerVar);
            }
        }
        
        /// <summary>
        /// 根据赋值方式生成赋值代码
        /// </summary>
        private static void AssignResult(CodeBuilder builder, AssignmentMode mode, string targetExpr, string indexOrKey, string valueVar)
        {
            switch (mode)
            {
                case AssignmentMode.BlobIndex:
                    builder.AppendBlobIndexAssign(targetExpr, indexOrKey, valueVar);
                    break;
                
                case AssignmentMode.BlobMap:
                    builder.AppendBlobMapAssign(targetExpr, indexOrKey, valueVar);
                    break;
                
                case AssignmentMode.Direct:
                    builder.AppendAssignment(targetExpr, valueVar);
                    break;
            }
        }
    }
}
