// using System;
// using System.Collections.Generic;
// using System.Linq;
// // using XMFrame.Editor.ConfigEditor.Data;
// // using XMFrame.Editor.ConfigEditor.Utility;
// using UnityToolkit;
// using ConfigTypeInfo = UnityToolkit.ConfigTypeInfo;
// using FieldInfo = UnityToolkit.FieldInfo;
//
// namespace XMFrame.Editor.ConfigEditor.CodeBuilder
// {
//     /// <summary>
//     /// ClassHelper 类代码生成器
//     /// 核心职责：整合所有组件，生成完整的 ClassHelper 类代码
//     /// </summary>
//     public class ClassHelperCodeBuilder
//     {
//         private readonly ConfigTypeInfo _typeInfo;
//         private readonly CodeBuilder _builder;
//         
//         public ClassHelperCodeBuilder(ConfigTypeInfo typeInfo)
//         {
//             _typeInfo = typeInfo ?? throw new ArgumentNullException(nameof(typeInfo));
//             _builder = new CodeBuilder();
//         }
//         
//         /// <summary>
//         /// 生成完整的 ClassHelper 类代码
//         /// </summary>
//         public string BuildClassHelperCode()
//         {
//             _builder.Clear();
//             
//             // 1. Using 语句
//             BuildUsings();
//             _builder.AppendEmptyLine();
//             
//             // 2. 命名空间（如果有）
//             if (!string.IsNullOrEmpty(_typeInfo.Namespace))
//             {
//                 _builder.BeginNamespace(_typeInfo.Namespace);
//             }
//             
//             // 3. 类声明和内容
//             BuildClassDeclaration();
//             
//             // 4. 结束命名空间
//             if (!string.IsNullOrEmpty(_typeInfo.Namespace))
//             {
//                 _builder.EndNamespace();
//             }
//             
//             return _builder.Build();
//         }
//         
//         /// <summary>
//         /// 生成 Using 语句
//         /// </summary>
//         private void BuildUsings()
//         {
//             // 基础命名空间
//             var namespaces = new List<string>(_typeInfo.RequiredUsings);
//             
//             // 排序并去重
//             namespaces = namespaces.Distinct().OrderBy(ns => ns).ToList();
//             
//             foreach (var ns in namespaces)
//             {
//                 _builder.AppendUsing(ns);
//             }
//         }
//         
//         /// <summary>
//         /// 生成类声明及其内容
//         /// </summary>
//         private void BuildClassDeclaration()
//         {
//             var className = NamingHelper.GetClassHelperClassName(_typeInfo.ManagedTypeName);
//             var baseClass = $"{CodeGenConstants.ConfigClassHelperTypeName}<{_typeInfo.ManagedTypeName}, {_typeInfo.UnmanagedTypeName}>";
//             
//             _builder.AppendSummary($"{_typeInfo.ManagedTypeName} 的配置加载辅助类，用于从 XML 反序列化（静态代码生成，无反射）。");
//             _builder.BeginBlock($"public sealed class {className} : {baseClass}");
//             
//             // 静态字段
//             BuildStaticFields();
//             
//             // 静态构造函数
//             BuildStaticConstructor();
//             
//             // 构造函数
//             BuildConstructor();
//             
//             // 接口实现方法
//             BuildInterfaceMethods();
//             
//             // ParseXXX 方法（字段解析）
//             BuildParseMethods();
//             
//             // AllocContainerWithFillImpl 方法
//             BuildAllocContainerWithFillImpl();
//             
//             // AllocXXX 辅助方法（容器分配）
//             BuildAllocMethods();
//             
//             // 私有字段
//             BuildPrivateFields();
//             
//             _builder.EndBlock();
//         }
//         
//         /// <summary>
//         /// 生成静态字段
//         /// </summary>
//         private void BuildStaticFields()
//         {
//             _builder.AppendLineWithIndent("public static TblI TblI { get; private set; }");
//             _builder.AppendLineWithIndent("public static TblS TblS { get; private set; }");
//             _builder.AppendEmptyLine();
//         }
//         
//         /// <summary>
//         /// 生成静态构造函数
//         /// </summary>
//         private void BuildStaticConstructor()
//         {
//             var className = NamingHelper.GetClassHelperClassName(_typeInfo.ManagedTypeName);
//             var tableName = _typeInfo.TableName ?? _typeInfo.ManagedTypeName;
//             var modName = CodeGenConstants.DefaultModName; // TODO: 从程序集获取
//             
//             _builder.AppendLineWithIndent($"static {className}()");
//             _builder.BeginBlock();
//             _builder.AppendLineWithIndent($"const string __tableName = \"{tableName}\";");
//             _builder.AppendLineWithIndent($"const string __modName = \"{modName}\";");
//             _builder.AppendLineWithIndent($"CfgS<{_typeInfo.UnmanagedTypeName}>.Table = new TblS(new ModS(__modName), __tableName);");
//             _builder.AppendLineWithIndent("TblS = new TblS(new ModS(__modName), __tableName);");
//             _builder.EndBlock();
//             _builder.AppendEmptyLine();
//         }
//         
//         /// <summary>
//         /// 生成构造函数
//         /// </summary>
//         private void BuildConstructor()
//         {
//             var className = NamingHelper.GetClassHelperClassName(_typeInfo.ManagedTypeName);
//             
//             _builder.AppendLineWithIndent($"public {className}(IConfigDataCenter dataCenter)");
//             _builder.IncreaseIndent();
//             _builder.AppendLineWithIndent(": base(dataCenter)");
//             _builder.DecreaseIndent();
//             _builder.BeginBlock();
//             
//             // TODO: 注册转换器
//             
//             _builder.EndBlock();
//             _builder.AppendEmptyLine();
//         }
//         
//         /// <summary>
//         /// 生成接口实现方法
//         /// </summary>
//         private void BuildInterfaceMethods()
//         {
//             // GetTblS
//             _builder.AppendLineWithIndent("public override TblS GetTblS()");
//             _builder.BeginBlock();
//             _builder.AppendLineWithIndent("return TblS;");
//             _builder.EndBlock();
//             _builder.AppendEmptyLine();
//             
//             // SetTblIDefinedInMod
//             _builder.AppendLineWithIndent("public override void SetTblIDefinedInMod(TblI tbl)");
//             _builder.BeginBlock();
//             _builder.AppendLineWithIndent("_definedInMod = tbl;");
//             _builder.EndBlock();
//             _builder.AppendEmptyLine();
//             
//             // ParseAndFillFromXml
//             BuildParseAndFillFromXml();
//             
//             // GetLinkHelperType
//             _builder.AppendLineWithIndent("public override Type GetLinkHelperType()");
//             _builder.BeginBlock();
//             _builder.AppendLineWithIndent("return null;");
//             _builder.EndBlock();
//             _builder.AppendEmptyLine();
//         }
//         
//         /// <summary>
//         /// 生成 ParseAndFillFromXml 方法
//         /// </summary>
//         private void BuildParseAndFillFromXml()
//         {
//             _builder.AppendLineWithIndent("public override void ParseAndFillFromXml(");
//             _builder.IncreaseIndent();
//             _builder.AppendLineWithIndent("IXConfig target,");
//             _builder.AppendLineWithIndent("XmlElement configItem,");
//             _builder.AppendLineWithIndent("ModS mod,");
//             _builder.AppendLineWithIndent("string configName,");
//             _builder.AppendLineWithIndent($"in {CodeGenConstants.ConfigParseContextTypeName} context)");
//             _builder.DecreaseIndent();
//             
//             _builder.BeginBlock();
//             _builder.AppendLineWithIndent($"var config = ({_typeInfo.ManagedTypeName})target;");
//             
//             // 如果有基类，先调用基类的解析方法
//             // TODO: 处理基类
//             
//             // 调用每个字段的 Parse 方法
//             foreach (var field in _typeInfo.Fields)
//             {
//                 var parseMethodName = NamingHelper.GetParseMethodName(field.Name);
//                 _builder.AppendLineWithIndent($"config.{field.Name} = {parseMethodName}(configItem, mod, configName, context);");
//             }
//             
//             _builder.EndBlock();
//             _builder.AppendEmptyLine();
//         }
//         
//         /// <summary>
//         /// 生成 ParseXXX 方法
//         /// </summary>
//         private void BuildParseMethods()
//         {
//             _builder.WithRegion(CodeGenConstants.ParseMethodsRegionName, b =>
//             {
//                 foreach (var field in _typeInfo.Fields)
//                 {
//                     var fieldInfo = ConvertToConfigFieldInfo(field);
//                     MethodBuilder.BuildParseMethod(
//                         b,
//                         fieldInfo,
//                         _typeInfo.ManagedTypeName,
//                         _typeInfo.UnmanagedTypeName);
//                 }
//             });
//         }
//         
//         /// <summary>
//         /// 生成 AllocContainerWithFillImpl 方法
//         /// </summary>
//         private void BuildAllocContainerWithFillImpl()
//         {
//             _builder.AppendLineWithIndent("public override void AllocContainerWithFillImpl(");
//             _builder.IncreaseIndent();
//             _builder.AppendLineWithIndent("IXConfig value,");
//             _builder.AppendLineWithIndent("TblI tbli,");
//             _builder.AppendLineWithIndent("CfgI cfgi,");
//             _builder.AppendLineWithIndent($"ref {_typeInfo.UnmanagedTypeName} data,");
//             _builder.AppendLineWithIndent("XM.ConfigDataCenter.ConfigDataHolder configHolderData,");
//             _builder.AppendLineWithIndent("XBlobPtr? linkParent = null)");
//             _builder.DecreaseIndent();
//             
//             _builder.BeginBlock();
//             _builder.AppendLineWithIndent($"var config = ({_typeInfo.ManagedTypeName})value;");
//             
//             // 检查是否有需要分配的容器或嵌套配置
//             var allocFields = _typeInfo.Fields
//                 .Where(f => NeedsAllocation(f))
//                 .ToList();
//             
//             if (allocFields.Count > 0)
//             {
//                 // 调用各个字段的 Alloc 方法
//                 foreach (var field in allocFields)
//                 {
//                     var allocMethodName = GetAllocOrFillMethodName(field);
//                     _builder.AppendLineWithIndent($"{allocMethodName}(config, ref data, cfgi, configHolderData);");
//                 }
//                 _builder.AppendEmptyLine();
//             }
//             
//             // 填充基本类型和引用类型字段
//             _builder.AppendComment("填充基本类型和引用类型字段");
//             foreach (var field in _typeInfo.Fields)
//             {
//                 if (!NeedsAllocation(field))
//                 {
//                     BuildSimpleFieldAssignment(_builder, field);
//                 }
//             }
//             
//             _builder.EndBlock();
//             _builder.AppendEmptyLine();
//         }
//         
//         /// <summary>
//         /// 生成 AllocXXX 辅助方法
//         /// </summary>
//         private void BuildAllocMethods()
//         {
//             var allocFields = _typeInfo.Fields
//                 .Where(f => NeedsAllocation(f))
//                 .ToList();
//             
//             if (allocFields.Count == 0)
//             {
//                 return;
//             }
//             
//             _builder.WithRegion(CodeGenConstants.AllocMethodsRegionName, b =>
//             {
//                 foreach (var field in allocFields)
//                 {
//                     if (IsContainerField(field))
//                     {
//                         var fieldInfo = ConvertToConfigFieldInfo(field);
//                         MethodBuilder.BuildAllocMethod(
//                             b,
//                             fieldInfo,
//                             _typeInfo.ManagedTypeName,
//                             _typeInfo.UnmanagedTypeName);
//                     }
//                     else if (IsNestedConfigField(field))
//                     {
//                         BuildFillNestedConfigMethod(b, field);
//                     }
//                 }
//             });
//         }
//         
//         /// <summary>
//         /// 生成嵌套配置填充方法
//         /// </summary>
//         private void BuildFillNestedConfigMethod(CodeBuilder builder, FieldInfo field)
//         {
//             var methodName = NamingHelper.GetFillMethodName(field.Name);
//             
//             builder.AppendSummary($"填充 {field.Name} 嵌套配置");
//             builder.AppendLineWithIndent($"private void {methodName}(");
//             builder.IncreaseIndent();
//             builder.AppendLineWithIndent($"{_typeInfo.ManagedTypeName} config,");
//             builder.AppendLineWithIndent($"ref {_typeInfo.UnmanagedTypeName} data,");
//             builder.AppendLineWithIndent("CfgI cfgi,");
//             builder.AppendLineWithIndent("XM.ConfigDataCenter.ConfigDataHolder configHolderData)");
//             builder.DecreaseIndent();
//             
//             builder.BeginBlock();
//             builder.AppendLineWithIndent($"if (config.{field.Name} != null)");
//             builder.BeginBlock();
//             
//             var nestedTypeName = field.ManagedType;
//             var helperClassName = NamingHelper.GetClassHelperClassName(nestedTypeName);
//             
//             builder.AppendLineWithIndent($"var nestedHelper = IConfigDataCenter.I.GetClassHelper<{nestedTypeName}>() as {helperClassName};");
//             builder.WithBlock("if (nestedHelper != null)", b =>
//             {
//                 b.AppendLineWithIndent("nestedHelper.AllocContainerWithFillImpl(");
//                 b.IncreaseIndent();
//                 b.AppendLineWithIndent($"config.{field.Name},");
//                 b.AppendLineWithIndent("_definedInMod,");
//                 b.AppendLineWithIndent("cfgi,");
//                 b.AppendLineWithIndent($"ref data.{field.Name},");
//                 b.AppendLineWithIndent("configHolderData);");
//                 b.DecreaseIndent();
//             });
//             
//             builder.EndBlock();
//             builder.EndBlock();
//             builder.AppendEmptyLine();
//         }
//         
//         /// <summary>
//         /// 生成简单字段赋值代码
//         /// </summary>
//         private void BuildSimpleFieldAssignment(CodeBuilder builder, FieldInfo field)
//         {
//             // 获取字段类型
//             var fieldType = _typeInfo.ManagedType.GetField(field.Name)?.FieldType;
//             if (fieldType == null)
//             {
//                 return;
//             }
//             
//             // 嵌套配置字段不应该在这里处理（应该通过 FillXXX 方法处理）
//             if (IsNestedConfigField(field))
//             {
//                 return;
//             }
//             
//             // CfgS 类型需要转换为 CfgI
//             if (IsConfigKeyType(fieldType))
//             {
//                 var cfgIVarName = NamingHelper.GetCfgIVariableName(field.Name);
//                 builder.AppendLineWithIndent($"if (IConfigDataCenter.I.{CodeGenConstants.TryGetCfgIMethodName}(config.{field.Name}.AsNonGeneric(), out var {cfgIVarName}))");
//                 builder.BeginBlock();
//                 var unmanagedType = TypeNameResolver.GetUnmanagedTypeName(fieldType.GetGenericArguments()[0]);
//                 builder.AppendLineWithIndent($"data.{field.Name} = {cfgIVarName}.As<{unmanagedType}>();");
//                 builder.EndBlock();
//             }
//             else if (field.NeedsConverter)
//             {
//                 // 需要转换器的类型（如 string -> StrI, LabelS -> LabelI）
//                 // 直接使用 UnityToolkit.FieldInfo 中已经分析好的转换器信息
//                 BuildConverterAssignmentFromFieldInfo(builder, field);
//             }
//             else
//             {
//                 // 基础类型直接赋值
//                 builder.AppendLineWithIndent($"data.{field.Name} = config.{field.Name};");
//             }
//         }
//         
//         /// <summary>
//         /// 生成私有字段
//         /// </summary>
//         private void BuildPrivateFields()
//         {
//             _builder.AppendLineWithIndent("private TblI _definedInMod;");
//         }
//         
//         #region 辅助方法
//         
//         /// <summary>
//         /// 判断字段是否需要分配（容器或嵌套配置）
//         /// </summary>
//         private bool NeedsAllocation(FieldInfo field)
//         {
//             var fieldType = _typeInfo.ManagedType.GetField(field.Name)?.FieldType;
//             if (fieldType == null)
//             {
//                 return false;
//             }
//             
//             return IsContainerField(field) || IsNestedConfigField(field);
//         }
//         
//         /// <summary>
//         /// 判断是否是容器字段
//         /// </summary>
//         private bool IsContainerField(FieldInfo field)
//         {
//             var fieldType = _typeInfo.ManagedType.GetField(field.Name)?.FieldType;
//             return fieldType != null && FieldAnalyzer.IsContainerType(fieldType);
//         }
//         
//         /// <summary>
//         /// 判断是否是嵌套配置字段
//         /// </summary>
//         private bool IsNestedConfigField(FieldInfo field)
//         {
//             var fieldType = _typeInfo.ManagedType.GetField(field.Name)?.FieldType;
//             return fieldType != null && FieldAnalyzer.IsNestedConfigType(fieldType);
//         }
//         
//         /// <summary>
//         /// 判断是否是 ConfigKey 类型
//         /// </summary>
//         private bool IsConfigKeyType(Type type)
//         {
//             return type != null && type.IsGenericType && type.GetGenericTypeDefinition().Name == "CfgS`1";
//         }
//         
//         /// <summary>
//         /// 获取 Alloc 或 Fill 方法名
//         /// </summary>
//         private string GetAllocOrFillMethodName(FieldInfo field)
//         {
//             if (IsNestedConfigField(field))
//             {
//                 return NamingHelper.GetFillMethodName(field.Name);
//             }
//             else
//             {
//                 return NamingHelper.GetAllocMethodName(field.Name);
//             }
//         }
//         
//         /// <summary>
//         /// 判断字段是否需要转换器
//         /// </summary>
//         private bool NeedsConverter(FieldInfo field)
//         {
//             var reflectionField = _typeInfo.ManagedType.GetField(field.Name);
//             if (reflectionField == null) return false;
//             
//             ConversionInfo conversionInfo;
//             return FieldAnalyzer.NeedsTypeConversion(reflectionField, out conversionInfo);
//         }
//         
//         /// <summary>
//         /// 生成使用转换器的字段赋值代码（使用 UnityToolkit.FieldInfo 中的转换器信息）
//         /// </summary>
//         private void BuildConverterAssignmentFromFieldInfo(CodeBuilder builder, FieldInfo field)
//         {
//             if (!field.NeedsConverter)
//             {
//                 return;
//             }
//             
//             var converterTypeName = field.ConverterTypeName;
//             if (string.IsNullOrEmpty(converterTypeName))
//             {
//                 return;
//             }
//             
//             // 直接调用静态转换器方法
//             builder.AppendLineWithIndent($"if ({converterTypeName}.TryConvert(config.{field.Name}, out var converted_{field.Name}))");
//             builder.BeginBlock();
//             builder.AppendLineWithIndent($"data.{field.Name} = converted_{field.Name};");
//             builder.EndBlock();
//         }
//         
//         /// <summary>
//         /// 生成使用转换器的字段赋值代码（使用 System.Reflection.FieldInfo）
//         /// </summary>
//         private void BuildConverterAssignment(CodeBuilder builder, FieldInfo field)
//         {
//             var reflectionField = _typeInfo.ManagedType.GetField(field.Name);
//             if (reflectionField == null) return;
//             
//             ConversionInfo conversionInfo;
//             if (!FieldAnalyzer.NeedsTypeConversion(reflectionField, out conversionInfo))
//             {
//                 return;
//             }
//             
//             var sourceTypeName = TypeNameResolver.GetCSharpName(conversionInfo.SourceType);
//             var targetTypeName = TypeNameResolver.GetCSharpName(conversionInfo.TargetType);
//             var domain = conversionInfo.Domain ?? string.Empty;
//             
//             // 从 IConfigDataCenter 获取转换器
//             builder.AppendLineWithIndent($"var converter_{field.Name} = IConfigDataCenter.I?.GetConverter<{sourceTypeName}, {targetTypeName}>(\"{domain}\");");
//             builder.AppendLineWithIndent($"if (converter_{field.Name} != null && converter_{field.Name}.Convert(config.{field.Name}, out var converted_{field.Name}))");
//             builder.BeginBlock();
//             builder.AppendLineWithIndent($"data.{field.Name} = converted_{field.Name};");
//             builder.EndBlock();
//         }
//         
//         /// <summary>
//         /// 转换为 ConfigFieldInfo（桥接旧数据结构和新数据结构）
//         /// </summary>
//         private ConfigFieldInfo ConvertToConfigFieldInfo(FieldInfo field)
//         {
//             var reflectionField = _typeInfo.ManagedType.GetField(field.Name);
//             var fieldType = reflectionField?.FieldType;
//             
//             var fieldInfo = new ConfigFieldInfo
//             {
//                 Name = field.Name,
//                 FieldType = fieldType,
//                 ManagedTypeName = fieldType != null ? TypeNameResolver.GetCSharpName(fieldType) : field.ManagedType,
//                 UnmanagedTypeName = field.UnmanagedType,
//                 IsNotNull = field.IsNotNull,
//                 DefaultValue = field.DefaultValueString,
//                 IsIndexField = field.IsIndexField,
//                 NeedsRefField = field.NeedsRefField,
//                 RefFieldName = field.RefFieldName
//             };
//             
//             // 分析字段类型
//             if (fieldType != null)
//             {
//                 fieldInfo.TypeInfo = FieldAnalyzer.AnalyzeType(fieldType);
//             }
//             
//             // 分析转换器信息
//             if (reflectionField != null)
//             {
//                 ConversionInfo conversionInfo;
//                 if (FieldAnalyzer.NeedsTypeConversion(reflectionField, out conversionInfo))
//                 {
//                     fieldInfo.ConversionInfo = conversionInfo;
//                 }
//             }
//             
//             return fieldInfo;
//         }
//         
//         #endregion
//     }
// }
