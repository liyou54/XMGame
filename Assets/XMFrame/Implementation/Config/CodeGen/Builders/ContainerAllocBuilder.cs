using System;
using XM.ConfigNew.Metadata;

namespace XM.ConfigNew.CodeGen.Builders
{
    /// <summary>
    /// 容器分配代码生成器
    /// 负责生成 AllocXXX 方法，将托管容器转换为非托管 Blob 容器
    /// </summary>
    public static class ContainerAllocBuilder
    {
        /// <summary>
        /// 生成容器分配方法的完整实现
        /// </summary>
        public static void GenerateAllocMethod(CodeBuilder builder, ConfigFieldMetadata field, string managedTypeName, string unmanagedTypeName)
        {
            var typeInfo = field.TypeInfo;
            
            builder.AppendXmlComment($"分配 {field.FieldName} 容器");
            builder.BeginPrivateMethod(
                $"void {field.AllocMethodName}({managedTypeName} config, ref {unmanagedTypeName} data, CfgI cfgi, XM.ConfigDataCenter.ConfigDataHolder configHolderData)",
                null);
            
            switch (typeInfo.ContainerType)
            {
                case EContainerType.List:
                    GenerateListAlloc(builder, field);
                    break;
                    
                case EContainerType.Dictionary:
                    GenerateDictionaryAlloc(builder, field);
                    break;
                    
                case EContainerType.HashSet:
                    GenerateHashSetAlloc(builder, field);
                    break;
                    
                default:
                    // 不支持的类型，生成 TODO
                    builder.AppendComment($"TODO: 不支持的容器类型 {typeInfo.ContainerType}");
                    break;
            }
            
            builder.EndMethod();
        }
        
        #region List 分配
        
        /// <summary>
        /// 生成 List 分配逻辑
        /// </summary>
        private static void GenerateListAlloc(CodeBuilder builder, ConfigFieldMetadata field)
        {
            var fieldName = field.FieldName;
            var elementType = field.TypeInfo.NestedValueType;
            
            // null 或空检查
            builder.AppendNullOrEmptyReturn(CodeBuilder.BuildConfigFieldAccess(fieldName));
            builder.AppendLine();
            
            // 检查是否是嵌套容器
            if (TypeHelper.IsContainerType(elementType))
            {
                GenerateNestedListAlloc(builder, field);
            }
            else if (TypeHelper.IsConfigType(elementType))
            {
                GenerateConfigListAlloc(builder, field);
            }
            else
            {
                GenerateSimpleListAlloc(builder, field, elementType);
            }
        }
        
        /// <summary>
        /// 生成简单类型 List 分配（int, float, string, enum, CfgS 等）
        /// </summary>
        private static void GenerateSimpleListAlloc(CodeBuilder builder, ConfigFieldMetadata field, Type elementType)
        {
            var fieldName = field.FieldName;
            var unmanagedElementTypeName = TypeHelper.GetUnmanagedElementTypeName(elementType);
            
            // 分配数组并遍历
            var configFieldAccess = CodeBuilder.BuildConfigFieldAccess(fieldName);
            builder.AppendAllocArray("array", unmanagedElementTypeName, $"{configFieldAccess}.{CodeGenConstants.CountProperty}");
            builder.BeginIndexLoop("i", configFieldAccess);
            
            // 使用统一的元素赋值生成器
            ElementValueGenerator.GenerateIndexAssignment(builder, elementType, CodeBuilder.BuildConfigFieldIndexAccess(fieldName, "i"), "array", "i");
            
            builder.EndBlock(); // for
            builder.AppendLine();
            builder.AppendAssignment(CodeBuilder.BuildDataFieldAccess(fieldName), "array");
        }
        
