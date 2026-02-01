using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Scriban;
using Scriban.Runtime;
using UnityEditor;
using UnityEngine;
using XModToolkit;
using XModToolkit.Config;

namespace UnityToolkit
{
    /// <summary>
    /// 生成 ConfigClassHelper&lt;TC, TI&gt; 用于解析 XML。
    /// 通过 XM/Config/Generate Code (Select Assemblies) 与 Unmanaged 代码一并生成。
    /// </summary>
    public static class ClassHelperCodeGenerator
    {
        private const string TemplateFileName = "ClassHelper.sbncs";

        /// <summary>
        /// 为指定程序集列表生成所有配置类型的 ClassHelper 代码。
        /// </summary>
        public static void GenerateClassHelperForAssemblies(List<Assembly> assemblies, string outputBasePath = null)
        {
            if (assemblies == null || assemblies.Count == 0)
            {
                Debug.LogWarning("[ClassHelperCodeGenerator] 未指定任何程序集");
                return;
            }

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
                catch (ReflectionTypeLoadException)
                {
                    // 忽略无法加载的程序集
                }
            }

            if (configTypes.Count == 0)
            {
                Debug.LogWarning("[ClassHelperCodeGenerator] 在选定的程序集中未找到任何 XConfig 类型");
                return;
            }

            if (!ScribanCodeGenerator.TryLoadTemplate(ScribanCodeGenerator.GetTemplatePath(TemplateFileName), out var template))
            {
                Debug.LogError("[ClassHelperCodeGenerator] 模板加载失败: " + TemplateFileName);
                return;
            }

            var typesByAssembly = configTypes.GroupBy(t => t.Assembly).ToList();
            foreach (var assemblyGroup in typesByAssembly)
            {
                var assembly = assemblyGroup.Key;
                var types = assemblyGroup.ToList();

                string outputDir = outputBasePath;
                if (string.IsNullOrEmpty(outputDir))
                {
                    outputDir = ConfigCodeGenCache.GetOutputDirectory(assembly);
                }

                if (string.IsNullOrEmpty(outputDir))
                {
                    Debug.LogWarning($"[ClassHelperCodeGenerator] 无法为程序集 {assembly.GetName().Name} 确定输出目录，跳过");
                    continue;
                }

                Directory.CreateDirectory(outputDir);

                foreach (var configType in types)
                {
                    try
                    {
                        GenerateClassHelperForType(configType, template, outputDir);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[ClassHelperCodeGenerator] 生成 {configType.Name} 的 ClassHelper 时出错: {ex.Message}");
                    }
                }
            }

            Debug.Log("[ClassHelperCodeGenerator] ClassHelper 代码生成完成");
            AssetDatabase.Refresh();
        }

        private static void GenerateClassHelperForType(Type configType, Template template, string outputDir)
        {
            var typeInfo = TypeAnalyzer.AnalyzeConfigType(configType);

            // 必要字段检查：若配置类型有字段但没有任何 [XmlNotNull]，给出警告
            if (typeInfo.Fields != null && typeInfo.Fields.Count > 0 &&
                !typeInfo.Fields.Any(f => f.IsNotNull))
            {
                UnityEngine.Debug.LogWarning($"[ClassHelperCodeGenerator] 配置类型 {typeInfo.ManagedTypeName} 没有任何 [XmlNotNull] 必要字段，建议至少标记一个关键字段。");
            }

            var fieldAssigns = BuildFieldAssignCodes(typeInfo);
            var converterRegistrations = BuildConverterRegistrations(typeInfo);
            var containerAllocCodes = BuildContainerAllocCodes(typeInfo);
            var containerAllocHelperMethods = BuildContainerAllocHelperMethods(typeInfo);
            var dto = ToClassHelperDto(typeInfo, fieldAssigns, converterRegistrations);
            dto.ModName = GetModNameFromAssembly(typeInfo.ManagedType?.Assembly);
            dto.ContainerAllocCode = containerAllocCodes;
            dto.ContainerAllocHelperMethods = containerAllocHelperMethods;
            var scriptObject = ClassHelperModelBuilder.Build(dto);
            if (scriptObject == null)
            {
                Debug.LogError($"[ClassHelperCodeGenerator] 构建模板模型失败: {typeInfo.ManagedTypeName}");
                return;
            }

            if (!ScribanCodeGeneratorCore.TryRender(template, scriptObject, out var result))
            {
                Debug.LogError($"[ClassHelperCodeGenerator] 渲染模板失败: {typeInfo.ManagedTypeName}");
                return;
            }

            var fileName = typeInfo.ManagedTypeName + "ClassHelper.Gen.cs";
            var filePath = Path.Combine(outputDir, fileName);
            File.WriteAllText(filePath, result);
            Debug.Log("[ClassHelperCodeGenerator] 已生成: " + filePath);
        }

        /// <summary>
        /// 为每个字段生成 ParseXXX 调用与方法（无反射），供模板输出 DeserializeConfigFromXml 与 #region 解析。
        /// </summary>
        private static List<ScriptObject> BuildFieldAssignCodes(ConfigTypeInfo typeInfo)
        {
            var list = new List<ScriptObject>();
            var configType = typeInfo.ManagedType;
            if (configType == null) return list;

            foreach (var field in typeInfo.Fields)
            {
                if (field.Name == "Data") continue;

                var fieldInfo = configType.GetField(field.Name);
                if (fieldInfo == null) continue;

                var fieldType = fieldInfo.FieldType;
                var parseName = "Parse" + field.Name;
                var (callCode, methodCode) = GetParseMethodCode(field.Name, parseName, fieldType, typeInfo, field);
                if (string.IsNullOrEmpty(callCode) || string.IsNullOrEmpty(methodCode)) continue;

                var obj = new ScriptObject();
                obj["call_code"] = callCode;
                obj["method_code"] = methodCode;
                list.Add(obj);
            }

            return list;
        }

        /// <summary>构建容器字段的内存分配代码，供模板生成 AllocContainerWithoutFillImpl 方法。</summary>
        private static string BuildContainerAllocCodes(ConfigTypeInfo typeInfo)
        {
            var configType = typeInfo.ManagedType;
            if (configType == null) return "";

            var sb = new StringBuilder();
            var managedVar = "config";
            var unmanagedVar = "data";
            var unmanagedTypeName = typeInfo.UnmanagedTypeName;
            var managedTypeName = typeInfo.ManagedTypeName;

            // 检查是否有容器字段需要分配
            bool hasContainers = false;
            foreach (var field in typeInfo.Fields)
            {
                if (field.Name == "Data") continue;
                var fieldInfo = configType.GetField(field.Name);
                if (fieldInfo != null && IsContainerType(fieldInfo.FieldType))
                {
                    hasContainers = true;
                    break;
                }
            }

            if (!hasContainers)
                return "";

            // 在最外层获取 unmanaged 数据的值（不是引用）
            sb.AppendLine($"        // 在最外层获取 unmanaged 数据的值（不是引用）");
            sb.AppendLine($"        var map = configHolderData.Data.GetMap<CfgI, {unmanagedTypeName}>(_definedInMod);");
            sb.AppendLine($"        if (!map.TryGetValue(configHolderData.Data.BlobContainer, cfgi, out var {unmanagedVar}))");
            sb.AppendLine($"        {{");
            sb.AppendLine($"            return;");
            sb.AppendLine($"        }}");
            sb.AppendLine();

            // 生成各个容器字段的分配调用
            foreach (var field in typeInfo.Fields)
            {
                if (field.Name == "Data") continue;

                var fieldInfo = configType.GetField(field.Name);
                if (fieldInfo == null) continue;

                var fieldType = fieldInfo.FieldType;
                if (IsContainerType(fieldType))
                {
                    // 生成方法调用，传递 ref data, cfgi
                    sb.AppendLine($"        Alloc{field.Name}({managedVar}, ref {unmanagedVar}, cfgi, configHolderData);");
                }
            }

            sb.AppendLine();
            sb.AppendLine($"        // 将修改后的 unmanaged 数据写回容器");
            sb.AppendLine($"        map[configHolderData.Data.BlobContainer, cfgi] = {unmanagedVar};");

            return sb.ToString().TrimEnd();
        }

