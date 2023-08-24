/*
 * File: WearableV1Serializer.cs
 * Project: DressingTools
 * Created Date: Saturday, Aug 22nd 2023, 16:35:11 pm
 * Author: chocopoi (poi@chocopoi.com)
 * -----
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

using System;
using Chocopoi.DressingTools.Lib.Cabinet.Modules;
using Chocopoi.DressingTools.Lib.Extensibility.Providers;
using Chocopoi.DressingTools.Lib.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Chocopoi.DressingTools.Lib.Cabinet.Serializers
{
    public class CabinetV1Serializer : ISerializer
    {
        private const string KeyVersion = "version";
        private const string KeyAvatarArmatureName = "avatarArmatureName";
        private const string KeyGroupDynamics = "groupDynamics";
        private const string KeyGroupDynamicsSeparateGameObjects = "groupDynamicsSeparateGameObjects";
        private const string KeyModules = "modules";

        private static CabinetModule DeserializeModule(JObject jObject)
        {
            if (!jObject.ContainsKey("moduleName") || !jObject.ContainsKey("config"))
            {
                throw new ArgumentException("JSON does not contain moduleName or config");
            }

            var configJObject = jObject["config"].Value<JObject>();
            var moduleName = jObject["moduleName"].Value<string>();
            var provider = CabinetModuleProviderLocator.Instance.GetProvider(moduleName);

            IModuleConfig moduleConfig = provider == null ?
                new UnknownModuleConfig(configJObject.ToString(Formatting.None)) :
                provider.DeserializeModuleConfig(configJObject);

            return new CabinetModule()
            {
                moduleName = moduleName,
                config = moduleConfig
            };
        }

        public void Deserialize(object obj, JObject jObject)
        {
            // TODO: perform schema check here
            var config = (CabinetConfig)obj;

            if (!jObject.ContainsKey(KeyVersion) || jObject[KeyVersion].Type != JTokenType.String ||
                 !jObject.ContainsKey(KeyAvatarArmatureName) || jObject[KeyAvatarArmatureName].Type != JTokenType.String ||
                 !jObject.ContainsKey(KeyGroupDynamics) || jObject[KeyGroupDynamics].Type != JTokenType.Boolean ||
                 !jObject.ContainsKey(KeyGroupDynamicsSeparateGameObjects) || jObject[KeyGroupDynamicsSeparateGameObjects].Type != JTokenType.Boolean ||
                 !jObject.ContainsKey(KeyModules) || jObject[KeyModules].Type != JTokenType.Array)
            {
                throw new JsonException("Config JSON is invalid");
            }

            JsonSerializer versionSerializer = new JsonSerializer();
            versionSerializer.Converters.Add(new SerializationVersionConverter());
            config.Version = jObject[KeyVersion].ToObject<SerializationVersion>(versionSerializer);

            config.AvatarArmatureName = jObject[KeyAvatarArmatureName].Value<string>();
            config.GroupDynamics = jObject[KeyGroupDynamics].Value<bool>();
            config.GroupDynamicsSeparateGameObjects = jObject[KeyGroupDynamicsSeparateGameObjects].Value<bool>();

            var modulesArray = (JArray)jObject[KeyModules];
            foreach (var moduleJtoken in modulesArray)
            {
                if (moduleJtoken.Type != JTokenType.Object)
                {
                    throw new JsonException("One of the config module is not an JObject");
                }
                var module = DeserializeModule((JObject)moduleJtoken);
                config.Modules.Add(module);
            }
        }

        public JObject Serialize(object obj)
        {
            var config = (CabinetConfig)obj;
            var jObject = new JObject
            {
                [KeyVersion] = config.Version.ToString(),
                [KeyAvatarArmatureName] = config.AvatarArmatureName,
                [KeyGroupDynamics] = config.GroupDynamics,
                [KeyGroupDynamicsSeparateGameObjects] = config.GroupDynamicsSeparateGameObjects,
            };

            var modulesArray = new JArray();
            foreach (var module in config.Modules)
            {
                modulesArray.Add(JObject.FromObject(module));
            }
            jObject[KeyModules] = modulesArray;

            return jObject;
        }
    }

}
