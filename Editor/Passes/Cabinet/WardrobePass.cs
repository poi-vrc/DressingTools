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
using System.Linq;
using Chocopoi.AvatarLib.Animations;
using Chocopoi.DressingFramework;
using Chocopoi.DressingFramework.Extensibility.Sequencing;
using Chocopoi.DressingFramework.Localization;
using Chocopoi.DressingFramework.Menu;
using Chocopoi.DressingTools.Components.Animations;
using Chocopoi.DressingTools.Components.Cabinet;
using Chocopoi.DressingTools.Components.Menu;
using Chocopoi.DressingTools.Dynamics;
using Chocopoi.DressingTools.Localization;
using UnityEngine;

namespace Chocopoi.DressingTools.Passes.Modifiers
{
    internal class WardrobePass : BuildPass
    {
        private const string LogLabel = "WardrobePass";
        private const float BaseValue = 0.0f;
        private static readonly I18nTranslator t = I18n.ToolTranslator;

        public override BuildConstraint Constraint =>
            InvokeAtStage(BuildStage.Generation)
                .Build();

        private static T GetOrAddComponent<T>(GameObject gameObject) where T : Component
        {
            if (gameObject.TryGetComponent<T>(out var comp))
            {
                return comp;
            }
            return gameObject.AddComponent<T>();
        }

        private static List<IDynamics> FindRequiredDynamics(Transform root, DTOutfitItem.RequiredDynamicsConfig reqDynConf)
        {
            var allDynamics = DynamicsUtils.ScanDynamics(root.gameObject);

            if (reqDynConf.SearchMode == DTOutfitItem.RequiredDynamicsConfig.DynamicsSearchMode.ControlRoot)
            {
                return allDynamics.Where(d => reqDynConf.Targets.Any(t => d.RootTransforms.Contains(t))).ToList();
            }
            else if (reqDynConf.SearchMode == DTOutfitItem.RequiredDynamicsConfig.DynamicsSearchMode.ComponentRoot)
            {
                return allDynamics.Where(d => reqDynConf.Targets.Any(t => d.Transform == t)).ToList();
            }

            return null;
        }

        private static bool LookUpForWardrobe(Transform root, out DTWardrobe wardrobe)
        {
            while (root != null)
            {
                if (root.TryGetComponent<DTWardrobe>(out var wardrobeComp))
                {
                    wardrobe = wardrobeComp;
                    return true;
                }
                root = root.parent;
            }
            wardrobe = null;
            return false;
        }

        private static bool LookUpForOutfit(Transform root, out IOutfit outfit)
        {
            while (root != null)
            {
                if (root.TryGetComponent<DTAlternateOutfit>(out var altComp))
                {
                    outfit = altComp;
                    return true;
                }
                if (root.TryGetComponent<DTBaseOutfit>(out var baseComp))
                {
                    outfit = baseComp;
                    return true;
                }
                root = root.parent;
            }
            outfit = null;
            return false;
        }

        private static void CollectExternalOutfitItemControls(Transform avatarRoot, DTExternalOutfitItem extItem, out List<DTSmartControl.ObjectToggle> objectToggles, out List<DTSmartControl.PropertyGroup> propertyGroups, out DTSmartControl.SCCrossControlActions crossControlActions)
        {
            objectToggles = new List<DTSmartControl.ObjectToggle>(extItem.SourceItem.ObjectToggles);
            propertyGroups = new List<DTSmartControl.PropertyGroup>(extItem.SourceItem.PropertyGroups);
            crossControlActions = extItem.SourceItem.CrossControlActions; // TODO: deep copy

            // since it's external, we need to turn on the required dynamics
            if (extItem.SourceItem.UseRequiredDynamicsOnly)
            {
                // use the required dynamics config
                var reqDyns = FindRequiredDynamics(avatarRoot, extItem.SourceItem.RequiredDynamics);
                foreach (var dyn in reqDyns)
                {
                    objectToggles.Add(new DTSmartControl.ObjectToggle()
                    {
                        Target = dyn.Component,
                        Enabled = true
                    });
                }
            }
            else if (LookUpForOutfit(extItem.SourceItem.transform, out var outfit))
            {
                // toggle the group dynamics object if exist
                if (outfit.GroupDynamics != null &&
                    objectToggles.Where(t => t.Target == outfit.GroupDynamics).Count() == 0)
                {
                    objectToggles.Add(new DTSmartControl.ObjectToggle()
                    {
                        Target = outfit.GroupDynamics,
                        Enabled = true
                    });
                }
            }
        }

