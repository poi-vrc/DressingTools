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
using Chocopoi.DressingFramework.Wearable.Modules;
using Chocopoi.DressingTools.Api.Wearable.Modules.BuiltIn;
using Chocopoi.DressingTools.Wearable.Modules;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Chocopoi.DressingTools.Tests.Wearable.Modules
{
    public class ArmatureMappingWearableModuleProviderTest : EditorTestBase
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

        [Test]
        public void MapAutoTest()
        {
            var provider = new ArmatureMappingWearableModuleProvider();

            var avatarObj = InstantiateEditorTestPrefab("DTTest_MapAutoAvatar.prefab");
            var wearableTrans = avatarObj.transform.Find("Wearable");
            Assert.NotNull(wearableTrans);

            var cabCtx = CreateCabinetContext(avatarObj);
            var wearCtx = CreateWearableContext(cabCtx, wearableTrans.gameObject);
            var amm = wearCtx.wearableConfig.FindModuleConfig<ArmatureMappingWearableModuleConfig>();

            Assert.True(provider.Invoke(cabCtx, wearCtx, new ReadOnlyCollection<WearableModule>(new List<WearableModule>() { new WearableModule() {
                moduleName = ArmatureMappingWearableModuleConfig.ModuleIdentifier,
                config = amm
            }}), false));

            Assert.NotNull(avatarObj.transform.Find("Armature/Hips/Hips_DT"));
            Assert.NotNull(avatarObj.transform.Find("Armature/Hips/MyBone/MyBone_DT"));
        }

        [Test]
        public void MapOverrideTest()
        {
            var provider = new ArmatureMappingWearableModuleProvider();

            var avatarObj = InstantiateEditorTestPrefab("DTTest_MapOverrideAvatar.prefab");
            var wearableTrans = avatarObj.transform.Find("Wearable");
            Assert.NotNull(wearableTrans);

            var cabCtx = CreateCabinetContext(avatarObj);
            var wearCtx = CreateWearableContext(cabCtx, wearableTrans.gameObject);
            var amm = wearCtx.wearableConfig.FindModuleConfig<ArmatureMappingWearableModuleConfig>();

            Assert.True(provider.Invoke(cabCtx, wearCtx, new ReadOnlyCollection<WearableModule>(new List<WearableModule>() { new WearableModule() {
                moduleName = ArmatureMappingWearableModuleConfig.ModuleIdentifier,
                config = amm
            }}), false));

            Assert.NotNull(avatarObj.transform.Find("Armature/Hips/Hips_DT"));
            Assert.Null(avatarObj.transform.Find("Armature/Hips/MyBone/MyBone_DT"));
        }

        [Test]
        public void MapManualTest()
        {
            var provider = new ArmatureMappingWearableModuleProvider();

            var avatarObj = InstantiateEditorTestPrefab("DTTest_MapManualAvatar.prefab");
            var wearableTrans = avatarObj.transform.Find("Wearable");
            Assert.NotNull(wearableTrans);

            var cabCtx = CreateCabinetContext(avatarObj);
            var wearCtx = CreateWearableContext(cabCtx, wearableTrans.gameObject);
            var amm = wearCtx.wearableConfig.FindModuleConfig<ArmatureMappingWearableModuleConfig>();

            Assert.True(provider.Invoke(cabCtx, wearCtx, new ReadOnlyCollection<WearableModule>(new List<WearableModule>() { new WearableModule() {
                moduleName = ArmatureMappingWearableModuleConfig.ModuleIdentifier,
                config = amm
            }}), false));

            Assert.NotNull(avatarObj.transform.Find("Armature/Hips/Hips_DT"));
            Assert.Null(avatarObj.transform.Find("Armature/Hips/MyBone/MyBone_DT"));
            Assert.NotNull(avatarObj.transform.Find("Armature/Hips/MyDynBone/MyDynBone_DT"));
            Assert.NotNull(avatarObj.transform.Find("Armature/Hips/MyAnotherDynBone/MyAnotherDynBone_DT"));
        }
    }
}
