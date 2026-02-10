using System;
using System.Collections.Generic;

namespace XM.ConfigNew.Metadata
{
    /// <summary>
    /// 转换器注册信息 - 单个转换器的定义
    /// </summary>
    public class ConverterRegistration
    {
        /// <summary>
        /// 转换器类型
        /// </summary>
        public Type ConverterType;
        
        /// <summary>
        /// 是否全局转换器
        /// 
        /// 规则:
        /// 1. 字段级 + bGlobal=false → 字段转换器 (Priority=0)
        /// 2. 字段级 + bGlobal=true → 全局转换器 (Priority=2)
        /// 3. 程序集级 → Mod转换器 (Priority=1)
        /// </summary>
        public bool IsGlobal;
        
        /// <summary>
        /// 定义位置
        /// </summary>
        public ConverterDefinitionLocation Location;
        
        /// <summary>
        /// 优先级(数值越小优先级越高)
        /// 字段级 = 0, Mod级 = 1, 全局级 = 2
        /// </summary>
        public int Priority;
    }
    
    /// <summary>
    /// 转换器定义位置
    /// </summary>
    public enum ConverterDefinitionLocation
    {
        /// <summary>定义在字段上(通过特性)</summary>
        Field,
        
        /// <summary>定义在Mod程序集上</summary>
        ModAssembly,
        
        /// <summary>定义在全局程序集上</summary>
        GlobalAssembly
    }
    
    /// <summary>
    /// 转换器信息 - 管理字段的类型转换器
    /// 通过bGlobal和Priority来区分优先级,支持多个转换器注册
    /// 
    /// 对于标量字段: 使用 Registrations
    /// 对于容器字段: 使用 KeyRegistrations 和 ValueRegistrations
    /// </summary>
    public class ConverterInfo
    {
        #region 转换器注册列表
        
        /// <summary>
        /// 所有可用的转换器注册列表(按优先级排序)
        /// 用于标量字段的转换
        /// </summary>
        public List<ConverterRegistration> Registrations;
        
        /// <summary>
        /// 容器Key的转换器注册列表(仅Dictionary有效)
        /// 用于将XML字符串转换为Dictionary的Key类型
        /// </summary>
        public List<ConverterRegistration> KeyRegistrations;
        
        /// <summary>
        /// 容器Value/Element的转换器注册列表
        /// 用于将XML字符串转换为容器的元素类型
        /// </summary>
        public List<ConverterRegistration> ValueRegistrations;
        
        #endregion
        
        #region 源和目标类型
        
        /// <summary>
        /// 转换源类型(通常是string,从XML读取)
        /// </summary>
        public Type SourceType;
        
        /// <summary>
        /// 转换目标类型(字段的实际类型)
        /// </summary>
        public Type TargetType;
        
        /// <summary>
        /// 非托管目标类型（来自 [assembly: XmlTypeConverter(typeof(Xxx), true)] 中 ITypeConverter&lt;ManagedType, UnmanagedType&gt;）
        /// 当托管类型无法直接用于 unmanaged 结构体时，通过转换器转换为可存储的类型
        /// </summary>
        public Type UnmanagedTargetType;
        
        /// <summary>
        /// 托管到非托管转换器类型（与 UnmanagedTargetType 配套）
        /// </summary>
        public Type UnmanagedConverterType;
        
        #endregion
        
        #region 属性
        
        /// <summary>
        /// 是否需要转换器
        /// </summary>
        public bool NeedsConverter => Registrations != null && Registrations.Count > 0;
        
        /// <summary>
        /// 是否有Key转换器(容器字段)
        /// </summary>
        public bool HasKeyConverter => KeyRegistrations != null && KeyRegistrations.Count > 0;
        
        /// <summary>
        /// 是否有Value转换器(容器字段)
        /// </summary>
        public bool HasValueConverter => ValueRegistrations != null && ValueRegistrations.Count > 0;
        
        #endregion
    }
}
