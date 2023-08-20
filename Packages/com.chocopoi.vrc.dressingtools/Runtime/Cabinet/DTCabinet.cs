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

using System;
using Chocopoi.DressingTools.Lib.Cabinet;
using Chocopoi.DressingTools.Lib.Logging;
using Chocopoi.DressingTools.Logging;
using UnityEngine;

namespace Chocopoi.DressingTools.Cabinet
{
    [AddComponentMenu("DressingTools/DT Cabinet")]
    [DefaultExecutionOrder(-19999)]
    public class DTCabinet : DTBaseComponent, ICabinet
    {
        private const string LogLabel = "DTCabinet";

        [field: SerializeField] public GameObject AvatarGameObject { get; set; }
        [field: SerializeField] public string AvatarArmatureName { get; set; }
        [field: SerializeField] public bool GroupDynamics { get; set; }
        [field: SerializeField] public bool GroupDynamicsSeparateGameObjects { get; set; }

        public DTCabinet()
        {
            // TOOD: Read default settings?
            AvatarGameObject = null;
            AvatarArmatureName = "Armature";
            GroupDynamics = true;
            GroupDynamicsSeparateGameObjects = true;
        }

        public DTCabinetWearable[] GetWearables()
        {
            if (AvatarGameObject == null)
            {
                return new DTCabinetWearable[0];
            }
            return AvatarGameObject.GetComponentsInChildren<DTCabinetWearable>();
        }

        public void Apply(DTReport report)
        {
            try
            {
                new CabinetApplier(report, this).Execute();
            }
            catch (Exception ex)
            {
                DTReportUtils.LogExceptionLocalized(report, LogLabel, ex, "cabinet.apply.msgCode.hasException");
            }
        }

        void Start()
        {
            // TODO: the report shouldn't put here
            Apply(new DTReport());
        }
    }
}