        /// <summary>构建容器字段分配的辅助方法定义</summary>
        private static string BuildContainerAllocHelperMethods(ConfigTypeInfo typeInfo)
        {
            var configType = typeInfo.ManagedType;
            if (configType == null) return "";

            var sb = new StringBuilder();
            var managedVar = "config";
            var unmanagedVar = "data";
            var unmanagedTypeName = typeInfo.UnmanagedTypeName;
            var managedTypeName = typeInfo.ManagedTypeName;

            foreach (var field in typeInfo.Fields)
            {
                if (field.Name == "Data") continue;

                var fieldInfo = configType.GetField(field.Name);
                if (fieldInfo == null) continue;

                var fieldType = fieldInfo.FieldType;
                var allocCode = GetContainerAllocCode(field.Name, fieldType, managedVar, unmanagedVar, typeInfo);
                if (!string.IsNullOrEmpty(allocCode))
                {
                    // 替换占位符为实际的 unmanaged 类型名称
                    allocCode = allocCode.Replace("UNMANAGED_TYPE", unmanagedTypeName);
                    
                    // 生成辅助方法
                    sb.AppendLine($"    private void Alloc{field.Name}(");
                    sb.AppendLine($"        {managedTypeName} {managedVar},");
                    sb.AppendLine($"        ref {unmanagedTypeName} {unmanagedVar},");
                    sb.AppendLine($"        CfgI cfgi,");
                    sb.AppendLine($"        XM.ConfigDataCenter.ConfigDataHolder configHolderData)");
                    sb.AppendLine("    {");
                    sb.AppendLine(allocCode);
                    sb.AppendLine("    }");
                    sb.AppendLine();
                }
            }

            return sb.ToString().TrimEnd();
        }

        /// <summary>判断是否为容器类型</summary>
        private static bool IsContainerType(Type type)
        {
            if (!type.IsGenericType) return false;
            var genericDef = type.GetGenericTypeDefinition();
            return genericDef == typeof(List<>) ||
                   genericDef == typeof(Dictionary<,>) ||
                   genericDef == typeof(HashSet<>);
        }

        /// <summary>判断是否为 CfgS&lt;T&gt; 类型</summary>
        private static bool IsCfgSType(Type type)
        {
            if (!type.IsGenericType) return false;
            var genericDef = type.GetGenericTypeDefinition();
            return genericDef.Name == "CfgS`1";
        }

        /// <summary>为键类型添加转换代码（如果需要）：CfgS -> CfgI 或自定义转换器</summary>
        /// <param name="varNamePrefix">用于生成转换后变量名的前缀，如 "kvp0" (从 "kvp0.Key" 提取)</param>
        /// <param name="convertedVarName">输出转换后的变量名（如 "kvp0_cfgI" 或 "typeId"）</param>
        private static void AppendKeyConversion(StringBuilder sb, Type keyType, string iteratorVarName, string varNamePrefix, 
            string indent, ConfigTypeInfo typeInfo, out string convertedVarName)
        {
            if (IsCfgSType(keyType))
            {
                // CfgS -> CfgI 转换
                convertedVarName = $"{varNamePrefix}_cfgI";
                sb.AppendLine($"{indent}if (!IConfigDataCenter.I.TryGetCfgI({iteratorVarName}.AsNonGeneric(), out var {convertedVarName}))");
                sb.AppendLine($"{indent}{{");
                sb.AppendLine($"{indent}    XM.XLog.Error($\"[Config] 无法找到配置 {{{iteratorVarName}.ConfigName}}, 跳过该项嵌套容器分配\");");
                sb.AppendLine($"{indent}    continue;");
                sb.AppendLine($"{indent}}}");
            }
            else if (TryGetKeyConverterInfo(keyType, typeInfo, varNamePrefix, out var targetTypeName, out var converterVarName))
            {
                // 自定义转换器（如 Type -> TypeI）
                convertedVarName = converterVarName;
                sb.AppendLine($"{indent}var converter_{varNamePrefix} = XM.Contracts.IConfigDataCenter.I?.GetConverterByType<{GetCSharpTypeName(keyType)}, {targetTypeName}>();");
                sb.AppendLine($"{indent}if (converter_{varNamePrefix} == null)");
                sb.AppendLine($"{indent}{{");
                sb.AppendLine($"{indent}    XM.XLog.Error($\"[Config] 无法找到类型转换器 {GetCSharpTypeName(keyType)} -> {targetTypeName}\");");
                sb.AppendLine($"{indent}    continue;");
                sb.AppendLine($"{indent}}}");
                sb.AppendLine($"{indent}if (!converter_{varNamePrefix}.Convert({iteratorVarName}, out var {convertedVarName})) continue;");
            }
            else
            {
                // 不需要转换
                convertedVarName = null;
            }
        }

        /// <summary>尝试获取键类型的转换器信息</summary>
        private static bool TryGetKeyConverterInfo(Type keyType, ConfigTypeInfo typeInfo, string varNamePrefix, out string targetTypeName, out string convertedVarName)
        {
            targetTypeName = null;
            convertedVarName = null;
            
            if (keyType == null || typeInfo == null) return false;
            
            // 查找该类型是否有转换器
            if (TryGetConverterTargetType(keyType, typeInfo, out var unmanagedTypeName))
            {
                targetTypeName = unmanagedTypeName;
                // 根据键类型和前缀生成唯一的变量名
                if (keyType == typeof(Type))
                    convertedVarName = $"{varNamePrefix}_typeId";
                else
                    convertedVarName = $"{varNamePrefix}_{keyType.Name.ToLower()}Id";
                return true;
            }
            
            return false;
        }

        /// <summary>获取转换后的键表达式（用于索引器）</summary>
        /// <param name="convertedVarName">转换后的变量名（来自 AppendKeyConversion），如果为 null 则使用原始键</param>
        /// <param name="keyUnmanagedType">键的 unmanaged 类型名称，如 "CfgI<TestConfigUnManaged>"</param>
        private static string GetConvertedKeyExpression(Type keyType, string originalKeyVar, string convertedVarName, string keyUnmanagedType)
        {
            if (string.IsNullOrEmpty(convertedVarName))
            {
                // 不需要转换，直接使用原始键
                return originalKeyVar;
            }
            else if (IsCfgSType(keyType))
            {
                // CfgS -> CfgI，在使用时需要调用 .As<UnmanagedType>()
                // keyUnmanagedType 格式为 "CfgI<TestConfigUnManaged>"，需要提取内层类型
                var innerType = ExtractInnerTypeFromCfgI(keyUnmanagedType);
                return $"{convertedVarName}.As<{innerType}>()";
            }
            else
            {
                // 其他转换（如 Type -> TypeI），直接使用转换后的变量
                return convertedVarName;
            }
        }

        /// <summary>从 "CfgI&lt;UnmanagedType&gt;" 中提取 "UnmanagedType"</summary>
        private static string ExtractInnerTypeFromCfgI(string cfgIType)
        {
            if (string.IsNullOrEmpty(cfgIType)) return cfgIType;
            
            // 格式: "CfgI<TestConfigUnManaged>"
            var startIndex = cfgIType.IndexOf('<');
            var endIndex = cfgIType.LastIndexOf('>');
            
            if (startIndex >= 0 && endIndex > startIndex)
            {
                return cfgIType.Substring(startIndex + 1, endIndex - startIndex - 1);
            }
            
            return cfgIType;
        }

        /// <summary>为容器类型字段生成内存分配代码，每次分配都重新获取引用以避免扩容导致的引用失效</summary>
        private static string GetContainerAllocCode(string fieldName, Type fieldType, string managedVar, string unmanagedVar, ConfigTypeInfo typeInfo)
        {
            // 获取 unmanaged 类型名称（需要从 typeInfo 中获取）
            var unmanagedTypeName = "UNMANAGED_TYPE"; // 这个会在调用处替换
            
            var sb = new StringBuilder();
            GenerateContainerAllocCode(sb, fieldName, fieldType, managedVar, unmanagedVar, unmanagedTypeName, "", 0, typeInfo);
            return sb.ToString().TrimEnd();
        }

