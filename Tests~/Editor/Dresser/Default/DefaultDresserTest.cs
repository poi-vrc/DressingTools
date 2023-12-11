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

using System.Collections.Generic;
using Chocopoi.DressingFramework.Logging;
using Chocopoi.DressingTools.Api.Wearable.Modules.BuiltIn.ArmatureMapping;
using Chocopoi.DressingTools.Dresser;
using Chocopoi.DressingTools.Dresser.Default;
using NUnit.Framework;
using UnityEngine;

namespace Chocopoi.DressingTools.Tests.Dresser.Default
{
    public class DefaultDresserTest : EditorTestBase
    {
        [Test]
        public void NotDTDefaultDresserSettings_ReturnsCorrectErrorCode()
        {
            var dresser = new DefaultDresser();
            var report = dresser.Execute(new DresserSettings(), out var boneMappings);
            Assert.True(report.HasLogCodeByType(DressingFramework.Logging.LogType.Error, DefaultDresser.MessageCode.NotDefaultSettingsSettings));
        }

        [Test]
        public void NullTargetAvatar_ReturnsCorrectErrorCode()
        {
            var wearableRoot = CreateGameObject("Wearable");

            var dresser = new DefaultDresser();
            var settings = new DefaultDresserSettings()
            {
                targetAvatar = null,
                targetWearable = wearableRoot
            };
            var report = dresser.Execute(settings, out var boneMappings);
            Assert.True(report.HasLogCodeByType(DressingFramework.Logging.LogType.Error, DefaultDresser.MessageCode.NullAvatarOrWearable));
        }

        [Test]
        public void NullTargetWearable_ReturnsCorrectErrorCode()
        {
            var avatarRoot = CreateGameObject("Avatar");

            var dresser = new DefaultDresser();
            var settings = new DefaultDresserSettings()
            {
                targetAvatar = avatarRoot,
                targetWearable = null
            };
            var report = dresser.Execute(settings, out var boneMappings);
            Assert.True(report.HasLogCodeByType(DressingFramework.Logging.LogType.Error, DefaultDresser.MessageCode.NullAvatarOrWearable));
        }

        private DKReport EvaluateDresser(GameObject avatarRoot, GameObject wearableRoot, out List<BoneMapping> boneMappings)
        {
            var dresser = new DefaultDresser();
            var settings = new DefaultDresserSettings()
            {
                targetAvatar = avatarRoot,
                targetWearable = wearableRoot,
                avatarArmatureName = "Armature",
                wearableArmatureName = "Armature",
                dynamicsOption = DefaultDresserDynamicsOption.RemoveDynamicsAndUseParentConstraint
            };
            return dresser.Execute(settings, out boneMappings);
        }

        [Test]
        public void AbortOnHookFalse()
        {
            // we create roots with no armature to simulate an error
            var avatarRoot = CreateGameObject("Avatar");
            var wearableRoot = CreateGameObject("Wearable");
            var report = EvaluateDresser(avatarRoot, wearableRoot, out var boneMappings);
            Assert.Null(boneMappings);
            Assert.True(report.HasLogCodeByType(DressingFramework.Logging.LogType.Error, DefaultDresser.MessageCode.HookHasErrors));
        }

        [Test]
        public void NewSettingsTest()
        {
            var dresser = new DefaultDresser();
            Assert.NotNull(dresser.NewSettings());
        }
    }
}
