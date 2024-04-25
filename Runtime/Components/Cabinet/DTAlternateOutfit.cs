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

using System.Collections.Generic;
using Chocopoi.DressingTools.Components.Animations;
using Chocopoi.DressingTools.Components.Menu;
using Chocopoi.DressingTools.Components.Modifiers;
using UnityEngine;

namespace Chocopoi.DressingTools.Components.Cabinet
{
    [AddComponentMenu("")]
    internal class DTAlternateOutfit : DTBaseComponent, IOutfit
    {
        public string Name { get => name; set => name = value; }
        public Texture2D Icon { get => m_Icon; set => m_Icon = value; }
        public DTMenuGroup MenuGroup { get => m_MenuGroup; set => m_MenuGroup = value; }
        public List<DTSmartControl.ObjectToggle> ObjectToggles { get => m_ObjectToggles; set => m_ObjectToggles = value; }
        public List<DTSmartControl.PropertyGroup> PropertyGroups { get => m_PropertyGroups; set => m_PropertyGroups = value; }
        public DTSmartControl.SCCrossControlActions CrossControlActions { get => m_CrossControlActions; set => m_CrossControlActions = value; }
        public DTGroupDynamics GroupDynamics { get => m_GroupDynamics; set => m_GroupDynamics = value; }
        public Transform RootTransform => transform;

        [SerializeField] private Texture2D m_Icon;
        [SerializeField] private DTMenuGroup m_MenuGroup;
        [SerializeField] private List<DTSmartControl.ObjectToggle> m_ObjectToggles;
        [SerializeField] private List<DTSmartControl.PropertyGroup> m_PropertyGroups;
        [SerializeField] private DTSmartControl.SCCrossControlActions m_CrossControlActions;
        [SerializeField] private DTGroupDynamics m_GroupDynamics;

        public DTAlternateOutfit()
        {
            m_Icon = null;
            m_MenuGroup = null;
            m_ObjectToggles = new List<DTSmartControl.ObjectToggle>();
            m_PropertyGroups = new List<DTSmartControl.PropertyGroup>();
            m_CrossControlActions = new DTSmartControl.SCCrossControlActions();
            m_GroupDynamics = null;
        }
    }
}
