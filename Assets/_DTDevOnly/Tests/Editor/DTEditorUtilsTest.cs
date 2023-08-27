using System.Collections.Generic;
using Chocopoi.DressingTools.Lib.Proxy;
using Chocopoi.DressingTools.Lib.Wearable;
using NUnit.Framework;
using UnityEngine;

namespace Chocopoi.DressingTools.Tests
{
    public class DTEditorUtilsTest : DTEditorTestBase
    {
        public class DummyClass1 { }
        public class DummyClass2 { }

        [Test]
        public void FindType_ReturnsCorrectType()
        {
            var myDummyType = typeof(DummyClass1);
            Debug.Log("DummyClass1 type full name: " + myDummyType.FullName);

            var ret = DTEditorUtils.FindType(myDummyType.FullName);
            Assert.NotNull(ret);
            Assert.AreEqual(myDummyType, ret);
        }

        [Test]
        public void FindType_CacheCoverage()
        {
            // just for passing coverage, doesn't really test anything
            var myDummyType = typeof(DummyClass2);
            Debug.Log("DummyClass2 type full name: " + myDummyType.FullName);

            var ret1 = DTEditorUtils.FindType(myDummyType.FullName);
            Assert.NotNull(ret1);
            Assert.AreEqual(myDummyType, ret1);

            var ret2 = DTEditorUtils.FindType(myDummyType.FullName);
            Assert.NotNull(ret2);
            Assert.AreEqual(myDummyType, ret2);
        }

        [Test]
        public void FindType_NoSuchType_ReturnsNull()
        {
            var ret = DTEditorUtils.FindType("Abababababa.No.Such.Class");
            Assert.Null(ret);
        }

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

            string path = DTEditorUtils.GetRelativePath(child3.transform);
            Assert.AreEqual("Child1/Child2/Child3", path);
        }

        [Test]
        public void GetRelativePath_ReturnsValidPath_WithUntilTransform()
        {
            var root = CreateGameObject("SomeObject2");
            var child1 = CreateGameObject("Child1", root.transform);
            var child2 = CreateGameObject("Child2", child1.transform);
            var child3 = CreateGameObject("Child3", child2.transform);

            string path = DTEditorUtils.GetRelativePath(child3.transform, child1.transform);
            Assert.AreEqual("Child2/Child3", path);
        }

        [Test]
        public void GetRelativePath_ReturnsValidPath_WithPrefixSuffix()
        {
            var root = CreateGameObject("SomeObject3");
            var child1 = CreateGameObject("Child1", root.transform);
            var child2 = CreateGameObject("Child2", child1.transform);
            var child3 = CreateGameObject("Child3", child2.transform);

            string path = DTEditorUtils.GetRelativePath(child3.transform, child1.transform, "SomePrefix/", "/SomeSuffix");
            Assert.AreEqual("SomePrefix/Child2/Child3/SomeSuffix", path);
        }

