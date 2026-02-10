using System;
using System.Linq;
using XM.ConfigNew.Metadata;
using XM.ConfigNew.CodeGen.Builders;
using XM.ConfigNew.CodeGen.Strategies.Alloc;
using XM.Utils.Attribute;

namespace XM.ConfigNew.CodeGen
{
    /// <summary>
    /// XmlHelper (ClassHelper) 代码生成器
    /// 负责生成从 XML 反序列化配置的 ClassHelper 类
    /// </summary>
    public class XmlHelperGenerator
    {
        private readonly ConfigClassMetadata _metadata;
        private readonly CodeBuilder _builder;
        private readonly AllocStrategyRegistry _allocRegistry;
        private readonly NestedConfigFillStrategy _fillStrategy;
        private readonly IndexInitializationBuilder _indexBuilder;
        
        public XmlHelperGenerator(ConfigClassMetadata metadata)
        {
            _metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            _builder = new CodeBuilder();
            _allocRegistry = new AllocStrategyRegistry();
            _fillStrategy = new NestedConfigFillStrategy();
            _indexBuilder = new IndexInitializationBuilder(metadata, _builder);
        }
        
        #region 主生成方法
        
        /// <summary>
        /// 生成完整的 ClassHelper 代码
        /// </summary>
        public string Generate()
        {
            _builder.Clear();
            
            // 1. 文件头: using 语句
            GenerateUsings();
            
            // 2. 命名空间
            if (!string.IsNullOrEmpty(_metadata.Namespace))
            {
                _builder.BeginNamespace(_metadata.Namespace);
            }
            
            // 3. 类声明和内容
            GenerateClass();
            
            // 4. 结束命名空间
            if (!string.IsNullOrEmpty(_metadata.Namespace))
            {
                _builder.EndNamespace();
            }
            
            return _builder.Build();
        }
        
        #endregion
        
        #region 文件头生成
        
        /// <summary>
        /// 生成 using 语句
        /// </summary>
        private void GenerateUsings()
        {
            if (_metadata.RequiredUsings != null && _metadata.RequiredUsings.Count > 0)
            {
                foreach (var ns in _metadata.RequiredUsings)
                {
                    _builder.AppendUsing(ns);
                }
            }
            else
            {
                // 默认 using 列表
                _builder.AppendUsing("System");
                _builder.AppendUsing("System.Collections.Generic");
                _builder.AppendUsing("System.Xml");
                _builder.AppendUsing("XM");
                _builder.AppendUsing("XM.Contracts");
                _builder.AppendUsing("XM.Contracts.Config");
            }
            
            _builder.AppendLine();
        }
        
        #endregion
        
        #region 类结构生成
        
        /// <summary>
        /// 生成 ClassHelper 类
        /// </summary>
        private void GenerateClass()
        {
            var className = _metadata.HelperTypeName;
            var managedTypeName = TypeHelper.GetGlobalQualifiedTypeName(_metadata.ManagedType);
            var unmanagedTypeName = TypeHelper.GetGlobalQualifiedTypeName(_metadata.UnmanagedType);
            var baseClass = $"ConfigClassHelper<{managedTypeName}, {unmanagedTypeName}>";
            
            // 类注释
            _builder.AppendXmlComment($"{_metadata.ManagedTypeName} 的配置加载辅助类，用于从 XML 反序列化（静态代码生成，无反射）。");
            
            // 类声明
            _builder.BeginClass(className, baseClass, isPartial: false, isStruct: false);
            
            // 静态字段
            GenerateStaticFields();
            
            // 静态构造函数
            GenerateStaticConstructor();
            
            // 实例构造函数
            GenerateConstructor();
            
            // 接口方法
            GenerateInterfaceMethods();
            
            // ParseXXX 方法区域
            GenerateParseMethodsRegion();
            
            // AllocContainerWithFillImpl 方法
            GenerateAllocContainerWithFillImpl();
            
            // AllocXXX/FillXXX 方法区域
            GenerateAllocMethodsRegion();
            
            // 索引初始化和填充方法
            GenerateIndexMethods();
            
            // 私有字段
            GeneratePrivateFields();
            
            _builder.EndClass();
        }
        