        /// <summary>递归生成容器分配代码（支持嵌套容器）</summary>
        /// <param name="parentIteratorKey">父层的迭代器键，用于写入父容器：List 用 "i{level-1}"，Dictionary 用转换后的键变量</param>
        /// <param name="parentFieldPath">父层字段的访问路径（相对于data），如 "TestKeyList1" 或 "TestKeyList1[container, kvp0_cfgI].TestNestedList"</param>
        private static void GenerateContainerAllocCode(StringBuilder sb, string fieldName, Type fieldType, 
            string managedVar, string unmanagedVar, string unmanagedTypeName, string accessPath, int level, ConfigTypeInfo typeInfo, string parentIteratorKey = null, string parentFieldPath = null)
        {
            var indent = new string(' ', 8 + level * 4);
            var managedFieldAccess = string.IsNullOrEmpty(accessPath) ? $"{managedVar}.{fieldName}" : accessPath;
            
            // List<T> -> XBlobArray<T>
            if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var elemType = fieldType.GetGenericArguments()[0];
                var unmanagedElemType = GetUnmanagedTypeName(elemType, typeInfo);
                
                if (level == 0)
                {
                    // 第一层：分配并直接赋值到传入的 ref data
                    sb.AppendLine($"{indent}if ({managedFieldAccess} != null && {managedFieldAccess}.Count > 0)");
                    sb.AppendLine($"{indent}{{");
                    sb.AppendLine($"{indent}    var allocated = configHolderData.Data.BlobContainer.AllocArray<{unmanagedElemType}>({managedFieldAccess}.Count);");
                    sb.AppendLine($"{indent}    {unmanagedVar}.{fieldName} = allocated;");
                    
                    // 检查元素类型是否还是容器
                    if (IsContainerType(elemType))
                    {
                        sb.AppendLine();
                        sb.AppendLine($"{indent}    // 分配嵌套容器");
                        sb.AppendLine($"{indent}    for (int i{level} = 0; i{level} < {managedFieldAccess}.Count; i{level}++)");
                        sb.AppendLine($"{indent}    {{");
                        var nestedAccess = $"{managedFieldAccess}[i{level}]";
                        // 传递字段名作为起始路径（不带 data. 前缀）
                        GenerateContainerAllocCode(sb, fieldName, elemType, managedVar, unmanagedVar, unmanagedTypeName, nestedAccess, level + 1, typeInfo, $"i{level}", fieldName);
                        sb.AppendLine($"{indent}    }}");
                    }
                    
                    sb.AppendLine($"{indent}}}");
                }
                else if (level == 1)
                {
                    // Level 1: 在父层循环内，为每个元素分配并赋值（需要在循环内获取和写回 data）
                    var varName = $"nested{level}";
                    sb.AppendLine($"{indent}if ({managedFieldAccess} != null && {managedFieldAccess}.Count > 0)");
                    sb.AppendLine($"{indent}{{");
                    sb.AppendLine($"{indent}    var {varName} = configHolderData.Data.BlobContainer.AllocArray<{unmanagedElemType}>({managedFieldAccess}.Count);");
                    
                    // 检查元素类型是否还是容器
                    if (IsContainerType(elemType))
                    {
                        sb.AppendLine();
                        sb.AppendLine($"{indent}    // 分配更深层的嵌套容器");
                        sb.AppendLine($"{indent}    for (int i{level} = 0; i{level} < {managedFieldAccess}.Count; i{level}++)");
                        sb.AppendLine($"{indent}    {{");
                        var nestedAccess = $"{managedFieldAccess}[i{level}]";
                        // 传递当前层的变量名，子层会直接赋值到这个变量
                        GenerateContainerAllocCode(sb, fieldName, elemType, managedVar, unmanagedVar, unmanagedTypeName, nestedAccess, level + 1, typeInfo, $"i{level}", varName);
                        sb.AppendLine($"{indent}    }}");
                    }
                    
                    // Level 1: 赋值到顶层 data 并写回（在父层循环内，每次迭代都要执行）
                    sb.AppendLine();
                    sb.AppendLine($"{indent}    // 将分配的容器赋值到顶层数据");
                    var iteratorKey = parentIteratorKey ?? $"i{level - 1}";
                    sb.AppendLine($"{indent}    var map{level} = configHolderData.Data.GetMap<CfgI, {unmanagedTypeName}>(_definedInMod);");
                    sb.AppendLine($"{indent}    if (!map{level}.TryGetValue(configHolderData.Data.BlobContainer, cfgi, out var data{level}))");
                    sb.AppendLine($"{indent}    {{");
                    sb.AppendLine($"{indent}        XM.XLog.Error($\"[Config] 配置 {{cfgi}} 不存在于表中，无法分配嵌套容器\");");
                    sb.AppendLine($"{indent}        return;");
                    sb.AppendLine($"{indent}    }}");
                    sb.AppendLine($"{indent}    data{level}.{fieldName}[configHolderData.Data.BlobContainer, {iteratorKey}] = {varName};");
                    sb.AppendLine($"{indent}    map{level}[configHolderData.Data.BlobContainer, cfgi] = data{level};");
                    sb.AppendLine($"{indent}}}");
                }
                else
                {
                    // Level >= 2: 直接赋值到父层变量（不需要访问 map）
                    var varName = $"nested{level}";
                    sb.AppendLine($"{indent}if ({managedFieldAccess} != null && {managedFieldAccess}.Count > 0)");
                    sb.AppendLine($"{indent}{{");
                    sb.AppendLine($"{indent}    var {varName} = configHolderData.Data.BlobContainer.AllocArray<{unmanagedElemType}>({managedFieldAccess}.Count);");
                    
                    // 检查元素类型是否还是容器
                    if (IsContainerType(elemType))
                    {
                        sb.AppendLine();
                        sb.AppendLine($"{indent}    // 分配更深层的嵌套容器");
                        sb.AppendLine($"{indent}    for (int i{level} = 0; i{level} < {managedFieldAccess}.Count; i{level}++)");
                        sb.AppendLine($"{indent}    {{");
                        var nestedAccess = $"{managedFieldAccess}[i{level}]";
                        // 传递当前层的变量名，子层会直接赋值到这个变量
                        GenerateContainerAllocCode(sb, fieldName, elemType, managedVar, unmanagedVar, unmanagedTypeName, nestedAccess, level + 1, typeInfo, $"i{level}", varName);
                        sb.AppendLine($"{indent}    }}");
                    }
                    
                    // Level >= 2: 直接赋值到父层变量
                    var iteratorKey = parentIteratorKey ?? $"i{level - 1}";
                    sb.AppendLine($"{indent}    {parentFieldPath}[configHolderData.Data.BlobContainer, {iteratorKey}] = {varName};");
                    sb.AppendLine($"{indent}}}");
                }
            }
            // Dictionary<K,V> -> XBlobMap<K,V>
            else if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                var keyType = fieldType.GetGenericArguments()[0];
                var valType = fieldType.GetGenericArguments()[1];
                var keyUnmanagedType = GetUnmanagedTypeName(keyType, typeInfo);
                var valUnmanagedType = GetUnmanagedTypeName(valType, typeInfo);
                
