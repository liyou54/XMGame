using Cysharp.Threading.Tasks;
using XMFrame.Implementation;

namespace XMFrame.Interfaces
{
    /// <summary>
    /// 管理器基础接口
    /// </summary>
    public interface IManager
    {
        public abstract UniTask OnCreate();

        public abstract UniTask OnInit();
        public abstract UniTask OnDestroy();
        public abstract UniTask OnEntryMainMenu();
        public abstract UniTask OnEntryWorld();
        public abstract UniTask OnStopWorld();
        public abstract UniTask OnResumeWorld();
        public abstract UniTask OnExitWorld();
        public abstract UniTask OnQuitGame();
    }

    public interface IManager< TI> : IManager 
        where TI : IManager<TI>
    {
        public static TI I;
    }
}