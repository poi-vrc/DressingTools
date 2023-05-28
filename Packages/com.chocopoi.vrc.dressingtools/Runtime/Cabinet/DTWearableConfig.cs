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
