using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Xml;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using XM;
using XM.Contracts.Config;

namespace XM.Editor
{
    /// <summary>
    /// ConfigItemProcessor 单元测试（无继承、无锁）
    /// </summary>
    [TestFixture]
    public class ConfigItemProcessorTests
    {
        #region ParseOverrideMode

        [Test] public void ParseOverrideMode_NullOrEmpty_ReturnsNone()
        {
            Assert.AreEqual(OverrideMode.None, ConfigItemProcessor.ParseOverrideMode(null));
            Assert.AreEqual(OverrideMode.None, ConfigItemProcessor.ParseOverrideMode(""));
        }

        [Test] public void ParseOverrideMode_RewriteOrAdd_ReturnsReWrite()
        {
            Assert.AreEqual(OverrideMode.ReWrite, ConfigItemProcessor.ParseOverrideMode("rewrite"));
            Assert.AreEqual(OverrideMode.ReWrite, ConfigItemProcessor.ParseOverrideMode("REWRITE"));
            Assert.AreEqual(OverrideMode.ReWrite, ConfigItemProcessor.ParseOverrideMode("add"));
        }

        [Test] public void ParseOverrideMode_DeleteOrDel_ReturnsDelete()
        {
            Assert.AreEqual(OverrideMode.Delete, ConfigItemProcessor.ParseOverrideMode("delete"));
            Assert.AreEqual(OverrideMode.Delete, ConfigItemProcessor.ParseOverrideMode("del"));
        }

        [Test] public void ParseOverrideMode_Modify_ReturnsModify()
        {
            Assert.AreEqual(OverrideMode.Modify, ConfigItemProcessor.ParseOverrideMode("modify"));
        }

        [Test] public void ParseOverrideMode_Unknown_ReturnsNone()
        {
            Assert.AreEqual(OverrideMode.None, ConfigItemProcessor.ParseOverrideMode("unknown"));
            Assert.AreEqual(OverrideMode.None, ConfigItemProcessor.ParseOverrideMode("   "));
        }

        #endregion

        #region Process 提前退出

        [Test] public void Process_ClsEmpty_Skips()
        {
            var (_, item) = CreateItem("", "cfg1", null);
            var (adds, deletes, modifies) = Process(item, "MyMod", ctx: new MockContext());
            AssertEmpty(adds, deletes, modifies);
        }

        [Test] public void Process_IdEmpty_Skips()
        {
            var (_, item) = CreateItem("MyMod::C", "", null);
            var (adds, deletes, modifies) = Process(item, "MyMod", ctx: new MockContext());
            AssertEmpty(adds, deletes, modifies);
        }

        [Test] public void Process_IdThreeSegments_Skips()
        {
            var (_, item) = CreateItem("MyMod::C", "A::B::C", null);
            var (adds, deletes, modifies) = Process(item, "MyMod", ctx: new MockContext());
            AssertEmpty(adds, deletes, modifies);
        }

        [Test] public void Process_IdModDifferentAndNoOverride_Skips()
        {
            var (_, item) = CreateItem("OtherMod::C", "OtherMod::cfg1", null);
            var (adds, deletes, modifies) = Process(item, "MyMod", ctx: new MockContext());
            AssertEmpty(adds, deletes, modifies);
        }

        [Test] public void Process_ResolveHelperNull_Skips()
        {
            var (_, item) = CreateItem("MyMod::C", "cfg1", null);
            var ctx = new MockContext { ResolveHelper = (_, __) => null };
            var (adds, deletes, modifies) = Process(item, "MyMod", ctx);
            AssertEmpty(adds, deletes, modifies);
        }

        #endregion

        #region Process Add (None / ReWrite)

        [Test] public void Process_Add_FirstAdd_AddsToPendingAdds()
        {
            var tbls = new TblS("MyMod", "Table");
            var parsed = new MockIXConfig();
            var ctx = CreateAddContext(tbls, parsed);
            var (_, item) = CreateItem("MyMod::C", "cfg1", null);

            var (adds, _, _) = Process(item, "MyMod", ctx);

            AssertAddContains(adds, tbls, "MyMod", "cfg1", parsed);
        }

