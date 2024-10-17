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

using Chocopoi.DressingTools.Components.Menu;
using Chocopoi.DressingTools.Components.Modifiers;
using UnityEngine;

namespace Chocopoi.DressingTools.Components.Cabinet
{
    /// <summary>
    /// Base outfit component. This component stores items related to the base outfit.
    /// It does not contain any toggles or property groups since they are supposingly applied beforehand.
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("")]
    internal class DTBaseOutfit : DTBaseComponent, IOutfitComponent
    {
        public string Name => "Base";
        public Texture2D Icon { get => m_Icon; set => m_Icon = value; }
        public DTMenuGroup MenuGroup { get => m_MenuGroup; set => m_MenuGroup = value; }
        public DTGroupDynamics GroupDynamics { get => m_GroupDynamics; set => m_GroupDynamics = value; }
        public Transform RootTransform => transform;

        [SerializeField] private Texture2D m_Icon;
        [SerializeField] private DTMenuGroup m_MenuGroup;
        [SerializeField] private DTGroupDynamics m_GroupDynamics;

        public DTBaseOutfit()
        {
            m_Icon = null;
            m_MenuGroup = null;
            m_GroupDynamics = null;
        }
    }
}