        /// <summary>
        /// 生成嵌套容器 List 分配（如 List<List<int>>，支持任意深度嵌套）
        /// </summary>
        private static void GenerateNestedListAlloc(CodeBuilder builder, ConfigFieldMetadata field)
        {
            var fieldName = field.FieldName;
            var elementType = field.TypeInfo.NestedValueType;
            var innerUnmanagedType = TypeHelper.GetUnmanagedContainerTypeName(elementType);
            
            // 分配外层数组
            var configFieldAccess = CodeBuilder.BuildConfigFieldAccess(fieldName);
            builder.AppendAllocArray("outerArray", innerUnmanagedType, $"{configFieldAccess}.{CodeGenConstants.CountProperty}");
            builder.BeginIndexLoop("i", configFieldAccess);
            
            builder.AppendVarDeclaration("innerList", CodeBuilder.BuildConfigFieldIndexAccess(fieldName, "i"));
            builder.AppendNullOrEmptyContinue("innerList");
            builder.AppendLine();
            
            // 递归生成内层容器分配代码
            GenerateRecursiveContainerAlloc(builder, elementType, "innerList", "outerArray", "i", 0);
            
            builder.EndBlock(); // for i
            builder.AppendLine();
            builder.AppendAssignment(CodeBuilder.BuildDataFieldAccess(fieldName), "outerArray");
        }
        
        /// <summary>
        /// 递归生成容器分配代码（支持任意深度嵌套）
        /// 通过 ref 方式赋值嵌套容器，确保不会将托管类型直接赋值给 Blob
        /// </summary>
        private static void GenerateRecursiveContainerAlloc(CodeBuilder builder, Type containerType, string sourceVar, string targetArrayVar, string targetIndexVar, int depth)
        {
            RecursiveContainerAllocator.GenerateAllocation(
                builder, containerType, sourceVar, targetArrayVar, targetIndexVar,
                depth, "arr", "j",
                RecursiveContainerAllocator.AssignmentMode.BlobIndex,
                GenerateInnerContainerAlloc);
        }
        
        /// <summary>
        /// 生成内部容器分配代码（填充到 ref 变量）
        /// </summary>
        private static void GenerateInnerContainerAlloc(CodeBuilder builder, Type containerType, string sourceVar, string targetVar, int depth)
        {
            RecursiveContainerAllocator.GenerateAllocation(
                builder, containerType, sourceVar, targetVar, null,
                depth, "innerArr", "n",
                RecursiveContainerAllocator.AssignmentMode.Direct,
                GenerateInnerContainerAlloc);
        }
        
        
        
        /// <summary>
        /// 生成配置类型 List 分配（如 List<AttributeConfig>）
        /// </summary>
        private static void GenerateConfigListAlloc(CodeBuilder builder, ConfigFieldMetadata field)
        {
            var fieldName = field.FieldName;
            var elementType = field.TypeInfo.NestedValueType;
            var elementTypeName = TypeHelper.GetGlobalQualifiedTypeName(elementType);
            var helperTypeName = elementTypeName + CodeGenConstants.ClassHelperSuffix;
            var unmanagedElementTypeName = TypeHelper.GetConfigUnmanagedTypeName(elementType);
            
            var configFieldAccess = CodeBuilder.BuildConfigFieldAccess(fieldName);
            builder.AppendAllocArray("array", unmanagedElementTypeName, $"{configFieldAccess}.{CodeGenConstants.CountProperty}");
            builder.BeginIndexLoop("i", configFieldAccess);
            
            // 使用统一的配置类型赋值方法
            ConfigHelperInvoker.GenerateIndexAssignment(
                builder, elementType, CodeBuilder.BuildConfigFieldIndexAccess(fieldName, "i"), 
                "array", "i");
            
            builder.EndBlock(); // for
            builder.AppendLine();
            builder.AppendAssignment(CodeBuilder.BuildDataFieldAccess(fieldName), "array");
        }
        
        #endregion
        
        #region Dictionary 分配
        
