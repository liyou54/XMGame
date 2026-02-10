using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using XM.ConfigNew.CodeGen;
using XM.ConfigNew.Metadata;
using XM.Utils.Attribute;

namespace XM.ConfigNew.Tools
{
    /// <summary>
    /// 类型分析器 - 负责分析配置类型并生成元数据
    /// 分两步填充: 1.基本数据 2.依赖数据
    /// </summary>
    public static class TypeAnalyzer
    {
        #region 主入口
        
        /// <summary>
        /// 分析配置类型,生成完整的元数据
        /// </summary>
        /// <param name="configType">配置类型(托管类型)</param>
        /// <returns>配置类元数据</returns>
        public static ConfigClassMetadata AnalyzeConfigType(Type configType)
        {
            if (configType == null)
                throw new ArgumentNullException(nameof(configType));
            
            if (!IsXConfigType(configType))
                throw new ArgumentException($"类型 {configType.Name} 不是 XConfig 类型");
            
            // 创建元数据对象
            var metadata = new ConfigClassMetadata();
            
            // 第一步: 填充基本数据
            FillBasicData(metadata, configType);
            
            // 第二步: 填充依赖数据
            FillDependentData(metadata, configType);
            
            return metadata;
        }
        
        #endregion
        
        #region 第一步: 填充基本数据
        
        /// <summary>
        /// 填充基本数据(类型信息、表信息、程序集信息)
        /// </summary>
        private static void FillBasicData(ConfigClassMetadata metadata, Type configType)
        {
            // 1. 类型信息
            metadata.ManagedType = configType;
            metadata.ManagedTypeName = configType.Name;
            metadata.Namespace = configType.Namespace ?? string.Empty;
            
            // 2. 获取非托管类型
            metadata.UnmanagedType = GetUnmanagedType(configType);
            metadata.UnmanagedTypeName = metadata.UnmanagedType?.Name ?? string.Empty;
            
            // 3. 获取Helper类型
            metadata.HelperTypeName = configType.Name + CodeGenConstants.ClassHelperSuffix;
            metadata.HelperType = GetHelperType(configType);
            
            // 4. 表信息
            var xmlDefinedAttr = configType.GetCustomAttribute<XmlDefinedAttribute>();
            metadata.TableName = !string.IsNullOrEmpty(xmlDefinedAttr?.XmlName) 
                ? xmlDefinedAttr.XmlName 
                : configType.Name;
            
            // 5. Mod信息
            metadata.ModName = GetModNameFromAssembly(configType.Assembly);
            
            // 6. 检测 XmlNested 标记（类级别）
            var xmlNestedAttr = configType.GetCustomAttribute<XmlNestedAttribute>();
            metadata.IsXmlNested = xmlNestedAttr != null;
            
            // 6. 程序集信息
            metadata.Assembly = configType.Assembly;
            metadata.AssemblyName = configType.Assembly.GetName().Name;
            
            // 7. 代码生成所需信息
            metadata.RequiredUsings = new List<string>();
        }
        
        /// <summary>
        /// 从配置类型获取非托管类型
        /// 优先从当前类型直接实现的接口获取，避免获取到父类的 Unmanaged 类型
        /// </summary>
        private static Type GetUnmanagedType(Type configType)
        {
            // 1. 优先从当前类型直接实现的接口查找 IXConfig<T, TUnmanaged>
            var targetInterfaceType = typeof(IXConfig<,>);
            foreach (var iface in configType.GetInterfaces())
            {
                if (iface.IsGenericType && iface.GetGenericTypeDefinition() == targetInterfaceType)
                {
                    var genericArgs = iface.GetGenericArguments();
                    // 确保第一个参数是当前类型（不是父类）
                    if (genericArgs.Length >= 2 && genericArgs[0] == configType)
                    {
                        return genericArgs[1]; // TUnmanaged
                    }
                }
            }
            
            // 2. 如果没找到，从基类链查找（但应该避免这种情况）
            var baseType = configType.BaseType;
            while (baseType != null && baseType.IsGenericType)
            {
                var genericArgs = baseType.GetGenericArguments();
                if (genericArgs.Length >= 2)
                {
                    return genericArgs[1]; // TUnmanaged
                }
                baseType = baseType.BaseType;
            }
            
            return null;
        }
        
        /// <summary>
        /// 获取Helper类型(生成的ClassHelper类)
        /// 通常命名为 {ConfigTypeName}ClassHelper
        /// </summary>
        private static Type GetHelperType(Type configType)
        {
            // 在同一程序集中查找 {TypeName}ClassHelper
            var helperTypeName = configType.Name + CodeGenConstants.ClassHelperSuffix;
            var helperType = configType.Assembly.GetType($"{configType.Namespace}.{helperTypeName}");
            
            return helperType;
        }
        
        /// <summary>
        /// 从程序集获取Mod名称
        /// 查找 [ModName] 特性
        /// </summary>
        private static string GetModNameFromAssembly(Assembly assembly)
        {
            if (assembly == null)
                return "Default";
            
            try
            {
                // 查找 ModNameAttribute
                var attrType = assembly.GetType(CodeGenConstants.ModNameAttributeTypeName);
                if (attrType == null)
                {
                    // 在所有程序集中查找
                    foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        attrType = a.GetType(CodeGenConstants.ModNameAttributeTypeName);
                        if (attrType != null) break;
                    }
                }
                
                if (attrType == null)
                    return CodeGenConstants.DefaultModName;
                
                var attr = System.Attribute.GetCustomAttribute(assembly, attrType);
                if (attr == null)
                    return CodeGenConstants.DefaultModName;
                
                var prop = attrType.GetProperty(CodeGenConstants.ModNamePropertyName);
                var modName = prop?.GetValue(attr) as string;
                
                return !string.IsNullOrEmpty(modName) ? modName : CodeGenConstants.DefaultModName;
            }
            catch
            {
                return CodeGenConstants.DefaultModName;
            }
        }
        
