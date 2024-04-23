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
using Chocopoi.AvatarLib.Animations;
using Chocopoi.DressingFramework.Detail.DK.Logging;
using Chocopoi.DressingTools.Dresser;
using Chocopoi.DressingTools.Dresser.Standard;
using Chocopoi.DressingTools.Dresser.Tags;
using Chocopoi.DressingTools.OneConf;
using NUnit.Framework;
using UnityEngine;

namespace Chocopoi.DressingTools.Tests.Dresser
{
    internal class DefaultDresserTest : EditorTestBase
    {
        private static bool EvaluateDresser(GameObject avatarRoot, GameObject wearableRoot, out DKReport report, out List<ObjectMapping> objectMappings, out List<ITag> tags, StandardDresserSettings.DynamicsOptions dynamicsOption = StandardDresserSettings.DynamicsOptions.RemoveDynamicsAndUseParentConstraint)
        {
            report = new DKReport();

            var sourceArmature = OneConfUtils.GuessArmature(wearableRoot, "Armature");
            Assert.NotNull(sourceArmature);

            var settings = new StandardDresserSettings()
            {
                SourceArmature = sourceArmature,
                TargetArmaturePath = "Armature",
                DynamicsOption = dynamicsOption
            };
            var dresser = new StandardDresser();
            dresser.Execute(report, avatarRoot, settings, out objectMappings, out tags);
            return !report.HasLogType(DressingFramework.Logging.LogType.Error);
        }

        private static void PrintMappings(GameObject wearableRoot, List<ObjectMapping> objectMappings, List<ITag> tags)
        {
            Debug.Log("Object Mappings:");
            foreach (var mapping in objectMappings)
            {
                Debug.Log(mapping.ToString());
            }
            Debug.Log("Tags:");
            foreach (var tag in tags)
            {
                Debug.Log($"{AnimationUtils.GetRelativePath(tag.SourceTransform, wearableRoot.transform)} : {tag.GetType().Name}");
            }
        }

        #region Log Code Tests
        [Test]
        public void SourceArmatureNotInsideAvatar_ReturnsCorrectErrorCodes()
        {
            var avatarRoot = CreateGameObject("Avatar");
            var wearableRoot = CreateGameObject("Wearable");

            CreateGameObject("Armature", avatarRoot.transform);
            CreateGameObject("Armature", wearableRoot.transform);

            var result = EvaluateDresser(avatarRoot, wearableRoot, out var report, out _, out _);
            Assert.False(result, "Hook should return false");
            Assert.True(report.HasLogCode(StandardDresser.MessageCode.SourceArmatureNotInsideAvatar));
        }

        [Test]
        public void NoBonesInTargetArmature_ReturnsCorrectErrorCodes()
        {
            var avatarRoot = CreateGameObject("Avatar");
            var wearableRoot = CreateGameObject("Wearable");
            wearableRoot.transform.SetParent(avatarRoot.transform);

            CreateGameObject("Armature", avatarRoot.transform);
            var wearableArmature = CreateGameObject("Armature", wearableRoot.transform);

            CreateGameObject("Hips", wearableArmature.transform);

            var result = EvaluateDresser(avatarRoot, wearableRoot, out var report, out _, out _);
            Assert.False(result, "Hook should return false");
            Assert.True(report.HasLogCode(StandardDresser.MessageCode.NoBonesInTargetArmatureFirstLevel));
        }

        [Test]
        public void NoBonesInSourceArmature_ReturnsCorrectErrorCodes()
        {
            var avatarRoot = CreateGameObject("Avatar");
            var wearableRoot = CreateGameObject("Wearable");
            wearableRoot.transform.SetParent(avatarRoot.transform);

            var avatarArmature = CreateGameObject("Armature", avatarRoot.transform);
            CreateGameObject("Armature", wearableRoot.transform);

            CreateGameObject("Hips", avatarArmature.transform);

            var result = EvaluateDresser(avatarRoot, wearableRoot, out var report, out _, out _);
            Assert.False(result, "Hook should return false");
            Assert.True(report.HasLogCode(StandardDresser.MessageCode.NoBonesInSourceArmatureFirstLevel));
        }
        #endregion Log Code Tests

