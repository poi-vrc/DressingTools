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
using Chocopoi.DressingFramework.Menu;
using Chocopoi.DressingTools.Components.Animations;
using Chocopoi.DressingTools.Components.Modifiers;
using Chocopoi.DressingTools.Dynamics.Proxy;
using Chocopoi.DressingTools.OneConf.Cabinet.Modules.BuiltIn;
using Chocopoi.DressingTools.OneConf.Wearable;
using Chocopoi.DressingTools.OneConf.Wearable.Modules.BuiltIn;
using Chocopoi.DressingTools.OneConf.Wearable.Passes;
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
        private class WearableNameCount
        {
            public int count;
            public Dictionary<string, int> customizableCounts;

            public WearableNameCount()
            {
                count = 1;
                customizableCounts = new Dictionary<string, int>();
            }
        }

        private readonly AnimatorController _controller;
        private readonly Context _ctx;
        private readonly GameObject _avatarObject;
        private readonly MenuGroup _menuGroup;
        private readonly Dictionary<GameObject, List<DTSmartControl>> _ctrls;
        private readonly Dictionary<string, WearableNameCount> _wearableNameCounts;
        private readonly CabinetAnimCabinetModuleConfig _cabAnimConfig;

        public CabinetAnimComposer(Context ctx, AnimatorController controller, GameObject avatarObject, CabinetAnimCabinetModuleConfig cabAnimConfig)
        {
            _ctx = ctx;
            _controller = controller;
            _avatarObject = avatarObject;
            _menuGroup = new MenuGroup();
            _ctrls = new Dictionary<GameObject, List<DTSmartControl>>();
            _wearableNameCounts = new Dictionary<string, WearableNameCount>();
            _cabAnimConfig = cabAnimConfig;
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

        private string MakeUniqueWearableName(string name)
        {
            if (!_wearableNameCounts.ContainsKey(name))
            {
                _wearableNameCounts[name] = new WearableNameCount();
                return name;
            }
            return $"{name}_{++_wearableNameCounts[name].count}";
        }

        private string MakeUniqueWearableCustomizableName(string wearableName, string customizableName)
        {
            WearableNameCount wearableNameCount;
            if (!_wearableNameCounts.ContainsKey(wearableName))
            {
                _wearableNameCounts[wearableName] = new WearableNameCount();
            }
            wearableNameCount = _wearableNameCounts[wearableName];

            if (!wearableNameCount.customizableCounts.ContainsKey(customizableName))
            {
                wearableNameCount.customizableCounts[customizableName] = 1;
                return $"{customizableName}";
            }
            return $"{customizableName}_{++wearableNameCount.customizableCounts[customizableName]}";
        }

        private void MakeBasicToggles(GameObject wearableObject, string uniqueWearableName, CabinetAnimWearableModuleConfig module, out DTSmartControl ctrl)
        {
            // TODO: separate into different gameobjects?
            ctrl = wearableObject.AddComponent<DTSmartControl>();
            ctrl.AnimatorConfig.ParameterName = $"cpCA_{uniqueWearableName}";
            ctrl.AnimatorConfig.NetworkSynced = _cabAnimConfig.networkSynced;
            ctrl.AnimatorConfig.Saved = _cabAnimConfig.saved;
            var binaryBuilder = ctrl.AsBinary();

            MakeAvatarToggles(binaryBuilder, module.avatarAnimationOnWear.toggles);
            MakeWearableToggles(binaryBuilder, wearableObject, module.wearableAnimationOnWear.toggles);
            MakeAvatarBlendshapeToggles(ctrl, binaryBuilder, module.avatarAnimationOnWear.blendshapes);
            MakeWearableBlendshapeToggles(ctrl, binaryBuilder, wearableObject, module.wearableAnimationOnWear.blendshapes);

            var dynamicsContainer = wearableObject.transform.Find(GroupDynamicsWearablePass.DynamicsContainerName);
            if (dynamicsContainer != null && dynamicsContainer.TryGetComponent<DTGroupDynamics>(out var comp))
            {
                binaryBuilder.Toggle(comp, true);
            }
        }

        private void MakeToggleCustomizable(GameObject wearableObject, string uniqueWearableName, CabinetAnimWearableModuleConfig.Customizable cst, out DTSmartControl ctrl)
        {
            // TODO: separate into different gameobjects?
            // TODO: animator config builder?
            ctrl = wearableObject.AddComponent<DTSmartControl>();
            ctrl.AnimatorConfig.ParameterName = $"cpCA_{uniqueWearableName}_{MakeUniqueWearableCustomizableName(uniqueWearableName, cst.name)}";
            ctrl.AnimatorConfig.NetworkSynced = cst.networkSynced;
            ctrl.AnimatorConfig.Saved = cst.saved;
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

        private void MakeBlendshapeCustomizable(GameObject wearableObject, string uniqueWearableName, CabinetAnimWearableModuleConfig.Customizable cst, out DTSmartControl ctrl)
        {
            ctrl = wearableObject.AddComponent<DTSmartControl>();
            ctrl.AnimatorConfig.ParameterName = $"cpCA_{uniqueWearableName}_{MakeUniqueWearableCustomizableName(uniqueWearableName, cst.name)}";
            ctrl.AnimatorConfig.NetworkSynced = cst.networkSynced;
            ctrl.AnimatorConfig.Saved = cst.saved;
            var mtBuilder = ctrl.AsMotionTime();

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

                mtBuilder.AddPropertyGroup(
                    ctrl.NewPropertyGroup()
                        .WithSelectedObjects(trans.gameObject)
                        .ChangeProperty($"blendShape.{blendshape.blendshapeName}", 0.0f, 100.0f)
                    );
            }

            if (firstValue != -1.0f)
            {
                ctrl.AnimatorConfig.ParameterDefaultValue = firstValue;
            }
        }

        private void MakeCustomizableToggles(GameObject wearableObject, string uniqueWearableName, CabinetAnimWearableModuleConfig module, out Dictionary<CabinetAnimWearableModuleConfig.Customizable, DTSmartControl> ctrls)
        {
            ctrls = new Dictionary<CabinetAnimWearableModuleConfig.Customizable, DTSmartControl>();

            foreach (var cst in module.wearableCustomizables)
            {
                if (cst.type == CabinetAnimWearableModuleConfig.CustomizableType.Toggle)
                {
                    MakeToggleCustomizable(wearableObject, uniqueWearableName, cst, out var ctrl);
                    ctrls[cst] = ctrl;
                }
                else if (cst.type == CabinetAnimWearableModuleConfig.CustomizableType.Blendshape)
                {
                    MakeBlendshapeCustomizable(wearableObject, uniqueWearableName, cst, out var ctrl);
                    ctrls[cst] = ctrl;
                }
            }
        }

        public void AddWearable(GameObject wearableObject, WearableConfig config)
        {
            var module = config.FindModuleConfig<CabinetAnimWearableModuleConfig>();
            if (module == null)
            {
                return;
            }

            var uniqueWearableName = MakeUniqueWearableName(string.IsNullOrEmpty(config.info.name) ? wearableObject.name : config.info.name);
            MakeBasicToggles(wearableObject, uniqueWearableName, module, out var basicCtrl);
            MakeCustomizableToggles(wearableObject, uniqueWearableName, module, out var cstCtrls);

            Texture2D icon = null;
            if (_cabAnimConfig.thumbnails && !string.IsNullOrEmpty(config.info.thumbnail))
            {
                icon = OneConfUtils.GetTextureFromBase64(config.info.thumbnail);
                icon.Compress(true);
                _ctx.CreateUniqueAsset(icon, $"cpCA_Icon_{config.info.name}_{DKEditorUtils.RandomString(8)}.asset");
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
                    if (cstCtrl.ControlType == DTSmartControl.SCControlType.Binary)
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
                    else if (cstCtrl.ControlType == DTSmartControl.SCControlType.MotionTime)
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

        private void FindAndDisableControls(GameObject wearableObj, DTSmartControl ctrl)
        {
            var binaryBuilder = ctrl.AsBinary();
            foreach (var kvp in _ctrls)
            {
                var anotherObj = kvp.Key;
                var anotherCtrls = kvp.Value;
                if (anotherObj == wearableObj)
                {
                    // skip our own collection
                    continue;
                }

                if (!_cabAnimConfig.resetCustomizablesOnSwitch)
                {
                    // only obtain the first ctrl (basic toggles)
                    // we expect this is binary already so skipping the check
                    var anotherCtrl = anotherCtrls[0];
                    binaryBuilder.CrossControlValueOnEnable(anotherCtrl, 0.0f);
                    continue;
                }

                foreach (var anotherCtrl in anotherCtrls)
                {
                    if (anotherCtrl.ControlType != DTSmartControl.SCControlType.Binary)
                    {
                        continue;
                    }
                    binaryBuilder.CrossControlValueOnEnable(anotherCtrl, 0.0f);
                }
            }
        }

        private void FillCrossControls()
        {
            foreach (var kvp in _ctrls)
            {
                var wearableObj = kvp.Key;
                var basicTogglesCtrl = kvp.Value[0];

                if (basicTogglesCtrl.ControlType != DTSmartControl.SCControlType.Binary)
                {
                    continue;
                }
                FindAndDisableControls(wearableObj, basicTogglesCtrl);
            }
        }

        public void Compose()
        {
            if (_ctrls.Count == 0)
            {
                // do not do anything if we don't have any cabinet anims to generate
                return;
            }

            // create cross controls to turn off other clothes
            FillCrossControls();

            // write our root menu to context
            var store = _ctx.Feature<MenuStore>();
            store.Append(new SubMenuItem()
            {
                Name = _cabAnimConfig.menuItemName,
                Icon = null, // TODO: add icon
                SubMenu = _menuGroup
            }, _cabAnimConfig.menuInstallPath);
        }
    }
}
