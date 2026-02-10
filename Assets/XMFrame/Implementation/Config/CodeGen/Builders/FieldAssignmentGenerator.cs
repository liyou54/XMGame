using System;
using XM.ConfigNew.Metadata;
using XM.ConfigNew.CodeGen.Strategies.Assignment;
using XM.Utils.Attribute;

namespace XM.ConfigNew.CodeGen.Builders
{
    /// <summary>
    /// 字段赋值代码生成器
    /// 统一处理从 Managed Config 到 Unmanaged Data 的字段赋值逻辑
    /// 使用策略模式消除硬编码类型判断
    /// </summary>
    public static class FieldAssignmentGenerator
    {
        // 策略注册表（单例）
        private static readonly AssignmentStrategyRegistry _registry = new AssignmentStrategyRegistry();
        
        /// <summary>
        /// 生成字段赋值代码（统一入口，使用策略模式）
        /// </summary>
        public static void GenerateAssignment(CodeBuilder builder, ConfigFieldMetadata field, Func<Type, string> getUnmanagedTypeNameFunc)
        {
            // 使用策略模式替代 switch
            var strategy = _registry.GetStrategy(field);
            
            // 构造临时的 CodeGenContext（需要支持 getUnmanagedTypeNameFunc）
            var tempMetadata = new ConfigClassMetadata 
            { 
                ManagedType = typeof(object),  // 占位符
                UnmanagedType = typeof(object)  // 占位符
            };
            var ctx = new CodeGenContext(builder, tempMetadata) { FieldMetadata = field };
            
            strategy.Generate(ctx);
        }
        
        #region 保留旧的枚举和方法签名（向后兼容）
        
        /// <summary>
        /// 字段赋值类型分类（已废弃，仅用于向后兼容）
        /// </summary>
        [Obsolete("使用策略模式替代，请直接调用 GenerateAssignment")]
        public enum FieldAssignmentType
        {
            CfgS,
            LabelS,
            Nullable,
            Enum,
            String,
            Direct
        }
        
        /// <summary>
        /// 获取字段赋值类型分类（已废弃）
        /// </summary>
        [Obsolete("使用策略模式替代")]
        public static FieldAssignmentType GetAssignmentType(ConfigFieldMetadata field)
        {
            var managedType = field.TypeInfo?.ManagedFieldType;
            
            if (managedType == null)
                return FieldAssignmentType.Direct;
            
            if (TypeHelper.IsCfgSType(managedType) || field.IsXmlLink)
                return FieldAssignmentType.CfgS;
            
            if (managedType.Name == "LabelS")
                return FieldAssignmentType.LabelS;
            
            if (field.TypeInfo.IsNullable)
                return FieldAssignmentType.Nullable;
            
            if (field.TypeInfo.IsEnum)
                return FieldAssignmentType.Enum;
            
            if (managedType == typeof(string))
                return FieldAssignmentType.String;
            
            return FieldAssignmentType.Direct;
        }
        
        #endregion
    }
}
