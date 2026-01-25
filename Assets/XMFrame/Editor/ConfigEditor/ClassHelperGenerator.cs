using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Scriban;
using Scriban.Runtime;
using UnityEditor;
using UnityEngine;
using XMFrame.Editor.ConfigEditor;
using XMFrame.Utils.Attribute;

namespace XMFrame.Editor.ConfigEditor
{
    /// <summary>
    /// ClassHelper 代码生成器
    /// </summary>
    public static class ClassHelperGenerator
    {
        /// <summary>
        /// 生成所有配置类型的 ClassHelper 代码
        /// </summary>
        public static void GenerateAllClassHelperCode(string outputBasePath = null)
        {
            try
            {
                // 查找所有 XConfig 类型
                var configTypes = FindAllXConfigTypes();
                
                if (configTypes.Count == 0)
                {
                    Debug.LogWarning("未找到任何 XConfig 类型");
                    return;
                }

                Debug.Log($"找到 {configTypes.Count} 个配置类型");

                // 加载 Scriban 模板
                var templatePath = "Assets/XMFrame/Editor/ConfigEditor/Templates/ClassHelper.sbncs";
                if (!File.Exists(templatePath))
                {
                    Debug.LogError($"模板文件不存在: {templatePath}");
                    return;
                }

                var templateContent = File.ReadAllText(templatePath);
                var template = Template.Parse(templateContent);

                if (template.HasErrors)
                {
                    Debug.LogError($"模板解析错误: {string.Join(", ", template.Messages)}");
                    return;
                }

                // 按程序集分组处理
                var typesByAssembly = configTypes.GroupBy(t => t.Assembly).ToList();

                foreach (var assemblyGroup in typesByAssembly)
                {
                    var assembly = assemblyGroup.Key;
                    var types = assemblyGroup.ToList();

                    // 查找 asmdef 文件并确定输出目录
                    string outputDir = outputBasePath;
                    if (string.IsNullOrEmpty(outputDir))
                    {
                        outputDir = FindOutputDirectory(assembly);
                    }

                    if (string.IsNullOrEmpty(outputDir))
                    {
                        Debug.LogWarning($"无法为程序集 {assembly.GetName().Name} 确定输出目录，跳过");
                        continue;
                    }

                    // 确保输出目录存在
                    Directory.CreateDirectory(outputDir);

                    // 为每个类型生成代码
                    foreach (var configType in types)
                    {
                        try
                        {
                            GenerateClassHelperCodeForType(configType, template, outputDir);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"生成 {configType.Name} 的 ClassHelper 代码时出错: {ex.Message}\n{ex.StackTrace}");
                        }
                    }
                }

                Debug.Log("ClassHelper 代码生成完成");
                AssetDatabase.Refresh();
            }
            catch (Exception ex)
            {
                Debug.LogError($"生成 ClassHelper 代码时出错: {ex.Message}\n{ex.StackTrace}");
            }
        }

        /// <summary>
        /// 为单个类型生成 ClassHelper 代码（公共方法，供其他生成器调用）
        /// </summary>
        public static void GenerateClassHelperForType(Type configType, string outputDir)
        {
            // 加载 Scriban 模板
            var templatePath = "Assets/XMFrame/Editor/ConfigEditor/Templates/ClassHelper.sbncs";
            if (!File.Exists(templatePath))
            {
                Debug.LogWarning($"ClassHelper 模板文件不存在: {templatePath}");
                return;
            }

            var templateContent = File.ReadAllText(templatePath);
            var template = Template.Parse(templateContent);

            if (template.HasErrors)
            {
                Debug.LogWarning($"ClassHelper 模板解析错误: {string.Join(", ", template.Messages)}");
                return;
            }

            GenerateClassHelperCodeForType(configType, template, outputDir);
        }

