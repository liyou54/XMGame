using Cysharp.Threading.Tasks;
using UnityEngine;
using XM.Contracts;

namespace XM
{
    [ManagerDependency(typeof(IAssetManager))]
    [ManagerDependency(typeof(IConfigManager))]
    [AutoCreate]
    public partial class UIManager : ManagerBase<IUIManager>, IUIManager
    {
        public override UniTask OnCreate()
        {
            return default;
        }

        public override async UniTask OnInit()
        {
            var modID = IModManager.I.GetModId("MyMod");
            var matI = await IAssetManager.I.LoadAssetAsync<AssetI>(modID, "New Material");
            var handle = await matI.CreateHandleAsync();
            var mat = handle.Get<Material>();
            
        }

        public UIType RegisterUI(CfgI cfgI)
        {
            throw new System.NotImplementedException();
        }

        public UIHandle CreateUI(UIType type)
        {
            throw new System.NotImplementedException();
        }

        public void CloseUI(UIHandle handle)
        {
            throw new System.NotImplementedException();
        }

        public void CloseUI(UIType type)
        {
            throw new System.NotImplementedException();
        }

        public void HideUI(UIHandle handle, float autoCloseTime = 5)
        {
            throw new System.NotImplementedException();
        }

        public void HideUI(UIType type, float autoCloseTime = 5)
        {
            throw new System.NotImplementedException();
        }

        public void ShowUI(UIHandle handle)
        {
            throw new System.NotImplementedException();
        }

        public UIHandle ShowUI(UIType type)
        {
            throw new System.NotImplementedException();
        }

        public void CloseAllUI(UIType type)
        {
            throw new System.NotImplementedException();
        }

        public void MoveToTop(UIHandle handle)
        {
            throw new System.NotImplementedException();
        }

        public void MoveToTop(UIType type)
        {
            throw new System.NotImplementedException();
        }
    }
}