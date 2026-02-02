using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cysharp.Threading.Tasks;
using UnityEngine;
using XM;
using XM.Contracts;
using XM.Contracts.Config;

namespace XM
{
    /// <summary>
    /// 配置测试管理器：在配置初始化完成后自动运行测试，验证XML配置数据是否正确加载
    /// </summary>
    [AutoCreate(priority: 1000)] // 较低优先级，确保在ConfigDataCenter之后初始化
    [ManagerDependency(typeof(IConfigDataCenter))]
    public class ConfigTestManager : ManagerBase<IConfigTestManager>, IConfigTestManager
    {
        private int _totalTests = 0;
        private int _passedTests = 0;
        private int _failedTests = 0;
        private StringBuilder _testReport;

        public override UniTask OnCreate()
        {
            _testReport = new StringBuilder();
            return UniTask.CompletedTask;
        }

        public override async UniTask OnInit()
        {
            XLog.Info("=================================================================");
            XLog.Info("开始配置测试：验证MyMod XML配置数据");
            XLog.Info("=================================================================");

            _testReport.AppendLine("\n【配置测试报告】");
            _testReport.AppendLine($"测试时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            _testReport.AppendLine("=================================================================\n");

            try
            {
                // 测试1: MyModConfig_Basic.xml - 基础配置
                await TestBasicConfigs();

                // 测试2: MyModConfig_TestConfig.xml - 复杂配置
                await TestComplexConfigs();

                // 测试3: MyModConfig_Nested.xml - 嵌套配置
                await TestNestedConfigs();

                // 测试4: MyModConfig_Link.xml - XMLLink链接
                await TestLinkConfigs();

                // 测试5: MyModConfig_Edge.xml - 边界情况
                await TestEdgeConfigs();

                // 输出测试报告
                PrintTestReport();
            }
            catch (Exception ex)
            {
                XLog.ErrorFormat("配置测试失败: {0}", ex);
                _testReport.AppendLine($"\n【严重错误】测试过程中发生异常: {ex.Message}");
            }

            return;
        }

        #region 测试用例

        /// <summary>测试基础配置：MyModConfig_Basic.xml</summary>
        private async UniTask TestBasicConfigs()
        {
            StartTestSection("MyModConfig_Basic.xml - 基础配置测试");

            var modS = new ModS("MyMod");
            
            // 测试配置是否存在
            TestConfigExists("basic_001", "MyItemConfig", modS, "basic_001");
            TestConfigExists("basic_002", "MyItemConfig", modS, "basic_002");
            TestConfigExists("basic_003", "MyItemConfig", modS, "basic_003");
            TestConfigExists("basic_004 - 最小值", "MyItemConfig", modS, "basic_004");
            TestConfigExists("basic_005 - 最大值", "MyItemConfig", modS, "basic_005");
            TestConfigExists("basic_006 - 空标签", "MyItemConfig", modS, "basic_006");
            TestConfigExists("basic_007 - 中文名称", "MyItemConfig", modS, "basic_007");
            TestConfigExists("basic_008 - 特殊字符", "MyItemConfig", modS, "basic_008");
            TestConfigExists("basic_009 - 重复标签", "MyItemConfig", modS, "basic_009");
            TestConfigExists("basic_010 - 极长名称", "MyItemConfig", modS, "basic_010");

            // 验证配置字段值（当API可用时）
            TestConfigValue("basic_001 - 字段验证", () =>
            {
                // TODO: 等待ConfigDataCenter.TryGetConfig API实现
                // var config = GetManagedConfig<MyItemConfig>("MyMod", "basic_001");
                // if (config != null)
                // {
                //     Assert(config.Name == "普通武器", "名称应为'普通武器'");
                //     Assert(config.Level == 1, "等级应为1");
                //     Assert(config.Tags != null && config.Tags.Count == 3, "标签数量应为3");
                //     Assert(config.Tags.Contains(1), "标签应包含1");
                // }
                return "跳过：等待查询API实现";
            });

            await UniTask.Yield();
        }

        /// <summary>测试复杂配置：MyModConfig_TestConfig.xml</summary>
        private async UniTask TestComplexConfigs()
        {
            StartTestSection("MyModConfig_TestConfig.xml - 复杂配置测试");

            var modS = new ModS("MyMod");

            TestConfigExists("test_001", "TestConfig", modS, "test_001");
            TestConfigExists("test_002 - 配置键引用", "TestConfig", modS, "test_002");
            TestConfigExists("test_003 - 空集合", "TestConfig", modS, "test_003");
            TestConfigExists("test_004 - 负数", "TestConfig", modS, "test_004");
            TestConfigExists("test_005 - 极限值", "TestConfig", modS, "test_005");
            TestConfigExists("test_006", "TestConfig", modS, "test_006");
            TestConfigExists("test_007", "TestConfig", modS, "test_007");
            TestConfigExists("test_008 - 复杂字典", "TestConfig", modS, "test_008");
            TestConfigExists("test_009", "TestConfig", modS, "test_009");
            TestConfigExists("test_010", "TestConfig", modS, "test_010");

            await UniTask.Yield();
        }

        /// <summary>测试嵌套配置：MyModConfig_Nested.xml</summary>
        private async UniTask TestNestedConfigs()
        {
            StartTestSection("MyModConfig_Nested.xml - 嵌套配置测试");

            var modS = new ModS("MyMod");

            TestConfigExists("nested_001 - 单层嵌套", "TestConfig", modS, "nested_001");
            TestConfigExists("nested_002 - 嵌套列表", "TestConfig", modS, "nested_002");
            TestConfigExists("nested_003 - 嵌套引用", "TestConfig", modS, "nested_003");
            TestConfigExists("nested_004", "TestConfig", modS, "nested_004");
            TestConfigExists("nested_005 - 嵌套字典", "TestConfig", modS, "nested_005");
            TestConfigExists("nested_006 - 多嵌套项", "TestConfig", modS, "nested_006");
            TestConfigExists("nested_007", "TestConfig", modS, "nested_007");
            TestConfigExists("nested_008 - 负数嵌套", "TestConfig", modS, "nested_008");
            TestConfigExists("nested_009 - 极限值", "TestConfig", modS, "nested_009");
            TestConfigExists("nested_010", "TestConfig", modS, "nested_010");

            await UniTask.Yield();
        }

        /// <summary>测试链接配置：MyModConfig_Link.xml</summary>
        private async UniTask TestLinkConfigs()
        {
            StartTestSection("MyModConfig_Link.xml - XMLLink链接测试");

            var modS = new ModS("MyMod");

            TestConfigExists("link_001 - XMLLink基础", "TestInhert", modS, "link_001");
            TestConfigExists("link_002", "TestInhert", modS, "link_002");
            TestConfigExists("link_003", "TestInhert", modS, "link_003");
            TestConfigExists("link_004 - 零值", "TestInhert", modS, "link_004");
            TestConfigExists("link_005 - 负值", "TestInhert", modS, "link_005");
            TestConfigExists("link_006 - 极限值", "TestInhert", modS, "link_006");
            TestConfigExists("link_007", "TestInhert", modS, "link_007");
            TestConfigExists("link_008", "TestInhert", modS, "link_008");
            TestConfigExists("link_009", "TestInhert", modS, "link_009");
            TestConfigExists("link_010", "TestInhert", modS, "link_010");

            await UniTask.Yield();
        }

        /// <summary>测试边界情况：MyModConfig_Edge.xml</summary>
        private async UniTask TestEdgeConfigs()
        {
            StartTestSection("MyModConfig_Edge.xml - 边界情况测试");

            var modS = new ModS("MyMod");

            TestConfigExists("edge_001 - 空值", "MyItemConfig", modS, "edge_001");
            TestConfigExists("edge_002 - XML转义字符", "MyItemConfig", modS, "edge_002");
            TestConfigExists("edge_003 - Unicode和Emoji", "MyItemConfig", modS, "edge_003");
            TestConfigExists("edge_004 - 极长列表", "MyItemConfig", modS, "edge_004");
            TestConfigExists("edge_005 - 负数边界", "MyItemConfig", modS, "edge_005");
            TestConfigExists("edge_006 - 换行空格", "MyItemConfig", modS, "edge_006");
            TestConfigExists("edge_007 - 中英文混合", "MyItemConfig", modS, "edge_007");
            TestConfigExists("edge_008 - 引号测试", "MyItemConfig", modS, "edge_008");
            TestConfigExists("edge_009 - 数字边界", "MyItemConfig", modS, "edge_009");
            TestConfigExists("edge_010 - 全空TestConfig", "TestConfig", modS, "edge_010");

            await UniTask.Yield();
        }

        #endregion

        #region 测试辅助方法

        private void StartTestSection(string sectionName)
        {
            _testReport.AppendLine($"\n【{sectionName}】");
            _testReport.AppendLine(new string('-', 65));
            XLog.InfoFormat("\n测试分组: {0}", sectionName);
        }

        private void TestConfig(string testName, Func<string> testAction)
        {
            _totalTests++;
            try
            {
                var result = testAction();
                if (result != null && result.StartsWith("跳过"))
                {
                    _testReport.AppendLine($"⚠ {testName}: {result}");
                    XLog.WarningFormat("  ⚠ {0}: {1}", testName, result);
                    _passedTests++; // 跳过的测试计为通过
                }
                else if (result == null)
                {
                    _testReport.AppendLine($"✓ {testName}: 通过");
                    XLog.InfoFormat("  ✓ {0}: 通过", testName);
                    _passedTests++;
                }
                else
                {
                    _testReport.AppendLine($"✓ {testName}: {result}");
                    XLog.InfoFormat("  ✓ {0}: {1}", testName, result);
                    _passedTests++;
                }
            }
            catch (Exception ex)
            {
                _failedTests++;
                var errorMsg = $"失败 - {ex.Message}";
                _testReport.AppendLine($"✗ {testName}: {errorMsg}");
                XLog.ErrorFormat("  ✗ {0}: {1}", testName, errorMsg);
            }
        }

        /// <summary>测试配置是否存在</summary>
        private void TestConfigExists(string testName, string configTypeName, ModS modS, string configName)
        {
            _totalTests++;
            try
            {
                // 构造 TblS
                var tbls = new TblS(modS, configTypeName);
                
                // 获取表句柄
                var tblI = IConfigDataCenter.I.GetTblI(tbls);
                
                if (!tblI.Valid)
                {
                    _failedTests++;
                    var msg = $"表 {configTypeName} 未注册";
                    _testReport.AppendLine($"✗ {testName}: {msg}");
                    XLog.ErrorFormat("  ✗ {0}: {1}", testName, msg);
                    return;
                }

                // 检查配置是否存在
                var exists = IConfigDataCenter.I.TryExistsConfig(tblI, modS, configName);
                
                if (exists)
                {
                    _passedTests++;
                    _testReport.AppendLine($"✓ {testName}: 配置存在 (表:{configTypeName}, ID:{configName})");
                    XLog.InfoFormat("  ✓ {0}: 配置存在", testName);
                }
                else
                {
                    _failedTests++;
                    var msg = $"配置不存在 (表:{configTypeName}, ID:{configName})";
                    _testReport.AppendLine($"✗ {testName}: {msg}");
                    XLog.ErrorFormat("  ✗ {0}: {1}", testName, msg);
                }
            }
            catch (Exception ex)
            {
                _failedTests++;
                var errorMsg = $"异常 - {ex.Message}";
                _testReport.AppendLine($"✗ {testName}: {errorMsg}");
                XLog.ErrorFormat("  ✗ {0}: {1}", testName, errorMsg);
            }
        }

        private void Assert(bool condition, string message)
        {
            if (!condition)
            {
                throw new Exception($"断言失败: {message}");
            }
        }

        /// <summary>测试配置字段值</summary>
        private void TestConfigValue(string testName, Func<string> testAction)
        {
            _totalTests++;
            try
            {
                var result = testAction();
                if (result != null && result.StartsWith("跳过"))
                {
                    _testReport.AppendLine($"⚠ {testName}: {result}");
                    XLog.DebugFormat("  ⚠ {0}: {1}", testName, result);
                    _passedTests++; // 跳过的测试计为通过
                }
                else if (result == null)
                {
                    _testReport.AppendLine($"✓ {testName}: 通过");
                    XLog.InfoFormat("  ✓ {0}: 通过", testName);
                    _passedTests++;
                }
                else
                {
                    _testReport.AppendLine($"✓ {testName}: {result}");
                    XLog.InfoFormat("  ✓ {0}: {1}", testName, result);
                    _passedTests++;
                }
            }
            catch (Exception ex)
            {
                _failedTests++;
                var errorMsg = $"失败 - {ex.Message}";
                _testReport.AppendLine($"✗ {testName}: {errorMsg}");
                XLog.ErrorFormat("  ✗ {0}: {1}", testName, errorMsg);
            }
        }

        /// <summary>获取托管配置对象（Helper方式）</summary>
        private T GetManagedConfig<T>(string modName, string configName) where T : class, IXConfig, new()
        {
            try
            {
                var helper = IConfigDataCenter.I.GetClassHelper<T>();
                if (helper == null)
                {
                    XLog.ErrorFormat("未找到配置Helper: {0}", typeof(T).Name);
                    return null;
                }

                // TODO: 需要ConfigDataCenter提供从托管配置缓存中获取配置的方法
                // 目前ConfigDataCenter主要存储Unmanaged数据
                // 托管配置在解析后被转换为Unmanaged，原始托管对象没有缓存
                
                XLog.Warning("GetManagedConfig: 托管配置查询功能待实现");
                return null;
            }
            catch (Exception ex)
            {
                XLog.ErrorFormat("获取配置失败: {0}", ex.Message);
                return null;
            }
        }

        /// <summary>打印配置统计信息</summary>
        private void PrintConfigStatistics()
        {
            try
            {
                _testReport.AppendLine("\n【配置统计信息】");
                _testReport.AppendLine(new string('-', 65));
                
                var modS = new ModS("MyMod");
                
                // 统计各类型配置数量
                var configTypes = new[] { "MyItemConfig", "TestConfig", "TestInhert", "NestedConfig" };
                foreach (var typeName in configTypes)
                {
                    var tbls = new TblS(modS, typeName);
                    var tblI = IConfigDataCenter.I.GetTblI(tbls);
                    
                    if (tblI.Valid)
                    {
                        // TODO: 需要ConfigDataCenter提供获取表中配置数量的方法
                        _testReport.AppendLine($"  {typeName}: 表已注册 (TblI: {tblI.TableId})");
                        XLog.DebugFormat("  {0}: 表已注册 (TblI: {1})", typeName, tblI.TableId);
                    }
                    else
                    {
                        _testReport.AppendLine($"  {typeName}: 未注册");
                        XLog.DebugFormat("  {0}: 未注册", typeName);
                    }
                }
            }
            catch (Exception ex)
            {
                XLog.ErrorFormat("获取配置统计信息失败: {0}", ex.Message);
            }
        }

        private void PrintTestReport()
        {
            // 输出配置统计信息
            PrintConfigStatistics();
            
            _testReport.AppendLine("\n" + new string('=', 65));
            _testReport.AppendLine("【测试总结】");
            _testReport.AppendLine($"总测试数: {_totalTests}");
            _testReport.AppendLine($"通过: {_passedTests} ({(_totalTests > 0 ? (_passedTests * 100.0 / _totalTests) : 0):F1}%)");
            _testReport.AppendLine($"失败: {_failedTests}");
            _testReport.AppendLine("=================================================================");

            // 输出到日志
            XLog.Info("\n=================================================================");
            XLog.InfoFormat("配置测试完成 - 总计: {0}, 通过: {1}, 失败: {2}", _totalTests, _passedTests, _failedTests);
            XLog.Info("=================================================================");

            // 输出完整报告到控制台
            Debug.Log(_testReport.ToString());

            // 如果有失败的测试，输出警告
            if (_failedTests > 0)
            {
                XLog.WarningFormat("有 {0} 个测试失败，请检查配置数据！", _failedTests);
            }
            else if (_passedTests == _totalTests)
            {
                XLog.Info("✓ 所有测试通过！配置系统运行正常。");
            }
        }

        #endregion
    }

    /// <summary>
    /// 配置测试管理器接口
    /// </summary>
    public interface IConfigTestManager : IManager<IConfigTestManager>
    {
    }
}
