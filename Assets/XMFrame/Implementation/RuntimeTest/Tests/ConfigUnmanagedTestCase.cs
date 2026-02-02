using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Cysharp.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using XM.Contracts;
using XM.Contracts.Config;
using XM.RuntimeTest;

namespace XM.RuntimeTest.Tests
{
#if UNITY_EDITOR
    /// <summary>
    /// ConfigUnmanaged 数据打印测试用例
    /// 遍历所有配置表，打印每个配置的 Unmanaged 结构体数据
    /// </summary>
    public class ConfigUnmanagedTestCase : ITestCase
    {
        public string Name => "ConfigUnmanaged数据打印";
        public string Description => "打印所有配置的Unmanaged结构体数据（包含容器内容）";

        public async UniTask<TestResult> ExecuteAsync()
        {
            var result = new TestResult();
            var stopwatch = Stopwatch.StartNew();

            try
            {
                result.DetailLog.AppendLine("=== ConfigUnmanaged 数据打印测试 ===\n");

                // 1. 获取 ConfigDataCenter
                var dataCenter = IConfigDataCenter.I;
                if (dataCenter == null)
                {
                    result.Success = false;
                    result.Message = "ConfigDataCenter 未初始化";
                    return result;
                }

                // 2. 通过反射获取 ConfigData
                var configData = GetConfigData(dataCenter);
                if (configData == null)
                {
                    result.Success = false;
                    result.Message = "无法通过反射获取 ConfigData";
                    return result;
                }

                // 3. 通过反射获取所有注册的 ClassHelper（从 _classHelperCache）
                var allHelpers = GetAllClassHelpers(dataCenter);
                if (allHelpers == null || allHelpers.Count == 0)
                {
                    result.Success = false;
                    result.Message = "未找到任何已注册的 ClassHelper";
                    return result;
                }

                result.DetailLog.AppendLine($"BlobContainer 有效: {configData.Value.BlobContainer.IsValid}\n");
                result.DetailLog.AppendLine($"找到 {allHelpers.Count} 个已注册的配置表\n");

                // 4. 遍历所有 ClassHelper 对应的表
                int totalTables = 0;
                int totalConfigs = 0;

                result.DetailLog.AppendLine("开始遍历所有配置表...\n");

                foreach (var helper in allHelpers)
                {
                    totalTables++;

                    // 获取表定义
                    var tblS = helper.GetTblS();
                    var tblI = dataCenter.GetTblI(tblS);
                    
                    if (!tblI.Valid)
                    {
                        result.DetailLog.AppendLine($"【表 {totalTables}】 {tblS.DefinedInMod.Name}::{tblS.TableName} - TblI 无效\n");
                        continue;
                    }

                    string tableName = $"{tblS.DefinedInMod.Name}::{tblS.TableName}";
                    result.DetailLog.AppendLine($"【表 {totalTables}】 TableId={tblI.TableId}, 名称={tableName}");
                    result.DetailLog.AppendLine(new string('-', 60));

                    // 获取 Unmanaged 类型
                    var unmanagedType = GetUnmanagedType(helper);
                    if (unmanagedType == null)
                    {
                        result.DetailLog.AppendLine($"  警告: 无法获取 {tableName} 的 Unmanaged 类型\n");
                        continue;
                    }

                    // 打印该表的所有配置
                    int configCount = PrintTableConfigs(configData.Value, tblI, unmanagedType, result.DetailLog);
                    totalConfigs += configCount;

                    result.DetailLog.AppendLine($"  表 {tableName} 共 {configCount} 个配置\n");

                    await UniTask.Yield(); // 避免长时间阻塞
                }

                // 5. 生成统计信息
                stopwatch.Stop();
                result.Success = true;
                result.Message = $"成功打印 {totalTables} 个表，共 {totalConfigs} 个配置";
                result.Statistics = $"[表: {totalTables}, 配置: {totalConfigs}]";
                result.ExecutionTime = (float)stopwatch.Elapsed.TotalSeconds;

                result.DetailLog.AppendLine("=== 测试完成 ===");
                result.DetailLog.AppendLine($"总表数: {totalTables}");
                result.DetailLog.AppendLine($"总配置数: {totalConfigs}");
                result.DetailLog.AppendLine($"耗时: {result.ExecutionTime:F2}s");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                result.Success = false;
                result.Message = $"测试执行异常: {ex.Message}";
                result.ExecutionTime = (float)stopwatch.Elapsed.TotalSeconds;
                result.DetailLog.AppendLine($"\n异常信息: {ex}");
                XLog.Error($"ConfigUnmanagedTestCase 执行失败: {ex}");
            }

            return result;
        }

        #region 反射辅助方法

