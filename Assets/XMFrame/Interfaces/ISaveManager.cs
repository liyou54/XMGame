using System.Collections.Generic;
using XM;

namespace XM.Contracts
{
    /// <summary>
    /// Mod存档信息
    /// </summary>
    public class SavedModInfo
    {
        public string ModName { get; set; }
        public string Version { get; set; }
        public bool IsEnabled { get; set; }

        public SavedModInfo(string modName, string version, bool isEnabled)
        {
            ModName = modName;
            Version = version;
            IsEnabled = isEnabled;
        }
    }

    public interface ISaveManager:IManager<ISaveManager>
    {
        /// <summary>
        /// 读取已保存的Mod配置列表
        /// </summary>
        /// <returns>Mod配置列表，如果不存在则返回空列表</returns>
        List<SavedModInfo> LoadModConfigs();

        /// <summary>
        /// 保存Mod配置列表
        /// </summary>
        /// <param name="modConfigs">要保存的Mod配置列表</param>
        void SaveModConfigs(List<SavedModInfo> modConfigs);
    }

  
}