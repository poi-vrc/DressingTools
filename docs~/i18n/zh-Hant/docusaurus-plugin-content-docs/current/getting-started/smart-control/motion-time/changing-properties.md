---
sidebar_position: 3
---

# 使用徑向選單項更改屬性

在本指南中，我們將在選單中製作徑向選單項 (Radial)，使 Moe 的「Big ear」混合形狀從 0 更改為 100。

### 0. 建立選單組和智能控制項

本指南要您先準備好現有的菜單組。 依照[本指南](/docs/getting-started/smart-control/menu-basics)建立新的選單組。

您可以直接從 DT Menu Group 元件建立一個，或在該選單組建立新的 GameObject 的子並新增 DT Smart Control 元件。

![Add SmartControl](/img/smartcontrol-create-0.PNG)

### 1. 設定選單項目驅動

:::caution
目前 UI 有問題，因此您必須先將控制類型變更為 `動態動畫時間`，然後才能將選單項目變更為 `徑向`。
:::

先將控制類型變更為 `動態動畫時間`.

![Control Type](/img/smartcontrol-motiontime-1-ctrltype.PNG)

確保驅動是 `選單項目` 並且類型是 `徑向`。 根據您的喜好自訂名稱和圖示。

![Menu Item](/img/smartcontrol-motiontime-1-menuitem.PNG)

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

### 5. 映射混合形狀值

`從` 和 `到` 值會對應到徑向控制項的浮點參數（從 0.0 到 1.0）。

![Pick Object](/img/smartcontrol-motiontime-5.PNG)

### 6. 完成！

進入運行模式並測試結果！

![Result](/img/smartcontrol-motiontime-done.gif)