        private static void ComposeExternalOutfitItems(Transform avatarRoot)
        {
            // generate menu items
            var extItemCtrlCache = new Dictionary<DTExternalOutfitItem, DTSmartControl>();
            var extItemComps = avatarRoot.GetComponentsInChildren<DTExternalOutfitItem>(true);
            foreach (var extItem in extItemComps)
            {
                var sc = extItem.gameObject.AddComponent<DTSmartControl>();
                sc.DriverType = extItem.GenerateMenuItem ?
                    DTSmartControl.SCDriverType.MenuItem :
                    DTSmartControl.SCDriverType.AnimatorParameter;
                sc.MenuItemDriverConfig.ItemType = DTMenuItem.ItemType.Toggle;
                sc.MenuItemDriverConfig.ItemIcon = extItem.SourceItem.Icon;
                sc.ControlType = DTSmartControl.SCControlType.Binary;

                CollectExternalOutfitItemControls(avatarRoot, extItem, out var objectToggles, out var propertyGroups, out var crossControlActions);
                sc.ObjectToggles = objectToggles;
                sc.PropertyGroups = propertyGroups;
                sc.CrossControlActions = crossControlActions;

                extItemCtrlCache[extItem] = sc;
            }

            // change all smartcontrols containing externaloutfititem to controls
            var allCtrls = avatarRoot.GetComponentsInChildren<DTSmartControl>(true);
            foreach (var ctrl in allCtrls)
            {
                if (ctrl.ControlType != DTSmartControl.SCControlType.Binary)
                {
                    continue;
                }

                // find ext items and remove those toggles
                var toRmv = new List<DTSmartControl.ObjectToggle>();
                foreach (var toggle in ctrl.ObjectToggles)
                {
                    if (toggle.Target is DTExternalOutfitItem extItem)
                    {
                        if (!extItemCtrlCache.TryGetValue(extItem, out var extItemCtrl))
                        {
                            // skip this
                            continue;
                        }

                        ctrl.AsBinary()
                            .CrossControlValueOnEnable(extItemCtrl, toggle.Enabled ? 1.0f : 0.0f)
                            .CrossControlValueOnDisable(extItemCtrl, extItem.SourceItem.DefaultValue);
                        toRmv.Add(toggle);
                    }
                }
                ctrl.ObjectToggles.RemoveAll(t => toRmv.Contains(t));
            }
        }

        private static void ComposeOutfitItems(Transform parent)
        {
            var outfitItems = parent.GetComponentsInChildren<DTOutfitItem>(true);
            foreach (var outfitItem in outfitItems)
            {
                // generate menu items
                if (!outfitItem.GenerateMenuItem)
                {
                    continue;
                }

                var sc = outfitItem.gameObject.AddComponent<DTSmartControl>();
                sc.DriverType = DTSmartControl.SCDriverType.MenuItem;
                sc.AnimatorConfig.ParameterDefaultValue = outfitItem.DefaultValue;
                sc.MenuItemDriverConfig.ItemType = DTMenuItem.ItemType.Toggle;
                sc.MenuItemDriverConfig.ItemIcon = outfitItem.Icon;
                sc.ControlType = DTSmartControl.SCControlType.Binary;
                sc.ObjectToggles = outfitItem.ObjectToggles;
                sc.PropertyGroups = outfitItem.PropertyGroups;
                sc.CrossControlActions = outfitItem.CrossControlActions;

                // the original outfit item itself doesn't need to use
                // the required dynamics config, since the dynamics should be
                // already turned on
            }
        }

        private static void CreateOutfitSubMenu(Context ctx, DTWardrobe wardrobeComp, Transform outfitTransform, IOutfit outfit)
        {
            if (wardrobeComp.UseAsMenuGroup)
            {
                var subMenuItem = outfitTransform.gameObject.AddComponent<DTMenuItem>();
                subMenuItem.Type = DTMenuItem.ItemType.SubMenu;
                subMenuItem.Name = outfit.Name;
                subMenuItem.Icon = outfit.Icon;
                subMenuItem.SubMenuType = DTMenuItem.ItemSubMenuType.DTMenuGroupComponent;
                subMenuItem.DTSubMenu = outfit.MenuGroup;
            }
            else
            {
                var menuItemInstallPath = GetMenuItemInstallPath(wardrobeComp, outfitTransform);
                var store = ctx.Feature<MenuStore>();
                store.Append(new SubMenuItem()
                {
                    Name = outfit.Name,
                    Icon = outfit.Icon,
                }, menuItemInstallPath);

                var menuInstall = GetOrAddComponent<DTMenuInstall>(outfit.MenuGroup.gameObject);
                menuInstall.InstallPath = $"{menuItemInstallPath}/{outfit.Name}";
            }
        }

