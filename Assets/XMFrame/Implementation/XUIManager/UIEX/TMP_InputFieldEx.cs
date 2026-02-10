using TMPro;

namespace XM.UIEX
{
    /// <summary>
    /// 扩展 TMP_InputField 组件，支持 Bind/BindName 用于 UI 绑定
    /// </summary>
    public class TMP_InputFieldEx : TMP_InputField, IUIEx
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
