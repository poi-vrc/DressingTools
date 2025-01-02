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

using Chocopoi.DressingFramework;
using Chocopoi.DressingFramework.Localization;
using Chocopoi.DressingTools.Animations;
using Chocopoi.DressingTools.Components.Animations;
using Chocopoi.DressingTools.Inspector.Views;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Presenters
{
    internal class SmartControlPropertyGroupPresenter
    {
        private readonly ISmartControlPropertyGroupView _view;

        public SmartControlPropertyGroupPresenter(ISmartControlPropertyGroupView view)
        {
            _view = view;
            _view.Load += OnLoad;
        }

        private void SubscribeEvents()
        {
            _view.Unload += OnUnload;
            _view.SettingsChanged += OnSettingsChanged;
            _view.AddGameObject += OnAddGameObject;
            _view.RemoveGameObject += OnRemoveGameObject;
            _view.ChangeGameObject += OnChangeGameObject;
        }

        private void UnsubscribeEvents()
        {
            _view.Unload -= OnUnload;
            _view.SettingsChanged -= OnSettingsChanged;
            _view.AddGameObject -= OnAddGameObject;
            _view.RemoveGameObject -= OnRemoveGameObject;
        }

        private void OnChangeGameObject(int index, GameObject go)
        {
            _view.Target.GameObjects[index] = go;
            _view.SelectionGameObjects[index] = go;
            if (_view.SelectionType == 0)
            {
                // if in normal mode, set this as the pick from transform
                _view.PickFromTransform = go.transform;
                SearchComponents();
                _view.Repaint();
            }
        }

        private void OnSettingsChanged()
        {
            _view.Target.SelectionType = (DTSmartControl.PropertyGroup.PropertySelectionType)_view.SelectionType;
            _view.Target.SearchTransform = _view.SearchTransform;
            SearchComponents();
            _view.Repaint();
        }

        private void OnAddGameObject(GameObject go)
        {
            _view.Target.GameObjects.Add(go);
            _view.SelectionGameObjects.Add(go);
            SuggestPickFromTransform();
            SearchComponents();
            _view.Repaint();
        }

        private void OnRemoveGameObject(GameObject go)
        {
            _view.Target.GameObjects.Remove(go);
            _view.SelectionGameObjects.Remove(go);
            SuggestPickFromTransform();
            SearchComponents();
            _view.Repaint();
        }

        private void SearchComponents()
        {
            _view.FoundComponents.Clear();

            if (_view.PickFromTransform == null)
            {
                return;
            }

            var comps = _view.PickFromTransform.GetComponentsInChildren<Component>();
            foreach (var comp in comps)
            {
                // TODO: we could show all but for now only Transform,SkinnedMeshRenderer
                if (comp is Transform ||
                    comp is SkinnedMeshRenderer)
                {
                    _view.FoundComponents.Add(comp);
                }
            }
        }

        private void UpdateView()
        {
            _view.SelectionType = (int)_view.Target.SelectionType;
            _view.SearchTransform = _view.Target.SearchTransform;
            _view.SelectionGameObjects.Clear();
            _view.SelectionGameObjects.AddRange(_view.Target.GameObjects);
            SearchComponents();
            _view.Repaint();
        }

        private void SuggestPickFromTransform(bool mustSet = false)
        {
            if (_view.SelectionType == 0 && _view.SelectionGameObjects.Count > 0 && _view.SelectionGameObjects[^1] != null)
            {
                // only suggest if in normal mode, otherwise it's meaningless
                _view.PickFromTransform = _view.SelectionGameObjects[^1].transform;
            }
            else if (mustSet)
            {
                // only set if it's a must
                _view.PickFromTransform = _view.Target.SearchTransform;
            }
        }

        private void OnLoad()
        {
            SubscribeEvents();
            UpdateView();

            SuggestPickFromTransform(true);
            SearchComponents();
            _view.Repaint();
        }

        private void OnUnload()
        {
            UnsubscribeEvents();
        }
    }
}