        /// <summary>
        /// 生成 Dictionary 分配逻辑
        /// </summary>
        private static void GenerateDictionaryAlloc(CodeBuilder builder, ConfigFieldMetadata field)
        {
            var fieldName = field.FieldName;
            var keyType = field.TypeInfo.NestedKeyType;
            var valueType = field.TypeInfo.NestedValueType;
            
            // null 或空检查
            builder.AppendNullOrEmptyReturn(CodeBuilder.BuildConfigFieldAccess(fieldName));
            builder.AppendLine();
            
            // 分配 Map
            var configFieldAccess = CodeBuilder.BuildConfigFieldAccess(fieldName);
            var unmanagedKeyType = TypeHelper.GetUnmanagedElementTypeName(keyType);
            var unmanagedValueType = TypeHelper.GetUnmanagedElementTypeName(valueType);
            
            builder.AppendAllocMap("map", unmanagedKeyType, unmanagedValueType, $"{configFieldAccess}.{CodeGenConstants.CountProperty}");
            builder.BeginForeachLoop("var", CodeGenConstants.KvpVar, configFieldAccess);
            
            // 处理 Key（使用统一转换器）
            var kvpKeyAccess = $"{CodeGenConstants.KvpVar}.Key";
            var keyResult = DictionaryKeyValueHelper.GenerateKeyProcessing(builder, keyType, kvpKeyAccess, "");
            
            // 处理 Value
            ContainerElementHandler.GenerateDictionaryValueProcessing(
                builder, valueType, $"{CodeGenConstants.KvpVar}.Value", "map", keyResult.KeyExprVar, "",
                GenerateInnerContainerAlloc, 0);
            
            // 关闭 Key 处理的 if 块
            DictionaryKeyValueHelper.CloseKeyProcessingBlock(builder, keyResult);
            
            builder.EndBlock(); // foreach
            builder.AppendLine();
            builder.AppendAssignment(CodeBuilder.BuildDataFieldAccess(fieldName), "map");
        }
        
        /// <summary>
        /// 生成深层嵌套容器赋值代码（递归）
        /// </summary>
        private static void GenerateDeepNestedContainerAssignment(CodeBuilder builder, Type containerType, string sourceAccess, string targetArrayVarName, string indexVar)
        {
            if (TypeHelper.IsListType(containerType))
            {
                var innerElementType = TypeHelper.GetContainerElementType(containerType);
                var innerElementUnmanagedType = TypeHelper.GetUnmanagedElementTypeName(innerElementType);
                
                builder.BeginIfBlock(CodeBuilder.BuildNotNullAndNotEmptyCondition(sourceAccess));
                builder.AppendAllocArray("deepArray", innerElementUnmanagedType, $"{sourceAccess}.{CodeGenConstants.CountProperty}");
                builder.BeginCountLoop("k", $"{sourceAccess}.{CodeGenConstants.CountProperty}");
                
                // 使用统一的元素赋值生成器
                ElementValueGenerator.GenerateIndexAssignment(builder, innerElementType, $"{sourceAccess}[k]", "deepArray", "k");
                
                builder.EndBlock(); // for k
                builder.AppendBlobIndexAssign(targetArrayVarName, indexVar, "deepArray");
                builder.EndBlock(); // if
            }
            else
            {
                builder.AppendComment($"TODO: 深层嵌套容器类型 {containerType.Name} 暂不支持");
            }
        }
        
        #endregion
        
        #region HashSet 分配
        
        /// <summary>
        /// 生成 HashSet 分配逻辑
        /// </summary>
        private static void GenerateHashSetAlloc(CodeBuilder builder, ConfigFieldMetadata field)
        {
            var fieldName = field.FieldName;
            var elementType = field.TypeInfo.NestedValueType;
            
            // null 或空检查
            builder.AppendNullOrEmptyReturn(CodeBuilder.BuildConfigFieldAccess(fieldName));
            builder.AppendLine();
            
            // 分配 Set
            var configFieldAccess = CodeBuilder.BuildConfigFieldAccess(fieldName);
            var unmanagedElementTypeName = TypeHelper.GetUnmanagedElementTypeName(elementType);
            
            builder.AppendAllocSet("set", unmanagedElementTypeName, $"{configFieldAccess}.{CodeGenConstants.CountProperty}");
            builder.BeginForeachLoop("var", "item", configFieldAccess);
            
            // 使用统一的元素添加生成器
            ElementValueGenerator.GenerateSetAdd(builder, elementType, "item", "set");
            
            builder.EndBlock(); // foreach
            builder.AppendLine();
            builder.AppendAssignment(CodeBuilder.BuildDataFieldAccess(fieldName), "set");
        }
        
        #endregion
        
    }
}
