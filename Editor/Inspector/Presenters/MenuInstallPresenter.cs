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
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Presenters
{
    internal class MenuInstallPresenter
    {
        private readonly IMenuInstallView _view;

        public MenuInstallPresenter(IMenuInstallView view)
        {
            _view = view;
            _view.Load += OnLoad;
        }

        private void SubscribeEvents()
        {
            _view.Unload += OnUnload;
            _view.SettingsChanged += OnSettingsChanged;

            EditorApplication.hierarchyChanged += OnHierarchyChange;
        }

        private void UnsubscribeEvents()
        {
            _view.Unload -= OnUnload;
            _view.SettingsChanged -= OnSettingsChanged;

            EditorApplication.hierarchyChanged -= OnHierarchyChange;
        }

        private void OnHierarchyChange()
        {
            if (_view.Target == null)
            {
                return;
            }
            UpdateView();
        }

        private void OnSettingsChanged()
        {
            _view.Target.InstallPath = _view.InstallPath;
#if DT_VRCSDK3A
            _view.Target.VRCSourceMenu = _view.VRCSourceMenu;
#endif
        }

        private void UpdateView()
        {
            _view.HasMenuGroupComponent = _view.Target.TryGetComponent<DTMenuGroup>(out _);
            _view.InstallPath = _view.Target.InstallPath;
#if DT_VRCSDK3A
            _view.VRCSourceMenu = _view.Target.VRCSourceMenu;
#endif
            _view.Repaint();
        }

        private void OnLoad()
        {
            SubscribeEvents();
            UpdateView();
        }

        private void OnUnload()
        {
            UnsubscribeEvents();
        }
    }
}
