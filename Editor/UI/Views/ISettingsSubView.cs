/*
 * File: ISettingsSubView.cs
 * Project: DressingTools
 * Created Date: Saturday, July 22nd 2023, 12:36:56 am
 * Author: chocopoi (poi@chocopoi.com)
 * -----
 * Copyright (c) 2023 chocopoi
 * 
 * This file is part of DressingTools.
 * 
 * DressingTools is free software: you can redistribute it and/or modify it under the terms of the GNU General License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 * 
 * DressingTools is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General License for more details.
 * 
 * You should have received a copy of the GNU General License along with DressingTools. If not, see <https://www.gnu.org/licenses/>.
 */

using System;
using System.Collections.Generic;

namespace Chocopoi.DressingTools.UI.Views
{
    internal interface ISettingsSubView : IEditorView
    {
        event Action LanguageChanged;
        event Action SettingsChanged;
        event Action UpdaterCheckUpdateButtonClicked;
        event Action ResetToDefaultsButtonClicked;

        bool ShowLanguageReloadWindowHelpbox { get; set; }

        List<string> AvailableLanguageKeys { get; set; }

        string LanguageSelected { get; set; }

        string CabinetDefaultsArmatureName { get; set; }
        bool CabinetDefaultsGroupDynamics { get; set; }
        bool CabinetDefaultsSeparateDynamics { get; set; }
        bool CabinetDefaultsAnimWriteDefaults { get; set; }

        string UpdaterCurrentVersion { get; set; }
        bool UpdaterShowHelpboxUpdateNotChecked { get; set; }
    }
}
