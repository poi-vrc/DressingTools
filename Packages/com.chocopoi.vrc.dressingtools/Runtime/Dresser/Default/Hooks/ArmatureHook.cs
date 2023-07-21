using System.Collections.Generic;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Logging;
using Chocopoi.DressingTools.Proxy;
using UnityEngine;

namespace Chocopoi.DressingTools.Dresser.Default.Hooks
{
    public class ArmatureHook : IDefaultDresserHook
    {
        private static void AddRecursiveDynamicsBindings(Transform targetAvatarRoot, Transform targetWearableRoot, Transform avatarDynamicsRoot, Transform wearableDynamicsRoot, List<DTBoneMapping> boneMappings, DTBoneMappingType bindingType)
        {
            // add bone mapping

            boneMappings.Add(new DTBoneMapping()
            {
                mappingType = bindingType,
                avatarBonePath = DTRuntimeUtils.GetRelativePath(avatarDynamicsRoot, targetAvatarRoot),
                wearableBonePath = DTRuntimeUtils.GetRelativePath(wearableDynamicsRoot, targetWearableRoot)
            });

            // scan for other child bones

            var childs = new List<Transform>();

            for (var i = 0; i < wearableDynamicsRoot.childCount; i++)
            {
                childs.Add(wearableDynamicsRoot.GetChild(i));
            }

            foreach (var child in childs)
            {
                var avatarTrans = avatarDynamicsRoot.Find(child.name);
                if (avatarTrans != null)
                {
                    AddRecursiveDynamicsBindings(targetAvatarRoot, targetWearableRoot, avatarTrans, child, boneMappings, bindingType);
                }
            }
        }

        private static void ProcessBone(DTReport report, DTDefaultDresserSettings settings, List<IDynamicsProxy> avatarDynamicsList, List<IDynamicsProxy> wearableDynamicsList, int level, Transform avatarBoneParent, Transform clothesBoneParent, List<DTBoneMapping> boneMappings)
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

                var avatarTrans = DTRuntimeUtils.GuessMatchingAvatarBone(avatarBoneParent, child.name);

