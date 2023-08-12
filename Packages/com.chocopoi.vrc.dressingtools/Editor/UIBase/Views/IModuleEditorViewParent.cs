/*
 * File: IModuleEditorViewParent.cs
 * Project: DressingTools
 * Created Date: Saturday, August 5th 2023, 11:44:30 am
 * Author: chocopoi (poi@chocopoi.com)
 * -----
 * Copyright (c) 2023 chocopoi
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
using Chocopoi.DressingTools.UI.Views.Modules;
using Chocopoi.DressingTools.Wearable;
using UnityEngine;

namespace Chocopoi.DressingTools.UIBase.Views
{
    internal interface IModuleEditorViewParent : IEditorView
    {
        event Action TargetAvatarOrWearableChange;
        GameObject TargetAvatar { get; }
        GameObject TargetWearable { get; }
    }
}
