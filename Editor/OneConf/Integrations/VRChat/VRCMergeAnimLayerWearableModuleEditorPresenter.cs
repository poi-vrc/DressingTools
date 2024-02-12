/*
 * File: VRCMergeAnimLayerWearableModuleEditorPresenter.cs
 * Project: DressingTools
 * Created Date: Wednesday, August 23th 2023, 7:56:36 pm
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

#if DT_VRCSDK3A
using System;
using Chocopoi.AvatarLib.Animations;
using Chocopoi.DressingFramework;
using Chocopoi.DressingTools.OneConf.Wearable.Modules.Integrations.VRChat;
using Chocopoi.DressingTools.UI.Views.Modules;
using UnityEngine;

namespace Chocopoi.DressingTools.OneConf.Integrations.VRChat
{
    internal class VRCMergeAnimLayerWearableModuleEditorPresenter
    {
        private static readonly VRCMergeAnimLayerWearableModuleConfig.AnimLayer[] AnimLayerEnums = new VRCMergeAnimLayerWearableModuleConfig.AnimLayer[] {
            VRCMergeAnimLayerWearableModuleConfig.AnimLayer.Base,
            VRCMergeAnimLayerWearableModuleConfig.AnimLayer.Additive,
            VRCMergeAnimLayerWearableModuleConfig.AnimLayer.Gesture,
            VRCMergeAnimLayerWearableModuleConfig.AnimLayer.Action,
            VRCMergeAnimLayerWearableModuleConfig.AnimLayer.FX,
            VRCMergeAnimLayerWearableModuleConfig.AnimLayer.Sitting,
            VRCMergeAnimLayerWearableModuleConfig.AnimLayer.TPose,
            VRCMergeAnimLayerWearableModuleConfig.AnimLayer.IKPose
        };
        private IVRCMergeAnimLayerWearableModuleEditorView _view;
        private IWearableModuleEditorViewParent _parentView;
        private VRCMergeAnimLayerWearableModuleConfig _module;

        public VRCMergeAnimLayerWearableModuleEditorPresenter(IVRCMergeAnimLayerWearableModuleEditorView view, IWearableModuleEditorViewParent parentView, VRCMergeAnimLayerWearableModuleConfig module)
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
            _view.ConfigChange += OnConfigChange;
        }

        private void UnsubscribeEvents()
        {
            _view.Load -= OnLoad;
            _view.Unload -= OnUnload;

            _view.ForceUpdateView -= OnForceUpdateView;
            _view.ConfigChange -= OnConfigChange;
        }

        private void OnForceUpdateView()
        {
            UpdateView();
        }

        private void OnConfigChange()
        {
            _module.animLayer = AnimLayerEnums[_view.SelectedAnimLayerIndex];
            _module.pathMode = (VRCMergeAnimLayerWearableModuleConfig.PathMode)_view.SelectedPathMode;
            _module.removeAnimatorAfterApply = _view.RemoveAnimatorAfterApply;
            _module.matchLayerWriteDefaults = _view.MatchLayerWriteDefaults;

            if (_parentView.TargetWearable == null)
            {
                return;
            }

            if (_view.AnimatorObject == null)
            {
                _module.animatorPath = "";
            }
            else if (DKEditorUtils.IsGrandParent(_parentView.TargetWearable.transform, _view.AnimatorObject.transform))
            {
                _module.animatorPath = AnimationUtils.GetRelativePath(_view.AnimatorObject.transform, _parentView.TargetWearable.transform);
            }

            UpdateView();
        }

        public bool IsValid()
        {
            return true;
        }

        private void UpdateView()
        {
            // make anim layer keys
            _view.AnimLayerKeys = new string[AnimLayerEnums.Length];
            for (var i = 0; i < _view.AnimLayerKeys.Length; i++)
            {
                _view.AnimLayerKeys[i] = AnimLayerEnums[i].ToString();
            }

            _view.SelectedAnimLayerIndex = Array.IndexOf(AnimLayerEnums, _module.animLayer);
            if (_view.SelectedAnimLayerIndex == -1)
            {
                _view.SelectedAnimLayerIndex = 0;
            }

            _view.SelectedPathMode = (int)_module.pathMode;
            _view.RemoveAnimatorAfterApply = _module.removeAnimatorAfterApply;
            _view.MatchLayerWriteDefaults = _module.matchLayerWriteDefaults;

            if (_parentView.TargetAvatar == null || _parentView.TargetWearable == null)
            {
                _view.ShowNoTargetAvatarOrWearableHelpbox = true;
                return;
            }
            _view.ShowNoTargetAvatarOrWearableHelpbox = false;

            // use root
            if (_module.animatorPath == "")
            {
                _view.AnimatorObject = _parentView.TargetWearable;
            }
            else
            {
                // find animator object
                var animatorTransform = _parentView.TargetWearable.transform.Find(_module.animatorPath);
                if (animatorTransform == null)
                {
                    _view.AnimatorObject = null;
                    _view.ShowAnimatorObjectPathNotFoundHelpbox = true;
                    return;
                }
                _view.AnimatorObject = animatorTransform.gameObject;
                _view.ShowAnimatorObjectPathNotFoundHelpbox = false;

                // verify if in wearable
                if (!DKEditorUtils.IsGrandParent(_parentView.TargetWearable.transform, _view.AnimatorObject.transform))
                {
                    _view.ShowNotInWearableHelpbox = true;
                    return;
                }
                _view.ShowNotInWearableHelpbox = false;
            }

            // find animator
            if (!_view.AnimatorObject.TryGetComponent<Animator>(out _))
            {
                _view.ShowNoAnimatorHelpbox = true;
                return;
            }
            _view.ShowNoAnimatorHelpbox = false;
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
#endif
