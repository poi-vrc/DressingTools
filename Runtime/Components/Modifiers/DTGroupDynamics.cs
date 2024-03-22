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

namespace Chocopoi.DressingTools.Components.Modifiers
{
    /// <summary>
    /// DT Group Dynamics
    /// 
    /// A component to group found dynamics to a single GameObject and animate them.
    /// </summary>
    ///
    [AddComponentMenu("DressingTools/DT Group Dynamics")]
    internal class DTGroupDynamics : DTBaseComponent
    {
        public enum DynamicsSearchMode
        {
            ControlRoot = 0,
            ComponentRoot = 1
        }

        public DynamicsSearchMode SearchMode { get => m_SearchMode; set => m_SearchMode = value; }
        public List<Transform> IncludeTransforms { get => m_IncludeTransforms; set => m_IncludeTransforms = value; }
        public List<Transform> ExcludeTransforms { get => m_ExcludeTransforms; set => m_ExcludeTransforms = value; }
        public bool SeparateGameObjects { get => m_SeparateGameObjects; set => m_SeparateGameObjects = value; }
        public bool SetToCurrentState { get => m_SetToCurrentState; set => m_SetToCurrentState = value; }

        [SerializeField] private DynamicsSearchMode m_SearchMode;
        [SerializeField] private List<Transform> m_IncludeTransforms;
        [SerializeField] private List<Transform> m_ExcludeTransforms;
        [SerializeField] private bool m_SeparateGameObjects;
        [SerializeField] private bool m_SetToCurrentState;

        public DTGroupDynamics()
        {
            SearchMode = DynamicsSearchMode.ControlRoot;
            IncludeTransforms = new List<Transform>();
            ExcludeTransforms = new List<Transform>();
            SeparateGameObjects = true;
            SetToCurrentState = true;
        }

        // just to make the checkbox available
#pragma warning disable UNT0001 // Empty Unity message
        void Start()
#pragma warning restore UNT0001 // Empty Unity message
        {
        }
    }
}
