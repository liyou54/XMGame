using System;

namespace XM.Contracts
{

    public struct UIHandle : IEquatable<UIHandle>
    {
        UIType UIType;
        int instanceID;

        public bool Equals(UIHandle other)
        {
            return UIType.Equals(other.UIType) && instanceID == other.instanceID;
        }

        public override bool Equals(object obj)
        {
            return obj is UIHandle other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(UIType, instanceID);
        }
    }

    public struct UIType : IEquatable<UIType>
    {
        public CfgI Cfg;

        public bool Equals(UIType other)
        {
            return Cfg.Equals(other.Cfg);
        }

        public override bool Equals(object obj)
        {
            return obj is UIType other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Cfg.GetHashCode();
        }
    }
    
    public interface IUIManager:IManager<IUIManager>
    {
        // 注册UI 
        public UIType RegisterUI(CfgI cfgI);
        // 创建UI
        public UIHandle CreateUI(UIType type);
        // 根据句柄关闭ui 一般用于Multi模式 也可以用于Stack/Single
        public void CloseUI(UIHandle handle);
        // 根据类型关闭UI 用于 Stack/Single 也可以作用Multi栈顶元素
        public void CloseUI(UIType type);
        // 根据句柄关闭ui 一般用于Multi模式 也可以用于Stack/Single
        public void HideUI(UIHandle handle,float autoCloseTime = 5);
        // 根据类型隐藏UI 用于 Stack/Single  也可以作用Multi栈顶元素
        public void HideUI(UIType type,float autoCloseTime = 5);
        // 根据句柄展示UI 用于 Stack/Single 句柄失效/UI没有被隐藏 则 CreateUI
        public void ShowUI(UIHandle handle);
        // 根据类型关闭ui  用于 Stack/Single  也可以作用Multi栈顶元素
        public UIHandle ShowUI(UIType type);
        // 根据类型关闭UI  用于 Stack/Single 也可以作用Multi栈顶元素
        public void CloseAllUI(UIType type);
        // 根据句柄移动UI栈 一般用于Multi模式 也可以用于Stack/Single
        public void MoveToTop(UIHandle  handle);
        // 根据类型移动UI栈  用于 Stack/Single  也可以作用Multi栈顶元素
        public void MoveToTop(UIType  type);
    }
}