        #endregion
        
        #region 静态字段和构造函数
        
        /// <summary>
        /// 生成静态字段
        /// </summary>
        private void GenerateStaticFields()
        {
            var className = _metadata.HelperTypeName;
            _builder.AppendLine($"public static {className} Instance {{ get; private set; }}");
            _builder.AppendLine("public static TblI TblI { get; private set; }");
            _builder.AppendLine("public static TblS TblS { get; private set; }");
            _builder.AppendLine($"private static readonly string {CodeGenConstants.ModNameFieldName};");
            _builder.AppendLine();
        }
        
        /// <summary>
        /// 生成静态构造函数
        /// </summary>
        private void GenerateStaticConstructor()
        {
            var className = _metadata.HelperTypeName;
            var tableName = _metadata.TableName ?? _metadata.ManagedTypeName;
            var modName = _metadata.ModName ?? CodeGenConstants.DefaultModName;
            var unmanagedTypeName = TypeHelper.GetGlobalQualifiedTypeName(_metadata.UnmanagedType);
            
            // 静态构造函数不能有访问修饰符
            _builder.AppendXmlComment("静态构造函数");
            _builder.AppendLine($"static {className}()");
            _builder.BeginBlock();
            
            _builder.AppendLine($"const string __tableName = \"{tableName}\";");
            _builder.AppendLine($"{CodeGenConstants.ModNameFieldName} = \"{modName}\";");
            _builder.AppendLine($"CfgS<{unmanagedTypeName}>.Table = new TblS(new ModS({CodeGenConstants.ModNameFieldName}), __tableName);");
            _builder.AppendLine($"TblS = new TblS(new ModS({CodeGenConstants.ModNameFieldName}), __tableName);");
            // 在静态构造函数中创建实例
            _builder.AppendLine($"Instance = new {className}();");
            
            _builder.EndMethod();
        }
        
        #endregion
        
        #region 构造函数
        
        /// <summary>
        /// 生成实例构造函数（无参数）
        /// </summary>
        private void GenerateConstructor()
        {
            var className = _metadata.HelperTypeName;
            
            _builder.AppendXmlComment("构造函数");
            _builder.AppendLine($"public {className}()");
            _builder.BeginBlock();
            _builder.EndBlock();
        }
        
        #endregion
        
        #region 接口方法
        
        /// <summary>
        /// 生成接口实现方法
        /// </summary>
        private void GenerateInterfaceMethods()
        {
            // GetTblS
            GenerateGetTblSMethod();
            
            // SetTblIDefinedInMod
            GenerateSetTblIDefinedInModMethod();
            
            // ParseAndFillFromXml
            GenerateParseAndFillFromXmlMethod();
        }
        
        /// <summary>
        /// 生成 GetTblS 方法
        /// </summary>
        private void GenerateGetTblSMethod()
        {
            _builder.BeginMethod("override TblS GetTblS()", "获取表静态标识");
            _builder.AppendLine("return TblS;");
            _builder.EndMethod();
        }
        
        /// <summary>
        /// 生成 SetTblIDefinedInMod 方法
        /// </summary>
        private void GenerateSetTblIDefinedInModMethod()
        {
            _builder.BeginMethod("override void SetTblIDefinedInMod(TblI tbl)", "设置表所属Mod");
            _builder.AppendLine("_definedInMod = tbl;");
            _builder.EndMethod();
        }
        
