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
using UnityEngine;

namespace Chocopoi.DressingTools.Tests.Passes.Modifiers
{
    internal class GroupDynamicsPassTest : EditorTestBase
    {
        private static readonly Type DynamicBoneType = DKEditorUtils.FindType("DynamicBone");
        private static readonly Type PhysBoneType = DKEditorUtils.FindType("VRC.SDK3.Dynamics.PhysBone.Components.VRCPhysBone");

        private void SetupEnv(Type dynamicsType, out GameObject root, out GameObject a, out GameObject aa, out GameObject ab, out GameObject ac, out GameObject ad, out GameObject b, out GameObject ba, out GameObject bb)
        {
            // Root
            // |- A
            // |  |- AA (PB controlled)
            // |  |- AB (PB controlled)
            // |  |- AC (PB controlled)
            // |  |- AD (PB that controls BB)
            // |- B
            // |  |- BA (PB controlled)
            // |  |- BB (controlled by PB in AD)

            root = CreateGameObject("Root");

            a = CreateGameObject("A", root.transform);
            aa = CreateGameObject("AA", a.transform);
            aa.AddComponent(dynamicsType);
            ab = CreateGameObject("AB", a.transform);
            ab.AddComponent(dynamicsType);
            ac = CreateGameObject("AC", a.transform);
            ac.AddComponent(dynamicsType);
            ad = CreateGameObject("AD", a.transform);
            var adDynamics = ad.AddComponent(dynamicsType);

            b = CreateGameObject("B", root.transform);
            ba = CreateGameObject("BA", b.transform);
            ba.AddComponent(dynamicsType);
            bb = CreateGameObject("BB", b.transform);

            // point dynamics of AD to BB
            SingleRootDynamicsProxy dyn;
            if (dynamicsType == DynamicBoneType)
            {
                dyn = new DynamicBoneProxy(adDynamics);
            }
            else if (dynamicsType == PhysBoneType)
            {
                dyn = new PhysBoneProxy(adDynamics);
            }
            else
            {
                Assert.Fail($"Unknown test dynamics type specified: {dynamicsType}");
                return;
            }
            dyn.RootTransform = bb.transform;
        }

        private void ControlRootTest(Type dynamicsType)
        {
            SetupEnv(dynamicsType, out var root, out var a, out var aa, out var ab, out var ac, out var ad, out var _, out var ba, out var bb);

            var c = CreateGameObject("C", root.transform);
            var comp = c.AddComponent<DTGroupDynamics>();
            comp.SearchMode = DTGroupDynamics.DynamicsSearchMode.ControlRoot;
            comp.SeparateGameObjects = true;
            comp.IncludeTransforms.Add(a.transform);
            comp.ExcludeTransforms.Add(ab.transform);

            var ctx = new DKNativeContext(root);
            new GroupDynamicsPass().Invoke(ctx);

            Assert.NotNull(c.transform.Find(aa.name));
            Assert.Null(c.transform.Find(ab.name)); // ignored
            Assert.NotNull(c.transform.Find(ac.name));
            Assert.Null(c.transform.Find(ad.name)); // not controlled

            Assert.Null(c.transform.Find(ba.name)); // not included
            Assert.Null(c.transform.Find(bb.name)); // not included
        }

        [Test]
        public void ControlRootTest_DynBone()
        {
            AssertPassImportedDynamicBone();
            ControlRootTest(DynamicBoneType);
        }

        [Test]
        public void ControlRootTest_PhysBone()
        {
            AssertPassImportedVRCSDK();
            ControlRootTest(PhysBoneType);
        }

        private void ComponentRootTest(Type dynamicsType)
        {
            SetupEnv(dynamicsType, out var root, out var a, out var aa, out var ab, out var ac, out var ad, out var _, out var ba, out var bb);

            var c = CreateGameObject("C", root.transform);
            var comp = c.AddComponent<DTGroupDynamics>();
            comp.SearchMode = DTGroupDynamics.DynamicsSearchMode.ComponentRoot;
            comp.SeparateGameObjects = true;
            comp.IncludeTransforms.Add(a.transform);
            comp.ExcludeTransforms.Add(ab.transform);

            var ctx = new DKNativeContext(root);
            new GroupDynamicsPass().Invoke(ctx);

            Assert.NotNull(c.transform.Find(aa.name));
            Assert.Null(c.transform.Find(ab.name)); // ignored
            Assert.NotNull(c.transform.Find(ac.name));
            Assert.Null(c.transform.Find(ad.name)); // not the actual controlling one
            Assert.NotNull(c.transform.Find(bb.name)); // component root mode

            Assert.Null(c.transform.Find(ba.name)); // not included
        }

        [Test]
        public void ComponentRootTest_DynBone()
        {
            AssertPassImportedDynamicBone();
            ComponentRootTest(DynamicBoneType);
        }

        [Test]
        public void ComponentRootTest_PhysBone()
        {
            AssertPassImportedVRCSDK();
            ComponentRootTest(PhysBoneType);
        }

