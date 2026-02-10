using System;
using XM.Contracts;
using XM.Contracts.Config;

namespace XM
{
    public class XAssetPathConvert : TypeConverterBase<string, XAssetPath, XAssetPathConvert>
    {
        public override bool Convert(string source, string modName, out XAssetPath target)
        {
            target = default;
            var strs = source.Split("::");
            if (strs.Length != 2)
            {
                return false;
            }

            modName = strs[0].Trim();
            var path = strs[1].Trim();
            target = new XAssetPath(modName, path);
            return true;
        }
    }

    public class XAssetPathToIConvert : TypeConverterBase<XAssetPath, AssetI, XAssetPathToIConvert>
    {
        public override bool Convert(XAssetPath source, string modName, out AssetI target)
        {
            var modId = IModManager.I.GetModId(source.ModName);
            target = default;
            if (!modId.Valid)
            {
                return false;
            }

            target = IAssetManager.I.GetAsstIdByModIdAndPath(modId, source.Path);
            return target.Valid;
        }
    }

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