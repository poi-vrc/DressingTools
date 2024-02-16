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
    internal class SmartControlPropertyGroupComponentView : ElementView, ISmartControlPropertyGroupComponentView
    {
        private static readonly I18nTranslator t = I18n.ToolTranslator;

        public event Action PropertiesChanged { add => _parentView.PropertiesChanged += value; remove => _parentView.PropertiesChanged -= value; }
        public event Action ControlTypeChanged { add => _parentView.ControlTypeChanged += value; remove => _parentView.ControlTypeChanged -= value; }
        public event Action SearchResultModeChange;
        public event Action SearchQueryChange;
        public event Action<string, object> AddProperty;
        public event Action<string> RemoveProperty;
        public event Action<string, object> ChangeProperty;
        public event Action<string, float, float> ChangePropertyFromToValue;
        public int ControlType { get => _parentView.ControlType; set => _parentView.ControlType = value; }

        public Component TargetComponent { get; set; }
        public List<DTSmartControl.PropertyGroup.PropertyValue> TargetPropertyValues { get; set; }
        public List<KeyValuePair<string, object>> SearchResults { get; set; }
        public SmartControlComponentViewResultMode SearchResultMode { get; set; }
        public Dictionary<string, SmartControlComponentViewPropertyValue> Properties { get; set; }
        public string SearchQuery { get => _searchField.value; set => _searchField.value = value; }
        public bool DisplayAllResults { get; set; }
        public bool HasBlendshapes { get; set; }
        public bool HasMaterialProperties { get; set; }
        public bool HasGenericProperties { get; set; }

        private readonly ISmartControlPropertyGroupComponentViewParent _parentView;
        private readonly SmartControlPropertyGroupComponentPresenter _presenter;
        private ToolbarButton _blendshapeBtn;
        private ToolbarButton _materialBtn;
        private ToolbarButton _genericBtn;
        private TextField _searchField;
        private VisualElement _searchResultContainer;
        private VisualElement _propertiesContainer;

        public SmartControlPropertyGroupComponentView(ISmartControlPropertyGroupComponentViewParent parentView, Component targetComponent, List<DTSmartControl.PropertyGroup.PropertyValue> propertyValues)
        {
            _parentView = parentView;
            TargetComponent = targetComponent;
            TargetPropertyValues = propertyValues;
            SearchResults = new List<KeyValuePair<string, object>>();
            SearchResultMode = SmartControlComponentViewResultMode.None;
            Properties = new Dictionary<string, SmartControlComponentViewPropertyValue>();
            DisplayAllResults = false;
            HasBlendshapes = false;
            HasMaterialProperties = false;
            HasGenericProperties = false;
            _presenter = new SmartControlPropertyGroupComponentPresenter(this);
        }

        private void InitVisualTree()
        {
            var tree = Resources.Load<VisualTreeAsset>("SmartControlPropertyGroupComponentView");
            tree.CloneTree(this);
            var styleSheet = Resources.Load<StyleSheet>("SmartControlPropertyGroupComponentViewStyles");
            if (!styleSheets.Contains(styleSheet))
            {
                styleSheets.Add(styleSheet);
            }
        }

        private void InitComponentsObjFields()
        {
            var container = Q<VisualElement>("component-obj-fields-container").First();

            var objField = new ObjectField()
            {
                objectType = typeof(GameObject),
                value = TargetComponent.gameObject,
            };
            objField.SetEnabled(false);
            container.Add(objField);

            var compField = new ObjectField()
            {
                objectType = typeof(Component),
                value = TargetComponent,
            };
            compField.SetEnabled(false);
            container.Add(compField);
        }

        private void InitSearch()
        {
            _searchField = Q<TextField>("search-field").First();
            _searchField.RegisterValueChangedCallback((evt) => SearchQueryChange?.Invoke());
            _searchResultContainer = Q<VisualElement>("search-result-container").First();
            _propertiesContainer = Q<VisualElement>("properties-container").First();
        }

        private void SetResultModeOrNoMode(SmartControlComponentViewResultMode wantedMode)
        {
            // set the mode to no mode if it's already that mode
            if (SearchResultMode == wantedMode)
            {
                SearchResultMode = SmartControlComponentViewResultMode.None;
            }
            else
            {
                SearchResultMode = wantedMode;
            }
            UpdateActiveResultMode();
            SearchQuery = "";
            SearchResultModeChange?.Invoke();
            RepaintSearchResults();
        }

        private void UpdateActiveResultMode()
        {
            DisplayAllResults = false;
            _blendshapeBtn.RemoveFromClassList("active");
            _materialBtn.RemoveFromClassList("active");
            _genericBtn.RemoveFromClassList("active");
            _searchField.style.display = SearchResultMode == SmartControlComponentViewResultMode.None ? DisplayStyle.None : DisplayStyle.Flex;

            if (SearchResultMode == SmartControlComponentViewResultMode.Blendshapes)
            {
                _blendshapeBtn.AddToClassList("active");
            }
            else if (SearchResultMode == SmartControlComponentViewResultMode.Material)
            {
                _materialBtn.AddToClassList("active");
            }
            else if (SearchResultMode == SmartControlComponentViewResultMode.Generic)
            {
                _genericBtn.AddToClassList("active");
            }
        }

        private void InitPropertyTypeBtns()
        {
            _blendshapeBtn = Q<ToolbarButton>("blendshapes-btn").First();
            _materialBtn = Q<ToolbarButton>("material-btn").First();
            _genericBtn = Q<ToolbarButton>("generic-btn").First();

            _blendshapeBtn.clicked += () => SetResultModeOrNoMode(SmartControlComponentViewResultMode.Blendshapes);
            _materialBtn.clicked += () => SetResultModeOrNoMode(SmartControlComponentViewResultMode.Material);
            _genericBtn.clicked += () => SetResultModeOrNoMode(SmartControlComponentViewResultMode.Generic);
        }

        public override void OnEnable()
        {
            InitVisualTree();
            InitComponentsObjFields();
            InitPropertyTypeBtns();
            InitSearch();

            t.LocalizeElement(this);

            RaiseLoadEvent();
        }

        public override void OnDisable()
        {
            base.OnDisable();
        }

        private VisualElement MakeAddEntry(string name, object value, Action onAdd)
        {
            var elem = new VisualElement();

            elem.AddToClassList("property-entry");

            var propertyLabel = new Label(name);
            propertyLabel.AddToClassList("name");
            elem.Add(propertyLabel);

            var supported = true;
            VisualElement field;
            if (value is float f)
            {
                field = new FloatField() { value = f };
            }
            else if (value is bool b)
            {
                field = new Toggle() { value = b };
            }
            else if (value is Color c)
            {
                field = new ColorField() { value = c };
            }
            else if (value is Object o)
            {
                field = new ObjectField() { value = o };
            }
            else
            {
                supported = false;
                var rightLabel = new Label(t._("inspector.smartcontrol.propertyGroup.component.textField.search"));
                rightLabel.AddToClassList("right");
                field = rightLabel;
            }
            elem.Add(field);
            field.SetEnabled(false);

            if (supported)
            {
                elem.Add(new Button(onAdd) { text = t._("inspector.smartcontrol.propertyGroup.component.btn.add") });
            }

            return elem;
        }

        private void RepaintSearchResults()
        {
            _blendshapeBtn.style.display = HasBlendshapes ? DisplayStyle.Flex : DisplayStyle.None;
            _materialBtn.style.display = HasMaterialProperties ? DisplayStyle.Flex : DisplayStyle.None;
            _genericBtn.style.display = HasGenericProperties ? DisplayStyle.Flex : DisplayStyle.None;
            // TODO empty component

            _searchResultContainer.Clear();

            if (SearchResults.Count > 20 && !DisplayAllResults)
            {
                // default not to display all to prevent UI hang
                _searchResultContainer.Add(CreateHelpBox(t._("inspector.smartcontrol.propertyGroup.component.helpbox.tooManySearchResults", SearchResults.Count), UnityEditor.MessageType.Warning));
                _searchResultContainer.Add(new Button(() =>
                {
                    DisplayAllResults = true;
                    RepaintSearchResults();
                })
                {
                    text = t._("inspector.smartcontrol.propertyGroup.component.btn.displayAllSearchResults")
                });
            }
            else
            {
                foreach (var kvp in SearchResults)
                {
                    _searchResultContainer.Add(MakeAddEntry(kvp.Key, kvp.Value, () => AddProperty?.Invoke(kvp.Key, kvp.Value)));
                }
            }

            UpdateActiveResultMode();
        }

        private VisualElement MakePropertyEntry(string name, object value, Action<object> onChange, Action onRemove)
        {
            var elem = new VisualElement();

            elem.AddToClassList("property-entry");

            var propertyLabel = new Label(name);
            propertyLabel.AddToClassList("name");
            elem.Add(propertyLabel);

            if (value is float f)
            {
                var field = new FloatField()
                {
                    value = f,
                    isDelayed = true
                };
                field.RegisterValueChangedCallback((evt) => onChange?.Invoke(evt.newValue));
                elem.Add(field);
            }
            else if (value is bool b)
            {
                var toggle = new Toggle() { value = b };
                toggle.RegisterValueChangedCallback((evt) => onChange?.Invoke(evt.newValue));
                elem.Add(toggle);
            }
            else if (value is Color c)
            {
                var field = new ColorField() { value = c };
                field.RegisterValueChangedCallback((evt) => onChange?.Invoke(evt.newValue));
                elem.Add(field);
            }
            else if (value is Object o)
            {
                var field = new ObjectField() { value = o };
                field.RegisterValueChangedCallback((evt) => onChange?.Invoke(evt.newValue));
                elem.Add(field);
            }

            elem.Add(new Button(onRemove) { text = "x" });

            return elem;
        }

        private VisualElement MakeIncompatiblePropertyEntry(string name, Action onRemove)
        {
            var elem = new VisualElement();

            elem.AddToClassList("property-entry");

            var propertyLabel = new Label(name);
            propertyLabel.AddToClassList("name");
            elem.Add(propertyLabel);

            elem.Add(new Label(t._("inspector.smartcontrol.propertyGroup.component.label.incompatible")));

            elem.Add(new Button(onRemove) { text = "x" });

            return elem;
        }

        private VisualElement MakeFromToFloatValueEntry(string name, float fromValue, float toValue, Action<float, float> onChange, Action onRemove)
        {
            var elem = new VisualElement();

            elem.AddToClassList("property-entry");

            var propertyLabel = new Label(name);
            propertyLabel.AddToClassList("name");
            elem.Add(propertyLabel);

            elem.Add(new Label(t._("inspector.smartcontrol.propertyGroup.component.label.from")));

            var fromField = new FloatField()
            {
                value = fromValue,
                isDelayed = true
            };
            elem.Add(fromField);

            elem.Add(new Label(t._("inspector.smartcontrol.propertyGroup.component.label.to")));

            var toField = new FloatField
            {
                value = toValue,
                isDelayed = true
            };
            elem.Add(toField);

            fromField.RegisterValueChangedCallback(evt => onChange?.Invoke(fromField.value, toField.value));
            toField.RegisterValueChangedCallback(evt => onChange?.Invoke(fromField.value, toField.value));

            elem.Add(new Button(onRemove) { text = "x" });

            return elem;
        }

        private void RepaintProperties()
        {
            _propertiesContainer.Clear();
            var copy = new List<KeyValuePair<string, SmartControlComponentViewPropertyValue>>(Properties);
            foreach (var kvp in copy)
            {
                var propVal = kvp.Value;
                if (ControlType == 1) // Motion time
                {
                    if (propVal.type != typeof(float))
                    {
                        _propertiesContainer.Add(MakeIncompatiblePropertyEntry(kvp.Key, () => RemoveProperty?.Invoke(kvp.Key)));
                        continue;
                    }
                    _propertiesContainer.Add(MakeFromToFloatValueEntry(kvp.Key, propVal.fromValue, propVal.toValue, (fromVal, toVal) =>
                    {
                        ChangePropertyFromToValue?.Invoke(kvp.Key, fromVal, toVal);
                    }, () =>
                    {
                        RemoveProperty?.Invoke(kvp.Key);
                    }));
                }
                else
                {
                    _propertiesContainer.Add(MakePropertyEntry(kvp.Key, propVal.value, (newVal) => ChangeProperty?.Invoke(kvp.Key, newVal), () => RemoveProperty?.Invoke(kvp.Key)));
                }
            }
        }

        public override void Repaint()
        {
            RepaintSearchResults();
            RepaintProperties();
        }

        public void RaisePropertiesChangedEvent()
        {
            _parentView.RaisePropertiesChangedEvent();
        }
    }
}
