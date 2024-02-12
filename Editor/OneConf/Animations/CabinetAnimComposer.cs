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

using System.Collections.Generic;
using Chocopoi.AvatarLib.Animations;
using Chocopoi.DressingFramework;
using Chocopoi.DressingFramework.Animations;
using Chocopoi.DressingFramework.Menu;
using Chocopoi.DressingTools.Animations.Fluent;
using Chocopoi.DressingTools.Components.Animations;
using Chocopoi.DressingTools.Dynamics.Proxy;
using Chocopoi.DressingTools.OneConf.Wearable;
using Chocopoi.DressingTools.OneConf.Wearable.Modules.BuiltIn;
using UnityEditor.Animations;
using UnityEngine;

namespace Chocopoi.DressingTools.OneConf.Animations
{
    /// <summary>
    /// This class is temporary for generating smart controls directly from OneConf modules.
    /// This will be probably removed after changing OneConf to multiple components
    /// </summary>
    internal class CabinetAnimComposer
    {
        // a temporary hack here, just to obtain the default frame rate
        private static float s_frameRate = -1.0f;
        private static float DefaultFrameRate => s_frameRate == -1.0f ? s_frameRate = new AnimationClip().frameRate : s_frameRate;

        private readonly AnimatorController _controller;
        private readonly AnimatorOptions _options;
        private readonly GameObject _avatarObject;
        private readonly MenuGroup _menuGroup;
        private readonly Dictionary<GameObject, List<DTSmartControl>> _ctrls;
        private readonly bool _useThumbnails;

        public CabinetAnimComposer(AnimatorController controller, AnimatorOptions options, GameObject avatarObject, PathRemapper pathRemapper, bool useThumbnails)
        {
            _controller = controller;
            _options = options;
            _avatarObject = avatarObject;
            _menuGroup = new MenuGroup();
            _ctrls = new Dictionary<GameObject, List<DTSmartControl>>();
            _useThumbnails = useThumbnails;
        }

        private Transform GetAvatarTransform(string avatarPath)
        {
            return _avatarObject.transform.Find(avatarPath);
        }

        private Transform GetWearableTransform(GameObject wearableObject, string wearablePath)
        {
            var basePath = AnimationUtils.GetRelativePath(wearableObject.transform, _avatarObject.transform);
            return _avatarObject.transform.Find(basePath + "/" + wearablePath);
        }

        private void MakeAvatarToggles(DTSmartControl.BinarySmartControlBuilder binaryBuilder, List<CabinetAnimWearableModuleConfig.Toggle> toggles)
        {
            foreach (var toggle in toggles)
            {
                var trans = GetAvatarTransform(toggle.path);
                if (trans == null)
                {
                    // _report.LogWarnLocalized(t, LogLabel, MessageCode.IgnoredAvatarToggleObjectNotFound, toggle.path);
                    continue;
                }
                binaryBuilder.Toggle(trans, toggle.state);
            }
        }

        private void MakeWearableToggles(DTSmartControl.BinarySmartControlBuilder binaryBuilder, GameObject wearableObject, List<CabinetAnimWearableModuleConfig.Toggle> toggles)
        {
            foreach (var toggle in toggles)
            {
                var trans = GetWearableTransform(wearableObject, toggle.path);
                if (trans == null)
                {
                    // _report.LogWarnLocalized(t, LogLabel, MessageCode.IgnoredAvatarToggleObjectNotFound, toggle.path);
                    continue;
                }
                binaryBuilder.Toggle(trans, toggle.state);
            }
        }

        private void MakeAvatarBlendshapeToggles(DTSmartControl ctrl, DTSmartControl.BinarySmartControlBuilder binaryBuilder, List<CabinetAnimWearableModuleConfig.BlendshapeValue> blendshapes)
        {
            foreach (var blendshape in blendshapes)
            {
                var trans = GetAvatarTransform(blendshape.path);
                if (trans == null)
                {
                    // _report.LogWarnLocalized(t, LogLabel, MessageCode.IgnoredAvatarBlendshapeObjectNotFound, _avatarObject.name, blendshape.path);
                    continue;
                }

                binaryBuilder.AddPropertyGroup(
                    ctrl.NewPropertyGroup()
                        .WithSelectedObjects(trans.gameObject)
                        .ChangeProperty($"blendShape.{blendshape.blendshapeName}", blendshape.value)
                        );
            }
        }

        private void MakeWearableBlendshapeToggles(DTSmartControl ctrl, DTSmartControl.BinarySmartControlBuilder binaryBuilder, GameObject wearableObject, List<CabinetAnimWearableModuleConfig.BlendshapeValue> blendshapes)
        {
            foreach (var blendshape in blendshapes)
            {
                var trans = GetWearableTransform(wearableObject, blendshape.path);
                if (trans == null)
                {
                    // _report.LogWarnLocalized(t, LogLabel, MessageCode.IgnoredAvatarBlendshapeObjectNotFound, _avatarObject.name, blendshape.path);
                    continue;
                }

                binaryBuilder.AddPropertyGroup(
                    ctrl.NewPropertyGroup()
                        .WithSelectedObjects(trans.gameObject)
                        .ChangeProperty($"blendShape.{blendshape.blendshapeName}", blendshape.value)
                    );
            }
        }

