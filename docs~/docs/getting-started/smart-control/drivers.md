---
sidebar_position: 3
---

# Drivers

Driver means how you want this smart control to be driven by which mechanism.

:::caution
🚧 This part of the documentation is still a work-in-progress.
:::

## Animator parameter driver

This is a generic driver that allows to use a custom animator parameter to control this SmartControl.

If the animator parameter name is left empty, it will be automatically generated on build.

## Menu item driver

This driver will add or replace the existing `DT Menu Item` component in the same GameObject.
The menu item will control the SmartControl directly with a single parameter.

![Menu Item Driver](/img/smartcontrol-basics-driver-menuitem.PNG)

If the animator parameter name is left empty, it will be automatically generated on build.

Currently, it supports the following:

|             | Button | Toggle | Radial | Two-axis | Four-axis | Sub-menu |
|-------------|--------|--------|--------|----------|-----------|----------|
| Binary      | ✅     | ✅    | ❌     | ❌      | ❌        | ❌      | 
| Motion Time | ❌     | ❌    | ✅     | ❌      | ❌        | ❌      | 

✅: Supported, ❌: Unsupported

## Parameter slot driver

*Since 2.5.0*

SmartControls using the same parameter slot are assigned with a specific value, and controlled by a single `int` or `float` animator parameter.
Only the SmartControl that the parameter value matches with will be enabled.

![Parameter slot driver](/img/smartcontrol-basics-driver-parameterslot.png)

The mechanism is similar to Unity animator's Any State but internally it is not used.

For more details, please read the documentation for [Parameter Slot](/docs/components/parameter-slot).

## VRCPhysBone driver

*Since 2.4.0*

This driver makes the SmartControl to be controlled by a VRC PhysBone component, allowing
to easily create controls to interact with other players.

![VRCPhysBone Driver](/img/smartcontrol-basics-driver-vrcphysbone.png)

If the animator parameter prefix is left empty, it will be automatically generated on build.

For more details about PhysBone interactive features, please refer to [the official VRChat documentation here](https://creators.vrchat.com/avatars/avatar-dynamics/physbones/).

:::caution
You must select at least one condition or one source. Otherwise, the SmartControl will be ignored and will not be generated.
:::

For different combinations of conditions and sources, they generate different control type of animations:

|         | None        | Grabbed     | Posed       | Grabbed or Posed |
|---------|-------------|-------------|-------------|------------------|
| None    | ❌          | Binary      | Binary      | Binary           |
| Angle   | Motion Time | Motion Time | Motion Time | Motion Time      |
| Stretch | Motion Time | Motion Time | Motion Time | Motion Time      |
| Squish  | Motion Time | Motion Time | Motion Time | Motion Time      |

❌: Will be ignored and no generation

For example, if you want your Ahoge hair to be always affecting the control when being squished without grabbing or posing,
you would want to set **Condition** to **None** and **Source** to **Squish**.

If you want it to only be affected by stretching when grabbed or posed,
you would want to set **Condition** to **Grabbed or Posed** and **Source** to **Stretch**.
