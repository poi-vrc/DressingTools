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
using Chocopoi.DressingTools.Components.Modifiers;
using Chocopoi.DressingTools.OneConf.Wearable.Modules;
using Chocopoi.DressingTools.OneConf.Wearable.Modules.BuiltIn;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Chocopoi.DressingTools.Tests.OneConf.Wearable.Modules
{
    internal class MoveRootWearableModuleProviderTest : EditorTestBase
    {
        [Test]
        public void DeserializeModuleConfigTest()
        {
            var provider = new MoveRootWearableModuleProvider();
            var makeUpConfig = new MoveRootWearableModuleConfig();
            var deserializedConfig = (MoveRootWearableModuleConfig)provider.DeserializeModuleConfig(JObject.Parse(JsonConvert.SerializeObject(makeUpConfig)));

            Assert.AreEqual(makeUpConfig.version.ToString(), deserializedConfig.version.ToString());
            Assert.AreEqual(makeUpConfig.avatarPath, deserializedConfig.avatarPath);
        }

        [Test]
        public void NewModuleConfig()
        {
            var provider = new MoveRootWearableModuleProvider();
            Assert.IsInstanceOf(typeof(MoveRootWearableModuleConfig), provider.NewModuleConfig());
        }

        [Test]
        public void InvokeTest()
        {
            var provider = new MoveRootWearableModuleProvider();

            var avatarObj = InstantiateEditorTestPrefab("DTTest_MoveRootAvatar.prefab");
            var wearableTrans = avatarObj.transform.Find("Wearable");
            Assert.NotNull(wearableTrans);

            var cabCtx = CreateCabinetContext(avatarObj);
            var wearCtx = CreateWearableContext(cabCtx, wearableTrans.gameObject);
            var mrm = wearCtx.wearableConfig.FindModuleConfig<MoveRootWearableModuleConfig>();

            Assert.True(provider.Invoke(cabCtx, wearCtx, new ReadOnlyCollection<WearableModule>(new List<WearableModule>() { new WearableModule() {
                moduleName = MoveRootWearableModuleConfig.ModuleIdentifier,
                config = mrm
            }}), false));

            Assert.True(wearableTrans.TryGetComponent<DTMoveRoot>(out var comp));
            Assert.AreEqual(mrm.avatarPath, comp.DestinationPath);
        }
    }
}
