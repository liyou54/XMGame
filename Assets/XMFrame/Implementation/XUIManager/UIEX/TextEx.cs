using UnityEngine.UI;

namespace XM.UIEX
{
    /// <summary>
    /// 扩展 Text 组件，支持 Bind/BindName 用于 UI 绑定
    /// </summary>
    public class TextEx : Text, IUIEx
    {
        [UnityEngine.SerializeField]
        private bool _bind;

        [UnityEngine.SerializeField]
        private string _bindName = string.Empty;

        public bool Bind
        {
            get => _bind;
            set => _bind = value;
        }

        public string BindName
        {
            get => _bindName;
            set => _bindName = value ?? string.Empty;
        }
    }
}
