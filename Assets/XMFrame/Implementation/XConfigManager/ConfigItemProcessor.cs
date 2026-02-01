using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;
using XM.Contracts.Config;
using XM.Utils;

namespace XM
{
    #region 接口

    /// <summary>
    /// 单条 ConfigItem 的并发处理上下文，用于依赖注入与单元测试。
    /// </summary>
    public interface IConfigItemProcessorContext
    {
        /// <summary>表是否已注册（TblS -> TblI 已存在）</summary>
        bool HasTable(TblS tbls);

        /// <summary>从 cls 字符串解析出对应 Helper（支持 "Mod::TypeName" 形式）</summary>
        ConfigClassHelper ResolveHelper(string cls, string configInMod);

        /// <summary>Mod 加载顺序索引，用于覆盖优先级</summary>
        int GetModSortIndex(string modName);

        /// <summary>从 pendingAdds 中移除指定表下的已有键（覆盖前清理）</summary>
        void RemovePendingAdd(
            ConcurrentDictionary<TblS, ConcurrentDictionary<CfgS, IXConfig>> pendingAdds,
            TblS tbls,
            CfgS existingKey);
    }

    #endregion

    #region ConfigItemProcessor

    /// <summary>
    /// 将单条 ConfigItem 解析并写入 pendingAdds / pendingDeletes / pendingModifies，可单独做单元测试。
    /// </summary>
    public sealed class ConfigItemProcessor
    {
        #region 私有字段

        private readonly IConfigItemProcessorContext _context;

        #endregion

        #region 构造

