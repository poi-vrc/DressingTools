---
sidebar_position: 6
---

# 設定飾品或其他

本部分對於設定不包含 **骨架** 的各種穿戴物品非常有用。 例如眼鏡、頭髮等。

對於預製件創作者，或希望將穿戴物品根移動到特定位置，以進行更好管理的用戶來說也很有用。

## 指南

:::info
本指南假定您已根據[設定衣櫃指南](setup-cabinet)為您的角色建立了衣櫃。
:::

在本指南中，我們將創建一個立方體並將其綁定到 [Kyubi Closet的Moe](https://kyubihome.booth.pm/items/4667400) 的右手，同時**仍將立方體保持在角色根**。

### 1. 將飾品拖入角色並調整位置

這裡我們使用一個立方體來示範流程，現在我們的角色中有一個立方體並調整了位置，同時保持在根部。

[![Put into place](/img/setup-moveroot-1-put-into-place.PNG)](/img/setup-moveroot-1-put-into-place.PNG)

### 2. 將飾品設定為穿戴物品

右鍵單擊飾品根，然後單擊 `DressingTools -> Setup wearable in editor`

[![Menu](/img/setup-moveroot-2-menu.PNG)](/img/setup-moveroot-2-menu.PNG)

### 3. 映射

目前有兩種方法可以將你的飾品映射到你的角色上:

#### 3.1 用移動穿戴物品根模組

對於**不包含骨架**的飾品，您可以使用它來映射到您的頭像，這就是本指南中我們的立方體的情況。

此模組與用手直接將物體移動到目標位置沒有分別。

:::tip
您可能需要根據需要停用 `移動穿戴物品根` 以外的模組。

例如，您可能必須停用 `衣櫃動畫`，因為您不會將此穿戴物品視為要換的衣服。
:::

在移動穿戴物品根編輯器中，選擇要將穿戴物品移動到的位置。

[![Move root editor](/img/setup-moveroot-3-moverooteditor.png)](/img/setup-moveroot-3-moverooteditor.png)

#### 3.2 用映射編輯器

對於**包含骨架**的飾品，您可以嘗試使用映射編輯器根據[本指南](guides/mapping-editor.md)設定手動映射。

### 4. 完成！

將配置加入衣櫃中，然後運行模式並查看結果！ 您的穿戴物品已移動到目標位置並綁定在一起！

[![Done](/img/setup-moveroot-4-done.PNG)](/img/setup-moveroot-4-done.PNG)
