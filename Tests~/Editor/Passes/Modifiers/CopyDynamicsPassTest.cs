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

using System;
using Chocopoi.DressingFramework;
using Chocopoi.DressingFramework.Detail.DK;
using Chocopoi.DressingTools.Components.Modifiers;
using Chocopoi.DressingTools.Dynamics.Proxy;
using Chocopoi.DressingTools.Passes.Modifiers;
using NUnit.Framework;

namespace Chocopoi.DressingTools.Tests.Passes.Modifiers
{
    internal class CopyDynamicsPassTest : EditorTestBase
    {
        private static readonly Type DynamicBoneType = DKEditorUtils.FindType("DynamicBone");
        private static readonly Type PhysBoneType = DKEditorUtils.FindType("VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBone");

        public void InvokeTest(Type componentType)
        {
            var pass = new CopyDynamicsPass();
            var avatar = CreateGameObject("Avatar");
            var ctx = new DKNativeContext(avatar);

            var a = CreateGameObject("A", avatar.transform);
            var b = CreateGameObject("B", avatar.transform);

            var dynComp = a.AddComponent(componentType);
            SingleRootDynamicsProxy dynamics;
            if (componentType == DynamicBoneType)
            {
                dynamics = new DynamicBoneProxy(dynComp);
            }
            else if (componentType == PhysBoneType)
            {
                dynamics = new PhysBoneProxy(dynComp);
            }
            else
            {
                Assert.Fail("unknown type");
                return;
            }
            dynamics.RootTransform = a.transform;

            var copyDynComp = b.AddComponent<DTCopyDynamics>();
            copyDynComp.SourceSearchMode = DTCopyDynamics.DynamicsSearchMode.ControlRoot;
            copyDynComp.SourcePath = "A";

            pass.Invoke(ctx);

            Assert.True(b.TryGetComponent(componentType, out _));
        }

        [Test]
        public void InvokeTest_DynBone()
        {
            AssertPassImportedDynamicBone();
            InvokeTest(DynamicBoneType);
        }

        [Test]
        public void InvokeTest_PhysBone()
        {
            AssertPassImportedVRCSDK();
            InvokeTest(PhysBoneType);
        }
    }
}
