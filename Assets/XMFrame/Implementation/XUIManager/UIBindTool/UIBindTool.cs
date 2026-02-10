using System;
using System.Collections.Generic;
using UnityEngine;
using XM.UIEX;

namespace XM
{
    /// <summary>
    /// UI 绑定工具：负责绑定 UICtrl 及视图层代码生成。
    /// 递归收集子节点下 bind=true 且非 UICtrl 的节点。
    /// </summary>
    public class UIBindTool : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("关联的 UICtrl，一般为挂载于同一 GameObject 或父节点的 UICtrlBase")]
        private UICtrlBase _uiCtrl;

        [SerializeField]
        [Tooltip("收集到的绑定项（由编辑器收集，用于代码生成）")]
        private List<UIBindEntry> _bindEntries = new List<UIBindEntry>();

        /// <summary>关联的 UICtrl</summary>
        public UICtrlBase UICtrl
        {
            get => _uiCtrl;
            set => _uiCtrl = value;
        }

        /// <summary>收集到的绑定项</summary>
        public List<UIBindEntry> BindEntries => _bindEntries;

        /// <summary>
        /// 设置绑定项（供编辑器调用）
        /// </summary>
        public void SetBindEntries(List<UIBindEntry> entries)
        {
            _bindEntries.Clear();
            if (entries != null)
                _bindEntries.AddRange(entries);
        }
    }

    /// <summary>
    /// 单条 UI 绑定项
    /// </summary>
    [Serializable]
    public class UIBindEntry
    {
        [Tooltip("相对路径，用于 transform.Find()")]
        public string Path;

        [Tooltip("绑定名称，用于生成字段名")]
        public string BindName;

        [Tooltip("组件类型名，如 ImageEx、TextEx、ButtonEx")]
        public string ComponentType;

        [Tooltip("GameObject 名称")]
        public string GameObjectName;

        public UIBindEntry() { }

        public UIBindEntry(string path, string bindName, string componentType, string goName)
        {
            Path = path ?? string.Empty;
            BindName = bindName ?? string.Empty;
            ComponentType = componentType ?? string.Empty;
            GameObjectName = goName ?? string.Empty;
        }
    }
}
