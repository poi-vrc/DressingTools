/*
 * File: CabinetSubView.cs
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
using System.Diagnostics.CodeAnalysis;
using Chocopoi.DressingTools.Lib.Cabinet;
using Chocopoi.DressingTools.Lib.UI;
using Chocopoi.DressingTools.UI.Presenters;
using Chocopoi.DressingTools.UIBase.Views;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Views
{
    [ExcludeFromCodeCoverage]
    internal class CabinetSubView : EditorViewBase, ICabinetSubView
    {
        public event Action CreateCabinetButtonClick;
        public event Action AddWearableButtonClick;
        public event Action SelectedCabinetChange;
        public event Action CabinetSettingsChange;

        public bool ShowCreateCabinetWizard { get; set; }
        public bool ShowCabinetWearables { get; set; }
        public int SelectedCabinetIndex { get => _selectedCabinetIndex; set => _selectedCabinetIndex = value; }
        public string[] AvailableCabinetSelections { get; set; }
        public GameObject CabinetAvatarGameObject { get => _cabinetAvatarGameObject; set => _cabinetAvatarGameObject = value; }
        public string CabinetAvatarArmatureName { get => _cabinetAvatarArmatureName; set => _cabinetAvatarArmatureName = value; }
        public List<WearablePreview> WearablePreviews { get; set; }
        public GameObject SelectedCreateCabinetGameObject { get => _selectedCreateCabinetGameObject; }

        private IMainView _mainView;
        private CabinetPresenter _cabinetPresenter;
        private GameObject _selectedCreateCabinetGameObject;
        private int _selectedCabinetIndex;
        private GameObject _cabinetAvatarGameObject;
        private string _cabinetAvatarArmatureName;

        public CabinetSubView(IMainView mainView)
        {
            _mainView = mainView;
            _cabinetPresenter = new CabinetPresenter(this);
            _selectedCabinetIndex = 0;

            ShowCreateCabinetWizard = false;
            ShowCabinetWearables = false;
            AvailableCabinetSelections = new string[0];
            WearablePreviews = new List<WearablePreview>();
        }

        public void SelectTab(int selectedTab)
        {
            _mainView.SelectedTab = selectedTab;
        }

        public void StartSetupWizard()
        {
            _mainView.StartSetupWizard(_cabinetAvatarGameObject);
        }

        public void SelectCabinet(DTCabinet cabinet) => _cabinetPresenter.SelectCabinet(cabinet);

        public override void OnGUI()
        {
            if (ShowCreateCabinetWizard)
            {
                Label("There are no existing cabinets. Create one below for your avatar:");
                GameObjectField("Avatar", ref _selectedCreateCabinetGameObject, true);
                Button("Create cabinet", CreateCabinetButtonClick);
            }

            if (ShowCabinetWearables)
            {
                // create dropdown menu for cabinet selection
                Popup("Cabinet", ref _selectedCabinetIndex, AvailableCabinetSelections, SelectedCabinetChange);

                GameObjectField("Avatar", ref _cabinetAvatarGameObject, true, CabinetSettingsChange);
                TextField("Armature Name", ref _cabinetAvatarArmatureName, CabinetSettingsChange);

                var copy = new List<WearablePreview>(WearablePreviews);
                foreach (var preview in copy)
                {
                    Label(preview.name);
                    Button("Remove", preview.RemoveButtonClick);
                }

                Button("Add Wearable", AddWearableButtonClick);
            }
        }
    }
}
