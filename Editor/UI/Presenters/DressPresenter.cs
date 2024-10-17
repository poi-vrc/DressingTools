/*
 * File: DressingPresenter.cs
 * Project: DressingTools
 * Created Date: Saturday, August 12th 2023, 1:22:09 am
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
using Chocopoi.DressingFramework;
using Chocopoi.DressingTools.Components.OneConf;
using Chocopoi.DressingTools.Configurator;
using Chocopoi.DressingTools.Configurator.Cabinet;
using Chocopoi.DressingTools.UI.Views;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Presenters
{
    internal class DressPresenter
    {
        private IDressSubView _view;

        public DressPresenter(IDressSubView view)
        {
            _view = view;

            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            _view.Load += OnLoad;
            _view.Unload += OnUnload;

            _view.ForceUpdateView += OnForceUpdateView;
            _view.StartButtonClick += OnStartButtonClick;
        }

        private void UnsubscribeEvents()
        {
            _view.Load -= OnLoad;
            _view.Unload -= OnUnload;

            _view.ForceUpdateView -= OnForceUpdateView;
            _view.StartButtonClick -= OnStartButtonClick;
        }

        private void OnForceUpdateView()
        {
            UpdateView();
        }

        private void OnStartButtonClick()
        {
            if (_view.SelectedAvatarGameObject == null)
            {
                Debug.LogError("Avatar is not selected.");
                return;
            }
            if (_view.SelectedOutfitGameObject == null)
            {
                Debug.LogError("Outfit is not selected.");
                return;
            }
            if (!DKEditorUtils.IsGrandParent(_view.SelectedAvatarGameObject.transform, _view.SelectedOutfitGameObject.transform) && _view.SelectedAvatarGameObject.transform.Find(_view.SelectedOutfitGameObject.name) != null)
            {
                Debug.LogError("There is already an outfit with the same name in the avatar.");
                return;
            }
            if (!_view.SelectedOutfitGameObject.TryGetComponent<DTWearable>(out var oneConfWearableComp))
            {
                oneConfWearableComp = _view.SelectedOutfitGameObject.AddComponent<DTWearable>();
            }
            var outfit = new OneConfConfigurableOutfit(_view.SelectedAvatarGameObject, oneConfWearableComp);
            AvatarPreviewUtility.StartAvatarPreview(_view.SelectedAvatarGameObject, outfit);
        }

        private void UpdateView()
        {
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
    }
}
