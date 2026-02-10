using System;
using Unity.Collections;
using XM;
using XM.Contracts.Config;

namespace XM
{
    /// <summary>
    /// UIConfig 的非托管数据结构 (代码生成)
    /// </summary>
    public partial struct UIConfigUnManaged : IConfigUnManaged<global::XM.UIConfigUnManaged>
    {
        // 字段

        public CfgI<global::XM.UIConfigUnManaged> Id;
        /// <summary>枚举</summary>
        public global::XM.Contracts.EUILayer UILayer;
        /// <summary>枚举</summary>
        public global::XM.Contracts.EUIType UIType;
        public bool IsFullScreen;
        public global::XM.Contracts.AssetI AssetPath;
        public global::XM.TypeI type;

        /// <summary>
        /// ToString方法
        /// </summary>
        /// <param name="dataContainer">数据容器</param>
        public string ToString(object dataContainer)
        {
            return "UIConfig";
        }
    }
}
