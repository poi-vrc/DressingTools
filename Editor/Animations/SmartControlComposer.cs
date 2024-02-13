﻿/*
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

using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Chocopoi.DressingFramework;
using Chocopoi.DressingFramework.Animations;
using Chocopoi.DressingTools.Animations.Fluent;
using Chocopoi.DressingTools.Components.Animations;
using Chocopoi.DressingTools.Components.Menu;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Chocopoi.DressingTools.Animations
{
    internal class SmartControlComposer
    {
        private const string BlendshapePropertyPrefix = "blendShape.";
        private const string MaterialsArrayPrefix = "materials[";
        private const string MaterialPrefix = "material";

        private readonly HashSet<DTSmartControl> _controls;
        private readonly AnimatorOptions _options;
        private readonly AnimatorController _controller;

        public SmartControlComposer(AnimatorOptions options, AnimatorController controller)
        {
            _options = options;
            _controller = controller;
            _controls = new HashSet<DTSmartControl>();
        }

        private void AddParameterConfig(DTSmartControl ctrl)
        {
            var animParams = _options.context.Feature<AnimatorParameters>();
            animParams.AddConfig(new AnimatorParameters.ParameterConfig($"^{Regex.Escape(ctrl.AnimatorConfig.ParameterName)}$")
            {
                networkSynced = ctrl.AnimatorConfig.NetworkSynced,
                saved = ctrl.AnimatorConfig.Saved,
                defaultValue = ctrl.AnimatorConfig.ParameterDefaultValue
            });
        }

        private static void GenerateParameterNameIfNeeded(DTSmartControl ctrl)
        {
            // generate if empty
            if (string.IsNullOrEmpty(ctrl.AnimatorConfig.ParameterName))
            {
                ctrl.AnimatorConfig.ParameterName = $"cpSC_{DKEditorUtils.RandomString(8)}";
                // TODO: human readable
            }
        }

        private static void HandleDriver(DTSmartControl ctrl)
        {
            if (ctrl.DriverType == DTSmartControl.SmartControlDriverType.MenuItem)
            {
                if (!ctrl.TryGetComponent<DTMenuItem>(out var menuItem))
                {
                    return;
                }

                menuItem.Controller.AnimatorParameterName = ctrl.AnimatorConfig.ParameterName;
            }
        }

        public void Compose(DTSmartControl ctrl)
        {
            if (_controls.Contains(ctrl))
            {
                return;
            }
            _controls.Add(ctrl);

            // TODO: customizable animator
            // if (ctrl.Controller == null)
            // {
            //     return;
            // }

            GenerateParameterNameIfNeeded(ctrl);
            HandleDriver(ctrl);
            AddParameterConfig(ctrl);

            if (ctrl.ControlType == DTSmartControl.SmartControlControlType.Binary)
            {
                ComposeBinary(ctrl);
            }
            else if (ctrl.ControlType == DTSmartControl.SmartControlControlType.MotionTime)
            {
                ComposeMotionTime(ctrl);
            }

            // EditorUtility.SetDirty(ctrl.Controller);
        }

        private void ComposeBinary(DTSmartControl ctrl)
        {
            if (HasCrossControlCycle(ctrl))
            {
                Debug.LogError("Cross control cycle detected, skipping generation of this control");
                return;
            }

            var animator = new AnimatorBuilder(_options, _controller);
            var layer = animator.NewLayer($"cpSC_{DKEditorUtils.RandomString(8)}");
            var disabledState = layer.NewState("Disabled");
            var enabledState = layer.NewState("Enabled");

            layer.WithDefaultState(disabledState);
            disabledState.AddTransition(enabledState)
                .If(animator.BoolParameter(ctrl.AnimatorConfig.ParameterName));
            enabledState.AddTransition(disabledState)
                .IfNot(animator.BoolParameter(ctrl.AnimatorConfig.ParameterName));

            ComposeBinaryToggles(_controller, disabledState, enabledState, ctrl);
        }

        private void ComposePropertyGroupWithCurve(AnimationClipBuilder clip, DTSmartControl.PropertyGroup propGp, AnimationCurve curve)
        {
            // search through all objects and animate them
            var searchObjs = SmartControlUtils.GetSelectedObjects(propGp.SearchTransform, propGp.GameObjects.ToList(), propGp.SelectionType == DTSmartControl.PropertyGroup.PropertySelectionType.Inverted);
            foreach (var go in searchObjs)
            {
                var comps = go.GetComponentsInChildren<Component>(true);
                foreach (var comp in comps)
                {
                    var compType = comp.GetType();

                    // if (propGp.PropertyValues.TryGetValue(compType.FullName, out var propVal))
                    foreach (var propVal in propGp.PropertyValues)
                    {
                        if (TryGetComponentProperty(comp, propVal.Name, out _))
                        {
                            clip.SetCurve(comp.transform, compType, propVal.Name, curve);
                        }
                    }
                }
            }
        }

        private static bool TryGetBlendshapeProperty(SkinnedMeshRenderer smr, string propertyName, out object value)
        {
            value = -1;
            var name = propertyName.Substring(BlendshapePropertyPrefix.Length);

            if (smr.sharedMesh == null)
            {
                return false;
            }

            for (var i = 0; i < smr.sharedMesh.blendShapeCount; i++)
            {
                if (smr.sharedMesh.GetBlendShapeName(i) == name)
                {
                    value = smr.GetBlendShapeWeight(i);
                    return true;
                }
            }

            return false;
        }

        private static bool TryGetMaterialProperty(Renderer rr, string propertyName, out object value)
        {
            value = null;

            var propertyNameStartIndex = -1;
            Material material;
            if (propertyName.StartsWith(MaterialsArrayPrefix))
            {
                // array
                var closingBracketIndex = propertyName.IndexOf("]");
                var index = -1;
                try
                {
                    index = int.Parse(propertyName.Substring(MaterialsArrayPrefix.Length, closingBracketIndex));
                }
                catch { return false; }
                propertyNameStartIndex = closingBracketIndex + 2; // after the dot
                material = rr.sharedMaterials[index];
            }
            else if (propertyName.StartsWith(MaterialPrefix))
            {
                material = rr.sharedMaterial;
                propertyNameStartIndex = MaterialPrefix.Length + 1;
            }
            else
            {
                return false;
            }

            if (propertyNameStartIndex >= propertyName.Length)
            {
                // invalid name
                return false;
            }

            var shaderPropertyName = propertyName.Substring(propertyNameStartIndex);
            var shaderPropertyIndex = material.shader.FindPropertyIndex(shaderPropertyName);
            if (shaderPropertyIndex == -1)
            {
                return false;
            }

            value = SmartControlUtils.GetShaderProperty(material, shaderPropertyIndex);
            return true;
        }

        private static bool TryGetComponentProperty(Component comp, string propertyName, out object value)
        {
            if (comp is SkinnedMeshRenderer smr && propertyName.StartsWith(BlendshapePropertyPrefix))
            {
                return TryGetBlendshapeProperty(smr, propertyName, out value);
            }

            if (comp is Renderer rr && propertyName.StartsWith(MaterialPrefix))
            {
                return TryGetMaterialProperty(rr, propertyName, out value);
            }

            var so = new SerializedObject(comp);
            var prop = so.FindProperty(propertyName);
            if (prop == null)
            {
                value = null;
                return false;
            }
            value = SmartControlUtils.GetSerializedPropertyValue(prop);

            return true;
        }

        private void ComposePropertyGroupToggles(AnimationClipBuilder disabledClip, AnimationClipBuilder enabledClip, DTSmartControl.PropertyGroup propGp)
        {
            // search through all objects and animate them
            var searchObjs = SmartControlUtils.GetSelectedObjects(propGp.SearchTransform, propGp.GameObjects.ToList(), propGp.SelectionType == DTSmartControl.PropertyGroup.PropertySelectionType.Inverted);
            foreach (var go in searchObjs)
            {
                var comps = go.GetComponentsInChildren<Component>(true);
                foreach (var comp in comps)
                {
                    var compType = comp.GetType();

                    // if (propGp.PropertyValues.TryGetValue(compType.FullName, out var propVal))
                    foreach (var propVal in propGp.PropertyValues)
                    {
                        if (TryGetComponentProperty(comp, propVal.Name, out var originalValue))
                        {
                            // TODO: customizable curve by values
                            if (propVal.ValueObjectReference != null && originalValue is Object obj)
                            {
                                var enabledFrames = new ObjectReferenceKeyframe[] { new ObjectReferenceKeyframe() {
                                        time = 0.0f,
                                        value = propVal.ValueObjectReference
                                }};
                                enabledClip.SetCurve(comp.transform, compType, propVal.Name, enabledFrames);
                                if (_options.writeDefaultsMode == AnimatorOptions.WriteDefaultsMode.Off)
                                {
                                    var disabledFrames = new ObjectReferenceKeyframe[] { new ObjectReferenceKeyframe() {
                                        time = 0.0f,
                                        value = obj
                                    }};
                                    disabledClip.SetCurve(comp.transform, compType, propVal.Name, disabledFrames);
                                }
                            }
                            else if (originalValue is float f)
                            {
                                enabledClip.SetCurve(comp.transform, compType, propVal.Name, AnimationCurve.Constant(0.0f, 0.0f, propVal.Value));
                                if (_options.writeDefaultsMode == AnimatorOptions.WriteDefaultsMode.Off)
                                {
                                    disabledClip.SetCurve(comp.transform, compType, propVal.Name, AnimationCurve.Constant(0.0f, 0.0f, f));
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void ClipToggle(AnimationClipBuilder clip, Component target, bool enabled)
        {
            if (target is Transform)
            {
                // treat it as GameObject
                clip.Toggle(target.gameObject, enabled);
            }
            else
            {
                clip.Toggle(target, enabled);
            }
        }

        private static bool GetComponentOrGameObjectOriginalState(Component target)
        {
            // as we are sharing the Component type for both components and GameObjects
            // we would want to get the activeSelf of GameObject instead
            if (target is Transform)
            {
                return target.gameObject.activeSelf;
            }
            else if (target is Behaviour behaviour)
            {
                return behaviour.enabled;
            }
            else
            {
                // unknown
                return false;
            }
        }

#if DT_VRCSDK3A
        // TODO: directly copied from the pass, should move this to a common location to remove code duplication
        private static VRC.SDK3.Avatars.Components.VRCAvatarParameterDriver MakeVRCAvatarParameterDriver(string name, float value)
        {
            var driver = ScriptableObject.CreateInstance<VRC.SDK3.Avatars.Components.VRCAvatarParameterDriver>();
            driver.parameters.Add(new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter()
            {
                type = VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType.Set,
                name = name,
                value = value
            });
            return driver;
        }

        private void ComposeCrossControlValueActions(AnimatorController controller, AnimatorStateBuilder state, List<DTSmartControl.SmartControlCrossControlActions.ControlValueActions.ControlValue> values)
        {
            foreach (var value in values)
            {
                var anotherCtrl = value.Control;

                // TODO
                // if (anotherCtrl.Controller != controller)
                // {
                //     // not sharing the same controller, skipping!
                //     continue;
                // }

                GenerateParameterNameIfNeeded(anotherCtrl);
                state.AddBehaviour(MakeVRCAvatarParameterDriver(anotherCtrl.AnimatorConfig.ParameterName, value.Value));
            }
        }
#endif

        private void ComposeBinaryToggles(AnimatorController controller, AnimatorStateBuilder disabledState, AnimatorStateBuilder enabledState, DTSmartControl ctrl)
        {
            var disabledClip = disabledState.WithNewAnimation();
            var enabledClip = enabledState.WithNewAnimation();

            foreach (var toggle in ctrl.ObjectToggles)
            {
                if (toggle.Target == null)
                {
                    continue;
                }

                ClipToggle(enabledClip, toggle.Target, toggle.Enabled);
                if (_options.writeDefaultsMode == AnimatorOptions.WriteDefaultsMode.Off)
                {
                    ClipToggle(disabledClip, toggle.Target, GetComponentOrGameObjectOriginalState(toggle.Target));
                }
            }

            foreach (var propGp in ctrl.PropertyGroups)
            {
                ComposePropertyGroupToggles(disabledClip, enabledClip, propGp);
            }

#if DT_VRCSDK3A
            // cross-controls are currently only available in VRC environments
            ComposeCrossControlValueActions(controller, disabledState, ctrl.CrossControlActions.ValueActions.ValuesOnDisable);
            ComposeCrossControlValueActions(controller, enabledState, ctrl.CrossControlActions.ValueActions.ValuesOnEnable);
#endif
        }

        private void ComposeMotionTime(DTSmartControl ctrl)
        {
            var animator = new AnimatorBuilder(_options, _controller);
            var layer = animator.NewLayer($"cpSC_{DKEditorUtils.RandomString(8)}");
            var state = layer.NewState("Motion Time")
                                .WithMotionTime(animator.FloatParameter(ctrl.AnimatorConfig.ParameterName));
            layer.WithDefaultState(state);

            var clip = state.WithNewAnimation();

            foreach (var propGp in ctrl.PropertyGroups)
            {
                ComposePropertyGroupWithCurve(clip, propGp, ctrl.Curve);
            }
        }

        private bool HasCrossControlCycle(DTSmartControl ctrl)
        {
            return HasCrossControlCycleValueGroup(ctrl, ctrl.CrossControlActions.ValueActions.ValuesOnEnable, true) &&
                HasCrossControlCycleValueGroup(ctrl, ctrl.CrossControlActions.ValueActions.ValuesOnDisable, false);
        }

        private bool HasCrossControlCycleValueGroup(DTSmartControl ctrl, List<DTSmartControl.SmartControlCrossControlActions.ControlValueActions.ControlValue> values, bool enabled)
        {
            foreach (var value in values)
            {
                if (value.Control.ControlType != DTSmartControl.SmartControlControlType.Binary)
                {
                    continue;
                }

                var anotherValues = value.Value == 1.0f ? ctrl.CrossControlActions.ValueActions.ValuesOnEnable : ctrl.CrossControlActions.ValueActions.ValuesOnDisable;

                foreach (var anotherValue in anotherValues)
                {
                    if (anotherValue.Control == ctrl && anotherValue.Value == 1.0f != enabled)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}