        private static void CreateOutfitEnableItem(Context ctx, DTWardrobe wardrobeComp, DTParameterSlot slot, Transform outfitTransform, IOutfit outfit, float mappedVal)
        {
            if (wardrobeComp.UseAsMenuGroup)
            {
                // let the step later to handle it
                var outfitEnableComp = GetOrAddComponent<DTOutfitEnableMenuItem>(outfitTransform.gameObject);
                outfitEnableComp.Icon = outfit.Icon;
                outfitEnableComp.TargetOutfit = outfit is DTAlternateOutfit altOutfit ? altOutfit : null;
            }
            else
            {
                // need to explicitly write to menu store
                var menuItemInstallPath = wardrobeComp.MenuItemName;
                if (!string.IsNullOrEmpty(wardrobeComp.MenuInstallPath))
                {
                    menuItemInstallPath = $"{wardrobeComp.MenuInstallPath}/{menuItemInstallPath}";
                }
                if (outfit.MenuGroup != null)
                {
                    // if outfit has menu group, this needs to be installed under the outfit's menu group
                    menuItemInstallPath += $"/{outfit.Name}";
                }
                var store = ctx.Feature<MenuStore>();
                store.Append(new ToggleItem()
                {
                    Name = outfit.MenuGroup != null ? "Enable" : outfit.Name, // TODO
                    Icon = outfit.Icon,
                    Controller = new AnimatorParameterController()
                    {
                        ParameterName = slot.ParameterName,
                        ParameterValue = mappedVal
                    }
                }, menuItemInstallPath);
            }
        }

        private static void ComposeBaseOutfit(Context ctx, DTWardrobe wardrobeComp, DTParameterSlot slot, DTBaseOutfit baseOutfitComp)
        {
            ComposeOutfitItems(baseOutfitComp.transform);

            if (baseOutfitComp.TryGetComponent<DTSmartControl>(out var existingSc))
            {
                // destroy any existing SC
                Object.DestroyImmediate(existingSc);
                ctx.Report.LogWarn(LogLabel, $"There is an existing SmartControl on {baseOutfitComp.name}, it will be removed.");
            }

            // a dummy base control with no controls
            var sc = baseOutfitComp.gameObject.AddComponent<DTSmartControl>();
            sc.DriverType = DTSmartControl.SCDriverType.ParameterSlot;
            sc.ParameterSlotConfig.ParameterSlot = slot;
            sc.ParameterSlotConfig.MappedValue = BaseValue;

            if (baseOutfitComp.MenuGroup == null)
            {
                // if no menu group, just create a enable menu item
                CreateOutfitEnableItem(ctx, wardrobeComp, slot, baseOutfitComp.transform, baseOutfitComp, BaseValue);
                return;
            }

            // for maxmimum customization, we just create a new submenu item to that menu group
            CreateOutfitSubMenu(ctx, wardrobeComp, baseOutfitComp.transform, baseOutfitComp);
        }