        /// <summary>
        /// 生成 ParseAndFillFromXml 方法
        /// </summary>
        private void GenerateParseAndFillFromXmlMethod()
        {
            var managedTypeName = TypeHelper.GetGlobalQualifiedTypeName(_metadata.ManagedType);
            
            _builder.AppendXmlComment("从 XML 解析并填充配置对象", 
                new System.Collections.Generic.Dictionary<string, string>
                {
                    { "target", "目标配置对象" },
                    { "configItem", "XML 元素" },
                    { "mod", "Mod 标识" },
                    { "configName", "配置名称" },
                    { "context", "解析上下文" }
                });
            
            _builder.AppendLine("public override void ParseAndFillFromXml(");
            _builder.PushIndent();
            _builder.AppendLine("IXConfig target,");
            _builder.AppendLine("XmlElement configItem,");
            _builder.AppendLine("ModS mod,");
            _builder.AppendLine("string configName,");
            _builder.AppendLine("in ConfigParseContext context)");
            _builder.PopIndent();
            _builder.BeginBlock();
            
            _builder.AppendLine($"var config = ({managedTypeName})target;");
            _builder.AppendLine();
            
            // 调用每个字段的 Parse 方法
            if (_metadata.Fields != null && _metadata.Fields.Count > 0)
            {
                _builder.AppendComment("解析所有字段");
                foreach (var field in _metadata.Fields)
                {
                    _builder.AppendLine($"config.{field.FieldName} = {field.ParseMethodName}(configItem, mod, configName, context);");
                }
            }
            
            _builder.EndMethod();
        }
        
        // GetLinkHelperType 方法已废弃（XMLLink 新设计不再需要）
        // 保留方法定义以供参考
        
        #endregion
        
        #region ParseXXX 方法区域
        
        /// <summary>
        /// 生成 ParseXXX 方法区域
        /// </summary>
        private void GenerateParseMethodsRegion()
        {
            _builder.BeginRegion("字段解析方法 (ParseXXX)");
            
            if (_metadata.Fields != null && _metadata.Fields.Count > 0)
            {
                foreach (var field in _metadata.Fields)
                {
                    GenerateParseMethod(field);
                    _builder.AppendLine(); // 方法之间空行
                }
            }
            
            _builder.EndRegion();
        }
        
        /// <summary>
        /// 生成单个 Parse 方法
        /// </summary>
        private void GenerateParseMethod(ConfigFieldMetadata field)
        {
            // 使用 ParseMethodBuilder 生成完整方法
            var methodBuilder = new ParseMethodBuilder(field, _metadata);
            var methodCode = methodBuilder.Generate();
            
            // 直接追加生成的代码（移除首尾空行）
            var lines = methodCode.Trim().Split('\n');
            foreach (var line in lines)
            {
                _builder.AppendLine(line.TrimEnd());
            }
        }
        
        #endregion
        
        #region AllocContainerWithFillImpl 方法
        
        /// <summary>
        /// 生成 AllocContainerWithFillImpl 方法
        /// </summary>
        private void GenerateAllocContainerWithFillImpl()
        {
            var managedTypeName = TypeHelper.GetGlobalQualifiedTypeName(_metadata.ManagedType);
            var unmanagedTypeName = TypeHelper.GetGlobalQualifiedTypeName(_metadata.UnmanagedType);
            
            _builder.AppendXmlComment("分配容器并填充非托管数据",
                new System.Collections.Generic.Dictionary<string, string>
                {
                    { "value", "托管配置对象" },
                    { "tbli", "表ID" },
                    { "cfgi", "配置ID" },
                    { "data", "非托管数据结构（ref 传递）" },
                    { "configHolderData", "配置数据持有者" },
                    { "linkParent", "Link 父节点指针" }
                });
            
            _builder.AppendLine("public override void AllocContainerWithFillImpl(");
            _builder.PushIndent();
            _builder.AppendLine("IXConfig value,");
            _builder.AppendLine("TblI tbli,");
            _builder.AppendLine("CfgI cfgi,");
            _builder.AppendLine($"ref {unmanagedTypeName} data,");
            _builder.AppendLine("XM.ConfigDataCenter.ConfigDataHolder configHolderData,");
            _builder.AppendLine("XBlobPtr? linkParent = null)");
            _builder.PopIndent();
            _builder.BeginBlock();
            
            _builder.AppendLine($"var config = ({managedTypeName})value;");
            _builder.AppendLine();
            
            // 分配容器和嵌套配置
            var allocFields = _metadata.Fields?
                .Where(f => f.IsContainer || (f.IsNestedConfig && !f.IsContainer))
                .ToList();
            
            if (allocFields != null && allocFields.Count > 0)
            {
                _builder.AppendComment("分配容器和嵌套配置");
                foreach (var field in allocFields)
                {
                    if (field.IsContainer)
                    {
                        _builder.AppendLine($"{field.AllocMethodName}(config, ref data, cfgi, configHolderData);");
                    }
                    else if (field.IsNestedConfig)
                    {
                        _builder.AppendLine($"{field.FillMethodName}(config, ref data, cfgi, configHolderData);");
                    }
                }
                _builder.AppendLine();
            }
            
            // 填充基本类型字段
            var simpleFields = _metadata.Fields?
                .Where(f => !f.IsContainer && !(f.IsNestedConfig && !f.IsContainer))
                .ToList();
            
            if (simpleFields != null && simpleFields.Count > 0)
            {
                _builder.AppendComment("填充基本类型字段");
                foreach (var field in simpleFields)
                {
                    GenerateSimpleFieldAssignment(field);
                }
            }
            
            _builder.EndMethod();
        }
        
