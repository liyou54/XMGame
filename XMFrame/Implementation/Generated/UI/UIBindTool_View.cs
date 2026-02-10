using UnityEngine;
using XM.UIEX;

namespace XM
{
    /// <summary>
    /// TestUIView 视图层，由 UIBindTool 自动生成
    /// </summary>
    public class TestUIView
    {
        private Transform _root;

        public ToggleEx tgl_ { get; private set; }

        public void Bind(Transform root)
        {
            _root = root;
            tgl_ = root.Find("tgl_")?.GetComponent<ToggleEx>();
        }
    }
}