        #endregion
        
        #region 第二步: 填充依赖数据
        
        /// <summary>
        /// 填充依赖数据(字段、索引、Link)
        /// </summary>
        private static void FillDependentData(ConfigClassMetadata metadata, Type configType)
        {
            // 1. 分析字段(不包含索引信息)
            metadata.Fields = AnalyzeFields(configType, metadata);
            
            // 2. 分析索引(并更新字段的索引信息)
            metadata.Indexes = AnalyzeIndexes(configType, metadata);
            
            // 3. 分析Link关系
            metadata.Link = AnalyzeLink(configType, metadata);
            
            // 4. 构建快速查找表
            BuildLookupTables(metadata);
            
            // 5. 收集代码生成所需的using列表
            CollectRequiredUsings(metadata);
            
            // 6. 验证配置类的有效性
            ValidateConfigClass(metadata);
        }
        
        /// <summary>
        /// 收集代码生成所需的using命名空间
        /// </summary>
        private static void CollectRequiredUsings(ConfigClassMetadata metadata)
        {
            var usings = new HashSet<string>
            {
                "System",
                "System.Collections.Generic",
                "System.Xml",
                "XM",
                "XM.Contracts",
                "XM.Contracts.Config",
                "Unity.Collections"
            };
            
            // 添加配置类自己的命名空间
            if (!string.IsNullOrEmpty(metadata.Namespace))
                usings.Add(metadata.Namespace);
            
            // 遍历字段，收集需要的命名空间
            if (metadata.Fields != null)
            {
                foreach (var field in metadata.Fields)
                {
                    // 嵌套配置的命名空间
                    if (field.TypeInfo?.IsNestedConfig == true && field.TypeInfo.NestedConfigMetadata != null)
                    {
                        var nestedNamespace = field.TypeInfo.NestedConfigMetadata.Namespace;
                        if (!string.IsNullOrEmpty(nestedNamespace))
                            usings.Add(nestedNamespace);
                    }
                    
                    // Link目标类型的命名空间
                    if (field.IsXmlLink && field.XmlLinkTargetType != null)
                    {
                        var linkNamespace = field.XmlLinkTargetType.Namespace;
                        if (!string.IsNullOrEmpty(linkNamespace))
                            usings.Add(linkNamespace);
                    }
                }
            }
            
            metadata.RequiredUsings = usings.OrderBy(u => u).ToList();
        }
        
        /// <summary>
        /// 分析所有字段（包括继承的字段）
        /// </summary>
        private static List<ConfigFieldMetadata> AnalyzeFields(Type configType, ConfigClassMetadata classMetadata)
        {
            var fields = new List<ConfigFieldMetadata>();
            
            // 获取所有公共实例字段（包括继承的）
            // 注意: GetFields 默认会返回继承的字段，但为了确保，我们使用 FlattenHierarchy
            var fieldInfos = configType.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            
            // 去重: 子类字段覆盖父类同名字段
            var fieldDict = new Dictionary<string, FieldInfo>();
            foreach (var fieldInfo in fieldInfos)
            {
                // 只保留声明在当前类型或其父配置类型中的字段
                if (IsXConfigType(fieldInfo.DeclaringType))
                {
                    fieldDict[fieldInfo.Name] = fieldInfo;
                }
            }
            
            foreach (var fieldInfo in fieldDict.Values)
            {
                var fieldMetadata = AnalyzeField(fieldInfo, classMetadata);
                if (fieldMetadata != null)
                    fields.Add(fieldMetadata);
            }
            
            return fields;
        }
        
        /// <summary>
        /// 分析单个字段
        /// </summary>
        private static ConfigFieldMetadata AnalyzeField(FieldInfo fieldInfo, ConfigClassMetadata classMetadata)
        {
            var fieldMetadata = new ConfigFieldMetadata
            {
                FieldName = fieldInfo.Name,
                FieldReflectionInfo = fieldInfo,
                SourceComment = ExtractFieldComment(fieldInfo)
            };
            
            // 1. 分析字段类型信息
            fieldMetadata.TypeInfo = AnalyzeFieldType(fieldInfo, classMetadata);
            
            // 2. 分析转换器信息
            fieldMetadata.Converter = AnalyzeConverter(fieldInfo, classMetadata);
            
            // 3. 分析XML解析规则
            AnalyzeXmlParseRules(fieldInfo, fieldMetadata);
            
            // 4. 初始化索引信息(后续在AnalyzeIndexes中填充)
            fieldMetadata.IsIndexField = false;
            fieldMetadata.IndexNames = new List<(int, ConfigIndexMetadata)>();
            
            // 5. 分析Link信息
            AnalyzeLinkInfo(fieldInfo, fieldMetadata);
            
            // 6. 预计算代码生成所需字段
            FillCodeGenFields(fieldMetadata, classMetadata);
            
            return fieldMetadata;
        }
        
        /// <summary>
        /// 填充代码生成预计算字段
        /// </summary>
        private static void FillCodeGenFields(ConfigFieldMetadata fieldMetadata, ConfigClassMetadata classMetadata)
        {
            var fieldName = fieldMetadata.FieldName;
            
            // 方法名称
            fieldMetadata.ParseMethodName = "Parse" + fieldName;
            fieldMetadata.AllocMethodName = "Alloc" + fieldName;
            fieldMetadata.FillMethodName = "Fill" + fieldName;
            
            // 托管类型名称（使用全局限定名避免冲突）
            fieldMetadata.ManagedFieldTypeName = TypeHelper.GetGlobalQualifiedTypeName(fieldMetadata.TypeInfo?.ManagedFieldType);
            
            // 非托管类型名称
            if (fieldMetadata.TypeInfo != null)
            {
                // 注意: 可空类型需要优先处理，然后再判断其他类型
                Type targetType = fieldMetadata.TypeInfo.ManagedFieldType;
                
                // 如果是可空类型，使用基础类型进行后续判断
                if (fieldMetadata.TypeInfo.IsNullable && fieldMetadata.TypeInfo.UnderlyingType != null)
                {
                    targetType = fieldMetadata.TypeInfo.UnderlyingType;
                }
                
                // 使用统一的字段类型获取方法（避免硬编码和遗漏）
                fieldMetadata.UnmanagedFieldTypeName = TypeHelper.GetUnmanagedFieldTypeName(fieldMetadata, GetStringModeTypeNameQualified);
            }
        }
        
