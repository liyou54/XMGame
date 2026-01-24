using UnityEngine;

namespace XMFrame.Interfaces
{
    public abstract class UICtrlBase:MonoBehaviour
    {
    }

    public abstract class UIWindowCtrlBase : UICtrlBase
    {
       [field:SerializeField] public EUILayer Layer {get; private set;}
       [field:SerializeField] public EUIInstanceType  UIType {get; private set;}
       [field:SerializeField] public bool IsShowMask;
    }
}