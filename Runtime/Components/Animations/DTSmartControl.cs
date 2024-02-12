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
using UnityEngine;
using Object = UnityEngine.Object;

namespace Chocopoi.DressingTools.Components.Animations
{
    /// <summary>
    /// DT Smart Control
    /// 
    /// An experimental, general-purpose component to generate animator layers and animations
    /// which are then driven by specific features.
    /// </summary>
    [AddComponentMenu("DressingTools/DT Smart Control (Beta)")]
    internal partial class DTSmartControl : DTBaseComponent
    {
        public enum SmartControlDriverType
        {
            AnimatorParameter = 0,
            MenuItem = 1
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
                public Object ValueObjectReference { get => m_ValueObjectReference; set => m_ValueObjectReference = value; }

                [SerializeField] private string m_Name;
                [SerializeField] private float m_Value;
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
                Inverted = 1
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

        public enum SmartControlControlType
        {
            Binary = 0,
            MotionTime = 1
        }

        [Serializable]
        public class SmartControlCrossControlActions
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

            public SmartControlCrossControlActions()
            {
                m_ValueActions = new ControlValueActions();
            }
        }

        [Serializable]
        public class SmartControlAnimatorConfig
        {
            public string ParameterName { get => m_ParameterName; set => m_ParameterName = value; }
            public float ParameterDefaultValue { get => m_ParameterDefaultValue; set => m_ParameterDefaultValue = value; }
            public bool NetworkSynced { get => m_NetworkSynced; set => m_NetworkSynced = value; }
            public bool Saved { get => m_Saved; set => m_Saved = value; }

            [SerializeField] private string m_ParameterName;
            [SerializeField] private float m_ParameterDefaultValue;
            [SerializeField] private bool m_NetworkSynced;
            [SerializeField] private bool m_Saved;

            public SmartControlAnimatorConfig()
            {
                m_ParameterName = "";
                m_ParameterDefaultValue = 0.0f;
                m_NetworkSynced = true;
                m_Saved = true;
            }
        }

        public SmartControlDriverType DriverType { get => m_DriverType; set => m_DriverType = value; }
        public SmartControlControlType ControlType { get => m_ControlType; set => m_ControlType = value; }
        public AnimationCurve Curve { get => m_Curve; set => m_Curve = value; }
        public SmartControlAnimatorConfig AnimatorConfig { get => m_AnimatorConfig; set => m_AnimatorConfig = value; }
        public List<ObjectToggle> ObjectToggles { get => m_ObjectToggles; set => m_ObjectToggles = value; }
        public List<PropertyGroup> PropertyGroups { get => m_PropertyGroups; set => m_PropertyGroups = value; }
        public SmartControlCrossControlActions CrossControlActions { get => m_CrossControlActions; set => m_CrossControlActions = value; }

        [SerializeField] private SmartControlDriverType m_DriverType;
        [SerializeField] private SmartControlControlType m_ControlType;
        [SerializeField] private AnimationCurve m_Curve;
        [SerializeField] private SmartControlAnimatorConfig m_AnimatorConfig;
        [SerializeField] private List<ObjectToggle> m_ObjectToggles;
        [SerializeField] private List<PropertyGroup> m_PropertyGroups;
        [SerializeField] private SmartControlCrossControlActions m_CrossControlActions;

        public DTSmartControl()
        {
            m_DriverType = SmartControlDriverType.AnimatorParameter;
            m_ControlType = SmartControlControlType.Binary;
            m_Curve = AnimationCurve.Linear(0.0f, 0.0f, 1.0f, 1.0f);
            m_AnimatorConfig = new SmartControlAnimatorConfig();
            m_ObjectToggles = new List<ObjectToggle>();
            m_PropertyGroups = new List<PropertyGroup>();
            m_CrossControlActions = new SmartControlCrossControlActions();
        }
    }
}