        /// <summary>
        /// 根据字符串模式获取全局限定的类型名称
        /// 使用 Type 对象避免硬编码
        /// </summary>
        private static string GetStringModeTypeNameQualified(EXmlStrMode mode)
        {
            switch (mode)
            {
                case EXmlStrMode.EFix32:
                    return TypeHelper.GetGlobalQualifiedTypeName(typeof(Unity.Collections.FixedString32Bytes));
                case EXmlStrMode.EFix64:
                    return TypeHelper.GetGlobalQualifiedTypeName(typeof(Unity.Collections.FixedString64Bytes));
                case EXmlStrMode.ELabelI:
                    return TypeHelper.GetGlobalQualifiedTypeName(typeof(LabelI));
                case EXmlStrMode.EStrI:
                default:
                    // 默认使用 StrI，这是最常见的字符串索引类型
                    return TypeHelper.GetGlobalQualifiedTypeName(typeof(StrI));
            }
        }
        
        /// <summary>
        /// 根据字符串模式获取类型名称
        /// </summary>
        private static string GetStringModeTypeName(EXmlStrMode mode)
        {
            switch (mode)
            {
                case EXmlStrMode.EFix32:
                    return CodeGenConstants.FixedString32TypeName;
                case EXmlStrMode.EFix64:
                    return CodeGenConstants.FixedString64TypeName;
                case EXmlStrMode.EStrI:
                    return CodeGenConstants.StrITypeName;
                case EXmlStrMode.ELabelI:
                    return CodeGenConstants.LabelITypeName;
                default:
                    return CodeGenConstants.StrITypeName;
            }
        }
        
        /// <summary>
        /// 分析字段类型信息
        /// </summary>
        private static FieldTypeInfo AnalyzeFieldType(FieldInfo fieldInfo, ConfigClassMetadata classMetadata)
        {
            var fieldType = fieldInfo.FieldType;
            var typeInfo = new FieldTypeInfo
            {
                ManagedFieldType = fieldType
            };
            
            // 1. 检查可空类型
            if (TypeHelper.IsNullableType(fieldType))
            {
                typeInfo.IsNullable = true;
                typeInfo.UnderlyingType = Nullable.GetUnderlyingType(fieldType);
                fieldType = typeInfo.UnderlyingType; // 使用基础类型继续分析
            }
            
            // 2. 检查枚举类型
            if (fieldType.IsEnum)
            {
                typeInfo.IsEnum = true;
                typeInfo.EnumUnderlyingType = Enum.GetUnderlyingType(fieldType);
                typeInfo.EnumValueNames = Enum.GetNames(fieldType).ToList();
                typeInfo.EnumValues = new Dictionary<string, object>();
                foreach (var name in typeInfo.EnumValueNames)
                {
                    typeInfo.EnumValues[name] = Enum.Parse(fieldType, name);
                }
            }
            
            // 3. 自定义解析器在AnalyzeConverter中处理
            
            // 4. 分析容器类型和嵌套结构
            AnalyzeContainerType(fieldType, typeInfo, 0);
            
            // TODO: 分析非托管类型(需要类型映射)
            typeInfo.UnmanagedFieldType = null; // 暂时为null,后续实现
            
            return typeInfo;
        }
        
