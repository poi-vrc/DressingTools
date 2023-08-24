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
using Chocopoi.DressingTools.Lib.Extensibility.Providers;
using Chocopoi.DressingTools.Lib.Serialization;
using Chocopoi.DressingTools.Lib.Wearable.Modules;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Chocopoi.DressingTools.Lib.Wearable.Serializers
{
    public class WearableV1Serializer : ISerializer
    {
        private static WearableModule DeserializeModule(JObject jObject)
        {
            if (!jObject.ContainsKey("moduleName") || !jObject.ContainsKey("config"))
            {
                throw new ArgumentException("JSON does not contain moduleName or config");
            }

            var configJObject = jObject["config"].Value<JObject>();
            var moduleName = jObject["moduleName"].Value<string>();
            var provider = WearableModuleProviderLocator.Instance.GetProvider(moduleName);

            IModuleConfig moduleConfig = provider == null ?
                new UnknownModuleConfig(configJObject.ToString(Formatting.None)) :
                provider.DeserializeModuleConfig(configJObject);

            return new WearableModule()
            {
                moduleName = moduleName,
                config = moduleConfig
            };
        }

        private const string KeyVersion = "version";
        private const string KeyInfo = "info";
        private const string KeyAvatarConfig = "avatarConfig";
        private const string KeyModules = "modules";

        public void Deserialize(object obj, JObject jObject)
        {
            // TODO: perform schema check here
            var config = (WearableConfig)obj;

            if (!jObject.ContainsKey(KeyVersion) || jObject[KeyVersion].Type != JTokenType.String ||
                 !jObject.ContainsKey(KeyInfo) || jObject[KeyInfo].Type != JTokenType.Object ||
                 !jObject.ContainsKey(KeyAvatarConfig) || jObject[KeyAvatarConfig].Type != JTokenType.Object ||
                 !jObject.ContainsKey(KeyModules) || jObject[KeyModules].Type != JTokenType.Array)
            {
                throw new JsonException("Config JSON is invalid");
            }

            JsonSerializer versionSerializer = new JsonSerializer();
            versionSerializer.Converters.Add(new SerializationVersionConverter());
            config.Version = jObject[KeyVersion].ToObject<SerializationVersion>(versionSerializer);

            config.Info = jObject[KeyInfo].ToObject<WearableInfo>();
            config.AvatarConfig = jObject[KeyAvatarConfig].ToObject<AvatarConfig>();

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
            var config = (WearableConfig)obj;
            var jObject = new JObject
            {
                [KeyVersion] = config.Version.ToString(),
                [KeyInfo] = JObject.FromObject(config.Info),
                [KeyAvatarConfig] = JObject.FromObject(config.AvatarConfig)
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
