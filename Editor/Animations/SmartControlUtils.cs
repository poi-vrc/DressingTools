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
using Object = UnityEngine.Object;

namespace Chocopoi.DressingTools.Animations
{
    internal static class SmartControlUtils
    {
        private const string MaterialArrayPrefix = "materials[";
        private const string SingleMaterialPrefix = "material.";

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

        public static bool TrySetBlendshapeProperty(SkinnedMeshRenderer smr, string path, float val)
        {
            if (!path.StartsWith("blendShape."))
            {
                return false;
            }

            var name = path.Substring("blendShape.".Length);
            var idx = smr.sharedMesh.GetBlendShapeIndex(name);
            if (idx < 0)
            {
                return false;
            }

            smr.SetBlendShapeWeight(idx, val);
            return true;
        }

        public static bool TrySetShaderProperty(Material[] materials, string path, Type type, object val)
        {
            var materialIdx = 0;
            var propertyNameIdx = SingleMaterialPrefix.Length;
            if (path.StartsWith(MaterialArrayPrefix))
            {
                var start = path.IndexOf("[") + 1;
                var end = path.IndexOf("]");
                if (start < 0 || end < 0)
                {
                    return false;
                }
                if (!int.TryParse(path.Substring(start, end - start), out materialIdx))
                {
                    return false;
                }
                propertyNameIdx = end + 1;
            }
            else if (!path.StartsWith(SingleMaterialPrefix))
            {
                return false;
            }

            if (propertyNameIdx >= path.Length)
            {
                return false;
            }

            var material = materials[materialIdx];
            var name = path.Substring(propertyNameIdx);

            if (type == typeof(float))
            {
                material.SetFloat(name, (float)val);
            }
            else if (type == typeof(Vector4))
            {
                material.SetVector(name, (Vector4)val);
            }
            else if (type == typeof(Color))
            {
                material.SetColor(name, (Color)val);
            }
            else if (type == typeof(Texture) || type.IsSubclassOf(typeof(Texture)))
            {
                material.SetTexture(name, (Texture)val);
            }
            else
            {
                return false;
            }
            return true;
        }

        public static bool TryGetShaderProperty(Material material, int i, out Type valType, out object val)
        {
            var shader = material.shader;
            var type = shader.GetPropertyType(i);
            var name = shader.GetPropertyName(i);

            // TODO handle arrays?
            switch (type)
            {
                case UnityEngine.Rendering.ShaderPropertyType.Float:
                case UnityEngine.Rendering.ShaderPropertyType.Range:
                    valType = typeof(float);
                    val = material.GetFloat(name);
                    return true;
                case UnityEngine.Rendering.ShaderPropertyType.Vector:
                    valType = typeof(Vector4);
                    val = material.GetVector(name);
                    return true;
                case UnityEngine.Rendering.ShaderPropertyType.Color:
                    valType = typeof(Color);
                    val = material.GetColor(name);
                    return true;
                case UnityEngine.Rendering.ShaderPropertyType.Texture:
                    valType = typeof(Texture);
                    val = material.GetTexture(name);
                    return true;
            }

            valType = null;
            val = null;
            return false;
        }

