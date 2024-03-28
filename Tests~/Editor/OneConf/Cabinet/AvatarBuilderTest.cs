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

using Chocopoi.DressingFramework.Detail.DK;
using Chocopoi.DressingFramework.Detail.DK.Logging;
using Chocopoi.DressingTools.Components.OneConf;
using NUnit.Framework;

namespace Chocopoi.DressingTools.Tests.OneConf.Cabinet
{
    internal class AvatarBuilderTest : EditorTestBase
    {
        [Test]
        public void ApplyErrors_ReturnsCorrectErrorCodes()
        {
            // TODO: dynbone check?
            // This test requires PhysBone
            AssertPassImportedVRCSDK();

            var avatarRoot = InstantiateEditorTestPrefab("DTTest_PhysBoneAvatarWithWearableModuleError.prefab");
            var cabinet = avatarRoot.GetComponent<DTCabinet>();
            Assert.NotNull(cabinet);

            var ab = new AvatarBuilder(avatarRoot);
            ab.RunStages();
            var report = (DKReport)ab.Context.Report;

            // Assert.True(report.HasLogCode(DefaultDresser.MessageCode.NoArmatureInWearable), "Should have NoArmatureInWearable error");
            Assert.True(report.HasLogCode(AvatarBuilder.MessageCode.PassHasErrors), "Should have PassHasError error");
        }
    }
}
