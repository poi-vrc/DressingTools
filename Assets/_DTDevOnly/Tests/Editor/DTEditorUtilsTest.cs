using System.Collections.Generic;
using Chocopoi.AvatarLib.Animations;
using Chocopoi.DressingTools.Api.Wearable.Modules.BuiltIn.ArmatureMapping;
using NUnit.Framework;
using UnityEngine;

namespace Chocopoi.DressingTools.Tests
{
    public class DTEditorUtilsTest : EditorTestBase
    {
        #region Expected Bone Mapping Overrides
        private static readonly BoneMapping[] OriginalBoneMappings = {
            new BoneMapping() { mappingType = BoneMappingType.MoveToBone, avatarBonePath = "Armature/Hips", wearableBonePath = "Armature/Hips" },
            new BoneMapping() { mappingType = BoneMappingType.ParentConstraint, avatarBonePath = "Armature/Hips/MyBone", wearableBonePath = "Armature/Hips/MyBone" },
            new BoneMapping() { mappingType = BoneMappingType.MoveToBone, avatarBonePath = "Armature/Hips/MyDynBone", wearableBonePath = "Armature/Hips/MyDynBone" },
            new BoneMapping() { mappingType = BoneMappingType.IgnoreTransform, avatarBonePath = "Armature/Hips/MyDynBone/MyDynBone1", wearableBonePath = "Armature/Hips/MyDynBone/MyDynBone1" },
        };

        private static readonly BoneMapping[] BoneOverrides1 = {
            new BoneMapping() { mappingType = BoneMappingType.ParentConstraint, avatarBonePath = "Armature/Hips", wearableBonePath = "Armature/Hips" },
            new BoneMapping() { mappingType = BoneMappingType.MoveToBone, avatarBonePath = "Armature/Hips/SomeAnotherMyBone", wearableBonePath = "Armature/Hips/MyBone" },
            new BoneMapping() { mappingType = BoneMappingType.IgnoreTransform, avatarBonePath = "Armature/Hips/SomeAnotherMyDynBone", wearableBonePath = "Armature/Hips/MyDynBone" },
            new BoneMapping() { mappingType = BoneMappingType.ParentConstraint, avatarBonePath = "Armature/Hips/MyDynBone/MyDynBone1", wearableBonePath = "Armature/Hips/MyDynBone/MyDynBone1" },
        };

        private static readonly BoneMapping[] ExpectedBoneMappings1 = {
            new BoneMapping() { mappingType = BoneMappingType.ParentConstraint, avatarBonePath = "Armature/Hips", wearableBonePath = "Armature/Hips" },
            new BoneMapping() { mappingType = BoneMappingType.MoveToBone, avatarBonePath = "Armature/Hips/SomeAnotherMyBone", wearableBonePath = "Armature/Hips/MyBone" },
            new BoneMapping() { mappingType = BoneMappingType.IgnoreTransform, avatarBonePath = "Armature/Hips/SomeAnotherMyDynBone", wearableBonePath = "Armature/Hips/MyDynBone" },
            new BoneMapping() { mappingType = BoneMappingType.ParentConstraint, avatarBonePath = "Armature/Hips/MyDynBone/MyDynBone1", wearableBonePath = "Armature/Hips/MyDynBone/MyDynBone1" },
        };

        private static readonly BoneMapping[] BoneOverrides2 = {
            new BoneMapping() { mappingType = BoneMappingType.ParentConstraint, avatarBonePath = "Armature/Hips/SomeAnotherMyBone", wearableBonePath = "Armature/Hips/MyBone" },
            new BoneMapping() { mappingType = BoneMappingType.ParentConstraint, avatarBonePath = "Armature/Hips/MyDynBone", wearableBonePath = "Armature/Hips/MyDynBone" },
        };

        private static readonly BoneMapping[] ExpectedBoneMappings2 = {
            new BoneMapping() { mappingType = BoneMappingType.MoveToBone, avatarBonePath = "Armature/Hips", wearableBonePath = "Armature/Hips" },
            new BoneMapping() { mappingType = BoneMappingType.ParentConstraint, avatarBonePath = "Armature/Hips/SomeAnotherMyBone", wearableBonePath = "Armature/Hips/MyBone" },
            new BoneMapping() { mappingType = BoneMappingType.ParentConstraint, avatarBonePath = "Armature/Hips/MyDynBone", wearableBonePath = "Armature/Hips/MyDynBone" },
            new BoneMapping() { mappingType = BoneMappingType.IgnoreTransform, avatarBonePath = "Armature/Hips/MyDynBone/MyDynBone1", wearableBonePath = "Armature/Hips/MyDynBone/MyDynBone1" },
        };

