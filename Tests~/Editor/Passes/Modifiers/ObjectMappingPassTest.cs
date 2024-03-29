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

using Chocopoi.DressingFramework;
using Chocopoi.DressingFramework.Detail.DK;
using Chocopoi.DressingTools.Components.Modifiers;
using Chocopoi.DressingTools.Passes.Modifiers;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Animations;

namespace Chocopoi.DressingTools.Tests.Passes.Modifiers
{
    internal class ObjectMappingPassTest : EditorTestBase
    {
        private void SetupEnv(out Context ctx, out ObjectMappingPass pass, out DTObjectMapping comp, out GameObject a, out GameObject aa, out GameObject ab, out GameObject b, out GameObject ba, out GameObject bb, out GameObject bc)
        {
            pass = new ObjectMappingPass();
            var avatar = CreateGameObject("Avatar");
            ctx = new DKNativeContext(avatar);

            a = CreateGameObject("A", avatar.transform);
            aa = CreateGameObject("AA", a.transform);
            ab = CreateGameObject("AB", a.transform);
            b = CreateGameObject("B", avatar.transform);
            ba = CreateGameObject("BA", b.transform);
            bb = CreateGameObject("BB", b.transform);
            bc = CreateGameObject("BC", b.transform);

            comp = b.AddComponent<DTObjectMapping>();
            comp.Mappings.Add(new DTObjectMapping.Mapping()
            {
                Type = DTObjectMapping.Mapping.MappingType.MoveToBone,
                SourceTransform = ba.transform,
                TargetPath = "A/AA"
            });
            comp.Mappings.Add(new DTObjectMapping.Mapping()
            {
                Type = DTObjectMapping.Mapping.MappingType.ParentConstraint,
                SourceTransform = bb.transform,
                TargetPath = "A/AB"
            });
            comp.Mappings.Add(new DTObjectMapping.Mapping()
            {
                Type = DTObjectMapping.Mapping.MappingType.DoNothing,
                SourceTransform = bc.transform,
            });
        }

        [Test]
        public void MappingTest()
        {
            SetupEnv(out var ctx, out var pass, out var comp, out _, out var aa, out var ab, out var b, out var ba, out var bb, out var bc);

            comp.GroupObjects = false;
            pass.Invoke(ctx);

            // BA->AA
            Assert.AreEqual(aa.transform, ba.transform.parent);

            // BB->AB via PC
            Assert.AreEqual(b.transform, bb.transform.parent);
            Assert.True(bb.TryGetComponent<ParentConstraint>(out var pc));
            Assert.AreEqual(1, pc.sourceCount);
            Assert.AreEqual(ab.transform, pc.GetSource(0).sourceTransform);

            // BC nothing
            Assert.AreEqual(b.transform, bc.transform.parent);
            Assert.False(bc.TryGetComponent<ParentConstraint>(out _));
        }

        [Test]
        public void GroupObjectsTest()
        {
            SetupEnv(out var ctx, out var pass, out var comp, out _, out var aa, out _, out _, out var ba, out _, out _);

            comp.GroupObjects = true;
            pass.Invoke(ctx);

            var container = aa.transform.Find("AA_DT");
            Assert.NotNull(container);
            Assert.AreEqual(container.transform, ba.transform.parent);
        }

        [Test]
        public void PrefixSuffixTest()
        {
            SetupEnv(out var ctx, out var pass, out var comp, out _, out _, out _, out _, out var ba, out _, out _);

            comp.GroupObjects = false;
            comp.PreventDuplicateNames = false;
            comp.Prefix = "Abc ";
            comp.Suffix = " efG";

            pass.Invoke(ctx);

            Assert.AreEqual("Abc BA efG", ba.name);
        }

        [Test]
        public void DuplicateNameTest()
        {
            SetupEnv(out var ctx, out var pass, out var comp, out _, out _, out _, out var b, out var ba, out _, out _);

            comp.GroupObjects = false;
            comp.PreventDuplicateNames = true;

            // add an object with same name as BA
            var duplicateObj = CreateGameObject("BA", b.transform);
            comp.Mappings.Add(new DTObjectMapping.Mapping()
            {
                Type = DTObjectMapping.Mapping.MappingType.MoveToBone,
                SourceTransform = duplicateObj.transform,
                TargetPath = "A/AA"
            });

            pass.Invoke(ctx);

            Debug.Log($"{ba.name} {duplicateObj.name}");
            Assert.AreNotEqual(ba.name, duplicateObj.name);
        }
    }
}
