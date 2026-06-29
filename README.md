# Unity Localization Tool

Unity 多语言本地化框架，支持简体中文（zh-CN）、英文（en-US）、繁体中文（zh-TW）。

## 目录结构

```
LocalizationUnity/
├── LocalizationManager.cs    # 核心管理器（单例）
├── LocalizedText.cs          # 文本本地化组件
├── LocalizedImage.cs         # 图片本地化组件
└── LocalizationWeb/          # JSON 生成工具
    ├── app.js
    └── index.html
```

## 核心组件

### LocalizationManager
单例管理器，负责加载语言文件和提供文案查询。

**主要 API：**
- `SetLanguage(lang)` - 切换语言
- `Get(key)` / `G(key)` - 获取文案
- `Format(key, args)` / `F(key, args)` - 格式化字符串
- `OnLanguageChanged` 事件 - 语言切换回调

**语言文件位置：** `Resources/Localization/<语言>.json`

### LocalizedText
文本本地化组件，挂到 Text 节点并设置 key 值即可自动切换语言。

### LocalizedImage
图片本地化组件，支持为不同语言配置不同的 Sprite 资源。

## 使用方式

1. 将 `.cs` 文件放入 Unity 项目的 `Assets/Scripts/` 目录
2. 语言文件放入 `Resources/Localization/` 目录
3. UI 组件挂载 `LocalizedText` 或 `LocalizedImage` 并配置 key
4. 程序入口调用 `LocalizationManager.Instance.InitializeDefaultLanguage()`

## 语言文件格式

```json
{
  "Key1": "Value1",
  "Key2": "Value2 - {0}"
}
```

## Web 工具

使用 `LocalizationWeb/` 中的工具从 Excel 表生成 JSON 语言文件。
