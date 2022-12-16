using System.Collections.Generic;
using System.Reflection;
using Chocopoi.DressingTools.Proxy;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools
{
    public class DressingUtils
    {
        private static Dictionary<string, System.Type> reflectionTypeCache = new Dictionary<string, System.Type>();

        public static IDynamicsProxy FindDynamicsWithRoot(List<IDynamicsProxy> avatarDynamics, Transform dynamicsRoot)
        {
            foreach (IDynamicsProxy bone in avatarDynamics)
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

        //Reference: https://forum.unity.com/threads/horizontal-line-in-editor-window.520812/#post-3416790
        public static void DrawHorizontalLine(int i_height = 1)
        {
            EditorGUILayout.Separator();
            Rect rect = EditorGUILayout.GetControlRect(false, i_height);
            rect.height = i_height;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 1));
            EditorGUILayout.Separator();
        }
    }
}
