using System;
using System.Collections.Generic;
using System.Linq;
using XM.ConfigNew.Metadata;

namespace XM.ConfigNew.CodeGen
{
    /// <summary>
    /// 索引结构体代码生成器
    /// 为每个索引生成一个部分类文件
    /// </summary>
    public class IndexStructGenerator
    {
        private readonly ConfigClassMetadata _classMetadata;
        private readonly ConfigIndexMetadata _indexMetadata;
        private readonly CodeBuilder _builder;
        
        
        public IndexStructGenerator(ConfigClassMetadata classMetadata, ConfigIndexMetadata indexMetadata)
        {
            _classMetadata = classMetadata ?? throw new ArgumentNullException(nameof(classMetadata));
            _indexMetadata = indexMetadata ?? throw new ArgumentNullException(nameof(indexMetadata));
            _builder = new CodeBuilder();
        }
        
        #region 主生成方法 
        
        /// <summary>
        /// 生成索引结构体代码
        /// </summary>
        public string Generate()
        {
            _builder.Clear();
            
            // 1. 生成文件头
            GenerateFileHeader();
            
            // 2. 生成命名空间（如果有）
            if (!string.IsNullOrEmpty(_classMetadata.Namespace))
            {
                _builder.BeginNamespace(_classMetadata.Namespace);
            }
            
            // 3. 生成Unmanaged部分类声明
            _builder.AppendComment($"{_classMetadata.UnmanagedTypeName} 的部分类 - {_indexMetadata.IndexName}索引");
            _builder.BeginClass(
                _classMetadata.UnmanagedTypeName,
                baseClass: null,
                isPartial: true,
                isStruct: true
            );
            
            // 4. 生成索引结构体
            GenerateIndexStruct();
            
            // 5. 结束部分类
            _builder.EndClass();
            
            // 6. 生成扩展方法静态类
            GenerateExtensionClass();
            
            // 7. 结束命名空间（如果有）
            if (!string.IsNullOrEmpty(_classMetadata.Namespace))
            {
                _builder.EndNamespace();
            }
            
            return _builder.Build();
        }
        
        #endregion
        
        #region 文件头生成
        
        /// <summary>
        /// 生成文件头
        /// </summary>
        private void GenerateFileHeader()
        {
            _builder.AppendUsing("System");
            _builder.AppendUsing("XM");
            _builder.AppendUsing("XM.Contracts.Config");
            _builder.AppendUsing("Unity.Collections");
            _builder.AppendLine();
        }
        
        #endregion
        
        #region 索引结构体生成
        
        /// <summary>
        /// 生成索引结构体
        /// </summary>
        private void GenerateIndexStruct()
        {
            var structName = _indexMetadata.GeneratedStructName;
            var unmanagedTypeName = TypeHelper.GetGlobalQualifiedTypeName(_classMetadata.UnmanagedType);
            // 实现 IConfigIndexGroup<T> 和 IEquatable<T>
            var interfaceName = $"IConfigIndexGroup<{unmanagedTypeName}>, IEquatable<{structName}>";
            
            _builder.AppendXmlComment($"{_indexMetadata.IndexName} 索引结构体");
            _builder.BeginClass(structName, interfaceName, isPartial: false, isStruct: true);
            
            // 生成静态 IndexType 属性
            GenerateStaticIndexType();
            
            // 生成索引字段
            GenerateIndexFields();
            
            // 生成构造方法
            GenerateConstructor();
            
            // 生成Equals和GetHashCode方法
            GenerateEquatableMethods();
            
            _builder.EndClass();
        }
        
        #endregion
        
        #region 静态 IndexType 生成
        
        /// <summary>
        /// 生成静态 IndexType 属性
        /// </summary>
        private void GenerateStaticIndexType()
        {
            var indexNumber = GetIndexNumber();
            
            // 生成私有静态字段用于缓存
            _builder.AppendLine("private static IndexType? indexType;");
            _builder.AppendLine();
            
            _builder.AppendXmlComment("索引类型标识（实现 IConfigIndexGroup 接口要求）");
            _builder.AppendLine("public static IndexType IndexType");
            _builder.BeginBlock();
            _builder.AppendLine("get");
            _builder.BeginBlock();
            _builder.AppendLine("if (indexType == null)");
            _builder.BeginBlock();
            _builder.AppendLine("indexType = new IndexType");
            _builder.BeginBlock();
            _builder.AppendLine($"Tbl = {_classMetadata.HelperTypeName}.TblI,");
            _builder.AppendLine($"Index = {indexNumber}");
            _builder.EndBlock(true); // semicolon = true
            _builder.EndBlock();
            _builder.AppendLine();
            _builder.AppendLine("return indexType.Value;");
            _builder.EndBlock();
            _builder.EndBlock();
            _builder.AppendLine();
        }
        
