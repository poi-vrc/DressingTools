---
sidebar_position: 3
---

# Changing Properties with Radial Control

In this guide, we will make a radial in the menu to make Moe's `Big Ear` blendshape changes from 0 to 100.

### 0. Create a menu group and a smart control item

This guide requires you to have an existing menu group ready. Follow [this guide](/docs/getting-started/smart-control/menu-basics) to create a new menu group.

You can create one directly from the menu group component, or alternatively create a new GameObject under that menu group and add the DT Smart Control component.

![Add SmartControl](/img/smartcontrol-create-0.PNG)

### 1. Set up the menu item driver

:::caution
There is a problem in the UI currently so you must change the control type to `Motion Time` first before you can change the menu item to `Radial`.
:::

Set the control to `Motion Time` first.

![Control Type](/img/smartcontrol-motiontime-1-ctrltype.PNG)

Make sure the driver is `Menu Item` and the type is `Radial`. Customize the name and icon as you like.

![Menu Item](/img/smartcontrol-motiontime-1-menuitem.PNG)

### 2. Create a property group

![Create Property Group](/img/smartcontrol-chg-prop-2.PNG)

### 3. Drag the Ear object to the fields

Make sure the selection type is `Normal`. Drag the Ear object to the add field of `Include these objects` and `Pick Properties From`.

![Pick Object](/img/smartcontrol-chg-prop-3.PNG)

:::info
For more details about property groups, [check here](/docs/getting-started/smart-control/property-groups).
:::

### 4. Add the blendshape property

Find the `SkinnedMeshRenderer` component and click on `Blendshapes`. And find the `Big ear` blendshape and add it.

![Pick Object](/img/smartcontrol-chg-prop-4.PNG)

### 5. Map the blendshape values

The from and to values map to the float parameter of the radial control from 0.0 to 1.0.

![Pick Object](/img/smartcontrol-motiontime-5.PNG)


### 6. Done!

Enter play mode and test the result!

![Result](/img/smartcontrol-motiontime-done.gif)