        private static readonly BoneMapping[] BoneOverrides3 = {
            new BoneMapping() { mappingType = BoneMappingType.ParentConstraint, avatarBonePath = "Armature/Hips/MyBone", wearableBonePath = "Armature/Hips/MyBone" },
            new BoneMapping() { mappingType = BoneMappingType.ParentConstraint, avatarBonePath = "Armature/Hips/SomeAnotherMyDynBone", wearableBonePath = "Armature/Hips/MyDynBone" },
            new BoneMapping() { mappingType = BoneMappingType.ParentConstraint, avatarBonePath = "Armature/Hips/SomeRandomBone", wearableBonePath = "Armature/Hips/SomeRandomBone" },
        };

        private static readonly BoneMapping[] ExpectedBoneMappings3 = {
            new BoneMapping() { mappingType = BoneMappingType.MoveToBone, avatarBonePath = "Armature/Hips", wearableBonePath = "Armature/Hips" },
            new BoneMapping() { mappingType = BoneMappingType.ParentConstraint, avatarBonePath = "Armature/Hips/MyBone", wearableBonePath = "Armature/Hips/MyBone" },
            new BoneMapping() { mappingType = BoneMappingType.ParentConstraint, avatarBonePath = "Armature/Hips/SomeAnotherMyDynBone", wearableBonePath = "Armature/Hips/MyDynBone" },
            new BoneMapping() { mappingType = BoneMappingType.IgnoreTransform, avatarBonePath = "Armature/Hips/MyDynBone/MyDynBone1", wearableBonePath = "Armature/Hips/MyDynBone/MyDynBone1" },
            new BoneMapping() { mappingType = BoneMappingType.ParentConstraint, avatarBonePath = "Armature/Hips/SomeRandomBone", wearableBonePath = "Armature/Hips/SomeRandomBone" },
        };

        #endregion Expected Bone Mapping Overrides

        private static void PrintMappings(List<BoneMapping> boneMappings)
        {
            foreach (var mapping in boneMappings)
            {
                Debug.Log(mapping.ToString());
            }
        }

        private static List<BoneMapping> DeepCopyBoneMappings(BoneMapping[] array)
        {
            var output = new List<BoneMapping>();
            foreach (var mapping in array)
            {
                output.Add(new BoneMapping()
                {
                    mappingType = mapping.mappingType,
                    avatarBonePath = mapping.avatarBonePath,
                    wearableBonePath = mapping.wearableBonePath
                });
            }
            return output;
        }

        private static void AssertBoneMappingsAreEqual(BoneMapping[] expected, List<BoneMapping> actual)
        {
            PrintMappings(actual);

            Assert.AreEqual(expected.Length, actual.Count);

            foreach (var actualMapping in actual)
            {
                Debug.Log("Checking: " + actualMapping.ToString());

                var found = false;
                foreach (var expectedMapping in expected)
                {
                    if (expectedMapping.Equals(actualMapping))
                    {
                        found = true;
                        break;
                    }
                }

                Assert.True(found, "Could not find such mapping in expected bone mappings: " + actualMapping.ToString());
            }
        }

        [Test]
        public void HandleBoneMappingOverrides_BoneOverrides1()
        {
            var newMappings = DeepCopyBoneMappings(OriginalBoneMappings);
            var overrideMappings = DeepCopyBoneMappings(BoneOverrides1);
            DTEditorUtils.HandleBoneMappingOverrides(newMappings, overrideMappings);

            AssertBoneMappingsAreEqual(ExpectedBoneMappings1, newMappings);
        }

        [Test]
        public void HandleBoneMappingOverrides_BoneOverrides2()
        {
            var newMappings = DeepCopyBoneMappings(OriginalBoneMappings);
            var overrideMappings = DeepCopyBoneMappings(BoneOverrides2);
            DTEditorUtils.HandleBoneMappingOverrides(newMappings, overrideMappings);

            AssertBoneMappingsAreEqual(ExpectedBoneMappings2, newMappings);
        }

