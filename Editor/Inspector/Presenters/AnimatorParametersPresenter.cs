/*
 * Copyright (c) 2024 chocopoi
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
using System.Linq;
using Chocopoi.DressingFramework;
using Chocopoi.DressingTools.Animations;
using Chocopoi.DressingTools.Components.Animations;
using Chocopoi.DressingTools.Inspector.Views;
using UnityEditor;
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Presenters
{
    internal class AnimatorParametersPresenter
    {
        private readonly IAnimatorParametersView _view;
        private Dictionary<string, AnimatorControllerParameterType> _parameters;
        private bool _refreshed = false;

        public AnimatorParametersPresenter(IAnimatorParametersView view)
        {
            _view = view;
            _parameters = new Dictionary<string, AnimatorControllerParameterType>();
            SubscribeEvents();
        }

        private void SubscribeEvents()
        {
            _view.Load += OnLoad;
            _view.Unload += OnUnload;

            _view.AddConfig += OnAddConfig;
            _view.RemoveConfig += OnRemoveConfig;
            _view.ChangeConfig += OnChangeConfig;
            _view.MouseEnter += OnMouseEnter;
            _view.MouseLeave += OnMouseLeave;

            EditorApplication.hierarchyChanged += OnHierachyChanged;
        }

        private void UnsubscribeEvents()
        {
            _view.Load -= OnLoad;
            _view.Unload -= OnUnload;

            _view.AddConfig -= OnAddConfig;
            _view.RemoveConfig -= OnRemoveConfig;
            _view.ChangeConfig -= OnChangeConfig;
            _view.MouseEnter -= OnMouseEnter;
            _view.MouseLeave -= OnMouseLeave;

            EditorApplication.hierarchyChanged -= OnHierachyChanged;
        }

        private void UpdateAnimatorParameterTextFieldAvatarGameObject()
        {
            var avatarRoot = DKRuntimeUtils.GetAvatarRoot(_view.Target.gameObject);
            var updateNeeded = false;
            if (_view.AnimatorParameterTextFieldAvatarGameObject != avatarRoot)
            {
                updateNeeded = true;
            }
            _view.AnimatorParameterTextFieldAvatarGameObject = avatarRoot;
            if (updateNeeded)
            {
                _view.Repaint();
            }
        }

        private void OnHierachyChanged()
        {
            UpdateAnimatorParameterTextFieldAvatarGameObject();
        }

        private void RefreshAnimatorParameters()
        {
            var avatarRoot = DKRuntimeUtils.GetAvatarRoot(_view.Target.gameObject);
            if (avatarRoot == null)
            {
                return;
            }
            _parameters = AnimUtils.ScanAnimatorParameters(avatarRoot);
        }

        private void OnMouseEnter()
        {
            if (!_refreshed)
            {
                UpdateView();
                _refreshed = true;
            }
        }

        private void OnMouseLeave()
        {
            _refreshed = false;
        }

        private void OnAddConfig()
        {
            _view.Target.Configs.Add(new DTAnimatorParameters.ParameterConfig());
            UpdateView();
        }

        private void OnChangeConfig(int idx)
        {
            var viewConfig = _view.Configs[idx];
            var targetConfig = _view.Target.Configs[idx];

            var updateNeeded = targetConfig.ParameterName != viewConfig.parameterName;

            targetConfig.ParameterName = viewConfig.parameterName;
            targetConfig.ParameterDefaultValue = viewConfig.defaultValue;
            targetConfig.NetworkSynced = viewConfig.networkSynced;
            targetConfig.Saved = viewConfig.saved;

            if (updateNeeded)
            {
                UpdateView();
            }
        }

        private void OnRemoveConfig(int idx)
        {
            _view.Target.Configs.RemoveAt(idx);
            _view.Configs.RemoveAt(idx);
            UpdateView();
        }

        private Type UnityTypeToCompatibleSystemType(AnimatorControllerParameterType type)
        {
            if (type == AnimatorControllerParameterType.Float)
            {
                return typeof(float);
            }
            else if (type == AnimatorControllerParameterType.Int)
            {
                return typeof(int);
            }
            else if (type == AnimatorControllerParameterType.Bool)
            {
                return typeof(bool);
            }
            else
            {
                return null;
            }
        }

        private void UpdateAnimatorParameterConfigs()
        {
            _view.Configs.Clear();
            foreach (var config in _view.Target.Configs)
            {
                Type type = null;

                if (!string.IsNullOrEmpty(config.ParameterName) &&
                    _parameters.ContainsKey(config.ParameterName))
                {
                    type = UnityTypeToCompatibleSystemType(_parameters[config.ParameterName]);
                }

                var viewConfig = new AnimatorParametersConfig()
                {
                    parameterName = config.ParameterName,
                    defaultValue = config.ParameterDefaultValue,
                    networkSynced = config.NetworkSynced,
                    saved = config.Saved,
                    type = type
                };
                _view.Configs.Add(viewConfig);
            }
        }

        private void UpdateView()
        {
            UpdateAnimatorParameterTextFieldAvatarGameObject();
            RefreshAnimatorParameters();
            UpdateAnimatorParameterConfigs();
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
