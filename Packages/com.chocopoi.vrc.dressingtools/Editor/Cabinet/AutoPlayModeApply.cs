/*
 * File: DTCabinetApplier.cs
 * Project: DressingTools
 * Created Date: Saturday, July 29th 2023, 10:31:11 am
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
using Chocopoi.DressingTools.Lib;
using Chocopoi.DressingTools.Lib.Cabinet;
using Chocopoi.DressingTools.Lib.Logging;
using Chocopoi.DressingTools.UI;
using UnityEditor;

namespace Chocopoi.DressingTools.Cabinet
{
    [InitializeOnLoad]
    internal static class AutoPlayModeApply
    {
        private static DTLibRuntimeUtils.LifecycleStage s_applyLifeCycle = DTLibRuntimeUtils.LifecycleStage.Awake;

        static AutoPlayModeApply()
        {
            DTLibRuntimeUtils.OnCabinetLifecycle = OnCabinetLifecycle;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnCabinetLifecycle(DTLibRuntimeUtils.LifecycleStage stage, DTCabinet cabinet)
        {
            if (s_applyLifeCycle == stage)
            {
                var report = new DTReport();
                new CabinetApplier(report, cabinet).Execute();
                if (report.HasLogType(DTReportLogType.Error))
                {
                    ReportWindow.ShowWindow(report);
                }
            }
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            switch (change)
            {
                case PlayModeStateChange.EnteredPlayMode:
                    s_applyLifeCycle = DTLibRuntimeUtils.LifecycleStage.Start;
                    break;
                case PlayModeStateChange.EnteredEditMode:
                    s_applyLifeCycle = DTLibRuntimeUtils.LifecycleStage.Awake;
                    break;
            }
        }
    }
}
