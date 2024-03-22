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
using Chocopoi.DressingFramework;
using Chocopoi.DressingFramework.Detail.DK.Passes;
using Chocopoi.DressingFramework.Extensibility.Sequencing;
using Chocopoi.DressingFramework.Logging;
using Chocopoi.DressingFramework.Menu;
using Chocopoi.DressingTools.Components.Menu;
using UnityEngine;
using MenuItem = Chocopoi.DressingFramework.Menu.MenuItem;
#if DT_VRCSDK3A
using Chocopoi.DressingFramework.Menu.VRChat;
#endif

namespace Chocopoi.DressingTools.Passes.Menu
{
    internal class ComposeAndInstallMenuPass : BuildPass
    {
        public override BuildConstraint Constraint =>
            InvokeAtStage(BuildStage.Transpose)
                .BeforePass<FlushMenuStorePass>() // TODO: this affects non-DK
                .Build();

        private static MenuItemController GetRespectiveController(DTMenuItem.ItemController itemController)
        {
            MenuItemController menuItemController = null;

            if (itemController.Type == DTMenuItem.ItemController.ControllerType.AnimatorParameter)
            {
                menuItemController = new AnimatorParameterController()
                {
                    ParameterName = itemController.AnimatorParameterName,
                    ParameterValue = itemController.AnimatorParameterValue
                };
            }
            else
            {
                Debug.LogWarning("No such controller");
            }

            return menuItemController;
        }

        private static ButtonItem ConvertToButtonItem(DTMenuItem compItem)
        {
            var item = new ButtonItem()
            {
                Name = compItem.Name,
                Icon = compItem.Icon,
                Controller = GetRespectiveController(compItem.Controller)
            };
            return item;
        }

        private static ToggleItem ConvertToToggleItem(DTMenuItem compItem)
        {
            var item = new ToggleItem()
            {
                Name = compItem.Name,
                Icon = compItem.Icon,
                Controller = GetRespectiveController(compItem.Controller)
            };
            return item;
        }

        private static MenuItem ConvertToSubMenuItem(Report report, DTMenuItem compItem, Stack<DTMenuGroup> selfRefCheckStack)
        {
            if (compItem.SubMenuType == DTMenuItem.ItemSubMenuType.Children)
            {
                return new SubMenuItem()
                {
                    Name = compItem.Name,
                    Icon = compItem.Icon,
                    SubMenu = ConvertChildrenToMenuGroup(report, compItem.transform, selfRefCheckStack),
                    ControllerOnOpen = GetRespectiveController(compItem.Controller)
                };
            }
            else if (compItem.SubMenuType == DTMenuItem.ItemSubMenuType.DTMenuGroupComponent)
            {
                if (selfRefCheckStack.Contains(compItem.DTSubMenu))
                {
                    // self-referencing detected, aborting
                    report.LogError("ComposeAndInstallMenuPass", "Self referencing sub-menu item detected, aborting");
                    return null;
                }

                // generate a new even we have generated it before
                return new SubMenuItem()
                {
                    Name = compItem.Name,
                    Icon = compItem.Icon,
                    SubMenu = compItem.DTSubMenu != null ? ConvertToMenuGroup(report, compItem.DTSubMenu, selfRefCheckStack) : null,
                    ControllerOnOpen = GetRespectiveController(compItem.Controller)
                };
            }
#if DT_VRCSDK3A
            else if (compItem.SubMenuType == DTMenuItem.ItemSubMenuType.VRCExpressionsMenuAsset)
            {
                return new VRCSubMenuItem()
                {
                    Name = compItem.Name,
                    Icon = compItem.Icon,
                    SubMenu = compItem.VRCSubMenu,
                    ControllerOnOpen = GetRespectiveController(compItem.Controller)
                };
            }
#endif
            return null;
        }

        private static MenuItemController GetRespectiveControllerOrNull(DTMenuItem.ItemController[] controllers, int index)
        {
            if (index < 0 || index >= controllers.Length)
            {
                return null;
            }

            return GetRespectiveController(controllers[index]);
        }

        private static MenuItem.Label GetLabelOrNull(DTMenuItem.Label[] labels, int index)
        {
            if (index < 0 || index >= labels.Length)
            {
                return null;
            }

            return new MenuItem.Label()
            {
                Name = labels[index].Name,
                Icon = labels[index].Icon
            };
        }

