using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using VRC.SDK3.Dynamics.PhysBone.Components;
using Chocopoi.DressingTools.Containers;

namespace Chocopoi.DressingTools
{
    public class DressingUtils
    {
        private static Dictionary<string, System.Type> reflectionTypeCache = new Dictionary<string, System.Type>();

        public static DTDynamicBone FindDynBoneWithRoot(List<DTDynamicBone> avatarDynBones, Transform dynamicsRoot)
        {
            foreach (DTDynamicBone bone in avatarDynBones)
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

        public static bool IsDynBoneExists(List<DTDynamicBone> avatarDynBones, Transform dynamicsRoot)
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

        public static System.Type FindType(string typeName)
        {
            // try getting from cache to avoid scanning the assemblies again
            if (reflectionTypeCache.ContainsKey(typeName))
            {
                return reflectionTypeCache[typeName];
            }

            // scan from assemblies and save to cache
            Assembly[] assemblies = System.AppDomain.CurrentDomain.GetAssemblies();

            foreach (Assembly assembly in assemblies)
            {
                System.Type type = assembly.GetType(typeName);
                if (type != null)
                {
                    reflectionTypeCache[typeName] = type;
                    return type;
                }
            }

            // no such type found
            return null;
        }
    }
}
