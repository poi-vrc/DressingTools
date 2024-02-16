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
#if DT_VRCSDK3A
        public VRCExpressionsMenu VRCSourceMenu { get => m_VRCSourceMenu; set => m_VRCSourceMenu = value; }
#endif
        public string InstallPath { get => m_InstallPath; set => m_InstallPath = value; }

#if DT_VRCSDK3A
        [SerializeField] private VRCExpressionsMenu m_VRCSourceMenu;
#else
        [SerializeField] private ScriptableObject m_VRCSourceMenu;
#endif
        [SerializeField] private string m_InstallPath;
    }
}
