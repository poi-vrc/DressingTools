using System.Collections.Generic;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Dresser;
using Chocopoi.DressingTools.Dresser.Default;
using Chocopoi.DressingTools.Dresser.Default.Hooks;
using Chocopoi.DressingTools.Logging;
using NUnit.Framework;
using UnityEngine;

namespace Chocopoi.DressingTools.Tests.Dresser.Default.Hooks
{
    public class ArmatureHookTest : DTTestBase
    {
        private bool EvaluateHook(GameObject avatarRoot, GameObject wearableRoot, out DTReport report, out List<DTBoneMapping> boneMappings, DTDefaultDresserDynamicsOption dynamicsOption = DTDefaultDresserDynamicsOption.RemoveDynamicsAndUseParentConstraint)
        {
            report = new DTReport();
            var settings = new DTDefaultDresserSettings();
            boneMappings = new List<DTBoneMapping>();
            var objectMappings = new List<DTObjectMapping>();
            var hook = new ArmatureHook();

            settings.targetAvatar = avatarRoot;
            settings.targetWearable = wearableRoot;
            settings.avatarArmatureName = "Armature";
            settings.wearableArmatureName = "Armature";
            settings.dynamicsOption = dynamicsOption;

            return hook.Evaluate(report, settings, boneMappings, objectMappings);
        }

        #region Log Code Tests
        [Test]
        public void NoAvatarArmature_ReturnsCorrectErrorCodes()
        {
            var avatarRoot = CreateGameObject("Avatar");
            var wearableRoot = CreateGameObject("Wearable");

            CreateGameObject("Armature", wearableRoot.transform);

            var result = EvaluateHook(avatarRoot, wearableRoot, out var report, out var boneMappings);
            Assert.False(result, "Hook should return false");
            Assert.True(report.HasLogCode(DTDefaultDresser.MessageCode.NoArmatureInAvatar));
        }

        [Test]
        public void GuessAvatarArmature_ReturnsCorrectLogCodes()
        {
            var avatarRoot = CreateGameObject("Avatar");
            var wearableRoot = CreateGameObject("Wearable");

            var avatarArmature = CreateGameObject("Armature ", avatarRoot.transform);
            var wearableArmature = CreateGameObject("Armature", wearableRoot.transform);

            CreateGameObject("Hips", avatarArmature.transform);
            CreateGameObject("Hips", wearableArmature.transform);

            var result = EvaluateHook(avatarRoot, wearableRoot, out var report, out var boneMappings);
            Assert.True(result, "Hook should return true");
            Assert.True(report.HasLogCode(DTDefaultDresser.MessageCode.AvatarArmatureObjectGuessed));
        }

        [Test]
        public void NoWearableArmature_ReturnsCorrectErrorCodes()
        {
            var avatarRoot = CreateGameObject("Avatar");
            var wearableRoot = CreateGameObject("Wearable");

            CreateGameObject("Armature", avatarRoot.transform);

            var result = EvaluateHook(avatarRoot, wearableRoot, out var report, out var boneMappings);
            Assert.False(result, "Hook should return false");
            Assert.True(report.HasLogCode(DTDefaultDresser.MessageCode.NoArmatureInWearable));
        }

        [Test]
        public void GuessWearableArmature_ReturnsCorrectLogCodes()
        {
            var avatarRoot = CreateGameObject("Avatar");
            var wearableRoot = CreateGameObject("Wearable");

            var avatarArmature = CreateGameObject("Armature", avatarRoot.transform);
            var wearableArmature = CreateGameObject("Armature ", wearableRoot.transform);

            CreateGameObject("Hips", avatarArmature.transform);
            CreateGameObject("Hips", wearableArmature.transform);

            var result = EvaluateHook(avatarRoot, wearableRoot, out var report, out var boneMappings);
            Assert.True(result, "Hook should return true");
            Assert.True(report.HasLogCode(DTDefaultDresser.MessageCode.WearableArmatureObjectGuessed));
        }

