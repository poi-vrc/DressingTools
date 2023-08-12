---
sidebar_position: 2
---

# Setup simple clothes (without Animations)

It is easy and clean to dress up your avatar with DressingTools. Although most of use case of DressingTools should be
using the wizard to automatically generate the menu and animations, you can also use it for dressing simple clothes without animations.

## Guide

In the series of getting started, we will dress [Eliya Workshop's Summer Streat](https://booth.pm/ja/items/4666271) into [Kyubi Closet's Moe](https://kyubihome.booth.pm/items/4667400).

:::caution
The current alpha version does not have animation path remapping implemented. Adding animations to wearable root objects work okay. But
animating objects inside wearable bone will not work, because the bone names and locations are changed in build. (https://github.com/poi-vrc/DressingTools/issues/91)
:::

:::info
You can embed prefabs in your avatars directly in v2 now!
:::

### 0. Open the DressingTools<sup>2</sup> window

[![New Cabinet](/img/setup-simple-0-open-window.PNG)](/img/setup-simple-0-open-window.PNG)

### 1. Create a cabinet (if not already)

:::info
UI subject to change
:::

First, we need to create a cabinet for handling our wearables. A cabinet saves our avatar-specific settings and presets.

[![New Cabinet](/img/setup-simple-1-new-cabinet.PNG)](/img/setup-simple-1-new-cabinet.PNG)

### 2. Drag the clothes to the avatar

Then, drag the clothes to the avatar and right-click to choose `DressingTools -> Auto-setup wearable (Mappings Only)`

[![Setup wearable](/img/setup-simple-2-setup-wearable.PNG)](/img/setup-simple-2-setup-wearable.PNG)

### 3. Done!

Get into Play Mode and then you will be able to test the wearable whether it's mapped to the bones correctly.

[![Setup done](/img/setup-simple-3-done.PNG)](/img/setup-simple-3-done.PNG)

## Behind the Scenes

### Creating cabinet

When you create a cabinet, a `DT Cabinet` component is added to the avatar root. It internally stores your avatar-specific settings
and presets that help you to save time dressing up different wearables later on.

[![Cabinet component created](/img/setup-simple-bts-cabinet-component-created.PNG)](/img/setup-simple-bts-cabinet-component-created.PNG)

### Using "Auto-setup wearable (Mappings Only)"

When you click on `Auto-setup wearable (Mappings Only)`, DressingTools runs an automatic setup to find the wearable armature and attempts
to dry-run and generate bone mappings for it.

It will create a `DT Cabinet Wearable` component on the wearable root with the generated configuration.

### Entering Play Mode

When entering play mode, the attached `DT Cabinet` will apply the wearables contained just before Unity has completed entering play mode.
