using System;

namespace XM.Contracts
{
    /// <summary>
    /// 标记 Mod 程序集的 Mod 名称。用于在程序集上声明该 DLL 对应的 Mod 名，主工程可通过反射读取。
    /// Mod 工程在「创建 Mod」时会自动生成带 [assembly: ModNameAttribute("ModName")] 的文件。
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public sealed class ModNameAttribute : Attribute
    {
        /// <summary>Mod 名称，与 ModDefine.xml 中的 Name 一致。</summary>
        public string ModName { get; }

        public ModNameAttribute(string modName)
        {
            ModName = modName ?? string.Empty;
        }
    }
}
