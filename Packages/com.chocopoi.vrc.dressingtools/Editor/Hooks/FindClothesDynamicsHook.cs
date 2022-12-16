using System.Collections;
using System.Collections.Generic;
using Chocopoi.DressingTools.DynamicsProxy;
using Chocopoi.DressingTools.Reporting;
using UnityEngine;
using UnityEngine.Animations;

namespace Chocopoi.DressingTools.Hooks
{
    public class FindClothesDynamicsHook : IDressHook
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
            System.Type PhysBoneType = DressingUtils.FindType("VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBone");

            // scan clothes dynbones

            if (DynamicBoneType != null)
            {
                Component[] clothesDynBones = targetAvatar.GetComponentsInChildren(DynamicBoneType);
                foreach (Component comp in clothesDynBones)
                {
                    DynamicBoneProxy dynBone = new DynamicBoneProxy(comp);

                    if (!DressingUtils.IsDynBoneExists(report.avatarDynBones, dynBone.m_Root))
                    {
                        report.clothesDynBones.Add(dynBone);
                    }
                }
            }

            // scan clothes physbones

            if (PhysBoneType != null)
            {
                Component[] clothesPhysBones = targetAvatar.GetComponentsInChildren(PhysBoneType);
                foreach (Component physBone in clothesPhysBones)
                {
                    PhysBoneProxy PhysBoneProxy = new PhysBoneProxy(physBone);
                    Transform physBoneRoot = PhysBoneProxy.rootTransform ?? PhysBoneProxy.transform;

                    if (!DressingUtils.IsPhysBoneExists(report.avatarPhysBones, physBoneRoot))
                    {
                        report.clothesPhysBones.Add(PhysBoneProxy);
                    }
                }
            }

            return true;
        }
    }
}