        /// <summary>
        /// 递归分析容器类型
        /// </summary>
        private static void AnalyzeContainerType(Type fieldType, FieldTypeInfo typeInfo, int level)
        {
            // 检查是否是容器类型
            if (TypeHelper.IsListType(fieldType))
            {
                typeInfo.ContainerType = EContainerType.List;
                
                var elementType = fieldType.GetGenericArguments()[0];
                typeInfo.NestedValueType = elementType;
                typeInfo.IsValueContainer = TypeHelper.IsContainerType(elementType);
                
                // 递归分析嵌套容器
                if (typeInfo.IsValueContainer)
                {
                    typeInfo.NestedValueTypeInfo = new FieldTypeInfo
                    {
                        ManagedFieldType = elementType
                    };
                    AnalyzeContainerType(elementType, typeInfo.NestedValueTypeInfo, level + 1);
                    // 从嵌套容器中获取最终元素类型和嵌套层级
                    typeInfo.SingleValueType = typeInfo.NestedValueTypeInfo.SingleValueType;
                    typeInfo.IsNestedConfig = typeInfo.NestedValueTypeInfo.IsNestedConfig;
                    typeInfo.NestedLevel = typeInfo.NestedValueTypeInfo.NestedLevel + 1;
                }
                else
                {
                    typeInfo.SingleValueType = elementType;
                    typeInfo.IsNestedConfig = IsXConfigType(elementType);
                    typeInfo.NestedLevel = 1; // 单层容器
                    
                    // 递归分析嵌套配置
                    if (typeInfo.IsNestedConfig)
                    {
                        typeInfo.NestedConfigMetadata = AnalyzeConfigType(elementType);
                    }
                }
            }
            else if (TypeHelper.IsDictionaryType(fieldType))
            {
                typeInfo.ContainerType = EContainerType.Dictionary;
                
                var genericArgs = fieldType.GetGenericArguments();
                typeInfo.NestedKeyType = genericArgs[0];
                typeInfo.NestedValueType = genericArgs[1];
                
                typeInfo.IsKeyContainer = TypeHelper.IsContainerType(typeInfo.NestedKeyType);
                typeInfo.IsValueContainer = TypeHelper.IsContainerType(typeInfo.NestedValueType);
                
                // 递归分析Value的嵌套容器
                if (typeInfo.IsValueContainer)
                {
                    typeInfo.NestedValueTypeInfo = new FieldTypeInfo
                    {
                        ManagedFieldType = typeInfo.NestedValueType
                    };
                    AnalyzeContainerType(typeInfo.NestedValueType, typeInfo.NestedValueTypeInfo, level + 1);
                    // 从嵌套容器中获取最终元素类型和嵌套层级
                    typeInfo.SingleValueType = typeInfo.NestedValueTypeInfo.SingleValueType;
                    typeInfo.IsNestedConfig = typeInfo.NestedValueTypeInfo.IsNestedConfig;
                    typeInfo.NestedLevel = typeInfo.NestedValueTypeInfo.NestedLevel + 1;
                }
                else
                {
                    typeInfo.SingleValueType = typeInfo.NestedValueType;
                    typeInfo.IsNestedConfig = IsXConfigType(typeInfo.NestedValueType);
                    typeInfo.NestedLevel = 1; // 单层容器
                    
                    // 递归分析嵌套配置
                    if (typeInfo.IsNestedConfig)
                    {
                        typeInfo.NestedConfigMetadata = AnalyzeConfigType(typeInfo.NestedValueType);
                    }
                }
            }
            else if (TypeHelper.IsHashSetType(fieldType))
            {
                typeInfo.ContainerType = EContainerType.HashSet;
                
                var elementType = fieldType.GetGenericArguments()[0];
                typeInfo.NestedValueType = elementType;
                typeInfo.IsValueContainer = TypeHelper.IsContainerType(elementType);
                
                // 递归分析嵌套容器
                if (typeInfo.IsValueContainer)
                {
                    typeInfo.NestedValueTypeInfo = new FieldTypeInfo
                    {
                        ManagedFieldType = elementType
                    };
                    AnalyzeContainerType(elementType, typeInfo.NestedValueTypeInfo, level + 1);
                    // 从嵌套容器中获取最终元素类型和嵌套层级
                    typeInfo.SingleValueType = typeInfo.NestedValueTypeInfo.SingleValueType;
                    typeInfo.IsNestedConfig = typeInfo.NestedValueTypeInfo.IsNestedConfig;
                    typeInfo.NestedLevel = typeInfo.NestedValueTypeInfo.NestedLevel + 1;
                }
                else
                {
                    typeInfo.SingleValueType = elementType;
                    typeInfo.IsNestedConfig = IsXConfigType(elementType);
                    typeInfo.NestedLevel = 1; // 单层容器
                    
                    // 递归分析嵌套配置
                    if (typeInfo.IsNestedConfig)
                    {
                        typeInfo.NestedConfigMetadata = AnalyzeConfigType(elementType);
                    }
                }
            }
            else
            {
                // 非容器类型
                typeInfo.ContainerType = EContainerType.None;
                typeInfo.NestedLevel = 0;
                typeInfo.SingleValueType = fieldType;
                typeInfo.IsNestedConfig = IsXConfigType(fieldType);
                
                // 递归分析嵌套配置
                if (typeInfo.IsNestedConfig)
                {
                    typeInfo.NestedConfigMetadata = AnalyzeConfigType(fieldType);
                }
            }
            
            // 构建嵌套容器链
            if (typeInfo.NestedLevel > 0)
            {
                typeInfo.NestedContainerChain = new List<EContainerType> { typeInfo.ContainerType };
                
                var current = typeInfo.NestedValueTypeInfo;
                while (current != null && current.ContainerType != EContainerType.None)
                {
                    typeInfo.NestedContainerChain.Add(current.ContainerType);
                    current = current.NestedValueTypeInfo;
                }
            }
        }
        
        /// <summary>
        /// 分析转换器信息
        /// </summary>
        private static ConverterInfo AnalyzeConverter(FieldInfo fieldInfo, ConfigClassMetadata classMetadata)
        {
            var converterInfo = new ConverterInfo
            {
                SourceType = typeof(string), // 默认从XML字符串转换
                TargetType = fieldInfo.FieldType,
                Registrations = new List<ConverterRegistration>()
            };
            
            // 1. 字段级转换器 [XmlTypeConverter] (支持多个,用于容器Key/Value)
            var typeConverterAttrs = fieldInfo.GetCustomAttributes<XmlTypeConverterAttribute>();
            foreach (var attr in typeConverterAttrs)
            {
                if (attr.ConverterType == null)
                    continue;
                
                var isGlobal = attr.BGlobal;
                var priority = isGlobal ? CodeGenConstants.GlobalConverterPriority : CodeGenConstants.FieldConverterPriority;
                var location = isGlobal ? ConverterDefinitionLocation.GlobalAssembly : ConverterDefinitionLocation.Field;
                
                converterInfo.Registrations.Add(new ConverterRegistration
                {
                    ConverterType = attr.ConverterType,
                    IsGlobal = isGlobal,
                    Location = location,
                    Priority = priority
                });
            }
            
            // 2. 程序集级 [assembly: XmlTypeConverter(typeof(Xxx), true)] (BGlobal=true 时生效)
            CollectAssemblyXmlTypeConverters(converterInfo, fieldInfo);
            
            // 3. Mod级转换器 - 从当前程序集查找 [assembly: XmlGlobalConvert(..., "ModName")]
            CollectModLevelConverters(converterInfo, classMetadata);
            
            // 4. 全局级转换器 - 从所有程序集查找 [assembly: XmlGlobalConvert(..., "")]
            CollectGlobalLevelConverters(converterInfo, classMetadata);
            
            // 按优先级排序
            converterInfo.Registrations.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            
            return converterInfo;
        }
        
