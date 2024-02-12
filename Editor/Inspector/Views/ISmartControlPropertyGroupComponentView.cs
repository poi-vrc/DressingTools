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
    internal enum SmartControlComponentViewResultMode
    {
        None = 0,
        Blendshapes = 1,
        Material = 2,
        Generic = 3,
    }

    internal interface ISmartControlPropertyGroupComponentView : IEditorView
    {
        event Action SearchResultModeChange;
        event Action SearchQueryChange;
        event Action<string, object> AddProperty;
        event Action<string> RemoveProperty;
        event Action<string, object> ChangeProperty;


        Component TargetComponent { get; set; }
        List<DTSmartControl.PropertyGroup.PropertyValue> TargetPropertyValues { get; set; }
        List<KeyValuePair<string, object>> SearchResults { get; set; }
        SmartControlComponentViewResultMode SearchResultMode { get; set; }
        Dictionary<string, object> Properties { get; set; }
        string SearchQuery { get; set; }
        bool DisplayAllResults { get; set; }
        bool HasBlendshapes { get; set; }
        bool HasMaterialProperties { get; set; }
        bool HasGenericProperties { get; set; }
    }
}
