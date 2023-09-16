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
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Chocopoi.DressingTools.UI.Views
{
    [ExcludeFromCodeCoverage]
    internal class CabinetSubView : ElementViewBase, ICabinetSubView
    {
        private static readonly Localization.I18n t = Localization.I18n.Instance;
        private static VisualTreeAsset s_installedThumbnailVisualTree = null;
        private static VisualTreeAsset s_addWearablePlaceholderVisualTree = null;
        private static StyleSheet s_wearableThumbnailStyleSheet = null;
        private static Texture2D s_thumbnailPlaceholder = null;

        public event Action CreateCabinetButtonClick;
        public event Action AddWearableButtonClick;
        public event Action SelectedCabinetChange;
        public event Action CabinetSettingsChange;

        public bool ShowCreateCabinetWizard { get; set; }
        public bool ShowCabinetWearables { get; set; }
        public int SelectedCabinetIndex { get => _selectedCabinetIndex; set => _selectedCabinetIndex = value; }
        public List<string> AvailableCabinetSelections { get; set; }
        public GameObject CabinetAvatarGameObject { get => _cabinetAvatarGameObject; set => _cabinetAvatarGameObject = value; }
        public string CabinetAvatarArmatureName { get => _cabinetAvatarArmatureName; set => _cabinetAvatarArmatureName = value; }
        public bool CabinetGroupDynamics { get; set; }
        public bool CabinetGroupDynamicsSeparateGameObjects { get; set; }
        public bool CabinetAnimationWriteDefaults { get; set; }
        public List<CabinetModulePreview> CabinetModulePreviews { get; set; }
        public List<WearablePreview> InstalledWearablePreviews { get; set; }
        public GameObject SelectedCreateCabinetGameObject { get => _selectedCreateCabinetGameObject; }

        private IMainView _mainView;
        private CabinetPresenter _cabinetPresenter;
        private GameObject _selectedCreateCabinetGameObject;
        private int _selectedCabinetIndex;
        private GameObject _cabinetAvatarGameObject;
        private string _cabinetAvatarArmatureName;
        private VisualElement _installedWearableIconsContainer;
        private VisualElement _installedWearableListContainer;
        private TextField _avatarArmatureNameField;
        private Toggle _groupDynamicsToggle;
        private Toggle _groupDynamicsSeparateGameObjectsToggle;
        private Toggle _animationWriteDefaultsToggle;
        private VisualElement _cabinetModulesContainer;
        private Button[] _displayModeBtns;
        private int _selectedDisplayMode;

        public CabinetSubView(IMainView mainView)
        {
            _mainView = mainView;
            _cabinetPresenter = new CabinetPresenter(this);
            _selectedCabinetIndex = 0;
            _selectedDisplayMode = 0;

            ShowCreateCabinetWizard = false;
            ShowCabinetWearables = false;
            AvailableCabinetSelections = new List<string>() { "" };
            CabinetModulePreviews = new List<CabinetModulePreview>();
            InstalledWearablePreviews = new List<WearablePreview>();
        }

        public void SelectTab(int selectedTab)
        {
            _mainView.SelectedTab = selectedTab;
        }

        public void StartDressing(GameObject avatarGameObject = null, GameObject wearableGameObject = null)
        {
            _mainView.StartDressing(avatarGameObject != null ? avatarGameObject : _cabinetAvatarGameObject, wearableGameObject);
        }

        public void SelectCabinet(DTCabinet cabinet) => _cabinetPresenter.SelectCabinet(cabinet);

        public override void OnEnable()
        {
            InitVisualTree();
            InitCabinetModules();
            BindCabinetSettings();
            base.OnEnable();
            InitCabinetPopup();
            BindFoldouts();
            BindDisplayModes();

            t.LocalizeElement(this);
        }

        private void InitVisualTree()
        {
            var tree = Resources.Load<VisualTreeAsset>("CabinetSubView");
            tree.CloneTree(this);
            var styleSheet = Resources.Load<StyleSheet>("CabinetSubViewStyles");
            if (!styleSheets.Contains(styleSheet))
            {
                styleSheets.Add(styleSheet);
            }

            _installedWearableIconsContainer = Q<VisualElement>("installed-wearable-icons-container").First();
            _installedWearableListContainer = Q<VisualElement>("installed-wearable-list-container").First();
        }

        private void BindDisplayModes()
        {
            var iconsBtn = Q<Button>("btn-display-mode-icons");
            var listBtn = Q<Button>("btn-display-mode-list");

            _displayModeBtns = new Button[] { iconsBtn, listBtn };

            for (var i = 0; i < _displayModeBtns.Length; i++)
            {
                var displayModeIndex = i;
                _displayModeBtns[i].clicked += () =>
                {
                    if (_selectedDisplayMode == displayModeIndex) return;
                    _selectedDisplayMode = displayModeIndex;
                    UpdateDisplayModes();
                };
                _displayModeBtns[i].EnableInClassList("active", displayModeIndex == _selectedDisplayMode);
            }
        }

        private void UpdateDisplayModes()
        {
            for (var i = 0; i < _displayModeBtns.Length; i++)
            {
                _displayModeBtns[i].EnableInClassList("active", i == _selectedDisplayMode);
            }

            if (_selectedDisplayMode == 0)
            {
                _installedWearableListContainer.style.display = DisplayStyle.None;
                _installedWearableIconsContainer.style.display = DisplayStyle.Flex;
            }
            else if (_selectedDisplayMode == 1)
            {
                _installedWearableListContainer.style.display = DisplayStyle.Flex;
                _installedWearableIconsContainer.style.display = DisplayStyle.None;
            }
        }

        private void InitCabinetPopup()
        {
            var popupContainer = Q<VisualElement>("popup-cabinet-selection").First();
            var cabinetPopup = new PopupField<string>("Cabinet", AvailableCabinetSelections, 0);
            cabinetPopup.RegisterValueChangedCallback((ChangeEvent<string> evt) =>
            {
                int index = AvailableCabinetSelections.IndexOf(evt.newValue);
                _selectedCabinetIndex = index;
                SelectedCabinetChange?.Invoke();
            });
            popupContainer.Add(cabinetPopup);
        }

        private void InitCabinetModules()
        {
            // TODO: cabinet module editors
            _cabinetModulesContainer = Q<VisualElement>("settings-modules-container").First();
        }

        private void BindCabinetSettings()
        {
            _avatarArmatureNameField = Q<TextField>("settings-avatar-armature-name").First();
            _avatarArmatureNameField.RegisterValueChangedCallback((ChangeEvent<string> evt) =>
            {
                CabinetAvatarArmatureName = evt.newValue;
                CabinetSettingsChange?.Invoke();
            });

            _groupDynamicsToggle = Q<Toggle>("settings-group-dynamics").First();
            _groupDynamicsToggle.RegisterValueChangedCallback((ChangeEvent<bool> evt) =>
            {
                CabinetGroupDynamics = evt.newValue;
                CabinetSettingsChange?.Invoke();
            });

            _groupDynamicsSeparateGameObjectsToggle = Q<Toggle>("settings-group-dynamics-separate").First();
            _groupDynamicsSeparateGameObjectsToggle.RegisterValueChangedCallback((ChangeEvent<bool> evt) =>
            {
                CabinetGroupDynamicsSeparateGameObjects = evt.newValue;
                CabinetSettingsChange?.Invoke();
            });

            _animationWriteDefaultsToggle = Q<Toggle>("settings-anim-write-defaults").First();
            _animationWriteDefaultsToggle.RegisterValueChangedCallback((ChangeEvent<bool> evt) =>
            {
                CabinetAnimationWriteDefaults = evt.newValue;
                CabinetSettingsChange?.Invoke();
            });
        }

        private void BindFoldouts()
        {
            var settingsFoldout = Q<Foldout>("foldout-settings").First();
            var settingsContainer = Q<VisualElement>("settings-container").First();
            settingsFoldout.RegisterValueChangedCallback((ChangeEvent<bool> evt) => settingsContainer.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None);

            var installedWearablesFoldout = Q<Foldout>("foldout-installed-wearables").First();
            var installedWearablesContainer = Q<VisualElement>("installed-wearables-container").First();
            installedWearablesFoldout.RegisterValueChangedCallback((ChangeEvent<bool> evt) => installedWearablesContainer.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None);

            // var availableWearablesFoldout = Q<Foldout>("foldout-available-wearables").First();
            // var availableWearablesContainer = Q<VisualElement>("available-wearables-container").First();
            // availableWearablesFoldout.RegisterValueChangedCallback((ChangeEvent<bool> evt) => availableWearablesContainer.style.display = evt.newValue ? DisplayStyle.Flex : DisplayStyle.None);
        }

        private VisualElement CreateAddPlaceholderElement()
        {
            if (s_addWearablePlaceholderVisualTree == null)
            {
                s_addWearablePlaceholderVisualTree = Resources.Load<VisualTreeAsset>("AddWearablePlaceholder");
            }

            if (s_wearableThumbnailStyleSheet == null)
            {
                s_wearableThumbnailStyleSheet = Resources.Load<StyleSheet>("CabinetWearableThumbnailStyles");
            }

            var element = new VisualElement();
            element.style.width = 128;
            element.style.height = 128;
            element.styleSheets.Add(s_wearableThumbnailStyleSheet);
            s_addWearablePlaceholderVisualTree.CloneTree(element);
            t.LocalizeElement(element);

            element.RegisterCallback((MouseDownEvent evt) => AddWearableButtonClick?.Invoke());
            element.RegisterCallback((MouseEnterEvent evt) => element.EnableInClassList("hover", true));
            element.RegisterCallback((MouseLeaveEvent evt) => element.EnableInClassList("hover", false));

            return element;
        }

        private VisualElement CreateInstalledWearableThumbnailElement(string wearableName, Texture2D thumbnail, Action removeBtnClick, Action editBtnClick)
        {
            if (s_installedThumbnailVisualTree == null)
            {
                s_installedThumbnailVisualTree = Resources.Load<VisualTreeAsset>("CabinetWearableInstalledThumbnail");
            }

            if (s_wearableThumbnailStyleSheet == null)
            {
                s_wearableThumbnailStyleSheet = Resources.Load<StyleSheet>("CabinetWearableThumbnailStyles");
            }

            if (s_thumbnailPlaceholder == null)
            {
                s_thumbnailPlaceholder = Resources.Load<Texture2D>("thumbnailPlaceholder");
            }

            var element = new VisualElement();
            element.style.width = 128;
            element.style.height = 128;
            element.styleSheets.Add(s_wearableThumbnailStyleSheet);
            s_installedThumbnailVisualTree.CloneTree(element);
            t.LocalizeElement(element);

            element.Q<Label>("label-name").text = wearableName;
            element.Q<Button>("btn-remove").clicked += () =>
            {
                if (EditorUtility.DisplayDialog(t._("tool.name"), t._("cabinet.editor.dialog.msg.removeConfirm"), t._("common.dialog.btn.yes"), t._("common.dialog.btn.no")))
                {
                    removeBtnClick.Invoke();
                }
            };
            element.Q<Button>("btn-edit").clicked += () =>
            {
                editBtnClick.Invoke();
            };
            element.RegisterCallback((MouseEnterEvent evt) => element.EnableInClassList("hover", true));
            element.RegisterCallback((MouseLeaveEvent evt) => element.EnableInClassList("hover", false));

            element.style.backgroundImage = new StyleBackground(thumbnail != null ? thumbnail : s_thumbnailPlaceholder);

            return element;
        }

        private VisualElement CreateWearablePreviewListItem(WearablePreview preview)
        {
            var listItem = new VisualElement();
            listItem.style.flexWrap = new StyleEnum<Wrap>(Wrap.Wrap);
            listItem.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
            listItem.Add(new Label(preview.name));

            var listItemRemoveBtn = new Button()
            {
                text = t._("cabinet.editor.btn.remove")
            };

            listItemRemoveBtn.clicked += () =>
            {
                if (EditorUtility.DisplayDialog(t._("tool.name"), t._("cabinet.editor.dialog.msg.removeConfirm"), t._("common.dialog.btn.yes"), t._("common.dialog.btn.no")))
                {
                    preview.RemoveButtonClick.Invoke();
                }
            };

            listItem.Add(listItemRemoveBtn);

            return listItem;
        }

        public void Repaint()
        {
            // update cabinet settings
            _avatarArmatureNameField.value = CabinetAvatarArmatureName;
            _groupDynamicsToggle.value = CabinetGroupDynamics;
            _groupDynamicsSeparateGameObjectsToggle.value = CabinetGroupDynamicsSeparateGameObjects;
            _animationWriteDefaultsToggle.value = CabinetAnimationWriteDefaults;

            _cabinetModulesContainer.Clear();
            foreach (var preview in CabinetModulePreviews)
            {
                // TODO: cabinet module editor
                var previewContainer = new VisualElement();
                previewContainer.style.flexWrap = new StyleEnum<Wrap>(Wrap.Wrap);
                previewContainer.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
                previewContainer.Add(new Label(preview.name));

                var removeBtn = new Button
                {
                    text = t._("cabinet.editor.btn.remove")
                };
                removeBtn.clicked += () =>
                {
                    if (EditorUtility.DisplayDialog(t._("tool.name"), t._("cabinet.editor.dialog.msg.removeConfirm"), t._("common.dialog.btn.yes"), t._("common.dialog.btn.no")))
                    {
                        preview.RemoveButtonClick.Invoke();
                    }
                };

                previewContainer.Add(removeBtn);
                _cabinetModulesContainer.Add(previewContainer);
            }

            // update installed wearable container
            _installedWearableIconsContainer.Clear();
            _installedWearableListContainer.Clear();
            foreach (var preview in InstalledWearablePreviews)
            {
                // TODO: edit button
                var thumbnail = CreateInstalledWearableThumbnailElement(preview.name, preview.thumbnail, preview.RemoveButtonClick, preview.EditButtonClick);
                _installedWearableIconsContainer.Add(thumbnail);

                _installedWearableListContainer.Add(CreateWearablePreviewListItem(preview));
            }
            _installedWearableIconsContainer.Add(CreateAddPlaceholderElement());
        }
    }
}