        /// <summary>
        /// 收集Mod级转换器
        /// 查找当前配置类所在程序集的 [assembly: XmlGlobalConvert(..., "ModName")]
        /// </summary>
        private static void CollectModLevelConverters(ConverterInfo converterInfo, ConfigClassMetadata classMetadata)
        {
            if (classMetadata.Assembly == null)
                return;
            
            var modName = classMetadata.ModName;
            if (string.IsNullOrEmpty(modName) || modName == CodeGenConstants.DefaultModName)
                return;
            
            // 查找 XmlGlobalConvertAttribute 类型
            var attrType = FindXmlGlobalConvertAttributeType();
            if (attrType == null)
                return;
            
            // 获取当前程序集上的所有 XmlGlobalConvertAttribute
            var attrs = System.Attribute.GetCustomAttributes(classMetadata.Assembly, attrType);
            
            foreach (var attr in attrs)
            {
                // 获取 ConverterType 和 Domain 属性
                var converterTypeProp = attrType.GetProperty("ConverterType");
                var domainProp = attrType.GetProperty("Domain");
                
                if (converterTypeProp == null || domainProp == null)
                    continue;
                
                var converterType = converterTypeProp.GetValue(attr) as Type;
                var domain = domainProp.GetValue(attr) as string;
                
                // 只收集 Domain == ModName 的转换器
                if (converterType != null && domain == modName)
                {
                    converterInfo.Registrations.Add(new ConverterRegistration
                    {
                        ConverterType = converterType,
                        IsGlobal = false,
                        Location = ConverterDefinitionLocation.ModAssembly,
                        Priority = CodeGenConstants.ModConverterPriority
                    });
                }
            }
        }
        
        /// <summary>
        /// 收集全局级转换器
        /// 查找所有程序集的 [assembly: XmlGlobalConvert(..., "")]
        /// </summary>
        private static void CollectGlobalLevelConverters(ConverterInfo converterInfo, ConfigClassMetadata classMetadata)
        {
            // 查找 XmlGlobalConvertAttribute 类型
            var attrType = FindXmlGlobalConvertAttributeType();
            if (attrType == null)
                return;
            
            // 遍历所有程序集
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            
            foreach (var assembly in assemblies)
            {
                try
                {
                    // 获取程序集上的所有 XmlGlobalConvertAttribute
                    var attrs = System.Attribute.GetCustomAttributes(assembly, attrType);
                    
                    foreach (var attr in attrs)
                    {
                        // 获取 ConverterType 和 Domain 属性
                        var converterTypeProp = attrType.GetProperty("ConverterType");
                        var domainProp = attrType.GetProperty("Domain");
                        
                        if (converterTypeProp == null || domainProp == null)
                            continue;
                        
                        var converterType = converterTypeProp.GetValue(attr) as Type;
                        var domain = domainProp.GetValue(attr) as string;
                        
                        // 只收集 Domain 为空的全局转换器
                        if (converterType != null && string.IsNullOrEmpty(domain))
                        {
                            converterInfo.Registrations.Add(new ConverterRegistration
                            {
                                ConverterType = converterType,
                                IsGlobal = true,
                                Location = ConverterDefinitionLocation.GlobalAssembly,
                                Priority = CodeGenConstants.GlobalConverterPriority
                            });
                        }
                    }
                }
                catch
                {
                    // 忽略无法访问的程序集
                }
            }
        }
        
