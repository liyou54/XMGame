using System;
using System.Collections.Generic;
using System.IO;
using XM.ConfigNew.Metadata;
using XM.ConfigNew.Tools;

namespace XM.ConfigNew.CodeGen
{
    /// <summary>
    /// 代码生成管理器 - 统一管理代码生成流程
    /// </summary>
    public class CodeGenerationManager
    {
        /// <summary>
        /// 为程序集列表中的所有配置类型生成代码（用于 Mod 工程批量生成）
        /// </summary>
        /// <param name="assemblies">程序集列表</param>
        /// <param name="outputDirectory">输出目录</param>
        /// <returns>生成的文件数量</returns>
        public static int GenerateForAssemblies(List<System.Reflection.Assembly> assemblies, string outputDirectory)
        {
            if (assemblies == null || assemblies.Count == 0)
                return 0;
            
            var manager = new CodeGenerationManager();
            var totalFiles = 0;
            
            foreach (var assembly in assemblies)
            {
                if (assembly == null)
                    continue;
                
                // 获取程序集中所有实现 IXConfig 的配置类型
                var configTypes = TypeAnalyzer.FindConfigTypesInAssembly(assembly);
                
                foreach (var configType in configTypes)
                {
                    try
                    {
                        var files = manager.GenerateForType(configType, outputDirectory);
                        totalFiles += files.Count;
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogError($"[CodeGenerationManager] 生成 {configType.Name} 失败: {ex.Message}");
                    }
                }
            }
            
            return totalFiles;
        }
        
        /// <summary>
        /// 为配置类型生成所有代码文件
        /// </summary>
        /// <param name="configType">配置类型</param>
        /// <param name="outputDirectory">输出目录</param>
        /// <returns>生成的文件路径列表</returns>
        public List<string> GenerateForType(Type configType, string outputDirectory)
        {
            if (configType == null)
                throw new ArgumentNullException(nameof(configType));
            
            if (string.IsNullOrEmpty(outputDirectory))
                throw new ArgumentException("输出目录不能为空", nameof(outputDirectory));
            
            // 确保输出目录存在
            Directory.CreateDirectory(outputDirectory);
            
            // 1. 分析类型元数据
            var metadata = TypeAnalyzer.AnalyzeConfigType(configType);
            
            // 2. 生成文件
            var generatedFiles = new List<string>();
            
            // 2.1 生成Unmanaged结构体
            var unmanagedFile = GenerateUnmanagedStruct(metadata, outputDirectory);
            generatedFiles.Add(unmanagedFile);
            
            // 2.2 生成索引结构体(每个索引一个文件)
            if (metadata.Indexes != null)
            {
                foreach (var index in metadata.Indexes)
                {
                    var indexFile = GenerateIndexStruct(metadata, index, outputDirectory);
                    generatedFiles.Add(indexFile);
                }
            }
            
            // 2.3 生成 XmlHelper (ClassHelper)
            var helperFile = GenerateXmlHelper(metadata, outputDirectory);
            generatedFiles.Add(helperFile);
            
            return generatedFiles;
        }
        
        #region 私有生成方法

        /// <summary>
        /// 仅当文件内容发生变化时写入，避免不必要的磁盘写入和编译触发
        /// </summary>
        private static void WriteIfChanged(string filePath, string newContent)
        {
            if (File.Exists(filePath) && File.ReadAllText(filePath) == newContent)
                return;
            File.WriteAllText(filePath, newContent);
        }
        
        /// <summary>
        /// 生成Unmanaged结构体文件
        /// </summary>
        private string GenerateUnmanagedStruct(ConfigClassMetadata metadata, string outputDirectory)
        {
            var generator = new UnmanagedGenerator(metadata);
            var code = generator.Generate();
            
            var fileName = $"{metadata.UnmanagedTypeName}.Gen.cs";
            var filePath = Path.Combine(outputDirectory, fileName);
            
            WriteIfChanged(filePath, code);
            
            return filePath;
        }
        
        /// <summary>
        /// 生成索引结构体文件
        /// </summary>
        private string GenerateIndexStruct(ConfigClassMetadata classMetadata, ConfigIndexMetadata indexMetadata, string outputDirectory)
        {
            var generator = new IndexStructGenerator(classMetadata, indexMetadata);
            var code = generator.Generate();
            
            var fileName = $"{classMetadata.UnmanagedTypeName}.{indexMetadata.IndexName}.Gen.cs";
            var filePath = Path.Combine(outputDirectory, fileName);
            
            WriteIfChanged(filePath, code);
            
            return filePath;
        }
        
        /// <summary>
        /// 生成 XmlHelper (ClassHelper) 文件
        /// </summary>
        private string GenerateXmlHelper(ConfigClassMetadata metadata, string outputDirectory)
        {
            var generator = new XmlHelperGenerator(metadata);
            var code = generator.Generate();
            
            var fileName = $"{metadata.HelperTypeName}.Gen.cs";
            var filePath = Path.Combine(outputDirectory, fileName);
            
            WriteIfChanged(filePath, code);
            
            return filePath;
        }
        
        #endregion
    }
}
