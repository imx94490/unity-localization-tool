# Unity Localization Tool

Unity 多语言本地化框架，支持文本和图片的运行时切换。

## 目录结构

```
LocalizationUnity/
├── LocalizationManager.cs    # 核心管理器（单例）
├── LocalizedText.cs           # 文本本地化组件
├── LocalizedImage.cs          # 图片本地化组件
└── LocalizationWeb/           # JSON生成工具
    ├── app.js
    └── index.html
```

## 核心组件

### LocalizationManager
核心管理器，负责加载语言文件和提供文案查询接口。

- 单例模式，场景切换时不销毁
- 从 `Resources/Localization/<语言>.json` 加载词典
- 支持语言：zh-CN（简体中文）、en-US（英文）、zh-TW（繁体中文）
- 语言切换后触发 `OnLanguageChanged` 事件

**主要API：**
```csharp
// 切换语言
LocalizationManager.SetLanguage("zh-CN");

// 获取文案
string text = LocalizationManager.G("KeyName");

// 格式化字符串
string formatted = LocalizationManager.F("PlayerName", playerName);
```

### LocalizedText
文本本地化组件，绑定到 Unity UI Text。

- 挂到 Text 节点后设置 key 值即可
- 语言切换时自动刷新文本
- 支持最多3个格式化参数（arg0/arg1/arg2）

### LocalizedImage
图片本地化组件，绑定到 Unity UI Image。

- 挂到 Image 节点后配置各语言 Sprite
- 语言切换时自动切换对应图片

## 使用方式

1. 将 `LocalizationManager.cs`、`LocalizedText.cs`、`LocalizedImage.cs` 放入项目
2. 在 `Resources/Localization/` 目录下创建语言 JSON 文件（如 `zh-CN.json`）
3. 程序入口调用 `LocalizationManager.Instance.InitializeDefaultLanguage()`
4. UI 组件挂上对应脚本并配置 key 值

## 语言文件格式

```json
{
  "KeyName": "对应文案",
  "PlayerName": "玩家 {0}"
}
```

## 依赖

- Unity 2017.1+
- MiniJSON（用于 JSON 解析，已在 LocalizationManager.cs 中使用）