        #region Expected Bone Mappings (Identical for either PhysBone or DynamicBones)
        // expected bone mappings
        private static ObjectMapping[] ExpectedRemoveDynamicsAndUseParentConstraintsBoneMappings(Transform wearableRoot)
        {
            return new ObjectMapping[] {
                new ObjectMapping() { Type = ObjectMapping.MappingType.MoveToBone, TargetPath = "Armature/Hips", SourceTransform = wearableRoot.Find("Armature/Hips") },
                new ObjectMapping() { Type = ObjectMapping.MappingType.MoveToBone, TargetPath = "Armature/Hips/MyBone", SourceTransform = wearableRoot.Find("Armature/Hips/MyBone") },
                new ObjectMapping() { Type = ObjectMapping.MappingType.ParentConstraint, TargetPath = "Armature/Hips/MyDynBone", SourceTransform = wearableRoot.Find("Armature/Hips/MyDynBone") },
                new ObjectMapping() { Type = ObjectMapping.MappingType.ParentConstraint, TargetPath = "Armature/Hips/MyDynBone/MyDynBone1", SourceTransform = wearableRoot.Find("Armature/Hips/MyDynBone/MyDynBone1") },
                new ObjectMapping() { Type = ObjectMapping.MappingType.ParentConstraint, TargetPath = "Armature/Hips/MyDynBone/MyDynBone2", SourceTransform = wearableRoot.Find("Armature/Hips/MyDynBone/MyDynBone2") },
                new ObjectMapping() { Type = ObjectMapping.MappingType.ParentConstraint, TargetPath = "Armature/Hips/MyAnotherDynBone", SourceTransform = wearableRoot.Find("Armature/Hips/MyAnotherDynBone") },
                new ObjectMapping() { Type = ObjectMapping.MappingType.ParentConstraint, TargetPath = "Armature/Hips/MyAnotherDynBone/MyAnotherDynBone1", SourceTransform = wearableRoot.Find("Armature/Hips/MyAnotherDynBone/MyAnotherDynBone1") },
                new ObjectMapping() { Type = ObjectMapping.MappingType.ParentConstraint, TargetPath = "Armature/Hips/MyAnotherDynBone/MyAnotherDynBone2", SourceTransform = wearableRoot.Find("Armature/Hips/MyAnotherDynBone/MyAnotherDynBone2") },
            };
        }

        private static ObjectMapping[] ExpectedRemoveDynamicsAndUseParentConstraintsWithSuffixBoneMappings(Transform wearableRoot)
        {
            return new ObjectMapping[] {
                new ObjectMapping() { Type = ObjectMapping.MappingType.MoveToBone, TargetPath = "Armature/Hips", SourceTransform = wearableRoot.Find("Armature (Suffix)/Hips (Suffix)") },
                new ObjectMapping() { Type = ObjectMapping.MappingType.MoveToBone, TargetPath = "Armature/Hips/MyBone", SourceTransform = wearableRoot.Find("Armature (Suffix)/Hips (Suffix)/MyBone (Suffix)") },
                new ObjectMapping() { Type = ObjectMapping.MappingType.ParentConstraint, TargetPath = "Armature/Hips/MyDynBone", SourceTransform = wearableRoot.Find("Armature (Suffix)/Hips (Suffix)/MyDynBone (Suffix)") },
                new ObjectMapping() { Type = ObjectMapping.MappingType.ParentConstraint, TargetPath = "Armature/Hips/MyDynBone/MyDynBone1", SourceTransform = wearableRoot.Find("Armature (Suffix)/Hips (Suffix)/MyDynBone (Suffix)/MyDynBone1 (Suffix)") },
                new ObjectMapping() { Type = ObjectMapping.MappingType.ParentConstraint, TargetPath = "Armature/Hips/MyDynBone/MyDynBone2", SourceTransform = wearableRoot.Find("Armature (Suffix)/Hips (Suffix)/MyDynBone (Suffix)/MyDynBone2 (Suffix)") },
                new ObjectMapping() { Type = ObjectMapping.MappingType.ParentConstraint, TargetPath = "Armature/Hips/MyAnotherDynBone", SourceTransform = wearableRoot.Find("Armature (Suffix)/Hips (Suffix)/MyAnotherDynBone (Suffix)") },
                new ObjectMapping() { Type = ObjectMapping.MappingType.ParentConstraint, TargetPath = "Armature/Hips/MyAnotherDynBone/MyAnotherDynBone1", SourceTransform = wearableRoot.Find("Armature (Suffix)/Hips (Suffix)/MyAnotherDynBone (Suffix)/MyAnotherDynBone1 (Suffix)") },
                new ObjectMapping() { Type = ObjectMapping.MappingType.ParentConstraint, TargetPath = "Armature/Hips/MyAnotherDynBone/MyAnotherDynBone2", SourceTransform = wearableRoot.Find("Armature (Suffix)/Hips (Suffix)/MyAnotherDynBone (Suffix)/MyAnotherDynBone2 (Suffix)") },
            };
        }

