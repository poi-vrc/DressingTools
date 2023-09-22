---
sidebar_position: 4
---

# 設定衣服用衣櫃動畫

DressingTools 可以為您的角色穿多件衣服的時候更方便和輕鬆。

DressingTools 為您起草所有必要的穿戴物品動畫和同名 Blendshape 同步（如 Big breast）。您只需添加額外的角色開關/混合形狀並確認起草的內容就可以了！

**設定動畫時還可以即時預覽**！

## 指南

:::info 本指南假定您已根據[設定衣櫃指南](setup-cabinet)為您的角色建立了衣櫃。 :::

在入門系列中，我們將把[Eliya Workshop的Summer Streat](https://booth.pm/ja/items/4666271)裝扮進[Kyubi Closet的Moe](https://kyubihome.booth.pm/items/4667400)裡面。

### 1.將衣服拖到角色上

將衣服拖曳到角色上，右鍵選擇`DressingTools -&gt;Setup wearable with wizard`

[![設定穿戴物品](/img/setup-cabanim-1-setup-wearable.PNG)](/img/setup-cabanim-1-setup-wearable.PNG)

### 2. 工具視窗彈出

DressingTools 工具窗口會彈出，並運行自動設定來為您起草配置：

[![工具視窗彈出](/img/setup-cabanim-2-tool-window.PNG)](/img/setup-cabanim-2-tool-window.PNG)

### 3. 拍攝縮圖（可選）

縮圖可以裝飾您的菜單，讓您更容易分辨穿戴物品。按一下`拍攝新的縮圖`。移動場景視圖以選擇縮圖的適當位置，然後按 `拍照`。

[![拍攝縮圖](/img/setup-cabanim-3-thumbnail.PNG)](/img/setup-cabanim-3-thumbnail.PNG)

:::caution 如果您的場景視圖不隨相機移動，請嘗試開啟 `Gizmos` 並重試。

[![Gizmos](/img/setup-cabanim-3-gizmos.PNG)](/img/setup-cabanim-3-gizmos.PNG) :::

### 4. 確認映射

在 `映射` 中，確認骨架骨頭映射是否報告沒有錯誤。

[![映射](/img/setup-cabanim-4-mapping.PNG)](/img/setup-cabanim-4-mapping.PNG)

### 5. 衣櫃動畫

DressingTools 會自動起草穿戴物品開關（新增目前啟用的物件），因此您只需準備角色穿着時的開關和混合形狀。

編輯這些開關和混合形狀將自動開啟**即時預覽**，讓您立即預覽動畫！

:::tip 您可以使用預設。儲存並載入您自己的預設，以便下次穿得更快！ [![預設](/img/setup-cabanim-5-tip-presets.PNG)](/img/setup-cabanim-5-tip-presets.PNG) :::

#### 角色穿着時的開關

利用給予的`建議`並添加角色開關，直到你的衣服不再與現有的衣服重曡。物件欄位旁的複選框是物件是否啟用或不啟用的狀態。

您也可以使用 `+ 新增` 新增位於其他位置的開關，或使用 `x` 刪除開關。

[![開關](/img/setup-cabanim-5-avatar-onwear-toggles.PNG)](/img/setup-cabanim-5-avatar-onwear-toggles.PNG)

#### 角色穿着時的混合形狀

利用`建議`添加更改混合形狀，直到您的角色和衣服看起來正常。 （即更改胸部/腳/高跟鞋形狀鍵）

[![混合形狀](/img/setup-cabanim-5-avatar-onwear-blendshapes.PNG)](/img/setup-cabanim-5-avatar-onwear-blendshapes.PNG)

#### 確認穿戴物品穿着時的開關

確認自動產生的穿戴物品穿着時的開關是否符合您的偏好。只有當最初啟用並位於穿戴物品根時，它們才會自動添加。

[![穿戴物品穿着時](/img/setup-cabanim-5-wearable-onwear.PNG)](/img/setup-cabanim-5-wearable-onwear.PNG)

### 6. 確認混合形狀同步

:::caution DressingTools 可能會增加不正確的混合形狀同步。因此，安裝新的穿戴物品時，您每次都應該檢查這一部份。 :::

DressingTools 將具有相同名稱的角色 Blendshape 和穿戴物品 Blendshape 配對在一起，並在此處自動添加同步。

[![穿戴物品穿着時](/img/setup-cabanim-6-blendshape-sync.PNG)](/img/setup-cabanim-6-blendshape-sync.PNG)

### 7. 新增到衣櫃

:::caution 在您按下此按鈕之前，配置不會被儲存。 :::

按`新增至衣櫃`，穿戴物品將顯示在衣櫃視圖中。

[![按鈕](/img/setup-cabanim-7-addtocabinet.PNG)](/img/setup-cabanim-7-addtocabinet.PNG)

[![衣櫃視圖](/img/setup-cabanim-7-cabinetview.PNG)](/img/setup-cabanim-7-cabinetview.PNG)

### 8. 完成！

你準備好了！進入運行模式並查看結果！您可以使用 BlackStartX 的 Gesture Manager 在 Unity 編輯器中進行測試！

[![](/img/setup-cabanim-8-done.PNG)](/img/setup-cabanim-8-done.PNG)

:::tip 您可以隨時再次從衣櫃編輯器修改您的配置。

[![衣櫃](/img/setup-cabanim-8-tip-edit.PNG)](/img/setup-cabanim-8-tip-edit.PNG) :::

:::tip 如果你想增加額外的模組，你可以使用`進階`模式！

[![進階](/img/setup-cabanim-8-tip-advanced.PNG)](/img/setup-cabanim-8-tip-advanced.PNG) :::