        /// <summary>
        /// 为单个类型生成 ClassHelper 代码
        /// </summary>
        private static void GenerateClassHelperCodeForType(Type configType, Template template, string outputDir)
        {
            // 分析类型
            var typeInfo = TypeAnalyzer.AnalyzeConfigType(configType);

            // 准备模板数据
            var scriptObject = new ScriptObject();
            scriptObject["namespace"] = typeInfo.Namespace;
            scriptObject["managed_type_name"] = typeInfo.ManagedTypeName;
            scriptObject["unmanaged_type_name"] = typeInfo.UnmanagedTypeName;
            
            // 收集所需的 using 语句
            var requiredUsings = new HashSet<string>(typeInfo.RequiredUsings)
            {
                "System",
                "System.Collections.Generic",
                "System.Xml",
                "XMFrame",
                "XMFrame.Interfaces",
                "XMFrame.Utils",
                "XMFrame.Utils.Attribute"
            };
            scriptObject["required_usings"] = requiredUsings.ToList();

            // 收集程序集级别的全局转换器
            var assemblyGlobalConverters = new Dictionary<Type, XmlGlobalConvertAttribute>();
            try
            {
                var assembly = configType.Assembly;
                var assemblyAttrs = assembly.GetCustomAttributes(typeof(XmlGlobalConvertAttribute), false)
                    .Cast<XmlGlobalConvertAttribute>();
                foreach (var attr in assemblyAttrs)
                {
                    if (attr.ConverterType != null)
                    {
                        // 获取转换器的目标类型（从 XmlConvertBase<T> 中提取）
                        var baseType = attr.ConverterType.BaseType;
                        if (baseType != null && baseType.IsGenericType)
                        {
                            var genericArgs = baseType.GetGenericArguments();
                            if (genericArgs.Length == 1)
                            {
                                var targetType = genericArgs[0];
                                if (targetType.Namespace == "Unity.Mathematics")
                                {
                                    assemblyGlobalConverters[targetType] = attr;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {
                // 忽略错误
            }

            // 准备字段数据
            var fields = new List<ScriptObject>();
            var unityMathConverters = new Dictionary<Type, ScriptObject>(); // 收集所有 Unity.Mathematics 类型的转换器信息
            var configKeyTypes = new Dictionary<Type, ScriptObject>(); // 收集所有 ConfigKey<T> 类型信息
            var nestedConfigTypes = new Dictionary<Type, ScriptObject>(); // 收集所有嵌套 XConfig 类型信息
            var containerParsers = new Dictionary<string, ScriptObject>(); // 收集所有需要生成解析方法的容器类型
            
            foreach (var field in typeInfo.Fields)
            {
                var fieldObj = PrepareFieldData(field, typeInfo.ManagedType, assemblyGlobalConverters);
                fields.Add(fieldObj);
                
                // 收集 Unity.Mathematics 类型的转换器信息
                var fieldType = typeInfo.ManagedType.GetField(field.Name)?.FieldType;
                if (fieldType != null && fieldType.Namespace == "Unity.Mathematics")
                {
                    // 确保添加 Unity.Mathematics 命名空间
                    requiredUsings.Add("Unity.Mathematics");
                    
                    if (!unityMathConverters.ContainsKey(fieldType))
                    {
                        var converterInfo = new ScriptObject();
                        converterInfo["math_type"] = GetTypeName(fieldType);
                        converterInfo["math_type_full"] = fieldType.FullName ?? fieldType.Name;
                        
                        // 检查是否有字段级别的转换器
                        if (fieldObj.ContainsKey("has_global_convert") && (bool)fieldObj["has_global_convert"])
                        {
                            converterInfo["converter_type"] = fieldObj["global_convert_type"] ?? "";
                            converterInfo["converter_domain"] = fieldObj["global_convert_domain"] ?? "";
                            converterInfo["has_converter"] = true;
                        }
                        // 检查是否有程序集级别的转换器
                        else if (assemblyGlobalConverters.TryGetValue(fieldType, out var assemblyAttr))
                        {
                            converterInfo["converter_type"] = assemblyAttr.ConverterType.FullName ?? assemblyAttr.ConverterType.Name;
                            converterInfo["converter_domain"] = assemblyAttr.Domain ?? "";
                            converterInfo["has_converter"] = true;
                        }
                        else
                        {
                            converterInfo["has_converter"] = false;
                        }
                        
                        unityMathConverters[fieldType] = converterInfo;
                    }
                }
                
                // 检查容器中的 Unity.Mathematics 类型
                if (fieldType != null && fieldType.IsGenericType)
                {
                    var genericArgs = fieldType.GetGenericArguments();
                    foreach (var argType in genericArgs)
                    {
                        if (argType.Namespace == "Unity.Mathematics")
                        {
                            // 确保添加 Unity.Mathematics 命名空间
                            requiredUsings.Add("Unity.Mathematics");
                            
                            if (!unityMathConverters.ContainsKey(argType))
                            {
                                var converterInfo = new ScriptObject();
                                converterInfo["math_type"] = GetTypeName(argType);
                                converterInfo["math_type_full"] = argType.FullName ?? argType.Name;
                                
                                if (assemblyGlobalConverters.TryGetValue(argType, out var assemblyAttr))
                                {
                                    converterInfo["converter_type"] = assemblyAttr.ConverterType.FullName ?? assemblyAttr.ConverterType.Name;
                                    converterInfo["converter_domain"] = assemblyAttr.Domain ?? "";
                                    converterInfo["has_converter"] = true;
                                }
                                else
                                {
                                    converterInfo["has_converter"] = false;
                                }
                                
                                unityMathConverters[argType] = converterInfo;
                            }
                        }
                    }
                }
                
                // 收集 ConfigKey<T> 类型信息
                if (fieldType != null && IsConfigKeyType(fieldType))
                {
                    var genericArgs = fieldType.GetGenericArguments();
                    if (genericArgs.Length > 0)
                    {
                        var keyElementType = genericArgs[0];
                        if (!configKeyTypes.ContainsKey(keyElementType))
                        {
                            var keyInfo = new ScriptObject();
                            keyInfo["element_type"] = GetTypeName(keyElementType);
                            keyInfo["element_type_full"] = keyElementType.FullName ?? keyElementType.Name;
                            configKeyTypes[keyElementType] = keyInfo;
                        }
                    }
                }
                
                // 检查容器中的 ConfigKey<T> 类型
                if (fieldType != null && fieldType.IsGenericType)
                {
                    var genericArgs = fieldType.GetGenericArguments();
                    foreach (var argType in genericArgs)
                    {
                        if (IsConfigKeyType(argType) && argType.IsGenericType)
                        {
                            var keyGenericArgs = argType.GetGenericArguments();
                            if (keyGenericArgs.Length > 0)
                            {
                                var keyElementType = keyGenericArgs[0];
                                if (!configKeyTypes.ContainsKey(keyElementType))
                                {
                                    var keyInfo = new ScriptObject();
                                    keyInfo["element_type"] = GetTypeName(keyElementType);
                                    keyInfo["element_type_full"] = keyElementType.FullName ?? keyElementType.Name;
                                    configKeyTypes[keyElementType] = keyInfo;
                                }
                            }
                        }
                    }
                }
                
                // 收集嵌套 XConfig 类型信息
                if (fieldType != null && IsNestedConfigType(fieldType))
                {
                    if (!nestedConfigTypes.ContainsKey(fieldType))
                    {
                        var nestedInfo = new ScriptObject();
                        nestedInfo["type_name"] = GetTypeName(fieldType);
                        nestedInfo["type_name_full"] = fieldType.FullName ?? fieldType.Name;
                        nestedInfo["helper_class"] = fieldType.Name + "ClassHelper";
                        nestedConfigTypes[fieldType] = nestedInfo;
                    }
                }
                
                // 检查容器中的嵌套 XConfig 类型
                if (fieldType != null && fieldType.IsGenericType)
                {
                    var genericArgs = fieldType.GetGenericArguments();
                    foreach (var argType in genericArgs)
                    {
                        if (IsNestedConfigType(argType))
                        {
                            if (!nestedConfigTypes.ContainsKey(argType))
                            {
                                var nestedInfo = new ScriptObject();
                                nestedInfo["type_name"] = GetTypeName(argType);
                                nestedInfo["type_name_full"] = argType.FullName ?? argType.Name;
                                nestedInfo["helper_class"] = argType.Name + "ClassHelper";
                                nestedConfigTypes[argType] = nestedInfo;
                            }
                        }
                    }
                }
                
                // 收集嵌套容器类型（List<List<T>>, List<Dictionary<K,V>> 等）
                if (fieldType != null && (IsListType(fieldType) || IsHashSetType(fieldType)))
                {
                    var elementType = fieldType.GetGenericArguments()[0];
                    if (IsListType(elementType) || IsDictionaryType(elementType) || IsHashSetType(elementType))
                    {
                        // 生成唯一的解析器名称
                        var parserKey = GetTypeName(elementType);
                        if (!containerParsers.ContainsKey(parserKey))
                        {
                            var parserInfo = new ScriptObject();
                            parserInfo["type_name"] = GetTypeName(elementType);
                            parserInfo["type_name_full"] = elementType.FullName ?? elementType.Name;
                            parserInfo["parser_method_name"] = "Parse" + parserKey.Replace("<", "_").Replace(">", "_").Replace(",", "_").Replace(" ", "");
                            
                            // 分析容器的内部结构
                            AnalyzeContainerStructure(elementType, parserInfo);
                            
                            containerParsers[parserKey] = parserInfo;
                        }
                    }
                }
            }
            scriptObject["fields"] = fields;
            scriptObject["unity_math_converters"] = unityMathConverters.Values.ToList();
            scriptObject["config_key_types"] = configKeyTypes.Values.ToList();
            scriptObject["nested_config_types"] = nestedConfigTypes.Values.ToList();
            scriptObject["container_parsers"] = containerParsers.Values.ToList();
            
            // 更新 required_usings（可能已添加 Unity.Mathematics）
            scriptObject["required_usings"] = requiredUsings.ToList();

            // 渲染模板
            var context = new TemplateContext();
            context.PushGlobal(scriptObject);
            var result = template.Render(context);

            // 检查模板渲染错误
            if (template.HasErrors)
            {
                Debug.LogError($"渲染模板时出错: {string.Join(", ", template.Messages.Select(m => m.Message))}");
                return;
            }

            // 保存生成的文件
            var fileName = $"{typeInfo.ManagedTypeName}ClassHelper.Gen.cs";
            var filePath = Path.Combine(outputDir, fileName);
            File.WriteAllText(filePath, result);

            Debug.Log($"已生成: {filePath}");
        }

        /// <summary>
        /// 准备字段数据
        /// </summary>
        private static ScriptObject PrepareFieldData(FieldInfo fieldInfo, Type managedType, Dictionary<Type, XmlGlobalConvertAttribute> assemblyGlobalConverters)
        {
            var fieldObj = new ScriptObject();
            fieldObj["name"] = fieldInfo.Name;
            fieldObj["managed_type"] = fieldInfo.ManagedType;
            fieldObj["unmanaged_type"] = fieldInfo.UnmanagedType;
            fieldObj["needs_converter"] = fieldInfo.NeedsConverter;
            fieldObj["converter_type_name"] = fieldInfo.ConverterTypeName ?? "";
            fieldObj["converter_domain"] = fieldInfo.ConverterDomain ?? "";
            fieldObj["source_type"] = fieldInfo.SourceType ?? "";
            fieldObj["target_type"] = fieldInfo.TargetType ?? "";

            // 获取字段的实际类型
            var field = managedType.GetField(fieldInfo.Name);
            if (field != null)
            {
                var fieldType = field.FieldType;
                
                // 判断字段类型
                fieldObj["is_list"] = IsListType(fieldType);
                fieldObj["is_dictionary"] = IsDictionaryType(fieldType);
                fieldObj["is_hashset"] = IsHashSetType(fieldType);
                fieldObj["is_config_key"] = IsConfigKeyType(fieldType);
                fieldObj["is_nested_config"] = IsNestedConfigType(fieldType);
                fieldObj["is_str_label"] = fieldType.Name == "StrLabel";
                fieldObj["is_string"] = fieldType == typeof(string);
                fieldObj["is_unity_math"] = fieldType.Namespace == "Unity.Mathematics";
                fieldObj["is_type"] = fieldType == typeof(Type);

                // 检查字符串模式
                var strModeAttr = field.GetCustomAttribute<XmlStringModeAttribute>();
                if (strModeAttr != null)
                {
                    var strModeField = typeof(XmlStringModeAttribute).GetField("StrMode", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (strModeField != null)
                    {
                        var strMode = (EXmlStrMode)strModeField.GetValue(strModeAttr);
                        fieldObj["str_mode"] = strMode.ToString();
                    }
                }

                // 检查全局转换器（字段级别优先，然后是程序集级别）
                var globalConvertAttr = field.GetCustomAttribute<XmlGlobalConvertAttribute>();
                if (globalConvertAttr != null)
                {
                    fieldObj["has_global_convert"] = true;
                    fieldObj["global_convert_type"] = globalConvertAttr.ConverterType.FullName ?? globalConvertAttr.ConverterType.Name;
                    fieldObj["global_convert_domain"] = globalConvertAttr.Domain ?? "";
                }
                else if (fieldType.Namespace == "Unity.Mathematics" && assemblyGlobalConverters.TryGetValue(fieldType, out var assemblyAttr))
                {
                    // 使用程序集级别的转换器
                    fieldObj["has_global_convert"] = true;
                    fieldObj["global_convert_type"] = assemblyAttr.ConverterType.FullName ?? assemblyAttr.ConverterType.Name;
                    fieldObj["global_convert_domain"] = assemblyAttr.Domain ?? "";
                }
                else
                {
                    fieldObj["has_global_convert"] = false;
                }

                // 提取泛型参数
                if (fieldType.IsGenericType)
                {
                    var genericArgs = fieldType.GetGenericArguments();
                    if (IsListType(fieldType) || IsHashSetType(fieldType))
                    {
                        var elementType = genericArgs[0];
                        fieldObj["element_type"] = GetTypeName(elementType);
                        fieldObj["element_type_full"] = elementType.FullName ?? elementType.Name;
                        
                        // 检查元素类型是否是嵌套配置
                        if (IsNestedConfigType(elementType))
                        {
                            fieldObj["element_is_nested_config"] = true;
                            fieldObj["element_helper_class"] = elementType.Name + "ClassHelper";
                        }
                        else
                        {
                            fieldObj["element_is_nested_config"] = false;
                        }
                        
                        // 检查元素类型是否是容器（嵌套容器）
                        if (IsListType(elementType) || IsDictionaryType(elementType) || IsHashSetType(elementType))
                        {
                            fieldObj["element_is_container"] = true;
                            fieldObj["element_container_type"] = GetContainerTypeName(elementType);
                        }
                        else
                        {
                            fieldObj["element_is_container"] = false;
                        }
                    }
                    else if (IsDictionaryType(fieldType))
                    {
                        var keyType = genericArgs[0];
                        var valueType = genericArgs[1];
                        fieldObj["key_type"] = GetTypeName(keyType);
                        fieldObj["value_type"] = GetTypeName(valueType);
                        // 检查键或值类型是否是嵌套配置
                        if (IsNestedConfigType(keyType))
                        {
                            fieldObj["key_is_nested_config"] = true;
                            fieldObj["key_helper_class"] = keyType.Name + "ClassHelper";
                        }
                        else
                        {
                            fieldObj["key_is_nested_config"] = false;
                        }
                        if (IsNestedConfigType(valueType))
                        {
                            fieldObj["value_is_nested_config"] = true;
                            fieldObj["value_helper_class"] = valueType.Name + "ClassHelper";
                        }
                        else
                        {
                            fieldObj["value_is_nested_config"] = false;
                        }
                    }
                    else if (IsConfigKeyType(fieldType))
                    {
                        fieldObj["config_key_type"] = GetTypeName(genericArgs[0]);
                    }
                }

                // 嵌套配置的 Helper 类名
                if (IsNestedConfigType(fieldType))
                {
                    fieldObj["nested_helper_class"] = fieldType.Name + "ClassHelper";
                }
            }

            return fieldObj;
        }

        /// <summary>
        /// 检查是否是 List 类型
        /// </summary>
        private static bool IsListType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
        }

        /// <summary>
        /// 检查是否是 Dictionary 类型
        /// </summary>
        private static bool IsDictionaryType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>);
        }

        /// <summary>
        /// 检查是否是 HashSet 类型
        /// </summary>
        private static bool IsHashSetType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(HashSet<>);
        }

        /// <summary>
        /// 检查是否是 ConfigKey 类型
        /// </summary>
        private static bool IsConfigKeyType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition().Name == "ConfigKey`1";
        }

        /// <summary>
        /// 检查是否是嵌套 XConfig 类型
        /// </summary>
        private static bool IsNestedConfigType(Type type)
        {
            return UnmanagedCodeGenerator.IsXConfigType(type);
        }

        /// <summary>
        /// 获取类型名称（简化版本）
        /// </summary>
        private static string GetTypeName(Type type)
        {
            if (type.IsGenericType)
            {
                var name = type.Name.Split('`')[0];
                var args = type.GetGenericArguments().Select(GetTypeName);
                return $"{name}<{string.Join(", ", args)}>";
            }
            return type.Name;
        }

        /// <summary>
        /// 获取容器类型名称
        /// </summary>
        private static string GetContainerTypeName(Type type)
        {
            if (IsListType(type))
                return "List";
            if (IsDictionaryType(type))
                return "Dictionary";
            if (IsHashSetType(type))
                return "HashSet";
            return "Unknown";
        }

        /// <summary>
        /// 分析容器结构
        /// </summary>
        private static void AnalyzeContainerStructure(Type containerType, ScriptObject parserInfo)
        {
            if (IsListType(containerType))
            {
                parserInfo["container_type"] = "List";
                var elementType = containerType.GetGenericArguments()[0];
                parserInfo["element_type"] = GetTypeName(elementType);
                parserInfo["element_type_full"] = elementType.FullName ?? elementType.Name;
                parserInfo["is_list"] = true;
                parserInfo["is_dictionary"] = false;
                parserInfo["is_hashset"] = false;
            }
            else if (IsDictionaryType(containerType))
            {
                parserInfo["container_type"] = "Dictionary";
                var genericArgs = containerType.GetGenericArguments();
                parserInfo["key_type"] = GetTypeName(genericArgs[0]);
                parserInfo["key_type_full"] = genericArgs[0].FullName ?? genericArgs[0].Name;
                parserInfo["value_type"] = GetTypeName(genericArgs[1]);
                parserInfo["value_type_full"] = genericArgs[1].FullName ?? genericArgs[1].Name;
                parserInfo["is_list"] = false;
                parserInfo["is_dictionary"] = true;
                parserInfo["is_hashset"] = false;
            }
            else if (IsHashSetType(containerType))
            {
                parserInfo["container_type"] = "HashSet";
                var elementType = containerType.GetGenericArguments()[0];
                parserInfo["element_type"] = GetTypeName(elementType);
                parserInfo["element_type_full"] = elementType.FullName ?? elementType.Name;
                parserInfo["is_list"] = false;
                parserInfo["is_dictionary"] = false;
                parserInfo["is_hashset"] = true;
            }
        }

        /// <summary>
        /// 查找所有 XConfig 类型
        /// </summary>
        private static List<Type> FindAllXConfigTypes()
        {
            var configTypes = new List<Type>();

            // 获取所有程序集
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes()
                        .Where(t => UnmanagedCodeGenerator.IsXConfigType(t) && !t.IsAbstract)
                        .ToList();

                    configTypes.AddRange(types);
                }
                catch (ReflectionTypeLoadException ex)
                {
                    // 忽略无法加载的类型
                    Debug.LogWarning($"加载程序集 {assembly.GetName().Name} 的类型时出错: {ex.Message}");
                }
            }

            return configTypes;
        }

