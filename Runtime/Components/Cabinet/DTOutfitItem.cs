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
using Chocopoi.DressingTools.Components.Animations;
using Chocopoi.DressingTools.Components.Modifiers;
using UnityEngine;

namespace Chocopoi.DressingTools.Components.Cabinet
{
    /// <summary>
    /// Outfit item component.
    /// A outfit item is a way to expose an outfit's objects and related
    /// dynamics to other outfits for mixed dressing.
    /// </summary>
    [AddComponentMenu("")]
    internal class DTOutfitItem : DTBaseComponent
    {
        [Serializable]
        public class RequiredDynamicsConfig
        {
            public enum DynamicsSearchMode
            {
                ControlRoot = 0,
                ComponentRoot = 1
            }

            public DynamicsSearchMode SearchMode { get => m_SearchMode; set => m_SearchMode = value; }
            public List<Transform> Targets { get => m_Targets; set => m_Targets = value; }

            [SerializeField] private DynamicsSearchMode m_SearchMode;
            [SerializeField] private List<Transform> m_Targets;

            public RequiredDynamicsConfig()
            {
                m_SearchMode = DynamicsSearchMode.ControlRoot;
                m_Targets = new List<Transform>();
            }
        }

        public string Name { get => m_Name; set => m_Name = value; }
        public Texture2D Icon { get => m_Icon; set => m_Icon = value; }
        /// <summary>
        /// Default value of this item. Be careful that this has to be matched with the current state of the outfit. Or you might get unexpected results.
        /// </summary>
        public float DefaultValue { get => m_DefaultValue; set => m_DefaultValue = value; }
        public List<DTSmartControl.ObjectToggle> ObjectToggles { get => m_ObjectToggles; set => m_ObjectToggles = value; }
        public List<DTSmartControl.PropertyGroup> PropertyGroups { get => m_PropertyGroups; set => m_PropertyGroups = value; }
        public DTSmartControl.SCCrossControlActions CrossControlActions { get => m_CrossControlActions; set => m_CrossControlActions = value; }
        public bool GenerateMenuItem { get => m_GenerateMenuItem; set => m_GenerateMenuItem = value; }
        public bool UseRequiredDynamicsOnly { get => m_UseRequiredDynamicsOnly; set => m_UseRequiredDynamicsOnly = value; }
        public RequiredDynamicsConfig RequiredDynamics { get => m_RequiredDynamics; set => m_RequiredDynamics = value; }

        [SerializeField] private string m_Name;
        [SerializeField] private Texture2D m_Icon;
        [SerializeField] private float m_DefaultValue;
        [SerializeField] private List<DTSmartControl.ObjectToggle> m_ObjectToggles;
        [SerializeField] private List<DTSmartControl.PropertyGroup> m_PropertyGroups;
        [SerializeField] private DTSmartControl.SCCrossControlActions m_CrossControlActions;
        [SerializeField] private bool m_GenerateMenuItem;
        [SerializeField] private bool m_UseRequiredDynamicsOnly;
        [SerializeField] private RequiredDynamicsConfig m_RequiredDynamics;

        public DTOutfitItem()
        {
            m_Name = "";
            m_Icon = null;
            m_DefaultValue = 0.0f;
            m_ObjectToggles = new List<DTSmartControl.ObjectToggle>();
            m_PropertyGroups = new List<DTSmartControl.PropertyGroup>();
            m_CrossControlActions = new DTSmartControl.SCCrossControlActions();
            m_UseRequiredDynamicsOnly = false;
            m_RequiredDynamics = new RequiredDynamicsConfig();
        }
    }
}