        private static ObjectMapping[] ExpectedKeepDynamicsAndUseParentConstraintIfNecessaryBoneMappings(Transform wearableRoot)
        {
            return new ObjectMapping[] {
                new ObjectMapping() { Type = ObjectMapping.MappingType.MoveToBone, TargetPath = "Armature/Hips", SourceTransform = wearableRoot.Find("Armature/Hips") },
                new ObjectMapping() { Type = ObjectMapping.MappingType.MoveToBone, TargetPath = "Armature/Hips/MyBone", SourceTransform = wearableRoot.Find("Armature/Hips/MyBone") },
                new ObjectMapping() { Type = ObjectMapping.MappingType.ParentConstraint, TargetPath = "Armature/Hips/MyAnotherDynBone", SourceTransform = wearableRoot.Find("Armature/Hips/MyAnotherDynBone") },
                new ObjectMapping() { Type = ObjectMapping.MappingType.ParentConstraint, TargetPath = "Armature/Hips/MyAnotherDynBone/MyAnotherDynBone1", SourceTransform = wearableRoot.Find("Armature/Hips/MyAnotherDynBone/MyAnotherDynBone1") },
                new ObjectMapping() { Type = ObjectMapping.MappingType.ParentConstraint, TargetPath = "Armature/Hips/MyAnotherDynBone/MyAnotherDynBone2", SourceTransform = wearableRoot.Find("Armature/Hips/MyAnotherDynBone/MyAnotherDynBone2") },
            };
        }

        private static ObjectMapping[] ExpectedIgnoreTransformBoneMappings(Transform wearableRoot)
        {
            return new ObjectMapping[] {
                new ObjectMapping() { Type = ObjectMapping.MappingType.MoveToBone, TargetPath = "Armature/Hips", SourceTransform = wearableRoot.Find("Armature/Hips") },
                new ObjectMapping() { Type = ObjectMapping.MappingType.MoveToBone, TargetPath = "Armature/Hips/MyBone", SourceTransform = wearableRoot.Find("Armature/Hips/MyBone") },
                new ObjectMapping() { Type = ObjectMapping.MappingType.IgnoreTransform, TargetPath = "Armature/Hips/MyDynBone", SourceTransform = wearableRoot.Find("Armature/Hips/MyDynBone") },
                new ObjectMapping() { Type = ObjectMapping.MappingType.IgnoreTransform, TargetPath = "Armature/Hips/MyDynBone/MyDynBone1", SourceTransform = wearableRoot.Find("Armature/Hips/MyDynBone/MyDynBone1") },
                new ObjectMapping() { Type = ObjectMapping.MappingType.IgnoreTransform, TargetPath = "Armature/Hips/MyDynBone/MyDynBone2", SourceTransform = wearableRoot.Find("Armature/Hips/MyDynBone/MyDynBone2") },
                new ObjectMapping() { Type = ObjectMapping.MappingType.IgnoreTransform, TargetPath = "Armature/Hips/MyAnotherDynBone", SourceTransform = wearableRoot.Find("Armature/Hips/MyAnotherDynBone") },
                new ObjectMapping() { Type = ObjectMapping.MappingType.IgnoreTransform, TargetPath = "Armature/Hips/MyAnotherDynBone/MyAnotherDynBone1", SourceTransform = wearableRoot.Find("Armature/Hips/MyAnotherDynBone/MyAnotherDynBone1") },
                new ObjectMapping() { Type = ObjectMapping.MappingType.IgnoreTransform, TargetPath = "Armature/Hips/MyAnotherDynBone/MyAnotherDynBone2", SourceTransform = wearableRoot.Find("Armature/Hips/MyAnotherDynBone/MyAnotherDynBone2") },
            };
        }