        [Test]
        public void NoBonesInAvatarArmature_ReturnsCorrectErrorCodes()
        {
            var avatarRoot = CreateGameObject("Avatar");
            var wearableRoot = CreateGameObject("Wearable");

            var avatarArmature = CreateGameObject("Armature", avatarRoot.transform);
            var wearableArmature = CreateGameObject("Armature", wearableRoot.transform);

            CreateGameObject("Hips", wearableArmature.transform);

            var result = EvaluateHook(avatarRoot, wearableRoot, out var report, out var boneMappings);
            Assert.False(result, "Hook should return false");
            Assert.True(report.HasLogCode(DTDefaultDresser.MessageCode.NoBonesInAvatarArmatureFirstLevel));
        }

        [Test]
        public void NoBonesInWearableArmature_ReturnsCorrectErrorCodes()
        {
            var avatarRoot = CreateGameObject("Avatar");
            var wearableRoot = CreateGameObject("Wearable");

            var avatarArmature = CreateGameObject("Armature", avatarRoot.transform);
            var wearableArmature = CreateGameObject("Armature", wearableRoot.transform);

            CreateGameObject("Hips", avatarArmature.transform);

            var result = EvaluateHook(avatarRoot, wearableRoot, out var report, out var boneMappings);
            Assert.False(result, "Hook should return false");
            Assert.True(report.HasLogCode(DTDefaultDresser.MessageCode.NoBonesInWearableArmatureFirstLevel));
        }

        [Test]
        public void OnlyOneEnabledBoneInAvatarArmature_ReturnsCorrectLogCodes()
        {
            CreateRootWithArmatureAndHipsBone("Avatar", out var avatarRoot, out var avatarArmature, out var avatarHips);
            CreateRootWithArmatureAndHipsBone("Wearable", out var wearableRoot, out var wearableArmature, out var wearableHips);

            var obj = CreateGameObject("A", avatarArmature.transform);
            obj.SetActive(false);

            var result = EvaluateHook(avatarRoot, wearableRoot, out var report, out var boneMappings);
            Assert.True(result, "Hook should return true");
            Assert.True(report.HasLogCode(DTDefaultDresser.MessageCode.MultipleBonesInAvatarArmatureDetectedWarningRemoved));
        }

        [Test]
        public void MoreThanOneEnabledBoneInAvatarArmature_ReturnsCorrectLogCodes()
        {
            CreateRootWithArmatureAndHipsBone("Avatar", out var avatarRoot, out var avatarArmature, out var avatarHips);
            CreateRootWithArmatureAndHipsBone("Wearable", out var wearableRoot, out var wearableArmature, out var wearableHips);

            var obj = CreateGameObject("A", avatarArmature.transform);

            var result = EvaluateHook(avatarRoot, wearableRoot, out var report, out var boneMappings);
            Assert.True(result, "Hook should return true");
            Assert.True(report.HasLogCode(DTDefaultDresser.MessageCode.MultipleBonesInAvatarArmatureFirstLevel));
        }

        [Test]
        public void MoreThanOneEnabledBoneInWearableArmature_ReturnsCorrectLogCodes()
        {
            CreateRootWithArmatureAndHipsBone("Avatar", out var avatarRoot, out var avatarArmature, out var avatarHips);
            CreateRootWithArmatureAndHipsBone("Wearable", out var wearableRoot, out var wearableArmature, out var wearableHips);

            var obj = CreateGameObject("A", wearableArmature.transform);

            var result = EvaluateHook(avatarRoot, wearableRoot, out var report, out var boneMappings);
            Assert.True(result, "Hook should return true");
            Assert.True(report.HasLogCode(DTDefaultDresser.MessageCode.MultipleBonesInWearableArmatureFirstLevel));
        }

