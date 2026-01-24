using Cysharp.Threading.Tasks;
using XMFrame.Implementation;
using XMFrame.Interfaces;

namespace XMFrame
{
    public class ConfigDataCenter : ManagerBase<IConfigDataCenter>
    {
        public override UniTask OnCreate()
        {
            return UniTask.CompletedTask;
        }

        public override UniTask OnInit()
        {
            return UniTask.CompletedTask;
        }

        public void RegisterConfigTable()
        {
            
        }
    }  
}  