        [Test]
        public void HandleBoneMappingOverrides_BoneOverrides3()
        {
            var newMappings = DeepCopyBoneMappings(OriginalBoneMappings);
            var overrideMappings = DeepCopyBoneMappings(BoneOverrides3);
            DTEditorUtils.HandleBoneMappingOverrides(newMappings, overrideMappings);

            AssertBoneMappingsAreEqual(ExpectedBoneMappings3, newMappings);
        }


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

        [Test]
        public void GuessMatchingAvatarBoneTest()
        {
            var root = CreateGameObject("GuessMatchingAvatarBoneRoot");
            var bone1 = CreateGameObject("LeftUpperLeg", root.transform);
            var bone2 = CreateGameObject("RightUpperLeg", root.transform);

            Transform trans;

            // remove prefix
            trans = DTEditorUtils.GuessMatchingAvatarBone(root.transform, "(SomePrefix) LeftUpperLeg");
            Assert.NotNull(trans);
            Assert.AreEqual(bone1.transform, trans);

            // remove suffix
            trans = DTEditorUtils.GuessMatchingAvatarBone(root.transform, "LeftUpperLeg (SomeSuffix)");
            Assert.NotNull(trans);
            Assert.AreEqual(bone1.transform, trans);

            // trim and remove prefix and suffix
            trans = DTEditorUtils.GuessMatchingAvatarBone(root.transform, "  (SomePrefix)   LeftUpperLeg(SomeSuffix)    ");
            Assert.NotNull(trans);
            Assert.AreEqual(bone1.transform, trans);

            // guess bone using bone mapping names list
            trans = DTEditorUtils.GuessMatchingAvatarBone(root.transform, "Leg_L");
            Assert.NotNull(trans);
            Assert.AreEqual(bone1.transform, trans);

            trans = DTEditorUtils.GuessMatchingAvatarBone(root.transform, "Right leg");
            Assert.NotNull(trans);
            Assert.AreEqual(bone2.transform, trans);

            // no such bone
            trans = DTEditorUtils.GuessMatchingAvatarBone(root.transform, "Baka bone");
            Assert.Null(trans);
        }

        [Test]
        public void GuessArmature_PrefixedSuffixed()
        {
            var root = CreateGameObject("GuessArmatureRoot1");
            var armature = CreateGameObject("(SomePrefix) Armature   (SomeSuffix)", root.transform);
            var trans = DTEditorUtils.GuessArmature(root, "Armature");
            Assert.NotNull(trans);
            Assert.AreEqual(armature.transform, trans);
        }

        [Test]
        public void GuessArmature_CamelName()
        {
            var root = CreateGameObject("GuessArmatureRoot1");
            var armature = CreateGameObject("aRmaTURe", root.transform);
            var trans = DTEditorUtils.GuessArmature(root, "Armature");
            Assert.NotNull(trans);
            Assert.AreEqual(armature.transform, trans);
        }

        [Test]
        public void GuessArmature_MultipleMatches()
        {
            var root = CreateGameObject("GuessArmatureRoot1");
            CreateGameObject("aRmaTURe", root.transform);
            CreateGameObject("Armature", root.transform);
            var trans = DTEditorUtils.GuessArmature(root, "Armature");
            Assert.Null(trans);
        }

        [Test]
        public void GuessArmature_NoArmature()
        {
            var root = CreateGameObject("GuessArmatureRoot1");
            CreateGameObject("Bakabaka", root.transform);
            CreateGameObject("HelloWorld", root.transform);
            var trans = DTEditorUtils.GuessArmature(root, "Armature");
            Assert.Null(trans);
        }

        [Test]
        public void GuessArmature_Rename()
        {
            var root = CreateGameObject("GuessArmatureRoot1");
            var armature = CreateGameObject("aRmaTURe", root.transform);
            var trans = DTEditorUtils.GuessArmature(root, "Armature", true);
            Assert.NotNull(trans);
            Assert.AreEqual(armature.transform, trans);
            Assert.AreEqual("Armature", trans.name);
        }
    }
}
