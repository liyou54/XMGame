using UnityEngine;
using Cysharp.Threading.Tasks;
using XM.Contracts;

namespace XM
{
    /// <summary>
    /// 管理器基类
    /// </summary>
    /// <typeparam name="T">管理器类型</typeparam>
    public abstract class ManagerBase<T> : MonoBehaviour, IManager<T> 
        where T : IManager<T>,IManager
    {
        public bool IsAvailable => IManager<T>.I != null;

        public abstract UniTask OnCreate();

        public abstract UniTask OnInit();

        public virtual UniTask OnDestroy()
        {
            return UniTask.CompletedTask;
        }

        public virtual UniTask OnEntryMainMenu()
        {
            return UniTask.CompletedTask;
        }

        public virtual UniTask OnEntryWorld()
        {
            return UniTask.CompletedTask;
        }

        public virtual UniTask OnStopWorld()
        {
            return UniTask.CompletedTask;
        }

        public virtual UniTask OnResumeWorld()
        {
            return UniTask.CompletedTask;
        }

        public virtual UniTask OnExitWorld()
        {
            return UniTask.CompletedTask;
        }

        public virtual UniTask OnQuitGame()
        {
            return UniTask.CompletedTask;
        }
    }
}
