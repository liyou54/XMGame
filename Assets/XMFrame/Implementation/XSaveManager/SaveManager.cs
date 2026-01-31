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

        public static string MetaSavedPath = "../metaSave";
        
        public override UniTask OnCreate()
        {
            return UniTask.CompletedTask;
        }

        public override UniTask OnInit()
        {
            return UniTask.CompletedTask;
        }

        public List<SavedModInfo> LoadGameMetaSave()
        {
            var saveMod = new SavedModInfo("MyMod", "1.0.0", true);
            return new List<SavedModInfo> { saveMod };
        }

        public void SaveModConfigs(List<SavedModInfo> modConfigs)
        {
            throw new System.NotImplementedException();
        }
    }
}
