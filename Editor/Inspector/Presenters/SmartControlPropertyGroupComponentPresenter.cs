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
using System.Linq;
using System.Reflection;
using Chocopoi.DressingFramework.Localization;
using Chocopoi.DressingTools.Animations;
using Chocopoi.DressingTools.Components.Animations;
using Chocopoi.DressingTools.Inspector.Views;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Chocopoi.DressingTools.UI.Presenters
{
    internal class SmartControlPropertyGroupComponentPresenter
    {
        private readonly ISmartControlPropertyGroupComponentView _view;
        private readonly Dictionary<string, object> _blendshapes;
        private readonly Dictionary<string, object> _materialProperties;
        private readonly Dictionary<string, object> _genericProperties;

        public SmartControlPropertyGroupComponentPresenter(ISmartControlPropertyGroupComponentView view)
        {
            _view = view;
            _blendshapes = new Dictionary<string, object>();
            _materialProperties = new Dictionary<string, object>();
            _genericProperties = new Dictionary<string, object>();
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            _view.Load += OnLoad;
            _view.Unload += OnUnload;

            _view.SearchResultModeChange += OnSearchResultModeChange;
            _view.SearchQueryChange += OnSearchQueryChange;
            _view.AddProperty += OnAddProperty;
            _view.ChangeProperty += OnChangeProperty;
            _view.RemoveProperty += OnRemoveProperty;
            _view.PropertiesChanged += OnPropertiesChanged;
            _view.ControlTypeChanged += OnControlTypeChanged;
            _view.ChangePropertyFromToValue += OnChangePropertyFromToValue;
        }

        private void UnsubscribeEvents()
        {
            _view.Load -= OnLoad;
            _view.Unload -= OnUnload;

            _view.SearchResultModeChange -= OnSearchResultModeChange;
            _view.SearchQueryChange -= OnSearchQueryChange;
            _view.AddProperty -= OnAddProperty;
            _view.ChangeProperty -= OnChangeProperty;
            _view.RemoveProperty -= OnRemoveProperty;
            _view.PropertiesChanged -= OnPropertiesChanged;
            _view.ControlTypeChanged -= OnControlTypeChanged;
            _view.ChangePropertyFromToValue -= OnChangePropertyFromToValue;
        }

        private void OnControlTypeChanged()
        {
            RefreshSearchAndProperties();
        }

        private void OnPropertiesChanged()
        {
            RefreshSearchAndProperties();
        }

        private void OnRemoveProperty(string key)
        {
            _view.TargetPropertyValues.RemoveAll(propVal => propVal.Name.StartsWith(key));
            RefreshSearchAndProperties();
            _view.RaisePropertiesChangedEvent();
        }

        private void OnChangeProperty(string key, object val)
        {
            SetTargetPropertyValue(key, val);
            _view.Properties[key].value = val;
            _view.RaisePropertiesChangedEvent();
        }

        private void OnChangePropertyFromToValue(string key, float fromVal, float toVal)
        {
            SetOrAddTargetPropertyFromToValue(key, fromVal, toVal);
            _view.Properties[key].fromValue = fromVal;
            _view.Properties[key].toValue = toVal;
            _view.RaisePropertiesChangedEvent();
        }

        private void UpdateMaterialShaderProperties(Material material, string path)
        {
            ForEachShaderProperty(material, (name, val) =>
            {
                // the val here is used for determining type only
                var shaderPath = $"{path}.{name}";
                var type = val.GetType();
                if (val is Object)
                {
                    var propVal = GetPropertyValue(shaderPath);
                    if (propVal == null)
                    {
                        return;
                    }
                    _view.Properties[shaderPath] = new SmartControlComponentViewPropertyValue()
                    {
                        type = type,
                        value = propVal.ValueObjectReference
                    };
                }
                else if (type.IsPrimitive)
                {
                    var propVal = GetPropertyValue(shaderPath);
                    if (propVal == null)
                    {
                        return;
                    }
                    _view.Properties[shaderPath] = new SmartControlComponentViewPropertyValue()
                    {
                        type = type,
                        value = propVal.Value,
                        fromValue = propVal.FromValue,
                        toValue = propVal.ToValue
                    };
                }
                else
                {
                    // create a new object and fill the public fields to there
                    var newObj = Activator.CreateInstance(type);
                    var foundAll = true;

                    ForEachPublicField(val, (fieldName, fieldVal) =>
                    {
                        if (!(fieldVal is float))
                        {
                            return;
                        }

                        var propVal = GetPropertyValue($"{shaderPath}.{fieldName}");
                        if (propVal == null)
                        {
                            foundAll = false;
                            return;
                        }

                        type.GetField(fieldName).SetValue(newObj, propVal.Value);
                    });

                    if (foundAll)
                    {
                        _view.Properties[shaderPath] = new SmartControlComponentViewPropertyValue()
                        {
                            type = type,
                            value = newObj
                        };
                    }
                }
            });
        }

        private void UpdateRendererProperties(Renderer rr)
        {
            for (var i = 0; i < rr.sharedMaterials.Length; i++)
            {
                if (i == 0)
                {
                    UpdateMaterialShaderProperties(rr.sharedMaterials[i], "material");
                }
                UpdateMaterialShaderProperties(rr.sharedMaterials[i], $"materials[{i}]");
            }
        }

        private void UpdateSkinnedMeshRenderer(SkinnedMeshRenderer smr)
        {
            if (smr.sharedMesh == null)
            {
                return;
            }

            for (var i = 0; i < smr.sharedMesh.blendShapeCount; i++)
            {
                var name = smr.sharedMesh.GetBlendShapeName(i);
                var path = $"blendShape.{name}";
                var propVal = GetPropertyValue(path);
                if (propVal != null)
                {
                    _view.Properties.Add(path, new SmartControlComponentViewPropertyValue()
                    {
                        type = typeof(float),
                        value = propVal.Value,
                        fromValue = propVal.FromValue,
                        toValue = propVal.ToValue
                    });
                }
            }
        }

        private void UpdateGenericProperties()
        {
            ForEachSerializedObjectProperty(_view.TargetComponent, (path, val) =>
            {
                var propVal = GetPropertyValue(path);
                if (propVal == null)
                {
                    return;
                }
                _view.Properties.Add(path, new SmartControlComponentViewPropertyValue()
                {
                    type = val.GetType(),
                    value = val
                });
            });
        }

        private void UpdateProperties()
        {
            _view.Properties.Clear();
            if (_view.TargetComponent is SkinnedMeshRenderer smr)
            {
                UpdateSkinnedMeshRenderer(smr);
            }
            if (_view.TargetComponent is Renderer rr)
            {
                UpdateRendererProperties(rr);
            }
            UpdateGenericProperties();
        }

        private float PrimitivesToFloat(object value)
        {
            if (value is bool b)
            {
                return b ? 1.0f : 0.0f;
            }
            else if (value is int i)
            {
                return i;
            }
            else if (value is float f)
            {
                return f;
            }
            else
            {
                return 0.0f;
            }
        }

        private static void ForEachPublicField(object obj, Action<string, object> func)
        {
            var fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetField | BindingFlags.SetField);
            foreach (var field in fields)
            {
                func(field.Name, field.GetValue(obj));
            }
        }

        private DTSmartControl.PropertyGroup.PropertyValue GetPropertyValue(string name)
        {
            var propVals = _view.TargetPropertyValues.Where(p => p.Name == name);
            var len = propVals.Count();
            if (len > 1)
            {
                // remove duplicate entries
                for (var i = 1; i < len; i++)
                {
                    _view.TargetPropertyValues.RemoveAt(i);
                }
            }
            return propVals.FirstOrDefault();
        }

        private void SetOrAddTargetPropertyValue(string name, float value, Object objRef)
        {
            var propVal = GetPropertyValue(name);
            if (propVal == null)
            {
                propVal = new DTSmartControl.PropertyGroup.PropertyValue();
                _view.TargetPropertyValues.Add(propVal);
            }
            propVal.Name = name;
            propVal.Value = value;
            propVal.ValueObjectReference = objRef;
        }

        private void SetOrAddTargetPropertyFromToValue(string name, float fromValue, float toValue)
        {
            var propVal = GetPropertyValue(name);
            if (propVal == null)
            {
                propVal = new DTSmartControl.PropertyGroup.PropertyValue();
                _view.TargetPropertyValues.Add(propVal);
            }
            propVal.Name = name;
            propVal.FromValue = fromValue;
            propVal.ToValue = toValue;
        }

        private void SetTargetPropertyValue(string name, object value)
        {
            var type = value.GetType();
            if (value is Object o)
            {
                SetOrAddTargetPropertyValue(name, 0.0f, o);
            }
            else if (type.IsPrimitive)
            {
                SetOrAddTargetPropertyValue(name, PrimitivesToFloat(value), null);
            }
            else
            {
                ForEachPublicField(value, (fieldName, fieldVal) =>
                {
                    if (!(fieldVal is float f)) return;
                    SetOrAddTargetPropertyValue($"{name}.{fieldName}", f, null);
                });
            }
        }

        private void RefreshSearchAndProperties()
        {
            PerformSearch();
            UpdateProperties();
            _view.Repaint();
        }

        private void OnAddProperty(string name, object value)
        {
            SetTargetPropertyValue(name, value);
            if (value is float)
            {
                SetOrAddTargetPropertyFromToValue(name, 0, 100);
            }
            RefreshSearchAndProperties();
            _view.RaisePropertiesChangedEvent();
        }

        private void OnSearchQueryChange()
        {
            PerformSearch();
        }

        private void OnSearchResultModeChange()
        {
            PerformSearch();
        }

        private static bool CheckIfHasExistingProperty(List<string> existingNames, string name, object value)
        {
            var type = value.GetType();
            if (type == typeof(Object) || type.IsPrimitive)
            {
                return existingNames.Contains(name);
            }
            else
            {
                var foundAll = true;
                ForEachPublicField(value, (fieldName, fieldVal) =>
                {
                    foundAll &= existingNames.Contains($"{name}.{fieldName}");
                });
                return foundAll;
            }
        }

        // reference: https://stackoverflow.com/questions/9453731/how-to-calculate-distance-similarity-measure-of-given-2-strings
        private static int CalculateLevenshteinDistance(string a, string b)
        {
            if (string.IsNullOrEmpty(a) && string.IsNullOrEmpty(b))
            {
                return 0;
            }

            if (string.IsNullOrEmpty(a))
            {
                return b.Length;
            }

            if (string.IsNullOrEmpty(b))
            {
                return a.Length;
            }

            var lengthA = a.Length;
            var lengthB = b.Length;
            var distances = new int[lengthA + 1, lengthB + 1];

            for (var i = 0; i <= lengthA; distances[i, 0] = i++) { }
            for (var j = 0; j <= lengthB; distances[0, j] = j++) { }

            for (var i = 1; i <= lengthA; i++)
            {
                for (var j = 1; j <= lengthB; j++)
                {
                    var cost = b[j - 1] == a[i - 1] ? 0 : 1;

                    distances[i, j] = Math.Min(
                        Math.Min(distances[i - 1, j] + 1, distances[i, j - 1] + 1),
                        distances[i - 1, j - 1] + cost
                    );
                }
            }

            return distances[lengthA, lengthB];
        }

        private void PerformSearch()
        {
            _view.SearchResults.Clear();

            Dictionary<string, object> source;
            if (_view.SearchResultMode == SmartControlComponentViewResultMode.Blendshapes)
            {
                source = _blendshapes;
            }
            else if (_view.SearchResultMode == SmartControlComponentViewResultMode.Material)
            {
                source = _materialProperties;
            }
            else if (_view.SearchResultMode == SmartControlComponentViewResultMode.Generic)
            {
                source = _genericProperties;
            }
            else
            {
                return;
            }

            var results = source.ToList();
            var existingPropertyNames = new List<string>();
            foreach (var propVal in _view.TargetPropertyValues)
            {
                existingPropertyNames.Add(propVal.Name);
            }
            results.RemoveAll(kvp => CheckIfHasExistingProperty(existingPropertyNames, kvp.Key, kvp.Value));
            if (_view.ControlType == 1)
            {
                // for motion time, remove all non-float properties
                results.RemoveAll(kvp => kvp.Value.GetType() != typeof(float));
            }

            if (!string.IsNullOrEmpty(_view.SearchQuery))
            {
                var editDistances = new Dictionary<KeyValuePair<string, object>, int>();
                foreach (var kvp in source)
                {
                    editDistances[kvp] = CalculateLevenshteinDistance(kvp.Key, _view.SearchQuery);
                }

                results.Sort((x, y) => editDistances[x].CompareTo(editDistances[y]));

                // TODO use global constant
                var len = Math.Min(results.Count, 20);
                for (var i = 0; i < len; i++)
                {
                    _view.SearchResults.Add(results[i]);
                }
            }
            else
            {
                _view.SearchResults.AddRange(results);
            }

            _view.Repaint();
        }

        private void SearchBlendshapes(SkinnedMeshRenderer smr)
        {
            if (smr.sharedMesh == null)
            {
                return;
            }

            for (var i = 0; i < smr.sharedMesh.blendShapeCount; i++)
            {
                var name = smr.sharedMesh.GetBlendShapeName(i);
                var value = smr.GetBlendShapeWeight(i);
                _blendshapes[$"blendShape.{name}"] = value;
            }
            _view.HasBlendshapes = _blendshapes.Count > 0;
        }

        private static void ForEachSerializedObjectProperty(Object obj, Action<string, object> func)
        {
            var so = new SerializedObject(obj);
            so.Update();
            var prop = so.GetIterator();
            prop.NextVisible(enterChildren: true);
            while (prop.NextVisible(enterChildren: false))
            {
                object value = SmartControlUtils.GetSerializedPropertyValue(prop);
                if (value == null)
                {
                    continue;
                }
                func(prop.propertyPath, value);
            }
        }

        private static void ForEachShaderProperty(Material material, Action<string, object> func)
        {
            var shader = material.shader;
            var len = shader.GetPropertyCount();
            for (var i = 0; i < len; i++)
            {
                var name = shader.GetPropertyName(i);
                var val = SmartControlUtils.GetShaderProperty(material, i);
                if (val == null)
                {
                    continue;
                }
                func(name, val);
            }
        }

        private void SearchMaterial(Material material, string prefix)
        {
            _materialProperties[prefix] = material;
            ForEachShaderProperty(material, (name, val) =>
            {
                _materialProperties[$"{prefix}.{name}"] = val;
            });
            ForEachSerializedObjectProperty(material, (path, val) =>
            {
                _materialProperties[$"{prefix}.{path}"] = val;
            });
        }

        private void SearchMaterials(Renderer rr)
        {
            for (var i = 0; i < rr.sharedMaterials.Length; i++)
            {
                var material = rr.sharedMaterials[i];
                if (i == 0)
                {
                    SearchMaterial(material, "material");
                }
                SearchMaterial(material, $"materials[{i}]");
            }
            _view.HasMaterialProperties = _materialProperties.Count > 0;
        }

        private void SearchGenericProperties(Component comp)
        {
            ForEachSerializedObjectProperty(comp, (path, val) =>
            {
                _genericProperties[path] = val;
            });
            _view.HasGenericProperties = _genericProperties.Count > 0;
        }

        private void SearchComponentProperties()
        {
            _view.DisplayAllResults = false;
            if (_view.TargetComponent is SkinnedMeshRenderer smr)
            {
                SearchBlendshapes(smr);
            }
            if (_view.TargetComponent is Renderer rr)
            {
                SearchMaterials(rr);
            }
            SearchGenericProperties(_view.TargetComponent);
        }

        private void UpdateView()
        {
            UpdateProperties();
            _view.Repaint();
        }

        private void OnLoad()
        {
            SearchComponentProperties();
            UpdateView();
        }

        private void OnUnload()
        {
            UnsubscribeEvents();
        }
    }
}
