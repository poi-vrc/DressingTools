﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

namespace Chocopoi.DressingTools
{
    public class ArmatureRule : IDressCheckRule
    {
        private bool ProcessBone(DressReport report, DressSettings settings, int level, Transform avatarBoneParent, Transform clothesBoneParent)
        {
            List<Transform> childs = new List<Transform>();

            for (int i = 0; i < clothesBoneParent.childCount; i++)
            {
                childs.Add(clothesBoneParent.GetChild(i));
            }

            foreach (Transform child in childs)
            {
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
                    // Find whether there is a DynamicBone component in the bone

                    DynamicBone avatarDynBone = avatarTrans.GetComponent<DynamicBone>();
                    DynamicBone childDynBone = child.GetComponent<DynamicBone>();

                    if (avatarDynBone != null && avatarDynBone.m_Root.Equals(avatarTrans))
                    {
                        if (settings.dynamicBoneOption == 0) //remove and use parent constraints
                        {
                            if (childDynBone != null)
                            {
                                Object.DestroyImmediate(childDynBone);
                            }

                            ParentConstraint comp = child.gameObject.AddComponent<ParentConstraint>();
                            comp.constraintActive = true;

                            ConstraintSource source = new ConstraintSource
                            {
                                sourceTransform = avatarTrans,
                                weight = 1
                            };
                            comp.AddSource(source);
                        }
                        else if (settings.dynamicBoneOption == 1) //keep dynbone and use parentconstraint if necessary
                        {
                            if (childDynBone == null)
                            {
                                ParentConstraint comp = child.gameObject.AddComponent<ParentConstraint>();
                                comp.constraintActive = true;

                                ConstraintSource source = new ConstraintSource
                                {
                                    sourceTransform = avatarTrans,
                                    weight = 1
                                };
                                comp.AddSource(source);
                            }
                        }
                        else if (settings.dynamicBoneOption == 2) //use legacy child gameobject
                        {
                            string name = avatarTrans.name + "_DBExcluded";
                            GameObject dynBoneChild = avatarTrans.Find(name)?.gameObject;

                            if (dynBoneChild == null)
                            {
                                dynBoneChild = new GameObject(name);
                                dynBoneChild.transform.SetParent(avatarTrans);
                            }

                            //verify if it is excluded

                            if (!avatarDynBone.m_Exclusions.Contains(dynBoneChild.transform))
                            {
                                avatarDynBone.m_Exclusions.Add(dynBoneChild.transform);
                            }

                            // destroy the child dyn bone component

                            if (childDynBone != null)
                            {
                                Object.DestroyImmediate(childDynBone);
                            }

                            child.name = settings.prefixToBeAdded + child.name + settings.suffixToBeAdded;
                            child.SetParent(dynBoneChild.transform);
                        }
                        else if (settings.dynamicBoneOption == 3) //copy dyn bone to clothes bone
                        {
                            //destroy the existing dyn bone
                            if (childDynBone != null)
                            {
                                Object.DestroyImmediate(childDynBone);
                            }

                            //copy component using unityeditor internal method (easiest way)
                            UnityEditorInternal.ComponentUtility.CopyComponent(avatarDynBone);
                            UnityEditorInternal.ComponentUtility.PasteComponentAsNew(child.gameObject);
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
                        child.transform.SetParent(avatarTrans);
                        child.name = settings.prefixToBeAdded + child.name + settings.suffixToBeAdded;
                    }

                    if (!ProcessBone(report, settings, level + 1, avatarTrans, child))
                    {
                        return false;
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
            Transform avatarArmature = targetAvatar.transform.Find("Armature");
            Transform clothesArmature = targetClothes.transform.Find("Armature");

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