        private void MakeBasicToggles(GameObject wearableObject, List<IDynamicsProxy> wearableDynamics, CabinetAnimWearableModuleConfig module, out DTSmartControl ctrl)
        {
            // TODO: separate into different gameobjects?
            ctrl = wearableObject.AddComponent<DTSmartControl>();
            ctrl.AnimatorConfig.ParameterName = $"cpCA_{wearableObject.name}_{DKEditorUtils.RandomString(8)}";
            var binaryBuilder = ctrl.AsBinary();

            MakeAvatarToggles(binaryBuilder, module.avatarAnimationOnWear.toggles);
            MakeWearableToggles(binaryBuilder, wearableObject, module.wearableAnimationOnWear.toggles);
            MakeAvatarBlendshapeToggles(ctrl, binaryBuilder, module.avatarAnimationOnWear.blendshapes);
            MakeWearableBlendshapeToggles(ctrl, binaryBuilder, wearableObject, module.wearableAnimationOnWear.blendshapes);

            foreach (var dynamics in wearableDynamics)
            {
                binaryBuilder.Toggle(dynamics.Component, true);
            }
        }

        private void MakeToggleCustomizable(GameObject wearableObject, CabinetAnimWearableModuleConfig.Customizable cst, out DTSmartControl ctrl)
        {
            // TODO: separate into different gameobjects?
            // TODO: animator config builder?
            ctrl = wearableObject.AddComponent<DTSmartControl>();
            ctrl.AnimatorConfig.ParameterName = $"cpCA_{wearableObject.name}_{cst.name}_{DKEditorUtils.RandomString(8)}";
            var binaryBuilder = ctrl.AsBinary();

            MakeAvatarToggles(binaryBuilder, cst.avatarToggles);
            MakeWearableToggles(binaryBuilder, wearableObject, cst.wearableToggles);

            MakeAvatarBlendshapeToggles(ctrl, binaryBuilder, cst.avatarBlendshapes);
            MakeWearableBlendshapeToggles(ctrl, binaryBuilder, wearableObject, cst.wearableBlendshapes);
        }

        private bool TryGetBlendshapeValue(GameObject obj, string blendshapeName, out float value)
        {
            value = -1.0f;

            SkinnedMeshRenderer smr;
            if ((smr = obj.GetComponent<SkinnedMeshRenderer>()) == null)
            {
                // _report.LogWarnLocalized(t, LogLabel, MessageCode.IgnoredObjectHasNoSkinnedMeshRendererAttached, obj.name);
                return false;
            }

            Mesh mesh;
            if ((mesh = smr.sharedMesh) == null)
            {
                // _report.LogWarnLocalized(t, LogLabel, MessageCode.IgnoredObjectHasNoMeshAttached, obj.name);
                return false;
            }

            int blendshapeIndex;
            if ((blendshapeIndex = mesh.GetBlendShapeIndex(blendshapeName)) == -1)
            {
                // _report.LogWarnLocalized(t, LogLabel, MessageCode.IgnoredObjectHasNoSuchBlendshape, obj.name, blendshapeName);
                return false;
            }

            value = smr.GetBlendShapeWeight(blendshapeIndex);
            return true;
        }

        private void MakeBlendshapeCustomizable(GameObject wearableObject, CabinetAnimWearableModuleConfig.Customizable cst, out DTSmartControl ctrl)
        {
            ctrl = wearableObject.AddComponent<DTSmartControl>();
            ctrl.AnimatorConfig.ParameterName = $"cpCA_{wearableObject.name}_{cst.name}_{DKEditorUtils.RandomString(8)}";
            var mtBuilder = ctrl.AsMotionTime();

            mtBuilder.WithCurve(AnimationCurve.Linear(0.0f, 0.0f, 100.0f / DefaultFrameRate, 100.0f));

            // TODO: animator config builder?

            var firstValue = -1.0f;

            foreach (var blendshape in cst.wearableBlendshapes)
            {
                var trans = wearableObject.transform.Find(blendshape.path);
                if (trans == null)
                {
                    // _report.LogWarnLocalized(t, LogLabel, MessageCode.IgnoredWearableBlendshapeObjectNotFound, _wearableObject.name, blendshape.path);
                    continue;
                }

                // we only use the first found value as default value
                if (firstValue == -1.0f && TryGetBlendshapeValue(trans.gameObject, blendshape.blendshapeName, out var value))
                {
                    firstValue = value / 100.0f;
                }

                // the 0.0f here has no effect
                mtBuilder.AddPropertyGroup(
                    ctrl.NewPropertyGroup()
                        .WithSelectedObjects(trans.gameObject)
                        .ChangeProperty($"blendShape.{blendshape.blendshapeName}", 0.0f)
                    );
            }

            if (firstValue != -1.0f)
            {
                ctrl.AnimatorConfig.ParameterDefaultValue = firstValue;
            }
        }

