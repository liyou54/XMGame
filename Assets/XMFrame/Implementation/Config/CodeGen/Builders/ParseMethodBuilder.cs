using System;
using XM.ConfigNew.Metadata;
using XM.ConfigNew.CodeGen.Strategies.Parse;

namespace XM.ConfigNew.CodeGen.Builders
{
    /// <summary>
    /// Parse 方法构建器 - 根据字段类型生成完整的 ParseXXX 方法
    /// 使用策略模式选择合适的解析策略
    /// </summary>
    public class ParseMethodBuilder
    {
        private readonly ConfigFieldMetadata _field;
        private readonly ConfigClassMetadata _classMetadata;
        private readonly CodeBuilder _builder;
        private readonly ParseStrategyRegistry _registry;
        
        public ParseMethodBuilder(ConfigFieldMetadata field, ConfigClassMetadata classMetadata)
        {
            _field = field ?? throw new ArgumentNullException(nameof(field));
            _classMetadata = classMetadata ?? throw new ArgumentNullException(nameof(classMetadata));
            _builder = new CodeBuilder();
            _registry = new ParseStrategyRegistry();
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
        /// 生成方法体 - 使用策略模式路由到不同的解析逻辑
        /// </summary>
        private void GenerateMethodBody()
        {
            // 获取合适的策略并生成代码
            var strategy = _registry.GetStrategy(_field);
            var ctx = new CodeGenContext(_builder, _classMetadata) { FieldMetadata = _field };
            strategy.Generate(ctx);
        }
        
        #endregion
    }
}
