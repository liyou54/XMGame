namespace XMFrame.Interfaces
{
    /// <summary>
    /// Mod 基础类
    /// </summary>
    public abstract class ModBase
    {
        public abstract void OnCreate();

        public abstract void OnInit();

        public virtual void OnDestroy()
        {
        }

        public virtual void OnEntryMainMenu()
        {
        }

        public virtual void OnEntryWorld()
        {
        }

        public virtual void OnStopWorld()
        {
        }

        public virtual void OnResumeWorld()
        {
        }

        public virtual void OnExitWorld()
        {
        }

        public virtual void OnQuitGame()
        {
        }
    }
}