        /// <summary>
        /// 获取索引编号（在配置类的所有索引中的位置）
        /// </summary>
        private short GetIndexNumber()
        {
            if (_classMetadata.Indexes == null)
                return 0;
            
            for (short i = 0; i < _classMetadata.Indexes.Count; i++)
            {
                if (_classMetadata.Indexes[i].IndexName == _indexMetadata.IndexName)
                    return i;
            }
            
            return 0;
        }
        
        #endregion
        
        #region 索引字段生成
        
        /// <summary>
        /// 生成索引字段
        /// </summary>
        private void GenerateIndexFields()
        {
            _builder.AppendComment(CodeGenConstants.IndexFieldComment);
            
            foreach (var field in _indexMetadata.IndexFields)
            {
                var fieldType = GetIndexFieldType(field);
                var fieldName = field.FieldName;
                
                _builder.AppendField(fieldType, fieldName, $"{CodeGenConstants.IndexFieldComment}: {fieldName}");
            }
            
            _builder.AppendLine();
        }
        
        /// <summary>
        /// 获取索引字段的类型
        /// </summary>
        /// <param name="field">字段元数据</param>
        /// <returns>索引字段的非托管类型名称</returns>
        private string GetIndexFieldType(ConfigFieldMetadata field)
        {
            // 对于 XMLLink 自动生成的索引，字段类型应该是 CfgI<ParentType>
            // 检查字段名是否以任何 Link 字段名开头（没有后缀，因为 LinkParentSuffix 为空）
            var linkField = _classMetadata.Fields?.FirstOrDefault(f => 
                f.IsXmlLink && field.FieldName == f.FieldName);
            
            if (linkField != null && linkField.XmlLinkTargetType != null)
            {
                // 返回父节点的 CfgI 类型
                return TypeHelper.GetCfgITypeName(linkField.XmlLinkTargetType);
            }
            
            // 优先使用预计算的非托管类型名
            if (!string.IsNullOrEmpty(field.UnmanagedFieldTypeName))
            {
                return field.UnmanagedFieldTypeName;
            }
            
            var typeInfo = field.TypeInfo;
            
            // 枚举类型（使用全局限定名）
            if (typeInfo.IsEnum && typeInfo.SingleValueType != null)
                return TypeHelper.GetGlobalQualifiedTypeName(typeInfo.SingleValueType);
            
            // CfgS 类型
            if (typeInfo?.ManagedFieldType != null && TypeHelper.IsCfgSType(typeInfo.ManagedFieldType))
            {
                var targetType = TypeHelper.GetContainerElementType(typeInfo.ManagedFieldType);
                if (targetType != null)
                {
                    return TypeHelper.GetCfgITypeName(targetType);
                }
            }
            
            // 基本类型
            if (typeInfo.SingleValueType == typeof(int))
                return "int";
            if (typeInfo.SingleValueType == typeof(string))
                return "int"; // StrI
            if (typeInfo.SingleValueType == typeof(float))
                return "float";
            if (typeInfo.SingleValueType == typeof(bool))
                return "bool";
            if (typeInfo.SingleValueType == typeof(long))
                return "long";
            
            return "int"; // 默认
        }
        
        #endregion
        
        #region 构造方法生成
        
        /// <summary>
        /// 生成构造方法
        /// </summary>
        private void GenerateConstructor()
        {
            var structName = _indexMetadata.GeneratedStructName;
            var parameters = GetConstructorParameters();
            
            _builder.AppendXmlComment(
                "构造方法",
                _indexMetadata.IndexFields.ToDictionary(
                    f => f.FieldName.ToLower(),
                    f => $"{f.FieldName}值"
                )
            );
            
            _builder.BeginMethod($"{structName}({parameters})");
            
            // 赋值语句
            foreach (var field in _indexMetadata.IndexFields)
            {
                var paramName = field.FieldName.ToLower();
                _builder.AppendLine($"this.{field.FieldName} = {paramName};");
            }
            
            _builder.EndMethod();
            _builder.AppendLine();
        }
        
        /// <summary>
        /// 获取构造方法参数列表
        /// </summary>
        /// <returns>参数列表字符串,格式: "type1 name1, type2 name2"</returns>
        private string GetConstructorParameters()
        {
            var parameters = _indexMetadata.IndexFields.Select(f =>
            {
                var type = GetIndexFieldType(f);
                var name = f.FieldName.ToLower();
                return $"{type} {name}";
            });
            
            return string.Join(", ", parameters);
        }
        
