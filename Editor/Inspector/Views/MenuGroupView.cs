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
using Chocopoi.DressingTools.Components.Menu;
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
    internal class MenuGroupView : ElementView, IMenuGroupView
    {
        private static readonly I18nTranslator t = I18n.ToolTranslator;

        public event Action AddItem;
        public event Action<int> RemoveItem;

        public DTMenuGroup Target { get; set; }

        private readonly MenuGroupPresenter _presenter;
        private VisualElement _itemsContainer;
        private Button _addBtn;
        private List<MenuItemView> _subViews;

        public MenuGroupView()
        {
            _subViews = new List<MenuItemView>();
            _presenter = new MenuGroupPresenter(this);
        }

        private void InitVisualTree()
        {
            var tree = Resources.Load<VisualTreeAsset>("MenuGroupView");
            tree.CloneTree(this);
            var styleSheet = Resources.Load<StyleSheet>("MenuGroupViewStyles");
            if (!styleSheets.Contains(styleSheet))
            {
                styleSheets.Add(styleSheet);
            }

            _itemsContainer = Q<VisualElement>("items-container").First();
            _addBtn = Q<Button>("add-btn").First();
            _addBtn.clicked += AddItem;
        }

        public override void OnEnable()
        {
            InitVisualTree();

            t.LocalizeElement(this);

            RaiseLoadEvent();
        }

        public override void OnDisable()
        {
            base.OnDisable();
        }

        private void UpdateItems()
        {
            _itemsContainer.Clear();

            // disable existing views
            foreach (var view in _subViews)
            {
                view.OnDisable();
            }
            _subViews.Clear();

            var len = Target.transform.childCount;
            for (var i = 0; i < len; i++)
            {
                var trans = Target.transform.GetChild(i);
                if (!trans.TryGetComponent<DTMenuItem>(out var menuItem))
                {
                    continue;
                }

                var box = new Box();
                box.AddToClassList("item-view-container");

                var rmvBtnContainer = new VisualElement();
                rmvBtnContainer.AddToClassList("rmv-btn-container");
                rmvBtnContainer.Add(new Label());
                var myIdx = i;
                var rmvBtn = new Button(() => RemoveItem?.Invoke(myIdx))
                {
                    text = "x"
                };
                rmvBtnContainer.Add(rmvBtn);
                box.Add(rmvBtnContainer);

                var view = new MenuItemView() { Target = menuItem };
                view.OnEnable();
                _subViews.Add(view);

                box.Add(view);
                _itemsContainer.Add(box);
            }
        }

        public override void Repaint()
        {
            UpdateItems();
        }

        public bool ShowConfirmRemoveDialog()
        {
            return EditorUtility.DisplayDialog("DressingTools", "Are you sure to remove this menu item? The underlying objects will also be removed!", "Yes", "No");
        }
    }
}
