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

using UnityEngine;

namespace Chocopoi.DressingTools.Components.Modifiers
{
    /// <summary>
    /// Copy dynamics component. It is intentionally marked as internal and disabled the add component menu for now.
    /// </summary>
    [AddComponentMenu("")]
    internal class DTCopyDynamics : DTBaseComponent
    {
        public enum DynamicsSearchMode
        {
            ControlRoot = 0,
            ComponentRoot = 1
        }

        public DynamicsSearchMode SearchMode { get => m_SearchMode; set => m_SearchMode = value; }
        public string SearchPath { get => m_SearchPath; set => m_SearchPath = value; }
        public bool SetToCurrentState { get => m_SetToCurrentState; set => m_SetToCurrentState = value; }

        [SerializeField] private DynamicsSearchMode m_SearchMode;
        [SerializeField] private string m_SearchPath;
        [SerializeField] private bool m_SetToCurrentState;

        public DTCopyDynamics()
        {
            m_SearchMode = DynamicsSearchMode.ControlRoot;
            m_SearchPath = "";
            SetToCurrentState = false;
        }

        // just to make the checkbox available
#pragma warning disable UNT0001 // Empty Unity message
        void Start()
#pragma warning restore UNT0001 // Empty Unity message
        {
        }
    }
}
