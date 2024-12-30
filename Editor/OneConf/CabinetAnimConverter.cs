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
using Chocopoi.AvatarLib.Animations;
using Chocopoi.DressingFramework;
using Chocopoi.DressingTools.Components.Animations;
using Chocopoi.DressingTools.Components.Cabinet;
using Chocopoi.DressingTools.Components.Menu;
using Chocopoi.DressingTools.Components.Modifiers;
using Chocopoi.DressingTools.OneConf.Cabinet;
using Chocopoi.DressingTools.OneConf.Cabinet.Modules.BuiltIn;
using Chocopoi.DressingTools.OneConf.Wearable;
using Chocopoi.DressingTools.OneConf.Wearable.Modules.BuiltIn;
using UnityEngine;

namespace Chocopoi.DressingTools.OneConf
{
    /// <summary>
    /// This class converts OneConf cabinet animation into multiple components
    /// </summary>
    internal class CabinetAnimConverter
    {
        private const string DynamicsContainerName = "DT_Dynamics";

        private readonly Context _ctx;
        private readonly GameObject _avatarObject;
        private readonly CabinetConfig _cabConf;
        private CabinetAnimCabinetModuleConfig _cabAnimConf;
        private readonly Dictionary<GameObject, WearableConfig> _wearConfs;

