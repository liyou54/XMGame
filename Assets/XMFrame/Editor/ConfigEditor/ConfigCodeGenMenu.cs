using UnityEditor;
using XMFrame.Editor.ConfigEditor;

namespace XMFrame.Editor.ConfigEditor
{
    /// <summary>
    /// 配置代码生成菜单
    /// </summary>
    public static class ConfigCodeGenMenu
    {
        [MenuItem("XMFrame/Config/Generate Unmanaged Code")]
        public static void GenerateUnmanagedCode()
        {
            UnmanagedCodeGenerator.GenerateAllUnmanagedCode();
        }

        [MenuItem("XMFrame/Config/Generate Unmanaged Code (Custom Path)")]
        public static void GenerateUnmanagedCodeCustomPath()
        {
            var path = EditorUtility.OpenFolderPanel("选择输出目录", "Assets", "");
            if (!string.IsNullOrEmpty(path))
            {
                // 转换为项目相对路径
                var projectPath = UnityEngine.Application.dataPath.Replace("/Assets", "").Replace("\\Assets", "");
                if (path.StartsWith(projectPath))
                {
                    var relativePath = path.Substring(projectPath.Length).TrimStart('\\', '/');
                    UnmanagedCodeGenerator.GenerateAllUnmanagedCode(relativePath);
                }
                else
                {
                    EditorUtility.DisplayDialog("错误", "选择的目录必须在项目目录内", "确定");
                }
            }
        }
    }
}
