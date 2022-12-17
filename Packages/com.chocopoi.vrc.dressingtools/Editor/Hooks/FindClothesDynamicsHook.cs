using Chocopoi.DressingTools.Proxy;
using Chocopoi.DressingTools.Reporting;
using UnityEngine;

namespace Chocopoi.DressingTools.Hooks
{
    public class FindClothesDynamicsHook : IDressHook
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

            // scan clothes dynbones

            if (DynamicBoneType != null)
            {
                var clothesDynBones = targetAvatar.GetComponentsInChildren(DynamicBoneType);
                foreach (var comp in clothesDynBones)
                {
                    var dynBone = new DynamicBoneProxy(comp);

                    if (!DressingUtils.IsDynamicsExists(report.avatarDynamics, dynBone.RootTransform))
                    {
                        report.clothesDynamics.Add(dynBone);
                    }
                }
            }

            // scan clothes physbones

            if (PhysBoneType != null)
            {
                var clothesPhysBones = targetAvatar.GetComponentsInChildren(PhysBoneType);
                foreach (var physBone in clothesPhysBones)
                {
                    var PhysBoneProxy = new PhysBoneProxy(physBone);
                    var physBoneRoot = PhysBoneProxy.RootTransform ?? PhysBoneProxy.Transform;

                    if (!DressingUtils.IsDynamicsExists(report.avatarDynamics, physBoneRoot))
                    {
                        report.avatarDynamics.Add(PhysBoneProxy);
                    }
                }
            }

            return true;
        }
    }
}
