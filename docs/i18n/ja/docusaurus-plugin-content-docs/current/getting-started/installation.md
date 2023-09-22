---
sidebar_position: 1
---

# Installation

DressingTools is a **standalone Unity UPM package** and **does not require any
game or tool SDKs to be installed to work**. Unless you are trying to integrate
DressingTools generated cabinets into these games or tools.

This is useful for workflows that is **not related to VRChat at all**. For
example, creating **VRM** projects, **ChilloutVR**, **MMD** or Unity projects
that does not support VRCSDK at all.

### Dependencies

DressingTools depends on two external projects `DressingFramework` and
`AvatarLib` currently.

## For VRChat

To integrate DressingTools with VRChat, **it is recommended to use VCC for
installation** since it manages all libraries that DressingTools require and the
version of VRCSDK that we support.

### Via VCC (VRChat Creator Companion)

1. Install the VPM repository with the following button, or add the repository
   using `https://vpm.chocopoi.com/index.json` manually.
   <a
className="button button--success button--lg"
target="_self"
href="vcc://vpm/addRepo?url=https%3A%2F%2Fvpm.chocopoi.com%2Findex.json">
   Install with VCC </a>

2. Press `I Understand, Add Repository` to add the repository.

   [![Installation VCC
   Understand](/img/installation-vcc-repo-understand.PNG)](/img/installation-vcc-repo-understand.PNG)

3. Add the DressingTools package to your avatar project.

   [![Installation VCC Add
   Package](/img/installation-vcc-add-package.PNG)](/img/installation-vcc-add-package.PNG)

   :::caution The packages might not be visible because DressingTools is still
   at pre-release (beta). You need to enable `Show Pre-release Packages` which
   available under `Settings -> Packages`. [![Instllation VCC
   Pre-release](/img/installation-vcc-prerelease.png)](/img/installation-vcc-prerelease.png)
   :::

4. Open the project and you are good to go!

   [![Installation VCC Teaser](/img/teaser-1.PNG)](/img/teaser-1.PNG)

## Other Projects

### Via UPM Git URL

:::danger If you have already installed DressingTools via VCC, do not perform
these steps. :::

:::caution Unity does not handle updates and dependencies properly for UPM Git
URLs. Make sure you install the correct versions of the dependencies to work
properly. :::

1. Start the Unity Package Manager

   [![Installation UPM Git Start Package
   Manager](/img/installation-upmgit-open-pkg-mgr.PNG)](/img/installation-upmgit-open-pkg-mgr.PNG)

2. Press `Add package from git URL...`

   [![Installation UPM Git Add from git
   URL](/img/installation-upmgit-install-from-git.PNG)](/img/installation-upmgit-install-from-git.PNG)

3. Add the following packages and make sure the required versions match the
   version of DressingTools that you are installing. Change the version after
   the `#` to your desired one.

   - DressingTools
     - `https://github.com/poi-vrc/DressingTools.git?path=Packages/com.chocopoi.vrc.dressingtools#2.0.0-beta`
   - DressingFramework
     - `https://github.com/poi-vrc/DressingFramework.git#1.0.0-beta`
   - AvatarLib
     - `https://github.com/poi-vrc/AvatarLib.git#1.0.0-beta`

### Via zip files

:::danger It is not recommended to install DressingTools in this way. :::

:::caution Remove the old packages before copying. Make sure you install the
correct versions of the dependencies to work properly. :::

You can download the zip files directly and decompress them into your `Packages`
folder. It will look like this:
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

### Via .unitypackage

:::danger Currently, it is not supported to use `.unitypackage` files for
installation. It might be supported in the future but it is not the recommended
way for installing DressingTools. :::