        private static ObjectMapping[] ExpectedCopyDynamicsBoneMappings(Transform wearableRoot)
        {
            return new ObjectMapping[] {
                new ObjectMapping() { Type = ObjectMapping.MappingType.MoveToBone, TargetPath = "Armature/Hips", SourceTransform = wearableRoot.Find("Armature/Hips") },
                new ObjectMapping() { Type = ObjectMapping.MappingType.MoveToBone, TargetPath = "Armature/Hips/MyBone", SourceTransform = wearableRoot.Find("Armature/Hips/MyBone") },
            };
        }

        private static ITag[] ExpectedCopyDynamicsTags(Transform wearableRoot)
        {
            return new ITag[] {
                new CopyDynamicsTag() { SourceTransform = wearableRoot.Find("Armature/Hips/MyDynBone"), TargetPath = "Armature/Hips/MyDynBone" },
                new CopyDynamicsTag() { SourceTransform = wearableRoot.Find("Armature/Hips/MyAnotherDynBone"), TargetPath = "Armature/Hips/MyAnotherDynBone" }
            };
        }

        private static ObjectMapping[] ExpectedIgnoreAllBoneMappings(Transform wearableRoot)
        {
            return new ObjectMapping[] {
                new ObjectMapping() { Type = ObjectMapping.MappingType.MoveToBone, TargetPath = "Armature/Hips", SourceTransform = wearableRoot.Find("Armature/Hips") },
                new ObjectMapping() { Type = ObjectMapping.MappingType.MoveToBone, TargetPath = "Armature/Hips/MyBone", SourceTransform = wearableRoot.Find("Armature/Hips/MyBone") },
            };
        }
        #endregion Expected Bone Mappings (Identical for either PhysBone or DynamicBones)

        #region Bone Mapping Tests
        private void AvatarDynamics_RemoveDynamicsAndUseParentConstraints_ReturnsCorrectBoneMappingAndLogCodes(GameObject avatarRoot, GameObject wearableRoot)
        {
            var result = EvaluateDresser(avatarRoot, wearableRoot, out var report, out var objectMappings, out var tags, StandardDresserSettings.DynamicsOptions.RemoveDynamicsAndUseParentConstraint);
            Assert.True(result, "Hook should return true");

            PrintMappings(wearableRoot, objectMappings, tags);

            var expectedMappings = ExpectedRemoveDynamicsAndUseParentConstraintsBoneMappings(wearableRoot.transform);
            Assert.AreEqual(expectedMappings.Length, objectMappings.Count, "Bone mapping length is not equal to expected length");
            Assert.AreEqual(0, tags.Count);

            foreach (var mapping in objectMappings)
            {
                Debug.Log("Checking: " + mapping.ToString());

                bool found = false;
                foreach (var expectedMapping in expectedMappings)
                {
                    if (expectedMapping.Equals(mapping))
                    {
                        found = true;
                        break;
                    }
                }

                Assert.True(found, "Could not find such mapping in expected bone mapping array: " + mapping.ToString());
            }
        }

