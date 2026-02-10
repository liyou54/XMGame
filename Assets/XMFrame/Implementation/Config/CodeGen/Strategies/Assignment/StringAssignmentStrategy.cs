using System;
using XM.ConfigNew.Metadata;
using XM.Utils.Attribute;

namespace XM.ConfigNew.CodeGen.Strategies.Assignment
{
    /// <summary>
    /// 字符串类型字段赋值策略（根据 StringMode 转换）
    /// </summary>
    public class StringAssignmentStrategy : IAssignmentStrategy
    {
        public bool CanHandle(ConfigFieldMetadata field)
        {
            var managedType = field.TypeInfo?.ManagedFieldType;
            return managedType == typeof(string);
        }
        
        public void Generate(CodeGenContext ctx)
        {
            var field = ctx.FieldMetadata;
            var builder = ctx.Builder;
            
            var configFieldAccess = CodeBuilder.BuildConfigFieldAccess(field.FieldName);
            var dataFieldAccess = CodeBuilder.BuildDataFieldAccess(field.FieldName);
            
            switch (field.StringMode)
            {
                case EXmlStrMode.EFix32:
                    // FixedString32Bytes（使用 SafeConvert 避免换行和超长导致的截断异常）
                    builder.AppendAssignment(dataFieldAccess, 
                        $"{CodeGenConstants.SafeConvertToFixedString32Method}({configFieldAccess} ?? string.Empty)");
                    break;
                
                case EXmlStrMode.EFix64:
                    // FixedString64Bytes（使用 SafeConvert 避免换行和超长导致的截断异常）
                    builder.AppendAssignment(dataFieldAccess, 
                        $"{CodeGenConstants.SafeConvertToFixedString64Method}({configFieldAccess} ?? string.Empty)");
                    break;
                
                case EXmlStrMode.ELabelI:
                    // LabelI 需要转换
                    builder.BeginIfBlock($"TryGetLabelI({configFieldAccess}, out var {field.FieldName}LabelI)");
                    builder.AppendAssignment(dataFieldAccess, $"{field.FieldName}LabelI");
                    builder.EndBlock();
                    break;
                
                case EXmlStrMode.EStrI:
                default:
                    // StrI 需要转换（默认）
                    builder.BeginIfBlock($"{CodeGenConstants.TryGetStrIMethod}({configFieldAccess}, out var {field.FieldName}StrI)");
                    builder.AppendAssignment(dataFieldAccess, $"{field.FieldName}StrI");
                    builder.EndBlock();
                    break;
            }
        }
    }
}
