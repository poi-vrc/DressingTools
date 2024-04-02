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
using Chocopoi.DressingTools.Components.Menu;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Chocopoi.DressingTools.Components.Animations
{
    /// <summary>
    /// DT Smart Control
    /// 
    /// A general-purpose component to generate animator layers and animations
    /// which are then driven by specific features.
    /// </summary>
    [AddComponentMenu("DressingTools/DT Smart Control")]
    internal partial class DTSmartControl : DTBaseComponent
    {
        public enum SCDriverType
        {
            // 0-99: internal drivers
            AnimatorParameter = 0,
            MenuItem = 1,
            ParameterSlot = 2,

            // 100: reserved
            // 101-199: vrc platform drivers
            VRCPhysBone = 101,
        }

        [Serializable]
        public class ObjectToggle
        {
            public Component Target { get => m_Target; set => m_Target = value; }
            public bool Enabled { get => m_Enabled; set => m_Enabled = value; }

            [SerializeField] private Component m_Target;
            [SerializeField] private bool m_Enabled;
        }

        [Serializable]
        public class PropertyGroup
        {
            [Serializable]
            public class PropertyValue
            {
                public string Name { get => m_Name; set => m_Name = value; }
                public float Value { get => m_Value; set => m_Value = value; }
                public float FromValue { get => m_FromValue; set => m_FromValue = value; }
                public float ToValue { get => m_ToValue; set => m_ToValue = value; }
                public Object ValueObjectReference { get => m_ValueObjectReference; set => m_ValueObjectReference = value; }

                [SerializeField] private string m_Name;
                [SerializeField] private float m_Value;
                [SerializeField] private float m_FromValue;
                [SerializeField] private float m_ToValue;
                [SerializeField] private Object m_ValueObjectReference;

                public PropertyValue()
                {
                    m_Name = "";
                    m_Value = 0.0f;
                    m_ValueObjectReference = null;
                }
            }

            public enum PropertySelectionType
            {
                Normal = 0,
                Inverted = 1,
                AvatarWide = 2
            }

            public Transform SearchTransform { get => m_SearchTransform; set => m_SearchTransform = value; }
            public PropertySelectionType SelectionType { get => m_SelectionType; set => m_SelectionType = value; }
            public List<GameObject> GameObjects { get => m_GameObjects; set => m_GameObjects = value; }
            public List<PropertyValue> PropertyValues { get => m_PropertyValues; set => m_PropertyValues = value; }

            [SerializeField] private Transform m_SearchTransform;
            [SerializeField] private PropertySelectionType m_SelectionType;
            [SerializeField] private List<GameObject> m_GameObjects;
            [SerializeField] private List<PropertyValue> m_PropertyValues;

            public PropertyGroup()
            {
                m_SearchTransform = null;
                m_GameObjects = new List<GameObject>();
                m_PropertyValues = new List<PropertyValue>();
            }
        }

        public enum SCControlType
        {
            Binary = 0,
            MotionTime = 1
        }

        [Serializable]
        public class SCCrossControlActions
        {
            [Serializable]
            public class ControlValueActions
            {
                [Serializable]
                public class ControlValue
                {
                    public DTSmartControl Control { get => m_Control; set => m_Control = value; }
                    public float Value { get => m_Value; set => m_Value = value; }

                    [SerializeField] private DTSmartControl m_Control;
                    [SerializeField] private float m_Value;

                    public ControlValue()
                    {
                        m_Control = null;
                        m_Value = 0.0f;
                    }
                }

                public List<ControlValue> ValuesOnEnable { get => m_ValuesOnEnable; set => m_ValuesOnDisable = value; }
                public List<ControlValue> ValuesOnDisable { get => m_ValuesOnDisable; set => m_ValuesOnDisable = value; }

                [SerializeField] private List<ControlValue> m_ValuesOnEnable;
                [SerializeField] private List<ControlValue> m_ValuesOnDisable;

                public ControlValueActions()
                {
                    m_ValuesOnEnable = new List<ControlValue>();
                    m_ValuesOnDisable = new List<ControlValue>();
                }
            }

            public ControlValueActions ValueActions { get => m_ValueActions; set => m_ValueActions = value; }

            [SerializeField] private ControlValueActions m_ValueActions;

            public SCCrossControlActions()
            {
                m_ValueActions = new ControlValueActions();
            }
        }

        [Serializable]
        public class SCAnimatorConfig
        {
            public string ParameterName { get => m_ParameterName; set => m_ParameterName = value; }
            public float ParameterDefaultValue { get => m_ParameterDefaultValue; set => m_ParameterDefaultValue = value; }
            public bool NetworkSynced { get => m_NetworkSynced; set => m_NetworkSynced = value; }
            public bool Saved { get => m_Saved; set => m_Saved = value; }

            [SerializeField] private string m_ParameterName;
            [SerializeField] private float m_ParameterDefaultValue;
            [SerializeField] private bool m_NetworkSynced;
            [SerializeField] private bool m_Saved;

            public SCAnimatorConfig()
            {
                m_ParameterName = "";
                m_ParameterDefaultValue = 0.0f;
                m_NetworkSynced = true;
                m_Saved = true;
            }
        }

        [Serializable]
        public class SCMenuItemDriverConfig
        {
            public Texture2D ItemIcon { get => m_ItemIcon; set => m_ItemIcon = value; }
            public DTMenuItem.ItemType ItemType { get => m_ItemType; set => m_ItemType = value; }

            [SerializeField] private Texture2D m_ItemIcon;
            [SerializeField] private DTMenuItem.ItemType m_ItemType;

            public SCMenuItemDriverConfig()
            {
                m_ItemIcon = null;
                m_ItemType = DTMenuItem.ItemType.Toggle;
            }
        }

        [Serializable]
        public class SCVRCPhysBoneDriverConfig
        {
            public enum PhysBoneCondition
            {
                None = 0,
                Grabbed = 1,
                Posed = 2,
                GrabbedOrPosed = 3,
            }

            public enum DataSource
            {
                None = 0,
                Angle = 1,
                Stretch = 2,
                Squish = 3,
            }

#if DT_VRCSDK3A
            public VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBone VRCPhysBone { get => m_VRCPhysBone; set => m_VRCPhysBone = value; }
#endif
            public string ParameterPrefix { get => m_ParameterPrefix; set => m_ParameterPrefix = value; }
            public PhysBoneCondition Condition { get => m_Condition; set => m_Condition = value; }
            public DataSource Source { get => m_Source; set => m_Source = value; }

#if DT_VRCSDK3A
            [SerializeField] private VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBone m_VRCPhysBone;
#else
            [SerializeField] private Component m_VRCPhysBone;
#endif
            [SerializeField] private string m_ParameterPrefix;
            [SerializeField] private PhysBoneCondition m_Condition;
            [SerializeField] private DataSource m_Source;

            public SCVRCPhysBoneDriverConfig()
            {
                m_VRCPhysBone = null;
                m_ParameterPrefix = "";
                m_Condition = PhysBoneCondition.Grabbed;
                m_Source = DataSource.None;
            }
        }

        [Serializable]
        public class SCParameterSlotConfig
        {
            public DTParameterSlot ParameterSlot { get => m_ParameterSlot; set => m_ParameterSlot = value; }
            public float MappedValue { get => m_MappedValue; set => m_MappedValue = value; }
            public bool GenerateMenuItem { get => m_GenerateMenuItem; set => m_GenerateMenuItem = value; }
            public Texture2D MenuItemIcon { get => m_MenuItemIcon; set => m_MenuItemIcon = value; }
            public DTMenuItem.ItemType MenuItemType { get => m_MenuItemType; set => m_MenuItemType = value; }

            [SerializeField] private DTParameterSlot m_ParameterSlot;
            [SerializeField] private float m_MappedValue;
            [SerializeField] private bool m_GenerateMenuItem;
            [SerializeField] private Texture2D m_MenuItemIcon;
            [SerializeField] private DTMenuItem.ItemType m_MenuItemType;

            public SCParameterSlotConfig()
            {
                m_ParameterSlot = null;
                m_MappedValue = 0.0f;
                m_GenerateMenuItem = false;
                m_MenuItemIcon = null;
                m_MenuItemType = DTMenuItem.ItemType.Toggle;
            }
        }

        public SCDriverType DriverType { get => m_DriverType; set => m_DriverType = value; }
        public SCControlType ControlType { get => m_ControlType; set => m_ControlType = value; }
        public SCAnimatorConfig AnimatorConfig { get => m_AnimatorConfig; set => m_AnimatorConfig = value; }
        public SCMenuItemDriverConfig MenuItemDriverConfig { get => m_MenuItemDriverConfig; set => m_MenuItemDriverConfig = value; }
        public SCVRCPhysBoneDriverConfig VRCPhysBoneDriverConfig { get => m_VRCPhysBoneDriverConfig; set => m_VRCPhysBoneDriverConfig = value; }
        public SCParameterSlotConfig ParameterSlotConfig { get => m_ParameterSlotConfig; set => m_ParameterSlotConfig = value; }
        public List<ObjectToggle> ObjectToggles { get => m_ObjectToggles; set => m_ObjectToggles = value; }
        public List<PropertyGroup> PropertyGroups { get => m_PropertyGroups; set => m_PropertyGroups = value; }
        public SCCrossControlActions CrossControlActions { get => m_CrossControlActions; set => m_CrossControlActions = value; }

        [SerializeField] private SCDriverType m_DriverType;
        [SerializeField] private SCControlType m_ControlType;
        [SerializeField] private SCAnimatorConfig m_AnimatorConfig;
        [SerializeField] private SCMenuItemDriverConfig m_MenuItemDriverConfig;
        [SerializeField] private SCVRCPhysBoneDriverConfig m_VRCPhysBoneDriverConfig;
        [SerializeField] private SCParameterSlotConfig m_ParameterSlotConfig;
        [SerializeField] private List<ObjectToggle> m_ObjectToggles;
        [SerializeField] private List<PropertyGroup> m_PropertyGroups;
        [SerializeField] private SCCrossControlActions m_CrossControlActions;

        public DTSmartControl()
        {
            m_DriverType = SCDriverType.AnimatorParameter;
            m_ControlType = SCControlType.Binary;
            m_AnimatorConfig = new SCAnimatorConfig();
            m_MenuItemDriverConfig = new SCMenuItemDriverConfig();
            m_VRCPhysBoneDriverConfig = new SCVRCPhysBoneDriverConfig();
            m_ParameterSlotConfig = new SCParameterSlotConfig();
            m_ObjectToggles = new List<ObjectToggle>();
            m_PropertyGroups = new List<PropertyGroup>();
            m_CrossControlActions = new SCCrossControlActions();
        }
    }
}
