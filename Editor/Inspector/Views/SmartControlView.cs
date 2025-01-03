﻿/*
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
using Chocopoi.DressingTools.UI.Elements;
using Chocopoi.DressingTools.UI.Presenters;
using Chocopoi.DressingTools.UI.Views;
using UnityEditor;
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
        public event Action<int> ChangeObjectToggle;
        public event Action<int> RemoveObjectToggle;
        public event Action<int, Type> ChangeObjectToggleComponentType;
        public event Action<SmartControlCrossControlValue> ChangeCrossControlValueOnEnabled;
        public event Action<SmartControlCrossControlValue> ChangeCrossControlValueOnDisabled;
        public event Action<SmartControlCrossControlValue> RemoveCrossControlValueOnEnabled;
        public event Action<SmartControlCrossControlValue> RemoveCrossControlValueOnDisabled;
        public event Action AddNewPropertyGroup;
        public event Action<DTSmartControl.PropertyGroup> RemovePropertyGroup;
        public event Action MenuItemConfigChanged;
        public event Action VRCPhysBoneConfigChanged;
        public event Action ParameterSlotConfigChanged;

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
        public string MenuItemName { get => _driverMenuItemNameField.value; set => _driverMenuItemNameField.value = value; }
        public Texture2D MenuItemIcon { get => (Texture2D)_driverMenuItemIconObjField.value; set => _driverMenuItemIconObjField.value = value; }
        public int MenuItemType { get => _driverMenuItemPopup.index; set => _driverMenuItemPopup.index = value; }

#if DT_VRCSDK3A
        public VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBone VRCPhysBone { get => (VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBone)_driverVRCPhysBoneObjField.value; set => _driverVRCPhysBoneObjField.value = value; }
#endif
        public int VRCPhysBoneCondition { get => _driverVRCPhysBoneConditionPopup.index; set => _driverVRCPhysBoneConditionPopup.index = value; }
        public int VRCPhysBoneSource { get => _driverVRCPhysBoneSourcePopup.index; set => _driverVRCPhysBoneSourcePopup.index = value; }
        public DTParameterSlot ParameterSlot { get => (DTParameterSlot)_driverParamSlotObjField.value; set => _driverParamSlotObjField.value = value; }
        public bool ShowParameterSlotNotAssignedHelpbox { get; set; }
        public bool ParamSlotGenerateMenuItem { get => _driverParamSlotGenerateMenuItemToggle.value; set => _driverParamSlotGenerateMenuItemToggle.value = value; }
        public string ParamSlotMenuItemName { get => _driverParamSlotMenuItemNameField.value; set => _driverParamSlotMenuItemNameField.value = value; }
        public Texture2D ParamSlotMenuItemIcon { get => (Texture2D)_driverParamSlotMenuItemIconObjField.value; set => _driverParamSlotMenuItemIconObjField.value = value; }
        public int ParamSlotMenuItemType { get => _driverParamSlotMenuItemPopup.index; set => _driverParamSlotMenuItemPopup.index = value; }

        public List<SmartControlObjectToggleValue> ObjectToggles { get; set; }
        public List<SmartControlCrossControlValue> CrossControlValuesOnEnabled { get; set; }
        public List<SmartControlCrossControlValue> CrossControlValuesOnDisabled { get; set; }
        public List<DTSmartControl.PropertyGroup> PropertyGroups { get; set; }

        private readonly SmartControlPresenter _presenter;
        private Foldout _driverFoldout;
        private VisualElement _driverContainer;
        private VisualElement _driverTypePopupContainer;
        private PopupField<string> _driverTypePopup;
        private VisualElement _driverAnimParamContainer;
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
        private TableView _objectTogglesTable;
        private TableView.TableModel _objectTogglesTableModel;
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
        private VisualElement _driverMenuItemContainer;
        private ObjectField _driverMenuItemIconObjField;
        private TextField _driverMenuItemNameField;
        private PopupField<string> _driverMenuItemPopup;
        private VisualElement _driverVRCPhysBoneContainer;
        private VisualElement _objFieldHelpboxContainer;
        private ObjectField _driverVRCPhysBoneObjField;
        private PopupField<string> _driverVRCPhysBoneConditionPopup;
        private PopupField<string> _driverVRCPhysBoneSourcePopup;
        private VisualElement _condSrcHelpboxContainer;
        private VisualElement _driverParamSlotNotAssignedHelpboxContainer;
        private ObjectField _driverParamSlotObjField;
        private ObjectField _driverParamSlotMenuItemIconObjField;
        private PopupField<string> _driverParamSlotMenuItemPopup;
        private VisualElement _driverParamSlotContainer;
        private VisualElement _driverParamSlotViewContainer;
        private ElementView _driverParamSlotView;
        private Toggle _driverParamSlotGenerateMenuItemToggle;
        private Box _driverParamSlotMenuItemBox;
        private TextField _driverParamSlotMenuItemNameField;

        public SmartControlView()
        {
            ObjectToggles = new List<SmartControlObjectToggleValue>();
            CrossControlValuesOnEnabled = new List<SmartControlCrossControlValue>();
            CrossControlValuesOnDisabled = new List<SmartControlCrossControlValue>();
            PropertyGroups = new List<DTSmartControl.PropertyGroup>();
            _driverParamSlotView = null;
            _presenter = new SmartControlPresenter(this);
            InitVisualTree();
            InitDriverFoldout();
            InitControlFoldout();
            InitObjectTogglesFoldout();
            InitPropertyGroupsFoldout();
            InitCrossCtrlFoldout();
            t.LocalizeElement(this);
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

        private void UpdateMenuItemTypeDisplay()
        {
            if (MenuItemType == 0 || MenuItemType == 1)
            {
                ControlType = 0; // binary
            }
            else if (MenuItemType == 2)
            {
                ControlType = 1; // motion time
            }
            RepaintControlType();
            ControlTypeChanged?.Invoke();
        }

        private void InitDriverMenuItem()
        {
            _driverMenuItemContainer = Q<VisualElement>("menu-item-driver-container");
            _driverMenuItemNameField = Q<TextField>("menu-item-name-field");
            _driverMenuItemNameField.RegisterValueChangedCallback(evt =>
            {
                MenuItemConfigChanged?.Invoke();
            });

            var iconObjFieldContainer = Q<VisualElement>("menu-icon-obj-field-container");
            _driverMenuItemIconObjField = new ObjectField(t._("inspector.smartcontrol.driver.menuItem.objectField.icon"))
            {
                objectType = typeof(Texture2D)
            };
            _driverMenuItemIconObjField.RegisterValueChangedCallback(evt =>
            {
                MenuItemConfigChanged?.Invoke();
            });
            iconObjFieldContainer.Add(_driverMenuItemIconObjField);

            var popupContainer = Q<VisualElement>("menu-item-type-popup-container");
            var choices = new List<string>() {
                t._("inspector.smartcontrol.driver.menuItem.itemType.button"),
                t._("inspector.smartcontrol.driver.menuItem.itemType.toggle"),
                t._("inspector.smartcontrol.driver.menuItem.itemType.radial")
                };
            _driverMenuItemPopup = new PopupField<string>(t._("inspector.smartcontrol.driver.menuItem.popup.itemType"), choices, 0);
            _driverMenuItemPopup.RegisterValueChangedCallback(evt =>
            {
                UpdateMenuItemTypeDisplay();
                MenuItemConfigChanged?.Invoke();
            });
            popupContainer.Add(_driverMenuItemPopup);
        }

        private void InitDriverAnimParam()
        {
            _driverAnimParamContainer = Q<VisualElement>("anim-param-driver-container");

            _driverAnimParamDefaultValueBoolToggle = Q<Toggle>("anim-param-defval-bool-toggle");
            _driverAnimParamDefaultValueFloatContainer = Q<VisualElement>("anim-param-defval-float-container");
            _driverAnimParamDefaultValueFloatSlider = Q<Slider>("anim-param-defval-float-slider");
            _driverAnimParamDefaultValueFloatField = Q<FloatField>("anim-param-defval-float-field");

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

        private void InitDriverParameterSlot()
        {
            _driverParamSlotContainer = Q<VisualElement>("parameter-slot-driver-container");

            var descHelpboxContainer = Q<VisualElement>("parameter-slot-description-helpbox-container");
            descHelpboxContainer.Add(CreateHelpBox(t._("inspector.parameterSlot.helpbox.description"), MessageType.Info));

            _driverParamSlotNotAssignedHelpboxContainer = Q<VisualElement>("parameter-slot-not-assigned-helpbox-container");
            _driverParamSlotNotAssignedHelpboxContainer.Add(CreateHelpBox(t._("inspector.smartcontrol.driver.parameterSlot.helpbox.notAssigned"), MessageType.Error));

            var objFieldContainer = Q<VisualElement>("parameter-slot-obj-field-container");
            _driverParamSlotObjField = new ObjectField(t._("inspector.smartcontrol.driver.parameterSlot.objField.parameterSlot"))
            {
                objectType = typeof(DTParameterSlot)
            };
            _driverParamSlotObjField.RegisterValueChangedCallback(evt =>
            {
                ParameterSlotConfigChanged?.Invoke();
            });
            objFieldContainer.Add(_driverParamSlotObjField);

            _driverParamSlotGenerateMenuItemToggle = Q<Toggle>("parameter-slot-generate-menu-item-toggle");
            _driverParamSlotGenerateMenuItemToggle.RegisterValueChangedCallback(evt =>
            {
                UpdateParamSlot();
                ParameterSlotConfigChanged?.Invoke();
            });

            _driverParamSlotMenuItemBox = Q<Box>("parameter-slot-menu-item-box");

            _driverParamSlotMenuItemNameField = Q<TextField>("parameter-slot-menu-item-name-field");
            _driverParamSlotMenuItemNameField.RegisterValueChangedCallback(evt =>
            {
                ParameterSlotConfigChanged?.Invoke();
            });

            var iconObjFieldContainer = Q<VisualElement>("parameter-slot-menu-icon-obj-field-container");
            _driverParamSlotMenuItemIconObjField = new ObjectField(t._("inspector.smartcontrol.driver.parameterSlot.objectField.menuItemIcon"))
            {
                objectType = typeof(Texture2D)
            };
            _driverParamSlotMenuItemIconObjField.RegisterValueChangedCallback(evt =>
            {
                ParameterSlotConfigChanged?.Invoke();
            });
            iconObjFieldContainer.Add(_driverParamSlotMenuItemIconObjField);

            var popupContainer = Q<VisualElement>("parameter-slot-menu-item-type-popup-container");
            var choices = new List<string>() {
                t._("inspector.smartcontrol.driver.parameterSlot.menuItemType.button"),
                t._("inspector.smartcontrol.driver.parameterSlot.menuItemType.toggle")
                };
            _driverParamSlotMenuItemPopup = new PopupField<string>(t._("inspector.smartcontrol.driver.parameterSlot.popup.menuItemType"), choices, 0);
            _driverParamSlotMenuItemPopup.RegisterValueChangedCallback(evt =>
            {
                ParameterSlotConfigChanged?.Invoke();
            });
            popupContainer.Add(_driverParamSlotMenuItemPopup);

            _driverParamSlotViewContainer = Q<VisualElement>("parameter-slot-view-container");
        }

        private void UpdateDriverVRCPhysBoneDisplay()
        {
#if DT_VRCSDK3A
            UpdateDriverVRCPhysBoneObjHelpboxDisplay();
#endif

            _condSrcHelpboxContainer.style.display = VRCPhysBoneCondition == 0 && VRCPhysBoneSource == 0 ? DisplayStyle.Flex : DisplayStyle.None;
            _controlTypePopupContainer.SetEnabled(false);

            if (VRCPhysBoneSource == 0)
            {
                // none source use binary
                ControlType = 0;
            }
            else
            {
                ControlType = 1;
            }
            RepaintControlType();
            ControlTypeChanged?.Invoke();
        }

#if DT_VRCSDK3A
        private void UpdateDriverVRCPhysBoneObjHelpboxDisplay()
        {
            _objFieldHelpboxContainer.style.display = VRCPhysBone == null ? DisplayStyle.Flex : DisplayStyle.None;
        }
#endif

        private void InitDriverVRCPhysBone()
        {
            _driverVRCPhysBoneContainer = Q<VisualElement>("vrcphysbone-driver-container");

            _objFieldHelpboxContainer = Q<VisualElement>("vrcphysbone-obj-field-helpbox-container");
            _objFieldHelpboxContainer.style.display = DisplayStyle.None;
            _objFieldHelpboxContainer.Add(CreateHelpBox(t._("inspector.smartcontrol.driver.vrcPhysBone.helpbox.emptyPhysbone"), MessageType.Error));

            var objFieldContainer = Q<VisualElement>("vrcphysbone-obj-field-container");
#if DT_VRCSDK3A
            _driverVRCPhysBoneObjField = new ObjectField(t._("inspector.smartcontrol.driver.vrcPhysBone.objField.physBone"))
            {
                objectType = typeof(VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBone)
            };
            _driverVRCPhysBoneObjField.RegisterValueChangedCallback(evt =>
            {
                UpdateDriverVRCPhysBoneObjHelpboxDisplay();
                VRCPhysBoneConfigChanged?.Invoke();
            });
            objFieldContainer.Add(_driverVRCPhysBoneObjField);
#else
            objFieldContainer.Add(CreateHelpBox(t._("inspector.smartcontrol.driver.vrcPhysBone.helpbox.vrcsdkNotDetected"), MessageType.Error));
#endif

            var paramHelpboxContainer = Q<VisualElement>("vrcphysbone-parameter-prefix-helpbox-container");
            paramHelpboxContainer.Add(CreateHelpBox(t._("inspector.smartcontrol.driver.vrcPhysBone.helpbox.emptyParameterPrefixAutoGenerated"), MessageType.Info));

            _condSrcHelpboxContainer = Q<VisualElement>("vrcphysbone-condition-source-helpbox-container");
            _condSrcHelpboxContainer.Add(CreateHelpBox(t._("inspector.smartcontrol.driver.vrcPhysBone.helpbox.noneCondNoneSrc"), MessageType.Error));

            var conditionChoices = new List<string>()
            {
                t._("inspector.smartcontrol.driver.vrcPhysBone.popup.condition.none"),
                t._("inspector.smartcontrol.driver.vrcPhysBone.popup.condition.grabbed"),
                t._("inspector.smartcontrol.driver.vrcPhysBone.popup.condition.posed"),
                t._("inspector.smartcontrol.driver.vrcPhysBone.popup.condition.grabbedOrPosed")
            };
            var conditionPopupContainer = Q<VisualElement>("vrcphysbone-condition-popup-container");
            _driverVRCPhysBoneConditionPopup = new PopupField<string>(t._("inspector.smartcontrol.driver.vrcPhysBone.popup.condition"), conditionChoices, 0);
            _driverVRCPhysBoneConditionPopup.RegisterValueChangedCallback(evt =>
            {
                UpdateDriverVRCPhysBoneDisplay();
                VRCPhysBoneConfigChanged?.Invoke();
            });
            conditionPopupContainer.Add(_driverVRCPhysBoneConditionPopup);

            var sourceChoices = new List<string>()
            {
                t._("inspector.smartcontrol.driver.vrcPhysBone.popup.source.none"),
                t._("inspector.smartcontrol.driver.vrcPhysBone.popup.source.angle"),
                t._("inspector.smartcontrol.driver.vrcPhysBone.popup.source.stretch"),
                t._("inspector.smartcontrol.driver.vrcPhysBone.popup.source.squish")
            };
            var sourcePopupContainer = Q<VisualElement>("vrcphysbone-source-popup-container");
            _driverVRCPhysBoneSourcePopup = new PopupField<string>(t._("inspector.smartcontrol.driver.vrcPhysBone.popup.source"), sourceChoices, 0);
            _driverVRCPhysBoneSourcePopup.RegisterValueChangedCallback(evt =>
            {
                UpdateDriverVRCPhysBoneDisplay();
                VRCPhysBoneConfigChanged?.Invoke();
            });
            sourcePopupContainer.Add(_driverVRCPhysBoneSourcePopup);
        }

        private void InitDriverFoldout()
        {
            _driverFoldout = Q<Foldout>("driver-foldout");
            _driverContainer = Q<VisualElement>("driver-container");
            BindFoldoutHeaderAndContainerWithPrefix("driver");

            var helpboxContainer = Q<VisualElement>("anim-param-helpbox-container");
            helpboxContainer.Add(CreateHelpBox(t._("inspector.smartcontrol.driver.animatorParameter.helpbox.emptyParameterAutoGenerated"), MessageType.Info));

            _driverTypePopupContainer = Q<VisualElement>("driver-type-popup-container");
            var choices = new List<string>() {
                t._("inspector.smartcontrol.driver.animatorParameter"),
                t._("inspector.smartcontrol.driver.menuItem"),
                t._("inspector.smartcontrol.driver.parameterSlot"),
                t._("inspector.smartcontrol.driver.vrcPhysBone")
                };
            _driverTypePopup = new PopupField<string>(t._("inspector.smartcontrol.driver.popup.driverType"), choices, 0);
            _driverTypePopup.RegisterValueChangedCallback((evt) =>
            {
                UpdateDriverConfigDisplay();
                DriverChanged?.Invoke();
            });
            _driverTypePopupContainer.Add(_driverTypePopup);

            InitDriverMenuItem();
            InitDriverAnimParam();
            InitDriverParameterSlot();
            InitDriverVRCPhysBone();
        }

        private void InitControlFoldout()
        {
            _controlFoldout = Q<Foldout>("control-foldout");
            _controlContainer = Q<VisualElement>("control-container");
            BindFoldoutHeaderAndContainerWithPrefix("control");

            _controlBinaryContainer = Q<VisualElement>("control-binary-container");
            _controlMotionTimeContainer = Q<VisualElement>("control-motion-time-container");

            _controlTypePopupContainer = Q<VisualElement>("control-type-popup-container");
            var choices = new List<string>() { t._("inspector.smartcontrol.control.controlType.binary"), t._("inspector.smartcontrol.control.controlType.motionTime") };
            _controlTypePopupField = new PopupField<string>(t._("inspector.smartcontrol.control.popup.controlType"), choices, 0);
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
            _objectTogglesFoldout = Q<Foldout>("object-toggles-foldout");
            _objectTogglesContainer = Q<VisualElement>("object-toggles-container");
            BindFoldoutHeaderAndContainerWithPrefix("object-toggles");

            var objectTogglesListContainer = Q<VisualElement>("object-toggles-list-container");
            _objectTogglesTableModel = new TableView.TableModel(new string[] {
                t._("inspector.smartcontrol.objectToggles.column.object"),
                t._("inspector.smartcontrol.objectToggles.column.current"),
                t._("inspector.smartcontrol.objectToggles.column.setTo"),
                ""
            });
            _objectTogglesTable = new TableView(_objectTogglesTableModel);
            objectTogglesListContainer.Add(_objectTogglesTable);
            _objectTogglesAddFieldContainer = Q<VisualElement>("object-toggles-add-field-container");
            MakeAddField<Component>(_objectTogglesAddFieldContainer, (comp) => AddObjectToggle?.Invoke(comp));
        }

        private void InitPropertyGroupsFoldout()
        {
            _propGpsFoldout = Q<Foldout>("prop-gps-foldout");
            _propGpsContainer = Q<VisualElement>("prop-gps-container");
            BindFoldoutHeaderAndContainerWithPrefix("prop-gps");

            _propGpsListContainer = Q<VisualElement>("prop-gps-list-container");
            _propGpAddBtn = Q<Button>("prop-gp-add-btn");
            _propGpAddBtn.clicked += () => AddNewPropertyGroup?.Invoke();
        }

        private void InitCrossCtrlValuesFoldout()
        {
            _crossCtrlValuesFoldout = Q<Foldout>("cross-ctrl-values-foldout");
            _crossCtrlValuesContainer = Q<VisualElement>("cross-ctrl-values-container");
            BindFoldoutHeaderAndContainerWithPrefix("cross-ctrl-values");

            _crossCtrlValuesOnEnabledContainer = Q<VisualElement>("cross-ctrl-values-on-enabled-container");
            _crossCtrlValuesOnEnabledAddFieldContainer = Q<VisualElement>("cross-ctrl-values-on-enabled-add-field-container");
            MakeAddField<DTSmartControl>(_crossCtrlValuesOnEnabledAddFieldContainer, (ctrl) => AddCrossControlValueOnEnabled?.Invoke(ctrl));

            _crossCtrlValuesOnDisabledContainer = Q<VisualElement>("cross-ctrl-values-on-disabled-container");
            _crossCtrlValuesOnDisabledAddFieldContainer = Q<VisualElement>("cross-ctrl-values-on-disabled-add-field-container");
            MakeAddField<DTSmartControl>(_crossCtrlValuesOnDisabledAddFieldContainer, (ctrl) => AddCrossControlValueOnDisabled?.Invoke(ctrl));
        }

        private void InitCrossCtrlFoldout()
        {
            _crossCtrlFoldout = Q<Foldout>("cross-ctrl-foldout");
            _crossCtrlContainer = Q<VisualElement>("cross-ctrl-container");
            BindFoldoutHeaderAndContainerWithPrefix("cross-ctrl");

            InitCrossCtrlValuesFoldout();
        }

        private void UpdateDriverConfigDisplay()
        {
            _driverMenuItemContainer.style.display = DisplayStyle.None;
            _driverAnimParamContainer.style.display = DisplayStyle.None;
            _driverVRCPhysBoneContainer.style.display = DisplayStyle.None;
            _driverParamSlotContainer.style.display = DisplayStyle.None;

            if (DriverType == 0)
            {
                // animator parameter
                _driverAnimParamContainer.style.display = DisplayStyle.Flex;
                _controlTypePopupContainer.SetEnabled(true);
            }
            else if (DriverType == 1)
            {
                // menu item
                _driverMenuItemContainer.style.display = DisplayStyle.Flex;
                _driverAnimParamContainer.style.display = DisplayStyle.Flex;
                _controlTypePopupContainer.SetEnabled(false);
                UpdateMenuItemTypeDisplay();
            }
            else if (DriverType == 2)
            {
                // parameter slot
                _driverParamSlotContainer.style.display = DisplayStyle.Flex;
                _controlTypePopupContainer.SetEnabled(false);
                UpdateParamSlot();
            }
            else if (DriverType == 3)
            {
                // vrc physbone
                _driverVRCPhysBoneContainer.style.display = DisplayStyle.Flex;
                UpdateDriverVRCPhysBoneDisplay();
            }
        }

        private void UpdateParamSlot()
        {
            _driverParamSlotMenuItemBox.style.display = _driverParamSlotGenerateMenuItemToggle.value ? DisplayStyle.Flex : DisplayStyle.None;
            _driverParamSlotNotAssignedHelpboxContainer.style.display = ShowParameterSlotNotAssignedHelpbox ? DisplayStyle.Flex : DisplayStyle.None;

            if (_driverParamSlotView != null)
            {
                _driverParamSlotView.Unbind();
                if (_driverParamSlotViewContainer.Contains(_driverParamSlotView))
                {
                    _driverParamSlotViewContainer.Remove(_driverParamSlotView);
                }
            }
            _driverParamSlotViewContainer.Clear();

            if (ParameterSlot != null)
            {
                _driverParamSlotView = new ParameterSlotView(Target) { Target = ParameterSlot };
                _driverParamSlotView.Bind(new SerializedObject(ParameterSlot));
                _driverParamSlotViewContainer.Add(_driverParamSlotView);
            }
        }

        private void SetAnimParamDefaultValueDisplayBool(bool useBool)
        {
            _driverAnimParamDefaultValueBoolToggle.style.display = useBool ? DisplayStyle.Flex : DisplayStyle.None;
            _driverAnimParamDefaultValueFloatContainer.style.display = useBool ? DisplayStyle.None : DisplayStyle.None;
        }

        private void RepaintControlType()
        {
            if (ControlType == 0)
            {
                SetAnimParamDefaultValueDisplayBool(true);

                _controlBinaryContainer.style.display = DisplayStyle.Flex;
                _controlMotionTimeContainer.style.display = DisplayStyle.None;
                _controlFoldout.text = t._("inspector.smartcontrol.foldout.control", t._("inspector.smartcontrol.control.controlType.binary"));

                _objectTogglesFoldout.style.display = DisplayStyle.Flex;
                _objectTogglesContainer.style.display = _objectTogglesFoldout.value ? DisplayStyle.Flex : DisplayStyle.None;

                _propGpsFoldout.style.display = DisplayStyle.Flex;
                _propGpsContainer.style.display = _propGpsFoldout.value ? DisplayStyle.Flex : DisplayStyle.None;

                _crossCtrlFoldout.style.display = DisplayStyle.Flex;
                _crossCtrlContainer.style.display = _crossCtrlFoldout.value ? DisplayStyle.Flex : DisplayStyle.None;
            }
            else if (ControlType == 1)
            {
                SetAnimParamDefaultValueDisplayBool(false);

                _controlBinaryContainer.style.display = DisplayStyle.None;
                _controlMotionTimeContainer.style.display = DisplayStyle.Flex;
                _controlFoldout.text = t._("inspector.smartcontrol.foldout.control", t._("inspector.smartcontrol.control.controlType.motionTime"));

                _objectTogglesFoldout.style.display = DisplayStyle.None;
                _objectTogglesContainer.style.display = DisplayStyle.None;

                _propGpsFoldout.style.display = DisplayStyle.Flex;
                _propGpsContainer.style.display = _propGpsFoldout.value ? DisplayStyle.Flex : DisplayStyle.None;

                _crossCtrlFoldout.style.display = DisplayStyle.None;
                _crossCtrlContainer.style.display = DisplayStyle.None;
            }
        }

        private void RepaintObjectToggles()
        {
            _objectTogglesFoldout.text = t._("inspector.smartcontrol.foldout.objectToggles", ObjectToggles.Count);

            _objectTogglesTableModel.Clear();
            var copy = new List<SmartControlObjectToggleValue>(ObjectToggles);
            for (var i = 0; i < copy.Count; i++)
            {
                var myIdx = i;
                var objToggle = copy[i];

                var nowToggle = new Toggle()
                {
                    value = objToggle.currentEnabled
                };
                nowToggle.RegisterValueChangedCallback((evt) =>
                {
                    objToggle.currentEnabled = nowToggle.value;
                    ChangeObjectToggle?.Invoke(myIdx);
                });

                var objFieldContainer = new VisualElement();
                objFieldContainer.AddToClassList("obj-field-container");

                // TODO: check duplicate entries
                var objField = new ObjectField()
                {
                    objectType = typeof(Component),
                    value = objToggle.target
                };
                objField.RegisterValueChangedCallback((evt) =>
                {
                    objToggle.target = (Component)objField.value;
                    ChangeObjectToggle?.Invoke(myIdx);
                });
                objFieldContainer.Add(objField);

                if (objToggle.sameObjectComponentTypes.Count > 0)
                {
                    var compChoices = new List<string>();
                    var compChoiceIndex = 0;
                    for (var j = 0; j < objToggle.sameObjectComponentTypes.Count; j++)
                    {
                        var compType = objToggle.sameObjectComponentTypes[j];
                        if (objToggle.target.GetType().Name == compType.Name)
                        {
                            compChoiceIndex = j;
                        }
                        compChoices.Add(compType.Name);
                    }
                    var typePopupField = new PopupField<string>(compChoices, compChoiceIndex);
                    objFieldContainer.Add(typePopupField);
                    typePopupField.RegisterValueChangedCallback(evt =>
                    {
                        ChangeObjectToggleComponentType?.Invoke(myIdx, objToggle.sameObjectComponentTypes[typePopupField.index]);
                    });
                }

                var setToToggle = new Toggle()
                {
                    value = objToggle.setToEnabled
                };
                setToToggle.RegisterValueChangedCallback((evt) =>
                {
                    objToggle.setToEnabled = setToToggle.value;
                    ChangeObjectToggle?.Invoke(myIdx);
                });

                var removeBtn = new Button()
                {
                    text = "x"
                };
                removeBtn.clicked += () => RemoveObjectToggle?.Invoke(myIdx);

                _objectTogglesTableModel.AddRow(objFieldContainer, nowToggle, setToToggle, removeBtn);
            }
            _objectTogglesTable.Repaint();
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

            _crossCtrlValuesFoldout.text = t._("inspector.smartcontrol.crossControlActions.foldout.values", total);
            return total;
        }

        private void RepaintCrossControls()
        {
            var total = 0;

            total += RepaintCrossControlValues();

            _crossCtrlFoldout.text = t._("inspector.smartcontrol.foldout.crossControlActions", total);
        }

        private void RepaintPropertyGroups()
        {
            _propGpsFoldout.text = t._("inspector.smartcontrol.foldout.propertyGroups", PropertyGroups.Count);

            // TODO: selectively repaint
            _propGpsListContainer.Clear();
            var copy = new List<DTSmartControl.PropertyGroup>(PropertyGroups);
            for (var i = 0; i < copy.Count; i++)
            {
                var propGp = copy[i];

                var view = new SmartControlPropertyGroupView(this, propGp, t._("inspector.smartcontrol.propertyGroups.label.groupTitle", i + 1, propGp.PropertyValues.Count), () =>
                {
                    RemovePropertyGroup?.Invoke(propGp);
                });

                _propGpsListContainer.Add(view);
            }
        }

        public override void Repaint()
        {
            _driverFoldout.text = t._("inspector.smartcontrol.foldout.driver", _driverTypePopup.text);
            UpdateDriverConfigDisplay();
            RepaintControlType();
            RepaintObjectToggles();
            RepaintPropertyGroups();
            RepaintCrossControls();
        }
    }
}
