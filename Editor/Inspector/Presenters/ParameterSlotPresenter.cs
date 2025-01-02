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
using Chocopoi.DressingFramework;
using Chocopoi.DressingTools.Components.Animations;
using Chocopoi.DressingTools.Inspector.Views;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Presenters
{
    internal class ParameterSlotPresenter
    {
        private readonly IParameterSlotView _view;

        public ParameterSlotPresenter(IParameterSlotView view)
        {
            _view = view;
            _view.Load += OnLoad;
        }

        private void SubscribeEvents()
        {
            _view.Unload += OnUnload;
            _view.ConfigChanged += OnConfigChanged;
            _view.AddMapping += OnAddMapping;

            EditorApplication.hierarchyChanged += OnHierarchyChange;
        }

        private void UnsubscribeEvents()
        {
            _view.Unload -= OnUnload;
            _view.ConfigChanged -= OnConfigChanged;
            _view.AddMapping -= OnAddMapping;

            EditorApplication.hierarchyChanged -= OnHierarchyChange;
        }

        private void OnConfigChanged()
        {
            _view.Target.ValueType = (DTParameterSlot.ParameterValueType)_view.ValueType;
            _view.Target.ParameterDefaultValue = _view.ParameterDefaultValue;
            UpdateMappingHelpboxes();
            _view.RepaintMappingHelpboxes();
        }

        private float SuggestNewValue()
        {
            var hasDefault = false;
            var currentMax = -1.0f;
            foreach (var mapping in _view.Mappings)
            {
                if (mapping.ctrl == null)
                {
                    continue;
                }
                hasDefault |= mapping.ctrl.ParameterSlotConfig.MappedValue == _view.Target.ParameterDefaultValue;
                currentMax = Math.Max(currentMax, mapping.ctrl.ParameterSlotConfig.MappedValue);
            }
            return hasDefault ? currentMax + 1.0f : _view.Target.ParameterDefaultValue;
        }

        private void OnAddMapping(DTSmartControl ctrl)
        {
            if (_view.Mappings.Where(m => m.ctrl == ctrl).Count() > 0)
            {
                return;
            }
            ctrl.DriverType = DTSmartControl.SCDriverType.ParameterSlot;
            ctrl.ParameterSlotConfig.ParameterSlot = _view.Target;
            ctrl.ParameterSlotConfig.MappedValue = SuggestNewValue();
            UpdateMappings();
        }

        private void OnHierarchyChange()
        {
            if (_view.Target == null)
            {
                return;
            }
            UpdateView();
        }

        private void UpdateMappingHelpboxes()
        {
            var list = new List<float>();
            var hasDuplicates = false;
            var hasDefault = false;

            foreach (var mapping in _view.Mappings)
            {
                var ctrl = mapping.ctrl;
                if (ctrl == null)
                {
                    continue;
                }
                hasDuplicates |= list.Where(f => Mathf.Approximately(f, ctrl.ParameterSlotConfig.MappedValue)).Count() > 0;
                list.Add(ctrl.ParameterSlotConfig.MappedValue);
                hasDefault |= Mathf.Approximately(ctrl.ParameterSlotConfig.MappedValue, _view.Target.ParameterDefaultValue);
            }

            _view.ShowDuplicateMappingsHelpbox = hasDuplicates;
            _view.ShowNoDefaultValueMappingHelpbox = !hasDefault;
        }

        private void UpdateMappings()
        {
            _view.Mappings.Clear();
            var avatarRoot = DKRuntimeUtils.GetAvatarRoot(_view.Target.gameObject);
            if (avatarRoot == null)
            {
                _view.ShowAvatarRootNotFoundHelpbox = true;
                return;
            }
            _view.ShowAvatarRootNotFoundHelpbox = false;

            var ctrls = avatarRoot.GetComponentsInChildren<DTSmartControl>()
                .Where(c =>
                    c.DriverType == DTSmartControl.SCDriverType.ParameterSlot &&
                    c.ParameterSlotConfig.ParameterSlot == _view.Target);
            foreach (var ctrl in ctrls)
            {
                var myCtrl = ctrl;
                _view.Mappings.Add(new ParameterSlotMapping()
                {
                    ctrl = myCtrl,
                    onChange = val =>
                    {
                        myCtrl.ParameterSlotConfig.MappedValue = val;
                        UpdateMappingHelpboxes();
                        _view.RepaintMappingHelpboxes();
                    },
                    onRemove = () =>
                    {
                        myCtrl.ParameterSlotConfig.ParameterSlot = null;
                        UpdateMappings();
                    }
                });
            }

            _view.Mappings.Sort((a, b) => a.ctrl.ParameterSlotConfig.MappedValue.CompareTo(b.ctrl.ParameterSlotConfig.MappedValue));

            UpdateMappingHelpboxes();
            _view.Repaint();
        }

        private void UpdateView()
        {
            _view.ValueType = (int)_view.Target.ValueType;
            _view.ParameterDefaultValue = _view.Target.ParameterDefaultValue;
            UpdateMappings();
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
