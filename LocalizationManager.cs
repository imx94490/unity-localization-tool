using System;
using System.Collections.Generic;
using UnityEngine;
using MiniJSON;

namespace Framework.Managers
{
    /// <summary>
    /// 本地化管理器（单例）。从 Resources/Localization/&lt;语言&gt;.json 加载键值文案并提供查询与格式化。
    /// </summary>
    /// <remarks>
    /// JSON 文件结构：{"Key":"Value",...}；值为字符串。支持 string.Format 占位符（例如 "玩家{0}"）。
    /// 切换语言后会广播 OnLanguageChanged 事件，已绑定的 UI 可据此刷新。
    /// </remarks>
    public sealed class LocalizationManager : MonoBehaviour
    {
        private static LocalizationManager _instance;
        public static LocalizationManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("LocalizationManager");
                    _instance = go.AddComponent<LocalizationManager>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        /// <summary>
        /// 当前语言代码（例如 zh-CN、en-US）。
        /// </summary>
        public string CurrentLanguage { get; private set; } = "zh-CN";
        private readonly Dictionary<string, string> _dict = new Dictionary<string, string>();
        /// <summary>
        /// 语言切换事件。参数为语言代码。
        /// </summary>
        public event Action<string> OnLanguageChanged;

        private void Awake()
        {
            if (_dict.Count == 0) LoadLanguage(CurrentLanguage);
        }

        /// <summary>
        /// 初始化默认语言（依据 Application.systemLanguage 推断）。
        /// </summary>
        /// <remarks>
        /// 简体/繁体中文推断为 zh-CN；英文推断为 en-US。可在程序入口调用一次。
        /// </remarks>
        public void InitializeDefaultLanguage()
        {
            var sys = Application.systemLanguage;
            var lang = "zh-CN";
            if (sys == SystemLanguage.English) lang = "en-US";
            else if (sys == SystemLanguage.Chinese || sys == SystemLanguage.ChineseSimplified || sys == SystemLanguage.ChineseTraditional) lang = "zh-CN";
            SetLanguage(lang);
        }

        /// <summary>
        /// 设置语言并加载对应 JSON 词典。成功后触发 OnLanguageChanged。
        /// </summary>
        /// <param name="lang">语言代码（如 zh-CN）。</param>
        /// <returns>成功返回 true；若加载失败会回退到 zh-CN。</returns>
        public bool SetLanguage(string lang)
        {
            if (string.IsNullOrEmpty(lang)) return false;
            if (lang == CurrentLanguage && _dict.Count > 0) return true;
            if (!LoadLanguage(lang))
            {
                if (!LoadLanguage("zh-CN")) return false;
                CurrentLanguage = "zh-CN";
                OnLanguageChanged?.Invoke(CurrentLanguage);
                return true;
            }
            CurrentLanguage = lang;
            OnLanguageChanged?.Invoke(CurrentLanguage);
            return true;
        }

        private bool LoadLanguage(string lang)
        {
            var ta = Resources.Load<TextAsset>($"Localization/{lang}");
            if (ta == null) return false;
            var obj = Json.Deserialize(ta.text) as Dictionary<string, object>;
            if (obj == null) return false;
            _dict.Clear();
            foreach (var kv in obj)
            {
                _dict[kv.Key] = kv.Value != null ? kv.Value.ToString() : string.Empty;
            }
            return true;
        }

        /// <summary>
        /// 尝试获取键对应的文案。
        /// </summary>
        /// <param name="key">键名。</param>
        /// <param name="value">输出的文案字符串。</param>
        /// <returns>存在返回 true；否则返回 false。</returns>
        public bool TryGet(string key, out string value)
        {
            value = null;
            if (string.IsNullOrEmpty(key)) return false;
            return _dict.TryGetValue(key, out value);
        }

        /// <summary>
        /// 获取键对应的文案；若不存在则返回键名本身（便于发现缺失）。
        /// </summary>
        /// <param name="key">键名。</param>
        /// <returns>文案字符串或键名。</returns>
        public string Get(string key)
        {
            if (string.IsNullOrEmpty(key)) return string.Empty;
            if (_dict.TryGetValue(key, out var v)) return v;
            return key;
        }

        /// <summary>
        /// 获取键对应的文案；若不存在则返回给定默认值。
        /// </summary>
        /// <param name="key">键名。</param>
        /// <param name="defaultValue">默认字符串。</param>
        /// <returns>文案字符串或默认值。</returns>
        public string GetOrDefault(string key, string defaultValue)
        {
            if (_dict.TryGetValue(key, out var v)) return v;
            return defaultValue;
        }

        /// <summary>
        /// 使用键的格式字符串执行 string.Format。
        /// </summary>
        /// <param name="key">格式键（例如 PlayerNameFallback）。</param>
        /// <param name="args">格式化参数。</param>
        /// <returns>格式化后的字符串。</returns>
        public string Format(string key, params object[] args)
        {
            var fmt = Get(key);
            return string.Format(fmt, args);
        }

        /// <summary>
        /// 使用键的格式字符串执行格式化；若缺失则使用默认格式。
        /// </summary>
        /// <param name="key">格式键。</param>
        /// <param name="defaultFormat">默认格式（例如 "玩家{0}"）。</param>
        /// <param name="args">格式化参数。</param>
        /// <returns>格式化后的字符串。</returns>
        public string FormatOrDefault(string key, string defaultFormat, params object[] args)
        {
            string fmt;
            if (_dict.TryGetValue(key, out var v)) fmt = v;
            else fmt = defaultFormat;
            return string.Format(fmt, args);
        }

        /// <summary>
        /// 静态便捷：设置语言。
        /// </summary>
        public static bool Set(string lang) => Instance.SetLanguage(lang);
        /// <summary>
        /// 静态便捷：获取文案（缺失返回键名）。
        /// </summary>
        public static string G(string key) => Instance.Get(key);
        /// <summary>
        /// 静态便捷：获取文案（缺失返回默认值）。
        /// </summary>
        public static string GD(string key, string def) => Instance.GetOrDefault(key, def);
        /// <summary>
        /// 静态便捷：格式化字符串（使用键对应的格式）。
        /// </summary>
        public static string F(string key, params object[] args) => Instance.Format(key, args);
        /// <summary>
        /// 静态便捷：格式化字符串（键缺失时使用默认格式）。
        /// </summary>
        public static string FD(string key, string def, params object[] args) => Instance.FormatOrDefault(key, def, args);
    }
}
