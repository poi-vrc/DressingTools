using System.Collections;
using System.Collections.Generic;
using Chocopoi.DressingTools.Proxy;
using Chocopoi.DressingTools.Reporting;
using UnityEngine;
using UnityEngine.Animations;

namespace Chocopoi.DressingTools.Hooks
{
    public class ArmatureHook : IDressHook
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

        private void AddRecursiveIgnoreTransforms(DressSettings settings, IDynamicsProxy dynamics, Transform avatarDynamicsRoot, Transform clothesDynamicsRoot)
        {
            string name = avatarDynamicsRoot.name + "_DBExcluded";
            GameObject dynBoneChild = avatarDynamicsRoot.Find(name)?.gameObject;

            if (dynBoneChild == null)
            {
                dynBoneChild = new GameObject(name);
                dynBoneChild.transform.SetParent(avatarDynamicsRoot);
            }

            //verify if it is excluded

            if (dynamics != null && !dynamics.IgnoreTransforms.Contains(dynBoneChild.transform))
            {
                dynamics.IgnoreTransforms.Add(dynBoneChild.transform);
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
                    AddRecursiveIgnoreTransforms(settings, dynamics, avatarTrans, child);
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
                // skip our container
                if (child.name.EndsWith("_DT"))
                {
                    continue;
                }

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

                    IDynamicsProxy avatarDynamics = DressingUtils.FindDynamicsWithRoot(report.avatarDynamics, avatarTrans);
                    IDynamicsProxy clothesDynamics = DressingUtils.FindDynamicsWithRoot(report.clothesOriginalDynamics, child);

                    if (avatarDynamics != null)
                    {
                        if (settings.dynamicBoneOption == 0) //remove and use parent constraints
                        {
                            if (clothesDynamics != null)
                            {
                                Object.DestroyImmediate(clothesDynamics.Component);
                            }

                            AddRecursiveDynamicsParentConstraints(avatarTrans, child);
                        }
                        else if (settings.dynamicBoneOption == 1) //keep dynbone and use parentconstraint if necessary
                        {
                            if (clothesDynamics == null)
                            {
                                AddRecursiveDynamicsParentConstraints(avatarTrans, child);
                            }
                        }
                        else if (settings.dynamicBoneOption == 2) //use legacy child gameobject
                        {
                            // destroy the child dynbone / physbone component

                            if (clothesDynamics != null)
                            {
                                Object.DestroyImmediate(clothesDynamics.Component);
                            }

                            AddRecursiveIgnoreTransforms(settings, avatarDynamics, avatarTrans, child);
                        }
                        else if (settings.dynamicBoneOption == 3) //copy dyn bone to clothes bone
                        {
                            //destroy the existing dynbone / physbone

                            if (clothesDynamics != null)
                            {
                                Object.DestroyImmediate(clothesDynamics.Component);
                            }

                            //copy component using unityeditor internal method (easiest way)

                            if (avatarDynamics != null)
                            {
                                // get the dynamics type
                                System.Type DynamicsType = null;

                                // TODO: move these to DressingUtils
                                if (avatarDynamics is DynamicBoneProxy)
                                {
                                    DynamicsType = DressingUtils.FindType("DynamicBone");
                                } else if (avatarDynamics is PhysBoneProxy)
                                {
                                    DynamicsType = DressingUtils.FindType("VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBone");
                                }

                                if (DynamicsType != null)
                                {
                                    UnityEditorInternal.ComponentUtility.CopyComponent(avatarDynamics.Component);
                                    UnityEditorInternal.ComponentUtility.PasteComponentAsNew(child.gameObject);

                                    if (avatarDynamics is DynamicBoneProxy)
                                    {
                                        DynamicBoneProxy copiedDb = new DynamicBoneProxy(child.GetComponent(DynamicsType))
                                        {
                                            RootTransform = child
                                        };
                                    }
                                    else if (avatarDynamics is PhysBoneProxy)
                                    {
                                        PhysBoneProxy copiedPb = new PhysBoneProxy(child.GetComponent(DynamicsType))
                                        {
                                            RootTransform = child
                                        };
                                    }
                                }
                                else
                                {
                                    Debug.LogError("[DressingTools] Cannot copy component without DynamicBone/PhysBone installed in project!");
                                    return false;
                                }
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
                        }
                        else
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
            Transform avatarArmature = targetAvatar.transform.Find(settings.avatarArmatureObjectName);
            Transform clothesArmature = targetClothes.transform.Find(settings.clothesArmatureObjectName);

            if (!avatarArmature)
            {
                //guess the armature object by finding if the object name contains settings.avatarArmatureObjectName, but don't rename it
                avatarArmature = DressingUtils.GuessArmature(targetAvatar, settings.avatarArmatureObjectName, false);

                if (avatarArmature)
                {
                    report.infos |= DressCheckCodeMask.Info.AVATAR_ARMATURE_OBJECT_GUESSED;
                }
                else
                {
                    report.errors |= DressCheckCodeMask.Error.NO_ARMATURE_IN_AVATAR;
                    return false;
                }
            }

            if (!clothesArmature)
            {
                //guess the armature object by finding if the object name contains settings.clothesArmatureObjectName and rename it
                clothesArmature = DressingUtils.GuessArmature(targetClothes, settings.clothesArmatureObjectName, true);

                if (clothesArmature)
                {
                    report.infos |= DressCheckCodeMask.Info.CLOTHES_ARMATURE_OBJECT_GUESSED;
                }
                else
                {
                    report.errors |= DressCheckCodeMask.Error.NO_ARMATURE_IN_CLOTHES;
                    return false;
                }
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
