/*
 * File: IMainView.cs
 * Project: DressingTools
 * Created Date: Wednesday, August 9th 2023, 8:34:36 pm
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
using Chocopoi.DressingFramework.UI;
using UnityEngine;

namespace Chocopoi.DressingTools.UIBase.Views
{
    internal interface IMainView : IEditorView
    {
        event Action UpdateAvailableUpdateButtonClick;
        event Action MouseMove;

        string UpdateAvailableFromVersion { get; set; }
        string UpdateAvailableToVersion { get; set; }
        bool ShowExitPlayModeHelpbox { get; set; }
        int SelectedTab { get; set; }

        void ForceUpdateCabinetSubView();
        void StartDressing(GameObject targetAvatar, GameObject targetWearable = null);
        void OpenUrl(string url);
    }
}
