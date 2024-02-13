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
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Chocopoi.DressingTools.Inspector.Views
{
    [ExcludeFromCodeCoverage]
    internal class MenuItemView : ElementView, IMenuItemView
    {
        private static readonly I18nTranslator t = I18n.ToolTranslator;
        private static Texture2D s_iconPlaceholderImage;

        public event Action NameChanged;
        public event Action TypeChanged;
        public event Action IconChanged;

        public DTMenuItem Target { get; set; }
        public string Name { get => _itemInfoNameField.value; set => _itemInfoNameField.value = value; }
        public int Type { get => _itemTypePopup.index; set => _itemTypePopup.index = value; }
        public Texture2D Icon { get; set; }

        private Foldout _itemInfoFoldout;
        private VisualElement _itemInfoContainer;
        private Foldout _itemDetailsFoldout;
        private VisualElement _itemDetailsContainer;
        private VisualElement _itemInfoIconContainer;
        private ObjectField _itemInfoIconObjField;
        private Label _itemInfoNameLabel;
        private TextField _itemInfoNameField;
        private PopupField<string> _itemTypePopup;
        private PopupField<string> _subMenuTypePopup;
        private ObjectField _dtSubMenuObjField;
        private ObjectField _vrcSubMenuObjField;
        private VisualElement _itemInfoParameterNameField;
        private VisualElement _itemInfoParameterValueField;
        private VisualElement _itemDetailsParameterNameField;
        private VisualElement _itemDetailsParameterValueField;
        private VisualElement _itemDetailsHorizontalParameterField;
        private VisualElement _itemDetailsVerticalParameterField;
        private VisualElement _itemDetailsUpParameterField;
        private VisualElement _itemDetailsRightParameterField;
        private VisualElement _itemDetailsDownParameterField;
        private VisualElement _itemDetailsLeftParameterField;
        private VisualElement _itemDetailsRadialParameterField;
        private readonly MenuItemPresenter _presenter;

        public MenuItemView(DTMenuItem target)
        {
            Target = target;
            _presenter = new MenuItemPresenter(this);
        }

        private void InitVisualTree()
        {
            var tree = Resources.Load<VisualTreeAsset>("MenuItemView");
            tree.CloneTree(this);
            var styleSheet = Resources.Load<StyleSheet>("MenuItemViewStyles");
            if (!styleSheets.Contains(styleSheet))
            {
                styleSheets.Add(styleSheet);
            }
        }

        private void InitInfoPanel()
        {
            _itemInfoFoldout = Q<Foldout>("item-info-foldout").First();
            _itemInfoContainer = Q<VisualElement>("item-info-container").First();
            BindFoldoutHeaderAndContainerWithPrefix("item-info");

            _itemInfoIconContainer = Q<VisualElement>("item-icon").First();
            var iconObjFieldContainer = Q<VisualElement>("item-icon-objfield-container").First();
            _itemInfoIconObjField = new ObjectField()
            {
                objectType = typeof(Texture2D)
            };
            _itemInfoIconObjField.RegisterValueChangedCallback(evt =>
            {
                Icon = (Texture2D)_itemInfoIconObjField.value;
                IconChanged?.Invoke();
            });
            iconObjFieldContainer.Add(_itemInfoIconObjField);

            _itemInfoNameLabel = Q<Label>("item-info-name-label").First();
            _itemInfoNameField = Q<TextField>("item-info-name-field").First();
            _itemInfoNameField.RegisterValueChangedCallback((evt) => NameChanged?.Invoke());

            var itemTypePopupContainer = Q<VisualElement>("item-info-item-type-popup-container").First();
            var itemTypeChoices = new List<string>() { "Button", "Toggle", "Sub-menu", "Two Axis", "Four Axis", "Radial" };
            _itemTypePopup = new PopupField<string>("Type", itemTypeChoices, 0);
            _itemTypePopup.RegisterValueChangedCallback((evt) =>
            {
                UpdateItemDisplay();
                TypeChanged?.Invoke();
            });
            itemTypePopupContainer.Add(_itemTypePopup);

            _itemInfoParameterNameField = Q<VisualElement>("item-info-parameter-name-field").First();
            _itemInfoParameterValueField = Q<VisualElement>("item-info-parameter-value-field").First();

            var subMenuTypePopupContainer = Q<VisualElement>("item-info-submenu-type-popup-container").First();
            var subMenuTypeChoices = new List<string>() { "Children", "DT Menu Group Component", "VRC Expressions Menu Asset" };
            _subMenuTypePopup = new PopupField<string>("Sub-menu Type", subMenuTypeChoices, 0);
            subMenuTypePopupContainer.Add(_subMenuTypePopup);

            var dtSubMenuObjFieldContainer = Q<VisualElement>("item-info-dtsubmenu-objfield-container").First();
            _dtSubMenuObjField = new ObjectField("Sub-menu")
            {
                objectType = typeof(DTMenuGroup),
                bindingPath = "m_DTSubMenu"
            };
            dtSubMenuObjFieldContainer.Add(_dtSubMenuObjField);

#if DT_VRCSDK3A
            var vrcSubMenuObjFieldContainer = Q<VisualElement>("item-info-vrcsubmenu-objfield-container").First();
            _vrcSubMenuObjField = new ObjectField("Sub-menu")
            {
                objectType = typeof(VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu),
                bindingPath = "m_VRCSubMenu"
            };
            vrcSubMenuObjFieldContainer.Add(_vrcSubMenuObjField);
#endif
        }

        private void InitDetailsFoldout()
        {
            _itemInfoParameterNameField.style.display = DisplayStyle.None;
            _itemInfoParameterValueField.style.display = DisplayStyle.None;

            _itemDetailsFoldout = Q<Foldout>("details-foldout").First();
            _itemDetailsContainer = Q<VisualElement>("details-container").First();
            BindFoldoutHeaderAndContainerWithPrefix("details");

            _itemDetailsParameterNameField = Q<VisualElement>("details-parameter-name-field").First();
            _itemDetailsParameterValueField = Q<VisualElement>("details-parameter-value-field").First();

            _itemDetailsHorizontalParameterField = Q<VisualElement>("details-horizontal-parameter-text-field").First();
            _itemDetailsVerticalParameterField = Q<VisualElement>("details-vertical-parameter-text-field").First();

            _itemDetailsUpParameterField = Q<VisualElement>("details-up-parameter-text-field").First();
            _itemDetailsRightParameterField = Q<VisualElement>("details-right-parameter-text-field").First();
            _itemDetailsDownParameterField = Q<VisualElement>("details-down-parameter-text-field").First();
            _itemDetailsLeftParameterField = Q<VisualElement>("details-left-parameter-text-field").First();

            _itemDetailsRadialParameterField = Q<VisualElement>("details-radial-parameter-text-field").First();
        }

        private void UpdateItemDisplay()
        {
            _itemInfoFoldout.text = $"{_itemTypePopup.text}: {Name}";
            _itemInfoNameLabel.text = Name;
            // TODO: labels

            _itemInfoParameterNameField.style.display = DisplayStyle.None;
            _itemInfoParameterValueField.style.display = DisplayStyle.None;

            _itemDetailsFoldout.style.display = DisplayStyle.None;
            _itemDetailsContainer.style.display = DisplayStyle.None;

            _itemDetailsParameterNameField.style.display = DisplayStyle.None;
            _itemDetailsParameterValueField.style.display = DisplayStyle.None;

            _itemDetailsHorizontalParameterField.style.display = DisplayStyle.None;
            _itemDetailsVerticalParameterField.style.display = DisplayStyle.None;

            _itemDetailsUpParameterField.style.display = DisplayStyle.None;
            _itemDetailsRightParameterField.style.display = DisplayStyle.None;
            _itemDetailsDownParameterField.style.display = DisplayStyle.None;
            _itemDetailsLeftParameterField.style.display = DisplayStyle.None;

            _itemDetailsRadialParameterField.style.display = DisplayStyle.None;

            _subMenuTypePopup.style.display = DisplayStyle.None;
            _dtSubMenuObjField.style.display = DisplayStyle.None;
#if DT_VRCSDK3A
            _vrcSubMenuObjField.style.display = DisplayStyle.None;
#endif
            if (Type == 0)
            {
                // Button
                _itemInfoParameterNameField.style.display = DisplayStyle.Flex;
                _itemInfoParameterValueField.style.display = DisplayStyle.Flex;
            }
            else if (Type == 1)
            {
                // Toggle
                _itemInfoParameterNameField.style.display = DisplayStyle.Flex;
                _itemInfoParameterValueField.style.display = DisplayStyle.Flex;
            }
            else if (Type == 2)
            {
                // Submenu
                _itemDetailsFoldout.style.display = DisplayStyle.Flex;
                _itemDetailsContainer.style.display = _itemDetailsFoldout.value ? DisplayStyle.Flex : DisplayStyle.None;

                // we move parameter on open to details
                _itemDetailsParameterNameField.style.display = DisplayStyle.Flex;
                _itemDetailsParameterValueField.style.display = DisplayStyle.Flex;

                _subMenuTypePopup.style.display = DisplayStyle.Flex;
                _dtSubMenuObjField.style.display = DisplayStyle.Flex;
#if DT_VRCSDK3A
                _vrcSubMenuObjField.style.display = DisplayStyle.Flex;
#endif
            }
            else if (Type == 3)
            {
                // Two-axis
                _itemDetailsFoldout.style.display = DisplayStyle.Flex;
                _itemDetailsContainer.style.display = _itemDetailsFoldout.value ? DisplayStyle.Flex : DisplayStyle.None;

                _itemDetailsParameterNameField.style.display = DisplayStyle.Flex;
                _itemDetailsParameterValueField.style.display = DisplayStyle.Flex;

                _itemDetailsHorizontalParameterField.style.display = DisplayStyle.Flex;
                _itemDetailsVerticalParameterField.style.display = DisplayStyle.Flex;
            }
            else if (Type == 4)
            {
                // Four-axis
                _itemDetailsFoldout.style.display = DisplayStyle.Flex;
                _itemDetailsContainer.style.display = _itemDetailsFoldout.value ? DisplayStyle.Flex : DisplayStyle.None;

                _itemDetailsParameterNameField.style.display = DisplayStyle.Flex;
                _itemDetailsParameterValueField.style.display = DisplayStyle.Flex;

                _itemDetailsUpParameterField.style.display = DisplayStyle.Flex;
                _itemDetailsRightParameterField.style.display = DisplayStyle.Flex;
                _itemDetailsDownParameterField.style.display = DisplayStyle.Flex;
                _itemDetailsLeftParameterField.style.display = DisplayStyle.Flex;
            }
            else if (Type == 5)
            {
                // Radial
                _itemDetailsFoldout.style.display = DisplayStyle.Flex;
                _itemDetailsContainer.style.display = _itemDetailsFoldout.value ? DisplayStyle.Flex : DisplayStyle.None;

                _itemDetailsParameterNameField.style.display = DisplayStyle.Flex;
                _itemDetailsParameterValueField.style.display = DisplayStyle.Flex;

                _itemDetailsRadialParameterField.style.display = DisplayStyle.Flex;
            }
        }

        private void UpdateIcon()
        {
            if (s_iconPlaceholderImage == null)
            {
                s_iconPlaceholderImage = Resources.Load<Texture2D>("thumbnailPlaceholder");
            }

            _itemInfoIconObjField.value = Icon;
            _itemInfoIconContainer.style.backgroundImage = new StyleBackground(Icon != null ? Icon : s_iconPlaceholderImage);
        }

        public override void OnEnable()
        {
            InitVisualTree();
            InitInfoPanel();
            InitDetailsFoldout();

            t.LocalizeElement(this);

            RaiseLoadEvent();
        }

        public override void OnDisable()
        {
            base.OnDisable();
        }

        public override void Repaint()
        {
            UpdateIcon();
            UpdateItemDisplay();
        }
    }
}
