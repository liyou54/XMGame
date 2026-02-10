using System;
using XM.Contracts;
using XM.Contracts.Config;
using XM.Utils.Attribute;

namespace XM
{
    public class UIConfig : IXConfig<UIConfig, UIConfigUnManaged>
    {
        [XmlKey] public CfgS<UIConfig> Id;
        public EUILayer UILayer;
        public EUIType UIType;
        public bool IsFullScreen;
        public XAssetPath AssetPath;
        public Type type;
    }

    public partial struct UIConfigUnManaged : IConfigUnManaged<UIConfigUnManaged>
    {
        
    }
}