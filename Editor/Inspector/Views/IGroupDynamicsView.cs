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
using Chocopoi.DressingTools.Components.Modifiers;
using Chocopoi.DressingTools.UI.Views;
using UnityEngine;

namespace Chocopoi.DressingTools.Inspector.Views
{
    internal interface IGroupDynamicsView : IEditorView
    {
        event Action ConfigChange;
        event Action<Transform> AddInclude;
        event Action<Transform> AddExclude;
        event Action<int, Transform> ChangeInclude;
        event Action<int, Transform> ChangeExclude;
        event Action<int> RemoveInclude;
        event Action<int> RemoveExclude;

        DTGroupDynamics Target { get; set; }
        int SearchMode { get; set; }
        List<Transform> IncludeTransforms { get; set; }
        List<Transform> ExcludeTransforms { get; set; }
    }
}