                if (level == 0)
                {
                    // 第一层：分配并直接赋值到传入的 ref data
                    sb.AppendLine($"{indent}if ({managedFieldAccess} != null && {managedFieldAccess}.Count > 0)");
                    sb.AppendLine($"{indent}{{");
                    sb.AppendLine($"{indent}    var allocated = configHolderData.Data.BlobContainer.AllocMap<{keyUnmanagedType}, {valUnmanagedType}>({managedFieldAccess}.Count);");
                    sb.AppendLine($"{indent}    {unmanagedVar}.{fieldName} = allocated;");
                    
                    // 检查 value 类型是否还是容器
                    if (IsContainerType(valType))
                    {
                        sb.AppendLine();
                        sb.AppendLine($"{indent}    // 分配嵌套容器");
                        sb.AppendLine($"{indent}    foreach (var kvp{level} in {managedFieldAccess})");
                        sb.AppendLine($"{indent}    {{");
                        
                        // 添加键类型转换代码（如果需要）
                        AppendKeyConversion(sb, keyType, $"kvp{level}.Key", $"kvp{level}", $"{indent}        ", typeInfo, out var convertedVarName);
                        var convertedKey = GetConvertedKeyExpression(keyType, $"kvp{level}.Key", convertedVarName, keyUnmanagedType);
                        
                        var nestedAccess = $"kvp{level}.Value";
                        // 传递字段名作为起始路径（不带 data. 前缀）
                        GenerateContainerAllocCode(sb, fieldName, valType, managedVar, unmanagedVar, unmanagedTypeName, nestedAccess, level + 1, typeInfo, convertedKey, fieldName);
                        sb.AppendLine($"{indent}    }}");
                    }
                    
                    sb.AppendLine($"{indent}}}");
                }
                else if (level == 1)
                {
                    // Level 1: 在父层循环内，为每个元素分配并赋值（需要在循环内获取和写回 data）
                    var varName = $"nested{level}";
                    sb.AppendLine($"{indent}if ({managedFieldAccess} != null && {managedFieldAccess}.Count > 0)");
                    sb.AppendLine($"{indent}{{");
                    sb.AppendLine($"{indent}    var {varName} = configHolderData.Data.BlobContainer.AllocMap<{keyUnmanagedType}, {valUnmanagedType}>({managedFieldAccess}.Count);");
                    
                    // 检查 value 类型是否还是容器
                    if (IsContainerType(valType))
                    {
                        sb.AppendLine();
                        sb.AppendLine($"{indent}    // 分配更深层的嵌套容器");
                        sb.AppendLine($"{indent}    foreach (var kvp{level} in {managedFieldAccess})");
                        sb.AppendLine($"{indent}    {{");
                        
                        // 添加键类型转换代码（如果需要）
                        AppendKeyConversion(sb, keyType, $"kvp{level}.Key", $"kvp{level}", $"{indent}        ", typeInfo, out var convertedVarName);
                        var convertedKey = GetConvertedKeyExpression(keyType, $"kvp{level}.Key", convertedVarName, keyUnmanagedType);
                        
                        var nestedAccess = $"kvp{level}.Value";
                        // 传递当前层的变量名，子层会直接赋值到这个变量
                        GenerateContainerAllocCode(sb, fieldName, valType, managedVar, unmanagedVar, unmanagedTypeName, nestedAccess, level + 1, typeInfo, convertedKey, varName);
                        sb.AppendLine($"{indent}    }}");
                    }
                    
                    // Level 1: 赋值到顶层 data 并写回（在父层循环内，每次迭代都要执行）
                    sb.AppendLine();
                    sb.AppendLine($"{indent}    // 将分配的容器赋值到顶层数据");
                    var iteratorKey = parentIteratorKey ?? $"kvp{level - 1}.Key";
                    sb.AppendLine($"{indent}    var map{level} = configHolderData.Data.GetMap<CfgI, {unmanagedTypeName}>(_definedInMod);");
                    sb.AppendLine($"{indent}    if (!map{level}.TryGetValue(configHolderData.Data.BlobContainer, cfgi, out var data{level}))");
                    sb.AppendLine($"{indent}    {{");
                    sb.AppendLine($"{indent}        XM.XLog.Error($\"[Config] 配置 {{cfgi}} 不存在于表中，无法分配嵌套容器\");");
                    sb.AppendLine($"{indent}        return;");
                    sb.AppendLine($"{indent}    }}");
                    sb.AppendLine($"{indent}    data{level}.{fieldName}[configHolderData.Data.BlobContainer, {iteratorKey}] = {varName};");
                    sb.AppendLine($"{indent}    map{level}[configHolderData.Data.BlobContainer, cfgi] = data{level};");
                    sb.AppendLine($"{indent}}}");
                }
                else
                {
                    // Level >= 2: 直接赋值到父层变量（不需要访问 map）
                    var varName = $"nested{level}";
                    sb.AppendLine($"{indent}if ({managedFieldAccess} != null && {managedFieldAccess}.Count > 0)");
                    sb.AppendLine($"{indent}{{");
                    sb.AppendLine($"{indent}    var {varName} = configHolderData.Data.BlobContainer.AllocMap<{keyUnmanagedType}, {valUnmanagedType}>({managedFieldAccess}.Count);");
                    
                    // 检查 value 类型是否还是容器
                    if (IsContainerType(valType))
                    {
                        sb.AppendLine();
                        sb.AppendLine($"{indent}    // 分配更深层的嵌套容器");
                        sb.AppendLine($"{indent}    foreach (var kvp{level} in {managedFieldAccess})");
                        sb.AppendLine($"{indent}    {{");
                        
                        // 添加键类型转换代码（如果需要）
                        AppendKeyConversion(sb, keyType, $"kvp{level}.Key", $"kvp{level}", $"{indent}        ", typeInfo, out var convertedVarName);
                        var convertedKey = GetConvertedKeyExpression(keyType, $"kvp{level}.Key", convertedVarName, keyUnmanagedType);
                        
                        var nestedAccess = $"kvp{level}.Value";
                        // 传递当前层的变量名，子层会直接赋值到这个变量
                        GenerateContainerAllocCode(sb, fieldName, valType, managedVar, unmanagedVar, unmanagedTypeName, nestedAccess, level + 1, typeInfo, convertedKey, varName);
                        sb.AppendLine($"{indent}    }}");
                    }
                    
                    // Level >= 2: 直接赋值到父层变量
                    var iteratorKey = parentIteratorKey ?? $"kvp{level - 1}.Key";
                    sb.AppendLine($"{indent}    {parentFieldPath}[configHolderData.Data.BlobContainer, {iteratorKey}] = {varName};");
                    sb.AppendLine($"{indent}}}");
                }
            }
            // HashSet<T> -> XBlobSet<T>
            else if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(HashSet<>))
            {
                var elemType = fieldType.GetGenericArguments()[0];
                var unmanagedElemType = GetUnmanagedTypeName(elemType, typeInfo);
                
                // HashSet 不太可能嵌套容器，所以简化处理
                if (level == 0)
                {
                    sb.AppendLine($"{indent}if ({managedFieldAccess} != null && {managedFieldAccess}.Count > 0)");
                    sb.AppendLine($"{indent}{{");
                    sb.AppendLine($"{indent}    var allocated = configHolderData.Data.BlobContainer.AllocSet<{unmanagedElemType}>({managedFieldAccess}.Count);");
                    sb.AppendLine($"{indent}    {unmanagedVar}.{fieldName} = allocated;");
                    sb.AppendLine($"{indent}}}");
                }
            }
        }

        /// <summary>构建嵌套容器的访问路径</summary>
        private static string BuildNestedContainerAccessPath(string fieldName, int level)
        {
            // level 0: fieldName
            // level 1: fieldName.GetRef(...)[i0]
            // level 2: fieldName.GetRef(...)[i0].GetRef(...)[i1]
            if (level == 0)
                return fieldName;
            
            // 简化：只返回字段名，实际访问路径在生成时构建
            return fieldName;
        }

        /// <summary>获取托管类型对应的 Unmanaged 类型名称</summary>
        private static string GetUnmanagedTypeName(Type type)
        {
            return GetUnmanagedTypeName(type, null);
        }

        /// <summary>获取托管类型对应的 Unmanaged 类型名称（支持自定义转换器）</summary>
        private static string GetUnmanagedTypeName(Type type, ConfigTypeInfo typeInfo)
        {
            if (type == null) return "object";
            
            // 检查是否有自定义转换器
            if (typeInfo != null && TryGetConverterTargetType(type, typeInfo, out var targetTypeName))
            {
                // 递归获取目标类型的 unmanaged 类型名称
                return targetTypeName;
            }
            
            // 基本类型
            if (type == typeof(int)) return "Int32";
            if (type == typeof(long)) return "Int64";
            if (type == typeof(short)) return "Int16";
            if (type == typeof(byte)) return "Byte";
            if (type == typeof(bool)) return "Boolean";
            if (type == typeof(float)) return "Single";
            if (type == typeof(double)) return "Double";
            if (type == typeof(decimal)) return "Decimal";
            if (type == typeof(string)) return "StrI";
            
            // CfgS<T> -> CfgI<T>
            if (type.IsGenericType && type.GetGenericTypeDefinition().Name == "CfgS`1")
            {
                var innerType = type.GetGenericArguments()[0];
                // 如果 innerType 已经是 UnManaged 类型（以 "UnManaged" 结尾），直接使用
                var innerUnmanagedName = innerType.Name.EndsWith("UnManaged") 
                    ? innerType.Name 
                    : innerType.Name + "UnManaged";
                return $"CfgI<{innerUnmanagedName}>";
            }
            
            // List<T> -> XBlobArray<T>
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                var elemType = type.GetGenericArguments()[0];
                return $"XBlobArray<{GetUnmanagedTypeName(elemType, typeInfo)}>";
            }
            
            // Dictionary<K,V> -> XBlobMap<K,V>
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                var keyType = type.GetGenericArguments()[0];
                var valType = type.GetGenericArguments()[1];
                return $"XBlobMap<{GetUnmanagedTypeName(keyType, typeInfo)}, {GetUnmanagedTypeName(valType, typeInfo)}>";
            }
            
            // HashSet<T> -> XBlobSet<T>
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(HashSet<>))
            {
                var elemType = type.GetGenericArguments()[0];
                return $"XBlobSet<{GetUnmanagedTypeName(elemType, typeInfo)}>";
            }
            
            // IXConfig -> UnManaged
            if (IsXConfigType(type))
            {
                return type.Name + "UnManaged";
            }
            
            // 枚举类型和其他值类型：直接返回类型名
            if (type.IsEnum || type.IsValueType)
            {
                return type.Name;
            }
            
            // 其他类型（可能是自定义类）：尝试返回完整类型名，避免泛型参数名
            if (type.IsGenericParameter)
            {
                // 如果是泛型参数（如 T），返回参数名
                return type.Name;
            }
            
            return type.Name;
        }

        /// <summary>构建需在 Helper 构造函数中注册的转换器列表，供模板生成 TypeConverterRegistry.RegisterLocalConverter 调用。</summary>
        private static List<ScriptObject> BuildConverterRegistrations(ConfigTypeInfo typeInfo)
        {
            var list = new List<ScriptObject>();
            foreach (var field in typeInfo.Fields)
            {
                if (!field.NeedsConverter || string.IsNullOrEmpty(field.ConverterTypeName) || string.IsNullOrEmpty(field.TargetType)) continue;
                var shortName = field.ConverterTypeName;
                if (shortName.Contains(".")) shortName = shortName.Substring(shortName.LastIndexOf('.') + 1);
                var domainEscaped = (field.ConverterDomain ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");
                list.Add(new ScriptObject
                {
                    ["source_type"] = field.SourceType ?? "string",
                    ["target_type"] = field.TargetType,
                    ["domain_escaped"] = domainEscaped,
                    ["converter_type_name"] = shortName
                });
            }
            return list;
        }

        /// <summary>从程序集读取 [ModName] 特性，生成时静态解析，供模板直接赋字符串（无运行时反射）。</summary>
        private static string GetModNameFromAssembly(Assembly assembly)
        {
            if (assembly == null) return "Default";
            try
            {
                var attrType = assembly.GetType("XM.Contracts.ModNameAttribute");
                if (attrType == null)
                {
                    foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        attrType = a.GetType("XM.Contracts.ModNameAttribute");
                        if (attrType != null) break;
                    }
                }
                if (attrType == null) return "Default";
                var attr = Attribute.GetCustomAttribute(assembly, attrType);
                if (attr == null) return "Default";
                var prop = attrType.GetProperty("ModName");
                var v = prop?.GetValue(attr) as string;
                return !string.IsNullOrEmpty(v) ? v : "Default";
            }
            catch { return "Default"; }
        }

        /// <summary>将 Editor 侧类型信息与生成结果转为 Toolkit 用 DTO，供 XModToolkit 渲染（不依赖 Unity）。</summary>
        private static ConfigTypeInfoDto ToClassHelperDto(ConfigTypeInfo typeInfo, List<ScriptObject> fieldAssigns, List<ScriptObject> converterRegistrations)
        {
            var dto = new ConfigTypeInfoDto
            {
                Namespace = typeInfo.Namespace ?? "",
                ManagedTypeName = typeInfo.ManagedTypeName,
                UnmanagedTypeName = typeInfo.UnmanagedTypeName,
                TableName = typeInfo.TableName ?? typeInfo.ManagedTypeName,
                RequiredUsings = typeInfo.RequiredUsings?.OrderBy(x => x).ToList() ?? new List<string>()
            };
            foreach (var o in fieldAssigns ?? new List<ScriptObject>())
            {
                dto.FieldAssigns.Add(new FieldAssignDto
                {
                    CallCode = o["call_code"]?.ToString() ?? "",
                    MethodCode = o["method_code"]?.ToString() ?? ""
                });
            }
            foreach (var o in converterRegistrations ?? new List<ScriptObject>())
            {
                dto.ConverterRegistrations.Add(new ConverterRegistrationDto
                {
                    SourceType = o["source_type"]?.ToString() ?? "string",
                    TargetType = o["target_type"]?.ToString() ?? "",
                    DomainEscaped = o["domain_escaped"]?.ToString() ?? "",
                    ConverterTypeName = o["converter_type_name"]?.ToString() ?? ""
                });
            }
            return dto;
        }

        /// <summary>生成代码使用文件顶部 using 解析类型，不再使用 global::，避免 CS7000（Unexpected use of an aliased name）。</summary>
        private static string ToGlobal(string code)
        {
            return code ?? string.Empty;
        }

        /// <summary>供生成代码使用的“空值”块：必要字段告警 [XmlNotNull] + 默认值 [XmlDefault(str)]；标量有效，容器仅支持告警。</summary>
        private static string GetEmptyValueBlock(string fieldName, FieldInfo field)
        {
            if (field == null || (!field.IsNotNull && string.IsNullOrEmpty(field.DefaultValueString))) return "";
            var sb = new StringBuilder();
            sb.Append("if (string.IsNullOrEmpty(s)) { ");
            if (field.IsNotNull)
                sb.Append("ConfigParseHelper.LogParseWarning(\"").Append(fieldName.Replace("\\", "\\\\").Replace("\"", "\\\"")).Append("\", s ?? \"\", null); ");
            if (!string.IsNullOrEmpty(field.DefaultValueString))
                sb.Append("s = \"").Append(EscapeForCSharpStringLiteral(field.DefaultValueString)).Append("\"; ");
            sb.Append("}");
            return sb.ToString();
        }

        private static string EscapeForCSharpStringLiteral(string s)
        {
            if (string.IsNullOrEmpty(s)) return "";
            return s.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\r", "\\r").Replace("\n", "\\n");
        }

        /// <summary>尝试获取类型的自定义转换器目标类型（unmanaged 类型名称）</summary>
        private static bool TryGetConverterTargetType(Type type, ConfigTypeInfo typeInfo, out string unmanagedTypeName)
        {
            unmanagedTypeName = null;
            if (type == null) return false;
            
            // 1. 在字段中查找是否有该类型的转换器定义（字段级别转换器）
            if (typeInfo != null)
            {
                foreach (var field in typeInfo.Fields)
                {
                    if (!field.NeedsConverter) continue;
                    var fieldInfo = typeInfo.ManagedType?.GetField(field.Name);
                    if (fieldInfo == null) continue;
                    
                    // 检查字段类型是否匹配（直接类型或容器的元素/键/值类型）
                    if (fieldInfo.FieldType == type || IsTypeInContainer(fieldInfo.FieldType, type))
                    {
                        // 找到了转换器，TargetType 是托管类型名称
                        // 对于大多数情况（枚举、值类型），托管类型名就是 unmanaged 类型名
                        unmanagedTypeName = field.TargetType;
                        return !string.IsNullOrEmpty(unmanagedTypeName);
                    }
                }
            }
            
            // 2. 检查全局转换器（程序集级别转换器）
            if (typeInfo?.ManagedType?.Assembly != null)
            {
                // 2.1 尝试查找 type -> target 的转换器（支持任意类型对）
                if (TypeAnalyzer.TryGetConverterTargetBySourceType(typeInfo.ManagedType.Assembly, type, out var targetType, out var _))
                {
                    unmanagedTypeName = targetType.Name;
                    Debug.Log($"[ClassHelperCodeGenerator] 通过转换器查找: {type.Name} -> {targetType.Name}");
                    return true;
                }
                else
                {
                    Debug.Log($"[ClassHelperCodeGenerator] 未找到转换器: {type.Name} (Assembly: {typeInfo.ManagedType.Assembly.GetName().Name})");
                }
                
                // 2.2 向后兼容：查找 string -> type 的转换器
                var domain = TypeAnalyzer.GetConverterDomainForType(typeInfo.ManagedType.Assembly, type);
                if (domain != null)
                {
                    // 找到了全局转换器（string -> type），目标类型就是 type 本身
                    if (type.IsEnum || type.IsValueType)
                    {
                        // 枚举和值类型直接使用类型名
                        unmanagedTypeName = type.Name;
                        return true;
                    }
                    // 其他情况尝试使用 "TypeName + I" 模式
                    unmanagedTypeName = type.Name + "I";
                    return true;
                }
            }
            
            return false;
        }

        /// <summary>检查 containerType 是否包含 targetType（递归检查嵌套容器）</summary>
        private static bool IsTypeInContainer(Type containerType, Type targetType)
        {
            if (!containerType.IsGenericType) return false;
            
            var genericDef = containerType.GetGenericTypeDefinition();
            
            // List<T> 或 HashSet<T>
            if (genericDef == typeof(List<>) || genericDef == typeof(HashSet<>))
            {
                var elemType = containerType.GetGenericArguments()[0];
                if (elemType == targetType) return true;
                // 递归检查元素类型（如果元素也是容器）
                return IsTypeInContainer(elemType, targetType);
            }
            // Dictionary<K,V>
            else if (genericDef == typeof(Dictionary<,>))
            {
                var args = containerType.GetGenericArguments();
                var keyType = args[0];
                var valType = args[1];
                
                // 检查键类型
                if (keyType == targetType) return true;
                // 递归检查键类型（如果键也是容器）
                if (IsTypeInContainer(keyType, targetType)) return true;
                
                // 检查值类型
                if (valType == targetType) return true;
                // 递归检查值类型（如果值也是容器）
                return IsTypeInContainer(valType, targetType);
            }
            
            return false;
        }

        /// <summary>检测元素类型是否已注册自定义解析器（程序集或本类型字段），若已注册则返回域与类型名，供容器/嵌套容器统一使用。</summary>
        private static bool TryGetElementConverter(Type elemType, ConfigTypeInfo typeInfo, out string domain, out string elemTypeName)
        {
            elemTypeName = GetCSharpTypeName(elemType);
            domain = null;
            if (elemType == null || typeInfo?.ManagedType == null) return false;
            foreach (var f in typeInfo.Fields)
            {
                if (!f.NeedsConverter) continue;
                var ft = typeInfo.ManagedType.GetField(f.Name)?.FieldType;
                if (ft == elemType)
                {
                    domain = f.ConverterDomain ?? "";
                    return true;
                }
            }
            domain = TypeAnalyzer.GetConverterDomainForType(typeInfo.ManagedType.Assembly, elemType);
            if (domain != null) return true;
            domain = null;
            return false;
        }

        /// <summary>根据 Type 拼接 C# 类型名，不硬编码；便于维护与扩展。</summary>
        private static string GetCSharpTypeName(Type type)
        {
            if (type == null) return "object";
            if (type == typeof(int)) return "int";
            if (type == typeof(long)) return "long";
            if (type == typeof(short)) return "short";
            if (type == typeof(byte)) return "byte";
            if (type == typeof(bool)) return "bool";
            if (type == typeof(float)) return "float";
            if (type == typeof(double)) return "double";
            if (type == typeof(decimal)) return "decimal";
            if (type == typeof(string)) return "string";
            if (!type.IsGenericType) return type.Name;
            var name = type.Name;
            var idx = name.IndexOf('`');
            if (idx > 0) name = name.Substring(0, idx);
            var args = string.Join(", ", type.GetGenericArguments().Select(GetCSharpTypeName));
            return name + "<" + args + ">";
        }

        private static (string callCode, string methodCode) GetParseMethodCode(string fieldName, string parseName, Type fieldType, ConfigTypeInfo typeInfo, FieldInfo field)
        {
            var typeName = GetCSharpTypeName(fieldType);

            // 基本类型：调用基类通用解析方法（含 [XmlNotNull] 告警与 [XmlDefault] 默认值）
            if (fieldType == typeof(int) || fieldType == typeof(long) || fieldType == typeof(short) || fieldType == typeof(byte) ||
                fieldType == typeof(float) || fieldType == typeof(double) || fieldType == typeof(bool) || fieldType == typeof(decimal))
            {
                var tryParse = fieldType == typeof(int) ? "TryParseInt" : fieldType == typeof(long) ? "TryParseLong" : fieldType == typeof(short) ? "TryParseShort" : fieldType == typeof(byte) ? "TryParseByte"
                    : fieldType == typeof(float) ? "TryParseFloat" : fieldType == typeof(double) ? "TryParseDouble" : fieldType == typeof(bool) ? "TryParseBool" : "TryParseDecimal";
                var defaultVal = fieldType == typeof(bool) ? "false" : "default";
                var emptyBlock = GetEmptyValueBlock(fieldName, field);
                var body = $"var s = ConfigParseHelper.GetXmlFieldValue(configItem, \"{fieldName}\");\n        " + (string.IsNullOrEmpty(emptyBlock) ? "" : emptyBlock + "\n        ") + $"if (string.IsNullOrEmpty(s)) return {defaultVal};\n        return ConfigParseHelper.{tryParse}(s, \"{fieldName}\", out var v) ? v : {defaultVal};";
                return ($"config.{fieldName} = {parseName}(configItem, mod, configName, context);", ToGlobal($"private static {typeName} {parseName}(XmlElement configItem, ModS mod, string configName,\n        in ConfigParseContext context)\n    {{\n        {body}\n    }}"));
            }

            if (fieldType == typeof(string))
            {
                var emptyBlock = GetEmptyValueBlock(fieldName, field);
                var body = $"var s = ConfigParseHelper.GetXmlFieldValue(configItem, \"{fieldName}\");\n        " + (string.IsNullOrEmpty(emptyBlock) ? "" : emptyBlock + "\n        ") + "return s ?? \"\";";
                return ($"config.{fieldName} = {parseName}(configItem, mod, configName, context);", ToGlobal($"private static {typeName} {parseName}(XmlElement configItem, ModS mod, string configName,\n        in ConfigParseContext context)\n    {{\n        {body}\n    }}"));
            }

            // CfgS<T>：使用基类 TryParseCfgSString（含 [XmlNotNull]/[XmlDefault]）
            if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition().Name == "CfgS`1")
            {
                var tUnmanaged = GetCSharpTypeName(fieldType.GetGenericArguments()[0]);
                var emptyBlock = GetEmptyValueBlock(fieldName, field);
                var body = $"var s = ConfigParseHelper.GetXmlFieldValue(configItem, \"{fieldName}\");\n            " + (string.IsNullOrEmpty(emptyBlock) ? "" : emptyBlock + "\n            ") + $"if (string.IsNullOrEmpty(s)) return default;\n            if (!ConfigParseHelper.TryParseCfgSString(s, \"{fieldName}\", out var modName, out var cfgName))\n                return default;\n            return new CfgS<{tUnmanaged}>(new ModS(modName), cfgName);";
                return ($"config.{fieldName} = {parseName}(configItem, mod, configName, context);", ToGlobal($"private static {typeName} {parseName}(XmlElement configItem, ModS mod, string configName,\n        in ConfigParseContext context)\n    {{\n        try\n        {{\n            {body}\n        }}\n        catch (Exception ex)\n        {{\n            {GetParseCatchBlock(fieldName, $"ConfigParseHelper.GetXmlFieldValue(configItem, \"{fieldName}\")")}\n            return default;\n        }}\n    }}"));
            }

            // 嵌套 IXConfig：try-catch + 日志；[XmlNotNull] 时子节点缺失打告警
            if (IsXConfigType(fieldType))
            {
                var nestedName = GetCSharpTypeName(fieldType);
                var nullBlock = field != null && field.IsNotNull
                    ? $"if (el == null) {{ ConfigParseHelper.LogParseWarning(\"{fieldName.Replace("\"", "\\\"")}\", \"\", null); return null; }}"
                    : "if (el == null) return null;";
                var body = $"var el = configItem.SelectSingleNode(\"{fieldName}\") as System.Xml.XmlElement;\n            {nullBlock}\n            var helper = XM.Contracts.IConfigDataCenter.I?.GetClassHelper(typeof({nestedName}));\n            return helper != null\n                ? ({nestedName})helper.DeserializeConfigFromXml(el, mod, configName + \"_{fieldName}\", context)\n                : null;";
                return ($"config.{fieldName} = {parseName}(configItem, mod, configName, context);", ToGlobal($"private static {typeName} {parseName}(XmlElement configItem, ModS mod, string configName,\n        in ConfigParseContext context)\n    {{\n        try\n        {{\n            {body}\n        }}\n        catch (Exception ex)\n        {{\n            {GetParseCatchBlock(fieldName, "null")}\n            return null;\n        }}\n    }}"));
            }

            // List<T>：try-catch + 日志，类型名由 GetCSharpTypeName 拼接
            if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var elemType = fieldType.GetGenericArguments()[0];
                var elemName = GetCSharpTypeName(elemType);
                string body;
                if (elemType == typeof(int))
                    body = $"var list = new {typeName}();\n            var nodes = configItem.SelectNodes(\"{fieldName}\");\n            if (nodes != null)\n            foreach (System.Xml.XmlNode n in nodes) {{ var t = (n as System.Xml.XmlElement)?.InnerText?.Trim(); if (!string.IsNullOrEmpty(t) && ConfigParseHelper.TryParseInt(t, \"{fieldName}\", out var vi)) list.Add(vi); }}\n            if (list.Count == 0) {{ var csv = ConfigParseHelper.GetXmlFieldValue(configItem, \"{fieldName}\"); if (!string.IsNullOrEmpty(csv)) foreach (var p in csv.Split(',', ';')) if (!string.IsNullOrWhiteSpace(p) && ConfigParseHelper.TryParseInt(p.Trim(), \"{fieldName}\", out var vi)) list.Add(vi); }}\n            return list;";
                else if (elemType == typeof(string))
                    body = $"var list = new {typeName}();\n            var nodes = configItem.SelectNodes(\"{fieldName}\");\n            if (nodes != null)\n            foreach (System.Xml.XmlNode n in nodes) {{ var t = (n as System.Xml.XmlElement)?.InnerText?.Trim(); if (t != null) list.Add(t); }}\n            return list;";
                else if (elemType.IsGenericType && elemType.GetGenericTypeDefinition().Name == "CfgS`1")
                {
                    var tU = GetCSharpTypeName(elemType.GetGenericArguments()[0]);
                    body = $"var list = new {typeName}();\n            var nodes = configItem.SelectNodes(\"{fieldName}\");\n            if (nodes != null)\n            foreach (System.Xml.XmlNode n in nodes) {{ var t = (n as System.Xml.XmlElement)?.InnerText?.Trim(); if (!string.IsNullOrEmpty(t) && ConfigParseHelper.TryParseCfgSString(t, \"{fieldName}\", out var mn, out var cn)) list.Add(new CfgS<{tU}>(new ModS(mn), cn)); }}\n            return list;";
                }
                else if (IsXConfigType(elemType))
                {
                    var nestedName = GetCSharpTypeName(elemType);
                    body = $"var list = new {typeName}();\n            var dc = XM.Contracts.IConfigDataCenter.I; if (dc == null) return list;\n            var nodes = configItem.SelectNodes(\"{fieldName}\");\n            if (nodes != null)\n            foreach (System.Xml.XmlNode n in nodes) {{ var el = n as System.Xml.XmlElement; if (el == null) continue; var helper = dc.GetClassHelper(typeof({nestedName})); if (helper != null) {{ var item = ({nestedName})helper.DeserializeConfigFromXml(el, mod, configName + \"_{fieldName}_\" + list.Count); if (item != null) list.Add(item); }} }}\n            return list;";
                }
                else if (TryGetElementConverter(elemType, typeInfo, out var listConvDomain, out var listElemName))
                {
                    var listDomainEscaped = (listConvDomain ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");
                    body = $"var list = new {typeName}();\n            var nodes = configItem.SelectNodes(\"{fieldName}\");\n            if (nodes != null)\n            foreach (System.Xml.XmlNode n in nodes) {{ var t = (n as System.Xml.XmlElement)?.InnerText?.Trim(); if (!string.IsNullOrEmpty(t)) {{ var converter = XM.Contracts.IConfigDataCenter.I?.GetConverterByType<string, {listElemName}>(); if (converter != null && converter.Convert(t, out var result)) list.Add(result); }} }}\n            if (list.Count == 0) {{ var csv = ConfigParseHelper.GetXmlFieldValue(configItem, \"{fieldName}\"); if (!string.IsNullOrEmpty(csv)) foreach (var p in csv.Split(',', ';')) if (!string.IsNullOrWhiteSpace(p)) {{ var converter = XM.Contracts.IConfigDataCenter.I?.GetConverterByType<string, {listElemName}>(); if (converter != null && converter.Convert(p.Trim(), out var result)) list.Add(result); }} }}\n            return list;";
                }
                else
                    return (null, null);
                if (field != null && field.IsNotNull)
                    body = body.Replace("return list;", "if (list.Count == 0) ConfigParseHelper.LogParseWarning(\"" + fieldName.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\", \"\", null);\n            return list;");
                return ($"config.{fieldName} = {parseName}(configItem, mod, configName, context);", ToGlobal($"private static {typeName} {parseName}(XmlElement configItem, ModS mod, string configName,\n        in ConfigParseContext context)\n    {{\n        try\n        {{\n            {body}\n        }}\n        catch (Exception ex)\n        {{\n            {GetParseCatchBlock(fieldName, "null")}\n            return new {typeName}();\n        }}\n    }}"));
            }

            // Dictionary<K,V>：try-catch + 日志，类型名由 GetCSharpTypeName 拼接；[XmlNotNull] 时空容器打告警
            if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                var keyType = fieldType.GetGenericArguments()[0];
                var valType = fieldType.GetGenericArguments()[1];
                string body;
                if (keyType == typeof(int) && valType == typeof(int))
                    body = $"var dict = new {typeName}();\n            var dictNodes = configItem.SelectNodes(\"{fieldName}/Item\");\n            if (dictNodes != null)\n            foreach (System.Xml.XmlNode n in dictNodes) {{ var el = n as System.Xml.XmlElement; if (el == null) continue; var k = el.GetAttribute(\"Key\"); var v = el.InnerText?.Trim(); if (!string.IsNullOrEmpty(k) && !string.IsNullOrEmpty(v) && ConfigParseHelper.TryParseInt(k, \"{fieldName}.Key\", out var kv) && ConfigParseHelper.TryParseInt(v, \"{fieldName}.Value\", out var vv)) dict[kv] = vv; }}\n            return dict;";
                else if (keyType == typeof(int) && TryGetElementConverter(valType, typeInfo, out var dictValDomain, out var dictValName))
                {
                    var dictValDomainEscaped = (dictValDomain ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");
                    body = $"var dict = new {typeName}();\n            var dictNodes = configItem.SelectNodes(\"{fieldName}/Item\");\n            if (dictNodes != null)\n            foreach (System.Xml.XmlNode n in dictNodes) {{ var el = n as System.Xml.XmlElement; if (el == null) continue; var k = el.GetAttribute(\"Key\"); var v = el.InnerText?.Trim(); if (!string.IsNullOrEmpty(k) && ConfigParseHelper.TryParseInt(k, \"{fieldName}.Key\", out var kv) && !string.IsNullOrEmpty(v)) {{ var converter = XM.Contracts.IConfigDataCenter.I?.GetConverterByType<string, {dictValName}>(); if (converter != null && converter.Convert(v, out var vv)) dict[kv] = vv; }} }}\n            return dict;";
                }
                else if (keyType.IsGenericType && keyType.GetGenericTypeDefinition().Name == "CfgS`1" && valType.IsGenericType && valType.GetGenericTypeDefinition().Name == "CfgS`1")
                {
                    var tU = GetCSharpTypeName(keyType.GetGenericArguments()[0]);
                    body = $"var dict = new {typeName}();\n            var dictNodes = configItem.SelectNodes(\"{fieldName}/Item\");\n            if (dictNodes != null)\n            foreach (System.Xml.XmlNode n in dictNodes) {{ var el = n as System.Xml.XmlElement; if (el == null) continue; var kStr = el.GetAttribute(\"Key\") ?? (el.SelectSingleNode(\"Key\") as System.Xml.XmlElement)?.InnerText?.Trim(); var vStr = el.GetAttribute(\"Value\") ?? (el.SelectSingleNode(\"Value\") as System.Xml.XmlElement)?.InnerText?.Trim() ?? el.InnerText?.Trim(); if (!string.IsNullOrEmpty(kStr) && ConfigParseHelper.TryParseCfgSString(kStr, \"{fieldName}.Key\", out var km, out var kc) && !string.IsNullOrEmpty(vStr) && ConfigParseHelper.TryParseCfgSString(vStr, \"{fieldName}.Value\", out var vm, out var vc)) dict[new CfgS<{tU}>(new ModS(km), kc)] = new CfgS<{tU}>(new ModS(vm), vc); }}\n            return dict;";
                }
                else if (keyType == typeof(int) && valType.IsGenericType && valType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    var innerListType = valType.GetGenericArguments()[0];
                    if (innerListType.IsGenericType && innerListType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        var cfgType = innerListType.GetGenericArguments()[0];
                        if (cfgType.IsGenericType && cfgType.GetGenericTypeDefinition().Name == "CfgS`1")
                        {
                            var tU = GetCSharpTypeName(cfgType.GetGenericArguments()[0]);
                            var valTypeName = GetCSharpTypeName(valType);
                            var innerTypeName = GetCSharpTypeName(innerListType);
                            body = $"var dict = new {typeName}();\n            var dictNodes = configItem.SelectNodes(\"{fieldName}/Item\");\n            if (dictNodes != null)\n            foreach (System.Xml.XmlNode keyNode in dictNodes) {{ var keyEl = keyNode as System.Xml.XmlElement; if (keyEl == null) continue; var kStr = keyEl.GetAttribute(\"Key\"); if (!string.IsNullOrEmpty(kStr) && ConfigParseHelper.TryParseInt(kStr, \"{fieldName}.Key\", out var key)) {{ var outerList = new {valTypeName}(); var midNodes = keyEl.SelectNodes(\"Item\"); if (midNodes != null) foreach (System.Xml.XmlNode midNode in midNodes) {{ var midEl = midNode as System.Xml.XmlElement; if (midEl == null) continue; var innerList = new {innerTypeName}(); var leafNodes = midEl.SelectNodes(\"Item\"); if (leafNodes != null) foreach (System.Xml.XmlNode leafNode in leafNodes) {{ var leafText = (leafNode as System.Xml.XmlElement)?.InnerText?.Trim(); if (!string.IsNullOrEmpty(leafText) && ConfigParseHelper.TryParseCfgSString(leafText, \"{fieldName}\", out var lm, out var lc)) innerList.Add(new CfgS<{tU}>(new ModS(lm), lc)); }} outerList.Add(innerList); }} dict[key] = outerList; }} }}\n            return dict;";
                        }
                        else if (TryGetElementConverter(cfgType, typeInfo, out var leafDomain, out var leafTypeName))
                        {
                            var leafDomainEscaped = (leafDomain ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");
                            var valTypeName = GetCSharpTypeName(valType);
                            var innerTypeName = GetCSharpTypeName(innerListType);
                            body = $"var dict = new {typeName}();\n            var dictNodes = configItem.SelectNodes(\"{fieldName}/Item\");\n            if (dictNodes != null)\n            foreach (System.Xml.XmlNode keyNode in dictNodes) {{ var keyEl = keyNode as System.Xml.XmlElement; if (keyEl == null) continue; var kStr = keyEl.GetAttribute(\"Key\"); if (!string.IsNullOrEmpty(kStr) && ConfigParseHelper.TryParseInt(kStr, \"{fieldName}.Key\", out var key)) {{ var outerList = new {valTypeName}(); var midNodes = keyEl.SelectNodes(\"Item\"); if (midNodes != null) foreach (System.Xml.XmlNode midNode in midNodes) {{ var midEl = midNode as System.Xml.XmlElement; if (midEl == null) continue; var innerList = new {innerTypeName}(); var leafNodes = midEl.SelectNodes(\"Item\"); if (leafNodes != null) foreach (System.Xml.XmlNode leafNode in leafNodes) {{ var leafText = (leafNode as System.Xml.XmlElement)?.InnerText?.Trim(); if (!string.IsNullOrEmpty(leafText)) {{ var converter = XM.Contracts.IConfigDataCenter.I?.GetConverterByType<string, {leafTypeName}>(); if (converter != null && converter.Convert(leafText, out var result)) innerList.Add(result); }} }} outerList.Add(innerList); }} dict[key] = outerList; }} }}\n            return dict;";
                        }
                        else
                            return (null, null);
                    }
                    else
                        return (null, null);
                }
                else
                    return (null, null);
                if (field != null && field.IsNotNull)
                    body = body.Replace("return dict;", "if (dict.Count == 0) ConfigParseHelper.LogParseWarning(\"" + fieldName.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\", \"\", null);\n            return dict;");
                return ($"config.{fieldName} = {parseName}(configItem, mod, configName, context);", ToGlobal($"private static {typeName} {parseName}(XmlElement configItem, ModS mod, string configName,\n        in ConfigParseContext context)\n    {{\n        try\n        {{\n            {body}\n        }}\n        catch (Exception ex)\n        {{\n            {GetParseCatchBlock(fieldName, "null")}\n            return new {typeName}();\n        }}\n    }}"));
            }

            // HashSet<T>：try-catch + 日志；支持已注册类型的自定义解析器；[XmlNotNull] 时空集合打告警
            if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(HashSet<>))
            {
                var elemType = fieldType.GetGenericArguments()[0];
                string body;
                if (elemType == typeof(int))
                    body = $"var set = new {typeName}();\n            var nodes = configItem.SelectNodes(\"{fieldName}\");\n            if (nodes != null)\n            foreach (System.Xml.XmlNode n in nodes) {{ var t = (n as System.Xml.XmlElement)?.InnerText?.Trim(); if (!string.IsNullOrEmpty(t) && ConfigParseHelper.TryParseInt(t, \"{fieldName}\", out var vi)) set.Add(vi); }}\n            if (set.Count == 0) {{ var csv = ConfigParseHelper.GetXmlFieldValue(configItem, \"{fieldName}\"); if (!string.IsNullOrEmpty(csv)) foreach (var p in csv.Split(',', ';')) if (!string.IsNullOrWhiteSpace(p) && ConfigParseHelper.TryParseInt(p.Trim(), \"{fieldName}\", out var vi)) set.Add(vi); }}\n            return set;";
                else if (elemType.IsGenericType && elemType.GetGenericTypeDefinition().Name == "CfgS`1")
                {
                    var tU = GetCSharpTypeName(elemType.GetGenericArguments()[0]);
                    body = $"var set = new {typeName}();\n            var nodes = configItem.SelectNodes(\"{fieldName}\");\n            if (nodes != null)\n            foreach (System.Xml.XmlNode n in nodes) {{ var t = (n as System.Xml.XmlElement)?.InnerText?.Trim(); if (!string.IsNullOrEmpty(t) && ConfigParseHelper.TryParseCfgSString(t, \"{fieldName}\", out var mn, out var cn)) set.Add(new CfgS<{tU}>(new ModS(mn), cn)); }}\n            return set;";
                }
                else if (TryGetElementConverter(elemType, typeInfo, out var setConvDomain, out var setElemName))
                {
                    var setDomainEscaped = (setConvDomain ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");
                    body = $"var set = new {typeName}();\n            var nodes = configItem.SelectNodes(\"{fieldName}\");\n            if (nodes != null)\n            foreach (System.Xml.XmlNode n in nodes) {{ var t = (n as System.Xml.XmlElement)?.InnerText?.Trim(); if (!string.IsNullOrEmpty(t)) {{ var converter = XM.Contracts.IConfigDataCenter.I?.GetConverterByType<string, {setElemName}>(); if (converter != null && converter.Convert(t, out var result)) set.Add(result); }} }}\n            if (set.Count == 0) {{ var csv = ConfigParseHelper.GetXmlFieldValue(configItem, \"{fieldName}\"); if (!string.IsNullOrEmpty(csv)) foreach (var p in csv.Split(',', ';')) if (!string.IsNullOrWhiteSpace(p)) {{ var converter = XM.Contracts.IConfigDataCenter.I?.GetConverterByType<string, {setElemName}>(); if (converter != null && converter.Convert(p.Trim(), out var result)) set.Add(result); }} }}\n            return set;";
                }
                else
                    return (null, null);
                if (field != null && field.IsNotNull)
                    body = body.Replace("return set;", "if (set.Count == 0) ConfigParseHelper.LogParseWarning(\"" + fieldName.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\", \"\", null);\n            return set;");
                return ($"config.{fieldName} = {parseName}(configItem, mod, configName, context);", ToGlobal($"private static {typeName} {parseName}(XmlElement configItem, ModS mod, string configName,\n        in ConfigParseContext context)\n    {{\n        try\n        {{\n            {body}\n        }}\n        catch (Exception ex)\n        {{\n            {GetParseCatchBlock(fieldName, $"ConfigParseHelper.GetXmlFieldValue(configItem, \"{fieldName}\")")}\n            return new {typeName}();\n        }}\n    }}"));
            }

            // LabelS：使用基类 TryParseLabelSString（含 [XmlNotNull]/[XmlDefault]）
            if (fieldType.Name == "LabelS")
            {
                var emptyBlock = GetEmptyValueBlock(fieldName, field);
                var body = $"var s = ConfigParseHelper.GetXmlFieldValue(configItem, \"{fieldName}\");\n            " + (string.IsNullOrEmpty(emptyBlock) ? "" : emptyBlock + "\n            ") + "if (string.IsNullOrEmpty(s)) return default;\n            if (!ConfigParseHelper.TryParseLabelSString(s, \"" + fieldName + "\", out var modName, out var labelName))\n                return default;\n            return new LabelS { ModName = modName, LabelName = labelName };";
                return ($"config.{fieldName} = {parseName}(configItem, mod, configName, context);", ToGlobal($"private static {typeName} {parseName}(XmlElement configItem, ModS mod, string configName,\n        in ConfigParseContext context)\n    {{\n        try\n        {{\n            {body}\n        }}\n        catch (Exception ex)\n        {{\n            {GetParseCatchBlock(fieldName, $"ConfigParseHelper.GetXmlFieldValue(configItem, \"{fieldName}\")")}\n            return default;\n        }}\n    }}"));
            }

            // 自定义转换器：从 IConfigDataCenter 按域获取转换器（含 [XmlNotNull]/[XmlDefault]）；有 domain 时用 GetConverter(domain)，否则用 GetConverterByType
            if (field.NeedsConverter && !string.IsNullOrEmpty(field.TargetType))
            {
                var domain = field.ConverterDomain ?? "";
                var domainEscaped = domain.Replace("\\", "\\\\").Replace("\"", "\\\"");
                var converterExpr = string.IsNullOrEmpty(domain)
                    ? $"XM.Contracts.IConfigDataCenter.I?.GetConverterByType<string, {typeName}>()"
                    : $"XM.Contracts.IConfigDataCenter.I?.GetConverter<string, {typeName}>(\"{domainEscaped}\")";
                var emptyBlock = GetEmptyValueBlock(fieldName, field);
                var body = $"var s = ConfigParseHelper.GetXmlFieldValue(configItem, \"{fieldName}\");\n            " + (string.IsNullOrEmpty(emptyBlock) ? "" : emptyBlock + "\n            ") + $"if (string.IsNullOrEmpty(s)) return default;\n            var converter = {converterExpr};\n            return converter != null && converter.Convert(s, out var result) ? result : default;";
                return ($"config.{fieldName} = {parseName}(configItem, mod, configName, context);", ToGlobal($"private static {typeName} {parseName}(XmlElement configItem, ModS mod, string configName,\n        in ConfigParseContext context)\n    {{\n        try\n        {{\n            {body}\n        }}\n        catch (Exception ex)\n        {{\n            {GetParseCatchBlock(fieldName, $"ConfigParseHelper.GetXmlFieldValue(configItem, \"{fieldName}\")")}\n            return default;\n        }}\n    }}"));
            }

            return (null, null);
        }

        /// <summary>生成 ParseXXX 内 catch 块：严格模式 LogParseError(文件,行,字段)，否则 LogParseWarning。</summary>
        private static string GetParseCatchBlock(string fieldName, string valueExpr)
        {
            var fn = (fieldName ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");
            return $"if (ConfigParseHelper.IsStrictMode(context))\n                ConfigParseHelper.LogParseError(context, \"{fn}\", ex);\n            else\n                ConfigParseHelper.LogParseWarning(\"{fn}\",\n                    {valueExpr}, ex);";
        }

        private static bool IsXConfigType(Type type)
        {
            if (type == null) return false;
            foreach (var i in type.GetInterfaces())
                if (i.IsGenericType && i.GetGenericTypeDefinition().Name == "IXConfig`2")
                    return true;
            return type.BaseType != null && type.BaseType.IsGenericType &&
                   type.BaseType.GetGenericTypeDefinition().Name == "XConfig`2";
        }
    }
}
