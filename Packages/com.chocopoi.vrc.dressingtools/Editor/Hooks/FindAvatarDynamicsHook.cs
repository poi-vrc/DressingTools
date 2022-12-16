using System.Collections;
using System.Collections.Generic;
using Chocopoi.DressingTools.Proxy;
using Chocopoi.DressingTools.Reporting;
using UnityEngine;
using UnityEngine.Animations;

namespace Chocopoi.DressingTools.Hooks
{
    public class FindAvatarDynamicsHook : IDressHook
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

            // scan avatar dynbones

            if (DynamicBoneType != null)
            {
                Component[] avatarDynBones = targetAvatar.GetComponentsInChildren(DynamicBoneType);
                foreach (Component dynBone in avatarDynBones)
                {
                    report.avatarDynamics.Add(new DynamicBoneProxy(dynBone));
                }
            }

            // scan avatar physbones

            if (PhysBoneType != null)
            {
                Component[] avatarPhysBones = targetAvatar.GetComponentsInChildren(PhysBoneType);
                foreach (Component physBone in avatarPhysBones)
                {
                    report.avatarDynamics.Add(new PhysBoneProxy(physBone));
                }
            }

            // scan original clothes dynbones

            if (DynamicBoneType != null)
            {
                Component[] clothesDynBones = targetClothes.GetComponentsInChildren(DynamicBoneType);
                foreach (Component dynBone in clothesDynBones)
                {
                    report.clothesOriginalDynamics.Add(new DynamicBoneProxy(dynBone));
                }
            }

            // scan original clothes physbones

            if (PhysBoneType != null)
            {
                Component[] clothesPhysBones = targetClothes.GetComponentsInChildren(PhysBoneType);
                foreach (Component physBone in clothesPhysBones)
                {
                    report.clothesOriginalDynamics.Add(new PhysBoneProxy(physBone));
                }
            }

            return true;
        }
    }
}