        [Test]
        public void SkipWearableBoneDTContainer_ReturnsCorrectLogCodes()
        {
            CreateRootWithArmatureAndHipsBone("Avatar", out var avatarRoot, out var avatarArmature, out var avatarHips);
            CreateRootWithArmatureAndHipsBone("Wearable", out var wearableRoot, out var wearableArmature, out var wearableHips);

            CreateGameObject("Hips_DT", wearableHips.transform);

            var result = EvaluateHook(avatarRoot, wearableRoot, out var report, out var boneMappings);
            Assert.True(result, "Hook should return true");
        }

        [Test]
        public void NonMatchingWearableBonesKeptUntouched_ReturnsCorrectLogCodes()
        {
            CreateRootWithArmatureAndHipsBone("Avatar", out var avatarRoot, out var avatarArmature, out var avatarHips);
            CreateRootWithArmatureAndHipsBone("Wearable", out var wearableRoot, out var wearableArmature, out var wearableHips);

            CreateGameObject("MyBone", wearableHips.transform);

            var result = EvaluateHook(avatarRoot, wearableRoot, out var report, out var boneMappings);
            Assert.True(result, "Hook should return true");
            Assert.True(report.HasLogCode(DTDefaultDresser.MessageCode.NonMatchingWearableBoneKeptUntouched));
        }
        #endregion Log Code Tests

        #region Expected Bone Mappings (Identical for either PhysBone or DynamicBones)
        // expected bone mappings
        private static readonly DTBoneMapping[] ExpectedRemoveDynamicsAndUseParentConstraintsBoneMapping =
        {
            new DTBoneMapping() { mappingType = DTBoneMappingType.MoveToBone, avatarBonePath = "Armature/Hips", wearableBonePath = "Armature/Hips" },
            new DTBoneMapping() { mappingType = DTBoneMappingType.MoveToBone, avatarBonePath = "Armature/Hips/MyBone", wearableBonePath = "Armature/Hips/MyBone" },
            new DTBoneMapping() { mappingType = DTBoneMappingType.ParentConstraint, avatarBonePath = "Armature/Hips/MyDynBone", wearableBonePath = "Armature/Hips/MyDynBone" },
            new DTBoneMapping() { mappingType = DTBoneMappingType.ParentConstraint, avatarBonePath = "Armature/Hips/MyDynBone/MyDynBone1", wearableBonePath = "Armature/Hips/MyDynBone/MyDynBone1" },
            new DTBoneMapping() { mappingType = DTBoneMappingType.ParentConstraint, avatarBonePath = "Armature/Hips/MyDynBone/MyDynBone2", wearableBonePath = "Armature/Hips/MyDynBone/MyDynBone2" },
            new DTBoneMapping() { mappingType = DTBoneMappingType.ParentConstraint, avatarBonePath = "Armature/Hips/MyAnotherDynBone", wearableBonePath = "Armature/Hips/MyAnotherDynBone" },
            new DTBoneMapping() { mappingType = DTBoneMappingType.ParentConstraint, avatarBonePath = "Armature/Hips/MyAnotherDynBone/MyAnotherDynBone1", wearableBonePath = "Armature/Hips/MyAnotherDynBone/MyAnotherDynBone1" },
            new DTBoneMapping() { mappingType = DTBoneMappingType.ParentConstraint, avatarBonePath = "Armature/Hips/MyAnotherDynBone/MyAnotherDynBone2", wearableBonePath = "Armature/Hips/MyAnotherDynBone/MyAnotherDynBone2" },
        };

