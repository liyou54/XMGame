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
            
            // 2. 生成命名空间
            _builder.BeginNamespace(_classMetadata.Namespace);
            
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
            
            // 7. 结束命名空间
            _builder.EndNamespace();
            
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
            var unmanagedTypeName = GetQualifiedTypeName(_classMetadata.UnmanagedType);
            // 实现 IConfigIndexGroup<T> 和 IEquatable<T>
            var interfaceName = $"IConfigIndexGroup<{unmanagedTypeName}>, IEquatable<{structName}>";
            
            _builder.AppendXmlComment($"{_indexMetadata.IndexName} 索引结构体");
            _builder.BeginClass(structName, interfaceName, isPartial: false, isStruct: true);
            
            // 生成索引字段
            GenerateIndexFields();
            
            // 生成构造方法
            GenerateConstructor();
            
            // 生成Equals和GetHashCode方法
            GenerateEquatableMethods();
            
            _builder.EndClass();
        }
        
        #endregion
        
        #region 索引字段生成
        
        /// <summary>
        /// 生成索引字段
        /// </summary>
        private void GenerateIndexFields()
        {
            _builder.AppendComment("索引字段");
            
            foreach (var field in _indexMetadata.IndexFields)
            {
                var fieldType = GetIndexFieldType(field);
                var fieldName = field.FieldName;
                
                _builder.AppendField(fieldType, fieldName, $"索引字段: {fieldName}");
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
            // 优先使用预计算的非托管类型名
            if (!string.IsNullOrEmpty(field.UnmanagedFieldTypeName))
            {
                return field.UnmanagedFieldTypeName;
            }
            
            var typeInfo = field.TypeInfo;
            
            // 枚举类型（使用全局限定名）
            if (typeInfo.IsEnum && typeInfo.SingleValueType != null)
                return GetQualifiedTypeName(typeInfo.SingleValueType);
            
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
            _builder.BeginMethod("int GetHashCode()");
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
                // 唯一索引: GetVal<T>(this index) -> 单个值
                GenerateGetValMethod(unmanagedTypeName, GetValMethodName);
            }
            else
            {
                // 多值索引: GetVals<T>(this index, Allocator) -> NativeArray<T>
                GenerateGetValsMethod(unmanagedTypeName, GetValsMethodName);
            }
        }
        
        /// <summary>
        /// 生成GetVal方法(唯一索引)
        /// </summary>
        private void GenerateGetValMethod(string unmanagedTypeName, string methodName)
        {
            var qualifiedUnmanagedTypeName = GetQualifiedTypeName(_classMetadata.UnmanagedType);
            
            _builder.AppendXmlComment(
                "获取索引对应的配置数据(唯一索引)",
                new Dictionary<string, string>
                {
                    { "index", "索引值" },
                    { "returns", "配置数据" }
                }
            );
            
            // 扩展方法参数需要完整类型路径
            var indexTypeName = $"{unmanagedTypeName}.{_indexMetadata.GeneratedStructName}";
            var methodSignature = $"static {qualifiedUnmanagedTypeName} {methodName}(this {indexTypeName} index)";
            _builder.BeginMethod(methodSignature);
            
            _builder.AppendComment("TODO: 实现索引查询逻辑");
            _builder.AppendLine($"return default({qualifiedUnmanagedTypeName});");
            
            _builder.EndMethod();
        }
        
        /// <summary>
        /// 生成GetVals方法(多值索引)
        /// </summary>
        private void GenerateGetValsMethod(string unmanagedTypeName, string methodName)
        {
            var qualifiedUnmanagedTypeName = GetQualifiedTypeName(_classMetadata.UnmanagedType);
            
            _builder.AppendXmlComment(
                "获取索引对应的配置数据列表(多值索引)",
                new Dictionary<string, string>
                {
                    { "index", "索引值" },
                    { "allocator", "内存分配器" },
                    { "returns", "配置数据数组" }
                }
            );
            
            // 扩展方法参数需要完整类型路径
            var indexTypeName = $"{unmanagedTypeName}.{_indexMetadata.GeneratedStructName}";
            var methodSignature = $"static NativeArray<{qualifiedUnmanagedTypeName}> {methodName}(this {indexTypeName} index, Allocator allocator)";
            _builder.BeginMethod(methodSignature);
            
            _builder.AppendComment("TODO: 实现多值索引查询逻辑");
            _builder.AppendLine($"return new NativeArray<{qualifiedUnmanagedTypeName}>(0, allocator);");
            
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
        
        #region 辅助方法
        
        /// <summary>
        /// 获取全局限定的类型名称（避免命名冲突）
        /// </summary>
        private string GetQualifiedTypeName(Type type)
        {
            if (type == null)
                return "object";
            
            return TypeHelper.GetGlobalQualifiedTypeName(type);
        }
        
        #endregion
    }
}
