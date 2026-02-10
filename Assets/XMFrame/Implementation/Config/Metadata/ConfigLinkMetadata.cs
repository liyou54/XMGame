using System;
using System.Collections.Generic;

namespace XM.ConfigNew.Metadata
{
    /// <summary>
    /// 配置Link元数据 - 描述配置类之间的继承和引用关系
    /// </summary>
    public class ConfigLinkMetadata
    {
        #region Link字段信息
        
        /// <summary>
        /// 所有Link字段的列表(标记了[XMLLink]的字段)
        /// </summary>
        public List<ConfigFieldMetadata> LinkFields;
        
        /// <summary>
        /// 是否有Link字段
        /// </summary>
        public bool HasLinkFields => LinkFields != null && LinkFields.Count > 0;
        
        /// <summary>
        /// 是否为多重Link(任一Link字段标记了multilink=true)
        /// </summary>
        public bool IsMultiLink;
        
        #endregion
        
        #region 反向Link(SubLink)
        
        /// <summary>
        /// 被哪些配置类型引用(反向Link)
        /// 例如: 如果ItemConfig有Link到WeaponConfig, 则WeaponConfig的SubLinkTypes包含ItemConfig
        /// </summary>
        public List<Type> SubLinkTypes;
        
        /// <summary>
        /// 被引用的Helper类型列表
        /// </summary>
        public List<Type> SubLinkHelperTypes;
        
        /// <summary>
        /// 是否被其他配置引用
        /// </summary>
        public bool HasSubLinks => SubLinkTypes != null && SubLinkTypes.Count > 0;
        
        #endregion
        
    }
}