        [Test]
        public void IsGrandParentTest()
        {
            //
            // Root
            //  |- Child1
            //  |   |- GrandChild1
            //  |   |   |- GrandGrandChild1
            //  |   |   |- GrandGrandChild2
            //  |   |- GrandChild2
            //  |- Child2
            //
            var root = InstantiateEditorTestPrefab("IsGrandParentTestPrefab.prefab");
            var randomObject = CreateGameObject("IsGrandParentRandomObject");

            var child1 = root.transform.Find("Child1");
            var grandChild1 = child1.Find("GrandChild1");
            var grandGrandChild1 = grandChild1.Find("GrandGrandChild1");
            var grandGrandChild2 = grandChild1.Find("GrandGrandChild2");
            var grandChild2 = child1.Find("GrandChild2");
            var child2 = root.transform.Find("Child2");
            Assert.NotNull(child1);
            Assert.NotNull(grandChild1);
            Assert.NotNull(grandGrandChild1);
            Assert.NotNull(grandGrandChild2);
            Assert.NotNull(grandChild2);
            Assert.NotNull(child2);

            // depth 0
            Assert.True(DTEditorUtils.IsGrandParent(root.transform, child1));
            Assert.True(DTEditorUtils.IsGrandParent(child1, grandChild1));
            Assert.True(DTEditorUtils.IsGrandParent(grandChild1, grandGrandChild1));
            Assert.True(DTEditorUtils.IsGrandParent(grandChild1, grandGrandChild2));
            Assert.True(DTEditorUtils.IsGrandParent(child1, grandChild2));
            Assert.True(DTEditorUtils.IsGrandParent(root.transform, child2));
            Assert.False(DTEditorUtils.IsGrandParent(child2, grandChild1));
            Assert.False(DTEditorUtils.IsGrandParent(child2, grandChild2));
            Assert.False(DTEditorUtils.IsGrandParent(grandChild2, grandGrandChild1));
            Assert.False(DTEditorUtils.IsGrandParent(grandChild2, grandGrandChild2));

            // depth 1
            Assert.True(DTEditorUtils.IsGrandParent(root.transform, grandChild1));
            Assert.True(DTEditorUtils.IsGrandParent(root.transform, grandChild2));
            Assert.True(DTEditorUtils.IsGrandParent(child1, grandGrandChild1));
            Assert.True(DTEditorUtils.IsGrandParent(child1, grandGrandChild2));
            Assert.False(DTEditorUtils.IsGrandParent(child2, grandGrandChild1));
            Assert.False(DTEditorUtils.IsGrandParent(child2, grandGrandChild2));

            // depth 2
            Assert.True(DTEditorUtils.IsGrandParent(root.transform, grandGrandChild1));
            Assert.True(DTEditorUtils.IsGrandParent(root.transform, grandGrandChild2));

            // Random object
            Assert.False(DTEditorUtils.IsGrandParent(randomObject.transform, root.transform));
            Assert.False(DTEditorUtils.IsGrandParent(randomObject.transform, child1));
            Assert.False(DTEditorUtils.IsGrandParent(randomObject.transform, grandChild1));
            Assert.False(DTEditorUtils.IsGrandParent(randomObject.transform, grandGrandChild1));
            Assert.False(DTEditorUtils.IsGrandParent(randomObject.transform, grandGrandChild2));
            Assert.False(DTEditorUtils.IsGrandParent(randomObject.transform, grandChild2));
            Assert.False(DTEditorUtils.IsGrandParent(randomObject.transform, child2));
        }

        private static void AssertScannedDynamics(GameObject root)
        {
            var excludeWearableDynamics = DTEditorUtils.ScanDynamics(root, true);
            Assert.AreEqual(2, excludeWearableDynamics.Count, "Should have 2 dynamics with wearable dynamics excluded");

            var includeWearableDynamics = DTEditorUtils.ScanDynamics(root, false);
            Assert.AreEqual(4, includeWearableDynamics.Count, "Should have 4 dynamics with wearable dynamics included");
        }

        [Test]
        public void ScanDynamics_DynamicsBone()
        {
            var DynamicBoneType = DTEditorUtils.FindType("DynamicBone");
            if (DynamicBoneType == null)
            {
                Assert.Pass("DynamicBone is not imported, skipping this test");
                return;
            }

            var root = InstantiateEditorTestPrefab("DTTest_DynamicBoneAvatar.prefab");
            InstantiateEditorTestPrefab("DTTest_DynamicBoneWearable.prefab", root.transform);
            AssertScannedDynamics(root);
        }

        [Test]
        public void ScanDynamics_PhysBone()
        {
#if !VRC_SDK_VRCSDK3
            Assert.Pass("VRCSDK is not imported, skipping this test");
#else
            var root = InstantiateEditorTestPrefab("DTTest_PhysBoneAvatar.prefab");
            InstantiateEditorTestPrefab("DTTest_PhysBoneWearable.prefab", root.transform);
            AssertScannedDynamics(root);
#endif
        }

        private class DummyDynamicsProxy : IDynamicsProxy
        {
            public Component Component { get; set; } = null;
            public Transform Transform => null;
            public GameObject GameObject => null;
            public Transform RootTransform { get; set; }
            public List<Transform> IgnoreTransforms { get; set; }

            public DummyDynamicsProxy(Transform rootTransform)
            {
                RootTransform = rootTransform;
                IgnoreTransforms = null;
            }
        }

