using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

namespace Chocopoi.DressingTools
{
    public class FindClothesDynamicBonesRule : IDressCheckRule
    {
        private static bool IsSameDynamicBoneExistsInAvatar(List<DynamicBone> avatarDynBones, DynamicBone target)
        {
            foreach (DynamicBone bone in avatarDynBones)
            {
                if (bone.m_Root == target.m_Root)
                {
                    return true;
                }
            }
            return false;
        }

        public bool Evaluate(DressReport report, DressSettings settings, GameObject targetAvatar, GameObject targetClothes)
        {
            Transform avatarArmature = targetAvatar.transform.Find("Armature");

            if (!avatarArmature)
            {
                report.errors |= DressCheckCodeMask.Error.NO_ARMATURE_IN_AVATAR;
                return false;
            }

            // scan clothes dynbones

            DynamicBone[] clothesDynBones = targetAvatar.GetComponentsInChildren<DynamicBone>();
            foreach (DynamicBone dynBone in clothesDynBones)
            {
                if (!IsSameDynamicBoneExistsInAvatar(report.avatarDynBones, dynBone))
                {
                    report.clothesDynBones.Add(dynBone);
                }
            }

            return true;
        }
    }
}
