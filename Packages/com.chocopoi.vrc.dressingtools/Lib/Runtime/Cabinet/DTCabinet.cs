/*
 * File: DTCabinet.cs
 * Project: DressingTools
 * Created Date: Thursday, August 10th 2023, 11:42:41 pm
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

using UnityEngine;

namespace Chocopoi.DressingTools.Lib.Cabinet
{
    [AddComponentMenu("DressingTools/DT Cabinet")]
    [DefaultExecutionOrder(-19999)]
    [ExecuteInEditMode]
    public class DTCabinet : DTBaseComponent
    {
        public GameObject avatarGameObject;
        public string configJson;

        public DTCabinet()
        {
            avatarGameObject = null;
            configJson = null;
        }

        public void Awake()
        {
            OnLifecycle(DTLibRuntimeUtils.LifecycleStage.Awake);
        }

        public void Start()
        {
            OnLifecycle(DTLibRuntimeUtils.LifecycleStage.Start);
        }

        private void OnLifecycle(DTLibRuntimeUtils.LifecycleStage lifecycleStage)
        {
#if UNITY_EDITOR
            var playing = UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode;
#else
            var playing = true;
#endif
            // ensure we are in play mode and not destroyed
            if (playing && this != null)
            {
                DTLibRuntimeUtils.OnCabinetLifecycle(lifecycleStage, this);
            }
        }
    }
}
