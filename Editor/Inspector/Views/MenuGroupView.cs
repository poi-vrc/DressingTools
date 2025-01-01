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
        public event Action AddSmartControl;
        public event Action<int> RemoveItem;

        public DTMenuGroup Target { get; set; }

        private readonly MenuGroupPresenter _presenter;
        private VisualElement _itemsContainer;
        private Button _addItemBtn;
        private readonly List<MenuItemView> _subViews;
        private Button _addSmartControlBtn;

        public MenuGroupView()
        {
            _subViews = new List<MenuItemView>();
            _presenter = new MenuGroupPresenter(this);

            InitVisualTree();
            t.LocalizeElement(this);
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

            _itemsContainer = Q<VisualElement>("items-container");

            _addItemBtn = Q<Button>("add-item-btn");
            _addItemBtn.clicked += AddItem;

            _addSmartControlBtn = Q<Button>("add-smart-control-btn");
            _addSmartControlBtn.clicked += AddSmartControl;
        }

        private Box MakeMenuItemBox<T>(int i, T comp) where T : Component
        {
            var box = new Box();
            box.AddToClassList("item-view-container");

            var headerContainer = new VisualElement();
            headerContainer.AddToClassList("header-container");
            box.Add(headerContainer);

            var objField = new ObjectField()
            {
                objectType = typeof(T),
                value = comp
            };
            objField.RegisterValueChangedCallback(evt => objField.value = comp);
            headerContainer.Add(objField);

            var myIdx = i;
            var rmvBtn = new Button(() => RemoveItem?.Invoke(myIdx))
            {
                text = "x"
            };
            headerContainer.Add(rmvBtn);

            return box;
        }

        private void UpdateItems()
        {
            _itemsContainer.Clear();
            _subViews.Clear();

            var len = Target.transform.childCount;
            for (var i = 0; i < len; i++)
            {
                var trans = Target.transform.GetChild(i);

                var sc = trans.GetComponent<DTSmartControl>();
                var menuItem = trans.GetComponent<DTMenuItem>();

                var useSc = sc != null && sc.DriverType == DTSmartControl.SCDriverType.MenuItem;
                Component comp;
                if (useSc)
                {
                    comp = sc;
                }
                else if (menuItem != null)
                {
                    comp = menuItem;
                }
                else
                {
                    continue;
                }

                var box = MakeMenuItemBox(i, comp);
                _itemsContainer.Add(box);

                if (useSc)
                {
                    box.Add(CreateHelpBox(t._("inspector.menu.group.helpbox.modifySmartControlGoToComponent"), MessageType.Info));
                }
                else
                {
                    var view = new MenuItemView() { Target = menuItem };
                    _subViews.Add(view);
                    box.Add(view);
                }
            }
        }

        public override void Repaint()
        {
            UpdateItems();
        }

        public bool ShowConfirmRemoveDialog()
        {
            return EditorUtility.DisplayDialog(t._("tool.name"), t._("inspector.menu.group.dialog.confirmRemoveMenuItem"), t._("common.dialog.btn.yes"), t._("common.dialog.btn.no"));
        }
    }
}
