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
using Chocopoi.DressingTools.Lib.Cabinet;
using Chocopoi.DressingTools.Lib.UI;
using Chocopoi.DressingTools.UI.Presenters;
using Chocopoi.DressingTools.UI.Views;
using Chocopoi.DressingTools.UIBase.Views;
using UnityEngine;
using UnityEngine.UIElements;

namespace Chocopoi.DressingTools.UI.View
{
    [ExcludeFromCodeCoverage]
    internal class MainView : ElementViewBase, IMainView
    {
        private static readonly Localization.I18n t = Localization.I18n.Instance;

        public int SelectedTab
        {
            get => _selectedTab;
            set
            {
                _selectedTab = value;
                UpdateTabs();
            }
        }

        private MainPresenter _presenter;
        private CabinetSubView _cabinetSubView;
        private DressingSubView _dressingSubView;
        private SettingsSubView _settingsSubView;
        private int _selectedTab;
        private Button[] _tabBtns;

        public MainView()
        {
            _presenter = new MainPresenter(this);
            _cabinetSubView = new CabinetSubView(this);
            _dressingSubView = new DressingSubView(this);
            _settingsSubView = new SettingsSubView(this);
        }

        public void StartDressing(GameObject targetAvatar, GameObject targetWearable = null)
        {
            _selectedTab = 1;
            UpdateTabs();
            _dressingSubView.StartDressing(targetAvatar, targetWearable);
        }

        public void SelectCabinet(DTCabinet cabinet) => _cabinetSubView.SelectCabinet(cabinet);

        public void ForceUpdateCabinetSubView()
        {
            _cabinetSubView.RaiseForceUpdateViewEvent();
        }

        public override void OnEnable()
        {
            base.OnEnable();

            InitVisualTree();
            BindTabs();

            t.LocalizeElement(this);

            _cabinetSubView.OnEnable();
            _dressingSubView.OnEnable();
        }

        private void InitVisualTree()
        {
            var tree = Resources.Load<VisualTreeAsset>("MainView");
            tree.CloneTree(this);
            var styleSheet = Resources.Load<StyleSheet>("MainViewStyles");
            if (!styleSheets.Contains(styleSheet))
            {
                styleSheets.Add(styleSheet);
            }

            _cabinetSubView.style.display = DisplayStyle.Flex;
            // hide all
            _dressingSubView.style.display = DisplayStyle.None;
            _settingsSubView.style.display = DisplayStyle.None;

            var container = Q<VisualElement>("tab-content").First();
            container.Add(_cabinetSubView);
            container.Add(_dressingSubView);
            container.Add(_settingsSubView);
        }

        private void BindTabs()
        {
            var cabinetBtn = Q<Button>("tab-cabinet");
            var dressingBtn = Q<Button>("tab-dressing");
            var settingsBtn = Q<Button>("tab-settings");

            _tabBtns = new Button[] { cabinetBtn, dressingBtn, settingsBtn };

            for (var i = 0; i < _tabBtns.Length; i++)
            {
                var tabIndex = i;
                _tabBtns[i].clicked += () =>
                {
                    if (_selectedTab == tabIndex) return;
                    _selectedTab = tabIndex;
                    UpdateTabs();
                };
                _tabBtns[i].EnableInClassList("active", tabIndex == _selectedTab);
            }
        }

        private void UpdateTabs()
        {
            for (var i = 0; i < _tabBtns.Length; i++)
            {
                _tabBtns[i].EnableInClassList("active", i == _selectedTab);
            }

            // hide all
            _cabinetSubView.style.display = DisplayStyle.None;
            _dressingSubView.style.display = DisplayStyle.None;
            _settingsSubView.style.display = DisplayStyle.None;

            if (_selectedTab == 0)
            {
                _cabinetSubView.style.display = DisplayStyle.Flex;
            }
            else if (_selectedTab == 1)
            {
                _dressingSubView.style.display = DisplayStyle.Flex;
            }
            else if (_selectedTab == 2)
            {
                _settingsSubView.style.display = DisplayStyle.Flex;
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            _cabinetSubView.OnDisable();
            _dressingSubView.OnDisable();
        }
    }
}