                if (avatarTrans == null)
                {
                    if (level == 0)
                    {
                        report.LogWarn(DTDefaultDresser.MessageCode.BonesNotMatchingInArmatureFirstLevel, "Bones not matching in armature first level.");
                        continue;
                    }
                    else
                    {
                        report.LogInfo(DTDefaultDresser.MessageCode.NonMatchingWearableBoneKeptUntouched, "Non-matching wearable bone kept untouched");
                    }
                }
                else
                {
                    // Find whether there is a DynamicBone/PhysBone component controlling the bone

                    var avatarDynamics = DTRuntimeUtils.FindDynamicsWithRoot(avatarDynamicsList, avatarTrans);
                    var wearableDynamics = DTRuntimeUtils.FindDynamicsWithRoot(wearableDynamicsList, child);

                    if (avatarDynamics != null)
                    {
                        if (settings.dynamicsOption == DTDefaultDresserDynamicsOption.RemoveDynamicsAndUseParentConstraint) //remove and use parent constraints
                        {
                            AddRecursiveDynamicsBindings(settings.targetAvatar.transform, settings.targetWearable.transform, avatarTrans, child, boneMappings, DTBoneMappingType.ParentConstraint);
                        }
                        else if (settings.dynamicsOption == DTDefaultDresserDynamicsOption.KeepDynamicsAndUseParentConstraintIfNecessary) //keep dynbone and use parentconstraint if necessary
                        {
                            if (wearableDynamics == null)
                            {
                                AddRecursiveDynamicsBindings(settings.targetAvatar.transform, settings.targetWearable.transform, avatarTrans, child, boneMappings, DTBoneMappingType.ParentConstraint);
                            }
                        }
                        else if (settings.dynamicsOption == DTDefaultDresserDynamicsOption.IgnoreTransform) //use legacy child gameobject
                        {
                            boneMappings.Add(new DTBoneMapping()
                            {
                                mappingType = DTBoneMappingType.IgnoreTransform,
                                avatarBonePath = DTRuntimeUtils.GetRelativePath(avatarTrans, settings.targetAvatar.transform),
                                wearableBonePath = DTRuntimeUtils.GetRelativePath(child, settings.targetWearable.transform)
                            });
                        }
                        else if (settings.dynamicsOption == DTDefaultDresserDynamicsOption.CopyDynamics) //copy dyn bone to clothes bone
                        {
                            boneMappings.Add(new DTBoneMapping()
                            {
                                mappingType = DTBoneMappingType.CopyDynamics,
                                avatarBonePath = DTRuntimeUtils.GetRelativePath(avatarTrans, settings.targetAvatar.transform),
                                wearableBonePath = DTRuntimeUtils.GetRelativePath(child, settings.targetWearable.transform)
                            });
                        }
                        else if (settings.dynamicsOption == DTDefaultDresserDynamicsOption.IgnoreAll) //ignore all
                        {
                            report.LogInfo(DTDefaultDresser.MessageCode.DynamicBoneAllIgnored, "Dynamics all ignored.");
                            boneMappings.Add(new DTBoneMapping()
                            {
                                mappingType = DTBoneMappingType.DoNothing,
                                avatarBonePath = DTRuntimeUtils.GetRelativePath(avatarTrans, settings.targetAvatar.transform),
                                wearableBonePath = DTRuntimeUtils.GetRelativePath(child, settings.targetWearable.transform)
                            });
                            //do nothing
                        }
                    }
                    else
                    {
                        boneMappings.Add(new DTBoneMapping()
                        {
                            mappingType = DTBoneMappingType.MoveToBone,
                            avatarBonePath = DTRuntimeUtils.GetRelativePath(avatarTrans, settings.targetAvatar.transform),
                            wearableBonePath = DTRuntimeUtils.GetRelativePath(child, settings.targetWearable.transform)
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

        public bool Evaluate(DTReport report, DTDresserSettings settings, List<DTBoneMapping> boneMappings)
        {
            var avatarArmature = settings.targetAvatar.transform.Find(settings.avatarArmatureName);
            var wearableArmature = settings.targetWearable.transform.Find(settings.wearableArmatureName);

            if (!avatarArmature)
            {
                //guess the armature object by finding if the object name contains settings.avatarArmatureObjectName, but don't rename it
                avatarArmature = DTRuntimeUtils.GuessArmature(settings.targetAvatar, settings.avatarArmatureName, false);

                if (avatarArmature)
                {
                    report.LogInfo(DTDefaultDresser.MessageCode.AvatarArmatureObjectGuessed, "Avatar armature object guessed.");
                }
                else
                {
                    report.LogError(DTDefaultDresser.MessageCode.NoArmatureInAvatar, "No armature in avatar.");
                }
            }

            if (!wearableArmature)
            {
                //guess the armature object by finding if the object name contains settings.clothesArmatureObjectName and do not rename it
                wearableArmature = DTRuntimeUtils.GuessArmature(settings.targetWearable, settings.wearableArmatureName, false);

                if (wearableArmature)
                {
                    report.LogInfo(DTDefaultDresser.MessageCode.WearableArmatureObjectGuessed, "Wearable armature object guessed.");
                }
                else
                {
                    report.LogError(DTDefaultDresser.MessageCode.NoArmatureInWearable, "No armature in wearable.");
                }
            }

            if (!avatarArmature || !wearableArmature)
            {
                return false;
            }

            // Checking precautions

            if (avatarArmature.childCount == 0)
            {
                report.LogError(DTDefaultDresser.MessageCode.NoBonesInAvatarArmatureFirstLevel, "No bones in avatar armature first level.");
            }

            if (wearableArmature.childCount == 0)
            {
                report.LogError(DTDefaultDresser.MessageCode.NoBonesInWearableArmatureFirstLevel, "No bones in wearable armature first level.");
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
                    report.LogInfo(DTDefaultDresser.MessageCode.MultipleBonesInAvatarArmatureDetectedWarningRemoved, "Multiple bones in avatar armature first level detected.");
                }
                else
                {
                    report.LogWarn(DTDefaultDresser.MessageCode.MultipleBonesInAvatarArmatureFirstLevel, "Multiple bones in avatar armature first level detected.");
                }
            }

            if (wearableArmature.childCount > 1)
            {
                report.LogWarn(DTDefaultDresser.MessageCode.MultipleBonesInWearableArmatureFirstLevel, "Multiple bones in wearable armature first level detected.");
            }

            // Scan dynamics
            var avatarDynamicsList = DTRuntimeUtils.ScanDynamics(settings.targetAvatar);
            var wearableDynamicsList = DTRuntimeUtils.ScanDynamics(settings.targetWearable);

            // Process Armature
            ProcessBone(report, (DTDefaultDresserSettings)settings, avatarDynamicsList, wearableDynamicsList, 0, avatarArmature, wearableArmature, boneMappings);
            return true;
        }
    }
}