        /// <remarks>主要步骤：校验 context 非空并保存引用。</remarks>
        public ConfigItemProcessor(IConfigItemProcessorContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        #endregion

        #region 处理逻辑

        /// <summary>
        /// 处理单条 ConfigItem：解析 cls/id/override，按模式写入 pendingAdds / pendingDeletes / pendingModifies。
        /// </summary>
        /// <remarks>主要步骤：1. 取 cls/id/override 并校验必填；2. 解析 id 为 idModName + configName；3. 校验跨 Mod 时需 override；4. 解析 Helper 与 cfgKey；5. Modify 则写 pendingModifies；6. None/ReWrite 则处理重复与覆盖后解析并写 pendingAdds；7. Delete 则写 pendingDeletes。</remarks>
        /// <param name="configItem">XML ConfigItem 节点</param>
        /// <param name="xmlFilePath">XML 文件路径（用于日志）</param>
        /// <param name="modName">当前 Mod 名</param>
        /// <param name="modId">当前 Mod ID</param>
        /// <param name="pendingAdds">待添加：表 -> (CfgS -> IXConfig)</param>
        /// <param name="pendingDeletes">待删除：CfgS 占位</param>
        /// <param name="pendingModifies">待修改：CfgS -> XmlElement（后续 FillFromXml）</param>
        public void Process(
            XmlElement configItem,
            string xmlFilePath,
            string modName,
            ModI modId,
            ConcurrentDictionary<TblS, ConcurrentDictionary<CfgS, IXConfig>> pendingAdds,
            ConcurrentDictionary<CfgS, byte> pendingDeletes,
            ConcurrentDictionary<CfgS, XmlElement> pendingModifies)
        {
            // 1. 取 cls / id / override 属性
            var cls = configItem.GetAttribute("cls");
            var id = configItem.GetAttribute("id");
            var overrideAttr = configItem.GetAttribute("override");

            if (string.IsNullOrEmpty(cls) || string.IsNullOrEmpty(id))
            {
                XLog.DebugFormat("[Config] 跳过 ConfigItem cls 或 id 为空: cls={0}, id={1}, file={2}", cls ?? "", id ?? "",
                    xmlFilePath);
                return;
            }

            // 2. 解析 id：支持 "mod::name" 或 "name"（当前 Mod）
            var idParts = id.Split(new[] { "::" }, StringSplitOptions.None);
            string idModName;
            string configName;
            if (idParts.Length == 1)
            {
                idModName = modName;
                configName = idParts[0];
            }
            else if (idParts.Length == 2)
            {
                idModName = idParts[0];
                configName = idParts[1];
            }
            else
                return;

            // 3. 跨 Mod 引用且无 override 则跳过（不允许隐式覆盖）
            if (!string.Equals(idModName, modName, StringComparison.Ordinal) && string.IsNullOrEmpty(overrideAttr))
                return;

            var overrideMode = ParseOverrideMode(overrideAttr);
            var helper = _context.ResolveHelper(cls, modName);

            if (helper == null)
            {
                XLog.ErrorFormat("[Config] 未解析到 Helper: cls={0}, mod={1}, id={2}", cls, modName, configName);
                return;
            }

            // 4. 构造 CfgS 键
            var tbls = helper.GetTblS();
            var modKey = new ModS(idModName);
            var cfgKey = new CfgS(modKey, tbls, configName);

            // 5. Modify：仅记录 XmlElement，后续 FillFromXml
            if (overrideMode == OverrideMode.Modify)
            {
                if (!_context.HasTable(tbls))
                {
                    XLog.ErrorFormat("[Config] Modify 未找到表跳过: tbls={0}, key={1}", tbls, cfgKey);
                    return;
                }

                pendingModifies[cfgKey] = configItem;
                return;
            }

            // 6. None / ReWrite：新增或覆盖，先处理同键冲突（按 Mod 顺序），再解析并写入 pendingAdds
            if (overrideMode == OverrideMode.None || overrideMode == OverrideMode.ReWrite)
            {
                var adds = pendingAdds.GetOrAdd(tbls, static _ => new ConcurrentDictionary<CfgS, IXConfig>());
                if (adds.ContainsKey(cfgKey))
                {
                    var oriIndex = _context.GetModSortIndex(cfgKey.ConfigInMod.Name);
                    var newIndex = _context.GetModSortIndex(modName);
                    if (newIndex > oriIndex)
                    {
                        _context.RemovePendingAdd(pendingAdds, tbls, cfgKey);
                    }
                    else if (newIndex < oriIndex)
                    {
                        return;
                    }
                    else
                    {
                        XLog.Error("[Config] pending add 重复: 同一配置已被添加 {0}", cfgKey);
                        return;
                    }
                }

                var parseContext = new ConfigParseContext
                    { FilePath = xmlFilePath ?? "", Line = 0, Mode = overrideMode };
                var task = new ConfigParseTask(parseContext, configItem, helper, modKey, configName, overrideMode,
                    xmlFilePath ?? "");
                var result = task.Execute();
                if (!result.IsValid)
                    return;

                adds[cfgKey] = result.Config;
                XLog.DebugFormat("[Config] pending Add: {0}", cfgKey);
            }
            // 7. Delete：仅登记到 pendingDeletes
            else if (overrideMode == OverrideMode.Delete)
            {
                pendingDeletes[cfgKey] = 0;
                XLog.DebugFormat("[Config] pending Delete: {0}", cfgKey);
            }
        }

        #endregion

        #region Override 解析

        /// <summary>
        /// 解析 override 属性为 OverrideMode。
        /// </summary>
        /// <remarks>主要步骤：1. 空则返回 None；2. 转小写后匹配 rewrite/add、delete/del、modify；3. 未知则告警并返回 None。</remarks>
        /// <param name="overrideAttr">XML 中 override 属性值，可为空</param>
        /// <returns>None / ReWrite / Delete / Modify</returns>
        public static OverrideMode ParseOverrideMode(string overrideAttr)
        {
            if (string.IsNullOrEmpty(overrideAttr))
                return OverrideMode.None;

            switch (overrideAttr.ToLowerInvariant())
            {
                case "rewrite":
                case "add":
                    return OverrideMode.ReWrite;
                case "delete":
                case "del":
                    return OverrideMode.Delete;
                case "modify":
                    return OverrideMode.Modify;
                default:
                    Debug.LogWarning($"未知的 override 模式: {overrideAttr}，将视为 None");
                    return OverrideMode.None;
            }
        }

        #endregion
    }

    #endregion
}
