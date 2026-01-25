using UnityEditor;
using XMFrame.Editor.ConfigEditor;

namespace XMFrame.Editor.ConfigEditor
{
    /// <summary>
    /// 配置代码生成菜单
    /// </summary>
    public static class ConfigCodeGenMenu
    {
        [MenuItem("XMFrame/Config/Generate Code (Select Assemblies)")]
        public static void ShowCodeGenWindow()
        {
            UnmanagedCodeGenWindow.ShowWindow();
        }
    }
}
