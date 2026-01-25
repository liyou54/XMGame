using System.Collections.Generic;
using XMFrame.Implementation;

namespace XMFrame.Interfaces
{
    public interface IModManager:IManager<IModManager>
    {
        /// <summary>
        /// 通过Mod名称获取ModId
        /// </summary>
        ModId GetModId(string modName);
    }
    

}
