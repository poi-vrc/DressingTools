---
sidebar_position: 1
---

# 參數槽

*自 2.5.0*

參數槽元件作為多個 SmartControl 元件之間的共用標籤。
這些 SmartControl 元件將被指派特定值，並由單一 `int` 或 `float` 動畫參數控制。
只有與參數值相符的 SmartControl 才會啟用。

類似於 Unity 的 Any State，但內部並沒有使用它。

![Group Dynamics](/img/comp-parameter-slot.png)

## 映射

您可以在參數槽元件介面或 SmartControl 介面中直接編輯映射。 
您必須將其中一個 SmartControl 對應為預設值，否則它可能無法正常運作。

## 設定

- **參數名稱**: `int`/`float` 參數的名稱。 如果留空則會自動產生。
- **值類型**: `Int` 或 `Float` 值類型
- **預設值**: 預設值
- **網路同步**: 此參數是否需要網路同步
- **會儲存**: 不同世界間是否保存此參數
