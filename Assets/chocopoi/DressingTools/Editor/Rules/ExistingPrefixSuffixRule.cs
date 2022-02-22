using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Chocopoi.DressingTools
{
    public class ExistingPrefixSuffixRule : IDressCheckRule
    {
        public void ProcessBone(DressReport report, DressSettings settings, Transform boneParent)
        {
            for (int i = 0; i < boneParent.childCount; i++)
            {
                Transform child = boneParent.GetChild(i);

                // check if there is a prefix
                if (child.name.StartsWith("("))
                {
                    //find the first closing bracket
                    int prefixBracketEnd = child.name.IndexOf(")");
                    if (prefixBracketEnd != -1 && prefixBracketEnd != child.name.Length - 1) //remove it if there is
                    {
                        if (settings.removeExistingPrefixSuffix)
                        {
                            child.name = child.name.Substring(prefixBracketEnd + 1).Trim();
                            report.infos |= DressCheckCodeMask.Info.EXISTING_PREFIX_DETECTED_AND_REMOVED;
                        } else
                        {
                            report.infos |= DressCheckCodeMask.Info.EXISTING_PREFIX_DETECTED_NOT_REMOVED;
                        }
                    }
                }

                // check if there is a suffix
                if (child.name.EndsWith(")"))
                {
                    //find the first closing bracket
                    int suffixBracketStart = child.name.LastIndexOf("(");
                    if (suffixBracketStart != -1 && suffixBracketStart != 0) //remove it if there is
                    {
                        if (settings.removeExistingPrefixSuffix)
                        {
                            child.name = child.name.Substring(0, suffixBracketStart).Trim();
                            report.infos |= DressCheckCodeMask.Info.EXISTING_SUFFIX_DETECTED_AND_REMOVED;
                        }
                        else
                        {
                            report.infos |= DressCheckCodeMask.Info.EXISTING_SUFFIX_DETECTED_NOT_REMOVED;
                        }
                    }
                }

                ProcessBone(report, settings, child);
            }
        }

        public Transform GuessArmature(GameObject targetClothes)
        {
            List<Transform> transforms = new List<Transform>();

            for (int i = 0; i < targetClothes.transform.childCount; i++)
            {
                Transform child = targetClothes.transform.GetChild(i);

                if (child.name.Contains("Armature"))
                {
                    transforms.Add(child);
                }
            }

            if (transforms.Count == 1)
            {
                transforms[0].name = "Armature";
                return transforms[0];
            } else
            {
                return null;
            }
        }

        public bool Evaluate(DressReport report, DressSettings settings, GameObject targetAvatar, GameObject targetClothes)
        {
            Transform clothesArmature = targetClothes.transform.Find("Armature");

            if (!clothesArmature)
            {
                //guess the armature object by finding if the object name contains "Armature" and rename it
                clothesArmature = GuessArmature(targetClothes);

                if (clothesArmature)
                {
                    report.infos |= DressCheckCodeMask.Info.ARMATURE_OBJECT_GUESSED;
                } else
                {
                    report.errors |= DressCheckCodeMask.Error.NO_ARMATURE_IN_CLOTHES;
                    return false;
                }
            }

            ProcessBone(report, settings, clothesArmature);
            return true;
        }
    }
}
