---
sidebar_position: 2
---

# 在 VRM 使用

您可以使用 DressingTools 在僅限 UniVRM 的專案中穿衣服。

:::info
此功能是實驗性的。 整合建造、VRM Spring Bone 和物理會在未來支援。
:::

## 指南

### 1. 準備 UniVRM 項目

按照[官方專案](https://github.com/vrm-c/UniVRM#installation)中的說明安裝 UniVRM。 曾使用 Unity 2022.3 進行測試。

### 2. 安裝 DressingTools

如果您想使用 UniVRM，您**無法透過 VCC 安裝 DressingTools**。 請依照[安裝指南](http://localhost:3000/docs/getting-started/installation#via-unitypackage)中的說明, 僅使用下面列出的三種可用方式進行安裝：

- 透過 .unitypackage
- 透過 zip 檔案
- 透過 UPM Git URL

Unity 套件管理器中將顯示以下套件：（liltoon 是可選的）

[![UPM](/img/guide-vrm-3-upmgr.PNG)](/img/guide-vrm-3-upmgr.PNG)

### 3. 導入你的角色和衣服以及其他必要的套件

您可能還需要安裝 Shaders（如 UTS、poiyomi、liltoon）以使您的頭像和衣服正確地顯示在 Unity 中，
不過它們會被 UniVRM 取代成 VRM shaders。

### 4. 刪除所有缺少的腳本

缺少腳本是一個非常令人頭痛的問題，尤其是在處理非 VRChat 平台時。

所有 VRCSDK 腳本（即 VRC Avatar Descriptor）都必須刪除。
您必須將所有 `VRCPhysBone` 和 `DynamicBone` 轉換為 VRM Spring Bone，或將它們全部刪除。

您可以利用 Hierarchy 搜尋並刪除所有缺少的腳本：

[![Search](/img/guide-vrm-4-search.PNG)](/img/guide-vrm-4-search.PNG)
[![Delete](/img/guide-vrm-4-delete.PNG)](/img/guide-vrm-4-delete.PNG)

### 5. 穿你的衣服

像先前指南中所寫的一樣，[建立一個衣櫃](/docs/getting-started/cabinet/setup-cabinet)和[穿衣服](/docs/getting-started/cabinet/quick-setup-clothes)。

:::info
衣櫃動畫模組不支援 VRM，不會產生任何內容。
:::

### 6. 將衣櫃套用成一個副本

這將創建您的角色的副本，並且將衣櫃套用在副本中。然後您用此副本匯出 VRM 檔案。

[![Copy](/img/guide-vrm-6-copy.PNG)](/img/guide-vrm-6-copy.PNG)

### 7. 匯出 VRM

用副本匯出您的 VRM，就完成了！

[![Export](/img/guide-vrm-7-export.PNG)](/img/guide-vrm-7-export.PNG)
