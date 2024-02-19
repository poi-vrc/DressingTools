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

using Chocopoi.DressingFramework.Localization;
using Chocopoi.DressingTools.Components.Animations;
using Chocopoi.DressingTools.Components.Menu;
using Chocopoi.DressingTools.Inspector.Views;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Chocopoi.DressingTools.UI.Presenters
{
    internal class MenuGroupPresenter
    {
        private readonly IMenuGroupView _view;

        public MenuGroupPresenter(IMenuGroupView view)
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

            _view.AddItem += OnAddItem;
            _view.AddSmartControl += OnAddSmartControl;
            _view.RemoveItem += OnRemoveItem;

            EditorApplication.hierarchyChanged += OnHierarchyChanged;
        }

        private void UnsubscribeEvents()
        {
            _view.Load -= OnLoad;
            _view.Unload -= OnUnload;

            _view.AddItem -= OnAddItem;
            _view.AddSmartControl -= OnAddSmartControl;
            _view.RemoveItem -= OnRemoveItem;

            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
        }

        private void OnHierarchyChanged()
        {
            UpdateView();
        }

        private void OnAddItem()
        {
            var obj = new GameObject($"MenuItem{_view.Target.transform.childCount + 1}");
            obj.AddComponent<DTMenuItem>();
            obj.transform.SetParent(_view.Target.transform);
            _view.Repaint();
        }

        private void OnAddSmartControl()
        {
            var obj = new GameObject($"SmartControl{_view.Target.transform.childCount + 1}");
            var sc = obj.AddComponent<DTSmartControl>();
            sc.DriverType = DTSmartControl.SmartControlDriverType.MenuItem;
            obj.transform.SetParent(_view.Target.transform);
            Selection.activeGameObject = obj;
            _view.Repaint();
        }

        private void OnRemoveItem(int idx)
        {
            if (idx < 0 || idx >= _view.Target.transform.childCount)
            {
                return;
            }

            if (_view.ShowConfirmRemoveDialog())
            {
                var obj = _view.Target.transform.GetChild(idx).gameObject;
                Undo.RecordObject(obj, "Remove Menu Item");
                Object.DestroyImmediate(obj);
                _view.Repaint();
            }
        }

        private void UpdateView()
        {
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
