---
sidebar_position: 1
---

# 安裝

DressingTools 是一個 **獨立的 Unity UPM 套件** 和 **不需要安裝任何遊戲或工具 SDK 即可運作**。

這對於 **與 VRChat 根本不相關的工作流程** 非常有用。例如建立 **VRM** 專案、**MMD**、其他VR社交平台、 或根本不支援 VRCSDK 的 Unity 專案。

### 透過 VCC (VRChat Creator Companion)

**強烈建議使用 VCC 進行安裝**，因為它管理 DressingTools 所需的所有庫以及我們支援的 VRCSDK 版本。
它還使你更容易更新 DressingTools。

1. 用下面的按鈕安裝 VPM 庫，或使用 `https://vpm.chocopoi.com/index.json` 手動新增庫。

  <a
  className="button button--success button--lg"
  target="_self"
  href="vcc://vpm/addRepo?url=https%3A%2F%2Fvpm.chocopoi.com%2Findex.json"> 使用 VCC 安裝 </a>

2. 按`I Understand, Add Repository`新增庫。

   [![安裝VCC了解](/img/installation-vcc-repo-understand.PNG)](/img/installation-vcc-repo-understand.PNG)

3. 將 DressingTools 套件新增到您的角色專案中。

   [![安裝VCC套件](/img/installation-vcc-add-package.PNG)](/img/installation-vcc-add-package.PNG)

4. 打開專案就可以用了！

   [![安裝 VCC 預告片](/img/teaser-1.PNG)](/img/teaser-1.PNG)

### 透過 OpenUPM

:::info
[OpenUPM](https://openupm.com) 是一個供開源 Unity 套件的套件庫。相較於 UPM Git URL，它可以更加妥善的處理套件更新和相依性。

如果您不使用 VCC，建議使用 OpenUPM 進行安裝。特別是對於不使用 DressingTools 用於 VRChat 的用戶。
:::

您可以按照以下說明透過 OpenUPM 安裝 DressingTools。

#### A. 透過 OpenUPM 命令列介面 (CLI)
1. 如果您尚未安裝，請按照 [OpenUPM 網站](https://openupm.com/docs/getting-started-cli.html) 上的說明安裝 OpenUPM CLI。

2. 在您的專案目錄中執行以下命令以安裝 DressingTools，它的相依套件也將自動安裝：
```shell
openupm add com.chocopoi.vrc.dressingtools
```

#### B. 透過 Unity 套件管理器 (UPM) 手動安裝
請按照以下指示：
1. 開啟 **Edit/Project Settings/Package Manager**
2. 新增以下 scoped registry:
    - Name: `package.openupm.com`
    - URL: `https://package.openupm.com`
    - Scopes:
        - `com.chocopoi.vrc.avatarlib`
        - `com.chocopoi.vrc.dressingframework`
        - `com.chocopoi.vrc.dressingtools`
3. 點擊 `Save` 或是 `Apply`
4. 開啟 **Window/Package Manager**
5. 選擇 `Add package by name...` 或 `Add package from git URL...`
6. 將 `com.chocopoi.vrc.dressingtools` 貼上到 `Name`
7. 參考[版本清單](https://openupm.com/packages/com.chocopoi.vrc.dressingtools/?subPage=versions)，貼上您想要安裝的版本到 `Version`，或是留空以取得最新版本。
8. 點擊 `Add`

### 透過 .unitypackage

:::info
如果您用 DressingTools 於 VRChat，建議您使用 VCC 進行安裝。
:::

1. 前往[這網站](https://github.com/poi-vrc/DressingTools/releases/latest), 然後下載 `DressingTools-x.x.x-with-deps.unitypackage`.

    [![Installation unitypackage GitHub](/img/installation-unitypackage-github.png)](/img/installation-unitypackage-github.png)

2. 雙擊 unitypackage 檔案或拖曳到您的專案中，就完成了！

### 透過 zip 檔案

:::caution
在複製之前刪除舊套件。確保安裝正確版本的依賴項才能正常運作。
:::

您可以直接下載 zip 檔案並解壓縮到您的 `Packages` 資料夾中。它看起來像這樣：
```
Packages
  |- com.chocopoi.vrc.dressingtools
  |- com.chocopoi.vrc.dressingframework
  |- com.chocopoi.vrc.avatarlib
```

- DressingTools
  - https://github.com/poi-vrc/DressingTools/releases
- DressingFramework
  - https://github.com/poi-vrc/DressingFramework/releases
- AvatarLib
  - https://github.com/poi-vrc/AvatarLib/releases
   
### 透過 UPM Git URL

:::caution
Unity 不會處理 UPM Git URL 的更新和依賴項。確保安裝正確版本的依賴項才能正常運作。
:::

1. 啟動Unity套件管理器

   [![安裝 UPM Git 啟動套件管理器](/img/installation-upmgit-open-pkg-mgr.PNG)](/img/installation-upmgit-open-pkg-mgr.PNG)

2. 按 `Add package from git URL...`

   [![安裝 UPM Git](/img/installation-upmgit-install-from-git.PNG)](/img/installation-upmgit-install-from-git.PNG)

3. 從上到下新增以下套件，並確保所需版本與您正在安裝的 DressingTools 版本相符。將`#`後面的版本變更為您想要的版本。

    - AvatarLib
        - `https://github.com/poi-vrc/AvatarLib.git#1.0.2`
    - DressingFramework
        - `https://github.com/poi-vrc/DressingFramework.git#1.0.4`
    - DressingTools
        - `https://github.com/poi-vrc/DressingTools.git#2.0.0`
