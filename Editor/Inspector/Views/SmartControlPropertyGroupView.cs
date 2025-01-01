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
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Chocopoi.DressingTools.Inspector.Views
{
    [ExcludeFromCodeCoverage]
    internal class SmartControlPropertyGroupView : ElementView, ISmartControlPropertyGroupView
    {
        private static readonly I18nTranslator t = I18n.ToolTranslator;

        public event Action SettingsChanged;
        public event Action<GameObject> AddGameObject;
        public event Action<int, GameObject> ChangeGameObject;
        public event Action<GameObject> RemoveGameObject;
        public event Action PropertiesChanged;
        public event Action ControlTypeChanged { add => _parentView.ControlTypeChanged += value; remove => _parentView.ControlTypeChanged -= value; }

        public DTSmartControl.PropertyGroup Target { get; set; }
        public int SelectionType { get => _selectionTypePopup.index; set => _selectionTypePopup.index = value; }
        public Transform SearchTransform { get => (Transform)_searchFromObjField.value; set => _searchFromObjField.value = value; }
        public Transform PickFromTransform { get => (Transform)_pickFromObjField.value; set => _pickFromObjField.value = value; }
        public List<GameObject> SelectionGameObjects { get; set; }
        public List<Component> FoundComponents { get; set; }
        public int ControlType { get => _parentView.ControlType; set => _parentView.ControlType = value; }

        private readonly ISmartControlPropertyGroupViewParent _parentView;
        private readonly SmartControlPropertyGroupPresenter _presenter;
        private readonly string _title;
        private readonly Action _onRemove;
        private PopupField<string> _selectionTypePopup;
        private ObjectField _searchFromObjField;
        private Label _includeExcludeLabel;
        private VisualElement _selectionObjsContainer;
        private ObjectField _pickFromObjField;
        private VisualElement _compsContainer;
        private Label _titleLabel;
        private Button _removeBtn;
        private VisualElement _selectionObjAddFieldContainer;

        public SmartControlPropertyGroupView(ISmartControlPropertyGroupViewParent parentView, DTSmartControl.PropertyGroup target, string title, Action onRemove)
        {
            _parentView = parentView;
            Target = target;
            _title = title;
            _onRemove = onRemove;
            _presenter = new SmartControlPropertyGroupPresenter(this);
            SelectionGameObjects = new List<GameObject>();
            FoundComponents = new List<Component>();
            InitVisualTree();
            InitTitleContainer();
            InitSelectionTypePopup();
            InitSearchFromObjField();
            InitSelectionObjs();
            InitObjAddField();
            InitComponentsContainer();
            t.LocalizeElement(this);
        }

        private void InitVisualTree()
        {
            var tree = Resources.Load<VisualTreeAsset>("SmartControlPropertyGroupView");
            tree.CloneTree(this);
            var styleSheet = Resources.Load<StyleSheet>("SmartControlPropertyGroupViewStyles");
            if (!styleSheets.Contains(styleSheet))
            {
                styleSheets.Add(styleSheet);
            }
        }

        private void InitTitleContainer()
        {
            _titleLabel = Q<Label>("title-label");
            _titleLabel.text = _title;

            _removeBtn = Q<Button>("remove-btn");
            _removeBtn.clicked += _onRemove;
        }

        private void UpdateSelectionTypeUI()
        {
            _searchFromObjField.style.display = SelectionType == 1 ? DisplayStyle.Flex : DisplayStyle.None;
            _includeExcludeLabel.text = SelectionType == 1 || SelectionType == 2 ? t._("inspector.smartcontrol.propertyGroup.label.excludeTheseObjects") : t._("inspector.smartcontrol.propertyGroup.label.includeTheseObjects");
        }

        private void InitSelectionTypePopup()
        {
            var popupContainer = Q<VisualElement>("selection-type-popup-container");
            var choices = new List<string>() { t._("inspector.smartcontrol.propertyGroup.selectionType.normal"), t._("inspector.smartcontrol.propertyGroup.selectionType.inverted"), t._("inspector.smartcontrol.propertyGroup.selectionType.avatarWide") };
            _selectionTypePopup = new PopupField<string>(t._("inspector.smartcontrol.propertyGroup.popup.selectionType"), choices, 0);
            _selectionTypePopup.RegisterValueChangedCallback((evt) =>
            {
                UpdateSelectionTypeUI();
                SettingsChanged?.Invoke();
            });
            popupContainer.Add(_selectionTypePopup);
        }

        private void InitSearchFromObjField()
        {
            var searchFromObjFieldContainer = Q<VisualElement>("search-from-objfield-container");
            _searchFromObjField = new ObjectField(t._("inspector.smartcontrol.propertyGroup.objectField.searchObjectsFrom"))
            {
                objectType = typeof(Transform)
            };
            _searchFromObjField.RegisterValueChangedCallback((evt) =>
            {
                SettingsChanged?.Invoke();
            });
            searchFromObjFieldContainer.Add(_searchFromObjField);
        }

        private void InitSelectionObjs()
        {
            _includeExcludeLabel = Q<Label>("include-exclude-objs-label");
            _selectionObjsContainer = Q<VisualElement>("selection-objs-container");
        }

        private void InitObjAddField()
        {
            _selectionObjAddFieldContainer = Q<VisualElement>("selection-obj-add-field-container");
            _selectionObjAddFieldContainer.AddToClassList("add-field-container");
            var label = new Label("+");
            _selectionObjAddFieldContainer.Add(label);
            var objAddField = new ObjectField()
            {
                objectType = typeof(GameObject)
            };
            objAddField.RegisterValueChangedCallback((evt) =>
            {
                if (objAddField.value != null)
                {
                    AddGameObject?.Invoke((GameObject)objAddField.value);
                    objAddField.value = null;
                }
            });
            _selectionObjAddFieldContainer.Add(objAddField);
        }

        private void InitComponentsContainer()
        {
            var pickFromObjFieldContainer = Q<VisualElement>("pick-from-objfield-container");
            _pickFromObjField = new ObjectField(t._("inspector.smartcontrol.propertyGroup.objectField.pickPropertiesFrom"))
            {
                objectType = typeof(Transform),
                value = SearchTransform
            };
            pickFromObjFieldContainer.Add(_pickFromObjField);
            _pickFromObjField.RegisterValueChangedCallback((evt) =>
            {
                SettingsChanged?.Invoke();
            });
            _compsContainer = Q<VisualElement>("components-container");
        }

        private void RepaintSelectionObjects()
        {
            _selectionObjsContainer.Clear();
            for (var i = 0; i < SelectionGameObjects.Count; i++)
            {
                var myIdx = i;
                var go = SelectionGameObjects[i];
                var elem = new VisualElement();
                elem.AddToClassList("object-field-entry");

                var objField = new ObjectField()
                {
                    objectType = typeof(GameObject),
                    value = go
                };
                objField.RegisterValueChangedCallback((evt) => ChangeGameObject?.Invoke(myIdx, (GameObject)objField.value));
                elem.Add(objField);

                var removeBtn = new Button()
                {
                    text = "x"
                };
                removeBtn.clicked += () => RemoveGameObject?.Invoke(go);
                elem.Add(removeBtn);

                _selectionObjsContainer.Add(elem);
            }
        }

        private void RepaintComponentsContainer()
        {
            _compsContainer.Clear();

            if (FoundComponents.Count > 10)
            {
                _compsContainer.Add(CreateHelpBox(t._("inspector.smartcontrol.propertyGroup.helpbox.tooManyComponentsFound", FoundComponents.Count), MessageType.Warning));
            }
            else
            {
                foreach (var comp in FoundComponents)
                {
                    var view = new SmartControlPropertyGroupComponentView(this, comp, Target.PropertyValues);
                    _compsContainer.Add(view);
                }
            }

        }

        public override void Repaint()
        {
            UpdateSelectionTypeUI();
            RepaintSelectionObjects();
            RepaintComponentsContainer();
        }

        public void RaisePropertiesChangedEvent()
        {
            PropertiesChanged?.Invoke();
        }
    }
}
