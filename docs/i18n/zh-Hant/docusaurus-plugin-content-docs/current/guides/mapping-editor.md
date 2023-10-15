---
sidebar_position: 1
---

# 映射編輯器

映射編輯器允許使用 DressingTools 透過手動設定覆蓋和映射來**對不相容的穿戴物品進行穿戴**。

儘管 DressingTools 應該能夠對各種穿戴物品進行穿戴，但也可能存在例外。 這個映射編輯器可以讓您可以預覽骨骼映射並根據需要進行修改。

:::info
也建議報告此類問題/提出功能請求，看看我們是否可以找到解決方法。
:::

[![Mapping editor manual](/img/mapping-editor-manual.PNG)](/img/mapping-editor-manual.PNG)

### 啟動映射編輯器

[![Mapping editor start](/img/mapping-editor-start.png)](/img/mapping-editor-start.png)

### 映射模式

[![Mapping editor modes](/img/mapping-editor-modes.PNG)](/img/mapping-editor-modes.PNG)

#### 自動（預設）

一切都是由穿著器控制並自動生成的, 沒有覆蓋和手動映射。

#### 覆蓋

穿著器會產生一堆映射，您可以為這些映射新增覆蓋。 對於修復次要映射很有用。

#### 手動

:::caution
這可能會導致某些用戶的角色有不相容問題。 謹慎使用。
:::

所有映射都是手動定義的。

### 新增映射

可以使用加號 `+` 按鈕新增映射，並將所需的穿戴物品骨頭拖曳到右側的物件欄位。

:::info
支援對同一角色骨頭的多個映射。 只需按下加號 `+` 按鈕即可新增多個映射。
:::

[![Mapping editor add](/img/mapping-editor-add-mapping.PNG)](/img/mapping-editor-add-mapping.PNG)

目前有 5 種映射類型：

[![Mapping editor mapping types](/img/mapping-editor-mapping-types.PNG)](/img/mapping-editor-mapping-types.PNG)

- **不做任何事情**：什麼都不做
   - 用例：覆蓋生成的映射
- **移動到角色骨頭**：將穿戴物品骨頭移動到目標角色骨頭
   - 用例：普通穿戴物品骨頭、配件
- **對角色骨頭做父約束**：創建從穿戴物品骨頭到目標角色骨頭的父約束，而無需移動
   - 用例：有動骨的骨頭（如胸骨）
- **當有動骨,添加變換組件忽略**：將穿戴物品骨頭添加到角色動骨的忽略變換清單中（當角色骨頭有動骨時）
   - 用例：**用於 Quest** 有動骨的骨頭（如胸骨），在**ParentConstraints 無法使用的情況下**
   - 缺點：它與所有穿戴物品**不 100% 相容**。
- **複製角色的動骨數據**：角色骨頭動骨數據複製到穿戴物品骨頭（當角色骨頭有動骨時）
   - 用例：有動骨的骨頭（如胸骨），分離兩個動骨元件以用於抓取骨頭目的🤔

### 刪除映射

可以使用 `x` 按鈕刪除映射。

[![Mapping editor remove](/img/mapping-editor-remove-mapping.PNG)](/img/mapping-editor-remove-mapping.PNG)
