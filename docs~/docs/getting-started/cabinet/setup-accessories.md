---
sidebar_position: 5
---

# Setup accessories or others

This section is useful for wearing all kinds of wearables that do not contain an **Armature**. For example, glasses, hair etc.

It is also useful for prefab creators or users that want to move the wearable root to position on apply only for better management.

## Guide

:::info
The guide assumes that you have already created a cabinet for your avatar according to the [setup cabinet guide](setup-cabinet).
:::

In this guide, we will create a cube and bind it to the right hand of [Kyubi Closet's Moe](https://kyubihome.booth.pm/items/4667400), while **still keeping the cube at the root of the avatar Moe**.

### 1. Drag your accessory into the avatar and adjust its position

Here we use a cube to demonstrate the workflow. We have a cube inside our avatar now and have adjusted its position while keeping it at the root.

[![Put into place](/img/setup-moveroot-1-put-into-place.PNG)](/img/setup-moveroot-1-put-into-place.PNG)

### 2. Setup accessory as a wearable

Right-click the accessory and click `DressingTools -> Setup wearable in editor`

[![Menu](/img/setup-moveroot-2-menu.PNG)](/img/setup-moveroot-2-menu.PNG)

### 3. Mapping

There are two ways currently to map your accessories to your avatar.

#### 3.1 Using the move wearable root module

For accessories that **do not contain an armature**, you can use this to map to your avatar, which is the case for our cube in this guide.
This module is no difference with simply moving the object to the target location by hand.

:::tip
You might need to disable modules other than `Move wearable root` according to your needs. 
For example, you might have to disable `Cabinet animation` as you are not treating this wearable as a clothes to change.
:::
In the move wearable root editor, select the location that you want the wearable to be moved to on apply.

[![Move root editor](/img/setup-moveroot-3-moverooteditor.png)](/img/setup-moveroot-3-moverooteditor.png)

#### 3.2 Using the mapping editor

For accessories that **contain an armature**, you can instead try to use the mapping editor to set manual mappings according to [this guide](guides/mapping-editor.md).

### 4. Done!

Add the configuration to the cabinet, and get into play mode to see the result! Your wearable is moved to the target location and binded together now!

[![Done](/img/setup-moveroot-4-done.PNG)](/img/setup-moveroot-4-done.PNG)