        /// <summary>
        /// 生成简单字段的赋值代码（使用统一的字段赋值生成器）
        /// </summary>
        private void GenerateSimpleFieldAssignment(ConfigFieldMetadata field)
        {
            Builders.FieldAssignmentGenerator.GenerateAssignment(_builder, field, GetXmlLinkUnmanagedTypeName);
        }
        
        #endregion
        
        // EstablishLinks 方法已废弃（XMLLink 新设计不再需要双向引用）
        // XMLLink 现在只存储 CfgI，通过索引查询实现父子关系
        // 不再需要在链接阶段填充指针
        
        #region AllocXXX/FillXXX 方法区域
        
        /// <summary>
        /// 生成 AllocXXX 和 FillXXX 方法区域
        /// </summary>
        private void GenerateAllocMethodsRegion()
        {
            var allocFields = _metadata.Fields?
                .Where(f => f.IsContainer || (f.IsNestedConfig && !f.IsContainer))
                .ToList();
            
            if (allocFields == null || allocFields.Count == 0)
                return;
            
            _builder.BeginRegion("容器分配和嵌套配置填充方法");
            
            foreach (var field in allocFields)
            {
                if (field.IsContainer)
                {
                    GenerateAllocMethodStub(field);
                }
                else if (field.IsNestedConfig)
                {
                    GenerateFillMethodStub(field);
                }
            }
            
            _builder.EndRegion();
        }
        
        /// <summary>
        /// 生成 AllocXXX 方法的完整实现（使用策略模式）
        /// </summary>
        private void GenerateAllocMethodStub(ConfigFieldMetadata field)
        {
            var strategy = _allocRegistry.GetStrategy(field);
            if (strategy != null)
            {
                var ctx = new CodeGenContext(_builder, _metadata) { FieldMetadata = field };
                strategy.GenerateAllocMethod(ctx);
            }
        }
        
        /// <summary>
        /// 生成 FillXXX 方法的完整实现（使用策略模式）
        /// </summary>
        private void GenerateFillMethodStub(ConfigFieldMetadata field)
        {
            if (_fillStrategy.CanHandle(field))
            {
                var ctx = new CodeGenContext(_builder, _metadata) { FieldMetadata = field };
                _fillStrategy.GenerateFillMethod(ctx);
            }
        }
        
        #endregion
        
        #region 索引方法
        
        /// <summary>
        /// 生成索引初始化和填充方法
        /// </summary>
        private void GenerateIndexMethods()
        {
            if (!_metadata.HasIndexes)
                return;
            
            _builder.BeginRegion("索引初始化和查询方法");
            
            // 生成索引初始化方法（包含填充逻辑）
            _indexBuilder.GenerateIndexInitializationMethod();
            
            _builder.EndRegion();
        }
        
