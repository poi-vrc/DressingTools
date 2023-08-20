/*
 * File: DTWearableModuleBase.cs
 * Project: DressingTools
 * Created Date: Tuesday, August 1st 2023, 12:37:10 am
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
using System.Collections.Generic;
using Chocopoi.DressingTools.Lib.Cabinet;
using Chocopoi.DressingTools.Lib.Logging;
using Chocopoi.DressingTools.Lib.Proxy;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Chocopoi.DressingTools.Lib.Wearable.Modules
{
    internal class WearableModuleConverter : JsonConverter
    {
        private const string ModuleTypeKey = "$dtModuleType";

        public override bool CanConvert(Type objectType) => objectType == typeof(WearableModuleBase) || objectType == typeof(List<WearableModuleBase>);

        private static JObject GetJsonWithModuleType(WearableModuleBase module)
        {
            var obj = JObject.FromObject(module);
            obj.AddFirst(new JProperty(ModuleTypeKey, module.GetType().FullName));
            return obj;
        }

        public sealed override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is WearableModuleBase singleModule)
            {
                // single
                GetJsonWithModuleType(singleModule).WriteTo(writer);
            }
            else if (value is List<WearableModuleBase> modules)
            {
                var arr = new JArray();
                // array
                foreach (var module in modules)
                {
                    arr.Add(GetJsonWithModuleType(module));
                }
                arr.WriteTo(writer);
            }
        }

        private static WearableModuleBase GetModuleFromJson(JObject obj)
        {
            if (!obj.ContainsKey(ModuleTypeKey))
            {
                throw new Exception("JSON does not contain ModuleTypeKey! Cannot proceed deserialization");
            }

            string moduleTypeName = obj.Value<string>(ModuleTypeKey);
            var type = DTLibRuntimeUtils.FindType(moduleTypeName);
            var rawJson = obj.ToString(Formatting.None);

            if (type == null)
            {
                // return UnknownModule containing the raw JSON
                return new UnknownModule(moduleTypeName, rawJson);
            }

            return (WearableModuleBase)JsonConvert.DeserializeObject(rawJson, type);
        }

        public sealed override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);

            if (token.Type == JTokenType.Object)
            {
                return GetModuleFromJson((JObject)token);
            }
            else if (token.Type == JTokenType.Array)
            {
                var arr = (JArray)token;
                var output = new List<WearableModuleBase>();
                for (var i = 0; i < arr.Count; i++)
                {
                    output.Add(GetModuleFromJson((JObject)arr[i]));
                }
                return output;
            }

            return null;
        }
    }

    public abstract class WearableModuleBase
    {
        [JsonIgnore]
        public abstract int ApplyOrder { get; }

        [JsonIgnore]
        public abstract bool AllowMultiple { get; }

        public virtual string Serialize()
        {
            return JsonConvert.SerializeObject(this, new WearableModuleConverter());
        }

        public abstract bool Apply(DTReport report, ICabinet cabinet, List<IDynamicsProxy> avatarDynamics, WearableConfig config, GameObject wearableGameObject);

        public override string ToString()
        {
            return Serialize();
        }

        public static WearableModuleBase Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<WearableModuleBase>(json, new WearableModuleConverter());
        }
    }
}
