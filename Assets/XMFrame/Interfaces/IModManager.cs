#nullable enable
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

        /// <summary>
        /// 通过ModI获取Mod名称
        /// </summary>
        string GetModName(ModI modId);
        
        public IEnumerable<SortedModConfig> GetSortedModConfigs();
        public IEnumerable<ModRuntime> GetModRuntime();
        ModRuntime? GetModRuntimeByName(string modName);
        public IEnumerable<string> GetModXmlFilePathByModId(ModI modId);
        int GetModSortIndex(string modName);
    }
    

}