        [Test] public void Process_Add_IdSingleSegment_UsesFileModAsIdMod()
        {
            var tbls = new TblS("MyMod", "T");
            var ctx = CreateAddContext(tbls, new MockIXConfig());
            var (_, item) = CreateItem("MyMod::C", "cfg1", null);

            var (adds, _, _) = Process(item, "MyMod", ctx);

            var key = new CfgS(new ModS("MyMod"), tbls, "cfg1");
            Assert.IsTrue(adds[tbls].ContainsKey(key));
        }

        [Test] public void Process_Add_IdTwoSegments_UsesIdMod()
        {
            var tbls = new TblS("OtherMod", "T");
            var ctx = CreateAddContext(tbls, new MockIXConfig());
            var (_, item) = CreateItem("OtherMod::C", "OtherMod::cfg1", "rewrite");

            var (adds, _, _) = Process(item, "MyMod", ctx);

            var key = new CfgS(new ModS("OtherMod"), tbls, "cfg1");
            Assert.IsTrue(adds[tbls].ContainsKey(key));
        }

        /// <summary>解析结果为 null 时不写入 pendingAdds；GetOrAdd 可能已创建表桶，但桶内不应有该配置。</summary>
        [Test] public void Process_Add_ParsedConfigNull_DoesNotAdd()
        {
            var tbls = new TblS("MyMod", "T");
            var ctx = CreateAddContext(tbls, null);
            var (_, item) = CreateItem("MyMod::C", "cfg1", null);

            var (adds, _, _) = Process(item, "MyMod", ctx);

            Assert.AreEqual(0, adds.TryGetValue(tbls, out var inner) ? inner.Count : 0, "解析为 null 时不应向该表加入任何配置");
        }

        [Test] public void Process_Add_OverrideAdd_SameAsReWrite()
        {
            var tbls = new TblS("MyMod", "T");
            var parsed = new MockIXConfig();
            var ctx = CreateAddContext(tbls, parsed);
            var (_, item) = CreateItem("MyMod::C", "cfg1", "add");

            var (adds, _, _) = Process(item, "MyMod", ctx);

            AssertAddContains(adds, tbls, "MyMod", "cfg1", parsed);
        }

        [Test] public void Process_Add_MultipleConfigsSameTable_AllAdded()
        {
            var tbls = new TblS("MyMod", "T");
            var parsed1 = new MockIXConfig();
            var parsed2 = new MockIXConfig();
            var helper = new MockConfigClassHelper(tbls);
            var ctx = new MockContext { ResolveHelper = (_, __) => helper, HasTable = _ => true };
            var (_, item1) = CreateItem("MyMod::C", "cfg1", null);
            var (_, item2) = CreateItem("MyMod::C", "cfg2", null);

            helper.ParsedConfig = parsed1;
            var (adds, _, _) = Process(item1, "MyMod", ctx);
            helper.ParsedConfig = parsed2;
            Process(item2, "MyMod", ctx, adds);

            Assert.AreEqual(2, adds[tbls].Count);
            Assert.AreSame(parsed1, adds[tbls][new CfgS(new ModS("MyMod"), tbls, "cfg1")]);
            Assert.AreSame(parsed2, adds[tbls][new CfgS(new ModS("MyMod"), tbls, "cfg2")]);
        }

        [Test] public void Process_Add_DifferentTables_BothAdded()
        {
            var tbls1 = new TblS("MyMod", "Table1");
            var tbls2 = new TblS("MyMod", "Table2");
            var parsed1 = new MockIXConfig();
            var parsed2 = new MockIXConfig();
            var helper1 = new MockConfigClassHelper(tbls1) { ParsedConfig = parsed1 };
            var helper2 = new MockConfigClassHelper(tbls2) { ParsedConfig = parsed2 };
            var ctx = new MockContext
            {
                ResolveHelper = (cls, _) => cls.Contains("Table1") ? helper1 : helper2,
                HasTable = _ => true
            };
            var (_, item1) = CreateItem("MyMod::Table1Config", "cfg1", null);
            var (_, item2) = CreateItem("MyMod::Table2Config", "cfg1", null);

            var (adds, _, _) = Process(item1, "MyMod", ctx);
            Process(item2, "MyMod", ctx, adds);

            Assert.AreEqual(2, adds.Count);
            Assert.AreSame(parsed1, adds[tbls1][new CfgS(new ModS("MyMod"), tbls1, "cfg1")]);
            Assert.AreSame(parsed2, adds[tbls2][new CfgS(new ModS("MyMod"), tbls2, "cfg1")]);
        }

