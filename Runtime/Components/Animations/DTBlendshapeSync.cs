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
using UnityEngine;

namespace Chocopoi.DressingTools.Components.Animations
{
    /// <summary>
    /// Blendshape sync component. It is intentionally marked as internal and disabled the add component menu for now.
    /// </summary>
    [AddComponentMenu("")]
    internal class DTBlendshapeSync : DTBaseComponent
    {
        public class Entry
        {
            /// <summary>
            /// Source SMR object path
            /// </summary>
            public string SourcePath { get => m_SourcePath; set => m_SourcePath = value; }

            /// <summary>
            /// Source SMR blendshape name to sync
            /// </summary>
            public string SourceBlendshape { get => m_SourceBlendshape; set => m_SourceBlendshape = value; }

            /// <summary>
            /// Destination SMR object path
            /// </summary>
            public SkinnedMeshRenderer DestinationSkinnedMeshRenderer { get => m_DestinationSkinnedMeshRenderer; set => m_DestinationSkinnedMeshRenderer = value; }

            /// <summary>
            /// Destination SMR blendshape name to sync
            /// </summary>
            public string DestinationBlendshape { get => m_DestinationBlendshape; set => m_DestinationBlendshape = value; }

            [SerializeField] private string m_SourcePath;
            [SerializeField] private string m_SourceBlendshape;
            [SerializeField] private SkinnedMeshRenderer m_DestinationSkinnedMeshRenderer;
            [SerializeField] private string m_DestinationBlendshape;

            public Entry()
            {
                m_SourcePath = "";
                m_SourceBlendshape = "";
                m_DestinationSkinnedMeshRenderer = null;
                m_DestinationBlendshape = "";
            }
        }

        public List<Entry> Entries { get => m_Entries; set => m_Entries = value; }

        [SerializeField] private List<Entry> m_Entries;

        public DTBlendshapeSync()
        {
            m_Entries = new List<Entry>();
        }
    }
}
