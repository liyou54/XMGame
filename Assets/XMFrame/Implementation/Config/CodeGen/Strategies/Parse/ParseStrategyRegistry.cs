using System;
using System.Collections.Generic;
using System.Linq;
using XM.ConfigNew.Metadata;

namespace XM.ConfigNew.CodeGen.Strategies.Parse
{
    /// <summary>
    /// Parse 策略注册表
    /// 负责管理和选择合适的 Parse 策略
    /// </summary>
    public class ParseStrategyRegistry
    {
        private readonly List<IParseStrategy> _strategies = new List<IParseStrategy>();
        
        /// <summary>
        /// 构造函数 - 按优先级注册所有策略
        /// </summary>
        public ParseStrategyRegistry()
        {
            // 注册顺序即为优先级（先注册的先匹配）
            // XmlKey 字段优先级最高
            _strategies.Add(new XmlKeyParseStrategy());
            
            // 容器类型次之
            _strategies.Add(new ContainerParseStrategy());
            
            // 嵌套配置
            _strategies.Add(new NestedConfigParseStrategy());
            
            // CfgS/XMLLink
            _strategies.Add(new CfgSParseStrategy());
            
            // 基本类型兜底（int, float, string, enum, nullable 等）
            _strategies.Add(new BasicTypeParseStrategy());
        }
        
        /// <summary>
        /// 根据字段元数据获取对应的策略
        /// </summary>
        /// <param name="field">字段元数据</param>
        /// <returns>匹配的策略</returns>
        /// <exception cref="NotSupportedException">没有找到合适的策略</exception>
        public IParseStrategy GetStrategy(ConfigFieldMetadata field)
        {
            var strategy = _strategies.FirstOrDefault(s => s.CanHandle(field));
            
            if (strategy == null)
            {
                throw new NotSupportedException(
                    $"未找到适合字段 '{field.FieldName}' 的 Parse 策略。" +
                    $"字段类型: {field.TypeInfo?.ManagedFieldType?.FullName ?? "unknown"}");
            }
            
            return strategy;
        }
        
        /// <summary>
        /// 注册自定义策略（用于扩展）
        /// </summary>
        /// <param name="strategy">自定义策略</param>
        /// <param name="insertAtBeginning">是否插入到最前面（高优先级）</param>
        public void RegisterStrategy(IParseStrategy strategy, bool insertAtBeginning = false)
        {
            if (strategy == null)
                throw new ArgumentNullException(nameof(strategy));
            
            if (insertAtBeginning)
                _strategies.Insert(0, strategy);
            else
                _strategies.Add(strategy);
        }
    }
}
