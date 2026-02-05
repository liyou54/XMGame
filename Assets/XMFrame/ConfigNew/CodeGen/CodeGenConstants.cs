namespace XM.ConfigNew.CodeGen
{
    /// <summary>
    /// 代码生成常量 - 统一管理所有硬编码的字符串和数字
    /// 避免魔法字符串和魔法数字
    /// </summary>
    public static class CodeGenConstants
    {
        #region 命名后缀
        
        /// <summary>Unmanaged类型后缀</summary>
        public const string UnmanagedSuffix = "Unmanaged";
        
        /// <summary>ClassHelper类型后缀</summary>
        public const string ClassHelperSuffix = "ClassHelper";
        
        /// <summary>索引结构体后缀</summary>
        public const string IndexSuffix = "Index";
        
        /// <summary>生成文件后缀</summary>
        public const string GeneratedFileSuffix = ".Gen.cs";
        
        /// <summary>扩展方法类后缀</summary>
        public const string ExtensionsSuffix = "Extensions";
        
        #endregion
        
        #region 命名前缀
        
        /// <summary>查询方法前缀</summary>
        public const string GetByPrefix = "GetBy";
        
        #endregion
        
        #region 方法名称
        
        /// <summary>获取单个值方法名</summary>
        public const string GetValMethodName = "GetVal";
        
        /// <summary>获取多个值方法名</summary>
        public const string GetValsMethodName = "GetVals";
        
        /// <summary>ToString方法名</summary>
        public const string ToStringMethodName = "ToString";
        
        /// <summary>Equals方法名</summary>
        public const string EqualsMethodName = "Equals";
        
        /// <summary>GetHashCode方法名</summary>
        public const string GetHashCodeMethodName = "GetHashCode";
        
        #endregion
        
        #region 类型名称
        
        /// <summary>StrI类型名</summary>
        public const string StrITypeName = "StrI";
        
        /// <summary>LabelI类型名</summary>
        public const string LabelITypeName = "LabelI";
        
        /// <summary>FixedString32Bytes类型名</summary>
        public const string FixedString32TypeName = "FixedString32Bytes";
        
        /// <summary>FixedString64Bytes类型名</summary>
        public const string FixedString64TypeName = "FixedString64Bytes";
        
        /// <summary>默认Mod名</summary>
        public const string DefaultModName = "Default";
        
        /// <summary>未知类型名</summary>
        public const string UnknownTypeName = "Unknown";
        
        /// <summary>object类型名</summary>
        public const string ObjectTypeName = "object";
        
        #endregion
        
        #region 分隔符
        
        /// <summary>默认值分隔符</summary>
        public const string DefaultValueSeparator = ",";
        
        #endregion
         
        #region 哈希码计算常量
        
        /// <summary>哈希码初始值</summary>
        public const int HashInitialValue = 17;
        
        /// <summary>哈希码乘数</summary>
        public const int HashMultiplier = 31;
        
        #endregion
        
        #region 转换器优先级
        
        /// <summary>字段级转换器优先级(最高)</summary>
        public const int FieldConverterPriority = 0;
        
        /// <summary>Mod级转换器优先级(中等)</summary>
        public const int ModConverterPriority = 1;
        
        /// <summary>全局级转换器优先级(最低)</summary>
        public const int GlobalConverterPriority = 2;
        
        #endregion
        
        #region 反射相关常量
        
        /// <summary>ModName特性类型全名</summary>
        public const string ModNameAttributeTypeName = "XM.Contracts.ModNameAttribute";
        
        /// <summary>ModName属性名</summary>
        public const string ModNamePropertyName = "ModName";
        
        /// <summary>StrMode字段名</summary>
        public const string StrModeFieldName = "StrMode";
        
        #endregion
        
        #region 注释文本
        
        /// <summary>字段区域注释</summary>
        public const string FieldsComment = "字段";
        
        /// <summary>Link父指针注释</summary>
        public const string LinkParentPtrComment = "Link父节点指针";
        
        /// <summary>Link父索引注释</summary>
        public const string LinkParentIndexComment = "Link父节点索引";
        
        /// <summary>代码生成注释</summary>
        public const string CodeGeneratedComment = "代码生成";
        
        /// <summary>索引扩展方法注释</summary>
        public const string IndexExtensionComment = "索引扩展方法";
        
        #endregion
        
        #region 布尔值字符串
        
        /// <summary>"true"字符串</summary>
        public const string TrueString = "true";
        
        /// <summary>"false"字符串</summary>
        public const string FalseString = "false";
        
        /// <summary>"1"字符串</summary>
        public const string OneString = "1";
        
        /// <summary>"0"字符串</summary>
        public const string ZeroString = "0";
        
        /// <summary>"yes"字符串</summary>
        public const string YesString = "yes";
        
        /// <summary>"no"字符串</summary>
        public const string NoString = "no";
        
        #endregion
        
        #region 代码生成变量名
        
        /// <summary>托管配置变量名</summary>
        public const string ConfigVar = "config";
        
        /// <summary>非托管数据变量名</summary>
        public const string DataVar = "data";
        
        /// <summary>配置持有者数据变量名</summary>
        public const string ConfigHolderDataVar = "configHolderData";
        
        /// <summary>CfgI变量名</summary>
        public const string CfgIVar = "cfgi";
        
        /// <summary>循环索引变量名</summary>
        public const string LoopIndexVar = "i";
        
        /// <summary>键值对变量名</summary>
        public const string KvpVar = "kvp";
        
        /// <summary>元素变量名</summary>
        public const string ItemVar = "item";
        
        #endregion
        
        #region BlobContainer 访问路径
        
        /// <summary>BlobContainer访问表达式</summary>
        public const string BlobContainerAccess = "configHolderData.Data.BlobContainer";
        
        /// <summary>完整的BlobContainer带索引赋值模板: {容器}[{BlobContainer}, {索引}] = {值}</summary>
        public const string BlobIndexAssignTemplate = "{0}[configHolderData.Data.BlobContainer, {1}] = {2};";
        
        #endregion
        
        #region 容器分配方法名
        
        /// <summary>分配数组方法名</summary>
        public const string AllocArrayMethod = "AllocArray";
        
        /// <summary>分配Map方法名</summary>
        public const string AllocMapMethod = "AllocMap";
        
        /// <summary>分配Set方法名</summary>
        public const string AllocSetMethod = "AllocSet";
        
        /// <summary>Set添加方法名</summary>
        public const string SetAddMethod = "Add";
        
        #endregion
        
        #region 转换辅助方法名
        
        /// <summary>尝试获取StrI方法名</summary>
        public const string TryGetStrIMethod = "TryGetStrI";
        
        /// <summary>尝试获取CfgI方法名</summary>
        public const string TryGetCfgIMethod = "TryGetCfgI";
        
        /// <summary>获取默认值方法名（可空类型）</summary>
        public const string GetValueOrDefaultMethod = "GetValueOrDefault";
        
        /// <summary>AllocContainerWithFillImpl方法名</summary>
        public const string AllocContainerWithFillImplMethod = "AllocContainerWithFillImpl";
        
        /// <summary>Instance属性名</summary>
        public const string InstanceProperty = "Instance";
        
        /// <summary>As方法名（CfgI类型转换）</summary>
        public const string AsMethod = "As";
        
        /// <summary>Count属性名</summary>
        public const string CountProperty = "Count";
        
        #endregion
        
        #region 类型包装常量
        
        /// <summary>EnumWrapper全局限定前缀</summary>
        public const string EnumWrapperPrefix = "global::XM.ConfigNew.CodeGen.EnumWrapper<";
        
        /// <summary>EnumWrapper全局限定后缀</summary>
        public const string EnumWrapperSuffix = ">";
        
        /// <summary>CfgI泛型前缀</summary>
        public const string CfgIGenericPrefix = "CfgI<";
        
        /// <summary>CfgI泛型后缀</summary>
        public const string CfgIGenericSuffix = ">";
        
        /// <summary>default(TblI)表达式</summary>
        public const string DefaultTblI = "default(TblI)";
        
        #endregion
        
        #region XBlob容器类型名
        
        /// <summary>XBlobArray类型前缀</summary>
        public const string XBlobArrayPrefix = "global::XBlobArray<";
        
        /// <summary>XBlobMap类型前缀</summary>
        public const string XBlobMapPrefix = "global::XBlobMap<";
        
        /// <summary>XBlobSet类型前缀</summary>
        public const string XBlobSetPrefix = "global::XBlobSet<";
        
        /// <summary>XBlobPtr类型名</summary>
        public const string XBlobPtrTypeName = "global::XBlobPtr";
        
        /// <summary>泛型闭合符号</summary>
        public const string GenericClose = ">";
        
        #endregion
        
        #region 空检查模板
        
        /// <summary>null或空集合检查条件模板: {变量} == null || {变量}.Count == 0</summary>
        public const string NullOrEmptyCheckTemplate = "{0} == null || {0}.Count == 0";
        
        /// <summary>null检查条件模板: {变量} == null</summary>
        public const string NullCheckTemplate = "{0} == null";
        
        /// <summary>非null检查条件模板: {变量} != null</summary>
        public const string NotNullCheckTemplate = "{0} != null";
        
        /// <summary>非null且非空检查条件模板: {变量} != null && {变量}.Count > 0</summary>
        public const string NotNullAndNotEmptyCheckTemplate = "{0} != null && {0}.Count > 0";
        
        #endregion
        
        #region 解析辅助常量
        
        /// <summary>ConfigParseHelper全局限定名</summary>
        public const string ConfigParseHelperFullName = "global::XM.Contracts.Config.ConfigParseHelper";
        
        /// <summary>XmlNode全局限定名</summary>
        public const string XmlNodeFullName = "global::System.Xml.XmlNode";
        
        /// <summary>XmlElement全局限定名</summary>
        public const string XmlElementFullName = "global::System.Xml.XmlElement";
        
        /// <summary>Enum全局限定名</summary>
        public const string EnumFullName = "global::System.Enum";
        
        /// <summary>StringSplitOptions全局限定名</summary>
        public const string StringSplitOptionsFullName = "global::System.StringSplitOptions";
        
        #endregion
        
        #region CSV解析常量
        
        /// <summary>CSV分隔符字符数组</summary>
        public static readonly char[] CsvSeparators = { ',', ';', '|' };
        
        /// <summary>CSV分隔符代码表示</summary>
        public const string CsvSeparatorsCode = "',', ';', '|'";
        
        #endregion
    }
}
