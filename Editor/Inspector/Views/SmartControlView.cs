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
using Chocopoi.DressingTools.UI.Presenters;
using Chocopoi.DressingTools.UI.Views;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Chocopoi.DressingTools.Inspector.Views
{
    [ExcludeFromCodeCoverage]
    internal class SmartControlView : ElementView, ISmartControlView
    {
        private static readonly I18nTranslator t = I18n.ToolTranslator;

        public event Action DriverChanged;
        public event Action AnimatorParameterDefaultValueChanged;
        public event Action ControlTypeChanged;
        public event Action<Component> AddObjectToggle;
        public event Action<DTSmartControl> AddCrossControlValueOnEnabled;
        public event Action<DTSmartControl> AddCrossControlValueOnDisabled;
        public event Action<SmartControlObjectToggleValue> ChangeObjectToggle;
        public event Action<SmartControlObjectToggleValue> RemoveObjectToggle;
        public event Action<SmartControlCrossControlValue> ChangeCrossControlValueOnEnabled;
        public event Action<SmartControlCrossControlValue> ChangeCrossControlValueOnDisabled;
        public event Action<SmartControlCrossControlValue> RemoveCrossControlValueOnEnabled;
        public event Action<SmartControlCrossControlValue> RemoveCrossControlValueOnDisabled;
        public event Action AddNewPropertyGroup;
        public event Action<DTSmartControl.PropertyGroup> RemovePropertyGroup;

        public DTSmartControl Target { get; set; }
        public int DriverType { get => _driverTypePopup.index; set => _driverTypePopup.index = value; }
        public float AnimatorParameterDefaultValue
        {
            get => _driverAnimParamDefaultValueFloatField.value;
            set
            {
                // a better way to do this?
                _driverAnimParamDefaultValueBoolToggle.value = value == 1.0f;
                _driverAnimParamDefaultValueFloatField.value = value;
                _driverAnimParamDefaultValueFloatSlider.value = value;
            }
        }

        public int ControlType { get => _controlTypePopupField.index; set => _controlTypePopupField.index = value; }

        public List<SmartControlObjectToggleValue> ObjectToggles { get; set; }
        public List<SmartControlCrossControlValue> CrossControlValuesOnEnabled { get; set; }
        public List<SmartControlCrossControlValue> CrossControlValuesOnDisabled { get; set; }
        public List<DTSmartControl.PropertyGroup> PropertyGroups { get; set; }

        private readonly SmartControlPresenter _presenter;
        private Foldout _driverFoldout;
        private VisualElement _driverContainer;
        private VisualElement _driverTypePopupContainer;
        private PopupField<string> _driverTypePopup;
        private Toggle _driverAnimParamDefaultValueBoolToggle;
        private VisualElement _driverAnimParamDefaultValueFloatContainer;
        private Slider _driverAnimParamDefaultValueFloatSlider;
        private FloatField _driverAnimParamDefaultValueFloatField;
        private Foldout _controlFoldout;
        private VisualElement _controlContainer;
        private VisualElement _controlTypePopupContainer;
        private PopupField<string> _controlTypePopupField;
        private VisualElement _controlBinaryContainer;
        private VisualElement _controlMotionTimeContainer;
        private Foldout _objectTogglesFoldout;
        private VisualElement _objectTogglesContainer;
        private VisualElement _crossCtrlValuesOnEnabledContainer;
        private VisualElement _crossCtrlValuesOnEnabledAddFieldContainer;
        private VisualElement _objectTogglesListContainer;
        private VisualElement _objectTogglesAddFieldContainer;
        private Foldout _propGpsFoldout;
        private VisualElement _propGpsContainer;
        private VisualElement _propGpsListContainer;
        private Button _propGpAddBtn;
        private Foldout _crossCtrlFoldout;
        private VisualElement _crossCtrlContainer;
        private Foldout _crossCtrlValuesFoldout;
        private VisualElement _crossCtrlValuesContainer;
        private VisualElement _crossCtrlValuesOnDisabledContainer;
        private VisualElement _crossCtrlValuesOnDisabledAddFieldContainer;

        public SmartControlView()
        {
            ObjectToggles = new List<SmartControlObjectToggleValue>();
            CrossControlValuesOnEnabled = new List<SmartControlCrossControlValue>();
            CrossControlValuesOnDisabled = new List<SmartControlCrossControlValue>();
            PropertyGroups = new List<DTSmartControl.PropertyGroup>();

            _presenter = new SmartControlPresenter(this);
        }

        private void InitVisualTree()
        {
            var tree = Resources.Load<VisualTreeAsset>("SmartControlView");
            tree.CloneTree(this);
            var styleSheet = Resources.Load<StyleSheet>("SmartControlViewStyles");
            if (!styleSheets.Contains(styleSheet))
            {
                styleSheets.Add(styleSheet);
            }
        }

        private void InitDriverFoldout()
        {
            _driverFoldout = Q<Foldout>("driver-foldout").First();
            _driverContainer = Q<VisualElement>("driver-container").First();
            BindFoldoutHeaderAndContainerWithPrefix("driver");

            _driverTypePopupContainer = Q<VisualElement>("driver-type-popup-container").First();
            var choices = new List<string>() { "Animator Parameter", "Menu Item" };
            _driverTypePopup = new PopupField<string>("Driver", choices, 0);
            _driverTypePopup.RegisterValueChangedCallback((evt) =>
            {
                DriverChanged?.Invoke();
            });
            _driverTypePopupContainer.Add(_driverTypePopup);

            _driverAnimParamDefaultValueBoolToggle = Q<Toggle>("anim-param-defval-bool-toggle").First();
            _driverAnimParamDefaultValueFloatContainer = Q<VisualElement>("anim-param-defval-float-container").First();
            _driverAnimParamDefaultValueFloatSlider = Q<Slider>("anim-param-defval-float-slider").First();
            _driverAnimParamDefaultValueFloatField = Q<FloatField>("anim-param-defval-float-field").First();

            _driverAnimParamDefaultValueBoolToggle.RegisterValueChangedCallback((evt) =>
            {
                _driverAnimParamDefaultValueFloatField.value = evt.newValue ? 1.0f : 0.0f;
            });
            _driverAnimParamDefaultValueFloatSlider.RegisterValueChangedCallback((evt) =>
            {
                // we don't need all decimals
                _driverAnimParamDefaultValueFloatField.value = Mathf.Round(evt.newValue * 100f) / 100f;
            });
            _driverAnimParamDefaultValueFloatField.RegisterValueChangedCallback((evt) =>
            {
                _driverAnimParamDefaultValueFloatSlider.value = evt.newValue;
                AnimatorParameterDefaultValueChanged?.Invoke();
            });

            _driverAnimParamDefaultValueBoolToggle.style.display = DisplayStyle.None;
            _driverAnimParamDefaultValueFloatContainer.style.display = DisplayStyle.None;
        }

        private void InitControlFoldout()
        {
            _controlFoldout = Q<Foldout>("control-foldout").First();
            _controlContainer = Q<VisualElement>("control-container").First();
            BindFoldoutHeaderAndContainerWithPrefix("control");

            _controlBinaryContainer = Q<VisualElement>("control-binary-container").First();
            _controlMotionTimeContainer = Q<VisualElement>("control-motion-time-container").First();

            _controlTypePopupContainer = Q<VisualElement>("control-type-popup-container").First();
            var choices = new List<string>() { "Binary", "Motion Time" };
            _controlTypePopupField = new PopupField<string>("Type", choices, 0);
            _controlTypePopupContainer.Add(_controlTypePopupField);

            _controlTypePopupField.RegisterValueChangedCallback((evt) =>
            {
                RepaintControlType();
                ControlTypeChanged?.Invoke();
            });
        }

        private void MakeAddField<T>(VisualElement container, Action<T> newCompAction) where T : Object
        {
            var label = new Label("+");
            var objField = new ObjectField
            {
                objectType = typeof(T)
            };
            container.Add(label);
            container.Add(objField);
            objField.RegisterValueChangedCallback((evt) =>
            {
                if (objField.value != null)
                {
                    newCompAction?.Invoke((T)objField.value);
                    objField.value = null;
                }
            });
        }

        private void InitObjectTogglesFoldout()
        {
            _objectTogglesFoldout = Q<Foldout>("object-toggles-foldout").First();
            _objectTogglesContainer = Q<VisualElement>("object-toggles-container").First();
            BindFoldoutHeaderAndContainerWithPrefix("object-toggles");

            _objectTogglesListContainer = Q<VisualElement>("object-toggles-list-container").First();
            _objectTogglesAddFieldContainer = Q<VisualElement>("object-toggles-add-field-container").First();
            MakeAddField<Component>(_objectTogglesAddFieldContainer, (comp) => AddObjectToggle?.Invoke(comp));
        }

        private void InitPropertyGroupsFoldout()
        {
            _propGpsFoldout = Q<Foldout>("prop-gps-foldout").First();
            _propGpsContainer = Q<VisualElement>("prop-gps-container").First();
            BindFoldoutHeaderAndContainerWithPrefix("prop-gps");

            _propGpsListContainer = Q<VisualElement>("prop-gps-list-container").First();
            _propGpAddBtn = Q<Button>("prop-gp-add-btn").First();
            _propGpAddBtn.clicked += AddNewPropertyGroup;
        }

        private void InitCrossCtrlValuesFoldout()
        {
            _crossCtrlValuesFoldout = Q<Foldout>("cross-ctrl-values-foldout").First();
            _crossCtrlValuesContainer = Q<VisualElement>("cross-ctrl-values-container").First();
            BindFoldoutHeaderAndContainerWithPrefix("cross-ctrl-values");

            _crossCtrlValuesOnEnabledContainer = Q<VisualElement>("cross-ctrl-values-on-enabled-container").First();
            _crossCtrlValuesOnEnabledAddFieldContainer = Q<VisualElement>("cross-ctrl-values-on-enabled-add-field-container").First();
            MakeAddField<DTSmartControl>(_crossCtrlValuesOnEnabledAddFieldContainer, (ctrl) => AddCrossControlValueOnEnabled?.Invoke(ctrl));

            _crossCtrlValuesOnDisabledContainer = Q<VisualElement>("cross-ctrl-values-on-disabled-container").First();
            _crossCtrlValuesOnDisabledAddFieldContainer = Q<VisualElement>("cross-ctrl-values-on-disabled-add-field-container").First();
            MakeAddField<DTSmartControl>(_crossCtrlValuesOnDisabledAddFieldContainer, (ctrl) => AddCrossControlValueOnDisabled?.Invoke(ctrl));
        }

        private void InitCrossCtrlFoldout()
        {
            _crossCtrlFoldout = Q<Foldout>("cross-ctrl-foldout").First();
            _crossCtrlContainer = Q<VisualElement>("cross-ctrl-container").First();
            BindFoldoutHeaderAndContainerWithPrefix("cross-ctrl");

            InitCrossCtrlValuesFoldout();
        }

        public override void OnEnable()
        {
            InitVisualTree();
            InitDriverFoldout();
            InitControlFoldout();
            InitObjectTogglesFoldout();
            InitPropertyGroupsFoldout();
            InitCrossCtrlFoldout();

            t.LocalizeElement(this);

            RaiseLoadEvent();
        }

        public override void OnDisable()
        {
            base.OnDisable();
        }

        private void RepaintControlType()
        {
            if (ControlType == 0)
            {
                _driverAnimParamDefaultValueBoolToggle.style.display = DisplayStyle.Flex;
                _driverAnimParamDefaultValueFloatContainer.style.display = DisplayStyle.None;

                _controlBinaryContainer.style.display = DisplayStyle.Flex;
                _controlMotionTimeContainer.style.display = DisplayStyle.None;
                _controlFoldout.text = "Control: Binary";

                _objectTogglesFoldout.style.display = DisplayStyle.Flex;
                _objectTogglesContainer.style.display = _objectTogglesFoldout.value ? DisplayStyle.Flex : DisplayStyle.None;

                _propGpsFoldout.style.display = DisplayStyle.Flex;
                _propGpsContainer.style.display = _propGpsFoldout.value ? DisplayStyle.Flex : DisplayStyle.None;
            }
            else if (ControlType == 1)
            {
                _driverAnimParamDefaultValueBoolToggle.style.display = DisplayStyle.None;
                _driverAnimParamDefaultValueFloatContainer.style.display = DisplayStyle.Flex;

                _controlBinaryContainer.style.display = DisplayStyle.None;
                _controlMotionTimeContainer.style.display = DisplayStyle.Flex;
                _controlFoldout.text = "Control: Motion Time";

                _objectTogglesFoldout.style.display = DisplayStyle.None;
                _objectTogglesContainer.style.display = DisplayStyle.None;

                _propGpsFoldout.style.display = DisplayStyle.Flex;
                _propGpsContainer.style.display = _propGpsFoldout.value ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }

        private void RepaintObjectToggles()
        {
            _objectTogglesFoldout.text = $"Object Toggles ({ObjectToggles.Count})";

            _objectTogglesListContainer.Clear();
            var copy = new List<SmartControlObjectToggleValue>(ObjectToggles);
            foreach (var objToggle in copy)
            {
                var element = new VisualElement();
                element.AddToClassList("object-field-entry");

                // TODO: check duplicate entries
                var objField = new ObjectField()
                {
                    objectType = typeof(Component),
                    value = objToggle.target
                };
                objField.RegisterValueChangedCallback((evt) =>
                {
                    objToggle.target = (Component)objField.value;
                    ChangeObjectToggle?.Invoke(objToggle);
                });
                element.Add(objField);

                var toggle = new Toggle()
                {
                    value = objToggle.enabled
                };
                toggle.RegisterValueChangedCallback((evt) =>
                {
                    objToggle.enabled = toggle.value;
                    ChangeObjectToggle?.Invoke(objToggle);
                });
                element.Add(toggle);

                var removeBtn = new Button()
                {
                    text = "x"
                };
                removeBtn.clicked += () => RemoveObjectToggle?.Invoke(objToggle);
                element.Add(removeBtn);

                _objectTogglesListContainer.Add(element);
            }
        }

        private int RepaintCrossControlValuesSection(VisualElement valuesContainer, List<SmartControlCrossControlValue> values, Action<SmartControlCrossControlValue> onChange, Action<SmartControlCrossControlValue> onRemove)
        {
            valuesContainer.Clear();
            var copy = new List<SmartControlCrossControlValue>(values);
            foreach (var value in copy)
            {
                var element = new VisualElement();
                element.AddToClassList("object-field-entry");

                // TODO: check duplicate entries
                var objField = new ObjectField()
                {
                    objectType = typeof(DTSmartControl),
                    value = value.control
                };
                objField.RegisterValueChangedCallback((evt) =>
                {
                    value.control = (DTSmartControl)objField.value;
                    onChange?.Invoke(value);
                });
                element.Add(objField);

                var floatField = new FloatField()
                {
                    value = value.value
                };
                floatField.RegisterValueChangedCallback((evt) =>
                {
                    value.value = floatField.value;
                    onChange?.Invoke(value);
                });
                element.Add(floatField);

                var removeBtn = new Button()
                {
                    text = "x"
                };
                removeBtn.clicked += () => onRemove?.Invoke(value);
                element.Add(removeBtn);

                valuesContainer.Add(element);
            }
            return values.Count;
        }

        private int RepaintCrossControlValues()
        {
            var total = 0;

            total += RepaintCrossControlValuesSection(_crossCtrlValuesOnEnabledContainer, CrossControlValuesOnEnabled, ChangeCrossControlValueOnEnabled, RemoveCrossControlValueOnEnabled);

            total += RepaintCrossControlValuesSection(_crossCtrlValuesOnDisabledContainer, CrossControlValuesOnDisabled, ChangeCrossControlValueOnDisabled, RemoveCrossControlValueOnDisabled);

            _crossCtrlValuesFoldout.text = $"Values ({total})";
            return total;
        }

        private void RepaintCrossControls()
        {
            var total = 0;

            total += RepaintCrossControlValues();

            _crossCtrlFoldout.text = $"Cross-control Actions ({total})";
        }

        private void RepaintPropertyGroups()
        {
            _propGpsFoldout.text = $"Property Groups ({PropertyGroups.Count})";

            // disable existing views
            foreach (var elem in _propGpsListContainer.Children())
            {
                if (elem is SmartControlPropertyGroupView propGpView)
                {
                    propGpView.OnDisable();
                }
            }

            // TODO: selectively repaint
            _propGpsListContainer.Clear();
            var copy = new List<DTSmartControl.PropertyGroup>(PropertyGroups);
            for (var i = 0; i < copy.Count; i++)
            {
                var propGp = copy[i];

                var view = new SmartControlPropertyGroupView(propGp, $"Group {i + 1}", () =>
                {
                    RemovePropertyGroup?.Invoke(propGp);
                });
                view.OnEnable();

                _propGpsListContainer.Add(view);
            }
        }

        public override void Repaint()
        {
            _driverFoldout.text = $"Driver: {_driverTypePopup.text}";
            RepaintControlType();
            RepaintObjectToggles();
            RepaintPropertyGroups();
            RepaintCrossControls();
        }
    }
}
