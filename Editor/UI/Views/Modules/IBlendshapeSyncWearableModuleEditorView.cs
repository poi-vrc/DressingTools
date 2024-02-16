/*
 * File: IBlendshapeSyncModuleEditorView.cs
 * Project: DressingTools
 * Created Date: Wednesday, August 9th 2023, 8:34:36 pm
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

namespace Chocopoi.DressingTools.UI.Views.Modules
{
    internal class BlendshapeSyncData
    {
        public bool isAvatarGameObjectInvalid;

        public GameObject avatarGameObject;
        public string[] avatarAvailableBlendshapeNames;
        public int avatarSelectedBlendshapeIndex;
        public float avatarBlendshapeValue;

        public bool isWearableGameObjectInvalid;

        public GameObject wearableGameObject;
        public string[] wearableAvailableBlendshapeNames;
        public int wearableSelectedBlendshapeIndex;
        public float wearableBlendshapeValue;

        public bool inverted;

        public Action avatarGameObjectFieldChangeEvent;
        public Action avatarBlendshapeNameChangeEvent;

        public Action wearableGameObjectFieldChangeEvent;
        public Action wearableBlendshapeNameChangeEvent;

        public Action invertedToggleChangeEvent;

        public Action removeButtonClickEvent;

        public BlendshapeSyncData()
        {
            isAvatarGameObjectInvalid = true;

            avatarGameObject = null;
            avatarAvailableBlendshapeNames = new string[] { "---" };
            avatarSelectedBlendshapeIndex = 0;
            avatarBlendshapeValue = 0;

            isWearableGameObjectInvalid = true;

            wearableGameObject = null;
            wearableAvailableBlendshapeNames = new string[] { "---" };
            wearableSelectedBlendshapeIndex = 0;
            wearableBlendshapeValue = 0;
        }
    }

    internal interface IBlendshapeSyncWearableModuleEditorView : IEditorView
    {
        event Action AddBlendshapeSyncButtonClick;
        GameObject TargetAvatar { get; }
        GameObject TargetWearable { get; }
        bool ShowCannotRenderWithoutTargetAvatarAndWearableHelpBox { get; set; }
        List<BlendshapeSyncData> BlendshapeSyncs { get; set; }
    }
}
