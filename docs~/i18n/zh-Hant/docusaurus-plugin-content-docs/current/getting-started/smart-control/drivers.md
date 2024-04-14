---
sidebar_position: 3
---

# 驅動

驅動是指你希望這個智能控制由甚麽原理去推動。

:::caution
🚧 這部分文檔尚未完成。
:::

## Animator 參數作為驅動

這是一個通用的驅動，允許使用自訂 Animator 參數來控制此 SmartControl。

如果 Animator 參數名稱留空，它將在建置時自動產生。

## 選單項作為驅動

此驅動將會新增或替換同一遊戲物件中現有的 `DT Menu Item` 元件。
此選單項目將使用參數直接控制 SmartControl。

![Menu Item Driver](/img/smartcontrol-basics-driver-menuitem.PNG)

如果 Animator 參數名稱留空，它將在建置時自動產生。

目前，它支援以下選單項目：

|             | Button | Toggle | Radial | Two-axis | Four-axis | Sub-menu |
|-------------|--------|--------|--------|----------|-----------|----------|
| Binary      | ✅     | ✅    | ❌     | ❌      | ❌        | ❌      | 
| Motion Time | ❌     | ❌    | ✅     | ❌      | ❌        | ❌      | 

✅: 支援, ❌: 不支援

## VRCPhysBone 驅動

*自 2.4.0*

此驅動使 SmartControl 由 VRC PhysBone 元件控制，輕鬆建立與其他玩家互動的動畫。

![VRCPhysBone Driver](/img/smartcontrol-basics-driver-vrcphysbone.PNG)

如果 Animator 參數名稱留空，它將在建置時自動產生。

有關 PhysBone 互動功能的更多詳細信息，請參閱 [VRChat官方文件](https://creators.vrchat.com/avatars/avatar-dynamics/physbones/)。

:::caution
您必須至少選擇一種條件或一種來源。 否則，SmartControl 將被忽略並且不會產生動畫。
:::

對於不同的條件和來源組合，它們會產生不同的控制類型的動畫：

|         | 沒有        | 抓住時     | 擺好姿勢時       | 抓住或擺好姿勢時 |
|---------|-------------|-------------|-------------|------------------|
| 沒有    | ❌          | Binary      | Binary      | Binary           |
| 角度    | Motion Time | Motion Time | Motion Time | Motion Time      |
| 拉緊    | Motion Time | Motion Time | Motion Time | Motion Time      |
| 壓扁    | Motion Time | Motion Time | Motion Time | Motion Time      |

❌: 將被忽略並且不會產生動畫

例如，如果您希望 `Ahoge` 頭髮總是受到壓扁的數值影響，而不只是抓住或擺出姿勢時，
您需要將 **條件** 設為 **沒有**，將 **來源** 設為 **壓扁**。

如果您希望它僅在抓取或擺姿勢時受到拉緊的數值影響，您需要將 **條件** 設定為 **抓住或擺好姿勢時**，將 **來源** 設定為 **拉緊**。
