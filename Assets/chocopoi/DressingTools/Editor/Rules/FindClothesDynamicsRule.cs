using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace Chocopoi.DressingTools
{
    public class FindClothesDynamicsRule : IDressCheckRule
    {
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
                if (!DressingUtils.IsDynBoneExists(report.avatarDynBones, dynBone.m_Root))
                {
                    report.clothesDynBones.Add(dynBone);
                }
            }

            // scan clothes physbones

            VRCPhysBone[] clothesPhysBones = targetAvatar.GetComponentsInChildren<VRCPhysBone>();
            foreach (VRCPhysBone physBone in clothesPhysBones)
            {
                Transform physBoneRoot = physBone.rootTransform == null ? physBone.transform : physBone.rootTransform;

                if (!DressingUtils.IsPhysBoneExists(report.avatarPhysBones, physBoneRoot))
                {
                    report.clothesPhysBones.Add(physBone);
                }
            }

            return true;
        }
    }
}
