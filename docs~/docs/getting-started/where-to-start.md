---
sidebar_position: 2
---

# Where to start?

There are currently two systems for automatically creating animations and menus to toggle different items and outfits, targeted at different use cases.

**They are currently not interchangable. They will be migrated to integrate better in the future.**

## 1. Smart Control

*Since 2.3.0*

[![Smart Control](/img/where-to-start-smartcontrol.PNG)](/img/where-to-start-smartcontrol.PNG)

:::caution
Smart controls **do not** setup your outfit! You have to either use [Quick Setup Clothes](/docs/getting-started/cabinet/quick-setup-clothes) to **setup without cabinet animations**, or alternative tools to setup up armature merging etc.
:::

Generic component for creating animations to toggle objects or components on and off, or changing blendshape, component, shader properties etc.

Driven by a animator parameter, menu item and more to be added in the future.

<a
    className="button button--success button--lg"
    target="_self"
    href="/docs/category/smart-control">
    Get started!
</a>

### Use cases

- Creating avatar-based controls
- Maximizing the wardobe menu customization
- Making your own wardobe menu without using the cabinet system

## 2. Cabinet

[![Cabinet](/img/where-to-start-cabinet.PNG)](/img/where-to-start-cabinet.PNG)

:::info
This system will be eventually migrated to have better integration with the smart control system.
:::

Dedicated to setup and manage wearables. It provides a window to setup a outfit and its animations quickly.
It is based on the new smart controls system.

When cabinet animations are turned on, the system treats it as a outfit and generates smart controls for it.
Avatar dynamics are automatically grouped and disabled when it is not active.

<a
    className="button button--success button--lg"
    target="_self"
    href="/docs/category/cabinet">
    Get started!
</a>

### Use cases

- Wearing multiple new outfits as quick as possible