        #endregion
        
        #region Equals和GetHashCode生成
        
        /// <summary>
        /// 生成Equals和GetHashCode方法
        /// </summary>
        private void GenerateEquatableMethods()
        {
            var structName = _indexMetadata.GeneratedStructName;
            
            // Equals方法
            _builder.AppendXmlComment("判断索引是否相等");
            _builder.BeginMethod($"bool Equals({structName} other)");
            
            var conditions = _indexMetadata.IndexFields.Select(f => $"this.{f.FieldName} == other.{f.FieldName}");
            var condition = string.Join(" && ", conditions);
            _builder.AppendLine($"return {condition};");
            
            _builder.EndMethod();
            _builder.AppendLine();
            
            // GetHashCode方法
            _builder.AppendXmlComment("获取哈希码");
            _builder.BeginMethod("override int GetHashCode()");
            _builder.AppendLine("unchecked");
            _builder.BeginBlock();
            _builder.AppendLine($"int hash = {CodeGenConstants.HashInitialValue};");
            foreach (var field in _indexMetadata.IndexFields)
            {
                _builder.AppendLine($"hash = hash * {CodeGenConstants.HashMultiplier} + {field.FieldName}.GetHashCode();");
            }
            _builder.AppendLine("return hash;");
            _builder.EndBlock();
            _builder.EndMethod();
            _builder.AppendLine();
        }
        
        #endregion
        
        #region GetVal方法生成
        
        /// <summary>
        /// 生成GetVal方法
        /// </summary>
        private void GenerateGetValMethods()
        {
            var unmanagedTypeName = _classMetadata.UnmanagedTypeName;
            
            const string GetValMethodName = "GetVal";
            const string GetValsMethodName = "GetVals";
            
            if (_indexMetadata.IsUnique)
            {
                // 唯一索引: GetVal(this index) -> CfgI<T>
                GenerateGetValMethod(unmanagedTypeName, GetValMethodName);
            }
            else
            {
                // 多值索引: GetVals(this index, Allocator) -> NativeArray<CfgI<T>>
                GenerateGetValsMethod(unmanagedTypeName, GetValsMethodName);
            }
        }
        
        /// <summary>
        /// 生成GetVal方法(唯一索引)
        /// </summary>
        private void GenerateGetValMethod(string unmanagedTypeName, string methodName)
        {
            var qualifiedUnmanagedTypeName = TypeHelper.GetGlobalQualifiedTypeName(_classMetadata.UnmanagedType);
            var cfgIReturnType = CodeBuilder.BuildCfgITypeName(qualifiedUnmanagedTypeName);
            
            _builder.AppendXmlComment(
                "获取索引对应的配置引用(唯一索引)",
                new Dictionary<string, string>
                {
                    { "index", "索引值" },
                    { "data", "配置数据容器" },
                    { "returns", "配置引用 CfgI" }
                }
            );
            
            // 扩展方法参数需要完整类型路径
            var indexTypeName = $"{unmanagedTypeName}.{_indexMetadata.GeneratedStructName}";
            var methodSignature = $"static {cfgIReturnType} {methodName}(this {indexTypeName} index, in XM.ConfigData data)";
            _builder.BeginMethod(methodSignature);
            
            _builder.AppendComment(CodeGenConstants.GetIndexContainerComment);
            _builder.AppendLine(CodeBuilder.BuildGetIndexCall(indexTypeName, qualifiedUnmanagedTypeName));
            _builder.AppendLine($"if (!{CodeGenConstants.IndexMapVar}.{CodeGenConstants.ValidProperty})");
            _builder.BeginBlock();
            _builder.AppendLine($"return default({cfgIReturnType});");
            _builder.EndBlock();
            _builder.AppendLine();
            
            _builder.AppendComment(CodeGenConstants.QueryIndexCfgIComment);
            _builder.AppendLine($"if (!{CodeGenConstants.IndexMapVar}.{CodeGenConstants.TryGetValueMethod}({CodeGenConstants.DataVarBlobContainerAccess}, index, out var cfgI))");
            _builder.BeginBlock();
            _builder.AppendLine($"return default({cfgIReturnType});");
            _builder.EndBlock();
            _builder.AppendLine();
            
            _builder.AppendLine($"return {CodeBuilder.BuildCfgIAsExpression("cfgI", qualifiedUnmanagedTypeName)};");
            
            _builder.EndMethod();
        }
        
