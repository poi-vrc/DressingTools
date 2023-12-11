---
sidebar_position: 1
---

# Mapping Editor

The mapping editor **allows incompatible wearables to be dressed** using DressingTools by manually setting up overrides and mappings.

There might be exceptions even though DressingTools should be able to dress all kinds of wearables. This mapping editor can let
you preview the bone mappings and make modifications if necessary.

:::info
It is also recommended to report such issues / make feature requests to see if we can have a workaround.
:::

[![Mapping editor manual](/img/mapping-editor-manual.PNG)](/img/mapping-editor-manual.PNG)

### Starting the mapping editor

[![Mapping editor start](/img/mapping-editor-start.png)](/img/mapping-editor-start.png)

### Mapping modes

[![Mapping editor modes](/img/mapping-editor-modes.PNG)](/img/mapping-editor-modes.PNG)

#### Auto (Default)

Everything is dresser controlled and automatically generated. No overrides and manual mappings.

#### Override

Dresser generates a bunch of mappings and you can add overrides for those mappings. Useful for fixing minor bone mappings.

#### Manual

:::caution
This might cause incompatiblity issues with some users' avatar that have some bones removed/relocated. Use with caution.
:::

All mappings are manually defined and the final overcome will be the same defined mappings.

### Adding a mapping

A mapping can be added using the plus `+` button, and drag the desired wearable bone to the object field at the right.

:::info
Multiple mappings to the same avatar bone is supported. Just press the plus `+` button to add multiple mappings.
:::

[![Mapping editor add](/img/mapping-editor-add-mapping.PNG)](/img/mapping-editor-add-mapping.PNG)

There are currently 5 mapping types:

[![Mapping editor mapping types](/img/mapping-editor-mapping-types.PNG)](/img/mapping-editor-mapping-types.PNG)

- **Do Nothing**: Literally do nothing
  - Use cases: To override generated mappings
- **Move to Avatar Bone**: Move the wearable bone to the target avatar bone
  - Use cases: normal wearable bones, accessories
- **ParentConstraint to Avatar Bone**: Create ParentConstraint from wearable bone to target avatar bone without moving
  - Use cases: Bones with dynamics (i.e. Breast bones)
- **IgnoreTransform on Dynamics**: The wearable bone will be added to the avatar dynamics' ignore transforms list (Only if the avatar bone is controlled by a dynamics)
  - Use cases: **For Quest avatar** bones with dynamics (i.e. Breast bones), in situations where **ParentConstraints cannot be used.**
  - Downsides: It is **not 100% compatible** with all wearables.
- **Copy Avatar Data on Dynamics**: The avatar dynamics will be copied to the wearable bone (Only if the avatar bone is controlled by a dynamics)
  - Use cases: Bones with dynamics (i.e. Breast bones), separating two dynamics component for grabbing purposes 🤔

### Removing a mapping

A mapping can be removed using the cross `x` button.

[![Mapping editor remove](/img/mapping-editor-remove-mapping.PNG)](/img/mapping-editor-remove-mapping.PNG)
