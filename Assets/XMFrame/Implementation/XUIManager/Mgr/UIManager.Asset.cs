using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using XM.Contracts;

namespace XM
{
    
    
    
    
    public partial class UIManager:ManagerBase<IUIManager>, IUIManager
    {

        public Dictionary<CfgI, Queue<UICtrlBase>> WillBeRecycled = new Dictionary<CfgI, Queue<UICtrlBase>>();
        
        public UICtrlBase TryGetUICtrl(CfgI cfg)
        {
            if (WillBeRecycled.TryGetValue(cfg, out var queue) && queue.Count > 0)
            {
                return queue.Dequeue();
            }
            
        }
    }
}
