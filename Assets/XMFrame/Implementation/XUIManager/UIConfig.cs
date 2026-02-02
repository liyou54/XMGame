using System;
using UnityEngine;
using XM.Contracts;
using XM.Contracts.Config;
using XM.Utils;
using XM.Utils.Attribute;

namespace XM
{
    public class UIConfigClass : IXConfig<UIConfigClass, UIConfigUnManaged>
    {
        public CfgS<UIConfigClass> Id;
        public XAssetPath AssetPath;
        [XmlIndex("WindowType", false, 0)] public Type ConfigType;
        public CfgI Data { get; set; }
    }

    public struct UIConfigUnManaged : IConfigUnManaged<UIConfigUnManaged>
    {
        public struct PrefabAssetIndex : IConfigIndexGroup<UIConfigUnManaged>
        {
            public AssetI PrefabAsset;

            public PrefabAssetIndex(AssetI prefabAsset)
            {
                PrefabAsset = prefabAsset;
            }
        }

        public CfgI<UIConfigUnManaged> Id;
        public CfgI UIHandle;
        public AssetI Prefab;
        public string ToString(object dataContainer)
        {
            throw new NotImplementedException();
        }
    }
}