        #endregion

        #region Process Modify

        [Test] public void Process_Modify_HasTableFalse_Skips()
        {
            var tbls = new TblS("MyMod", "T");
            var ctx = new MockContext { ResolveHelper = (_, __) => new MockConfigClassHelper(tbls), HasTable = _ => false };
            var (_, item) = CreateItem("MyMod::C", "cfg1", "modify");

            var (_, _, modifies) = Process(item, "MyMod", ctx);

            Assert.AreEqual(0, modifies.Count);
        }

        [Test] public void Process_Modify_HasTableTrue_AddsToPendingModifies()
        {
            var tbls = new TblS("MyMod", "T");
            var helper = new MockConfigClassHelper(tbls);
            var ctx = new MockContext { ResolveHelper = (_, __) => helper, HasTable = _ => true };
            var (_, item) = CreateItem("MyMod::C", "cfg1", "modify");

            var (_, _, modifies) = Process(item, "MyMod", ctx);

            var key = new CfgS(new ModS("MyMod"), tbls, "cfg1");
            Assert.AreEqual(1, modifies.Count);
            Assert.IsTrue(modifies.ContainsKey(key));
            Assert.AreSame(item, modifies[key]);
        }

        #endregion

        #region Process Delete

        [Test] public void Process_Delete_AddsToPendingDeletes()
        {
            var tbls = new TblS("MyMod", "T");
            var ctx = new MockContext { ResolveHelper = (_, __) => new MockConfigClassHelper(tbls) };
            var (_, item) = CreateItem("MyMod::C", "cfg1", "delete");

            var (_, deletes, _) = Process(item, "MyMod", ctx);

            var key = new CfgS(new ModS("MyMod"), tbls, "cfg1");
            Assert.AreEqual(1, deletes.Count);
            Assert.IsTrue(deletes.ContainsKey(key));
        }

        [Test] public void Process_Delete_IdWithModPrefix_UsesIdModInKey()
        {
            var tbls = new TblS("OtherMod", "T");
            var ctx = new MockContext { ResolveHelper = (_, __) => new MockConfigClassHelper(tbls) };
            var (_, item) = CreateItem("OtherMod::C", "OtherMod::cfg2", "del");

            var (_, deletes, _) = Process(item, "MyMod", ctx);

            var key = new CfgS(new ModS("OtherMod"), tbls, "cfg2");
            Assert.IsTrue(deletes.ContainsKey(key));
        }

        #endregion

        #region Process Conflict (CfgS 含 Mod，不同 Mod 为不同 key；同 key 重复时按 Mod 顺序/重复报错)

        /// <summary>CfgS 含 Mod，不同 Mod 的同一 configName 视为不同 key；两 Mod 均会加入，高优先级 Mod 的配置可被后续 merge 逻辑处理。</summary>
        [Test] public void Process_Conflict_NewModIndexHigher_ReplacesExisting()
        {
            var tbls = new TblS("ModA", "Table");
            var parsedA = new MockIXConfig();
            var helperA = new MockConfigClassHelper(tbls) { ParsedConfig = parsedA };
            var parsedB = new MockIXConfig();
            var helperB = new MockConfigClassHelper(tbls) { ParsedConfig = parsedB };
            var ctx = new MockContext
            {
                ResolveHelper = (_, mod) => mod == "ModA" ? helperA : helperB,
                GetModSortIndex = name => name == "ModA" ? 0 : 1
            };
            var (_, itemA) = CreateItem("ModA::C", "same", null);
            var (_, itemB) = CreateItem("ModB::C", "ModB::same", null);

            var (adds, _, _) = Process(itemA, "ModA", ctx);
            Assert.IsTrue(adds[tbls].ContainsKey(new CfgS(new ModS("ModA"), tbls, "same")));

            Process(itemB, "ModB", ctx, adds);

            Assert.IsTrue(adds[tbls].ContainsKey(new CfgS(new ModS("ModA"), tbls, "same")), "ModA 的 key 仍存在");
            Assert.IsTrue(adds[tbls].TryGetValue(new CfgS(new ModS("ModB"), tbls, "same"), out var final), "ModB 的 key 已加入");
            Assert.AreSame(parsedB, final);
        }

