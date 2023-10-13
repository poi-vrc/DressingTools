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
using Chocopoi.DressingTools.Dresser.Default.Hooks;
using NUnit.Framework;
using UnityEngine;

namespace Chocopoi.DressingTools.Tests.Dresser.Default.Hooks
{
    public class NoMissingScriptsHookTest : EditorTestBase
    {
        private bool EvaluateHook(GameObject avatarRoot, GameObject wearableRoot, out DKReport report)
        {
            report = new DKReport();
            var settings = new DefaultDresserSettings();
            var boneMappings = new List<BoneMapping>();
            var hook = new NoMissingScriptsHook();

            settings.targetAvatar = avatarRoot;
            settings.targetWearable = wearableRoot;
            settings.avatarArmatureName = "Armature";
            settings.wearableArmatureName = "Armature";
            settings.dynamicsOption = DefaultDresserDynamicsOption.RemoveDynamicsAndUseParentConstraint;

            return hook.Evaluate(report, settings, boneMappings);
        }

        [Test]
        public void AvatarMissingScripts_ReturnsCorrectErrorCode()
        {
            var avatarRoot = InstantiateEditorTestPrefab("DTTest_MissingScriptsObject.prefab");

            CreateRootWithArmatureAndHipsBone("Wearable", out var wearableRoot, out var wearableArmature, out var wearableHips);

            var result = EvaluateHook(avatarRoot, wearableRoot, out var report);
            Assert.False(result, "Hook should return false");
            Assert.True(report.HasLogCode(DefaultDresser.MessageCode.MissingScriptsDetectedInAvatar));
        }

        [Test]
        public void WearableMissingScripts_ReturnsCorrectErrorCode()
        {
            var wearableRoot = InstantiateEditorTestPrefab("DTTest_MissingScriptsObject.prefab");

            CreateRootWithArmatureAndHipsBone("Avatar", out var avatarRoot, out var avatarArmature, out var avatarHips);

            var result = EvaluateHook(avatarRoot, wearableRoot, out var report);
            Assert.False(result, "Hook should return false");
            Assert.True(report.HasLogCode(DefaultDresser.MessageCode.MissingScriptsDetectedInWearable));
        }
    }
}
