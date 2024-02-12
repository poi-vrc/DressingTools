/*
 * File: IMappingEditorView.cs
 * Project: DressingTools
 * Created Date: Saturday, August 12th 2023, 12:21:25 am
 * Author: chocopoi (poi@chocopoi.com)
 * -----
 * Copyright (c) 2023 chocopoi
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
using UnityEngine;

namespace Chocopoi.DressingTools.UI.Views
{
    internal class ViewBoneMapping
    {
        public bool isInvalid;
        public int mappingType;
        public string wearablePath;
        public GameObject wearableObject;
        public Action MappingChange;
        public Action RemoveMappingButtonClick;

        public ViewBoneMapping()
        {
            isInvalid = true;
            wearablePath = null;
            mappingType = 0;
            wearableObject = null;
        }
    }

    internal class ViewAvatarHierachyNode
    {
        public bool foldout;
        public Transform avatarObjectTransform;
        public List<ViewBoneMapping> wearableMappings;
        public List<ViewAvatarHierachyNode> childs;
        public Action AddMappingButtonClick;

        public ViewAvatarHierachyNode()
        {
            foldout = true;
            avatarObjectTransform = null;
            wearableMappings = new List<ViewBoneMapping>();
            childs = new List<ViewAvatarHierachyNode>();
        }
    }

    internal interface IMappingEditorView : IEditorView
    {
        event Action BoneMappingModeChange;
        event Action BoneMappingDisplayModeChange;

        GameObject TargetAvatar { get; set; }
        GameObject TargetWearable { get; set; }
        int SelectedBoneMappingMode { get; set; }
        int SelectedBoneMappingDisplayMode { get; set; }
        List<ViewAvatarHierachyNode> AvatarHierachyNodes { get; set; }
        bool ShowNoAvatarOrWearableSelectedHelpbox { get; set; }
        bool ShowGeneratedBoneMappingNotAvailableHelpbox { get; set; }
    }
}
