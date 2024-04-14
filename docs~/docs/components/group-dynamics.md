---
sidebar_position: 1
---

# Group Dynamics

*Since 2.4.0*

This component groups found dynamics (including **both DynamicBone and VRCPhysBone components**) from specified locations to a single GameObject and animate them.

It is a feature extracted from the cabinet system to use as a component for other purposes.

![Group Dynamics](/img/comp-group-dyn.png)

## Animating

If you want to animate the dynamics components on and off, you can **directly use Unity animation editor** to animate the component enabled checkbox. The animation will be automatically converted to curves animating multiple dynamics components.

![Unity Anim](/img/comp-group-dyn-unity-anim.png)

You can also use **SmartControl** to animate this component directly as a toggle. Please refer to [Toggling a Component](/docs/getting-started/smart-control/binary/toggling-a-component) and set component type as `DTGroupDynamics`.

![SmartControl](/img/comp-group-dyn-smartcontrol.png)

:::caution
Animating the GameObject holding the dynamics instead of the component itself might still keep the dynamics running in the background, which is not a good idea for performance. It is recommended enable `Separate GameObjects` to separate dynamics in different GameObjects to animate them separately.
:::

## Settings

### Dynamics search mode
#### Control root

Search the dynamics controlling the specified roots. **Most likely you want to use this instead.**

#### Component root

Search the dynamics components that are inside the roots.

### Include and exclude transforms

Include and exclude the specified transforms in the search.

### Separate GameObjects

:::caution
You are recommended to have this enabled unless you understand what you are doing.
:::

Separate the found dynamics into separate child GameObjects under the current component GameObject. **This is required to be on if you want to animate components separately. Or they will not animate properly.**

From:

![From](/img/comp-group-dyn-separate-obj-1.png)

To:

![To](/img/comp-group-dyn-separate-obj-2.png)

### Set to current state

Enable to set all of the found dynamics to the current component enabled state of this component. This allows you to centrally turn on or off dynamics.

![Set state](/img/comp-group-dyn-set-state.png)
