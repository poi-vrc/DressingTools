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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Chocopoi.DressingFramework.Localization;
using Chocopoi.DressingTools.Localization;
using Chocopoi.DressingTools.UI.Presenters;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

#if UNITY_2021_2_OR_NEWER
using PrefabStage = UnityEditor.SceneManagement.PrefabStage;
#elif UNITY_2018_3_OR_NEWER
using PrefabStage = UnityEditor.Experimental.SceneManagement.PrefabStage;
#else
#error The current Unity version does not support PrefabStage.
#endif

namespace Chocopoi.DressingTools.UI.Views
{
    [ExcludeFromCodeCoverage]
    internal class MainView : ElementView, IMainView
    {
        private static readonly I18nTranslator t = I18n.ToolTranslator;

        public event Action<PrefabStage> PrefabStageClosing { add { PrefabStage.prefabStageClosing += value; } remove { PrefabStage.prefabStageClosing -= value; } }
        public event Action<PrefabStage> PrefabStageOpened { add { PrefabStage.prefabStageOpened += value; } remove { PrefabStage.prefabStageOpened -= value; } }
        public event Action UpdateAvailableUpdateButtonClick;
        public event Action MouseMove;
        public event Action AvatarSelectionPopupChange;

        public int SelectedAvatarIndex { get; set; }
        public List<GameObject> AvailableAvatars { get; set; }
        public GameObject SelectedAvatarGameObject
        {
            get => (AvailableAvatars.Count > 0 && SelectedAvatarIndex < AvailableAvatars.Count) ? AvailableAvatars[SelectedAvatarIndex] : null;
        }

        public int SelectedTab
        {
            get => _selectedTab;
            set
            {
                _selectedTab = value;
                UpdateTabs();
            }
        }

        public string UpdateAvailableFromVersion { get; set; }
        public string UpdateAvailableToVersion { get; set; }
        public bool ShowExitPlayModeHelpbox { get; set; }
        public bool ShowExitPrefabModeHelpbox { get; set; }
        public string ToolVersionText { get; set; }

        private MainPresenter _presenter;
        private AvatarSubView _avatarSubView;
        private OneConfDressSubView _dressSubView;
        private ToolSettingsSubView _toolSettingsSubView;
        private int _selectedTab;
        private Button[] _tabBtns;
        private Foldout _updateAvailableFoldout;
        private VisualElement _helpboxContainer;
        private VisualElement _tabContainer;
        private VisualElement _tabContentContainer;
        private VisualElement _avatarSelectionPopupContainer;
        private VisualElement _debugInfoContainer;
        private PopupField<string> _avatarSelectionPopup;
        private int _lastSelectedAvatarIndex;

        public MainView()
        {
            _presenter = new MainPresenter(this);
            _avatarSubView = new AvatarSubView(this);
            _dressSubView = new OneConfDressSubView(this);
            _toolSettingsSubView = new ToolSettingsSubView(this);

            UpdateAvailableFromVersion = null;
            UpdateAvailableToVersion = null;
            ShowExitPlayModeHelpbox = false;
            SelectedAvatarIndex = 0;
            _lastSelectedAvatarIndex = 0;
            AvailableAvatars = new List<GameObject>();
        }

        public void StartDressing(GameObject outfitGameObject = null, GameObject avatarGameObject = null)
        {
            _selectedTab = 1;
            UpdateTabs();
            if (avatarGameObject != null)
            {
                SelectAvatar(avatarGameObject);
            }
            _dressSubView.StartDressing(outfitGameObject);
        }

        public void SelectAvatar(GameObject avatarGameObject) => _presenter.SelectAvatar(avatarGameObject);

        public void ForceUpdateCabinetSubView()
        {
            _avatarSubView.RaiseForceUpdateViewEvent();
        }

        public override void OnEnable()
        {
            InitVisualTree();
            BindTabs();

            t.LocalizeElement(this);

            RaiseLoadEvent();
            _avatarSubView.OnEnable();
            _dressSubView.OnEnable();
            _toolSettingsSubView.OnEnable();
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

            _avatarSubView.style.display = DisplayStyle.Flex;
            // hide all
            _dressSubView.style.display = DisplayStyle.None;
            _toolSettingsSubView.style.display = DisplayStyle.None;

            _helpboxContainer = Q<VisualElement>("helpbox-container").First();
            _tabContainer = Q<VisualElement>("tab").First();

            _tabContentContainer = Q<VisualElement>("tab-content").First();
            _tabContentContainer.Add(_avatarSubView);
            _tabContentContainer.Add(_dressSubView);
            _tabContentContainer.Add(_toolSettingsSubView);

            _updateAvailableFoldout = Q<Foldout>("update-available-foldout").First();
            var updateBtn = Q<Button>("update-available-update-btn").First();
            updateBtn.clicked += UpdateAvailableUpdateButtonClick;

            _avatarSelectionPopupContainer = Q<VisualElement>("popup-avatar-selection").First();
            _debugInfoContainer = Q<VisualElement>("debug-info").First();

            RegisterCallback((MouseMoveEvent evt) => MouseMove?.Invoke());
        }

