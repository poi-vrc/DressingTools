/*
 * Copyright (c) 2023 chocopoi
 * 
 * This file is part of DressingTools.
 * 
 * DressingTools is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 * 
 * DressingTools is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with DressingTools. If not, see <https://www.gnu.org/licenses/>.
 */

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Chocopoi.DressingFramework.Extensibility.Sequencing;
using Chocopoi.DressingFramework.Localization;
using Chocopoi.DressingFramework.Serialization;
using Chocopoi.DressingTools.Components.Modifiers;
using Chocopoi.DressingTools.Localization;
using Chocopoi.DressingTools.OneConf.Serialization;
using Chocopoi.DressingTools.OneConf.Wearable.Modules.BuiltIn;
using Chocopoi.DressingTools.OneConf.Wearable.Modules.BuiltIn.ArmatureMapping;
using Chocopoi.DressingTools.Passes.Modifiers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.OneConf.Wearable.Modules
{
    [InitializeOnLoad]
    internal class ArmatureMappingWearableModuleProvider : WearableModuleProvider
    {
        public class DefaultDresserSettings
        {
            public enum DynamicsOptions
            {
                RemoveDynamicsAndUseParentConstraint = 0,
                KeepDynamicsAndUseParentConstraintIfNecessary = 1,
                IgnoreTransform = 2,
                CopyDynamics = 3,
                IgnoreAll = 4,
                Auto = 5,
            }

            public DynamicsOptions dynamicsOption;

            public DefaultDresserSettings()
            {
                // default settings
                dynamicsOption = DynamicsOptions.Auto;
            }

            private static int DynamicsOptionToUI(DynamicsOptions dynamicsOptions)
            {
                switch (dynamicsOptions)
                {
                    case DynamicsOptions.RemoveDynamicsAndUseParentConstraint:
                        return 1;
                    case DynamicsOptions.KeepDynamicsAndUseParentConstraintIfNecessary:
                        return 2;
                    case DynamicsOptions.IgnoreTransform:
                        return 3;
                    case DynamicsOptions.CopyDynamics:
                        return 4;
                    case DynamicsOptions.IgnoreAll:
                        return 5;
                    default:
                    case DynamicsOptions.Auto:
                        return 0;
                }
            }

            private static DynamicsOptions UIToDynamicsOption(int index)
            {
                switch (index)
                {
                    case 1:
                        return DynamicsOptions.RemoveDynamicsAndUseParentConstraint;
                    case 2:
                        return DynamicsOptions.KeepDynamicsAndUseParentConstraintIfNecessary;
                    case 3:
                        return DynamicsOptions.IgnoreTransform;
                    case 4:
                        return DynamicsOptions.CopyDynamics;
                    case 5:
                        return DynamicsOptions.IgnoreAll;
                    default:
                    case 0:
                        return DynamicsOptions.Auto;
                }
            }

            [ExcludeFromCodeCoverage]
            public bool DrawEditorGUI()
            {
                var modified = false;

                // Dynamics Option
                var newDynamicsOption = UIToDynamicsOption(EditorGUILayout.Popup(t._("dressers.standard.settings.dynamicsOptionPopup.label"), DynamicsOptionToUI(dynamicsOption), new string[] {
                        t._("dressers.standard.settings.dynamicsOptionPopup.auto"),
                        t._("dressers.standard.settings.dynamicsOptionPopup.removeDynamicsAndAddParentConstraint"),
                        t._("dressers.standard.settings.dynamicsOptionPopup.keepDynamicsAndAddParentConstraintIfNeeded"),
                        t._("dressers.standard.settings.dynamicsOptionPopup.removeDynamicsAndAddIgnoreTransform"),
                        t._("dressers.standard.settings.dynamicsOptionPopup.copyAvatarDynamicsData"),
                        t._("dressers.standard.settings.dynamicsOptionPopup.ignoreAllDynamics")
                    }));

                modified |= dynamicsOption != newDynamicsOption;
                dynamicsOption = newDynamicsOption;

                return modified;
            }
        }

        private static readonly I18nTranslator t = I18n.ToolTranslator;
        private const string LogLabel = "ArmatureModule";

        [ExcludeFromCodeCoverage] public override string Identifier => ArmatureMappingWearableModuleConfig.ModuleIdentifier;
        [ExcludeFromCodeCoverage] public override string FriendlyName => t._("modules.wearable.armatureMapping.friendlyName");
        [ExcludeFromCodeCoverage] public override bool AllowMultiple => false;
        [ExcludeFromCodeCoverage]
        public override BuildConstraint Constraint =>
            InvokeAtStage(BuildStage.Transpose)
                .BeforePass<ArmatureMappingPass>()
                .Build();

        private static DefaultDresserSettings DeserializeDefaultDresserSettings(string serializedJson)
        {
            return JsonConvert.DeserializeObject<DefaultDresserSettings>(serializedJson);
        }
        public override IModuleConfig DeserializeModuleConfig(JObject jObject)
        {
            // TODO: do schema check

            var version = jObject["version"].ToObject<SerializationVersion>();
            if (version.Major > ArmatureMappingWearableModuleConfig.CurrentConfigVersion.Major)
            {
                throw new System.Exception("Incompatible ArmatureMappingWearableModuleConfig version: " + version.Major + " > " + ArmatureMappingWearableModuleConfig.CurrentConfigVersion.Major);
            }

            return jObject.ToObject<ArmatureMappingWearableModuleConfig>();
        }

        public override IModuleConfig NewModuleConfig() => new ArmatureMappingWearableModuleConfig();

        private static DTArmatureMapping.MappingMode ConvertToNewMappingMode(BoneMappingMode boneMappingMode)
        {
            if (boneMappingMode == BoneMappingMode.Override)
            {
                return DTArmatureMapping.MappingMode.Override;
            }
            else if (boneMappingMode == BoneMappingMode.Manual)
            {
                return DTArmatureMapping.MappingMode.Manual;
            }
            else
            {
                return DTArmatureMapping.MappingMode.Auto;
            }
        }

        private static void ConvertToNewMappings(Transform wearableRoot, List<BoneMapping> boneMappings, List<DTObjectMapping.Mapping> objectMappings, List<DTArmatureMapping.Tag> tags)
        {
            if (boneMappings == null)
            {
                objectMappings.Clear();
                tags.Clear();
                return;
            }

            foreach (var boneMapping in boneMappings)
            {
                if (string.IsNullOrEmpty(boneMapping.avatarBonePath) ||
                    string.IsNullOrEmpty(boneMapping.wearableBonePath))
                {
                    continue;
                }
                var sourceTransform = wearableRoot.Find(boneMapping.wearableBonePath);

                if (boneMapping.mappingType == BoneMappingType.DoNothing)
                {
                    // for now we can only do this, add DoNothing in both
                    objectMappings.Add(new DTObjectMapping.Mapping()
                    {
                        Type = DTObjectMapping.Mapping.MappingType.DoNothing,
                        SourceTransform = sourceTransform,
                        TargetPath = boneMapping.avatarBonePath
                    });
                    tags.Add(new DTArmatureMapping.Tag()
                    {
                        Type = DTArmatureMapping.Tag.TagType.DoNothing,
                        SourceTransform = sourceTransform
                    });
                }
                else if (boneMapping.mappingType == BoneMappingType.MoveToBone)
                {
                    objectMappings.Add(new DTObjectMapping.Mapping()
                    {
                        Type = DTObjectMapping.Mapping.MappingType.MoveToBone,
                        SourceTransform = sourceTransform,
                        TargetPath = boneMapping.avatarBonePath
                    });
                }
                else if (boneMapping.mappingType == BoneMappingType.ParentConstraint)
                {
                    objectMappings.Add(new DTObjectMapping.Mapping()
                    {
                        Type = DTObjectMapping.Mapping.MappingType.ParentConstraint,
                        SourceTransform = sourceTransform,
                        TargetPath = boneMapping.avatarBonePath
                    });
                }
                else if (boneMapping.mappingType == BoneMappingType.IgnoreTransform)
                {
                    objectMappings.Add(new DTObjectMapping.Mapping()
                    {
                        Type = DTObjectMapping.Mapping.MappingType.IgnoreTransform,
                        SourceTransform = sourceTransform,
                        TargetPath = boneMapping.avatarBonePath
                    });
                }
                else if (boneMapping.mappingType == BoneMappingType.CopyDynamics)
                {
                    var tag = new DTArmatureMapping.Tag
                    {
                        Type = DTArmatureMapping.Tag.TagType.CopyDynamics,
                        SourceTransform = sourceTransform,
                        TargetPath = boneMapping.avatarBonePath
                    };
                    tags.Add(tag);
                }
            }
        }

        private static DTArmatureMapping.AMDresserStandardConfig.DynamicsOptions ConvertToNewDynamicsOption(DefaultDresserSettings.DynamicsOptions dynamicsOptions)
        {
            if (dynamicsOptions == DefaultDresserSettings.DynamicsOptions.RemoveDynamicsAndUseParentConstraint)
            {
                return DTArmatureMapping.AMDresserStandardConfig.DynamicsOptions.RemoveDynamicsAndUseParentConstraint;
            }
            else if (dynamicsOptions == DefaultDresserSettings.DynamicsOptions.KeepDynamicsAndUseParentConstraintIfNecessary)
            {
                return DTArmatureMapping.AMDresserStandardConfig.DynamicsOptions.KeepDynamicsAndUseParentConstraintIfNecessary;
            }
            else if (dynamicsOptions == DefaultDresserSettings.DynamicsOptions.IgnoreTransform)
            {
                return DTArmatureMapping.AMDresserStandardConfig.DynamicsOptions.IgnoreTransform;
            }
            else if (dynamicsOptions == DefaultDresserSettings.DynamicsOptions.CopyDynamics)
            {
                return DTArmatureMapping.AMDresserStandardConfig.DynamicsOptions.CopyDynamics;
            }
            else if (dynamicsOptions == DefaultDresserSettings.DynamicsOptions.IgnoreAll)
            {
                return DTArmatureMapping.AMDresserStandardConfig.DynamicsOptions.IgnoreAll;
            }
            else
            {
                return DTArmatureMapping.AMDresserStandardConfig.DynamicsOptions.Auto;
            }
        }

        public override bool Invoke(CabinetContext cabCtx, WearableContext wearCtx, ReadOnlyCollection<WearableModule> modules, bool isPreview)
        {
            if (isPreview) return true;

            if (modules.Count == 0)
            {
                return true;
            }

            var armatureMappingConfig = (ArmatureMappingWearableModuleConfig)modules[0].config;

            var wearableArmature = OneConfUtils.GuessArmature(wearCtx.wearableGameObject,
                string.IsNullOrEmpty(armatureMappingConfig.wearableArmatureName) ?
                cabCtx.cabinetConfig.avatarArmatureName :
                armatureMappingConfig.wearableArmatureName);
            if (wearableArmature == null)
            {
                cabCtx.dkCtx.Report.LogError(LogLabel, $"Could not find wearable armature: {armatureMappingConfig.wearableArmatureName}");
                return false;
            }

            var comp = wearCtx.wearableGameObject.AddComponent<DTArmatureMapping>();
            comp.TargetArmaturePath = cabCtx.cabinetConfig.avatarArmatureName;
            comp.SourceArmature = wearableArmature;
            comp.Mode = ConvertToNewMappingMode(armatureMappingConfig.boneMappingMode);
            ConvertToNewMappings(
                wearCtx.wearableGameObject.transform,
                armatureMappingConfig.boneMappings,
                comp.Mappings,
                comp.Tags);
            comp.GroupBones = armatureMappingConfig.groupBones;
            comp.Prefix = "";
            comp.Suffix = $" ({wearCtx.wearableConfig.info.name})";
            comp.PreventDuplicateNames = true;
            // removeExistingPrefixSuffix is ignored for now

            // TODO: for now we ignore dresserName and default all to default dresser
            comp.DresserType = DTArmatureMapping.DresserTypes.Standard;
            var defaultDresserSettings = DeserializeDefaultDresserSettings(armatureMappingConfig.serializedDresserConfig) ?? new DefaultDresserSettings();
            comp.DresserStandardConfig.DynamicsOption = ConvertToNewDynamicsOption(defaultDresserSettings.dynamicsOption);

            return true;
        }
    }
}
