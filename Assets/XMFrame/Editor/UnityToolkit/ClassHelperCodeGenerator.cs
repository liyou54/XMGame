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
            var dto = ToClassHelperDto(typeInfo, fieldAssigns, converterRegistrations);
            dto.ModName = GetModNameFromAssembly(typeInfo.ManagedType?.Assembly);
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
                HasBase = typeInfo.HasBase,
                BaseManagedTypeName = typeInfo.BaseManagedTypeName ?? "",
                BaseUnmanagedTypeName = typeInfo.BaseUnmanagedTypeName ?? "",
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
                sb.Append("ConfigClassHelper.LogParseWarning(\"").Append(fieldName.Replace("\\", "\\\\").Replace("\"", "\\\"")).Append("\", s ?? \"\", null); ");
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
                var body = $"var s = ConfigClassHelper.GetXmlFieldValue(configItem, \"{fieldName}\");\n            " + (string.IsNullOrEmpty(emptyBlock) ? "" : emptyBlock + "\n            ") + $"if (string.IsNullOrEmpty(s)) return {defaultVal};\n            return ConfigClassHelper.{tryParse}(s, \"{fieldName}\", out var v) ? v : {defaultVal};";
                return ($"config.{fieldName} = {parseName}(configItem, mod, configName);", ToGlobal($"private static {typeName} {parseName}(XmlElement configItem, ModS mod, string configName)\n        {{\n            {body}\n        }}"));
            }

            if (fieldType == typeof(string))
            {
                var emptyBlock = GetEmptyValueBlock(fieldName, field);
                var body = $"var s = ConfigClassHelper.GetXmlFieldValue(configItem, \"{fieldName}\");\n            " + (string.IsNullOrEmpty(emptyBlock) ? "" : emptyBlock + "\n            ") + "return s ?? \"\";";
                return ($"config.{fieldName} = {parseName}(configItem, mod, configName);", ToGlobal($"private static {typeName} {parseName}(XmlElement configItem, ModS mod, string configName)\n        {{\n            {body}\n        }}"));
            }

            // CfgS<T>：使用基类 TryParseCfgSString（含 [XmlNotNull]/[XmlDefault]）
            if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition().Name == "CfgS`1")
            {
                var tUnmanaged = GetCSharpTypeName(fieldType.GetGenericArguments()[0]);
                var emptyBlock = GetEmptyValueBlock(fieldName, field);
                var body = $"var s = ConfigClassHelper.GetXmlFieldValue(configItem, \"{fieldName}\");\n            " + (string.IsNullOrEmpty(emptyBlock) ? "" : emptyBlock + "\n            ") + $"if (string.IsNullOrEmpty(s)) return default;\n            if (!ConfigClassHelper.TryParseCfgSString(s, \"{fieldName}\", out var modName, out var cfgName)) return default;\n            return new CfgS<{tUnmanaged}>(new ModS(modName), cfgName);";
                return ($"config.{fieldName} = {parseName}(configItem, mod, configName);", ToGlobal($"private static {typeName} {parseName}(XmlElement configItem, ModS mod, string configName)\n        {{\n            try\n            {{\n                {body}\n            }}\n            catch (Exception ex)\n            {{\n                {GetParseCatchBlock(fieldName, $"ConfigClassHelper.GetXmlFieldValue(configItem, \"{fieldName}\")")}\n                return default;\n            }}\n        }}"));
            }

            // 嵌套 IXConfig：try-catch + 日志；[XmlNotNull] 时子节点缺失打告警
            if (IsXConfigType(fieldType))
            {
                var nestedName = GetCSharpTypeName(fieldType);
                var nullBlock = field != null && field.IsNotNull
                    ? $"if (el == null) {{ ConfigClassHelper.LogParseWarning(\"{fieldName.Replace("\"", "\\\"")}\", \"\", null); return null; }}"
                    : "if (el == null) return null;";
                var body = $"var el = configItem.SelectSingleNode(\"{fieldName}\") as System.Xml.XmlElement;\n            " + nullBlock + "\n            var helper = XM.Contracts.IConfigDataCenter.I?.GetClassHelper(typeof(" + nestedName + "));\n            return helper != null ? (" + nestedName + ")helper.DeserializeConfigFromXml(el, mod, configName + \"_" + fieldName + "\") : null;";
                return ($"config.{fieldName} = {parseName}(configItem, mod, configName);", ToGlobal($"private static {typeName} {parseName}(XmlElement configItem, ModS mod, string configName)\n        {{\n            try\n            {{\n                {body}\n            }}\n            catch (Exception ex)\n            {{\n                {GetParseCatchBlock(fieldName, "null")}\n                return null;\n            }}\n        }}"));
            }

            // List<T>：try-catch + 日志，类型名由 GetCSharpTypeName 拼接
            if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
            {
                var elemType = fieldType.GetGenericArguments()[0];
                var elemName = GetCSharpTypeName(elemType);
                string body;
                if (elemType == typeof(int))
                    body = $"var list = new {typeName}();\n            var nodes = configItem.SelectNodes(\"{fieldName}\");\n            if (nodes != null)\n            foreach (System.Xml.XmlNode n in nodes) {{ var t = (n as System.Xml.XmlElement)?.InnerText?.Trim(); if (!string.IsNullOrEmpty(t) && ConfigClassHelper.TryParseInt(t, \"{fieldName}\", out var vi)) list.Add(vi); }}\n            if (list.Count == 0) {{ var csv = ConfigClassHelper.GetXmlFieldValue(configItem, \"{fieldName}\"); if (!string.IsNullOrEmpty(csv)) foreach (var p in csv.Split(',', ';')) if (!string.IsNullOrWhiteSpace(p) && ConfigClassHelper.TryParseInt(p.Trim(), \"{fieldName}\", out var vi)) list.Add(vi); }}\n            return list;";
                else if (elemType == typeof(string))
                    body = $"var list = new {typeName}();\n            var nodes = configItem.SelectNodes(\"{fieldName}\");\n            if (nodes != null)\n            foreach (System.Xml.XmlNode n in nodes) {{ var t = (n as System.Xml.XmlElement)?.InnerText?.Trim(); if (t != null) list.Add(t); }}\n            return list;";
                else if (elemType.IsGenericType && elemType.GetGenericTypeDefinition().Name == "CfgS`1")
                {
                    var tU = GetCSharpTypeName(elemType.GetGenericArguments()[0]);
                    body = $"var list = new {typeName}();\n            var nodes = configItem.SelectNodes(\"{fieldName}\");\n            if (nodes != null)\n            foreach (System.Xml.XmlNode n in nodes) {{ var t = (n as System.Xml.XmlElement)?.InnerText?.Trim(); if (!string.IsNullOrEmpty(t) && ConfigClassHelper.TryParseCfgSString(t, \"{fieldName}\", out var mn, out var cn)) list.Add(new CfgS<{tU}>(new ModS(mn), cn)); }}\n            return list;";
                }
                else if (IsXConfigType(elemType))
                {
                    var nestedName = GetCSharpTypeName(elemType);
                    body = $"var list = new {typeName}();\n            var dc = XM.Contracts.IConfigDataCenter.I; if (dc == null) return list;\n            var nodes = configItem.SelectNodes(\"{fieldName}\");\n            if (nodes != null)\n            foreach (System.Xml.XmlNode n in nodes) {{ var el = n as System.Xml.XmlElement; if (el == null) continue; var helper = dc.GetClassHelper(typeof({nestedName})); if (helper != null) {{ var item = ({nestedName})helper.DeserializeConfigFromXml(el, mod, configName + \"_{fieldName}_\" + list.Count); if (item != null) list.Add(item); }} }}\n            return list;";
                }
                else if (TryGetElementConverter(elemType, typeInfo, out var listConvDomain, out var listElemName))
                {
                    var listDomainEscaped = (listConvDomain ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");
                    body = $"var list = new {typeName}();\n            var nodes = configItem.SelectNodes(\"{fieldName}\");\n            if (nodes != null)\n            foreach (System.Xml.XmlNode n in nodes) {{ var t = (n as System.Xml.XmlElement)?.InnerText?.Trim(); if (!string.IsNullOrEmpty(t)) {{ var converter = XM.Contracts.IConfigDataCenter.I?.GetConverterByType<string, {listElemName}>(); if (converter != null) list.Add(converter.Convert(t)); }} }}\n            if (list.Count == 0) {{ var csv = ConfigClassHelper.GetXmlFieldValue(configItem, \"{fieldName}\"); if (!string.IsNullOrEmpty(csv)) foreach (var p in csv.Split(',', ';')) if (!string.IsNullOrWhiteSpace(p)) {{ var converter = XM.Contracts.IConfigDataCenter.I?.GetConverterByType<string, {listElemName}>(); if (converter != null) list.Add(converter.Convert(p.Trim())); }} }}\n            return list;";
                }
                else
                    return (null, null);
                if (field != null && field.IsNotNull)
                    body = body.Replace("return list;", "if (list.Count == 0) ConfigClassHelper.LogParseWarning(\"" + fieldName.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\", \"\", null);\n            return list;");
                return ($"config.{fieldName} = {parseName}(configItem, mod, configName);", ToGlobal($"private static {typeName} {parseName}(XmlElement configItem, ModS mod, string configName)\n        {{\n            try\n            {{\n                {body}\n            }}\n            catch (Exception ex)\n            {{\n                {GetParseCatchBlock(fieldName, "null")}\n                return new {typeName}();\n            }}\n        }}"));
            }

            // Dictionary<K,V>：try-catch + 日志，类型名由 GetCSharpTypeName 拼接；[XmlNotNull] 时空容器打告警
            if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                var keyType = fieldType.GetGenericArguments()[0];
                var valType = fieldType.GetGenericArguments()[1];
                string body;
                if (keyType == typeof(int) && valType == typeof(int))
                    body = $"var dict = new {typeName}();\n            var dictNodes = configItem.SelectNodes(\"{fieldName}/Item\");\n            if (dictNodes != null)\n            foreach (System.Xml.XmlNode n in dictNodes) {{ var el = n as System.Xml.XmlElement; if (el == null) continue; var k = el.GetAttribute(\"Key\"); var v = el.InnerText?.Trim(); if (!string.IsNullOrEmpty(k) && !string.IsNullOrEmpty(v) && ConfigClassHelper.TryParseInt(k, \"{fieldName}.Key\", out var kv) && ConfigClassHelper.TryParseInt(v, \"{fieldName}.Value\", out var vv)) dict[kv] = vv; }}\n            return dict;";
                else if (keyType == typeof(int) && TryGetElementConverter(valType, typeInfo, out var dictValDomain, out var dictValName))
                {
                    var dictValDomainEscaped = (dictValDomain ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");
                    body = $"var dict = new {typeName}();\n            var dictNodes = configItem.SelectNodes(\"{fieldName}/Item\");\n            if (dictNodes != null)\n            foreach (System.Xml.XmlNode n in dictNodes) {{ var el = n as System.Xml.XmlElement; if (el == null) continue; var k = el.GetAttribute(\"Key\"); var v = el.InnerText?.Trim(); if (!string.IsNullOrEmpty(k) && ConfigClassHelper.TryParseInt(k, \"{fieldName}.Key\", out var kv) && !string.IsNullOrEmpty(v)) {{ var converter = XM.Contracts.IConfigDataCenter.I?.GetConverterByType<string, {dictValName}>(); if (converter != null) dict[kv] = converter.Convert(v); }} }}\n            return dict;";
                }
                else if (keyType.IsGenericType && keyType.GetGenericTypeDefinition().Name == "CfgS`1" && valType.IsGenericType && valType.GetGenericTypeDefinition().Name == "CfgS`1")
                {
                    var tU = GetCSharpTypeName(keyType.GetGenericArguments()[0]);
                    body = $"var dict = new {typeName}();\n            var dictNodes = configItem.SelectNodes(\"{fieldName}/Item\");\n            if (dictNodes != null)\n            foreach (System.Xml.XmlNode n in dictNodes) {{ var el = n as System.Xml.XmlElement; if (el == null) continue; var kStr = el.GetAttribute(\"Key\") ?? (el.SelectSingleNode(\"Key\") as System.Xml.XmlElement)?.InnerText?.Trim(); var vStr = el.GetAttribute(\"Value\") ?? (el.SelectSingleNode(\"Value\") as System.Xml.XmlElement)?.InnerText?.Trim() ?? el.InnerText?.Trim(); if (!string.IsNullOrEmpty(kStr) && ConfigClassHelper.TryParseCfgSString(kStr, \"{fieldName}.Key\", out var km, out var kc) && !string.IsNullOrEmpty(vStr) && ConfigClassHelper.TryParseCfgSString(vStr, \"{fieldName}.Value\", out var vm, out var vc)) dict[new CfgS<{tU}>(new ModS(km), kc)] = new CfgS<{tU}>(new ModS(vm), vc); }}\n            return dict;";
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
                            body = $"var dict = new {typeName}();\n            var dictNodes = configItem.SelectNodes(\"{fieldName}/Item\");\n            if (dictNodes != null)\n            foreach (System.Xml.XmlNode keyNode in dictNodes) {{ var keyEl = keyNode as System.Xml.XmlElement; if (keyEl == null) continue; var kStr = keyEl.GetAttribute(\"Key\"); if (!string.IsNullOrEmpty(kStr) && ConfigClassHelper.TryParseInt(kStr, \"{fieldName}.Key\", out var key)) {{ var outerList = new {valTypeName}(); var midNodes = keyEl.SelectNodes(\"Item\"); if (midNodes != null) foreach (System.Xml.XmlNode midNode in midNodes) {{ var midEl = midNode as System.Xml.XmlElement; if (midEl == null) continue; var innerList = new {innerTypeName}(); var leafNodes = midEl.SelectNodes(\"Item\"); if (leafNodes != null) foreach (System.Xml.XmlNode leafNode in leafNodes) {{ var leafText = (leafNode as System.Xml.XmlElement)?.InnerText?.Trim(); if (!string.IsNullOrEmpty(leafText) && ConfigClassHelper.TryParseCfgSString(leafText, \"{fieldName}\", out var lm, out var lc)) innerList.Add(new CfgS<{tU}>(new ModS(lm), lc)); }} outerList.Add(innerList); }} dict[key] = outerList; }} }}\n            return dict;";
                        }
                        else if (TryGetElementConverter(cfgType, typeInfo, out var leafDomain, out var leafTypeName))
                        {
                            var leafDomainEscaped = (leafDomain ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");
                            var valTypeName = GetCSharpTypeName(valType);
                            var innerTypeName = GetCSharpTypeName(innerListType);
                            body = $"var dict = new {typeName}();\n            var dictNodes = configItem.SelectNodes(\"{fieldName}/Item\");\n            if (dictNodes != null)\n            foreach (System.Xml.XmlNode keyNode in dictNodes) {{ var keyEl = keyNode as System.Xml.XmlElement; if (keyEl == null) continue; var kStr = keyEl.GetAttribute(\"Key\"); if (!string.IsNullOrEmpty(kStr) && ConfigClassHelper.TryParseInt(kStr, \"{fieldName}.Key\", out var key)) {{ var outerList = new {valTypeName}(); var midNodes = keyEl.SelectNodes(\"Item\"); if (midNodes != null) foreach (System.Xml.XmlNode midNode in midNodes) {{ var midEl = midNode as System.Xml.XmlElement; if (midEl == null) continue; var innerList = new {innerTypeName}(); var leafNodes = midEl.SelectNodes(\"Item\"); if (leafNodes != null) foreach (System.Xml.XmlNode leafNode in leafNodes) {{ var leafText = (leafNode as System.Xml.XmlElement)?.InnerText?.Trim(); if (!string.IsNullOrEmpty(leafText)) {{ var converter = XM.Contracts.IConfigDataCenter.I?.GetConverterByType<string, {leafTypeName}>(); if (converter != null) innerList.Add(converter.Convert(leafText)); }} }} outerList.Add(innerList); }} dict[key] = outerList; }} }}\n            return dict;";
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
                    body = body.Replace("return dict;", "if (dict.Count == 0) ConfigClassHelper.LogParseWarning(\"" + fieldName.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\", \"\", null);\n            return dict;");
                return ($"config.{fieldName} = {parseName}(configItem, mod, configName);", ToGlobal($"private static {typeName} {parseName}(XmlElement configItem, ModS mod, string configName)\n        {{\n            try\n            {{\n                {body}\n            }}\n            catch (Exception ex)\n            {{\n                {GetParseCatchBlock(fieldName, "null")}\n                return new {typeName}();\n            }}\n        }}"));
            }

            // HashSet<T>：try-catch + 日志；支持已注册类型的自定义解析器；[XmlNotNull] 时空集合打告警
            if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(HashSet<>))
            {
                var elemType = fieldType.GetGenericArguments()[0];
                string body;
                if (elemType == typeof(int))
                    body = $"var set = new {typeName}();\n            var nodes = configItem.SelectNodes(\"{fieldName}\");\n            if (nodes != null)\n            foreach (System.Xml.XmlNode n in nodes) {{ var t = (n as System.Xml.XmlElement)?.InnerText?.Trim(); if (!string.IsNullOrEmpty(t) && ConfigClassHelper.TryParseInt(t, \"{fieldName}\", out var vi)) set.Add(vi); }}\n            if (set.Count == 0) {{ var csv = ConfigClassHelper.GetXmlFieldValue(configItem, \"{fieldName}\"); if (!string.IsNullOrEmpty(csv)) foreach (var p in csv.Split(',', ';')) if (!string.IsNullOrWhiteSpace(p) && ConfigClassHelper.TryParseInt(p.Trim(), \"{fieldName}\", out var vi)) set.Add(vi); }}\n            return set;";
                else if (elemType.IsGenericType && elemType.GetGenericTypeDefinition().Name == "CfgS`1")
                {
                    var tU = GetCSharpTypeName(elemType.GetGenericArguments()[0]);
                    body = $"var set = new {typeName}();\n            var nodes = configItem.SelectNodes(\"{fieldName}\");\n            if (nodes != null)\n            foreach (System.Xml.XmlNode n in nodes) {{ var t = (n as System.Xml.XmlElement)?.InnerText?.Trim(); if (!string.IsNullOrEmpty(t) && ConfigClassHelper.TryParseCfgSString(t, \"{fieldName}\", out var mn, out var cn)) set.Add(new CfgS<{tU}>(new ModS(mn), cn)); }}\n            return set;";
                }
                else if (TryGetElementConverter(elemType, typeInfo, out var setConvDomain, out var setElemName))
                {
                    var setDomainEscaped = (setConvDomain ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");
                    body = $"var set = new {typeName}();\n            var nodes = configItem.SelectNodes(\"{fieldName}\");\n            if (nodes != null)\n            foreach (System.Xml.XmlNode n in nodes) {{ var t = (n as System.Xml.XmlElement)?.InnerText?.Trim(); if (!string.IsNullOrEmpty(t)) {{ var converter = XM.Contracts.IConfigDataCenter.I?.GetConverterByType<string, {setElemName}>(); if (converter != null) set.Add(converter.Convert(t)); }} }}\n            if (set.Count == 0) {{ var csv = ConfigClassHelper.GetXmlFieldValue(configItem, \"{fieldName}\"); if (!string.IsNullOrEmpty(csv)) foreach (var p in csv.Split(',', ';')) if (!string.IsNullOrWhiteSpace(p)) {{ var converter = XM.Contracts.IConfigDataCenter.I?.GetConverterByType<string, {setElemName}>(); if (converter != null) set.Add(converter.Convert(p.Trim())); }} }}\n            return set;";
                }
                else
                    return (null, null);
                if (field != null && field.IsNotNull)
                    body = body.Replace("return set;", "if (set.Count == 0) ConfigClassHelper.LogParseWarning(\"" + fieldName.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\", \"\", null);\n            return set;");
                return ($"config.{fieldName} = {parseName}(configItem, mod, configName);", ToGlobal($"private static {typeName} {parseName}(XmlElement configItem, ModS mod, string configName)\n        {{\n            try\n            {{\n                {body}\n            }}\n            catch (Exception ex)\n            {{\n                {GetParseCatchBlock(fieldName, $"ConfigClassHelper.GetXmlFieldValue(configItem, \"{fieldName}\")")}\n                return new {typeName}();\n            }}\n        }}"));
            }

            // LabelS：使用基类 TryParseLabelSString（含 [XmlNotNull]/[XmlDefault]）
            if (fieldType.Name == "LabelS")
            {
                var emptyBlock = GetEmptyValueBlock(fieldName, field);
                var body = $"var s = ConfigClassHelper.GetXmlFieldValue(configItem, \"{fieldName}\");\n            " + (string.IsNullOrEmpty(emptyBlock) ? "" : emptyBlock + "\n            ") + "if (string.IsNullOrEmpty(s)) return default;\n            if (!ConfigClassHelper.TryParseLabelSString(s, \"" + fieldName + "\", out var modName, out var labelName)) return default;\n            return new LabelS { ModName = modName, LabelName = labelName };";
                return ($"config.{fieldName} = {parseName}(configItem, mod, configName);", ToGlobal($"private static {typeName} {parseName}(XmlElement configItem, ModS mod, string configName)\n        {{\n            try\n            {{\n                {body}\n            }}\n            catch (Exception ex)\n            {{\n                {GetParseCatchBlock(fieldName, $"ConfigClassHelper.GetXmlFieldValue(configItem, \"{fieldName}\")")}\n                return default;\n            }}\n        }}"));
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
                var body = $"var s = ConfigClassHelper.GetXmlFieldValue(configItem, \"{fieldName}\");\n            " + (string.IsNullOrEmpty(emptyBlock) ? "" : emptyBlock + "\n            ") + $"if (string.IsNullOrEmpty(s)) return default;\n            var converter = {converterExpr};\n            return converter != null ? converter.Convert(s) : default;";
                return ($"config.{fieldName} = {parseName}(configItem, mod, configName);", ToGlobal($"private static {typeName} {parseName}(XmlElement configItem, ModS mod, string configName)\n        {{\n            try\n            {{\n                {body}\n            }}\n            catch (Exception ex)\n            {{\n                {GetParseCatchBlock(fieldName, $"ConfigClassHelper.GetXmlFieldValue(configItem, \"{fieldName}\")")}\n                return default;\n            }}\n        }}"));
            }

            return (null, null);
        }

        /// <summary>生成 ParseXXX 内 catch 块：严格模式 LogParseError(文件,行,字段)，否则 LogParseWarning。</summary>
        private static string GetParseCatchBlock(string fieldName, string valueExpr)
        {
            var fn = (fieldName ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");
            return $"if (ConfigClassHelper.IsStrictMode) ConfigClassHelper.LogParseError(ConfigClassHelper.CurrentParseContext.FilePath, ConfigClassHelper.CurrentParseContext.Line, \"{fn}\", ex); else ConfigClassHelper.LogParseWarning(\"{fn}\", {valueExpr}, ex);";
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
