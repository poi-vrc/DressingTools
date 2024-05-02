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
using Chocopoi.DressingFramework.Localization;
using Chocopoi.DressingTools.Localization;
using Chocopoi.DressingTools.UI.Presenters;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Chocopoi.DressingTools.UI.Views
{
    [ExcludeFromCodeCoverage]
    internal class AvatarSubView : ElementView, IAvatarSubView
    {
        private static readonly I18nTranslator t = I18n.ToolTranslator;
        private static VisualTreeAsset s_installedThumbnailVisualTree = null;
        private static VisualTreeAsset s_addWearablePlaceholderVisualTree = null;
        private static StyleSheet s_outfitThumbnailStyleSheet = null;
        private static Texture2D s_thumbnailPlaceholder = null;

        public event Action AddOutfitButtonClick;
        public event Action SelectedAvatarChange;
        public event Action AvatarSettingsChange;

        public int SelectedAvatarIndex { get; set; }
        public List<string> AvailableAvatarSelections { get; set; }
        public GameObject SelectedAvatarGameObject { get; set; }
        public int SettingsAnimationWriteDefaultsMode { get; set; }
        public List<OutfitPreview> InstalledOutfitPreviews { get; set; }

        private IMainView _mainView;
        private AvatarPresenter _avatarPresenter;
        private VisualElement _avatarContentContainer;
        private VisualElement _installedOutfitIconsContainer;
        private VisualElement _installedOutfitListContainer;
        private PopupField<string> _animationWriteDefaultsPopup;
        private Button[] _displayModeBtns;
        private int _selectedDisplayMode;
        private PopupField<string> _avatarsPopup;

        public AvatarSubView(IMainView mainView)
        {
            _mainView = mainView;
            _avatarPresenter = new AvatarPresenter(this);
            SelectedAvatarIndex = 0;
            SelectedAvatarGameObject = null;
            _selectedDisplayMode = 0;

            AvailableAvatarSelections = new List<string>() { "" };
            InstalledOutfitPreviews = new List<OutfitPreview>();

            SettingsAnimationWriteDefaultsMode = 0;
        }

        public void SelectTab(int selectedTab)
        {
            _mainView.SelectedTab = selectedTab;
        }

        public void StartDressing(GameObject avatarGameObject = null, GameObject wearableGameObject = null)
        {
            _mainView.StartDressing(avatarGameObject != null ? avatarGameObject : SelectedAvatarGameObject, wearableGameObject);
        }

        public void SelectAvatar(GameObject avatarGameObject) => _avatarPresenter.SelectAvatar(avatarGameObject);

        private void BindCabinetContentDisplayModes()
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
                    UpdateCabinetContentDisplayModes();
                };
                _displayModeBtns[i].EnableInClassList("active", displayModeIndex == _selectedDisplayMode);
            }
        }

        private void UpdateCabinetContentDisplayModes()
        {
            for (var i = 0; i < _displayModeBtns.Length; i++)
            {
                _displayModeBtns[i].EnableInClassList("active", i == _selectedDisplayMode);
            }

            if (_selectedDisplayMode == 0)
            {
                _installedOutfitListContainer.style.display = DisplayStyle.None;
                _installedOutfitIconsContainer.style.display = DisplayStyle.Flex;
            }
            else if (_selectedDisplayMode == 1)
            {
                _installedOutfitListContainer.style.display = DisplayStyle.Flex;
                _installedOutfitIconsContainer.style.display = DisplayStyle.None;
            }
        }

        private void InitCabinetContentPopup()
        {
            var popupContainer = Q<VisualElement>("popup-cabinet-selection").First();
            _avatarsPopup = new PopupField<string>(t._("cabinet.editor.cabinetContent.popup.cabinet"), AvailableAvatarSelections, 0);
            _avatarsPopup.RegisterValueChangedCallback((ChangeEvent<string> evt) =>
            {
                int index = AvailableAvatarSelections.IndexOf(evt.newValue);
                SelectedAvatarIndex = index;
                SelectedAvatarChange?.Invoke();
            });
            popupContainer.Add(_avatarsPopup);
        }

        private void InitCabinetContentSettings()
        {
            var writeDefaultsPopupContainer = Q<VisualElement>("settings-anim-write-defaults-popup-container").First();
            var choices = new List<string>() {
                t._("cabinet.editor.cabinetContent.settings.popup.animationWriteDefaultsMode.auto"),
                t._("cabinet.editor.cabinetContent.settings.popup.animationWriteDefaultsMode.on"),
                t._("cabinet.editor.cabinetContent.settings.popup.animationWriteDefaultsMode.off")
                };
            _animationWriteDefaultsPopup = new PopupField<string>(t._("cabinet.editor.cabinetContent.settings.popup.animationWriteDefaultsMode"), choices, 0);
            _animationWriteDefaultsPopup.RegisterValueChangedCallback((evt) =>
            {
                SettingsAnimationWriteDefaultsMode = _animationWriteDefaultsPopup.index;
                AvatarSettingsChange?.Invoke();
            });
            writeDefaultsPopupContainer.Add(_animationWriteDefaultsPopup);
        }

        private void BindCabinetContentFoldouts()
        {
            BindFoldoutHeaderWithContainer("foldout-settings", "settings-container");
            BindFoldoutHeaderWithContainer("foldout-installed-wearables", "installed-wearables-container");
            // BindFoldoutHeaderWithContainer("foldout-available-wearables", "available-wearables-container");
        }

        private void InitAvatarContent()
        {
            _avatarContentContainer = Q<VisualElement>("cabinet-content-container").First();

            _installedOutfitIconsContainer = Q<VisualElement>("installed-wearable-icons-container").First();
            _installedOutfitListContainer = Q<VisualElement>("installed-wearable-list-container").First();

            InitCabinetContentSettings();
            InitCabinetContentPopup();
            BindCabinetContentFoldouts();
            BindCabinetContentDisplayModes();
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
        }

        public override void OnEnable()
        {
            InitVisualTree();
            InitAvatarContent();

            t.LocalizeElement(this);

            RaiseLoadEvent();
        }

        private VisualElement CreateAddPlaceholderElement()
        {
            if (s_addWearablePlaceholderVisualTree == null)
            {
                s_addWearablePlaceholderVisualTree = Resources.Load<VisualTreeAsset>("AddWearablePlaceholder");
            }

            if (s_outfitThumbnailStyleSheet == null)
            {
                s_outfitThumbnailStyleSheet = Resources.Load<StyleSheet>("CabinetWearableThumbnailStyles");
            }

            var element = new VisualElement();
            element.style.width = 128;
            element.style.height = 128;
            element.styleSheets.Add(s_outfitThumbnailStyleSheet);
            s_addWearablePlaceholderVisualTree.CloneTree(element);
            t.LocalizeElement(element);

            element.RegisterCallback((MouseDownEvent evt) => AddOutfitButtonClick?.Invoke());
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

            if (s_outfitThumbnailStyleSheet == null)
            {
                s_outfitThumbnailStyleSheet = Resources.Load<StyleSheet>("CabinetWearableThumbnailStyles");
            }

            if (s_thumbnailPlaceholder == null)
            {
                s_thumbnailPlaceholder = Resources.Load<Texture2D>("thumbnailPlaceholder");
            }

            var element = new VisualElement();
            element.style.width = 128;
            element.style.height = 128;
            element.styleSheets.Add(s_outfitThumbnailStyleSheet);
            s_installedThumbnailVisualTree.CloneTree(element);
            t.LocalizeElement(element);

            element.Q<Label>("label-name").text = wearableName;
            element.Q<Button>("btn-remove").clicked += () =>
            {
                if (EditorUtility.DisplayDialog(t._("tool.name"), t._("cabinet.editor.cabinetContent.dialog.msg.removeConfirm"), t._("common.dialog.btn.yes"), t._("common.dialog.btn.no")))
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

        private VisualElement CreateWearablePreviewListItem(OutfitPreview preview)
        {
            var listItem = new VisualElement();
            listItem.style.flexWrap = new StyleEnum<Wrap>(Wrap.Wrap);
            listItem.style.flexDirection = new StyleEnum<FlexDirection>(FlexDirection.Row);
            listItem.Add(new Label(preview.name));

            var listItemRemoveBtn = new Button()
            {
                text = t._("cabinet.editor.cabinetContent.btn.remove")
            };

            listItemRemoveBtn.clicked += () =>
            {
                if (EditorUtility.DisplayDialog(t._("tool.name"), t._("cabinet.editor.cabinetContent.dialog.msg.removeConfirm"), t._("common.dialog.btn.yes"), t._("common.dialog.btn.no")))
                {
                    preview.RemoveButtonClick.Invoke();
                }
            };

            listItem.Add(listItemRemoveBtn);

            return listItem;
        }

        private void RepaintCabinetContentSettings()
        {
            // update cabinet settings
            _animationWriteDefaultsPopup.index = SettingsAnimationWriteDefaultsMode;
        }

        private void RepaintCabinetContentInstalledWearables()
        {
            // update installed wearable container
            _installedOutfitIconsContainer.Clear();
            _installedOutfitListContainer.Clear();
            foreach (var preview in InstalledOutfitPreviews)
            {
                // TODO: edit button
                var thumbnail = CreateInstalledWearableThumbnailElement(preview.name, preview.thumbnail, preview.RemoveButtonClick, preview.EditButtonClick);
                _installedOutfitIconsContainer.Add(thumbnail);

                _installedOutfitListContainer.Add(CreateWearablePreviewListItem(preview));
            }
            _installedOutfitIconsContainer.Add(CreateAddPlaceholderElement());
        }

        private void RepaintCabinetContent()
        {
            _avatarsPopup.index = SelectedAvatarIndex;

            RepaintCabinetContentSettings();
            RepaintCabinetContentInstalledWearables();
        }

        public override void Repaint()
        {
            RepaintCabinetContent();
        }
    }
}