        [Test]
        public void FindDynamicsWithRootTest()
        {
            var obj1 = CreateGameObject("FindDynamicsWithRootObj1");
            var obj2 = CreateGameObject("FindDynamicsWithRootObj2");
            var obj3 = CreateGameObject("FindDynamicsWithRootObj3");
            var obj4 = CreateGameObject("FindDynamicsWithRootObj4");
            var list = new List<IDynamicsProxy>() {
                new DummyDynamicsProxy(obj1.transform),
                new DummyDynamicsProxy(obj2.transform),
                new DummyDynamicsProxy(obj3.transform)
            };

            var dynObj1 = DTEditorUtils.FindDynamicsWithRoot(list, obj1.transform);
            Assert.NotNull(dynObj1);
            Assert.AreEqual(obj1.transform, dynObj1.RootTransform);

            var dynObj2 = DTEditorUtils.FindDynamicsWithRoot(list, obj2.transform);
            Assert.NotNull(dynObj2);
            Assert.AreEqual(obj2.transform, dynObj2.RootTransform);

            var dynObj3 = DTEditorUtils.FindDynamicsWithRoot(list, obj3.transform);
            Assert.NotNull(dynObj3);
            Assert.AreEqual(obj3.transform, dynObj3.RootTransform);

            Assert.Null(DTEditorUtils.FindDynamicsWithRoot(list, obj4.transform));
        }

        [Test]
        public void IsDynamicsExistsTest()
        {

            var obj1 = CreateGameObject("IsDynamicsExistsTestObj1");
            var obj2 = CreateGameObject("IsDynamicsExistsTestObj2");
            var list = new List<IDynamicsProxy>() {
                new DummyDynamicsProxy(obj1.transform),
            };
            Assert.True(DTEditorUtils.IsDynamicsExists(list, obj1.transform));
            Assert.False(DTEditorUtils.IsDynamicsExists(list, obj2.transform));
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

        private class DummyComponent : MonoBehaviour
        {
            public static int SomeStaticField = 24680;

            public string SomeStringProperty { get; set; }
            public int SomeIntProperty { get; set; }

            public string someString;
            public bool someBool;
            public int someInt;
        }

        [Test]
        public void CopyComponentTest()
        {
            var obj1 = CreateGameObject("CopyComponentObj1");
            var obj2 = CreateGameObject("CopyComponentObj2");

            var comp = obj1.AddComponent<DummyComponent>();
            comp.SomeStringProperty = "Bakabaka";
            comp.SomeIntProperty = 123456;
            comp.someBool = true;
            comp.someInt = 654321;
            comp.someString = "HelloWorld";

            var copiedComp = DTEditorUtils.CopyComponent(comp, obj2);

            var gotComp = obj2.GetComponent<DummyComponent>();
            Assert.NotNull(gotComp);
            Assert.AreEqual(copiedComp, gotComp);

            Assert.AreEqual(comp.SomeStringProperty, gotComp.SomeStringProperty);
            Assert.AreEqual(comp.SomeIntProperty, gotComp.SomeIntProperty);
            Assert.AreEqual(comp.someBool, gotComp.someBool);
            Assert.AreEqual(comp.someInt, gotComp.someInt);
            Assert.AreEqual(comp.someString, gotComp.someString);
        }

        [Test]
        public void IsOriginatedFromAnyWearableTest()
        {
            var avatarRoot = InstantiateEditorTestPrefab("DTTest_PhysBoneAvatar.prefab");
            var wearableRoot = InstantiateEditorTestPrefab("DTTest_PhysBoneWearable.prefab", avatarRoot.transform);

            // we take the armatures GameObject as the test subjects
            var avatarRootArmature = avatarRoot.transform.Find("Armature");
            Assert.NotNull(avatarRootArmature);
            var wearableRootArmature = wearableRoot.transform.Find("Armature");
            Assert.NotNull(wearableRootArmature);

            Assert.True(DTEditorUtils.IsOriginatedFromAnyWearable(avatarRoot.transform, wearableRootArmature));
            Assert.False(DTEditorUtils.IsOriginatedFromAnyWearable(avatarRoot.transform, avatarRootArmature));
        }
    }
}
