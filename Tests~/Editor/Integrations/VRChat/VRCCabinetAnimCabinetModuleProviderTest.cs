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

#if VRC_SDK_VRCSDK3
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Chocopoi.DressingFramework.Cabinet.Modules;
using Chocopoi.DressingTools.Api.Integration.VRChat.Cabinet.Modules;
using Chocopoi.DressingTools.Integration.VRChat.Modules;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace Chocopoi.DressingTools.Tests.Integrations.VRChat
{
    public class VRCCabinetAnimCabinetModuleProviderTest : EditorTestBase
    {
        [Test]
        public void DeserializeModuleConfigTest()
        {
            var provider = new VRCCabinetAnimCabinetModuleProvider();
            var makeUpConfig = new VRCCabinetAnimCabinetModuleConfig();
            var deserializedConfig = (VRCCabinetAnimCabinetModuleConfig)provider.DeserializeModuleConfig(JObject.Parse(JsonConvert.SerializeObject(makeUpConfig)));

            Assert.AreEqual(makeUpConfig.cabinetThumbnails, deserializedConfig.cabinetThumbnails);
        }

        [Test]
        public void NewModuleConfig()
        {
            var provider = new VRCCabinetAnimCabinetModuleProvider();
            Assert.IsInstanceOf(typeof(VRCCabinetAnimCabinetModuleConfig), provider.NewModuleConfig());
        }

        [Test]
        public void InvokeTest()
        {
            var provider = new VRCCabinetAnimCabinetModuleProvider();
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
    }
}
#endif
