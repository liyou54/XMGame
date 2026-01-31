using System.Collections.Generic;
using XM;

namespace XM.Contracts
{
    public interface IModManager:IManager<IModManager>
    {
        /// <summary>
        /// 通过Mod名称获取ModId
        /// </summary>
        ModI GetModId(string modName);
        
        public IEnumerable<SortedModConfig> GetSortedModConfigs();
        public IEnumerable<ModRuntime> GetModRuntime();
        public IEnumerable<string> GetModXmlFilePathByModId(ModI modId);
    }
    

}