        #endregion
        
        #region 私有字段
        
        /// <summary>
        /// 生成私有字段
        /// </summary>
        private void GeneratePrivateFields()
        {
            _builder.AppendLine();
            
            // 生成索引字段
            if (_metadata.HasIndexes)
            {
                _indexBuilder.GenerateIndexFields();
            }
            
            _builder.AppendField("TblI", "_definedInMod", "配置定义所属的 Mod");
        }
        
        #endregion
        
        #region 辅助方法
        
        /// <summary>
        /// 获取 XmlLink 目标的非托管类型名称（全局限定）
        /// 优先从 IXConfig&lt;T, TUnmanaged&gt; 泛型参数获取，避免拼接导致的大小写问题
        /// </summary>
        private string GetXmlLinkUnmanagedTypeName(Type targetManagedType)
        {
            if (targetManagedType == null)
                return "object";
            
            return TypeHelper.GetConfigUnmanagedTypeName(targetManagedType);
        }
        
        #endregion
        
        #region 父节点扩展方法生成
        
        /// <summary>
        /// 生成父节点扩展方法类（用于 XMLLink 查询）
        /// </summary>
        private void GenerateParentExtensionsClass()
        {
            // 只有当配置类有 XMLLink 字段时才生成扩展方法类
            var linkFields = _metadata.Fields?.Where(f => f.IsXmlLink).ToList();
            if (linkFields == null || linkFields.Count == 0)
                return;
            
            _builder.AppendLine();
            
            // 扩展方法类注释
            _builder.AppendXmlComment($"{_metadata.ManagedTypeName} 的父节点扩展方法，用于查询子 Link");
            
            // 扩展方法类必须是静态的
            var extensionsClassName = _metadata.UnmanagedTypeName + CodeGenConstants.ParentExtensionsSuffix;
            _builder.AppendLine($"public static class {extensionsClassName}");
            _builder.BeginBlock();
            
            // 为每个 XMLLink 字段生成扩展方法
            foreach (var linkField in linkFields)
            {
                GenerateParentExtensionMethods(linkField);
                _builder.AppendLine();
            }
            
            _builder.EndBlock(); // end class
        }
        
        /// <summary>
        /// 为单个 XMLLink 字段生成父节点扩展方法
        /// </summary>
        private void GenerateParentExtensionMethods(ConfigFieldMetadata linkField)
        {
            var parentType = linkField.XmlLinkTargetType;
            if (parentType == null)
                return;
            
            var parentUnmanagedTypeName = GetXmlLinkUnmanagedTypeName(parentType);
            var childUnmanagedTypeName = TypeHelper.GetGlobalQualifiedTypeName(_metadata.UnmanagedType);
            var childManagedTypeName = TypeHelper.GetGlobalQualifiedTypeName(_metadata.ManagedType);
            var indexName = linkField.ParentLinkIndexName;
            
            if (linkField.IsUniqueLinkToParent)
            {
                // 唯一子 Link：返回单个 CfgI
                GenerateUniqueSubLinkExtension(parentUnmanagedTypeName, childUnmanagedTypeName, childManagedTypeName, indexName, linkField.FieldName);
            }
            else
            {
                // 多个子 Link：返回 CfgI 数组
                GenerateMultiSubLinksExtension(parentUnmanagedTypeName, childUnmanagedTypeName, childManagedTypeName, indexName, linkField.FieldName);
            }
        }
        
