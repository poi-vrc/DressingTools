using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;

namespace Chocopoi.DressingTools
{
    public class FindAvatarDynamicBonesRule : IDressCheckRule
    {
        public bool Evaluate(DressReport report, DressSettings settings, GameObject targetAvatar, GameObject targetClothes)
        {
            Transform avatarArmature = targetAvatar.transform.Find("Armature");

            if (!avatarArmature)
            {
                report.errors |= DressCheckCodeMask.Error.NO_ARMATURE_IN_AVATAR;
                return false;
            }

            // scan avatar dynbones

            DynamicBone[] avatarDynBones = targetAvatar.GetComponentsInChildren<DynamicBone>();
            foreach (DynamicBone dynBone in avatarDynBones)
            {
                report.avatarDynBones.Add(dynBone);
            }

            return true;
        }
    }
}