        [Test]
        public void DynamicBone_RemoveDynamicsAndUseParentConstraints_ReturnsCorrectBoneMappingAndLogCodes()
        {
            // DynamicBone has to be imported to run this test
            AssertPassImportedDynamicBone();

            var avatarRoot = InstantiateEditorTestPrefab("DTTest_DynamicBoneAvatar.prefab");
            var wearableRoot = InstantiateEditorTestPrefab("DTTest_DynamicBoneWearable.prefab");
            wearableRoot.transform.SetParent(avatarRoot.transform);

            AvatarDynamics_RemoveDynamicsAndUseParentConstraints_ReturnsCorrectBoneMappingAndLogCodes(avatarRoot, wearableRoot);
        }

        [Test]
        public void PhysBone_RemoveDynamicsAndUseParentConstraints_ReturnsCorrectBoneMappingAndLogCodes()
        {
            // VRCSDK3 has to be imported to run this test
            AssertPassImportedVRCSDK();

            var avatarRoot = InstantiateEditorTestPrefab("DTTest_PhysBoneAvatar.prefab");
            var wearableRoot = InstantiateEditorTestPrefab("DTTest_PhysBoneWearable.prefab");
            wearableRoot.transform.SetParent(avatarRoot.transform);

            AvatarDynamics_RemoveDynamicsAndUseParentConstraints_ReturnsCorrectBoneMappingAndLogCodes(avatarRoot, wearableRoot);
        }

        private void AvatarDynamics_RemoveDynamicsAndUseParentConstraintsWithSuffix_ReturnsCorrectBoneMappingAndLogCodes(GameObject avatarRoot, GameObject wearableRoot)
        {
            var result = EvaluateDresser(avatarRoot, wearableRoot, out var report, out var objectMappings, out var tags, StandardDresserSettings.DynamicsOptions.RemoveDynamicsAndUseParentConstraint);
            Assert.True(result, "Hook should return true");

            PrintMappings(wearableRoot, objectMappings, tags);

            var expectedMappings = ExpectedRemoveDynamicsAndUseParentConstraintsWithSuffixBoneMappings(wearableRoot.transform);
            Assert.AreEqual(expectedMappings.Length, objectMappings.Count, "Bone mapping length is not equal to expected length");
            Assert.AreEqual(0, tags.Count);

            foreach (var mapping in objectMappings)
            {
                Debug.Log("Checking: " + mapping.ToString());

                bool found = false;
                foreach (var expectedMapping in expectedMappings)
                {
                    if (expectedMapping.Equals(mapping))
                    {
                        found = true;
                        break;
                    }
                }

                Assert.True(found, "Could not find such mapping in expected bone mapping array: " + mapping.ToString());
            }
        }

        [Test]
        public void DynamicBone_RemoveDynamicsAndUseParentConstraintsWithSuffix_ReturnsCorrectBoneMappingAndLogCodes()
        {
            // DynamicBone has to be imported to run this test
            AssertPassImportedDynamicBone();

            var avatarRoot = InstantiateEditorTestPrefab("DTTest_DynamicBoneAvatar.prefab");
            var wearableRoot = InstantiateEditorTestPrefab("DTTest_DynamicBoneWearableWithSuffix.prefab");
            wearableRoot.transform.SetParent(avatarRoot.transform);

            AvatarDynamics_RemoveDynamicsAndUseParentConstraintsWithSuffix_ReturnsCorrectBoneMappingAndLogCodes(avatarRoot, wearableRoot);
        }