        /// <summary>CfgS 含 Mod，不同 Mod 的同一 configName 视为不同 key；两 Mod 均会加入，低优先级 Mod 的配置也会在 pendingAdds 中。</summary>
        [Test] public void Process_Conflict_NewModIndexLower_SkipsSecond()
        {
            var tbls = new TblS("ModA", "Table");
            var parsedA = new MockIXConfig();
            var parsedB = new MockIXConfig();
            var helperA = new MockConfigClassHelper(tbls) { ParsedConfig = parsedA };
            var helperB = new MockConfigClassHelper(tbls) { ParsedConfig = parsedB };
            var ctx = new MockContext
            {
                ResolveHelper = (_, mod) => mod == "ModA" ? helperA : helperB,
                GetModSortIndex = name => name == "ModA" ? 1 : 0
            };
            var (_, itemA) = CreateItem("ModA::C", "same", null);
            var (_, itemB) = CreateItem("ModB::C", "ModB::same", null);

            var (adds, _, _) = Process(itemA, "ModA", ctx);
            Process(itemB, "ModB", ctx, adds);

            Assert.IsTrue(adds[tbls].ContainsKey(new CfgS(new ModS("ModA"), tbls, "same")));
            Assert.IsTrue(adds[tbls].ContainsKey(new CfgS(new ModS("ModB"), tbls, "same")), "CfgS 含 Mod，ModB 的 key 会单独加入");
            Assert.AreSame(parsedA, adds[tbls][new CfgS(new ModS("ModA"), tbls, "same")]);
            Assert.AreSame(parsedB, adds[tbls][new CfgS(new ModS("ModB"), tbls, "same")]);
        }

        [Test] public void Process_Conflict_SameModIndex_LogsErrorAndKeepsFirst()
        {
            LogAssert.Expect(LogType.Error, new Regex("pending add 重复"));

            var tbls = new TblS("MyMod", "Table");
            var parsed1 = new MockIXConfig();
            var ctx = new MockContext { ResolveHelper = (_, __) => new MockConfigClassHelper(tbls) { ParsedConfig = parsed1 }, GetModSortIndex = _ => 0 };
            var (_, item1) = CreateItem("MyMod::C", "dup", null);
            var (_, item2) = CreateItem("MyMod::C", "dup", null);

            var (adds, _, _) = Process(item1, "MyMod", ctx);
            Process(item2, "MyMod", ctx, adds);

            var key = new CfgS(new ModS("MyMod"), tbls, "dup");
            Assert.AreEqual(1, adds[tbls].Count);
            Assert.AreSame(parsed1, adds[tbls][key]);
        }

        #endregion

        #region Constructor

