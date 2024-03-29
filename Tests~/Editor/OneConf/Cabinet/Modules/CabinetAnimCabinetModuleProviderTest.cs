﻿/*
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
using Chocopoi.DressingTools.OneConf.Cabinet;
using Chocopoi.DressingTools.OneConf.Cabinet.Modules;
using Chocopoi.DressingTools.OneConf.Cabinet.Modules.BuiltIn;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Chocopoi.DressingTools.Tests.OneConf.Cabinet.Modules
{
    internal class CabinetAnimCabinetModuleProviderTest : EditorTestBase
    {
        [Test]
        public void DeserializeModuleConfigTest()
        {
            var provider = new CabinetAnimCabinetModuleProvider();
            var makeUpConfig = new CabinetAnimCabinetModuleConfig();
            var deserializedConfig = (CabinetAnimCabinetModuleConfig)provider.DeserializeModuleConfig(JObject.Parse(JsonConvert.SerializeObject(makeUpConfig)));

            Assert.AreEqual(makeUpConfig.version.ToString(), deserializedConfig.version.ToString());
            Assert.AreEqual(makeUpConfig.savedAvatarPresets.Count, deserializedConfig.savedAvatarPresets.Count);
            Assert.AreEqual(makeUpConfig.savedWearablePresets.Count, deserializedConfig.savedWearablePresets.Count);
        }

        [Test]
        public void NewModuleConfig()
        {
            var provider = new CabinetAnimCabinetModuleProvider();
            Assert.IsInstanceOf(typeof(CabinetAnimCabinetModuleConfig), provider.NewModuleConfig());
        }

#if DT_VRCSDK3A
        [Test]
        public void InvokeTest()
        {
            var provider = new CabinetAnimCabinetModuleProvider();
            var avatarObj = InstantiateEditorTestPrefab("VRCAvatar.prefab");

            var wearable1Trans = avatarObj.transform.Find("Wearable1");
            Assert.NotNull(wearable1Trans);
            var wearable2Trans = avatarObj.transform.Find("Wearable2");
            Assert.NotNull(wearable2Trans);

            var cabCtx = CreateCabinetContext(avatarObj);
            CreateWearableContext(cabCtx, wearable1Trans.gameObject);
            CreateWearableContext(cabCtx, wearable2Trans.gameObject);

            Assert.True(provider.Invoke(
                cabCtx,
                new ReadOnlyCollection<CabinetModule>(new List<CabinetModule>()),
                false), "Provider invoke returned failure");

            // TODO: check content
        }
#endif
    }
}
