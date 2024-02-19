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
using Chocopoi.DressingFramework.Localization;
using Chocopoi.DressingTools.Components.Animations;
using Chocopoi.DressingTools.Components.Menu;
using Chocopoi.DressingTools.Inspector.Views;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Presenters
{
    internal class SmartControlPresenter
    {
        private readonly ISmartControlView _view;

        public SmartControlPresenter(ISmartControlView view)
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

            _view.DriverChanged += OnDriverChanged;
            _view.AnimatorParameterDefaultValueChanged += OnAnimatorParameterDefaultValueChanged;
            _view.ControlTypeChanged += OnControlTypeChanged;

            _view.AddObjectToggle += OnAddObjectToggle;
            _view.RemoveObjectToggle += OnRemoveObjectToggle;
            _view.ChangeObjectToggle += OnChangeObjectToggle;
            _view.ChangeObjectToggleComponentType += OnChangeObjectToggleComponentType;

            _view.AddNewPropertyGroup += OnAddNewPropertyGroup;
            _view.RemovePropertyGroup += OnRemovePropertyGroup;

            _view.AddCrossControlValueOnEnabled += OnAddCrossControlValueOnEnabled;
            _view.AddCrossControlValueOnDisabled += OnAddCrossControlValueOnDisabled;

            _view.ChangeCrossControlValueOnEnabled += OnChangeCrossControlValueOnEnabled;
            _view.ChangeCrossControlValueOnDisabled += OnChangeCrossControlValueOnDisabled;

            _view.RemoveCrossControlValueOnEnabled += OnRemoveCrossControlValueOnEnabled;
            _view.RemoveCrossControlValueOnDisabled += OnRemoveCrossControlValueOnDisabled;

            _view.MenuItemConfigChanged += OnMenuItemConfigChanged;
        }

        private void UnsubscribeEvents()
        {
            _view.Load -= OnLoad;
            _view.Unload -= OnUnload;

            _view.DriverChanged -= OnDriverChanged;
            _view.AnimatorParameterDefaultValueChanged -= OnAnimatorParameterDefaultValueChanged;
            _view.ControlTypeChanged -= OnControlTypeChanged;

            _view.AddObjectToggle -= OnAddObjectToggle;
            _view.RemoveObjectToggle -= OnRemoveObjectToggle;
            _view.ChangeObjectToggle -= OnChangeObjectToggle;
            _view.ChangeObjectToggleComponentType -= OnChangeObjectToggleComponentType;

            _view.AddNewPropertyGroup -= OnAddNewPropertyGroup;
            _view.RemovePropertyGroup -= OnRemovePropertyGroup;

            _view.AddCrossControlValueOnEnabled -= OnAddCrossControlValueOnEnabled;
            _view.AddCrossControlValueOnDisabled -= OnAddCrossControlValueOnDisabled;

            _view.ChangeCrossControlValueOnEnabled -= OnChangeCrossControlValueOnEnabled;
            _view.ChangeCrossControlValueOnDisabled -= OnChangeCrossControlValueOnDisabled;

            _view.RemoveCrossControlValueOnEnabled -= OnRemoveCrossControlValueOnEnabled;
            _view.RemoveCrossControlValueOnDisabled -= OnRemoveCrossControlValueOnDisabled;

            _view.MenuItemConfigChanged -= OnMenuItemConfigChanged;
        }

        private void OnMenuItemConfigChanged()
        {
            _view.Target.name = _view.MenuItemName;
            _view.Target.MenuItemDriverConfig.ItemIcon = _view.MenuItemIcon;

            if (_view.Target.ControlType == DTSmartControl.SmartControlControlType.Binary &&
                _view.MenuItemType == 2)
            {
                // force radial to toggle
                _view.MenuItemType = 1;
                _view.Repaint();
            }

            switch (_view.MenuItemType)
            {
                case 0: // button
                    _view.Target.MenuItemDriverConfig.ItemType = DTMenuItem.ItemType.Button;
                    break;
                case 1: // toggle
                    _view.Target.MenuItemDriverConfig.ItemType = DTMenuItem.ItemType.Toggle;
                    break;
                case 2: // radial
                    _view.Target.MenuItemDriverConfig.ItemType = DTMenuItem.ItemType.Radial;
                    break;
            }

            if (_view.Target.TryGetComponent<DTMenuItem>(out var menuItem))
            {
                menuItem.Icon = _view.Target.MenuItemDriverConfig.ItemIcon;
                menuItem.Type = _view.Target.MenuItemDriverConfig.ItemType;
            }
        }

        private void OnRemovePropertyGroup(DTSmartControl.PropertyGroup propGp)
        {
            _view.Target.PropertyGroups.Remove(propGp);
            _view.PropertyGroups.Remove(propGp);
            _view.Repaint();
        }

        private void OnAddNewPropertyGroup()
        {
            var propGp = new DTSmartControl.PropertyGroup()
            {
                SearchTransform = _view.Target.transform
            };
            _view.Target.PropertyGroups.Add(propGp);
            _view.PropertyGroups.Add(propGp);
            _view.Repaint();
        }

        private void OnRemoveCrossControlValueOnDisabled(SmartControlCrossControlValue value)
        {
            var index = _view.CrossControlValuesOnDisabled.IndexOf(value);
            if (index != -1)
            {
                _view.CrossControlValuesOnDisabled.Remove(value);
                _view.Target.CrossControlActions.ValueActions.ValuesOnDisable.RemoveAt(index);
                _view.Repaint();
            }
        }

        private void OnRemoveCrossControlValueOnEnabled(SmartControlCrossControlValue value)
        {
            var index = _view.CrossControlValuesOnEnabled.IndexOf(value);
            if (index != -1)
            {
                _view.CrossControlValuesOnEnabled.Remove(value);
                _view.Target.CrossControlActions.ValueActions.ValuesOnEnable.RemoveAt(index);
                _view.Repaint();
            }
        }

        private void OnChangeCrossControlValueOnDisabled(SmartControlCrossControlValue value)
        {
            var index = _view.CrossControlValuesOnDisabled.IndexOf(value);
            if (index != -1)
            {
                var compValue = _view.Target.CrossControlActions.ValueActions.ValuesOnDisable[index];
                compValue.Control = value.control;
                compValue.Value = value.value;
                _view.Repaint();
            }
        }

        private void OnChangeCrossControlValueOnEnabled(SmartControlCrossControlValue value)
        {
            var index = _view.CrossControlValuesOnEnabled.IndexOf(value);
            if (index != -1)
            {
                var compValue = _view.Target.CrossControlActions.ValueActions.ValuesOnEnable[index];
                compValue.Control = value.control;
                compValue.Value = value.value;
                _view.Repaint();
            }
        }

        private void OnChangeObjectToggle(int idx)
        {
            var value = _view.ObjectToggles[idx];
            var objToggle = _view.Target.ObjectToggles[idx];
            objToggle.Target = value.target;
            objToggle.Enabled = value.enabled;
            GetSameObjectComponentTypes(value.target, value.sameObjectComponentTypes);
            _view.Repaint();
        }

        private void OnRemoveObjectToggle(int idx)
        {
            var value = _view.ObjectToggles[idx];
            _view.ObjectToggles.Remove(value);
            _view.Target.ObjectToggles.RemoveAt(idx);
            _view.Repaint();
        }

        private void OnChangeObjectToggleComponentType(int idx, Type type)
        {
            var value = _view.ObjectToggles[idx];
            var objToggle = _view.Target.ObjectToggles[idx];

            if (value.target != null && value.target.TryGetComponent(type, out var comp))
            {
                value.target = comp;
                objToggle.Target = comp;
                _view.Repaint();
            }
        }

        private void OnAddCrossControlValueOnDisabled(DTSmartControl control)
        {
            AddCrossControlValueIfNotExist(_view.Target.CrossControlActions.ValueActions.ValuesOnDisable, control);
        }

        private void OnAddCrossControlValueOnEnabled(DTSmartControl control)
        {
            AddCrossControlValueIfNotExist(_view.Target.CrossControlActions.ValueActions.ValuesOnEnable, control);
        }

        private void AddCrossControlValueIfNotExist(List<DTSmartControl.SmartControlCrossControlActions.ControlValueActions.ControlValue> values, DTSmartControl control)
        {
            // we invert the state here for user convenience
            if (values.Where(value => value.Control == control).FirstOrDefault() == null)
            {
                values.Add(new DTSmartControl.SmartControlCrossControlActions.ControlValueActions.ControlValue()
                {
                    Control = control,
                    Value = control.AnimatorConfig.ParameterDefaultValue
                });
                UpdateCrossControlValues();
                _view.Repaint();
            }
        }

        private static bool GetComponentOrGameObjectOriginalState(Component component)
        {
            if (component is Transform)
            {
                // treat as gameobject
                return component.gameObject.activeSelf;
            }
            else if (component is Behaviour behaviour)
            {
                return behaviour.enabled;
            }
            else
            {
                return false;
            }
        }

        private void OnAddObjectToggle(Component component)
        {
            AddObjectToggleIfNotExist(component);
        }

        private void AddObjectToggleIfNotExist(Component component)
        {
            // we invert the state here for user convenience
            if (_view.Target.ObjectToggles.Where(toggle => toggle.Target == component).FirstOrDefault() == null)
            {
                _view.Target.ObjectToggles.Add(new DTSmartControl.ObjectToggle()
                {
                    Target = component,
                    Enabled = !GetComponentOrGameObjectOriginalState(component)
                });
                UpdateObjectToggles();
                _view.Repaint();
            }
        }

        private void OnDriverChanged()
        {
            // TODO: other driver types
            Undo.RecordObject(_view.Target, "Driver Changed");
            _view.Target.DriverType = (DTSmartControl.SmartControlDriverType)_view.DriverType;
            _view.Repaint();
        }

        private void OnAnimatorParameterDefaultValueChanged()
        {
            Undo.RecordObject(_view.Target, "Animator Parameter Default Value Changed");
            _view.Target.AnimatorConfig.ParameterDefaultValue = _view.AnimatorParameterDefaultValue;
        }

        private void OnControlTypeChanged()
        {
            Undo.RecordObject(_view.Target, "Control Type Changed");
            _view.Target.ControlType = (DTSmartControl.SmartControlControlType)_view.ControlType;

            if (_view.Target.ControlType == DTSmartControl.SmartControlControlType.Binary &&
                _view.Target.DriverType == DTSmartControl.SmartControlDriverType.MenuItem &&
                _view.Target.MenuItemDriverConfig.ItemType != DTMenuItem.ItemType.Button &&
                _view.Target.MenuItemDriverConfig.ItemType != DTMenuItem.ItemType.Toggle)
            {
                _view.MenuItemType = 1; // toggle
                _view.Target.MenuItemDriverConfig.ItemType = DTMenuItem.ItemType.Toggle;
                _view.Repaint();
            }
        }

        private static void GetSameObjectComponentTypes(Component go, List<Type> types)
        {
            types.Clear();
            if (go == null)
            {
                return;
            }

            var comps = go.GetComponents<Component>();
            foreach (var comp in comps)
            {
                var type = comp.GetType();
                if (!types.Contains(type))
                {
                    types.Add(comp.GetType());
                }
            }
        }

        private void UpdateObjectToggles()
        {
            _view.ObjectToggles.Clear();
            foreach (var objToggle in _view.Target.ObjectToggles)
            {
                var types = new List<Type>();
                GetSameObjectComponentTypes(objToggle.Target, types);
                _view.ObjectToggles.Add(new SmartControlObjectToggleValue()
                {
                    target = objToggle.Target,
                    enabled = objToggle.Enabled,
                    sameObjectComponentTypes = types,
                });
            }
        }

        private void UpdatePropertyGroups()
        {
            _view.PropertyGroups.Clear();
            _view.PropertyGroups.AddRange(_view.Target.PropertyGroups);
        }

        private void UpdateCrossControlValuesSection(List<SmartControlCrossControlValue> uiValues, List<DTSmartControl.SmartControlCrossControlActions.ControlValueActions.ControlValue> compValues)
        {
            uiValues.Clear();
            foreach (var compValue in compValues)
            {
                uiValues.Add(new SmartControlCrossControlValue()
                {
                    control = compValue.Control,
                    value = compValue.Value
                });
            }
        }

        private void UpdateCrossControlValues()
        {
            UpdateCrossControlValuesSection(_view.CrossControlValuesOnEnabled, _view.Target.CrossControlActions.ValueActions.ValuesOnEnable);
            UpdateCrossControlValuesSection(_view.CrossControlValuesOnDisabled, _view.Target.CrossControlActions.ValueActions.ValuesOnDisable);
        }

        private void UpdateCrossControls()
        {
            UpdateCrossControlValues();
        }

        private void UpdateDriverMenuItem()
        {
            _view.MenuItemName = _view.Target.name;
            _view.MenuItemIcon = _view.Target.MenuItemDriverConfig.ItemIcon;
            switch (_view.Target.MenuItemDriverConfig.ItemType)
            {
                case DTMenuItem.ItemType.Button:
                    _view.MenuItemType = 0;
                    break;
                default:
                case DTMenuItem.ItemType.Toggle:
                    _view.MenuItemType = 1;
                    break;
                case DTMenuItem.ItemType.Radial:
                    _view.MenuItemType = 2;
                    break;
            }
        }

        private void UpdateDrivers()
        {
            _view.DriverType = (int)_view.Target.DriverType;
            UpdateDriverMenuItem();
        }

        private void UpdateView()
        {
            _view.AnimatorParameterDefaultValue = _view.Target.AnimatorConfig.ParameterDefaultValue;
            _view.ControlType = (int)_view.Target.ControlType;

            UpdateDrivers();
            UpdateObjectToggles();
            UpdatePropertyGroups();
            UpdateCrossControls();
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
