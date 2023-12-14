/*
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

using Chocopoi.DressingFramework;
using Chocopoi.DressingFramework.Logging;
using Chocopoi.DressingTools.Api.Cabinet;
using Chocopoi.DressingTools.Dresser;
using NUnit.Framework;

namespace Chocopoi.DressingTools.Tests.Cabinet
{
    public class CabinetApplierTest : EditorTestBase
    {
        private static void ApplyCabinet(DKReport report, DTCabinet cabinet)
        {
            new CabinetApplier(report, cabinet).RunStages();
        }

        [Test]
        public void ApplyErrors_ReturnsCorrectErrorCodes()
        {
            // TODO: dynbone check?
            // This test requires PhysBone
            AssertPassImportedVRCSDK();

            var avatarRoot = InstantiateEditorTestPrefab("DTTest_PhysBoneAvatarWithWearableModuleError.prefab");
            var cabinet = avatarRoot.GetComponent<DTCabinet>();
            Assert.NotNull(cabinet);

            var report = new DKReport();
            ApplyCabinet(report, cabinet);

            Assert.True(report.HasLogCode(DefaultDresser.MessageCode.NoArmatureInWearable), "Should have NoArmatureInWearable error");
            Assert.True(report.HasLogCode(CabinetApplier.MessageCode.WearableHookHasErrors), "Should have WearableHookHasErrors error");
        }
    }
}
