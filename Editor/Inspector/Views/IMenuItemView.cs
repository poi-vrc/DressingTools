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
using Chocopoi.DressingTools.Components.Menu;
using Chocopoi.DressingTools.UI.Views;
using UnityEngine;

namespace Chocopoi.DressingTools.Inspector.Views
{
    internal class AxisLabel
    {
        public string name;
        public Texture2D icon;

        public AxisLabel()
        {
            name = "";
            icon = null;
        }
    }

    internal interface IMenuItemView : IEditorView
    {
        event Action NameChanged;
        event Action TypeChanged;
        event Action IconChanged;
        event Action SubMenuTypeChanged;
        event Action AxisLabelChanged;
        event Action TwoAxisControllerChanged;
        event Action FourAxisControllerChanged;
        event Action RadialControllerChanged;
        event Action InfoParameterFieldsChanged;
        event Action DetailsParameterFieldsChanged;
        event Action SubMenuObjectFieldsChanged;

        DTMenuItem Target { get; set; }
        string Name { get; set; }
        int Type { get; set; }
        Texture2D Icon { get; set; }
        int SubMenuType { get; set; }

        string ParameterName { get; set; }
        float ParameterValue { get; set; }

        AxisLabel AxisUpLabel { get; set; }
        AxisLabel AxisRightLabel { get; set; }
        AxisLabel AxisDownLabel { get; set; }
        AxisLabel AxisLeftLabel { get; set; }

        string HorizontalParameter { get; set; }
        string VerticalParameter { get; set; }

        string UpParameter { get; set; }
        string RightParameter { get; set; }
        string DownParameter { get; set; }
        string LeftParameter { get; set; }

        string RadialParameter { get; set; }

        DTMenuGroup DTSubMenu { get; set; }
#if DT_VRCSDK3A
        VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu VRCSubMenu { get; set; }
#endif

        void SetAnimatorParameterTextFieldAvatarGameObject(GameObject avatarGameObject);
        void RepaintInfoParameterFields();
        void RepaintDetailsParameterFields();
    }
}
