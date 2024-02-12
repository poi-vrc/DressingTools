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

using System;
using Chocopoi.DressingTools.OneConf.Wearable.Modules;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Chocopoi.DressingTools.OneConf.Serialization
{
    /// <summary>
    /// Wearable module converter
    /// </summary>
    internal class WearableModuleConverter : JsonConverter<WearableModule>
    {
        private const string ModuleNameKey = "moduleName";
        private const string ConfigKey = "config";

        public override WearableModule ReadJson(JsonReader reader, Type objectType, WearableModule existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);

            if (token.Type != JTokenType.Object)
            {
                throw new Exception("module JSON is not an JObject");
            }

            var jObject = (JObject)token;

            if (!jObject.ContainsKey(ModuleNameKey) || !jObject.ContainsKey(ConfigKey))
            {
                throw new Exception("module JSON does not contain moduleName or config");
            }

            var configJObject = jObject[ConfigKey].Value<JObject>();
            var moduleName = jObject[ModuleNameKey].Value<string>();
            var provider = ModuleManager.Instance.GetWearableModuleProvider(moduleName);

            IModuleConfig moduleConfig = provider == null ?
                new UnknownModuleConfig(configJObject.ToString(Formatting.None)) :
                provider.DeserializeModuleConfig(configJObject);

            return new WearableModule()
            {
                moduleName = moduleName,
                config = moduleConfig
            };
        }

        public override void WriteJson(JsonWriter writer, WearableModule value, JsonSerializer serializer)
        {
            var jObject = new JObject
            {
                { "moduleName", value.moduleName },
                { "config", JObject.FromObject(value.config) }
            };
            jObject.WriteTo(writer);
        }
    }
}
