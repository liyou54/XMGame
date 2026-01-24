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
        public AssetPath AssetPath;
       [XmlIndex("WindowType",0)] public Type ConfigType;
    }
    
    public struct UIConfigUnManaged:IConfigUnManaged<UIConfigUnManaged>
    {
        
        public struct WindowTypeIndex:IConfigIndexGroup<UIConfigUnManaged>
        {
            public TypeId TypeId;

            public WindowTypeIndex(TypeId id)
            {
                TypeId = id;
            }
        }
        
        public ConfigHandle<UIConfigUnManaged> Id;
        public ConfigHandle UIHandle;
        public AssetId Prefab;
        public TypeId TypeId;
    }
}