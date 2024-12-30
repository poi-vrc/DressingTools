/*
 * File: DTLogo.cs
 * Project: DressingTools
 * Created Date: Saturday, July 22nd 2023, 12:36:56 am
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
using Chocopoi.DressingTools.Animations;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Chocopoi.DressingTools.UI.Elements
{
    [ExcludeFromCodeCoverage]
    internal class AnimatorParameterTextField : VisualElement, INotifyValueChanged<string>
    {
        public new class UxmlFactory : UxmlFactory<AnimatorParameterTextField, UxmlTraits> { }

        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            readonly UxmlStringAttributeDescription m_Value =
                new()
                { name = "value", defaultValue = "" };
            readonly UxmlStringAttributeDescription m_Text =
                new()
                { name = "text", defaultValue = "" };
            readonly UxmlStringAttributeDescription m_Label =
                new()
                { name = "label", defaultValue = "" };

            public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
            {
                get { yield break; }
            }

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var ate = ve as AnimatorParameterTextField;

                ate.value = m_Value.GetValueFromBag(bag, cc);
                ate.text = m_Text.GetValueFromBag(bag, cc);
                ate.label = m_Label.GetValueFromBag(bag, cc);
            }
        }

        public string value
        {
            get => _textField.value;
            set
            {
                _textField.value = value;
                UpdatePopupSelectedIndex();
            }
        }

        public string text { get => _label.text; set => _label.text = value; }
        public string label { get => _label.text; set => _label.text = value; }
        public bool isDelayed { get => _textField.isDelayed; set => _textField.isDelayed = value; }

        public GameObject avatarGameObject
        {
            get => _avatarGameObject;
            set
            {
                _avatarGameObject = value;
                UpdateParameterChoices();
            }
        }

        private Label _label;
        private PopupField<string> _popupField;
        private TextField _textField;
        private readonly List<string> _parameterChoices;
        private bool _refreshed;
        private GameObject _avatarGameObject;

        public AnimatorParameterTextField()
        {
            _parameterChoices = new List<string>();
            _refreshed = false;
            avatarGameObject = null;

            InitVisualTree();
            InitCallbacks();
        }

        private void InitVisualTree()
        {
            // var tree = Resources.Load<VisualTreeAsset>("AnimatorParameterTextField");
            // tree.CloneTree(this);
            var styleSheet = Resources.Load<StyleSheet>("AnimatorParameterTextFieldStyles");
            if (!styleSheets.Contains(styleSheet))
            {
                styleSheets.Add(styleSheet);
            }
            AddToClassList("parameter-field-with-popup");

            _label = new Label();
            Add(_label);
            _popupField = new PopupField<string>(_parameterChoices, 0);
            Add(_popupField);
            _textField = new TextField();
            Add(_textField);
        }

        private void InitCallbacks()
        {
            RegisterCallback<MouseEnterEvent>(evt =>
            {
                if (!_refreshed)
                {
                    UpdateParameterChoices();
                    _refreshed = true;
                }
            });

            RegisterCallback<MouseLeaveEvent>(evt =>
            {
                _refreshed = false;
            });

            _textField.RegisterValueChangedCallback(evt =>
            {
                UpdatePopupSelectedIndex();
            });

            _popupField.RegisterValueChangedCallback(evt =>
            {
                value = _popupField.value;
            });
        }

        private void UpdatePopupSelectedIndex()
        {
            for (var i = 0; i < _parameterChoices.Count; i++)
            {
                if (_parameterChoices[i] == value)
                {
                    _popupField.index = i;
                    return;
                }
            }
            _popupField.index = 0;
        }

        private void UpdateParameterChoices()
        {
            _parameterChoices.Clear();
            _parameterChoices.Add("");

            if (avatarGameObject != null)
            {
                var scannedParams = AnimUtils.ScanAnimatorParameters(avatarGameObject);
                foreach (var key in scannedParams.Keys)
                {
                    _parameterChoices.Add(key);
                }
            }
        }

        public void SetValueWithoutNotify(string newValue)
        {
            _textField.value = newValue;
            UpdatePopupSelectedIndex();
        }
    }
}
