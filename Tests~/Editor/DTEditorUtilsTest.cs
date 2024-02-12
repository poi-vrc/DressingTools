using System.Collections.Generic;
using Chocopoi.AvatarLib.Animations;
using NUnit.Framework;
using UnityEngine;

namespace Chocopoi.DressingTools.Tests
{
    internal class DTEditorUtilsTest : EditorTestBase
    {
        [Test]
        public void GetRelativePath_ReturnsValidPath_WithoutUntilTransform()
        {
            var root = CreateGameObject("SomeObject1");
            var child1 = CreateGameObject("Child1", root.transform);
            var child2 = CreateGameObject("Child2", child1.transform);
            var child3 = CreateGameObject("Child3", child2.transform);

            string path = AnimationUtils.GetRelativePath(child3.transform);
            Assert.AreEqual("Child1/Child2/Child3", path);
        }

        [Test]
        public void GetRelativePath_ReturnsValidPath_WithUntilTransform()
        {
            var root = CreateGameObject("SomeObject2");
            var child1 = CreateGameObject("Child1", root.transform);
            var child2 = CreateGameObject("Child2", child1.transform);
            var child3 = CreateGameObject("Child3", child2.transform);

            string path = AnimationUtils.GetRelativePath(child3.transform, child1.transform);
            Assert.AreEqual("Child2/Child3", path);
        }

        [Test]
        public void GetRelativePath_ReturnsValidPath_WithPrefixSuffix()
        {
            var root = CreateGameObject("SomeObject3");
            var child1 = CreateGameObject("Child1", root.transform);
            var child2 = CreateGameObject("Child2", child1.transform);
            var child3 = CreateGameObject("Child3", child2.transform);

            string path = AnimationUtils.GetRelativePath(child3.transform, child1.transform, "SomePrefix/", "/SomeSuffix");
            Assert.AreEqual("SomePrefix/Child2/Child3/SomeSuffix", path);
        }
    }
}
