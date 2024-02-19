---
sidebar_position: 2
---

# Toggling a Component

## Guide

In this guide, we will make a toggle in the menu to turn on and off some VRCPhysBone component on the avatar.

### 0. Create a menu group and a smart control item

This guide requires you to have an existing menu group ready. Follow [this guide](/docs/getting-started/smart-control/menu-basics) to create a new menu group.

You can create one directly from the menu group component, or alternatively create a new GameObject under that menu group and add the DT Smart Control component.

![Add SmartControl](/img/smartcontrol-create-0.PNG)

### 1. Set up the menu item driver

Make sure the driver is `Menu Item` and the type is `Toggle`. Customize the name and icon as you like.

![Setup Driver](/img/smartcontrol-toggle-go-1.PNG)

### 2. Set control type to binary and drag the object with the VRCPhysBone to the add field

The dropdown box specifies what component to be toggled. In this case, we want it to toggle the VRCPhysBone instead of the whole GameObject.

The checkbox next to the object field specifies the state when this control is active. In this case, it means the object will be turned off.

![Toggle Component](/img/smartcontrol-toggle-comp-2.PNG)

### 3. Done!

You have just successfully created a toggle for the component!
