using System.Collections.Generic;
using Chocopoi.AvatarLib.Animations;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Logging;
using Chocopoi.DressingTools.Proxy;
using UnityEngine;

namespace Chocopoi.DressingTools.Dresser.Default.Hooks
{
    public class ArmatureHook : IDefaultDresserHook
    {
        private static void AddRecursiveDynamicsBindings(Transform targetAvatarRoot, Transform targetWearableRoot, Transform avatarDynamicsRoot, Transform wearableDynamicsRoot, List<DTBoneMapping> boneMappings, DTDynamicsBindingType dynamicsBindingType)
        {
            // add bone mapping

            boneMappings.Add(new DTBoneMapping()
            {
                mappingType = DTBoneMappingType.DoNothing,
                dynamicsBindingType = dynamicsBindingType,
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
                var avatarTrans = avatarDynamicsRoot.Find(child.name);
                if (avatarTrans != null)
                {
                    AddRecursiveDynamicsBindings(targetAvatarRoot, targetWearableRoot, avatarTrans, child, boneMappings, dynamicsBindingType);
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

                var avatarTrans = DressingUtils.GuessMatchingAvatarBone(avatarBoneParent, child.name);

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

                    var avatarDynamics = DressingUtils.FindDynamicsWithRoot(avatarDynamicsList, avatarTrans);
                    var wearableDynamics = DressingUtils.FindDynamicsWithRoot(wearableDynamicsList, child);

                    if (avatarDynamics != null)
                    {
                        if (settings.dynamicsOption == DTDefaultDresserDynamicsOption.RemoveDynamicsAndUseParentConstraint) //remove and use parent constraints
                        {
                            AddRecursiveDynamicsBindings(settings.targetAvatar.transform, settings.targetWearable.transform, avatarTrans, child, boneMappings, DTDynamicsBindingType.ParentConstraint);
                        }
                        else if (settings.dynamicsOption == DTDefaultDresserDynamicsOption.KeepDynamicsAndUseParentConstraintIfNecessary) //keep dynbone and use parentconstraint if necessary
                        {
                            if (wearableDynamics == null)
                            {
                                AddRecursiveDynamicsBindings(settings.targetAvatar.transform, settings.targetWearable.transform, avatarTrans, child, boneMappings, DTDynamicsBindingType.ParentConstraint);
                            }
                        }
                        else if (settings.dynamicsOption == DTDefaultDresserDynamicsOption.IgnoreTransform) //use legacy child gameobject
                        {
                            AddRecursiveDynamicsBindings(settings.targetAvatar.transform, settings.targetWearable.transform, avatarTrans, child, boneMappings, DTDynamicsBindingType.IgnoreTransform);
                        }
                        else if (settings.dynamicsOption == DTDefaultDresserDynamicsOption.CopyDynamics) //copy dyn bone to clothes bone
                        {
                            boneMappings.Add(new DTBoneMapping()
                            {
                                mappingType = DTBoneMappingType.DoNothing,
                                dynamicsBindingType = DTDynamicsBindingType.CopyDynamics,
                                avatarBonePath = AnimationUtils.GetRelativePath(avatarTrans, settings.targetAvatar.transform),
                                wearableBonePath = AnimationUtils.GetRelativePath(child, settings.targetWearable.transform)
                            });
                        }
                        else if (settings.dynamicsOption == DTDefaultDresserDynamicsOption.IgnoreAll) //ignore all
                        {
                            report.LogInfo(DTDefaultDresser.MessageCode.DynamicBoneAllIgnored, "Dynamics all ignored.");
                            boneMappings.Add(new DTBoneMapping()
                            {
                                mappingType = DTBoneMappingType.DoNothing,
                                dynamicsBindingType = DTDynamicsBindingType.DoNothing,
                                avatarBonePath = AnimationUtils.GetRelativePath(avatarTrans, settings.targetAvatar.transform),
                                wearableBonePath = AnimationUtils.GetRelativePath(child, settings.targetWearable.transform)
                            });
                            //do nothing
                        }
                    }
                    else
                    {
                        boneMappings.Add(new DTBoneMapping()
                        {
                            mappingType = DTBoneMappingType.MoveToBone,
                            dynamicsBindingType = DTDynamicsBindingType.DoNothing,
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

        private static void ScanAvatarDynamics(GameObject targetAvatar, GameObject targetWearable, out List<IDynamicsProxy> avatarDynamicsList, out List<IDynamicsProxy> wearableDynamicsList)
        {
            avatarDynamicsList = new List<IDynamicsProxy>();
            wearableDynamicsList = new List<IDynamicsProxy>();

            // TODO: replace by reading YAML

            // get the dynbone type
            var DynamicBoneType = DressingUtils.FindType("DynamicBone");
            var PhysBoneType = DressingUtils.FindType("VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBone");

            // scan avatar dynbones

            if (DynamicBoneType != null)
            {
                var avatarDynBones = targetAvatar.GetComponentsInChildren(DynamicBoneType);
                foreach (var dynBone in avatarDynBones)
                {
                    avatarDynamicsList.Add(new DynamicBoneProxy(dynBone));
                }
            }

            // scan avatar physbones

            if (PhysBoneType != null)
            {
                var avatarPhysBones = targetAvatar.GetComponentsInChildren(PhysBoneType);
                foreach (var physBone in avatarPhysBones)
                {
                    avatarDynamicsList.Add(new PhysBoneProxy(physBone));
                }
            }

            // scan original clothes dynbones

            if (DynamicBoneType != null)
            {
                var wearableDynBones = targetWearable.GetComponentsInChildren(DynamicBoneType);
                foreach (var dynBone in wearableDynBones)
                {
                    wearableDynamicsList.Add(new DynamicBoneProxy(dynBone));
                }
            }

            // scan original clothes physbones

            if (PhysBoneType != null)
            {
                var wearablePhysBones = targetWearable.GetComponentsInChildren(PhysBoneType);
                foreach (var physBone in wearablePhysBones)
                {
                    wearableDynamicsList.Add(new PhysBoneProxy(physBone));
                }
            }
        }

        public bool Evaluate(DTReport report, DTDresserSettings settings, List<DTBoneMapping> boneMappings)
        {
            var avatarArmature = settings.targetAvatar.transform.Find(settings.avatarArmatureName);
            var wearableArmature = settings.targetWearable.transform.Find(settings.wearableArmatureName);

            if (!avatarArmature)
            {
                //guess the armature object by finding if the object name contains settings.avatarArmatureObjectName, but don't rename it
                avatarArmature = DressingUtils.GuessArmature(settings.targetAvatar, settings.avatarArmatureName, false);

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
                wearableArmature = DressingUtils.GuessArmature(settings.targetWearable, settings.wearableArmatureName, false);

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

            if (avatarArmature.childCount == 0 || avatarArmature.childCount == 0)
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
            ScanAvatarDynamics(settings.targetAvatar, settings.targetWearable, out var avatarDynamicsList, out var wearableDynamicsList);

            // Process Armature
            ProcessBone(report, (DTDefaultDresserSettings)settings, avatarDynamicsList, wearableDynamicsList, 0, avatarArmature, wearableArmature, boneMappings);
            return true;
        }
    }
}
