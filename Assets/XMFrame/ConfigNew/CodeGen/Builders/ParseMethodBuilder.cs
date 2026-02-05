using System;
using XM.ConfigNew.Metadata;

namespace XM.ConfigNew.CodeGen.Builders
{
    /// <summary>
    /// Parse 方法构建器 - 根据字段类型生成完整的 ParseXXX 方法
    /// </summary>
    public class ParseMethodBuilder
    {
        private readonly ConfigFieldMetadata _field;
        private readonly ConfigClassMetadata _classMetadata;
        private readonly CodeBuilder _builder;
        
        public ParseMethodBuilder(ConfigFieldMetadata field, ConfigClassMetadata classMetadata)
        {
            _field = field ?? throw new ArgumentNullException(nameof(field));
            _classMetadata = classMetadata ?? throw new ArgumentNullException(nameof(classMetadata));
            _builder = new CodeBuilder();
        }
        
        /// <summary>
        /// 生成完整的 Parse 方法
        /// </summary>
        public string Generate()
        {
            _builder.Clear();
            
            // 方法注释和签名
            GenerateMethodSignature();
            
            // 方法体
            _builder.BeginBlock();
            GenerateMethodBody();
            _builder.EndBlock();
            _builder.AppendLine(); // 方法之间空行
            
            return _builder.Build();
        }
        
        #region 方法签名生成
        
        /// <summary>
        /// 生成方法签名
        /// </summary>
        private void GenerateMethodSignature()
        {
            var returnType = _field.ManagedFieldTypeName ?? "object";
            var methodName = _field.ParseMethodName;
            
            _builder.AppendXmlComment($"解析 {_field.FieldName} 字段");
            _builder.AppendLine($"private static {returnType} {methodName}(");
            _builder.PushIndent();
            _builder.AppendLine("global::System.Xml.XmlElement configItem,");
            _builder.AppendLine("global::XM.Contracts.Config.ModS mod,");
            _builder.AppendLine("string configName,");
            _builder.AppendLine("in global::XM.Contracts.Config.ConfigParseContext context)");
            _builder.PopIndent();
        }
        
        #endregion
        
        #region 方法体生成
        
        /// <summary>
        /// 生成方法体 - 根据字段类型路由到不同的解析逻辑
        /// </summary>
        private void GenerateMethodBody()
        {
            var typeInfo = _field.TypeInfo;
            
            // 1. 容器类型
            if (typeInfo.IsContainer)
            {
                GenerateContainerParseLogic();
                return;
            }
            
            // 2. 嵌套配置
            if (typeInfo.IsNestedConfig)
            {
                GenerateNestedConfigParseLogic();
                return;
            }
            
            // 3. CfgS/Link 类型
            if (_field.IsXmlLink)
            {
                GenerateCfgSParseLogic();
                return;
            }
            
            // 4. 基本类型（包括枚举、可空）
            GenerateBasicTypeParseLogic();
        }
        
        #endregion
        
        #region 基本类型解析
        
        /// <summary>
        /// 生成基本类型解析逻辑
        /// </summary>
        private void GenerateBasicTypeParseLogic()
        {
            BasicTypeParser.GenerateParseLogic(_builder, _field);
        }
        
        #endregion
        
        #region 容器解析
        
        /// <summary>
        /// 生成容器解析逻辑
        /// </summary>
        private void GenerateContainerParseLogic()
        {
            ContainerParser.GenerateParseLogic(_builder, _field);
        }
        
        #endregion
        
        #region 嵌套配置解析
        
        /// <summary>
        /// 生成嵌套配置解析逻辑
        /// </summary>
        private void GenerateNestedConfigParseLogic()
        {
            // 判断是单个还是容器中的嵌套配置
            if (_field.TypeInfo.IsContainer)
            {
                // 容器中的嵌套配置
                NestedConfigParser.GenerateListParse(_builder, _field);
            }
            else
            {
                // 单个嵌套配置
                NestedConfigParser.GenerateSingleParse(_builder, _field);
            }
        }
        
        #endregion
        
        #region CfgS 解析
        
        /// <summary>
        /// 生成 CfgS 解析逻辑
        /// </summary>
        private void GenerateCfgSParseLogic()
        {
            CfgSParser.GenerateParseLogic(_builder, _field);
        }
        
        #endregion
    }
}