        /// <summary>
        /// 生成GetVals方法(多值索引)
        /// </summary>
        private void GenerateGetValsMethod(string unmanagedTypeName, string methodName)
        {
            var qualifiedUnmanagedTypeName = TypeHelper.GetGlobalQualifiedTypeName(_classMetadata.UnmanagedType);
            var cfgIReturnType = CodeBuilder.BuildCfgITypeName(qualifiedUnmanagedTypeName);
            
            _builder.AppendXmlComment(
                "获取索引对应的配置引用列表(多值索引)",
                new Dictionary<string, string>
                {
                    { "index", "索引值" },
                    { "data", "配置数据容器" },
                    { "allocator", "内存分配器" },
                    { "returns", "配置引用 CfgI 数组" }
                }
            );
            
            // 扩展方法参数需要完整类型路径
            var indexTypeName = $"{unmanagedTypeName}.{_indexMetadata.GeneratedStructName}";
            var methodSignature = $"static NativeArray<{cfgIReturnType}> {methodName}(this {indexTypeName} index, in XM.ConfigData data, Allocator allocator)";
            _builder.BeginMethod(methodSignature);
            
            _builder.AppendComment(CodeGenConstants.GetMultiIndexContainerComment);
            _builder.AppendLine(CodeBuilder.BuildGetMultiIndexCall(indexTypeName, qualifiedUnmanagedTypeName));
            _builder.AppendLine($"if (!{CodeGenConstants.IndexMultiMapVar}.{CodeGenConstants.ValidProperty})");
            _builder.BeginBlock();
            _builder.AppendLine($"return new NativeArray<{cfgIReturnType}>(0, allocator);");
            _builder.EndBlock();
            _builder.AppendLine();
            
            _builder.AppendComment(CodeGenConstants.QueryIndexCountComment);
            _builder.AppendLine($"var count = {CodeGenConstants.IndexMultiMapVar}.{CodeGenConstants.GetValueCountMethod}({CodeGenConstants.DataVarBlobContainerAccess}, index);");
            _builder.AppendLine("if (count == 0)");
            _builder.BeginBlock();
            _builder.AppendLine($"return new NativeArray<{cfgIReturnType}>(0, allocator);");
            _builder.EndBlock();
            _builder.AppendLine();
            
            _builder.AppendComment(CodeGenConstants.ConvertToCfgIArrayComment);
            _builder.AppendLine($"var {CodeGenConstants.ResultsVar} = new NativeArray<{cfgIReturnType}>(count, allocator);");
            _builder.AppendLine($"var {CodeGenConstants.LoopIndexVar} = 0;");
            _builder.AppendLine($"foreach (var {CodeGenConstants.IndexLoopCfgIVar} in {CodeGenConstants.IndexMultiMapVar}.{CodeGenConstants.GetValuesPerKeyEnumeratorMethod}({CodeGenConstants.DataVarBlobContainerAccess}, index))");
            _builder.BeginBlock();
            _builder.AppendLine($"{CodeGenConstants.ResultsVar}[{CodeGenConstants.LoopIndexVar}++] = {CodeBuilder.BuildCfgIAsExpression(CodeGenConstants.IndexLoopCfgIVar, qualifiedUnmanagedTypeName)};");
            _builder.EndBlock();
            _builder.AppendLine();
            
            _builder.AppendLine($"return {CodeGenConstants.ResultsVar};");
            
            _builder.EndMethod();
        }
        
        #endregion
        
        #region 扩展方法静态类生成
        
        /// <summary>
        /// 生成扩展方法静态类
        /// </summary>
        private void GenerateExtensionClass()
        {
            // 包含配置类名避免冲突
            var className = $"{_classMetadata.UnmanagedTypeName}_{_indexMetadata.GeneratedStructName}{CodeGenConstants.ExtensionsSuffix}";
            var unmanagedTypeNameSimple = _classMetadata.UnmanagedTypeName;
            
            _builder.AppendLine();
            _builder.AppendXmlComment($"{_indexMetadata.IndexName} 索引扩展方法");
            // 静态类
            _builder.AppendLine($"public static class {className}");
            _builder.BeginBlock();
            
            const string GetValMethodName = "GetVal";
            const string GetValsMethodName = "GetVals";
            
            if (_indexMetadata.IsUnique)
            {
                // 唯一索引: GetVal
                GenerateGetValMethod(unmanagedTypeNameSimple, GetValMethodName);
            }
            else
            {
                // 多值索引: GetVals
                GenerateGetValsMethod(unmanagedTypeNameSimple, GetValsMethodName);
            }
            
            _builder.EndBlock(); // 结束静态类
        }
        
        #endregion
        
    }
}
