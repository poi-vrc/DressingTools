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

#if VRC_SDK_VRCSDK3
using Chocopoi.DressingFramework;
using Chocopoi.DressingFramework.Animations;
using Chocopoi.DressingFramework.Context;
using Chocopoi.DressingFramework.Logging;
using Chocopoi.DressingFramework.Serialization;
using Chocopoi.DressingTools.Api.Cabinet;
using Chocopoi.DressingTools.Integrations.VRChat;
using NUnit.Framework;
using VRC.SDK3.Avatars.Components;

namespace Chocopoi.DressingTools.Tests.Integrations.VRChat
{
    public class VRCScanAnimationsCabinetHookTest : EditorTestBase
    {
        [Test]
        public void ScanAnimTest()
        {
            var prefab = InstantiateEditorTestPrefab("VRCScanAnimAvatar.prefab");

            var cabinetComp = prefab.GetComponent<DTCabinet>();
            Assert.NotNull(cabinetComp);

            var avatarDesc = prefab.GetComponent<VRCAvatarDescriptor>();
            Assert.NotNull(avatarDesc);

            // shadow copy for checking later
            var baseAnimLayers = new VRCAvatarDescriptor.CustomAnimLayer[avatarDesc.baseAnimationLayers.Length];
            avatarDesc.baseAnimationLayers.CopyTo(baseAnimLayers, 0);

            // shadow copy for checking later
            var specAnimLayers = new VRCAvatarDescriptor.CustomAnimLayer[avatarDesc.specialAnimationLayers.Length];
            avatarDesc.specialAnimationLayers.CopyTo(specAnimLayers, 0);

            var cabCtx = new ApplyCabinetContext()
            {
                report = new DKReport(),
                avatarGameObject = prefab,
                cabinetConfig = CabinetConfigUtility.Deserialize(cabinetComp.configJson),
                avatarDynamics = DKEditorUtils.ScanDynamics(prefab, true),
                pathRemapper = new PathRemapper(prefab)
            };
            cabCtx.animationStore = new AnimationStore(cabCtx);

            var hook = new VRCScanAnimationsCabinetHook();
            Assert.True(hook.Invoke(cabCtx));

            // assert that we have DTTest_Clip1 to 13 in the animation store
            for (var i = 1; i <= 13; i++)
            {
                bool found = false;
                var expectedName = $"DTTest_Clip{i}";
                foreach (var clip in cabCtx.animationStore.Clips)
                {
                    Assert.NotNull(clip.originalClip);
                    Assert.Null(clip.newClip);
                    Assert.NotNull(clip.dispatchFunc);
                    if (clip.originalClip.name == expectedName)
                    {
                        found = true;
                        break;
                    }
                }
                Assert.True(found, "Could not find " + expectedName);
            }

            // check if the all layers are not the same as original and made a copy
            for (var i = 0; i < baseAnimLayers.Length; i++)
            {
                Assert.AreNotEqual(baseAnimLayers[i], avatarDesc.baseAnimationLayers[i], $"Base anim layer {i} are equal");
            }
            for (var i = 0; i < specAnimLayers.Length; i++)
            {
                Assert.AreNotEqual(specAnimLayers[i], avatarDesc.specialAnimationLayers[i], $"Special anim layer {i} are equal");
            }
        }
    }
}
#endif
