using System;
using System.Collections.Generic;
using UnityEngine;
using XMFrame.Implementation;
using XMFrame.Interfaces;

public abstract class UICtrlBase : MonoBehaviour, IUICtrlBase
{
    // 编辑器下挂载的子组件 关闭时不回收
    
    [field: NonSerialized]  public List<IUICtrlBase> StaticSubUICtrl { get; set; }

    // 动态挂载的组件 关闭时回收
    [field: NonSerialized] public List<UILoadBox> DynamicLoadBox { get; set; } = new List<UILoadBox>();

    [field: NonSerialized] public List<XAssetHandle> LoadedAssetIdList { get; set; }

    public UIHandle Id { get; set; }

    public void AttachLoadBox(UILoadBox uiLoadBox)
    {
        DynamicLoadBox.Add(uiLoadBox);
    }



    public virtual void OnShow()
    {
        // 子类可以重写此方法
    }

    public virtual void OnHide()
    {
        // 子类可以重写此方法
    }

    public virtual void OnClose()
    {
        // 子类可以重写此方法
    }
}

public abstract class UIWindowCtrlBase : UICtrlBase, IUIWindowCtrlBase
{
    [field: SerializeField] public EUILayer Layer { get; set; }
    [field: SerializeField] public EUIInstanceType UIType { get; set; }
    [field: SerializeField] public bool IsShowMask { get; set; }
}