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
using Chocopoi.DressingFramework;
using Chocopoi.DressingFramework.Localization;
using Chocopoi.DressingTools.Components.Menu;
using Chocopoi.DressingTools.Inspector.Views;
using UnityEditor;

namespace Chocopoi.DressingTools.UI.Presenters
{
    internal class MenuItemPresenter
    {
        private const int SubParametersControllersLength = 4;

        private readonly IMenuItemView _view;

        public MenuItemPresenter(IMenuItemView view)
        {
            _view = view;

            // TODO: set this from the editor level and move to a common place
            var prefs = PreferencesUtility.GetPreferences();
            I18nManager.Instance.SetLocale(prefs.app.selectedLanguage);

            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            _view.Load += OnLoad;
            _view.Unload += OnUnload;

            _view.NameChanged += OnNameChanged;
            _view.TypeChanged += OnTypeChanged;
            _view.IconChanged += OnIconChanged;
            _view.SubMenuTypeChanged += OnSubMenuTypeChanged;
            _view.AxisLabelChanged += OnAxisLabelChanged;
            _view.TwoAxisControllerChanged += OnTwoAxisControllerChanged;
            _view.FourAxisControllerChanged += OnFourAxisControllerChanged;
            _view.RadialControllerChanged += OnRadialControllerChanged;

            _view.InfoParameterFieldsChanged += OnInfoParameterFieldsChanged;
            _view.DetailsParameterFieldsChanged += OnDetailsParameterFieldsChanged;

            _view.SubMenuObjectFieldsChanged += OnSubMenuObjectFieldsChanged;

            EditorApplication.hierarchyChanged += OnHierachyChanged;
        }

        private void UnsubscribeEvents()
        {
            _view.Load -= OnLoad;
            _view.Unload -= OnUnload;

            _view.NameChanged -= OnNameChanged;
            _view.TypeChanged -= OnTypeChanged;
            _view.IconChanged -= OnIconChanged;
            _view.SubMenuTypeChanged -= OnSubMenuTypeChanged;
            _view.AxisLabelChanged -= OnAxisLabelChanged;
            _view.TwoAxisControllerChanged -= OnTwoAxisControllerChanged;
            _view.FourAxisControllerChanged -= OnFourAxisControllerChanged;
            _view.RadialControllerChanged -= OnRadialControllerChanged;

            _view.InfoParameterFieldsChanged -= OnInfoParameterFieldsChanged;
            _view.DetailsParameterFieldsChanged -= OnDetailsParameterFieldsChanged;

            _view.SubMenuObjectFieldsChanged -= OnSubMenuObjectFieldsChanged;

            EditorApplication.hierarchyChanged -= OnHierachyChanged;
        }

        private void UpdateAnimatorParameterTextFieldAvatarGameObject()
        {
            var avatarRoot = DKRuntimeUtils.GetAvatarRoot(_view.Target.gameObject);
            _view.SetAnimatorParameterTextFieldAvatarGameObject(avatarRoot);
        }

        private void OnHierachyChanged()
        {
            if (_view.Target == null)
            {
                return;
            }
            UpdateAnimatorParameterTextFieldAvatarGameObject();
        }

        private void OnSubMenuObjectFieldsChanged()
        {
            _view.Target.DTSubMenu = _view.DTSubMenu;
#if DT_VRCSDK3A
            _view.Target.VRCSubMenu = _view.VRCSubMenu;
#endif
        }

        private void WriteControllerToTarget()
        {
            _view.Target.Controller.Type = DTMenuItem.ItemController.ControllerType.AnimatorParameter;
            _view.Target.Controller.AnimatorParameterName = _view.ParameterName;
            _view.Target.Controller.AnimatorParameterValue = _view.ParameterValue;
        }

        private void OnInfoParameterFieldsChanged()
        {
            WriteControllerToTarget();
            _view.RepaintDetailsParameterFields();
        }

        private void OnDetailsParameterFieldsChanged()
        {
            WriteControllerToTarget();
            _view.RepaintInfoParameterFields();
        }

        private void OnSubMenuTypeChanged()
        {
            // TODO: magic numbers
            switch (_view.SubMenuType)
            {
                case 0:
                    _view.Target.SubMenuType = DTMenuItem.ItemSubMenuType.Children;
                    break;
                case 1:
                    _view.Target.SubMenuType = DTMenuItem.ItemSubMenuType.DTMenuGroupComponent;
                    break;
                case 2:
                    _view.Target.SubMenuType = DTMenuItem.ItemSubMenuType.VRCExpressionsMenuAsset;
                    break;
            }
        }

        private static void WriteAnimatorParameterController(DTMenuItem.ItemController controller, string parameter)
        {
            controller.Type = DTMenuItem.ItemController.ControllerType.AnimatorParameter;
            controller.AnimatorParameterName = parameter;
            controller.AnimatorParameterValue = 0.0f;
        }

        private void OnRadialControllerChanged()
        {
            CreateTargetSubControllersIfNotExist();
            WriteAnimatorParameterController(_view.Target.SubControllers[0], _view.RadialParameter);
        }

        private void OnFourAxisControllerChanged()
        {
            CreateTargetSubControllersIfNotExist();
            WriteAnimatorParameterController(_view.Target.SubControllers[0], _view.UpParameter);
            WriteAnimatorParameterController(_view.Target.SubControllers[1], _view.RightParameter);
            WriteAnimatorParameterController(_view.Target.SubControllers[2], _view.DownParameter);
            WriteAnimatorParameterController(_view.Target.SubControllers[3], _view.LeftParameter);
        }

