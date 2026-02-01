using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Xml;
using XM.Contracts;
using XM.Contracts.Config;
using XM;

namespace XM.Editor.Tests.Fakes
{
    /// <summary>
    /// Fake ConfigClassHelper，可配置返回值，记录调用
    /// 特点：
    /// 1. 轻量级实现，无需真实的序列化逻辑
    /// 2. 记录所有方法调用，用于验证
    /// 3. 支持配置自定义行为
    /// </summary>
    public class FakeConfigClassHelper : ConfigClassHelper
    {
        // 可配置的返回值
        public TblS TblSToReturn { get; set; }
        public IXConfig ConfigToReturn { get; set; }
        public Func<XmlElement, IXConfig> ParseXmlFunc { get; set; }
        
        // 调用记录（用于验证）
        public List<string> MethodCalls { get; } = new List<string>();
        public Dictionary<string, int> CallCounts { get; } = new Dictionary<string, int>();
        
        // 记录调用的辅助方法
        private void RecordCall(string methodName)
        {
            MethodCalls.Add(methodName);
            
            if (!CallCounts.ContainsKey(methodName))
                CallCounts[methodName] = 0;
            CallCounts[methodName]++;
        }
        
        public override TblS GetTblS()
        {
            RecordCall(nameof(GetTblS));
            return TblSToReturn;
        }
        
        public override IXConfig Create()
        {
            RecordCall(nameof(Create));
            return ConfigToReturn ?? new FakeXConfig();
        }
        
        public override void SetTblIDefinedInMod(TblI tbl)
        {
            RecordCall($"{nameof(SetTblIDefinedInMod)}({tbl})");
        }
        
        public override IXConfig DeserializeConfigFromXml(
            XmlElement configItem, 
            ModS mod, 
            string configName, 
            in ConfigParseContext context)
        {
            RecordCall($"{nameof(DeserializeConfigFromXml)}({configName})");
            
            // 如果配置了自定义解析函数，使用它
            if (ParseXmlFunc != null)
                return ParseXmlFunc(configItem);
            
            return ConfigToReturn ?? new FakeXConfig();
        }
        
        public override void ParseAndFillFromXml(
            IXConfig target, 
            XmlElement configItem, 
            ModS mod, 
            string configName,
            in ConfigParseContext context)
        {
            RecordCall($"{nameof(ParseAndFillFromXml)}({configName})");
            // Fake实现：不做实际解析，仅记录调用
        }
        
        public override void AllocUnManagedAndInitHeadVal(
            TblI table, 
            ConcurrentDictionary<CfgS, IXConfig> kvValue, 
            object configHolder)
        {
            RecordCall($"{nameof(AllocUnManagedAndInitHeadVal)}(table={table})");
        }
        
        public override Type GetLinkHelperType()
        {
            RecordCall(nameof(GetLinkHelperType));
            return null; // Fake实现：返回null
        }
        
        public override void FillBasicData(
            TblI tblI, 
            ConcurrentDictionary<CfgS, IXConfig> kvValue, 
            object configHolder)
        {
            RecordCall($"{nameof(FillBasicData)}(table={tblI})");
        }
        
        public override void AllocContainerWithoutFill(
            TblI tblI, 
            TblS tblS, 
            ConcurrentDictionary<CfgS, IXConfig> kvValue,
            ConcurrentDictionary<TblS, ConcurrentDictionary<CfgS, IXConfig>> allData,
            object configHolder)
        {
            RecordCall($"{nameof(AllocContainerWithoutFill)}(table={tblI})");
        }
        
        /// <summary>
        /// 清除调用记录
        /// </summary>
        public void ClearCallHistory()
        {
            MethodCalls.Clear();
            CallCounts.Clear();
        }
        
        /// <summary>
        /// 验证方法是否被调用
        /// </summary>
        public bool WasCalled(string methodName)
        {
            return CallCounts.ContainsKey(methodName) && CallCounts[methodName] > 0;
        }
        
        /// <summary>
        /// 获取方法调用次数
        /// </summary>
        public int GetCallCount(string methodName)
        {
            return CallCounts.TryGetValue(methodName, out var count) ? count : 0;
        }
    }
    
    /// <summary>
    /// Fake IXConfig实现（用于测试）
    /// </summary>
    public class FakeXConfig : IXConfig
    {
        public CfgI Data { get; set; }
    }
}
