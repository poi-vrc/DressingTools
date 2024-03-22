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
using Chocopoi.DressingTools.Components.Modifiers;
using Chocopoi.DressingTools.Inspector.Views;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Presenters
{
    internal class GroupDynamicsPresenter
    {
        private readonly IGroupDynamicsView _view;

        public GroupDynamicsPresenter(IGroupDynamicsView view)
        {
            _view = view;

            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            _view.Load += OnLoad;
            _view.Unload += OnUnload;

            _view.ConfigChange += OnConfigChange;
            _view.AddInclude += OnAddInclude;
            _view.AddExclude += OnAddExclude;
            _view.ChangeInclude += OnChangeInclude;
            _view.ChangeExclude += OnChangeExclude;
            _view.RemoveInclude += OnRemoveInclude;
            _view.RemoveExclude += OnRemoveExclude;
        }

        private void UnsubscribeEvents()
        {
            _view.Load -= OnLoad;
            _view.Unload -= OnUnload;

            _view.ConfigChange -= OnConfigChange;
            _view.AddInclude -= OnAddInclude;
            _view.AddExclude -= OnAddExclude;
            _view.ChangeInclude -= OnChangeInclude;
            _view.ChangeExclude -= OnChangeExclude;
            _view.RemoveInclude -= OnRemoveInclude;
            _view.RemoveExclude -= OnRemoveExclude;
        }

        private void OnAddInclude(Transform transform)
        {
            _view.Target.IncludeTransforms.Add(transform);
            UpdateView();
        }

        private void OnAddExclude(Transform transform)
        {
            _view.Target.ExcludeTransforms.Add(transform);
            UpdateView();
        }

        private void OnRemoveInclude(int idx)
        {
            _view.Target.IncludeTransforms.RemoveAt(idx);
            UpdateView();
        }

        private void OnRemoveExclude(int idx)
        {
            _view.Target.ExcludeTransforms.RemoveAt(idx);
            UpdateView();
        }

        private void OnChangeInclude(int idx, Transform transform)
        {
            _view.Target.IncludeTransforms[idx] = transform;
            UpdateView();
        }

        private void OnChangeExclude(int idx, Transform transform)
        {
            _view.Target.ExcludeTransforms[idx] = transform;
            UpdateView();
        }

        private void OnConfigChange()
        {
            _view.Target.SearchMode = (DTGroupDynamics.DynamicsSearchMode)_view.SearchMode;
        }

        private void UpdateView()
        {
            _view.SearchMode = (int)_view.Target.SearchMode;
            _view.IncludeTransforms = new List<Transform>(_view.Target.IncludeTransforms);
            _view.ExcludeTransforms = new List<Transform>(_view.Target.ExcludeTransforms);
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
