using System.Collections.Generic;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Proxy;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools
{
    public class DTRuntimeUtils
    {
        private static Dictionary<string, System.Type> reflectionTypeCache = new Dictionary<string, System.Type>();

        public static System.Type FindType(string typeName)
        {
            // try getting from cache to avoid scanning the assemblies again
            if (reflectionTypeCache.ContainsKey(typeName))
            {
                return reflectionTypeCache[typeName];
            }

            // scan from assemblies and save to cache
            var assemblies = System.AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                var type = assembly.GetType(typeName);
                if (type != null)
                {
                    reflectionTypeCache[typeName] = type;
                    return type;
                }
            }

            // no such type found
            return null;
        }

        public static string GetGameObjectOriginalPrefabGuid(GameObject obj)
        {
            var assetPath = AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromOriginalSource(obj));
            var guid = AssetDatabase.AssetPathToGUID(assetPath);
            return guid;
        }

        public static List<IDynamicsProxy> ScanDynamics(GameObject obj)
        {
            var dynamicsList = new List<IDynamicsProxy>();

            // TODO: replace by reading YAML

            // get the dynbone type
            var DynamicBoneType = FindType("DynamicBone");
            var PhysBoneType = FindType("VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBone");

            // scan dynbones
            if (DynamicBoneType != null)
            {
                var dynBones = obj.GetComponentsInChildren(DynamicBoneType);
                foreach (var dynBone in dynBones)
                {
                    dynamicsList.Add(new DynamicBoneProxy(dynBone));
                }
            }

            // scan physbones
            if (PhysBoneType != null)
            {
                var physBones = obj.GetComponentsInChildren(PhysBoneType);
                foreach (var physBone in physBones)
                {
                    dynamicsList.Add(new PhysBoneProxy(physBone));
                }
            }

            return dynamicsList;
        }

        public static IDynamicsProxy FindDynamicsWithRoot(List<IDynamicsProxy> avatarDynamics, Transform dynamicsRoot)
        {
            foreach (var bone in avatarDynamics)
            {
                if (bone.RootTransform == dynamicsRoot)
                {
                    return bone;
                }
            }
            return null;
        }

        public static bool IsDynamicsExists(List<IDynamicsProxy> avatarDynamics, Transform dynamicsRoot)
        {
            return FindDynamicsWithRoot(avatarDynamics, dynamicsRoot) != null;
        }

        public static Transform GuessMatchingAvatarBone(Transform avatarBoneParent, string childBoneName)
        {
            // check if there is a prefix
            if (childBoneName.StartsWith("("))
            {
                //find the first closing bracket
                var prefixBracketEnd = childBoneName.IndexOf(")");
                if (prefixBracketEnd != -1 && prefixBracketEnd != childBoneName.Length - 1) //remove it if there is
                {
                    childBoneName = childBoneName.Substring(prefixBracketEnd + 1).Trim();
                }
            }

            // check if there is a suffix
            if (childBoneName.EndsWith(")"))
            {
                //find the first closing bracket
                var suffixBracketStart = childBoneName.LastIndexOf("(");
                if (suffixBracketStart != -1 && suffixBracketStart != 0) //remove it if there is
                {
                    childBoneName = childBoneName.Substring(0, suffixBracketStart).Trim();
                }
            }

            // TODO: Guess bone?

            return avatarBoneParent.Find(childBoneName);
        }

        public static Transform GuessArmature(GameObject targetClothes, string armatureObjectName, bool rename = false)
        {
            var transforms = new List<Transform>();

            for (var i = 0; i < targetClothes.transform.childCount; i++)
            {
                var child = targetClothes.transform.GetChild(i);

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
