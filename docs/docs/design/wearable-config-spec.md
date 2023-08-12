---
sidebar_position: 2
---

# Wearable Configuration Specification

DressingTools' wearable configuration is stored in JSON file format and follows a set of rules in a defined JSON schema.

The choice to use JSON instead of Unity assets is to decouple the need to use Unity to process or read the configuration.
This allows editing without Unity and sharing the configuration over the Internet without sending the original prefab.

:::danger
This specification is still unstable and subject to change.
:::

### Schema

:::caution
Schema is in the future. It is not created yet. (https://github.com/poi-vrc/DressingTools/issues/122)
:::

### Example file

```json
{
    "configVersion": 1,
    "info": {
        "uuid": "1202461e-0683-44fd-806b-52a5e1c856c4",
        "name": "[FOIV] Autumn Memory",
        "author": "",
        "description": "",
        "createdTime": "2023-08-10T13:27:12.8494484Z",
        "updatedTime": "2023-08-10T13:27:12.8494484Z"
    },
    "targetAvatarConfig": {
        "guids": [],
        "name": "Maya",
        "armatureName": "Armature",
        "worldPosition": {
            "x": 0,
            "y": 0,
            "z": 0
        },
        "worldRotation": {
            "x": 0,
            "y": 0,
            "z": 0,
            "w": 1
        },
        "avatarLossyScale": {
            "x": 1,
            "y": 1,
            "z": 1
        },
        "wearableLossyScale": {
            "x": 1,
            "y": 1,
            "z": 1
        }
    },
    "modules": [
        {
            "$dtModuleType": "Chocopoi.DressingTools.Wearable.Modules.ArmatureMappingModule",
            "dresserName": "Chocopoi.DressingTools.Dresser.DTDefaultDresser",
            "wearableArmatureName": "Armature",
            "boneMappingMode": 0,
            "boneMappings": [],
            "serializedDresserConfig": "{\"dynamicsOption\":0}",
            "removeExistingPrefixSuffix": true,
            "groupBones": true
        },
        {
            "$dtModuleType": "Chocopoi.DressingTools.Wearable.Modules.AnimationGenerationModule",
            "avatarAnimationOnWear": {
                "toggles": [],
                "blendshapes": []
            },
            "wearableAnimationOnWear": {
                "toggles": [
                    {
                        "path": "Pumps",
                        "state": true
                    },
                    {
                        "path": "Shirt Ribbon",
                        "state": true
                    },
                    {
                        "path": "Shirt Skirt",
                        "state": true
                    },
                    {
                        "path": "Short Cardigan",
                        "state": true
                    },
                    {
                        "path": "PhysBones",
                        "state": true
                    }
                ],
                "blendshapes": []
            },
            "wearableCustomizables": []
        },
        {
            "$dtModuleType": "Chocopoi.DressingTools.Wearable.Modules.BlendshapeSyncModule",
            "blendshapeSyncs": [
                {
                    "avatarPath": "Body all",
                    "avatarBlendshapeName": "Big breasts",
                    "avatarFromValue": 0,
                    "avatarToValue": 100,
                    "wearablePath": "Shirt Ribbon",
                    "wearableBlendshapeName": "Big breasts",
                    "wearableFromValue": 0,
                    "wearableToValue": 100
                },
                {
                    "avatarPath": "Body all",
                    "avatarBlendshapeName": "Big breasts",
                    "avatarFromValue": 0,
                    "avatarToValue": 100,
                    "wearablePath": "Shirt Skirt",
                    "wearableBlendshapeName": "Big breasts",
                    "wearableFromValue": 0,
                    "wearableToValue": 100
                }
            ]
        }
    ]
}
```