        private static void ComposeAlternateOutfit(Context ctx, DTWardrobe wardrobeComp, DTParameterSlot slot, float mappedVal, DTAlternateOutfit altOutfitComp)
        {
            ComposeOutfitItems(altOutfitComp.transform);

            if (altOutfitComp.TryGetComponent<DTSmartControl>(out var existingSc))
            {
                // destroy any existing SC
                Object.DestroyImmediate(existingSc);
                ctx.Report.LogWarn(LogLabel, $"There is an existing SmartControl on {altOutfitComp.name}, it will be removed.");
            }

            var sc = altOutfitComp.gameObject.AddComponent<DTSmartControl>();
            sc.DriverType = DTSmartControl.SCDriverType.ParameterSlot;
            sc.ParameterSlotConfig.ParameterSlot = slot;
            sc.ParameterSlotConfig.MappedValue = mappedVal;
            sc.ControlType = DTSmartControl.SCControlType.Binary;
            sc.ObjectToggles = altOutfitComp.ObjectToggles;
            sc.PropertyGroups = altOutfitComp.PropertyGroups;
            sc.CrossControlActions = altOutfitComp.CrossControlActions;

            if (altOutfitComp.GroupDynamics != null &&
                sc.ObjectToggles.Where(t => t.Target == altOutfitComp.GroupDynamics).Count() == 0)
            {
                // add toggle if not exist
                sc.ObjectToggles.Add(new DTSmartControl.ObjectToggle()
                {
                    Target = altOutfitComp.GroupDynamics,
                    Enabled = true,
                });
            }

            if (altOutfitComp.MenuGroup == null)
            {
                // if no menu group, just create a enable menu item
                CreateOutfitEnableItem(ctx, wardrobeComp, slot, altOutfitComp.transform, altOutfitComp, mappedVal);
                return;
            }

            // for maxmimum customization, we just create a new submenu item to that menu group
            CreateOutfitSubMenu(ctx, wardrobeComp, altOutfitComp.transform, altOutfitComp);
        }

        private static void VisitComponents(Context ctx, DTWardrobe wardrobeComp, DTParameterSlot slot, Transform parent, ref DTBaseOutfit baseOutfit, List<DTAlternateOutfit> altOutfits)
        {
            for (var i = 0; i < parent.childCount; i++)
            {
                var child = parent.GetChild(i);

                if (child.TryGetComponent<DTOutfitGroup>(out var outfitGpComp))
                {
                    if (wardrobeComp.UseAsMenuGroup)
                    {
                        // if used as menu group, we can directly add a submenu item here
                        var subMenuItem = GetOrAddComponent<DTMenuItem>(outfitGpComp.gameObject);
                        subMenuItem.Type = DTMenuItem.ItemType.SubMenu;
                        subMenuItem.Name = outfitGpComp.Name;
                        subMenuItem.Icon = outfitGpComp.Icon;
                        subMenuItem.SubMenuType = DTMenuItem.ItemSubMenuType.Children;
                    }
                    else
                    {
                        // if not, we do some magic here to add an empty submenu via DK API
                        // and add menu install to append the underlying items seamlessly
                        var cabMenuPath = $"{wardrobeComp.MenuInstallPath}/{wardrobeComp.MenuItemName}";
                        var store = ctx.Feature<MenuStore>();
                        store.Append(new SubMenuItem()
                        {
                            Name = outfitGpComp.Name,
                            Icon = outfitGpComp.Icon
                        }, cabMenuPath);

                        GetOrAddComponent<DTMenuGroup>(outfitGpComp.gameObject);
                        var menuInstall = GetOrAddComponent<DTMenuInstall>(outfitGpComp.gameObject);
                        menuInstall.InstallPath = $"{cabMenuPath}/{outfitGpComp.Name}";
                    }
                    VisitComponents(ctx, wardrobeComp, slot, outfitGpComp.transform, ref baseOutfit, altOutfits);
                    continue;
                }

                if (child.TryGetComponent<DTAlternateOutfit>(out var altOutfitComp))
                {
                    altOutfits.Add(altOutfitComp);
                    continue;
                }

                if (child.TryGetComponent<DTBaseOutfit>(out var baseOutfitComp))
                {
                    if (baseOutfit != null)
                    {
                        ctx.Report.LogWarn(LogLabel, "There are more than one DTBaseOutfit components. Only the first one will be processed.");
                        continue;
                    }
                    baseOutfit = baseOutfitComp;
                    continue;
                }
            }
        }

        private static bool IsCrossControllable(DTSmartControl sc)
        {
            return (sc.DriverType == DTSmartControl.SCDriverType.AnimatorParameter ||
            sc.DriverType == DTSmartControl.SCDriverType.MenuItem) &&
            sc.ControlType == DTSmartControl.SCControlType.Binary;
        }

        private static void CrossTurnOffControls(DTAlternateOutfit altOutfit, List<DTSmartControl> ctrls)
        {
            foreach (var ctrl in ctrls)
            {
                if (altOutfit.CrossControlActions.ValueActions.ValuesOnEnable.Where(voe => voe.Control == ctrl).Count() > 0)
                {
                    continue;
                }
                altOutfit.CrossControlActions.ValueActions.ValuesOnEnable.Add(new DTSmartControl.SCCrossControlActions.ControlValueActions.ControlValue()
                {
                    Control = ctrl,
                    Value = 0.0f
                });
            }
        }

