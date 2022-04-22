using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace Chocopoi.DressingTools
{
    public class FindClothesDynamicsRule : IDressCheckRule
    {
        private static bool IsSameDynamicsExistsInAvatar(List<Transform> avatarDynamics, Transform dynamicsRoot)
        {
            foreach (Transform bone in avatarDynamics)
            {
                if (bone == dynamicsRoot)
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
                if (!IsSameDynamicsExistsInAvatar(report.avatarDynBones, dynBone.m_Root))
                {
                    report.clothesDynBones.Add(dynBone.m_Root);
                }
            }

            // scan clothes physbones

            VRCPhysBone[] clothesPhysBones = targetAvatar.GetComponentsInChildren<VRCPhysBone>();
            foreach (VRCPhysBone physBone in clothesPhysBones)
            {
                if (!IsSameDynamicsExistsInAvatar(report.avatarPhysBones, physBone.rootTransform))
                {
                    report.clothesPhysBones.Add(physBone.rootTransform);
                }
            }

            return true;
        }
    }
}
