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
using System.Collections.ObjectModel;
using System.Linq;
using Chocopoi.DressingTools.Components.Modifiers;
using Chocopoi.DressingTools.OneConf.Wearable.Modules;
using Chocopoi.DressingTools.OneConf.Wearable.Modules.BuiltIn;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Chocopoi.DressingTools.Tests.OneConf.Wearable.Modules
{
    internal class ArmatureMappingWearableModuleProviderTest : EditorTestBase
    {
        [Test]
        public void DeserializeModuleConfigTest()
        {
            var provider = new ArmatureMappingWearableModuleProvider();
            var makeUpConfig = new ArmatureMappingWearableModuleConfig();
            var deserializedConfig = (ArmatureMappingWearableModuleConfig)provider.DeserializeModuleConfig(JObject.Parse(JsonConvert.SerializeObject(makeUpConfig)));

            Assert.AreEqual(makeUpConfig.version.ToString(), deserializedConfig.version.ToString());
            Assert.AreEqual(makeUpConfig.boneMappingMode, deserializedConfig.boneMappingMode);
        }

        [Test]
        public void NewModuleConfig()
        {
            var provider = new ArmatureMappingWearableModuleProvider();
            Assert.IsInstanceOf(typeof(ArmatureMappingWearableModuleConfig), provider.NewModuleConfig());
        }

        private static void PrintMappingsAndTags(List<DTObjectMapping.Mapping> mappings, List<DTArmatureMapping.Tag> tags)
        {
            Debug.Log("Mappings:");
            foreach (var mapping in mappings)
            {
                Debug.Log(mapping);
            }
            Debug.Log("Tags:");
            foreach (var tag in tags)
            {
                Debug.Log(tag);
            }
        }

        [Test]
        public void MapAutoTest()
        {
            // TODO: dynbone check?
            // This test requires PhysBone
            AssertPassImportedVRCSDK();

            var provider = new ArmatureMappingWearableModuleProvider();

            var avatarObj = InstantiateEditorTestPrefab("DTTest_MapAutoAvatar.prefab");
            var wearableTrans = avatarObj.transform.Find("Wearable");
            Assert.NotNull(wearableTrans);
            var wearableArmature = avatarObj.transform.Find("Wearable/Armature");
            Assert.NotNull(wearableArmature);

            var cabCtx = CreateCabinetContext(avatarObj);
            var wearCtx = CreateWearableContext(cabCtx, wearableTrans.gameObject);
            var amm = wearCtx.wearableConfig.FindModuleConfig<ArmatureMappingWearableModuleConfig>();

            Assert.True(provider.Invoke(cabCtx, wearCtx, new ReadOnlyCollection<WearableModule>(new List<WearableModule>() { new WearableModule() {
                moduleName = ArmatureMappingWearableModuleConfig.ModuleIdentifier,
                config = amm
            }}), false));

            Assert.True(wearableTrans.TryGetComponent<DTArmatureMapping>(out var comp));
            Assert.AreEqual(DTArmatureMapping.MappingMode.Auto, comp.Mode);
            Assert.AreEqual(DTArmatureMapping.DresserTypes.Default, comp.DresserType);
            Assert.AreEqual("Armature", comp.TargetArmaturePath);
            Assert.AreEqual(wearableArmature, comp.SourceArmature);
            Assert.AreEqual(DTArmatureMapping.AMDresserDefaultConfig.DynamicsOptions.RemoveDynamicsAndUseParentConstraint, comp.DresserDefaultConfig.DynamicsOption);
            Assert.AreEqual(0, comp.Mappings.Count);
            Assert.AreEqual(0, comp.Tags.Count);
        }

        [Test]
        public void MapOverrideTest()
        {
            // TODO: dynbone check?
            // This test requires PhysBone
            AssertPassImportedVRCSDK();

            var provider = new ArmatureMappingWearableModuleProvider();

            var avatarObj = InstantiateEditorTestPrefab("DTTest_MapOverrideAvatar.prefab");
            var wearableTrans = avatarObj.transform.Find("Wearable");
            Assert.NotNull(wearableTrans);
            var wearableArmature = avatarObj.transform.Find("Wearable/Armature");
            Assert.NotNull(wearableArmature);
            var wearableMyBone = avatarObj.transform.Find("Wearable/Armature/Hips/MyBone");
            Assert.NotNull(wearableMyBone);

            var cabCtx = CreateCabinetContext(avatarObj);
            var wearCtx = CreateWearableContext(cabCtx, wearableTrans.gameObject);
            var amm = wearCtx.wearableConfig.FindModuleConfig<ArmatureMappingWearableModuleConfig>();

            Assert.True(provider.Invoke(cabCtx, wearCtx, new ReadOnlyCollection<WearableModule>(new List<WearableModule>() { new WearableModule() {
                moduleName = ArmatureMappingWearableModuleConfig.ModuleIdentifier,
                config = amm
            }}), false));

            Assert.True(wearableTrans.TryGetComponent<DTArmatureMapping>(out var comp));
            Assert.AreEqual(DTArmatureMapping.MappingMode.Override, comp.Mode);
            Assert.AreEqual(DTArmatureMapping.DresserTypes.Default, comp.DresserType);
            Assert.AreEqual("Armature", comp.TargetArmaturePath);
            Assert.AreEqual(wearableArmature, comp.SourceArmature);
            Assert.AreEqual(DTArmatureMapping.AMDresserDefaultConfig.DynamicsOptions.RemoveDynamicsAndUseParentConstraint, comp.DresserDefaultConfig.DynamicsOption);
            PrintMappingsAndTags(comp.Mappings, comp.Tags);
            Assert.AreEqual(1, comp.Mappings.Count);
            Assert.AreEqual(1, comp.Tags.Count);
            Assert.True(comp.Mappings
                .Where(m =>
                    m.Type == DTObjectMapping.Mapping.MappingType.DoNothing &&
                    m.SourceTransform == wearableMyBone &&
                    m.TargetPath == "Armature/Hips/MyBone")
                .Count() == 1);
            Assert.True(comp.Tags
                .Where(t =>
                    t.Type == DTArmatureMapping.Tag.TagType.DoNothing &&
                    t.SourceTransform == wearableMyBone)
                .Count() == 1);
        }

        [Test]
        public void MapManualTest()
        {
            var provider = new ArmatureMappingWearableModuleProvider();

            var avatarObj = InstantiateEditorTestPrefab("DTTest_MapManualAvatar.prefab");
            var wearableTrans = avatarObj.transform.Find("Wearable");
            Assert.NotNull(wearableTrans);
            var wearableArmature = avatarObj.transform.Find("Wearable/Armature");
            Assert.NotNull(wearableArmature);
            var wearableHips = avatarObj.transform.Find("Wearable/Armature/Hips");
            Assert.NotNull(wearableHips);
            var wearableMyBone = avatarObj.transform.Find("Wearable/Armature/Hips/MyBone");
            Assert.NotNull(wearableMyBone);
            var wearableMyDynBone = avatarObj.transform.Find("Wearable/Armature/Hips/MyDynBone");
            Assert.NotNull(wearableMyDynBone);
            var wearableMyAnotherDynBone = avatarObj.transform.Find("Wearable/Armature/Hips/MyAnotherDynBone");
            Assert.NotNull(wearableMyAnotherDynBone);

            var cabCtx = CreateCabinetContext(avatarObj);
            var wearCtx = CreateWearableContext(cabCtx, wearableTrans.gameObject);
            var amm = wearCtx.wearableConfig.FindModuleConfig<ArmatureMappingWearableModuleConfig>();

            Assert.True(provider.Invoke(cabCtx, wearCtx, new ReadOnlyCollection<WearableModule>(new List<WearableModule>() { new WearableModule() {
                moduleName = ArmatureMappingWearableModuleConfig.ModuleIdentifier,
                config = amm
            }}), false));

            Assert.True(wearableTrans.TryGetComponent<DTArmatureMapping>(out var comp));
            Assert.AreEqual(DTArmatureMapping.MappingMode.Manual, comp.Mode);
            Assert.AreEqual(DTArmatureMapping.DresserTypes.Default, comp.DresserType);
            Assert.AreEqual("Armature", comp.TargetArmaturePath);
            Assert.AreEqual(wearableArmature, comp.SourceArmature);
            Assert.AreEqual(DTArmatureMapping.AMDresserDefaultConfig.DynamicsOptions.RemoveDynamicsAndUseParentConstraint, comp.DresserDefaultConfig.DynamicsOption);
            PrintMappingsAndTags(comp.Mappings, comp.Tags);
            Assert.AreEqual(4, comp.Mappings.Count);
            Assert.AreEqual(1, comp.Tags.Count);
            Assert.True(comp.Mappings
                .Where(m =>
                    m.Type == DTObjectMapping.Mapping.MappingType.MoveToBone &&
                    m.SourceTransform == wearableHips &&
                    m.TargetPath == "Armature/Hips")
                .Count() == 1);
            Assert.True(comp.Mappings
                .Where(m =>
                    m.Type == DTObjectMapping.Mapping.MappingType.DoNothing &&
                    m.SourceTransform == wearableMyBone &&
                    m.TargetPath == "Armature/Hips/MyBone")
                .Count() == 1);
            Assert.True(comp.Mappings
                .Where(m =>
                    m.Type == DTObjectMapping.Mapping.MappingType.MoveToBone &&
                    m.SourceTransform == wearableMyDynBone &&
                    m.TargetPath == "Armature/Hips/MyDynBone")
                .Count() == 1);
            Assert.True(comp.Mappings
                .Where(m =>
                    m.Type == DTObjectMapping.Mapping.MappingType.MoveToBone &&
                    m.SourceTransform == wearableMyAnotherDynBone &&
                    m.TargetPath == "Armature/Hips/MyAnotherDynBone")
                .Count() == 1);
            Assert.True(comp.Tags
                .Where(t =>
                    t.Type == DTArmatureMapping.Tag.TagType.DoNothing &&
                    t.SourceTransform == wearableMyBone)
                .Count() == 1);
        }
    }
}
