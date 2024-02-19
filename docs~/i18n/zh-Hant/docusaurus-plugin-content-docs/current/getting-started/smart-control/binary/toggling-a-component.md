---
sidebar_position: 2
---

# 開關元件

## 指南

在本指南中，我們將在選單中製作開關以開啟和關閉角色的 VRCPhysBone 元件。

### 0. 建立選單組和智能控制項

本指南要您先準備好現有的菜單組。 依照[本指南](/docs/getting-started/smart-control/menu-basics)建立新的選單組。

您可以直接從 DT Menu Group 元件建立一個，或在該選單組建立新的 GameObject 的子並新增 DT Smart Control 元件。

![Add SmartControl](/img/smartcontrol-create-0.PNG)

### 1. 設定選單項目驅動

確保驅動是 `選單項目` 並且類型是 `開關`。 根據您的喜好自訂名稱和圖示。

![Setup Driver](/img/smartcontrol-toggle-go-1.PNG)

### 2. 將控制類型設為 二進位 並將外部物件拖曳至新增框子

下拉框用於要開關的元件（這對於下一部分很有用）。 現在的話，我們希望它開關 VRCPhysBone 而不是整個 GameObject。

物件欄位旁的複選框用於指定此控制處於啟用狀態時 GameObject 的狀態。 現在的話，這表示該物件將被關閉。

![Toggle Component](/img/smartcontrol-toggle-comp-2.PNG)

### 3. 完成！

您剛剛成功為元件創建了一個開關！
