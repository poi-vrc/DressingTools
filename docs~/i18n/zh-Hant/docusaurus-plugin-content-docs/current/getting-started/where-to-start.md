---
sidebar_position: 2
---

# 從哪裡開始？

目前有兩個系統可以自動建立動畫和選單來切換不同的物品和服裝，針對不同的用例。

**它們目前不可互換, 將來會更新以更好地整合在一起。**

## 1. 智能控制 (Smart Control)

*自 2.3.0*

[![Smart Control](/img/where-to-start-smartcontrol.PNG)](/img/where-to-start-smartcontrol.PNG)

:::caution

智能控制**不會**設定您的服裝！ 您必須使用[快速設定衣服](/docs/getting-started/cabinet/quick-setup-clothes) **和不啟用衣櫃動畫**，或使用其他工具等來設定骨架合併
:::

用於建立動畫以開啟和關閉物件或元件，或變更混合形狀、元件、Shader屬性等的通用元件。

由 Animator 參數、選單項目以及將來添加的更多方法來驅動。

<a
    className="button button--success button--lg"
    target="_self"
    href="/docs/category/smart-control">
    開始設定吧！
</a>

### 用例
- 建立角色物件或元件的控制
- 加大衣櫃菜單的定制性
- 不使用衣櫃系統, 製作自己的衣櫃選單

## 2. 衣櫃系統

[![Cabinet](/img/where-to-start-cabinet.PNG)](/img/where-to-start-cabinet.PNG)

:::info
將來會更新以更好地和智能控制系統整合在一起。
:::

專用於設定和管理穿戴式物件, 提供了一個快速設定服裝及動畫的視窗。
它現在內部在使用新的智能控制系統。

當衣櫃動畫模組啟用時，衣櫃系統將其視為一套服裝並為其生成智能控制。
當未切換其服裝時，角色動骨會自動分組並停用。

<a
    className="button button--success button--lg"
    target="_self"
    href="/docs/category/cabinet">
    開始設定吧！
</a>

### 用例

- 想盡快穿上多套新衣服
