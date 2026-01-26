using System.Collections.Generic;
using XMFrame.Implementation;

namespace XMFrame.Interfaces
{
    public interface IModManager:IManager<IModManager>
    {
        /// <summary>
        /// 通过Mod名称获取ModId
        /// </summary>
        ModHandle GetModId(string modName);
        
        public IEnumerable<ModConfig> GetEnabledModConfigs();
        public IEnumerable<string> GetModXmlFilePathByModId(ModHandle modId);
    }
    

}
