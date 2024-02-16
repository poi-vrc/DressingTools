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
    internal interface ISmartControlPropertyGroupView : IEditorView, ISmartControlPropertyGroupComponentViewParent
    {
        event Action SettingsChanged;
        event Action<GameObject> AddGameObject;
        event Action<GameObject> RemoveGameObject;
        event Action<int, GameObject> ChangeGameObject;

        DTSmartControl.PropertyGroup Target { get; set; }
        int SelectionType { get; set; }
        Transform SearchTransform { get; set; }
        Transform PickFromTransform { get; set; }
        List<GameObject> SelectionGameObjects { get; set; }
        List<Component> FoundComponents { get; set; }
    }
}
