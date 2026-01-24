using System;

namespace XMFrame.Interfaces
{
    /// <summary>
    /// 标记管理器是否自动创建
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class AutoCreateAttribute : Attribute
    {
        /// <summary>
        /// 创建优先级，数字越小优先级越高
        /// </summary>
        public int Priority { get; set; } = 0;

        public AutoCreateAttribute()
        {
        }

        public AutoCreateAttribute(int priority)
        {
            Priority = priority;
        }
    }

    /// <summary>
    /// 标记管理器的依赖关系
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class ManagerDependencyAttribute : Attribute
    {
        /// <summary>
        /// 依赖的管理器类型
        /// </summary>
        public Type DependencyType { get; }

        public ManagerDependencyAttribute(Type dependencyType)
        {
            DependencyType = dependencyType;
        }
    }
}