        private void BindTabs()
        {
            var avatarBtn = Q<Button>("tab-avatar");
            var dressBtn = Q<Button>("tab-dress");
            var settingsBtn = Q<Button>("tab-tool-settings");

            _tabBtns = new Button[] { avatarBtn, dressBtn, settingsBtn };

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
            _avatarSubView.style.display = DisplayStyle.None;
            _dressSubView.style.display = DisplayStyle.None;
            _toolSettingsSubView.style.display = DisplayStyle.None;

            if (_selectedTab == 0)
            {
                _avatarSubView.style.display = DisplayStyle.Flex;
                _avatarSelectionPopupContainer.style.display = DisplayStyle.Flex;
            }
            else if (_selectedTab == 1)
            {
                _dressSubView.style.display = DisplayStyle.Flex;
                _avatarSelectionPopupContainer.style.display = DisplayStyle.Flex;
            }
            else if (_selectedTab == 2)
            {
                _toolSettingsSubView.style.display = DisplayStyle.Flex;
                _avatarSelectionPopupContainer.style.display = DisplayStyle.None;
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            _avatarSubView.OnDisable();
            _dressSubView.OnDisable();
            _toolSettingsSubView.OnDisable();
        }
        public void OpenUrl(string url) => Application.OpenURL(url);

        private void RepaintHelpboxes()
        {
            _helpboxContainer.Clear();
            if (ShowExitPlayModeHelpbox)
            {
                _helpboxContainer.Add(CreateHelpBox(t._("editor.main.helpbox.exitPlayMode"), UnityEditor.MessageType.Warning));
            }
            if (ShowExitPrefabModeHelpbox)
            {
                _helpboxContainer.Add(CreateHelpBox(t._("editor.main.helpbox.exitPrefabMode"), UnityEditor.MessageType.Warning));
            }
        }

        private void RepaintTabs()
        {
            _tabContainer.style.display = ShowExitPlayModeHelpbox || ShowExitPrefabModeHelpbox ? DisplayStyle.None : DisplayStyle.Flex;
            _tabContentContainer.style.display = ShowExitPlayModeHelpbox || ShowExitPrefabModeHelpbox ? DisplayStyle.None : DisplayStyle.Flex;
        }

        private void RepaintUpdateNotification()
        {
            if (UpdateAvailableFromVersion != null && UpdateAvailableToVersion != null)
            {
                _updateAvailableFoldout.style.display = DisplayStyle.Flex;
                _updateAvailableFoldout.text = t._("editor.main.updateAvailable.foldout", UpdateAvailableFromVersion, UpdateAvailableToVersion);
            }
            else
            {
                _updateAvailableFoldout.style.display = DisplayStyle.None;
            }
        }

        private void RepaintAvatarSelection()
        {
            var labels = new List<string>();
            foreach (var avatar in AvailableAvatars)
            {
                labels.Add(avatar.name);
            }

            _avatarSelectionPopupContainer.Clear();
            _avatarSelectionPopup = new PopupField<string>(t._("editor.main.popup.selectedAvatar"), labels, SelectedAvatarIndex);
            _avatarSelectionPopup.RegisterValueChangedCallback((ChangeEvent<string> evt) =>
            {
                SelectedAvatarIndex = _avatarSelectionPopup.index;
                AvatarSelectionPopupChange?.Invoke();
            });
            _avatarSelectionPopupContainer.Add(_avatarSelectionPopup);

            // force update avatar subview if the selected index has changed
            if (_lastSelectedAvatarIndex != SelectedAvatarIndex)
            {
                _lastSelectedAvatarIndex = SelectedAvatarIndex;
                _avatarSubView.RaiseForceUpdateViewEvent();
            }
        }

        private void RepaintDebugInfo()
        {
            _debugInfoContainer.Clear();
            _debugInfoContainer.Add(new Label(ToolVersionText));
        }

        public override void Repaint()
        {
            RepaintHelpboxes();
            RepaintTabs();
            RepaintUpdateNotification();
            RepaintAvatarSelection();
            RepaintDebugInfo();
        }

        public void RaiseAvatarSelectionPopupChangeEvent() => AvatarSelectionPopupChange?.Invoke();
    }
}
