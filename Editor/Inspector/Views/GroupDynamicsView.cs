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
using Chocopoi.DressingTools.Components.Modifiers;
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
    internal class GroupDynamicsView : ElementView, IGroupDynamicsView
    {
        private static readonly I18nTranslator t = I18n.ToolTranslator;

        public event Action ConfigChange;
        public event Action<Transform> AddInclude;
        public event Action<Transform> AddExclude;
        public event Action<int, Transform> ChangeInclude;
        public event Action<int, Transform> ChangeExclude;
        public event Action<int> RemoveInclude;
        public event Action<int> RemoveExclude;

        public DTGroupDynamics Target { get; set; }
        public int SearchMode { get; set; }
        public List<Transform> IncludeTransforms { get; set; }
        public List<Transform> ExcludeTransforms { get; set; }

        private readonly GroupDynamicsPresenter _presenter;
        private PopupField<string> _searchModePopup;
        private VisualElement _includesListContainer;
        private VisualElement _excludesListContainer;

        public GroupDynamicsView()
        {
            _presenter = new GroupDynamicsPresenter(this);
            SearchMode = 0;
            IncludeTransforms = new List<Transform>();
            ExcludeTransforms = new List<Transform>();

            InitVisualTree();
            InitSearchModePopup();
            InitIncludes();
            InitExcludes();

            t.LocalizeElement(this);
        }

        private void InitVisualTree()
        {
            var tree = Resources.Load<VisualTreeAsset>("GroupDynamicsView");
            tree.CloneTree(this);
            var styleSheet = Resources.Load<StyleSheet>("GroupDynamicsViewStyles");
            if (!styleSheets.Contains(styleSheet))
            {
                styleSheets.Add(styleSheet);
            }
        }

        // TODO: code reuse
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

        private void InitSearchModePopup()
        {
            var popupContainer = Q<VisualElement>("search-mode-popup-container");
            var choices = new List<string>() { t._("inspector.groupDynamics.searchMode.controlRoot"), t._("inspector.groupDynamics.searchMode.componentRoot") };
            _searchModePopup = new PopupField<string>(t._("inspector.groupDynamics.popup.searchMode"), choices, 0);
            _searchModePopup.RegisterValueChangedCallback(evt =>
            {
                SearchMode = _searchModePopup.index;
                ConfigChange?.Invoke();
            });
            popupContainer.Add(_searchModePopup);
        }

        private void InitIncludes()
        {
            _includesListContainer = Q<VisualElement>("includes-list-container");
            var addFieldContainer = Q<VisualElement>("includes-add-field-container");
            MakeAddField<Transform>(addFieldContainer, (t) => AddInclude?.Invoke(t));
        }

        private void InitExcludes()
        {
            _excludesListContainer = Q<VisualElement>("excludes-list-container");
            var addFieldContainer = Q<VisualElement>("excludes-add-field-container");
            MakeAddField<Transform>(addFieldContainer, (t) => AddExclude?.Invoke(t));
        }

        private void RepaintSearchModePopup()
        {
            _searchModePopup.index = SearchMode;
        }

        private void RepaintListContainer<T>(VisualElement listContainer, List<T> transforms, Action<int, T> onChange, Action<int> onRemove) where T : Object
        {
            listContainer.Clear();
            var copy = new List<T>(transforms);
            for (var i = 0; i < copy.Count; i++)
            {
                var myIdx = i;

                var element = new VisualElement();
                element.AddToClassList("object-field-entry");

                // TODO: check duplicate entries
                var objField = new ObjectField()
                {
                    objectType = typeof(T),
                    value = copy[i]
                };
                objField.RegisterValueChangedCallback((evt) =>
                {
                    onChange?.Invoke(myIdx, (T)objField.value);
                });
                element.Add(objField);

                var removeBtn = new Button()
                {
                    text = "x"
                };
                removeBtn.clicked += () => onRemove?.Invoke(myIdx);
                element.Add(removeBtn);

                listContainer.Add(element);
            }
        }

        private void RepaintIncludesExcludes()
        {
            RepaintListContainer(_includesListContainer, IncludeTransforms, ChangeInclude, RemoveInclude);
            RepaintListContainer(_excludesListContainer, ExcludeTransforms, ChangeExclude, RemoveExclude);
        }

        public override void Repaint()
        {
            RepaintSearchModePopup();
            RepaintIncludesExcludes();
        }
    }
}
