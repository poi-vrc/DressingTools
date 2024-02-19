---
sidebar_position: 5
---

# Property Groups

Property groups in smart control is an advanced feature to allow grouping different components with the same property/blendshapes/shader parameter and control them at once.

During build, smart control searches for components from the objects found using the provided selection type. And attempts to find if they contain the properties that the group contains.
If yes, smart control includes them in the animation.

## Object selection types

Smart Control can search for objects during build by three different methods:

### Normal

Smart Control will only use the provided objects.

### Inverted

Smart Control will search from the provided root transfrom in the `Search Objects From` field and ignore the provided objects.

![Inverted](/img/smartcontrol-propgps-inverted.PNG)

:::caution
`Search Objects From` means the root transform that Smart Control will use to search during the build. `Pick Properties From` is used for picking properties during setup only!
:::

### Avatar-wide

Smart Control will search from the avatar root and ignore the provided objects.

## Multiple property groups

When you have multiple objects with the same property/blendshape, but you want to control them with different values.

You need to create a new property group and separately control them.

![Multiple](/img/smartcontrol-propgps-multi.PNG)
