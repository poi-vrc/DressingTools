/*
 * File: ICabinetSubView.cs
 * Project: DressingTools
 * Created Date: Wednesday, August 9th 2023, 11:38:52 pm
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
using Chocopoi.DressingTools.UI.Views;
using UnityEngine;

namespace Chocopoi.DressingTools.Configurator.Views
{
    internal interface IOneConfCabinetView : IEditorView
    {
        event Action SettingsChanged;

        string ArmatureName { get; set; }
        bool GroupDynamics { get; set; }
        bool GroupDynamicsSeparate { get; set; }
        bool UseThumbnails { get; set; }
        bool ResetCustomizablesOnSwitch { get; set; }
        string MenuInstallPathField { get; set; }
        string MenuItemNameField { get; set; }
        bool NetworkSyncedToggle { get; set; }
        bool SavedToggle { get; set; }
    }
}
