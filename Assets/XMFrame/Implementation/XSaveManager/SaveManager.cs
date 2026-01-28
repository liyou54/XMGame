using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using XM;
using XM.Contracts;

namespace XM
{
    /// <summary>
    /// 存档管理器
    /// </summary>
    [AutoCreate]
    public class SaveManager : ManagerBase<ISaveManager>, ISaveManager
    {
        public override UniTask OnCreate()
        {
            return UniTask.CompletedTask;
        }

        public override UniTask OnInit()
        {
            return UniTask.CompletedTask;
        }

        public List<SavedModInfo> LoadModConfigs()
        {
            throw new System.NotImplementedException();
        }

        public void SaveModConfigs(List<SavedModInfo> modConfigs)
        {
            throw new System.NotImplementedException();
        }
    }
}
