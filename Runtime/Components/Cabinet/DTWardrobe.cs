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
using UnityEngine;

namespace Chocopoi.DressingTools.Components.Cabinet
{
    [AddComponentMenu("")]
    internal class DTWardrobe : DTBaseComponent
    {
        [Serializable]
        public class OutfitPreset
        {
            public string Name { get => m_Name; set => m_Name = value; }
            public List<DTSmartControl.ObjectToggle> ObjectToggles { get => m_ObjectToggles; set => m_ObjectToggles = value; }
            public List<DTSmartControl.PropertyGroup> PropertyGroups { get => m_PropertyGroups; set => m_PropertyGroups = value; }

            [SerializeField] private string m_Name;
            [SerializeField] private List<DTSmartControl.ObjectToggle> m_ObjectToggles;
            [SerializeField] private List<DTSmartControl.PropertyGroup> m_PropertyGroups;

            public OutfitPreset()
            {
                m_Name = "";
                m_ObjectToggles = new List<DTSmartControl.ObjectToggle>();
                m_PropertyGroups = new List<DTSmartControl.PropertyGroup>();
            }
        }

        /// <summary>
        /// Make this wardrobe to be a menu group root. When disabled, the wardrobe menu is directly installed using the DK API.
        /// For compatibility reasons, this option is off when converting from OneConf cabinet, since
        /// the wardrobe will be placed on the avatar root.
        /// </summary>
        public bool UseAsMenuGroup { get => m_UseAsMenuGroup; set => m_UseAsMenuGroup = value; }
        /// <summary>
        /// Generate a sub-menu item with the specified item name and icon. This option has no effect if not used as menu group.
        /// </summary>
        public bool GenerateSubMenuItem { get => m_GenerateSubMenuItem; set => m_GenerateSubMenuItem = value; }
        /// <summary>
        /// Menu install path. This option has no effect if used as menu group.
        /// </summary>
        public string MenuInstallPath { get => m_MenuInstallPath; set => m_MenuInstallPath = value; }
        /// <summary>
        /// Menu item name. This option only has effect if not used as menu group, or used as menu group with generate sub-menu item on.
        /// </summary>
        public string MenuItemName { get => m_MenuItemName; set => m_MenuItemName = value; }
        /// <summary>
        /// Menu item icon. This option only has effect if not used as menu group, or used as menu group with generate sub-menu item on.
        /// </summary>
        public Texture2D MenuItemIcon { get => m_MenuItemIcon; set => m_MenuItemIcon = value; }
        public bool NetworkSynced { get => m_NetworkSynced; set => m_NetworkSynced = value; }
        public bool Saved { get => m_Saved; set => m_Saved = value; }
        public bool ResetControlsOnSwitch { get => m_ResetControlsOnSwitch; set => m_ResetControlsOnSwitch = value; }
        public List<OutfitPreset> OutfitPresets { get => m_OutfitPresets; set => m_OutfitPresets = value; }

        [SerializeField] private bool m_UseAsMenuGroup;
        [SerializeField] private bool m_GenerateSubMenuItem;
        [SerializeField] private string m_MenuInstallPath;
        [SerializeField] private string m_MenuItemName;
        [SerializeField] private Texture2D m_MenuItemIcon;
        [SerializeField] private bool m_NetworkSynced;
        [SerializeField] private bool m_Saved;
        [SerializeField] private bool m_ResetControlsOnSwitch;
        [SerializeField] private List<OutfitPreset> m_OutfitPresets;

        public DTWardrobe()
        {
            m_UseAsMenuGroup = true;
            m_GenerateSubMenuItem = true;
            m_MenuInstallPath = "";
            m_NetworkSynced = true;
            m_Saved = true;
            m_ResetControlsOnSwitch = true;
            m_OutfitPresets = new List<OutfitPreset>();
        }
    }
}
