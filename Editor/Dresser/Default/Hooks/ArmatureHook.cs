﻿/*
 * File: ArmatureHook.cs
 * Project: DressingTools
 * Created Date: Saturday, July 29th 2023, 10:31:11 am
 * Author: chocopoi (poi@chocopoi.com)
 * -----
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
using Chocopoi.AvatarLib.Animations;
using Chocopoi.DressingFramework.Detail.DK.Logging;
using Chocopoi.DressingFramework.Localization;
using Chocopoi.DressingTools.Dynamics;
using Chocopoi.DressingTools.Dynamics.Proxy;
using Chocopoi.DressingTools.Localization;
using Chocopoi.DressingTools.OneConf;
using Chocopoi.DressingTools.OneConf.Wearable.Modules.BuiltIn.ArmatureMapping;
using UnityEngine;

namespace Chocopoi.DressingTools.Dresser.Default.Hooks
{
    internal class ArmatureHook : IDefaultDresserHook
    {
        private static readonly I18nTranslator t = I18n.ToolTranslator;

        private static void AddRecursiveDynamicsBindings(Transform targetAvatarRoot, Transform targetWearableRoot, Transform avatarDynamicsRoot, Transform wearableDynamicsRoot, List<BoneMapping> boneMappings, BoneMappingType bindingType)
        {
            // add bone mapping

            boneMappings.Add(new BoneMapping()
            {
                mappingType = bindingType,
                avatarBonePath = AnimationUtils.GetRelativePath(avatarDynamicsRoot, targetAvatarRoot),
                wearableBonePath = AnimationUtils.GetRelativePath(wearableDynamicsRoot, targetWearableRoot)
            });

            // scan for other child bones

            var childs = new List<Transform>();

            for (var i = 0; i < wearableDynamicsRoot.childCount; i++)
            {
                childs.Add(wearableDynamicsRoot.GetChild(i));
            }

            foreach (var child in childs)
            {
                var avatarTrans = OneConfUtils.GuessMatchingAvatarBone(avatarDynamicsRoot, child.name);
                if (avatarTrans != null)
                {
                    AddRecursiveDynamicsBindings(targetAvatarRoot, targetWearableRoot, avatarTrans, child, boneMappings, bindingType);
                }
            }
        }

        private static void ProcessBone(DKReport report, DefaultDresserSettings settings, List<IDynamics> avatarDynamicsList, List<IDynamics> wearableDynamicsList, int level, Transform avatarBoneParent, Transform clothesBoneParent, List<BoneMapping> boneMappings)
        {
            var childs = new List<Transform>();

            for (var i = 0; i < clothesBoneParent.childCount; i++)
            {
                childs.Add(clothesBoneParent.GetChild(i));
            }

            foreach (var child in childs)
            {
                // skip our container
                if (child.name.EndsWith("_DT"))
                {
                    continue;
                }

                var avatarTrans = OneConfUtils.GuessMatchingAvatarBone(avatarBoneParent, child.name);

                if (avatarTrans == null)
                {
                    if (level == 0)
                    {
                        report.LogWarnLocalized(t, DefaultDresser.LogLabel, DefaultDresser.MessageCode.BonesNotMatchingInArmatureFirstLevel, child.name);
                        continue;
                    }
                    else
                    {
                        report.LogInfoLocalized(t, DefaultDresser.LogLabel, DefaultDresser.MessageCode.NonMatchingWearableBoneKeptUntouched, child.name);
                    }
                }
                else
                {
                    // Find whether there is a DynamicBone/PhysBone component controlling the bone

                    var avatarDynamics = DynamicsUtils.FindDynamicsWithRoot(avatarDynamicsList, avatarTrans);
                    var wearableDynamics = DynamicsUtils.FindDynamicsWithRoot(wearableDynamicsList, child);

                    if (avatarDynamics != null)
                    {
                        if (settings.dynamicsOption == DefaultDresserDynamicsOption.RemoveDynamicsAndUseParentConstraint) //remove and use parent constraints
                        {
                            AddRecursiveDynamicsBindings(settings.targetAvatar.transform, settings.targetWearable.transform, avatarTrans, child, boneMappings, BoneMappingType.ParentConstraint);
                        }
                        else if (settings.dynamicsOption == DefaultDresserDynamicsOption.KeepDynamicsAndUseParentConstraintIfNecessary) //keep dynbone and use parentconstraint if necessary
                        {
                            if (wearableDynamics == null)
                            {
                                AddRecursiveDynamicsBindings(settings.targetAvatar.transform, settings.targetWearable.transform, avatarTrans, child, boneMappings, BoneMappingType.ParentConstraint);
                            }
                        }
                        else if (settings.dynamicsOption == DefaultDresserDynamicsOption.IgnoreTransform) //use legacy child gameobject
                        {
                            boneMappings.Add(new BoneMapping()
                            {
                                mappingType = BoneMappingType.IgnoreTransform,
                                avatarBonePath = AnimationUtils.GetRelativePath(avatarTrans, settings.targetAvatar.transform),
                                wearableBonePath = AnimationUtils.GetRelativePath(child, settings.targetWearable.transform)
                            });
                        }
                        else if (settings.dynamicsOption == DefaultDresserDynamicsOption.CopyDynamics) //copy dyn bone to clothes bone
                        {
                            boneMappings.Add(new BoneMapping()
                            {
                                mappingType = BoneMappingType.CopyDynamics,
                                avatarBonePath = AnimationUtils.GetRelativePath(avatarTrans, settings.targetAvatar.transform),
                                wearableBonePath = AnimationUtils.GetRelativePath(child, settings.targetWearable.transform)
                            });
                        }
                        else if (settings.dynamicsOption == DefaultDresserDynamicsOption.IgnoreAll) //ignore all
                        {
                            report.LogInfoLocalized(t, DefaultDresser.LogLabel, DefaultDresser.MessageCode.DynamicBoneAllIgnored);
                            boneMappings.Add(new BoneMapping()
                            {
                                mappingType = BoneMappingType.DoNothing,
                                avatarBonePath = AnimationUtils.GetRelativePath(avatarTrans, settings.targetAvatar.transform),
                                wearableBonePath = AnimationUtils.GetRelativePath(child, settings.targetWearable.transform)
                            });
                            //do nothing
                        }
                    }
                    else
                    {
                        boneMappings.Add(new BoneMapping()
                        {
                            mappingType = BoneMappingType.MoveToBone,
                            avatarBonePath = AnimationUtils.GetRelativePath(avatarTrans, settings.targetAvatar.transform),
                            wearableBonePath = AnimationUtils.GetRelativePath(child, settings.targetWearable.transform)
                        });

                        ProcessBone(report, settings, avatarDynamicsList, wearableDynamicsList, level + 1, avatarTrans, child, boneMappings);
                    }
                }
            }
        }

        private static bool IsOnlyOneEnabledChildBone(Transform armature)
        {
            var count = 0;
            for (var i = 0; i < armature.childCount; i++)
            {
                if (armature.GetChild(i).gameObject.activeSelf)
                {
                    count++;
                }
            }
            return count == 1;
        }

        public bool Evaluate(DKReport report, DresserSettings settings, List<BoneMapping> boneMappings)
        {
            var avatarArmature = settings.targetAvatar.transform.Find(settings.avatarArmatureName);
            var wearableArmature = settings.targetWearable.transform.Find(settings.wearableArmatureName);

            if (!avatarArmature)
            {
                //guess the armature object by finding if the object name contains settings.avatarArmatureObjectName, but don't rename it
                avatarArmature = OneConfUtils.GuessArmature(settings.targetAvatar, settings.avatarArmatureName, false);

                if (avatarArmature)
                {
                    report.LogInfoLocalized(t, DefaultDresser.LogLabel, DefaultDresser.MessageCode.AvatarArmatureObjectGuessed, avatarArmature.name);
                }
                else
                {
                    report.LogErrorLocalized(t, DefaultDresser.LogLabel, DefaultDresser.MessageCode.NoArmatureInAvatar);
                }
            }

            if (!wearableArmature)
            {
                //guess the armature object by finding if the object name contains settings.clothesArmatureObjectName and do not rename it
                wearableArmature = OneConfUtils.GuessArmature(settings.targetWearable, settings.wearableArmatureName, false);

                if (wearableArmature)
                {
                    report.LogInfoLocalized(t, DefaultDresser.LogLabel, DefaultDresser.MessageCode.WearableArmatureObjectGuessed, wearableArmature.name);
                }
                else
                {
                    report.LogErrorLocalized(t, DefaultDresser.LogLabel, DefaultDresser.MessageCode.NoArmatureInWearable);
                }
            }

            if (!avatarArmature || !wearableArmature)
            {
                return false;
            }

            // Checking precautions

            if (avatarArmature.childCount == 0)
            {
                report.LogErrorLocalized(t, DefaultDresser.LogLabel, DefaultDresser.MessageCode.NoBonesInAvatarArmatureFirstLevel);
            }

            if (wearableArmature.childCount == 0)
            {
                report.LogErrorLocalized(t, DefaultDresser.LogLabel, DefaultDresser.MessageCode.NoBonesInWearableArmatureFirstLevel);
            }

            if (avatarArmature.childCount == 0 || wearableArmature.childCount == 0)
            {
                return false;
            }

            if (avatarArmature.childCount > 1)
            {
                //only one enabled bone detected, others are disabled (e.g. Maya has a C object that is disabled)
                //otherwise the UI will always just say Compatible but not OK
                if (IsOnlyOneEnabledChildBone(avatarArmature))
                {
                    report.LogInfoLocalized(t, DefaultDresser.LogLabel, DefaultDresser.MessageCode.MultipleBonesInAvatarArmatureDetectedWarningRemoved);
                }
                else
                {
                    report.LogWarnLocalized(t, DefaultDresser.LogLabel, DefaultDresser.MessageCode.MultipleBonesInAvatarArmatureFirstLevel);
                }
            }

            if (wearableArmature.childCount > 1)
            {
                report.LogWarnLocalized(t, DefaultDresser.LogLabel, DefaultDresser.MessageCode.MultipleBonesInWearableArmatureFirstLevel);
            }

            // Scan dynamics
            var avatarDynamicsList = OneConfUtils.ScanAvatarOnlyDynamics(settings.targetAvatar); // TODO remove connections with OneConf
            var wearableDynamicsList = DynamicsUtils.ScanDynamics(settings.targetWearable);

            // Process Armature
            ProcessBone(report, (DefaultDresserSettings)settings, avatarDynamicsList, wearableDynamicsList, 0, avatarArmature, wearableArmature, boneMappings);
            return true;
        }
    }
}
