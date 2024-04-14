---
sidebar_position: 1
---

# 分組動骨

*自 2.4.0*

此元件從指定位置搜尋動骨（包括 **DynamicBone 和 VRCPhysBone 元件**）, 然後將它們分組為到一個遊戲物件裡面, 並為它們設定動畫。

它是從衣櫃系統中提取的元件，用作其他用途。

![Group Dynamics](/img/comp-group-dyn.PNG)

## 動畫

如果您想要為動骨元件設定開啟和關閉動畫，您可以 **直接使用 Unity 動畫編輯器** 用啟用元件的複選框設定動畫。 元件將自動轉換動畫為多個動骨元件。

![Unity Anim](/img/comp-group-dyn-unity-anim.PNG)

您也可以使用 **SmartControl** 直接為該元件設定開關。 請參閱 [開關元件](/docs/getting-started/smart-control/binary/toggling-a-component) 並將元件類型設定為 `DTGroupDynamics`。

![SmartControl](/img/comp-group-dyn-smartcontrol.PNG)

:::caution
對持有動骨元件的遊戲物件（而不是元件本身）進行動畫處理可能仍會保持動骨在背景運行，這對於效能來說不太好。 建議啟用 `分開成不同的 GameObject` 以動骨分開成不同的遊戲物件中來分別為它們設定動畫。
:::

## 設定

### 動骨搜尋模式
#### 控制的根部

搜尋控制指定根的動骨。 **(你通常想要這個)**

#### 元件的根部

搜尋指定根內部的動骨組件。

### 包含和排除物件

在搜尋中包含和排除指定的物件。

### 分開成不同的 GameObject

:::caution
除非您了解自己在做什麼，否則建議您啟用此功能。
:::

將找到的動骨元件分離到目前遊戲物件下的不同的遊戲物件中。 **如果您想單獨為元件設定動畫，則需要啟用此選項。 否則將無法正確設定動畫。**

從:

![From](/img/comp-group-dyn-separate-obj-1.PNG)

變成:

![To](/img/comp-group-dyn-separate-obj-2.PNG)

### 設定為目前元件的狀態

啟用將所有找到的動骨元件設定為此元件的目前元件啟用狀態。 這允許您集中開啟或關閉動骨元件。

![Set state](/img/comp-group-dyn-set-state.PNG)