        [Test]
        public void PhysBone_RemoveDynamicsAndUseParentConstraintsWithSuffix_ReturnsCorrectBoneMappingAndLogCodes()
        {
            // VRCSDK3 has to be imported to run this test
            AssertPassImportedVRCSDK();

            var avatarRoot = InstantiateEditorTestPrefab("DTTest_PhysBoneAvatar.prefab");
            var wearableRoot = InstantiateEditorTestPrefab("DTTest_PhysBoneWearableWithSuffix.prefab");
            wearableRoot.transform.SetParent(avatarRoot.transform);

            AvatarDynamics_RemoveDynamicsAndUseParentConstraintsWithSuffix_ReturnsCorrectBoneMappingAndLogCodes(avatarRoot, wearableRoot);
        }

        private void AvatarDynamics_KeepDynamicsAndUseParentConstraintIfNecessary_ReturnsCorrectBoneMappingAndLogCodes(GameObject avatarRoot, GameObject wearableRoot)
        {
            var result = EvaluateDresser(avatarRoot, wearableRoot, out var report, out var objectMappings, out var tags, StandardDresserSettings.DynamicsOptions.KeepDynamicsAndUseParentConstraintIfNecessary);
            Assert.True(result, "Hook should return true");

            PrintMappings(wearableRoot, objectMappings, tags);

            var expectedMappings = ExpectedKeepDynamicsAndUseParentConstraintIfNecessaryBoneMappings(wearableRoot.transform);
            Assert.AreEqual(expectedMappings.Length, objectMappings.Count, "Bone mapping length is not equal to expected length");
            Assert.AreEqual(0, tags.Count);

            foreach (var mapping in objectMappings)
            {
                Debug.Log("Checking: " + mapping.ToString());

                bool found = false;
                foreach (var expectedMapping in expectedMappings)
                {
                    if (expectedMapping.Equals(mapping))
                    {
                        found = true;
                        break;
                    }
                }

                Assert.True(found, "Could not find such mapping in expected bone mapping array: " + mapping.ToString());
            }
        }

        [Test]
        public void DynamicBone_KeepDynamicsAndUseParentConstraintIfNecessary_ReturnsCorrectBoneMappingAndLogCodes()
        {
            // DynamicBone has to be imported to run this test
            AssertPassImportedDynamicBone();

            var avatarRoot = InstantiateEditorTestPrefab("DTTest_DynamicBoneAvatar.prefab");
            var wearableRoot = InstantiateEditorTestPrefab("DTTest_DynamicBoneWearable.prefab");
            wearableRoot.transform.SetParent(avatarRoot.transform);

            AvatarDynamics_KeepDynamicsAndUseParentConstraintIfNecessary_ReturnsCorrectBoneMappingAndLogCodes(avatarRoot, wearableRoot);
        }

        [Test]
        public void PhysBone_KeepDynamicsAndUseParentConstraintIfNecessary_ReturnsCorrectBoneMappingAndLogCodes()
        {
            // VRCSDK3 has to be imported to run this test
            AssertPassImportedVRCSDK();

            var avatarRoot = InstantiateEditorTestPrefab("DTTest_PhysBoneAvatar.prefab");
            var wearableRoot = InstantiateEditorTestPrefab("DTTest_PhysBoneWearable.prefab");
            wearableRoot.transform.SetParent(avatarRoot.transform);

            AvatarDynamics_KeepDynamicsAndUseParentConstraintIfNecessary_ReturnsCorrectBoneMappingAndLogCodes(avatarRoot, wearableRoot);
        }

        private void AvatarDynamics_IgnoreTransform_ReturnsCorrectBoneMappingAndLogCodes(GameObject avatarRoot, GameObject wearableRoot)
        {
            var result = EvaluateDresser(avatarRoot, wearableRoot, out var report, out var objectMappings, out var tags, StandardDresserSettings.DynamicsOptions.IgnoreTransform);
            Assert.True(result, "Hook should return true");

            PrintMappings(wearableRoot, objectMappings, tags);

            var expectedMappings = ExpectedIgnoreTransformBoneMappings(wearableRoot.transform);
            Assert.AreEqual(expectedMappings.Length, objectMappings.Count, "Bone mapping length is not equal to expected length");
            Assert.AreEqual(0, tags.Count);

            foreach (var mapping in expectedMappings)
            {
                Debug.Log("Checking: " + mapping.ToString());

                bool found = false;
                foreach (var expectedMapping in expectedMappings)
                {
                    if (expectedMapping.Equals(mapping))
                    {
                        found = true;
                        break;
                    }
                }

                Assert.True(found, "Could not find such mapping in expected bone mapping array: " + mapping.ToString());
            }
        }