        private static string GetMenuItemInstallPath(DTWardrobe wardrobeComp, Transform itemTransform)
        {
            var menuItemInstallPath = wardrobeComp.MenuItemName;
            if (!string.IsNullOrEmpty(wardrobeComp.MenuInstallPath))
            {
                menuItemInstallPath = $"{wardrobeComp.MenuInstallPath}/{menuItemInstallPath}";
            }
            if (itemTransform.parent != wardrobeComp.transform)
            {
                menuItemInstallPath += $"/{AnimationUtils.GetRelativePath(itemTransform, wardrobeComp.transform)}";
            }
            return menuItemInstallPath;
        }

        private static void ComposeOutfitEnableItems(Context ctx, DTParameterSlot slot, DTBaseOutfit baseOutfit, List<DTAlternateOutfit> altOutfits)
        {
            var outfitEnableItems = ctx.AvatarGameObject.GetComponentsInChildren<DTOutfitEnableMenuItem>(true);
            foreach (var outfitEnableItem in outfitEnableItems)
            {
                float mappedValue = BaseValue;
                if (outfitEnableItem.TargetOutfit != null)
                {
                    // TODO: allow giving custom slot map value, now using index directly
                    var idx = altOutfits.IndexOf(outfitEnableItem.TargetOutfit);
                    if (idx != -1)
                    {
                        mappedValue = idx + 1;
                    }
                }

                if (LookUpForWardrobe(outfitEnableItem.transform, out var wardrobeComp) && !wardrobeComp.UseAsMenuGroup)
                {
                    // look up for wardrobe. if found, check if not used as menu group
                    IOutfit outfit;
                    if (outfitEnableItem.TargetOutfit != null)
                    {
                        outfit = outfitEnableItem.TargetOutfit;
                    }
                    else
                    {
                        outfit = baseOutfit;
                    }
                    var menuGroupRoot = outfit.MenuGroup != null ? outfit.MenuGroup.transform : wardrobeComp.transform;
                    var store = ctx.Feature<MenuStore>();

                    // prepare install path
                    var menuItemInstallPath = wardrobeComp.MenuItemName;
                    if (!string.IsNullOrEmpty(wardrobeComp.MenuInstallPath))
                    {
                        menuItemInstallPath = $"{wardrobeComp.MenuInstallPath}/{menuItemInstallPath}";
                    }
                    if (outfit.MenuGroup != null)
                    {
                        // if outfit has menu group, this needs to be installed under the outfit's menu group
                        menuItemInstallPath += $"/{outfit.Name}";
                    }
                    if (outfitEnableItem.transform.parent != menuGroupRoot)
                    {
                        menuItemInstallPath += $"/{AnimationUtils.GetRelativePath(outfitEnableItem.transform, menuGroupRoot)}";
                    }

                    store.Append(new ToggleItem()
                    {
                        Name = outfitEnableItem.Name,
                        Icon = outfitEnableItem.Icon,
                        Controller = new AnimatorParameterController()
                        {
                            ParameterName = slot.ParameterName,
                            ParameterValue = mappedValue
                        }
                    }, menuItemInstallPath);
                }
                else
                {
                    // otherwise, treat it as a normal menu item
                    var menuItem = outfitEnableItem.gameObject.AddComponent<DTMenuItem>();
                    menuItem.Type = DTMenuItem.ItemType.Toggle;
                    menuItem.Icon = outfitEnableItem.Icon;
                    menuItem.Controller = new DTMenuItem.ItemController()
                    {
                        Type = DTMenuItem.ItemController.ControllerType.AnimatorParameter,
                        AnimatorParameterName = slot.ParameterName,
                        AnimatorParameterValue = mappedValue
                    };
                }
            }
        }

