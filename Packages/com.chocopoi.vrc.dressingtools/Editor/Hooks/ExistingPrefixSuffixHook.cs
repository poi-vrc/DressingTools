using Chocopoi.DressingTools.Reporting;
using UnityEngine;

namespace Chocopoi.DressingTools.Hooks
{
    public class ExistingPrefixSuffixHook : IDressHook
    {
        public void ProcessBone(DressReport report, DressSettings settings, Transform boneParent)
        {
            for (var i = 0; i < boneParent.childCount; i++)
            {
                var child = boneParent.GetChild(i);

                // check if there is a prefix
                if (child.name.StartsWith("("))
                {
                    //find the first closing bracket
                    var prefixBracketEnd = child.name.IndexOf(")");
                    if (prefixBracketEnd != -1 && prefixBracketEnd != child.name.Length - 1) //remove it if there is
                    {
                        if (settings.removeExistingPrefixSuffix)
                        {
                            child.name = child.name.Substring(prefixBracketEnd + 1).Trim();
                            report.infos |= DressCheckCodeMask.Info.EXISTING_PREFIX_DETECTED_AND_REMOVED;
                        }
                        else
                        {
                            report.infos |= DressCheckCodeMask.Info.EXISTING_PREFIX_DETECTED_NOT_REMOVED;
                        }
                    }
                }

                // check if there is a suffix
                if (child.name.EndsWith(")"))
                {
                    //find the first closing bracket
                    var suffixBracketStart = child.name.LastIndexOf("(");
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

        public bool Evaluate(DressReport report, DressSettings settings, GameObject targetAvatar, GameObject targetClothes)
        {
            var clothesArmature = targetClothes.transform.Find(settings.clothesArmatureObjectName);

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

            ProcessBone(report, settings, clothesArmature);
            return true;
        }
    }
}
