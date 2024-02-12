/*
 * File: IMainView.cs
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
using UnityEngine;

#if UNITY_2021_2_OR_NEWER
using PrefabStage = UnityEditor.SceneManagement.PrefabStage;
#elif UNITY_2018_3_OR_NEWER
using PrefabStage = UnityEditor.Experimental.SceneManagement.PrefabStage;
#else
#error The current Unity version does not support PrefabStage.
#endif

namespace Chocopoi.DressingTools.UI.Views
{
    internal interface IMainView : IEditorView
    {
        event Action UpdateAvailableUpdateButtonClick;
        event Action MouseMove;
        event Action<PrefabStage> PrefabStageClosing;
        event Action<PrefabStage> PrefabStageOpened;

        string UpdateAvailableFromVersion { get; set; }
        string UpdateAvailableToVersion { get; set; }
        bool ShowExitPlayModeHelpbox { get; set; }
        bool ShowExitPrefabModeHelpbox { get; set; }
        int SelectedTab { get; set; }

        void ForceUpdateCabinetSubView();
        void StartDressing(GameObject targetAvatar, GameObject targetWearable = null);
        void OpenUrl(string url);
    }
}
