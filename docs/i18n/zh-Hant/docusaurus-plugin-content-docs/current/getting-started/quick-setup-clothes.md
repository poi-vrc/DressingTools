---
sidebar_position: 3
---

# 快速設定衣服

使用 DressingTools 來裝扮你的角色又快又簡潔。

使用 DressingTools ** 單擊即可穿著簡單的衣服，並手動為它們設定動畫 **，儘管 DressingTools 的大多數用例應該使用衣櫃系統自動生成選單和動畫。

:::info 這不會自動產生衣櫃動畫和選單。您必須使用[另一種方法](setup-clothes-with-cabinet-anim)進行設定。您也可以在衣櫃視窗中`編輯`穿戴物品，然後按`自動設定`以達到相同的效果。 :::

:::info 提示 您可以為穿戴物品內的物件像平常一樣製作動畫！它們在套用期間無縫自動重新映射。 :::

## 指南

:::info 本指南假定您已根據[設定衣櫃指南](setup-cabinet)為您的角色建立了衣櫃。 :::

在入門系列中，我們將把[Eliya Workshop的Summer Streat](https://booth.pm/ja/items/4666271)裝扮進[Kyubi Closet的Moe](https://kyubihome.booth.pm/items/4667400)裡面。

### 1.將衣服拖到角色上

將衣服拖曳到角色裡面，右鍵選擇`DressingTools -&gt; Auto-setup wearable (Mappings Only)`

[![設定穿戴物品](/img/setup-simple-2-setup-wearable.PNG)](/img/setup-simple-2-setup-wearable.PNG)

### 2. 完成！

進入運行模式，然後您將能夠測試穿戴物品是否正確地對應到骨頭。

[![設定完成](/img/setup-simple-3-done.PNG)](/img/setup-simple-3-done.PNG)

## 背後發生的事情

### 使用“Auto-setup wearable (Mappings Only)”

當您按一下 `Auto-setup wearable (Mappings Only)` 時，DressingTools 會執行自動設定來尋找穿戴物品骨架，並嘗試空運行並為其產生骨頭映射。

它將使用產生的配置在穿戴物品根上建立一個 `DT Cabinet Wearable` 元件。

### 進入運行模式

進入運行模式時，隨附的 `DT Cabinet` 將在Unity 進入運行模式之前套用包含的穿戴物品。