        /// <summary>
        /// 查找输出目录（基于 asmdef 文件位置）
        /// </summary>
        private static string FindOutputDirectory(Assembly assembly)
        {
            var assemblyName = assembly.GetName().Name;
            var projectPath = Application.dataPath.Replace("/Assets", "").Replace("\\Assets", "");

            // 在 Assets 目录下查找 asmdef 文件
            var assetsPath = Path.Combine(projectPath, "Assets");
            var asmdefFiles = Directory.GetFiles(assetsPath, "*.asmdef", SearchOption.AllDirectories);

            foreach (var asmdefFile in asmdefFiles)
            {
                try
                {
                    // 读取 asmdef 文件内容，检查 name 字段
                    var content = File.ReadAllText(asmdefFile);
                    if (content.Contains($"\"name\": \"{assemblyName}\""))
                    {
                        // 找到匹配的 asmdef 文件，在同级目录创建 Config/Code.Gen
                        var asmdefDir = Path.GetDirectoryName(asmdefFile);
                        var configDir = Path.Combine(asmdefDir, "Config", "Code.Gen");
                        return configDir;
                    }
                }
                catch
                {
                    // 忽略读取错误
                }
            }

            // 如果找不到，尝试根据程序集中的类型位置推断
            var types = assembly.GetTypes().Where(t => !string.IsNullOrEmpty(t.Namespace)).ToList();
            if (types.Count > 0)
            {
                // 使用第一个类型的命名空间来推断路径
                var firstType = types[0];
                var namespaceParts = firstType.Namespace.Split('.');
                
                // 查找可能的目录
                var searchDirs = Directory.GetDirectories(assetsPath, namespaceParts[0], SearchOption.AllDirectories);
                foreach (var dir in searchDirs)
                {
                    var asmdefInDir = Directory.GetFiles(dir, "*.asmdef", SearchOption.TopDirectoryOnly);
                    if (asmdefInDir.Length > 0)
                    {
                        var configDir = Path.Combine(dir, "Config", "Code.Gen");
                        return configDir;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 为指定的程序集生成 ClassHelper 代码
        /// </summary>
        public static void GenerateClassHelperCodeForAssemblies(List<Assembly> assemblies, string outputBasePath = null)
        {
            try
            {
                if (assemblies == null || assemblies.Count == 0)
                {
                    Debug.LogWarning("未指定任何程序集");
                    return;
                }

                // 查找所有 XConfig 类型（仅从指定程序集）
                var configTypes = new List<Type>();
                foreach (var assembly in assemblies)
                {
                    try
                    {
                        var types = assembly.GetTypes()
                            .Where(t => UnmanagedCodeGenerator.IsXConfigType(t) && !t.IsAbstract)
                            .ToList();
                        configTypes.AddRange(types);
                    }
                    catch (ReflectionTypeLoadException ex)
                    {
                        Debug.LogWarning($"加载程序集 {assembly.GetName().Name} 的类型时出错: {ex.Message}");
                    }
                }

                if (configTypes.Count == 0)
                {
                    Debug.LogWarning("在选定的程序集中未找到任何 XConfig 类型");
                    return;
                }

                Debug.Log($"找到 {configTypes.Count} 个配置类型");

                // 加载 Scriban 模板
                var templatePath = "Assets/XMFrame/Editor/ConfigEditor/Templates/ClassHelper.sbncs";
                if (!File.Exists(templatePath))
                {
                    Debug.LogError($"模板文件不存在: {templatePath}");
                    return;
                }

                var templateContent = File.ReadAllText(templatePath);
                var template = Template.Parse(templateContent);

                if (template.HasErrors)
                {
                    Debug.LogError($"模板解析错误: {string.Join(", ", template.Messages)}");
                    return;
                }

                // 按程序集分组处理
                var typesByAssembly = configTypes.GroupBy(t => t.Assembly).ToList();

                foreach (var assemblyGroup in typesByAssembly)
                {
                    var assembly = assemblyGroup.Key;
                    var types = assemblyGroup.ToList();

                    // 查找 asmdef 文件并确定输出目录
                    string outputDir = outputBasePath;
                    if (string.IsNullOrEmpty(outputDir))
                    {
                        outputDir = FindOutputDirectory(assembly);
                    }

                    if (string.IsNullOrEmpty(outputDir))
                    {
                        Debug.LogWarning($"无法为程序集 {assembly.GetName().Name} 确定输出目录，跳过");
                        continue;
                    }

                    // 确保输出目录存在
                    Directory.CreateDirectory(outputDir);

                    // 为每个类型生成代码
                    foreach (var configType in types)
                    {
                        try
                        {
                            GenerateClassHelperCodeForType(configType, template, outputDir);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"生成 {configType.Name} 的 ClassHelper 代码时出错: {ex.Message}");
                        }
                    }
                }

                Debug.Log("ClassHelper 代码生成完成");
                AssetDatabase.Refresh();
            }
            catch (Exception ex)
            {
                Debug.LogError($"生成 ClassHelper 代码时出错: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }
    }
}
