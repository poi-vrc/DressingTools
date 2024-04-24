---
sidebar_position: 1
---

# Parameter Slot

*Since 2.5.0*

A parameter slot component acts as a shared tag among multiple SmartControl components.
These SmartControls are assigned with a specific value, and controlled by a single `int` or `float` animator parameter.
Only the SmartControl that the parameter value matches with will be enabled.

The mechanism is similar to Unity animator's Any State but internally it is not used.

![Group Dynamics](/img/comp-parameter-slot.png)

## Mappings

You can directly edit mappings either in the parameter slot component inspector or the SmartControl
driver inspector. You have to have one of the SmartControl mapped with the default value, otherwise it might not work properly.

## Settings

- **Parameter name**: Name of the `int`/`float` parameter. It will be automatically generated if it is left blank.
- **Value type**: `Int` or `Float` value type.
- **Parameter default value**: Default value
- **Network synced**: Whether this parameter should be network synced
- **Saved**: Whether this parameter should be saved among different worlds