        [Test]
        public void DynamicBone_IgnoreTransform_ReturnsCorrectBoneMappingAndLogCodes()
        {
            // DynamicBone has to be imported to run this test
            AssertPassImportedDynamicBone();

            var avatarRoot = InstantiateEditorTestPrefab("DTTest_DynamicBoneAvatar.prefab");
            var wearableRoot = InstantiateEditorTestPrefab("DTTest_DynamicBoneWearable.prefab");
            wearableRoot.transform.SetParent(avatarRoot.transform);

            AvatarDynamics_IgnoreTransform_ReturnsCorrectBoneMappingAndLogCodes(avatarRoot, wearableRoot);
        }

        [Test]
        public void PhysBone_IgnoreTransform_ReturnsCorrectBoneMappingAndLogCodes()
        {
            // VRCSDK3 has to be imported to run this test
            AssertPassImportedVRCSDK();

            var avatarRoot = InstantiateEditorTestPrefab("DTTest_PhysBoneAvatar.prefab");
            var wearableRoot = InstantiateEditorTestPrefab("DTTest_PhysBoneWearable.prefab");
            wearableRoot.transform.SetParent(avatarRoot.transform);

            AvatarDynamics_IgnoreTransform_ReturnsCorrectBoneMappingAndLogCodes(avatarRoot, wearableRoot);
        }

        private void AvatarDynamics_CopyDynamics_ReturnsCorrectBoneMappingAndLogCodes(GameObject avatarRoot, GameObject wearableRoot)
        {
            var result = EvaluateDresser(avatarRoot, wearableRoot, out var report, out var objectMappings, out var tags, StandardDresserSettings.DynamicsOptions.CopyDynamics);
            Assert.True(result, "Hook should return true");

            PrintMappings(wearableRoot, objectMappings, tags);

            var expectedMappings = ExpectedCopyDynamicsBoneMappings(wearableRoot.transform);
            var expectedTags = ExpectedCopyDynamicsTags(wearableRoot.transform);
            Assert.AreEqual(expectedMappings.Length, objectMappings.Count, "Bone mapping length is not equal to expected length");
            Assert.AreEqual(expectedTags.Length, tags.Count);

            foreach (var mapping in objectMappings)
            {
                Debug.Log("Checking: " + mapping.ToString());

                bool found = false;
                foreach (var expectedMapping in expectedMappings)
                {
                    if (expectedMapping.Equals(mapping))
                    {
                        found = true;
                        break;
                    }
                }

                Assert.True(found, "Could not find such mapping in expected bone mapping array: " + mapping.ToString());
            }

            foreach (var tag in tags)
            {
                Debug.Log("Checking: " + tag.ToString());

                bool found = false;
                foreach (var expectedTag in expectedTags)
                {
                    if (expectedTag.Equals(tag))
                    {
                        found = true;
                        break;
                    }
                }

                Assert.True(found, "Could not find such tag in expected tag array: " + tag.ToString());
            }
        }

        [Test]
        public void DynamicBone_CopyDynamics_ReturnsCorrectBoneMappingAndLogCodes()
        {
            // DynamicBone has to be imported to run this test
            AssertPassImportedDynamicBone();

            var avatarRoot = InstantiateEditorTestPrefab("DTTest_DynamicBoneAvatar.prefab");
            var wearableRoot = InstantiateEditorTestPrefab("DTTest_DynamicBoneWearable.prefab");
            wearableRoot.transform.SetParent(avatarRoot.transform);

            AvatarDynamics_CopyDynamics_ReturnsCorrectBoneMappingAndLogCodes(avatarRoot, wearableRoot);
        }

