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

using System.Linq;
using Chocopoi.DressingFramework.Detail.DK;
using Chocopoi.DressingTools.Components.Modifiers;
using Chocopoi.DressingTools.Passes.Modifiers;
using NUnit.Framework;
using UnityEngine;

namespace Chocopoi.DressingTools.Tests.Passes.Modifiers
{
    internal class ArmatureMappingPassTest : EditorTestBase
    {
        [Test]
        public void MapAutoTest()
        {
            // This test requires PhysBone
            AssertPassImportedVRCSDK();

            var pass = new ArmatureMappingPass();

            var avatarObj = InstantiateEditorTestPrefab("DTTest_MapAutoAvatar.prefab");
            var wearableArmature = avatarObj.transform.Find("Wearable/Armature");
            Assert.NotNull(wearableArmature);
            var wearableArmatureHips = avatarObj.transform.Find("Wearable/Armature/Hips");
            Assert.NotNull(wearableArmatureHips);
            var wearableArmatureMyBone = avatarObj.transform.Find("Wearable/Armature/Hips/MyBone");
            Assert.NotNull(wearableArmatureMyBone);
            var wearableArmatureDynBone = avatarObj.transform.Find("Wearable/Armature/Hips/MyDynBone");
            Assert.NotNull(wearableArmatureDynBone);
            var ctx = new DKNativeContext(avatarObj);

            var armMapComp = wearableArmature.gameObject.AddComponent<DTArmatureMapping>();
            armMapComp.Mode = DTArmatureMapping.MappingMode.Auto;
            armMapComp.DresserType = DTArmatureMapping.DresserTypes.Default;
            // dynamics option auto mode will change to IgnoreTransform, so we explicitly set here
            armMapComp.DresserDefaultConfig.DynamicsOption = DTArmatureMapping.AMDresserDefaultConfig.DynamicsOptions.RemoveDynamicsAndUseParentConstraint;
            armMapComp.GroupBones = true;
            armMapComp.SourceArmature = wearableArmature;
            armMapComp.TargetArmaturePath = "Armature";

            pass.Invoke(ctx);

            Assert.True(armMapComp.TryGetComponent<DTObjectMapping>(out var objMapComp));
            objMapComp.Mappings.ForEach(m => Debug.Log(m));

            Assert.AreEqual(1, objMapComp.Mappings
                .Where(m =>
                    m.Type == DTObjectMapping.Mapping.MappingType.MoveToBone &&
                    m.SourceTransform == wearableArmatureHips &&
                    m.TargetPath == "Armature/Hips")
                .Count());
            Assert.AreEqual(1, objMapComp.Mappings
                .Where(m =>
                    m.Type == DTObjectMapping.Mapping.MappingType.MoveToBone &&
                    m.SourceTransform == wearableArmatureMyBone &&
                    m.TargetPath == "Armature/Hips/MyBone")
                .Count());
            Assert.AreEqual(1, objMapComp.Mappings
                .Where(m =>
                    m.Type == DTObjectMapping.Mapping.MappingType.ParentConstraint &&
                    m.SourceTransform == wearableArmatureDynBone &&
                    m.TargetPath == "Armature/Hips/MyDynBone")
                .Count());
        }

