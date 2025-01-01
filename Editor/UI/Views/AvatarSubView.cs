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
using Chocopoi.DressingTools.Configurator.Avatar;
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
        private static VisualTreeAsset s_avatarOutfitPreviewVisualTree = null;
        private static VisualTreeAsset s_addWearablePlaceholderVisualTree = null;
        private static StyleSheet s_avatarSubViewStyleSheet = null;
        private static Texture2D s_thumbnailPlaceholder = null;

        public event Action AddOutfitButtonClick;
        public event Action AvatarSettingsChange;
        public event Action AvatarSelectionChange { add => _mainView.AvatarSelectionChange += value; remove => _mainView.AvatarSelectionChange -= value; }

        public int SettingsAnimationWriteDefaultsMode { get; set; }
        public List<OutfitPreview> InstalledOutfitPreviews { get; set; }
        public GameObject SelectedAvatarGameObject { get => _mainView.SelectedAvatarGameObject; }

        private readonly IMainView _mainView;
        private readonly AvatarPresenter _avatarPresenter;
        private VisualElement _conceptChangedHelpboxContainer;
        private VisualElement _createAvatarContainer;
        private VisualElement _avatarContentContainer;
        private VisualElement _wardrobeSettingsContainer;
        private VisualElement _installedOutfitContainer;
        private PopupField<string> _animationWriteDefaultsPopup;

        public AvatarSubView(IMainView mainView)
        {
            _mainView = mainView;
            _avatarPresenter = new AvatarPresenter(this);
            InstalledOutfitPreviews = new List<OutfitPreview>();

            SettingsAnimationWriteDefaultsMode = 0;

            InitVisualTree();
            InitAvatarContent();
            t.LocalizeElement(this);
        }

        public void SelectTab(int selectedTab)
        {
            _mainView.SelectedTab = selectedTab;
        }

        public void StartDressing(GameObject outfitGameObject = null, GameObject avatarGameObject = null)
        {
            _mainView.StartDressing(outfitGameObject, avatarGameObject);
        }

        private void InitSettings()
        {
            // TODO: Create view from avatar settings directly?
            var container = Q<VisualElement>("settings-container");
            container.Clear();

            var choices = new List<string>() {
                t._("editor.main.avatar.settings.avatar.popup.animationWriteDefaultsMode.auto"),
                t._("editor.main.avatar.settings.avatar.popup.animationWriteDefaultsMode.on"),
                t._("editor.main.avatar.settings.avatar.popup.animationWriteDefaultsMode.off")
                };
            _animationWriteDefaultsPopup = new PopupField<string>(t._("editor.main.avatar.settings.avatar.popup.animationWriteDefaultsMode"), choices, 0);
            _animationWriteDefaultsPopup.RegisterValueChangedCallback((evt) =>
            {
                SettingsAnimationWriteDefaultsMode = _animationWriteDefaultsPopup.index;
                AvatarSettingsChange?.Invoke();
            });
            container.Add(_animationWriteDefaultsPopup);

            _wardrobeSettingsContainer = new VisualElement();
            container.Add(_wardrobeSettingsContainer);
        }

        private void BindFoldouts()
        {
            BindFoldoutHeaderWithContainer("foldout-settings", "settings-container");
            BindFoldoutHeaderWithContainer("foldout-outfits", "outfits-container");
        }

        private void InitAvatarContent()
        {
            _conceptChangedHelpboxContainer = Q<VisualElement>("concept-changed-helpbox-container");
            _conceptChangedHelpboxContainer.Add(CreateHelpBox(t._("editor.main.avatar.settings.avatar.helpbox.conceptAndLocationChanged"), MessageType.Info));

            _createAvatarContainer = Q<VisualElement>("create-avatar-container");
            _avatarContentContainer = Q<VisualElement>("avatar-content-container");
            _installedOutfitContainer = Q<VisualElement>("outfits-container");

            InitSettings();
            BindFoldouts();
        }

        private void InitVisualTree()
        {
            var tree = Resources.Load<VisualTreeAsset>("AvatarSubView");
            tree.CloneTree(this);
            var styleSheet = Resources.Load<StyleSheet>("AvatarSubViewStyles");
            if (!styleSheets.Contains(styleSheet))
            {
                styleSheets.Add(styleSheet);
            }
        }

        private VisualElement CreateAddPlaceholderElement()
        {
            if (s_addWearablePlaceholderVisualTree == null)
            {
                s_addWearablePlaceholderVisualTree = Resources.Load<VisualTreeAsset>("AddOutfitPlaceholder");
            }

            if (s_avatarSubViewStyleSheet == null)
            {
                s_avatarSubViewStyleSheet = Resources.Load<StyleSheet>("AvatarSubViewStyles");
            }

            var element = new VisualElement();
            element.style.width = 128;
            element.style.height = 128;
            element.styleSheets.Add(s_avatarSubViewStyleSheet);
            s_addWearablePlaceholderVisualTree.CloneTree(element);
            t.LocalizeElement(element);

            element.RegisterCallback((MouseDownEvent evt) => AddOutfitButtonClick?.Invoke());
            element.RegisterCallback((MouseEnterEvent evt) => element.EnableInClassList("hover", true));
            element.RegisterCallback((MouseLeaveEvent evt) => element.EnableInClassList("hover", false));

            return element;
        }

        private VisualElement CreateAvatarOutfitPreviewElement(string wearableName, Texture2D thumbnail, Action removeBtnClick, Action editBtnClick)
        {
            if (s_avatarOutfitPreviewVisualTree == null)
            {
                s_avatarOutfitPreviewVisualTree = Resources.Load<VisualTreeAsset>("AvatarOutfitPreview");
            }

            if (s_avatarSubViewStyleSheet == null)
            {
                s_avatarSubViewStyleSheet = Resources.Load<StyleSheet>("AvatarSubViewStyles");
            }

            if (s_thumbnailPlaceholder == null)
            {
                s_thumbnailPlaceholder = Resources.Load<Texture2D>("thumbnailPlaceholder");
            }

            var element = new VisualElement();
            element.style.width = 128;
            element.style.height = 128;
            element.styleSheets.Add(s_avatarSubViewStyleSheet);
            s_avatarOutfitPreviewVisualTree.CloneTree(element);
            t.LocalizeElement(element);

            element.Q<Label>("name-label").text = wearableName;
            element.Q<Button>("remove-btn").clicked += () =>
            {
                if (EditorUtility.DisplayDialog(t._("tool.name"), t._("editor.main.avatar.dialog.msg.removeConfirm"), t._("common.dialog.btn.yes"), t._("common.dialog.btn.no")))
                {
                    removeBtnClick.Invoke();
                }
            };
            element.Q<Button>("edit-btn").clicked += () =>
            {
                editBtnClick.Invoke();
            };
            element.RegisterCallback((MouseEnterEvent evt) => element.EnableInClassList("hover", true));
            element.RegisterCallback((MouseLeaveEvent evt) => element.EnableInClassList("hover", false));

            element.style.backgroundImage = new StyleBackground(thumbnail != null ? thumbnail : s_thumbnailPlaceholder);

            return element;
        }

        public override void Repaint()
        {
            if (SelectedAvatarGameObject == null)
            {
                _createAvatarContainer.style.display = DisplayStyle.Flex;
                _avatarContentContainer.style.display = DisplayStyle.None;
            }
            else
            {
                _createAvatarContainer.style.display = DisplayStyle.None;
                _avatarContentContainer.style.display = DisplayStyle.Flex;
            }

            _wardrobeSettingsContainer.Clear();
            var wardrobeProvider = AvatarUtils.GetWardrobeProvider(SelectedAvatarGameObject);
            if (wardrobeProvider != null)
            {
                _wardrobeSettingsContainer.Add(wardrobeProvider.CreateView());
            }
            else
            {
                _wardrobeSettingsContainer.Add(CreateHelpBox(t._("editor.main.avatar.settings.avatar.helpbox.noWardrobeProviderSettings"), MessageType.Info));
            }

            // update avatar settings
            _animationWriteDefaultsPopup.index = SettingsAnimationWriteDefaultsMode;

            // update outfits container
            _installedOutfitContainer.Clear();
            foreach (var preview in InstalledOutfitPreviews)
            {
                // TODO: edit button
                var thumbnail = CreateAvatarOutfitPreviewElement(preview.name, preview.thumbnail, preview.RemoveButtonClick, preview.EditButtonClick);
                _installedOutfitContainer.Add(thumbnail);
            }
            _installedOutfitContainer.Add(CreateAddPlaceholderElement());
        }
    }
}
