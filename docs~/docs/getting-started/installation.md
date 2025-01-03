---
sidebar_position: 1
---

# Installation

DressingTools is a **standalone Unity UPM package** and **does not require any game or tool SDKs to be installed to work**.

This is useful for workflows that is **not related to VRChat at all**. For example, creating **VRM** projects, **MMD**, other VR social platforms or Unity projects that does not support VRCSDK at all.

### Via VCC (VRChat Creator Companion)

**It is strongly recommended to use VCC for installation** since it manages all libraries that DressingTools require and the version of VRCSDK that we support.
It also makes updating DressingTools easier for users.

1. Install the VPM repository with the following button, or add the repository using `https://vpm.chocopoi.com/index.json` manually.
    <a
      className="button button--success button--lg"
      target="_self"
      href="vcc://vpm/addRepo?url=https%3A%2F%2Fvpm.chocopoi.com%2Findex.json">
      Install with VCC
    </a>

2. Press `I Understand, Add Repository` to add the repository.

    [![Installation VCC Understand](/img/installation-vcc-repo-understand.PNG)](/img/installation-vcc-repo-understand.PNG)

3. Add the DressingTools package to your avatar project.

    [![Installation VCC Add Package](/img/installation-vcc-add-package.PNG)](/img/installation-vcc-add-package.PNG)

4. Open the project and you are good to go!

    [![Installation VCC Teaser](/img/teaser-1.PNG)](/img/teaser-1.PNG)

### Via OpenUPM

:::info
[OpenUPM](https://openupm.com) is a package registry for open-source Unity packages. It can handle package updates and dependencies properly compared to UPM Git URL.

It is recommended to use OpenUPM for installation if you are not using VCC. Especially for users who are not using DressingTools for VRChat.
:::

You can install DressingTools via OpenUPM by following the instructions below.

#### A. Install via OpenUPM command-line interface (CLI)
1. If you haven't already, install the OpenUPM CLI by following the instructions on the [OpenUPM website](https://openupm.com/docs/getting-started-cli.html).

2. Run the following command in your project directory to install DressingTools, its dependencies will also be installed automatically:
```shell
openupm add com.chocopoi.vrc.dressingtools
```

#### B. Install manually via Unity Package Manager (UPM)
Please follow the instrustions:
1. Open **Edit/Project Settings/Package Manager**
2. Add the following scoped registry:
    - Name: `package.openupm.com`
    - URL: `https://package.openupm.com`
    - Scopes:
        - `com.chocopoi.vrc.avatarlib`
        - `com.chocopoi.vrc.dressingframework`
        - `com.chocopoi.vrc.dressingtools`
3. Click `Save` or `Apply`
4. Open **Window/Package Manager**
5. Select `Add package by name...` or `Add package from git URL...`
6. Paste `com.chocopoi.vrc.dressingtools` into `Name`
7. Paste your desired version ([listed here](https://openupm.com/packages/com.chocopoi.vrc.dressingtools/?subPage=versions)) into `Version`, or leave it empty to get the latest version.
8. Click `Add`

### Via .unitypackage

:::info
You are always recommended to use VCC for installation if you are using it for VRChat.
:::

1. Navigate to [this](https://github.com/poi-vrc/DressingTools/releases/latest) website, and download the `DressingTools-x.x.x-with-deps.unitypackage`.

    [![Installation unitypackage GitHub](/img/installation-unitypackage-github.png)](/img/installation-unitypackage-github.png)

2. Double-click the unitypackage file or drag to your project, and done!


### Via zip files

:::caution
Remove the old packages before copying. Make sure you install the correct versions of the dependencies to work properly.
:::

You can download the zip files directly and decompress them into your `Packages` folder. It will look like this:
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


### Via UPM Git URL

:::caution
Unity does not handle updates and dependencies properly for UPM Git URLs. Make sure you install the correct versions of the dependencies to work properly.
:::

1. Start the Unity Package Manager

    [![Installation UPM Git Start Package Manager](/img/installation-upmgit-open-pkg-mgr.PNG)](/img/installation-upmgit-open-pkg-mgr.PNG)

2. Press `Add package from git URL...`

    [![Installation UPM Git Add from git URL](/img/installation-upmgit-install-from-git.PNG)](/img/installation-upmgit-install-from-git.PNG)

3. Add the following packages from top to bottom, make sure the required versions match the version of DressingTools that you are installing. Change the version after the `#` to your desired one.

    - AvatarLib
        - `https://github.com/poi-vrc/AvatarLib.git#1.0.2`
    - DressingFramework
        - `https://github.com/poi-vrc/DressingFramework.git#1.0.4`
    - DressingTools
        - `https://github.com/poi-vrc/DressingTools.git#2.0.0`
