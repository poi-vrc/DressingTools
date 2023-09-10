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
using Chocopoi.DressingTools.Lib.UI;
using UnityEngine;

namespace Chocopoi.DressingTools.UIBase.Views
{
    public struct WearablePreview
    {
        public string name;
        public Action RemoveButtonClick;
        public Action EditButtonClick;
        public Texture2D thumbnail;
    }

    public struct CabinetModulePreview
    {
        public string name;
        public Action RemoveButtonClick;
    }

    internal interface ICabinetSubView : IEditorView
    {
        event Action CreateCabinetButtonClick;
        event Action AddWearableButtonClick;
        event Action SelectedCabinetChange;
        event Action CabinetSettingsChange;

        bool ShowCreateCabinetWizard { get; set; }
        bool ShowCabinetWearables { get; set; }
        int SelectedCabinetIndex { get; set; }
        List<string> AvailableCabinetSelections { get; set; }
        GameObject CabinetAvatarGameObject { get; set; }
        string CabinetAvatarArmatureName { get; set; }
        bool CabinetGroupDynamics { get; set; }
        bool CabinetGroupDynamicsSeparateGameObjects { get; set; }
        bool CabinetAnimationWriteDefaults { get; set; }
        List<CabinetModulePreview> CabinetModulePreviews { get; set; }
        List<WearablePreview> InstalledWearablePreviews { get; set; }
        GameObject SelectedCreateCabinetGameObject { get; }

        void SelectTab(int selectedTab);
        void StartSetupWizard(GameObject avatarGameObject = null, GameObject wearableGameObject = null);
        void Repaint();
    }
}
