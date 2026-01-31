#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace XMod.Editor
{
    /// <summary>
    /// 为 TestConfigLargenum 生成大批量配置 XML，用于性能测试。
    /// 10 个配置类型，每种 1000 个 XML 文件，每个文件 1~20 条配置项。
    /// </summary>
    public static class GenerateTestConfigLargenumXml
    {
        private const string ModName = "TestConfigLargenum";
        private const string ModsFolderName = "Mods";
        private const string XmlFolderName = "Xml";
        private const int ConfigTypesCount = 10;
        private const int FilesPerConfigType = 1000;
        private const int MinItemsPerFile = 1;
        private const int MaxItemsPerFile = 20;

        /// <summary>
        /// XML 生成到：当前工程 Assets/Mods/TestConfigLargenum/Xml/
        /// 例如 XModTest 工程下为：XModTest/Assets/Mods/TestConfigLargenum/Xml/
        /// </summary>
        [MenuItem("Mod/TestConfigLargenum/生成大批量配置 XML (性能测试)", false, 20)]
        public static void Generate()
        {
            // 生成到当前打开工程的 Assets/Mods/TestConfigLargenum/Xml/
            string xmlRoot = Path.Combine(Application.dataPath, ModsFolderName, ModName, XmlFolderName);
            if (!Directory.Exists(xmlRoot))
            {
                Directory.CreateDirectory(xmlRoot);
            }

            var rng = new System.Random(12345);
            int totalFiles = 0;
            int totalItems = 0;

            for (int configIndex = 1; configIndex <= ConfigTypesCount; configIndex++)
            {
                string cls = $"PerfConfig{configIndex}";
                string idPrefix = $"perf{configIndex}_";
                for (int fileIndex = 0; fileIndex < FilesPerConfigType; fileIndex++)
                {
                    int itemCount = rng.Next(MinItemsPerFile, MaxItemsPerFile + 1);
                    string fileName = $"{cls}_{fileIndex:D4}.xml";
                    string filePath = Path.Combine(xmlRoot, fileName);
                    WriteOneXmlFile(filePath, cls, idPrefix, fileIndex, itemCount, rng);
                    totalFiles++;
                    totalItems += itemCount;
                }
            }

            AssetDatabase.Refresh();
            string absolutePath = Path.GetFullPath(xmlRoot);
            EditorUtility.DisplayDialog("生成完成",
                $"已生成 {totalFiles} 个 XML 文件，共 {totalItems} 条配置项。\n\n路径:\n{absolutePath}", "确定");
            Debug.Log($"[TestConfigLargenum] XML 已生成到: {absolutePath}");
        }

        private static void WriteOneXmlFile(string filePath, string cls, string idPrefix, int fileIndex, int itemCount, System.Random rng)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            sb.AppendLine("<!-- 性能测试用批量配置 -->");
            sb.AppendLine("<Root>");
            for (int i = 0; i < itemCount; i++)
            {
                string configId = $"{idPrefix}{fileIndex:D4}_{i}";
                string fullId = $"{ModName}::{configId}";
                string name = $"Item_{configId}";
                int level = rng.Next(1, 101);
                int tagCount = rng.Next(0, 5);
                sb.AppendLine($"  <ConfigItem cls=\"{cls}\" id=\"{configId}\">");
                sb.AppendLine($"    <Id>{fullId}</Id>");
                sb.AppendLine($"    <Name>{name}</Name>");
                sb.AppendLine($"    <Level>{level}</Level>");
                if (tagCount > 0)
                {
                    for (int t = 0; t < tagCount; t++)
                        sb.AppendLine($"    <Tags>{rng.Next(1, 20)}</Tags>");
                }
                else
                {
                    sb.AppendLine("    <Tags>1</Tags>");
                }
                sb.AppendLine("  </ConfigItem>");
            }
            sb.AppendLine("</Root>");
            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
        }
    }
}
#endif
