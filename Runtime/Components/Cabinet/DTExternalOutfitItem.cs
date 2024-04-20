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
    /// External outfit item component. This connects to the source item of another outfit and acts like a on-off object of that outfit's related objects and dynamics.
    /// </summary>
    [AddComponentMenu("")]
    internal class DTExternalOutfitItem : DTBaseComponent
    {
        /// <summary>
        /// Source outfit item
        /// </summary>
        public DTOutfitItem SourceItem { get => m_SourceItem; set => m_SourceItem = value; }
        public bool GenerateMenuItem { get => m_GenerateMenuItem; set => m_GenerateMenuItem = value; }

        [SerializeField] private DTOutfitItem m_SourceItem;
        [SerializeField] private bool m_GenerateMenuItem;

        public DTExternalOutfitItem()
        {
            m_SourceItem = null;
            m_GenerateMenuItem = false;
        }
    }
}