        public static bool TryGetSerializedPropertyValue(SerializedProperty prop, out Type type, out object value)
        {
            if (prop.propertyType == SerializedPropertyType.Integer)
            {
                type = typeof(int);
                value = prop.intValue;
            }
            else if (prop.propertyType == SerializedPropertyType.Boolean)
            {
                type = typeof(bool);
                value = prop.boolValue;
            }
            if (prop.propertyType == SerializedPropertyType.Float)
            {
                type = typeof(float);
                value = prop.floatValue;
            }
            else if (prop.propertyType == SerializedPropertyType.Color)
            {
                type = typeof(Color);
                value = prop.colorValue;
            }
            else if (prop.propertyType == SerializedPropertyType.ObjectReference)
            {
                type = prop.objectReferenceValue != null ? prop.objectReferenceValue.GetType() : typeof(Object);
                value = prop.objectReferenceValue;
            }
            else if (prop.propertyType == SerializedPropertyType.Vector2)
            {
                type = typeof(Vector2);
                value = prop.vector2Value;
            }
            else if (prop.propertyType == SerializedPropertyType.Vector2Int)
            {
                type = typeof(Vector2Int);
                value = prop.vector2IntValue;
            }
            else if (prop.propertyType == SerializedPropertyType.Vector3)
            {
                type = typeof(Vector3);
                value = prop.vector3Value;
            }
            else if (prop.propertyType == SerializedPropertyType.Vector3Int)
            {
                type = typeof(Vector3Int);
                value = prop.vector3IntValue;
            }
            else if (prop.propertyType == SerializedPropertyType.Vector4)
            {
                type = typeof(Vector4);
                value = prop.vector4Value;
            }
            else if (prop.propertyType == SerializedPropertyType.Quaternion)
            {
                type = typeof(Quaternion);
                value = prop.quaternionValue;
            }
            else if (prop.propertyType == SerializedPropertyType.ExposedReference)
            {
                type = prop.exposedReferenceValue != null ? prop.exposedReferenceValue.GetType() : typeof(Object);
                value = prop.exposedReferenceValue;
            }
            else
            {
                type = null;
                value = null;
                return false;
            }
            return true;
        }

        public static bool TryWriteSerializedPropertyValue(SerializedProperty prop, Type type, object val)
        {
            if (prop.propertyType == SerializedPropertyType.Integer && type == typeof(int))
            {
                prop.intValue = val == null ? 0 : (int)val;
            }
            else if (prop.propertyType == SerializedPropertyType.Boolean && type == typeof(bool))
            {
                prop.boolValue = val != null && (bool)val;
            }
            if (prop.propertyType == SerializedPropertyType.Float && type == typeof(float))
            {
                prop.floatValue = val == null ? 0f : (float)val;
            }
            else if (prop.propertyType == SerializedPropertyType.Color && type == typeof(Color))
            {
                prop.colorValue = val == null ? Color.black : (Color)val;
            }
            else if (prop.propertyType == SerializedPropertyType.ObjectReference && (type == typeof(Object) || type.IsSubclassOf(typeof(Object))))
            {
                prop.objectReferenceValue = (Object)val;
            }
            else if (prop.propertyType == SerializedPropertyType.Vector2 && type == typeof(Vector2))
            {
                prop.vector2Value = val == null ? Vector2.zero : (Vector2)val;
            }
            else if (prop.propertyType == SerializedPropertyType.Vector2Int && type == typeof(Vector2Int))
            {
                prop.vector2IntValue = val == null ? Vector2Int.zero : (Vector2Int)val;
            }
            else if (prop.propertyType == SerializedPropertyType.Vector3 && type == typeof(Vector3))
            {
                prop.vector3Value = val == null ? Vector3.zero : (Vector3)val;
            }
            else if (prop.propertyType == SerializedPropertyType.Vector3Int && type == typeof(Vector3Int))
            {
                prop.vector3IntValue = val == null ? Vector3Int.zero : (Vector3Int)val;
            }
            else if (prop.propertyType == SerializedPropertyType.Vector4 && type == typeof(Vector4))
            {
                prop.vector4Value = val == null ? Vector4.zero : (Vector4)val;
            }
            else if (prop.propertyType == SerializedPropertyType.Quaternion && type == typeof(Quaternion))
            {
                prop.quaternionValue = val == null ? Quaternion.identity : (Quaternion)val;
            }
            else if (prop.propertyType == SerializedPropertyType.ExposedReference && (type == typeof(Object) || type.IsSubclassOf(typeof(Object))))
            {
                prop.exposedReferenceValue = (Object)val;
            }
            else
            {
                return false;
            }
            return true;
        }
    }
}
