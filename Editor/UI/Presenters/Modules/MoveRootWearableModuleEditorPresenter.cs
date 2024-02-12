/*
 * File: MoveRootModuleEditorPresenter.cs
 * Project: DressingTools
 * Created Date: Wednesday, August 9th 2023, 8:34:36 pm
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

using Chocopoi.AvatarLib.Animations;
using Chocopoi.DressingFramework;
using Chocopoi.DressingTools.OneConf.Wearable.Modules.BuiltIn;
using Chocopoi.DressingTools.UI.Views.Modules;

namespace Chocopoi.DressingTools.UI.Presenters.Modules
{
    internal class MoveRootWearableModuleEditorPresenter
    {
        private IMoveRootWearableModuleEditorView _view;
        private IWearableModuleEditorViewParent _parentView;
        private MoveRootWearableModuleConfig _module;

        public MoveRootWearableModuleEditorPresenter(IMoveRootWearableModuleEditorView view, IWearableModuleEditorViewParent parentView, MoveRootWearableModuleConfig module)
        {
            _view = view;
            _parentView = parentView;
            _module = module;

            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            _view.Load += OnLoad;
            _view.Unload += OnUnload;

            _view.ForceUpdateView += OnForceUpdateView;
            _view.MoveToGameObjectFieldChange += OnMoveToGameObjectFieldChange;
        }

        private void UnsubscribeEvents()
        {
            _view.Load -= OnLoad;
            _view.Unload -= OnUnload;

            _view.ForceUpdateView -= OnForceUpdateView;
            _view.MoveToGameObjectFieldChange -= OnMoveToGameObjectFieldChange;
        }

        private void OnForceUpdateView()
        {
            ApplyMoveToGameObjectFieldChanges();
            UpdateView();
        }

        private void UpdateView()
        {
            if (_parentView.TargetAvatar != null)
            {
                _view.ShowSelectAvatarFirstHelpBox = false;

                var avatarTransform = _module.avatarPath != null ? _parentView.TargetAvatar.transform.Find(_module.avatarPath) : null;

                if (avatarTransform != null)
                {
                    _view.MoveToGameObject = avatarTransform.gameObject;
                    _view.IsGameObjectInvalid = false;
                }
                else
                {
                    _view.MoveToGameObject = null;
                    _view.IsGameObjectInvalid = true;
                }
            }
            else
            {
                _view.ShowSelectAvatarFirstHelpBox = true;
            }
        }

        private void OnMoveToGameObjectFieldChange()
        {
            ApplyMoveToGameObjectFieldChanges();
        }

        private void ApplyMoveToGameObjectFieldChanges()
        {
            if (_parentView.TargetAvatar != null && _view.MoveToGameObject != null && DKEditorUtils.IsGrandParent(_parentView.TargetAvatar.transform, _view.MoveToGameObject.transform))
            {
                _view.IsGameObjectInvalid = false;

                // renew path if valid
                _module.avatarPath = AnimationUtils.GetRelativePath(_view.MoveToGameObject.transform, _parentView.TargetAvatar.transform);
            }
            else
            {
                // show helpbox that the path is invalid
                _view.IsGameObjectInvalid = true;
            }
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
