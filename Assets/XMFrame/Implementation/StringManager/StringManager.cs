using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using XM;
using XM.Contracts;
using XM.Contracts.Config;

namespace XM
{
    /// <summary>
    /// 字符串管理器：提供 StrI&lt;-&gt;string、LabelS&lt;-&gt;LabelI&lt;-&gt;string 的注册与查询。
    /// </summary>
    [AutoCreate]
    [ManagerDependency(typeof(IModManager))]
    public class StringManager : ManagerBase<IStringManager>, IStringManager
    {
        private readonly Dictionary<string, int> _strToId = new();
        private readonly Dictionary<int, string> _idToStr = new();
        private int _nextStrId = 1;

        private readonly Dictionary<(ModI mod, int labelId), LabelS> _labelIToS = new();
        private readonly Dictionary<(string modName, string labelName), LabelI> _labelSToI = new();
        private readonly Dictionary<ModI, int> _nextLabelIdPerMod = new();

        public override UniTask OnCreate() => UniTask.CompletedTask;
        public override UniTask OnInit() => UniTask.CompletedTask;

        #region StrI 与 string 互转

        public bool TryGetStrI(string value, out StrI strI)
        {
            strI = default;
            if (value == null)
                return true;
            if (string.IsNullOrEmpty(value))
            {
                strI = new StrI { Id = 0 };
                return true;
            }
            if (_strToId.TryGetValue(value, out var id))
            {
                strI = new StrI { Id = id };
                return true;
            }
            id = _nextStrId++;
            _strToId[value] = id;
            _idToStr[id] = value;
            strI = new StrI { Id = id };
            return true;
        }

        public bool TryGetStr(StrI strI, out string value)
        {
            value = null;
            if (strI.Id == 0)
                return true;
            return _idToStr.TryGetValue(strI.Id, out value);
        }

        #endregion

        #region LabelS 与 LabelI 互转

        public bool TryGetLabelI(LabelS labelS, out LabelI labelI)
        {
            labelI = default;
            if (string.IsNullOrEmpty(labelS.ModName) || string.IsNullOrEmpty(labelS.LabelName))
                return false;
            var key = (labelS.ModName, labelS.LabelName);
            if (_labelSToI.TryGetValue(key, out labelI))
                return true;
            var modI = IModManager.I.GetModId(labelS.ModName);
            if (!modI.Valid)
                return false;
            if (!_nextLabelIdPerMod.TryGetValue(modI, out var nextId))
                nextId = 1;
            var lid = nextId++;
            _nextLabelIdPerMod[modI] = nextId;
            labelI = new LabelI { DefinedModId = modI, labelId = lid };
            _labelSToI[key] = labelI;
            _labelIToS[(modI, lid)] = labelS;
            return true;
        }

        public bool TryGetLabelS(LabelI labelI, out LabelS labelS)
        {
            labelS = default;
            if (!labelI.DefinedModId.Valid)
                return false;
            return _labelIToS.TryGetValue((labelI.DefinedModId, labelI.labelId), out labelS);
        }

        public bool TryGetLabel(LabelI labelI, out string value)
        {
            value = null;
            if (!TryGetLabelS(labelI, out var labelS))
                return false;
            value = $"{labelS.ModName}::{labelS.LabelName}";
            return true;
        }

        #endregion
    }
}
