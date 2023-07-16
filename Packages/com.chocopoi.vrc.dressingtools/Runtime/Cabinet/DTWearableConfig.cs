using System;
using System.Collections.Generic;
using System.Globalization;
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
    public enum DTWearableMappingMode
    {
        Auto = 0,
        Override = 1,
        Manual = 2
    }

    [Serializable]
    public class DTWearableConfig
    {
        public const int CurrentConfigVersion = 1;

        public int configVersion;
        public DTWearableInfo info;
        public DTAvatarConfig targetAvatarConfig;
        public DTWearableType wearableType;

        // Generic
        public string avatarPath;

        // Armature-based
        public string dresserName;
        public string wearableArmatureName;
        public DTWearableMappingMode boneMappingMode;
        public DTBoneMapping[] boneMappings;
        public DTWearableMappingMode objectMappingMode;
        public DTObjectMapping[] objectMappings;
        public string serializedDresserConfig;

        // Animation generation
        public DTAnimationPreset avatarAnimationOnWear; // execute on wear
        public DTAnimationPreset wearableAnimationOnWear;
        public DTWearableCustomizable[] wearableCustomizables; // items that show up in action menu for customization
        public DTAnimationBlendshapeSync[] blendshapeSyncs; // blendshapes to sync from avatar to wearables

        public DTWearableConfig()
        {
            // initialize some fields
            var isoTimeStr = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
            info = new DTWearableInfo
            {
                uuid = Guid.NewGuid().ToString(),
                createdTime = isoTimeStr,
                updatedTime = isoTimeStr
            };

            targetAvatarConfig = new DTAvatarConfig();
            avatarAnimationOnWear = new DTAnimationPreset()
            {
                toggles = new DTAnimationToggle[0],
                blendshapes = new DTAnimationBlendshapeValue[0]
            };
            wearableAnimationOnWear = new DTAnimationPreset()
            {
                toggles = new DTAnimationToggle[0],
                blendshapes = new DTAnimationBlendshapeValue[0]
            };
            wearableCustomizables = new DTWearableCustomizable[0];
            blendshapeSyncs = new DTAnimationBlendshapeSync[0];
        }

        // copy constructor
        public DTWearableConfig(DTWearableConfig toCopy)
        {
            // shallow copy
            configVersion = toCopy.configVersion;
            info = toCopy.info;
            targetAvatarConfig = toCopy.targetAvatarConfig;
            wearableType = toCopy.wearableType;

            avatarPath = toCopy.avatarPath;

            dresserName = toCopy.dresserName;
            wearableArmatureName = toCopy.wearableArmatureName;
            boneMappingMode = toCopy.boneMappingMode;
            boneMappings = toCopy.boneMappings;
            objectMappingMode = toCopy.objectMappingMode;
            objectMappings = toCopy.objectMappings;

            avatarAnimationOnWear = toCopy.avatarAnimationOnWear;
            wearableAnimationOnWear = toCopy.wearableAnimationOnWear;
            wearableCustomizables = toCopy.wearableCustomizables;
            blendshapeSyncs = toCopy.blendshapeSyncs;

            serializedDresserConfig = toCopy.serializedDresserConfig;
        }
    }
}
