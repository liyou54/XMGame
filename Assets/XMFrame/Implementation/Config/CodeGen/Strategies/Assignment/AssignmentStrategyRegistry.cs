using System;
using System.Collections.Generic;
using System.Linq;
using XM.ConfigNew.Metadata;

namespace XM.ConfigNew.CodeGen.Strategies.Assignment
{
    /// <summary>
    /// Assignment 策略注册表
    /// 负责管理和选择合适的字段赋值策略
    /// </summary>
    public class AssignmentStrategyRegistry
    {
        private readonly List<IAssignmentStrategy> _strategies = new List<IAssignmentStrategy>();
        
        /// <summary>
        /// 构造函数 - 按优先级注册所有策略
        /// </summary>
        public AssignmentStrategyRegistry()
        {
            // 注册顺序即为优先级（先注册的先匹配）
            
            // CfgS 类型（包括 XMLLink）
            _strategies.Add(new CfgSAssignmentStrategy());
            
            // LabelS 类型
            _strategies.Add(new LabelSAssignmentStrategy());
            
            // 可空类型
            _strategies.Add(new NullableAssignmentStrategy());
            
            // 枚举类型
            _strategies.Add(new EnumAssignmentStrategy());
            
            // 字符串类型（需要根据 StringMode 转换）
            _strategies.Add(new StringAssignmentStrategy());
            
            // XAssetPath -> AssetI 转换
            _strategies.Add(new XAssetPathAssignmentStrategy());
            
            // 基本类型兜底（直接赋值）
            _strategies.Add(new DirectAssignmentStrategy());
        }
        
        /// <summary>
        /// 根据字段元数据获取对应的策略
        /// </summary>
        /// <param name="field">字段元数据</param>
        /// <returns>匹配的策略</returns>
        /// <exception cref="NotSupportedException">没有找到合适的策略</exception>
        public IAssignmentStrategy GetStrategy(ConfigFieldMetadata field)
        {
            var strategy = _strategies.FirstOrDefault(s => s.CanHandle(field));
            
            if (strategy == null)
            {
                throw new NotSupportedException(
                    $"未找到适合字段 '{field.FieldName}' 的 Assignment 策略。" +
                    $"字段类型: {field.TypeInfo?.ManagedFieldType?.FullName ?? "unknown"}");
            }
            
            return strategy;
        }
        
        /// <summary>
        /// 注册自定义策略（用于扩展）
        /// </summary>
        /// <param name="strategy">自定义策略</param>
        /// <param name="insertAtBeginning">是否插入到最前面（高优先级）</param>
        public void RegisterStrategy(IAssignmentStrategy strategy, bool insertAtBeginning = false)
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
