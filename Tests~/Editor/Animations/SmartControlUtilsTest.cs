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

using System.Collections.Generic;
using Chocopoi.DressingTools.Animations;
using NUnit.Framework;
using UnityEngine;

namespace Chocopoi.DressingTools.Tests.Animations
{
    internal class SmartControlUtilsTest : EditorTestBase
    {
        [Test]
        public void SuggestRelativePathNameTest()
        {
            var root = CreateGameObject("root");
            var a = CreateGameObject("A", root.transform);
            var b = CreateGameObject("B", a.transform);
            var comp1 = b.AddComponent<MeshCollider>();
            Assert.AreEqual("A/B", SmartControlUtils.SuggestRelativePathName(root.transform, comp1));

            // now an extra component is added, we suggest another more unique name
            var comp2 = b.AddComponent<MeshCollider>();

            Assert.AreEqual("A/B_1", SmartControlUtils.SuggestRelativePathName(root.transform, comp1));
            Assert.AreEqual("A/B_2", SmartControlUtils.SuggestRelativePathName(root.transform, comp2));

            // components on the root, we let the root's name to be our name

            var comp3 = root.AddComponent<MeshCollider>();
            Assert.AreEqual("root", SmartControlUtils.SuggestRelativePathName(root.transform, comp3));

            var comp4 = root.AddComponent<MeshCollider>();
            Assert.AreEqual("root_1", SmartControlUtils.SuggestRelativePathName(root.transform, comp3));
            Assert.AreEqual("root_2", SmartControlUtils.SuggestRelativePathName(root.transform, comp4));
        }

        [Test]
        public void GetSelectedObjectsTest()
        {
            var root = CreateGameObject("root");
            var a = CreateGameObject("A", root.transform);
            var ab = CreateGameObject("AB", a.transform);
            var b = CreateGameObject("B", root.transform);
            var c = CreateGameObject("C", root.transform);

            var objs = SmartControlUtils.GetSelectedObjects(root.transform, new List<GameObject>(), false);
            Assert.AreEqual(0, objs.Count);

            objs = SmartControlUtils.GetSelectedObjects(root.transform, new List<GameObject>(), true);
            Assert.AreEqual(4, objs.Count);
            Assert.True(objs.Contains(a));
            Assert.True(objs.Contains(ab));
            Assert.True(objs.Contains(b));
            Assert.True(objs.Contains(c));

            objs = SmartControlUtils.GetSelectedObjects(root.transform, new List<GameObject>() { a, ab, }, false);
            Assert.AreEqual(2, objs.Count);
            Assert.True(objs.Contains(a));
            Assert.True(objs.Contains(ab));

            objs = SmartControlUtils.GetSelectedObjects(root.transform, new List<GameObject>() { b }, true);
            Assert.AreEqual(3, objs.Count);
            Assert.True(objs.Contains(a));
            Assert.True(objs.Contains(ab));
            Assert.False(objs.Contains(b));
            Assert.True(objs.Contains(c));

            objs = SmartControlUtils.GetSelectedObjects(root.transform, new List<GameObject>() { a }, true);
            Assert.AreEqual(2, objs.Count);
            Assert.False(objs.Contains(a));
            Assert.False(objs.Contains(ab));
            Assert.True(objs.Contains(b));
            Assert.True(objs.Contains(c));
        }
    }
}
