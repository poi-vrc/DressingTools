/*
 * Copyright (c) 2024 chocopoi
 * 
 * This file is part of DressingTools.
 * 
 * DressingTools is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 * 
 * DressingTools is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with DressingTools. If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;
using Chocopoi.AvatarLib.Animations;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.Animations
{
    internal static class SmartControlUtils
    {
        public static string SuggestRelativePathName<T>(Transform avatarRoot, T comp) where T : Component
        {
            var relPath = avatarRoot.transform == comp.transform ?
                avatarRoot.name :
                AnimationUtils.GetRelativePath(comp.transform, avatarRoot);

            var sameObjComps = comp.GetComponents<T>();
            if (sameObjComps.Length > 1)
            {
                var myIndex = Array.IndexOf(sameObjComps, comp);
                return $"{relPath}_{myIndex + 1}";
            }
            else
            {
                return relPath;
            }
        }

        private static void RecursiveSearch(Transform transform, List<GameObject> excludes, HashSet<GameObject> result)
        {
            for (var i = 0; i < transform.childCount; i++)
            {
                var child = transform.GetChild(i);
                if (excludes.Contains(child.gameObject))
                {
                    continue;
                }
                result.Add(child.gameObject);
                RecursiveSearch(child, excludes, result);
            }
        }

        public static HashSet<GameObject> GetSelectedObjects(Transform searchTransform, List<GameObject> includesOrExcludes, bool inverted)
        {
            // TODO: the actual impl should share the same share function, prevent duplicate code
            var searchObjs = new HashSet<GameObject>();

            if (inverted)
            {
                if (searchTransform == null)
                {
                    // no search transform
                    return searchObjs;
                }
                RecursiveSearch(searchTransform, includesOrExcludes, searchObjs);
            }
            else
            {
                foreach (var go in includesOrExcludes)
                {
                    if (go != null)
                    {
                        searchObjs.Add(go);
                    }
                }
            }

            return searchObjs;
        }

        public static object GetShaderProperty(Material material, int i)
        {
            var shader = material.shader;
            var type = shader.GetPropertyType(i);
            var name = shader.GetPropertyName(i);

            // TODO handle arrays?
            switch (type)
            {
                case UnityEngine.Rendering.ShaderPropertyType.Float:
                case UnityEngine.Rendering.ShaderPropertyType.Range:
                    return material.GetFloat(name);
                case UnityEngine.Rendering.ShaderPropertyType.Vector:
                    return material.GetVector(name);
                case UnityEngine.Rendering.ShaderPropertyType.Color:
                    return material.GetColor(name);
            }

            return null;
        }

        public static object GetSerializedPropertyValue(SerializedProperty prop)
        {
            if (prop.propertyType == SerializedPropertyType.Integer)
            {
                return prop.intValue;
            }
            else if (prop.propertyType == SerializedPropertyType.Boolean)
            {
                return prop.boolValue;
            }
            if (prop.propertyType == SerializedPropertyType.Float)
            {
                return prop.floatValue;
            }
            else if (prop.propertyType == SerializedPropertyType.Color)
            {
                return prop.colorValue;
            }
            else if (prop.propertyType == SerializedPropertyType.ObjectReference)
            {
                return prop.objectReferenceValue;
            }
            else if (prop.propertyType == SerializedPropertyType.Vector2)
            {
                return prop.vector2Value;
            }
            else if (prop.propertyType == SerializedPropertyType.Vector2Int)
            {
                return prop.vector2IntValue;
            }
            else if (prop.propertyType == SerializedPropertyType.Vector3)
            {
                return prop.vector3Value;
            }
            else if (prop.propertyType == SerializedPropertyType.Vector3Int)
            {
                return prop.vector3IntValue;
            }
            else if (prop.propertyType == SerializedPropertyType.Vector4)
            {
                return prop.vector4Value;
            }
            else if (prop.propertyType == SerializedPropertyType.Quaternion)
            {
                return prop.quaternionValue;
            }
            else if (prop.propertyType == SerializedPropertyType.ExposedReference)
            {
                return prop.exposedReferenceValue;
            }
            else
            {
                return null;
            }
        }
    }
}
