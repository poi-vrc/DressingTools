---
sidebar_position: 2
---

# Quick setup clothes

It is easy and clean to dress up your avatar with DressingTools. 

You can use DressingTools **for dressing simple clothes with a single click and animate them manually**, although most of use case of DressingTools should be using the cabinet system to automatically generate the menu and animations.

:::info
This does not automatically generate cabinet animations and menus. You have to setup using [another method](setup-clothes-with-cabinet-anim) instead. You can also `Edit` the wearable in the cabinet window and press `Auto-setup` to have the same effect.
:::

:::info Pro Tip
You can animate the objects inside your wearable just like how you make animations usually!
They are automatically remapped during apply seamlessly.
:::

## Guide

:::info
The guide assumes that you have already created a cabinet for your avatar according to the [setup cabinet guide](setup-cabinet).
:::

In the series of getting started, we will dress [Eliya Workshop's Summer Streat](https://booth.pm/ja/items/4666271) into [Kyubi Closet's Moe](https://kyubihome.booth.pm/items/4667400).

### 1. Drag the clothes to the avatar

Drag the clothes to the avatar and right-click to choose `DressingTools -> Quick setup clothes`

[![Setup wearable](/img/setup-simple-2-setup-wearable.PNG)](/img/setup-simple-2-setup-wearable.PNG)

### 2. Done!

Get into Play Mode and then you will be able to test the wearable whether it's mapped to the bones correctly.

[![Setup done](/img/setup-simple-3-done.PNG)](/img/setup-simple-3-done.PNG)

## Behind the Scenes

### Using "Auto-setup wearable (Mappings Only)"

When you click on `Quick setup clothes`, DressingTools runs an automatic setup to find the wearable armature and attempts
to dry-run and generate bone mappings for it.

It will create a `DT Wearable` component on the wearable root with the generated configuration.

### Entering Play Mode

When entering play mode, the attached `DT Cabinet` will apply the wearables contained just before Unity has completed entering play mode.
