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

        // TODO 读取存档
        public List<SavedModInfo> LoadGameMetaSave()
        {
            var saveMod = new SavedModInfo("MyMod", "1.0.0", true);
            var saveMod1 = new SavedModInfo("TestConfigLargenum", "1.0.0", false);
            return new List<SavedModInfo> { saveMod ,saveMod1};
        }

        public void SaveModConfigs(List<SavedModInfo> modConfigs)
        {
            throw new System.NotImplementedException();
        }
    }
}