        private static TwoAxisItem ConvertToTwoAxisItem(DTMenuItem compItem)
        {
            var item = new TwoAxisItem()
            {
                Name = compItem.Name,
                Icon = compItem.Icon,
                ControllerOnOpen = GetRespectiveController(compItem.Controller),
                HorizontalController = GetRespectiveControllerOrNull(compItem.SubControllers, 0),
                VerticalController = GetRespectiveControllerOrNull(compItem.SubControllers, 1),
                UpLabel = GetLabelOrNull(compItem.SubLabels, 0),
                RightLabel = GetLabelOrNull(compItem.SubLabels, 1),
                DownLabel = GetLabelOrNull(compItem.SubLabels, 2),
                LeftLabel = GetLabelOrNull(compItem.SubLabels, 3)
            };
            return item;
        }

        private static FourAxisItem ConvertToFourAxisItem(DTMenuItem compItem)
        {
            var item = new FourAxisItem()
            {
                Name = compItem.Name,
                Icon = compItem.Icon,
                ControllerOnOpen = GetRespectiveController(compItem.Controller),
                UpController = GetRespectiveControllerOrNull(compItem.SubControllers, 0),
                RightController = GetRespectiveControllerOrNull(compItem.SubControllers, 1),
                DownController = GetRespectiveControllerOrNull(compItem.SubControllers, 2),
                LeftController = GetRespectiveControllerOrNull(compItem.SubControllers, 3),
                UpLabel = GetLabelOrNull(compItem.SubLabels, 0),
                RightLabel = GetLabelOrNull(compItem.SubLabels, 1),
                DownLabel = GetLabelOrNull(compItem.SubLabels, 2),
                LeftLabel = GetLabelOrNull(compItem.SubLabels, 3)
            };
            return item;
        }

        private static RadialItem ConvertToRadialItem(DTMenuItem compItem)
        {
            var item = new RadialItem()
            {
                Name = compItem.Name,
                Icon = compItem.Icon,
                ControllerOnOpen = GetRespectiveController(compItem.Controller),
                RadialController = GetRespectiveControllerOrNull(compItem.SubControllers, 0)
            };
            return item;
        }

        private static MenuItem ConvertToMenuItem(Report report, DTMenuItem compItem, Stack<DTMenuGroup> selfRefCheckStack)
        {
            MenuItem item = null;

            switch (compItem.Type)
            {
                case DTMenuItem.ItemType.Button:
                    item = ConvertToButtonItem(compItem);
                    break;
                case DTMenuItem.ItemType.Toggle:
                    item = ConvertToToggleItem(compItem);
                    break;
                case DTMenuItem.ItemType.SubMenu:
                    item = ConvertToSubMenuItem(report, compItem, selfRefCheckStack);
                    break;
                case DTMenuItem.ItemType.TwoAxis:
                    item = ConvertToTwoAxisItem(compItem);
                    break;
                case DTMenuItem.ItemType.FourAxis:
                    item = ConvertToFourAxisItem(compItem);
                    break;
                case DTMenuItem.ItemType.Radial:
                    item = ConvertToRadialItem(compItem);
                    break;
            }

            return item;
        }

        private static MenuGroup ConvertChildrenToMenuGroup(Report report, Transform parent, Stack<DTMenuGroup> selfRefCheckStack)
        {
            var menuGroup = new MenuGroup();
            for (var i = 0; i < parent.transform.childCount; i++)
            {
                var child = parent.transform.GetChild(i);
                if (child.TryGetComponent<DTMenuItem>(out var compItem))
                {
                    var menuItem = ConvertToMenuItem(report, compItem, selfRefCheckStack);
                    menuGroup.Add(menuItem);
                }
            }
            return menuGroup;
        }

        private static MenuGroup ConvertToMenuGroup(Report report, DTMenuGroup mgComp, Stack<DTMenuGroup> selfRefCheckStack)
        {
            // push this component into the stack for future checks
            selfRefCheckStack.Push(mgComp);
            var menuGroup = ConvertChildrenToMenuGroup(report, mgComp.transform, selfRefCheckStack);
            selfRefCheckStack.Pop();
            return menuGroup;
        }

        public override bool Invoke(Context ctx)
        {
            var miComps = ctx.AvatarGameObject.GetComponentsInChildren<DTMenuInstall>(false);

            var store = ctx.Feature<MenuStore>();
            foreach (var miComp in miComps)
            {
                IMenuRepository menuRepo;

                if (miComp.TryGetComponent<DTMenuGroup>(out var menuGroup))
                {
                    // convert them into API form
                    menuRepo = ConvertToMenuGroup(ctx.Report, menuGroup, new Stack<DTMenuGroup>());
                }
#if DT_VRCSDK3A
                else if (miComp.VRCSourceMenu != null)
                {
                    // context is not provided because we just want to look into it
                    menuRepo = new VRCMenuWrapper(miComp.VRCSourceMenu);
                }
#endif
                else
                {
                    continue;
                }

                foreach (var item in menuRepo)
                {
                    store.Append(item, miComp.InstallPath);
                }
            }
            return true;
        }
    }
}
