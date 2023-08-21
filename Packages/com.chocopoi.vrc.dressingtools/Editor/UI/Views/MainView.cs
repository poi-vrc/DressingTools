/*
 * File: MainView.cs
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

using System.Diagnostics.CodeAnalysis;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Lib.UI;
using Chocopoi.DressingTools.UI.Presenters;
using Chocopoi.DressingTools.UI.Views;
using Chocopoi.DressingTools.UIBase.Views;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.View
{
    [ExcludeFromCodeCoverage]
    internal class MainView : EditorViewBase, IMainView
    {
        public int SelectedTab { get => _selectedTab; set => _selectedTab = value; }

        private MainPresenter _presenter;
        private CabinetSubView _cabinetSubView;
        private DressingSubView _dressingSubView;
        private SettingsSubView _settingsSubView;
        private int _selectedTab;

        public MainView()
        {
            _presenter = new MainPresenter(this);
            _cabinetSubView = new CabinetSubView(this);
            _dressingSubView = new DressingSubView(this);
            _settingsSubView = new SettingsSubView(this);
        }

        public void StartSetupWizard(GameObject targetAvatar, GameObject targetWearable = null)
        {
            _selectedTab = 1;
            _dressingSubView.StartSetupWizard(targetAvatar, targetWearable);
        }

        public void SelectCabinet(DTCabinet cabinet) => _cabinetSubView.SelectCabinet(cabinet);

        public void ForceUpdateCabinetSubView()
        {
            _cabinetSubView.RaiseForceUpdateViewEvent();
        }

        public override void OnEnable()
        {
            base.OnEnable();
            _cabinetSubView.OnEnable();
            _dressingSubView.OnEnable();
        }

        public override void OnDisable()
        {
            base.OnDisable();
            _cabinetSubView.OnDisable();
            _dressingSubView.OnDisable();
        }

        public override void OnGUI()
        {
            DTLogo.Show();

            Toolbar(ref _selectedTab, new string[] { "Cabinet", "Dressing", "Settings" });

            if (_selectedTab == 0)
            {
                _cabinetSubView.OnGUI();
            }
            else if (_selectedTab == 1)
            {
                _dressingSubView.OnGUI();
            }
            else if (_selectedTab == 2)
            {
                _settingsSubView.OnGUI();
            }
        }
    }
}
