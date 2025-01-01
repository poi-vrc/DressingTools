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
using Chocopoi.DressingTools.UI.Elements;
using Chocopoi.DressingTools.UI.Presenters;
using Chocopoi.DressingTools.UI.Views;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
#if DT_VRCSDK3A
using VRC.SDK3.Avatars.ScriptableObjects;
#endif

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
        public event Action SubMenuTypeChanged;
        public event Action AxisLabelChanged;
        public event Action TwoAxisControllerChanged;
        public event Action FourAxisControllerChanged;
        public event Action RadialControllerChanged;
        public event Action InfoParameterFieldsChanged;
        public event Action DetailsParameterFieldsChanged;
        public event Action SubMenuObjectFieldsChanged;

        public DTMenuItem Target { get; set; }
        public string Name { get => _itemInfoNameField.value; set => _itemInfoNameField.value = value; }
        public int Type { get => _itemTypePopup.index; set => _itemTypePopup.index = value; }
        public int SubMenuType { get => _subMenuTypePopup.index; set => _subMenuTypePopup.index = value; }
        public Texture2D Icon { get; set; }

        public string ParameterName { get; set; }
        public float ParameterValue { get; set; }

        public AxisLabel AxisUpLabel { get; set; }
        public AxisLabel AxisRightLabel { get; set; }
        public AxisLabel AxisDownLabel { get; set; }
        public AxisLabel AxisLeftLabel { get; set; }

        public string HorizontalParameter { get => _itemDetailsHorizontalParameterField.value; set => _itemDetailsHorizontalParameterField.value = value; }
        public string VerticalParameter { get => _itemDetailsVerticalParameterField.value; set => _itemDetailsVerticalParameterField.value = value; }
        public string UpParameter { get => _itemDetailsUpParameterField.value; set => _itemDetailsUpParameterField.value = value; }
        public string RightParameter { get => _itemDetailsRightParameterField.value; set => _itemDetailsRightParameterField.value = value; }
        public string DownParameter { get => _itemDetailsDownParameterField.value; set => _itemDetailsDownParameterField.value = value; }
        public string LeftParameter { get => _itemDetailsLeftParameterField.value; set => _itemDetailsLeftParameterField.value = value; }
        public string RadialParameter { get => _itemDetailsRadialParameterField.value; set => _itemDetailsRadialParameterField.value = value; }

        public DTMenuGroup DTSubMenu { get => (DTMenuGroup)_dtSubMenuObjField.value; set => _dtSubMenuObjField.value = value; }
#if DT_VRCSDK3A
        public VRCExpressionsMenu VRCSubMenu { get => (VRCExpressionsMenu)_vrcSubMenuObjField.value; set => _vrcSubMenuObjField.value = value; }
