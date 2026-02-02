using System;
using System.Text;
using XM;
using XM.Contracts;
using XM.Contracts.Config;

namespace MyMod
{
#if UNITY_EDITOR
    /// <summary>
    /// 配置打印工具 - 避免反射调用 ToString(XBlobContainer) 导致的栈指针问题
    /// 直接调用具体类型的 ToString 方法
    /// </summary>
    public static class ConfigPrinter
    {
        /// <summary>
        /// 打印 MyItemConfig 表的所有配置
        /// </summary>
        public static string PrintMyItemConfigs()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== MyItemConfig 配置打印 ===\n");

            try
            {
                var dataCenter = IConfigDataCenter.I;
                if (dataCenter == null)
                {
                    sb.AppendLine("错误: ConfigDataCenter 未初始化");
                    return sb.ToString();
                }

                // 获取 ConfigData
                var configData = GetConfigData(dataCenter);
                if (!configData.HasValue)
                {
                    sb.AppendLine("错误: 无法获取 ConfigData");
                    return sb.ToString();
                }

                // 获取 MyItemConfig 的 TblS
                var helper = dataCenter.GetClassHelper<MyItemConfig>();
                if (helper == null)
                {
                    sb.AppendLine("错误: 未找到 MyItemConfig 的 ClassHelper");
                    return sb.ToString();
                }

                var tblS = helper.GetTblS();
                var tblI = dataCenter.GetTblI(tblS);

                if (!tblI.Valid)
                {
                    sb.AppendLine("错误: TblI 无效");
                    return sb.ToString();
                }

                sb.AppendLine($"表名: {tblS.DefinedInMod.Name}::{tblS.TableName}");
                sb.AppendLine($"TableId: {tblI.TableId}");
                sb.AppendLine(new string('-', 80));

                // 获取 Map
                var map = configData.Value.GetMap<CfgI<MyItemConfigUnManaged>, MyItemConfigUnManaged>(tblI);
                var container = configData.Value.BlobContainer;
                int length = map.GetLength(container);

                sb.AppendLine($"配置数量: {length}\n");

                if (length == 0)
                {
                    sb.AppendLine("(空表)");
                    return sb.ToString();
                }

                // 遍历打印
                int count = 0;
                int maxPrint = 20; // 最多打印 20 个

                foreach (var kvp in map.GetEnumerator(container))
                {
                    if (count >= maxPrint) break;

                    var cfgI = kvp.Key;
                    var config = kvp.Value;

                    // 直接调用 ToString(container) - 不使用反射
                    string configStr = config.ToString(container);

                    sb.AppendLine($"[{count + 1}] CfgI={cfgI}");
                    sb.AppendLine($"    {configStr}");
                    sb.AppendLine();

                    count++;
                }

                if (length > maxPrint)
                {
                    sb.AppendLine($"... 还有 {length - maxPrint} 个配置（已省略）");
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"\n异常: {ex.Message}");
                sb.AppendLine($"堆栈: {ex.StackTrace}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// 打印 TestConfig 表的所有配置
        /// </summary>
        public static string PrintTestConfigs()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== TestConfig 配置打印 ===\n");

            try
            {
                var dataCenter = IConfigDataCenter.I;
                if (dataCenter == null)
                {
                    sb.AppendLine("错误: ConfigDataCenter 未初始化");
                    return sb.ToString();
                }

                // 获取 ConfigData
                var configData = GetConfigData(dataCenter);
                if (!configData.HasValue)
                {
                    sb.AppendLine("错误: 无法获取 ConfigData");
                    return sb.ToString();
                }

                // 获取 TestConfig 的 TblS
                var helper = dataCenter.GetClassHelper<TestConfig>();
                if (helper == null)
                {
                    sb.AppendLine("错误: 未找到 TestConfig 的 ClassHelper");
                    return sb.ToString();
                }

                var tblS = helper.GetTblS();
                var tblI = dataCenter.GetTblI(tblS);

                if (!tblI.Valid)
                {
                    sb.AppendLine("错误: TblI 无效");
                    return sb.ToString();
                }

                sb.AppendLine($"表名: {tblS.DefinedInMod.Name}::{tblS.TableName}");
                sb.AppendLine($"TableId: {tblI.TableId}");
                sb.AppendLine(new string('-', 80));

                // 获取 Map
                var map = configData.Value.GetMap<CfgI<TestConfigUnManaged>, TestConfigUnManaged>(tblI);
                var container = configData.Value.BlobContainer;
                int length = map.GetLength(container);

                sb.AppendLine($"配置数量: {length}\n");

                if (length == 0)
                {
                    sb.AppendLine("(空表)");
                    return sb.ToString();
                }

                // 遍历打印
                int count = 0;
                int maxPrint = 10; // TestConfig 比较复杂，只打印 10 个

                foreach (var kvp in map.GetEnumerator(container))
                {
                    if (count >= maxPrint) break;

                    var cfgI = kvp.Key;
                    var config = kvp.Value;

                    // 直接调用 ToString(container) - 不使用反射
                    string configStr = config.ToString(container);

                    sb.AppendLine($"[{count + 1}] CfgI={cfgI}");
                    sb.AppendLine($"    {configStr}");
                    sb.AppendLine();

                    count++;
                }

                if (length > maxPrint)
                {
                    sb.AppendLine($"... 还有 {length - maxPrint} 个配置（已省略）");
                }
            }
            catch (Exception ex)
            {
                sb.AppendLine($"\n异常: {ex.Message}");
                sb.AppendLine($"堆栈: {ex.StackTrace}");
            }

            return sb.ToString();
        }

        /// <summary>
        /// 打印所有 MyMod 中的配置表
        /// </summary>
        public static string PrintAllMyModConfigs()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== MyMod 所有配置表打印 ===\n");
            sb.AppendLine($"时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n");
            sb.AppendLine(new string('=', 80));
            sb.AppendLine();

            // 打印 MyItemConfig
            sb.AppendLine(PrintMyItemConfigs());
            sb.AppendLine();
            sb.AppendLine(new string('=', 80));
            sb.AppendLine();

            // 打印 TestConfig
            sb.AppendLine(PrintTestConfigs());
            sb.AppendLine();
            sb.AppendLine(new string('=', 80));

            return sb.ToString();
        }

        /// <summary>
        /// 通过反射获取 ConfigData（私有字段访问）
        /// </summary>
        private static ConfigData? GetConfigData(IConfigDataCenter dataCenter)
        {
            try
            {
                var dataCenterType = dataCenter.GetType();
                var configHolderField = dataCenterType.GetField("_configHolder",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (configHolderField == null)
                    return null;

                var configHolder = configHolderField.GetValue(dataCenter);
                if (configHolder == null)
                    return null;

                var holderType = configHolder.GetType();
                var dataField = holderType.GetField("Data", 
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                if (dataField == null)
                    return null;

                return (ConfigData)dataField.GetValue(configHolder);
            }
            catch (Exception ex)
            {
                XLog.Error($"反射获取 ConfigData 失败: {ex.Message}");
                return null;
            }
        }
    }
#endif
}