        private static readonly DTBoneMapping[] ExpectedKeepDynamicsAndUseParentConstraintIfNecessaryBoneMapping =
        {
            new DTBoneMapping() { mappingType = DTBoneMappingType.MoveToBone, avatarBonePath = "Armature/Hips", wearableBonePath = "Armature/Hips" },
            new DTBoneMapping() { mappingType = DTBoneMappingType.MoveToBone, avatarBonePath = "Armature/Hips/MyBone", wearableBonePath = "Armature/Hips/MyBone" },
            new DTBoneMapping() { mappingType = DTBoneMappingType.ParentConstraint, avatarBonePath = "Armature/Hips/MyAnotherDynBone", wearableBonePath = "Armature/Hips/MyAnotherDynBone" },
            new DTBoneMapping() { mappingType = DTBoneMappingType.ParentConstraint, avatarBonePath = "Armature/Hips/MyAnotherDynBone/MyAnotherDynBone1", wearableBonePath = "Armature/Hips/MyAnotherDynBone/MyAnotherDynBone1" },
            new DTBoneMapping() { mappingType = DTBoneMappingType.ParentConstraint, avatarBonePath = "Armature/Hips/MyAnotherDynBone/MyAnotherDynBone2", wearableBonePath = "Armature/Hips/MyAnotherDynBone/MyAnotherDynBone2" },
        };

        private static readonly DTBoneMapping[] ExpectedIgnoreTransformBoneMapping =
        {
            new DTBoneMapping() { mappingType = DTBoneMappingType.MoveToBone, avatarBonePath = "Armature/Hips", wearableBonePath = "Armature/Hips" },
            new DTBoneMapping() { mappingType = DTBoneMappingType.MoveToBone, avatarBonePath = "Armature/Hips/MyBone", wearableBonePath = "Armature/Hips/MyBone" },
            new DTBoneMapping() { mappingType = DTBoneMappingType.IgnoreTransform, avatarBonePath = "Armature/Hips/MyDynBone", wearableBonePath = "Armature/Hips/MyDynBone" },
            new DTBoneMapping() { mappingType = DTBoneMappingType.IgnoreTransform, avatarBonePath = "Armature/Hips/MyDynBone/MyDynBone1", wearableBonePath = "Armature/Hips/MyDynBone/MyDynBone1" },
            new DTBoneMapping() { mappingType = DTBoneMappingType.IgnoreTransform, avatarBonePath = "Armature/Hips/MyDynBone/MyDynBone2", wearableBonePath = "Armature/Hips/MyDynBone/MyDynBone2" },
            new DTBoneMapping() { mappingType = DTBoneMappingType.IgnoreTransform, avatarBonePath = "Armature/Hips/MyAnotherDynBone", wearableBonePath = "Armature/Hips/MyAnotherDynBone" },
            new DTBoneMapping() { mappingType = DTBoneMappingType.IgnoreTransform, avatarBonePath = "Armature/Hips/MyAnotherDynBone/MyAnotherDynBone1", wearableBonePath = "Armature/Hips/MyAnotherDynBone/MyAnotherDynBone1" },
            new DTBoneMapping() { mappingType = DTBoneMappingType.IgnoreTransform, avatarBonePath = "Armature/Hips/MyAnotherDynBone/MyAnotherDynBone2", wearableBonePath = "Armature/Hips/MyAnotherDynBone/MyAnotherDynBone2" },
        };

        private static readonly DTBoneMapping[] ExpectedCopyDynamicsBoneMapping =
        {
            new DTBoneMapping() { mappingType = DTBoneMappingType.MoveToBone, avatarBonePath = "Armature/Hips", wearableBonePath = "Armature/Hips" },
            new DTBoneMapping() { mappingType = DTBoneMappingType.MoveToBone, avatarBonePath = "Armature/Hips/MyBone", wearableBonePath = "Armature/Hips/MyBone" },
            new DTBoneMapping() { mappingType = DTBoneMappingType.CopyDynamics, avatarBonePath = "Armature/Hips/MyDynBone", wearableBonePath = "Armature/Hips/MyDynBone" },
            new DTBoneMapping() { mappingType = DTBoneMappingType.CopyDynamics, avatarBonePath = "Armature/Hips/MyAnotherDynBone", wearableBonePath = "Armature/Hips/MyAnotherDynBone" },
        };

