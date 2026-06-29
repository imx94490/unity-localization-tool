using System;
using Framework.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Localization
{
    /// <summary>
    /// 中文注释：用于多语言图片的本地化组件。
    /// 将本组件挂到带有 Image 的对象上，分别配置中文/英文（可选繁中）Sprite，即可随语言切换自动替换 Image.sprite。
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Image))]
    public sealed class LocalizedImage : MonoBehaviour
    {
        /// <summary>
        /// 中文注释：简体中文图片（zh-CN / zh-* 默认走这里）。
        /// </summary>
        [SerializeField] private Sprite spriteZhCN;

        /// <summary>
        /// 中文注释：英文图片（en-US / en-* 默认走这里）。
        /// </summary>
        [SerializeField] private Sprite spriteEnUS;

        /// <summary>
        /// 中文注释：繁体中文图片（可选；若不配置，会按回退逻辑使用简中或英文）。
        /// </summary>
        [SerializeField] private Sprite spriteZhTW;

        /// <summary>
        /// 中文注释：切换 Sprite 后是否调用 SetNativeSize（适用于按钮图集尺寸不同的情况）。
        /// </summary>
        [SerializeField] private bool setNativeSize;

        private Image _image;

        private void Awake()
        {
            _image = GetComponent<Image>();
        }

        private void OnEnable()
        {
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.OnLanguageChanged += OnLanguageChanged;
            }
            Apply(LocalizationManager.Instance != null ? LocalizationManager.Instance.CurrentLanguage : null);
        }

        private void OnDisable()
        {
            if (LocalizationManager.Instance != null)
            {
                LocalizationManager.Instance.OnLanguageChanged -= OnLanguageChanged;
            }
        }

        private void OnLanguageChanged(string lang)
        {
            Apply(lang);
        }

        private void Apply(string lang)
        {
            if (_image == null) return;

            Sprite target;
            if (string.IsNullOrEmpty(lang))
            {
                target = spriteZhCN != null ? spriteZhCN : (spriteEnUS != null ? spriteEnUS : spriteZhTW);
            }
            else if (lang.StartsWith("zh-TW", StringComparison.OrdinalIgnoreCase))
            {
                target = spriteZhTW != null ? spriteZhTW : (spriteZhCN != null ? spriteZhCN : spriteEnUS);
            }
            else if (lang.StartsWith("zh", StringComparison.OrdinalIgnoreCase))
            {
                target = spriteZhCN != null ? spriteZhCN : (spriteZhTW != null ? spriteZhTW : spriteEnUS);
            }
            else if (lang.StartsWith("en", StringComparison.OrdinalIgnoreCase))
            {
                target = spriteEnUS != null ? spriteEnUS : (spriteZhCN != null ? spriteZhCN : spriteZhTW);
            }
            else
            {
                target = spriteEnUS != null ? spriteEnUS : (spriteZhCN != null ? spriteZhCN : spriteZhTW);
            }

            if (target == null) return;
            if (_image.sprite == target) return;

            _image.sprite = target;
            if (setNativeSize) _image.SetNativeSize();
        }
    }
}
