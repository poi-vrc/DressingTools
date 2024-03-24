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
using Chocopoi.DressingTools.Animations.Fluent;
using Chocopoi.DressingTools.Components.Animations;
using Chocopoi.DressingTools.Components.Menu;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Chocopoi.DressingTools.Animations
{
    internal class SmartControlComposer
    {
        private const string BlendshapePropertyPrefix = "blendShape.";
        private const string MaterialsArrayPrefix = "materials[";
        private const string MaterialPrefix = "material";
        private const string VRCPhysBoneIsGrabbedSuffix = "_IsGrabbed";
        private const string VRCPhysBoneIsPosedSuffix = "_IsPosed";
        private const string VRCPhysBoneAngleSuffix = "_Angle";
        private const string VRCPhysBoneStretchSuffix = "_Stretch";
        private const string VRCPhysBoneSquishSuffix = "_Squish";

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
            if (!ctrl.TryGetComponent<DTAnimatorParameters>(out var comp))
            {
                comp = ctrl.gameObject.AddComponent<DTAnimatorParameters>();
            }

            comp.Configs.Add(new DTAnimatorParameters.ParameterConfig()
            {
                ParameterName = ctrl.AnimatorConfig.ParameterName,
                NetworkSynced = ctrl.AnimatorConfig.NetworkSynced,
                Saved = ctrl.AnimatorConfig.Saved,
                ParameterDefaultValue = ctrl.AnimatorConfig.ParameterDefaultValue
            });
        }

        private void GenerateParameterNameIfNeeded(DTSmartControl ctrl)
        {
            // generate if empty
            if (string.IsNullOrEmpty(ctrl.AnimatorConfig.ParameterName))
            {
                ctrl.AnimatorConfig.ParameterName = SmartControlUtils.SuggestRelativePathName(_options.context.AvatarGameObject.transform, ctrl);
            }
        }

        private void HandleDriverMenuItem(DTSmartControl ctrl)
        {
            if (!ctrl.TryGetComponent<DTMenuItem>(out var menuItem))
            {
                menuItem = ctrl.gameObject.AddComponent<DTMenuItem>();
            }

            menuItem.Type = ctrl.MenuItemDriverConfig.ItemType;
            menuItem.Icon = ctrl.MenuItemDriverConfig.ItemIcon;

            if (menuItem.Type == DTMenuItem.ItemType.Button || menuItem.Type == DTMenuItem.ItemType.Toggle)
            {
                ctrl.ControlType = DTSmartControl.SCControlType.Binary;
                menuItem.Controller.Type = DTMenuItem.ItemController.ControllerType.AnimatorParameter;
                menuItem.Controller.AnimatorParameterName = ctrl.AnimatorConfig.ParameterName;
            }
            else if (menuItem.Type == DTMenuItem.ItemType.Radial)
            {
                ctrl.ControlType = DTSmartControl.SCControlType.MotionTime;
                menuItem.SubControllers = new DTMenuItem.ItemController[] {
                        new DTMenuItem.ItemController() {
                            Type = DTMenuItem.ItemController.ControllerType.AnimatorParameter,
                            AnimatorParameterName = ctrl.AnimatorConfig.ParameterName,
                            AnimatorParameterValue = 1.0f
                        }
                    };
            }
            else
            {
                _options.context.Report.LogWarn("SmartControlComposer", "Unsupported menu item type");
            }
        }

        private string VRCPhysBoneDataSourceToParam(string prefix, DTSmartControl.SCVRCPhysBoneDriverConfig.DataSource source)
        {
            if (source == DTSmartControl.SCVRCPhysBoneDriverConfig.DataSource.Angle)
            {
                return prefix + VRCPhysBoneAngleSuffix;
            }
            else if (source == DTSmartControl.SCVRCPhysBoneDriverConfig.DataSource.Stretch)
            {
                return prefix + VRCPhysBoneStretchSuffix;
            }
            else if (source == DTSmartControl.SCVRCPhysBoneDriverConfig.DataSource.Squish)
            {
                return prefix + VRCPhysBoneSquishSuffix;
            }
            return null;
        }

        private List<string> VRCPhysBoneConditionToParams(string prefix, DTSmartControl.SCVRCPhysBoneDriverConfig.PhysBoneCondition cond)
        {
            var list = new List<string>();
            if (cond == DTSmartControl.SCVRCPhysBoneDriverConfig.PhysBoneCondition.Grabbed ||
                cond == DTSmartControl.SCVRCPhysBoneDriverConfig.PhysBoneCondition.GrabbedOrPosed)
            {
                list.Add(prefix + VRCPhysBoneIsGrabbedSuffix);
            }
            if (cond == DTSmartControl.SCVRCPhysBoneDriverConfig.PhysBoneCondition.Posed ||
                cond == DTSmartControl.SCVRCPhysBoneDriverConfig.PhysBoneCondition.GrabbedOrPosed)
            {
                list.Add(prefix + VRCPhysBoneIsPosedSuffix);
            }
            return list;
        }

#if DT_VRCSDK3A
        private void HandleDriverVRCPhysBone(DTSmartControl ctrl)
        {
            var config = ctrl.VRCPhysBoneDriverConfig;
            if (config.Condition == DTSmartControl.SCVRCPhysBoneDriverConfig.PhysBoneCondition.None &&
                config.Source == DTSmartControl.SCVRCPhysBoneDriverConfig.DataSource.None)
            {
                _options.context.Report.LogWarn("SmartControlComposer", "SmartControl VRC PhysBone driver has no condition and source, ignoring: " + ctrl.name);
                return;
            }

            // we don't use this config
            ctrl.AnimatorConfig.ParameterName = null;

            // prepare prefix
            if (string.IsNullOrEmpty(config.ParameterPrefix))
            {
                config.ParameterPrefix = SmartControlUtils.SuggestRelativePathName(_options.context.AvatarGameObject.transform, ctrl);
            }

            if (config.VRCPhysBone == null)
            {
                _options.context.Report.LogWarn("SmartControlComposer", "SmartControl no VRCPhysBone provided, ignoring: " + ctrl.name);
                return;
            }
            config.VRCPhysBone.parameter = config.ParameterPrefix;

            if (config.Source == DTSmartControl.SCVRCPhysBoneDriverConfig.DataSource.None)
            {
                // direct condition to binary
                ctrl.ControlType = DTSmartControl.SCControlType.Binary;
                ComposeBinary(ctrl, VRCPhysBoneConditionToParams(config.ParameterPrefix, config.Condition));
            }
            else
            {
                ctrl.ControlType = DTSmartControl.SCControlType.MotionTime;
                ComposeMotionTime(ctrl, VRCPhysBoneDataSourceToParam(config.ParameterPrefix, config.Source), VRCPhysBoneConditionToParams(config.ParameterPrefix, config.Condition));
            }
        }
#endif

        private void HandleDriver(DTSmartControl ctrl)
        {
            if (ctrl.DriverType == DTSmartControl.SCDriverType.AnimatorParameter ||
                ctrl.DriverType == DTSmartControl.SCDriverType.MenuItem)
            {
                GenerateParameterNameIfNeeded(ctrl);
                AddParameterConfig(ctrl);

                if (ctrl.DriverType == DTSmartControl.SCDriverType.MenuItem)
                {
                    HandleDriverMenuItem(ctrl);
                }

                if (ctrl.ControlType == DTSmartControl.SCControlType.Binary)
                {
                    ComposeBinary(ctrl, new List<string>() { ctrl.AnimatorConfig.ParameterName });
                }
                else if (ctrl.ControlType == DTSmartControl.SCControlType.MotionTime)
                {
                    ComposeMotionTime(ctrl, ctrl.AnimatorConfig.ParameterName, new List<string>());
                }
            }
            else if (ctrl.DriverType == DTSmartControl.SCDriverType.VRCPhysBone)
            {
#if DT_VRCSDK3A
                HandleDriverVRCPhysBone(ctrl);
#else
                _options.context.Report.LogWarn("SmartControlComposer", "VRCSDK not imported, skipping VRCPhysBone SmartControl composition: " + ctrl.name);
#endif
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

            HandleDriver(ctrl);

            // EditorUtility.SetDirty(ctrl.Controller);
        }

        private void ComposeBinary(DTSmartControl ctrl, List<string> conditionalParams)
        {
            if (HasCrossControlCycle(ctrl))
            {
                _options.context.Report.LogError("SmartControlComposer", "Cross control cycle detected, skipping generation of this control");
                return;
            }

            var animator = new AnimatorBuilder(_options, _controller);
            var layer = animator.NewLayer(SmartControlUtils.SuggestRelativePathName(_options.context.AvatarGameObject.transform, ctrl));
            var disabledState = layer.NewState("Disabled");
            var enabledState = layer.NewState("Enabled");
            var prepareDisabledState = layer.NewState("Prepare Disabled");

            // a dummy empty animation
            disabledState.WithNewAnimation();

            layer.WithDefaultState(disabledState);
            conditionalParams.ForEach(cp =>
            {
                // work as OR
                disabledState.AddTransition(enabledState)
                    .If(animator.BoolParameter(cp));
            });

            var enabledToPrepareDisabled = enabledState.AddTransition(prepareDisabledState);
            conditionalParams.ForEach(cp => enabledToPrepareDisabled.IfNot(animator.BoolParameter(cp)));

            var prepareDisabledToDisabled = prepareDisabledState.AddTransition(disabledState);
            conditionalParams.ForEach(cp => prepareDisabledToDisabled.IfNot(animator.BoolParameter(cp)));

            ComposeBinaryToggles(_controller, prepareDisabledState, enabledState, ctrl);
        }

        private void ComposePropertyGroupFromToValue(AnimationClipBuilder clip, DTSmartControl.PropertyGroup propGp)
        {
            // search through all objects and animate them
            var searchTrans = propGp.SelectionType == DTSmartControl.PropertyGroup.PropertySelectionType.AvatarWide ? _options.context.AvatarGameObject.transform : propGp.SearchTransform;
            var inverted = propGp.SelectionType == DTSmartControl.PropertyGroup.PropertySelectionType.Inverted || propGp.SelectionType == DTSmartControl.PropertyGroup.PropertySelectionType.AvatarWide;
            var searchObjs = SmartControlUtils.GetSelectedObjects(searchTrans, propGp.GameObjects.ToList(), inverted);

            foreach (var go in searchObjs)
            {
                var comps = go.GetComponents<Component>();
                foreach (var comp in comps)
                {
                    var compType = comp.GetType();

                    // if (propGp.PropertyValues.TryGetValue(compType.FullName, out var propVal))
                    foreach (var propVal in propGp.PropertyValues)
                    {
                        if (TryGetComponentProperty(comp, propVal.Name, out var val))
                        {
                            if (!(val is float))
                            {
                                continue;
                            }
                            clip.SetCurve(comp.transform, compType, propVal.Name, AnimationCurve.Linear(0.0f, propVal.FromValue, 1.0f, propVal.ToValue));
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
            var searchTrans = propGp.SelectionType == DTSmartControl.PropertyGroup.PropertySelectionType.AvatarWide ? _options.context.AvatarGameObject.transform : propGp.SearchTransform;
            var inverted = propGp.SelectionType == DTSmartControl.PropertyGroup.PropertySelectionType.Inverted || propGp.SelectionType == DTSmartControl.PropertyGroup.PropertySelectionType.AvatarWide;
            var searchObjs = SmartControlUtils.GetSelectedObjects(searchTrans, propGp.GameObjects.ToList(), inverted);

            foreach (var go in searchObjs)
            {
                var comps = go.GetComponents<Component>();
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
                                if (!_options.writeDefaults)
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
                                if (!_options.writeDefaults)
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

        private bool GetComponentOrGameObjectOriginalState(Component target)
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
                _options.context.Report.LogWarn("SmartControlComposer", "Unsupported component detected {target.GetType().FullName} in {target.name}, defaulting original value as false.");
                return false;
            }
        }

#if DT_VRCSDK3A
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

        private void ComposeCrossControlValueActions(AnimatorController controller, AnimatorStateBuilder state, List<DTSmartControl.SCCrossControlActions.ControlValueActions.ControlValue> values)
        {
            foreach (var value in values)
            {
                var anotherCtrl = value.Control;

                if (anotherCtrl.DriverType != DTSmartControl.SCDriverType.AnimatorParameter &&
                    anotherCtrl.DriverType != DTSmartControl.SCDriverType.MenuItem)
                {
                    continue;
                }

                // TODO
                // if (anotherCtrl.Controller != controller)
                // {
                //     // not sharing the same controller, skipping!
                //     continue;
                // }

                GenerateParameterNameIfNeeded(anotherCtrl);
                var driver = MakeVRCAvatarParameterDriver(anotherCtrl.AnimatorConfig.ParameterName, value.Value);
                AssetDatabase.AddObjectToAsset(driver, controller);
                state.AddBehaviour(driver);
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
                if (!_options.writeDefaults)
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

        private void ComposeMotionTime(DTSmartControl ctrl, string floatParameter, List<string> conditionalParams)
        {
            var animator = new AnimatorBuilder(_options, _controller);
            var layer = animator.NewLayer(SmartControlUtils.SuggestRelativePathName(_options.context.AvatarGameObject.transform, ctrl));

            var motionTimeState = layer.NewState("Motion Time")
                                .WithMotionTime(animator.FloatParameter(floatParameter));
            var clip = motionTimeState.WithNewAnimation();

            foreach (var propGp in ctrl.PropertyGroups)
            {
                ComposePropertyGroupFromToValue(clip, propGp);
            }

            if (conditionalParams.Count == 0)
            {
                layer.WithDefaultState(motionTimeState);
            }
            else
            {
                var disabledState = layer.NewState("Disabled");
                disabledState.WithNewAnimation();
                conditionalParams.ForEach(cp =>
                {
                    // we want this to work as OR
                    disabledState.AddTransition(motionTimeState)
                        .If(animator.BoolParameter(cp));
                });

                var motionTimeToEnabled = motionTimeState.AddTransition(disabledState);
                // require both parameters (AND) are not
                conditionalParams.ForEach(cp => motionTimeToEnabled.IfNot(animator.BoolParameter(cp)));

                layer.WithDefaultState(disabledState);
            }
        }

        private bool HasCrossControlCycle(DTSmartControl ctrl)
        {
            return HasCrossControlCycleValueGroup(ctrl, ctrl.CrossControlActions.ValueActions.ValuesOnEnable, true) &&
                HasCrossControlCycleValueGroup(ctrl, ctrl.CrossControlActions.ValueActions.ValuesOnDisable, false);
        }

        private bool HasCrossControlCycleValueGroup(DTSmartControl ctrl, List<DTSmartControl.SCCrossControlActions.ControlValueActions.ControlValue> values, bool enabled)
        {
            foreach (var value in values)
            {
                var anotherCtrl = value.Control;

                if (anotherCtrl.DriverType != DTSmartControl.SCDriverType.AnimatorParameter &&
                    anotherCtrl.DriverType != DTSmartControl.SCDriverType.MenuItem)
                {
                    continue;
                }

                if (anotherCtrl.ControlType != DTSmartControl.SCControlType.Binary)
                {
                    continue;
                }

                var anotherValues = Mathf.Approximately(value.Value, 1.0f) ? anotherCtrl.CrossControlActions.ValueActions.ValuesOnEnable : anotherCtrl.CrossControlActions.ValueActions.ValuesOnDisable;

                foreach (var anotherValue in anotherValues)
                {
                    if (anotherValue.Control == ctrl && Mathf.Approximately(anotherValue.Value, 1.0f) != enabled)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
