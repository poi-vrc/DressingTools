using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace Chocopoi.DressingTools
{
    public class FindAvatarDynamicsRule : IDressCheckRule
    {
        public bool Evaluate(DressReport report, DressSettings settings, GameObject targetAvatar, GameObject targetClothes)
        {
            Transform avatarArmature = targetAvatar.transform.Find(settings.armatureObjectName);

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

            // scan avatar physbones

            VRCPhysBone[] avatarPhysBones = targetAvatar.GetComponentsInChildren<VRCPhysBone>();
            foreach (VRCPhysBone physBone in avatarPhysBones)
            {
                report.avatarPhysBones.Add(physBone);
            }

            // scan original clothes dynbones

            DynamicBone[] clothesDynBones = targetClothes.GetComponentsInChildren<DynamicBone>();
            foreach (DynamicBone dynBone in clothesDynBones)
            {
                report.clothesOriginalDynBones.Add(dynBone);
            }

            // scan original clothes physbones

            VRCPhysBone[] clothesPhysBones = targetClothes.GetComponentsInChildren<VRCPhysBone>();
            foreach (VRCPhysBone physBone in avatarPhysBones)
            {
                report.clothesOriginalPhysBones.Add(physBone);
            }

            return true;
        }
    }
}
