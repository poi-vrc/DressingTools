---
sidebar_position: 3
---

# 改變屬性

## 指南

在本指南中，我們將在選單中製作開關，使 Moe 的「Big ear」混合形狀更改為 100。

### 0. 建立選單組和智能控制項

本指南要您先準備好現有的菜單組。 依照[本指南](/docs/getting-started/smart-control/menu-basics)建立新的選單組。

您可以直接從 DT Menu Group 元件建立一個，或在該選單組建立新的 GameObject 的子並新增 DT Smart Control 元件。

![Add SmartControl](/img/smartcontrol-create-0.PNG)

### 1. 設定選單項目驅動

確保驅動是 `選單項目` 並且類型是 `開關`。 根據您的喜好自訂名稱和圖示。

![Setup Driver](/img/smartcontrol-chg-prop-1.PNG)

### 2. 建立屬性組

![Create Property Group](/img/smartcontrol-chg-prop-2.PNG)

### 3. 將 Ear 物件拖到欄位中

確保選擇類型為 `正常`。 將 Ear 物件拖曳到 `包括這些物件`和 `從這選擇屬性` 的新增欄位。

![Pick Object](/img/smartcontrol-chg-prop-3.PNG)

:::info
有關屬性組的更多詳細，請[查看此處](/docs/getting-started/smart-control/property-groups)。
:::

### 4. 新增混合形狀屬性

找到 `SkinnedMeshRenderer` 元件並點擊 `混合形狀`。 找到 `Big ear` 混合形狀並添加它。

再次按一下 `混合形狀` 按鈕以隱藏結果。

![Pick Object](/img/smartcontrol-chg-prop-4.PNG)

### 5. 設定啟用時的值

當控制項啟用時，將其設為 `100` 或任何您想要的值。

![Pick Object](/img/smartcontrol-chg-prop-5.PNG)

### 6. 完成！

進入運行模式並測試結果！

![Result](/img/smartcontrol-chg-prop-done.gif)