        private static readonly DTBoneMapping[] ExpectedIgnoreAllBoneMapping =
        {
            new DTBoneMapping() { mappingType = DTBoneMappingType.MoveToBone, avatarBonePath = "Armature/Hips", wearableBonePath = "Armature/Hips" },
            new DTBoneMapping() { mappingType = DTBoneMappingType.MoveToBone, avatarBonePath = "Armature/Hips/MyBone", wearableBonePath = "Armature/Hips/MyBone" },
            new DTBoneMapping() { mappingType = DTBoneMappingType.DoNothing, avatarBonePath = "Armature/Hips/MyDynBone", wearableBonePath = "Armature/Hips/MyDynBone" },
            new DTBoneMapping() { mappingType = DTBoneMappingType.DoNothing, avatarBonePath = "Armature/Hips/MyAnotherDynBone", wearableBonePath = "Armature/Hips/MyAnotherDynBone" },
        };
        #endregion Expected Bone Mappings (Identical for either PhysBone or DynamicBones)

        #region Bone Mapping Tests
        private void AvatarDynamics_RemoveDynamicsAndUseParentConstraints_ReturnsCorrectBoneMappingAndLogCodes(GameObject avatarRoot, GameObject wearableRoot)
        {
            var result = EvaluateHook(avatarRoot, wearableRoot, out var report, out var boneMappings, DTDefaultDresserDynamicsOption.RemoveDynamicsAndUseParentConstraint);
            Assert.True(result, "Hook should return true");

            Assert.AreEqual(ExpectedRemoveDynamicsAndUseParentConstraintsBoneMapping.Length, boneMappings.Count, "Bone mapping length is not equal to expected length");

            foreach (var mapping in boneMappings)
            {
                Debug.Log("Checking: " + mapping.ToString());

                bool found = false;
                foreach (var expectedMapping in ExpectedRemoveDynamicsAndUseParentConstraintsBoneMapping)
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
            Assert.NotNull(DTUtils.FindType("DynamicBone"), "This test requires DynamicBones to be imported");

            var avatarRoot = InstantiateEditorTestPrefab("DTTest_DynamicBoneAvatar.prefab");
            var wearableRoot = InstantiateEditorTestPrefab("DTTest_DynamicBoneWearable.prefab");

            AvatarDynamics_RemoveDynamicsAndUseParentConstraints_ReturnsCorrectBoneMappingAndLogCodes(avatarRoot, wearableRoot);
        }

        [Test]
        public void PhysBone_RemoveDynamicsAndUseParentConstraints_ReturnsCorrectBoneMappingAndLogCodes()
        {
#if !VRC_SDK_VRCSDK3
            // VRCSDK3 has to be imported to run this test
            Assert.Fail("This test requires VRCSDK3 (>=2022.04.21.03.29) to be imported");
#endif
            var avatarRoot = InstantiateEditorTestPrefab("DTTest_PhysBoneAvatar.prefab");
            var wearableRoot = InstantiateEditorTestPrefab("DTTest_PhysBoneWearable.prefab");

            AvatarDynamics_RemoveDynamicsAndUseParentConstraints_ReturnsCorrectBoneMappingAndLogCodes(avatarRoot, wearableRoot);
        }

        private void AvatarDynamics_KeepDynamicsAndUseParentConstraintIfNecessary_ReturnsCorrectBoneMappingAndLogCodes(GameObject avatarRoot, GameObject wearableRoot)
        {
            var result = EvaluateHook(avatarRoot, wearableRoot, out var report, out var boneMappings, DTDefaultDresserDynamicsOption.KeepDynamicsAndUseParentConstraintIfNecessary);
            Assert.True(result, "Hook should return true");

            Assert.AreEqual(ExpectedKeepDynamicsAndUseParentConstraintIfNecessaryBoneMapping.Length, boneMappings.Count, "Bone mapping length is not equal to expected length");

            foreach (var mapping in boneMappings)
            {
                Debug.Log("Checking: " + mapping.ToString());

                bool found = false;
                foreach (var expectedMapping in ExpectedKeepDynamicsAndUseParentConstraintIfNecessaryBoneMapping)
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
            Assert.NotNull(DTUtils.FindType("DynamicBone"), "This test requires DynamicBones to be imported");

            var avatarRoot = InstantiateEditorTestPrefab("DTTest_DynamicBoneAvatar.prefab");
            var wearableRoot = InstantiateEditorTestPrefab("DTTest_DynamicBoneWearable.prefab");

            AvatarDynamics_KeepDynamicsAndUseParentConstraintIfNecessary_ReturnsCorrectBoneMappingAndLogCodes(avatarRoot, wearableRoot);
        }

        [Test]
        public void PhysBone_KeepDynamicsAndUseParentConstraintIfNecessary_ReturnsCorrectBoneMappingAndLogCodes()
        {
#if !VRC_SDK_VRCSDK3
            // VRCSDK3 has to be imported to run this test
            Assert.Fail("This test requires VRCSDK3 (>=2022.04.21.03.29) to be imported");
#endif
            var avatarRoot = InstantiateEditorTestPrefab("DTTest_PhysBoneAvatar.prefab");
            var wearableRoot = InstantiateEditorTestPrefab("DTTest_PhysBoneWearable.prefab");

            AvatarDynamics_KeepDynamicsAndUseParentConstraintIfNecessary_ReturnsCorrectBoneMappingAndLogCodes(avatarRoot, wearableRoot);
        }

        private void AvatarDynamics_IgnoreTransform_ReturnsCorrectBoneMappingAndLogCodes(GameObject avatarRoot, GameObject wearableRoot)
        {
            var result = EvaluateHook(avatarRoot, wearableRoot, out var report, out var boneMappings, DTDefaultDresserDynamicsOption.IgnoreTransform);
            Assert.True(result, "Hook should return true");

            Assert.AreEqual(ExpectedIgnoreTransformBoneMapping.Length, boneMappings.Count, "Bone mapping length is not equal to expected length");

            foreach (var mapping in boneMappings)
            {
                Debug.Log("Checking: " + mapping.ToString());

                bool found = false;
                foreach (var expectedMapping in ExpectedIgnoreTransformBoneMapping)
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
            Assert.NotNull(DTUtils.FindType("DynamicBone"), "This test requires DynamicBones to be imported");

            var avatarRoot = InstantiateEditorTestPrefab("DTTest_DynamicBoneAvatar.prefab");
            var wearableRoot = InstantiateEditorTestPrefab("DTTest_DynamicBoneWearable.prefab");

            AvatarDynamics_IgnoreTransform_ReturnsCorrectBoneMappingAndLogCodes(avatarRoot, wearableRoot);
        }

        [Test]
        public void PhysBone_IgnoreTransform_ReturnsCorrectBoneMappingAndLogCodes()
        {
#if !VRC_SDK_VRCSDK3
            // VRCSDK3 has to be imported to run this test
            Assert.Fail("This test requires VRCSDK3 (>=2022.04.21.03.29) to be imported");
#endif
            var avatarRoot = InstantiateEditorTestPrefab("DTTest_PhysBoneAvatar.prefab");
            var wearableRoot = InstantiateEditorTestPrefab("DTTest_PhysBoneWearable.prefab");

            AvatarDynamics_IgnoreTransform_ReturnsCorrectBoneMappingAndLogCodes(avatarRoot, wearableRoot);
        }

        private void AvatarDynamics_CopyDynamics_ReturnsCorrectBoneMappingAndLogCodes(GameObject avatarRoot, GameObject wearableRoot)
        {
            var result = EvaluateHook(avatarRoot, wearableRoot, out var report, out var boneMappings, DTDefaultDresserDynamicsOption.CopyDynamics);
            Assert.True(result, "Hook should return true");

            Assert.AreEqual(ExpectedCopyDynamicsBoneMapping.Length, boneMappings.Count, "Bone mapping length is not equal to expected length");

            foreach (var mapping in boneMappings)
            {
                Debug.Log("Checking: " + mapping.ToString());

                bool found = false;
                foreach (var expectedMapping in ExpectedCopyDynamicsBoneMapping)
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
        public void DynamicBone_CopyDynamics_ReturnsCorrectBoneMappingAndLogCodes()
        {
            // DynamicBone has to be imported to run this test
            Assert.NotNull(DTUtils.FindType("DynamicBone"), "This test requires DynamicBones to be imported");

            var avatarRoot = InstantiateEditorTestPrefab("DTTest_DynamicBoneAvatar.prefab");
            var wearableRoot = InstantiateEditorTestPrefab("DTTest_DynamicBoneWearable.prefab");

            AvatarDynamics_CopyDynamics_ReturnsCorrectBoneMappingAndLogCodes(avatarRoot, wearableRoot);
        }

        [Test]
        public void PhysBone_CopyDynamics_ReturnsCorrectBoneMappingAndLogCodes()
        {
#if !VRC_SDK_VRCSDK3
            // VRCSDK3 has to be imported to run this test
            Assert.Fail("This test requires VRCSDK3 (>=2022.04.21.03.29) to be imported");
#endif
            var avatarRoot = InstantiateEditorTestPrefab("DTTest_PhysBoneAvatar.prefab");
            var wearableRoot = InstantiateEditorTestPrefab("DTTest_PhysBoneWearable.prefab");

            AvatarDynamics_CopyDynamics_ReturnsCorrectBoneMappingAndLogCodes(avatarRoot, wearableRoot);
        }

        private void AvatarDynamics_IgnoreAll_ReturnsCorrectBoneMappingAndLogCodes(GameObject avatarRoot, GameObject wearableRoot)
        {
            var result = EvaluateHook(avatarRoot, wearableRoot, out var report, out var boneMappings, DTDefaultDresserDynamicsOption.IgnoreAll);
            Assert.True(result, "Hook should return true");

            Assert.AreEqual(ExpectedIgnoreAllBoneMapping.Length, boneMappings.Count, "Bone mapping length is not equal to expected length");

            foreach (var mapping in boneMappings)
            {
                Debug.Log("Checking: " + mapping.ToString());

                bool found = false;
                foreach (var expectedMapping in ExpectedIgnoreAllBoneMapping)
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
            Assert.NotNull(DTUtils.FindType("DynamicBone"), "This test requires DynamicBones to be imported");

            var avatarRoot = InstantiateEditorTestPrefab("DTTest_DynamicBoneAvatar.prefab");
            var wearableRoot = InstantiateEditorTestPrefab("DTTest_DynamicBoneWearable.prefab");

            AvatarDynamics_IgnoreAll_ReturnsCorrectBoneMappingAndLogCodes(avatarRoot, wearableRoot);
        }

        [Test]
        public void PhysBone_IgnoreAll_ReturnsCorrectBoneMappingAndLogCodes()
        {
#if !VRC_SDK_VRCSDK3
            // VRCSDK3 has to be imported to run this test
            Assert.Fail("This test requires VRCSDK3 (>=2022.04.21.03.29) to be imported");
#endif
            var avatarRoot = InstantiateEditorTestPrefab("DTTest_PhysBoneAvatar.prefab");
            var wearableRoot = InstantiateEditorTestPrefab("DTTest_PhysBoneWearable.prefab");

            AvatarDynamics_IgnoreAll_ReturnsCorrectBoneMappingAndLogCodes(avatarRoot, wearableRoot);
        }
        #endregion Bone Mapping Tests
    }
}
