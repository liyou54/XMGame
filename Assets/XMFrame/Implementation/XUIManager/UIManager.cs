using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using XMFrame.Implementation;
using XMFrame.Interfaces;
using XMFrame.Utils;

namespace XMFrame.Implementation
{
    public class UIStack
    {
        public bool IsHideSelf;
        public bool IsHideByStack;
    }


    public class LoadedUIData
    {
        public ConfigHandle<UIConfigUnManaged> UITypeId;
        public GameObject UIPrefab;
        public UIConfig Config;
        public XAssetHandle AssetHandle;
        public UICtrlBase UICtrl;
        public Dictionary<UIHandle, GameObject> ActiveUIHandles = new Dictionary<UIHandle, GameObject>();

        public Dictionary<UIHandle, (GameObject go, float closeTime)> WillReleaseUIHandles =
            new Dictionary<UIHandle, (GameObject go, float delay)>();

        public EAssetStatus Status;
    }


    public class UIConfig
    {
    }


    public class UIWindowConfig : UIConfig
    {
        public ConfigHandle UITypeId;
        public EUILayer Layer;
        public EUIInstanceType InstanceType;
        public bool IsShowMask;
    }


    /// <summary>
    /// UI管理器
    /// </summary>
    [AutoCreate]
    public class UIManager : ManagerBase<IUIManager>, IUIManager
    {
        public Dictionary<Type, LoadedUIData> loadedUIData = new Dictionary<Type, LoadedUIData>();

        public override UniTask OnCreate()
        {
            return UniTask.CompletedTask;
        }

        public override UniTask OnInit()
        {
            return UniTask.CompletedTask;
        }

        public bool TryGetUIConfig<T>(out UIConfigUnManaged config)
        {
            TypeId typeid = TypeSystem.GetTypeId<T>();
            return IConfigDataCenter.I.TryGetConfigBySingleIndex(
                new UIConfigUnManaged.WindowTypeIndex(typeid), out config);
        }


        public async UniTask<UIHandle> OpenUI<T>() where T : UIWindowCtrlBase
        {
            if (!loadedUIData.TryGetValue(typeof(T), out LoadedUIData data))
            {
                data = await LoadUI<T>();
                if (data == null)
                {
                    // log
                    return default;
                }
            }
            return default;
        }

        public void CloseUI<T>(UIHandle? uiHandle = null) where T : UIWindowCtrlBase
        {
            throw new NotImplementedException();
        }

        public void HideUI<T>(UIHandle? uiHandle = null) where T : UIWindowCtrlBase
        {
            throw new NotImplementedException();
        }

        public bool RegisterUI<T>(ConfigHandle uiConfigId)
        {
            throw new NotImplementedException();
        }

        private async UniTask<LoadedUIData> LoadUI<T>() where T : UIWindowCtrlBase
        {
            if (!TryGetUIConfig<T>(out UIConfigUnManaged config))
            {
                // log
                return null;
            }

            var loadedUIData = new LoadedUIData();
            loadedUIData.UITypeId = config.Id;

            loadedUIData.Status = EAssetStatus.Loading;
            var handle = await config.Prefab.CreateHandleAsync();
            if (handle == null)
            {
                loadedUIData.Status = EAssetStatus.Failed;
                return loadedUIData;
            }

            loadedUIData.AssetHandle = handle;
            loadedUIData.UIPrefab = handle.Get<GameObject>();
            if (loadedUIData.UIPrefab == null)
            {
                loadedUIData.AssetHandle.Release();
                loadedUIData.Status = EAssetStatus.ErrorAndRelease;
                return loadedUIData;
            }

            var comp = loadedUIData.UIPrefab.GetComponent<T>();
            if (comp == null)
            {
                loadedUIData.AssetHandle.Release();
                loadedUIData.Status = EAssetStatus.ErrorAndRelease;
                return loadedUIData;
            }

            loadedUIData.UICtrl = comp;
            if (comp is UIWindowCtrlBase windowCtrl)
            {
                var windowConfig = new UIWindowConfig();
                windowConfig.UITypeId = config.UIHandle;
                windowConfig.Layer = windowCtrl.Layer;
                windowConfig.InstanceType = windowConfig.InstanceType;
                windowConfig.IsShowMask = windowConfig.IsShowMask;
                loadedUIData.Config = windowConfig;
            }
            else
            {
                // todo
            }

            return loadedUIData;
        }
    }
}