        private void OnTwoAxisControllerChanged()
        {
            CreateTargetSubControllersIfNotExist();
            WriteAnimatorParameterController(_view.Target.SubControllers[0], _view.HorizontalParameter);
            WriteAnimatorParameterController(_view.Target.SubControllers[1], _view.VerticalParameter);
        }

        private void WriteAxisLabel(DTMenuItem.Label compLabel, AxisLabel label)
        {
            compLabel.Name = label.name;
            compLabel.Icon = label.icon;
        }

        private void WriteAxisLabels()
        {
            CreateTargetSubLabelsIfNotExist();
            WriteAxisLabel(_view.Target.SubLabels[0], _view.AxisUpLabel);
            WriteAxisLabel(_view.Target.SubLabels[1], _view.AxisRightLabel);
            WriteAxisLabel(_view.Target.SubLabels[2], _view.AxisDownLabel);
            WriteAxisLabel(_view.Target.SubLabels[3], _view.AxisLeftLabel);
        }

        private void OnAxisLabelChanged()
        {
            WriteAxisLabels();
            _view.Repaint();
        }

        private void OnIconChanged()
        {
            _view.Target.Icon = _view.Icon;
            _view.Repaint();
        }


        private void OnNameChanged()
        {
            _view.Target.Name = _view.Name;
            _view.Repaint();
        }

        private void OnTypeChanged()
        {
            _view.Target.Type = (DTMenuItem.ItemType)_view.Type;
            _view.Repaint();
        }

        private void UpdateAxisLabel(DTMenuItem.Label compLabel, AxisLabel label)
        {
            label.name = compLabel.Name;
            label.icon = compLabel.Icon;
        }

        private void CreateTargetSubLabelsIfNotExist()
        {
            if (_view.Target.SubLabels.Length != SubParametersControllersLength)
            {
                _view.Target.SubLabels = new DTMenuItem.Label[] {
                    new DTMenuItem.Label(),
                    new DTMenuItem.Label(),
                    new DTMenuItem.Label(),
                    new DTMenuItem.Label(),
                };
            }
        }

        private void CreateTargetSubControllersIfNotExist()
        {
            if (_view.Target.SubControllers.Length != SubParametersControllersLength)
            {
                _view.Target.SubControllers = new DTMenuItem.ItemController[SubParametersControllersLength];
                for (var i = 0; i < SubParametersControllersLength; i++)
                {
                    _view.Target.SubControllers[i] = new DTMenuItem.ItemController()
                    {
                        Type = DTMenuItem.ItemController.ControllerType.AnimatorParameter,
                        AnimatorParameterName = "",
                        AnimatorParameterValue = 1.0f
                    };
                }
            }
        }

        private void UpdateAxisLabels()
        {
            CreateTargetSubLabelsIfNotExist();
            UpdateAxisLabel(_view.Target.SubLabels[0], _view.AxisUpLabel);
            UpdateAxisLabel(_view.Target.SubLabels[1], _view.AxisRightLabel);
            UpdateAxisLabel(_view.Target.SubLabels[2], _view.AxisDownLabel);
            UpdateAxisLabel(_view.Target.SubLabels[3], _view.AxisLeftLabel);
        }

        private void UpdateParameters()
        {
            CreateTargetSubControllersIfNotExist();
            _view.UpParameter = _view.Target.SubControllers[0].AnimatorParameterName;
            _view.RightParameter = _view.Target.SubControllers[1].AnimatorParameterName;
            _view.DownParameter = _view.Target.SubControllers[2].AnimatorParameterName;
            _view.LeftParameter = _view.Target.SubControllers[3].AnimatorParameterName;

            _view.HorizontalParameter = _view.Target.SubControllers[0].AnimatorParameterName;
            _view.VerticalParameter = _view.Target.SubControllers[1].AnimatorParameterName;

            _view.RadialParameter = _view.Target.SubControllers[0].AnimatorParameterName;
        }

        private void UpdateView()
        {
            _view.Name = _view.Target.Name;
            _view.Type = (int)_view.Target.Type;
            _view.Icon = _view.Target.Icon;

            _view.ParameterName = _view.Target.Controller.AnimatorParameterName;
            _view.ParameterValue = _view.Target.Controller.AnimatorParameterValue;

            switch (_view.Target.SubMenuType)
            {
                case DTMenuItem.ItemSubMenuType.Children:
                    _view.SubMenuType = 0;
                    break;
                case DTMenuItem.ItemSubMenuType.DTMenuGroupComponent:
                    _view.SubMenuType = 1;
                    break;
                case DTMenuItem.ItemSubMenuType.VRCExpressionsMenuAsset:
                    _view.SubMenuType = 2;
                    break;
            }

            _view.DTSubMenu = _view.Target.DTSubMenu;
#if DT_VRCSDK3A
            _view.VRCSubMenu = _view.Target.VRCSubMenu;
#endif

            UpdateAxisLabels();
            UpdateParameters();
            UpdateAnimatorParameterTextFieldAvatarGameObject();
            _view.Repaint();
        }

        private void OnLoad()
        {
            UpdateView();
        }

        private void OnUnload()
        {
            UnsubscribeEvents();
        }
    }
}
