using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using VRC.SDK3.Dynamics.PhysBone.Components;
using Chocopoi.DressingTools.Reporting;
using Chocopoi.DressingTools.Containers;

namespace Chocopoi.DressingTools.Rules
{
    public class FindClothesDynamicsRule : IDressCheckRule
    {
        public bool Evaluate(DressReport report, DressSettings settings, GameObject targetAvatar, GameObject targetClothes)
        {
            Transform avatarArmature = targetAvatar.transform.Find(settings.avatarArmatureObjectName);

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

            // get the dynbone type
            System.Type DynamicBoneType = DressingUtils.FindType("DynamicBone");

            // scan clothes dynbones

            if (DynamicBoneType != null)
            {
                Component[] clothesDynBones = targetAvatar.GetComponentsInChildren(DynamicBoneType);
                foreach (Component comp in clothesDynBones)
                {
                    DTDynamicBone dynBone = new DTDynamicBone(comp);

                    if (!DressingUtils.IsDynBoneExists(report.avatarDynBones, dynBone.m_Root))
                    {
                        report.clothesDynBones.Add(dynBone);
                    }
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
