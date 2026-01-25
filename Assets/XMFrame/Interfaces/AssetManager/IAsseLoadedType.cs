using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine.Pool;

namespace XMFrame.Interfaces
{
    public interface IAsseLoadedType
    {
        public List<XAssetHandle> LoadedAssetIdList { get; set; }

        public  async UniTask<XAssetHandle>  LoadAsset(XAssetId xAsset)
        {
            if (LoadedAssetIdList == null)
            {
                LoadedAssetIdList = ListPool<XAssetHandle>.Get();
            }

            if (!xAsset.Valid)
            {
                return null;
            }
            return await xAsset.CreateHandleAsync();
        }

        public void ReleaseAll()
        {
            if (LoadedAssetIdList == null)
            {
                return;
            }
            foreach (var assetId in LoadedAssetIdList)
            {
                assetId.Release();
            }
            LoadedAssetIdList.Clear();
            ListPool<XAssetHandle>.Release(LoadedAssetIdList);
            LoadedAssetIdList = null;
        }
    }
}