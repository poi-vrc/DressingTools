/*
 * Copyright (c) 2024 chocopoi
 * 
 * This file is part of DressingTools.
 * 
 * DressingTools is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
 * 
 * DressingTools is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License along with DressingTools. If not, see <https://www.gnu.org/licenses/>.
 */

#if DT_VRCSDK3A
using Chocopoi.DressingFramework.Detail.DK;
using Chocopoi.DressingTools.Animations.Passes.VRChat;
using Chocopoi.DressingTools.Components.Animations;
using Chocopoi.DressingTools.Components.OneConf;
using Chocopoi.DressingTools.OneConf.Cabinet;
using Chocopoi.DressingTools.OneConf.Serialization;
using Moq;
using NUnit.Framework;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace Chocopoi.DressingTools.Tests.Animations.Passes
{
    internal class ComposeSmartControlsPassTest : EditorTestBase
    {
        private void SetupEnv(out GameObject avatar, out DKNativeContext ctx, out AnimatorController controller)
        {
            avatar = CreateGameObject("Avatar");
            ctx = new DKNativeContext(avatar);

            controller = new AnimatorController();
            ctx.CreateUniqueAsset(controller, "TestAnimatorController");

            var avatarDesc = avatar.AddComponent<VRCAvatarDescriptor>();
            avatarDesc.baseAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[] {
                new VRCAvatarDescriptor.CustomAnimLayer() {
                    type = VRCAvatarDescriptor.AnimLayerType.FX,
                    isDefault = false,
                    isEnabled = true,
                    animatorController = controller
                }
            };
            avatarDesc.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];

            avatar.AddComponent<DTSmartControl>();
        }

        [Test]
        public void InvokeTest()
        {
            SetupEnv(out _, out var ctx, out _);
            new ComposeSmartControlsPass().Invoke(ctx);
            Assert.False(ctx.Report.HasLogType(DressingFramework.Logging.LogType.Error));
            Assert.False(ctx.Report.HasLogType(DressingFramework.Logging.LogType.Warning));
        }

        private static void MessUpWriteDefaults(AnimatorController ac)
        {
            // let's mess up the write defaults
            ac.AddLayer("SomeLayer");
            var layer = ac.layers[0];
            var state1 = layer.stateMachine.AddState("1");
            state1.writeDefaultValues = true;
            var state2 = layer.stateMachine.AddState("2");
            state2.writeDefaultValues = false;
        }

        [Test]
        public void ExplicitWriteDefaultsTest()
        {
            SetupEnv(out var avatar, out var ctx, out var ac);
            var mock = new Mock<ComposeSmartControlsPass.UI>();
            var pass = new ComposeSmartControlsPass
            {
                ui = mock.Object
            };
            MessUpWriteDefaults(ac);

            // TODO: temporarily store setting here
            var config = new CabinetConfig
            {
                animationWriteDefaultsMode = CabinetConfig.WriteDefaultsMode.Off
            };
            var cabinet = avatar.AddComponent<DTCabinet>();
            cabinet.ConfigJson = CabinetConfigUtility.Serialize(config);

            pass.Invoke(ctx);

            mock.Verify(m => m.ShowInconsistentWriteDefaultsDialog(1, 1), Times.Never);
            Assert.False(ctx.Report.HasLogType(DressingFramework.Logging.LogType.Error));
            Assert.False(ctx.Report.HasLogType(DressingFramework.Logging.LogType.Warning));
        }

        [Test]
        public void InconsistentWriteDefaultsTest()
        {
            SetupEnv(out _, out var ctx, out var ac);
            var mock = new Mock<ComposeSmartControlsPass.UI>();
            var pass = new ComposeSmartControlsPass
            {
                ui = mock.Object
            };
            MessUpWriteDefaults(ac);

            pass.Invoke(ctx);

            mock.Verify(m => m.ShowInconsistentWriteDefaultsDialog(1, 1), Times.Once);
            Assert.False(ctx.Report.HasLogType(DressingFramework.Logging.LogType.Error));
            Assert.False(ctx.Report.HasLogType(DressingFramework.Logging.LogType.Warning));
        }
    }
}
#endif