#endif

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
        private AnimatorParameterTextField _itemInfoParameterNameField;
        private FloatField _itemInfoParameterValueField;
        private AnimatorParameterTextField _itemDetailsParameterNameField;
        private FloatField _itemDetailsParameterValueField;
        private AnimatorParameterTextField _itemDetailsHorizontalParameterField;
        private AnimatorParameterTextField _itemDetailsVerticalParameterField;
        private VisualElement _axisPanel;
        private VisualElement _paramsPanel;
        private AnimatorParameterTextField _itemDetailsUpParameterField;
        private AnimatorParameterTextField _itemDetailsRightParameterField;
        private AnimatorParameterTextField _itemDetailsDownParameterField;
        private AnimatorParameterTextField _itemDetailsLeftParameterField;
        private AnimatorParameterTextField _itemDetailsRadialParameterField;
        private VisualElement _axisUpIcon;
        private ObjectField _axisUpIconObjField;
        private TextField _axisUpLabelField;
        private VisualElement _axisLeftIcon;
        private ObjectField _axisLeftIconObjField;
        private TextField _axisLeftLabelField;
        private VisualElement _axisRightIcon;
        private ObjectField _axisRightIconObjField;
        private TextField _axisRightLabelField;
        private VisualElement _axisDownIcon;
        private ObjectField _axisDownIconObjField;
        private TextField _axisDownLabelField;
        private readonly MenuItemPresenter _presenter;

        public MenuItemView()
        {
            AxisUpLabel = new AxisLabel();
            AxisRightLabel = new AxisLabel();
            AxisDownLabel = new AxisLabel();
            AxisLeftLabel = new AxisLabel();
            _presenter = new MenuItemPresenter(this);
            InitVisualTree();
            InitInfoPanel();
            InitDetailsFoldout();
            t.LocalizeElement(this);
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
            _itemInfoIconContainer = Q<VisualElement>("item-icon");
            var iconObjFieldContainer = Q<VisualElement>("item-icon-objfield-container");
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

            _itemInfoNameLabel = Q<Label>("item-info-name-label");
            _itemInfoNameField = Q<TextField>("item-info-name-field");
            _itemInfoNameField.RegisterValueChangedCallback((evt) => NameChanged?.Invoke());

            var itemTypePopupContainer = Q<VisualElement>("item-info-item-type-popup-container");
            var itemTypeChoices = new List<string>() { t._("inspector.menu.item.info.itemType.button"), t._("inspector.menu.item.info.itemType.toggle"), t._("inspector.menu.item.info.itemType.subMenu"), t._("inspector.menu.item.info.itemType.twoAxis"), t._("inspector.menu.item.info.itemType.fourAxis"), t._("inspector.menu.item.info.itemType.radial") };
            _itemTypePopup = new PopupField<string>(t._("inspector.menu.item.info.popup.itemType"), itemTypeChoices, 0);
            _itemTypePopup.RegisterValueChangedCallback((evt) =>
            {
                UpdateItemDisplay();
                TypeChanged?.Invoke();
            });
            itemTypePopupContainer.Add(_itemTypePopup);

            _itemInfoParameterNameField = Q<AnimatorParameterTextField>("item-info-parameter-name-field");
            _itemInfoParameterValueField = Q<FloatField>("item-info-parameter-value-field");
            _itemInfoParameterNameField.RegisterValueChangedCallback(evt =>
            {
                ParameterName = evt.newValue;
                InfoParameterFieldsChanged?.Invoke();
            });
            _itemInfoParameterValueField.RegisterValueChangedCallback(evt =>
            {
                ParameterValue = evt.newValue;
                InfoParameterFieldsChanged?.Invoke();
            });

            var subMenuTypePopupContainer = Q<VisualElement>("item-info-submenu-type-popup-container");
            var subMenuTypeChoices = new List<string>() { t._("inspector.menu.item.info.subMenuType.children"), t._("inspector.menu.item.info.subMenuType.dtMenuGroupComponent"), t._("inspector.menu.item.info.subMenuType.vrcMenuAsset") };
            _subMenuTypePopup = new PopupField<string>(t._("inspector.menu.item.info.popup.subMenuType"), subMenuTypeChoices, 0);
            _subMenuTypePopup.RegisterValueChangedCallback((evt) =>
            {
                UpdateSubMenuFieldsDisplay();
                SubMenuTypeChanged?.Invoke();
            });
            subMenuTypePopupContainer.Add(_subMenuTypePopup);

            var dtSubMenuObjFieldContainer = Q<VisualElement>("item-info-dtsubmenu-objfield-container");
            _dtSubMenuObjField = new ObjectField(t._("inspector.menu.item.info.objectField.subMenu"))
            {
                objectType = typeof(DTMenuGroup)
            };
            _dtSubMenuObjField.RegisterValueChangedCallback(evt =>
            {
                DTSubMenu = (DTMenuGroup)evt.newValue;
                SubMenuObjectFieldsChanged?.Invoke();
            });
            dtSubMenuObjFieldContainer.Add(_dtSubMenuObjField);

#if DT_VRCSDK3A
            var vrcSubMenuObjFieldContainer = Q<VisualElement>("item-info-vrcsubmenu-objfield-container");
            _vrcSubMenuObjField = new ObjectField(t._("inspector.menu.item.info.objectField.subMenu"))
            {
                objectType = typeof(VRCExpressionsMenu)
            };
            _vrcSubMenuObjField.RegisterValueChangedCallback(evt =>
            {
                VRCSubMenu = (VRCExpressionsMenu)evt.newValue;
                SubMenuObjectFieldsChanged?.Invoke();
            });
            vrcSubMenuObjFieldContainer.Add(_vrcSubMenuObjField);
#endif
        }

        private void InitDetailsFoldout()
        {
            _itemInfoParameterNameField.style.display = DisplayStyle.None;
            _itemInfoParameterValueField.style.display = DisplayStyle.None;

            _itemDetailsFoldout = Q<Foldout>("details-foldout");
            _itemDetailsContainer = Q<VisualElement>("details-container");
            BindFoldoutHeaderAndContainerWithPrefix("details");

            _itemDetailsParameterNameField = Q<AnimatorParameterTextField>("details-parameter-name-field");
            _itemDetailsParameterValueField = Q<FloatField>("details-parameter-value-field");
            _itemDetailsParameterNameField.RegisterValueChangedCallback(evt =>
            {
                ParameterName = evt.newValue;
                DetailsParameterFieldsChanged?.Invoke();
            });
            _itemDetailsParameterValueField.RegisterValueChangedCallback(evt =>
            {
                ParameterValue = evt.newValue;
                DetailsParameterFieldsChanged?.Invoke();
            });

            _itemDetailsHorizontalParameterField = Q<AnimatorParameterTextField>("details-horizontal-parameter-text-field");
            _itemDetailsVerticalParameterField = Q<AnimatorParameterTextField>("details-vertical-parameter-text-field");
            _itemDetailsHorizontalParameterField.RegisterValueChangedCallback(evt => TwoAxisControllerChanged?.Invoke());
            _itemDetailsVerticalParameterField.RegisterValueChangedCallback(evt => TwoAxisControllerChanged?.Invoke());

            _axisPanel = Q<VisualElement>("axis-panel");
            _paramsPanel = Q<VisualElement>("params-panel");

            _itemDetailsUpParameterField = Q<AnimatorParameterTextField>("details-up-parameter-text-field");
            _itemDetailsRightParameterField = Q<AnimatorParameterTextField>("details-right-parameter-text-field");
            _itemDetailsDownParameterField = Q<AnimatorParameterTextField>("details-down-parameter-text-field");
            _itemDetailsLeftParameterField = Q<AnimatorParameterTextField>("details-left-parameter-text-field");
            _itemDetailsUpParameterField.RegisterValueChangedCallback(evt => FourAxisControllerChanged?.Invoke());
            _itemDetailsRightParameterField.RegisterValueChangedCallback(evt => FourAxisControllerChanged?.Invoke());
            _itemDetailsDownParameterField.RegisterValueChangedCallback(evt => FourAxisControllerChanged?.Invoke());
            _itemDetailsLeftParameterField.RegisterValueChangedCallback(evt => FourAxisControllerChanged?.Invoke());

            _itemDetailsRadialParameterField = Q<AnimatorParameterTextField>("details-radial-parameter-text-field");
            _itemDetailsRadialParameterField.RegisterValueChangedCallback(evt => RadialControllerChanged?.Invoke());

            InitAxisLabels();
        }

        private void InitAxisLabel(ref VisualElement axisContainer, ref VisualElement iconContainer, ref ObjectField objField, ref TextField textField, Action onChange)
        {
            iconContainer = new VisualElement();
            objField = new ObjectField()
            {
                objectType = typeof(Texture2D)
            };
            objField.RegisterValueChangedCallback(evt => onChange?.Invoke());
            textField = new TextField();
            textField.RegisterValueChangedCallback(evt => onChange?.Invoke());
            axisContainer.Add(iconContainer);
            axisContainer.Add(objField);
            axisContainer.Add(textField);
        }

        private void InitAxisLabels()
        {
            var axisUpContainer = Q<VisualElement>("axis-up-container");
            var axisLeftContainer = Q<VisualElement>("axis-left-container");
            var axisRightContainer = Q<VisualElement>("axis-right-container");
            var axisDownContainer = Q<VisualElement>("axis-down-container");

            InitAxisLabel(ref axisUpContainer, ref _axisUpIcon, ref _axisUpIconObjField, ref _axisUpLabelField, () =>
            {
                AxisUpLabel.icon = (Texture2D)_axisUpIconObjField.value;
                AxisUpLabel.name = _axisUpLabelField.value;
                AxisLabelChanged?.Invoke();
            });

            InitAxisLabel(ref axisRightContainer, ref _axisRightIcon, ref _axisRightIconObjField, ref _axisRightLabelField, () =>
            {
                AxisRightLabel.icon = (Texture2D)_axisRightIconObjField.value;
                AxisRightLabel.name = _axisRightLabelField.value;
                AxisLabelChanged?.Invoke();
            });

            InitAxisLabel(ref axisDownContainer, ref _axisDownIcon, ref _axisDownIconObjField, ref _axisDownLabelField, () =>
            {
                AxisDownLabel.icon = (Texture2D)_axisDownIconObjField.value;
                AxisDownLabel.name = _axisDownLabelField.value;
                AxisLabelChanged?.Invoke();
            });

            InitAxisLabel(ref axisLeftContainer, ref _axisLeftIcon, ref _axisLeftIconObjField, ref _axisLeftLabelField, () =>
            {
                AxisLeftLabel.icon = (Texture2D)_axisLeftIconObjField.value;
                AxisLeftLabel.name = _axisLeftLabelField.value;
                AxisLabelChanged?.Invoke();
            });
        }

        private void UpdateSubMenuFieldsDisplay()
        {
            // TODO: magic number
            _dtSubMenuObjField.style.display = Type == 2 && SubMenuType == 1 ? DisplayStyle.Flex : DisplayStyle.None;
#if DT_VRCSDK3A
            _vrcSubMenuObjField.style.display = Type == 2 && SubMenuType == 2 ? DisplayStyle.Flex : DisplayStyle.None;
#endif
        }

        private void UpdateItemDisplay()
        {
            _itemInfoNameLabel.text = Name;

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

            _axisPanel.style.display = DisplayStyle.None;
            _paramsPanel.style.display = DisplayStyle.None;

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
            }
            else if (Type == 3)
            {
                // Two-axis
                _itemDetailsFoldout.style.display = DisplayStyle.Flex;
                _itemDetailsContainer.style.display = _itemDetailsFoldout.value ? DisplayStyle.Flex : DisplayStyle.None;

                _axisPanel.style.display = DisplayStyle.Flex;
                _paramsPanel.style.display = DisplayStyle.Flex;

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

                _axisPanel.style.display = DisplayStyle.Flex;
                _paramsPanel.style.display = DisplayStyle.Flex;

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

                _paramsPanel.style.display = DisplayStyle.Flex;

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

        private void RepaintAxisLabels()
        {
            UpdateAxisLabel(_axisUpIcon, _axisUpIconObjField, _axisUpLabelField, AxisUpLabel);
            UpdateAxisLabel(_axisRightIcon, _axisRightIconObjField, _axisRightLabelField, AxisRightLabel);
            UpdateAxisLabel(_axisDownIcon, _axisDownIconObjField, _axisDownLabelField, AxisDownLabel);
            UpdateAxisLabel(_axisLeftIcon, _axisLeftIconObjField, _axisLeftLabelField, AxisLeftLabel);
        }

        private void UpdateAxisLabel(VisualElement iconContainer, ObjectField objField, TextField textField, AxisLabel data)
        {
            iconContainer.style.backgroundImage = new StyleBackground(data.icon != null ? data.icon : s_iconPlaceholderImage);
            objField.value = data.icon;
            textField.value = data.name;
        }

        public void RepaintInfoParameterFields()
        {
            _itemInfoParameterNameField.value = ParameterName;
            _itemInfoParameterValueField.value = ParameterValue;
        }

        public void RepaintDetailsParameterFields()
        {
            _itemDetailsParameterNameField.value = ParameterName;
            _itemDetailsParameterValueField.value = ParameterValue;
        }

        public void SetAnimatorParameterTextFieldAvatarGameObject(GameObject avatarGameObject)
        {
            _itemInfoParameterNameField.avatarGameObject = avatarGameObject;
            _itemDetailsHorizontalParameterField.avatarGameObject = avatarGameObject;
            _itemDetailsVerticalParameterField.avatarGameObject = avatarGameObject;
            _itemDetailsUpParameterField.avatarGameObject = avatarGameObject;
            _itemDetailsRightParameterField.avatarGameObject = avatarGameObject;
            _itemDetailsDownParameterField.avatarGameObject = avatarGameObject;
            _itemDetailsLeftParameterField.avatarGameObject = avatarGameObject;
            _itemDetailsRadialParameterField.avatarGameObject = avatarGameObject;
            _itemDetailsParameterNameField.avatarGameObject = avatarGameObject;
        }

        public override void Repaint()
        {
            UpdateIcon();
            UpdateItemDisplay();
            UpdateSubMenuFieldsDisplay();

            RepaintAxisLabels();
            RepaintInfoParameterFields();
            RepaintDetailsParameterFields();
        }
    }
}
