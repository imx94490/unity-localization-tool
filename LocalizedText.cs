using UnityEngine;
using UnityEngine.UI;
using Framework.Managers;

namespace UI.Localization
{
    /// <summary>
    /// 绑定到 <see cref="UnityEngine.UI.Text"/> 的本地化辅助组件。
    /// </summary>
    /// <remarks>
    /// - 在 <c>OnEnable</c> 时调用 <c>UpdateText</c> 并订阅 <c>LocalizationManager.OnLanguageChanged</c>。
    /// - 当语言切换时自动刷新文本。
    /// - 支持使用 <c>arg0/arg1/arg2</c> 作为 <c>string.Format</c> 的格式化参数。
    /// 用法：将此组件挂到包含 Text 的节点，并设置 <c>key</c> 为字典键。
    /// </remarks>
    public sealed class LocalizedText : MonoBehaviour
    {
        /// <summary>
        /// 本地化字典键（例如：<c>TipReady</c>、<c>PlayerNameFallback</c>）。
        /// </summary>
        public string key;
        /// <summary>
        /// 可选格式化参数 0（对应字典值中的 <c>{0}</c>）。
        /// </summary>
        public string arg0;
        /// <summary>
        /// 可选格式化参数 1（对应字典值中的 <c>{1}</c>）。
        /// </summary>
        public string arg1;
        /// <summary>
        /// 可选格式化参数 2（对应字典值中的 <c>{2}</c>）。
        /// </summary>
        public string arg2;

        private Text _text;

        /// <summary>
        /// 缓存绑定的 <see cref="UnityEngine.UI.Text"/>。
        /// </summary>
        private void Awake()
        {
            _text = GetComponent<Text>();
        }

        /// <summary>
        /// 启用时订阅语言切换事件并立即刷新文本。
        /// </summary>
        private void OnEnable()
        {
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;
            }
            UpdateText();
        }

        /// <summary>
        /// 禁用时取消订阅语言切换事件。
        /// </summary>
        private void OnDisable()
        {
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChanged;
            }
        }

        /// <summary>
        /// 语言切换时回调，刷新文本。
        /// </summary>
        private void OnLanguageChanged(string lang)
        {
            UpdateText();
        }

        /// <summary>
        /// 根据键与可选参数更新文本内容。
        /// </summary>
        public void UpdateText()
        {
            if (_text == null || string.IsNullOrEmpty(key)) return;
            var has0 = !string.IsNullOrEmpty(arg0);
            var has1 = !string.IsNullOrEmpty(arg1);
            var has2 = !string.IsNullOrEmpty(arg2);
            if (!has0 && !has1 && !has2)
            {
                _text.text = LocalizationManager.G(key);
            }
            else
            {
                var args = new System.Collections.Generic.List<object>();
                if (has0) args.Add(arg0);
                if (has1) args.Add(arg1);
                if (has2) args.Add(arg2);
                _text.text = LocalizationManager.F(key, args.ToArray());
            }
        }
    }
}
