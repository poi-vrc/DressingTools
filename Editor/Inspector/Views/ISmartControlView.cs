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
using Chocopoi.DressingTools.Components.Animations;
using Chocopoi.DressingTools.UI.Views;
using UnityEngine;

namespace Chocopoi.DressingTools.Inspector.Views
{
    internal class SmartControlObjectToggleValue
    {
        public Component target;
        public bool enabled;
    }

    internal class SmartControlCrossControlValue
    {
        public DTSmartControl control;
        public float value;
    }

    internal interface ISmartControlView : IEditorView
    {
        event Action DriverChanged;
        event Action AnimatorParameterDefaultValueChanged;
        event Action ControlTypeChanged;
        event Action<Component> AddObjectToggle;
        event Action<DTSmartControl> AddCrossControlValueOnEnabled;
        event Action<DTSmartControl> AddCrossControlValueOnDisabled;
        event Action<SmartControlObjectToggleValue> ChangeObjectToggle;
        event Action<SmartControlObjectToggleValue> RemoveObjectToggle;
        event Action<SmartControlCrossControlValue> ChangeCrossControlValueOnEnabled;
        event Action<SmartControlCrossControlValue> ChangeCrossControlValueOnDisabled;
        event Action<SmartControlCrossControlValue> RemoveCrossControlValueOnEnabled;
        event Action<SmartControlCrossControlValue> RemoveCrossControlValueOnDisabled;
        event Action AddNewPropertyGroup;
        event Action<DTSmartControl.PropertyGroup> RemovePropertyGroup;

        List<SmartControlObjectToggleValue> ObjectToggles { get; set; }
        List<SmartControlCrossControlValue> CrossControlValuesOnEnabled { get; set; }
        List<SmartControlCrossControlValue> CrossControlValuesOnDisabled { get; set; }
        List<DTSmartControl.PropertyGroup> PropertyGroups { get; set; }

        DTSmartControl Target { get; set; }
        int DriverType { get; set; }
        float AnimatorParameterDefaultValue { get; set; }

        int ControlType { get; set; }
    }
}