        [Test]
        public void PhysBone_CopyDynamics_ReturnsCorrectBoneMappingAndLogCodes()
        {
            // VRCSDK3 has to be imported to run this test
            AssertPassImportedVRCSDK();

            var avatarRoot = InstantiateEditorTestPrefab("DTTest_PhysBoneAvatar.prefab");
            var wearableRoot = InstantiateEditorTestPrefab("DTTest_PhysBoneWearable.prefab");
            wearableRoot.transform.SetParent(avatarRoot.transform);

            AvatarDynamics_CopyDynamics_ReturnsCorrectBoneMappingAndLogCodes(avatarRoot, wearableRoot);
        }

        private void AvatarDynamics_IgnoreAll_ReturnsCorrectBoneMappingAndLogCodes(GameObject avatarRoot, GameObject wearableRoot)
        {
            var result = EvaluateDresser(avatarRoot, wearableRoot, out var report, out var objectMappings, out var tags, StandardDresserSettings.DynamicsOptions.IgnoreAll);
            Assert.True(result, "Hook should return true");

            PrintMappings(wearableRoot, objectMappings, tags);

            var expectedMappings = ExpectedIgnoreAllBoneMappings(wearableRoot.transform);
            Assert.AreEqual(expectedMappings.Length, objectMappings.Count, "Bone mapping length is not equal to expected length");
            Assert.AreEqual(0, tags.Count);

            foreach (var mapping in objectMappings)
            {
                Debug.Log("Checking: " + mapping.ToString());

                bool found = false;
                foreach (var expectedMapping in expectedMappings)
                {
                    if (expectedMapping.Equals(mapping))
                    {
                        found = true;
                        break;
                    }
                }

                Assert.True(found, "Could not find such mapping in expected bone mapping array: " + mapping.ToString());
            }
        }

        [Test]
        public void DynamicBone_IgnoreAll_ReturnsCorrectBoneMappingAndLogCodes()
        {
            // DynamicBone has to be imported to run this test
            AssertPassImportedDynamicBone();

            var avatarRoot = InstantiateEditorTestPrefab("DTTest_DynamicBoneAvatar.prefab");
            var wearableRoot = InstantiateEditorTestPrefab("DTTest_DynamicBoneWearable.prefab");
            wearableRoot.transform.SetParent(avatarRoot.transform);

            AvatarDynamics_IgnoreAll_ReturnsCorrectBoneMappingAndLogCodes(avatarRoot, wearableRoot);
        }

        [Test]
        public void PhysBone_IgnoreAll_ReturnsCorrectBoneMappingAndLogCodes()
        {
            // VRCSDK3 has to be imported to run this test
            AssertPassImportedVRCSDK();

            var avatarRoot = InstantiateEditorTestPrefab("DTTest_PhysBoneAvatar.prefab");
            var wearableRoot = InstantiateEditorTestPrefab("DTTest_PhysBoneWearable.prefab");
            wearableRoot.transform.SetParent(avatarRoot.transform);

            AvatarDynamics_IgnoreAll_ReturnsCorrectBoneMappingAndLogCodes(avatarRoot, wearableRoot);
        }
        #endregion Bone Mapping Tests

        [Test]
        public void AvatarMissingScripts_ReturnsCorrectErrorCode()
        {
            var avatarRoot = InstantiateEditorTestPrefab("DTTest_MissingScriptsObject.prefab");

            CreateRootWithArmatureAndHipsBone("Wearable", out var wearableRoot, out var wearableArmature, out var wearableHips);

            var result = EvaluateDresser(avatarRoot, wearableRoot, out var report, out _, out _);
            Assert.False(result, "Hook should return false");
            Assert.True(report.HasLogCode(StandardDresser.MessageCode.MissingScriptsDetectedInAvatar));
        }
    }
}
