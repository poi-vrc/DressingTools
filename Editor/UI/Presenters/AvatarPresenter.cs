/*
 * File: CabinetPresenter.cs
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

using System.Collections.Generic;
using Chocopoi.DressingFramework.Localization;
using Chocopoi.DressingTools.Configurator.Avatar;
using Chocopoi.DressingTools.Localization;
using Chocopoi.DressingTools.UI.Views;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Chocopoi.DressingTools.UI.Presenters
{
    internal class AvatarPresenter
    {
        private static readonly I18nTranslator t = I18n.ToolTranslator;

        private IAvatarSubView _view;

        public AvatarPresenter(IAvatarSubView view)
        {
            _view = view;

            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            _view.Load += OnLoad;
            _view.Unload += OnUnload;

            _view.AddOutfitButtonClick += OnAddOutfitButtonClick;
            _view.ForceUpdateView += OnForceUpdateView;
            _view.AvatarSettingsChange += OnAvatarSettingsChange;

            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void UnsubscribeEvents()
        {
            _view.Load -= OnLoad;
            _view.Unload -= OnUnload;

            _view.AddOutfitButtonClick -= OnAddOutfitButtonClick;
            _view.ForceUpdateView -= OnForceUpdateView;
            _view.AvatarSettingsChange -= OnAvatarSettingsChange;

            EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredEditMode)
            {
                UpdateView();
            }
        }

        private void OnHierarchyChanged()
        {
            if (Application.isPlaying) return;
            UpdateView();
        }

        private void OnAvatarSettingsChange()
        {
            var settings = AvatarUtils.GetAvatarSettings(_view.SelectedAvatarGameObject);
            if (settings == null)
            {
                return;
            }

            // TODO: explicitly convert one by one
            settings.WriteDefaultsMode = (WriteDefaultsModes)_view.SettingsAnimationWriteDefaultsMode;
        }

        private void OnForceUpdateView()
        {
            UpdateView();
        }

        private void UpdateAvatarContentView()
        {
            _view.InstalledOutfitPreviews.Clear();

            // TODO: explicitly convert one by one
            var settings = AvatarUtils.GetAvatarSettings(_view.SelectedAvatarGameObject);
            if (settings != null)
            {
                _view.SettingsAnimationWriteDefaultsMode = (int)settings.WriteDefaultsMode;
            }

            var wardrobe = AvatarUtils.GetWardrobeProvider(_view.SelectedAvatarGameObject);
            if (wardrobe != null)
            {
                var outfits = wardrobe.GetOutfits();

                foreach (var outfit in outfits)
                {
                    _view.InstalledOutfitPreviews.Add(new OutfitPreview()
                    {
                        name = outfit.Name,
                        thumbnail = outfit.Icon,
                        RemoveButtonClick = () =>
                        {
                            wardrobe.RemoveOutfit(outfit);
                            UpdateView();
                        },
                        EditButtonClick = () =>
                        {
                            _view.StartDressing(_view.SelectedAvatarGameObject, outfit.RootTransform.gameObject);
                        }
                    });
                }
            }
        }

        private void UpdateView()
        {
            UpdateAvatarContentView();
            _view.Repaint();
        }

        private void OnLoad()
        {
            UpdateView();
        }

        private void OnUnload()
        {
            UnsubscribeEvents();
        }

        private void OnAddOutfitButtonClick()
        {
            _view.StartDressing(_view.SelectedAvatarGameObject);
        }
    }
}
