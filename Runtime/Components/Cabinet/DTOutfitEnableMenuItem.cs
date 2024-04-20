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

namespace Chocopoi.DressingTools.Components.Cabinet
{
    /// <summary>
    /// Outfit enable menu item component.
    /// This acts as a placeholder for the outfit enable toggle in the menu.
    /// </summary>
    [AddComponentMenu("")]
    internal class DTOutfitEnableMenuItem : DTBaseComponent
    {
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get => name; set => name = value; }

        /// <summary>
        /// Icon
        /// </summary>
        public Texture2D Icon { get => m_Icon; set => m_Icon = value; }

        /// <summary>
        /// Target alternate outfit. If it is null, it will control the base outfit instead.
        /// </summary>
        public DTAlternateOutfit TargetOutfit { get => m_TargetOutfit; set => m_TargetOutfit = value; }

        [SerializeField] private Texture2D m_Icon;
        [SerializeField] private DTAlternateOutfit m_TargetOutfit;

        public DTOutfitEnableMenuItem()
        {
            m_Icon = null;
        }
    }
}