        [Test]
        public void MapOverrideTest()
        {
            // This test requires PhysBone
            AssertPassImportedVRCSDK();

            var pass = new ArmatureMappingPass();

            var avatarObj = InstantiateEditorTestPrefab("DTTest_MapOverrideAvatar.prefab");
            var wearableArmature = avatarObj.transform.Find("Wearable/Armature");
            Assert.NotNull(wearableArmature);
            var wearableArmatureHips = avatarObj.transform.Find("Wearable/Armature/Hips");
            Assert.NotNull(wearableArmatureHips);
            var wearableArmatureMyBone = avatarObj.transform.Find("Wearable/Armature/Hips/MyBone");
            Assert.NotNull(wearableArmatureMyBone);
            var wearableArmatureDynBone = avatarObj.transform.Find("Wearable/Armature/Hips/MyDynBone");
            Assert.NotNull(wearableArmatureDynBone);
            var ctx = new DKNativeContext(avatarObj);

            var armMapComp = wearableArmature.gameObject.AddComponent<DTArmatureMapping>();
            armMapComp.Mode = DTArmatureMapping.MappingMode.Override;
            armMapComp.DresserType = DTArmatureMapping.DresserTypes.Default;
            armMapComp.DresserDefaultConfig.DynamicsOption = DTArmatureMapping.AMDresserDefaultConfig.DynamicsOptions.IgnoreTransform;
            armMapComp.GroupBones = true;
            armMapComp.SourceArmature = wearableArmature;
            armMapComp.TargetArmaturePath = "Armature";
            armMapComp.Mappings.Add(new DTObjectMapping.Mapping()
            {
                Type = DTObjectMapping.Mapping.MappingType.DoNothing,
                SourceTransform = wearableArmatureMyBone
            });
            var overrideTag = new DTArmatureMapping.Tag
            {
                Type = DTArmatureMapping.Tag.TagType.CopyDynamics,
                SourceTransform = wearableArmatureDynBone,
                TargetPath = "Armature/Hips/MyDynBone"
            };
            armMapComp.Tags.Add(overrideTag);

            pass.Invoke(ctx);

            Assert.True(armMapComp.TryGetComponent<DTObjectMapping>(out var objMapComp));
            objMapComp.Mappings.ForEach(m => Debug.Log(m));

            Assert.AreEqual(1, objMapComp.Mappings
                .Where(m =>
                    m.Type == DTObjectMapping.Mapping.MappingType.MoveToBone &&
                    m.SourceTransform == wearableArmatureHips &&
                    m.TargetPath == "Armature/Hips")
                .Count());
            Assert.AreEqual(1, objMapComp.Mappings
                .Where(m =>
                    m.Type == DTObjectMapping.Mapping.MappingType.DoNothing &&
                    m.SourceTransform == wearableArmatureMyBone)
                .Count());
            Assert.True(wearableArmatureDynBone.TryGetComponent<DTCopyDynamics>(out var copyDynComp));
            Assert.AreEqual(DTCopyDynamics.DynamicsSearchMode.ControlRoot, copyDynComp.SourceSearchMode);
            Assert.AreEqual("Armature/Hips/MyDynBone", copyDynComp.SourcePath);
        }

        [Test]
        public void MapManualTest()
        {
            var pass = new ArmatureMappingPass();

            var avatarObj = InstantiateEditorTestPrefab("DTTest_MapManualAvatar.prefab");
            var wearableArmature = avatarObj.transform.Find("Wearable/Armature");
            Assert.NotNull(wearableArmature);
            var wearableArmatureHips = avatarObj.transform.Find("Wearable/Armature/Hips");
            Assert.NotNull(wearableArmatureHips);
            var wearableArmatureDynBone = avatarObj.transform.Find("Wearable/Armature/Hips/MyDynBone");
            Assert.NotNull(wearableArmatureDynBone);
            var ctx = new DKNativeContext(avatarObj);

            var armMapComp = wearableArmature.gameObject.AddComponent<DTArmatureMapping>();
            armMapComp.Mode = DTArmatureMapping.MappingMode.Manual;
            armMapComp.GroupBones = true;
            armMapComp.SourceArmature = wearableArmature;
            armMapComp.TargetArmaturePath = "Armature";

            armMapComp.Mappings.Add(new DTObjectMapping.Mapping()
            {
                Type = DTObjectMapping.Mapping.MappingType.MoveToBone,
                SourceTransform = wearableArmatureHips,
                TargetPath = "Armature/Hips"
            });
            var newTag = new DTArmatureMapping.Tag()
            {
                Type = DTArmatureMapping.Tag.TagType.CopyDynamics,
                SourceTransform = wearableArmatureDynBone,
                TargetPath = "Armature/Hips/MyDynBone"
            };
            armMapComp.Tags.Add(newTag);

            pass.Invoke(ctx);

            Assert.True(armMapComp.TryGetComponent<DTObjectMapping>(out var objMapComp));
            objMapComp.Mappings.ForEach(m => Debug.Log(m));

            Assert.AreEqual(1, objMapComp.Mappings
                .Where(m =>
                    m.Type == DTObjectMapping.Mapping.MappingType.MoveToBone &&
                    m.SourceTransform == wearableArmatureHips &&
                    m.TargetPath == "Armature/Hips")
                .Count());
            Assert.True(wearableArmatureDynBone.TryGetComponent<DTCopyDynamics>(out var copyDynComp));
            Assert.AreEqual(DTCopyDynamics.DynamicsSearchMode.ControlRoot, copyDynComp.SourceSearchMode);
            Assert.AreEqual("Armature/Hips/MyDynBone", copyDynComp.SourcePath);
        }
    }
}
