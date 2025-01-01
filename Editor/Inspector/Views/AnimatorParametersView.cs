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
using System.Diagnostics.CodeAnalysis;
using Chocopoi.DressingFramework.Localization;
using Chocopoi.DressingTools.Components.Animations;
using Chocopoi.DressingTools.Localization;
using Chocopoi.DressingTools.UI.Elements;
using Chocopoi.DressingTools.UI.Presenters;
using Chocopoi.DressingTools.UI.Views;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Chocopoi.DressingTools.Inspector.Views
{
    [ExcludeFromCodeCoverage]
    internal class AnimatorParametersView : ElementView, IAnimatorParametersView
    {
        private static readonly I18nTranslator t = I18n.ToolTranslator;

        public event Action MouseEnter;
        public event Action MouseLeave;
        public event Action AddConfig;
        public event Action<int> RemoveConfig;
        public event Action<int> ChangeConfig;

        public DTAnimatorParameters Target { get; set; }
        public List<AnimatorParametersConfig> Configs { get; set; }

        public GameObject AnimatorParameterTextFieldAvatarGameObject { get; set; }

        private readonly AnimatorParametersPresenter _presenter;
        private readonly List<AnimatorParameterTextField> _animParamTextFields;
        private VisualElement _helpBoxContainer;
        private VisualElement _configsContainer;

        public AnimatorParametersView()
        {
            Configs = new List<AnimatorParametersConfig>();
            _presenter = new AnimatorParametersPresenter(this);
            _animParamTextFields = new List<AnimatorParameterTextField>();

            InitVisualTree();
            RegisterCallback<MouseEnterEvent>(evt => MouseEnter?.Invoke());
            RegisterCallback<MouseLeaveEvent>(evt => MouseLeave?.Invoke());
            t.LocalizeElement(this);
        }

        private void InitVisualTree()
        {
            var tree = Resources.Load<VisualTreeAsset>("AnimatorParametersView");
            tree.CloneTree(this);
            var styleSheet = Resources.Load<StyleSheet>("AnimatorParametersViewStyles");
            if (!styleSheets.Contains(styleSheet))
            {
                styleSheets.Add(styleSheet);
            }

            _helpBoxContainer = Q<VisualElement>("helpbox-container");
            _helpBoxContainer.Add(CreateHelpBox(t._("inspector.animParams.helpbox.description"), MessageType.Info));
            _configsContainer = Q<VisualElement>("configs-container");
            var addConfigBtn = Q<Button>("add-config-btn");
            addConfigBtn.clicked += AddConfig;
        }

        private Box MakeConfigBox(int idx, AnimatorParametersConfig config)
        {
            var box = new Box();
            box.AddToClassList("config-view-container");

            var headerContainer = new VisualElement();
            headerContainer.AddToClassList("header-container");

            var nameField = new AnimatorParameterTextField()
            {
                label = t._("inspector.animParams.label.parameter"),
                isDelayed = true,
                avatarGameObject = AnimatorParameterTextFieldAvatarGameObject,
                value = config.parameterName
            };
            _animParamTextFields.Add(nameField);
            nameField.RegisterValueChangedCallback(evt =>
            {
                config.parameterName = nameField.value;
                ChangeConfig?.Invoke(idx);
            });
            headerContainer.Add(nameField);
            box.Add(headerContainer);

            var rmvBtn = new Button(() => RemoveConfig?.Invoke(idx))
            {
                text = "x"
            };
            headerContainer.Add(rmvBtn);

            if (config.type == typeof(bool))
            {
                var defValToggle = new Toggle(t._("inspector.animParams.defaultValue"))
                {
                    value = config.defaultValue == 1.0f
                };
                defValToggle.RegisterValueChangedCallback(evt =>
                {
                    config.defaultValue = defValToggle.value ? 1.0f : 0.0f;
                    ChangeConfig?.Invoke(idx);
                });
                box.Add(defValToggle);
            }
            else
            {
                if (config.type == null)
                {
                    box.Add(CreateHelpBox(t._("inspector.animParams.helpbox.noCompatibleParameterFound"), MessageType.Warning));
                }
                // TODO add int,float constraints
                var defValFloatField = new FloatField(t._("inspector.animParams.defaultValue"))
                {
                    value = config.defaultValue
                };
                defValFloatField.RegisterValueChangedCallback(evt =>
                {
                    config.defaultValue = defValFloatField.value;
                    ChangeConfig?.Invoke(idx);
                });
                box.Add(defValFloatField);
            }

            var settingsContainer = new VisualElement();
            settingsContainer.AddToClassList("settings-container");
            box.Add(settingsContainer);

            var networkSyncedToggle = new Toggle(t._("inspector.animParams.toggle.networkSynced"))
            {
                value = config.networkSynced
            };
            networkSyncedToggle.RegisterValueChangedCallback(evt =>
            {
                config.networkSynced = networkSyncedToggle.value;
                ChangeConfig?.Invoke(idx);
            });
            settingsContainer.Add(networkSyncedToggle);

            var savedToggle = new Toggle(t._("inspector.animParams.toggle.saved"))
            {
                value = config.saved
            };
            savedToggle.RegisterValueChangedCallback(evt =>
            {
                config.saved = savedToggle.value;
                ChangeConfig?.Invoke(idx);
            });
            settingsContainer.Add(savedToggle);

            return box;
        }

        private void RepaintConfigs()
        {
            _configsContainer.Clear();
            _animParamTextFields.Clear();
            for (var i = 0; i < Configs.Count; i++)
            {
                _configsContainer.Add(MakeConfigBox(i, Configs[i]));
            }
        }

        public override void Repaint()
        {
            RepaintConfigs();
        }
    }
}
