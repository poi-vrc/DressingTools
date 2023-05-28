using System;
using System.Collections.Generic;
using UnityEngine;

namespace Chocopoi.DressingTools.Cabinet
{
    [Serializable]
    public enum DTWearableType
    {
        Generic = 0,
        ArmatureBased = 1
    }

    [Serializable]
    public class DTAnimationToggle
    {
        public string path;
        public bool state;
    }

    [Serializable]
    public class DTAnimationBlendshapeValue
    {
        public string path;
        public string blendshapeName;
        public float value;
    }

    [Serializable]
    public class DTAnimationBlendshapeSync
    {
        public string avatarPath;
        public string avatarBlendshapeName;
        public string wearablePath;
        public string wearableBlendshapeName;
        public bool inverted;
    }

    [Serializable]
    public class DTWearableAnimationPreset
    {
        public DTAnimationToggle[] toggles;
        public DTAnimationBlendshapeValue[] blendshapes;
    }

    [Serializable]
    public enum DTWearableCustomizableType
    {
        Toggle = 0,
        Blendshape = 1,
        HybridToggle = 2,
        HybridBlendshape = 3,
    }

    [Serializable]
    public class DTWearableCustomizable
    {
        public DTWearableCustomizableType type;
        public DTAnimationToggle[] avatarRequiredToggles;
        public DTAnimationToggle[] wearableToggles;
        public DTAnimationBlendshapeValue[] avatarRequiredBlendshapes;
        public DTAnimationBlendshapeValue[] wearableBlendshapes;
    }

    [Serializable]
    public class DTAvatarConfig
    {
        public string guid;
        public string name;
        public string armatureName;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;
    }

    [Serializable]
    public class DTCabinetWearable : DTWearableConfig
    {
        public DTGameObjectReference[] objectReferences;
    }

    [Serializable]
    public class DTWearableConfig
    {
        public string name;
        public DTAvatarConfig[] targetAvatarConfigs;
        public DTWearableType wearableType;

        // Generic
        public string avatarPath;

        // Armature-based
        public string wearableArmatureName;
        public DTBoneMapping[] boneMapping;
        public DTObjectMapping[] objectMapping;

        // Animation generation
        public DTWearableAnimationPreset avatarAnimationOnWear; // execute on wear
        public DTWearableAnimationPreset wearableAnimationOnWear;
        public DTWearableCustomizable[] wearableCustomizables; // items that show up in action menu for customization
        public DTAnimationBlendshapeSync[] blendshapeSyncs; // blendshapes to sync from avatar to wearables
    }
}
