/*
 * File: ApplyCabinetHook.cs
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

#if VRC_SDK_VRCSDK3
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Logging;
using UnityEditor;
namespace Chocopoi.DressingTools.Integrations.VRChat
{
    internal class ApplyCabinetHook : IBuildDTCabinetHook
    {
        private DTReport _report;

        private DTCabinet _cabinet;

        public ApplyCabinetHook(DTReport report, DTCabinet cabinet)
        {
            _report = report;
            _cabinet = cabinet;
        }

        public bool OnPreprocessAvatar()
        {
            EditorUtility.DisplayProgressBar("DressingTools", "Applying cabinet...", 0);

            _cabinet.Apply(_report);

            return !_report.HasLogType(DTReportLogType.Error);
        }
    }
}
#endif
