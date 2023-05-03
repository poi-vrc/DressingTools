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
    public class DTCabinetWearable : DTWearable
    {
        public new DTCabinetBoneMapping[] boneMapping;
        public new DTCabinetRootObject[] rootObjects;
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

    public class DTWearable
    {
        public string name;
        public string[] targetAvatarGuids;
        public DTWearableType wearableType;

        // Generic
        public string avatarPath;

        // Armature-based
        public string wearableArmatureName;
        public DTBoneMapping[] boneMapping;
        public DTRootObject[] rootObjects;

        // Animation generation
        public DTAnimationToggle[] avatarTogglesOnWear; // avatar items to toggle on being wore
        public DTAnimationBlendshapeValue[] avatarBlendshapeValuesOnWear; // blendshapes to set on being wore

        public DTAnimationToggle[] wearableToggles; // wearable items that the user wants to be able to be toggled
        public DTAnimationBlendshapeValue[] wearableBlendshapeValues; // wearable items' blendshapes that want to be able to be changed (value is default value)
        public DTAnimationBlendshapeSync[] blendshapeSyncs; // blendshapes to sync from avatar to wearables
    }
}
