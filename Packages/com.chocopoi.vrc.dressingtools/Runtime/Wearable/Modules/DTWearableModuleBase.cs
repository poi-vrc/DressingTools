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
using System.Linq;
using Chocopoi.DressingTools.Cabinet;
using Chocopoi.DressingTools.Logging;
using Chocopoi.DressingTools.Proxy;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Chocopoi.DressingTools.Wearable.Modules
{
    public class DTWearableModuleBaseConverter : JsonConverter
    {
        private const string ModuleTypeKey = "$dtModuleType";

        public override bool CanConvert(Type objectType) => objectType == typeof(DTWearableModuleBase) || objectType == typeof(List<DTWearableModuleBase>);

        private static JObject GetJsonWithModuleType(DTWearableModuleBase module)
        {
            var obj = JObject.FromObject(module);
            obj.AddFirst(new JProperty(ModuleTypeKey, module.GetType().FullName));
            return obj;
        }

        public sealed override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (value is DTWearableModuleBase singleModule)
            {
                // single
                GetJsonWithModuleType(singleModule).WriteTo(writer);
            }
            else if (value is List<DTWearableModuleBase> modules)
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

        private static DTWearableModuleBase GetModuleFromJson(JObject obj)
        {
            if (!obj.ContainsKey(ModuleTypeKey))
            {
                throw new Exception("JSON does not contain ModuleTypeKey! Cannot proceed deserialization");
            }

            string moduleTypeName = obj.Value<string>(ModuleTypeKey);
            var type = DTRuntimeUtils.FindType(moduleTypeName);
            var rawJson = obj.ToString(Formatting.None);

            if (type == null)
            {
                // return UnknownModule containing the raw JSON
                return new UnknownModule(moduleTypeName, rawJson);
            }

            return (DTWearableModuleBase)JsonConvert.DeserializeObject(rawJson, type);
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
                var output = new List<DTWearableModuleBase>();
                for (var i = 0; i < arr.Count; i++)
                {
                    output.Add(GetModuleFromJson((JObject)arr[i]));
                }
                return output;
            }

            return null;
        }
    }

    public abstract class DTWearableModuleBase
    {
        [JsonIgnore]
        public abstract int ApplyOrder { get; }

        [JsonIgnore]
        public abstract bool AllowMultiple { get; }

        public virtual string Serialize()
        {
            return JsonConvert.SerializeObject(this, new DTWearableModuleBaseConverter());
        }

        public abstract bool Apply(DTReport report, DTCabinet cabinet, List<IDynamicsProxy> avatarDynamics, DTWearableConfig config, GameObject wearableGameObject);

        public override string ToString()
        {
            return Serialize();
        }

        public static DTWearableModuleBase Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<DTWearableModuleBase>(json, new DTWearableModuleBaseConverter());
        }
    }
}