        /// <summary>
        /// 查找 XmlGlobalConvertAttribute 类型
        /// </summary>
        private static Type FindXmlGlobalConvertAttributeType()
        {
            // 先从常见命名空间查找
            var attrType = Type.GetType("XM.Utils.Attribute.XmlGlobalConvertAttribute");
            if (attrType != null)
                return attrType;
            
            // 在所有程序集中查找
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    attrType = assembly.GetType("XM.Utils.Attribute.XmlGlobalConvertAttribute");
                    if (attrType != null)
                        return attrType;
                }
                catch
                {
                    // 忽略无法访问的程序集
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// 收集程序集级 [assembly: XmlTypeConverter(typeof(Xxx), true)] 转换器
        /// 1. XML 解析: string -> fieldType 的转换器加入 Registrations
        /// 2. 非托管转换: fieldType -> UnmanagedType 的转换器设置 UnmanagedTargetType/UnmanagedConverterType
        /// </summary>
        private static void CollectAssemblyXmlTypeConverters(ConverterInfo converterInfo, FieldInfo fieldInfo)
        {
            var fieldType = fieldInfo.FieldType;
            // 处理可空类型
            if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var args = fieldType.GetGenericArguments();
                if (args.Length > 0)
                    fieldType = args[0];
            }
            
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                try
                {
                    var attrs = System.Attribute.GetCustomAttributes(assembly, typeof(XmlTypeConverterAttribute));
                    foreach (XmlTypeConverterAttribute attr in attrs)
                    {
                        if (attr.ConverterType == null || !attr.BGlobal)
                            continue;
                        
                        var converterType = attr.ConverterType;
                        foreach (var i in converterType.GetInterfaces())
                        {
                            if (!i.IsGenericType)
                                continue;
                            var genDef = i.GetGenericTypeDefinition();
                            if (genDef.Name != "ITypeConverter`2")
                                continue;
                            
                            var args = i.GetGenericArguments();
                            if (args.Length != 2)
                                continue;
                            var sourceType = args[0];
                            var targetType = args[1];
                            
                            // XML 解析: string -> fieldType
                            if (sourceType == typeof(string) && targetType == fieldType)
                            {
                                converterInfo.Registrations.Add(new ConverterRegistration
                                {
                                    ConverterType = converterType,
                                    IsGlobal = true,
                                    Location = ConverterDefinitionLocation.GlobalAssembly,
                                    Priority = CodeGenConstants.GlobalConverterPriority
                                });
                                break;
                            }
                            
                            // 非托管转换: fieldType -> UnmanagedType（用于 Unmanaged 结构体字段类型推导）
                            if (sourceType == fieldType && converterInfo.UnmanagedTargetType == null)
                            {
                                converterInfo.UnmanagedTargetType = targetType;
                                converterInfo.UnmanagedConverterType = converterType;
                            }
                        }
                    }
                }
                catch
                {
                    // 忽略无法访问的程序集
                }
            }
        }
        
        /// <summary>
        /// 分析XML解析规则
        /// </summary>
        private static void AnalyzeXmlParseRules(FieldInfo fieldInfo, ConfigFieldMetadata fieldMetadata)
        {
            // [XmlNotNull]
            fieldMetadata.IsNotNull = fieldInfo.GetCustomAttribute<XmlNotNullAttribute>() != null;
            
            // [XmlDefault]
            var defaultAttr = fieldInfo.GetCustomAttribute<XmlDefaultAttribute>();
            fieldMetadata.DefaultValue = defaultAttr?.Value;
            
            // 容器默认值分隔符(默认逗号)
            fieldMetadata.DefaultValueSeparator = CodeGenConstants.DefaultValueSeparator;
            
            // [XmlStringMode]
            var stringModeAttr = fieldInfo.GetCustomAttribute<XmlStringModeAttribute>();
            if (stringModeAttr != null)
            {
                // 通过反射获取私有字段 StrMode
                var strModeField = typeof(XmlStringModeAttribute).GetField(CodeGenConstants.StrModeFieldName, 
                    BindingFlags.NonPublic | BindingFlags.Instance);
                if (strModeField != null)
                {
                    fieldMetadata.StringMode = (EXmlStrMode)strModeField.GetValue(stringModeAttr);
                }
            }
            
            // [XmlDefined]
            var xmlDefinedAttr = fieldInfo.GetCustomAttribute<XmlDefinedAttribute>();
            fieldMetadata.XmlName = xmlDefinedAttr?.XmlName;
            
            // [XmlKey]
            fieldMetadata.IsXmlKey = fieldInfo.GetCustomAttribute<XmlKeyAttribute>() != null;
            
            // [XmlNested] - 在字段级别检测
            fieldMetadata.IsXmlNested = fieldInfo.GetCustomAttribute<XmlNestedAttribute>() != null;
        }
        
        /// <summary>
        /// 分析Link信息
        /// </summary>
        private static void AnalyzeLinkInfo(FieldInfo fieldInfo, ConfigFieldMetadata fieldMetadata)
        {
            var xmlLinkAttr = fieldInfo.GetCustomAttribute<XMLLinkAttribute>();
            if (xmlLinkAttr != null)
            {
                fieldMetadata.IsXmlLink = true;
                fieldMetadata.IsUniqueLinkToParent = xmlLinkAttr.IsUnique;
                
                // 获取Link目标类型(父节点类型，从 CfgS<T> 中提取 T)
                // 只支持单个Link: CfgS<T> → 提取 T
                var fieldType = fieldInfo.FieldType;
                var cfgSType = ExtractCfgSType(fieldType);
                
                if (cfgSType != null && cfgSType.IsGenericType)
                {
                    var genericArgs = cfgSType.GetGenericArguments();
                    if (genericArgs.Length > 0)
                    {
                        fieldMetadata.XmlLinkTargetType = genericArgs[0];
                        // 设置自动生成的索引名称
                        fieldMetadata.ParentLinkIndexName = TypeHelper.GenerateParentLinkIndexName(genericArgs[0].Name);
                    }
                }
            }
        }
        
        /// <summary>
        /// 从字段类型中提取 CfgS&lt;T&gt; 类型
        /// 支持: CfgS&lt;T&gt;, List&lt;CfgS&lt;T&gt;&gt;, HashSet&lt;CfgS&lt;T&gt;&gt; 等
        /// </summary>
        private static Type ExtractCfgSType(Type fieldType)
        {
            if (fieldType == null || !fieldType.IsGenericType)
                return null;
            
            var genericTypeDef = fieldType.GetGenericTypeDefinition();
            
            // 情况1: 直接是 CfgS<T>
            if (genericTypeDef.Name == typeof(Contracts.Config.CfgS<>).Name)
            {
                return fieldType;
            }
            
            // 情况2: 是容器类型 List<CfgS<T>>, HashSet<CfgS<T>> 等
            if (TypeHelper.IsListType(fieldType) || TypeHelper.IsHashSetType(fieldType))
            {
                var elementType = fieldType.GetGenericArguments()[0];
                if (elementType.IsGenericType && 
                    elementType.GetGenericTypeDefinition().Name == typeof(Contracts.Config.CfgS<>).Name)
                {
                    return elementType;
                }
            }
            
            // 情况3: Dictionary<K, CfgS<T>> 的 Value 是 CfgS<T>
            if (TypeHelper.IsDictionaryType(fieldType))
            {
                var genericArgs = fieldType.GetGenericArguments();
                if (genericArgs.Length >= 2)
                {
                    var valueType = genericArgs[1];
                    if (valueType.IsGenericType && 
                        valueType.GetGenericTypeDefinition().Name == typeof(Contracts.Config.CfgS<>).Name)
                    {
                        return valueType;
                    }
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// 分析索引
        /// </summary>
        private static List<ConfigIndexMetadata> AnalyzeIndexes(Type configType, ConfigClassMetadata classMetadata)
        {
            var indexes = new Dictionary<string, ConfigIndexMetadata>();
            var fieldIndexInfos = new Dictionary<string, List<(int Position, string IndexName)>>();
            
            // 第一遍: 收集所有索引信息
            foreach (var field in classMetadata.Fields)
            {
                var indexAttrs = field.FieldReflectionInfo.GetCustomAttributes<XmlIndexAttribute>().ToList();
                
                if (indexAttrs.Count > 0)
                {
                    field.IsIndexField = true;
                    fieldIndexInfos[field.FieldName] = new List<(int, string)>();
                }
                
                foreach (var indexAttr in indexAttrs)
                {
                    var indexName = indexAttr.IndexName;
                    
                    // 记录字段的索引信息
                    fieldIndexInfos[field.FieldName].Add((indexAttr.Position, indexName));
                    
                    // 获取或创建索引元数据
                    if (!indexes.TryGetValue(indexName, out var indexMetadata))
                    {
                        indexMetadata = new ConfigIndexMetadata
                        {
                            IndexName = indexName,
                            IsMultiValue = indexAttr.IsMultiValue,
                            IsUnique = !indexAttr.IsMultiValue, // 非多值即为唯一
                            IndexFields = new List<ConfigFieldMetadata>(),
                            GeneratedStructName = indexName + "Index",
                            GeneratedQueryMethodName = "GetBy" + indexName
                        };
                        indexes[indexName] = indexMetadata;
                    }
                }
            }
            
            // 第二遍: 填充索引的字段列表(按Position排序)
            foreach (var index in indexes.Values)
            {
                var fieldsWithPosition = new List<(ConfigFieldMetadata Field, int Position)>();
                
                foreach (var field in classMetadata.Fields)
                {
                    if (fieldIndexInfos.TryGetValue(field.FieldName, out var indexInfos))
                    {
                        var indexInfo = indexInfos.FirstOrDefault(i => i.IndexName == index.IndexName);
                        if (indexInfo.IndexName != null)
                        {
                            fieldsWithPosition.Add((field, indexInfo.Position));
                        }
                    }
                }
                
                // 按Position排序
                index.IndexFields = fieldsWithPosition
                    .OrderBy(fp => fp.Position)
                    .Select(fp => fp.Field)
                    .ToList();
            }
            
            // 第三遍: 更新字段的IndexNames
            foreach (var field in classMetadata.Fields)
            {
                if (fieldIndexInfos.TryGetValue(field.FieldName, out var indexInfos))
                {
                    foreach (var (position, indexName) in indexInfos)
                    {
                        if (indexes.TryGetValue(indexName, out var indexMetadata))
                        {
                            field.IndexNames.Add((position, indexMetadata));
                        }
                    }
                }
            }
            
            // 第四遍: 为所有 XMLLink 字段自动生成索引（父节点 -> 子节点）
            foreach (var field in classMetadata.Fields.Where(f => f.IsXmlLink))
            {
                var indexName = field.ParentLinkIndexName;
                if (string.IsNullOrEmpty(indexName))
                    continue;
                
                // 避免重复生成（如果用户手动定义了同名索引）
                if (indexes.ContainsKey(indexName))
                    continue;
                
                // 创建一个虚拟字段元数据，代表 XMLLink 字段（它本身就会变成 CfgI 类型）
                var parentCfgIField = new ConfigFieldMetadata
                {
                    FieldName = field.FieldName, // 直接使用原字段名
                    TypeInfo = new FieldTypeInfo
                    {
                        ManagedFieldType = typeof(object), // 占位，实际类型为 CfgI<ParentType>
                        UnmanagedFieldType = typeof(object),
                        ContainerType = EContainerType.None // 非容器类型
                    }
                };
                
                // 创建自动生成的索引元数据
                var autoIndex = new ConfigIndexMetadata
                {
                    IndexName = indexName,
                    IsUnique = field.IsUniqueLinkToParent,
                    IsMultiValue = !field.IsUniqueLinkToParent,
                    IndexFields = new List<ConfigFieldMetadata> { parentCfgIField },
                    GeneratedStructName = indexName + "Index",
                    GeneratedQueryMethodName = "GetBy" + indexName,
                    IsAutoGeneratedForLink = true
                };
                
                indexes[indexName] = autoIndex;
            }
            
            return indexes.Values.ToList();
        }
        
        /// <summary>
        /// 分析Link关系
        /// </summary>
        private static ConfigLinkMetadata AnalyzeLink(Type configType, ConfigClassMetadata classMetadata)
        {
            var linkMetadata = new ConfigLinkMetadata();
            
            // 1. 收集Link字段
            linkMetadata.LinkFields = classMetadata.Fields
                .Where(f => f.IsXmlLink)
                .ToList();
            
            // 2. IsMultiLink 在新设计中已废弃（唯一性现在是字段级别的 IsUniqueLinkToParent）
            linkMetadata.IsMultiLink = false; // 保留字段以保持兼容性，但不再使用
            
            // 3. SubLink信息(反向引用)暂时为空,需要在所有类型分析完成后填充
            linkMetadata.SubLinkTypes = new List<Type>();
            linkMetadata.SubLinkHelperTypes = new List<Type>();
            
            return linkMetadata;
        }
        
        /// <summary>
        /// 构建快速查找表
        /// </summary>
        private static void BuildLookupTables(ConfigClassMetadata metadata)
        {
            // 构建字段查找表
            if (metadata.Fields != null)
            {
                metadata.FieldByName = new Dictionary<string, ConfigFieldMetadata>();
                foreach (var field in metadata.Fields)
                {
                    if (!string.IsNullOrEmpty(field.FieldName))
                        metadata.FieldByName[field.FieldName] = field;
                }
            }
            
            // 构建索引查找表
            if (metadata.Indexes != null)
            {
                metadata.IndexByName = new Dictionary<string, ConfigIndexMetadata>();
                foreach (var index in metadata.Indexes)
                {
                    if (!string.IsNullOrEmpty(index.IndexName))
                        metadata.IndexByName[index.IndexName] = index;
                }
            }
        }
        
        #endregion
        
        #region 程序集扫描
        
        /// <summary>
        /// 查找程序集中所有的配置类型
        /// </summary>
        /// <param name="assembly">要扫描的程序集</param>
        /// <returns>配置类型列表</returns>
        public static List<Type> FindConfigTypesInAssembly(Assembly assembly)
        {
            var configTypes = new List<Type>();
            
            if (assembly == null)
                return configTypes;
            
            try
            {
                var types = assembly.GetTypes();
                foreach (var type in types)
                {
                    if (IsXConfigType(type) && !type.IsAbstract)
                    {
                        configTypes.Add(type);
                    }
                }
            }
            catch (ReflectionTypeLoadException ex)
            {
                // 处理加载异常，返回已成功加载的类型
                if (ex.Types != null)
                {
                    foreach (var type in ex.Types)
                    {
                        if (type != null && IsXConfigType(type) && !type.IsAbstract)
                        {
                            configTypes.Add(type);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[TypeAnalyzer] 扫描程序集失败: {assembly.GetName().Name}, {ex.Message}");
            }
            
            return configTypes;
        }
        
        #endregion
        
        #region 类型判断辅助方法
        
        /// <summary>
        /// 判断是否是XConfig类型
        /// </summary>
        public static bool IsXConfigType(Type type)
        {
            if (type == null || !type.IsClass)
                return false;
            
            // 使用 typeof 避免魔法字符串
            var targetInterface = typeof(IXConfig);
            var targetGenericInterface = typeof(IXConfig<,>);
            
            // 检查是否实现 IXConfig 接口
            return type.GetInterfaces().Any(i => 
                i == targetInterface || 
                (i.IsGenericType && i.GetGenericTypeDefinition() == targetGenericInterface));
        }
        
        /// <summary>
        /// 从配置类型获取 Unmanaged 类型（从 IXConfig&lt;T, TUnmanaged&gt; 泛型参数获取）
        /// 这是正确的方式，避免通过类型名拼接导致的大小写和命名问题
        /// </summary>
        /// <param name="configType">配置类型（Managed）</param>
        /// <returns>Unmanaged 类型，如果获取失败返回 null</returns>
        public static Type GetUnmanagedTypeFromConfig(Type configType)
        {
            if (configType == null || !IsXConfigType(configType))
                return null;
            
            // 1. 检查是否实现 IXConfig<T, TUnmanaged> 接口
            foreach (var iface in configType.GetInterfaces())
            {
                if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IXConfig<,>))
                {
                    var genericArgs = iface.GetGenericArguments();
                    if (genericArgs.Length >= 2)
                    {
                        return genericArgs[1]; // TUnmanaged
                    }
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// 从程序集 [assembly: XmlTypeConverter(typeof(Xxx), true)] 推导托管类型的非托管目标类型
        /// 查找 ITypeConverter&lt;managedType, unmanagedType&gt; 转换器，返回 unmanagedType
        /// </summary>
        /// <param name="managedType">托管类型（如 XAssetPath）</param>
        /// <returns>非托管目标类型，未找到则返回 null</returns>
        public static Type GetUnmanagedTypeFromXmlTypeConverter(Type managedType)
        {
            if (managedType == null)
                return null;
            
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var attrs = System.Attribute.GetCustomAttributes(assembly, typeof(XmlTypeConverterAttribute));
                    foreach (XmlTypeConverterAttribute attr in attrs)
                    {
                        if (attr.ConverterType == null || !attr.BGlobal)
                            continue;
                        
                        foreach (var i in attr.ConverterType.GetInterfaces())
                        {
                            if (!i.IsGenericType)
                                continue;
                            if (i.GetGenericTypeDefinition().Name != "ITypeConverter`2")
                                continue;
                            
                            var args = i.GetGenericArguments();
                            if (args.Length != 2)
                                continue;
                            if (args[0] == managedType)
                                return args[1]; // 找到 ManagedType -> UnmanagedType
                        }
                    }
                }
                catch
                {
                    // 忽略无法访问的程序集
                }
            }
            return null;
        }
        
        #endregion
        
        #region 配置类验证
        
        /// <summary>
        /// 验证配置类的有效性（编译时检查）
        /// </summary>
        private static void ValidateConfigClass(ConfigClassMetadata metadata)
        {
            if (metadata.IsXmlNested)
            {
                // 嵌套容器：允许有或没有 XmlKey（通常不需要）
                return;
            }
            
            // 非嵌套容器：必须有至少一个 XmlKey 字段
            var hasXmlKey = metadata.Fields?.Exists(f => f.IsXmlKey) ?? false;
            
            if (!hasXmlKey)
            {
                throw new InvalidOperationException(
                    $"配置类 '{metadata.ManagedTypeName}' 不是嵌套容器（未标记 [XmlNested]），" +
                    $"必须至少有一个字段标记 [XmlKey] 特性。");
            }
        }
        
        #endregion
        
        #region 注释提取
        
        /// <summary>
        /// 提取字段的源码注释
        /// </summary>
        /// <param name="fieldInfo">字段反射信息</param>
        /// <returns>注释文本,如果没有则返回null</returns>
        private static string ExtractFieldComment(FieldInfo fieldInfo)
        {
            // 尝试从自定义特性中获取注释
            // 注意: C#反射无法直接读取XML注释,这里返回null
            // 如果需要读取XML注释,需要使用Roslyn或其他工具
            
            // 临时方案: 返回null,后续可以通过Roslyn读取
            return null;
        }
        
        #endregion
    }
}
