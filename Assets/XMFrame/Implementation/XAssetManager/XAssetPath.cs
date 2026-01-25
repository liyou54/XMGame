using System;
using XMFrame.Interfaces;

namespace XMFrame.Implementation
{
    [Serializable]
    public struct XAssetPath
    {
        public readonly string ModName;
        public readonly string Path;

        public XAssetPath(string modName, string path)
        {
            ModName = modName;
            Path = path;
        }

        public XAssetId GetAssetId()
        {
            var modId = IModManager.I.GetModId(ModName);
            var assetId = IAssetManager.I.GetAsstIdByModIdAndPath(modId, Path);
            return assetId;
        }
    }
}