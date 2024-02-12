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
#if DT_VRCSDK3A
using VRC.SDK3.Avatars.ScriptableObjects;
#endif

namespace Chocopoi.DressingTools.Components.Menu
{
    /// <summary>
    /// DT Menu Install
    /// </summary>
    [AddComponentMenu("DressingTools/DT Menu Install (Beta)")]
    internal class DTMenuInstall : DTBaseComponent
    {
        public DTMenuGroup SourceMenuGroup { get => m_SourceMenuGroup; set => m_SourceMenuGroup = value; }
#if DT_VRCSDK3A
        public VRCExpressionsMenu VRCSourceMenu { get => m_VRCSourceMenu; set => m_VRCSourceMenu = value; }
        public VRCExpressionsMenu VRCTargetMenu { get => m_VRCTargetMenu; set => m_VRCTargetMenu = value; }
#else
        public ScriptableObject VRCSourceMenu { get => m_VRCSourceMenu; set => m_VRCSourceMenu = value; }
        public ScriptableObject VRCTargetMenu { get => m_VRCTargetMenu; set => m_VRCTargetMenu = value; }
#endif

        [SerializeField] private DTMenuGroup m_SourceMenuGroup;
#if DT_VRCSDK3A
        [SerializeField] private VRCExpressionsMenu m_VRCSourceMenu;
        [SerializeField] private VRCExpressionsMenu m_VRCTargetMenu;
#else
        [SerializeField] private ScriptableObject m_VRCSourceMenu;
        [SerializeField] private ScriptableObject m_VRCTargetMenu;
#endif
    }
}
