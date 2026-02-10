namespace XM.Contracts
{
    public enum EUIType
    {
        Stack, // 栈内UI受UI栈帧管理
        Single, //单例UI 栈帧内只有一份
        Multi  // 多实例UI 受UI栈帧管理
    }
}