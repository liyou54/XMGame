using System;
using System.Collections.Generic;
using UnityEngine;

namespace XMFrame.Interfaces
{
    public interface IUICtrlBase : IAsseLoadedType
    {
       public UIHandle Id {get; set;}

        /// <summary>
        /// 子UI控制器列表（用于递归显示/隐藏/关闭）
        /// </summary>
        List<IUICtrlBase> StaticSubUICtrl { get; }

        /// <summary>
        /// UI显示时调用
        /// </summary>
        void OnShow();

        /// <summary>
        /// UI隐藏时调用
        /// </summary>
        void OnHide();

        /// <summary>
        /// UI关闭时调用
        /// </summary>
        void OnClose();
    }

    public interface IUIWindowCtrlBase : IUICtrlBase
    {
    }
}