        [Test] public void Constructor_NullContext_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => new ConfigItemProcessor(null));
        }

        #endregion

        #region Helpers

        private static (XmlDocument doc, XmlElement item) CreateItem(string cls, string id, string overrideAttr)
        {
            var doc = new XmlDocument();
            var item = doc.CreateElement("ConfigItem");
            item.SetAttribute("cls", cls);
            item.SetAttribute("id", id);
            if (!string.IsNullOrEmpty(overrideAttr))
                item.SetAttribute("override", overrideAttr);
            return (doc, item);
        }

        private static MockContext CreateAddContext(TblS tbls, IXConfig parsed)
        {
            return new MockContext
            {
                ResolveHelper = (_, __) => new MockConfigClassHelper(tbls) { ParsedConfig = parsed },
                HasTable = _ => true
            };
        }

        private static (
            ConcurrentDictionary<TblS, ConcurrentDictionary<CfgS, IXConfig>> adds,
            ConcurrentDictionary<CfgS, byte> deletes,
            ConcurrentDictionary<CfgS, XmlElement> modifies) Process(
            XmlElement item, string modName, MockContext ctx,
            ConcurrentDictionary<TblS, ConcurrentDictionary<CfgS, IXConfig>> existingAdds = null)
        {
            var adds = existingAdds ?? new ConcurrentDictionary<TblS, ConcurrentDictionary<CfgS, IXConfig>>();
            var deletes = new ConcurrentDictionary<CfgS, byte>();
            var modifies = new ConcurrentDictionary<CfgS, XmlElement>();
            new ConfigItemProcessor(ctx).Process(item, "file.xml", modName, default, adds, deletes, modifies);
            return (adds, deletes, modifies);
        }


        private static void AssertEmpty(
            ConcurrentDictionary<TblS, ConcurrentDictionary<CfgS, IXConfig>> adds,
            ConcurrentDictionary<CfgS, byte> deletes,
            ConcurrentDictionary<CfgS, XmlElement> modifies)
        {
            Assert.AreEqual(0, adds.Count);
            Assert.AreEqual(0, deletes.Count);
            Assert.AreEqual(0, modifies.Count);
        }

        private static void AssertAddContains(
            ConcurrentDictionary<TblS, ConcurrentDictionary<CfgS, IXConfig>> adds,
            TblS tbls, string modName, string configName, IXConfig expected)
        {
            var key = new CfgS(new ModS(modName), tbls, configName);
            Assert.IsTrue(adds.TryGetValue(tbls, out var inner));
            Assert.IsTrue(inner.TryGetValue(key, out var cfg));
            Assert.AreSame(expected, cfg);
        }

        #endregion

        #region Mock types

        private sealed class MockContext : IConfigItemProcessorContext
        {
            public Func<TblS, bool> HasTable { get; set; } = _ => false;
            public Func<string, string, ConfigClassHelper> ResolveHelper { get; set; } = (_, __) => null;
            public Func<string, int> GetModSortIndex { get; set; } = _ => 0;
            public Action<ConcurrentDictionary<TblS, ConcurrentDictionary<CfgS, IXConfig>>, TblS, CfgS> RemovePendingAdd { get; set; } = (_, __, ___) => { };

            bool IConfigItemProcessorContext.HasTable(TblS tbls) => HasTable(tbls);
            ConfigClassHelper IConfigItemProcessorContext.ResolveHelper(string cls, string configInMod) => ResolveHelper(cls, configInMod);
            int IConfigItemProcessorContext.GetModSortIndex(string modName) => GetModSortIndex(modName);
            void IConfigItemProcessorContext.RemovePendingAdd(
                ConcurrentDictionary<TblS, ConcurrentDictionary<CfgS, IXConfig>> pendingAdds,
                TblS tbls, CfgS existingKey) => RemovePendingAdd(pendingAdds, tbls, existingKey);
        }

        private sealed class MockConfigClassHelper : ConfigClassHelper
        {
            private readonly TblS _tbls;
            public IXConfig ParsedConfig { get; set; }

            public MockConfigClassHelper(TblS tbls) => _tbls = tbls;

            public override TblS GetTblS() => _tbls;
            public override IXConfig Create() => null;
            public override void SetTblIDefinedInMod(TblI tbl) { }
            public override IXConfig DeserializeConfigFromXml(XmlElement configItem, ModS mod, string configName, in ConfigParseContext context) => ParsedConfig;

            public override void ParseAndFillFromXml(IXConfig target, XmlElement configItem, ModS mod, string configName,
                in ConfigParseContext context)
            {
                throw new NotImplementedException();
            }

            public override void AllocUnManagedAndInitHeadVal(TblI table, ConcurrentDictionary<CfgS, IXConfig> kvValue, object configHolder) { }
            public override Type GetLinkHelperType()
            {
                throw new NotImplementedException();
            }
        }

        private sealed class MockIXConfig : IXConfig
        {
            public CfgI Data { get; set; }
        }

        #endregion
    }
}