        private void MultiIncludesTest(Type dynamicsType)
        {
            SetupEnv(dynamicsType, out var root, out var a, out var aa, out var ab, out var ac, out var ad, out var b, out var ba, out var bb);

            var c = CreateGameObject("C", root.transform);
            var comp = c.AddComponent<DTGroupDynamics>();
            comp.SearchMode = DTGroupDynamics.DynamicsSearchMode.ControlRoot;
            comp.SeparateGameObjects = true;
            comp.IncludeTransforms.Add(a.transform);
            comp.IncludeTransforms.Add(b.transform);
            comp.ExcludeTransforms.Add(ab.transform);

            var ctx = new DKNativeContext(root);
            new GroupDynamicsPass().Invoke(ctx);

            Assert.NotNull(c.transform.Find(aa.name));
            Assert.Null(c.transform.Find(ab.name)); // ignored
            Assert.NotNull(c.transform.Find(ac.name));
            Assert.Null(c.transform.Find(ad.name)); // not controlled

            Assert.NotNull(c.transform.Find(ba.name));
            Assert.NotNull(c.transform.Find(bb.name));
        }

        [Test]
        public void MultiIncludesTest_DynBone()
        {
            AssertPassImportedDynamicBone();
            MultiIncludesTest(DynamicBoneType);
        }

        [Test]
        public void MultiIncludesTest_PhysBone()
        {
            AssertPassImportedVRCSDK();
            MultiIncludesTest(PhysBoneType);
        }

        private void MultiExcludesTest(Type dynamicsType)
        {
            SetupEnv(dynamicsType, out var root, out var a, out var aa, out var ab, out var ac, out var ad, out var b, out var ba, out var bb);

            // put a dynamics in root
            root.AddComponent(dynamicsType);

            var c = CreateGameObject("C", root.transform);
            var comp = c.AddComponent<DTGroupDynamics>();
            comp.SearchMode = DTGroupDynamics.DynamicsSearchMode.ControlRoot;
            comp.SeparateGameObjects = true;
            comp.IncludeTransforms.Add(root.transform);
            comp.ExcludeTransforms.Add(a.transform);
            comp.ExcludeTransforms.Add(b.transform);

            var ctx = new DKNativeContext(root);
            new GroupDynamicsPass().Invoke(ctx);

            Assert.NotNull(c.transform.Find(root.name));
        }

        [Test]
        public void MultiExcludesTest_DynBone()
        {
            AssertPassImportedDynamicBone();
            MultiExcludesTest(DynamicBoneType);
        }

        [Test]
        public void MultiExcludesTest_PhysBone()
        {
            AssertPassImportedVRCSDK();
            MultiExcludesTest(PhysBoneType);
        }

        private void SeparateGameObjectsTest(Type dynamicsType)
        {
            SetupEnv(dynamicsType, out var root, out _, out _, out _, out _, out _, out var b, out _, out _);

            var c = CreateGameObject("C", root.transform);
            var comp = c.AddComponent<DTGroupDynamics>();
            comp.SearchMode = DTGroupDynamics.DynamicsSearchMode.ControlRoot;
            comp.SeparateGameObjects = false;
            comp.IncludeTransforms.Add(b.transform);

            var ctx = new DKNativeContext(root);
            new GroupDynamicsPass().Invoke(ctx);

            // BA, BB
            Assert.AreEqual(2, c.GetComponents(dynamicsType).Length);
        }

        [Test]
        public void SeparateGameObjectsTest_DynBone()
        {
            AssertPassImportedDynamicBone();
            SeparateGameObjectsTest(DynamicBoneType);
        }

        [Test]
        public void SeparateGameObjectsTest_PhysBone()
        {
            AssertPassImportedVRCSDK();
            SeparateGameObjectsTest(PhysBoneType);
        }

        private void SetToCurrentStateTest(Type dynamicsType)
        {
            SetupEnv(dynamicsType, out var root, out _, out _, out _, out _, out _, out _, out _, out _);

            var expectedState = false;

            var c = CreateGameObject("C", root.transform);
            var gd = c.AddComponent<DTGroupDynamics>();
            gd.SearchMode = DTGroupDynamics.DynamicsSearchMode.ControlRoot;
            gd.SeparateGameObjects = true;
            gd.SetToCurrentState = true;
            gd.enabled = expectedState; // set them to all off
            gd.IncludeTransforms.Add(root.transform);

            var ctx = new DKNativeContext(root);
            new GroupDynamicsPass().Invoke(ctx);

            Assert.True(dynamicsType.IsSubclassOf(typeof(Behaviour)), "Dynamics type is not subclass of behaviour!");

            var comps = root.GetComponentsInChildren(dynamicsType);
            foreach (var comp in comps)
            {
                if (comp is Behaviour behaviour)
                {
                    Assert.AreEqual(expectedState, behaviour.enabled);
                }
            }
        }

        [Test]
        public void SetToCurrentStateTest_DynBone()
        {
            AssertPassImportedDynamicBone();
            SetToCurrentStateTest(DynamicBoneType);
        }

        [Test]
        public void SetToCurrentStateTest_PhysBone()
        {
            AssertPassImportedVRCSDK();
            SetToCurrentStateTest(PhysBoneType);
        }
    }
}
