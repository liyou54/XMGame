using System;
using UnityEngine;
using XMFrame.Interfaces;
using XMFrame.Utils;
using XMFrame.Utils.Attribute;

namespace XMFrame.Implementation
{
    public class UIConfigClass:XConfig<UIConfigClass,UIConfigUnManaged>
    {
        public ConfigKey<UIConfigClass> Id;
        public XAssetPath AssetPath;
       [XmlIndex("WindowType",false,0)] public Type ConfigType;
    }
    
    public struct UIConfigUnManaged:IConfigUnManaged<UIConfigUnManaged>
    {
        
        public struct PrefabAssetIndex : IConfigIndexGroup<UIConfigUnManaged>
        {
            public XAssetId PrefabAsset;

            public PrefabAssetIndex(XAssetId prefabAsset)
            {
                PrefabAsset = prefabAsset;
            }
        }
        
        public CfgId<UIConfigUnManaged> Id;
        public CfgId UIHandle;
        public XAssetId Prefab;
        public TypeId TypeId;
    }
}