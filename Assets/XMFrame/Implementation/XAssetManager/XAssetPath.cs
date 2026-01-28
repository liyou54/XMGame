using System;
using XM.Contracts;

namespace XM
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

        public AssetI GetAssetId()
        {
            var modId = IModManager.I.GetModId(ModName);
            var assetId = IAssetManager.I.GetAsstIdByModIdAndPath(modId, Path);
            return assetId;
        }
    }
}