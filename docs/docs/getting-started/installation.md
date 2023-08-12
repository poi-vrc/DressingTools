---
sidebar_position: 1
---

# Installation

DressingTools is a **standalone Unity UPM package** and **does not require any game or tool SDKs to be installed to work**.
Unless you are trying to integrate DressingTools generated cabinets into these games or tools.

This is useful for workflows that is **not related to VRChat at all**. For example, creating **VRM** projects, **ChilloutVR**, **MMD** or 
Unity projects that does not support VRCSDK at all.

:::caution
It is by design to not require VRCSDK and DynamicBones (aka other toolkits). But the current alpha version might not 100% ready for this yet.
(Untested)

The current goal is to complete integration with VRCSDK first.
:::

## For VRChat

To integrate DressingTools with VRChat, it is **recommended to use VCC for installation** since it manages all libraries that
DressingTools require and the version of VRCSDK that we support.

### Via VCC (VRChat Creator Companion)

1. Install the VPM repository with the following button, or add the repository using `https://vpm.chocopoi.com/index.json` manually.
    <a
      className="button button--success button--lg"
      target="_self"
      href="vcc://vpm/addRepo?url=https%3A%2F%2Fvpm.chocopoi.com%2Findex.json">
      Install with VCC
    </a>

2. Press `I Understand, Add Repository` to add the repository.

    [![Installation VCC Underestand](/img/installation-vcc-repo-understand.PNG)](/img/installation-vcc-repo-understand.PNG)

3. Add the DressingTools package to your avatar project.

    [![Installation VCC Underestand](/img/installation-vcc-add-package.PNG)](/img/installation-vcc-add-package.PNG)

4. Open the project and you are good to go!

    [![Installation VCC Underestand](/img/teaser-1.PNG)](/img/teaser-1.PNG)

## Others

:::caution
Dependencies are still unstable right now.
:::

### Via .unitypackage

TODO

### Via UPM Git URL

TODO
