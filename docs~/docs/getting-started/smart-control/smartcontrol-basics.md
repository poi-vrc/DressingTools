---
sidebar_position: 2
---

# Smart Control Basics

Create your animations to toggle objects or components on and off, or changing blendshape, component, shader properties etc.!

## Control types

The control type determines how you want the objects to be controlled. Currently, there are two types of control implemented.

![Control Type](/img/smartcontrol-basics-ctrltype.PNG)

### Binary

As the name suggests, this type of control only has two states: off (0) and on (1), controlled by a boolean parameter.

**Use cases:** Toggling objects and components on and off, changing blendshapes, shaders and properties to a specific value etc.

<a
    className="button button--success button--lg"
    target="_self"
    href="/docs/category/binary-control">
    Get started!
</a>

### Motion Time

This creates an animation with its play time controlled by a float parameter.

**Use cases:** Controlling blendshapes, shaders and properties with a radial item dynamically etc.

<a
    className="button button--success button--lg"
    target="_self"
    href="/docs/category/motion-time-control">
    Get started!
</a>

## Drivers

Driver means how you want this smart control to be driven by which mechanism. There are two types of driver and more will be added in the future.

### Menu item as driver

![Menu Item Driver](/img/smartcontrol-basics-driver-menuitem.PNG)

If the animator parameter name is left empty, it will be automatically generated on build.

There are some limitations that each menu item type supports:

|             | Button | Toggle | Radial | Two-axis | Four-axis | Sub-menu |
|-------------|--------|--------|--------|----------|-----------|----------|
| Binary      | ✅     | ✅    | ❌     | ❌      | ❌        | ❌      | 
| Motion Time | ✅*    | ✅*   | ✅     | 🚧      | 🚧        | ❌      | 

✅: Supported, ❌: Unsupported 🚧: Support Planned

*The menu item directly writes 0 or 1 to the motion time float parameter without any immediate values.

### Animator parameter as driver

If the animator parameter name is left empty, it will be automatically generated on build.

|             | Float  | Int    | Bool   | Others   |
|-------------|--------|--------|--------|----------|
| Binary      | ❌     | ❌    | ✅     | ❌      |
| Motion Time | ✅     | ✅*   | ❌     | ❌      |

✅: Supported, ❌: Unsupported

*Using motion time with data types other than float implies that only the value 0 or 1 will be effective to the control.
