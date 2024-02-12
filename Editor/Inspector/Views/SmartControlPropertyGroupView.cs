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

        public DTSmartControl.PropertyGroup Target { get; set; }
        public int SelectionType { get => _selectionTypePopup.index; set => _selectionTypePopup.index = value; }
        public Transform SearchTransform { get => (Transform)_searchFromObjField.value; set => _searchFromObjField.value = value; }
        public Transform PickFromTransform { get => (Transform)_pickFromObjField.value; set => _pickFromObjField.value = value; }
        public List<GameObject> SelectionGameObjects { get; set; }
        public List<Component> FoundComponents { get; set; }


        private readonly SmartControlPropertyGroupPresenter _presenter;
        private readonly string _title;
        private readonly Action _onRemove;
        private PopupField<string> _selectionTypePopup;
        private ObjectField _searchFromObjField;
        private VisualElement _selectionObjsContainer;
        private ObjectField _pickFromObjField;
        private VisualElement _compsContainer;
        private Label _titleLabel;
        private Button _removeBtn;

        public SmartControlPropertyGroupView(DTSmartControl.PropertyGroup target, string title, Action onRemove)
        {
            Target = target;
            _title = title;
            _onRemove = onRemove;
            _presenter = new SmartControlPropertyGroupPresenter(this);
            SelectionGameObjects = new List<GameObject>();
            FoundComponents = new List<Component>();
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
            _titleLabel = Q<Label>("title-label").First();
            _titleLabel.text = _title;

            _removeBtn = Q<Button>("remove-btn").First();
            _removeBtn.clicked += _onRemove;
        }

        private void UpdateSelectionTypeUI()
        {
            // inverted
            _searchFromObjField.style.display = SelectionType == 1 ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void InitSelectionTypePopup()
        {
            var popupContainer = Q<VisualElement>("selection-type-popup-container").First();
            var choices = new List<string>() { "Normal", "Inverted" };
            _selectionTypePopup = new PopupField<string>("Selection Type", choices, 0);
            _selectionTypePopup.RegisterValueChangedCallback((evt) =>
            {
                UpdateSelectionTypeUI();
                SettingsChanged?.Invoke();
            });
            popupContainer.Add(_selectionTypePopup);
        }

        private void InitSearchFromObjField()
        {
            var searchFromObjFieldContainer = Q<VisualElement>("search-from-objfield-container").First();
            _searchFromObjField = new ObjectField("Search From")
            {
                objectType = typeof(Transform)
            };
            _searchFromObjField.RegisterValueChangedCallback((evt) =>
            {
                SettingsChanged?.Invoke();
            });
            searchFromObjFieldContainer.Add(_searchFromObjField);
        }

        private void InitSelectionObjsContainer()
        {
            _selectionObjsContainer = Q<VisualElement>("selection-objs-container").First();
        }

        private void InitObjAddField()
        {
            var selectionObjAddFieldContainer = Q<VisualElement>("selection-obj-add-field-container").First();
            selectionObjAddFieldContainer.AddToClassList("add-field-container");
            var label = new Label("+");
            selectionObjAddFieldContainer.Add(label);
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
            selectionObjAddFieldContainer.Add(objAddField);
        }

        private void InitComponentsContainer()
        {
            var pickFromObjFieldContainer = Q<VisualElement>("pick-from-objfield-container").First();
            _pickFromObjField = new ObjectField("Pick from")
            {
                objectType = typeof(Transform),
                value = SearchTransform
            };
            pickFromObjFieldContainer.Add(_pickFromObjField);
            _pickFromObjField.RegisterValueChangedCallback((evt) =>
            {
                SettingsChanged?.Invoke();
            });
            _compsContainer = Q<VisualElement>("components-container").First();
        }

        private void RepaintSelectionObjects()
        {
            _selectionObjsContainer.Clear();
            for (var i = 0; i < SelectionGameObjects.Count; i++)
            {
                var go = SelectionGameObjects[i];
                var elem = new VisualElement();
                elem.AddToClassList("object-field-entry");

                var objField = new ObjectField()
                {
                    objectType = typeof(GameObject),
                    value = go
                };
                objField.RegisterValueChangedCallback((evt) => ChangeGameObject?.Invoke(i, go));
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

            // disable existing views
            foreach (var elem in _compsContainer.Children())
            {
                if (elem is SmartControlPropertyGroupComponentView propGpCompView)
                {
                    propGpCompView.OnDisable();
                }
            }

            if (FoundComponents.Count > 10)
            {
                _compsContainer.Add(CreateHelpBox("Too many components found. Please narrow the results by picking from a location with less children.", MessageType.Warning));
            }
            else
            {
                foreach (var comp in FoundComponents)
                {
                    var view = new SmartControlPropertyGroupComponentView(comp, Target.PropertyValues);
                    view.OnEnable();
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

        public override void OnEnable()
        {
            InitVisualTree();
            InitTitleContainer();
            InitSelectionTypePopup();
            InitSearchFromObjField();
            InitSelectionObjsContainer();
            InitObjAddField();
            InitComponentsContainer();

            t.LocalizeElement(this);

            RaiseLoadEvent();
        }

        public override void OnDisable()
        {
            base.OnDisable();
        }
    }
}
