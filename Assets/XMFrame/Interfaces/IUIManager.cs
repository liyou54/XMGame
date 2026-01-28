using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using XM;

namespace XM.Contracts
{
    public enum EUILayer
    {
        None = 0,
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

    public struct UII : IEquatable<UII>
    {
        public CfgI TypeI;
        public short IsWidget;
        public int Id;

        public bool Valid => TypeI.Valid && Id > 0 ;

        public bool Equals(UII other)
        {
            return TypeI.Equals(other.TypeI) && IsWidget == other.IsWidget && Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            return obj is UII other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TypeI, IsWidget, Id);
        }
    }

    public interface IUIManager : IManager<IUIManager>
    {
        /// <summary>
        /// 打开UI窗口
        /// </summary>
        /// <param name="cfgId">UI配置ID</param>
        /// <returns>UI句柄</returns>
        UniTask<UII> OpenWindow(CfgI cfgId);

        /// <summary>
        /// 关闭UI窗口
        /// </summary>
        /// <param name="uiHandle">UI句柄</param>
        void CloseUI(UII uiHandle);

        /// <summary>
        /// 隐藏UI窗口（不销毁，可重新显示）
        /// </summary>
        /// <param name="uiHandle">UI句柄</param>
        void HideUI(UII uiHandle);

        /// <summary>
        /// 显示UI窗口（从隐藏状态恢复）
        /// </summary>
        /// <param name="uiHandle">UI句柄</param>
        void ShowUI(UII uiHandle);
        
        /// <summary>
        /// 注册UI类型
        /// </summary>
        /// <param name="uiConfigId">UI配置ID</param>
        /// <returns>是否注册成功</returns>
        bool RegisterUI(CfgI uiConfigId);

        /// <summary>
        /// 释放UI控制器资源
        /// </summary>
        /// <param name="uiCtrl">UI控制器实例</param>
        void ReleaseUICtrl(IUICtrlBase uiCtrl);

        /// <summary>
        /// 通过资源ID创建UI控制器
        /// </summary>
        /// <param name="mod">模块ID</param>
        /// <param name="path">资源路径</param>
        /// <param name="count">创建数量</param>
        /// <param name="uiHandles">输出的UI句柄列表</param>
        /// <returns>异步任务</returns>
        UniTask CreateUICtrlByAssetId(ModI mod, string path, int count, List<UII> uiHandles);

        /// <summary>
        /// 通过资源ID创建UI控制器
        /// </summary>
        /// <param name="id"></param>
        /// <param name="count">创建数量</param>
        /// <param name="uiHandles"></param>
        /// <returns>异步任务</returns>
        UniTask CreateUICtrlByConfig(CfgI id, int count,List<UII> uiHandles);

        /// <summary>
        /// 通过资源ID创建UI控制器
        /// </summary>
        /// <param name="count">创建数量</param>
        /// <param name="assetId"></param>
        /// <param name="uiHandles"></param>
        /// <returns>异步任务</returns>
        UniTask CreateUICtrlByConfig(AssetI assetId,int count,List<UII> uiHandles);
        
        IUICtrlBase GetUICtrlByHandle(UII handle);
    }
}