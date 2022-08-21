using System.Collections.Generic;
using UnityEngine;
using VRC.SDK3.Dynamics.PhysBone.Components;

namespace Chocopoi.DressingTools
{
    public class DressingUtils
    {
        public static DynamicBone FindDynBoneWithRoot(List<DynamicBone> avatarDynBones, Transform dynamicsRoot)
        {
            foreach (DynamicBone bone in avatarDynBones)
            {
                if (bone.m_Root == dynamicsRoot)
                {
                    return bone;
                }
            }
            return null;
        }

        public static VRCPhysBone FindPhysBoneWithRoot(List<VRCPhysBone> avatarPhysBones, Transform dynamicsRoot)
        {
            foreach (VRCPhysBone bone in avatarPhysBones)
            {
                if (bone.rootTransform != null ? bone.rootTransform == dynamicsRoot : bone.transform == dynamicsRoot)
                {
                    return bone;
                }
            }
            return null;
        }

        public static bool IsDynBoneExists(List<DynamicBone> avatarDynBones, Transform dynamicsRoot)
        {
            return FindDynBoneWithRoot(avatarDynBones, dynamicsRoot) != null;
        }

        public static bool IsPhysBoneExists(List<VRCPhysBone> avatarPhysBones, Transform dynamicsRoot)
        {
            return FindPhysBoneWithRoot(avatarPhysBones, dynamicsRoot) != null;
        }

        public static Transform GuessArmature(GameObject targetClothes, string armatureObjectName, bool rename = false)
        {
            List<Transform> transforms = new List<Transform>();

            for (int i = 0; i < targetClothes.transform.childCount; i++)
            {
                Transform child = targetClothes.transform.GetChild(i);

                if (child.name.ToLower().Trim().Contains(armatureObjectName.ToLower()))
                {
                    transforms.Add(child);
                }
            }

            if (transforms.Count == 1)
            {
                if (rename)
                {
                    transforms[0].name = armatureObjectName;
                }
                return transforms[0];
            }
            else
            {
                return null;
            }
        }
    }
}
