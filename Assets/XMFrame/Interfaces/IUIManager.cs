using System;
using Cysharp.Threading.Tasks;
using XMFrame.Implementation;

namespace XMFrame.Interfaces
{
    public enum EUILayer
    {
        Background,
        Menu,
        Normal,
        Top,
        Tips,
        System
    }


    /// <summary>
    /// UI 实例类型
    /// </summary>
    public enum EUIInstanceType
    {
        /// <summary>
        /// 正常实例
        /// </summary>
        Normal,

        /// <summary>
        /// 多实例
        /// </summary>
        Multiple,

        /// <summary>
        /// 独立实例
        /// </summary>
        Standalone,

        /// <summary>
        /// 栈独立实例
        /// </summary>
        StackStandalone,
    }

    public class UIFrameInfo
    {
    }

    public interface IUIWindow
    {
    }
    public struct UIHandle : IEquatable<UIHandle>
    {
        public ConfigHandle TypeId;
        public short IsWidget;
        public int Id;

        public bool Equals(UIHandle other)
        {
            return TypeId.Equals(other.TypeId) && IsWidget == other.IsWidget && Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            return obj is UIHandle other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TypeId, IsWidget, Id);
        }
    }

    public interface IUIManager : IManager<IUIManager>
    {
        public UniTask<UIHandle> OpenUI<T>() where T : UIWindowCtrlBase;
        public void CloseUI<T>(UIHandle? uiHandle = null) where T : UIWindowCtrlBase;

        public void HideUI<T>(UIHandle? uiHandle = null) where T : UIWindowCtrlBase;
        public bool RegisterUI<T>(ConfigHandle uiConfigId);
    }
}