        /// <summary>
        /// 生成获取唯一子 Link 的扩展方法
        /// </summary>
        private void GenerateUniqueSubLinkExtension(string parentUnmanagedTypeName, string childUnmanagedTypeName, 
            string childManagedTypeName, string indexName, string linkFieldName)
        {
            var methodName = CodeGenConstants.TryGetUniqueSubLinkPrefix + _metadata.ManagedTypeName;
            
            _builder.AppendXmlComment($"获取指定父节点的唯一子Link（{_metadata.ManagedTypeName}）",
                new System.Collections.Generic.Dictionary<string, string>
                {
                    { "parentCfgI", "父节点的 CfgI" },
                    { "subLinkCfgI", "子Link 的 CfgI（如果找到）" },
                    { "returns", "是否找到子Link" }
                });
            
            _builder.AppendLine($"public static bool {methodName}(");
            _builder.PushIndent();
            _builder.AppendLine($"this CfgI<{parentUnmanagedTypeName}> parentCfgI,");
            _builder.AppendLine($"out CfgI<{childUnmanagedTypeName}> subLinkCfgI)");
            _builder.PopIndent();
            _builder.BeginBlock();
            
            // 生成 Helper 获取逻辑（公共部分）
            GenerateHelperGetAndCheck(childManagedTypeName, "subLinkCfgI", "default");
            
            _builder.AppendComment($"构造索引键并查询");
            var indexStructName = $"{childUnmanagedTypeName}.{indexName}Index";
            _builder.AppendLine($"var indexKey = new {indexStructName}(parentCfgI);");
            _builder.AppendLine($"return helper.{CodeGenConstants.GetByPrefix}{indexName}(indexKey, out subLinkCfgI);");
            
            _builder.EndMethod();
        }
        
        /// <summary>
        /// 生成 Helper 获取和空检查逻辑（公共代码）
        /// </summary>
        /// <param name="childManagedTypeName">子节点托管类型名</param>
        /// <param name="outParamName">out 参数名</param>
        /// <param name="defaultValue">out 参数的默认值</param>
        private void GenerateHelperGetAndCheck(string childManagedTypeName, string outParamName, string defaultValue)
        {
            // 直接使用静态 Instance 属性，避免通过接口查找
            var helperTypeName = childManagedTypeName + CodeGenConstants.ClassHelperSuffix;
            _builder.AppendLine($"var helper = {helperTypeName}.{CodeGenConstants.InstanceProperty};");
            _builder.AppendLine("if (helper == null)");
            _builder.BeginBlock();
            _builder.AppendLine($"{outParamName} = {defaultValue};");
            _builder.AppendLine(CodeGenConstants.ReturnFalse + ";");
            _builder.EndBlock();
            _builder.AppendLine();
        }
        
        /// <summary>
        /// 生成获取多个子 Link 的扩展方法
        /// </summary>
        private void GenerateMultiSubLinksExtension(string parentUnmanagedTypeName, string childUnmanagedTypeName, 
            string childManagedTypeName, string indexName, string linkFieldName)
        {
            var methodName = CodeGenConstants.TryGetSubLinksPrefix + _metadata.ManagedTypeName;
            
            _builder.AppendXmlComment($"获取指定父节点的所有子Link（{_metadata.ManagedTypeName}）",
                new System.Collections.Generic.Dictionary<string, string>
                {
                    { "parentCfgI", "父节点的 CfgI" },
                    { "subLinkCfgIs", "子Link 的 CfgI 列表（如果找到）" },
                    { "returns", "是否找到子Link" }
                });
            
            _builder.AppendLine($"public static bool {methodName}(");
            _builder.PushIndent();
            _builder.AppendLine($"this CfgI<{parentUnmanagedTypeName}> parentCfgI,");
            _builder.AppendLine($"out global::System.Collections.Generic.List<CfgI<{childUnmanagedTypeName}>> subLinkCfgIs)");
            _builder.PopIndent();
            _builder.BeginBlock();
            
            // 生成 Helper 获取逻辑（公共部分）
            GenerateHelperGetAndCheck(childManagedTypeName, "subLinkCfgIs", "null");
            
            _builder.AppendComment($"构造索引键并查询");
            var indexStructName = $"{childUnmanagedTypeName}.{indexName}Index";
            _builder.AppendLine($"var indexKey = new {indexStructName}(parentCfgI);");
            _builder.AppendLine($"return helper.{CodeGenConstants.GetByPrefix}{indexName}(indexKey, out subLinkCfgIs);");
            
            _builder.EndMethod();
        }
        
        #endregion
    }
}