        private void MakeCustomizableToggles(GameObject wearableObject, CabinetAnimWearableModuleConfig module, out Dictionary<CabinetAnimWearableModuleConfig.Customizable, DTSmartControl> ctrls)
        {
            ctrls = new Dictionary<CabinetAnimWearableModuleConfig.Customizable, DTSmartControl>();

            foreach (var cst in module.wearableCustomizables)
            {
                if (cst.type == CabinetAnimWearableModuleConfig.CustomizableType.Toggle)
                {
                    MakeToggleCustomizable(wearableObject, cst, out var ctrl);
                    ctrls[cst] = ctrl;
                }
                else if (cst.type == CabinetAnimWearableModuleConfig.CustomizableType.Blendshape)
                {
                    MakeBlendshapeCustomizable(wearableObject, cst, out var ctrl);
                    ctrls[cst] = ctrl;
                }
            }
        }

        public void AddWearable(GameObject wearableObject, WearableConfig config, List<IDynamicsProxy> wearableDynamics)
        {
            var module = config.FindModuleConfig<CabinetAnimWearableModuleConfig>();
            if (module == null)
            {
                return;
            }

            MakeBasicToggles(wearableObject, wearableDynamics, module, out var basicCtrl);
            MakeCustomizableToggles(wearableObject, module, out var cstCtrls);

            Texture2D icon = null;
            if (_useThumbnails)
            {
                icon = OneConfUtils.GetTextureFromBase64(config.info.thumbnail);
                icon.Compress(true);
                _options.context.CreateUniqueAsset(icon, $"cpCA_Icon_{config.info.name}_{DKEditorUtils.RandomString(8)}.asset");
            }

            if (cstCtrls.Count == 0)
            {
                // just create a single toggle
                _menuGroup.Add(new ToggleItem()
                {
                    Name = config.info.name,
                    Icon = icon,
                    Controller = new AnimatorParameterController()
                    {
                        ParameterName = basicCtrl.AnimatorConfig.ParameterName,
                        ParameterValue = 1.0f
                    }
                });
            }
            else
            {
                // create a submenu and group them together
                var wearableMenu = new MenuGroup
                {
                    new ToggleItem()
                    {
                        Name = "Enabled", // TODO: allow to rename
                        Icon = icon,
                        Controller = new AnimatorParameterController()
                        {
                            ParameterName = basicCtrl.AnimatorConfig.ParameterName,
                            ParameterValue = 1.0f
                        }
                    }
                };

                foreach (var kvp in cstCtrls)
                {
                    var cstCtrl = kvp.Value;
                    if (cstCtrl.ControlType == DTSmartControl.SmartControlControlType.Binary)
                    {
                        wearableMenu.Add(new ToggleItem()
                        {
                            Name = kvp.Key.name,
                            Icon = null, // TODO: allow icons
                            Controller = new AnimatorParameterController()
                            {
                                ParameterName = cstCtrl.AnimatorConfig.ParameterName,
                                ParameterValue = 1.0f
                            }
                        });
                    }
                    else if (cstCtrl.ControlType == DTSmartControl.SmartControlControlType.MotionTime)
                    {
                        wearableMenu.Add(new RadialItem()
                        {
                            Name = kvp.Key.name,
                            Icon = null, // TODO: allow icons
                            RadialController = new AnimatorParameterController()
                            {
                                ParameterName = cstCtrl.AnimatorConfig.ParameterName,
                            }
                        });
                    }
                }

                _menuGroup.Add(new SubMenuItem()
                {
                    Name = config.info.name,
                    Icon = icon,
                    SubMenu = wearableMenu
                });
            }

            var list = new List<DTSmartControl>() { basicCtrl };
            list.AddRange(cstCtrls.Values);
            _ctrls[wearableObject] = list;
        }

        private void FillCrossControls()
        {
            // collect all controls first
            var allCtrls = new List<DTSmartControl>();
            foreach (var ctrls in _ctrls.Values)
            {
                foreach (var ctrl in ctrls)
                {
                    if (ctrl.ControlType == DTSmartControl.SmartControlControlType.Binary)
                    {
                        allCtrls.Add(ctrl);
                    }
                }
            }

            // iterate through dict to see if they are in the same collection, otherwise switch them off
            foreach (var ctrl in allCtrls)
            {
                var binaryBuilder = ctrl.AsBinary();
                foreach (var anotherCtrls in _ctrls.Values)
                {
                    if (!anotherCtrls.Contains(ctrl))
                    {
                        foreach (var anotherCtrl in anotherCtrls)
                        {
                            if (anotherCtrl.ControlType != DTSmartControl.SmartControlControlType.Binary)
                            {
                                continue;
                            }

                            binaryBuilder.CrossControlValueOnEnable(anotherCtrl, 0.0f);
                        }
                    }
                }
            }
        }

        public void Compose()
        {
            // create cross controls to turn off other clothes
            FillCrossControls();

            // write our root menu to context
            var store = _options.context.Feature<MenuStore>();
            store.Append(new SubMenuItem()
            {
                Name = "DT Cabinet", // TODO: allow to rename and add icon
                Icon = null,
                SubMenu = _menuGroup
            });
        }
    }
}