        /// <summary>通过反射获取 ConfigData</summary>
        private ConfigData? GetConfigData(IConfigDataCenter dataCenter)
        {
            try
            {
                var dataCenterType = dataCenter.GetType();
                var configHolderField = dataCenterType.GetField("_configHolder", 
                    BindingFlags.NonPublic | BindingFlags.Instance);
                
                if (configHolderField == null)
                    return null;

                var configHolder = configHolderField.GetValue(dataCenter);
                if (configHolder == null)
                    return null;

                var holderType = configHolder.GetType();
                var dataField = holderType.GetField("Data", BindingFlags.Public | BindingFlags.Instance);
                
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

        /// <summary>通过反射获取所有已注册的 ClassHelper</summary>
        private System.Collections.Generic.List<ConfigClassHelper> GetAllClassHelpers(IConfigDataCenter dataCenter)
        {
            try
            {
                var dataCenterType = dataCenter.GetType();
                var cacheField = dataCenterType.GetField("_classHelperCache", 
                    BindingFlags.NonPublic | BindingFlags.Instance);
                
                if (cacheField == null)
                {
                    XLog.Warning("未找到 _classHelperCache 字段");
                    return null;
                }

                var cache = cacheField.GetValue(dataCenter);
                if (cache == null)
                {
                    XLog.Warning("_classHelperCache 为空");
                    return null;
                }

                // MultiKeyDictionary 有 Values 属性
                var valuesProperty = cache.GetType().GetProperty("Values");
                if (valuesProperty == null)
                {
                    XLog.Warning("未找到 Values 属性");
                    return null;
                }

                var values = valuesProperty.GetValue(cache);
                if (values is System.Collections.IEnumerable enumerable)
                {
                    var helpers = new System.Collections.Generic.List<ConfigClassHelper>();
                    foreach (var item in enumerable)
                    {
                        if (item is ConfigClassHelper helper)
                        {
                            helpers.Add(helper);
                        }
                    }
                    return helpers;
                }

                return null;
            }
            catch (Exception ex)
            {
                XLog.Error($"反射获取 ClassHelper 列表失败: {ex.Message}");
                return null;
            }
        }


        /// <summary>获取 Unmanaged 类型</summary>
        private Type GetUnmanagedType(ConfigClassHelper helper)
        {
            try
            {
                var helperType = helper.GetType();
                var baseType = helperType.BaseType;
                
                if (baseType != null && baseType.IsGenericType)
                {
                    var genericArgs = baseType.GetGenericArguments();
                    if (genericArgs.Length >= 2)
                    {
                        return genericArgs[1]; // TUnmanaged 是第二个泛型参数
                    }
                }
            }
            catch (Exception ex)
            {
                XLog.Warning($"获取 Unmanaged 类型失败: {ex.Message}");
            }

            return null;
        }

        /// <summary>打印表中的所有配置</summary>
        private int PrintTableConfigs(ConfigData configData, TblI tblI, Type unmanagedType, StringBuilder log)
        {
            int count = 0;

            try
            {
                // 调用 ConfigData.GetMap<CfgI, TUnmanaged>(tblI)
                var getMapMethod = typeof(ConfigData).GetMethod("GetMap");
                if (getMapMethod == null)
                {
                    log.AppendLine("  错误: 未找到 GetMap 方法");
                    return 0;
                }

                var genericGetMap = getMapMethod.MakeGenericMethod(typeof(CfgI), unmanagedType);
                var boxedConfigData = (object)configData;
                var map = genericGetMap.Invoke(boxedConfigData, new object[] { tblI });

                if (map == null)
                {
                    log.AppendLine("  警告: Map 为空");
                    return 0;
                }

                // 获取 Map 的 GetLength 方法
                var mapType = map.GetType();
                var getLengthMethod = mapType.GetMethod("GetLength");
                if (getLengthMethod != null)
                {
                    var length = (int)getLengthMethod.Invoke(map, new object[] { configData.BlobContainer });
                    
                    if (length == 0)
                    {
                        log.AppendLine("  (空表)");
                        return 0;
                    }

                    // 获取枚举器
                    var getEnumeratorMethod = mapType.GetMethod("GetEnumerator");
                    if (getEnumeratorMethod != null)
                    {
                        var enumerator = getEnumeratorMethod.Invoke(map, new object[] { configData.BlobContainer });
                        var enumeratorType = enumerator.GetType();
                        var moveNextMethod = enumeratorType.GetMethod("MoveNext");
                        var currentProperty = enumeratorType.GetProperty("Current");

                        int maxPrint = 10; // 最多打印前10个配置
                        int printed = 0;

                        while ((bool)moveNextMethod.Invoke(enumerator, null) && printed < maxPrint)
                        {
                            var kvp = currentProperty.GetValue(enumerator);
                            var kvpType = kvp.GetType();
                            
                            var cfgI = kvpType.GetProperty("Key").GetValue(kvp);
                            var configValue = kvpType.GetProperty("Value").GetValue(kvp);

                            // 调用 ToString(container) 方法
                            var toStringMethod = unmanagedType.GetMethod("ToString", new[] { typeof(XBlobContainer) });
                            string configStr;
                            
                            if (toStringMethod != null)
                            {
                                configStr = (string)toStringMethod.Invoke(configValue, new object[] { configData.BlobContainer });
                            }
                            else
                            {
                                configStr = configValue.ToString();
                            }

                            log.AppendLine($"  [{printed + 1}] CfgI={cfgI}");
                            log.AppendLine($"      {configStr}");
                            
                            printed++;
                            count++;
                        }

                        // 继续计数剩余的配置
                        while ((bool)moveNextMethod.Invoke(enumerator, null))
                        {
                            count++;
                        }

                        if (count > maxPrint)
                        {
                            log.AppendLine($"  ... 还有 {count - maxPrint} 个配置（已省略）");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.AppendLine($"  错误: {ex.Message}");
                XLog.Warning($"打印表配置失败: {ex.Message}");
            }

            return count;
        }

        #endregion
    }
#endif
}
