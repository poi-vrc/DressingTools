---
sidebar_position: 2
---

# Using with VRM0

You can use DressingTools to dress clothes in a UniVRM-only (VRM0) project.

:::danger
This feature is experimental and not fully tested. Please expect bugs and unknown behaviours.

Since VRM is not the main focus right now, support for VRM is currently feature-lacking. Integrated build, VRM spring bones and physics will be supported soon.

This documentation could also be outdated.
:::

## Guide

### 1. Prepare a UniVRM (VRM0) project

Follow the instructions from the [official project](https://github.com/vrm-c/UniVRM#installation) for installing UniVRM. It is tested using Unity 2022.3 with no issues.

### 2. Install DressingTools

If you want to use DressingTools outside VCC supported Unity versions, follow the instructions from the [installation guide](/docs/getting-started/installation#via-unitypackage)
to install with the only three available ways listed below:

- Via .unitypackage
- Via zip files
- Via UPM Git URL

You will have the following packages shown in the Unity package manager: (liltoon is optional)

[![UPM](/img/guide-vrm-3-upmgr.PNG)](/img/guide-vrm-3-upmgr.PNG)

### 3. Import your avatar and clothes and other necessary packages

You might also need to install shaders (i.e. UTS, poiyomi, liltoon) to let your avatar and clothes to be shown in the editor,
although they will be swapped out by UniVRM soon.

### 4. Remove all missing scripts and convert to use VRM dynamics

Missing scripts are an extreme headache especially when dealing with Non-VRChat platforms.

All VRCSDK scripts (i.e. VRC Avatar Descriptor) have to be removed.
You have to either convert all `VRCPhysBone` and `DynamicBone` into VRM spring bones or delete them all.

You can make use of the hierarchy search and delete all the missing scripts:

[![Search](/img/guide-vrm-4-search.PNG)](/img/guide-vrm-4-search.PNG)
[![Delete](/img/guide-vrm-4-delete.PNG)](/img/guide-vrm-4-delete.PNG)

### 5. Dress your clothes

[Create a cabinet](/docs/getting-started/cabinet/setup-cabinet) and [dress your clothes](/docs/getting-started/cabinet/quick-setup-clothes) as usual as written in the previous guides.

:::info
Cabinet animation module is not supported in VRM and nothing will be generated.
:::

### 6. Apply cabinet as copy

This will create a copy of your avatar and the cabinet will applied in this copy. Then, you will use this copy to export a VRM file.

[![Copy](/img/guide-vrm-6-copy.PNG)](/img/guide-vrm-6-copy.PNG)

### 7. Export VRM

Export your VRM using the copy and you are done!

[![Export](/img/guide-vrm-7-export.PNG)](/img/guide-vrm-7-export.PNG)
