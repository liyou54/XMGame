using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using XMFrame.Interfaces;

namespace XMFrame.Utils
{
    /// <summary>
    /// 管理器扩展方法
    /// </summary>
    public static class ManagerExtensions
    {
        /// <summary>
        /// 检查类型是否标记了自动创建特性
        /// </summary>
        public static bool IsAutoCreate(this Type type)
        {
            return type.GetCustomAttribute<AutoCreateAttribute>() != null;
        }

        /// <summary>
        /// 获取自动创建优先级
        /// </summary>
        public static int GetAutoCreatePriority(this Type type)
        {
            var attr = type.GetCustomAttribute<AutoCreateAttribute>();
            return attr?.Priority ?? int.MaxValue;
        }

        /// <summary>
        /// 获取管理器的依赖类型列表
        /// </summary>
        public static IEnumerable<Type> GetDependencies(this Type type)
        {
            var attrs = type.GetCustomAttributes<ManagerDependencyAttribute>();
            return attrs.Select(attr => attr.DependencyType).Where(t => t != null);
        }
    }
}
