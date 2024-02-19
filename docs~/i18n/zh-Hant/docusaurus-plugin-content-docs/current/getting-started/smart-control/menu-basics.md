---
sidebar_position: 1
---

# 菜單基礎知識

DressingTools 有一個選單系統來處理和安裝自訂選單項目。

## 指南

在開始使用智能控制之前，讓我們先了解一些選單基礎知識。

### 1. 在角色內創建一個 GameObject

首先，您須在角色內建立一個新的 GameObject, 將其重命名為您喜歡的任何名稱。

![Add GameObject](/img/menu-basics-1.PNG)

### 2. 新增 DT Menu Group 元件

將 DT Menu Group 元件新增至新建立的 GameObject 中。 它將這個 GameObject 標記為選單的根, 該 GameObject 的子選單項目會被分組一起。

![Add Component](/img/menu-basics-2-1.PNG)
![Component](/img/menu-basics-2-2.PNG)

### 3. 新增 DT Menu Install 元件

將 DT Menu Install 元件新增至新建立的 GameObject 中。 此元件會將 DT 選單組元件中的選單項目加到角色根選單。

可以透過指定安裝路徑來變更安裝目的地（例如 `Menu1/Menu2/Menu3` 將遍歷 Menu1、Menu2 並將選單項目安裝到 Menu3）

如果留空，選單項目將直接追加到根選單中。

![Menu Install](/img/menu-basics-3.PNG)

### 4. 新增選單項

使用 DT Menu Group 元件中的按鈕新增些選單項目。

![Add Menu Items](/img/menu-basics-4.PNG)

:::info
參數下拉清單顯示從角色動畫圖層找到的參數。
如果您選擇其中一項參數，則所選參數將根據需要**自動配置為網路同步並儲存**。

可以透過使用 **DT Animator Parameters** 元件來覆寫此設定。

![Animator Parameters](/img/menu-basics-4-animparam.PNG)
:::

### 5. 新增智能控制項

您也可以直接從 DT Menu Group 元件新增智能控制。

![Smart Control](/img/menu-basics-5.PNG)

### 6. 完成！

進入運行模式時，然後您將能夠看到新的選單項目被加到角色根選單中！

![Play Mode](/img/menu-basics-6.PNG)
