using Chocopoi.DressingTools.Proxy;
using Chocopoi.DressingTools.Reporting;
using UnityEngine;

namespace Chocopoi.DressingTools.Hooks
{
    public class FindAvatarDynamicsHook : IDressHook
    {
        public bool Evaluate(DressReport report, DressSettings settings, GameObject targetAvatar, GameObject targetClothes)
        {
            var avatarArmature = targetAvatar.transform.Find(settings.avatarArmatureObjectName);

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
            var DynamicBoneType = DressingUtils.FindType("DynamicBone");
            var PhysBoneType = DressingUtils.FindType("VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBone");

            // scan avatar dynbones

            if (DynamicBoneType != null)
            {
                var avatarDynBones = targetAvatar.GetComponentsInChildren(DynamicBoneType);
                foreach (var dynBone in avatarDynBones)
                {
                    report.avatarDynamics.Add(new DynamicBoneProxy(dynBone));
                }
            }

            // scan avatar physbones

            if (PhysBoneType != null)
            {
                var avatarPhysBones = targetAvatar.GetComponentsInChildren(PhysBoneType);
                foreach (var physBone in avatarPhysBones)
                {
                    report.avatarDynamics.Add(new PhysBoneProxy(physBone));
                }
            }

            // scan original clothes dynbones

            if (DynamicBoneType != null)
            {
                var clothesDynBones = targetClothes.GetComponentsInChildren(DynamicBoneType);
                foreach (var dynBone in clothesDynBones)
                {
                    report.clothesOriginalDynamics.Add(new DynamicBoneProxy(dynBone));
                }
            }

            // scan original clothes physbones

            if (PhysBoneType != null)
            {
                var clothesPhysBones = targetClothes.GetComponentsInChildren(PhysBoneType);
                foreach (var physBone in clothesPhysBones)
                {
                    report.clothesOriginalDynamics.Add(new PhysBoneProxy(physBone));
                }
            }

            return true;
        }
    }
}
