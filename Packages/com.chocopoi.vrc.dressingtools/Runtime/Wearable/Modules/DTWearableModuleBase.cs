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

        public override bool CanConvert(Type objectType) => objectType == typeof(DTWearableModuleBase) || objectType == typeof(IEnumerable<DTWearableModuleBase>);

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
            else if (value is IEnumerable<DTWearableModuleBase> modules)
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
                var output = new DTWearableModuleBase[arr.Count];
                for (var i = 0; i < output.Length; i++)
                {
                    output[i] = GetModuleFromJson((JObject)arr[i]);
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