        public CabinetAnimConverter(Context ctx, GameObject avatarObject, CabinetConfig cabConf, Dictionary<GameObject, WearableConfig> wearConfs)
        {
            _ctx = ctx;
            _avatarObject = avatarObject;
            _cabConf = cabConf;
            _cabAnimConf = null;
            _wearConfs = wearConfs;
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

        private void AddToggles(List<DTSmartControl.ObjectToggle> scToggles, List<CabinetAnimWearableModuleConfig.Toggle> confToggles, Func<string, Transform> getTransFunc)
        {
            foreach (var toggle in confToggles)
            {
                var trans = getTransFunc(toggle.path);
                if (trans == null)
                {
                    continue;
                }
                scToggles.Add(new DTSmartControl.ObjectToggle()
                {
                    Target = trans,
                    Enabled = toggle.state
                });
            }
        }

        private void AddAvatarToggles(List<DTSmartControl.ObjectToggle> scToggles, List<CabinetAnimWearableModuleConfig.Toggle> confToggles)
        {
            AddToggles(scToggles, confToggles, path => GetAvatarTransform(path));
        }

        private void AddWearableToggles(List<DTSmartControl.ObjectToggle> scToggles, GameObject wearableObject, List<CabinetAnimWearableModuleConfig.Toggle> confToggles)
        {
            AddToggles(scToggles, confToggles, path => GetWearableTransform(wearableObject, path));
        }

        private void AddBlendshapeToggles(List<DTSmartControl.PropertyGroup> scPropGps, List<CabinetAnimWearableModuleConfig.BlendshapeValue> confBss, Func<string, Transform> getTransFunc)
        {
            foreach (var bs in confBss)
            {
                var trans = getTransFunc(bs.path);
                if (trans == null)
                {
                    continue;
                }
                var builder = new DTSmartControl.PropertyGroupBuilder(new DTSmartControl.PropertyGroup());
                builder.WithSelectedObjects(trans.gameObject)
                        .ChangeProperty($"blendShape.{bs.blendshapeName}", bs.value);
                scPropGps.Add(builder.Build());
            }
        }

        private void AddAvatarBlendshapeToggles(List<DTSmartControl.PropertyGroup> scPropGps, List<CabinetAnimWearableModuleConfig.BlendshapeValue> confBss)
        {
            AddBlendshapeToggles(scPropGps, confBss, path => GetAvatarTransform(path));
        }

        private void AddWearableBlendshapeToggles(List<DTSmartControl.PropertyGroup> scPropGps, GameObject wearableObject, List<CabinetAnimWearableModuleConfig.BlendshapeValue> confBss)
        {
            AddBlendshapeToggles(scPropGps, confBss, path => GetWearableTransform(wearableObject, path));
        }

        private void MakeGroupDynamics(GameObject wearableObject, CabinetAnimWearableModuleConfig module, DTAlternateOutfit outfit)
        {
            if (!_cabConf.groupDynamics)
            {
                return;
            }

            // create dynamics container (reuse if originally have)
            var dynamicsContainer = wearableObject.transform.Find(DynamicsContainerName);
            if (dynamicsContainer == null)
            {
                var obj = new GameObject(DynamicsContainerName);
                obj.transform.SetParent(wearableObject.transform);
                dynamicsContainer = obj.transform;
            }

            var comp = dynamicsContainer.gameObject.AddComponent<DTGroupDynamics>();
            comp.SearchMode = DTGroupDynamics.DynamicsSearchMode.ControlRoot;
            comp.SeparateGameObjects = _cabConf.groupDynamicsSeparateGameObjects;
            comp.IncludeTransforms.Add(wearableObject.transform);
            if (module.setWearableDynamicsInactive)
            {
                comp.SetToCurrentState = true;
                comp.enabled = false;
            }
            else
            {
                comp.SetToCurrentState = false;
                comp.enabled = false;
            }
            outfit.GroupDynamics = comp;
        }

        private void MakeToggleCustomizable(GameObject wearableObject, Transform menuGroupRoot, CabinetAnimWearableModuleConfig.Customizable cst)
        {
            var ctrlContainer = new GameObject(cst.name);
            ctrlContainer.transform.SetParent(menuGroupRoot.transform);

            var ctrl = ctrlContainer.AddComponent<DTSmartControl>();
            ctrl.DriverType = DTSmartControl.SCDriverType.MenuItem;
            ctrl.MenuItemDriverConfig.ItemType = DTMenuItem.ItemType.Toggle;
            ctrl.AnimatorConfig.NetworkSynced = cst.networkSynced;
            ctrl.AnimatorConfig.Saved = cst.saved;

            AddAvatarToggles(ctrl.ObjectToggles, cst.avatarToggles);
            AddWearableToggles(ctrl.ObjectToggles, wearableObject, cst.wearableToggles);
            AddAvatarBlendshapeToggles(ctrl.PropertyGroups, cst.avatarBlendshapes);
            AddWearableBlendshapeToggles(ctrl.PropertyGroups, wearableObject, cst.wearableBlendshapes);
        }

        private bool TryGetBlendshapeValue(GameObject obj, string blendshapeName, out float value)
        {
            value = -1.0f;

            SkinnedMeshRenderer smr;
            if ((smr = obj.GetComponent<SkinnedMeshRenderer>()) == null)
            {
                return false;
            }

            Mesh mesh;
            if ((mesh = smr.sharedMesh) == null)
            {
                return false;
            }

            int blendshapeIndex;
            if ((blendshapeIndex = mesh.GetBlendShapeIndex(blendshapeName)) == -1)
            {
                return false;
            }

            value = smr.GetBlendShapeWeight(blendshapeIndex);
            return true;
        }

        private void MakeBlendshapeCustomizable(GameObject wearableObject, Transform menuGroupRoot, CabinetAnimWearableModuleConfig.Customizable cst)
        {
            var ctrlContainer = new GameObject(cst.name);
            ctrlContainer.transform.SetParent(menuGroupRoot.transform);

            var ctrl = ctrlContainer.AddComponent<DTSmartControl>();
            ctrl.DriverType = DTSmartControl.SCDriverType.MenuItem;
            ctrl.MenuItemDriverConfig.ItemType = DTMenuItem.ItemType.Radial;
            ctrl.AnimatorConfig.NetworkSynced = cst.networkSynced;
            ctrl.AnimatorConfig.Saved = cst.saved;
            var mtBuilder = ctrl.AsMotionTime();

            var firstValue = -1.0f;

            foreach (var blendshape in cst.wearableBlendshapes)
            {
                var trans = wearableObject.transform.Find(blendshape.path);
                if (trans == null)
                {
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

        private void MakeCustomizableToggles(GameObject wearableObject, CabinetAnimWearableModuleConfig module, Transform menuGroupRoot)
        {
            foreach (var cst in module.wearableCustomizables)
            {
                if (cst.type == CabinetAnimWearableModuleConfig.CustomizableType.Toggle)
                {
                    MakeToggleCustomizable(wearableObject, menuGroupRoot, cst);
                }
                else if (cst.type == CabinetAnimWearableModuleConfig.CustomizableType.Blendshape)
                {
                    MakeBlendshapeCustomizable(wearableObject, menuGroupRoot, cst);
                }
            }
        }

        private void ConvertWearable(GameObject wearableObject, WearableConfig wearableConfig)
        {
            var module = wearableConfig.FindModuleConfig<CabinetAnimWearableModuleConfig>();
            if (module == null)
            {
                return;
            }

            var outfit = wearableObject.AddComponent<DTAlternateOutfit>();

            Texture2D icon = null;
            if (_cabAnimConf.thumbnails && !string.IsNullOrEmpty(wearableConfig.info.thumbnail))
            {
                icon = OneConfUtils.GetTextureFromBase64(wearableConfig.info.thumbnail);
                icon.Compress(true);
                _ctx.CreateUniqueAsset(icon, $"cpCA_Icon_{wearableConfig.info.name}");
            }
            outfit.Name = wearableConfig.info.name;
            outfit.Icon = icon;

            AddAvatarToggles(outfit.ObjectToggles, module.avatarAnimationOnWear.toggles);
            AddWearableToggles(outfit.ObjectToggles, wearableObject, module.wearableAnimationOnWear.toggles);
            AddAvatarBlendshapeToggles(outfit.PropertyGroups, module.avatarAnimationOnWear.blendshapes);
            AddWearableBlendshapeToggles(outfit.PropertyGroups, wearableObject, module.wearableAnimationOnWear.blendshapes);

            if (module.wearableCustomizables.Count > 0)
            {
                var custContainer = new GameObject("DT_Menu");
                custContainer.transform.SetParent(wearableObject.transform);

                var menuGroup = custContainer.AddComponent<DTMenuGroup>();
                outfit.MenuGroup = menuGroup;

                var enableItemObj = new GameObject("Enable"); // TODO
                enableItemObj.transform.SetParent(menuGroup.transform);
                var enableItem = enableItemObj.AddComponent<DTOutfitEnableMenuItem>();
                enableItem.TargetOutfit = outfit;
                enableItem.Icon = icon;

                MakeCustomizableToggles(wearableObject, module, custContainer.transform);
            }

            MakeGroupDynamics(wearableObject, module, outfit);
        }

        private bool IsGenerationNeeded()
        {
            return _wearConfs.Values.Any(v => v.FindModuleConfig<CabinetAnimWearableModuleConfig>() != null);
        }

        public void Convert()
        {
            if (!IsGenerationNeeded())
            {
                return;
            }

            _cabAnimConf = _cabConf.FindModuleConfig<CabinetAnimCabinetModuleConfig>();
            // use default if not exist
            _cabAnimConf ??= new CabinetAnimCabinetModuleConfig();

            // for compatibility reasons, we cannot use this as menu group
            // since if we add this component to avatar root, using this as menu
            // group will grab all existing menu items that users might have already
            // put in.
            // alternatively, we could move the existing wearables into a new location
            // but it will certainly break things. so for old setups, it has to use
            // DK API initially for the cabinet menu.
            var wardrobe = _avatarObject.AddComponent<DTWardrobe>();
            wardrobe.UseAsMenuGroup = false;
            wardrobe.MenuInstallPath = _cabAnimConf.menuInstallPath;
            wardrobe.MenuItemName = _cabAnimConf.menuItemName;
            wardrobe.MenuItemIcon = null; // not supported in OneConf
            wardrobe.NetworkSynced = _cabAnimConf.networkSynced;
            wardrobe.Saved = _cabAnimConf.saved;
            wardrobe.ResetControlsOnSwitch = _cabAnimConf.resetCustomizablesOnSwitch;

            foreach (var kvp in _wearConfs)
            {
                ConvertWearable(kvp.Key, kvp.Value);
            }
        }
    }
}
