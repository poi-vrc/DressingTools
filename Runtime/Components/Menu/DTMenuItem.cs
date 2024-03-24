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
using UnityEngine;

namespace Chocopoi.DressingTools.Components.Menu
{
    /// <summary>
    /// DT Menu Item
    /// </summary>
    [AddComponentMenu("DressingTools/DT Menu Item")]
    internal class DTMenuItem : DTBaseComponent
    {
        public enum ItemType
        {
            Button = 0,
            Toggle = 1,
            SubMenu = 2,
            TwoAxis = 3,
            FourAxis = 4,
            Radial = 5
        }

        public enum ItemSubMenuType
        {
            Children = 0,
            DTMenuGroupComponent = 1,
            VRCExpressionsMenuAsset = 100,
            // DTMenuAsset = 101,
        }

        [Serializable]
        public class ItemController
        {
            public enum ControllerType
            {
                AnimatorParameter = 0,
            }

            public ControllerType Type { get => m_Type; set => m_Type = value; }
            public string AnimatorParameterName { get => m_AnimatorParameterName; set => m_AnimatorParameterName = value; }
            public float AnimatorParameterValue { get => m_AnimatorParameterValue; set => m_AnimatorParameterValue = value; }

            [SerializeField] private ControllerType m_Type;
            [SerializeField] private string m_AnimatorParameterName;
            [SerializeField] private float m_AnimatorParameterValue;

            public ItemController()
            {
                m_Type = ControllerType.AnimatorParameter;
                m_AnimatorParameterName = "";
                m_AnimatorParameterValue = 1.0f;
            }
        }

        [Serializable]
        public class Label
        {
            public string Name { get => m_Name; set => m_Name = value; }
            public Texture2D Icon { get => m_Icon; set => m_Icon = value; }

            [SerializeField] private string m_Name;
            [SerializeField] private Texture2D m_Icon;

            public Label()
            {
                m_Name = "";
                m_Icon = null;
            }
        }

        public string Name { get => name; set => name = value; }
        public Texture2D Icon { get => m_Icon; set => m_Icon = value; }
        public ItemType Type { get => m_Type; set => m_Type = value; }
        public ItemController Controller { get => m_Controller; set => m_Controller = value; }
        public ItemController[] SubControllers { get => m_SubControllers; set => m_SubControllers = value; }
        public Label[] SubLabels { get => m_SubLabels; set => m_SubLabels = value; }
        public ItemSubMenuType SubMenuType { get => m_SubMenuType; set => m_SubMenuType = value; }
        public DTMenuGroup DTSubMenu { get => m_DTSubMenu; set => m_DTSubMenu = value; }
#if DT_VRCSDK3A
        public VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu VRCSubMenu { get => m_VRCSubMenu; set => m_VRCSubMenu = value; }
#else
        public ScriptableObject VRCSubMenu { get => m_VRCSubMenu; set => m_VRCSubMenu = value; }
#endif

        // [SerializeField] private string m_Name;
        [SerializeField] private Texture2D m_Icon;
        [SerializeField] private ItemType m_Type;
        [SerializeField] private ItemController m_Controller;
        [SerializeField] private ItemController[] m_SubControllers;
        [SerializeField] private Label[] m_SubLabels;
        [SerializeField] private ItemSubMenuType m_SubMenuType;
        [SerializeField] private DTMenuGroup m_DTSubMenu;
#if DT_VRCSDK3A
        [SerializeField] private VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu m_VRCSubMenu;
#else
        [SerializeField] private ScriptableObject m_VRCSubMenu;
#endif

        public DTMenuItem()
        {
            m_Type = ItemType.Button;
            m_Controller = new ItemController();
            m_SubControllers = new ItemController[0];
            m_SubLabels = new Label[0];
            m_SubMenuType = ItemSubMenuType.Children;
        }
    }
}
