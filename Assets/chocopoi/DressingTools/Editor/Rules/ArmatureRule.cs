using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace Chocopoi.DressingTools
{
    public class ArmatureRule : IDressCheckRule
    {
        private void AddRecursiveDynamicsParentConstraints(Transform avatarDynamicsRoot, Transform clothesDynamicsRoot)
        {
            // add parent constraint

            ParentConstraint comp = clothesDynamicsRoot.gameObject.AddComponent<ParentConstraint>();
            comp.constraintActive = true;

            ConstraintSource source = new ConstraintSource
            {
                sourceTransform = avatarDynamicsRoot,
                weight = 1
            };
            comp.AddSource(source);

            // scan for other child bones

            List<Transform> childs = new List<Transform>();

            for (int i = 0; i < clothesDynamicsRoot.childCount; i++)
            {
                childs.Add(clothesDynamicsRoot.GetChild(i));
            }

            foreach (Transform child in childs)
            {
                Transform avatarTrans = avatarDynamicsRoot.Find(child.name);
                if (avatarTrans != null)
                {
                    AddRecursiveDynamicsParentConstraints(avatarTrans, child);
                }
            }
        }

        private void AddRecursiveIgnoreTransforms(DressSettings settings, DynamicBone avatarDynBone, VRCPhysBone avatarPhysBone, Transform avatarDynamicsRoot, Transform clothesDynamicsRoot)
        {
            string name = avatarDynamicsRoot.name + "_DBExcluded";
            GameObject dynBoneChild = avatarDynamicsRoot.Find(name)?.gameObject;

            if (dynBoneChild == null)
            {
                dynBoneChild = new GameObject(name);
                dynBoneChild.transform.SetParent(avatarDynamicsRoot);
            }

            //verify if it is excluded

            if (avatarDynBone != null && !avatarDynBone.m_Exclusions.Contains(dynBoneChild.transform))
            {
                avatarDynBone.m_Exclusions.Add(dynBoneChild.transform);
            }

            if (avatarPhysBone != null && !avatarPhysBone.ignoreTransforms.Contains(dynBoneChild.transform))
            {
                avatarPhysBone.ignoreTransforms.Add(dynBoneChild.transform);
            }

            clothesDynamicsRoot.name = settings.prefixToBeAdded + clothesDynamicsRoot.name + settings.suffixToBeAdded;
            clothesDynamicsRoot.SetParent(dynBoneChild.transform);

            // scan for other child bones

            List<Transform> childs = new List<Transform>();

            for (int i = 0; i < clothesDynamicsRoot.childCount; i++)
            {
                childs.Add(clothesDynamicsRoot.GetChild(i));
            }

            foreach (Transform child in childs)
            {
                Transform avatarTrans = avatarDynamicsRoot.Find(child.name);
                if (avatarTrans != null)
                {
                    AddRecursiveIgnoreTransforms(settings, avatarDynBone, avatarPhysBone, avatarTrans, child);
                }
            }
        }

        private bool ProcessBone(DressReport report, DressSettings settings, int level, Transform avatarBoneParent, Transform clothesBoneParent)
        {
            List<Transform> childs = new List<Transform>();

            for (int i = 0; i < clothesBoneParent.childCount; i++)
            {
                childs.Add(clothesBoneParent.GetChild(i));
            }

            foreach (Transform child in childs)
            {
                report.clothesAllObjects.Add(child.gameObject);

                Transform avatarTrans = avatarBoneParent.Find(child.name);

                if (avatarTrans == null)
                {
                    if (level == 0)
                    {
                        report.warnings |= DressCheckCodeMask.Warn.BONES_NOT_MATCHING_IN_ARMATURE_FIRST_LEVEL;
                        continue;
                    }
                    else
                    {
                        report.infos |= DressCheckCodeMask.Info.NON_MATCHING_CLOTHES_BONE_KEPT_UNTOUCHED;
                    }
                }
                else
                {
                    // Find whether there is a DynamicBone/PhysBone component controlling the bone
                    
                    DynamicBone avatarDynBone = DressingUtils.FindDynBoneWithRoot(report.avatarDynBones, avatarTrans);
                    VRCPhysBone avatarPhysBone = DressingUtils.FindPhysBoneWithRoot(report.avatarPhysBones, avatarTrans);

                    DynamicBone clothesDynBone = DressingUtils.FindDynBoneWithRoot(report.clothesOriginalDynBones, child);
                    VRCPhysBone clothesPhysBone = DressingUtils.FindPhysBoneWithRoot(report.clothesOriginalPhysBones, child);

                    if (avatarDynBone != null || avatarPhysBone != null)
                    {
                        if (settings.dynamicBoneOption == 0) //remove and use parent constraints
                        {
                            if (clothesDynBone != null)
                            {
                                Object.DestroyImmediate(clothesDynBone);
                            }

                            if (clothesPhysBone != null)
                            {
                                Object.DestroyImmediate(clothesPhysBone);
                            }

                            AddRecursiveDynamicsParentConstraints(avatarTrans, child);
                        }
                        else if (settings.dynamicBoneOption == 1) //keep dynbone and use parentconstraint if necessary
                        {
                            if (clothesDynBone == null && clothesPhysBone == null)
                            {
                                AddRecursiveDynamicsParentConstraints(avatarTrans, child);
                            }
                        }
                        else if (settings.dynamicBoneOption == 2) //use legacy child gameobject
                        {
                            // destroy the child dynbone / physbone component

                            if (clothesDynBone != null)
                            {
                                Object.DestroyImmediate(clothesDynBone);
                            }

                            if (clothesPhysBone != null)
                            {
                                Object.DestroyImmediate(clothesPhysBone);
                            }

                            AddRecursiveIgnoreTransforms(settings, avatarDynBone, avatarPhysBone, avatarTrans, child);
                        }
                        else if (settings.dynamicBoneOption == 3) //copy dyn bone to clothes bone
                        {
                            //destroy the existing dynbone / physbone

                            if (clothesDynBone != null)
                            {
                                Object.DestroyImmediate(clothesDynBone);
                            }

                            if (clothesPhysBone != null)
                            {
                                Object.DestroyImmediate(clothesPhysBone);
                            }

                            //copy component using unityeditor internal method (easiest way)

                            if (avatarDynBone != null)
                            {
                                UnityEditorInternal.ComponentUtility.CopyComponent(avatarDynBone);
                                UnityEditorInternal.ComponentUtility.PasteComponentAsNew(child.gameObject);

                                DynamicBone copiedDb = child.GetComponent<DynamicBone>();
                                copiedDb.m_Root = child;
                            }

                            if (avatarPhysBone != null)
                            {
                                UnityEditorInternal.ComponentUtility.CopyComponent(avatarPhysBone);
                                UnityEditorInternal.ComponentUtility.PasteComponentAsNew(child.gameObject);

                                VRCPhysBone copiedPb = child.GetComponent<VRCPhysBone>();
                                copiedPb.rootTransform = child;
                            }
                        }
                        else if (settings.dynamicBoneOption == 4) //ignore all
                        {
                            report.infos |= DressCheckCodeMask.Info.DYNAMIC_BONE_ALL_IGNORED;
                            child.name = settings.prefixToBeAdded + child.name + settings.suffixToBeAdded;
                            //do nothing
                        }
                    }
                    else
                    {
                        Transform clothesBoneContainerTrans = null;

                        if (settings.groupBones)
                        {
                            string name = avatarTrans.name + "_DT";
                            GameObject clothesBoneContainer = avatarTrans.Find(name)?.gameObject;

                            if (clothesBoneContainer == null)
                            {
                                clothesBoneContainer = new GameObject(name);
                                clothesBoneContainer.transform.SetParent(avatarTrans);
                            }

                            clothesBoneContainerTrans = clothesBoneContainer.transform;
                        } else
                        {
                            clothesBoneContainerTrans = avatarTrans;
                        }

                        child.transform.SetParent(clothesBoneContainerTrans);
                        child.name = settings.prefixToBeAdded + child.name + settings.suffixToBeAdded;

                        if (!ProcessBone(report, settings, level + 1, avatarTrans, child))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        public bool IsOnlyOneEnabledChildBone(Transform armature)
        {
            int count = 0;
            for (int i = 0; i < armature.childCount; i++)
            {
                if (armature.GetChild(i).gameObject.activeSelf)
                {
                    count++;
                }
            }
            return count == 1;
        }

        public bool Evaluate(DressReport report, DressSettings settings, GameObject targetAvatar, GameObject targetClothes)
        {
            Transform avatarArmature = targetAvatar.transform.Find(settings.armatureObjectName);
            Transform clothesArmature = targetClothes.transform.Find(settings.armatureObjectName);

            if (!avatarArmature)
            {
                report.errors |= DressCheckCodeMask.Error.NO_ARMATURE_IN_AVATAR;
                return false;
            }

            if (!clothesArmature)
            {
                report.errors |= DressCheckCodeMask.Error.NO_ARMATURE_IN_CLOTHES;
                return false;
            }

            // Checking precautions

            if (avatarArmature.childCount == 0)
            {
                report.errors |= DressCheckCodeMask.Error.NO_BONES_IN_AVATAR_ARMATURE_FIRST_LEVEL;
            }

            if (clothesArmature.childCount == 0)
            {
                report.errors |= DressCheckCodeMask.Error.NO_BONES_IN_CLOTHES_ARMATURE_FIRST_LEVEL;
            }

            if (avatarArmature.childCount > 1)
            {
                //only one enabled bone detected, others are disabled (e.g. Maya has a C object that is disabled)
                //otherwise the UI will always just say Compatible but not OK
                if (IsOnlyOneEnabledChildBone(avatarArmature))
                {
                    report.infos |= DressCheckCodeMask.Info.MULTIPLE_BONES_IN_AVATAR_ARMATURE_FIRST_LEVEL_WARNING_REMOVED;
                }
                else
                {
                    report.warnings |= DressCheckCodeMask.Warn.MULTIPLE_BONES_IN_AVATAR_ARMATURE_FIRST_LEVEL;
                }
            }

            if (clothesArmature.childCount > 1)
            {
                report.warnings |= DressCheckCodeMask.Warn.MULTIPLE_BONES_IN_CLOTHES_ARMATURE_FIRST_LEVEL;
            }

            // Process Armature

            return ProcessBone(report, settings, 0, avatarArmature, clothesArmature);
        }
    }
}
