---
sidebar_position: 4
---

# Cross-control Actions

Cross-control actions allow smart controls to control another smart control at the same time.

Currently, only one kind of cross-control actions is implemented. More will be implemented in the future.

## Value

Sets another smart control to a specific value directly when enabled and disabled respectively.

![Cross-control value](/img/smartcontrol-crossctrl-value.PNG)

**Example use cases:**
- One outfit at a time
  - Turning off other smart controls when this smart control is active.
- Mixing outfits by enabling accessories of other outfits
  - Turning on other smart controls when this smart control is active.

:::info
For binary controls/bool parameters, a value of `0` means `false`, and `1` means `true`.
:::