        private static void ComposeWardrobe(Context ctx, DTWardrobe wardrobeComp)
        {
            // create menu group and sub-menu item
            if (wardrobeComp.UseAsMenuGroup)
            {
                GetOrAddComponent<DTMenuGroup>(wardrobeComp.gameObject);
                if (wardrobeComp.GenerateSubMenuItem)
                {
                    var menuItem = GetOrAddComponent<DTMenuItem>(wardrobeComp.gameObject);
                    menuItem.Name = wardrobeComp.MenuItemName;
                    menuItem.Icon = wardrobeComp.MenuItemIcon;
                    menuItem.Type = DTMenuItem.ItemType.SubMenu;
                    menuItem.SubMenuType = DTMenuItem.ItemSubMenuType.Children;
                }
            }
            else
            {
                // add a empty submenu item via DK API
                var store = ctx.Feature<MenuStore>();
                store.Append(new SubMenuItem()
                {
                    Name = wardrobeComp.MenuItemName,
                    Icon = wardrobeComp.MenuItemIcon
                }, wardrobeComp.MenuInstallPath);
            }

            // create parameter slot
            var slot = GetOrAddComponent<DTParameterSlot>(wardrobeComp.gameObject);
            slot.ParameterName = "cpDT_Cabinet";
            slot.ParameterDefaultValue = BaseValue; // TODO
            slot.ValueType = DTParameterSlot.ParameterValueType.Int; // TODO
            slot.NetworkSynced = wardrobeComp.NetworkSynced;
            slot.Saved = wardrobeComp.Saved;

            // visit containing components
            DTBaseOutfit baseOutfit = null;
            var altOutfits = new List<DTAlternateOutfit>();
            VisitComponents(ctx, wardrobeComp, slot, wardrobeComp.transform, ref baseOutfit, altOutfits);

            // create one base outfit if not found
            if (baseOutfit == null)
            {
                var baseOutfitObj = new GameObject("Base");
                baseOutfitObj.transform.SetParent(wardrobeComp.transform);
                baseOutfitObj.transform.SetAsFirstSibling();
                baseOutfit = baseOutfitObj.AddComponent<DTBaseOutfit>();
            }

            // make alt outfits to disable base outfit dynamics
            if (baseOutfit.GroupDynamics != null)
            {
                foreach (var altOutfit in altOutfits)
                {
                    if (altOutfit.ObjectToggles.Where(t => t.Target == baseOutfit.GroupDynamics).Count() > 0)
                    {
                        continue;
                    }
                    altOutfit.ObjectToggles.Add(new DTSmartControl.ObjectToggle()
                    {
                        Target = baseOutfit.GroupDynamics,
                        Enabled = false
                    });
                }
            }

            // compose everything into smartcontrols and menus
            ComposeBaseOutfit(ctx, wardrobeComp, slot, baseOutfit);
            var count = 0;
            foreach (var altOutfit in altOutfits)
            {
                // TODO: allow giving custom slot map value
                ComposeAlternateOutfit(ctx, wardrobeComp, slot, ++count, altOutfit);
            }

            // reset controls on switch by adding crosscontrols unrelated to us
            if (wardrobeComp.ResetControlsOnSwitch)
            {
                var baseCtrls = baseOutfit.GetComponentsInChildren<DTSmartControl>()
                    .Where(sc => IsCrossControllable(sc)).ToList();
                var altOutfitCtrlsCache = new Dictionary<DTAlternateOutfit, List<DTSmartControl>>();
                foreach (var altOutfit in altOutfits)
                {
                    // turn off base controls
                    CrossTurnOffControls(altOutfit, baseCtrls);

                    // turn off controls from another outfit
                    foreach (var anotherAltOutfit in altOutfits)
                    {
                        if (anotherAltOutfit == altOutfit)
                        {
                            continue;
                        }

                        // try obtain from cache first
                        if (!altOutfitCtrlsCache.TryGetValue(anotherAltOutfit, out var altOutfitCtrls))
                        {
                            altOutfitCtrls = altOutfitCtrlsCache[anotherAltOutfit] = anotherAltOutfit.GetComponentsInChildren<DTSmartControl>()
                                .Where(sc => IsCrossControllable(sc)).ToList();
                        }

                        CrossTurnOffControls(altOutfit, altOutfitCtrls);
                    }
                }
            }

            ComposeOutfitEnableItems(ctx, slot, baseOutfit, altOutfits);
            ComposeExternalOutfitItems(ctx.AvatarGameObject.transform);
        }

        public override bool Invoke(Context ctx)
        {
            var comps = ctx.AvatarGameObject.GetComponentsInChildren<DTWardrobe>(true);
            if (comps.Length == 0)
            {
                return true;
            }
            else if (comps.Length > 1)
            {
                ctx.Report.LogWarn(LogLabel, "There are more than one DTWardrobe compnent in the avatar. Only the first found will be used.");
            }
            var comp = comps[0];
            ComposeWardrobe(ctx, comp);

            return true;
        }
    }
}
