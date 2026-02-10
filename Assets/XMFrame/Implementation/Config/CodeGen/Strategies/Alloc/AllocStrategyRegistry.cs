using System;
using System.Collections.Generic;
using System.Linq;
using XM.ConfigNew.Metadata;

namespace XM.ConfigNew.CodeGen.Strategies.Alloc
{
    /// <summary>
    /// Alloc 策略注册表
    /// 负责管理和选择合适的容器分配策略
    /// </summary>
    public class AllocStrategyRegistry
    {
        private readonly List<IAllocStrategy> _strategies = new List<IAllocStrategy>();
        
        /// <summary>
        /// 构造函数 - 按优先级注册所有策略
        /// </summary>
        public AllocStrategyRegistry()
        {
            // 注册顺序即为优先级（先注册的先匹配）
            
            // List 容器
            _strategies.Add(new ListAllocStrategy());
            
            // Dictionary 容器
            _strategies.Add(new DictionaryAllocStrategy());
            
            // HashSet 容器
            _strategies.Add(new HashSetAllocStrategy());
        }
        
        /// <summary>
        /// 根据字段元数据获取对应的策略
        /// </summary>
        /// <param name="field">字段元数据</param>
        /// <returns>匹配的策略，如果没有匹配则返回 null</returns>
        public IAllocStrategy GetStrategy(ConfigFieldMetadata field)
        {
            return _strategies.FirstOrDefault(s => s.CanHandle(field));
        }
        
        /// <summary>
        /// 注册自定义策略（用于扩展）
        /// </summary>
        /// <param name="strategy">自定义策略</param>
        /// <param name="insertAtBeginning">是否插入到最前面（高优先级）</param>
        public void RegisterStrategy(IAllocStrategy strategy, bool insertAtBeginning = false)
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
